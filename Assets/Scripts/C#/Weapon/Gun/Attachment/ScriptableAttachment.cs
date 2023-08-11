using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableAttachment : ScriptableObject
{
    public AttachmentType attachmentType;
    public string attachmentName;
    [HideInInspector]
    public float originalvalue;
    [HideInInspector]
    public GunData gundata;

    public void Init(GunData gundata)
    {
        this.gundata = gundata;
        ApplyAttachmentEffect();
    }

    public abstract void ApplyAttachmentEffect();
    public abstract void RemoveAttachmentEffect();
}
