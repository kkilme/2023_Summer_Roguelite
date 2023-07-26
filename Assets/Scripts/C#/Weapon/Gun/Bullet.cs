using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private GameObject _bullethole;
    [SerializeField] private float _lifeTime;
    //[SerializeField] private LayerMask _hitLayer;

    private Vector3 _direction;
    private float _maxDistance;
    private float _dmg;

    private float _travelDistance;
    private Transform _effectparent;
    private Vector3 _prevpos;

    private CancellationTokenSource _cancellationTokenSource;

    public void Start()
    {
        _effectparent = GameObject.Find("Effect").transform;
        transform.SetParent(_effectparent);
        // Bullet 생성 후 바로 발사 (Server에서만 발사, Client는 서버로부터 받아서 위치/방향 동기화) <- 수정필요할듯?
        if (IsServer)
        {
            FireBullet();
            _cancellationTokenSource = new CancellationTokenSource();
            DestroySelf(_cancellationTokenSource.Token).Forget();
            UpdateDirection(_cancellationTokenSource.Token).Forget();
        }
        
    }

    public void Init(Vector3 dir, float maxDistance, float dmg)
    {
        _maxDistance = maxDistance;
        _dmg = dmg;
        _direction = dir.normalized;
        _prevpos = transform.position;
    }

    private void FixedUpdate()
    {
        MoveBullet();
    }

    private void MoveBullet()
    {
        float distanceThisFrame = _speed * Time.fixedDeltaTime;
        
        RaycastHit hit;

        Debug.DrawRay(transform.position, _direction);

        if (_travelDistance < _maxDistance && Physics.Raycast(transform.position, _direction, out hit, distanceThisFrame))
        {
            HitTargetClientRPC(hit.point, hit.normal);
            //Debug.Log(hit.transform.gameObject.name);
            GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            _travelDistance += distanceThisFrame;
        }
    }

    private async UniTaskVoid UpdateDirection(CancellationToken cancellationToken)
    {
        await UniTask.Delay(1000, cancellationToken: cancellationToken);
        while (true)
        {
            _direction = (transform.position - _prevpos).normalized;
            _prevpos = transform.position;
            transform.rotation = Quaternion.LookRotation(_direction);
            await UniTask.Delay(200, cancellationToken: cancellationToken);
        }
    }


    [ClientRpc]
    private void HitTargetClientRPC(Vector3 hitPoint, Vector3 hitNormal)
    {
        // 클라이언트는 서버로부터 받은 충돌 정보를 이용하여 피격 이펙트 등을 처리
        ShowHitEffect(hitPoint, hitNormal);
    }

    private void ShowHitEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        GameObject hitEffect = Instantiate(_bullethole, hitPoint, Quaternion.LookRotation(hitNormal), _effectparent);
        Destroy(hitEffect, 3f);
    }

    private void FireBullet()
    {
        _travelDistance = 0f;
        GetComponent<Rigidbody>().velocity = _direction * _speed;
    }

    private async UniTaskVoid DestroySelf(CancellationToken cancellationToken)
    {
        await UniTask.Delay((int)(_lifeTime * 1000), cancellationToken: cancellationToken);
        
        GetComponent<NetworkObject>().Despawn();
    }
    private new void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
    }
}