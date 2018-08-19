using UnityEngine;
using System.Collections;

public class visualizeDirection : MonoBehaviour {
	GameObject pseudoRay = null;
	public float length = 10.0f;
	public float size = 0.025f;
		
	// Use this for initialization
	void Start () {
		pseudoRay = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		pseudoRay.transform.localScale = new Vector3(size,length,size);
		pseudoRay.transform.parent = gameObject.transform;
		pseudoRay.transform.localPosition = new Vector3(0.0f, -0.1f, length);
		pseudoRay.transform.localRotation = Quaternion.Euler(new Vector3(0,90,90));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
