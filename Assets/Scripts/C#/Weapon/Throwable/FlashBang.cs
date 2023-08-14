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

    private void Start()
    {
        flashBangEffectImage = Array.Find(FindObjectsOfType<Image>(true), x => x.CompareTag("FlashBang Image"));
    }

    protected override void Explode()
    {
        var players = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Player"));

        for (int i = 0; i < players.Length; i++)
        {
            Vector3 flashbangDir = (transform.position - players[i].transform.position).normalized;
            Vector3 playerDir = players[i].transform.forward;

            float effectPercentage = (Vector3.Dot(flashbangDir, playerDir) + 1) / 2;

            float amount;

            if (effectPercentage > 0.5f)
            {
                amount = 1;
            }
            else if (effectPercentage > 0.25f)
            {
                amount = 0.75f;
            }
            else
            {
                amount = 0.5f;
            }

            flashBangEffectImage.gameObject.SetActive(true);
            flashBangEffectImage.color = Color.white * new Color(1, 1, 1, 0);
            DOTween.Sequence()
            .Append(flashBangEffectImage.DOFade(amount + 0.1f, 0.15f).SetEase(Ease.OutQuart))
            .AppendInterval(effectDuration * effectPercentage)
            .Append(flashBangEffectImage.DOFade(0, 1).SetEase(Ease.Linear))
            .OnComplete(() => flashBangEffectImage.gameObject.SetActive(false));

            Debug.Log(effectPercentage);
        }
    }

    protected override void Throw()
    {
        throw new System.NotImplementedException();
    }

    void Update()
    {
       if (Input.GetKeyDown(KeyCode.T))
            Explode();
    }
}
