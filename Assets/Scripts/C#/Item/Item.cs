using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_NAME
{
    BANDAGE
}

public enum ITEM_TYPE
{
    STAT
}

public abstract class Item
{
    protected ItemStat itemStat;
    protected ITEM_NAME itemName;
    protected ITEM_TYPE itemType;

    public virtual void Use() { }
    public virtual void Use(ref Stat stat) { }
}
