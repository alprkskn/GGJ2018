using System;
using UnityEngine;
using System.Collections.Generic;

namespace GGJ2018.Utils
{
    public static class PathUtils
    {
        // Gives the instersection point of a plane and AB line if it exists.
        public static Vector3? LineSegmentPlaneIntersection(Vector3 A, Vector3 B, Plane P)
        {
            Vector3 po = -P.normal * P.distance;
            Vector3 lineVec = B - A;
            float lineLen = lineVec.magnitude;
            lineVec.Normalize();

            //float dotPoPlane = Vector3.Dot((po - A), P.normal);
            float dotPoPlane = (po.x - A.x) * P.normal.x + (po.y - A.y) * P.normal.y + (po.z - A.z) * P.normal.z;

            //float dotLinePlane = Vector3.Dot((B - A).normalized, P.normal);
            float dotLinePlane = lineVec.x * P.normal.x + lineVec.y * P.normal.y + lineVec.z * P.normal.z;

            if (dotLinePlane == 0)
            {
                return null;
            }

            float d = dotPoPlane / dotLinePlane;

            //if (d < 0 || d > (B - A).magnitude)
            if (d < 0 || d > lineLen)
            {
                return null;
            }

            //return A + d * (B - A).normalized;
            return A + d * lineVec;
        }

        public static Vector3[] ClipLineSegmentIntoViewport(Vector3 A, Vector3 B, Camera C)
        {
            int dummy;
            return ClipLineSegmentIntoViewport(A, B, C, out dummy);
        }

        // Returns End points of a clipped line according to the given camera's viewport.
        // Status indicates if the given original nodes is outside or inside the viewport.
        // 0 : Both A and B are outside.
        // 1 : A is inside B is outside.
        // 2 : A is outside B is inside.
        // 3 : Both A and B are inside.
        public static Vector3[] ClipLineSegmentIntoViewport(Vector3 A, Vector3 B, Camera C, out int status)
        {
            int count = 0;
            int index = 0;
            status = 0;
            Vector3[] result = new Vector3[2];

            // Check if any of the points fall into the viewport
            Vector2 aProjected = C.WorldToViewportPoint(A);
            if (!((aProjected.x < -0.01f || aProjected.x > 1.01f) || (aProjected.y < -0.01f || aProjected.y > 1.01f)))
            {
                status |= 1;
                result[0] = A;
                count++;
                index++;
            }

            Vector2 bProjected = C.WorldToViewportPoint(B);
            if (!((bProjected.x < -0.01f || bProjected.x > 1.01f) || (bProjected.y < -0.01f || bProjected.y > 1.01f)))
            {
                status |= 2;
                result[1] = B;
                count++;
            }


            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(C);

            int i = 0;
            foreach (Plane p in frustumPlanes)
            {
                if (count == 2)
                    break;


                // Skip calculation for near and far plane.
                if (p.normal == C.transform.forward)
                    continue;
                else if ((p.normal + C.transform.forward).sqrMagnitude < 0.0001f)
                    continue;


                Vector3? intersection = LineSegmentPlaneIntersection(A, B, p);

                if (intersection.HasValue)
                {
                    Vector2 project = C.WorldToViewportPoint(intersection.Value);
                    if ((project.x < -0.01f || project.x > 1.01f) || (project.y < -0.01f || project.y > 1.01f))
                    {
                        continue;
                    }

                    result[index++] = intersection.Value;
                    count++;
                }
                i++;
            }

            if (count < 2)
            {
                return null;
            }

            //if (Vector3.Dot(result[1] - result[0], B - A) < 0)
            if ((result[1].x - result[0].x) * (B.x - A.x) + (result[1].y - result[0].y) * (B.y - A.y) + (result[1].z - result[0].z) * (B.z - A.z) < 0)
            {
                Vector3 temp = result[0];
                result[0] = result[1];
                result[1] = temp;
            }

            //Debug.Log(result[0] + " - " + result[1]);

            return result;
        }

        public static Vector3 PositionOnLine(Vector3 A, Vector3 B, float distance)
        {
            Vector3 n = (B - A);
            n.Normalize();

            return A + distance * n;
        }

        public static Vector2 PositionOnLine(Vector2 A, Vector2 B, float distance)
        {
            Vector2 n = (B - A);
            n.Normalize();

            return A + distance * n;
        }

        public static float Vec2Cross(Vector3 A, Vector3 B)
        {
            return (A.x * B.z) - (A.z * B.x);
        }

        public static float Vec2Cross(Vector2 A, Vector2 B)
        {
            return (A.x * B.y) - (A.y * B.x);
        }

