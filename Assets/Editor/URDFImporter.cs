﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using System.Xml;
using System;

public class URDFImporter : EditorWindow {

	[MenuItem ("Addons/URDF Importer")]
	static void Init() {
		EditorWindow.GetWindow(typeof (URDFImporter));
	}

	public class URDFData {

		public bool Failed;

		public string AbsolutePath;
		public string RelativePath;
		public string AbsoluteFolder;
		public string RelativeFolder;
		public string Name;
		public List<MaterialData> Materials;
		public List<JointData> Joints;
		public List<LinkData> Links;

		public URDFData(string path) {
			Failed = false;

			AbsolutePath = String.Empty;
			//RelativePath = String.Empty;
			//AbsoluteFolder = String.Empty;
			//RelativeFolder = string.Empty;

			Materials = new List<MaterialData>();
			Links = new List<LinkData>();
			Joints = new List<JointData>();

			RelativePath = path;
			AbsolutePath = Application.dataPath + "/" + RelativePath;
			RelativeFolder = RelativePath.Substring(0, RelativePath.LastIndexOf("/")+1);
			AbsoluteFolder = Application.dataPath + "/" + RelativeFolder;

			XmlDocument file = new XmlDocument();
			try {
				file.Load(AbsolutePath);
			} catch(Exception e) {
				Debug.Log(e.Message);
				Failed = true;
				return;
			}

			XmlNode robotNode = null;
			for(int i=0; i<file.ChildNodes.Count; i++) {
				if(file.ChildNodes[i].Name == "robot") {
					robotNode = file.ChildNodes[i];
					Name = robotNode.Attributes.GetNamedItem("name").Value;
				}
			}

			//Read all first-layer childs of <robot ...>
			for(int i=0; i<robotNode.ChildNodes.Count; i++) {

				//Read Material
				if(robotNode.ChildNodes[i].Name == "material") {
					XmlNode materialNode = robotNode.ChildNodes[i];
					URDFData.MaterialData materialData = new URDFData.MaterialData(materialNode.Attributes.GetNamedItem("name").Value);

					for(int j=0; j<materialNode.ChildNodes.Count; j++) {
						if(materialNode.ChildNodes[j].Name == "color") {
							XmlNode colorNode = materialNode.ChildNodes[j];

							XmlNode rgba = colorNode.Attributes.GetNamedItem("rgba");
							if(rgba != null) {
								materialData.Color = ReadColor(rgba.Value);
							}
						}

						if(materialNode.ChildNodes[j].Name == "texture") {
							XmlNode textureNode = materialNode.ChildNodes[j];

							XmlNode fileName = textureNode.Attributes.GetNamedItem("filename");
							if(fileName != null) {
								string texturePath = fileName.Value;
								texturePath = texturePath.Substring(10);
								texturePath = "Assets/" + RelativeFolder + texturePath;
								materialData.Texture = texturePath;
							}
						}
					}

					Materials.Add(materialData);
				}

				//Read Link
				if(robotNode.ChildNodes[i].Name == "link") {
					XmlNode linkNode = robotNode.ChildNodes[i];

					//Multiple instances of visual are possible
					string name = linkNode.Attributes.GetNamedItem("name").Value;
					URDFData.LinkData linkData = null;
					if(GetLinkData(name) != null) {
						linkData = GetLinkData(name);
					} else {
						linkData = new URDFData.LinkData(name);
					}

					bool visual = false;
					bool inertial = false;

					for(int j=0; j<linkNode.ChildNodes.Count; j++) {
						if(linkNode.ChildNodes[j].Name == "visual" && !visual) {
							visual = true;

							XmlNode visualNode = linkNode.ChildNodes[j];

							for(int k=0; k<visualNode.ChildNodes.Count; k++) {
								if(visualNode.ChildNodes[k].Name == "geometry") {
									XmlNode geometryNode = visualNode.ChildNodes[k];
									
									for(int l=0; l<geometryNode.ChildNodes.Count; l++) {
										if(geometryNode.ChildNodes[l].Name == "box") {
											linkData.Geometry = new URDFData.Box(ReadVector3(geometryNode.ChildNodes[l].Attributes.GetNamedItem("size").Value));
										} else if(geometryNode.ChildNodes[l].Name == "cylinder") {
											linkData.Geometry = new URDFData.Cylinder(ReadFloat(geometryNode.ChildNodes[l].Attributes.GetNamedItem("length").Value), ReadFloat(geometryNode.ChildNodes[l].Attributes.GetNamedItem("radius").Value));
										} else if(geometryNode.ChildNodes[l].Name == "sphere") {
											linkData.Geometry = new URDFData.Sphere(ReadFloat(geometryNode.ChildNodes[l].Attributes.GetNamedItem("radius").Value));
										} else if(geometryNode.ChildNodes[l].Name == "mesh") {
											string meshPath = geometryNode.ChildNodes[l].Attributes.GetNamedItem("filename").Value;
											meshPath = meshPath.Substring(10);
											meshPath = "Assets/" + RelativeFolder + meshPath;
											XmlNode scaleNode = geometryNode.ChildNodes[l].Attributes.GetNamedItem("scale");
											if(scaleNode != null) {
												//toLower
												linkData.Geometry = new URDFData.Mesh(meshPath, ReadVector3(scaleNode.Value));
											} else {
												//toLower
												linkData.Geometry = new URDFData.Mesh(meshPath, Vector3.one);
											}
										}
									}
								}

								if(visualNode.ChildNodes[k].Name == "material") {
									XmlNode materialNode = visualNode.ChildNodes[k];

									linkData.Material = materialNode.Attributes.GetNamedItem("name").Value;

									if(materialNode.HasChildNodes) {
										URDFData.MaterialData materialData = new URDFData.MaterialData(linkData.Material);

										for(int l=0; l<materialNode.ChildNodes.Count; l++) {
											if(materialNode.ChildNodes[l].Name == "color") {
												XmlNode colorNode = materialNode.ChildNodes[l];

												XmlNode rgba = colorNode.Attributes.GetNamedItem("rgba");
												if(rgba != null) {
													materialData.Color = ReadColor(rgba.Value);
												}
											}

											if(materialNode.ChildNodes[l].Name == "texture") {
												XmlNode textureNode = materialNode.ChildNodes[l];

												XmlNode fileName = textureNode.Attributes.GetNamedItem("filename");
												if(fileName != null) {
													string texturePath = fileName.Value;
													texturePath = texturePath.Substring(10);
													texturePath = "Assets/" + RelativeFolder + texturePath;
													materialData.Texture = texturePath;
												}
											}
										}

										Materials.Add(materialData);
									}

								}

								if(visualNode.ChildNodes[k].Name == "origin") {
									XmlNode originNode = visualNode.ChildNodes[k];

									XmlNode xyz = originNode.Attributes.GetNamedItem("xyz");
									if(xyz != null) {
										linkData.XYZ = ReadVector3(xyz.Value);
									}
									XmlNode rpy = originNode.Attributes.GetNamedItem("rpy");
									if(rpy != null) {
										linkData.RPY = ReadVector3(rpy.Value);
									}
								}
							}
						}

						if(linkNode.ChildNodes[j].Name == "inertial" && !inertial) {
							inertial = true;
							
							XmlNode inertialNode = linkNode.ChildNodes[j];

							for(int k=0; k<inertialNode.ChildNodes.Count; k++) {
								if(inertialNode.ChildNodes[k].Name == "mass") {
									XmlNode massNode = inertialNode.ChildNodes[k];

									linkData.Mass = ReadFloat(massNode.Attributes.GetNamedItem("value").Value);
								}
							}
						}
					}

					Links.Add(linkData);
				}

				//Read Joint
				if(robotNode.ChildNodes[i].Name == "joint") {
					XmlNode jointNode = robotNode.ChildNodes[i];
					URDFData.JointData jointData = new URDFData.JointData(jointNode.Attributes.GetNamedItem("name").Value, jointNode.Attributes.GetNamedItem("type").Value);

					for(int j=0; j<jointNode.ChildNodes.Count; j++) {
						if(jointNode.ChildNodes[j].Name == "parent") {
							jointData.Parent = jointNode.ChildNodes[j].Attributes.GetNamedItem("link").Value;
						}
						if(jointNode.ChildNodes[j].Name == "child") {
							jointData.Child = jointNode.ChildNodes[j].Attributes.GetNamedItem("link").Value;
						}
						if(jointNode.ChildNodes[j].Name == "axis") {
							XmlNode xyz = jointNode.ChildNodes[j].Attributes.GetNamedItem("xyz");
							if(xyz != null) {
								jointData.Axis = ReadVector3(xyz.Value);
							}
						}
						if(jointNode.ChildNodes[j].Name == "origin") {
							XmlNode xyz = jointNode.ChildNodes[j].Attributes.GetNamedItem("xyz");
							if(xyz != null) {
								jointData.OriginXYZ = ReadVector3(xyz.Value);
							}
							XmlNode rpy = jointNode.ChildNodes[j].Attributes.GetNamedItem("rpy");
							if(rpy != null) {
								jointData.OriginRPY = ReadVector3(rpy.Value);
							}
						}
						if(jointNode.ChildNodes[j].Name == "limit") {
							XmlNode effort = jointNode.ChildNodes[j].Attributes.GetNamedItem("effort");
							if(effort != null) {
								jointData.Force = ReadFloat(effort.Value);
							}
							XmlNode velocity = jointNode.ChildNodes[j].Attributes.GetNamedItem("velocity");
							if(velocity != null) {
								jointData.Velocity = ReadFloat(velocity.Value);
							}
							XmlNode lower = jointNode.ChildNodes[j].Attributes.GetNamedItem("lower");
							if(lower != null) {
								jointData.LowerLimit = ReadFloat(lower.Value);
							}
							XmlNode upper = jointNode.ChildNodes[j].Attributes.GetNamedItem("upper");
							if(upper != null) {
								jointData.UpperLimit = ReadFloat(upper.Value);
							}
						}
					}
					Joints.Add(jointData);
				}
			}
		}

