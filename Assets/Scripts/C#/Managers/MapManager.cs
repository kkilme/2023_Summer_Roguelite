using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MapManager : NetworkBehaviour
{
    private Dictionary<ROOMSIZE, List<Room>> roomPrefabsDic = new Dictionary<ROOMSIZE, List<Room>>();
    private Dictionary<ROOMTYPE, int[]> roomCountDic = new Dictionary<ROOMTYPE, int[]>(); // 0번 인덱스는 현재 룸 카운트. 1번 인덱스는 최대 룸 카운트
    private Dictionary<ROOMTYPE, float> specialRoomProbabilityDic = new Dictionary<ROOMTYPE, float>(); // 해당 방 타입이 special 타입일 시 해당 방의 생성확률을 정의해줌 (0 ~ 1)
    private Dictionary<ROOMTYPE, RandomWeightPicker<ITEMNAME>> roomsItemDic = new Dictionary<ROOMTYPE, RandomWeightPicker<ITEMNAME>>(); // 방에 해당하는 아이템들이 매핑되어 있음.
    
    [SerializeField] private Transform[] lifeShipPositions;
    [SerializeField] private GameObject lifeShipPrefab;

    [SerializeField] private List<GameObject> lifeShips = new List<GameObject>(); // 현재 배치된 구명선들 리스트
    [SerializeField] private NavMeshSurface _testMap;
    [SerializeField] private GameObject _playerObject;

    [SerializeField] private GameObject[] _rooms;

    [Header("Stat")]
    [SerializeField] private int lifeShipCount;

    public static MapManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        Init();
    }

    private void Init()
    {
        if (IsHost || IsServer)
        {
            if (roomPrefabsDic.ContainsKey(ROOMSIZE.SMALL))
            {
                return;
            }

            foreach (ROOMSIZE roomSize in Enum.GetValues(typeof(ROOMSIZE)))
                roomPrefabsDic.Add(roomSize, new List<Room>());

            // 추후에 데이터베이스에서 관리 생성하도록 설정
            //방 개수
            //roomCountDic.Add(ROOMTYPE.ARMORY, new int[] { 0, 4 });
            roomCountDic.Add(ROOMTYPE.MACHINE_ROOM, new int[] { 0, 2 });
            //roomCountDic.Add(ROOMTYPE.APEX_LABORATORY, new int[] { 0, 1 });
            //roomCountDic.Add(ROOMTYPE.BED_ROOM, new int[] { 0, 99 });
            roomCountDic.Add(ROOMTYPE.LABORATORY, new int[] { 0, 3 });
            roomCountDic.Add(ROOMTYPE.MANAGEMENT_ROOM, new int[] { 0, 5 });
            roomCountDic.Add(ROOMTYPE.MEDICAL_ROOM, new int[] { 0, 3 });

            //specialRoomProbabilityDic.Add(ROOMTYPE.APEX_LABORATORY, 0.05f);

            // 방들 추가
            //roomsItemDic.Add(ROOMTYPE.ARMORY, new RandomWeightPicker<ITEMNAME>());
            roomsItemDic.Add(ROOMTYPE.MACHINE_ROOM, new RandomWeightPicker<ITEMNAME>());
            //roomsItemDic.Add(ROOMTYPE.APEX_LABORATORY, new RandomWeightPicker<ITEMNAME>());
            //roomsItemDic.Add(ROOMTYPE.BED_ROOM, new RandomWeightPicker<ITEMNAME>());
            roomsItemDic.Add(ROOMTYPE.LABORATORY, new RandomWeightPicker<ITEMNAME>());
            roomsItemDic.Add(ROOMTYPE.MANAGEMENT_ROOM, new RandomWeightPicker<ITEMNAME>());
            roomsItemDic.Add(ROOMTYPE.MEDICAL_ROOM, new RandomWeightPicker<ITEMNAME>());

            // 방에 해당하는 아이템 및 가중치 추가
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.AMMO_762, 10);
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.AMMO_9, 30);
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.AMMO_556, 10);
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.GAUGE_12, 20);
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.TESTASSAULTRIFLE, 8);
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.TESTMACHINEGUN, 6);
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.TESTRAREHEAD, 5);
            //roomsItemDic[ROOMTYPE.ARMORY].Add(ITEMNAME.TESTHEAD, 5);

            //roomsItemDic[ROOMTYPE.MACHINE_ROOM].Add(ITEMNAME.JERRY_CAN, 50);

            //roomsItemDic[ROOMTYPE.APEX_LABORATORY].Add(ITEMNAME.AMMO_556, 100);

            //roomsItemDic[ROOMTYPE.BED_ROOM].Add(ITEMNAME.CANNED_FOOD, 5);
            //roomsItemDic[ROOMTYPE.BED_ROOM].Add(ITEMNAME.AMMO_9, 10);
            //roomsItemDic[ROOMTYPE.BED_ROOM].Add(ITEMNAME.JERRY_CAN, 0.1f);
            //roomsItemDic[ROOMTYPE.BED_ROOM].Add(ITEMNAME.TESTASSAULTRIFLE, 0.2f);

            roomsItemDic[ROOMTYPE.LABORATORY].Add(ITEMNAME.CANNED_FOOD, 10f);
            roomsItemDic[ROOMTYPE.LABORATORY].Add(ITEMNAME.AMMO_762, 3f);
            roomsItemDic[ROOMTYPE.LABORATORY].Add(ITEMNAME.GAUGE_12, 2f);
            roomsItemDic[ROOMTYPE.LABORATORY].Add(ITEMNAME.TESTASSAULTRIFLE, 0.1f);

            roomsItemDic[ROOMTYPE.MANAGEMENT_ROOM].Add(ITEMNAME.CANNED_FOOD, 2f);
            //roomsItemDic[ROOMTYPE.MANAGEMENT_ROOM].Add(ITEMNAME.JERRY_CAN, 0.5f);
            roomsItemDic[ROOMTYPE.MANAGEMENT_ROOM].Add(ITEMNAME.AMMO_9, 10f);
            roomsItemDic[ROOMTYPE.MANAGEMENT_ROOM].Add(ITEMNAME.AMMO_556, 7.5f);
            roomsItemDic[ROOMTYPE.MANAGEMENT_ROOM].Add(ITEMNAME.GAUGE_12, 8f);
            roomsItemDic[ROOMTYPE.MANAGEMENT_ROOM].Add(ITEMNAME.AMMO_762, 6f);
            roomsItemDic[ROOMTYPE.MANAGEMENT_ROOM].Add(ITEMNAME.TESTASSAULTRIFLE, 0.2f);


            roomsItemDic[ROOMTYPE.MEDICAL_ROOM].Add(ITEMNAME.CANNED_FOOD, 30f);
            roomsItemDic[ROOMTYPE.MEDICAL_ROOM].Add(ITEMNAME.AMMO_9, 10f);
            roomsItemDic[ROOMTYPE.MEDICAL_ROOM].Add(ITEMNAME.AMMO_762, 5f);
            roomsItemDic[ROOMTYPE.MEDICAL_ROOM].Add(ITEMNAME.TESTASSAULTRIFLE, 0.1f);

            roomsItemDic[ROOMTYPE.MACHINE_ROOM].Add(ITEMNAME.CANNED_FOOD, 30f);
            roomsItemDic[ROOMTYPE.MACHINE_ROOM].Add(ITEMNAME.AMMO_9, 10f);
            roomsItemDic[ROOMTYPE.MACHINE_ROOM].Add(ITEMNAME.AMMO_762, 5f);
            roomsItemDic[ROOMTYPE.MACHINE_ROOM].Add(ITEMNAME.TESTASSAULTRIFLE, 1f);


            var roomPrefabs = Resources.LoadAll<Room>("Room");

            //NetworkManager.AddNetworkPrefab(lifeShipPrefab);

            for (int i = 0; i < roomPrefabs.Length; i++)
            {
                //NetworkManager.AddNetworkPrefab(roomPrefabs[i].gameObject);
                roomPrefabsDic[roomPrefabs[i].roomSize].Add(roomPrefabs[i]);
            }
            GenerateMapServerRPC();
        }
    }

    private async UniTask MonsterGeneration(Room room)
    {
        int rand = Random.Range(0, 100);
        if (rand >= 34)
        {
            var spawner = room.monsterSpawners;
            spawner.Init();
            spawner.SpawnMonster();
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
        }
    }

    private void SetRoom(Room room)
    {
        GameObject item;
        for (int i = 0; i < room.ItemPlaces.Length; i++)
        {
            item = Instantiate(GettableItem.GetItemPrefab(roomsItemDic[room.roomType].GetRandomPick()), room.ItemPlaces[i], Quaternion.identity);
            item.GetComponent<NetworkObject>().Spawn();
            item.transform.parent = room.transform;
        }
    }

    private async UniTaskVoid MakeRoom()
    {
        Room room;
        List<int> ints = new List<int>(_rooms.Length);
        List<int> playerRoom = new List<int>(NetworkManager.Singleton.ConnectedClientsList.Count);
        int remove;

        for (int i = 0; i < ints.Capacity; ++i)
        {
            ints.Add(i);
        }

        for (int i = 0; i < playerRoom.Capacity; ++i)
        {
            remove = Random.Range(0, ints.Count);
            playerRoom.Add(ints[remove]);
            ints.RemoveAt(remove);
        }

        int count = 0;
        for (int i = 0; i < _rooms.Length; ++i)
        {
            room = _rooms[i].GetComponent<Room>();
            SetRoom(room);
            if (ints.Count > count && ints[count] == i)
            {
                ++count;
                await MonsterGeneration(room);
            }

            await UniTask.DelayFrame(5);
        }

        for (int i = 0; i < playerRoom.Count; ++i)
        {
            room = _rooms[playerRoom[i]].GetComponent<Room>();
            var networkObj = Instantiate(_playerObject, room.monsterSpawners.GetRandomSpawnPos(), quaternion.identity).GetComponent<NetworkObject>();
            networkObj.SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
        }
    }

