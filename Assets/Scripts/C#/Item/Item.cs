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
    JERRY_CAN
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
        switch (item)
        {
            case ITEMNAME.BANDAGE:
                return new Item_Bandage();
            default:
                return null;
        }
    }

    public static InventoryItem GetInventoryItem(ITEMNAME itemName, ROTATION_TYPE rotationType, int count, int posX, int posY)
    {
        return new InventoryItem(itemName, rotationType, count, posX, posY);
    }
}
