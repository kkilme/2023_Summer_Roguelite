using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AMMO_TYPE
{   
    NONE,
    AMMO_9,
    AMMO_556,
    AMMO_762,
    GAUGE_12
}

public struct GunData : INetworkSerializable
{
    public ITEMNAME gunName;
    public string gunNameStr;

    public float damage;
    //public float maxDistance; // 총알을 사용하기 때문에 일단 사용 안함
    public float bulletSpeed;
    public int bulletsPerShoot;
    public float bulletLifetime;
    public float spreadRate; // 탄퍼짐

    public float zoomSpeed; // 우클릭시 조준으로 전환되는 속도
    public float zoomRate; // 조준 배율
    
    public float fireRate; // 1초당 발사 수
    public bool isAutofire; // 자동사격 여부

    //public ATTACHMENT_TYPE[] availableAttachmentTypes; // 장착 가능한 부착물
    //public ATTACHMENT_NAME[] equippedAttachments; // 장착중인 부착물

    public AttachmentTypeList availableAttachmentTypes; // 장착 가능한 부착물
    public AttachmentDictionary equippedAttachments; // 장착중인 부착물

    // 반동
    public float recoilX;
    public float recoilY;
    public float recoilZ;
    public float aimRecoilX; // 조준 시 반동
    public float aimRecoilY;
    public float aimRecoilZ;

    public AMMO_TYPE ammoType;
    public int currentAmmo;
    public int magSize;
    public float reloadTime;

    public bool isReloading;

    public float basePosX;
    public float basePosY;
    public float basePosZ;
    public float baseRotX;
    public float baseRotY;
    public float baseRotZ;

    public int hashcode; // 유니크하게 수정할 필요가 잇을 듯?

    public GunData(ITEMNAME gunName, string gunNameStr, float damage, float bulletSpeed, int bulletsPerShoot, float bulletLifetime, float spreadRate, float zoomSpeed, float zoomRate, float fireRate, bool isAutofire, AttachmentTypeList availableAttachmentTypes, float recoilX, float recoilY, float recoilZ, float aimRecoilX, float aimRecoilY, float aimRecoilZ, AMMO_TYPE ammoType, int currentAmmo, int magSize, float reloadTime, float basePosX, float basePosY, float basePosZ, float baseRotX, float baseRotY, float baseRotZ) : this()
    {
        this.gunName = gunName;
        this.gunNameStr = gunNameStr;
        this.damage = damage;
        this.bulletSpeed = bulletSpeed;
        this.bulletsPerShoot = bulletsPerShoot;
        this.bulletLifetime = bulletLifetime;
        this.spreadRate = spreadRate;
        this.zoomSpeed = zoomSpeed;
        this.zoomRate = zoomRate;
        this.fireRate = fireRate;
        this.isAutofire = isAutofire;
        this.availableAttachmentTypes = availableAttachmentTypes;
        this.equippedAttachments = new AttachmentDictionary();
        this.recoilX = recoilX;
        this.recoilY = recoilY;
        this.recoilZ = recoilZ;
        this.aimRecoilX = aimRecoilX;
        this.aimRecoilY = aimRecoilY;
        this.aimRecoilZ = aimRecoilZ;
        this.ammoType = ammoType;
        this.currentAmmo = currentAmmo;
        this.magSize = magSize;
        this.reloadTime = reloadTime;
        this.isReloading = false;
        this.basePosX = basePosX;
        this.basePosY = basePosY;
        this.basePosZ = basePosZ;
        this.baseRotX = basePosX;
        this.baseRotY = basePosY;
        this.baseRotZ = basePosZ;
        this.hashcode = 1111;
        this.hashcode = GetHashCode();
    }


