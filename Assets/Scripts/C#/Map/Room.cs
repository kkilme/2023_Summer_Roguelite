using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ROOMSIZE
{
    SMALL, // 25 * 25
    MEDIUM, // 40 * 40
    LARGE // 65 * 40
}

public enum ROOMTYPE
{
    NECESSARY_START, // 사이에 있는 방들은 무조건 생성됨
    TEST1,
    NECESSARY_END,
    TEST2,
    TEST3,
    SPECIAL_START,
    SPECIAL_END
}

public class Room : MonoBehaviour
{
    public Transform[] itemPlaces; // 아이템들을 배치하기 위한 위치들
    public ROOMSIZE roomSize;
    public ROOMTYPE roomType;
}
