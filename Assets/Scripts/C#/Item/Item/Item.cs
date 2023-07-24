using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_NAME
{
    BANDAGE,
    AMMO_9,
    AMMO_556,
    AMMO_762,
    GAUGE_12
}

public abstract class Item
{
    protected ItemStat itemStat;
    protected ITEM_NAME itemName;

    public abstract void Use(Player player);

    public ItemStat GetItemStat()
    {
        return itemStat;
    }

    public static Item GetItem(ITEM_NAME item)
    {
        switch (item)
        {
            case ITEM_NAME.BANDAGE:
                return new Item_Bandage();
            case ITEM_NAME.AMMO_9:
                return new Item_Ammo(AMMOTYPE.AMMO_9, new ItemStat("9mm Ammo", "총알", null, 1, 1));
            case ITEM_NAME.AMMO_556:
                return new Item_Ammo(AMMOTYPE.AMMO_556, new ItemStat("5.56mm Ammo", "총알", null, 1, 1));
            case ITEM_NAME.AMMO_762:
                return new Item_Ammo(AMMOTYPE.AMMO_762, new ItemStat("7.62mm Ammo", "총알", null, 1, 1));
            case ITEM_NAME.GAUGE_12:
                return new Item_Ammo(AMMOTYPE.GAUGE_12, new ItemStat("12 Gauge", "총알", null, 1, 1));
            default:
                return null;
        }
    }
}
