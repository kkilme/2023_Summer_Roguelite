using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.UIElements;

public struct InventoryItem : INetworkSerializable, System.IEquatable<InventoryItem>
{
    public ITEMNAME itemName;
    public ROTATION_TYPE rotationType;
    public int currentCount;
    public int maxCount;
    public int sizeX, sizeY;
    public int posX, posY;
    public FixedString128Bytes hashCode;

    public InventoryItem(ITEMNAME itemName, ROTATION_TYPE rotationType, int currentCount, int posX, int posY)
    { 
        this.itemName = itemName;

        var data = Item.itemDataDic[itemName];

        this.rotationType = rotationType;
        this.currentCount = currentCount;
        maxCount = data.maxCount;
        sizeX = data.sizeX;
        sizeY = data.sizeY;
        this.posX = posX;
        this.posY = posY;
        hashCode = "";
        hashCode = Util.GetRealHashCode();
    }

    public bool Equals(InventoryItem other)
    {
        return (itemName == other.itemName && hashCode == other.hashCode);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref rotationType);
        serializer.SerializeValue(ref currentCount);
        serializer.SerializeValue(ref maxCount);
        serializer.SerializeValue(ref sizeX);
        serializer.SerializeValue(ref sizeY);
        serializer.SerializeValue(ref posX);
        serializer.SerializeValue(ref posY);
        serializer.SerializeValue(ref hashCode);
    }
}
