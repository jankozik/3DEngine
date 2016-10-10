using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunay {
    public class Tetrahedron {
        // Store and order the four vertices
        public Point3d[] vertices;
        public Point3d o;      // External center of ¥
        public double r;      // External radius of ¥

        public Tetrahedron(Point3d[] v) {
            this.vertices = v;
            getCenterCircumcircle();
        }

        public Tetrahedron(Point3d v1, Point3d v2, Point3d v3, Point3d v4) {
            this.vertices = new Point3d[4];
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
            vertices[3] = v4;
            getCenterCircumcircle();
        }

        public bool equals(Tetrahedron t) {
            int count = 0;
            foreach(Point3d p1 in this.vertices) {
                foreach(Point3d p2 in t.vertices) {
                    if(p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z) {
                        count++;
                    }
                }
            }
            if(count == 4) return true;
            return false;
        }

        public Line[] getLines() {
            Point3d v1 = vertices[0];
            Point3d v2 = vertices[1];
            Point3d v3 = vertices[2];
            Point3d v4 = vertices[3];

            Line[] lines = new Line[6];

            lines[0] = new Line(v1, v2);
            lines[1] = new Line(v1, v3);
            lines[2] = new Line(v1, v4);
            lines[3] = new Line(v2, v3);
            lines[4] = new Line(v2, v4);
            lines[5] = new Line(v3, v4);
            return lines;
        }

        // 外接円も求めちゃう
        private void getCenterCircumcircle() {
            Point3d v1 = vertices[0];
            Point3d v2 = vertices[1];
            Point3d v3 = vertices[2];
            Point3d v4 = vertices[3];

            double[][] A = new double[][] {
                                new double[]{v2.X - v1.X, v2.Y-v1.Y, v2.Z-v1.Z},
                                new double[]{v3.X - v1.X, v3.Y-v1.Y, v3.Z-v1.Z},
                                new double[]{v4.X - v1.X, v4.Y-v1.Y, v4.Z-v1.Z}
                            };
            double[] b = {
                                0.5 * (v2.X*v2.X - v1.X*v1.X + v2.Y*v2.Y - v1.Y*v1.Y + v2.Z*v2.Z - v1.Z*v1.Z),
                                0.5 * (v3.X*v3.X - v1.X*v1.X + v3.Y*v3.Y - v1.Y*v1.Y + v3.Z*v3.Z - v1.Z*v1.Z),
                                0.5 * (v4.X*v4.X - v1.X*v1.X + v4.Y*v4.Y - v1.Y*v1.Y + v4.Z*v4.Z - v1.Z*v1.Z)
                            };

            double[] x = new double[3];
            if(gauss(A, b, x) == 0) {
                o = null;
                r = -1;
            } else {
                o = new Point3d((double)x[0], (double)x[1], (double)x[2]);
                r = o.Distance(v1);
            }
        }

        // Solution of equations by LU decomposition
        private double lu(double[][] a, int[] ip) {
            int n = a.Length;
            double[] weight = new double[n];

            for(int k = 0; k < n; k++) {
                ip[k] = k;
                double u = 0;
                for(int j = 0; j < n; j++) {
                    double t = Math.Abs(a[k][j]);
                    if(t > u) u = t;
                }
                if(u == 0) return 0;
                weight[k] = 1 / u;
            }

            double det = 1;
            for(int k = 0; k < n; k++) {
                double u = -1;
                int m = 0;
                for(int i = k; i < n; i++) {
                    int ii = ip[i];
                    double t = Math.Abs(a[ii][k]) * weight[ii];
                    if(t > u) { u = t; m = i; }
                }
                int ik = ip[m];
                if(m != k) {
                    ip[m] = ip[k]; ip[k] = ik;
                    det = -det;
                }
                u = a[ik][k]; det *= u;
                if(u == 0) return 0;
                for(int i = k + 1; i < n; i++) {
                    int ii = ip[i];
                    double t = (a[ii][k] /= u);
                    for(int j = k + 1; j < n; j++) a[ii][j] -= t * a[ik][j];
                }
            }
            return det;
        }

        private void solve(double[][] a, double[] b, int[] ip, double[] x) {
            int n = a.Length;
            for(int i = 0; i < n; i++) {
                int ii = ip[i];
                double t = b[ii];
                for(int j = 0; j < i; j++) t -= a[ii][j] * x[j];
                x[i] = t;
            }
            for(int i = n - 1; i >= 0; i--) {
                double t = x[i];
                int ii = ip[i];
                for(int j = i + 1; j < n; j++) t -= a[ii][j] * x[j];
                x[i] = t / a[ii][i];
            }
        }

        private double gauss(double[][] a, double[] b, double[] x) {
            int[] ip = new int[a.Length];
            double det = lu(a, ip);

            if(det != 0) { solve(a, b, ip, x); }
            return det;
        }
    }
}
