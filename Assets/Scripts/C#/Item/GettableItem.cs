using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

// 플레이어가 획득하는 아이템. 프리팹 형태로 존재
public class GettableItem : NetworkBehaviour, IInteraction
{
    [SerializeField] protected NetworkVariable<ITEMNAME> itemName = new NetworkVariable<ITEMNAME>(); // 획득하는 아이템
    [SerializeField] protected NetworkVariable<int> itemCount = new NetworkVariable<int>(); // 들어있는 아이템 갯수

    public ITEMNAME ItemName { get => itemName.Value; }
    public int ItemCount { get => itemCount.Value; }

    List<Player> players = new List<Player>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
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

    public override void OnNetworkDespawn()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Inventory.RemoveNearItem(this);
        }
    }

    public static GameObject GetItemPrefab(ITEMNAME itemName)
    {
        StringBuilder path = new StringBuilder("Item/");

        switch (itemName)
        {
            case ITEMNAME.CANNED_FOOD:
                path.Append("CannedFood");
                break;
            case ITEMNAME.AMMO_9:
                path.Append("9mm Ammo");
                break;
            case ITEMNAME.AMMO_556:
                path.Append("5.56mm Ammo");
                break;
            case ITEMNAME.AMMO_762:
                path.Append("7.62mm Ammo");
                break;
            case ITEMNAME.GAUGE_12:
                path.Append("12Gauge Ammo");
                break;
            case ITEMNAME.JERRY_CAN:
                path.Append("Jerry Can");
                break;
            case ITEMNAME.TESTHEAD:
                path.Append("Test Head");
                break;
            case ITEMNAME.TESTRAREHEAD:
                path.Append("Test Rare Head");
                break;
            case ITEMNAME.TESTASSAULTRIFLE:
                path.Append("TestAssaultRifle");
                break;
            case ITEMNAME.TESTMACHINEGUN:
                path.Append("TestMachinegun");
                break;
            case ITEMNAME.ASSAULTRIFLE:
                path.Append("AssaultRifle");
                break;
            case ITEMNAME.SNIPERRIFLE:
                path.Append("SniperRifle");
                break;
            case ITEMNAME.SHOTGUN:
                path.Append("Shotgun");
                break;
            default:
                path.Append("");
                break;
        }

        return GameManager.Resource.GetObject<GameObject>(path.ToString());
    }
    public void Interactable(bool bCan)
    {
        Debug.Log(bCan);
    }
}
