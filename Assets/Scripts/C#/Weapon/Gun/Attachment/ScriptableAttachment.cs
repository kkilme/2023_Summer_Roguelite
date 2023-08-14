using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableAttachment : ScriptableObject
{
    public ATTACHMENT_TYPE attachmentType;
    public ATTACHMENT_NAME attachmentName;
    [HideInInspector]
    public float originalvalue;
    [HideInInspector]
    public GunData gundata;

    public void SetGunData(GunData gundata)
    {
        this.gundata = gundata;
    }

    public abstract void ApplyAttachmentEffect();
    public abstract void RemoveAttachmentEffect();
}
