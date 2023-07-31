using Mono.Cecil;
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

    List<Player> players = new List<Player>();

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
            var player = other.GetComponent<Player>();
            player.Inventory.AddNearItem(this);
            players.Add(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            player.Inventory.RemoveNearItem(this);
            players.Remove(player);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRPC()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    public override void OnNetworkDespawn()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Inventory.RemoveNearItem(this);
        }
    }

    public static GameObject GetItemPrefab(ITEMNAME itemName)
    {
        string path = "Item/";

        switch (itemName)
        {
            case ITEMNAME.BANDAGE:
                path += "Bandage";
                break;
            case ITEMNAME.AMMO_9:
                path += "9mm Ammo";
                break;
            case ITEMNAME.AMMO_556:
                path += "5.56mm Ammo";
                break;
            case ITEMNAME.AMMO_762:
                path += "7.62mm Ammo";
                break;
            case ITEMNAME.GAUGE_12:
                path += "12Gauge Ammo";
                break;
            default:
                path += "";
                break;
        }

        return Resources.Load(path) as GameObject;
    }
}
