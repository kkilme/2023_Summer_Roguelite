using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상호작용 시 필요한 조건들을 담은 구조체
// 플레이어가 해당 구조체를 통해 조건들을 판단하도록 함
[System.Serializable]
public struct ConditionStruct
{
    public float time; // 상호작용하는데 필요한 시간
    public int sizeX, sizeY; // 상호작용 하는데 필요한 인벤토리 창 크기. 주로 아이템에서 사용
    public ITEM_NAME[] items; // 상호작용 하는데 필요한 아이템들

    public ConditionStruct(float time, int sizeX, int sizeY, ITEM_NAME[] items)
    {
        this.time = time;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.items = items;
    }
}
