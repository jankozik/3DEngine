using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunay {

    public static class SutherlandHodgman {
        #region Class: Edge

        /// <summary>
        /// This represents a line segment
        /// </summary>
        private class Edge {
            public Edge(Point3d from, Point3d to) {
                this.From = from;
                this.To = to;
            }

            public readonly Point3d From;
            public readonly Point3d To;
        }

        #endregion

        /// <summary>
        /// This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
        /// </summary>
        /// <remarks>
        /// Based on the psuedocode from:
        /// http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman
        /// </remarks>
        /// <param name="subjectPoly">Can be concave or convex</param>
        /// <param name="clipPoly">Must be convex</param>
        /// <returns>The intersection of the two polygons (or null)</returns>
        public static Point3d[] GetIntersectedPolygon(Point3d[] subjectPoly, Point3d[] clipPoly) {
            if(subjectPoly.Length < 3 || clipPoly.Length < 3) {
                throw new ArgumentException(string.Format("The polygons passed in must have at least 3 points: subject={0}, clip={1}", subjectPoly.Length.ToString(), clipPoly.Length.ToString()));
            }

            List<Point3d> outputList = subjectPoly.ToList();

            //	Make sure it's clockwise
            bool? r = IsClockwise(subjectPoly);
            if(r == null) return outputList.ToArray();
            if(!r.Value) outputList.Reverse();

            //	Walk around the clip polygon clockwise
            foreach(Edge clipEdge in IterateEdgesClockwise(clipPoly)) {
                List<Point3d> inputList = outputList.ToList();		//	clone it
                outputList.Clear();

                if(inputList.Count == 0) {
                    //	Sometimes when the polygons don't intersect, this list goes to zero.  Jump out to avoid an index out of range exception
                    break;
                }

                Point3d S = inputList[inputList.Count - 1];

                foreach(Point3d E in inputList) {
                    if(IsInside(clipEdge, E)) {
                        if(!IsInside(clipEdge, S)) {
                            Point3d point = GetIntersect(S, E, clipEdge.From, clipEdge.To);
                            if(point == null) {
                                //System.Diagnostics.Debug.WriteLine("Line segments don't intersect");
                                return outputList.ToArray();
                                //throw new ApplicationException("Line segments don't intersect");		//	may be collinear, or may be a bug
                            } else {
                                outputList.Add(point);
                            }
                        }

                        outputList.Add(E);
                    } else if(IsInside(clipEdge, S)) {
                        Point3d point = GetIntersect(S, E, clipEdge.From, clipEdge.To);
                        if(point == null) {
                            //System.Diagnostics.Debug.WriteLine("Line segments don't intersect");
                            return outputList.ToArray();
                            //throw new ApplicationException("Line segments don't intersect");		//	may be collinear, or may be a bug
                        } else {
                            outputList.Add(point);
                        }
                    }

                    S = E;
                }
            }

            //	Exit Function
            return outputList.ToArray();
        }

        #region Private Methods

        /// <summary>
        /// This iterates through the edges of the polygon, always clockwise
        /// </summary>
        private static IEnumerable<Edge> IterateEdgesClockwise(Point3d[] polygon) {
            bool? r = IsClockwise(polygon);
            //if(r == null) yield return null;

            if((r == null) || r.Value) {
                for(int cntr = 0; cntr < polygon.Length - 1; cntr++) {
                    yield return new Edge(polygon[cntr], polygon[cntr + 1]);
                }

                yield return new Edge(polygon[polygon.Length - 1], polygon[0]);
            } else {
                for(int cntr = polygon.Length - 1; cntr > 0; cntr--) {
                    yield return new Edge(polygon[cntr], polygon[cntr - 1]);
                }

                yield return new Edge(polygon[0], polygon[polygon.Length - 1]);
            }
        }

        /// <summary>
        /// Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines)
        /// </summary>
        /// <remarks>
        /// Got this here:
        /// http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
        /// </remarks>
        private static Point3d GetIntersect_Original_For2D(Point3d line1From, Point3d line1To, Point3d line2From, Point3d line2To) {
            Point3d direction1 = line1To - line1From;
            Point3d direction2 = line2To - line2From;
            double dotPerp = (direction1.X * direction2.Y) - (direction1.Y * direction2.X);

            // If it's 0, it means the lines are parallel so have infinite intersection points
            if(IsNearZero(dotPerp)) {
                return null;
            }

            Point3d c = line2From - line1From;
            double t = (c.X * direction2.Y - c.Y * direction2.X) / dotPerp;

            return line1From + (t * direction1);
        }

        /// <summary>
        /// Calculates the intersection line segment between 2 lines (not segments).
        /// Returns the intersecting point is such exists.
        /// http://paulbourke.net/geometry/pointlineplane/
        /// </summary>
        /// <returns></returns>
        public static Point3d GetIntersect(Point3d line1From, Point3d line1To,
                                                            Point3d line2From, Point3d line2To) {
            // Algorithm is ported from the C algorithm of 
            // Paul Bourke at http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
            Point3d resultLineFrom = new Point3d();
            Point3d resultLineTo = new Point3d();

            Point3d p1 = line1From;
            Point3d p2 = line1To;
            Point3d p3 = line2From;
            Point3d p4 = line2To;
            Point3d p13 = p1 - p3;
            Point3d p43 = p4 - p3;

            if(p43.Length() < double.Epsilon) {
                return null;
            }
            Point3d p21 = p2 - p1;
            if(p21.Length() < double.Epsilon) {
                return null;
            }

            double d1343 = p13.X * (double)p43.X + (double)p13.Y * p43.Y + (double)p13.Z * p43.Z;
            double d4321 = p43.X * (double)p21.X + (double)p43.Y * p21.Y + (double)p43.Z * p21.Z;
            double d1321 = p13.X * (double)p21.X + (double)p13.Y * p21.Y + (double)p13.Z * p21.Z;
            double d4343 = p43.X * (double)p43.X + (double)p43.Y * p43.Y + (double)p43.Z * p43.Z;
            double d2121 = p21.X * (double)p21.X + (double)p21.Y * p21.Y + (double)p21.Z * p21.Z;

            double denom = d2121 * d4343 - d4321 * d4321;
            if(Math.Abs(denom) < double.Epsilon) {
                return null;
            }
            double numer = d1343 * d4321 - d1321 * d4343;

            double mua = numer / denom;
            double mub = (d1343 + d4321 * (mua)) / d4343;

            resultLineFrom.X = p1.X + mua * p21.X;
            resultLineFrom.Y = p1.Y + mua * p21.Y;
            resultLineFrom.Z = p1.Z + mua * p21.Z;
            resultLineTo.X = p3.X + mub * p43.X;
            resultLineTo.Y = p3.Y + mub * p43.Y;
            resultLineTo.Z = p3.Z + mub * p43.Z;

            if(IsNearZero(resultLineFrom.Distance(resultLineTo))) {
                return resultLineFrom;
            } else
                return null;
        }

        /// <summary>
        /// Calculates the intersection line segment between 2 lines (not segments).
        /// Returns false if no solution can be found.
        /// http://paulbourke.net/geometry/pointlineplane/
        /// </summary>
        /// <returns></returns>
        public static bool GetIntersectBestMatch(Point3d line1From, Point3d line1To,
                                                            Point3d line2From, Point3d line2To,
                                                            out Point3d resultLineFrom, out Point3d resultLineTo) {
            // Algorithm is ported from the C algorithm of 
            // Paul Bourke at http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
            resultLineFrom = new Point3d();
            resultLineTo = new Point3d();

            Point3d p1 = line1From;
            Point3d p2 = line1To;
            Point3d p3 = line2From;
            Point3d p4 = line2To;
            Point3d p13 = p1 - p3;
            Point3d p43 = p4 - p3;

            if(p43.Length() < double.Epsilon) {
                return false;
            }
            Point3d p21 = p2 - p1;
            if(p21.Length() < double.Epsilon) {
                return false;
            }

            double d1343 = p13.X * (double)p43.X + (double)p13.Y * p43.Y + (double)p13.Z * p43.Z;
            double d4321 = p43.X * (double)p21.X + (double)p43.Y * p21.Y + (double)p43.Z * p21.Z;
            double d1321 = p13.X * (double)p21.X + (double)p13.Y * p21.Y + (double)p13.Z * p21.Z;
            double d4343 = p43.X * (double)p43.X + (double)p43.Y * p43.Y + (double)p43.Z * p43.Z;
            double d2121 = p21.X * (double)p21.X + (double)p21.Y * p21.Y + (double)p21.Z * p21.Z;

            double denom = d2121 * d4343 - d4321 * d4321;
            if(Math.Abs(denom) < double.Epsilon) {
                return false;
            }
            double numer = d1343 * d4321 - d1321 * d4343;

            double mua = numer / denom;
            double mub = (d1343 + d4321 * (mua)) / d4343;

            resultLineFrom.X = p1.X + mua * p21.X;
            resultLineFrom.Y = p1.Y + mua * p21.Y;
            resultLineFrom.Z = p1.Z + mua * p21.Z;
            resultLineTo.X = p3.X + mub * p43.X;
            resultLineTo.Y = p3.Y + mub * p43.Y;
            resultLineTo.Z = p3.Z + mub * p43.Z;

            return true;
        }

        private static bool IsInside(Edge edge, Point3d test) {
            bool? isLeft = IsLeftOf(edge, test);
            if(isLeft == null) {
                //	Collinear points should be considered inside
                return true;
            }

            return !isLeft.Value;
        }

        private static bool? IsClockwise(Point3d[] polygon) {
            for(int cntr = 2; cntr < polygon.Length; cntr++) {
                bool? isLeft = IsLeftOf(new Edge(polygon[0], polygon[1]), polygon[cntr]);
                if(isLeft != null)		//	some of the points may be collinear.  That's ok as long as the overall is a polygon
                {
                    return !isLeft.Value;
                }
            }

            //System.Diagnostics.Debug.WriteLine("All the points in the polygon are collinear");

            return null;
            //throw new ArgumentException("All the points in the polygon are collinear");
        }

        /// <summary>
        /// Tells if the test point lies on the left side of the edge line
        /// </summary>
        private static bool? IsLeftOf(Edge edge, Point3d test) {
            Point3d tmp1 = edge.To - edge.From;
            Point3d tmp2 = test - edge.To;

            double x = (tmp1.X * tmp2.Y) - (tmp1.Y * tmp2.X);		//	dot product of perpendicular?
            // ^^ OP, ME: No, This is the sign of the determinant: http://en.wikipedia.org/wiki/Curve_orientation

            if(x < 0) {
                return false;
            } else if(x > 0) {
                return true;
            } else {
                //	Collinear points;
                return null;
            }
        }

        private static bool IsNearZero(double testValue) {
            return Math.Abs(testValue) <= .000000001;
        }

        #endregion
    }
}
