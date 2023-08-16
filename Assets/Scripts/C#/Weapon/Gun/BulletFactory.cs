using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFactory: MonoBehaviour
{
    public static GameObject CreateBullet(BULLET_TYPE type, Vector3 pos)
    {
        if (type == BULLET_TYPE.Client)
        {
            return Instantiate(GameManager.Resource.GetObject<GameObject>("Bullet/BulletClient"), pos, Quaternion.identity);
        } else if (type == BULLET_TYPE.Server)
        {
            return Instantiate(GameManager.Resource.GetObject<GameObject>("Bullet/BulletServer"), pos, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Wrong type");
            return null;
        }
    }
}

public enum BULLET_TYPE
{
    Client,
    Server
}
