#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
// using Random = UnityEngine.Random;
// ReSharper disable CompareOfFloatsByEqualityOperator
namespace FTG
{
	//this generator uses two different representations: A nested, hierarchical one, where each element holds a reference to its children, and a flat one, with several lists containing a type of element. Using the flat representation simplifies a lot of operations, while the hierarchical one is useful only sometimes

	[AddComponentMenu("FTG/Generator", 1)]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
	[ExecuteAlways]
	// ReSharper disable once InconsistentNaming
	public class FTGenerator : MonoBehaviour
	{

		[SerializeField]
		private MeshFilter filterReference;
		[SerializeField]
		private MeshRenderer meshRenderer;
		[SerializeField]
		private MeshCollider meshCollider;
		[SerializeReference]
		internal List<Branch> branches = new List<Branch>();
		[SerializeReference]
		internal List<Leaves> leaves = new List<Leaves>();
		[SerializeReference]
		internal List<SDFLeaves> sdfLeaves = new List<SDFLeaves>();
		[SerializeReference]
		internal List<SplineLeaf> splineLeaves = new List<SplineLeaf>();
		[SerializeReference]
		internal List<CustomMesh> customMeshes = new List<CustomMesh>();
		[SerializeField]
		private int vertexCount;
		[SerializeField]
		internal bool renderSplineWithThirdMaterial;
		[SerializeField]
		internal string meshName;
		private List<Vector3> verticesMesh;
		private List<int> trianglesMesh;
		private List<Vector3> normalsMesh;
		private List<Vector2> uvMesh;
		[SerializeField]
		private Material[] defaultMaterials = new Material[3];
		[SerializeField]
		private Material billboardNormals;
		[SerializeField]
		internal int numberOfLoDs;
		[SerializeField]
		internal int previewLevel;

		[SerializeField]
		internal int meshColliderLevel;
		[SerializeField]
		internal bool generating;
		[SerializeReference]
		internal BillboardCreator billboard;
		[SerializeReference]
		internal BillboardSettings billboardSettings;
		[FormerlySerializedAs("spherizeNormals")]
		[SerializeField]
		internal float overrideNormals = 1;
		private GameObject placeholderPrefab;
		private static readonly int mainTex = Shader.PropertyToID("_MainTex");
		private static readonly int bumpMap = Shader.PropertyToID("_BumpMap");
		private string asset;
		private const string billboardmaterialPath = "_BillboardMaterial.mat";
		private const string pngString = ".png";
		private const string billboardrendererString = "_BillboardRenderer";
		private const string billboardWithoutNormalsString = "FTG/FTG-Billboard Without Normals";
		private const string colliderString = "Collider";
		private const string billboardrendererAsset = "_BillboardRenderer.asset";
		private const string lodString = "_LoD";
		private const string billboardatlas = "_BillboardAtlas";
		private const string billboardatlasnormals = "_BillboardAtlasNormals";
		private const string prefab = ".prefab";
		private const string exportPath = "Assets/Exported FTG Models/";

		public int VertexCount
		{
			get
			{
				return vertexCount;
			}
			private set
			{
				vertexCount = value;
			}
		}
		internal void AddLoD()
		{
			numberOfLoDs++;
			foreach (var item in branches)
			{
				item.AddLoD();
			}
			foreach (var item in splineLeaves)
			{
				item.AddLoD();
			}

			foreach (var item in sdfLeaves)
			{
				item.AddLoD();
			}

			foreach (var item in customMeshes)
			{
				item.AddLoD();
			}

			foreach (var item in leaves)
			{
				item.AddLoD();
			}
		}
		internal void DeleteLoD()
		{
			numberOfLoDs--;
		}

		//the menu that allows you to create a new gameobjecct already set up
		[MenuItem("GameObject/FTG/Generator", false, 1)]
		static void CreateCustomGameObject(MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = new GameObject("FTGenerator");

			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;

			go.AddComponent<FTGenerator>();
		}

		//on first setting up the script or manually clicking reset
		void Reset()
		{
			billboard = new BillboardCreator();

			meshName = "Plant";
			branches.Clear();
			Branch trunk = new Branch
			{
				depth = 0,
				ID = 0,
				parent = null
			};
			BezierSpline curve = new BezierSpline
			{
				name = "Trunk"
			};
			trunk.curve = curve;
			curve.generator = transform;
			curve.curveColor = Color.white;
			numberOfLoDs = 4;
			generating = false;
			previewLevel = 0;
			branches.Add(trunk);
			FixReferences();
		}
		private void Awake()
		{
			// Debug.Log("awake");
			// FixReferences();
		}
		private void OnValidate()
		{
			Generate();
		}
		[ContextMenu("Fix References")]
		public void FixReferences()
		{
//			Debug.Log("FTG: Fixing references, probaly after an update");
			meshCollider = GetComponent<MeshCollider>();
			meshCollider.convex = true;
			filterReference = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();

			defaultMaterials[0] = Resources.Load<Material>("Materials/FTG-Wood");
			defaultMaterials[1] = Resources.Load<Material>("Materials/FTG-Leaves");
			defaultMaterials[2] = Resources.Load<Material>("Materials/FTG-SplineLeaves");

			billboardNormals = Resources.Load<Material>("Materials/FTG-BillboardNormal");
			placeholderPrefab = Resources.Load<GameObject>("Prefabs/FTG-Sphere");

			//add materials if there arent any
			if (meshRenderer.sharedMaterials[0] == null)
			{
				meshRenderer.material = defaultMaterials[0];
			}
			else
			{
				for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
				{
					defaultMaterials[i] = meshRenderer.sharedMaterials[i];
				}
			}

			billboardSettings = Resources.Load<BillboardSettings>("Settings/Billboard Default Settings");
			Resources.UnloadUnusedAssets();
			FixTransforms();
			Generate(meshColliderLevel, true);
			Generate();
		}

		private void FixTransforms()
		{
			foreach (var item in branches)
			{
				item.curve.generator = transform;
			}
			foreach (var item in splineLeaves)
			{
				item.curve.generator = transform;
			}
		}


