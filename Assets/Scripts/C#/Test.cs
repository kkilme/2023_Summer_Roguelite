using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * Time.deltaTime * 5;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * Time.deltaTime * 5;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * Time.deltaTime * 5;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * Time.deltaTime * 5;
        }

        if (Input.GetKey(KeyCode.J))
        {
            transform.eulerAngles += Vector3.up * 5;
        }

        if (Input.GetKey(KeyCode.L))
        {
            transform.eulerAngles -= Vector3.up * 5;
        }
    }
}
