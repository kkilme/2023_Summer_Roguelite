using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public struct ItemStat : INetworkSerializable, System.IEquatable<ItemStat>
{
    public FixedString32Bytes name;
    public FixedString32Bytes description;
    public int sizeX, sizeY;
    public int currentCount;
    public int maxCount; // 한 공간에 최대로 들어갈 수 있는 갯수

    public ItemStat(FixedString32Bytes name, FixedString32Bytes description, int sizeX, int sizeY, int currentCount = 1, int maxCount = 1)
    {
        this.name = name;
        this.description = description;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.currentCount = currentCount;
        this.maxCount = maxCount;
    }

    public bool Equals(ItemStat other)
    {
        return name.Equals(other.name) && description.Equals(other.description) && sizeX.Equals(other.sizeX) && sizeY.Equals(other.sizeY)
        && currentCount == other.currentCount && maxCount == other.maxCount;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref description);
        serializer.SerializeValue(ref sizeX);
        serializer.SerializeValue(ref sizeY);
        serializer.SerializeValue(ref currentCount);
        serializer.SerializeValue(ref maxCount);
    }
}
