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
        return Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 *
            (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;
    }

    // Returns first derivative B'(t) for given parameter 0 <= t <= 1
    public Vector3 GetFirstDerivative(float t)
    {
        return (-3) * Mathf.Pow(1 - t, 2) * p0 + 3 * p1 * (3 * Mathf.Pow(t, 2)
            - 4 * t + 1) + 3 * p2 * (-3 * Mathf.Pow(t, 2) + 2 * t) + 3 * Mathf.Pow(t, 2) * p3;
    }

    // Returns second derivative B''(t) for given parameter 0 <= t <= 1
    public Vector3 GetSecondDerivative(float t)
    {
        return 6 * (1 - t) * p0 + (18 * t - 12) * p1 - p2 * (18 * t - 6) + 6 * t * p3;
    }

    // Returns the tangent vector to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetTangent(float t)
    {
        return GetFirstDerivative(t).normalized;
    }

    // Returns the Frenet normal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetNormal(float t)
    {
        return  Vector3.Cross(GetTangent(t), GetBinormal(t)).normalized;
    }

    // Returns the Frenet binormal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetBinormal(float t)
    {
        Vector3 _t = (GetFirstDerivative(t) + GetSecondDerivative(t)).normalized;
        return Vector3.Cross(GetTangent(t), _t).normalized;
    }

    // Calculates the arc-lengths lookup table
    public void CalcCumLengths()
    {
        cumLengths = new float[numSteps + 1];
        cumLengths[0] = 0;
        for (int i = 1; i <= numSteps; i++)
        {
            float t_prev = (float)(i - 1) / numSteps;
            float t = (float)i / numSteps;
            Vector3 prevPoint = GetPoint(t_prev);
            Vector3 currPoint = GetPoint(t);
            float currDist = Vector3.Distance(prevPoint, currPoint);
            cumLengths[i] = cumLengths[i - 1] + currDist;
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
        // binary search for relavent index
        int l = 0;
        int r = numSteps;
        while (l < r){
            int m = (l + r) / 2;
            if (a == cumLengths[m]){
                return m;
            } 
            if (a > cumLengths[m]){ 

                l = m + 1;
            }
            else{
                r = m - 1;
            }
        }
        if (a == cumLengths[l]){
                return l;
        }

        // lower = index of closest cell with lower value
        // higher = index of closest cell with higher value
        int lower = (cumLengths[l] < a)? l : (l - 1);
        int higher = lower + 1;

        float proportion = Mathf.InverseLerp(cumLengths[lower], cumLengths[higher], a);
        float t = Mathf.Lerp((float) lower / numSteps, (float) higher / numSteps, proportion);
        return t;
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