    public GunData(ITEMNAME gunName, string gunNameStr, float damage, float zoomSpeed, float zoomRate, float fireRate, bool isAutofire, AttachmentTypeList availableAttachmentTypes, float recoilX, float recoilY, float recoilZ, float aimRecoilX, float aimRecoilY, float aimRecoilZ, AMMO_TYPE ammoType, int magSize, float reloadTime, float basePosX, float basePosY, float basePosZ, float baseRotX, float baseRotY, float baseRotZ) : this()
    {
        this.gunName = gunName;
        this.gunNameStr = gunNameStr;
        this.damage = damage;
        this.bulletSpeed = 30;
        this.bulletsPerShoot = 1;
        this.bulletLifetime = 5;
        this.spreadRate = 0;
        this.zoomSpeed = zoomSpeed;
        this.zoomRate = zoomRate;
        this.fireRate = fireRate;
        this.isAutofire = isAutofire;
        this.availableAttachmentTypes = availableAttachmentTypes;
        this.equippedAttachments = new AttachmentDictionary();
        this.recoilX = recoilX;
        this.recoilY = recoilY;
        this.recoilZ = recoilZ;
        this.aimRecoilX = aimRecoilX;
        this.aimRecoilY = aimRecoilY;
        this.aimRecoilZ = aimRecoilZ;
        this.ammoType = ammoType;
        this.currentAmmo = magSize;
        this.magSize = magSize;
        this.reloadTime = reloadTime;
        this.isReloading = false;
        this.basePosX = basePosX;
        this.basePosY = basePosY;
        this.basePosZ = basePosZ;
        this.baseRotX = basePosX;
        this.baseRotY = basePosY;
        this.baseRotZ = basePosZ;
        this.hashcode = 1111;
        this.hashcode = GetHashCode();
    }

    // C# 10.0 미만에서 구조체는 매개변수 없는 생성자 불가
    //public GunData(ITEMNAME gunName = ITEMNAME.TESTASSAULTRIFLE)
    //{
    //    this.gunName = gunName;
    //    this.damage = 10;
    //    this.bulletSpeed = 30;
    //    this.bulletsPerShoot = 1;
    //    this.bulletLifetime = 5;
    //    this.spreadRate = 0;
    //    this.zoomSpeed = 5;
    //    this.zoomRate = 1.2f;
    //    this.fireRate = 5;
    //    this.isAutofire = true;
    //    this.availableAttachmentTypes = new AttachmentTypeList { ATTACHMENT_TYPE.Scope, ATTACHMENT_TYPE.Stock, ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Muzzle, ATTACHMENT_TYPE.Grip, ATTACHMENT_TYPE.Flashlight };
    //    this.equippedAttachments = new AttachmentDictionary();
    //    this.recoilX = -1.2f;
    //    this.recoilY = 0.4f;
    //    this.recoilZ = 0;
    //    this.aimRecoilX = -0.6f;
    //    this.aimRecoilY = 0.2f;
    //    this.aimRecoilZ = 0;
    //    this.ammoType = AMMO_TYPE.AMMO_556;
    //    this.currentAmmo = 180;
    //    this.magSize = 180;
    //    this.reloadTime = 1.5f;
    //    this.isReloading = false;
    //    this.hashcode = 1111;
    //    this.hashcode = GetHashCode();
    //}

