using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEMNAME
{
    NONE,
    BANDAGE,
    AMMO_9,
    AMMO_556,
    AMMO_762,
    GAUGE_12,
    JERRY_CAN,
    EQUIPSTART = 1000,
    WEAPONSTART = 1001,
    WEAPONEND = 1100,
    SUBWEAPONSTART = 1101,
    SUBWEAPONEND = 1200,
    HEADSTART = 1201,
    TESTHEAD,
    HEADEND = 1300,
    CLOTHSTART = 1301,
    CLOTHEND = 1400,
    EQUIPEND = 1999
}

public abstract class Item
{
    private ItemStat itemStat;
    public ItemStat ItemStat { get => itemStat; protected set => itemStat = value; }
    public ITEMNAME ItemName { get; protected set; }

    public abstract void Use(Player player);

    public static Item GetItem(ITEMNAME item, int count = 1)
    {
        switch (item)
        {
            case ITEMNAME.BANDAGE:
                return new Item_Bandage(count);
            case ITEMNAME.AMMO_9:
                return new Item_Ammo(AMMOTYPE.AMMO_9, new ItemStat("9mm Ammo", "총알", 1, 1, count, 30), ITEMNAME.AMMO_9);
            case ITEMNAME.AMMO_556:
                return new Item_Ammo(AMMOTYPE.AMMO_556, new ItemStat("5.56mm Ammo", "총알", 1, 1, count, 30), ITEMNAME.AMMO_556);
            case ITEMNAME.AMMO_762:
                return new Item_Ammo(AMMOTYPE.AMMO_762, new ItemStat("7.62mm Ammo", "총알", 1, 1, count, 30), ITEMNAME.AMMO_762);
            case ITEMNAME.GAUGE_12:
                return new Item_Ammo(AMMOTYPE.GAUGE_12, new ItemStat("12 Gauge", "총알", 2, 2, count, 12), ITEMNAME.GAUGE_12);
            case ITEMNAME.TESTHEAD:
                return new EquipItem(ITEMNAME.TESTHEAD, new ItemStat("Test Head", "머리장비", 3, 3));
 
            default:
                return null;
        }
    }

    public static ItemStat GetItemStat(ITEMNAME item, int count)
    {
        switch (item)
        {
            case ITEMNAME.BANDAGE:
                return new Item_Bandage(count).itemStat;
            case ITEMNAME.AMMO_9:
                return new ItemStat("9mm Ammo", "총알", 1, 1, count, 30);
            case ITEMNAME.AMMO_556:
                return new ItemStat("5.56mm Ammo", "총알", 1, 1, count, 30);
            case ITEMNAME.AMMO_762:
                return new ItemStat("7.62mm Ammo", "총알", 1, 1, count, 30);
            case ITEMNAME.GAUGE_12:
                return new ItemStat("12 Gauge", "총알", 2, 1, count, 12);
            case ITEMNAME.JERRY_CAN:
                return new ItemStat("Jerry Can", "제리캔", 2, 2, count, 1);
            default:
                return new ItemStat("9mm Ammo", "총알", 1, 1, count, 1);
        }
    }

    public static InventoryItem GetInventoryItem(ITEMNAME itemName, int count)
    {
        return new InventoryItem(itemName, GetItemStat(itemName, count));
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
