using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCamera : MonoBehaviour
{
    private Camera _camera;
    private float _targetFOV;

    void Start()
    {
        
    }

    public void SetTargetFOV(float targetfov)
    {
        _targetFOV = targetfov;
    }

    // Update is called once per frame
    void Update()
    {
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, Time.deltaTime);
    }
}
