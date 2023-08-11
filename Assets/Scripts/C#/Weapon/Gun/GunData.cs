using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Weapon/Gun")]

public class GunData : ScriptableObject
{

    [Header("Info")]
    public new string name;

    [Header("Shooting")]
    public float damage;
    //public float maxDistance; // 총알을 사용하기 때문에 일단 사용 안함
    public float bulletSpeed = 25;
    public int bulletsPerShoot;
    public float bulletLifetime = 5f;
    public float spread;

    [Header("Aiming")]
    public float zoomSpeed; // 우클릭시 조준으로 전환되는 속도
    public float aimingZoomrate = 1.2f;
    
    [Tooltip("1초 당 발사 수")] public float fireRate;
    [Tooltip("자동 발사 여부")] public bool isAutofire;

    [Header("Attachment")]
    [Tooltip("부착 가능한 부착물 종류")]
    public List<AttachmentType> availableAttachmentTypes = new List<AttachmentType>();
    public Dictionary<AttachmentType, ScriptableAttachment> attachments = new Dictionary<AttachmentType, ScriptableAttachment>(); // 장착중인 부착물

    [Header("Recoil")] // 반동
    public float recoilX;
    public float recoilY;
    public float recoilZ;
    public float aimRecoilX; // 조준 시 반동
    public float aimRecoilY;
    public float aimRecoilZ;

    [Header("Reloading")]
    public AMMOTYPE ammoType;
    public int currentAmmo;
    public int magSize;
    public float reloadTime;

    [Header("Effect")]
    public GameObject serverBulletPrefab;
    public GameObject clientBulletPrefab;

    [HideInInspector] public bool reloading;
    [HideInInspector] public bool isFirstRef = true; // 맨 처음 gundata 사용 시 clone해야함.
    

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            InitAttachmentDict();
        }
    }

    public void InitAttachmentDict()
    {
        foreach (var attachmentType in availableAttachmentTypes)
        {
            attachments.Add(attachmentType, null); // 부착물 딕셔너리 초기화
        }
    }

    public void EquipAttachment(ScriptableAttachment attachment)
    {   
        if(!attachments.ContainsKey(attachment.attachmentType))
        {
            Debug.Log("장착 할 수 없는 부착물");
            return;
        }

        // 이미 해당 부착물 타입 장착중일 시 먼저 해제 
        if (attachments[attachment.attachmentType] != null)
        {
            UnequipAttachment(attachment.attachmentType);
        }

        attachments[attachment.attachmentType] = attachment;
        attachment.ApplyAttachmentEffect();
    }

    public void UnequipAttachment(AttachmentType attachmenttype)
    {
        attachments[attachmenttype].RemoveAttachmentEffect();
        attachments[attachmenttype] = null;
    }

}

public enum AttachmentType
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

public enum AMMOTYPE
{
    AMMO_9,
    AMMO_556,
    AMMO_762,
    GAUGE_12
}
