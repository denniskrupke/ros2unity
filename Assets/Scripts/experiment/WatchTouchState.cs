using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WatchTouchState : MonoBehaviour {
	//public List<GameObject> registeredObjects;

	public bool touched = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	void FixedUpdate(){
		CheckCollision[] checkCollisions = GetComponentsInChildren<CheckCollision> ();
		//Debug.Log ("found " + checkCollisions.Length);
		foreach (CheckCollision cc in checkCollisions) {
			if (cc.GetCollisionState ()) {
				//Debug.Log("touched something");
				touched = true;
				return;
			}
		}
	}

	public bool IsSomethingTouched(){
		return touched;
	}

	public void ResetTouchedState(){
		CheckCollision[] checkCollisions = GetComponentsInChildren<CheckCollision> ();
		foreach (CheckCollision cc in checkCollisions) {
			cc.Reset ();
		}
	}
}
