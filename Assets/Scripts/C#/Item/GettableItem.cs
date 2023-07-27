using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// 플레이어가 획득하는 아이템. 프리팹 형태로 존재
public class GettableItem : NetworkBehaviour, IInteraction
{
    [SerializeField] protected ITEMNAME itemName; // 획득하는 아이템
    [SerializeField] protected int itemCount; // 들어있는 아이템 갯수

    public ITEMNAME ItemName { get => itemName; }
    public int ItemCount { get => itemCount; }

    public void Init()
    {

    }

    public virtual void Interact(Player player)
    {
        //player.Inventory.PutItemServerRPC(itemName);
    }
    public virtual void InteractComplete(bool bSuccess)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().Inventory.AddNearItem(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().Inventory.RemoveNearItem(this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRPC()
    {
        DespawnClientRPC();
        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void DespawnClientRPC()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject.GetComponent<Player>().Inventory.RemoveNearItem(this);
        }
    }
}
