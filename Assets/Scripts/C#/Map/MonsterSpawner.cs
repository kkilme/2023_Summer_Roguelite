using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterSpawner : NetworkBehaviour
{
    [SerializeField] private ROOMSIZE _roomSize;
    [SerializeField] private List<Vector3> _spawnerPoses;
    private int _monsterCount;

    public void Init()
    {
        _spawnerPoses = new List<Vector3>(transform.childCount);

        for (short i = 0; i < transform.childCount; ++i) 
            _spawnerPoses.Add(transform.GetChild(i).transform.position);
        switch (_roomSize)
        {
            case ROOMSIZE.LARGE:
                _monsterCount = 10;
                break;
            case ROOMSIZE.MEDIUM:
                _monsterCount = 7;
                break;
            case ROOMSIZE.SMALL:
                _monsterCount = 4;
                break;
        }
    }

    public Vector3 GetRandomSpawnPos()
    {
        return _spawnerPoses[Random.Range(0, _spawnerPoses.Count)];
    }

    // 룸안의 랜덤한 좌표를 리턴하는 함수
    public Vector3 GetRandomRoomPos()
    {
        Vector3 pos = transform.position;

        switch (_roomSize)
        {
            case ROOMSIZE.SMALL:
                return new Vector3(Random.Range(pos.x - 12, pos.x + 12), 1, Random.Range(pos.z - 12, pos.z + 12));
            case ROOMSIZE.MEDIUM:
                return new Vector3(Random.Range(pos.x - 19, pos.x + 19), 1, Random.Range(pos.z - 19, pos.z + 19));
            case ROOMSIZE.LARGE:
                return new Vector3(Random.Range(pos.x - 32, pos.x + 32), 1, Random.Range(pos.z - 19, pos.z + 19));
            default:
                return Vector3.zero;
        }
    }

    // 해당 좌표가 방안에 해당하는지 리턴하는 함수
    public bool IsPosInRoom(Vector3 pos)
    {
        Vector3 roomPos = transform.position;

        switch (_roomSize)
        {
            case ROOMSIZE.SMALL:
                return roomPos.x - 12 <= pos.x && pos.x <= roomPos.x + 12 && roomPos.z - 12 <= pos.z && pos.z <= roomPos.z + 12;
            case ROOMSIZE.MEDIUM:
                return roomPos.x - 19 <= pos.x && pos.x <= roomPos.x + 19 && roomPos.z - 19 <= pos.z && pos.z <= roomPos.z + 19;
            case ROOMSIZE.LARGE:
                return roomPos.x - 32 <= pos.x && pos.x <= roomPos.x + 32 && roomPos.z - 19 <= pos.z && pos.z <= roomPos.z + 19;
            default:
                return false;
        }
    }

    public void SpawnMonster()
    {
        int spawnPos;
        for (short i = 0; i < _monsterCount; ++i)
        {
            spawnPos = Random.Range(0, _spawnerPoses.Count);
            var monster = Instantiate(GameManager.Resource.GetObject<GameObject>($"Monster/Creature_{Random.Range(1, 6)}"), _spawnerPoses[spawnPos], Quaternion.identity);
            var monsterController = Util.GetOrAddComponent<MonsterController>(monster);
            monsterController.Init(this);
            Util.GetOrAddComponent<NetworkObject>(monster).Spawn();
            _spawnerPoses.RemoveAt(spawnPos);
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return _spawnerPoses[Random.Range(0, _spawnerPoses.Count)];
    }
}