		public MaterialData GetMaterialData(string name) {
			return Materials.Find(x => x.Name.Equals(name));
		}

		public LinkData GetLinkData(string name) {
			return Links.Find(x => x.Name.Equals(name));
		}

		public JointData GetJointData(string name) {
			return Joints.Find(x => x.Name.Equals(name));
		}

		public void Log() {
			Debug.Log("Relative Path: " + RelativePath + "\n");
			Debug.Log("Absolute Path: " + AbsolutePath + "\n");
			Debug.Log("Relative Folder: " + RelativeFolder + "\n");
			Debug.Log("Absolute Folder: " + AbsoluteFolder + "\n");

			Debug.Log("Name: " + Name + "\n");
			Debug.Log("/////MATERIALS/////");
			for(int i=0; i<Materials.Count; i++) {
				Debug.Log(
					"Name: " + Materials[i].Name + "\n"
					+	"Color: " + Materials[i].Color + "\n"
					+	"Texture: " + Materials[i].Texture);
			}
			Debug.Log("/////LINKS/////");
			for(int i=0; i<Links.Count; i++) {
				Debug.Log(
					"Name: " + Links[i].Name + "\n"
					+	"Geometry Type: " + Links[i].Geometry.Type + "\n"
					+	Links[i].Geometry.GetDataInfo() + "\n"
					+	"Material: " + Links[i].Material + "\n"
					+	"Origin: " + Links[i].XYZ.ToString("F4") + "\n"
					+	"RPY: " + Links[i].RPY.ToString("F4"));
			}
			Debug.Log("/////JOINTS/////");
			for(int i=0; i<Joints.Count; i++) {
				Debug.Log(
					"Name: " + Joints[i].Name + "\n"
					+	"Type: " + Joints[i].Type + "\n"
					+	"Parent: " + GetLinkData(Joints[i].Parent).Name + "\n"
					+	"Parent: " + GetLinkData(Joints[i].Child).Name + "\n"
					+	"Origin: " + Joints[i].OriginXYZ.ToString("F4") + "\n"
					+	"RPY: " + Joints[i].OriginRPY.ToString("F4") + "\n"
					+	"Axis: " + Joints[i].Axis.ToString("F4"));
			}
		}

