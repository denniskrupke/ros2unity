using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class ExperimentOrder
{
    public short size;
    public short[] order;

    public ExperimentOrder(short[] order)
    {
        this.order = order;
    }
}

public class ExperimentEmbodymentVisualization : MonoBehaviour {
    public DataWriter dataWriter = null;
    //public ExperimentDataRecorder dataRecorder = null;
    private List<ExperimentOrder> experimentOrders;

    [Header("Participant properties")]
    public int participantId = 0;
    public bool leftHanded = false;

    private short[] order = { 0, 1, 2 };
    private string[] scenes = { "experiment_sort-cylinders", "experiment_sort-cylinders-withGhost", "experiment_sort-cylinders-withHands" };
    private short currentTrial = 0;

    // Use this for initialization
    void Start () {
        experimentOrders = new List<ExperimentOrder>();
        experimentOrders.Add(new ExperimentOrder(new short[] { 0, 1, 2 }));
        experimentOrders.Add(new ExperimentOrder(new short[] { 0, 2, 1 }));
        experimentOrders.Add(new ExperimentOrder(new short[] { 1, 0, 2 }));
        experimentOrders.Add(new ExperimentOrder(new short[] { 1, 2, 0 }));
        experimentOrders.Add(new ExperimentOrder(new short[] { 2, 0, 1 }));
        experimentOrders.Add(new ExperimentOrder(new short[] { 2, 1, 0 }));

        this.order = experimentOrders[participantId % experimentOrders.Count].order;

        //DontDestroyOnLoad(this);
        LoadNextScene();
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.KeypadEnter)){
            LoadNextScene();
        }
	}
    

    public void LoadNextScene()
    {
        /*
        if (currentTrial > 0) { ExperimentDataRecorder.SetEnd(); }

        if (currentTrial < 3)
        {
            ExperimentDataRecorder.StartNewRecording(scenes[order[currentTrial]], leftHanded, participantId);
            Debug.Log("Loading: " + scenes[order[currentTrial]]);
            SceneManager.LoadScene(scenes[order[currentTrial]]);
            currentTrial++;
        }
        else
        {
            Debug.Log("Done!");
            //dataWriter.WriteData();
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
        */
    }

}
