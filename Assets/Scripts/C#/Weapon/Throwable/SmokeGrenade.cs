using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SmokeGrenade : Throwable
{
    [SerializeField] private float effectDuration;

    private ParticleSystem particleSystem;

    public override void OnNetworkSpawn()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();

        if (IsServer)
        {
            // 서버만 리지드바디 생성
            rigidbody = Util.GetOrAddComponent<Rigidbody>(gameObject); 
            Throw();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void ExplodeServerRPC()
    {
        EffectClientRPC();
        WaitEffectEnd().Forget();
    }

    [ClientRpc]
    private void EffectClientRPC()
    {
        particleSystem.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndEffectServerRPC()
    {
        EndEffectClientRPC();
    }

    [ClientRpc]
    private void EndEffectClientRPC()
    {
        particleSystem.Stop();
    }

    protected override void Throw()
    {
        rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
        WaitExplode().Forget();
    }

    private async UniTaskVoid WaitExplode()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(explosionTime));
        ExplodeServerRPC();
    }

    private async UniTaskVoid WaitEffectEnd()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(effectDuration));
        EndEffectServerRPC();
    }
}