		public class MaterialData {
			public string Name;
			public Color Color;
			public string Texture;

			public MaterialData(string name) {
				Name = name;
				Color = Color.white;
				Texture = string.Empty;
			}
		}
		public class LinkData {
			public string Name;
			public Geometry Geometry;
			public string Material;
			public Vector3 XYZ;
			public Vector3 RPY;
			public float Mass;

			public LinkData(string name) {
				Name = name;
				Geometry = new Empty();
				Material = string.Empty;
				XYZ = Vector3.zero;
				RPY = Vector3.zero;
				Mass = 1f;
			}
		}
		public class JointData {
			public string Name;
			public string Type;
			public Vector3 OriginXYZ;
			public Vector3 OriginRPY;
			public Vector3 Axis;
			public string Parent;
			public string Child;
			public float Force;
			public float Velocity;
			public float LowerLimit;
			public float UpperLimit;

			public JointData(string name, string type) {
				Name = name;
				Type = type;
				OriginXYZ = Vector3.zero;
				OriginRPY = Vector3.zero;
				Axis = new Vector3(1f,0f,0f);
				Parent = string.Empty;
				Child = string.Empty;
				Force = 0f;
				Velocity = 0f;
				LowerLimit = 0f;
				UpperLimit = 0f;
			}
		}
		public enum GeometryType{Box, Cylinder, Sphere, Mesh, Empty};
		public abstract class Geometry {
			public GeometryType Type;
			public Geometry(GeometryType type) {
				Type = type;
			}
			public abstract string GetDataInfo();
		}
		public class Box : Geometry {
			public Vector3 Size;
			public Box(Vector3 size) : base(GeometryType.Box) {
				Size = size;
			}
			public override string GetDataInfo() {
				return "Size: " + Size.ToString();
			}
		}
		public class Cylinder : Geometry {
			public float Length;
			public float Radius;
			public Cylinder(float length, float radius) : base(GeometryType.Cylinder) {
				Length = length;
				Radius = radius;
			}
			public override string GetDataInfo() {
				return "Length: " + Length + "\n" + "Radius: " + Radius;
			}
		}
		public class Sphere : Geometry {
			public float Radius;
			public Sphere(float radius) : base(GeometryType.Sphere) {
				Radius = radius;
			}
			public override string GetDataInfo() {
				return "Radius: " + Radius;
			}
		}
		public class Mesh : Geometry {
			public string Path;
			public Vector3 Scale;
			public Mesh(string path, Vector3 scale) : base(GeometryType.Mesh) {
				Path = path;
				Scale = scale;
			}
			public override string GetDataInfo() {
				return "Path: " + Path + "\n" + "Scale: " + Scale.ToString("F4");
			}
		}
		public class Empty : Geometry {
			public Empty() : base(GeometryType.Empty) {

			}
			public override string GetDataInfo() {
				return "No Geometry";
			}
		}

