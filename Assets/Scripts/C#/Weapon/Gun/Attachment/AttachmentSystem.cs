using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class AttachmentSystem
{
    /*재장전 도중 부착물 장착 시 버그 있는 지 테스트 필요*/

    // GunData의 부착물 관리
    public static Dictionary<int, List<Attachment>> attachmentDataDic = new Dictionary<int, List<Attachment>>();

    // 부착물 장착
    public static void EquipAttachment(ref GunData gunData, Attachment attachment)
    {
        if (!gunData.CheckEquippableAttachment(attachment))
        {
            return;
        }

        // 이미 해당 부착물 타입 장착중일 시 먼저 해제 
        if (gunData.equippedAttachments[attachment.attachmentType] != ATTACHMENT_NAME.None)
        {
            Debug.Log("Already equipped");
            UnequipAttachment(ref gunData, GetAttachmentOfSpecificType(ref gunData, attachment));
        }

        if (!attachmentDataDic.ContainsKey(gunData.hashcode))
        {
            Debug.Log("Not equipped");
            attachmentDataDic[gunData.hashcode] = new List<Attachment>();
        }
        attachmentDataDic[gunData.hashcode].Add(attachment);
        gunData.equippedAttachments[attachment.attachmentType] = attachment.attachmentName;
        attachment.ApplyAttachmentEffect(ref gunData);
    }

    // 부착물 장착 해제
    public static void UnequipAttachment(ref GunData gunData, Attachment attachment)
    {
        Attachment willBeUnequipped = GetAttachmentOfSpecificType(ref gunData, attachment);
        if (attachmentDataDic.ContainsKey(gunData.hashcode))
        {
            Debug.Log("Unequip");
            willBeUnequipped.RemoveAttachmentEffect(ref gunData);
            attachmentDataDic[gunData.hashcode].Remove(willBeUnequipped);
            gunData.equippedAttachments[willBeUnequipped.attachmentType] = ATTACHMENT_NAME.None;
        }
        // Attachment 객체를 재사용하도록 코드 개선할 수 있을 듯
    }

    // 인자로 받은 attachment와 같은 타입의 attachment를 딕셔너리에서 찾아 반환
    private static Attachment GetAttachmentOfSpecificType(ref GunData gunData, Attachment attachment)
    {
        if (attachment is Scope)
        {
            return attachmentDataDic[gunData.hashcode].OfType<Scope>().FirstOrDefault();
        }
        else if (attachment is Mag)
        {
            return attachmentDataDic[gunData.hashcode].OfType<Mag>().FirstOrDefault();
        }
        else
        {
            return attachmentDataDic[gunData.hashcode].OfType<Stock>().FirstOrDefault();
        }
    }
}
public enum ATTACHMENT_NAME
{
    None,
    ScopeX2,
    ScopeX4,
    TestMag
}

public enum ATTACHMENT_TYPE
{
    [Tooltip("조준경")]
    Scope,
    [Tooltip("개머리판, 반동감소")]
    Stock,
    [Tooltip("탄창")]
    Mag,
    [Tooltip("총구")]
    Muzzle,
    [Tooltip("그립")]
    Grip,
    [Tooltip("라이트")]
    Flashlight,
}