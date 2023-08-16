using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
[CreateAssetMenu(fileName = "Scope", menuName = "Weapon/Attachment/Scope")]
public class Scope : Attachment
{
    public float zoomrate;
    public override void ApplyAttachmentEffect(ref GunData gunData)
    {
        originalValue = gunData.zoomRate;
        gunData.zoomRate = zoomrate;
    }

    public override void RemoveAttachmentEffect(ref GunData gunData)
    {
        gunData.zoomRate = originalValue;
    }

}
