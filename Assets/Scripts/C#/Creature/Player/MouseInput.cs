using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInput : MonoBehaviour
{
    private Vector2 _screenMid;

    private float _sensitive = 0.5f;

    private int _maxX = 35;
    private int _maxY = 60;

    private int _minX = -35;
    private int _minY = -60;

    private float _rotationX = 0;
    private float _rotationY = 0;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        _screenMid.x = Screen.width >> 1;
        _screenMid.y = Screen.height >> 1;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        RotateMouse();
    }

    private void RotateMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (mousePos.x > _screenMid.x + 10 || mousePos.x < _screenMid.x - 10)
            _rotationX = Mathf.Clamp(_rotationX + 10 * _sensitive * (mousePos.x - _screenMid.x) / _screenMid.x, _minY, _maxY);

        if (mousePos.y > _screenMid.y + 10 || mousePos.y < _screenMid.y - 10)
            _rotationY = Mathf.Clamp(_rotationY + 10 * _sensitive * (mousePos.y - _screenMid.y) / _screenMid.x, _minX, _maxX);

        transform.eulerAngles = new Vector3(-_rotationY, _rotationX, 0);

        //Mouse.current.WarpCursorPosition(_screenMid);
    }
}
