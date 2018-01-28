using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2018.Utils
{
	public static class LineSimplifier
	{
		public static List<Vector3> SimplifyLine(List<Vector3> points, float minArea)
		{
			// This algorithm requires more than 3 points
			if(points.Count < 3)
				return points;

			points = new List<Vector3>(points); // Shallow clone the list

			while(true)
			{
				float smallestArea = float.MaxValue;
				int smallestAreaI = 1;

				for(int i = 1; i < points.Count - 1; i++)
				{
					float nextArea = TriangleArea(points[i - 1], points[i], points[i + 1]);
					if(nextArea < smallestArea) {
						smallestArea = nextArea;
						smallestAreaI = i;
					}
				}


				if(smallestArea >= minArea || points.Count <= 3)
					break;

				// Remove the central point of the smallest triangle
				points.RemoveAt(smallestAreaI);
			}

			return points;
		}

		public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
		{
			return Math.Abs(
				(
					(float)a.x * (b.z - c.z) +
					(float)b.x * (c.z - a.z) +
					(float)c.x * (a.z - b.z)
				) / 2f
			);
		}
	}
}
