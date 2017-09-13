using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IKsolution{
	public Vector3 position { get; set; }
	public Quaternion rotation { get; set; }
	public bool reachable { get; set; }

	public IKsolution(Vector3 pos, Quaternion rot, bool reachable){
		this.position = pos;
		this.rotation = rot;
		this.reachable = reachable;
	}


}

public class ReachabilityCheck : MonoBehaviour {
	public Transform goalPosition;
	public Transform tcp;
	public int rangeTranslate = 500;
	public int maxFramesForSingleSearch = 200;

	public Mesh mesh;
	public Material materialPos;
	public Material materialNeg;

	private float distanceXYZ = 0.0f;
	private Quaternion distanceQuaternion = new Quaternion();
	private Vector3 goalPositionStart;

	private int frameCountOfLastStartOfSolutionSearch = 0;
	private bool result = false;
	private bool exploring = false;

	private List<IKsolution> solutions = new List<IKsolution>();
	private IKsolution currentSolution;

	private int[] indices;// = Enumerable.Range(0, mesh.vertices.Length-1).ToArray();

	// Use this for initialization
	void Start () {
		goalPositionStart = goalPosition.position;
		materialPos.shader = Shader.Find("Particles/Additive");
		materialNeg.shader = Shader.Find("Particles/Additive");

		mesh.Clear();
		mesh = new Mesh ();
		mesh.SetIndices(indices, MeshTopology.Points,0);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.S)) {
			exploring = true;
			GoToNextPosition ();
		} else if (Input.GetKeyDown (KeyCode.Escape)) {
			exploring = false;
		}
		if (exploring) {			
			float dist = CompareDistances (goalPosition, tcp);
			result = dist < 0.01;
			if (result) {
//			Debug.Log (""+dist);
				//Debug.Log (result);
				currentSolution = new IKsolution (goalPosition.position, goalPosition.rotation, true);
				solutions.Add (currentSolution);
				GoToNextPosition ();
			} else if (frameCountOfLastStartOfSolutionSearch + maxFramesForSingleSearch < Time.frameCount) { // NO SOLUTION FOUND
				//Debug.Log (result);
				currentSolution = new IKsolution (goalPosition.position, goalPosition.rotation, false);
				solutions.Add (currentSolution);
				GoToNextPosition ();
				// STORE DISTANCE, CATEGORY, ...
				Debug.Log (""+solutions.Count);
			}
		}
	}

	void FixedUpdate(){		
		Vector3[] vertices = new Vector3[solutions.Count];
		int count = 0;

		Color32[] colors = new Color32[solutions.Count];

		foreach (IKsolution sol in solutions) {
			colors [count] = sol.reachable ? Color.green : Color.red;
			vertices [count++] = sol.position;
//			Graphics.DrawMeshInstanced(mesh, sol.position, sol.rotation, material, 0);
		}
		mesh.vertices = vertices;

		int[] indices = Enumerable.Range(0, mesh.vertices.Length-1).ToArray();
		mesh.SetIndices(indices, MeshTopology.Points,0);

//		Color32[] colors = new Color32[mesh.vertices.Length];
//		for (int i = 0; i < colors.Length; i++)
//			colors[i] = Color32.Lerp(Color.red, Color.green, mesh.vertices[i].y);
		mesh.colors32 = colors;

		Graphics.DrawMesh(mesh, Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, 1)), materialPos, 0);
	}

	// Generates goal poses for the endeffector's tcp
	void GoToNextPosition(){
		frameCountOfLastStartOfSolutionSearch = Time.frameCount;	
		goalPosition.position = new Vector3 (Random.Range (0, rangeTranslate), Random.Range (0, rangeTranslate), Random.Range (0, rangeTranslate));
		goalPosition.position = goalPositionStart + goalPosition.position / 1000;
		goalPosition.rotation = new Quaternion((float)Random.Range (0, 1000)/1000, (float)Random.Range (0, 1000)/1000, (float)Random.Range (0, 1000)/1000, Random.Range (0, 314)/100);
	}
		

	float CompareDistances(Transform goal, Transform tcp){
		distanceXYZ = Vector3.Distance (goal.position, tcp.position);
		// = Quaternion.FromToRotation (goal.rotation, tcp.rotation);
		return distanceXYZ;
	}
		
}

