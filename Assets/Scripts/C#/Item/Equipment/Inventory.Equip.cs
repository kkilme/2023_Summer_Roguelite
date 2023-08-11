using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Progress;

public partial class Inventory : NetworkBehaviour
{
    public NetworkVariable<InventoryItem> WeaponItem;
    public NetworkVariable<InventoryItem> SubWeaponItem;
    public NetworkVariable<InventoryItem> HeadItem;
    public NetworkVariable<InventoryItem> ClothItem;

    [ServerRpc]
    public void EquipServerRPC(InventoryItem item, ServerRpcParams serverRpcParams = default)
    {
        bool bEquip = false;

        switch (item)
        {
            //무기
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

        if (!bEquip)
        {
            //장착 실패 -> 클라에게 메세지 띄우기
            Debug.Log("장착실패!");
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };

            // 인벤토리가 변경되지 않았기에 다시 클라에게 인벤토리 호출을 해 원상태로 복귀 시킴
            MoveItemClientRPC(clientRpcParams);
        }

        else
        {
            RemoveItemServerRPC(item, serverRpcParams);
            var equip = Item.GetUsableItem(item.itemName);
            equip.Use(curPlayer);
        }
    }

    [ServerRpc]
    public void RemoveEquipServerRPC(ITEMNAME itemName, ServerRpcParams serverRpcParams = default)
    {
        switch (itemName)
        {
            //무기
            case var _ when itemName > ITEMNAME.EQUIPSTART && itemName < ITEMNAME.WEAPONEND:
                if (WeaponItem.Value.itemName != ITEMNAME.NONE)
                    Debug.Log("OK");
                break;
            case var _ when itemName > ITEMNAME.WEAPONEND && itemName < ITEMNAME.SUBWEAPONEND:
                if (SubWeaponItem.Value.itemName != ITEMNAME.NONE)
                    Debug.Log("OK");
                break;
            case var _ when itemName > ITEMNAME.SUBWEAPONEND && itemName < ITEMNAME.HEADEND:
                if (HeadItem.Value.itemName != ITEMNAME.NONE)
                    Debug.Log("OK");
                break;
            case var _ when itemName > ITEMNAME.HEADEND && itemName < ITEMNAME.CLOTHEND:
                if (ClothItem.Value.itemName != ITEMNAME.NONE)
                    Debug.Log("OK");
                break;
        };
    }
}
