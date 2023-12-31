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
    NECESSARY_START = 0, // 사이에 있는 방들은 무조건 생성됨
    //ARMORY, // 무기고. 무기나 총알등이 배치되어 있음 - 제외
    MACHINE_ROOM, // 기계실. 엔진 부품등을 얻을 수 있음 (구호선 탈출 시 필요한 아이템)
    NECESSARY_END = 100,

    SPECIAL_START = 101,
    //APEX_LABORATORY, // 첨단 연구실. 특수 무기 획득 가능
    SPECIAL_END = 200,

    MEDICAL_ROOM = 201, // 의무실. 의약템 등이 있음
    //BED_ROOM, // 침실. 여러 잡템들이 있음 - 제외
    LABORATORY, // 실험실. 특수 템들이 있음
    MANAGEMENT_ROOM, // 관리실. 좀 더 좋은 잡템들이 있음
}

public class Room : MonoBehaviour
{
    //public Transform[] itemPlaces; // 아이템들을 배치하기 위한 위치들
    public Vector3[] ItemPlaces { get; private set; }
    [SerializeField]
    private Transform _itemSpawnPoint;
    public ROOMSIZE roomSize;
    public ROOMTYPE roomType;
    public MonsterSpawner monsterSpawners;

    private void Awake()
    {
        ItemPlaces = new Vector3[_itemSpawnPoint.childCount];
        monsterSpawners.Init();
        for (int i = 0; i < _itemSpawnPoint.childCount; ++i)
            ItemPlaces[i] = _itemSpawnPoint.GetChild(i).transform.position;
        //StaticBatchingUtility.Combine(gameObject);
    }
}
