using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FlashBang : Throwable
{
    [SerializeField] private float effectDuration;

    private Image flashBangEffectImage;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            flashBangEffectImage = Array.Find(FindObjectsOfType<Image>(true), x => x.CompareTag("FlashBang Image"));
        }

        if (IsServer)
        {
            rigidbody = Util.GetOrAddComponent<Rigidbody>(gameObject);
            Throw();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void ExplodeServerRPC()
    {
        var players = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Player"));

        for (int i = 0; i < players.Length; i++)
        {
            Vector3 flashbangDir = (transform.position - players[i].transform.position).normalized;
            Vector3 playerDir = players[i].transform.forward;

            float effectPercentage = (Vector3.Dot(flashbangDir, playerDir) + 1) / 2;
            float distancePercentage = Mathf.Min(1, 4.75f * effectPercentage * (range - Vector3.Distance(players[i].transform.position, transform.position)) / range);

            float amount;

            if (effectPercentage > 0.5f)
                amount = 1;
            else if (effectPercentage > 0.25f)
                amount = 0.75f;
            else
                amount = 0.5f;

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { players[i].GetComponent<NetworkObject>().OwnerClientId }
                }
            };

            EffectClientRPC(amount * distancePercentage, distancePercentage * effectPercentage * effectDuration, clientRpcParams);
        }
    }

    [ClientRpc]
    private void EffectClientRPC(float amount, float duration, ClientRpcParams clientRpcParams)
    {
        flashBangEffectImage.gameObject.SetActive(true);
        flashBangEffectImage.color = Color.white * new Color(1, 1, 1, 0);
        DOTween.Sequence()
        .Append(flashBangEffectImage.DOFade(amount, 0.15f).SetEase(Ease.OutQuart))
        .AppendInterval(duration)
        .Append(flashBangEffectImage.DOFade(0, 1).SetEase(Ease.Linear))
        .OnComplete(() => flashBangEffectImage.gameObject.SetActive(false));
    }

    protected override void Throw()
    {
        rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
        UniTask.Void(async () =>
        {
            await WaitExplode();
        });
    }

    private async UniTask WaitExplode()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(explosionTime));
        ExplodeServerRPC();
    }
}
