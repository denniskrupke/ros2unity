using UnityEngine;
using System.Collections;
//using Leap;
//using Leap.Unity;
using System.IO;
using System.Collections.Generic;

public enum HandRecordingMode{
	None,
	Palm,
	IndexFinger
};

public enum GripperControlMode{
	Pinch,
	Grab
};

public class processLeapFrames : MonoBehaviour
{
	public string positionTrackingFilename = "default.csv";
	public HandRecordingMode recordingMode = HandRecordingMode.Palm;
	public GripperControlMode gripperControlMode = GripperControlMode.Pinch;
	public bool leftHand = false;
	//public LeapHandController leapHandController;
    public TranslateToTarget trajectoryGenerator = null;

    //public LeapProvider leapProvider;

    //private Controller controller;
    //private static Controller handController;
    //private LeapOVREventListener listener;
    private StreamWriter streamWriter;

    //private static Frame currentFrame;
    private List<Vector3> positionData;
	//private List<Vector3> orientationData;
    private List<float> timeStamps;

	private static int fileCount = 0;
	private Vector3 vecPos = new Vector3();
    private Quaternion vecRot = new Quaternion();

	private float grabStrength = 0.0f;
	private float pinchStrength = 0.0f;

	private bool isActivated = true;
    private float smoothedGrabStrength = 0.0f;
    public float smoothingParameter = 0.95f;

    public actuateGripper gripperControl = null;
    public SetGraspTargetPose setGraspTargetPose = null;

    public bool controlGripperModel = false;


    // Use this for initialization
    void Start()
    {
	//	handController = new Controller();//GameObject.FindObjectOfType<Controller>() as Controller;
        //service = new LeapProviderService();
		//controller = Controller();//handController.GetLeapController();        
        //LeapProviderService
        positionData = new List<Vector3>();
		//orientationData = new List<Vector3> ();
        timeStamps = new List<float>();

        //listener = new LeapOVREventListener();
        //controller.addListener(listener);        
        
    }

    // Update is called once per frame
    void Update()
    {
        /* 
        if (Input.GetKeyDown (KeyCode.O)) {
			Debug.Log ("Writing File of length" + timeStamps.Count + "...");
			//WriteFile ();
		} 
		else if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		} 
		else if(this.isActivated){ 
            ProcessFrame(); 
        }
        */
//        ProcessFrame();
    }

	public void Activate(bool activate){
		this.isActivated = activate;
	}

	/*
    void ProcessFrame(){
        //currentFrame = handController.Frame();       
        currentFrame = leapProvider.CurrentFrame;            
        if (currentFrame.Hands.Count>0){
			//Hand hand = leftHand ? currentFrame.Hands.Leftmost : currentFrame.Hands.Rightmost;
			Hand currentHand = null;
			foreach (Hand hand in currentFrame.Hands){
				if ((leftHand && hand.IsLeft) || (!leftHand && hand.IsRight)) {
					this.grabStrength = hand.GrabStrength;
					this.pinchStrength = hand.PinchStrength;
					//((this.smoothedGrabStrength = smoothingParameter*smoothedGrabStrength + ((1.0f-smoothingParameter)*(actuateGripper.Normalize(Mathf.Atan(grabStrength * 2 - 1), -Mathf.PI/2.0f, Mathf.PI/2.0f, 0.0f, 1.0f)));
                    this.smoothedGrabStrength = smoothingParameter * smoothedGrabStrength + (1.0f-smoothingParameter)*(actuateGripper.Normalize(Mathf.Atan(grabStrength * 2*4 - 4), -Mathf.PI / 2.0f, Mathf.PI / 2.0f, 0.0f, 1.0f));
                }
                
			}
//			currentHand = leftHand ? currentFrame.Hands.Leftmost : currentFrame.Hands.Rightmost;

            
			//Debug.Log ("GrabStrength = " + hand.GrabStrength + " | PinchStrength = " + hand.PinchStrength);
			switch(this.recordingMode){
			case HandRecordingMode.None: 
				return;
			case HandRecordingMode.IndexFinger :
  //                  vecPos = currentHand.Finger(1).TipPosition.ToUnityScaled();				
				break;
			case HandRecordingMode.Palm:
                    {
                        vecPos = currentHand.PalmPosition.ToVector3();// ToUnityScaled();
  //                      vecRot = currentHand.Basis.Rotation();
                    }break;                    
			}
            
            /*
            if (smoothedGrabStrength > .9)
            {                               
                //setGraspTargetPose.SetTargetPose(vecPos, vecRot*Quaternion.Euler(110,0,0)); // imidiate movements of ghost arm               
                trajectoryGenerator.SetTargetPose(vecPos, vecRot*Quaternion.Euler(110, 0, 0));// sets target for ghost arm
            }
            */

