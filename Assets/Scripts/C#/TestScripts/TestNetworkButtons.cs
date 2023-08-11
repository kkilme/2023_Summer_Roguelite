using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Economy;
using UnityEngine;

public class TestNetworkButtons : MonoBehaviour
{

    public void StartClient()
    {
        UnityServices.InitializeAsync();
        NetworkManager.Singleton.StartClient();
    }

    public async void StartHost()
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
        NetworkManager.Singleton.StartHost();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

}
