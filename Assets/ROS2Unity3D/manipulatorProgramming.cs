using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ManipulatorProgrammingState{
	Free,
	Translation,
	Rotation
}

public class manipulatorProgramming : MonoBehaviour {
	public processLeapFrames leapController;
	private ManipulatorProgrammingState state;
	private bool manipulationActive;
	GameObject sphere = null;
	GameObject arrow = null;
	private List<GameObject> points;
	private List<GameObject> rotations;
	private int confidence = 0;
	private int confidenceLevel = 30;
	private ManipulatorProgrammingState stateCandidate = ManipulatorProgrammingState.Free;
	// Use this for initialization
	void Start () {
		state = ManipulatorProgrammingState.Free;
		manipulationActive = false;
		points = new List<GameObject>();
		rotations = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if(leapController.GetGrabStrength() > .7f){
			if(stateCandidate == ManipulatorProgrammingState.Rotation){
				if(confidence>=confidenceLevel){
					state = ManipulatorProgrammingState.Rotation;
				}
				else{
					confidence++;
				}
			}
			else{
				stateCandidate = ManipulatorProgrammingState.Rotation;
				confidence = 0;
			}
		}
		else if(leapController.GetPinchStrength() > .7f){
			if(stateCandidate == ManipulatorProgrammingState.Translation){
				if(confidence>=confidenceLevel){
					state = ManipulatorProgrammingState.Translation;
				}
				else{
					confidence++;
				}
			}
			else{
				stateCandidate = ManipulatorProgrammingState.Translation;
				confidence = 0;
			}
		}
		else{
			if(stateCandidate == ManipulatorProgrammingState.Free){
				if(confidence>=confidenceLevel){
					state = ManipulatorProgrammingState.Free;
					manipulationActive = false;
				}
				else{
					confidence++;
				}
			}
			else{
				stateCandidate = ManipulatorProgrammingState.Free;
				confidence = 0;
			}

		}
*/
		switch(state){
		case ManipulatorProgrammingState.Free : {
			} break;
		case ManipulatorProgrammingState.Translation : {				
				if(!manipulationActive){
					manipulationActive = true;
					sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					Vector3 pos = GameObject.FindGameObjectWithTag("RightHandPalm").transform.position;
					sphere.transform.position = pos;
					sphere.transform.localScale = new Vector3(0.05f,0.05f,0.05f);
					points.Add(sphere);
				}
				else{
					sphere.transform.position = GameObject.FindGameObjectWithTag("RightHandPalm").transform.position;
				}

			} break;
		case ManipulatorProgrammingState.Rotation : {
				if(!manipulationActive){
					manipulationActive = true;
					arrow = Instantiate(Resources.Load("arrow", typeof(GameObject))) as GameObject;
					Vector3 pos = GameObject.FindGameObjectWithTag("RightHandPalm").transform.position;
					arrow.transform.position = pos;
					arrow.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
					var rotation = Quaternion.Euler(new Vector3(180,0,0));
					arrow.transform.localRotation *= rotation;
					rotations.Add(arrow);
				}
				else{
					/*
					var targetDir = GameObject.FindGameObjectWithTag("RightHandPalm").transform.position - arrow.transform.position;
					var step = 1.0f*Time.deltaTime;
					var newDir = Vector3.RotateTowards(arrow.transform.forward, targetDir, step, 0.0f);
					arrow.transform.rotation = Quaternion.LookRotation(newDir);
*/
					var rotation = GameObject.FindGameObjectWithTag("RightHandPalm").transform.rotation;
					//rotation = Quaternion.Inverse(rotation);
					arrow.transform.localRotation = rotation;


					/*
					var lookPos = GameObject.FindGameObjectWithTag("RightHandPalm").transform.position - arrow.transform.position;
					lookPos.y = 0;
					var rotation = Quaternion.LookRotation(lookPos);
					arrow.transform.rotation = rotation;//Quaternion.Slerp(arrow.transform.rotation, rotation, Time.deltaTime*2);
					*/
				}
			} break;
		}
		for(int i=0; i<points.Count; i++) {
			if((i+1)<=points.Count){
				Debug.DrawLine(points[i].transform.position, points[i+1].transform.position);
			}
		}
	}
}
