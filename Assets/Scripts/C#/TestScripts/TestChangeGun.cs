using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChangeGun : MonoBehaviour
{
    [SerializeField] private List<GunData> examples = new List<GunData>();
    [SerializeField] private Gun gun;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    gun.SetGunData(examples[0]);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    gun.SetGunData(examples[1]);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    gun.SetGunData(examples[2]);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    gun.SetGunData(examples[3]);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    gun.SetGunData(examples[4]);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha6))
        //{
        //    gun.SetGunData(examples[5]);
        //}
    }
}
