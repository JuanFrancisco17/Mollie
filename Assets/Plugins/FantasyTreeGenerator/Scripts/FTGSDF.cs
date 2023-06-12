using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
namespace FTG
{
	//https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
	//https://iquilezles.org/www/articles/normalsSDF/normalsSDF.htm
	public class FTGSDF
	{
		// public static float Torus(Vector3 p, float innerRadius)
		// {
		// 	Vector2 t = new Vector2(0.5f, innerRadius);
		// 	Vector2 q = new Vector2(new Vector2(p.x, p.z).magnitude - t.x, p.y);
		// 	return q.magnitude - t.y;
		// }

		//arc = (sin pi, cos pi) is 360 torus, anything else is capped
		public static float CappedTorus(Vector3 p, in Vector2 arc, in float ra, in float rb)
		{
			p.x = Mathf.Abs(p.x);
			float k = (arc.y * p.x > arc.x * p.y) ? Vector2.Dot(new Vector2(p.x, p.y), arc) : (new Vector2(p.x, p.y)).magnitude;
			return Mathf.Sqrt(Vector3.Dot(p, p) + ra * ra - 2f * ra * k) - rb;
		}

		public static float Cylinder(Vector3 p, float h, float r)
		{
			Vector2 d = Abs(new Vector2((new Vector2(p.x, p.z)).magnitude, p.y)) - new Vector2(h, r);
			return Mathf.Min(Mathf.Max(d.x, d.y), 0f) + (Vector2.Max(d, Vector2.zero)).magnitude;
		}

		public static float Box(Vector3 p, Vector3 b)
		{
			Vector3 q = Abs(p) - b;
			return (Vector3.Max(q, Vector3.zero)).magnitude + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0f);
		}

		public static float Pyramid(Vector3 p, float h)
		{
			float m2 = h * h + 0.25f;
			p = new Vector3(Mathf.Abs(p.x), p.y, Mathf.Abs(p.z));
			p = (p.z > p.x) ? new Vector3(p.z, p.y, p.x) : p;
			p = new Vector3(p.x - 0.5f, p.y, p.z - 0.5f);

			Vector3 q = new Vector3(p.z, h * p.y - 0.5f * p.x, h * p.x + 0.5f * p.y);

			float s = Mathf.Max(-q.x, 0f);
			float t = Mathf.Clamp01((q.y - 0.5f * p.z) / (m2 + 0.25f));
			float a = m2 * (q.x + s) * (q.x + s) + q.y * q.y;
			float b = m2 * (q.x + 0.5f * t) * (q.x + 0.5f * t) + (q.y - m2 * t) * (q.y - m2 * t);

			float d2 = Mathf.Min(q.y, -q.x * m2 - q.y * 0.5f) > 0f ? 0f : Mathf.Min(a, b);

			return Mathf.Sqrt((d2 + q.z * q.z) / m2) * Mathf.Sign(Mathf.Max(q.z, -p.y));
		}
		public static float SolidAngle(Vector3 p, Vector2 c, float ra)
		{
			// c is the sin/cos of the angle
			Vector2 q = new Vector2( ( new Vector2(p.x,p.z)).magnitude, p.y );
			float l = (q).magnitude - ra;
			float m = (q - c*Mathf.Clamp(Vector2.Dot(q,c),0f,ra) ).magnitude;
			return Mathf.Max(l,m*Mathf.Sign(c.y*q.x-c.x*q.y));
		}
		public static float CappedCone(Vector3 p, float h, float r1, float r2)
		{
			Vector2 q = new Vector2((new Vector2(p.x, p.z)).magnitude, p.y);
			Vector2 k1 = new Vector2(r2, h);
			Vector2 k2 = new Vector2(r2 - r1, 2f * h);
			Vector2 ca = new Vector2(q.x - Mathf.Min(q.x, (q.y < 0.0) ? r1 : r2), Mathf.Abs(q.y) - h);
			Vector2 cb = q - k1 + k2 * Mathf.Clamp(Vector2.Dot(k1 - q, k2) / dot2(k2), 0f, 1f);
			float s = (cb.x < 0f && ca.y < 0f) ? -1f : 1f;
			return s * Mathf.Sqrt(Mathf.Min(dot2(ca), dot2(cb)));
		}

		public static Vector3 CalculateNormals(Vector3 pos01, Func<Vector3, float> action)
		{
			float epsilon = 0.5773f * 0.0005f;
			Vector2 e = new Vector2(1.0f, -1.0f);
			return (new Vector3(e.x, e.y, e.y) * action.Invoke(pos01 + new Vector3(e.x, e.y, e.y) * epsilon) +
			        new Vector3(e.y, e.y, e.x) * action.Invoke(pos01 + new Vector3(e.y, e.y, e.x) * epsilon) +
			        new Vector3(e.y, e.x, e.y) * action.Invoke(pos01 + new Vector3(e.y, e.x, e.y) * epsilon) +
			        new Vector3(e.x, e.x, e.x) * action.Invoke(pos01 + new Vector3(e.x, e.x, e.x) * epsilon)).normalized;
		}

		public static Vector3 InitInsideSDF(Func<Vector3, float> func, in float smoothness, out float value)
		{
			int counter = 0;
			Vector3 randomPos;
			do
			{
				randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
				value = func(randomPos);
				counter++;
				if (counter > 50)
				{
					// Debug.Log("max sampling iterations");
					break;
				}
			}
			while (Mathf.Abs(value) > (0.1f + smoothness));
			return randomPos;
		}

		static Vector2 Abs(Vector2 a)
		{
			return new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));
		}
		static Vector3 Abs(Vector3 a)
		{
			return new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
		}
		static float dot2(in Vector2 v) { return Vector2.Dot(v, v); }
		static float dot2(in Vector3 v) { return Vector3.Dot(v, v); }
		static float ndot(in Vector2 a, in Vector2 b) { return a.x * b.x - a.y * b.y; }

	}
}
