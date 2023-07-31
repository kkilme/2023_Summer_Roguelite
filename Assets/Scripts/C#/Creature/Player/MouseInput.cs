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

    [SerializeField] private float _sensitive = 0.5f;

    private int _maxX = 80;
    //private int _maxY = 60;

    private int _minX = -80;
    //private int _minY = -60;

    private float _rotationX = 0;
    private float _rotationY = 0;

    [SerializeField]
    private Transform _cam;
    [SerializeField]
    private Transform _player;

    private Transform _target;
    private Transform _targetOriginAngle;

    [SerializeField] private bool checkTruefortest = false;

    public void Init(Transform camera)
    {
        _screenMid.x = Screen.width >> 1;
        _screenMid.y = Screen.height >> 1;
        _target = transform.GetChild(0);
        _targetOriginAngle = transform.GetChild(1);
        _player = transform.parent;
        _cam = camera;

        Cursor.visible = false;
        OnOffSettingUI(true);
    }

    private void FixedUpdate()
    {
        RotateMouse();
    }

    private void RotateMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (mousePos.x > _screenMid.x + 10 || mousePos.x < _screenMid.x - 10)
            _rotationX = _rotationX + 10 * _sensitive * (mousePos.x - _screenMid.x) / _screenMid.x;

        if (mousePos.y > _screenMid.y + 10 || mousePos.y < _screenMid.y - 10)
            _rotationY = Mathf.Clamp(_rotationY + 10 * _sensitive * (mousePos.y - _screenMid.y) / _screenMid.x, _minX, _maxX);

        _cam.eulerAngles = new Vector3(-_rotationY, _rotationX, 0);
        _player.eulerAngles = new Vector3(0, _rotationX, 0);

        Vector3 cross = Vector3.Cross(_targetOriginAngle.position - transform.position, Vector3.up);

        Vector3 value = Quaternion.AngleAxis(_rotationY, cross) * (_targetOriginAngle.position - transform.position) + transform.position;
        _target.position = value;
        _target.localEulerAngles = new Vector3(-_rotationY, 0, 0);
        Mouse.current.WarpCursorPosition(_screenMid);
    }

    public void OnOffSettingUI(bool bOpen)
    {
        Cursor.visible = bOpen;
        Mouse.current.WarpCursorPosition(_screenMid);
        this.enabled = !bOpen;
    }
}
