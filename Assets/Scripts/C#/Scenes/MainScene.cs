using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnLocalClientConnectedCallback;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnLocalClientConnectedCallback;
        }
    }

    public void SetIP(string ip)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, (ushort)7777);
    }

    public void Connect()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("116.33.9.179", (ushort)7777);
        NetworkManager.Singleton.StartClient();
    }

    public void HostAndConnect()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", (ushort)7777);
        NetworkManager.Singleton.StartHost();
    }

    private void OnLocalClientConnectedCallback(ulong clientID)
    {
        if (NetworkManager.Singleton.LocalClientId == clientID)
        {
            SceneManager.LoadScene("WatingScene");
        }
    }
}
