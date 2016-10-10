// xFX JumpStart
// Xavier Flix (http://whenimbored.xfx.net)
// 2013 - 2016
//
// ================================================================
//
// This project is based on the following work: 
//
// ================================================================
//
// Simulation of a Wireframe Cube using GDI+
// Developed by leonelmachava <leonelmachava@gmail.com>
// http://codentronix.com
//
// Copyright (c) 2011 Leonel Machava
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this 
// software and associated documentation files (the "Software"), to deal in the Software 
// without restriction, including without limitation the rights to use, copy, modify, 
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to the following 
// conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies 
// or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

// http://paulbourke.net/geometry/polygonmesh/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// http://www.openprocessing.org/sketch/31295

namespace Delaunay {
    public class Triangualtor {
        public List<Tetrahedron> tetras;   // List of Tetrahedrons

        public List<Line> edges;

        public List<Line> surfaceEdges;
        public List<Triangle> triangles;

        public Triangualtor() {
            tetras = new List<Tetrahedron>();
            edges = new List<Line>();
            surfaceEdges = new List<Line>();
            triangles = new List<Triangle>();
        }

        public void SetData(List<Point3d> seq) {

            tetras.Clear();
            edges.Clear();

            //   1: Obtain a tetrahedron that includes the point group
            // 1-1: Obtain a sphere including point clouds
            Point3d vMax = new Point3d(-999, -999, -999);
            Point3d vMin = new Point3d(999, 999, 999);
            foreach(Point3d v in seq) {
                if(vMax.X < v.X) vMax.X = v.X;
                if(vMax.Y < v.Y) vMax.Y = v.Y;
                if(vMax.Z < v.Z) vMax.Z = v.Z;
                if(vMin.X > v.X) vMin.X = v.X;
                if(vMin.Y > v.Y) vMin.Y = v.Y;
                if(vMin.Z > v.Z) vMin.Z = v.Z;
            }

            Point3d center = new Point3d();     // Full external sphere center coordinates
            center.X = 0.5 * (vMax.X - vMin.X);
            center.Y = 0.5 * (vMax.Y - vMin.Y);
            center.Z = 0.5 * (vMax.Z - vMin.Z);
            double r = -1;                       // Radius
            foreach(Point3d v in seq) {
                if(r < center.Distance(v)) r = center.Distance(v);
            }
            r += 0.1;                          // A little extra

            //   1-2: Obtain a tetrahedron circumscribing the sphere
            Point3d v1 = new Point3d();
            v1.X = center.X;
            v1.Y = center.Y + 3.0 * r;
            v1.Z = center.Z;

            Point3d v2 = new Point3d();
            v2.X = center.X - 2.0 * (double)Math.Sqrt(2) * r;
            v2.Y = center.Y - r;
            v2.Z = center.Z;

            Point3d v3 = new Point3d();
            v3.X = center.X + (double)Math.Sqrt(2) * r;
            v3.Y = center.Y - r;
            v3.Z = center.Z + (double)Math.Sqrt(6) * r;

            Point3d v4 = new Point3d();
            v4.X = center.X + (double)Math.Sqrt(2) * r;
            v4.Y = center.Y - r;
            v4.Z = center.Z - (double)Math.Sqrt(6) * r;

            Point3d[] outer = { v1, v2, v3, v4 };
            tetras.Add(new Tetrahedron(v1, v2, v3, v4));

            bool[] isRedundancy;

            // Temporary list for dynamically changing the geometry
            List<Tetrahedron> tmpTList = new List<Tetrahedron>();
            List<Tetrahedron> newTList = new List<Tetrahedron>();

            foreach(Point3d v in seq) {
                tmpTList.Clear();
                newTList.Clear();

                foreach(Tetrahedron t in tetras) {
                    if((t.o != null) && (t.r > v.Distance(t.o))) tmpTList.Add(t);
                }

                foreach(Tetrahedron t1 in tmpTList) {
                    // Remove them first
                    tetras.Remove(t1);

                    v1 = t1.vertices[0];
                    v2 = t1.vertices[1];
                    v3 = t1.vertices[2];
                    v4 = t1.vertices[3];
                    newTList.Add(new Tetrahedron(v1, v2, v3, v));
                    newTList.Add(new Tetrahedron(v1, v2, v4, v));
                    newTList.Add(new Tetrahedron(v1, v3, v4, v));
                    newTList.Add(new Tetrahedron(v2, v3, v4, v));
                }

                isRedundancy = new bool[newTList.Count()];
                //for(int i = 0; i < isRedundancy.Count(); i++) isRedundancy[i] = false;
                for(int i = 0; i < newTList.Count() - 1; i++) {
                    if(isRedundancy[i]) continue; // Added by Xavier Flix
                    for(int j = i + 1; j < newTList.Count(); j++) {
                        if(newTList[i].equals(newTList[j])) isRedundancy[i] = isRedundancy[j] = true;
                    }
                }
                for(int i = 0; i < isRedundancy.Count(); i++) {
                    if(!isRedundancy[i]) {
                        tetras.Add(newTList[i]);
                    }
                }
            }

            bool isOuter = false;
            foreach(Tetrahedron t4 in tetras.ToList()) {
                isOuter = false;
                foreach(Point3d p1 in t4.vertices) {
                    foreach(Point3d p2 in outer) {
                        if(p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z) {
                            isOuter = true;
                            break; // Added by Xavier Flix
                        }
                    }
                    if(isOuter) break; // Added by Xavier Flix
                }
                if(isOuter) tetras.Remove(t4);
            }

            triangles.Clear();
            bool isSame = false;
            foreach(Tetrahedron t in tetras) {
                foreach(Line l1 in t.getLines()) {
                    isSame = false;
                    foreach(Line l2 in edges) {
                        if((l1.Start == l2.Start && l1.End == l2.End) || // Modified by Xavier Flix
                           (l1.Start == l2.End && l1.End == l2.Start)) { //if(l2.Equals(l1)) {
                            isSame = true;
                            break;
                        }
                    }
                    if(!isSame) edges.Add(l1);
                }
            }

            // ===
            // Obtain a face
            List<Triangle> triList = new List<Triangle>();
            foreach(Tetrahedron t in tetras) {
                v1 = t.vertices[0];
                v2 = t.vertices[1];
                v3 = t.vertices[2];
                v4 = t.vertices[3];

                Triangle tri1 = new Triangle(v1, v2, v3);
                Triangle tri2 = new Triangle(v1, v3, v4);
                Triangle tri3 = new Triangle(v1, v4, v2);
                Triangle tri4 = new Triangle(v4, v3, v2);

                Point3d n;
                // Decide direction of the surface
                n = tri1.GetNormal();
                if(n.Dot(v1) > n.Dot(v4)) tri1.TurnBack();

                n = tri2.GetNormal();
                if(n.Dot(v1) > n.Dot(v2)) tri2.TurnBack();

                n = tri3.GetNormal();
                if(n.Dot(v1) > n.Dot(v3)) tri3.TurnBack();

                n = tri4.GetNormal();
                if(n.Dot(v2) > n.Dot(v1)) tri4.TurnBack();

                triList.Add(tri1);
                triList.Add(tri2);
                triList.Add(tri3);
                triList.Add(tri4);
            }

            bool[] isSameTriangle = new bool[triList.Count()];
            for(int i = 0; i < triList.Count() - 1; i++) {
                if(isSameTriangle[i]) continue; // Added by Xavier Flix
                for(int j = i + 1; j < triList.Count(); j++) {
                    if(triList[i].Equals(triList[j])) isSameTriangle[i] = isSameTriangle[j] = true;
                }
            }

            for(int i = 0; i < isSameTriangle.Count(); i++) {
                if(!isSameTriangle[i]) triangles.Add(triList[i]);
            }

            surfaceEdges.Clear();
            List<Line> surfaceEdgeList = new List<Line>();
            foreach(Triangle tri in triangles) {
                surfaceEdgeList.AddRange(tri.GetLines());
            }

            isRedundancy = new bool[surfaceEdgeList.Count()];
            for(int i = 0; i < surfaceEdgeList.Count() - 1; i++) {
                for(int j = i + 1; j < surfaceEdgeList.Count(); j++) {
                    if(surfaceEdgeList[i].Equals(surfaceEdgeList[j])) isRedundancy[j] = true;
                }
            }

            for(int i = 0; i < isRedundancy.Count(); i++) {
                if(!isRedundancy[i]) surfaceEdges.Add(surfaceEdgeList[i]);
            }
        }
    }
}
