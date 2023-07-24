using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettableItem_Ammo : GettableItem
{
    public override void Interact(Player player)
    {
        var item = Item.GetItem(itemName);
        if (!player.Inventory.PutItem(item))
        {
            // 아이템 자동 넣기 실패
        }
    }

    public override void InteractComplete(bool bSuccess)
    {
    }
}
