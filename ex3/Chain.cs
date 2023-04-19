using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Chain : MonoBehaviour
{
    private BezierCurve curve; // The Bezier curve around which to build the chain
    private List<GameObject> chainLinks = new List<GameObject>(); // A list to contain the chain links GameObjects

    public GameObject ChainLink; // Reference to a GameObject representing a chain link
    public float LinkSize = 2.0f; // Distance between links

    // Awake is called when the script instance is being loaded
    public void Awake()
    {
        curve = GetComponent<BezierCurve>();
    }
    /*
    public void ShowChain()
    {
        // Clean up the list of old chain links
        foreach (GameObject link in chainLinks)
        {
            Destroy(link);
        }

        // Your implementation here...
        curve.CalcCumLengths();
        var curveLength = curve.ArcLength();
        float currentLength = 0f;
        bool flip = false;
        while (currentLength < curveLength)
        {
            var tEstimnate = curve.ArcLengthToT(currentLength);
            var upSide = flip ? curve.GetBinormal(tEstimnate) : curve.GetNormal(tEstimnate);
            flip = !flip;
            var bPoint = curve.GetPoint(tEstimnate);
            var forwardSide = curve.GetFirstDerivative(tEstimnate);
            chainLinks.Add(CreateChainLink(bPoint, forwardSide, upSide));
            currentLength += LinkSize;
        }
    }
    */

    // Constructs a chain made of links along the given Bezier curve, updates them in the chainLinks List
    public void ShowChain()
    {
        // Clean up the list of old chain links
        foreach (GameObject link in chainLinks)
        {
            Destroy(link);
        }
        float positionOfNewLink = -LinkSize;
        curve.CalcCumLengths();
        float total = curve.ArcLength();
        bool frontOrBack = false;
        while (true)
        {
            positionOfNewLink += LinkSize;
            if (Mathf.Abs(positionOfNewLink - total) < 0.00001f)
            {
                break;
            }
            float t = curve.ArcLengthToT(positionOfNewLink);
            Vector3 position = curve.GetPoint(t);
            Vector3 forward = curve.GetFirstDerivative(t);
            Vector3 up;
            if (frontOrBack)
            {
                up = curve.GetBinormal(t);
            }
            else
            {
                up = curve.GetNormal(t);
            }
            frontOrBack = !frontOrBack;
            chainLinks.Add(CreateChainLink(position, forward, up));
        }
    }

    // Instantiates & returns a ChainLink at given position, oriented towards the given forward and up vectors
    public GameObject CreateChainLink(Vector3 position, Vector3 forward, Vector3 up)
    {
        GameObject chainLink = Instantiate(ChainLink);
        chainLink.transform.position = position;
        chainLink.transform.rotation = Quaternion.LookRotation(forward, up);
        chainLink.transform.parent = transform;
        return chainLink;
    }

    // Rebuild chain when BezierCurve component is changed
    public void CurveUpdated()
    {
        ShowChain();
    }
}

[CustomEditor(typeof(Chain))]
class ChainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Show Chain"))
        {
            var chain = target as Chain;
            chain.ShowChain();
        }
    }
}