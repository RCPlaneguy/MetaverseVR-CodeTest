using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Debug")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector3 directionRot;
    [SerializeField] private float inputX;
    [SerializeField] private float inputY;
    [SerializeField] private Vector3 engineRot;
    [SerializeField] private Vector3 force;

    private const float SteeringSpeed = 0.6f;
    private const float ForceMultiplier = 1200f;

    private void Start()
    {
        engineRot = engines.localEulerAngles;
        directionRot = transform.eulerAngles;
    }

    private void Update()
    {
        HandlePhysicsMovement();
    }

    private void HandlePhysicsMovement()
    {
        // accelerate boat in direction it's facing, with impulse force scaled based on inputY
        force = ForceMultiplier * inputY * maxAcceleration * Time.deltaTime * transform.forward;

        // apply impulse force
        rb.AddForce(force, ForceMode.Force);

        // angle engines based on inverse of inputX
        float steerAngle = -inputX * turnSensitivty * maxSteerAngle * 2;
        engineRot.y = Mathf.LerpAngle(engines.localEulerAngles.y, steerAngle, SteeringSpeed);
        engines.localEulerAngles = engineRot;

        currentSpeed = rb.velocity.magnitude;

        // if boat is travelling below minimum steerage velocity, don't steer
        if (rb.velocity.magnitude < minSteerageSpeed)
            return;

        // determine turn increment based on current steerangle
        float increment = -steerAngle * Time.deltaTime;

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
