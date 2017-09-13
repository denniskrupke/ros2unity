using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class actuateGripper : MonoBehaviour {
	private Dictionary<string, float> jointStates;
	private setJointAngle[] joints;

    private float currentClosingValue = 1.0f;

    private string[] names = new string[11];


	// Use this for initialization
	void Start () {
		joints = gameObject.GetComponentsInChildren<setJointAngle> ();

		jointStates = new Dictionary<string, float> ();
		jointStates.Add ("finger_1_joint_1", 0.13065036114928982f*180/Mathf.PI);
		jointStates.Add ("finger_1_joint_2", 0.0f*180/Mathf.PI);
		jointStates.Add ("finger_1_joint_3", 0.037183101455531366f*180/Mathf.PI);

		jointStates.Add ("finger_2_joint_1", 0.13065036114928982f*180/Mathf.PI);
		jointStates.Add ("finger_2_joint_2", 0.0f*180/Mathf.PI);
		jointStates.Add ("finger_2_joint_3", 0.037183101455531366f*180/Mathf.PI);

		jointStates.Add ("finger_middle_joint_1", 0.13065036114928982f*180/Mathf.PI);
		jointStates.Add ("finger_middle_joint_2", 0.0f*180/Mathf.PI);
		jointStates.Add ("finger_middle_joint_3", 0.037183101455531366f*180/Mathf.PI);

		jointStates.Add ("palm_finger_1_joint", 0);//-0.01648813348658237f*180/Mathf.PI);
		jointStates.Add ("palm_finger_2_joint", 0);//0.01648813348658237f*180/Mathf.PI);

        names[0] = "finger_1_joint_1";
        names[1] = "finger_1_joint_2";
        names[2] = "finger_1_joint_3";

        names[3] = "finger_2_joint_1";
        names[4] = "finger_2_joint_2";
        names[5] = "finger_2_joint_3";

        names[6] = "finger_middle_joint_1";
        names[7] = "finger_middle_joint_2";
        names[8] = "finger_middle_joint_3";

        names[9] = "palm_finger_1_joint";
        names[10] = "palm_finger_2_joint";

        //Debug.Log("name "+names[0]);
	}


	// Update is called once per frame
	void Update () {
		SetGripperJointStates ();
	}

    void FixedUpdate(){
        SetGripperJointStates();
    }


	void SetGripperJointStates(){
		foreach (setJointAngle joint in joints) {
			if(jointStates.ContainsKey(joint.gameObject.name)){
				if (joint.gameObject.name.Contains ("joint_1")) {
					joint.SetAngle ((jointStates [joint.gameObject.name] * 180 / Mathf.PI)-30);
				} else if (joint.gameObject.name.Contains ("joint_2")) {
					joint.SetAngle ((jointStates [joint.gameObject.name] * 180 / Mathf.PI));
				} else if (joint.gameObject.name.Contains ("joint_3")){
					joint.SetAngle ((jointStates [joint.gameObject.name] * 180 / Mathf.PI)-20);
				} else {
					joint.SetAngle (jointStates [joint.gameObject.name] * 180 / Mathf.PI);
				}
			}
		}
	}


	public void UpdateJointStates(string[] jointNames, float[] values){
		if ((jointNames != null) && (values != null)) {
			for (int i = 0; i < jointNames.Length; i++) {
				jointStates [jointNames [i]] = values [i];
			}
		}
	}

    public void SetSimpleState(float closed) {

    }

    public static float Normalize(float value, float oldRangeMin, float oldRangeMax, float newRangeMin, float newRangeMax){
        return (value - oldRangeMin) * ((newRangeMax - newRangeMin) / (oldRangeMax - oldRangeMin)) + (newRangeMin);
    }


    // calculates an approximation of a single joint state of the gripper, assuming that no collison during the grasp occured (-> no object is grasped / empty hand model)
    private float calculateAngle(int joint, int fingerPosition){
        float angle = 0.0f;
        float finger_joint0_offset = 30.0f;
        float finger_joint2_offset = -30.0f;

        if (joint==0){
            if (fingerPosition>140){
                angle = 35.0f+finger_joint0_offset;
            }
            else{
                angle = Normalize(fingerPosition, 0.0f, 140.0f, -25.0f+finger_joint0_offset, 33.0f+finger_joint0_offset);
            }
        }

        else if (joint==1){
            if (fingerPosition<140){
                angle = 0.0f;
            }
            else if(fingerPosition>240){
                angle = 90.0f;
            }
            else{
                angle = Normalize(fingerPosition, 140.0f, 240.0f, 0.0f, 90.0f);
            }
        }

        else if(joint==2){
            if (fingerPosition>115){
                angle = -20.0f+finger_joint2_offset;
            }
            else{
                angle = Normalize(fingerPosition, 0.0f, 115.0f, 35.0f+finger_joint2_offset, -20.0f+finger_joint2_offset);
            }
        }

        return (angle / 180.0f) * (float)Math.PI;
    }



    // between 0 and 1; 0 means fully open and 1 corresponds to fully closed
    public void MoveGripper(float closingValue)
    {
        this.currentClosingValue = closingValue;
        float value = Normalize(closingValue, 0.0f, 1.0f, 0.0f, 255.0f);
        //Debug.Log("closingValue " + closingValue);

        for (int finger = 0; finger < 3; finger++) {
            for (int joint = 0; joint < 3; joint++) {
                jointStates[names[finger * 3 + joint]] = calculateAngle(joint, (int)value);
                //jointStates["finger_1_joint_1"] = calculateAngle(joint, (int)value);

                //float val = calculateAngle(joint, (int)value);
                //string n = names[0];
                //Debug.Log("name "+n);
                //Debug.Log(jointStates.Keys);
                //jointStates[n] = val;
            }
        }
    }

    public float GetCurrentClosingValue()
    {
        return currentClosingValue;
    }
}
