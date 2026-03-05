using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 orbitStartingView;
    [SerializeField] private Vector2 minMaxOrbitDistance;
    [SerializeField] private Vector2 minMaxVerticalOrbit;
    [SerializeField] private float orbitZoomSpeed = 1f;
    [SerializeField] private float orbitSpeed = 1f;
    [SerializeField] private float orbitDamping = 1f;

    private Transform target;
    private float _orbitDistance;
    private Vector3 _localPos;
    private Vector3 _localRot;
    private Mouse _curMouse;
    private Display _mainDisplay;

    private void Start()
    {
        // store ref to playerboat
        target = transform.parent;
        transform.parent = null;

        _curMouse = Mouse.current;
        _mainDisplay = Display.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _localPos = mainCamera.transform.localPosition;
        _orbitDistance = minMaxOrbitDistance.y;
    }

    private void LateUpdate()
    {
        // update position of dollyparent to match target (player powerboat)
        transform.position = target.position;

        // update orbitDistance with values from scroll wheel
        _orbitDistance += _curMouse.scroll.y.value * orbitZoomSpeed;

        // clamp
        _orbitDistance = Mathf.Clamp(_orbitDistance, minMaxOrbitDistance.x, minMaxOrbitDistance.y);

        // apply to camera distance along dolly arm
        _localPos.z = -_orbitDistance;
        mainCamera.transform.localPosition = _localPos;

        // update local rot with mouse input
        Vector2 mouseDelta = _curMouse.delta.value;
        _localRot.x -= mouseDelta.y * orbitSpeed;
        _localRot.y += mouseDelta.x * orbitSpeed;

        // clamp
        _localRot.x = Mathf.Clamp(_localRot.x, minMaxVerticalOrbit.x, minMaxVerticalOrbit.y);

        // obtain lerped quaternion rotation from values 
        // and apply to dolly rotation around target
        Quaternion newRot = Quaternion.Euler(_localRot.x, _localRot.y, 0f);
        transform.localRotation = Quaternion.LerpUnclamped(transform.localRotation, newRot, Time.deltaTime * orbitDamping);
    }
}
