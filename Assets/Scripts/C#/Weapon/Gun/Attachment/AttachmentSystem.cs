using System.Collections;
using UnityEngine;

public class AttachmentSystem
{
    public void EquipAttachment(ScriptableAttachment attachment, GunData gunData)
    {
        attachment.SetGunData(gunData);
        attachment.ApplyAttachmentEffect();
    }

    public void UnequipAttachment(ATTACHMENT_TYPE attachmentType)
    {

    }
}
