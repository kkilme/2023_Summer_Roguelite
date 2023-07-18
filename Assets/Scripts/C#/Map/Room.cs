using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ROOMSIZE
{
    SMALL, // 25 * 25
    MEDIUM, // 40 * 40
    LARGE // 55 * 55
}

public enum ROOMTYPE
{
    TEST1,
    TEST2,
    TEST3
}

public class Room : MonoBehaviour
{
    public Transform[] itemPlaces; // 아이템들을 배치하기 위한 위치들
    public ROOMSIZE roomSize;
    public ROOMTYPE roomType;
}
