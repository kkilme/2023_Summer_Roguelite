using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SetLightColorAction : MonoBehaviour
{
    public Color newColor;
    Light light;

    void Start()
    {
        light = GetComponent<Light>();
    }

    public void Action()
    {
        light.color = newColor;
    }
}
