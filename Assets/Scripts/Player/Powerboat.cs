using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Powerboat : MonoBehaviour
{
    [Header("Helm")]
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float turnSensitivty = 1f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float minSteerageSpeed = 10f;

    [Header("Elements")]
    [SerializeField] private Rigidbody rb;
    public Rigidbody Rigidbody => rb;
    [SerializeField] private Transform engines;

    [Header("Visual Effects")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float maxVignetteIntensity = 0.5f;
    [SerializeField] private float maxVignetteSpeed = 30f;

    [Header("Debug")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector3 directionRot;
    [SerializeField] private float inputX;
    [SerializeField] private float inputY;
    [SerializeField] private Vector3 engineRot;
    [SerializeField] private Vector3 force;

    private const float SteeringSpeed = 0.6f;
    private const float ForceMultiplier = 1200f;
    private Vignette _vignette;
    private bool _vignetteFound;

    private void Start()
    {
        engineRot = engines.localEulerAngles;
        directionRot = transform.eulerAngles;
        _vignetteFound = globalVolume.profile.TryGet(out _vignette);
        if (!_vignetteFound)
            Debug.LogError("No vignette found!");

    }

    private void Update()
    {
        if (GameManager.GamePaused)
            return;
        HandlePhysicsMovement();

        float ratio = currentSpeed / maxVignetteSpeed;
        _vignette.intensity.value = maxVignetteIntensity * ratio;

    }

    private void HandlePhysicsMovement()
    {
        // accelerate boat in direction it's facing, with impulse force scaled based on inputY
        force = ForceMultiplier * inputY * maxAcceleration * Time.deltaTime * transform.forward;

        // apply impulse force
        rb.AddForce(force, ForceMode.Force);

        // angle engines based on inputX,
        float steerAngle = -inputX * turnSensitivty * maxSteerAngle * 2;
        engineRot.y = Mathf.LerpAngle(engines.localEulerAngles.y, steerAngle, SteeringSpeed);
        engines.localEulerAngles = engineRot;

        currentSpeed = rb.velocity.magnitude;

        // if engines aren't running, don't steer
        if (force.magnitude < minSteerageSpeed)
            return;

        // determine turn increment based on current steerangle, invert it if going forward
        float forwardInvert = inputY > 0 ? -1 : 1;
        float increment = forwardInvert * steerAngle * Time.deltaTime;

        // increment boat direction by this value
        Vector3 tempRot = transform.eulerAngles;
        tempRot.y += increment;
        transform.eulerAngles = tempRot;
    }

    public void OnMove(InputValue input)
    {
        Vector2 inputVector = input.Get<Vector2>();
        inputX = inputVector.x;
        inputY = inputVector.y;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(engines.position, force);
    }
}
