using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEMNAME
{
    BANDAGE,
    AMMO_9,
    AMMO_556,
    AMMO_762,
    GAUGE_12
}

public abstract class Item
{
    private ItemStat itemStat;
    public ItemStat ItemStat { get => itemStat; protected set => itemStat = value; }
    public ITEMNAME ItemName { get; protected set; }

    public abstract void Use(Player player);

    public static Item GetItem(ITEMNAME item, int count)
    {
        switch (item)
        {
            case ITEMNAME.BANDAGE:
                return new Item_Bandage();
            case ITEMNAME.AMMO_9:
                return new Item_Ammo(AMMOTYPE.AMMO_9, new ItemStat("9mm Ammo", "총알", null, 1, 1), ITEMNAME.AMMO_9);
            case ITEMNAME.AMMO_556:
                return new Item_Ammo(AMMOTYPE.AMMO_556, new ItemStat("5.56mm Ammo", "총알", null, 1, 1), ITEMNAME.AMMO_556);
            case ITEMNAME.AMMO_762:
                return new Item_Ammo(AMMOTYPE.AMMO_762, new ItemStat("7.62mm Ammo", "총알", null, 1, 1), ITEMNAME.AMMO_762);
            case ITEMNAME.GAUGE_12:
                return new Item_Ammo(AMMOTYPE.GAUGE_12, new ItemStat("12 Gauge", "총알", null, 2, 1, count, 12), ITEMNAME.GAUGE_12);
            default:
                return null;
        }
    }

    public void AddCount(int amount)
    {
        itemStat.currentCount += amount;
        if (itemStat.currentCount > itemStat.maxCount)
        {
            Debug.Log("Current Count Exceed Max Count");
            itemStat.currentCount = itemStat.maxCount;
        }
        else if (itemStat.currentCount < 0)
        {
            Debug.Log("Current Count is Less than Zero");
            itemStat.currentCount = 0;
        }
    }
}
