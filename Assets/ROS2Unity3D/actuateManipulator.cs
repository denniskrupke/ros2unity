using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class actuateManipulator : MonoBehaviour {
	private Dictionary<string, double> jointStates;
	private setJointAngle[] joints;


	// Use this for initialization
	void Start () {
		joints = gameObject.GetComponentsInChildren<setJointAngle> ();

		jointStates = new Dictionary<string, double> ();
		jointStates.Add ("finger_1_joint_1", 0.13065036114928982f);
		jointStates.Add ("finger_1_joint_2", 0.0f);
		jointStates.Add ("finger_1_joint_3", 0.037183101455531366f);

		jointStates.Add ("finger_2_joint_1", 0.13065036114928982f);
		jointStates.Add ("finger_2_joint_2", 0.0f);
		jointStates.Add ("finger_2_joint_3", 0.037183101455531366f);

		jointStates.Add ("finger_middle_joint_1", 0.13065036114928982f);
		jointStates.Add ("finger_middle_joint_2", 0.0f);
		jointStates.Add ("finger_middle_joint_3", 0.037183101455531366f);

		jointStates.Add ("palm_finger_1_joint", 0);//-0.01648813348658237f*180/Mathf.PI);
		jointStates.Add ("palm_finger_2_joint", 0);//0.01648813348658237f*180/Mathf.PI);
        /*
        jointStates.Add("s_model_finger_1_joint_1", 0.13065036114928982f);
        jointStates.Add("s_model_finger_1_joint_2", 0.0f);
        jointStates.Add("s_model_finger_1_joint_3", 0.037183101455531366f);

        jointStates.Add("s_model_finger_2_joint_1", 0.13065036114928982f);
        jointStates.Add("s_model_finger_2_joint_2", 0.0f);
        jointStates.Add("s_model_finger_2_joint_3", 0.037183101455531366f);

        jointStates.Add("s_model_finger_middle_joint_1", 0.13065036114928982f);
        jointStates.Add("s_model_finger_middle_joint_2", 0.0f);
        jointStates.Add("s_model_finger_middle_joint_3", 0.037183101455531366f);

        jointStates.Add("s_model_palm_finger_1_joint", 0);//-0.01648813348658237f*180/Mathf.PI);
        jointStates.Add("s_model_palm_finger_2_joint", 0);//0.01648813348658237f*180/Mathf.PI);
        */
        jointStates.Add ("shoulder_pan_joint", -.10f);
		jointStates.Add ("shoulder_lift_joint", -1.0f);
		jointStates.Add ("elbow_joint", -.05f);
		jointStates.Add ("wrist_1_joint", -1.05f);
		jointStates.Add ("wrist_2_joint", .05f);
		jointStates.Add ("wrist_3_joint", -.05f);
	}


	// Update is called once per frame
	void Update () {
		//SetManipulatorJointStates ();
	}

	void FixedUpdate(){
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