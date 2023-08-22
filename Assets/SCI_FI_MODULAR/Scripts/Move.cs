using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public Vector3 motionVector = Vector3.right;
    public float distance = 1f;
    public float speed = 1f;
    public bool bounce = true;
    bool isReturning = false;
    Vector3 startPos, endPos;
    // Start is called before the first frame update
    void Start()
    {
        motionVector.Normalize();
        startPos = transform.position;
        endPos = startPos + motionVector * distance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isReturning)
        {
            float dist = (startPos - transform.position).magnitude;

            if (dist > distance)
            {
                isReturning = true;
                transform.position = endPos;
            }
            else
            {
                transform.position += motionVector * speed * Time.deltaTime;
            }
        }
        else
        {
            float dist = (endPos - transform.position).magnitude;

            if (dist > distance)
            {
                isReturning = false;
                transform.position = startPos;
            }
            else
            {
                transform.position -= motionVector * speed * Time.deltaTime;
            }

        }
    }
}
