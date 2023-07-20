using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField]
    private Transform[] _playerUpperBody;
    private float[] _playerOriginalX;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        _screenMid.x = Screen.width >> 1;
        _screenMid.y = Screen.height >> 1;

        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;

        _playerOriginalX = new float[_playerUpperBody.Length];

        for (short i = 0; i < _playerUpperBody.Length; ++i)
            _playerOriginalX[i] = _playerUpperBody[i].localEulerAngles.x;
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

        for (short i = 0; i < _playerUpperBody.Length; ++i)
        {
            float anglex = Mathf.Clamp(_playerOriginalX[i] - _rotationY * (_playerUpperBody.Length - i) / 10, _playerOriginalX[i] + _minX, _playerOriginalX[i] + _maxX);
            _playerUpperBody[i].localEulerAngles =
                new Vector3(anglex, _playerUpperBody[i].localEulerAngles.y, _playerUpperBody[i].localEulerAngles.z);
        }

        //Mouse.current.WarpCursorPosition(_screenMid);
    }

    private void OnOffSettingUI(bool bOpen)
    {
        Cursor.visible = bOpen;
        this.enabled = bOpen;
    }
}
