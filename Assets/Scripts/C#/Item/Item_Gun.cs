using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Gun : Item
{
    protected GunData gunData;

    public Item_Gun(ITEMNAME itemName)
    {
        this.ItemName = itemName;
        this.gunData = GunDataFactory.GetGunData(itemName);
    }

    public override bool Use(Player player) {
        player.Equip(gunData);
        return true; 
    }
}
