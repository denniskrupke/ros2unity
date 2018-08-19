using System;
using UnityEngine;
using RosBridge;
using RosMessages;
using RosMessages.geometry_msgs;
using RosMessages.moveit_msgs;

public class PR2Interface : MonoBehaviour {
    private ros2unityManager manager = null;

	// Use this for initialization
	void Start () {
        manager = gameObject.GetComponent(typeof(ros2unityManager)) as ros2unityManager;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P)) { // outgoing job
            SendPickCommandToRos(); //TODO: do this e.g. caused by the click on the phantom-button
        }

        ProcessCollisionObject(); // incoming job
    }

    private void SendPickCommandToRos() {
        if (manager != null) manager.GetRosBridgeClient().EnqueRosCommand(new RosPublish(
             "/pr2_phantom/pick", //this is the topic
             GeneratePickMessage("name of the object to be grasped (todo)", new Vector3()) //this is the pointStamped-message
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
        header.frame_id = "name of the object to be grasped"; //TODO: get name of the selected object
        pickPoint.header = header;
        Point point = new Point();
        point.x = 0; //TODO: take actual data from phantom
        point.y = 0; //TODO: take actual data from phantom
        point.z = 0; //TODO: take actual data from phantom
        pickPoint.point = point;

        return pickPoint;
    }

    public void ProcessCollisionObject() {
        CollisionObject co = null;
        if (manager != null) {
            if (co != manager.GetRosBridgeClient().GetLatestCollisionObject())
            {
                co = manager.GetRosBridgeClient().GetLatestCollisionObject();
                //TODO: spawn the object in ithe scene
            }
        }
    }
}
