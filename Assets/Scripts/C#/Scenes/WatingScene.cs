using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WatingScene : MonoBehaviour
{
    [SerializeField] private GameObject StorageObj;

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void TurnOnStorage()
    {
        StorageObj.SetActive(true);
    }

    public void TurnOnStore()
    {

    }

    public void Disconnect()
    {
        SceneManager.LoadScene("MainScene");
    }

    public async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await EconomyService.Instance.Configuration.SyncConfigurationAsync();

        // 아이템 데이터 생성
        foreach (ITEMNAME itemName in Enum.GetValues(typeof(ITEMNAME)))
            if (itemName != ITEMNAME.NONE && !Item.itemDataDic.ContainsKey(ITEMNAME.JERRY_CAN))
            {
                Item.itemDataDic.Add(itemName, EconomyService.Instance.Configuration.GetInventoryItem(itemName.ToString()).CustomDataDeserializable.GetAs<Storage.StorageItemData>());
            }
            
    }

    //private async void Test()
    //{
    //    var configAssignemntHash = EconomyService.Instance.Configuration.GetConfigAssignmentHash();

    //    try
    //    {
    //        var arguments = new IncrementBalanceParam("SCRAP", configAssignemntHash);
    //        var response = await CloudCodeService.Instance.CallModuleEndpointAsync<long>("StorageModule", "SayHello", new Dictionary<string, object> { { "param", arguments } });
    //        Debug.Log(response);
    //    }
    //    catch (CloudCodeException exception)
    //    {
    //        Debug.LogException(exception);
    //    }

    //}
}
