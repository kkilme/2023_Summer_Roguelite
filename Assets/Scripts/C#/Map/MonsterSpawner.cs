using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner
{
    private ROOMSIZE roomSize;
    private Vector3 pos;

    public MonsterSpawner(ROOMSIZE roomSize, Vector3 pos)
    {
        this.roomSize = roomSize;
        this.pos = pos;
    }

    // ·ë¾ÈÀÇ ·£´ýÇÑ ÁÂÇ¥¸¦ ¸®ÅÏÇÔ
    public Vector3 GetRandomRoomPos()
    {
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
}
