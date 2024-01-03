using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameManager : NetworkBehaviour
{   
    public static IngameManager Instance { get; set; }

    private Dictionary<ulong, bool> _playerStatus = new Dictionary<ulong, bool>();

    [SerializeField]
    private Transform _winnerUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        Init();
    }

    private void Init()
    {
        if (!IsServer) return;

        Button _gotomain = _winnerUI.GetComponentInChildren<Button>();
        _gotomain.onClick.AddListener(GoToMainScene);

        AddPlayerStatus();

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        //CheckWinner();
    }

    private void AddPlayerStatus()
    {
        _playerStatus.Clear();
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            Debug.Log(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
            _playerStatus.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId, true);
        }
    }

    public void OnPlayerDeath(ulong clientId)
    {
        if (_playerStatus.ContainsKey(clientId))
        {
            _playerStatus[clientId] = false;
        }
        CheckWinner();
    }

    private void CheckWinner()
    {
        int count = 0;
        foreach (var kvp in _playerStatus)
        {
            if (kvp.Value)
            {
                count++;
            }
        }

        if (count == 1)
        {
            ulong clientId = GetWinnerId();
            //if(clientId == 0)
            //{
            //    Debug.LogError("No winner exists");
            //    return;
            //}

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
            };
            ShowWinnerUIClientRPC(clientRpcParams);
        }
    }

    [ClientRpc]
    private void ShowWinnerUIClientRPC(ClientRpcParams clientRpcParams = default)
    {
        _winnerUI.gameObject.SetActive(true);
    }

    private ulong GetWinnerId()
    {
        foreach (var kvp in _playerStatus)
        {
            if (kvp.Value)
            {
                return kvp.Key;
            }
        }
        return 0;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        _playerStatus[clientId] = false;
        CheckWinner();
    }

    private void GoToMainScene()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton);
        SceneManager.LoadScene("MainScene");
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
        base.OnDestroy();
    }

}
