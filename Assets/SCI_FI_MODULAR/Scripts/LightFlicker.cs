using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    Light testLight;
    public float minWaitTime;
    public float maxWaitTime;

    void Start() {
        testLight = GetComponent<Light>();
        StartCoroutine(Flashing());
    }


    IEnumerator Flashing()
    {
        while (true)
        {
            yield return new WaitForSeconds (UnityEngine.Random.Range(minWaitTime, maxWaitTime));
            testLight.enabled = !testLight.enabled;

        }
    }

}
 