		//Called everytime there is a geometry change, everything is regenerated
		internal Mesh Generate(int level = -1, bool generateCollider = false)
		{
			//you can force a level, for exporting
			if (level == -1)
				level = previewLevel;
			FixTransforms();
			SetCorrectMaterials(level);
			UpdateAllOffsets();

			//It is divided into different meshes to allow for several different materials
			Mesh branchMesh = new Mesh();
			verticesMesh = new List<Vector3>();
			trianglesMesh = new List<int>();
			uvMesh = new List<Vector2>();
			branchMesh.name = "Wood";
			branches[0].offset = Vector3.zero;
			normalsMesh = new List<Vector3>();
			//generate all branches
			for (int i = 0; i < branches.Count; i++)
			{
				GenerateBranch(branches[i], branches[i].offset, level);
			}

			branchMesh.vertices = verticesMesh.ToArray();
			branchMesh.triangles = trianglesMesh.ToArray();
			branchMesh.uv = uvMesh.ToArray();
			branchMesh.normals = normalsMesh.ToArray();
			branchMesh.RecalculateTangents();
			VertexCount = verticesMesh.Count;

			//you can return just the mesh collider and stop here
			if (generateCollider)
			{
				meshCollider.sharedMesh = branchMesh;
				return branchMesh;
			}

			//does not include spline leaves
			Mesh leafMesh = new Mesh();
			verticesMesh = new List<Vector3>();
			trianglesMesh = new List<int>();
			uvMesh = new List<Vector2>();
			normalsMesh = new List<Vector3>();
			leafMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			for (int i = 0; i < leaves.Count; i++)
			{
				GenerateLeaves(leaves[i], level);
			}

			for (int i = 0; i < sdfLeaves.Count; i++)
			{
				GenerateSDFLeaves(sdfLeaves[i], level);
			}

			leafMesh.vertices = verticesMesh.ToArray();
			leafMesh.triangles = trianglesMesh.ToArray();
			leafMesh.uv = uvMesh.ToArray();
			leafMesh.normals = normalsMesh.ToArray();
			leafMesh.RecalculateTangents();
			VertexCount += verticesMesh.Count;

			//spline leaves
			Mesh splineLeafMesh = new Mesh();
			verticesMesh = new List<Vector3>();
			trianglesMesh = new List<int>();
			uvMesh = new List<Vector2>();

			for (int i = 0; i < splineLeaves.Count; i++)
			{
				GenerateSplineLeaf(splineLeaves[i], level);
			}

			splineLeafMesh.vertices = verticesMesh.ToArray();
			splineLeafMesh.triangles = trianglesMesh.ToArray();
			splineLeafMesh.uv = uvMesh.ToArray();
			splineLeafMesh.RecalculateNormals();
			splineLeafMesh.RecalculateTangents();
			VertexCount += verticesMesh.Count;

			GenerateCustomMesh();

			//by combining 3 different meshes we can easily create a single mesh with 3 different materials
			CombineInstance[] combineInstances;
			if (!renderSplineWithThirdMaterial)
			{
				CombineInstance[] combineInstancesLeaves = new CombineInstance[2];
				combineInstancesLeaves[0].mesh = leafMesh;
				combineInstancesLeaves[1].mesh = splineLeafMesh;
				combineInstancesLeaves[0].subMeshIndex = 0;
				combineInstancesLeaves[1].subMeshIndex = 0;

				Mesh combinedLeaves = new Mesh();

				combinedLeaves.CombineMeshes(combineInstancesLeaves, !renderSplineWithThirdMaterial, false);

				combineInstances = new CombineInstance[2];

				combineInstances[0].mesh = branchMesh;
				combineInstances[1].mesh = combinedLeaves;
				combineInstances[0].subMeshIndex = 0;
				combineInstances[1].subMeshIndex = 0;
			}
			else
			{
				combineInstances = new CombineInstance[3];

				combineInstances[0].mesh = branchMesh;
				combineInstances[1].mesh = leafMesh;
				combineInstances[2].mesh = splineLeafMesh;
				combineInstances[0].subMeshIndex = 0;
				combineInstances[1].subMeshIndex = 0;
				combineInstances[2].subMeshIndex = 0;
			}

			Mesh combined = filterReference.mesh = new Mesh();
			combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			combined.name = meshName;
			combined.CombineMeshes(combineInstances, false, false, false);
			return combined;
		}

		private Vector3 CalculatePoint(float t, BezierSpline curve, Vector3 offset)
		{
			return transform.InverseTransformPoint(curve.GetPoint(t) + offset);
		}

		private void GenerateBranch(Branch branch, Vector3 positionOffset, int level)
		{
			if (!branch.render[level])
				return;
			// branch.dirty = false;
			// ReSharper disable once LocalVariableHidesMember
			int vertexCount = branch.GetVertexCount(level);
			int pointCount = branch.GetDivisions(level);

			Vector3[] vertices = new Vector3[vertexCount];
			Vector2[] uv = new Vector2[vertexCount];
			Quaternion previousRotation = Quaternion.identity;
			Vector3 previousDirection = Vector3.up;
			Vector3 randomOffset = Vector3.zero;
			Vector3[] normals = new Vector3[vertexCount];
			Random.InitState(branch.seed);
			//for every ring
			for (int i = 0, y = 0; y < pointCount; y++)
			{
				//get what t from 0 to 1 is this located in
				float t = y / (pointCount - 1f);
				//get the actual position of that t
				Vector3 point = CalculatePoint(t, branch.curve, (positionOffset));
				//get the radious of this ring
				float radius = branch.curve.GetRadius(t);
				//get local direction of the curve
				Vector3 direction = branch.curve.GetDirection(t);
				//if this  isn't the first ring, calculate random offset
				if (y != 0)
				{
					randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
					randomOffset = Vector3.ProjectOnPlane(randomOffset, direction);
				}
				//rotating the previous rotation solves every continuity problem, it makes sure that the bending angle depends on the previous one and doesnt create angle discontinuities in weird bends
				Quaternion rotation = Quaternion.FromToRotation(previousDirection, direction) * previousRotation;
				previousRotation = rotation;
				previousDirection = direction;

				//This causes continuity problems
				// rotation = Quaternion.FromToRotation(Vector3.up, direction);

				//for every vertex in the ring
				for (int x = 0; x <= branch.GetSides(level); x++, i++)
				{
					//the the position along a circle
					float angle = 360 / (float)branch.GetSides(level) * (x);
					vertices[i] = new Vector3(radius * Mathf.Cos(Mathf.Deg2Rad * (angle)), 0, radius * Mathf.Sin(Mathf.Deg2Rad * (angle)));
					//get the correct magnitude vector
					vertices[i] = MultiplyByIndex(vertices[i], branch.curve.GetSize(t));
					//rotate the vector by the curve rotation and the offset it by the position of the ring and by the noise
					vertices[i] = rotation * vertices[i] + point + randomOffset * branch.noiseAmount;
					//easy uvs
					uv[i] = new Vector2((x) / ((float)(branch.GetSides(level))), y / (float)pointCount);

					normals[i] = (vertices[i] - (point + randomOffset * branch.noiseAmount)).normalized;
					// normals[i] = Vector3.down;
				}
			}
			normalsMesh.AddRange(normals);
			int vertexOffset = verticesMesh.Count;
			verticesMesh.AddRange(vertices);
			uvMesh.AddRange(uv);

			int[] triangles = new int[(branch.GetSides(level) + 1) * (pointCount) * 6];
			int j = 0;
			int finalCount = 0;

			//Enclosed mesh but the last strip
			for (int ti = 0, vi = 0; j < (pointCount - 1); j++, vi++)
			{
				for (int i = 0; i < branch.GetSides(level); i++, ti += 6, vi++)
				{
					triangles[ti] = vi + vertexOffset;
					triangles[ti + 1] = vi + branch.GetSides(level) + 1 + vertexOffset;
					triangles[ti + 2] = vi + 1 + vertexOffset;
					triangles[ti + 3] = vi + 1 + vertexOffset;
					triangles[ti + 4] = vi + branch.GetSides(level) + 1 + vertexOffset;
					triangles[ti + 5] = vi + branch.GetSides(level) + 1 + 1 + vertexOffset;
				}
				finalCount = ti;
			}

			//upper cap
			int aux = vertexCount - 2;
			for (int i = 0, count = 0; i < (branch.GetSides(level) - 2); i++, count += 3)
			{
				triangles[finalCount + count] = vertexCount - 1 + vertexOffset;
				triangles[finalCount + 1 + count] = aux + vertexOffset;
				triangles[finalCount + 2 + count] = aux - 1 + vertexOffset;
				aux--;
			}

			trianglesMesh.AddRange(triangles);
		}

