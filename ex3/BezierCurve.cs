using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class BezierCurve : MonoBehaviour
{
    // Bezier control points
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    private float[] cumLengths; // Cumulative lengths lookup table
    private readonly int numSteps = 128; // Number of points to sample for the cumLengths LUT

    // Returns position B(t) on the Bezier curve for given parameter 0 <= t <= 1
    public Vector3 GetPoint(float t)
    {
        Vector3 b = Mathf.Pow(1f - t, 3) * p0 +
                    3 * Mathf.Pow(1f - t, 2) * t * p1 + 
                    3 * (1f - t) * Mathf.Pow(t, 2) * p2 + 
                    Mathf.Pow(t, 3) * p3 ;  
        return b;
    }

    // Returns first derivative B'(t) for given parameter 0 <= t <= 1
    public Vector3 GetFirstDerivative(float t)
    {
        Vector3 firstDerivative = 3 * Mathf.Pow(1f - t, 2) * (p1 - p0) +
                                  6 * (1f - t) * t * (p2 - p1) +
                                  3 * Mathf.Pow(t, 2) * (p3 - p2);
        return firstDerivative;
    }

    // Returns second derivative B''(t) for given parameter 0 <= t <= 1
    public Vector3 GetSecondDerivative(float t)
    {
        Vector3 secondDerivative = 6 * (1f - t) * (p2 - 2f * p1 + p0) +
                                   6 * t * (p3 - 2f * p2 + p1);
        return secondDerivative;
    }

    // Returns the tangent vector to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetTangent(float t)
    {
        return GetFirstDerivative(t).normalized;
    }

    // Returns the Frenet normal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetNormal(float t)
    {
        Vector3 normal = Vector3.Cross(GetTangent(t), GetBinormal(t));
        return normal;
    }

    // Returns the Frenet binormal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetBinormal(float t)
    {
        Vector3 tTag = (GetFirstDerivative(t) + GetSecondDerivative(t)).normalized;
        Vector3 binormal = Vector3.Cross(GetTangent(t), tTag);
        return binormal;
    }

    // Calculates the arc-lengths lookup table
    public void CalcCumLengths()
    {
        cumLengths = new float[numSteps + 1];
        cumLengths[0] = 0f;
        Vector3 cur = new Vector3();
        Vector3 prev = new Vector3();

        for (int i = 1; i < cumLengths.Length; i++)
        {
            cur = GetPoint((float)i / numSteps);
            prev = GetPoint((i - 1f) / numSteps);
            cumLengths[i] = Vector3.Distance(cur, prev) + cumLengths[i - 1];
        }
    }

    // Returns the total arc-length of the Bezier curve
    public float ArcLength()
    {
        return cumLengths[numSteps];
    }

    // Returns approximate t s.t. the arc-length to B(t) = arcLength
    public float ArcLengthToT(float a)
    {
        float epsilon = 0.00001f;
        for (int i = 0; i < cumLengths.Length; i++)
        {
            if (Mathf.Abs(a - cumLengths[i]) <= epsilon)
            {
                return (float)i / numSteps;
            }
            if (cumLengths[i + 1] - a > epsilon)
            {
                float weight = Mathf.InverseLerp(cumLengths[i], cumLengths[i + 1], a);
                float start = (1f - weight) * ((float)i / numSteps);
                float end = weight * ((float)(i + 1) / numSteps);
                return start + end;
            }
        }
        return 0;
    }

    // Start is called before the first frame update
    public void Start()
    {
        Refresh();
    }

    // Update the curve and send a message to other components on the GameObject
    public void Refresh()
    {
        CalcCumLengths();
        if (Application.isPlaying)
        {
            SendMessage("CurveUpdated", SendMessageOptions.DontRequireReceiver);
        }
    }

    // Set default values in editor
    public void Reset()
    {
        p0 = new Vector3(1f, 0f, 1f);
        p1 = new Vector3(1f, 0f, -1f);
        p2 = new Vector3(-1f, 0f, -1f);
        p3 = new Vector3(-1f, 0f, 1f);

        Refresh();
    }
}



