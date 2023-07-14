using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{   
    public float Speed;
    // Start is called before the first frame update
   
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0,Speed * Time.deltaTime,0);
    }
}
