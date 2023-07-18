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
    NECESSARY_START, // ���̿� �ִ� ����� ������ ������
    TEST1,
    NECESSARY_END,
    TEST2,
    TEST3,
    SPECIAL_START,
    SPECIAL_END
}

public class Room : MonoBehaviour
{
    public Transform[] itemPlaces; // �����۵��� ��ġ�ϱ� ���� ��ġ��
    public ROOMSIZE roomSize;
    public ROOMTYPE roomType;
}
