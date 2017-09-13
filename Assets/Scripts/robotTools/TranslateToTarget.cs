using UnityEngine;
using System.Collections;

public class TranslateToTarget : MonoBehaviour {
    public Transform targetPose = null;    
    public int durationInSeconds = 5;
    private Vector3 startPosition = new Vector3();
    private Quaternion startRotation = new Quaternion();
    private float count = 0.0f;
    private float countGraspClose = 0.0f;
    private float countGraspOpen = 0.0f;
    private float countApproach = 0.0f;
    private bool coroutineHasStopped = false;
    public bool move = true;
    public bool loop = false;
    private float startGripperClosingValue = 0.0f;

    public actuateGripper gripper = null;

	// Use this for initialization
	void Start () {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startGripperClosingValue = gripper.GetCurrentClosingValue();
        StartCoroutine("IncreaseCount");
    }  
    
    public void SetTargetPose(Vector3 pos, Quaternion rot)
    {
        targetPose.position = pos;
        targetPose.rotation = rot;
    }  
	
	// Update is called once per frame
	void Update () {
        
	}

    void FixedUpdate()
    {
        if (coroutineHasStopped)
        {
            coroutineHasStopped = false;
            StartCoroutine("IncreaseCount");
        }
    }


    IEnumerator IncreaseCount()
    {
        if (move)
        {
            Debug.Log("Open Hand");
            float currentClosingValue = gripper.GetCurrentClosingValue();
            count = 0;
            while (countGraspOpen < (durationInSeconds / 8))
            {
                countGraspOpen += Time.deltaTime;
                gripper.MoveGripper(Mathf.SmoothStep(currentClosingValue, 0, (durationInSeconds / 8)));
                yield return null;
            }

            Debug.Log("Move Arm Phase 1");
            Vector3 approachPoint = targetPose.position + targetPose.forward * -.2f;
            //approachPoint.y += -.3f;
            while (count < (durationInSeconds / 2))
            {
                count += Time.deltaTime;
                transform.position = Vector3.Slerp(startPosition, approachPoint, count / (durationInSeconds / 2));
                transform.rotation = Quaternion.Slerp(startRotation, targetPose.rotation, count / (durationInSeconds / 2));
                yield return null;
            }

            Debug.Log("Move Arm Phase 2");
            Vector3 currentPosition = transform.position;
            while (countApproach < (durationInSeconds / 4))
            {
                countApproach += Time.deltaTime;
                transform.position = Vector3.Slerp(currentPosition, targetPose.position, countApproach / (durationInSeconds / 4));
                yield return null;
            }

            Debug.Log("Close Hand");
            currentClosingValue = gripper.GetCurrentClosingValue();
            while (countGraspClose < (durationInSeconds / 8))
            {
                countGraspClose += Time.deltaTime;
                gripper.MoveGripper(Mathf.SmoothStep(currentClosingValue, .33f, countGraspClose / (durationInSeconds / 8)));
                yield return null;
            }
            Debug.Log("done");
        }

        if (loop) {
            Debug.Log("Reset");                      
            coroutineHasStopped = true;
            countGraspOpen = 0;
            count = 0;
            countApproach = 0;            
            countGraspClose = 0;

            transform.position = startPosition;
            transform.rotation = startRotation;
            gripper.MoveGripper(this.startGripperClosingValue);

            float countWait = 0.0f;
            while (countWait < 2)
            {
                countWait += Time.deltaTime;
                yield return null;
            }
        }
    }

}
