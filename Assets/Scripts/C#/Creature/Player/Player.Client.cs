using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class Player : NetworkBehaviour, IAttackable
{
    [ClientRpc]
    private void InitClientRpc(ClientRpcParams clientRpcParams = default)
    {
        _followPlayerCam.Follow = _headTransform;
        _interact.gameObject.SetActive(true);
        _interact.Init(this, _followPlayerCam.transform);
        _playerController = Util.GetOrAddComponent<PlayerController>(gameObject);
        _playerController.Init(_followPlayerCam, _iaa, _interact);
        //_skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        //_skinnedMeshRenderer.materials[0].color = Color.clear;
        //_skinnedMeshRenderer.materials[2].color = Color.clear;
        //6, 10 12 - 15
        //for (int i = 0; i < _skinnedMeshRenderer?.materials.Length; ++i)
        //{
        //    if (i != 6 && i != 10 && (i < 12 || i > 15))
        //        _skinnedMeshRenderer.materials[i].color = Color.clear;//.SetFloat("_DistortionDepthTest", 2);
        //}
        _rotationTransform = transform.GetChild(0).GetChild(0).GetChild(0);
        if (_rotationTransform == null) _rotationTransform = transform; // SJPlayer용 테스트코드
        transform.GetChild(0).GetChild(1).gameObject.layer = 9;
    }

    public void SetColor()
    {
        _skinnedMeshRenderer.materials[1].color = Color.white;
    }
}
