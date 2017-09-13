using UnityEngine;
using System.Collections;

public class SetToPose : MonoBehaviour {
    public Transform referencePose = null;
    public float factor = 0.0f;
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Quaternion rot = referencePose.rotation;
        rot *= Quaternion.Euler(90, 0, 0);
        rot *= Quaternion.Euler(0, -90, 0);
        transform.rotation = rot;

        Vector3 pos = referencePose.position;
        pos += Vector3.forward * factor;                 
        transform.position = pos;

        if (Input.GetKeyDown(KeyCode.UpArrow)){
            factor += .1f;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            factor -= .1f;
        }
	}
}
