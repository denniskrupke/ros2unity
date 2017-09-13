using UnityEngine;
using System.Collections;

public class HighlightCollision : MonoBehaviour {

    private Material oldMaterial;
	// Use this for initialization
	void Start () {
        oldMaterial = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.tag != "ghost")
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1.0f, .0f, .0f);
            GetComponent<Renderer>().material = mat;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag != "ghost")
        {
            GetComponent<Renderer>().material = oldMaterial;
        }
    }


}