		private void GenerateLeaves(Leaves leaf, int level)
		{
			if (!leaf.render[level])
				return;
			// leaf.dirty = false;

			//this calculates the control point position
			float t = Mathf.Lerp(leaf.range.x, leaf.range.y, 0.5f);
			Vector3 point = CalculatePoint(t, leaf.parent.curve, leaf.parent.offset);
			leaf.averagePosition = point;

			Random.InitState(leaf.seed);
			//for every leaf in this LoD
			for (int i = 0; i < leaf.GetNumber(level); i++)
			{
				if (leaf.GetNumber(level) == 1)
					t = Mathf.Lerp(leaf.range.x, leaf.range.y, 0.5f);
				else
					t = Mathf.Lerp(leaf.range.x, leaf.range.y, i / (float)(leaf.GetNumber(level) - 1));

				//calculate the point in the branch
				point = CalculatePoint(t, leaf.parent.curve, leaf.parent.offset);
				//direction at that point
				Vector3 direction = leaf.parent.curve.GetDirection(t);
				//rotation acording to the settings
				Quaternion rotation = (leaf.rotationSpace == Leaves.RotationDirection.Local ? Quaternion.FromToRotation(Vector3.up, direction) : Quaternion.identity);
				rotation = transform.rotation * rotation * Quaternion.Euler(leaf.preferredRotation + new Vector3(Random.Range(-leaf.rotationRandomness.x / 2, leaf.rotationRandomness.x / 2), Random.Range(-leaf.rotationRandomness.y / 2, leaf.rotationRandomness.y / 2), Random.Range(-leaf.rotationRandomness.z / 2, leaf.rotationRandomness.z / 2)));

				//create the actual plane with this settings
				CreatePlane(point + rotation * Vector3.forward * (leaf.offsetFromBranch * leaf.parent.curve.GetRadius(t) * 10), rotation, leaf.GetSize(level) * leaf.sizeVariation.Evaluate(t), Vector3.back, length: leaf.length);
			}
		}
		// static float Remap(float value, float initMin, float initMax, float newMin, float newMax)
		// {
		// 	return newMin + (value - initMin) * (newMax - newMin) / (initMax - initMin);
		// }
		private void GenerateSDFLeaves(SDFLeaves leaf, int level)
		{
			if (!leaf.render[level])
				return;
			// leaf.dirty = false;
			Random.InitState(leaf.seed);

			//central point
			Vector3 point = CalculatePoint(leaf.Position, leaf.parent.curve, leaf.parent.offset);
			Vector3 faceNormal;
			for (int i = 0; i < leaf.GetNumber(level); i++)
			{
				Vector3 sdfPos;
				//all of them are values between 0 and 1
				float finalT = 1;
				float finalT01;
				if (!leaf.shell)
				{
					float heighT;
					float finalPos;
					int loops = 0;

					//brute force method to generate a random number from an arbitrary distribution, there is probably a deterministic way but...
					do
					{
						loops++;
						finalT = Random.Range(0, 1f);
						heighT = Random.Range(0f, 1f);
						finalPos = leaf.density.Evaluate(finalT);

						if (finalPos >= 1)
						{
							break;
						}
						//give up after a while
						if (loops > 200)
						{
							finalT = 1;
							break;
						}
					}
					while (finalPos <= heighT);

				}
				finalT01 = finalT;
/*
				finalT = leaf.Radius * finalT;
*/

				Vector3 randomPos;
				float value;
				Func<Vector3, float> func;
				//offset from position in branch, shapes defined by SDFs
				switch (leaf.formType)
				{

					default:
					case SDFLeaves.FormType.Sphere:
						//sdf
						float remapCut = Mathf.Lerp(0, Mathf.PI, leaf.cut);
						Vector2 cap = new Vector2(Mathf.Sin(remapCut), Mathf.Cos(remapCut));
						func = delegate(Vector3 pos) { return FTGSDF.SolidAngle(pos, cap, (1f - leaf.smoothness)); };

						//making sure the initial points are inside the sdf alleviates the distribution problem
						randomPos = FTGSDF.InitInsideSDF(func, leaf.smoothness, out value);

						faceNormal = FTGSDF.CalculateNormals(randomPos, func);

						randomPos -= faceNormal * value;
						//compensate smoothness
						randomPos += leaf.smoothness * faceNormal;
						sdfPos = randomPos * (leaf.Radius * finalT01);

						break;

					case SDFLeaves.FormType.Cube:
						//sdf
						func = delegate(Vector3 pos) { return FTGSDF.Box(pos, Vector3.one * (1f - leaf.smoothness)); };

						//making sure the initial points are inside the sdf alleviates the distribution problem
						randomPos = FTGSDF.InitInsideSDF(func, leaf.smoothness, out value);

						faceNormal = FTGSDF.CalculateNormals(randomPos, func);

						randomPos -= faceNormal * value;
						//compensate smoothness
						randomPos += leaf.smoothness * faceNormal;
						sdfPos = randomPos * (leaf.Radius * finalT01);

						break;

					case SDFLeaves.FormType.Cone:
						//sdf
						func = delegate(Vector3 pos) { return FTGSDF.CappedCone(pos, leaf.cut * (1 - leaf.smoothness), (1 - leaf.smoothness), (1 - leaf.smoothness) * (1 - leaf.cut)); };

						//making sure the initial points are inside the sdf alleviates the distribution problem
						randomPos = FTGSDF.InitInsideSDF(func, leaf.smoothness, out value);

						faceNormal = FTGSDF.CalculateNormals(randomPos, func);

						randomPos -= faceNormal * value;
						//compensate smoothness
						randomPos += leaf.smoothness * faceNormal;
						sdfPos = randomPos * (leaf.Radius * finalT01);

						break;

					case SDFLeaves.FormType.Cylinder:
						//sdf
						func = delegate(Vector3 pos) { return FTGSDF.Cylinder(pos, (1 - leaf.smoothness), (1 - leaf.smoothness)); };

						//making sure the initial points are inside the sdf alleviates the distribution problem
						randomPos = FTGSDF.InitInsideSDF(func, leaf.smoothness, out value);

						faceNormal = FTGSDF.CalculateNormals(randomPos, func);

						randomPos -= faceNormal * value;
						//compensate smoothness
						randomPos += leaf.smoothness * faceNormal;
						sdfPos = randomPos * (leaf.Radius * finalT01);


						break;

					case SDFLeaves.FormType.Pyramid:
						//sdf
						func = delegate(Vector3 pos) { return FTGSDF.Pyramid(pos, (1 - leaf.smoothness)); };

						//making sure the initial points are inside the sdf alleviates the distribution problem
						randomPos = FTGSDF.InitInsideSDF(func, leaf.smoothness, out value);

						faceNormal = FTGSDF.CalculateNormals(randomPos, func);

						randomPos -= faceNormal * value;
						//compensate smoothness
						randomPos += leaf.smoothness * faceNormal;
						sdfPos = randomPos * (leaf.Radius * finalT01);

						break;

					case SDFLeaves.FormType.Torus:
						//SDF 
						float remapSection = Mathf.Lerp(0, Mathf.PI, leaf.cut);
						func = vec3 => FTGSDF.CappedTorus(vec3, new Vector2(Mathf.Sin(remapSection), Mathf.Cos(remapSection)), (0.75f - leaf.smoothness), leaf.innerRadius * finalT01);
						//making sure the initial points are inside the sdf alleviates the distribution problem
						randomPos = FTGSDF.InitInsideSDF(func, leaf.smoothness, out value);
						faceNormal = FTGSDF.CalculateNormals(randomPos, func);
						//move points to their actual position
						randomPos -= faceNormal * value;
						//compensate smoothness
						randomPos += leaf.smoothness * faceNormal;
						//scale based on radius
						sdfPos = randomPos * (leaf.Radius * 1.4f);

						break;
				}

				//rotate the position
				sdfPos = leaf.rotation * transform.rotation * MultiplyByIndex(sdfPos, leaf.scale);
				faceNormal = DivideByIndex(faceNormal, leaf.scale);
				Quaternion rotation = ((leaf.rotationSpace == SDFLeaves.RotationDirection.Local) ? Quaternion.LookRotation(leaf.overrideSphereSpace ? sdfPos : faceNormal) : Quaternion.identity) * Quaternion.Euler(leaf.preferredRotation + new Vector3(Random.Range(-leaf.rotationRandomness.x / 2, leaf.rotationRandomness.x / 2), Random.Range(-leaf.rotationRandomness.y / 2, leaf.rotationRandomness.y / 2), Random.Range(-leaf.rotationRandomness.z / 2, leaf.rotationRandomness.z / 2)));

				CreatePlane(sdfPos + point + leaf.positionOffset, rotation, leaf.GetSize(level), leaf.useSphereNormals ? sdfPos : faceNormal, leaf.length, overrideNormals);
			}
		}

