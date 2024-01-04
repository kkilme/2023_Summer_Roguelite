using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEditor.Rendering;

public class ServerBullet : NetworkBehaviour
{
    private float _speed;
    private float _lifeTime;
    //[SerializeField] private LayerMask _hitLayer;

    private Vector3 _direction;
    private float _dmg;
    private float _travelDistance; // 거리에 따른 대미지 조정을 위해 필요할 듯?
    private Transform _effectparent;
    private Vector3 _prevpos;

    private CancellationTokenSource _cancellationTokenSource;


    public override void OnNetworkSpawn()
    {
        _effectparent = GameObject.Find("Effect").transform;
        transform.SetParent(_effectparent);
        if (!IsServer)
        {
            this.gameObject.SetActive(false);
            return;
        }
        _prevpos = transform.position;
        _travelDistance = 0f;

        FireBullet();
        _cancellationTokenSource = new CancellationTokenSource();
        DestroySelf(_cancellationTokenSource.Token).Forget();
        UpdateDirection(_cancellationTokenSource.Token).Forget();
    }

    public void Init(Vector3 dir, float bulletspeed, float bulletLifetime, float dmg)
    {
        _direction = dir.normalized;
        _speed = bulletspeed;
        _lifeTime = bulletLifetime;
        _dmg = dmg;
        transform.rotation = Quaternion.LookRotation(_direction);
    }

    private void FixedUpdate()
    {
        CollisionDetect();
    }

    private void CollisionDetect()
    {
        float distanceThisFrame = _speed * Time.fixedDeltaTime;
        
        RaycastHit hit;

        Debug.DrawRay(transform.position, _direction*5);

        if (Physics.Raycast(transform.position, _direction, out hit, distanceThisFrame)) // Raycast로 충돌 검사
        {
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Furniture")) // 충돌 대상에 따른 이펙트 종류가 많아지만 추후 코드 개선할 수 있을 듯.
            {
                HitTargetClientRPC(hit.point, hit.normal, HitType.Object);
            } else
            {
                if (hit.transform.gameObject.TryGetComponent(out IAttackable attackable))
                    attackable.OnDamaged((int)_dmg);
            }
            
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


    /// <summary>
    /// 총알의 Collision 시 모든 클라이언트에 메시지. 이펙트 생성 등 지시.
    /// </summary>
    /// <param name="hitPoint"></param>
    /// <param name="hitNormal"></param>
    [ClientRpc]
    private void HitTargetClientRPC(Vector3 hitPoint, Vector3 hitNormal, HitType hitType)
    {   
        if(hitType == HitType.Object) ShowHitEffect(hitPoint, hitNormal);
    }

    private void ShowHitEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        Instantiate(GameManager.Resource.GetObject<GameObject>("Bullet/SoftBodyHole"), hitPoint, Quaternion.LookRotation(hitNormal), _effectparent);
        
    }

    private void FireBullet()
    {
        GetComponent<Rigidbody>().velocity = _direction * _speed;
    }

    private async UniTaskVoid DestroySelf(CancellationToken cancellationToken)
    {
        await UniTask.Delay((int)(_lifeTime * 1000), cancellationToken: cancellationToken);
        if(GetComponent<NetworkObject>().IsSpawned) GetComponent<NetworkObject>().Despawn();
    }
    private new void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
    }

    
}

public enum HitType
{
    Player,
    Object
}