// 맵 생성 함수
    [ServerRpc]
    public void GenerateMapServerRPC()
    {
        //ClearMap();
        /*
        1. 각각의 위치에 방 종류를 랜덤으로 배정
        2. 해당 위치의 크기에 해당하는 방을 배치
        */

        //var roomPositionList = roomPositions.ToList();

        var roomTypes = Enum.GetValues(typeof(ROOMTYPE)) as ROOMTYPE[];
        int start = Array.FindIndex(roomTypes, element => element == ROOMTYPE.NECESSARY_START) + 1,
            end = Array.FindIndex(roomTypes, element => element == ROOMTYPE.NECESSARY_END);

        _testMap.BuildNavMesh();

        MakeRoom().Forget();


        // 구명선 배치
        //var lifeShipPositionList = lifeShipPositions.ToList(); 
        //for (int i = 0; i < lifeShipCount; i++)
        //{
        //    int idx = Random.Range(0, lifeShipPositionList.Count);
        //
        //    var obj = Instantiate(lifeShipPrefab, lifeShipPositionList[idx].position, Quaternion.identity, lifeShipPositionList[i].transform);
        //    lifeShips.Add(obj);
        //    var networkObj = Util.GetOrAddComponent<NetworkObject>(obj);
        //    networkObj.Spawn();
        //
        //    networkObj.TrySetParent(lifeShipPositionList[i].transform);
        //
        //    lifeShipPositionList.RemoveAt(idx);
        //}
    }

    [ServerRpc]
    public void GenPlayerServerRPC()
    {
        var networkObj = Instantiate(_playerObject, Vector3.zero, quaternion.identity).GetComponent<NetworkObject>();
        networkObj.SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsList[0].ClientId);
    }
    // 기존 맵 초기화 함수
    private void ClearMap()
    {
        for (int i = 0; i < _rooms.Length; i++)
            _rooms[i].GetComponent<NetworkObject>().Despawn();

        for (int i = 0; i < lifeShips.Count; i++)
            lifeShips[i].GetComponent<NetworkObject>().Despawn();

        foreach (ROOMTYPE roomType in Enum.GetValues(typeof(ROOMTYPE)))
            if (roomCountDic.ContainsKey(roomType))
                roomCountDic[roomType][0] = 0;

        _rooms.Initialize();
        lifeShips.Clear();
    }
}
