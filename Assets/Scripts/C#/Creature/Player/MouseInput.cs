using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class MouseInput : NetworkBehaviour
{
    private Vector2 _screenMid;

    [SerializeField] 
    private float _sensitive = 0.5f;

    private int _maxX = 80;
    private int _minX = -80;

    private float _rotationX = 0;
    private float _rotationY = 0;

    [SerializeField]
    private Transform _cam;
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private Transform _head;

    private Transform _target;
    private Transform _targetOriginAngle;
    private Transform _rootTarget;

    public void Init(Transform camera)
    {
        _screenMid.x = Screen.width >> 1;
        _screenMid.y = Screen.height >> 1;
        _target = transform.GetChild(0);
        _targetOriginAngle = transform.GetChild(1);
        _rootTarget = transform.GetChild(2);
        _player = transform.parent.parent;
        if (_player == null) _player = transform.parent;
        _cam = camera;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OnOffSettingUI(true);
    }

    private void FixedUpdate()
    {
        RotateMouse();
    }

    //현재 값과 이전 값의 차를 쓰기 굳이 가운데로 옮기지 말고
    private void RotateMouse()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * _sensitive;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _sensitive;

        _rotationX += mouseX;
        _rotationY = Mathf.Clamp(_rotationY + mouseY, _minX, _maxX);

        _cam.eulerAngles = new Vector3(-_rotationY, _cam.eulerAngles.y, 0);
        _player.eulerAngles = new Vector3(0, _rotationX, 0);

        Vector3 cross = Vector3.Cross(_targetOriginAngle.position - transform.position, Vector3.up);

        Vector3 value = Quaternion.AngleAxis(_rotationY, cross) * (_targetOriginAngle.position - transform.position) + transform.position;
        _target.position = value;
        _target.localEulerAngles = new Vector3(-_rotationY, 0, 0);
        value = Quaternion.AngleAxis(_rotationX, Vector3.up) * (_targetOriginAngle.position - transform.position) + transform.position;
        _rootTarget.position = value;
        //_cam.position = _head.position;
    }

    public void OnOffSettingUI(bool bOpen)
    {
        if (bOpen)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = bOpen;
        enabled = !bOpen;
    }

    public void Clear()
    {
        enabled = false;
        Cursor.visible = true;
    }
}
