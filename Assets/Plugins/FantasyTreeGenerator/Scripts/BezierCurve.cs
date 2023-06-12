using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//bezier code based on Catlikecoding tutorial
namespace FTG
{
	public enum BezierControlPointMode
	{
		Free,
		Aligned,
		Mirrored
	}
	[System.Serializable]
	public static class Bezier
	{
		public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return oneMinusT * oneMinusT * oneMinusT * p0 + 3f * oneMinusT * oneMinusT * t * p1 + 3f * oneMinusT * t * t * p2 + t * t * t * p3;
		}
		public static float GetRadius(Point p0, Point p3, float t)
		{
			t = Mathf.Clamp01(t);
			return Mathf.Lerp(p0.Radius, p3.Radius, t);
		}
		public static Vector3 GetSize(Point p0, Point p3, float t)
		{
			t = Mathf.Clamp01(t);
			return Vector3.Lerp(p0.scale, p3.scale, t);
		}

		public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return 3f * oneMinusT * oneMinusT * (p1 - p0) + 6f * oneMinusT * t * (p2 - p1) + 3f * t * t * (p3 - p2);
		}
	}
	[System.Serializable]
	public class Point
	{
		public Vector3 position;
		[SerializeField]
		private float radius;

		public Vector3 scale;

		public Point(Vector3 Position)
		{
			position = Position;
			Radius = 0.1f;
			scale = Vector3.one;
		}
		public Point(Vector3 Position, float _size)
		{
			position = Position;
			Radius = _size;
		}

		public float Radius
		{
			get
			{
				return radius;
			}
			set
			{
				if (value > 0)
					radius = value;
				else radius = 0.001f;
			}
		}
	}
	[Serializable]
	public class BezierSpline
	{
		public string name;
		[HideInInspector]
		public Color curveColor;
		public float size;
		public int id;
		public Transform generator;
		private Vector3 offset;
		[SerializeReference]
		internal List<Point> points;
		[SerializeReference]
		private List<BezierControlPointMode> modes;

		public int ControlPointCount
		{
			get
			{
				return points.Count;
			}
		}
		public int CurveCount
		{
			get
			{
				return (points.Count - 1) / 3;
			}
		}
		public int ActualPointCount
		{
			get
			{
				return CurveCount + 1;
			}
		}

		public Vector3 Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public Vector3 GetControlPoint(int index)
		{
			return points[index].position;
		}
		public float GetRadiusControlPoint(int index)
		{
			return points[index].Radius;
		}
		public void SetRadius(int index, float radius)
		{
			points[index].Radius = radius;
		}


		public Vector3 GetPoint(float t)
		{
			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = points.Count - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}
			// Debug.Log(i);
			return generator.TransformPoint(Bezier.GetPoint(
				points[i].position, points[i + 1].position, points[i + 2].position, points[i + 3].position, t));
		}
