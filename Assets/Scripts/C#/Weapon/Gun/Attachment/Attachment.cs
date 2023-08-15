using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Attachment: ScriptableObject
{
    public ATTACHMENT_TYPE attachmentType;
    public ATTACHMENT_NAME attachmentName;
    [HideInInspector]
    public float originalValue;

    public abstract void ApplyAttachmentEffect(ref GunData gunData);
    public abstract void RemoveAttachmentEffect(ref GunData gunData);

}
