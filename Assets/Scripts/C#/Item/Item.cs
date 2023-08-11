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
    //WEAPONSTART = 1001,
    WEAPONEND = 1100,
    //SUBWEAPONSTART = 1101,
    SUBWEAPONEND = 1200,
    //HEADSTART = 1201,
    TESTHEAD,
    TESTRAREHEAD,
    HEADEND = 1300,
    //CLOTHSTART = 1301,
    CLOTHEND = 1400,
    EQUIPEND = 2000
}

public abstract class Item
{
    public static Dictionary<ITEMNAME, Storage.StorageItemData> itemDataDic = new Dictionary<ITEMNAME, Storage.StorageItemData> ();

    public ITEMNAME ItemName { get; protected set; }

    public abstract void Use(Player player);

    /// <summary>
    /// 사용가능한 아이템을 리턴하는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static Item GetUsableItem(ITEMNAME item)
    {
        return item switch
        {
            ITEMNAME.BANDAGE => new Item_Bandage(),
            > ITEMNAME.EQUIPSTART and < ITEMNAME.EQUIPEND => new EquipItem(item),
            _ => null,
        };
    }

    public static InventoryItem GetInventoryItem(ITEMNAME itemName, ROTATION_TYPE rotationType, int count, int posX, int posY)
    {
        return new InventoryItem(itemName, rotationType, count, posX, posY);
    }
}
