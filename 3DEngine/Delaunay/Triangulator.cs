//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Delaunay {
//    public class Triangulator {
//        private List<Point3d> m_points = new List<Point3d>();

//        public Triangulator(Point3d[] points) {
//            m_points = new List<Point3d>(points);
//        }

//        public int[] Triangulate() {
//            List<int> indices = new List<int>();

//            int n = m_points.Count;
//            if(n < 3)
//                return indices.ToArray();

//            int[] V = new int[n];
//            if(Area() > 0) {
//                for(int v = 0; v < n; v++)
//                    V[v] = v;
//            } else {
//                for(int v = 0; v < n; v++)
//                    V[v] = (n - 1) - v;
//            }

//            int nv = n;
//            int count = 2 * nv;
//            for(int m = 0, v = nv - 1; nv > 2; ) {
//                if((count--) <= 0)
//                    return indices.ToArray();

//                int u = v;
//                if(nv <= u)
//                    u = 0;
//                v = u + 1;
//                if(nv <= v)
//                    v = 0;
//                int w = v + 1;
//                if(nv <= w)
//                    w = 0;

//                if(Snip(u, v, w, nv, V)) {
//                    int a, b, c, s, t;
//                    a = V[u];
//                    b = V[v];
//                    c = V[w];
//                    indices.Add(a);
//                    indices.Add(b);
//                    indices.Add(c);
//                    m++;
//                    for(s = v, t = v + 1; t < nv; s++, t++)
//                        V[s] = V[t];
//                    nv--;
//                    count = 2 * nv;
//                }
//            }

//            indices.Reverse();
//            return indices.ToArray();
//        }

//        // Calculating the area and centroid of a polygon
//        // http://paulbourke.net/geometry/polygonmesh/
//        private double Area() {
//            int n = m_points.Count;
//            int j;
//            double area = 0.0;

//            for(int i = 0; i < n; i++) {
//                j = (i + 1) % n;
//                area += m_points[i].X * m_points[j].Y;
//                area -= m_points[i].Y * m_points[j].X;
//            }
//            return area /= 2.0;
//        }

//        private bool Snip(int u, int v, int w, int n, int[] V) {
//            int p;
//            Point3d A = m_points[V[u]];
//            Point3d B = m_points[V[v]];
//            Point3d C = m_points[V[w]];
//            if(double.Epsilon > (((B.X - A.X) * (C.Y - A.Y)) - ((B.Y - A.Y) * (C.X - A.X))))
//                return false;
//            for(p = 0; p < n; p++) {
//                if((p == u) || (p == v) || (p == w))
//                    continue;
//                Point3d P = m_points[V[p]];
//                if(InsideTriangle(A, B, C, P))
//                    return false;
//            }
//            return true;
//        }

//        private bool InsideTriangle(Point3d A, Point3d B, Point3d C, Point3d P) {
//            double ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
//            double cCROSSap, bCROSScp, aCROSSbp;

//            ax = C.X - B.X; ay = C.Y - B.Y;
//            bx = A.X - C.X; by = A.Y - C.Y;
//            cx = B.X - A.X; cy = B.Y - A.Y;
//            apx = P.X - A.X; apy = P.Y - A.Y;
//            bpx = P.X - B.X; bpy = P.Y - B.Y;
//            cpx = P.X - C.X; cpy = P.Y - C.Y;

//            aCROSSbp = ax * bpy - ay * bpx;
//            cCROSSap = cx * apy - cy * apx;
//            bCROSScp = bx * cpy - by * cpx;

//            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
//        }
//    }
//}
