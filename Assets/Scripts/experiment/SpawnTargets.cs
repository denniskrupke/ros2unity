using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class Case {
    public int distance;
    public int size;

    public Case(int dist, int size){
        this.distance = dist;
        this.size = size;
    }
}

public class SpawnTargets : MonoBehaviour {
	public enum ExperimentType{
		SIMPLE,
		CUTE,
		DEXTERITY,
        IMPACT
	}

    [Header("Participant properties")]
	public int participantId = 0;
    public bool leftHanded = false;
	public List<ExperimentType> experimentTypes;

    [Space(10)]
	[Header("Target properties")]
	public int minDistanceInMillimeters = 10*10;
	public int maxDistanceInMillimeters = 60*10;
    public int stepDistance = 5;
    [Space(5)]
    public int numOfSteps = 2;
	public int stepDegree = 15;
    [Space(5)]
	public int minDiameterInMillimeters = 10;
	public int maxDiameterInMillimeters = 100;
    public int stepDiameter = 5;
    [Space(5)]
    public int numOfRepetitions = 1; // one means only once

	[Space(10)]
	[Header("Special Prefab Mode")]
	//public GameObject cat;
	private ExperimentType experimentType = ExperimentType.SIMPLE;
    public List<GameObject> pointingDevices;
	//public bool objectMode = false;

	[Space(10)]
	[Header("Input Setup")]
	public Transform viveController = null;
	public GameObject restingBase;
	public int restingTimeInMs = 1500;
	public DataWriter dataWriter = null;
	public TrackObject trackObject = null;

	private int countResting = 0;
	private float restingDistanceThreshold = 0.03f;

	private List<GameObject> targets;
	//private List<int> alreadyShownTargets;
	private float currentSize;
	private float currentDistance;
	private Transform currentTargetTransform = null;
	public GameObject targetObjects = null;
    
	private static int targetId = 0;

	private bool isValid = true;
	private bool isCalibrated = false;
	private float shoulderHeight = 0.0f;
    private float shoulderWidth = 0.0f;

	private int controllerId = 1;

	private float eggResizeFactor = 0.5f;
	private float catResizeFactor = 7.5f; //4
    private Transform transformBackupOfCurrentTarget = null;
    private GameObject targetClone = null;

    private List<Case> cases = null;
	private bool initial = true;

	private int currentCase = 0;
	private int currentTarget = 0;
	private int currentType = 0;

	// Use this for initialization
	void Start () {	
		//experimentTypes.RemoveAt(0);
		experimentType = experimentTypes[currentType];	
        InitType(experimentType);
		CreateCases();
		CreateTargets();
		ShowAllTargets(false);
	}

    private void InitType(ExperimentType type) {
        experimentType = type;
        targets = new List<GameObject>();
        //alreadyShownTargets = new List<int>();
        cases = new List<Case>();

		currentCase = 0;
		currentTarget = 0;

        switch (type) {
        	case ExperimentType.SIMPLE: {
                pointingDevices[0].SetActive(true);
                pointingDevices[1].SetActive(false);                
				viveController.gameObject.GetComponent<SphereCollider> ().isTrigger = false;
				if(viveController.GetComponent<Rigidbody> () != null) Destroy(viveController.GetComponent<Rigidbody> ());
            } break;
            case ExperimentType.CUTE: {
                pointingDevices[0].SetActive(false);
                pointingDevices[1].SetActive(true);                                             
				viveController.gameObject.GetComponent<SphereCollider> ().isTrigger = false;
				if(viveController.GetComponent<Rigidbody> () != null) Destroy(viveController.GetComponent<Rigidbody> ());
            } break;
			case ExperimentType.DEXTERITY: {
				pointingDevices[0].SetActive(true);
				pointingDevices[1].SetActive(false);
				viveController.gameObject.GetComponent<SphereCollider> ().isTrigger = true;
				viveController.gameObject.AddComponent<Rigidbody> ();
				viveController.gameObject.GetComponent<Rigidbody> ().useGravity = false;
			} break;
            case ExperimentType.IMPACT: {
				pointingDevices[0].SetActive(true);               
                pointingDevices[1].SetActive(false);
				viveController.gameObject.GetComponent<SphereCollider> ().isTrigger = false;
				if(viveController.GetComponent<Rigidbody> () != null) Destroy(viveController.GetComponent<Rigidbody> ());
            } break;
        }			        
    }

