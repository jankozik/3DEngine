using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunay {
    using System.Collections.Generic;

    public class Triangulator2 {
        private List<Point3d> m_points = new List<Point3d>();

        public void InitTriangulator(List<Point3d> points, Point3d normal) {
            //var quad = Quaternion.FromToRotation(normal, Point3d.forward);

            //foreach(var v in points)
            //    m_points.Add(quad * v);

            m_points = new List<Point3d>(points);
        }

        public int[] Triangulate(int offset) {
            var indices = new List<int>();

            var n = m_points.Count;
            if(n < 3)
                return indices.ToArray();

            var V = new int[n];
            if(Area() > 0) {
                for(var v = 0; v < n; v++)
                    V[v] = v;
            } else {
                for(var v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            var nv = n;
            var count = 2 * nv;
            var m = 0;
            for(var v = nv - 1; nv > 2; ) {
                if((count--) <= 0)
                    return indices.ToArray();

                var u = v;
                if(nv <= u)
                    u = 0;
                v = u + 1;
                if(nv <= v)
                    v = 0;
                var w = v + 1;
                if(nv <= w)
                    w = 0;

                if(Snip(u, v, w, nv, V)) {
                    int a;
                    int b;
                    int c;
                    int s;
                    int t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(offset + a);
                    indices.Add(offset + b);
                    indices.Add(offset + c);
                    m++;
                    s = v;
                    for(t = v + 1; t < nv; t++) {
                        V[s] = V[t];
                        s++;
                    }
                    nv--;
                    count = 2 * nv;
                }
            }

            return indices.ToArray();
        }

        private double det(double[][] a) {
            return a[0][0] * a[1][1] * a[2][2] + a[0][1] * a[1][2] * a[2][0] + a[0][2] * a[1][0] * a[2][1] - a[0][2] * a[1][1] * a[2][0] - a[0][1] * a[1][0] * a[2][2] - a[0][0] * a[1][2] * a[2][1];
        }

        private Point3d unit_normal(Point3d a, Point3d b, Point3d c) {
            double x = det(new double[][] {
                            new double[]{1,a.Y,a.Z},
                            new double[]{1,b.Y,b.Z},
                            new double[]{1,c.Y,c.Z}});

            double y = det(new double[][]{
                            new double[]{a.X,1,a.Z},
                            new double[]{b.X,1,b.Z},
                            new double[]{c.X,1,c.Z}});

            double z = det(new double[][]{
                            new double[]{a.X,a.Y,1},
                            new double[]{b.X,b.Y,1},
                            new double[]{c.X,c.Y,1}});

            double magnitude = Math.Sqrt((Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)));
            return new Point3d(x / magnitude, y / magnitude, z / magnitude);
        }

        private double Area() {
            //int n = m_points.Count;
            //double A = 0.0;
            //int q = 0;
            //for(var p = n - 1; q < n; p = q++) {
            //    var pval = m_points[p];
            //    var qval = m_points[q];
            //    A += pval.X * qval.Y - qval.X * pval.Y;
            //}
            //return (A * 0.5);

            //Point3d total = new Point3d(0, 0, 0);

            //for(int i = 0; i < m_points.Count; i++) {
            //    Point3d vi1 = m_points[i];
            //    Point3d vi2 = m_points[(i + 1) % m_points.Count];

            //    Point3d prod = vi1.Cross(vi2);

            //    total.X += prod.X;
            //    total.Y += prod.Y;
            //    total.Z += prod.Z;
            //}
            //double result = total.Dot(unit_normal(m_points[0], m_points[1], m_points[2]));
            //return Math.Abs(result / 2);

            // Calculating the area and centroid of a polygon
            // http://paulbourke.net/geometry/polygonmesh/
            int n = m_points.Count;
            int j;
            double area = 0.0;

            for(int i = 0; i < n; i++) {
                j = (i + 1) % n;
                area += m_points[i].X * m_points[j].Y;
                area -= m_points[i].Y * m_points[j].X;
            }
            return area /= 2.0;
        }

        private bool Snip(int u, int v, int w, int n, int[] V) {
            int p;
            var A = m_points[V[u]];
            var B = m_points[V[v]];
            var C = m_points[V[w]];

            if(double.Epsilon > (((B.X - A.X) * (C.Y - A.Y)) - ((B.Y - A.Y) * (C.X - A.X))))
                return false;
            for(p = 0; p < n; p++) {
                if((p == u) || (p == v) || (p == w))
                    continue;
                var P = m_points[V[p]];
                if(InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Point3d A, Point3d B, Point3d C, Point3d P) {
            double ax;
            double ay;
            double bx;
            double by;
            double cx;
            double cy;
            double apx;
            double apy;
            double bpx;
            double bpy;
            double cpx;
            double cpy;
            double cCROSSap;
            double bCROSScp;
            double aCROSSbp;

            ax = C.X - B.X; ay = C.Y - B.Y;
            bx = A.X - C.X; by = A.Y - C.Y;
            cx = B.X - A.X; cy = B.Y - A.Y;
            apx = P.X - A.X; apy = P.Y - A.Y;
            bpx = P.X - B.X; bpy = P.Y - B.Y;
            cpx = P.X - C.X; cpy = P.Y - C.Y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp > 0.0) && (bCROSScp > 0.0) && (cCROSSap > 0.0));
        }
    }
}
