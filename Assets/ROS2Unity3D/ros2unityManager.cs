using UnityEngine;
using System.Collections;
using RosBridge;
using System;

using AbstractionLayer;
using RosMessages;

public class ros2unityManager : MonoBehaviour {
	public bool autoConnect = false;
	public bool verbose = true;

	public bool imageStreaming = false;
	public bool jointStates = true;
	public bool useLeap = false;
	public bool testLatency = false;
	public bool pointCloud = false;

	public drawImage canvas;
	public actuateManipulator manipulatorControl;
	public processLeapFrames leapController;

	private RosBridgeClient rosBridge;
	private HandControlMessageGenerator gripperMsgGen;
	private JointStateGenerator jointStateMsgGen;

	private int millisSinceLastGripperCommand = Environment.TickCount;
	private static int messageSeq = 0;


	void Start () {
        //gripperMsgGen = new HandControlMessageGenerator ();
        rosBridge = new RosBridgeClient (this.verbose, this.imageStreaming, this.jointStates, this.testLatency, this.pointCloud);

		if (autoConnect) {
			Connect ();
		}

		// leapController.Activate (useLeap);

		if (testLatency) {
			jointStateMsgGen = new JointStateGenerator ();
		//	rosBridge.EnqueRosCommand (new RosAdvertise ("/SModelRobotOutput", "SModel_robot_output"));
		}
        /*
        if (autoConnect)
        {
            if (!rosBridge.IsConnected())
            {
                Connect();
            }
        }
        */
	}

	// starts the connection to the ROSbridge
	private void Connect(){
		if (verbose) {
			Debug.Log ("Try to connect with ROSbridge via websockets.");
		}
		rosBridge.Start ();
	}


	/* 
	 * Here commands to the robot-side can be send
	 */
	void Update () {		
		if (Input.GetKeyDown (KeyCode.C)) {
			rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.closeHand (1.0f)));
		} else if (Input.GetKeyDown (KeyCode.O)) {
			rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.openHand (1.0f)));
		} else if (Input.GetKey (KeyCode.M)) {	//sends a perormance-test ping message	
			JointState jointState = JointStateGenerator.emptyJointState();
			Stamp stamp = new Stamp ();
			DateTime currentTime = DateTime.Now;
			stamp.secs = currentTime.Second;
			stamp.nsecs = currentTime.Millisecond;
			Header header = new Header ();
			header.stamp = stamp;
			header.seq = (ulong)messageSeq++;
			header.frame_id = "Latency Test";
			jointState.header = header;
			rosBridge.EnqueRosCommand (new RosPublish ("/joint_states", jointState));
			//rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.openHand (1.0f)));
		}
		else if (Input.GetKeyDown(KeyCode.W)){
			rosBridge.WriteLatencyDataFile();
		}
	}


	// for efficiency reasons, motion of the robot joints and updates of the streamed video are done with 30 FPS
	void FixedUpdate(){
		// processing is only reasonable if connected to the ROSbridge
		if (rosBridge.IsConnected()) {
			if (imageStreaming) {
				// displaying the streamed images from the openni2 node on a canvas
				canvas.showImage (rosBridge.GetLatestImage ().data);
			} 
			if (jointStates) {
				// synchronizes the virtual robot with the joint state of the real one
				manipulatorControl.UpdateJointStates (rosBridge.GetLatestJoinState ().name, rosBridge.GetLatestJoinState ().position);
			}
			/*
			if (useLeap) {
				// processes data from tracked hands with reduced rate of 10Hz and sends control commands to the real gripper with the same rate
				if (millisSinceLastGripperCommand + 100 > Environment.TickCount) {
					return;
				}
				millisSinceLastGripperCommand = Environment.TickCount;
				//TODO normalizing to a reasonable range
				rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.closeHand (leapController.GetHandClosingState ()))); //direct mapping between PinchStrength and ClosingState of the gripper
			}
			*/
		} else {
			canvas.showTestImage ();
		}
	}


	// Closes the connection to the ROSbridge
	void OnDestroy(){
		rosBridge.Disconnect ();
	}
}
