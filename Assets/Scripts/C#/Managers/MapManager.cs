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
    [SerializeField] private List<GameObject> rooms = new List<GameObject>(); // ���� ��ġ�� ��� ����Ʈ

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

    // �� ���� �Լ�
    public void GenerateMap()
    {
        ClearMap();

        /*
        1. ������ ��ġ�� �� ������ �������� ����
        2. �ش� ��ġ�� ũ�⿡ �ش��ϴ� ���� ��ġ
        */

        for (int i = 0; i < roomPositions.Length; i++)
        {
            var roomList = roomPrefabsDic[roomPositions[i].roomSize];
            rooms.Add(Instantiate(roomList[Random.Range(0, roomList.Count)], roomPositions[i].transform.position, Quaternion.Euler(0, roomPositions[i].rotation, 0)).gameObject);
        }
    }

    // ���� �� �ʱ�ȭ �Լ�
    private void ClearMap()
    {
        for (int i = 0; i < rooms.Count; i++)
            Destroy(rooms[i]);

        rooms.Clear();
    }
}
