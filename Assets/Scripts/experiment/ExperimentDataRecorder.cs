using UnityEngine;
using System.Collections.Generic;
//using UnityEditor;

[System.Serializable]
public class ExperimentData
{
	public string name;
    public long startingTime;
    public long endingTime;
    public List<long> successTime;
    public int successes = 0;
    public string type;
    public int misses = 0;
    public List<long> missTime;
    public bool leftHanded;
    public int participantID;
}

public class DataList : ScriptableObject {
	public List<ExperimentData> data;
}

public class ExperimentDataRecorder {
    //private List<ExperimentData> data = new List<ExperimentData>();
    private ExperimentData currentData;

    public void StartNewRecording(string type, bool leftHanded, int participantID)
    {     		
        currentData = new ExperimentData();
		currentData.name = "" + participantID + "_" + type;        
        currentData.type = type;
        currentData.leftHanded = leftHanded;
        currentData.participantID = participantID;
        currentData.startingTime = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 + System.DateTime.Now.Minute * 60 * 1000 + System.DateTime.Now.Hour * 60 * 60 * 1000;

		DataList dataList = ReadDataListFromAsset ();
		dataList.data.Add (currentData);
		//AssetDatabase.SaveAssets();		
    }



//    public void AddSuccess()
//    {
//        currentData.successTime.Add(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 + System.DateTime.Now.Minute * 60 * 1000 + System.DateTime.Now.Minute * 60 * 60 * 1000);
//        currentData.successes+=1;
//    }
//
//    public void AddMiss()
//    {
//        currentData.missTime.Add(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 + System.DateTime.Now.Minute * 60 * 1000 + System.DateTime.Now.Minute * 60 * 60 * 1000);
//        currentData.misses+=1;
//    }

    public void SetEnd()
    {
		DataList dataList = ReadDataListFromAsset ();
		currentData = dataList.data [dataList.data.Count - 1];
        currentData.endingTime = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 + System.DateTime.Now.Minute * 60 * 1000 + System.DateTime.Now.Hour * 60 * 60 * 1000;
		//AssetDatabase.SaveAssets();
    }

//    public List<ExperimentData> GetData()
//    {
//        return data;
//    }
    
	public DataList ReadDataListFromAsset(){
		string relPath = "Assets/DataManagement.asset";
        /*
		DataList dataList = AssetDatabase.LoadAssetAtPath (relPath, typeof(DataList)) as DataList;

		Debug.Log(""+dataList.data.Count);
        return dataList;*/
        return new DataList();
	}
}
