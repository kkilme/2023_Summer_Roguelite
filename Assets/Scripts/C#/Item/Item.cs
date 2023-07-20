using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_TYPE
{
    BANDAGE
}

public abstract class Item
{
    protected ItemStat itemStat;

    public abstract void Use();
    public abstract void Use(ref Stat stat);
}
