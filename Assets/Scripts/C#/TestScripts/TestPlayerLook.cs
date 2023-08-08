using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestPlayerLook : NetworkBehaviour
{

    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform _target;

    [Header("Look Settings")]
    [SerializeField] private float sensX = 10f;
    [SerializeField] private float sensY = 10f;

    private float yRotation;
    private float xRotation;

    private void Start()
    {
        cameraHolder = GameObject.Find("FollowPlayerCam").transform;
    }

    public void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * 0.1f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * 0.1f;

        yRotation += mouseX * sensX;
        xRotation -= mouseY * sensY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        Ray ray = new Ray(cameraHolder.transform.position, cameraHolder.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            _target.transform.position = hit.point;
        }
        else
        {
            _target.transform.position = ray.GetPoint(1000);
        }

    }

}