		private float ReadFloat(string value) {
			value = FilterValueField(value);
			return ParseFloat(value);
		}

		private Vector3 ReadVector3(string value) {
			value = FilterValueField(value);

			string[] values = value.Split(' ');
			float x = ParseFloat(values[0]);
			float y = ParseFloat(values[1]);
			float z = ParseFloat(values[2]);

			return new Vector3(x,y,z);
		}

		private Vector4 ReadColor(string value) {
			value = FilterValueField(value);

			string[] values = value.Split(' ');
			float r = ParseFloat(values[0]);
			float g = ParseFloat(values[1]);
			float b = ParseFloat(values[2]);
			float a = ParseFloat(values[3]);

			return new Color(r,g,b,a);
		}

		private string FilterValueField(string value) {
			while(value.Contains("  ")) {
				value = value.Replace("  "," ");
			}
			while(value.Contains("< ")) {
				value = value.Replace("< ","<");
			}
			while(value.Contains(" >")) {
				value = value.Replace(" >",">");
			}
			while(value.Contains(" .")) {
				value = value.Replace(" ."," 0.");
			}
			while(value.Contains("<.")) {
				value = value.Replace("<.","<0.");
			}
			return value;
		}

		private float ParseFloat(string value) {
			float parsed = 0f;
			if(float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsed)) {
				return parsed;
			} else {
				Debug.Log("Error parsing " + value + "!");
				return 0f;
			}
		}
	}

	private string Path = string.Empty;
	private Vector3 MeshOrientation = new Vector3(270f, 90f, 0f);

	private string Output = string.Empty;
	private int Errors = 0;

	public GameObject Import() {
		if(Application.isPlaying) {
			Debug.Log("Can not import during runtime. Use in Edit-Mode only.");
			return null;
		}
		if(Path == string.Empty) {
			Debug.Log("Path is empty.");
			return null;
		}

		URDFData data = new URDFData(Path);
		if(data.Failed) {
			Debug.Log("Importing failed.");
			return null;
		} else {
			Debug.Log("Importing " + Path + "...");
			data.Log();
			return CreateFromData(data);
		}
	}

	private GameObject CreateFromData(URDFData data) {
		Transform actor = new GameObject(data.Name).transform;

		actor.position = new Vector3(0f,0f,0f);
		actor.rotation = Quaternion.identity;

		List<Transform> Links = new List<Transform>();
		List<Transform> Joints = new List<Transform>();

		//Create Link Transforms
		for(int i=0; i<data.Links.Count; i++) {
			Transform link = CreateGeometry(data.Links[i].Geometry).transform;
			link.name = data.Links[i].Name;
			link.SetParent(actor, false);
			Links.Add(link);
		}

		//Create Joint Transforms
		for(int i=0; i<data.Joints.Count; i++) {
			Transform joint = new GameObject().transform;
			joint.name = data.Joints[i].Name;
			joint.SetParent(actor);
			Joints.Add(joint);
		}

		//Apply Parent-Child Relations
		for(int i=0; i<Joints.Count; i++) {
			Transform joint = Joints[i];
			Transform parent = FindTransformByName(Links, data.GetJointData(joint.name).Parent);
			Transform child = FindTransformByName(Links, data.GetJointData(joint.name).Child);

			Transform parentJoint = actor;
			string parentName = data.GetLinkData(parent.name).Name;
			for(int j=0; j<Joints.Count; j++) {
				if(data.GetJointData(Joints[j].name).Child == parentName) {
					parentJoint = Joints[j];
					break;
				}
			}

			joint.SetParent(parentJoint);
			child.SetParent(joint);
		}

		Links = GetOrderedTransforms(actor.root, Links, new List<Transform>());
		Joints = GetOrderedTransforms(actor.root, Joints, new List<Transform>());

		for(int i=0; i<Joints.Count; i++) {
			Transform joint = Joints[i];

			Vector3 angles = -Mathf.Rad2Deg * ROSToUnity(data.GetJointData(joint.name).OriginRPY);
			Quaternion rotation = Quaternion.Euler(angles);
			joint.position = joint.parent.position + joint.parent.rotation * ROSToUnity(data.GetJointData(joint.name).OriginXYZ);
			joint.rotation = joint.parent.rotation * rotation;
		}
			
		for(int i=0; i<Links.Count; i++) {
			Transform link = Links[i];
			Vector3 angles = -Mathf.Rad2Deg * ROSToUnity(data.GetLinkData(link.name).RPY);
			Quaternion rotation = Quaternion.Euler(angles);
			link.localPosition += ROSToUnity(data.GetLinkData(link.name).XYZ);
			link.localRotation = rotation * link.localRotation;
		}
			
		//Initialize Links
		for(int i=0; i<Links.Count; i++) {
			//Nothing to do.
		}

		//Initialize Joints
		for(int i=0; i<Joints.Count; i++) {
			BioIK.KinematicJoint joint = Joints[i].gameObject.AddComponent<BioIK.KinematicJoint>();
			URDFData.JointData jointData = data.GetJointData(joint.name);

			if(jointData.Type == "fixed") {
				//Nothing to do
			} else {
				switch(jointData.Type) {
				case "prismatic":
					joint.SetJointType(BioIK.JointType.Prismatic);
					break;
				case "revolute":
					joint.SetJointType(BioIK.JointType.Revolute);
					break;
				case "continuous":
					joint.SetJointType(BioIK.JointType.Continuous);
					break;
				}

				joint.SetAnchor(Vector3.zero);
				Vector3 axis = ROSToUnity(jointData.Axis);
				if(joint.GetJointType() != BioIK.JointType.Prismatic) {
					axis = -axis;
				}

				joint.SetOrientation(Quaternion.FromToRotation(Vector3.right, axis).eulerAngles);
				//joint.SetMaximumVelocity(jointData.Velocity);
				//joint.SetMaximumAcceleration(jointData.Velocity);

				BioIK.Motion motion = joint.GetXMotion();
				motion.SetEnabled(true);
				motion.SetLowerLimit(jointData.LowerLimit);
				motion.SetUpperLimit(jointData.UpperLimit);

				joint.Initialise();
			}
		}

		if(Errors == 0) {
			Debug.Log("Successfully imported '" + actor.name + "'.");
		} else {
			Debug.Log(Errors + " errors or warnings during importing '" + actor.name + "'.\n" + Output);
		}
		Output = string.Empty;
		Errors = 0;

		return actor.gameObject;
	}

	private List<Transform> GetOrderedTransforms(Transform t, List<Transform> inList, List<Transform> outList) {
		if(inList.Contains(t)) {
			outList.Add(t);
		}
		for(int i=0; i<t.childCount; i++) {
			GetOrderedTransforms(t.GetChild(i), inList, outList);
		}
		return outList;
	}

	private GameObject CreatePlaceholder() {
		GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		placeholder.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
		return placeholder;
	}

	private GameObject CreateGeometry(URDFData.Geometry geometryData) {
		GameObject Object = null;
		switch(geometryData.Type) {
		case URDFData.GeometryType.Box:
			URDFData.Box box = (URDFData.Box)geometryData;
			Object = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Object.transform.localScale = ROSToUnity(box.Size);
			break;

		case URDFData.GeometryType.Cylinder:
			URDFData.Cylinder cylinder = (URDFData.Cylinder)geometryData;
			Object = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			Object.transform.localScale = ROSToUnity(new Vector3(2f*cylinder.Radius, 2f*cylinder.Radius, cylinder.Length/2f));
			break;

		case URDFData.GeometryType.Sphere:
			URDFData.Sphere sphere = (URDFData.Sphere)geometryData;
			Object = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Object.transform.localScale = ROSToUnity(new Vector3(2f*sphere.Radius, 2f*sphere.Radius, 2f*sphere.Radius));
			break;

		case URDFData.GeometryType.Mesh:
			URDFData.Mesh mesh = (URDFData.Mesh)geometryData;
			//Debug.Log("Loading Mesh " + mesh.Path);
			GameObject model = AssetDatabase.LoadAssetAtPath(mesh.Path, (typeof(GameObject))) as GameObject;
			if(model != null) {
				Object = Instantiate(model) as GameObject;
				Object.transform.localPosition = Object.transform.localPosition;
				Object.transform.localRotation = Object.transform.localRotation;
				Object.transform.localScale = Vector3.Scale(Object.transform.localScale, mesh.Scale);

				Object.transform.RotateAround(Vector3.zero, Vector3.right, MeshOrientation.x);
				Object.transform.RotateAround(Vector3.zero, Vector3.up, MeshOrientation.y);
				Object.transform.RotateAround(Vector3.zero, Vector3.forward, MeshOrientation.z);

				Object.name = model.name;
			} else {
				Output += "Failed to import model '" + mesh.Path + "'. Creating placeholder instead.\n";
				Errors += 1;
				Object = CreatePlaceholder();
			}

			break;

		case URDFData.GeometryType.Empty:
			Object = new GameObject();
			break;
		}

		if(Object.GetComponent<Collider>() != null) {
			DestroyImmediate(Object.GetComponent<Collider>());
		}

		return Object;
	}

	private Transform FindTransformByName(List<Transform> transforms, string name) {
		return transforms.Find(x => x.name.Equals(name));
	}

	private Vector3 ROSToUnity(Vector3 ROSVec) {
		return new Vector3(-ROSVec.y, ROSVec.z, ROSVec.x);
	}

	void OnGUI () {
		Path = EditorGUILayout.TextField("Path", Path);
		MeshOrientation = EditorGUILayout.Vector3Field("Mesh Orientation", MeshOrientation);
		if(GUILayout.Button("Import Model")) {
			Import();
		}
	}
}