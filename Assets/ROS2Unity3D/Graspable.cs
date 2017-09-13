using UnityEngine;
using System.Collections;

public enum GraspControlMode
{
    GripperControlled,
    HandControlled
}

public class Graspable : MonoBehaviour {
    

	public processLeapFrames leapProcessor;	
	public Transform gripperTCP;
    public actuateGripper gripperController;
	public float distThreshold = 0.01f;
	public float grabThreshold = 0.5f;
    public float gripperThreshold = 0.7f;
    public GraspControlMode graspMode = GraspControlMode.GripperControlled;
       

    private int notGrabCount = 0;
	private int notGrabThreshold = 15;

	private bool gripperClosed = false;
	private Vector3 updatePosition = new Vector3 ();
	private Quaternion updateRotation = new Quaternion ();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (gripperClosed) {
			if (Vector3.Distance (transform.position, gripperTCP.position) < distThreshold) {
				updatePosition.x = gripperTCP.position.x;
				updatePosition.y = gripperTCP.position.y;
				updatePosition.z = gripperTCP.position.z;
				
				updateRotation.x = gripperTCP.rotation.x;
				updateRotation.y = gripperTCP.rotation.y;
				updateRotation.z = gripperTCP.rotation.z;
				updateRotation.w = gripperTCP.rotation.w;

				transform.position = updatePosition;
				transform.rotation = updateRotation;                
			}
		}
	}

	void FixedUpdate(){
		setGripperState ();
	}

	private bool isHandClosed(){
		return true;//(leapProcessor.GetGrabStrength () > grabThreshold) ? true : false;
	}

    private bool isGripperClosed()
    {
        return (gripperController.GetCurrentClosingValue() > gripperThreshold) ? true : false;
    }


	private void setGripperState(){
        switch (this.graspMode)
        {
            case GraspControlMode.HandControlled :
                {
                    if (!isHandClosed())
                    {
                        notGrabCount++;
                    }
                    else
                    {
                        notGrabCount = 0;
                        gripperClosed = true;
                    }

                    if (notGrabCount > notGrabThreshold)
                    {
                        gripperClosed = false;
                    }
                } break;
            case GraspControlMode.GripperControlled:
                {
                    if (!isGripperClosed())
                    {
                        notGrabCount++;
                    }
                    else
                    {
                        notGrabCount = 0;
                        gripperClosed = true;
                    }

                    if (notGrabCount > notGrabThreshold)
                    {
                        gripperClosed = false;
                    }
                } break;
        }
		
	}
}
