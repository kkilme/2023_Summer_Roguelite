using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Ammo : Item
{
    protected AMMOTYPE ammoType;

    public Item_Ammo(AMMOTYPE ammoType, ItemStat itemStat, ITEMNAME itemName)
    {
        this.ammoType = ammoType;
        this.ItemStat = itemStat;
        this.ItemName = itemName;
    }

    public override void Use(Player player) { }
}
