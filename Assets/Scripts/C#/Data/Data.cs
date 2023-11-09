using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public struct EquipStat : INetworkSerializable
{
    public int Durability;
    public int Armor;

    public EquipStat(int durability = 0, int armor = 0)
    {
        Durability = durability;
        Armor = armor;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Durability);
        serializer.SerializeValue(ref Armor);
    }

    public static EquipStat operator +(EquipStat a, EquipStat b)
    {
        a.Durability = a.Durability + b.Durability < 100 ? a.Durability + b.Durability : 100;
        a.Armor += b.Armor;
        return a;
    }
}

[Serializable]
public struct Stat : INetworkSerializable
{
    public int MaxHp;
    public int Hp;
    public float Speed;
    public int Gold;
    public int Damage;
    public int Range;
    public EquipStat HeadEquip;
    public EquipStat ClothEquip;

    public Stat(int maxHp = 0, int hp = 0, 
        float speed = 0, int gold = 0, int damage = 0, int range = 0, EquipStat head = new EquipStat(), EquipStat cloth = new EquipStat())
    {
        MaxHp = maxHp;
        Hp = hp;
        Speed = speed;
        Gold = gold;
        Damage = damage;
        Range = range;
        HeadEquip = head;
        ClothEquip = cloth;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref MaxHp);
        serializer.SerializeValue(ref Hp);
        serializer.SerializeValue(ref Speed);
        serializer.SerializeValue(ref Gold);
        serializer.SerializeValue(ref Damage);
        serializer.SerializeValue(ref Range);
        serializer.SerializeValue(ref HeadEquip);
        serializer.SerializeValue(ref ClothEquip);
    }

    public static Stat operator +(Stat a, Stat b)
    {
        a.Hp += b.Hp;
        if (a.Hp > a.MaxHp)
            a.Hp = a.MaxHp;
        a.Speed += b.Speed;
        a.Gold += b.Gold;
        a.Damage += b.Damage;
        a.Range += b.Range;
        return a;
    }
}

[Serializable]
public struct RoomInformation
{
    [field: SerializeField]
    public Vector3 Position { get; private set; }
    [field: SerializeField]
    public int Rotation { get; private set; }
    [field: SerializeField]
    public ROOMSIZE Size { get; private set; }

    public RoomInformation(Vector3 pos, int Rot, ROOMSIZE size)
    {
        Position = pos;
        Rotation = Rot;
        Size = size;
    }
}