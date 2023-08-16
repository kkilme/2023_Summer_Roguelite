using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class Inventory : NetworkBehaviour
{
    public NetworkVariable<InventoryItem> WeaponItem;
    public NetworkVariable<InventoryItem> SubWeaponItem;
    public NetworkVariable<InventoryItem> HeadItem;
    public NetworkVariable<InventoryItem> ClothItem;

    private void EquipInit()
    {
        WeaponItem = new NetworkVariable<InventoryItem>();
        SubWeaponItem = new NetworkVariable<InventoryItem>();
        HeadItem = new NetworkVariable<InventoryItem>();
        ClothItem = new NetworkVariable<InventoryItem>();
    }

    public InventoryItem SelectEquip(ITEMNAME item, Vector2Int pos)
    {
        InventoryItem invenItem = new InventoryItem();
        switch (item)
        {
            case ITEMNAME.WEAPONEND:
                if (WeaponItem.Value.itemName != ITEMNAME.NONE)
                {
                    var equip = Item.GetUsableItem(item);
                    Debug.Log(equip.ItemName);
                    equip.Use(curPlayer);
                    invenItem = WeaponItem.Value;
                    invenItem.posX = pos.x;
                    invenItem.posY = pos.y;
                    items.Add(invenItem);
                    WeaponItem.Value = new InventoryItem();
                }
                break;
            case ITEMNAME.SUBWEAPONEND:
                if (SubWeaponItem.Value.itemName != ITEMNAME.NONE)
                {
                    var equip = Item.GetUsableItem(ITEMNAME.SUBWEAPONEND);
                    equip.Use(curPlayer);
                    invenItem = SubWeaponItem.Value;
                    invenItem.posX = pos.x;
                    invenItem.posY = pos.y;
                    items.Add(invenItem);
                    SubWeaponItem.Value = new InventoryItem();
                }
                break;
            case ITEMNAME.HEADEND:
                if (HeadItem.Value.itemName != ITEMNAME.NONE)
                {
                    var equip = Item.GetUsableItem(ITEMNAME.HEADEND);
                    equip.Use(curPlayer);
                    invenItem = HeadItem.Value;
                    invenItem.posX = pos.x;
                    invenItem.posY = pos.y;
                    items.Add(invenItem);
                    HeadItem.Value = new InventoryItem();
                }
                break;
            case ITEMNAME.CLOTHEND:
                if (ClothItem.Value.itemName != ITEMNAME.NONE)
                {
                    var equip = Item.GetUsableItem(ITEMNAME.CLOTHEND);
                    equip.Use(curPlayer);
                    invenItem = ClothItem.Value;
                    invenItem.posX = pos.x;
                    invenItem.posY = pos.y;
                    items.Add(invenItem);
                    ClothItem.Value = new InventoryItem();
                }
                break;
        };
        return invenItem;
    }

    [ServerRpc]
    public void EquipServerRPC(InventoryItem item, ServerRpcParams serverRpcParams = default)
    {
        bool bEquip = false;

        switch (item)
        {
            case var _ when item.itemName > ITEMNAME.EQUIPSTART && item.itemName < ITEMNAME.WEAPONEND:
                if (WeaponItem.Value.itemName == ITEMNAME.NONE)
                {
                    WeaponItem.Value = item;
                    bEquip = true;
                }
                break;
            case var _ when item.itemName > ITEMNAME.WEAPONEND && item.itemName < ITEMNAME.SUBWEAPONEND:
                if (SubWeaponItem.Value.itemName == ITEMNAME.NONE)
                {
                    SubWeaponItem.Value = item;
                    bEquip = true;
                }
                break;
            case var _ when item.itemName > ITEMNAME.SUBWEAPONEND && item.itemName < ITEMNAME.HEADEND:
                if (HeadItem.Value.itemName == ITEMNAME.NONE)
                {
                    HeadItem.Value = item;
                    bEquip = true;
                }
                break;
            case var _ when item.itemName > ITEMNAME.HEADEND && item.itemName < ITEMNAME.CLOTHEND:
                if (ClothItem.Value.itemName == ITEMNAME.NONE)
                {
                    ClothItem.Value = item;
                    bEquip = true;
                }
                break;
        };

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
            }
        };

        if (!bEquip)
        {
            //장착 실패 -> 클라에게 메세지 띄우기
            Debug.Log("장착실패!");

            // 인벤토리가 변경되지 않았기에 다시 클라에게 인벤토리 호출을 해 원상태로 복귀 시킴
            MoveItemClientRPC(clientRpcParams);
        }

        else
        {
            RemoveItem(item);
            var equip = Item.GetUsableItem(item.itemName);
            equip.Use(curPlayer);
            //inventoryUI clientRpc로 전달
            inventoryUI.EquipUISetClientRpc(item, clientRpcParams);
        }
    }
}
