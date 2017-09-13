using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;


public class DataWriter : MonoBehaviour {
    //public TrackObject trackObject = null;
    //public SpawnTargets spawnTargets = null;
    //public ExperimentDataRecorder dataRecorder = null;    

	public string experimentFilename = "default.csv";    

	// Use this for initialization
	void Start () {
       // DontDestroyOnLoad(this);
    }


	void Update () {}

	
    public void WriteData()
    {
        WriteLineToExperimentDataFile();
    }

	private void WriteLineToExperimentDataFile()
    {
		//spawnTargets.CheckValidityOfTrial ();
		if (!File.Exists (experimentFilename)) {            
            Debug.Log("File does not exist");			    
            File.WriteAllText(experimentFilename, 
                "participantId"
				+ ","
				+ "leftHanded"
				+ ","
				+ "conditionName"
				+ ","
				+ "numberOfSuccesses"
				+ ","
				+ "successTimes"
				+ ","
				+ "numberOfMisses"
				+ ","
				+ "missTimes"
				+ ","
				+ "startTime"
				+ ","
				+ "endTime"				
                + Environment.NewLine
			);
        }
        //foreach (ExperimentData data in ExperimentDataRecorder.GetData())
        {
            //WriteLineOfExperimentData(data);
        }
		
	}


	private void WriteLineOfExperimentData(ExperimentData data)
    {        
	    File.AppendAllText(experimentFilename, ""
		    + data.participantID
            + ","
            + data.leftHanded
            + ","
            + data.type
            + ","
            + data.successes
            + ","
            + data.successTime
            + ","
            + data.misses
            + ","
            + data.missTime
            + ","
            + data.startingTime
            + ","
            + data.endingTime
            + ","
            + Environment.NewLine);     
	}
}
