using UnityEngine;
using System.Collections;

public class SetGraspTargetPose : MonoBehaviour {
    public GameObject graspTargetPose = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetTargetPose(Vector3 pos, Quaternion rot)
    {
        graspTargetPose.transform.position = pos;// new Vector3(graspTargetPose.transform.localPosition.x+pos.x, graspTargetPose.transform.localPosition.y + pos.y, graspTargetPose.transform.localPosition.z + pos.z);
        graspTargetPose.transform.rotation = rot;
    }
}
