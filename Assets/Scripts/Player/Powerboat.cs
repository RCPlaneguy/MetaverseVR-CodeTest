using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Powerboat : MonoBehaviour
{
    [Header("Helm")]
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float maxSpeedReverse = 15f;
    [SerializeField] private float turningSpeed = 10f;

    [Header("Elements")]
    [SerializeField] private Rigidbody rb;
    public Rigidbody Rigidbody => rb;
    [SerializeField] private Transform engines;
    [SerializeField] private float maxEngineAngle = 30f;

    [Header("Visual Effects")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float maxVignetteIntensity = 0.5f;

    [Header("Debug")]
    [SerializeField] private Vector2 inputs = new();
    [SerializeField] private float forceMagnitude;
    [SerializeField] private float currentSpeed;

    [Space]
    [SerializeField] private float torqueMagnitude;
    [SerializeField] private float currentTurningSpeed;

    private Vector3 engineRot;
    private Vignette _vignette;
    private bool _vignetteFound;

    // divide the speed values by this to get the 
    // force value necessary to achieve the corresponding top speed
    private const float speedToForce = 4.5f;

    private void Start()
    {
        engineRot = engines.localEulerAngles;
        _vignetteFound = globalVolume.profile.TryGet(out _vignette);
        if (!_vignetteFound)
            Debug.LogError("No vignette found!");
    }

    private void Update()
    {
        if (GameManager.GamePaused)
            return;

        HandleMovement();
        HandleVisuals();
    }

    private void HandleMovement()
    {
        // calculate force magnitude using different force values for forward and reverse
        forceMagnitude = inputs.y > 0
            ? ToForce(maxSpeed) * inputs.y
            : ToForce(maxSpeedReverse) * inputs.y;

        // apply force to rb along forward axis
        rb.AddForce(forceMagnitude * transform.forward);

        // calculate torque magnitude
        torqueMagnitude = turningSpeed * inputs.x;

        // if reversing, reverse direction of turn
        if (inputs.y < 0)
            torqueMagnitude *= -1f;

        // apply torque to rb along vertical axis
        rb.AddTorque(torqueMagnitude * transform.up);

        // update values for use elsewhere and for debug
        currentSpeed = rb.velocity.magnitude;
        currentTurningSpeed = rb.angularVelocity.magnitude;
    }

    private void HandleVisuals()
    {
        // angle engines based on inputx
        engineRot.y = -inputs.x * maxEngineAngle;
        engines.localEulerAngles = engineRot;

        // handle speed vignette
        float ratio = currentSpeed / maxSpeed;
        if (ratio > 1)
            ratio = 1;
        _vignette.intensity.value = maxVignetteIntensity * ratio;
    }

    public void OnMove(InputValue input)
    {
        inputs = input.Get<Vector2>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + (Vector3.up * 2), rb.velocity);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + (Vector3.up * 2), forceMagnitude * transform.forward);


        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + (Vector3.up * 2), torqueMagnitude * transform.up);
    }

    private float ToForce(float speed) => speed / speedToForce;
}
