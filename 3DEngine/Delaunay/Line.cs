using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunay {
    public class Line {
        public Point3d Start, End;
        public Line(Point3d start, Point3d end) {
            this.Start = start;
            this.End = end;
        }

        // Swap start and end points
        public void Reverse() {
            Point3d tmp = this.Start;
            this.Start = this.End;
            this.End = tmp;
        }

        public override bool Equals(object obj) {
            if(obj is Line) {
                Line l = (Line)obj;
                return ((this.Start.Equals(l.Start)) && (this.End.Equals(l.End)))
                    || ((this.Start.Equals(l.End)) && (this.End.Equals(l.Start)));
            } else {
                return false;
            }
        }

        public Point3d[] Vertices() {
            return new Point3d[] { Start, End };
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return string.Format("{0} -> {1}", Start.ToString(), End.ToString());
        }

        public static Line operator +(Line v1, Line v2) {
            return new Line(v1.Start + v2.Start, v1.End + v2.End);
        }

        public static Line operator +(Line v1, Point3d p) {
            return new Line(v1.Start + p, v1.End + p);
        }

        public static Line operator -(Line v1, Line v2) {
            return new Line(v1.Start - v2.Start, v1.End - v2.End);
        }

        public static Line operator -(Line v1, Point3d p) {
            return new Line(v1.Start - p, v1.End - p);
        }

        public static Line operator *(Line v1, double scalar) {
            return new Line(v1.Start * scalar, v1.End * scalar);
        }

        public static Line operator *(double scalar, Line v1) {
            return v1 * scalar;
        }
    }
}
