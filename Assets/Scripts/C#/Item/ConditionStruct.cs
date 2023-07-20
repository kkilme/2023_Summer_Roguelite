using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ConditionStruct
{
    public float time;
    public int sizeX, sizeY;
    public ITEM_TYPE[] items;

    public ConditionStruct(float time, int sizeX, int sizeY, ITEM_TYPE[] items)
    {
        this.time = time;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.items = items;
    }
}
