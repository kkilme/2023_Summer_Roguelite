using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
    public int hashCode;

    public InventoryItem(ITEMNAME itemName, ROTATION_TYPE rotationType, int currentCount, int maxCount, int sizeX, int sizeY, int posX, int posY)
    { 
        this.itemName = itemName;
        this.rotationType = rotationType;
        this.currentCount = currentCount;
        this.maxCount = maxCount;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.posX = posX;
        this.posY = posY;
        hashCode = 18181818;
        hashCode = GetHashCode();
    }

    public InventoryItem(ITEMNAME itemName, ItemStat itemStat)
    {
        this.itemName = itemName;
        rotationType = ROTATION_TYPE.RIGHT;
        currentCount = itemStat.currentCount;
        maxCount = itemStat.maxCount;
        sizeX = itemStat.sizeX;
        sizeY = itemStat.sizeY;
        posX = -1;
        posY = -1;
        hashCode = 18181818;
        hashCode = GetHashCode();
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
