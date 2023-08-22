using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SetEmissiveColorAction : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color newColor;

    MeshRenderer mesh;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
    }

    public void Action()
    {
        mesh.material.SetColor("_EmissiveColor", newColor);
    }
}