        //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
        //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
        //same plane, use ClosestPointsOnTwoLines() instead.
        public static bool LineIntersection3D(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {

            intersection = Vector3.zero;

            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //Lines are not coplanar. Take into account rounding errors.
            if ((planarFactor >= 0.00001f) || (planarFactor <= -0.00001f))
            {

                return false;
            }

            //Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

            if ((s >= 0.0f) && (s <= 1.0f))
            {

                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }

            else
            {
                return false;
            }
        }

        //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
        //to each other. This function finds those two points. If the lines are not parallel, the function 
        //outputs true, otherwise false.
        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {

            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {

                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }

            else
            {
                return false;
            }
        }

        // Check if two given collinear line segments overlap in 2D
        public static bool CollinearLinesOverlap(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
        {
            float minV, maxV, minU, maxU;

            bool pickZ = (A.x == B.x);

            if (!pickZ)
            {
                if (A.x < B.x)
                {
                    minU = A.x;
                    maxU = B.x;
                }
                else
                {
                    minU = B.x;
                    maxU = A.x;
                }

                if (C.x < D.x)
                {
                    minV = C.x;
                    maxV = D.x;
                }
                else
                {
                    minV = D.x;
                    maxV = C.x;
                }
            }
            else
            {
                if (A.z < B.z)
                {
                    minU = A.z;
                    maxU = B.z;
                }
                else
                {
                    minU = B.z;
                    maxU = A.z;
                }

                if (C.z < D.z)
                {
                    minV = C.z;
                    maxV = D.z;
                }
                else
                {
                    minV = D.z;
                    maxV = C.z;
                }
            }

            if(maxU - minV > 0 && maxV - minU > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Returns intersection point on 2D(with a Vector3(x, 0, z).
        // Intersection point if lines intersect.
        // Vector3.one * float.PositiveInfinity if lines are collinear.
        // Vector3.one * flaot.NegativeInfinity if lines do not intersect.

        public static bool LineIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 D, out Vector3 intPoint, bool exclusive = false)
        {
            Vector3 dummyNormal = Vector3.zero;
            return LineIntersection(A, B, C, D, true, out intPoint, false, out dummyNormal, exclusive);
        }

        public static bool LineIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 D, out Vector3 intPoint, out Vector3 intNormal, bool exclusive = false)
        {
            return LineIntersection(A, B, C, D, true, out intPoint, true, out intNormal, exclusive);
        }

        public static bool LineIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 D, bool calculatePoint, out Vector3 intPoint, bool calculateNormal, out Vector3 intNormal, bool exclusive = false)
        {
            intPoint = Vector3.zero;
            intNormal = Vector3.zero;
            float bax = B.x - A.x;
            float bay = B.z - A.z;
            float dcx = D.x - C.x;
            float dcy = D.z - C.z;
            float cax = C.x - A.x;
            float cay = C.z - A.z;

            float cross1 = (bax * dcy) - (bay * dcx);
            float cross2 = (cay * bax) - (cax * bay);

            if (cross1 == 0)
            {
                if (cross2 == 0)
                {
                    return CollinearLinesOverlap(A, B, C, D);
                }

                return false;
            }

            dcx /= cross1;
            dcy /= cross1;

            bax /= cross1;
            bay /= cross1;

            float t = (cax * dcy) - (cay * dcx);
            float u = (cax * bay) - (cay * bax);

            if (!exclusive)
            {
                if ((t >= -0.001f && t <= 1.001f) && (u >= -0.001f && u <= 1.001f))
                {
                    if (calculatePoint)
                    {
                        Vector2 iP = new Vector2(A.x + t * (B.x - A.x), A.z + t * (B.z - A.z));
                        intPoint = new Vector3(iP.x, A.y + (B.y - A.y) * t, iP.y);
                    }
                    if (calculateNormal)
                    {
                        intNormal = NormalOfLine(A, B);
                    }
                    return true;
                }
            }
            else
            {
                if((t > .05f && t < .95f) && (u > .05f && u < .95f))
                {
                    if (calculatePoint)
                    {
                        Vector2 iP = new Vector2(A.x + t * (B.x - A.x), A.z + t * (B.z - A.z));
                        intPoint = new Vector3(iP.x, A.y + (B.y - A.y) * t, iP.y);
                    }
                    if (calculateNormal)
                    {
                        intNormal = NormalOfLine(C, D);
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool LineIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 D, bool exclusive = false)
        {
            Vector3 dummyPoint = Vector3.zero;
            Vector3 dummNormal = Vector3.zero;
            return LineIntersection(A, B, C, D, false, out dummyPoint, false, out dummNormal, exclusive);
        }

        public static Vector3 NormalOfLine(Vector3 A, Vector3 B)
        {
            float dx = B.x - A.x;
            float dy = B.z - A.z;

            Vector3 norm = new Vector3(-dy, 0, dx);
            norm.Normalize();

            return norm;
        }

        public static GameObject CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width)
        {
            Vector3 offset = end - start;
            Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
            Vector3 position = start + (offset / 2.0f);

            GameObject cylinder = GameObject.Instantiate(Resources.Load("Path"), position, Quaternion.identity) as GameObject;
            if (cylinder == null) Debug.Log(offset);
            cylinder.transform.up = offset;
            cylinder.transform.localScale = scale;
            return cylinder;
        }

        public static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
                System.Array.Copy(oldArray, newArray, preserveLength);
            return newArray;
        }

        // Computes a circle containing both nodes and sets the "center" and "radius" for the polygon.
        public static void DefCircleWith2Points(Vector3 A, Vector3 B, out Vector3 center, out float radius)
        {
            center = (A + B) / 2f;
            center.y = 0; // To keep all calculations on 2D.
            radius = (B - A).magnitude / 2f;
        }

        // Computes a circle containing all three nodes and sets the "center" and "radius" for the polygon.
        public static void DefCircleWith3Points(Vector3 A, Vector3 B, Vector3 C, out Vector3 center, out float radius)
        {
            // This function uses the definition of a circumcircle of a triangle
            // in order to find a unique circle for 3 given points.
            // By this way, it makes sure that the circle contains each node.
            // We are going to be using x and z axes to keep it in 2D.

            // First we calculate the perpendicular bisector of AB.
            float b1x = (A.x + B.x) / 2f;
            float b1y = (A.z + B.z) / 2f;
            float slopeAB = (B.z - A.z) / (B.x - A.x);
            float negAB = -(1f / slopeAB);

            float b1 = b1y - negAB * b1x; // Equation for 1st bisector is : y = negAB*x + b1


            // Then calculate the perp. bisect. of BC.
            float b2x = (B.x + C.x) / 2f;
            float b2y = (B.z + C.z) / 2f;
            float slopeBC = (C.z - B.z) / (C.x - B.x);
            float negBC = -(1f / slopeBC);

            float b2 = b2y - negBC * b2x; // Equation for 2nd bisector is : y = negBC*x + b2

            // Now calculate the intersection with 2 bisector formulas.

            float x = (b2 - b1) / (negAB - negBC);
            float y = negAB * x + b1;

            center = new Vector3(x, 0, y);
            radius = (center - A).magnitude;
        }

        public static float Distance2D(Vector3 A, Vector3 B)
        {
            float dx = B.x - A.x;
            float dy = B.z - A.z;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static bool IsPointOnLine(Vector3 point, Vector3 A, Vector3 B)
        {
            // AB Line x and y values.
            float lx = B.x - A.x;
            float ly = B.z - A.z;

            // Point - A, Delta vector.
            float dx = point.x - A.x;
            float dy = point.z - A.z;

            // Length of delta vector.
            float dl = Mathf.Sqrt(dx * dx + dy * dy);

            // Length of AB line.
            float ll = Mathf.Sqrt(lx * lx + ly * ly);

            // Normalize delta vector.
            dx /= dl;
            dy /= dl;
            // Normalize AB vector.
            lx /= ll;
            ly /= ll;

            float dot = (dx * lx) + (dy * ly);

            if (Mathf.Abs(1f - dot) > .00001f)
            {
                return false;
            }

            // if we are here, point is on the infinite line
            // but, we should check whether it between A and B

            if(ll < dl)
            {
                return false;
            }

            return true;
        }

        public static bool GetProjectPointOnLine(Vector3 point, Vector3 A, Vector3 B, out Vector3 projectedPoint)
        {
            projectedPoint = Vector3.zero;


            // AB Line x and y values.
            float lx = B.x - A.x;
            float ly = B.z - A.z;

            // Point - A, Delta vector.
            float dx = point.x - A.x;
            float dy = point.z - A.z;

            // Length of AB vector.
            float ll = Mathf.Sqrt(lx * lx + ly * ly);

            // Normalize AB vector.
            lx /= ll;
            ly /= ll;

            float dot = (dx * lx) + (dy * ly);

            if (dot < 0f)
            {
                projectedPoint = A;
                return false;
            }

            if (dot > ll)
            {
                projectedPoint = B;
                return false;
            }

            Vector2 projectedPoint2D = new Vector2(A.x + lx * dot, A.z + ly * dot);
            projectedPoint = new Vector3(projectedPoint2D.x, A.y + (B.y - A.y) * (dot / ll) , projectedPoint2D.y);

            return true;
       }

        public static float SquareDistanceToLineSegment3D(Vector3 point, Vector3 A, Vector3 B)
        {
            Vector3 project = Vector3.zero;
            float dist = float.MaxValue;

            if (GetProjectPointOnLine(point, A, B, out project))
            {
                float dx = point.x - project.x;
                float dy = point.y - project.y;
                float dz = point.z - project.z;

                dist = dx * dx + dy * dy * dz * dz;
            }
            else
            {
                float dx = point.x - A.x;
                float dy = point.y - A.y;
                float dz = point.z - A.z;
                float dist2A = dx * dx + dy * dy + dz * dz;

                dx = point.x - B.x;
                dy = point.y - B.y;
                dz = point.z - B.z;
                float dist2B = dx * dx + dy * dy + dz * dz;

                dist = (dist2A < dist2B) ? dist2A : dist2B;
            }

            return dist;
        }

        public static float DistanceToLineSegment3D(Vector3 point, Vector3 A, Vector3 B)
        {
            return Mathf.Sqrt(SquareDistanceToLineSegment3D(point, A, B));
        }

        public static float SquareDistanceToLineSegment2D(Vector3 point, Vector3 A, Vector3 B)
        {
            Vector3 project = Vector3.zero;
            float dist = float.MaxValue;

            if (GetProjectPointOnLine(point, A, B, out project))
            {
                float dx = point.x - project.x;
                float dy = point.z - project.z;

                dist = dx * dx + dy * dy;
            }
            else
            {
                float dx = point.x - A.x;
                float dy = point.z - A.z;

                float dist2A = dx * dx + dy * dy;

                dx = point.x - B.x;
                dy = point.z - B.z;

                float dist2B = dx * dx + dy * dy;

                dist = (dist2A < dist2B) ? dist2A : dist2B;
            }

            return dist;
        }

        public static float DistanceToLineSegment2D(Vector3 point, Vector3 A, Vector3 B)
        {
            return Mathf.Sqrt(SquareDistanceToLineSegment2D(point, A, B));
        }

        public static float GetSquareDistanceBetweenPoints2D(Vector3 A, Vector3 B)
        {
            float dx = A.x - B.x;
            float dy = A.z - B.z;

            return dx * dx + dy * dy;
        }

        public static float GetSquareDistanceBetweenPoints3D(Vector3 A, Vector3 B)
        {

            float dx = A.x - B.x;
            float dy = A.y - B.y;
            float dz = A.z - B.z;

            return dx * dx + dy * dy + dz * dz;
        }

        #region EXTENSION METHODS

        public static float LengthInMeters(this List<Vector3> positions)
        {
            float length = 0f;
            int count = positions.Count;
            for ( int i = 0; i < count - 1; i++ )
            {
                length += Vector3.Distance(positions[i], positions[i + 1]);
            }

            return length;
        }

        public static Vector3 GetCartesianPositionOfGivenOffsetOnPath(this List<Vector3> positions, float offset)
        {
            float offsetLeft = offset;
            int count = positions.Count;
            for (int i = 0; i < count - 1; i++)
            {
                float lineSegmentLength = Vector3.Distance(positions[i], positions[i + 1]);
                if (offsetLeft > lineSegmentLength)
                {
                    offsetLeft -= lineSegmentLength; 
                    continue;
                }

                float ratio = offsetLeft / lineSegmentLength;
                return positions[i] * (1 - ratio) + positions[i + 1] * ratio;
            }

            return Vector3.zero;
        }

        #endregion


        public static IEnumerable<Vector2> PointsInTriangle2(Vector2 vt1, Vector2 vt2, Vector2 vt3)
        {
            var minX = (int)Math.Min(vt1.x, Math.Min(vt2.x, vt3.x));
            var maxY = (int)Math.Max(vt1.y, Math.Max(vt2.y, vt3.y));
            var minY = (int)Math.Min(vt1.y, Math.Min(vt2.y, vt3.y));
            var maxX = (int)Math.Max(vt1.x, Math.Max(vt2.x, vt3.x));

            var vs1 = new Vector2(vt2.x - vt1.x, vt2.y - vt1.y);
            var vs2 = new Vector2(vt3.x - vt1.x, vt3.y - vt1.y);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var q = new Vector2(x - vt1.x, y - vt1.y);

                    float s = (float)CrossProduct(q, vs2) / CrossProduct(vs1, vs2);
                    float t = (float)CrossProduct(vs1, q) / CrossProduct(vs1, vs2);

                    if ((s >= 0) && (t >= 0) && (s + t <= 1))
                    { /* inside triangle */
                        yield return new Vector2(x, y);
                    }
                }
            }
        }

        private static float CrossProduct(Vector2 v1, Vector2 v2)
        {
            return (v1.x*v2.y) - (v1.y*v2.x);
        }

    }
}