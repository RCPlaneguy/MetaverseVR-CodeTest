using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DockingZoneManager : MonoBehaviour
{
    [SerializeField] private Powerboat playerBoat;
    [SerializeField] private Transform cleat;
    [SerializeField] private Collider dockTrigger;
    [SerializeField] private float maxDockingVelocity;
    [SerializeField] private float dockingTime;
    [SerializeField] private GameObject dockingTimerObj;
    [SerializeField] private TextMeshProUGUI dockingTimerText;
    [SerializeField] private UnityEvent onFullyDocked = new();

    [Header("Debug")]
    public float aheadDot;
    public float starboardDot;

    private float _timeRemaining;
    private bool _isDocking;

    private const string PlayerTag = "Player";

    private void Update()
    {
        if (!_isDocking)
            return;

        _timeRemaining -= Time.deltaTime;

        if (playerBoat.Rigidbody.velocity.magnitude > maxDockingVelocity)
            _timeRemaining = dockingTime;

        if (!IsBoatDockedPortside())
            _timeRemaining = dockingTime;

        dockingTimerText.text = $"Docking: {Mathf.RoundToInt(_timeRemaining)}s remaining";

        if (_timeRemaining <= 0f)
        {
            _isDocking = false;
            onFullyDocked.Invoke();
        }
    }

    private bool IsBoatDockedPortside()
    {
        Vector3 starboard = playerBoat.transform.right;
        Vector3 ahead = playerBoat.transform.forward;
        Vector3 toDock = Vector3.Normalize(cleat.position - playerBoat.transform.position);

        // if more to starboard, it's not docked portside
        starboardDot = Vector3.Dot(starboard, toDock);
        if (starboardDot > 0)
            return false;

        // checking that it's not quite fully ahead or 
        // astern to make sure it's not pointing into dock
        aheadDot = Vector3.Dot(ahead, toDock);
        return Mathf.Abs(aheadDot) < 0.75f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PlayerTag))
            return;

        _isDocking = true;
        _timeRemaining = dockingTime;
        dockingTimerObj.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PlayerTag))
            return;

        _isDocking = false;
        _timeRemaining = dockingTime;
        dockingTimerObj.SetActive(false);
    }
}