		private void GenerateSplineLeaf(SplineLeaf leaf, int level)
		{
			//works like a simplified branch

			if (!leaf.render[level])
				return;
			// leaf.dirty = false;
			// ReSharper disable once LocalVariableHidesMember
			int vertexCount = leaf.GetVertexCount(level);
			int pointCount = leaf.GetDivisions(level);

			Vector3[] vertices = new Vector3[vertexCount];
			Vector2[] uv = new Vector2[vertexCount];
			Transform transform1 = transform;
			Vector3 transform1Position = transform1.position;
			//vertices
			for (int i = 0, y = 0; y < pointCount; y++)
			{
				Vector3 point = CalculatePoint(y / ((float)pointCount - 1), leaf.curve, (transform1).TransformPoint(leaf.curve.Offset) - transform1Position);
				float radius = leaf.curve.GetRadius((y / ((float)pointCount - 1)));
				// Vector3 direction = leaf.curve.GetDirection(y / ((float)pointCount - 1));

				for (int x = 0; x < 2; x++, i++)
				{
					float angle = 360 / (float)2 * x;
					vertices[i] = new Vector3(radius * Mathf.Cos(Mathf.Deg2Rad * (angle)), 0, 0);
					vertices[i] = leaf.rotation * vertices[i] + point;
					uv[i] = new Vector2(x, y / (float)pointCount);
				}
			}

			int vertexOffset = verticesMesh.Count;
			verticesMesh.AddRange(vertices);
			uvMesh.AddRange(uv);

			int[] triangles = new int[(pointCount - 1) * 6];
			int j = 0;

			//mesh
			for (int ti = 0, vi = 0; j < (pointCount - 1); j++, vi++)
			{
				for (int i = 0; i < 1; i++, ti += 6, vi++)
				{
					triangles[ti] = vi + vertexOffset;
					triangles[ti + 1] = vi + 1 + vertexOffset;
					triangles[ti + 2] = vi + 2 + vertexOffset;
					triangles[ti + 3] = vi + 1 + vertexOffset;
					triangles[ti + 4] = vi + 3 + vertexOffset;
					triangles[ti + 5] = vi + 2 + vertexOffset;
				}
			}

			trianglesMesh.AddRange(triangles);
		}

