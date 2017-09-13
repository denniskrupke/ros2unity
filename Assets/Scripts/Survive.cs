using UnityEngine;
using System.Collections;

public class Survive : MonoBehaviour {

    GameObject instance;

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
//        if (instance == null)
//        {
//            instance = this.gameObject;
//        }
    }

//    public static GameObject GetInstance()
//    {
//        return instance;
//    }
//    // Use this for initialization
//    void Start () {
//	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
    
}
