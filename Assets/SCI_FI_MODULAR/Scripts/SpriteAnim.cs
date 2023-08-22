using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnim : MonoBehaviour
{
    public int uvAnimationTileX = 10; //Here you can place the number of columns of your sheet. 
    public int uvAnimationTileY = 8; //Here you can place the number of rows of your sheet. 
    public float framesPerSecond = 10.0f; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate index
        int index = (int)(Time.time * framesPerSecond);
        // repeat when exhausting all frames
        index = index % (uvAnimationTileX * uvAnimationTileY);

        // Size of every tile
        var size = new Vector2(1.0f / uvAnimationTileX, 1.0f / uvAnimationTileY);

        // split into horizontal and vertical index
        var uIndex = index % uvAnimationTileX;
        var vIndex = index / uvAnimationTileX;

        // build offset
        // v coordinate is the bottom of the image in opengl so we need to invert.
        var offset = new Vector2(uIndex * size.x, 1.0f - size.y - vIndex * size.y);

        GetComponent<Renderer>().material.SetTextureOffset("_BaseColorMap", offset);
        GetComponent<Renderer>().material.SetTextureScale("_BaseColorMap", size);

        GetComponent<Renderer>().material.SetTextureOffset("_EmissiveColorMap", offset);
        GetComponent<Renderer>().material.SetTextureScale("_EmissiveColorMap", size);
    }
}