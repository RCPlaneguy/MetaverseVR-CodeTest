using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyantObject : MonoBehaviour
{
    [Header("Water")]
    [SerializeField] private float waterHeight = 0.0f;

    [Header("Waves")]
    [SerializeField] private Material waterMat;
    [SerializeField] private float steepnessCorrection = -0.008f;
    [SerializeField] private float wavelengthCorrection = 125f;
    [SerializeField] private float speedCorrection = -125f;

    [Header("Buoyancy")]
    [SerializeField] private bool usePhysics = true;
    [Range(0.01f, 5f)] public float strength = 1f;
    [Range(0.2f, 5f)] public float objectDepth = 1f;

    public float velocityDrag = 0.99f;
    public float angularDrag = 0.5f;

    [Header("Effectors")]
    [Tooltip("Make sure these are inputted clockwise if not using physics!")]
    public Transform[] effectors;

    [Header("Debug")]
    [SerializeField] private float steepness;
    [SerializeField] private float wavelength;
    [SerializeField] private float waveSpeed;
    [SerializeField] private Vector4 directions;

    private Rigidbody rb;
    private Vector3[] effectorProjections;
    private bool _inContactWithWater;
    public bool InContactWithWater => _inContactWithWater;

    private void Awake()
    {
        if (usePhysics)
        {
            // Get rigidbody
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
        }

        effectorProjections = new Vector3[effectors.Length];
        for (int i = 0; i < effectors.Length; i++)
        {
            effectorProjections[i] = effectors[i].position;
        }
    }

    private void OnDisable()
    {
        if (usePhysics)
            rb.useGravity = true;
    }

    protected virtual void FixedUpdate()
    {
        steepness = waterMat.GetFloat("_WaveSteepness") * steepnessCorrection;
        wavelength = waterMat.GetFloat("_WaveLength") * wavelengthCorrection;
        waveSpeed = waterMat.GetFloat("_WaveSpeed") * speedCorrection;
        directions = waterMat.GetVector("_WaveDirections");

        // set to true if any effector is underwater
        _inContactWithWater = false;

        int effectorCount = effectors.Length;
        for (int i = 0; i < effectorCount; i++)
        {
            Vector3 effectorPosition = effectors[i].position;

            Vector3 waveDisplacement = GerstnerWaveDisplacement.GetWaveDisplacement(effectorPosition, steepness, wavelength, waveSpeed, new float[] { directions.x, directions.y, directions.z, directions.w });

            effectorProjections[i] = effectorPosition;
            effectorProjections[i].y = waterHeight + waveDisplacement.y;

            if (!usePhysics)
                continue;

            // gravity
            rb.AddForceAtPosition(Physics.gravity / effectorCount, effectorPosition, ForceMode.Acceleration);

            float effectorHeight = effectorPosition.y;
            float waveHeight = effectorProjections[i].y;

            // if not submerged, continue
            if (effectorHeight > waveHeight)
                continue;

            _inContactWithWater = true;

            float submersion = Mathf.Clamp01(waveHeight - effectorHeight) / objectDepth;
            float buoyancy = Mathf.Abs(Physics.gravity.y) * submersion * strength;

            // buoyancy
            rb.AddForceAtPosition(Vector3.up * buoyancy, effectorPosition, ForceMode.Acceleration);

            // drag
            rb.AddForce(-rb.velocity * (velocityDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);

            // torque
            rb.AddTorque(-rb.angularVelocity * (angularDrag * Time.fixedDeltaTime), ForceMode.Impulse);
        }

        if (!usePhysics)
        {
            if (effectorCount >= 3)
            {
                Plane plane = new Plane(
                    effectorProjections[0],
                    effectorProjections[1],
                    effectorProjections[2]);
            }
            else
            {

            }
        }
    }

    private void OnDrawGizmos()
    {
        if (effectors == null)
            return;

        for (int i = 0; i < effectors.Length; i++)
        {
            if (!Application.isPlaying && effectors[i] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(effectors[i].position, 0.06f);
            }
            else
            {
                if (effectors[i] == null)
                    return;

                Gizmos.color = effectors[i].position.y < effectorProjections[i].y
                    ? Color.red
                    : Color.green;

                Gizmos.DrawSphere(effectors[i].position, 0.06f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(effectorProjections[i], 0.06f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(effectors[i].position, effectorProjections[i]);
            }
        }
    }
}