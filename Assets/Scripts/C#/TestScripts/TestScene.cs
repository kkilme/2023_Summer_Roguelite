using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.LowLevel;

public class TestScene : NetworkBehaviour
{
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private bool _enabled;
    public override void OnNetworkSpawn()
    {
        if (!IsServer || !_enabled) return;
        NetworkManager.Singleton.SceneManager.OnSceneEvent += SpawnPlayer;
    }

    private void SpawnPlayer(SceneEvent sceneEvent)
    {
        if (!(sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)) return;
        int playernum = NetworkManager.Singleton.ConnectedClientsList.Count;
        for (int i = 0; i < playernum; ++i)
        {
            var networkObj = Instantiate(_playerObject, new Vector3(0 + i * 3, 0.5f, 0), Quaternion.identity).GetComponent<NetworkObject>();
            networkObj.SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
            Debug.Log(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
        }
    }
}
