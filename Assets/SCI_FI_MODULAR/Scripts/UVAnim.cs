using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVAnim : MonoBehaviour
{
    public bool affectMainMap = true;
    public bool affectEmisiveMap = true;
    public Vector2 scrollSpeed = new Vector2(0.0f, 0.0f);
    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent< Renderer > ();

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = Time.time * scrollSpeed;
        if (affectMainMap)
            rend.material.SetTextureOffset("_BaseColorMap", offset);
        if (affectEmisiveMap)
            rend.material.SetTextureOffset("_EmissiveColorMap", offset);
        
    }
}