	private void CreateCases(){
		for(int size = this.minDiameterInMillimeters; size <= this.maxDiameterInMillimeters; size += this.stepDiameter){
			for(int distance = this.minDistanceInMillimeters; distance <= this.maxDistanceInMillimeters; distance += this.stepDistance){
				for(int count=0; count<this.numOfRepetitions; count++){
					this.cases.Add(new Case(distance, size));
				}
			}
		}
		ExtensionMethods.Shuffle (cases);
		Debug.Log ("created "+cases.Count+" cases");
	}

	private void CreateTargets(){
		//int nextCaseIndex = Random.Range(0,cases.Count-1);            
		currentSize = 1.0f; //currentSize = (float)(Random.Range(minDiameterInMillimeters, maxDiameterInMillimeters)) / 1000;
		currentDistance = 1.0f;  //currentDistance = (float)(Random.Range(minDistanceInMillimeters, maxDistanceInMillimeters)) / 1000;		       

		Debug.Log("preparing prefab");
		GameObject targetPrefab = null;
		switch (experimentType) {
		case ExperimentType.SIMPLE: {
				targetPrefab = (GameObject) GameObject.Instantiate(Resources.Load("TargetSphere", typeof(GameObject) ));
			} break;
		case ExperimentType.IMPACT: {
				targetPrefab = (GameObject)GameObject.Instantiate(Resources.Load("DestroyableEgg", typeof(GameObject)));   
				//targetPrefab.transform.localScale = new Vector3(currentSize * eggResizeFactor, currentSize * eggResizeFactor, currentSize * eggResizeFactor);
			} break;
		case ExperimentType.CUTE: {
				//targetPrefab = (GameObject)GameObject.Instantiate(Resources.Load("KittenNPC", typeof(GameObject)));
				targetPrefab = (GameObject)GameObject.Instantiate(Resources.Load("KittenCentered", typeof(GameObject)));
				targetPrefab.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
				Vector3 forward = targetPrefab.transform.forward;
				forward.x *= -1;
				targetPrefab.transform.forward = forward;
				//targetPrefab.transform.position = new Vector3(targetPrefab.transform.position.x, targetPrefab.transform.position.y - .05f, targetPrefab.transform.position.z);			
			} break;
		case ExperimentType.DEXTERITY: {
				targetPrefab = (GameObject)GameObject.Instantiate(Resources.Load("DexteritySphere", typeof(GameObject)));
				targetPrefab.transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
			} break;
		}
		targetPrefab.SetActive(false);

		Debug.Log("loop0");
		for (int i = (-1 * numOfSteps); i < numOfSteps + 1; i++) {	
			GameObject target = Instantiate(targetPrefab);
			target.transform.position = new Vector3(currentDistance, 0, 0) + transform.position;
			transform.rotation = Quaternion.Euler (new Vector3 (0,0,i*stepDegree));
			target.transform.parent = transform;
			switch (experimentType) {
			case ExperimentType.CUTE:
				target.transform.localScale = new Vector3 (catResizeFactor * currentSize, catResizeFactor * currentSize, catResizeFactor * currentSize); break;
			case ExperimentType.IMPACT:
				target.transform.localScale = new Vector3 (eggResizeFactor * currentSize, eggResizeFactor * currentSize, eggResizeFactor * currentSize); break;
			}
			targets.Add(target);
		}
		Debug.Log("loop1");
		for (int i = (-1 * numOfSteps); i < numOfSteps + 1; i++) {
			if (i == 0) {
				continue;
			}
			GameObject target = Instantiate(targetPrefab);
			target.transform.position = new Vector3(currentDistance, 0, 0) + transform.position;
			transform.rotation = Quaternion.Euler (new Vector3 (0,i*stepDegree,0));
			target.transform.parent = transform;
			switch (experimentType) {
			case ExperimentType.CUTE:
				target.transform.localScale = new Vector3 (catResizeFactor * currentSize, catResizeFactor * currentSize, catResizeFactor * currentSize); break;
			case ExperimentType.IMPACT:
				target.transform.localScale = new Vector3 (eggResizeFactor * currentSize, eggResizeFactor * currentSize, eggResizeFactor * currentSize); break;
			}
			targets.Add(target);
		}
		Debug.Log("loop2");
		for (int i = (-1 * numOfSteps); i < numOfSteps + 1; i++) {
			if (i == 0) {
				continue;
			}
			GameObject target = Instantiate(targetPrefab);
			target.transform.position = new Vector3(currentDistance, 0, 0) + transform.position;
			transform.rotation = Quaternion.Euler (new Vector3 (45,i*stepDegree,0));
			target.transform.parent = transform;
			switch (experimentType) {
			case ExperimentType.CUTE:
				target.transform.localScale = new Vector3 (catResizeFactor * currentSize, catResizeFactor * currentSize, catResizeFactor * currentSize); break;
			case ExperimentType.IMPACT:
				target.transform.localScale = new Vector3 (eggResizeFactor * currentSize, eggResizeFactor * currentSize, eggResizeFactor * currentSize); break;
			}
			targets.Add(target);
		}
		Debug.Log("loop3");
		for (int i = (-1 * numOfSteps); i < numOfSteps + 1; i++) {
			if (i == 0) {
				continue;
			}
			GameObject target = Instantiate(targetPrefab);
			target.transform.position = new Vector3(currentDistance, 0, 0) + transform.position;
			transform.rotation = Quaternion.Euler (new Vector3 (-45,-i*stepDegree,0));
			target.transform.parent = transform;
			switch (experimentType) {
			case ExperimentType.CUTE:
				target.transform.localScale = new Vector3 (catResizeFactor * currentSize, catResizeFactor * currentSize, catResizeFactor * currentSize); break;
			case ExperimentType.IMPACT:
				target.transform.localScale = new Vector3 (eggResizeFactor * currentSize, eggResizeFactor * currentSize, eggResizeFactor * currentSize); break;
			}
			targets.Add(target);
		}
		transform.rotation = Quaternion.Euler (new Vector3 (0,0,0));
		ExtensionMethods.Shuffle (targets);
		Debug.Log("Creating of "+ targets.Count +" targets done");
	}
		
