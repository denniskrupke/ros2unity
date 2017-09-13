using UnityEngine;
using System.Collections;
using UnityEngine.UI;	

public class ControlSphereBehavior : MonoBehaviour {

	private MeshRenderer objectRenderer;
	private Vector3 initialPosition;
	private processLeapFrames leapProcessor;
	
	private bool toggleColor = false;
	private bool handIsInControlSphere = false;

	private const string NO_CONTROL= "STATUS: No Control";
	private const string CONTROL = "STATUS: Control";

	public bool movable = true;

	public float opacity = 0.4f;
	public Image statusPanel;
	public Text statusText;



	// Use this for initialization
	void Start () {
		this.objectRenderer = gameObject.GetComponent<MeshRenderer>();
		this.initialPosition = gameObject.transform.position;
		this.leapProcessor = gameObject.GetComponent<processLeapFrames>();
		this.objectRenderer.material.color = ChangeAlpha( Color.red,opacity ); 

		statusPanel.color = ChangeAlpha (Color.red, 0.2f);
		this.statusText.color = ChangeAlpha (Color.red, 0.8f);
		this.statusText.text = NO_CONTROL;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			if(toggleColor) {			
				this.objectRenderer.material.color = ChangeAlpha( Color.red,opacity ); 
				toggleColor = false;
			}
			else{
				this.objectRenderer.material.color = ChangeAlpha( Color.green,opacity);
				toggleColor = true;
			}
		}
		//Debug.Log("positionSphere: "+gameObject.transform.position + " positionPalm: "+leapProcessor.GetPalmPosition());
		if (handIsInControlSphere) {
			/*
			Vector3 translation = new Vector3(-1*(gameObject.transform.position.x - leapProcessor.GetPalmPosition().x),
			                                -1*(gameObject.transform.position.y - leapProcessor.GetPalmPosition().y),
			    							-1*(gameObject.transform.position.z - leapProcessor.GetPalmPosition().z));
			Debug.Log("translate: "+translation);
			gameObject.transform.Translate (translation);
			*/
			//Vector3 vec = leapProcessor.GetPalmPosition();
			//vec.y = vec.y + 1.7f;
			//gameObject.transform.position = vec;
			//gameObject.transform.SetParent(leapProcessor.GetHand().PalmPosition.)

			/*
			Vector3 position = movable ? new Vector3(initialPosition.x //left and right
			                                          + leapProcessor.GetPalmPosition().x - 0.15f
			                                         ,
			                                         initialPosition.y //up and down
			                                          - leapProcessor.GetPalmPosition().y + 0.2f
			                                         ,
			                                         initialPosition.z //back and forth
			                                          - leapProcessor.GetPalmPosition().z //- 0.01f
			                                         )
										: new Vector3(initialPosition.x,
				              							initialPosition.y,
				              							initialPosition.z);

			gameObject.transform.position = position;//controller.transform.TransformPoint(leapProcessor.GetPalmPosition());	
			*/

			/*
			if(movable) {
				Vector3 position = new Vector3(initialPosition.x //left and right
			                                                     + leapProcessor.GetPalmPosition().x - 0.15f
			                                                     ,
			                                                     initialPosition.y //up and down
			                                                     - leapProcessor.GetPalmPosition().y + 0.2f
			                                                     ,
			                                                     initialPosition.z //back and forth
			                                                     - leapProcessor.GetPalmPosition().z //- 0.01f
				                                         );
				gameObject.transform.position = position;//controller.transform.TransformPoint(leapProcessor.GetPalmPosition());		
			}
			*/
		}
	}
	
	void OnTriggerEnter(Collider other) {
		this.objectRenderer.material.color = ChangeAlpha(Color.green,opacity);
		statusPanel.color = ChangeAlpha (Color.green, 0.2f);
		this.statusText.color = ChangeAlpha (Color.green, 0.8f);
		this.statusText.text = CONTROL;

		handIsInControlSphere = true;
		//if (this.leapProcessor != null) {

			/*
			GameObject handModel = GameObject.Find("pepper_arm_left");//(Clone)
			if(handModel!=null) gameObject.transform.SetParent(handModel.transform);
			else Debug.Log("PepperMediumFullLeftHand is null");
			*/

			//this.leapProcessor.SetState (ProcessLeapFrames.SPHERE);
			//this.objectRenderer.material.color = Color.green;

			/*
			StartCoroutine(BlockWait(1.5f));
			if (this.writeFile) {
				this.leapProcessor.WriteSingleTrajectory (gameObject.transform.localScale.x, gameObject.transform.position);
				this.writeFile = false;
			}
			*/
		//}
		//else Debug.Log("leapProcessor is null!!!");
	}


	/*
	IEnumerator BlockWait(float seconds){
		yield return new WaitForSeconds(seconds);
	}
	*/
	

	void OnTriggerExit(Collider other) {
		this.objectRenderer.material.color = ChangeAlpha( Color.red,opacity ); 
		statusPanel.color = ChangeAlpha (Color.red, 0.2f);
		this.statusText.color = ChangeAlpha (Color.red, 0.8f);
		this.statusText.text = NO_CONTROL;

		handIsInControlSphere = false;
	}

	/*

	void OnCollisionEnter(Collision collision){
		this.objectRenderer.material.color = Color.green;
		Debug.Log ("Enter");
	}

	void OnCollisionExit(Collision collision){
		this.objectRenderer.material.color = Color.red;
		Debug.Log ("Exit");
	}
*/

	public static Color ChangeAlpha(Color oldColor, float alpha){
		Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);          
		return newColor;             
	}
}
