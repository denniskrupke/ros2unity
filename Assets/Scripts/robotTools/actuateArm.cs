using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class actuateArm : MonoBehaviour {
	private Dictionary<string, double> jointStates;
	private setJointAngle[] joints;

    public float shoulder_pan = -.10f;
    public float shoulder_lift = -1.0f;
    public float elbow = -.5f;
    public float wrist_1 = -1.05f;
    public float wrist_2 = .05f;
    public float wrist_3 = -.05f;


    // Use this for initialization
    void Start () {
		joints = gameObject.GetComponentsInChildren<setJointAngle> ();

		jointStates = new Dictionary<string, double> ();		

		jointStates.Add ("shoulder_pan_joint", shoulder_pan);
		jointStates.Add ("shoulder_lift_joint", shoulder_lift);
		jointStates.Add ("elbow_joint", elbow);
		jointStates.Add ("wrist_1_joint", wrist_1);
		jointStates.Add ("wrist_2_joint", wrist_2);
		jointStates.Add ("wrist_3_joint", wrist_3);
	}


	// Update is called once per frame
	void Update () {
		//SetManipulatorJointStates ();
	}

	void FixedUpdate(){
        jointStates["shoulder_pan_joint"] = shoulder_pan;
        jointStates["shoulder_lift_joint"] = shoulder_lift;
        jointStates["elbow_joint"] = elbow;
        jointStates["wrist_1_joint"] = wrist_1;
        jointStates["wrist_2_joint"] = wrist_2;
        jointStates["wrist_3_joint"] = wrist_3;

        SetManipulatorJointStates ();
	}


	void SetManipulatorJointStates(){
		foreach (setJointAngle joint in joints) {
			if(jointStates.ContainsKey(joint.gameObject.name)){

//				if (joint.gameObject.name.Contains ("joint_1")) {
//					joint.SetAngle ((jointStates [joint.gameObject.name] * 180 / Mathf.PI)-30);
//				} else if (joint.gameObject.name.Contains ("joint_2")) {
//					joint.SetAngle ((jointStates [joint.gameObject.name] * 180 / Mathf.PI));
//				} else if (joint.gameObject.name.Contains ("joint_3")){
//					joint.SetAngle ((jointStates [joint.gameObject.name] * 180 / Mathf.PI)-20);
//				} else {
					joint.SetAngle (jointStates [joint.gameObject.name] * 180 / Mathf.PI);
				//Debug.Log (joint.gameObject.name);
//				}
			}
		}
	}


	public void UpdateJointStates(string[] jointNames, double[] values){
		if ((jointNames != null) && (values != null)) {
			for (int i = 0; i < jointNames.Length; i++) {
				jointStates [jointNames [i]] = values [i];
			}
		}
	}
}