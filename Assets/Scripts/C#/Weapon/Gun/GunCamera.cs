using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCamera : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;
    //[SerializeField]
    //private Camera _weaponCamera;
    private float _targetFOV;
    private float _originalFOV;
    private float _zoomspeed;

    void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        _originalFOV = _camera.m_Lens.FieldOfView;
        _targetFOV = _camera.m_Lens.FieldOfView;
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

    /// <summary>
    /// 인자 없을 시 기본 FOV로 설정
    /// </summary>
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
        _camera.m_Lens.FieldOfView = Mathf.Lerp(_camera.m_Lens.FieldOfView, _targetFOV, Time.deltaTime * _zoomspeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            if (other.name.Contains("Collider"))
            {
                other.transform.parent.gameObject.layer = 13;
                Debug.Log(other.transform.parent.gameObject.layer);
            }
            else
            {
                other.gameObject.layer = 13;
                Debug.Log(other.gameObject.layer);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            if (other.name.Contains("Collider"))
            {
                other.transform.parent.gameObject.layer = 0;
                Debug.Log(other.transform.parent.gameObject.layer);
            }
            else
            {
                other.gameObject.layer = 0;
                Debug.Log(other.gameObject.layer);
            }
        }
    }
}
