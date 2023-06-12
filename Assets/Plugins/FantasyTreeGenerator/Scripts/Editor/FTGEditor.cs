#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

//bezier code based on Catlikecoding tutorial
namespace FTG
{
	[CustomEditor(typeof(FTGenerator)), CanEditMultipleObjects]
	// ReSharper disable once InconsistentNaming
	public class FTGEditor : Editor
	{
		[SerializeField]
		private FTGenerator plantGenerator;
		[SerializeField]
		private Transform handleTransform;
		[SerializeField]
		private Quaternion handleRotation;
		private const float handleSize = 0.1f;
		private const float pickSize = 0.1f;
		private const float arrowSize = 8f;

		private static readonly Color[] modeColors =
		{
			Color.white,
			Color.yellow,
			Color.blue
		};

		public int lastIDSelected;

		public AppendageType selectedType;
		[SerializeField]
		private int selectedIndex = -1;

		public enum DisplayHandles { OnlySelected, All };
		[SerializeField]
		public DisplayHandles displayHandles;


		Color defaultColor;
		private void OnEnable()
		{
			plantGenerator = target as FTGenerator;
		}

		private void Reset()
		{
			EditorStyles.label.wordWrap = true;
			defaultColor = EditorStyles.label.normal.textColor;
		}

		public override void OnInspectorGUI()
		{
			if (plantGenerator == null)
			{
				OnEnable();
				return;
			}
			EditorGUI.BeginChangeCheck();
			var meshName = EditorGUILayout.TextField("Name", plantGenerator.meshName);
			EditorGUILayout.LabelField("Number of LoDs: " + plantGenerator.numberOfLoDs);
			if (GUILayout.Button("Add LoD"))
			{
				RecordAndDirty("Add Lod", () => plantGenerator.AddLoD());
			}
			if (GUILayout.Button("Delete Last LoD"))
			{
				RecordAndDirty("Delete Lod", () => plantGenerator.DeleteLoD());
			}

			var previewLevel = EditorGUILayout.IntSlider("Preview Level", plantGenerator.previewLevel, 0, plantGenerator.numberOfLoDs - 1 - (plantGenerator.billboard.renderBillboard ? 1 : 0));
			EditorGUILayout.Space(5);
			var colliderLevel = EditorGUILayout.IntSlider(new GUIContent("Mesh Collider uses LoD Nº:", "Mesh Collider uses LoD Nº:"), plantGenerator.meshColliderLevel, 0, plantGenerator.numberOfLoDs - 1 - (plantGenerator.billboard.renderBillboard ? 1 : 0));
			EditorGUILayout.Space(5);
			var renderBillBoard = EditorGUILayout.Toggle("Last LoD is Billboard", plantGenerator.billboard.renderBillboard);
			var billboardSettings = (BillboardSettings)EditorGUILayout.ObjectField("Billboard Settings", plantGenerator.billboardSettings, typeof(BillboardSettings), false);


			if (GUILayout.Button("Save as Prefab"))
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(plantGenerator.SavePrefab(meshName));
			}
			EditorGUILayout.LabelField("Vertex count: " + plantGenerator.VertexCount);
			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("Visualization");

			var display = (DisplayHandles)EditorGUILayout.EnumPopup("Display Handles", displayHandles);
			EditorGUILayout.LabelField(selectedType.ToString());
			EditorGUILayout.Space(10);

			if (EditorGUI.EndChangeCheck())
			{
				RecordAndDirty("Changed mesh options", () =>
				{
					plantGenerator.previewLevel = previewLevel;
					plantGenerator.meshName = meshName;
					displayHandles = display;
					plantGenerator.meshColliderLevel = colliderLevel;
					plantGenerator.billboard.renderBillboard = renderBillBoard;
					plantGenerator.billboardSettings = billboardSettings;
				});
			}
			if (previewLevel != 0)
			{
				ChangeStylePreview();
				EditorGUILayout.LabelField("Red fields support different values per LoD");
				CleanStylePreview();
			}

			if (selectedType == AppendageType.Branch)
			{
				if (plantGenerator.branches.Count <= 0)
					return;
				if ((plantGenerator.branches.Count - 1) < lastIDSelected)
					return;
				Branch currentRama = plantGenerator.branches[lastIDSelected];

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.LabelField(plantGenerator.branches[lastIDSelected].curve.name);


				ChangeStylePreview();
				var render = EditorGUILayout.Toggle("Render", currentRama.render[previewLevel]);
				EditorGUILayout.Space(10);
				var numberOfSides = EditorGUILayout.IntField("Nº of Sides", currentRama.GetSides(previewLevel));
				var numberOfDivisions = EditorGUILayout.IntField("Nº of Divisions", currentRama.GetDivisions(previewLevel));
				CleanStylePreview();

				EditorGUILayout.Space(10);
				var noiseAmount = EditorGUILayout.Slider("Noise Amount", currentRama.noiseAmount, 0, 1f);
				var seed = EditorGUILayout.IntField("Seed", currentRama.seed);

				var position = currentRama.Position;
				if (lastIDSelected != 0)
					position = EditorGUILayout.Slider("Position in Branch", currentRama.Position, 0, 1);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Changed Branch Parameter", () =>
					{
						currentRama.seed = seed;
						currentRama.noiseAmount = noiseAmount;
						currentRama.SetDivisions(previewLevel, numberOfDivisions < 2 ? 2 : numberOfDivisions);
						currentRama.render[previewLevel] = render;
						currentRama.SetSides(previewLevel, numberOfSides < 2 ? 2 : numberOfSides);
						currentRama.Position = position;
						currentRama.SetBranchDirty();
					});
				}

