using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEMNAME
{
    NONE,
    CANNED_FOOD,
    AMMO_9,
    AMMO_556,
    AMMO_762,
    GAUGE_12,
    //JERRY_CAN,
    EQUIPSTART = 1000,
    //WEAPONSTART = 1000,
    ASSAULTRIFLE,
    SHOTGUN,
    SNIPERRIFLE,
    TESTASSAULTRIFLE,
    //TESTSHOTGUN,
    TESTMACHINEGUN,
    //TESTSNIPERRIFLE,
    //TESTHANDGUN,
    //TESTSUBMACHINEGUN,
    WEAPONEND = 1100,
    SUBWEAPONSTART = 1100,
    SUBWEAPONEND = 1200,
    HEADSTART = 1200,
    TESTHEAD,
    TESTRAREHEAD,
    HEADEND = 1300,
    CLOTHSTART = 1300,
    CLOTHEND = 1400,
    EQUIPEND = 2000
}

public abstract class Item
{
    public static Dictionary<ITEMNAME, Storage.StorageItemData> itemDataDic = new Dictionary<ITEMNAME, Storage.StorageItemData> ();

    public ITEMNAME ItemName { get; protected set; }

    public abstract bool Use(Player player);

    /// <summary>
    /// 사용가능한 아이템을 리턴하는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static Item GetUsableItem(ITEMNAME item)
    {
        return item switch
        {
            ITEMNAME.CANNED_FOOD => new Item_CannedFood(),
            > ITEMNAME.EQUIPSTART and <= ITEMNAME.WEAPONEND => new Item_Gun(item),
            > ITEMNAME.HEADSTART and < ITEMNAME.EQUIPEND => new ArmorItem(item),
            _ => null,
        };
    }

    public static InventoryItem GetInventoryItem(ITEMNAME itemName, ROTATION_TYPE rotationType, int count, int posX, int posY)
    {
        return new InventoryItem(itemName, rotationType, count, posX, posY);
    }
}
