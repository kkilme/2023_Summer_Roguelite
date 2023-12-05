using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerInputs
{
    None,
    Idle,
    Move,
    Attack,
    StopAttack,
    Reload,
    Aim,
    StopAim
}

public partial class Player : NetworkBehaviour, IAttackable
{
    [ClientRpc]
    private void InitClientRpc(ClientRpcParams clientRpcParams = default)
    {
        _mainCam.gameObject.SetActive(true);
        _followPlayerCam.Follow = _headTransform;
        _interact.gameObject.SetActive(true);
        _interact.Init(this, _followPlayerCam.transform);
        _playerController = Util.GetOrAddComponent<PlayerController>(gameObject);
        _playerController.Init(_followPlayerCam, _iaa, _interact);
        _rotationTransform = transform.GetChild(0).GetChild(0).GetChild(0);
        if (_rotationTransform == null) _rotationTransform = transform; // SJPlayer용 테스트코드
        transform.GetChild(0).GetChild(1).gameObject.layer = 9;

        _screenMid.x = Screen.width >> 1;
        _screenMid.y = Screen.height >> 1;
        _target = transform.GetChild(0);
        _targetOriginAngle = transform.GetChild(1);
        _rootTarget = transform.GetChild(2);
        _player = transform.parent.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OnOffSettingUI(true);
    }

    private void RotateMouse()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * _sensitive;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _sensitive;

        _rotationX += mouseX;
        _rotationY = Mathf.Clamp(_rotationY + mouseY, _minX, _maxX);

        _followPlayerCam.transform.eulerAngles = new Vector3(-_rotationY, _followPlayerCam.transform.eulerAngles.y, 0);
        _player.eulerAngles = new Vector3(0, _rotationX, 0);

        Vector3 cross = Vector3.Cross(_targetOriginAngle.position - transform.position, Vector3.up);

        Vector3 value = Quaternion.AngleAxis(_rotationY, cross) * (_targetOriginAngle.position - transform.position) + transform.position;
        _target.position = value;
        _target.localEulerAngles = new Vector3(-_rotationY, 0, 0);
        value = Quaternion.AngleAxis(_rotationX, Vector3.up) * (_targetOriginAngle.position - transform.position) + transform.position;
        _rootTarget.position = value;
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

    public void SetColor()
    {
        _skinnedMeshRenderer.materials[1].color = Color.white;
    }

    private void FixedUpdate()
    {
        if (IsOwner)
            MoveCharacterServerRpc(_playerController.MoveDir);
    }

    [ClientRpc]
    private void InputClientRpc(PlayerInputs pi = PlayerInputs.None, ClientRpcParams clientRpcParams = default)
    {
        if (pi != PlayerInputs.None)
        {
            //_anim.SetBool(pi.ToString(), true);
        }
    }

    [ClientRpc]
    private void InputClientRpc(Vector3 dir, PlayerInputs pi = PlayerInputs.None, ClientRpcParams clientRpcParams = default)
    {
        _rigidbody.velocity = Quaternion.AngleAxis(transform.localEulerAngles.y, Vector3.up) * dir.normalized * PlayerStat.Speed;
        if (pi != PlayerInputs.None)
        {
            //_anim.SetBool(pi.ToString(), true);
        }
    }
}
