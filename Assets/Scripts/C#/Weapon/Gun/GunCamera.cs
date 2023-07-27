using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCamera : MonoBehaviour
{
    private Camera _camera;
    //[SerializeField]
    //private Camera _weaponCamera;
    private float _targetFOV;
    private float _originalFOV;
    private float _zoomspeed;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _originalFOV = _camera.fieldOfView;
        _targetFOV = _camera.fieldOfView;
    }

    public void DisableCamera()
    {
        //_weaponCamera.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SetZoomSpeed(float target)
    {
        _zoomspeed = target;
    }

    public void SetTargetFOV()
    {
        _targetFOV = _originalFOV;
    }
    public void SetTargetFOV(float zoomrate)
    {
        _targetFOV = _originalFOV / zoomrate;
    }

    // Update is called once per frame
    void Update()
    {
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, Time.deltaTime * _zoomspeed);
    }
}
