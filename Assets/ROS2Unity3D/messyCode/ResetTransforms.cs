using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResetTransforms : MonoBehaviour {
	private List<Vector3> childPositions = new List<Vector3> ();
	private List<Quaternion> childRotations = new List<Quaternion> ();

	// Use this for initialization
	void Start () {
		foreach (Transform child in transform) {
			childPositions.Add (child.position);
			childRotations.Add (child.rotation);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.C)) {
			Reset ();
		}       
	}

	public void Reset(){
		int count = 0;
		foreach (Transform child in transform) {
			Rigidbody rb = child.GetComponent<Rigidbody> ();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			child.position = childPositions [count];
			child.rotation = childRotations [count];
			count++;
		}
	}
}
