using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MonsterSpawner : NetworkBehaviour
{
    [SerializeField] private ROOMSIZE _roomSize;
    [SerializeField] private Transform[] _spawnerPoses;
    [SerializeField] private GameObject[] _monsterObject;

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

    [ServerRpc]
    public void SpawnMonsterServerRPC()
    {
        switch (_roomSize)
        {
            case ROOMSIZE.SMALL://2
                SpawnMonsterRandom(2);
                break;
            case ROOMSIZE.MEDIUM://4
                SpawnMonsterRandom(4);
                break;
            case ROOMSIZE.LARGE://5
                SpawnMonsterRandom(5);
                break;
        }
    }

    private void SpawnMonsterRandom(int count)
    {
        System.Random _rand = new System.Random();

        for (short i = 0; i < count; ++i)
        {
            var monster = Instantiate(_monsterObject[_rand.Next(0, _monsterObject.Length)]);
            var monsterController = Util.GetOrAddComponent<MonsterController>(monster);
            monsterController.Init(this);
            monster.transform.position = GetRandomRoomPos();
        }
    }
}
