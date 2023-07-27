using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

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