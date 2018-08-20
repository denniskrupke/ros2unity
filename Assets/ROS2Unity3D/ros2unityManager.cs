using UnityEngine;
using System.Collections;
using RosBridge;
using System;

using AbstractionLayer;
using RosMessages;

public class ros2unityManager : MonoBehaviour {
    [Header("ROSbridge settings")]
    [SerializeField]
    private string rosmaster_ip = "134.100.13.221";

    [SerializeField]
    private string rosbridge_port = "8080";

    [SerializeField]
    private bool verbose = true;

    [SerializeField]
    private bool autoConnect = false;

    [Header("Subscribe to...")]
    [SerializeField]
    private bool collisionObject;

    //private int millisSinceLastGripperCommand = Environment.TickCount;
    //private static long messageSeq = 0;


    void Start () {      
		if (autoConnect && !RosBridgeClient.GetInstance(this.rosmaster_ip, this.rosbridge_port, this.verbose, this.collisionObject).IsConnected()) {
			Connect ();
		}
	}

	// starts the connection to the ROSbridge
	private void Connect(){
		if (verbose) {
			Debug.Log ("Try to connect with ROSbridge via websockets.");
		}
		RosBridgeClient.GetInstance(this.rosmaster_ip, this.rosbridge_port, this.verbose, this.collisionObject).Start ();
	}


	void Update () {		
        // Do stuff for each frame
	}

    private void OnApplicationQuit()
    {
        RosBridgeClient.GetInstance(this.rosmaster_ip, this.rosbridge_port, this.verbose, this.collisionObject).Stop();
    }

    // for efficiency reasons, motion of the robot joints and updates of the streamed video are done with 30 FPS
    void FixedUpdate(){
		// processing is only reasonable if connected to the ROSbridge
		if (RosBridgeClient.GetInstance(this.rosmaster_ip, this.rosbridge_port, this.verbose, this.collisionObject).IsConnected()) {
            // Do stuff with exactly 30 Hz
		}
	}


	// Closes the connection to the ROSbridge
	void OnDestroy(){
        RosBridgeClient.GetInstance(this.rosmaster_ip, this.rosbridge_port, this.verbose, this.collisionObject).Disconnect ();
        //TODO stop the threads
	}

    public RosBridgeClient GetRosBridgeClient() {
        return RosBridgeClient.GetInstance(this.rosmaster_ip, this.rosbridge_port, this.verbose, this.collisionObject);
    }
}