		private void GenerateCustomMesh()
		{
			// int vertexCount = 0;
			foreach (var item in customMeshes)
			{
				// item.dirty = false;
				if (!item.render[previewLevel] && (item.instancedObject != null))
				{
					item.instancedObject.SetActive(false);
					// vertexCount = 0;
				}
				else
				{
					item.meshPosition = item.parent.curve.GetPoint(item.Position) + item.parent.offset;
					if (item.instancedObject == null)
					{
						item.instancedObject = (GameObject)PrefabUtility.InstantiatePrefab(item.prefabObject, transform);
						// item.instancedObject = Instantiate(item.prefabObject, transform);
						item.instancedObject.transform.localPosition = transform.InverseTransformPoint(item.meshPosition + item.positionOffset);
						item.instancedObject.transform.localRotation = item.rotation;
						item.instancedObject.transform.localScale = item.scale;
						item.instancedObject.SetActive(true);
					}
					else
					{
						item.instancedObject.transform.localPosition = transform.InverseTransformPoint(item.meshPosition + item.positionOffset);
						item.instancedObject.transform.localRotation = item.rotation;
						item.instancedObject.transform.localScale = item.scale;
					}
					//To Do: get all meshes and count the vertices?
				}
			}
			//right now we are not returning the vertex count because there are plenty of special cases
		}

		public void DestroyCustomMesh(CustomMesh mesh)
		{
			//only way to destroy outside of play, must be very careful
			//will not work if the generator is inside a prefab
			DestroyImmediate(mesh.instancedObject);
		}

		//creates a single plane (2 tris) at a certain position and rotation
		private void CreatePlane(Vector3 position, Quaternion rotation, float size, Vector3 normal, float length = 1, float spherize = 0)
		{
			Vector3[] vertices = new Vector3[4];
			Vector2[] uv = new Vector2[4];
			Vector3[] normals = new Vector3[4];

			vertices[0] = new Vector3(0.5f, 0, -length / 2f) * size;
			vertices[1] = new Vector3(-0.5f, 0, -length / 2f) * size;
			vertices[2] = new Vector3(0.5f, 0, length / 2f) * size;
			vertices[3] = new Vector3(-0.5f, 0, length / 2f) * size;

			vertices[0] = rotation * vertices[0] + position;
			vertices[1] = rotation * vertices[1] + position;
			vertices[2] = rotation * vertices[2] + position;
			vertices[3] = rotation * vertices[3] + position;

			uv[0] = new Vector2(0, 0);
			uv[1] = new Vector2(1, 0);
			uv[2] = new Vector2(0, 1);
			uv[3] = new Vector2(1, 1);

			for (int i = 0; i < normals.Length; i++)
			{
				normals[i] = Vector3.Lerp(rotation * Vector3.up, normal, spherize);
			}

			normalsMesh.AddRange(normals);

			int vertexOffset = verticesMesh.Count;
			verticesMesh.AddRange(vertices);
			uvMesh.AddRange(uv);

			int[] triangles = new int[6];

			triangles[0] = 0 + vertexOffset;
			triangles[1] = 1 + vertexOffset;
			triangles[2] = 2 + vertexOffset;
			triangles[3] = 1 + vertexOffset;
			triangles[4] = 3 + vertexOffset;
			triangles[5] = 2 + vertexOffset;

			trianglesMesh.AddRange(triangles);
		}

		internal void AddBranch(Branch parent)
		{
			//create the new branch
			Branch childAppendage = new Branch();

			//add it to the flat list of branches
			branches.Add(childAppendage);
			//set its information
			childAppendage.ID = branches.Count - 1;
			Debug.Log("Created branch: " + (branches.Count - 1));
			childAppendage.parent = parent;
			childAppendage.depth = parent.depth + 1;
			//add a reference to the parent
			parent.appendages.Add(childAppendage);
			childAppendage.IDInBranch = parent.appendages.Count - 1;
			// more settings
			BezierSpline curve = new BezierSpline
			{
				name = "rama" + (branches.Count - 1)
			};
			childAppendage.curve = curve;
			curve.generator = transform;
			curve.Offset = parent.curve.GetPoint(0.5f) + parent.curve.Offset;

			switch (childAppendage.depth)
			{
				case 0:
					curve.curveColor = Color.white;
					break;
				case 1:
					curve.curveColor = Color.black;
					break;
				case 2:
					curve.curveColor = Color.red;
					break;
				case 3:
				default:
					curve.curveColor = Color.magenta;
					childAppendage.render[2] = false;
					break;
			}
		}

		internal void AddLeaves(Branch parent)
		{
			Leaves leaf;
			if (leaves.Count > 0)
				leaf = new Leaves(leaves[leaves.Count - 1]);
			else
				leaf = new Leaves();
			leaf.type = AppendageType.Leaves;
			leaves.Add(leaf);
			leaf.ID = leaves.Count - 1;
			leaf.parent = parent;
			parent.appendages.Add(leaf);
			leaf.IDInBranch = parent.appendages.Count - 1;
			leaf.depth = parent.depth + 1;
		}

		internal void AddSDFLeaves(Branch parent)
		{
			SDFLeaves leaf;
			if (sdfLeaves.Count > 0)
				leaf = new SDFLeaves(sdfLeaves[sdfLeaves.Count - 1]);
			else
				leaf = new SDFLeaves();
			leaf.type = AppendageType.SDFLeaves;
			sdfLeaves.Add(leaf);
			leaf.ID = sdfLeaves.Count - 1;
			leaf.parent = parent;
			parent.appendages.Add(leaf);
			leaf.IDInBranch = parent.appendages.Count - 1;
			leaf.depth = parent.depth + 1;
		}

