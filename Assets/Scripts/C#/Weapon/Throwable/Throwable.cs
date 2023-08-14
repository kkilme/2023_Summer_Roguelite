using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Throwable : NetworkBehaviour
{
    [SerializeField] protected float force; // 던지는 힘
    [SerializeField] protected float range; // 적용 범위
    [SerializeField] protected float explosionTime; // 던지고 나서 터지기 까지 걸리는 시간
    protected Rigidbody rigidbody;

    protected abstract void Throw();
    [ServerRpc] protected virtual void ExplodeServerRPC() { }
}
