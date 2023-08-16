using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Ammo : Item
{
    protected AMMO_TYPE ammoType;

    public Item_Ammo(AMMO_TYPE ammoType, ITEMNAME itemName)
    {
        this.ammoType = ammoType;
        this.ItemName = itemName;
    }

    public override bool Use(Player player) { return true; }
}
