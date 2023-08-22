using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class EmissiveFlicker : MonoBehaviour
{
    public Light flickeringLight;
    bool state;
    MeshRenderer mesh;
    Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        originalColor = mesh.material.GetColor("_EmissiveColor");
        state = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(flickeringLight.enabled != state)
        {
            if (state)
            {
                mesh.material.SetColor("_EmissiveColor", Color.black);
            } else
            {
                mesh.material.SetColor("_EmissiveColor", originalColor);
            }
            state = flickeringLight.enabled;
        }
    }
}
