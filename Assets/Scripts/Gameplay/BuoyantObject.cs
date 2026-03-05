using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyantObject : MonoBehaviour
{
    [Header("Water")]
    [SerializeField] private float waterHeight = 0.0f;

    [Header("Waves")]
    [SerializeField] private Material waterMat;

    [Header("Buoyancy")]
    [Range(0.01f, 5f)] public float strength = 1f;
    [Range(0.2f, 5f)] public float objectDepth = 1f;

    public float velocityDrag = 0.99f;
    public float angularDrag = 0.5f;

    [Header("Effectors")]
    public Transform[] effectors;

    [Header("Debug")]
    [SerializeField] private float steepness;
    [SerializeField] private float wavelength;
    [SerializeField] private float speed;
    [SerializeField] private Vector4 directions;

    private Rigidbody rb;
    private Vector3[] effectorProjections;
    private bool _inContactWithWater;
    public bool InContactWithWater => _inContactWithWater;

    // water material values, retrieved on start

    private readonly Color red = new(0.92f, 0.25f, 0.2f);
    private readonly Color green = new(0.2f, 0.92f, 0.51f);
    private readonly Color blue = new(0.2f, 0.67f, 0.92f);
    private readonly Color orange = new(0.97f, 0.79f, 0.26f);

    private void Awake()
    {
        // Get rigidbody
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        effectorProjections = new Vector3[effectors.Length];
        for (int i = 0; i < effectors.Length; i++)
        {
            effectorProjections[i] = effectors[i].position;
        }
    }

    private void OnDisable()
    {
        rb.useGravity = true;
    }

    private void FixedUpdate()
    {
        steepness = waterMat.GetFloat("_WaveSteepness");
        wavelength = waterMat.GetFloat("_WaveLength");
        speed = waterMat.GetFloat("_WaveSpeed");
        directions = waterMat.GetVector("_WaveDirections");

        // set to true if any effector is underwater
        _inContactWithWater = false;

        for (int i = 0, effectorCount = effectors.Length; i < effectorCount; i++)
        {
            Vector3 effectorPosition = effectors[i].position;

            Vector3 waveDisplacement = GerstnerWaveDisplacement.GetWaveDisplacement(effectorPosition, steepness, wavelength, speed, new float[] { directions.x, directions.y, directions.z, directions.w });

            effectorProjections[i] = effectorPosition;
            effectorProjections[i].y = waterHeight + waveDisplacement.y;

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
    }

    private void OnDrawGizmos()
    {
        if (effectors == null)
            return;

        for (int i = 0; i < effectors.Length; i++)
        {
            if (!Application.isPlaying && effectors[i] != null)
            {
                Gizmos.color = green;
                Gizmos.DrawSphere(effectors[i].position, 0.06f);
            }
            else
            {
                if (effectors[i] == null)
                    return;

                Gizmos.color = effectors[i].position.y < effectorProjections[i].y
                    ? red
                    : green;

                Gizmos.DrawSphere(effectors[i].position, 0.06f);

                Gizmos.color = orange;
                Gizmos.DrawSphere(effectorProjections[i], 0.06f);

                Gizmos.color = blue;
                Gizmos.DrawLine(effectors[i].position, effectorProjections[i]);
            }
        }
    }
}