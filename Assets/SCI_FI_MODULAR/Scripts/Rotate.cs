using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float speed = 1f;
    Vector3 startAngles;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.AngleAxis(-speed * Time.deltaTime, axis) * transform.localRotation;

    }
}
