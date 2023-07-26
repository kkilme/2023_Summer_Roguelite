using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class MouseInput : MonoBehaviour
{
    private Vector2 _screenMid;

    private float _sensitive = 0.5f;

    private int _maxX = 35;
    //private int _maxY = 60;

    private int _minX = -35;
    //private int _minY = -60;

    private float _rotationX = 0;
    private float _rotationY = 0;

    [SerializeField]
    private Transform _cam;
    [SerializeField]
    private Transform _player;

    private float _toCharacterDistance;
    private Transform _target;
    private Transform _targetOriginAngle;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _screenMid.x = Screen.width >> 1;
        _screenMid.y = Screen.height >> 1;
        _target = transform.GetChild(0);
        _targetOriginAngle = transform.GetChild(1);
        //타겟의 원 위치와 부모간의 거리. 각을 구해주기 위해 필요함
        _toCharacterDistance = Vector3.Distance(transform.parent.position, _target.position);

        //외적으로 회전축을 구하자, 유니티는 왼손 좌표계
        //_target.position - transform.position;
        //Vector3.up;
        Vector3 cross = Vector3.Cross(_target.position - transform.position, Vector3.up);
        //Vector3.Angle(_targetOriginAngle.position - transform.position, _target.position - transform.position);

        //회전축을 구한 후 그 회전축을 중심으로 구해준 각도만큼 회전
        //_target.Rotate(cross, -_rotationY);
        Debug.Log($"{Vector3.Angle(_targetOriginAngle.position - transform.position, _target.position - transform.position)} {_target.position.y}, {transform.position.y}");
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        //RotateMouse();
    }

    private void RotateMouse()
    {
        if (Mathf.Atan2(transform.position.y - _target.position.y, _toCharacterDistance) < 1)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (mousePos.x > _screenMid.x + 10 || mousePos.x < _screenMid.x - 10)
                _rotationX = _rotationX + 10 * _sensitive * (mousePos.x - _screenMid.x) / _screenMid.x;

            if (mousePos.y > _screenMid.y + 10 || mousePos.y < _screenMid.y - 10)
                _rotationY = Mathf.Clamp(_rotationY + 10 * _sensitive * (mousePos.y - _screenMid.y) / _screenMid.x, _minX, _maxX);

            _cam.eulerAngles = new Vector3(-_rotationY, _rotationX, 0);
            _player.eulerAngles = new Vector3(0, _rotationX, 0);

            Mouse.current.WarpCursorPosition(_screenMid);
        }
    }

    private void OnOffSettingUI(bool bOpen)
    {
        Cursor.visible = bOpen;
        this.enabled = bOpen;
    }
}