		internal void AddSplineLeaf(Branch parent)
		{
			SplineLeaf leaf = new SplineLeaf
			{
				type = AppendageType.SplineLeaf
			};
			splineLeaves.Add(leaf);
			leaf.ID = splineLeaves.Count - 1;
			leaf.parent = parent;
			parent.appendages.Add(leaf);
			leaf.IDInBranch = parent.appendages.Count - 1;
			leaf.rotation = Quaternion.FromToRotation(Vector3.up, parent.curve.GetDirection(0.5f));
			leaf.depth = parent.depth + 1;

			leaf.curve = new BezierSpline
			{
				name = "Spline Leaf" + (parent.appendages.Count - 1),
				generator = transform,
				Offset = parent.curve.GetPoint(0.5f) + parent.curve.Offset,
				// leaf.curve.id = leaf.ID;
				curveColor = Color.yellow
			};
		}

		internal void DuplicateSplineLeaf(SplineLeaf reference)
		{
			SplineLeaf leaf = new SplineLeaf(reference);
			splineLeaves.Add(leaf);
			// Debug.Log(reference.ID);
			leaf.parent = reference.parent;
			leaf.ID = splineLeaves.Count - 1;
			// Debug.Log(leaf.ID);
			leaf.parent.appendages.Add(leaf);
			leaf.IDInBranch = leaf.parent.appendages.Count - 1;
			// leaf.curve.id = leaf.ID;
			leaf.depth = reference.depth;
		}

		internal void AddCustomMesh(Branch parent)
		{
			CustomMesh mesh = new CustomMesh
			{
				prefabObject = placeholderPrefab,
				type = AppendageType.CustomMesh,
				parent = parent
			};

			customMeshes.Add(mesh);
			mesh.ID = customMeshes.Count - 1;
			parent.appendages.Add(mesh);
			mesh.depth = parent.depth + 1;
			mesh.IDInBranch = parent.appendages.Count - 1;
		}

		internal void DuplicateCustomMesh(CustomMesh reference)
		{
			CustomMesh mesh = new CustomMesh
			{
				type = AppendageType.CustomMesh
			};
			customMeshes.Add(mesh);
			mesh.ID = customMeshes.Count - 1;
			mesh.parent = reference.parent;
			mesh.parent.appendages.Add(mesh);
			mesh.IDInBranch = mesh.parent.appendages.Count - 1;

			mesh.prefabObject = reference.prefabObject;
			mesh.Position = reference.Position;
			mesh.positionOffset = reference.positionOffset;
			mesh.meshPosition = reference.meshPosition;
			mesh.rotation = reference.rotation;
			mesh.scale = reference.scale;

			mesh.depth = reference.depth;
		}

		internal void DeleteBranch(Branch branch)
		{
			// Debug.Log(branch.curve.name + " has " + branch.appendages.Count + " appendages");

			//There are several references, we can't just remove it from the list
			int initialCount = branch.appendages.Count;
			for (int i = initialCount - 1; i >= 0; i--)
			{
				switch (branch.appendages[i].type)
				{
					case AppendageType.Branch:
					default:
						DeleteBranch(branch.appendages[i] as Branch);
						break;
					case AppendageType.Leaves:
						DeleteLeaves(branch.appendages[i] as Leaves);
						break;
					case AppendageType.SDFLeaves:
						DeleteSDFLeaves(branch.appendages[i] as SDFLeaves);
						break;
					case AppendageType.SplineLeaf:
						DeleteSplineLeaf(branch.appendages[i] as SplineLeaf);
						break;
					case AppendageType.CustomMesh:
						DeleteCustomMesh(branch.appendages[i] as CustomMesh);
						break;
				}
			}
			// Debug.Log("Removing branch: " + branch.curve.name + ", with parent: " + branch.parent.curve.name);
			branch.parent.appendages.Remove(branch);
			// Debug.Log("removed from appendages: " + success);
			// Debug.Log("Parent has now: " + branch.parent.appendages.Count + " appendages");
			branches.Remove(branch);

			// Debug.Log("removed from Branches: " + success);
			// Debug.Log("There are: " + branches.Count + " branches");
			for (int i = 0; i < branches.Count; i++)
			{
				branches[i].ID = i;
			}
			UpdateIDInBranch(branch.parent);
		}

		//delete all references
		internal void DeleteLeaves(Leaves leaf)
		{
			leaf.parent.appendages.Remove(leaf);
			UpdateIDInBranch(leaf.parent);
			SaveLeafMaterial(AppendageType.Leaves);
			leaves.Remove(leaf);

			for (int i = 0; i < leaves.Count; i++)
			{
				leaves[i].ID = i;
			}
		}

		internal void DeleteSDFLeaves(SDFLeaves leaf)
		{
			leaf.parent.appendages.Remove(leaf);
			UpdateIDInBranch(leaf.parent);
			SaveLeafMaterial(AppendageType.SDFLeaves);
			sdfLeaves.Remove(leaf);

			for (int i = 0; i < sdfLeaves.Count; i++)
			{
				sdfLeaves[i].ID = i;
			}
		}

		internal void DeleteSplineLeaf(SplineLeaf leaf)
		{
			leaf.parent.appendages.Remove(leaf);
			UpdateIDInBranch(leaf.parent);
			SaveLeafMaterial(AppendageType.SplineLeaf);
			splineLeaves.Remove(leaf);

			for (int i = 0; i < splineLeaves.Count; i++)
			{
				splineLeaves[i].ID = i;
			}

		}

		internal void DeleteCustomMesh(CustomMesh mesh)
		{
			mesh.parent.appendages.Remove(mesh);
			UpdateIDInBranch(mesh.parent);
			if (mesh.instancedObject != null)
				DestroyImmediate(mesh.instancedObject);
			customMeshes.Remove(mesh);

			for (int i = 0; i < customMeshes.Count; i++)
			{
				customMeshes[i].ID = i;
			}
		}

		internal void UpdateIDInBranch(Branch branch)
		{
			int initialCount = branch.appendages.Count;
			for (int i = 0; i < initialCount; i++)
			{
				branch.appendages[i].IDInBranch = i;
			}
		}

		//make sure every element is correctly placed
		internal void UpdateAllOffsets()
		{
			for (int i = 0; i < branches.Count; i++)
			{
				Branch item = branches[i];
				UpdateOffset(item);
			}

			foreach (var item in splineLeaves)
			{
				UpdateOffset(item);
			}

			foreach (var item in sdfLeaves)
			{
				UpdateOffset(item);
			}

			foreach (var item in customMeshes)
			{
				UpdateOffset(item);
			}

			foreach (var item in leaves)
			{
				UpdateOffset(item);
			}
		}

