using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Blinker : MonoBehaviour
{
    public Color mColor1 = Color.red;
    public Color mColor2 = Color.black;
    public float mColorInstensity = 1f;
   // public bool mRandom = true;
    public float mMinTime = 0f;
    public float mMaxTime = 1f;
    float mTimer;
    bool mIsColor1;
    float factor;
    Renderer mRenderer;
    // Start is called before the first frame update
    void Start()
    {
        mRenderer = GetComponent<Renderer>();
        factor = Mathf.Pow(2, mColorInstensity);
        mRenderer.material.SetColor("_EmissiveColor", mColor1 * factor);
        mRenderer.material.SetFloat("_EmissiveExposureWeight", 0f);
        mTimer = Random.Range(mMinTime, mMaxTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (mTimer < 0f)
        {
            if (mIsColor1)
            {
                mRenderer.material.SetColor("_EmissiveColor", mColor2 * factor);
            }
            else
            {
                mRenderer.material.SetColor("_EmissiveColor", mColor1 * factor);
            }
            mRenderer.material.SetFloat("_EmissiveExposureWeight", 0f);
            mIsColor1 = !mIsColor1;
            mTimer = Random.Range(mMinTime, mMaxTime);
        }
        mTimer -= Time.deltaTime;
    }
}
