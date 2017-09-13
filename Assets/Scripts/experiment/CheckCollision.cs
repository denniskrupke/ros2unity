using UnityEngine;
using System.Collections;

public class CheckCollision : MonoBehaviour {

	public bool collisionState = false;
	private Color originalColor;
	// Use this for initialization
	void Start () {
		originalColor = gameObject.GetComponent<Renderer> ().material.color;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		collisionState = true;	
		Color color = gameObject.GetComponent<Renderer> ().material.color;
		color.a = 0.6f;
		gameObject.GetComponent<Renderer> ().material.color = color;
	}

	void OnTriggerExit(Collider other){
		//collisionState = false;
	}

	void OnTriggerStay(Collider other){
		collisionState = true;
	}

	public bool GetCollisionState(){
		return collisionState;
	}

	public void Reset(){
		gameObject.GetComponent<Renderer> ().material.color = originalColor;
	}


}