				if (GUILayout.Button("Insert Branch"))
				{
					RecordAndDirty("Add Branch", () =>
					{
						currentRama.SetBranchDirty();
						plantGenerator.AddBranch(currentRama);
					});
				}

				if ((lastIDSelected != 0) && (selectedIndex == 0))
					if (GUILayout.Button("Remove Branch"))
					{
						RecordAndDirty("Delete Branch", () =>
						{
							currentRama.SetBranchDirty();
							plantGenerator.DeleteBranch(currentRama);
						});
					}

				EditorGUILayout.Space(20);

				if (selectedIndex == 0)
				{
					if (GUILayout.Button("Add Randomized Leaves to Branch"))
					{
						RecordAndDirty("Add Leaves", () =>
						{
							plantGenerator.AddLeaves(currentRama);
							currentRama.SetBranchDirty();
						});
					}

					if (GUILayout.Button("Add SDFLeaves to Branch"))
					{
						RecordAndDirty("Add SDFLeaves", () =>
						{
							plantGenerator.AddSDFLeaves(currentRama);
							currentRama.SetBranchDirty();
						});
					}

					if (GUILayout.Button("Add Spline Leaf to Branch"))
					{
						RecordAndDirty("Add Spline Leaf", () =>
						{
							plantGenerator.AddSplineLeaf(currentRama);
							currentRama.SetBranchDirty();
						});
					}
					if (GUILayout.Button("Add Custom Model to Branch"))
					{
						RecordAndDirty("Add Custom Mesh", () =>
						{
							plantGenerator.AddCustomMesh(currentRama);
							currentRama.SetBranchDirty();
						});
					}
				}
				DrawInspectorSpline(currentRama.curve);
			}
			else if (selectedType == AppendageType.Leaves)
			{
				if (plantGenerator.leaves.Count <= 0)
					return;
				if ((plantGenerator.leaves.Count - 1) < lastIDSelected)
					return;
				Leaves currentLeaf = plantGenerator.leaves[lastIDSelected];

				EditorGUI.BeginChangeCheck();

				ChangeStylePreview();
				var render = EditorGUILayout.Toggle(new GUIContent("Render", "Be careful with deactivating render if you have a single leaf node, as it will render the wood with the leaves material too"), currentLeaf.render[previewLevel]);

				EditorGUILayout.Space(10);
				var number = EditorGUILayout.IntField("Number", currentLeaf.GetNumber(previewLevel));

				var size = EditorGUILayout.FloatField("Size", currentLeaf.GetSize(previewLevel));
				CleanStylePreview();
				var length = EditorGUILayout.FloatField("Length", currentLeaf.length);
				var variation = EditorGUILayout.CurveField("Size Variation", currentLeaf.sizeVariation);

				var range = currentLeaf.range;
				EditorGUILayout.MinMaxSlider("Range", ref range.x, ref range.y, 0, 1);
				var offset = EditorGUILayout.Slider("Offset from branch", currentLeaf.offsetFromBranch, 0, 1);
				EditorGUILayout.Space(10);
				var space = (Leaves.RotationDirection)EditorGUILayout.EnumPopup("Rotation Space", currentLeaf.rotationSpace);
				var preferredRotation = EditorGUILayout.Vector3Field("Preferred Rotation", currentLeaf.preferredRotation);
				var rotationRandomness = EditorGUILayout.Vector3Field("Rotation Randomness", currentLeaf.rotationRandomness);

				EditorGUILayout.Space(10);
				var seed = EditorGUILayout.IntField("Seed", currentLeaf.seed);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Changed Leaf Parameter", () =>
					{
						currentLeaf.seed = seed;
						currentLeaf.rotationRandomness = rotationRandomness;
						currentLeaf.preferredRotation = preferredRotation;
						currentLeaf.render[previewLevel] = render;
						currentLeaf.SetNumber(previewLevel, number);
						currentLeaf.SetSize(previewLevel, size);
						currentLeaf.rotationSpace = space;
						currentLeaf.sizeVariation = variation;
						currentLeaf.range = range;
						// currentLeaf.dirty = true;
						currentLeaf.offsetFromBranch = offset;
						currentLeaf.length = length;
					});
				}

				if (GUILayout.Button("Remove Leaves"))
				{
					RecordAndDirty("Delete Leaves", () =>
					{
						plantGenerator.DeleteLeaves(currentLeaf);
						currentLeaf.dirty = true;
					});
				}

			}
			else if (selectedType == AppendageType.SDFLeaves)
			{
				if (plantGenerator.sdfLeaves.Count <= 0)
					return;
				if ((plantGenerator.sdfLeaves.Count - 1) < lastIDSelected)
					return;
				SDFLeaves currentLeaf = plantGenerator.sdfLeaves[lastIDSelected];

				EditorGUI.BeginChangeCheck();
				ChangeStylePreview();
				var render = EditorGUILayout.Toggle(new GUIContent("Render", "Be careful with deactivating render if you have a single leaf node, as it will render the wood with the leaves material too"), currentLeaf.render[previewLevel]);

				EditorGUILayout.Space(10);
				var number = EditorGUILayout.IntField("Number", currentLeaf.GetNumber(previewLevel));
				var size = EditorGUILayout.FloatField("Leaf Size", currentLeaf.GetSize(previewLevel));
				CleanStylePreview();
				var length = EditorGUILayout.FloatField("Length", currentLeaf.length);
				var type = (SDFLeaves.FormType)EditorGUILayout.EnumPopup("Form Type", currentLeaf.formType);

				var shell = EditorGUILayout.Toggle("Shell", currentLeaf.shell);

				EditorGUI.BeginDisabledGroup(shell);
				var density = EditorGUILayout.CurveField("Density", currentLeaf.density, Color.red, new Rect(0, 0, 1, 1));
				EditorGUI.EndDisabledGroup();
				var radius = EditorGUILayout.FloatField("Radius", currentLeaf.Radius);

				float internalRadius = 0.5f;
				if (type == SDFLeaves.FormType.Torus)
				{
					internalRadius = EditorGUILayout.Slider("Inner Radius", currentLeaf.innerRadius, 0.001f, 1);
				}

				float smoothness = EditorGUILayout.Slider("Smoothness", currentLeaf.smoothness, 0, 1f);

				float semi = 1;
				if (type != SDFLeaves.FormType.Cylinder && type != SDFLeaves.FormType.Pyramid)
				{
					semi = EditorGUILayout.Slider("Cut Geometry", currentLeaf.cut, 0.001f, 1);
				}

				EditorGUILayout.Space(10);
				var spherize = EditorGUILayout.Slider("Override Normals (Global)", plantGenerator.overrideNormals, 0, 1);




				bool sphereNormal = false;
				if ((spherize > 0))
				{
					sphereNormal = EditorGUILayout.Toggle("Use Sphere Normals Instead", currentLeaf.useSphereNormals);
				}

				EditorGUILayout.Space(10);
				var offset = EditorGUILayout.Vector3Field("Position Offset", currentLeaf.positionOffset);
				var rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", currentLeaf.rotation.eulerAngles));
				var scale = EditorGUILayout.Vector3Field("Scale", currentLeaf.scale);

				EditorGUILayout.Space(10);
				var space = (SDFLeaves.RotationDirection)EditorGUILayout.EnumPopup("Rotation Space", currentLeaf.rotationSpace);
				bool overrideSphere = false;
				if ((space == SDFLeaves.RotationDirection.Local))
				{
					overrideSphere = EditorGUILayout.Toggle("Use Sphere as Local", currentLeaf.overrideSphereSpace);
				}
				var preferred = EditorGUILayout.Vector3Field("Preferred Rotation", currentLeaf.preferredRotation);
				var random = EditorGUILayout.Vector3Field("Rotation Randomness", currentLeaf.rotationRandomness);
				var position = EditorGUILayout.Slider("Position in Branch", currentLeaf.Position, 0, 1);


				EditorGUILayout.Space(10);
				var seed = EditorGUILayout.IntField("Seed", currentLeaf.seed);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Changed SDFLeaf Parameter", () =>
					{
						currentLeaf.render[previewLevel] = render;

						currentLeaf.formType = type;
						currentLeaf.SetNumber(previewLevel, number <= 15000 ? number : 15000);
						currentLeaf.SetSize(previewLevel, size);
						currentLeaf.Radius = radius;
						if (type != SDFLeaves.FormType.Cylinder && type != SDFLeaves.FormType.Pyramid)
						{
							currentLeaf.cut = semi;
						}
						currentLeaf.preferredRotation = preferred;
						currentLeaf.rotationRandomness = random;
						currentLeaf.Position = position;
						currentLeaf.positionOffset = offset;
						currentLeaf.seed = seed;
						currentLeaf.scale = scale;
						currentLeaf.dirty = true;
						currentLeaf.rotation = rotation;
						currentLeaf.shell = shell;
						currentLeaf.density = density;
						currentLeaf.length = length;
						currentLeaf.rotationSpace = space;
						if ((space == SDFLeaves.RotationDirection.Local))
						{
							currentLeaf.overrideSphereSpace = overrideSphere;
						}
						if ((spherize > 0))
						{
							currentLeaf.useSphereNormals = sphereNormal;
						}
						if (type == SDFLeaves.FormType.Torus)
						{
							currentLeaf.innerRadius = internalRadius;

						}

						currentLeaf.smoothness = smoothness;

						plantGenerator.overrideNormals = spherize;
					});
				}

				if (GUILayout.Button("Remove SDFLeaves"))
				{
					RecordAndDirty("Delete SDFLeaves", () =>
					{
						plantGenerator.DeleteSDFLeaves(currentLeaf);
						currentLeaf.dirty = true;
					});
				}
			}
			else if (selectedType == AppendageType.SplineLeaf)
			{
				if (plantGenerator.splineLeaves.Count <= 0)
					return;
				if ((plantGenerator.splineLeaves.Count - 1) < lastIDSelected)
					return;
				SplineLeaf currentLeaf = plantGenerator.splineLeaves[lastIDSelected];

				EditorGUI.BeginChangeCheck();
				ChangeStylePreview();
				var render = EditorGUILayout.Toggle(new GUIContent("Render", "Be careful with deactivating render if you have a single leaf node, as it will render the wood with the leaves material too"), currentLeaf.render[previewLevel]);

				EditorGUILayout.Space(10);
				var number = EditorGUILayout.IntField("Nº of Divisions", currentLeaf.GetDivisions(previewLevel));
				CleanStylePreview();

				var material = plantGenerator.renderSplineWithThirdMaterial;
				if (((plantGenerator.leaves.Count > 0) || (plantGenerator.sdfLeaves.Count > 0)) && (plantGenerator.splineLeaves.Count > 0))
				{
					EditorGUILayout.Space(10);
					material = EditorGUILayout.Toggle(new GUIContent("Use third material [READ TOOLTIP!]", "This is a common property for ALL spline leaves, in order to render spline leaves with a different material than the rest of the leaves. If there are only spline leafs, this parameter isn't shown"), plantGenerator.renderSplineWithThirdMaterial);
				}


				EditorGUILayout.Space(10);
				var position = EditorGUILayout.Slider("Position in Branch", currentLeaf.Position, 0, 1);
				var rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", currentLeaf.rotation.eulerAngles));
				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Changed Spline Leaf Parameter", () =>
					{
						currentLeaf.render[previewLevel] = render;
						currentLeaf.SetDivisions(previewLevel, number < 2 ? 2 : number);
						currentLeaf.Position = position;
						plantGenerator.renderSplineWithThirdMaterial = material;
						currentLeaf.rotation = rotation;
						currentLeaf.dirty = true;
					});
				}

				if (GUILayout.Button("Remove Spline Leaf"))
				{
					RecordAndDirty("Delete Spline Leaf", () =>
					{
						plantGenerator.DeleteSplineLeaf(currentLeaf);
						currentLeaf.dirty = true;
					});
				}

				if (GUILayout.Button("Duplicate"))
				{
					RecordAndDirty("DuplicateSpline", () =>
					{
						plantGenerator.DuplicateSplineLeaf(currentLeaf);
						currentLeaf.dirty = true;
					});
				}
				DrawInspectorSpline(currentLeaf.curve);
			}
			else if (selectedType == AppendageType.CustomMesh)
			{
				if (plantGenerator.customMeshes.Count <= 0)
					return;
				if ((plantGenerator.customMeshes.Count - 1) < lastIDSelected)
					return;
				CustomMesh currentMesh = plantGenerator.customMeshes[lastIDSelected];

				EditorGUI.BeginChangeCheck();

				var render = EditorGUILayout.Toggle("Render", currentMesh.render[previewLevel]);


				var offset = EditorGUILayout.Vector3Field("Offset", currentMesh.positionOffset);
				var rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", currentMesh.rotation.eulerAngles));
				var scale = EditorGUILayout.Vector3Field("Scale", currentMesh.scale);
				var position = EditorGUILayout.Slider("Position in Branch", currentMesh.Position, 0, 1);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Changed Custom Mesh Parameter", () =>
					{

						currentMesh.positionOffset = offset;
						currentMesh.rotation = rotation;
						currentMesh.scale = scale;
						currentMesh.Position = position;
						currentMesh.render[previewLevel] = render;
						currentMesh.dirty = true;
					});
				}
				EditorGUI.BeginChangeCheck();
				var prefab = (GameObject)EditorGUILayout.ObjectField("Model", currentMesh.prefabObject, typeof(GameObject), false);
				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Changed Custom Mesh", () =>
					{
						currentMesh.prefabObject = prefab;
						plantGenerator.DestroyCustomMesh(currentMesh);
					});
				}

				if (GUILayout.Button("Duplicate"))
				{
					RecordAndDirty("Duplicate Custom Model", () =>
					{
						plantGenerator.DuplicateCustomMesh(currentMesh);
						currentMesh.dirty = true;
					});
				}

				if (GUILayout.Button("Remove Custom Model"))
				{
					RecordAndDirty("Delete Custom Mesh", () =>
					{
						plantGenerator.DeleteCustomMesh(currentMesh);
						currentMesh.dirty = true;
					});
				}
			}

			// serializedObject.ApplyModifiedProperties();
		}

		private void DrawInspectorSpline(BezierSpline spline)
		{
			if ((selectedIndex >= 0) && (selectedIndex < spline.ControlPointCount))
			{
				DrawSelectedPointInspector(spline);
			}

			if (GUILayout.Button("Add Curve"))
			{
				RecordAndDirty("Add Curve", () => spline.AddCurve());
			}

			if ((selectedIndex % 3) == 0)
			{
				if (selectedIndex != (spline.ControlPointCount - 1))
				{
					if (GUILayout.Button("Insert Curve"))
					{
						RecordAndDirty("Insert Curve", () => spline.AddCurve(selectedIndex));
					}
				}
				if (selectedIndex != 0)
				{
					if (GUILayout.Button("Delete Curve"))
					{
						RecordAndDirty("Delete Curve", () => spline.DeleteCurve(selectedIndex));
					}
				}
			}
		}

		private void DrawSelectedPointInspector(BezierSpline spline)
		{
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			var point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
			var scale = Vector3.one;
			var radius = 1f;
			if ((selectedIndex % 3) == 0)
			{
				radius = EditorGUILayout.FloatField("Radius", spline.GetRadiusControlPoint(selectedIndex));
				//spline leaves do not have axis scaling
				if (selectedType != AppendageType.SplineLeaf)
					scale = EditorGUILayout.Vector3Field("Scale", spline.points[selectedIndex].scale);
			}
			if (EditorGUI.EndChangeCheck())
			{
				RecordAndDirty("Changed Point Parameters", () =>
					{
						spline.SetControlPoint(selectedIndex, point);
						if ((selectedIndex % 3) == 0)
						{
							spline.SetRadius(selectedIndex, radius);
							if (selectedType != AppendageType.SplineLeaf)
								spline.points[selectedIndex].scale = scale;
						}
					}
				);
			}

			EditorGUI.BeginChangeCheck();
			BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));

			if (EditorGUI.EndChangeCheck())
			{
				RecordAndDirty("Change Point Mode", () => spline.SetControlPointMode(selectedIndex, mode));
			}
		}

		private void OnSceneGUI()
		{
			if (plantGenerator == null)
				return;
			handleTransform = plantGenerator.transform;
			handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

			try { 
			// plantGenerator.UpdateAllOffsets();
			foreach (var item in plantGenerator.branches)
			{
				DrawGUISplines(item);
			}
			foreach (var item in plantGenerator.leaves)
			{
				DrawLeaveHandles(item);
			}
			foreach (var item in plantGenerator.sdfLeaves)
			{
				DrawSDFLeavesHandles(item);
			}
			foreach (var item in plantGenerator.splineLeaves)
			{
				DrawGUISplines(item);
			}
			foreach (var item in plantGenerator.customMeshes)
			{
				DrawCustomMeshHandles(item);
			}
			}
			catch 
			{
				plantGenerator.FixReferences();
			}
		}


		private void DrawGUISplines(Branch branch)
		{
			Vector3 p0 = handleTransform.TransformPoint(branch.curve.GetControlPoint(0) + branch.curve.Offset);
			DrawBranchHandles(0, branch, p0);

			for (int i = 1; i < branch.curve.ControlPointCount; i += 3)
			{
				Vector3 p1 = handleTransform.TransformPoint(branch.curve.GetControlPoint(i) + branch.curve.Offset);
				Vector3 p2 = handleTransform.TransformPoint(branch.curve.GetControlPoint(i + 1) + branch.curve.Offset);
				Vector3 p3 = handleTransform.TransformPoint(branch.curve.GetControlPoint(i + 2) + branch.curve.Offset);

				Handles.DrawBezier(p0, p3, p1, p2, branch.curve.curveColor, null, 8f);

				if ((displayHandles == DisplayHandles.All) || (((selectedType == AppendageType.Branch) || (selectedType == AppendageType.SplineLeaf)) && (displayHandles == DisplayHandles.OnlySelected) && (lastIDSelected == branch.ID)))
				{
					Handles.color = Color.black;
#if UNITY_2020_2_OR_NEWER
					Handles.DrawLine(p0, p1, 2);
					Handles.DrawLine(p2, p3, 2);
#else
                    Handles.DrawLine(p0, p1);
                    Handles.DrawLine(p2, p3);
#endif
				}

				DrawBranchHandles(i, branch, p1);
				DrawBranchHandles(i + 1, branch, p2);
				DrawBranchHandles(i + 2, branch, p3);
				p0 = p3;
			}
		}

		private void DrawGUISplines(SplineLeaf leaf)
		{
			Vector3 p0 = handleTransform.TransformPoint(leaf.curve.GetControlPoint(0) + leaf.curve.Offset);


			Handles.color = Color.yellow;
			float size = HandleUtility.GetHandleSize(p0) * 2;
			if ((lastIDSelected != leaf.ID) || (selectedType != AppendageType.SplineLeaf))
				if (Handles.Button(p0 + Vector3.up * ((plantGenerator.splineLeaves[leaf.ID].IDInBranch + 1) * 0.2f), handleRotation, size * handleSize, size * pickSize, Handles.SphereHandleCap))
				{
					selectedType = AppendageType.SplineLeaf;
					lastIDSelected = leaf.ID;
					selectedIndex = 0;
					Repaint();
				}
			DrawSplineLeafHandles(0, plantGenerator.splineLeaves[leaf.ID], p0);


			for (int i = 1; i < leaf.curve.ControlPointCount; i += 3)
			{
				Vector3 p1 = handleTransform.TransformPoint(leaf.curve.GetControlPoint(i) + leaf.curve.Offset);
				Vector3 p2 = handleTransform.TransformPoint(leaf.curve.GetControlPoint(i + 1) + leaf.curve.Offset);
				Vector3 p3 = handleTransform.TransformPoint(leaf.curve.GetControlPoint(i + 2) + leaf.curve.Offset);

				Handles.DrawBezier(p0, p3, p1, p2, leaf.curve.curveColor, null, 8f);

				if ((displayHandles == DisplayHandles.All) || (((selectedType == AppendageType.SplineLeaf) || (selectedType == AppendageType.SplineLeaf)) && (displayHandles == DisplayHandles.OnlySelected) && (lastIDSelected == leaf.ID)))
				{
					Handles.color = Color.black;
#if UNITY_2020_2_OR_NEWER
					Handles.DrawLine(p0, p1, 2);
					Handles.DrawLine(p2, p3, 2);
#else
                    Handles.DrawLine(p0, p1);
                    Handles.DrawLine(p2, p3);
#endif
				}
				DrawSplineLeafHandles(i, plantGenerator.splineLeaves[leaf.ID], p1);
				DrawSplineLeafHandles(i + 1, plantGenerator.splineLeaves[leaf.ID], p2);
				DrawSplineLeafHandles(i + 2, plantGenerator.splineLeaves[leaf.ID], p3);
				p0 = p3;
			}
		}


		private void DrawLeaveHandles(Leaves leaf)
		{
			Handles.color = Color.blue;
			Vector3 point = handleTransform.TransformPoint(leaf.averagePosition + Vector3.up * ((leaf.IDInBranch + 1) * 0.2f));

			float size = HandleUtility.GetHandleSize(point) * 2;
			HandleButton(point, size, Handles.SphereHandleCap, leaf.ID, 0, AppendageType.Leaves);

			if ((selectedType == AppendageType.Leaves) && (lastIDSelected == leaf.ID))
			{
				//scale radius
				EditorGUI.BeginChangeCheck();
				float scale = Handles.ScaleValueHandle(leaf.GetSize(plantGenerator.previewLevel), point, Quaternion.identity, HandleUtility.GetHandleSize(point) * 2f, Handles.SphereHandleCap, 0.1f);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Scale Leaves", () => leaf.SetSize(plantGenerator.previewLevel, scale));
				}
			}
		}

		private void DrawSDFLeavesHandles(SDFLeaves leaf)
		{
			Handles.color = Color.grey;
			Vector3 point = (leaf.parent.curve.GetPoint(leaf.Position) + leaf.parent.offset + Vector3.up * ((leaf.IDInBranch + 1) * 0.2f));
			float size = HandleUtility.GetHandleSize(point) * 2;
			HandleButton(point, size, Handles.SphereHandleCap, leaf.ID, 0, AppendageType.SDFLeaves);

			if ((selectedType == AppendageType.SDFLeaves) && (lastIDSelected == leaf.ID))
			{
				//slide position
				EditorGUI.BeginChangeCheck();
				Handles.color = Color.white;
				float t = Handles.ScaleValueHandle(leaf.Position, point + size * Vector3.up / 2f, Quaternion.LookRotation(Vector3.up, Vector3.up), HandleUtility.GetHandleSize(point) * arrowSize, Handles.ArrowHandleCap, 0.1f);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Move SDFLeave Origin", () => leaf.Position = t);
				}

				//rotation
				EditorGUI.BeginChangeCheck();
				Quaternion rotation = Handles.RotationHandle(handleTransform.rotation * leaf.rotation, point);
				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Rotate SDFLeaves", () => leaf.rotation = Quaternion.Inverse(handleTransform.rotation) * rotation);
				}

				EditorGUI.BeginChangeCheck();
				var scale = Handles.ScaleHandle(leaf.scale, point, handleTransform.rotation * leaf.rotation, HandleUtility.GetHandleSize(point) * 0.75f);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Scale Leaves", () => leaf.scale = scale);
				}

				//scale radius
				EditorGUI.BeginChangeCheck();
				float radius = Handles.ScaleValueHandle(leaf.Radius, point, Quaternion.identity, size, Handles.SphereHandleCap, 0.1f);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Scale Leaves radius", () => leaf.Radius = radius);
				}

			}
		}

		private void DrawSplineLeafHandles(int index, SplineLeaf leaf, Vector3 point)
		{
			Handles.color = Color.green;
			if ((selectedType == AppendageType.CustomMesh) && (lastIDSelected == leaf.ID) && ((index % 3) == 0))
			{
				Handles.color = Color.blue;
			}

			float size = HandleUtility.GetHandleSize(point) * (((index % 3) == 0) ? 2 : 1);

			if ((selectedType == AppendageType.SplineLeaf) && (lastIDSelected == leaf.ID))
			{
				if (((index % 3) == 0) || ((displayHandles == DisplayHandles.All) || ((selectedType == AppendageType.SplineLeaf) && (displayHandles == DisplayHandles.OnlySelected) && (lastIDSelected == leaf.ID))))
				{
					Handles.CapFunction cap = Handles.CubeHandleCap;
					if ((index % 3) == 0)
					{
						cap = Handles.SphereHandleCap;
					}

					HandleButton(point, size, cap, leaf.ID, index, AppendageType.SplineLeaf);
				}
				if (selectedIndex == index)
				{
					Handles.color = Color.blue;
					// if (selectedIndex % 3 == 0)
					// {
					//     //scale leaf control points 
					//     EditorGUI.BeginChangeCheck();
					//     var scale = Handles.ScaleHandle(leaf.curve.points[index].scale, point, handleTransform.rotation * Quaternion.FromToRotation(Vector3.up, leaf.curve.GetDirectionControlPoint(index)), size * 0.7f);
					//     if (EditorGUI.EndChangeCheck())
					//     {
					//         RecordAndDirty("scale spline leaf", () => leaf.curve.points[index].scale = scale);
					//     }
					// }

					//Move point
					if (selectedIndex != 0)
					{
						EditorGUI.BeginChangeCheck();
						point = Handles.DoPositionHandle(point, Quaternion.identity);

						if (EditorGUI.EndChangeCheck())
						{
							RecordAndDirty("Move Spline Leaf Point", () =>
							{
								leaf.curve.SetControlPoint(index, handleTransform.InverseTransformPoint(point + handleTransform.position - handleTransform.TransformPoint(leaf.curve.Offset)));
							});
						}
					}
					//Move branch origin
					else
					{
						EditorGUI.BeginChangeCheck();
						Handles.color = Color.white;
						float t = Handles.ScaleValueHandle(leaf.Position, point + size * Vector3.up / 2f, Quaternion.LookRotation(Vector3.up, Vector3.up), size / 2 * arrowSize, Handles.ArrowHandleCap, 0.1f);

						if (EditorGUI.EndChangeCheck())
						{
							RecordAndDirty("Move Spline Leaf Origin", () => leaf.Position = t);
						}

						//rotation
						EditorGUI.BeginChangeCheck();
						//rotate the transform with the object rotation too
						Quaternion rotation = Handles.RotationHandle(handleTransform.rotation * leaf.rotation, point);
						if (EditorGUI.EndChangeCheck())
						{
							RecordAndDirty("Rotate Spline Leaf", () => leaf.rotation = Quaternion.Inverse(handleRotation) * rotation);
						}
					}

					//the order matters
					if ((selectedIndex % 3) == 0)
					{
						//scale leaf control points radius
						Handles.color = Color.blue;
						EditorGUI.BeginChangeCheck();
						var radius = Handles.ScaleValueHandle(leaf.curve.GetRadiusControlPoint(index), point, Quaternion.identity, size, Handles.SphereHandleCap, 0.1f);
						if (EditorGUI.EndChangeCheck())
						{
							RecordAndDirty("scale spline leaf radius", () => leaf.curve.SetRadius(index, radius));
						}
					}
				}
			}
		}

		private void DrawCustomMeshHandles(CustomMesh mesh)
		{
			Handles.color = Color.cyan;
			if ((selectedType == AppendageType.CustomMesh) && (lastIDSelected == mesh.ID))
			{
				Handles.color = Color.blue;
			}

			Vector3 point = (mesh.parent.curve.GetPoint(mesh.Position) + mesh.parent.offset + Vector3.up * ((mesh.IDInBranch + 1) * 0.2f));
			float size = HandleUtility.GetHandleSize(point) * 2;

			HandleButton(point, size, Handles.SphereHandleCap, mesh.ID, 0, AppendageType.CustomMesh);

			if ((selectedType == AppendageType.CustomMesh) && (lastIDSelected == mesh.ID))
			{
				Vector3 position = mesh.meshPosition + mesh.positionOffset;

				//move origin
				EditorGUI.BeginChangeCheck();
				Handles.color = Color.white;
				float t = Handles.ScaleValueHandle(mesh.Position, position + size * Vector3.up / 1.2f, Quaternion.LookRotation(Vector3.up, Vector3.up), HandleUtility.GetHandleSize(point) * arrowSize, Handles.ArrowHandleCap, 0.1f);

				if (EditorGUI.EndChangeCheck())
				{

					RecordAndDirty("Move SDFLeave Origin", () => mesh.Position = t);
				}
				//hay que tener en cuenta la rotacion del objeto en el gizmo
				var rotation = handleTransform.rotation * mesh.rotation;
				// var offsetRot = Quaternion.FromToRotation(rotation, Quaternion.identity);
				var scale = mesh.scale;
				EditorGUI.BeginChangeCheck();

				Handles.TransformHandle(ref position, ref rotation, ref scale);

				if (EditorGUI.EndChangeCheck())
				{
					RecordAndDirty("Transform Leaves", () =>
					{
						mesh.positionOffset = position - mesh.meshPosition;
						mesh.rotation = Quaternion.Inverse(handleRotation) * rotation;
						mesh.scale = scale;
					});
				}

			}
		}

		//all branch selectable points
		private void DrawBranchHandles(int index, Branch branch, Vector3 point)
		{
			// Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index)) + spline.Offset;
			float size = HandleUtility.GetHandleSize(point) * (((index % 3) == 0) ? 2 : 1);

			if ((index % 3) == 0)
			{
				Handles.color = branch.curve.curveColor;
			}
			else
			{
				Handles.color = modeColors[(int)branch.curve.GetControlPointMode(index)];
			}

			Vector3 offset = (index == 0) ? Vector3.up * ((branch.IDInBranch + 1) * 0.2f) : Vector3.zero;
			bool isSelected = (selectedType == AppendageType.Branch) && (lastIDSelected == branch.ID) && (selectedIndex == index);
			// IF it is a control point OR (We are displaying all of them OR (we display only some and this happens to be selected)) 
			if ((((index % 3) == 0) || ((displayHandles == DisplayHandles.All) || ((selectedType == AppendageType.Branch) && (displayHandles == DisplayHandles.OnlySelected) && (lastIDSelected == branch.ID)))) && !isSelected)
			{
				Handles.CapFunction cap = Handles.CubeHandleCap;
				if ((index % 3) == 0)
				{
					cap = Handles.SphereHandleCap;
				}
				HandleButton(point + offset, size, cap, branch.ID, index, AppendageType.Branch);
			}

			if (isSelected)
			{
				//if it is not a secondary handle
				if ((selectedIndex % 3) == 0)
				{
					//Scale branch control point
					EditorGUI.BeginChangeCheck();

					var rotation = handleTransform.rotation * Quaternion.FromToRotation(Vector3.up, branch.curve.GetDirectionControlPoint(index));

					Handles.color = Color.red;
					var x = Handles.ScaleSlider(branch.curve.points[index].scale.x, point + rotation * Vector3.right * (0.6f * size), rotation * Vector3.right, rotation, size * 0.5f, 0.1f);
					Handles.color = Color.blue;
					var z = Handles.ScaleSlider(branch.curve.points[index].scale.z, point + rotation * Vector3.forward * (0.6f * size), rotation * Vector3.forward, rotation, size * 0.5f, 0.1f);
					// Handles.color = Color.green;
					// var y = Handles.ScaleSlider(branch.curve.points[index].scale.y, point + rotation * (Vector3.up * 0.6f * size), rotation * Vector3.up, rotation, size * 0.5f, 0.1f);
					Vector3 scale = new Vector3(x, 1, z);


					if (EditorGUI.EndChangeCheck())
					{
						RecordAndDirty("scale Branch", () => branch.curve.points[index].scale = scale);
					}
				}

				//Move point
				if (selectedIndex != 0)
				{

					EditorGUI.BeginChangeCheck();
					point = Handles.DoPositionHandle(point, Quaternion.identity);

					if (EditorGUI.EndChangeCheck())
					{
						RecordAndDirty("Move Branch Point", () =>
						{
							branch.curve.SetControlPoint(index, handleTransform.InverseTransformPoint(point + handleTransform.position - handleTransform.TransformPoint(branch.curve.Offset)));
						});
					}
				}
				//Move branch origin
				else if (branch.curve.curveColor != Color.white)
				{
					EditorGUI.BeginChangeCheck();
					Handles.color = Color.white;
					float t;
					t = Handles.ScaleValueHandle(plantGenerator.branches[branch.ID].Position, point + offset, Quaternion.LookRotation(Vector3.up, Vector3.up), HandleUtility.GetHandleSize(point) * arrowSize, Handles.ArrowHandleCap, 0.1f);

					if (EditorGUI.EndChangeCheck())
					{
						RecordAndDirty("Move Branch Origin", () => branch.Position = t);
					}
				}
				Handles.color = Color.yellow;
				//the order matters
				if ((selectedIndex % 3) == 0)
				{
					//Scale branch control point radius
					EditorGUI.BeginChangeCheck();
					var radius = Handles.ScaleValueHandle(branch.curve.GetRadiusControlPoint(index), ((lastIDSelected == 0) && (selectedIndex == 0)) ? point + offset : point, Quaternion.identity, size * (((lastIDSelected == 0) && (selectedIndex == 0)) ? 2 : 1), Handles.SphereHandleCap, 0.1f);
					if (EditorGUI.EndChangeCheck())
					{
						RecordAndDirty("scale Branch", () => branch.curve.SetRadius(index, radius));
					}
				}


			}
		}

		private void RecordAndDirty(string command, System.Action action = null)
		{
			Undo.RecordObject(plantGenerator, command);
			if (action != null)
				action.Invoke();
			EditorUtility.SetDirty(plantGenerator);
			plantGenerator.Generate(plantGenerator.meshColliderLevel, true);
			plantGenerator.Generate();
		}

		public void HandleButton(Vector3 point, float size, Handles.CapFunction cap, int id, int newSelectedIndex, AppendageType type, bool samePickSize = false)
		{
			if (Handles.Button(point, handleRotation, size * handleSize, samePickSize ? size * handleSize : size * pickSize, cap))
			{
				selectedType = type;
				lastIDSelected = id;
				selectedIndex = newSelectedIndex;
				Repaint();
			}
		}

		private void ChangeStylePreview()
		{
			if (plantGenerator.previewLevel != 0)
			{
				EditorStyles.label.fontStyle = FontStyle.Bold;
				EditorStyles.label.normal.textColor = Color.red;
				EditorStyles.label.active.textColor = Color.red;
				EditorStyles.label.hover.textColor = Color.red;
			}
		}
		private void CleanStylePreview()
		{
			EditorStyles.label.fontStyle = FontStyle.Normal;
			EditorStyles.label.normal.textColor = defaultColor;
			EditorStyles.label.active.textColor = defaultColor;
			EditorStyles.label.hover.textColor = defaultColor;
		}
	}
}
#endif