	private void ShowAllTargets(bool show){
		Debug.Log ("hiding all tragets");
		foreach (GameObject target in targets) {
			if (target != null) {
				target.SetActive(show);
			}
		}   
	}

	// Update is called once per frame
	void Update () {		
		
	}


	void FixedUpdate(){
        if (restingBase.activeSelf) {
            if ((countResting * 1000 / 30) > restingTimeInMs) {
                countResting = 0;
                ShowRestingPosition(false);
				ShowNextTarget();
            }
            CheckRestingPosition();
        }
	}


    private void ShowRestingPosition(bool show){        
        restingBase.SetActive(show);
    }


	public void CheckValidityOfTrial() {
		// TODO
		switch (experimentType) {
			case ExperimentType.SIMPLE: {
					
			} break;
			case ExperimentType.CUTE: {
					
			} break;
			case ExperimentType.DEXTERITY: {
				isValid = currentTargetTransform.gameObject.GetComponentInChildren<WatchTouchState> ().IsSomethingTouched ();
				currentTargetTransform.gameObject.GetComponentInChildren<WatchTouchState> ().ResetTouchedState ();

				// TODO check if rule is violated
	//				if (ruleBreak) {
	//					isValid = false;
	//				}
			} break;
			case ExperimentType.IMPACT: {
				// check if object had to be cloned/replaced/isExploded	
			} break;
		}
	}


	private void CheckRestingPosition(){
		if (Vector3.Distance (viveController.position, restingBase.transform.position) < restingDistanceThreshold) {
			countResting++;
		} else {
			countResting = 0;
		}
	}


