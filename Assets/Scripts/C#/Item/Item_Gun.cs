using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Gun : Item
{
    protected GunData gunData;

    public Item_Gun(GunData gunData, ITEMNAME itemName)
    {
        this.gunData = gunData;
        this.ItemName = itemName;
    }

    public override bool Use(Player player) {
        player.Equip(gunData);
        return true; 
    }
}
