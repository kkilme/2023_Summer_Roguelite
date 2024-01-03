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
    [SerializeField] private GameObject StoreObj;

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("KS", LoadSceneMode.Single);
        //NetworkManager.Singleton.SceneManager.LoadScene("SJ", LoadSceneMode.Single);
    }

    public void TurnOnStorage()
    {
        StorageObj.SetActive(true);
    }

    public void TurnOnStore()
    {
        StoreObj.SetActive(true);
    }

    public void Disconnect()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StoreObj.SetActive(false);
            StorageObj.SetActive(false);
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