//https://stackoverflow.com/questions/2742610/closest-point-on-a-cubic-bezier-curve
		/** Find the ~closest point on a Bézier curve to a point you supply.
 * out    : A vector to modify to be the point on the curve
 * curve  : Array of vectors representing control points for a Bézier curve
 * pt     : The point (vector) you want to find out to be near
 * tmps   : Array of temporary vectors (reduces memory allocations)
 * returns: The parameter t representing the location of `out`
 */
		public float GetClosestT2Point(Vector3 point,float error)
		{
			int mindex=0; // More scans -> better chance of being correct
			const float scans = 25; // More scans -> better chance of being correct
			float min = float.MaxValue;
			for (int i = (int)scans + 1; i >= 0; i--)
			{
				float  d2 = (point- GetPoint(i / scans)).sqrMagnitude;
				if (d2 < min)
				{
					min = d2;
					mindex = i;
				}
			}
			float t0 = Mathf.Max((mindex - 1) / scans, 0);
			float t1 = Mathf.Min((mindex + 1) / scans, 1);
			float D2ForT(float t) => (point - GetPoint(t)).sqrMagnitude;
			return  ( localMinimum(t0, t1, (Func<float,float>)D2ForT, error));
		}

		/** Find a minimum point for a bounded function. May be a local minimum.
 * minX   : the smallest input value
 * maxX   : the largest input value
 * ƒ      : a function that returns a value `y` given an `x`
 * ε      : how close in `x` the bounds must be before returning
 * returns: the `x` value that produces the smallest `y`
 */
		private float localMinimum(float minX,float maxX, Func<float,float> func,float error)
		{
			float m = minX;
			float n = maxX;
			float k=0;
			while ((n - m) > error)
			{
				k = (n + m) / 2f;
				if (func(k - error) < func(k + error)) n = k;
				else m = k;
			}
			return k;
		}

		public float GetRadius(float t)
		{
			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = points.Count - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}

			return Bezier.GetRadius(points[i], points[i + 3], t);
		}

		public Vector3 GetSize(float t)
		{
			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = points.Count - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}

			return Bezier.GetSize(points[i], points[i + 3], t);
		}

		public Vector3 GetVelocity(float t)
		{
			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = points.Count - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}

			return (Bezier.GetFirstDerivative(
				points[i].position, points[i + 1].position, points[i + 2].position, points[i + 3].position, t));
		}

		public Vector3 GetDirection(float t)
		{
			return GetVelocity(t).normalized;
		}

		// public Vector3 

		public Vector3 GetDirectionControlPoint(int index)
		{
			// ReSharper disable once PossibleLossOfFraction
			float t = index / (ActualPointCount - 1);
			return GetVelocity(t).normalized;
		}

		public BezierSpline()
		{
			curveColor = Color.black;
			points = new List<Point>();
			points.AddRange(new[] {new Point(new Vector3(0f, 0f, 0f) + offset), new Point(new Vector3(-0.5f, 0.8f, 0f) + offset), new Point(new Vector3(0f, 1.2f, 0f) + offset), new Point(new Vector3(-0.5f, 2f, 0f) + offset)});
			modes = new List<BezierControlPointMode>();
			modes.AddRange(new[]
			{
				BezierControlPointMode.Free,
				BezierControlPointMode.Free
			});
		}

		public BezierSpline(BezierSpline spline)
		{
			curveColor = spline.curveColor;
			name = spline.name + "Copy";
			size = spline.size;

			generator = spline.generator;
			Offset = spline.Offset;
			points = new List<Point>();
			Point[] newPoints = new Point[spline.points.Count];
			for (int i = 0; i < spline.points.Count; i++)
			{
				newPoints[i] = new Point(spline.points[i].position, spline.points[i].Radius);
			}

			points.AddRange(newPoints);

			BezierControlPointMode[] newBezierModes = new BezierControlPointMode[spline.modes.Count];
			for (int i = 0; i < spline.modes.Count; i++)
			{
				newBezierModes[i] = spline.modes[i];
			}

			modes = new List<BezierControlPointMode>();
			modes.AddRange(newBezierModes);
		}

		public void AddCurve()
		{
			Vector3 point = points[points.Count-1].position;

			point.y += 0.25f;
			points.Add(new Point(point));
			point.y += 0.25f;
			points.Add(new Point(point));
			point.y += 0.25f;
			point.x -= 0.5f;
			points.Add(new Point(point));

			modes.Add(modes[modes.Count - 1]);
			EnforceMode(points.Count - 4);
		}
		public void AddCurve(int selectedIndex)
		{

			//https://stackoverflow.com/questions/2613788/algorithm-for-inserting-points-in-a-piecewise-cubic-b%C3%A9zier-path
			float t = 0.5f;
			Vector3 P0_1 = (1 - t) * points[selectedIndex].position + t * points[selectedIndex + 1].position;
			Vector3 P1_2 = (1 - t) * points[selectedIndex + 1].position + t * points[selectedIndex + 2].position;
			Vector3 P2_3 = (1 - t) * points[selectedIndex + 2].position + t * points[selectedIndex + 3].position;

			Vector3 P01_12 = (1 - t) * P0_1 + t * P1_2;
			Vector3 P12_23 = (1 - t) * P1_2 + t * P2_3;
			Vector3 P0112_1223 = (1 - t) * P01_12 + t * P12_23;

			// points[selectedIndex]=p0;

			points.Insert(selectedIndex + 1, new Point(P0_1));
			points.Insert(selectedIndex + 2, new Point(P01_12));
			points.Insert(selectedIndex + 3, new Point(P0112_1223));

			points[selectedIndex + 4].position = P12_23;
			points[selectedIndex + 5].position = P2_3;
			points[selectedIndex + 3].Radius = Mathf.Lerp(points[selectedIndex].Radius, points[selectedIndex + 6].Radius, 0.5f);

			modes.Insert((selectedIndex + 1) / 3 + 1, BezierControlPointMode.Aligned);
			EnforceMode(selectedIndex + 1);
		}

		public void DeleteCurve(int selectedIndex)
		{
			// Debug.Log(selectedIndex);
			// Debug.Log(points.Count);
			if (selectedIndex == (points.Count - 1))
			{
				points.RemoveAt(points.Count - 1);
				points.RemoveAt(points.Count - 1);
				points.RemoveAt(points.Count - 1);
				modes.RemoveAt(modes.Count - 1);
			}
			else
			{
				points.RemoveAt(selectedIndex);
				points.RemoveAt(selectedIndex);
				points.RemoveAt(selectedIndex);
				modes.RemoveAt((selectedIndex + 1) / 3);
			}

		}


		public BezierControlPointMode GetControlPointMode(int index)
		{
			return modes[(index + 1) / 3];
		}

		public void SetControlPoint(int index, Vector3 point)
		{
			if ((index % 3) == 0)
			{
				Vector3 delta = point - points[index].position;
				if (index > 0)
				{
					points[index - 1].position += delta;
				}
				if ((index + 1) < points.Count)
				{
					points[index + 1].position += delta;
				}
			}
			points[index].position = point;
			EnforceMode(index);
		}

		public void SetControlPointMode(int index, BezierControlPointMode mode)
		{
			modes[(index + 1) / 3] = mode;
			EnforceMode(index);
		}

		private void EnforceMode(int index)
		{
			int modeIndex = (index + 1) / 3;
			BezierControlPointMode mode = modes[modeIndex];
			if ((mode == BezierControlPointMode.Free) || (modeIndex == 0) || (modeIndex == (modes.Count - 1)))
			{
				return;
			}

			int middleIndex = modeIndex * 3;
			int fixedIndex, enforcedIndex;

			if (index <= middleIndex)
			{
				fixedIndex = middleIndex - 1;
				enforcedIndex = middleIndex + 1;
			}
			else
			{
				fixedIndex = middleIndex + 1;
				enforcedIndex = middleIndex - 1;
			}

			Vector3 middle = points[middleIndex].position;
			Vector3 enforcedTangent = middle - points[fixedIndex].position;

			if (mode == BezierControlPointMode.Aligned)
			{
				enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex].position);
			}

			points[enforcedIndex].position = middle + enforcedTangent;
		}
	}
}
