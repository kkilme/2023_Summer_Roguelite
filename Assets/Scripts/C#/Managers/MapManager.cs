using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MapManager : NetworkBehaviour
{
    private static MapManager _instance;

    private Dictionary<ROOMSIZE, List<Room>> roomPrefabsDic = new Dictionary<ROOMSIZE, List<Room>>();
    private Dictionary<ROOMTYPE, int[]> roomCountDic = new Dictionary<ROOMTYPE, int[]>(); // 0�� �ε����� ���� �� ī��Ʈ. 1�� �ε����� �ִ� �� ī��Ʈ
    private Dictionary<ROOMTYPE, float> specialRoomProbabilityDic = new Dictionary<ROOMTYPE, float>(); // �ش� �� Ÿ���� special Ÿ���� �� �ش� ���� ����Ȯ���� �������� (0 ~ 1)

    [SerializeField] private RoomPosition[] roomPositions;
    [SerializeField] private Transform[] lifeShipPositions;
    [SerializeField] private GameObject lifeShipPrefab;

    [SerializeField] private List<GameObject> rooms = new List<GameObject>(); // ���� ��ġ�� ��� ����Ʈ
    [SerializeField] private List<GameObject> lifeShips = new List<GameObject>(); // ���� ��ġ�� ������ ����Ʈ

    [SerializeField] private NavMeshSurface _testMap;

    [Header("Stat")]
    [SerializeField] private int lifeShipCount;

    public MapManager Instance { get => _instance; }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        //Debug.Log("Connect");
        //if (!IsServer && !IsHost)
        //{
        //    Destroy(this.gameObject);
        //    return;
        //}

        if (_instance == null)
        {
            _instance = this;

            foreach (ROOMSIZE roomSize in Enum.GetValues(typeof(ROOMSIZE)))
                roomPrefabsDic.Add(roomSize, new List<Room>());

            // ���Ŀ� �����ͺ��̽����� ���� �����ϵ��� ����
            roomCountDic.Add(ROOMTYPE.ARMORY, new int[] { 0, 4 });
            roomCountDic.Add(ROOMTYPE.MACHINE_ROOM, new int[] { 0, 2 });
            roomCountDic.Add(ROOMTYPE.APEX_LABORATORY, new int[] { 0, 1 });
            roomCountDic.Add(ROOMTYPE.BED_ROOM, new int[] { 0, 99 });
            roomCountDic.Add(ROOMTYPE.LABORATORY, new int[] { 0, 3 });
            roomCountDic.Add(ROOMTYPE.MANAGEMENT_ROOM, new int[] { 0, 5 });
            roomCountDic.Add(ROOMTYPE.MEDICAL_ROOM, new int[] { 0, 3 });

            specialRoomProbabilityDic.Add(ROOMTYPE.APEX_LABORATORY, 0.05f);

            var roomPrefabs = Resources.LoadAll<Room>("Room");
            for (int i = 0; i < roomPrefabs.Length; i++)
            {
                NetworkManager.AddNetworkPrefab(roomPrefabs[i].gameObject);
                roomPrefabsDic[roomPrefabs[i].roomSize].Add(roomPrefabs[i]);
            }
        }
    }

    // �� ���� �Լ�
    [ServerRpc]
    public void GenerateMapServerRPC()
    {
        ClearMap();

        /*
        1. ������ ��ġ�� �� ������ �������� ����
        2. �ش� ��ġ�� ũ�⿡ �ش��ϴ� ���� ��ġ
        */

        var roomPositionList = roomPositions.ToList();

        // �ʼ� ����� ��ġ
        foreach (ROOMTYPE roomType in Enum.GetValues(typeof(ROOMTYPE)))
        {
            if (roomType.Equals(ROOMTYPE.NECESSARY_START))
                continue;

            if (roomType.Equals(ROOMTYPE.NECESSARY_END))
                break;

            int idx = Random.Range(0, roomPositionList.Count);
            var roomList = roomPrefabsDic[roomPositionList[idx].roomSize];

            var obj = Instantiate(roomList.Find(x => x.roomType.Equals(roomType)), roomPositionList[idx].transform.position, Quaternion.Euler(0, roomPositionList[idx].rotation, 0)).gameObject;
            rooms.Add(obj);

            var networkObj = Util.GetOrAddComponent<NetworkObject>(obj);
            networkObj.Spawn();
            //networkObj.TrySetParent(roomPositionList[idx].transform);

            ++roomCountDic[roomType][0];
            roomPositionList.RemoveAt(idx);
        }

        // Ư���� ��ġ (Ȯ���� �����ϹǷ� ��ġ�� �ȵ� ���� ����)
        foreach (ROOMTYPE roomType in Enum.GetValues(typeof(ROOMTYPE)))
        {
            if (roomType <= ROOMTYPE.SPECIAL_START)
                continue;

            if (roomType.Equals(ROOMTYPE.SPECIAL_END))
                break;

            if (Random.Range(0f, 1f) <= specialRoomProbabilityDic[roomType])
            {
                int idx = Random.Range(0, roomPositionList.Count);
                var roomList = roomPrefabsDic[roomPositionList[idx].roomSize];

                var obj = Instantiate(roomList.Find(x => x.roomType.Equals(roomType)), roomPositionList[idx].transform.position, Quaternion.Euler(0, roomPositionList[idx].rotation, 0), roomPositionList[idx].transform).gameObject;
                rooms.Add(obj);

                var networkObj = Util.GetOrAddComponent<NetworkObject>(obj);
                networkObj.Spawn();

                //networkObj.TrySetParent(roomPositionList[idx].transform);

                ++roomCountDic[roomType][0];
                roomPositionList.RemoveAt(idx);
            }
        }

        // �Ϲ� ��� ��ġ
        for (int i = 0; i < roomPositionList.Count; i++)
        {
            var roomList = roomPrefabsDic[roomPositionList[i].roomSize];
            while (true)
            {
                var room = roomList[Random.Range(0, roomList.Count)];

                if (roomCountDic[room.roomType][0] >= roomCountDic[room.roomType][1])
                    continue;

                var obj = Instantiate(room, roomPositionList[i].transform.position, Quaternion.Euler(0, roomPositionList[i].rotation, 0), roomPositionList[i].transform).gameObject;
                rooms.Add(obj);
                //var networkObj = Util.GetOrAddComponent<NetworkObject>(obj);
                //networkObj.Spawn();

                //networkObj.TrySetParent(roomPositionList[i].transform);

                ++roomCountDic[room.roomType][0];
                break;
            }
        }

        // ���� ��ġ
        var lifeShipPositionList = lifeShipPositions.ToList(); 
        for (int i = 0; i < lifeShipCount; i++)
        {
            int idx = Random.Range(0, lifeShipPositionList.Count);

            var obj = Instantiate(lifeShipPrefab, lifeShipPositionList[idx].position, Quaternion.identity, lifeShipPositionList[i].transform);
            lifeShips.Add(obj);
            var networkObj = obj.GetComponent<NetworkObject>();
            networkObj.GetComponent<NetworkObject>().Spawn();

            networkObj.TrySetParent(lifeShipPositionList[i].transform);

            lifeShipPositionList.RemoveAt(idx);
        }

        _testMap.BuildNavMesh();

        for (int i = 0; i < rooms.Count; ++i)
        {
            int rand = Random.Range(0, 100);
            if (rand >= 66)
            {
                var spawner = rooms[i].GetComponent<Room>().monsterSpawners;
                spawner.Init();
                spawner.SpawnMonster();
            }
        }
    }

    // ���� �� �ʱ�ȭ �Լ�
    private void ClearMap()
    {
        for (int i = 0; i < rooms.Count; i++)
            rooms[i].GetComponent<NetworkObject>().Despawn();

        for (int i = 0; i < lifeShips.Count; i++)
            lifeShips[i].GetComponent<NetworkObject>().Despawn();

        foreach (ROOMTYPE roomType in Enum.GetValues(typeof(ROOMTYPE)))
            if (roomCountDic.ContainsKey(roomType))
                roomCountDic[roomType][0] = 0;

        rooms.Clear();
        lifeShips.Clear();
    }
}
