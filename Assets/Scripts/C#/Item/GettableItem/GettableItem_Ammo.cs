using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettableItem_Ammo : GettableItem
{
    public override void Interact(Player player)
    {
        var item = Item.GetItem(itemName);
        player.Inventory.PutItemServerRPC(itemName);
    }

    public override void InteractComplete(bool bSuccess)
    {
    }
}
