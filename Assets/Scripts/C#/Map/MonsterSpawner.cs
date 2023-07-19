using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterSpawner : NetworkBehaviour
{
    [SerializeField] private ROOMSIZE roomSize;
    [SerializeField] private Transform[] spawnerPoses;

    // ·ë¾ÈÀÇ ·£´ýÇÑ ÁÂÇ¥¸¦ ¸®ÅÏÇÔ
    public Vector3 GetRandomRoomPos()
    {
        Vector3 pos = transform.position;

        switch (roomSize)
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

    [ServerRpc]
    public void SpawnMonsterServerRPC()
    {

    }
}