		private void UpdateOffset(Appendage appendage)
		{
			Vector3 offset = Vector3.zero;

			if (appendage.depth == 0)
				return;
			Branch parentReference = appendage.parent;
			// Debug.Log("reference: " + (appendage).ToString());

			Appendage childReference = appendage;
			int loops = 0;
			while (true)
			{
				if (loops > 10)
				{
					Debug.LogError("Passed 10 loops");
					break;
				}
				// Debug.Log("Parent: " + parentReference.curve.name);
				loops++;
				offset += transform.InverseTransformPoint(parentReference.curve.GetPoint(childReference.Position));

				childReference = parentReference;
				if (childReference.depth == 0)
					break;
				parentReference = childReference.parent;
			}
			Transform transform1;
			appendage.offset = (transform1 = transform).TransformPoint(offset) - transform1.position;

			if (appendage.type == AppendageType.Branch)
				(appendage as Branch).curve.Offset = offset;
			else if (appendage.type == AppendageType.SplineLeaf)
				(appendage as SplineLeaf).curve.Offset = offset;
		}

		private void SetCorrectMaterials(int level)
		{
			bool areLeavesVisible = leaves.Count > 0;
			bool areSplinesVisible = splineLeaves.Count > 0;
			bool areSDFsVisible = sdfLeaves.Count > 0;

			//is there at least one visible
			if (areLeavesVisible)
				areLeavesVisible = leaves.Find(x => x.render[level]) != null;

			if (areSplinesVisible)
				areSplinesVisible = splineLeaves.Find(x => x.render[level]) != null;

			if (areSDFsVisible)
				areSDFsVisible = sdfLeaves.Find(x => x.render[level]) != null;


			//if there are both spline leaves and normal leaves and we want 3 materials
			if (((areLeavesVisible || areSDFsVisible) && (areSplinesVisible) && renderSplineWithThirdMaterial))
			{
				SetMaterials(3);
			}
			//if there only spline leaves and we want a third material, but we ignore it
			else if (areSplinesVisible && (!areLeavesVisible && !areSDFsVisible))
			{
				renderSplineWithThirdMaterial = false;
				SetMaterials(2);
			}
			//if (there are both spline leaves and normal leaves ) AND we want 2 materials
			else if ((areSplinesVisible) && !renderSplineWithThirdMaterial)
			{
				SetMaterials(2);
			}
			//if we only have normal leaves
			else if (areSDFsVisible || areLeavesVisible)
			{
				renderSplineWithThirdMaterial = false;
				SetMaterials(2);
			}
			//we don't have leaves
			else
			{
				renderSplineWithThirdMaterial = false;
				SetMaterials(1);
			}
		}

		public void SetMaterials(int number)
		{

			Material[] materials = new Material[number];
			for (int i = 0; i < materials.Length; i++)
			{
				// Debug.Log(i);
				if (meshRenderer.sharedMaterials != null)
					if ((meshRenderer.sharedMaterials.Length > i) && (meshRenderer.sharedMaterials[i] != null))
					{
						materials[i] = meshRenderer.sharedMaterials[i];
					}
					else
					{
						materials[i] = defaultMaterials[i];
					}
			}
			meshRenderer.materials = materials;
		}

		private void SaveLeafMaterial(AppendageType type)
		{
			switch (type)
			{
				case AppendageType.Leaves:
				default:
					if (leaves.Count == 1)
					{
						defaultMaterials[1] = meshRenderer.sharedMaterials[1];
					}
					break;
				case AppendageType.SDFLeaves:
					if (sdfLeaves.Count == 1)
					{
						defaultMaterials[1] = meshRenderer.sharedMaterials[1];
					}
					break;
				case AppendageType.SplineLeaf:

					if (((leaves.Count > 0) || (sdfLeaves.Count > 0)) && (splineLeaves.Count == 1) && renderSplineWithThirdMaterial)
					{
						defaultMaterials[2] = meshRenderer.sharedMaterials[2];
					}
					else if (splineLeaves.Count == 1)
					{
						defaultMaterials[1] = meshRenderer.sharedMaterials[1];
					}
					break;
			}
		}

		internal void SaveMesh(string newMeshName, string path, int level = 0, bool isCollider = false, bool saveToAsset = true)
		{
			if (!AssetDatabase.IsValidFolder("Assets/Exported FTG Models"))
				AssetDatabase.CreateFolder("Assets", "Exported FTG Models");
			var mesh = Generate(level, isCollider);
			mesh.name = newMeshName;

			if (saveToAsset)
				AssetDatabase.AddObjectToAsset(mesh, path);
			else
				AssetDatabase.CreateAsset(mesh, path);
			AssetDatabase.SaveAssets();
		}

