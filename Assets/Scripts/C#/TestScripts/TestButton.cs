using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class TestButton : MonoBehaviour
{   
    public Button button;
    public Gun gun;
    public Attachment testatt1;
    public Attachment testatt2;
    public Attachment testatt3;
    PlayerController playerController;
    void Start()
    {

    }
    void Update()
    {
        
    }

    public void testgamestart()
    {
        //playerController.enabled = true;
    }

    public void TestEquipAttachment1()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData.equippedAttachments[testatt1.attachmentType] == testatt1.attachmentName)
        {
            Debug.Log("Already-test");
            AttachmentSystem.UnequipAttachment(ref NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData, testatt1);
        }
        else
        {
            AttachmentSystem.EquipAttachment(ref NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData, testatt1);
        }

    }
    public void TestEquipAttachment2()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData.equippedAttachments[testatt2.attachmentType] == testatt2.attachmentName)
        {
            AttachmentSystem.UnequipAttachment(ref NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData, testatt2);
        }
        else
        {
            AttachmentSystem.EquipAttachment(ref NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData, testatt2);
        }
    }
    public void TestEquipAttachment3()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData.equippedAttachments[testatt3.attachmentType] == testatt3.attachmentName)
        {
            AttachmentSystem.UnequipAttachment(ref NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData, testatt3);
        }
        else
        {
            AttachmentSystem.EquipAttachment(ref NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<Gun>().GunData, testatt3);
        }
    }
}
