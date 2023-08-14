using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public IInteraction Item { get; private set; }
    private Transform _itemTransform;
    private short _interactions = 0;
    private Player _curPlayer;
    private Transform _cam;

    public void Init(Player player, Transform cam)
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
            //Debug.DrawLine(_cam.position, _cam.position + 3 * _cam.forward, Color.red, 5f);

            if (Physics.Linecast(_cam.position, _cam.position + 3 * _cam.forward, out hit))
            {
                if (Item == null || hit.transform != _itemTransform)
                {
                    ClearItem();

                    if (hit.transform.TryGetComponent(out IInteraction item))
                    {
                        Item = item;
                        _itemTransform = hit.transform;
                        Item.Interactable(true);
                    }
                }
            }

            else
                ClearItem();
        }
    }

    private void ClearItem()
    {
        if (Item != null)
            Item.Interactable(false);
        Item = null;
        _itemTransform = null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(GOTag.Item.ToString()))
            --_interactions;
    }

    public void Clear()
    {
        _curPlayer = null;
        _cam = null;
        Item = null;
    }
}