		internal IEnumerator SavePrefab(string prefabName)
		{
			previewLevel = 0;
			generating = true;
			if (!AssetDatabase.IsValidFolder("Assets/Exported FTG Models"))
				AssetDatabase.CreateFolder("Assets", "Exported FTG Models");

			//we can't reference a scene mesh in a prefab, we have to save the meshes too

			asset = ".asset";
			string path = exportPath + prefabName + asset;

			if (AssetDatabase.LoadAssetAtPath<GameObject>(exportPath + prefabName + prefab) != null)
			{
				// Debug.Log("Nada");
				if (!EditorUtility.DisplayDialog("FTG: Override?", "A prefab with that name already exists. Do you want to proceed?", "Override", "Cancel"))
				{
					yield break;
				}
			}

			SaveMesh(prefabName, path, 0, saveToAsset: false);

			for (int i = 1; i < numberOfLoDs; i++)
			{
				// Debug.Log(i);
				SaveMesh(prefabName + lodString + i, path, i);
			}


			SaveMesh(prefabName + colliderString, path, meshColliderLevel, true);

			var transform1 = transform;
			GameObject parentObj = new GameObject(prefabName)
			{
				transform =
				{
					position = transform1.position,
					rotation = transform1.rotation,
					localScale = transform1.localScale
				}
			};
			Object[] obJs = AssetDatabase.LoadAllAssetsAtPath(path);
			List<Mesh> meshes = new List<Mesh>();
			for (int i = 0; i < obJs.Length; i++)
			{
				meshes.Add((Mesh)obJs[i]);
			}

			MeshCollider newCollider = parentObj.AddComponent<MeshCollider>();
			// newCollider.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Exported FTG Models/" + name + "Collider" + ".asset");
			newCollider.sharedMesh = meshes.Find(x => x.name == (prefabName + colliderString));
			newCollider.convex = true;

			LODGroup lodGroup = parentObj.AddComponent<LODGroup>();
			GameObject[] children = new GameObject[numberOfLoDs];
			LOD[] lods = new LOD[numberOfLoDs];

			for (int i = 0; i < (billboard.renderBillboard ? (numberOfLoDs - 1) : numberOfLoDs); i++)
			{

				GameObject aux = new GameObject
				{
					name = prefabName + lodString + i
				};
				children[i] = aux;
				MeshFilter auxFilter = aux.AddComponent<MeshFilter>();
				// auxFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Exported FTG Models/" + name + lodString + i + ".asset");
				if (i == 0)
				{
					auxFilter.sharedMesh = meshes.Find(x => x.name == prefabName);
				}
				else
				{
					auxFilter.sharedMesh = meshes.Find(x => x.name == (prefabName + lodString + i));
				}

				Renderer[] auxRenderer = new Renderer[1];
				auxRenderer[0] = aux.AddComponent<MeshRenderer>();
				auxRenderer[0].sharedMaterials = meshRenderer.sharedMaterials;
				aux.transform.position = parentObj.transform.position;
				aux.transform.rotation = parentObj.transform.rotation;
				aux.transform.localScale = parentObj.transform.localScale;
				aux.transform.parent = parentObj.transform;

				LOD lod = new LOD
				{
					renderers = auxRenderer,
					screenRelativeTransitionHeight = i switch
					{
						0 => 0.5f,
						1 => 0.2f,
						2 => 0.07f,
						3 => 0.02f,
						4 => 0.01f,
						_ => 0.005f
					}
				};
				lods[i] = lod;

			}
			if (billboard.renderBillboard)
			{
				int i = numberOfLoDs - 1;
				GameObject aux = new GameObject
				{
					name = prefabName + lodString + i
				};
				children[i] = aux;

				BillboardRenderer[] billboardRenderer = new BillboardRenderer[1];
				billboardRenderer[0] = aux.AddComponent<BillboardRenderer>();

				RenderBillboard(prefabName + billboardatlas);
				// RenderBillboard(prefabName + billboardatlasnormals, billboardNormals);
				//create billboard asset
				// AssetDatabase.AddObjectToAsset(billboard.CreateAsset(),path);

				AssetDatabase.CreateAsset(billboard.CreateAsset(prefabName), exportPath + prefabName + billboardrendererAsset);
				//create new billboard material instance


				AssetDatabase.CreateAsset(new Material(Shader.Find(billboardWithoutNormalsString)), exportPath + prefabName + billboardmaterialPath);
				//render both atlases

				//make sure unity fucking knows this stuff is now available
				AssetDatabase.Refresh();
				//x2
				yield return new EditorWaitForSeconds(0.1f);

				//get scene references to all of this because we are creating a prefab
				BillboardAsset billboardAsset = AssetDatabase.LoadAssetAtPath<BillboardAsset>(exportPath + prefabName + billboardrendererAsset);

				billboardAsset.name = prefabName + billboardrendererString;
				// Debug.Log("Image count: " + asset.imageCount);
				billboardRenderer[0].billboard = billboardAsset;
				billboardAsset.material = AssetDatabase.LoadAssetAtPath<Material>(exportPath + prefabName + billboardmaterialPath);
				var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(exportPath + prefabName + billboardatlas + pngString);
				// var bump = AssetDatabase.LoadAssetAtPath<Texture2D>(exportPath + prefabName + billboardatlasnormals + png);
				AssetDatabase.Refresh();

				// Debug.Log(atlas);
				billboardAsset.material.SetTexture(mainTex, atlas);
				// Debug.Log(bump);
				// billboardAsset.material.SetTexture(bumpMap, bump);

				aux.transform.position = parentObj.transform.position;
				aux.transform.rotation = parentObj.transform.rotation;
				aux.transform.localScale = parentObj.transform.localScale;
				aux.transform.parent = parentObj.transform;

				LOD lod = new LOD
				{
					// ReSharper disable once CoVariantArrayConversion
					renderers = billboardRenderer,
					screenRelativeTransitionHeight = i switch
					{
						0 => 0.5f,
						1 => 0.2f,
						2 => 0.07f,
						3 => 0.02f,
						4 => 0.01f,
						_ => 0.005f
					}
				};
				lods[i] = lod;
			}
			lodGroup.SetLODs(lods);
			lodGroup.fadeMode = LODFadeMode.CrossFade;
			lodGroup.animateCrossFading = true;
			foreach (var item in customMeshes)
			{
				item.meshPosition = item.parent.curve.GetPoint(item.Position) + item.parent.offset;
				// Debug.Log("Instantiating");
				GameObject instancedObject = (GameObject)PrefabUtility.InstantiatePrefab(item.prefabObject, parentObj.transform);
				instancedObject.transform.localPosition = transform.InverseTransformPoint(item.meshPosition + item.positionOffset);
				instancedObject.transform.localRotation = item.rotation;
				instancedObject.transform.localScale = item.scale;
			}

			PrefabUtility.SaveAsPrefabAssetAndConnect(parentObj, exportPath + prefabName + prefab, InteractionMode.UserAction, out bool success);
			Debug.Log("success: " + success);
			AssetDatabase.Refresh();
			generating = false;
			Generate();
		}

		internal void RenderBillboard(string billboardName, Material overrideMaterial = null)
		{
			billboard.settings = billboardSettings;
			filterReference.sharedMesh = Generate(0);
			filterReference.sharedMesh.RecalculateBounds();
			Material[] currentMaterials = null;
			if (overrideMaterial != null)
			{
				var sharedMaterials = meshRenderer.sharedMaterials;
				currentMaterials = sharedMaterials;
				Material[] overrideMaterials = new Material[sharedMaterials.Length];
				for (int i = 0; i < overrideMaterials.Length; i++)
				{
					overrideMaterials[i] = overrideMaterial;
				}
				meshRenderer.sharedMaterials = overrideMaterials;
			}
			billboard.Setup(filterReference.sharedMesh.bounds, transform.position);
			var bytes = billboard.Render().EncodeToPNG();
			if (overrideMaterial != null)
			{
				meshRenderer.sharedMaterials = currentMaterials;
			}
			System.IO.File.WriteAllBytes(exportPath + billboardName + pngString, bytes);
		}

		public static Vector3 MultiplyByIndex(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static Vector3 DivideByIndex(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
		}
	}
}
#endif
