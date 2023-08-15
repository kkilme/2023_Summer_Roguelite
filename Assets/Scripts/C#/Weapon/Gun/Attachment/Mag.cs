using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Mag", menuName = "Weapon/Attachment/Mag")]
public class Mag : Attachment
{
    public int magAddAmount;


    public override void ApplyAttachmentEffect(ref GunData gunData)
    {
        gunData.magSize += magAddAmount;
    }

    public override void RemoveAttachmentEffect(ref GunData gunData)
    {
        gunData.magSize -= magAddAmount;
    }

}
