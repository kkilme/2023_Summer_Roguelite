using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ClientBullet : MonoBehaviour
{
    private float _speed;
    private float _lifeTime;
    private Vector3 _direction;
    private Transform _effectparent;
    private Vector3 _prevpos;

    private CancellationTokenSource _cancellationTokenSource;

    private void Start()
    {
        _effectparent = GameObject.Find("Effect").transform;
        transform.SetParent(_effectparent);
        _prevpos = transform.position;

        FireBullet();
        _cancellationTokenSource = new CancellationTokenSource();
        DestroySelf(_cancellationTokenSource.Token).Forget();
        UpdateDirection(_cancellationTokenSource.Token).Forget();
    }

    public void Init(Vector3 dir, float bulletspeed, float bulletLifetime)
    {
        _direction = dir.normalized;
        _speed = bulletspeed;
        _lifeTime = bulletLifetime;
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

        Debug.DrawRay(transform.position, _direction);

        if (Physics.Raycast(transform.position, _direction, out hit, distanceThisFrame))
        {
            Destroy(this.gameObject);
        }
    }

    private async UniTaskVoid UpdateDirection(CancellationToken cancellationToken)
    {
        await UniTask.Delay(500, cancellationToken: cancellationToken);
        while (true)
        {
            _direction = (transform.position - _prevpos).normalized;
            _prevpos = transform.position;
            transform.rotation = Quaternion.LookRotation(_direction);
            await UniTask.Delay(200, cancellationToken: cancellationToken);
        }
    }
    private void FireBullet()
    {
        GetComponent<Rigidbody>().velocity = _direction * _speed;
    }
    
    private async UniTaskVoid DestroySelf(CancellationToken cancellationToken)
    {
        await UniTask.Delay((int)(_lifeTime * 1000), cancellationToken: cancellationToken);
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
    }
}