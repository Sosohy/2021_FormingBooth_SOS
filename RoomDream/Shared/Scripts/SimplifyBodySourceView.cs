using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class SimplifyBodySourceView : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mJointObject; //prefab 만들어 둔 것 넣을 예정 - PR_Joint

    public double addX = 0;
    public double addY= 0;
    public int addZ = 0;


    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>(); // Camera로 보여지는 모든 body 정보를 넣을 것.
    private List<JointType> _joints = new List<JointType> // Connecting Prefabs
    {
        JointType.HandLeft,
        JointType.HandRight
    };

    private void Update()
    {
        #region Get Kinect Data
        Body[] data = mBodySourceManager.GetData();
        if (data == null)
            return;

        List<ulong> trackedIds = new List<ulong>(); // body로부터 얻는 모든 tracking id를 저장할 공간
        foreach(var body in data)
        {
            if (body == null)
                continue;

            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }
        #endregion

        #region Delete Kinect bodies 
        List<ulong> knownIds = new List<ulong>(mBodies.Keys);
        foreach(ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(mBodies[trackingId]); // Destroy body object

                mBodies.Remove(trackingId); // Remove from list
            }
        }

        #endregion

        #region Create Kinect bodies
        foreach(var body in data) // soruce connect data
        {
            if (body == null)
                continue;

            if (body.IsTracked)
            {
                // If body isn't tracked, create body 
                if (!mBodies.ContainsKey(body.TrackingId))
                    mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                // Update positions - 2d object body
                UpdateBodyObject(body, mBodies[body.TrackingId]);
            }
        }

        #endregion
    }

    private GameObject CreateBodyObject(ulong id)
    {
        // Create body parent
        GameObject body = new GameObject("Body:" + id);
        body.transform.parent = GameObject.Find("PR_OfflineBalloonGame").transform;

        // Create Joints
        foreach (JointType joint in _joints)
        {
            // Create Object
            GameObject newJoint = Instantiate(mJointObject);
            newJoint.name = joint.ToString();

            // Parent to body
            newJoint.transform.parent = body.transform;
        }

        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject) 
    {
        // Update Joints
        foreach(JointType _joint in _joints)
        {
            // Get new target position
            Joint sourceJoint = body.Joints[_joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);

            if (SceneManager.GetActiveScene().name == "BallonScene")
                targetPosition.z = 10;
            else
                targetPosition.z = addZ;


            // Get Joint, set new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            jointObject.position = targetPosition;
        }
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        double tmpY = 0;
        if (SceneManager.GetActiveScene().name == "BallonScene")
        {
            tmpY = Constants.bubbleY;
        }

        return new Vector3((float)(joint.Position.X * 10 + addX), (float)(joint.Position.Y * 10 + addY), (float)(joint.Position.Z * 10 ));
    }
}