    public GunData(ITEMNAME gunName = ITEMNAME.NONE)
    {
        this.gunName = gunName;
        this.gunNameStr = "None";
        this.damage = 0;
        this.bulletSpeed = 0;
        this.bulletsPerShoot = 0;
        this.bulletLifetime = 0;
        this.spreadRate = 0;
        this.zoomSpeed = 0;
        this.zoomRate = 0f;
        this.fireRate = 0;
        this.isAutofire = false;
        this.availableAttachmentTypes = new AttachmentTypeList { };
        this.equippedAttachments = new AttachmentDictionary();
        this.recoilX = 0;
        this.recoilY = 0;
        this.recoilZ = 0;
        this.aimRecoilX = 0;
        this.aimRecoilY = 0;
        this.aimRecoilZ = 0;
        this.ammoType = AMMO_TYPE.NONE;
        this.currentAmmo = 0;
        this.magSize = 0;
        this.reloadTime = 999;
        this.isReloading = true;
        this.hashcode = 1111;
        this.basePosX = 0;
        this.basePosY = 0;
        this.basePosZ = 0;
        this.baseRotX = 0;
        this.baseRotY = 0;
        this.baseRotZ = 0;
        this.hashcode = GetHashCode();
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gunName);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref bulletSpeed);
        serializer.SerializeValue(ref bulletsPerShoot);
        serializer.SerializeValue(ref bulletLifetime);
        serializer.SerializeValue(ref spreadRate);
        serializer.SerializeValue(ref zoomSpeed);
        serializer.SerializeValue(ref zoomRate);
        serializer.SerializeValue(ref fireRate);
        serializer.SerializeValue(ref isAutofire);

        serializer.SerializeValue(ref recoilX);
        serializer.SerializeValue(ref recoilY);
        serializer.SerializeValue(ref recoilZ);
        serializer.SerializeValue(ref aimRecoilX);
        serializer.SerializeValue(ref aimRecoilY);
        serializer.SerializeValue(ref aimRecoilZ);

        serializer.SerializeValue(ref ammoType);
        serializer.SerializeValue(ref currentAmmo);
        serializer.SerializeValue(ref magSize);
        serializer.SerializeValue(ref reloadTime);

        serializer.SerializeValue(ref isReloading);

        if (serializer.IsReader)
        {
            availableAttachmentTypes = GlobalVariable.DummyAttachmentTypeList;
            equippedAttachments = GlobalVariable.DummyAttachmentDict; 
        }
        serializer.SerializeValue(ref availableAttachmentTypes);
        serializer.SerializeValue(ref equippedAttachments);

        serializer.SerializeValue(ref hashcode);
    }

    public void Init()
    {
        if (gunName != ITEMNAME.NONE) InitAttachmentDict();
    }
    /// <summary>
    /// AttachmentTypeList를 사용하여 Key값은 ATTACHMENT_TYPE, Value값은 ATTACHMENT_NAME.None으로 equippedAttachments 초기화. 
    /// 생성자에서 하고싶으나, 구조체의 완전한 생성 이전엔 this.availableAttachmentTypes를 사용할 수 없다고 오류가 떴기에 Init()에서 호출함.
    /// </summary>
    /// <returns></returns>
    private void InitAttachmentDict()
    {
        if (equippedAttachments.Count == availableAttachmentTypes.Count) return;
        foreach (ATTACHMENT_TYPE attachmentType in this.availableAttachmentTypes)
        {
            equippedAttachments.Add(attachmentType, ATTACHMENT_NAME.None);
        }
    }

    // 해당 부착물이 장착 가능한지 체크
    public bool CheckEquippableAttachment(Attachment attachment)
    {
        if (!availableAttachmentTypes.Contains(attachment.attachmentType))
        {
            Debug.Log("This attachment is not equippable on this weapon!");
            return false;
        }
        else return true;
    }

}


public class AttachmentDictionary : Dictionary<ATTACHMENT_TYPE, ATTACHMENT_NAME>, INetworkSerializable
{
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int count = Count;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader) // IsReader: 역직렬화
        {
            Clear();
            for (int i = 0; i < count; i++)
            {
                ATTACHMENT_TYPE key = default;
                ATTACHMENT_NAME value = default;

                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref value);

                Add(key, value);
            }
        }
        else // IsWriter: 직렬화
        {
            foreach (var kvp in this)
            {
                ATTACHMENT_TYPE key = kvp.Key;
                ATTACHMENT_NAME value = kvp.Value;
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref value);
            }
        }
    }
}

public class AttachmentTypeList : List<ATTACHMENT_TYPE>, INetworkSerializable
{
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {   
        int count = Count;
        serializer.SerializeValue(ref count);
        if (serializer.IsReader) // IsReader: 역직렬화
        {
            Clear();
            for (int i = 0; i < count; i++)
            {
                ATTACHMENT_TYPE value = default;
                serializer.SerializeValue(ref value);
                Add(value);
            }
        }
        else // IsWriter: 직렬화
        {
            for (int i = 0; i < count; i++)
            {
                ATTACHMENT_TYPE type = this[i];
                serializer.SerializeValue(ref type);
            }
        }

        //int length = 0;
        //ATTACHMENT_TYPE[] Array;
        //if (!serializer.IsReader)
        //{
        //    Array = availableAttachmentTypes.ToArray();
        //    length = Array.Length;
        //    serializer.SerializeValue(ref length);
        //}
        //else
        //{
        //    availableAttachmentTypes = new AttachmentTypeList();
        //    serializer.SerializeValue(ref length);
        //    Array = new ATTACHMENT_TYPE[length];
        //}

        //for (int n = 0; n < length; ++n)
        //{
        //    serializer.SerializeValue(ref Array[n]);
        //}

        //if (serializer.IsReader)
        //{
        //    // 역직렬화 후 데이터 설정
        //    availableAttachmentTypes.AddRange(Array);
        //}
    }
}