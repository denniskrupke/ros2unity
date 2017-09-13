using UnityEngine;
using System.Collections.Generic;



public class TrackingDataFrame{
	public Vector3 position;
	public Quaternion rotation;
	public long timeStamp;


	public TrackingDataFrame(Vector3 pos, Quaternion rot){
		UpdateCurrentTimestampInMilliseconds ();
		position = pos;
		rotation = rot;
	}


	void UpdateCurrentTimestampInMilliseconds(){
		timeStamp = System.DateTime.Now.Millisecond + System.DateTime.Now.Second*1000 + System.DateTime.Now.Minute*60*1000 + System.DateTime.Now.Minute*60*60*1000;
	}
}
		


/*! \brief Has to be attached to an object to capture position and rotation in world space
 * 
 */
public class TrackObject : MonoBehaviour {
	private List<TrackingDataFrame> trackingData;
	private bool active = false;


	void Start () {
		trackingData = new List<TrackingDataFrame>();
	}


	// Update is called once per frame
	void Update () {
		if (active) {
			Debug.Log ("data");
			trackingData.Add (new TrackingDataFrame (transform.position, transform.rotation));
		}
	}


	public List<TrackingDataFrame> GetTrackingData(){
		return trackingData;
	}


	public void Clear(){
		trackingData.Clear ();
	}


	public void activate(bool active){
		this.active = active;
	}

}
