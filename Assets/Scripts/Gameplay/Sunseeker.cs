using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunseeker : MonoBehaviour
{
    [SerializeField] private BezierPath path;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float minTargetDistance = 0.01f;

    [Header("Debug")]
    [SerializeField] private int currentTargetIndex;
    [SerializeField] private Vector3 currentPosTarget;
    [SerializeField] private float distToTarget;
    [SerializeField] private LineRenderer lineRen;

    private void Start()
    {
        // retrieve line renderer
        lineRen = path.LineRen;

        // get random start position
        int randomIndex = Random.Range(0, lineRen.positionCount);
        transform.position = lineRen.GetPosition(randomIndex);

        // get target position (next one along, or first position if at end)
        currentTargetIndex = randomIndex == lineRen.positionCount - 1
           ? 0
           : randomIndex + 1;
        currentPosTarget = lineRen.GetPosition(currentTargetIndex);

        // rotate ship to face targetPos
        transform.LookAt(currentPosTarget);
    }

    private void Update()
    {
        // move ship position towards next position
        transform.position = Vector3.MoveTowards(transform.position, currentPosTarget, speed * Time.deltaTime);

        // rotate ship rotation towards next position
        Vector3 dirToTarget = currentPosTarget - transform.position;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dirToTarget), turnSpeed * Time.deltaTime);

        distToTarget = dirToTarget.magnitude;

        // if reached target
        if (distToTarget <= minTargetDistance)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= lineRen.positionCount)
                currentTargetIndex = 0;

            currentPosTarget = lineRen.GetPosition(currentTargetIndex);
        }
    }
}
