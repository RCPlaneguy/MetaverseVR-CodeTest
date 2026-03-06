using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunseeker : BuoyantObject
{
    [SerializeField] private BezierPath path;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float minTargetDistance = 0.01f;

    [Header("Debug")]
    [SerializeField] private int currentTargetIndex;
    [SerializeField] private Vector3 currentPosTarget;
    [SerializeField] private float distToTarget;
    private LineRenderer lineRen;
    private bool _reverseDirection;

    private void Start()
    {
        // retrieve line renderer and path info
        lineRen = path.LineRen;
        _reverseDirection = path.ReverseDir;

        // get random start position
        int randomIndex = Random.Range(0, lineRen.positionCount);
        transform.position = lineRen.GetPosition(randomIndex);

        // get target position (next one along, or first position if at end)
        if (_reverseDirection)
        {
            currentTargetIndex = randomIndex == 0
                ? lineRen.positionCount - 1
                : randomIndex - 1;
        }
        else
        {
            currentTargetIndex = randomIndex == lineRen.positionCount - 1
               ? 0
               : randomIndex + 1;
        }
        currentPosTarget = lineRen.GetPosition(currentTargetIndex);

        // rotate ship to face targetPos
        transform.LookAt(currentPosTarget);
    }

    protected override void FixedUpdate()
    {
        // run basic buoyancy fixed update
        base.FixedUpdate();

        // transfer current floating height to target to avoid breaching whale behaviour
        currentPosTarget.y = transform.position.y;

        // move ship position towards next position
        transform.position = Vector3.MoveTowards(transform.position, currentPosTarget, speed * Time.deltaTime);

        // rotate ship rotation towards next position
        Vector3 dirToTarget = currentPosTarget - transform.position;
        if (dirToTarget != Vector3.zero)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dirToTarget), turnSpeed * Time.deltaTime);

        distToTarget = dirToTarget.magnitude;

        // if reached target
        if (distToTarget <= minTargetDistance)
        {
            if (_reverseDirection)
            {
                currentTargetIndex--;
                if (currentTargetIndex <= 0)
                    currentTargetIndex = lineRen.positionCount - 1;
            }
            else
            {
                currentTargetIndex++;
                if (currentTargetIndex >= lineRen.positionCount)
                    currentTargetIndex = 0;
            }

            currentPosTarget = lineRen.GetPosition(currentTargetIndex);
        }
    }
}
