using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Scope", menuName = "Weapon/Attachment/Scope")]
public class Scope : ScriptableAttachment
{
    public float zoomrate;
    public override void ApplyAttachmentEffect()
    {
        originalvalue = gundata.aimingZoomrate;
        gundata.aimingZoomrate = zoomrate;
    }

    public override void RemoveAttachmentEffect()
    {
        gundata.aimingZoomrate = originalvalue;
    }

}
