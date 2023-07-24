using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Ammo : Item
{
    protected AMMOTYPE ammoType;

    public Item_Ammo(AMMOTYPE ammoType, ItemStat itemStat)
    {
        this.ammoType = ammoType;
        this.itemStat = itemStat;
    }

    public override void Use(Player player) { }
}