     /*
            if (controlGripperModel)
            {
                gripperControl.MoveGripper(smoothedGrabStrength);
            }
            
        
	        //positionData.Add(vecPos);
	        //timeStamps.Add(Time.time);
		}
    }


    void WritePositionToFile(Vector vec, float timestamp){
        string dataline = "";
        dataline += timestamp;
        dataline += ",";
        dataline += vec.x;
        dataline += ",";
        dataline += vec.y;
        dataline += ",";
        dataline += vec.z;
        streamWriter.WriteLine(dataline);
    }


    void WritePositionToFile(Vector3 vec, float timestamp){
        string dataline = "";
        dataline += timestamp;
        dataline += ",";
        dataline += vec.x;
        dataline += ",";
        dataline += vec.y;
        dataline += ",";
        dataline += vec.z;
        streamWriter.WriteLine(dataline);
    }
	

    void WriteFile(){
		//streamWriter = new StreamWriter(positionTrackingFilename);
		string header = "time,x,y,z,roll,pitch,yaw";
		streamWriter.WriteLine(header);
        for(int i = 0; i < positionData.Count-1; i++)
        {
            string dataline = "";
            dataline += timeStamps[i];
            dataline += ",";
            dataline += positionData[i].x;
            dataline += ",";
            dataline += positionData[i].y;
            dataline += ",";
            dataline += positionData[i].z;
            streamWriter.WriteLine(dataline);
        }
		streamWriter.Close ();
    }


	public void WriteSingleTrajectory(){
		string[] filename = positionTrackingFilename.Split ('.');
		string name = filename [0] + processLeapFrames.fileCount + "." + filename [1];
		streamWriter = new StreamWriter(name);
		//WriteFile ();
		timeStamps.Clear ();
		positionData.Clear ();
		processLeapFrames.fileCount++;
	}


    public static void SetCurrentFrame(Frame frame){
        currentFrame = frame;
    }


	public float GetHandClosingState(){		
		float state = 0.0f;
		switch (gripperControlMode) {
			case GripperControlMode.Grab: state = GetGrabStrength ();
			break;
			case GripperControlMode.Pinch: state = GetPinchStrength ();	
			break;
		}
		return state;
	}

	// TODO calculate scaled grab strength and scaled ppinch strength
	public float GetGrabStrength(){
		return this.grabStrength;
	}

	public float GetPinchStrength(){
		return this.pinchStrength;
	}

	public Vector3 GetPalmPosition(){
		return vecPos;
	}

	public Vector3 GetPalmPositionInWorldCoordinates(){
		return leapHandController.transform.TransformPoint(vecPos);
	}
}
	
/*
     class LeapOVREventListener : public Leap:Listener
    {

        private Frame currentFrame;
        public override void OnFrame(Controller controller)
        {
            currentFrame = controller.Frame();
            if(currentFrame.IsValid)
            {
                processLeapFrames.SetCurrentFrame(currentFrame);
            }
        }

        public override void OnInit(Controller controller)
        {
            //Console.WriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            //Console.WriteLine("Connected");
            //If using gestures, enable them:
            //controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
        }

        //Not dispatched when running in debugger
        public override void OnDisconnect(Controller controller)
        {
            //Console.WriteLine("Disconnected");
        }
        */
		
}
