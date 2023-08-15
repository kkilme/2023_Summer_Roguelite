using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Stock", menuName = "Weapon/Attachment/Stock")]
public class Stock : Attachment
{
    public float recoilFixRate;


    public override void ApplyAttachmentEffect(ref GunData gunData)
    {
        float fixamountX = gunData.recoilX * (recoilFixRate / 100);
        float fixamountY = gunData.recoilY * (recoilFixRate / 100);
        float fixamountz = gunData.recoilZ * (recoilFixRate / 100);

    }

    public override void RemoveAttachmentEffect(ref GunData gunData)
    {
        throw new System.NotImplementedException();
    }

}
