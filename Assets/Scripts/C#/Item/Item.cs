using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_NAME
{
    BANDAGE
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
            default:
                return null;
        }
    }
}
