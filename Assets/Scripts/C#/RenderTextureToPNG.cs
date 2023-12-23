using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenderTextureToPNG : MonoBehaviour
{
    [SerializeField]
    RenderTexture texture;
    [SerializeField]
    string _fileName;
    [SerializeField]
    int _width = 1;
    [SerializeField]
    int _height = 1;
    WaitForEndOfFrame _wof = new WaitForEndOfFrame();
    [SerializeField]
    SpriteRenderer _s;
    readonly Color _blue = new Color(0, 0, 0.812f, 1);

    private void Awake()
    {
        //Debug.Log($"{texture.height} {texture.width}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            StartCoroutine(Save());
    }

    public void SetRenderTextureSize(int width, int height)
    {
        texture.width *= width;
        texture.height *= height;
    }

    IEnumerator Save()
    {
        yield return _wof;
        Debug.Log("Start");
        Texture2D tex = new Texture2D(texture.width, texture.height);
        tex.alphaIsTransparency = true;
        RenderTexture.active = texture;
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        tex.Apply();
        TextureBilinear(tex);
        Debug.Log("End");
    }

    void TextureBilinear(Texture2D tex)
    {
        Texture2D modifyTex = new Texture2D(256, 256, TextureFormat.BGRA32, false);
        modifyTex.alphaIsTransparency = true;

        for (int i = 0; i < tex.width; ++i)
        {
            //for (int j = 0; j <  tex.height; ++j)
            //{
            //    modifyTex.SetPixel(i, j, tex.GetPixel(i, j));
            //}
            for (int j = 0; j < 64; ++j)
            {
                modifyTex.SetPixel(i, j, Color.clear);
            }
            
            for (int j = 64; j < 192; ++j)
            {
                modifyTex.SetPixel(i, j, tex.GetPixel(i, j - 64));
            }
            
            for (int j = 192; j < 256; ++j)
            {
                modifyTex.SetPixel(i, j, Color.clear);
            }
        }
        modifyTex.Apply();
        Debug.Log($"{tex.width} {tex.height}");
        Sprite s = Sprite.Create(modifyTex, new Rect(0, 0, modifyTex.width, modifyTex.height), new Vector2(0.5f, 0.5f));
        _s.sprite = s;
        
        System.IO.File.WriteAllBytes(Application.dataPath + $"/Image/{_fileName}.PNG", modifyTex.EncodeToPNG());
    }
}
