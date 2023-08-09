using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public struct Stat : INetworkSerializable
{
    public int MaxHp;
    public int Hp;
    public float Speed;
    public int Gold;
    public int Damage;
    public int Range;

    public Stat(int maxHp = 0, int hp = 0, 
        float speed = 0, int gold = 0, int damage = 0, int range = 0)
    {
        MaxHp = maxHp;
        Hp = hp;
        Speed = speed;
        Gold = gold;
        Damage = damage;
        Range = range;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref MaxHp);
        serializer.SerializeValue(ref Hp);
        serializer.SerializeValue(ref Speed);
        serializer.SerializeValue(ref Gold);
        serializer.SerializeValue(ref Damage);
        serializer.SerializeValue(ref Range);
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
