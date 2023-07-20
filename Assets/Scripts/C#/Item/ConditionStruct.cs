using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��ȣ�ۿ� �� �ʿ��� ���ǵ��� ���� ����ü
// �÷��̾ �ش� ����ü�� ���� ���ǵ��� �Ǵ��ϵ��� ��
[System.Serializable]
public struct ConditionStruct
{
    public float time; // ��ȣ�ۿ��ϴµ� �ʿ��� �ð�
    public int sizeX, sizeY; // ��ȣ�ۿ� �ϴµ� �ʿ��� �κ��丮 â ũ��. �ַ� �����ۿ��� ���
    public ITEM_NAME[] items; // ��ȣ�ۿ� �ϴµ� �ʿ��� �����۵�

    public ConditionStruct(float time, int sizeX, int sizeY, ITEM_NAME[] items)
    {
        this.time = time;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.items = items;
    }
}
