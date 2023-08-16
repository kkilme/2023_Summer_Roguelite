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