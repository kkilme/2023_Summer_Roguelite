using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Multiplay;
using Unity.Services.Economy;

/*
* Note that you need to have a published script in order to use the Cloud Code SDK.
* You can do that from the Unity Dashboard - https://dashboard.unity3d.com/
*/
public class TestServer : MonoBehaviour
{
    /*
    * The response from the script, used for deserialization.
    * In this example, the script would return a JSON in the format
    * {"welcomeMessage": "Hello, arguments['name']. Welcome to Cloud Code!"}
    */
    private class CloudCodeResponse
    {
        public string welcomeMessage;
    }

    /*
     * Initialize all Unity Services and Sign In an anonymous player.
     * You can perform this operation in a more centralized spot in your project
     */
    public async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await EconomyService.Instance.Configuration.SyncConfigurationAsync();
        
        // 아이템 데이터 생성
        foreach (ITEMNAME itemName in Enum.GetValues(typeof(ITEMNAME)))
            if ((int)itemName % 100 != 0)
            {
                Item.itemDataDic.TryAdd(itemName, EconomyService.Instance.Configuration.GetInventoryItem(itemName.ToString()).CustomDataDeserializable.GetAs<Storage.StorageItemData>());
                //Item.itemDataDic.Add(itemName, EconomyService.Instance.Configuration.GetInventoryItem(itemName.ToString()).CustomDataDeserializable.GetAs<Storage.StorageItemData>());
            }
    }

/*
* Populate a Dictionary<string,object> with the arguments and invoke the script.
* Deserialize the response into a CloudCodeResponse object
*/
public async void OnClick()
    {
        var client = CloudSaveService.Instance.Data;
        var data = new Dictionary<string, object> { { "test", "testData" } };
        await client.ForceSaveAsync(data);

        var query = await client.LoadAsync(new HashSet<string> { "test" });
        Debug.Log(query["test"]);
    }
}