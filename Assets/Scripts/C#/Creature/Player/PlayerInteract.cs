using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private IInteraction _item;
    private Transform _itemTransform;
    private short _interactions = 0;
    private Player _curPlayer;
    private CinemachineVirtualCamera _cam;

    public void Init(Player player, CinemachineVirtualCamera cam)
    {
        _curPlayer = player;
        _cam = cam;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GOTag.Item.ToString()))
            ++_interactions;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_interactions > 0)
        {
            RaycastHit hit;
            Debug.DrawLine(_cam.transform.position, _cam.transform.position + 3 * _cam.transform.forward, Color.red, 5f);

            if (Physics.Linecast(_cam.transform.position, _cam.transform.position + 3 * _cam.transform.forward, out hit))
                if (_item == null || hit.transform != _itemTransform)
                {
                    IInteraction item = null;
                    ClearItem();

                    if (hit.transform.TryGetComponent(out item))
                    {
                        _item = item;
                        _itemTransform = hit.transform;
                        _item.Interactable(true);
                    }
                }
        }
    }

    private void ClearItem()
    {
        if (_item != null)
            _item.Interactable(false);
        _item = null;
        _itemTransform = null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(GOTag.Item.ToString()))
        {
            --_interactions;
        }
    }

    public void Clear()
    {
        _curPlayer = null;
        _cam = null;
        _item = null;
    }
}
