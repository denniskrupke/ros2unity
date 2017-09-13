using UnityEngine;
using System.Collections;

public class setJointAngle : MonoBehaviour {
	public bool xAxis;
	public bool yAxis;
	public bool zAxis;
	public bool invert = false;
	public double offset = 0.0;

	private Quaternion currentRotation;
	private Quaternion startRotation;

	// Use this for initialization
	void Start () {
		startRotation = transform.localRotation;
		currentRotation = startRotation;
	}

	// Joint angle is updated with 30Hz
	void FixedUpdate(){
		transform.localRotation = currentRotation;
	}
	
	// Update is called once per frame
	void Update () {
	}


	public void SetAngle(double angle){		
		int sign = invert ? -1 : 1;

		if(xAxis){	
			currentRotation = startRotation * Quaternion.Euler ((float)(sign * (angle + offset)), .0f, .0f);
		}
		else if(yAxis){
			currentRotation = startRotation * Quaternion.Euler (.0f, (float)(sign * (angle + offset)), .0f);
		}
		else if(zAxis){
			currentRotation = startRotation * Quaternion.Euler (.0f, .0f, (float)(sign * (angle + offset)));
		}
	}

}