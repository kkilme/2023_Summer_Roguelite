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
    ARMORY, // �����. ���⳪ �Ѿ˵��� ��ġ�Ǿ� ����
    MACHINE_ROOM, // ����. ���� ��ǰ���� ���� �� ���� (��ȣ�� Ż�� �� �ʿ��� ������)
    NECESSARY_END,
    MEDICAL_ROOM, // �ǹ���. �Ǿ��� ���� ����
    BED_ROOM, // ħ��. ���� ���۵��� ����
    LABORATORY, // �����. Ư�� �۵��� ����
    MANAGEMENT_ROOM, // ������. �� �� ���� ���۵��� ����
    SPECIAL_START,
    APEX_LABORATORY, // ÷�� ������. Ư�� ���� ȹ�� ����
    SPECIAL_END
}

public class Room : MonoBehaviour
{
    public Transform[] itemPlaces; // �����۵��� ��ġ�ϱ� ���� ��ġ��
    public ROOMSIZE roomSize;
    public ROOMTYPE roomType;
    public MonsterSpawner monsterSpawners;
}
