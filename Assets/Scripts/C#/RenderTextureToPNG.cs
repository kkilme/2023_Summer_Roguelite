using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Texture2D modifyTex = new Texture2D((tex.width >> 1) * _width, tex.height * _height, TextureFormat.BGRA32, false);
        modifyTex.alphaIsTransparency = true;
        Color col;

        for (int i = 0; i < tex.width; i += 2)
        {
            for (int j = 0; j < tex.height; ++j)
            {
                col = tex.GetPixel(i, j) / 2 + tex.GetPixel(i + 1, j) / 2;
                modifyTex.SetPixel(i >> 1, j, col);
            }
        }
        modifyTex.Apply();
        Debug.Log($"{modifyTex.width} {modifyTex.height}");
        Sprite s = Sprite.Create(modifyTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        _s.sprite = s;

        System.IO.File.WriteAllBytes(Application.dataPath + $"/Image/{_fileName}.PNG", tex.EncodeToPNG());
    }
}
