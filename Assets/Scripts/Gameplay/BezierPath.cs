using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath : MonoBehaviour
{
    [SerializeField] private Transform[] controlNodes = { };
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private bool closedPath;

    [Header("Debug")]
    [SerializeField] private float nodeGizmoSize = 1f;

    // non-serialized
    private int curveCount = 0;
    private Vector3 p0;
    private Vector3 p1;
    private Vector3 p2;
    private Vector3 p3;
    private const int Segment_Count = 50;

    [ContextMenu("Draw Curve")]
    private void DrawCurve()
    {
        int nodeCount = controlNodes.Length;
        curveCount = Mathf.CeilToInt(nodeCount / 3f);

        for (int i = 0; i < curveCount; i++)
        {
            bool isLastCurve = i == curveCount - 1;
            int nodeIndex = i * 3;

            p0 = controlNodes[nodeIndex].position;

            p1 = isLastCurve && nodeIndex + 1 >= nodeCount
                ? controlNodes[0].position
                : controlNodes[nodeIndex + 1].position;

            p2 = isLastCurve && nodeIndex + 2 >= nodeCount
                ? controlNodes[0].position
                : controlNodes[nodeIndex + 2].position;

            p3 = isLastCurve && nodeIndex + 3 >= nodeCount
                ? controlNodes[0].position
                : controlNodes[nodeIndex + 3].position;

            for (int j = 1; j <= Segment_Count; j++)
            {
                float t = j / (float)Segment_Count;

                Vector3 point;

                if (!isLastCurve)
                {
                    point = GetCubicBezierPoint(t, p0, p1, p2, p3);
                }
                else
                {
                    if (nodeIndex + 2 == nodeCount - 1)
                    {
                        // if p2 is the last node
                        if (closedPath)
                        {
                            point = GetCubicBezierPoint(t, p0, p1, p2, p3);
                        }
                        else
                        {
                            point = GetQuadraticBezierPoint(t, p0, p1, p2);
                        }
                    }
                    else if (nodeIndex + 1 == nodeCount - 1)
                    {
                        // if p1 is the last node
                        if (closedPath)
                        {
                            point = GetQuadraticBezierPoint(t, p0, p1, p2);
                        }
                        else
                        {
                            point = Vector3.Lerp(p0, p1, t);
                        }
                    }
                    else
                    {
                        // if p0 is the last node
                        if (closedPath)
                        {
                            point = Vector3.Lerp(p0, p1, t);
                        }
                        else
                        {
                            point = p0;
                        }
                    }
                }

                lineRenderer.positionCount = (i * Segment_Count) + j;
                lineRenderer.SetPosition((i * Segment_Count) + (j - 1), point);
            }
        }
    }

    public static Vector3 GetQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // obtain squares of T and its reverse
        float tSquared = t * t;
        float u = 1 - t;
        float uSquared = u * u;

        // use them in the quadratic Bezier point calculation
        Vector3 result = uSquared * p0;
        result += 2 * u * t * p1;
        result += tSquared * p2;

        return result;
    }

    public static Vector3 GetCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // obtain squares and cubes of T and its reverse
        float tSquared = t * t;
        float tCubed = tSquared * t;

        float u = 1 - t;
        float uSquared = u * u;
        float uCubed = uSquared * u;

        // use them in the cubic Bezier point calculation
        Vector3 result = uCubed * p0;
        result += 3 * uSquared * t * p1;
        result += 3 * u * tSquared * p2;
        result += tCubed * p3;

        return result;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0, nodeCount = controlNodes.Length; i < nodeCount; i++)
        {
            Gizmos.color = Color.yellow;
            if (i % 3 == 0 || (i == nodeCount - 1 && !closedPath))
            {
                Gizmos.color = Color.red;
                if (i > 0)
                {
                    Gizmos.DrawLine(controlNodes[i - 1].position, controlNodes[i].position);
                }
                else if (closedPath)
                {
                    Gizmos.DrawLine(controlNodes[^1].position, controlNodes[i].position);
                }

                if (i < nodeCount - 1)
                {
                    Gizmos.DrawLine(controlNodes[i].position, controlNodes[i + 1].position);
                }
            }
            Gizmos.DrawSphere(controlNodes[i].position, nodeGizmoSize / 2);
        }

        DrawCurve();
    }
}
