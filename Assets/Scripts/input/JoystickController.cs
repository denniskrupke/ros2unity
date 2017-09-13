using UnityEngine;
using System.Collections;

public class JoystickController : MonoBehaviour {
    public GameObject ghost_manipulator = null;
   // public CapsuleHand capsule_hand = null;
   // public InvisibleCapsuleHand invisible_hand = null;
    public ResetTransforms resetTargets = null;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1"))
        {
            ghost_manipulator.SetActive(!ghost_manipulator.activeSelf);
            Debug.Log("Button 0");
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            Debug.Log("Button 1");
        }
        else if (Input.GetButtonDown("Fire3"))
        {
            Debug.Log("Button 2");
        }
        else if (Input.GetButtonDown("Jump"))
        {
            resetTargets.Reset();
            Debug.Log("Button 3");
        }         
    }
}
