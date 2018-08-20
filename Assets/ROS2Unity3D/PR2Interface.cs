using System;
using UnityEngine;
using RosBridge;
using RosMessages;
using RosMessages.geometry_msgs;
using RosMessages.moveit_msgs;
using System.Collections.Generic;

public class PR2Interface : MonoBehaviour {
    private ros2unityManager manager = null;

	// Use this for initialization
	void Start () {
        manager = gameObject.GetComponent(typeof(ros2unityManager)) as ros2unityManager;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P)) { // outgoing job
            SendPickCommandToRos(
                GeneratePickMessage(
                    "object", //frame of the recognized object (bottle, box, aspirin, ...) 
                    new Vector3(0.01f,0,12.0f) //position in the object frame for the pick action
                )
            ); //TODO: do this e.g. caused by the click on the phantom-button
        }

        // TODO: processing only when reasonable/neccessary
        ProcessCollisionObjects(); // incoming job   
    }

    private void SendPickCommandToRos(PointStamped messageData) {
        if (manager != null) manager.GetRosBridgeClient().EnqueRosCommand(new RosPublish(
             "/pr2_phantom/plan_pick", //this is the topic
             messageData
         ));
        else Debug.Log("ros2unity manager not available");
    }

    private PointStamped GeneratePickMessage(string targetName, Vector3 positionInObjectFrame) {
        PointStamped pickPoint = new PointStamped();
        Stamp stamp = new Stamp();
        DateTime currentTime = DateTime.Now;
        stamp.secs = currentTime.Second;
        stamp.nsecs = currentTime.Millisecond;
        Header header = new Header();
        header.stamp = stamp;
        header.frame_id = targetName; 
        pickPoint.header = header;
        Point point = new Point();
        // TODO: correct coordinate transform
        point.x = positionInObjectFrame.x; 
        point.y = positionInObjectFrame.y; 
        point.z = positionInObjectFrame.z; 
        pickPoint.point = point;

        return pickPoint;
    }


    private void ProcessCollisionObjects() {
        if (manager != null)
        {
            if (manager.GetRosBridgeClient().GetCollisionObjects().Count > 0)
            {
                foreach (KeyValuePair<string, CollisionObject> co in manager.GetRosBridgeClient().GetCollisionObjects())
                {
                    Debug.Log(co.Value.id);
                    Debug.Log(co.Value.primitive_poses[0].position.x + ", " +
                        co.Value.primitive_poses[0].position.y + ", " +
                        co.Value.primitive_poses[0].position.z);
                }
                
            }
        }
        else Debug.Log("ROSbridge manager is null");
    }
}