	private void ModifyTargets(int distance, int steps, int stepsize, int diameter){
		Debug.Log ("ModifyTargets");
		currentSize = (float)diameter / 1000;
		currentDistance = (float)distance/1000;
		foreach (GameObject target in targets){	
			switch (experimentType) {
			case ExperimentType.CUTE: 
				{
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (0, 0, -target.transform.position.z));
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (0, 0, -currentDistance));
					target.transform.localScale = new Vector3 (currentSize * catResizeFactor, currentSize * catResizeFactor, currentSize * catResizeFactor);
				}
				break;
			case ExperimentType.DEXTERITY:
				{
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (0,0,target.transform.position.x));
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (0,0,-currentDistance));
					target.transform.localScale = new Vector3 (currentSize, currentSize, currentSize);
				}
				break;
			case ExperimentType.IMPACT:
				{
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (-target.transform.position.x, 0, 0));
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (currentDistance, 0, 0));
					target.transform.localScale =  new Vector3 (currentSize * eggResizeFactor, currentSize * eggResizeFactor, currentSize * eggResizeFactor);
				}			
				break;
			default:
				{					
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (-target.transform.position.x, 0, 0));
					target.transform.localPosition = target.transform.TransformDirection (new Vector3 (currentDistance, 0, 0));
					target.transform.localScale = new Vector3 (currentSize, currentSize, currentSize);
				}
				break;			
			}
		}
		//targets[currentTarget].SetActive(true);
	}


    private GameObject CloneCurrentTarget(int currentTarget) {
		Debug.Log ("CloneCurrentTarget");
        GameObject targetClone = Instantiate(targets[currentTarget]);
        targetClone.transform.position = targets[currentTarget].transform.position;
        targetClone.transform.localScale = targets[currentTarget].transform.localScale;
        targetClone.transform.rotation = targets[currentTarget].transform.rotation;
        targetClone.transform.localRotation = targets[currentTarget].transform.localRotation;
        targetClone.transform.parent = targets[currentTarget].transform.parent;
        this.targetClone = targetClone;

        return targetClone;
    }


	private bool AdjustExperiment(){
		bool cont = true;

		if ((currentTarget < targets.Count)) {	// targets left	
			//Debug.Log("show target "+currentTarget)
		} 
		else {	// no targets left
			if (currentCase < cases.Count-1) {// cases left
				currentCase++;
				currentTarget = 0;			
			} 
			else {	// no cases left
				if (currentType < experimentTypes.Count) {	// experiment types left
					experimentType = experimentTypes [++currentType];
					InitType (experimentType);
					CreateCases ();
					CreateTargets ();
					ShowAllTargets (false);
					ShowNextTarget ();
				} 
				else {		// done			
					cont = false;
				}
			}
		}
		Debug.Log ("currentTarget " + currentTarget + " currentCase " + currentCase + " experimentType " + experimentType);

		return cont;
	}
		

	private void ModifyTargetsIfNeccessary(){
		Debug.Log ("ModifyTargetsIfNeccessary");
		if (AdjustExperiment ()) {
			ModifyTargets (cases [currentCase].distance, numOfSteps, stepDegree, cases [currentCase].size);
		}			
	}


	private void HandleDestroyableTarget(){
		Debug.Log ("HandleDestroyableTarget");
		if (experimentType == ExperimentType.IMPACT) {
			CloneCurrentTarget(currentTarget).SetActive(true);
		}
		else { targets[currentTarget].SetActive(true); }            
	}


	private void ShowNextTarget(){
		targetId++;
		isValid = true;

		ModifyTargetsIfNeccessary ();
		HandleDestroyableTarget ();

		currentTargetTransform = targets [currentTarget].transform;
		trackObject.activate (true); // starts recording the tracking data
	}


	public float GetCurrentTargetSize(){
		return currentSize;
	}
		
	public float GetCurrentTargetDistance(){
		return currentDistance;
	}
		
	public Transform GetCurrentTargetTransform(){
		return currentTargetTransform;
	}
		
	public int GetTargetId(){
		return targetId;
	}
		
	public bool IsTrialValid(){
		return isValid;
	}

	public ExperimentType GetExperimentType(){
		return experimentType;
	}

	public float GetShoulderHeight(){
		return shoulderHeight;
	}
				
	public float GetShoulderWidth(){
		return shoulderWidth;
	}
}
