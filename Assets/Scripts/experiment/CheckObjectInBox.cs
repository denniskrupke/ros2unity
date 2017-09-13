using UnityEngine;
using System.Collections.Generic;
//using UnityEditor;

public class CheckObjectInBox : MonoBehaviour {
    private List<GameObject> targets;
    //private ExperimentDataRecorder dataRecorder = null;
	// Use this for initialization
	void Start () {
        targets = new List<GameObject>();
        //dataRecorder = (ExperimentDataRecorder) GameObject.FindObjectOfType(typeof(ExperimentDataRecorder));
        //dataRecorder = GameObject.Find("persistantObject").GetComponent<ExperimentDataRecorder>();
        //dataRecorder = FindObjectOfType<ExperimentDataRecorder>();
    }

    // Update is called once per frame
    void Update () {
	    
	}

    /*
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target") && !targets.Contains(other.gameObject))
        {
            targets.Add(other.gameObject);
            //Debug.Log("Good job!");
			string relPath = "Assets/DataManagement.asset";
			DataList dataList = AssetDatabase.LoadAssetAtPath (relPath, typeof(DataList)) as DataList;               
			dataList.data[dataList.data.Count-1].successTime.Add(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 + System.DateTime.Now.Minute * 60 * 1000 + System.DateTime.Now.Hour * 60 * 60 * 1000);
			dataList.data[dataList.data.Count-1].successes+=1;
			AssetDatabase.SaveAssets();
        }
    }
    */
}
