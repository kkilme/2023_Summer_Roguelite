using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;

    private Dictionary<ROOMSIZE, List<Room>> roomPrefabsDic = new Dictionary<ROOMSIZE, List<Room>>();

    [SerializeField] private RoomPosition[] roomPositions; 
    [SerializeField] private List<GameObject> rooms = new List<GameObject>(); // 현재 배치된 방들 리스트

    public MapManager Instance { get => _instance; }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;

            foreach (ROOMSIZE roomSize in Enum.GetValues(typeof(ROOMSIZE)))
                roomPrefabsDic.Add(roomSize, new List<Room>());

            var roomPrefabs = Resources.LoadAll<Room>("Room");
            for (int i = 0; i < roomPrefabs.Length; i++)
                roomPrefabsDic[roomPrefabs[i].roomSize].Add(roomPrefabs[i]);
        }
    }

    // 맵 생성 함수
    public void GenerateMap()
    {
        ClearMap();

        /*
        1. 각각의 위치에 방 종류를 랜덤으로 배정
        2. 해당 위치의 크기에 해당하는 방을 배치
        */

        for (int i = 0; i < roomPositions.Length; i++)
        {
            var roomList = roomPrefabsDic[roomPositions[i].roomSize];
            rooms.Add(Instantiate(roomList[Random.Range(0, roomList.Count)], roomPositions[i].transform.position, Quaternion.Euler(0, roomPositions[i].rotation, 0)).gameObject);
        }
    }

    // 기존 맵 초기화 함수
    private void ClearMap()
    {
        for (int i = 0; i < rooms.Count; i++)
            Destroy(rooms[i]);

        rooms.Clear();
    }
}
