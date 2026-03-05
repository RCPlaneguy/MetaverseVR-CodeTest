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
    [SerializeField] private float maxDockingForce;
    [SerializeField] private float dockingTime;
    [SerializeField] private TextMeshProUGUI dockingTimer;
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

        string reason = string.Empty;

        _timeRemaining -= Time.deltaTime;

        if (Mathf.Abs(playerBoat.forceMagnitude) > maxDockingForce)
        {
            _timeRemaining = dockingTime;
            reason = "\nMoving too much";
        }

        if (!IsBoatDockedPortside())
        {
            _timeRemaining = dockingTime;
            if (!string.Equals(reason, string.Empty))
                reason += " + ";
            reason += "\nNot docked portside";
        }
        if (string.Equals(reason, string.Empty))
            reason = "\nGood to go!";

        dockingTimer.text = $"Docking: {_timeRemaining:0.0}s remaining: {reason}";

        if (_timeRemaining <= 0f)
        {
            _isDocking = false;
            dockingTimer.transform.parent.gameObject.SetActive(false);
            onFullyDocked.Invoke();
        }
    }

    private bool IsBoatDockedPortside()
    {
        Vector3 starboard = playerBoat.transform.right;
        Vector3 ahead = playerBoat.transform.forward;
        Vector3 portwards = cleat.forward;

        // if above -0.8f, it's angled too far away from portside
        starboardDot = Vector3.Dot(starboard, portwards);
        if (starboardDot > -0.8f)
            return false;

        // checking that it's not quite fully ahead or 
        // astern to make sure it's not pointing into dock
        aheadDot = Vector3.Dot(ahead, portwards);
        return Mathf.Abs(aheadDot) < 0.75f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PlayerTag))
            return;

        _isDocking = true;
        _timeRemaining = dockingTime;
        dockingTimer.transform.parent.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PlayerTag))
            return;

        _isDocking = false;
        _timeRemaining = dockingTime;
        dockingTimer.transform.parent.gameObject.SetActive(false);
    }
}
