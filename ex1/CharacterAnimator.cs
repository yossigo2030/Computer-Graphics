using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public TextAsset BVHFile; // The BVH file that defines the animation and skeleton
    public bool animate; // Indicates whether or not the animation should be running
    private BVHData data; // BVH data of the BVHFile will be loaded here
    private int currFrame = 0; // Current frame of the animation
    private double secondPassed = 0.0; // 

    // Start is called before the first frame update
    void Start()
    {
        BVHParser parser = new BVHParser();
        data = parser.Parse(BVHFile);
        CreateJoint(data.rootJoint, Vector3.zero);
    }

    // Returns a Matrix4x4 representing a rotation aligning the up direction of an object with the given v
    Matrix4x4 RotateTowardsVector(Vector3 v)
    {
        Vector3 normalize = v.normalized;
        float tetaX = 90 - Mathf.Rad2Deg * Mathf.Atan2(normalize[1], normalize[2]);
        Matrix4x4 Rx = MatrixUtils.RotateX(tetaX);
        float tetaZ = 90 - (Mathf.Rad2Deg * Mathf.Atan2((float)Math.Sqrt((normalize[1]* normalize[1]) + (normalize[2]* normalize[2])), normalize[0]));
        Matrix4x4 Rz = MatrixUtils.RotateZ(-tetaZ);
        return Rx * Rz;  
    }

    // Creates a Cylinder GameObject between two given points in 3D space
    GameObject CreateCylinderBetweenPoints(Vector3 p1, Vector3 p2, float diameter)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Matrix4x4 T = MatrixUtils.Translate(new Vector3((p1.x + p2.x) / 2f, (p1.y + p2.y) / 2f, (p1.z + p2.z) / 2f));
        Vector3 directionOfRotation = p1 - p2;
        Matrix4x4 R = RotateTowardsVector(directionOfRotation);
        float scale = Vector3.Distance(p1, p2);
        Matrix4x4 S;      
        S = MatrixUtils.Scale(new Vector3(diameter, scale/2, diameter));
        MatrixUtils.ApplyTransform(cylinder, T * R * S);
        return cylinder;
    }

    // Creates a GameObject representing a given BVHJoint and recursively creates GameObjects for it's child joints
    GameObject CreateJoint(BVHJoint joint, Vector3 parentPosition)
    {
        Matrix4x4 bodyScale = MatrixUtils.Scale(new Vector3(2, 2, 2));
        Matrix4x4 headSCale = MatrixUtils.Scale(new Vector3(8, 8, 8));
        Matrix4x4 parentTranslations = MatrixUtils.Translate(parentPosition);
        return CreateJointHelper(joint, parentPosition, bodyScale, headSCale);     
    }

    GameObject CreateJointHelper(BVHJoint joint, Vector3 parentPosition, Matrix4x4 bodyScale, Matrix4x4 headSCale)
    {
        joint.gameObject = new GameObject(joint.name);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = joint.gameObject.transform;
        Matrix4x4 scaleMatrix =  joint.name == "Head" ? headSCale : bodyScale;
        MatrixUtils.ApplyTransform(sphere, scaleMatrix);
        Matrix4x4 translate = MatrixUtils.Translate(joint.offset);
        MatrixUtils.ApplyTransform(joint.gameObject, MatrixUtils.Translate(parentPosition) * translate);
        Vector4 newParentPosition = MatrixUtils.Translate(parentPosition) * new Vector4(joint.offset.x, joint.offset.y, joint.offset.z, 1);
        foreach (BVHJoint item in joint.children)
        {
            Vector4 translate111 = MatrixUtils.Translate(parentPosition) * translate * (new Vector4(item.offset.x, item.offset.y, item.offset.z, 1));           
            GameObject child = CreateJointHelper(item, new Vector3(newParentPosition.x, newParentPosition.y, newParentPosition.z), bodyScale, headSCale);
            GameObject cylinder = CreateCylinderBetweenPoints(new Vector3(newParentPosition.x, newParentPosition.y, newParentPosition.z), new Vector3(translate111.x, translate111.y, translate111.z), 0.5f);
            cylinder.transform.parent = joint.gameObject.transform;
        }
        return sphere;
    }


    // Transforms BVHJoint according to the keyframe channel data, and recursively transforms its children
    private void TransformJoint(BVHJoint joint, Matrix4x4 parentTransform, float[] keyframe)
    {
        Matrix4x4 R = Matrix4x4.identity;
        if (!joint.isEndSite)
        {
            Matrix4x4[] array = new Matrix4x4[3];
            array[joint.rotationOrder[0]] = MatrixUtils.RotateX(keyframe[joint.rotationChannels.x]);
            array[joint.rotationOrder[1]] = MatrixUtils.RotateY(keyframe[joint.rotationChannels.y]);
            array[joint.rotationOrder[2]] = MatrixUtils.RotateZ(keyframe[joint.rotationChannels.z]);
            R = array[0] * array[1] * array[2];
        }
        Matrix4x4 transform = MatrixUtils.Translate(joint.offset);
        Matrix4x4 M = parentTransform * transform  * R;
        MatrixUtils.ApplyTransform(joint.gameObject, M);
        foreach (BVHJoint item in joint.children)
        {           
            TransformJoint(item, M, keyframe);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animate)
        {            
            secondPassed += Time.deltaTime;
            int frame =(int)(secondPassed /  data.frameLength);            
            if (currFrame != frame)
            {
                if(frame >= data.numFrames)
                {
                    frame = 0;
                    secondPassed = 0;
                }
                currFrame = frame;              
                TransformJoint(data.rootJoint, MatrixUtils.Translate(new Vector3(data.keyframes[currFrame][data.rootJoint.positionChannels.x], 
                    data.keyframes[currFrame][data.rootJoint.positionChannels.y], data.keyframes[currFrame][data.rootJoint.positionChannels.z])),
                    data.keyframes[currFrame]);
            }   
        }
    }
}
