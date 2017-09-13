using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachabilityCheck_octree : MonoBehaviour {
	public int sizeInMM = 1000;
	public Transform startPose = null;
	public Transform goalPosition = null;
	public Transform tcp = null;

	public Mesh mesh;
	public Material material;

	private bool ikchecked = false;

	// Use this for initialization
	void Start () {
		mesh.Clear();
		mesh = new Mesh ();
		float val = (float)sizeInMM / 1000.0f;
		mesh.vertices = new Vector3[] {new Vector3(-val, -val, val), new Vector3(-val, val, val), new Vector3(val, val, val), new Vector3(val, -val, val), 
										new Vector3(-val, -val, -val), new Vector3(-val, val, -val), new Vector3(val, val, -val), new Vector3(val, -val, -val)};

		mesh.colors32 = new Color32[] {Color.green, Color.green, Color.green, Color.green,
			Color.green, Color.green, Color.green, Color.green};
//		mesh.uv = newUV;
		mesh.triangles = new int[] {0,3,2, 0,2,1, 4,7,6, 4,6,5, 4,0,1, 4,1,5, 7,3,2, 7,2,6, 1,2,6, 1,6,5, 0,3,7, 0,7,4};

		material.shader = Shader.Find("Particles/Additive");
	}
	
	// Update is called once per frame
	void Update () {
		if (ikchecked) {
			Graphics.DrawMesh (mesh, Matrix4x4.TRS (startPose.position, Quaternion.identity, new Vector3 (1, 1, 1)), material, 0);
		} else {
			CheckVertices ();
		}
			
	}

	bool CompareDistances(Transform goal, Transform tcp){		
		return Vector3.Distance (goal.position, tcp.position) < .002;
	}

	void CheckVertices(){
		int index = 0;
		foreach (Vector3 pos in mesh.vertices) {
			goalPosition.position = mesh.vertices [index];
			mesh.colors32 [index] = CompareDistances(goalPosition, tcp) ? Color.green : Color.red;
			index++;
		}
		ikchecked = true;
	}

	void AddVertex(Vector3 pos, Mesh mesh){
		// extending the list of vertices
		var z = new Vector3[mesh.vertices.Length + 1];
		(mesh.vertices).CopyTo(z, 0);
		(new Vector3[] {pos}).CopyTo(z, (mesh.vertices).Length);
		mesh.vertices = z;
	}
}
