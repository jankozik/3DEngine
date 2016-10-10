using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunay
{
    /**
     * 三角形クラス
     *
     * @author tercel
     */
    public class Triangle
    {
        public Point3d v1, v2, v3;
        public Triangle(Point3d v1, Point3d v2, Point3d v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        // Get the Normal Vector
        // It is assumed that vertices are in counterclockwise order
        public Point3d GetNormal()
        {
            Point3d edge1 = new Point3d(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);
            Point3d edge2 = new Point3d(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);

            // x
            Point3d normal = edge1.Cross(edge2);
            normal.Normalize();
            return normal;
        }

        // Reverse the order of the vertices, to flip the surface
        public void TurnBack()
        {
            Point3d tmp = this.v3;
            this.v3 = this.v1;
            this.v1 = tmp;
        }

        // Get a list of line segments
        public Line[] GetLines()
        {
            Line[] l = {
                        new Line(v1, v2),
                        new Line(v2, v3),
                        new Line(v3, v1)
                    };
            return l;
        }

        public Point3d[] Vertices()
        {
            return new Point3d[] {v1,v2,v3};
        }

        public void SetVertices(Point3d v1,Point3d v2,Point3d v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public override bool Equals(object obj) {
            if(obj is Triangle) {
                Triangle t = (Triangle)obj;
                Line[] lines1 = this.GetLines();
                Line[] lines2 = t.GetLines();

                int cnt = 0;
                for(int i = 0; i < lines1.Count(); i++) {
                    for(int j = 0; j < lines2.Count(); j++) {
                        if(lines1[i].Equals(lines2[j]))
                            cnt++;
                    }
                }

                return (cnt == 3);
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
