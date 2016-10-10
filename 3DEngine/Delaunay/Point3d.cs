using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunay {
    public class Point3d {
        public const double ToRad = Math.PI / 180.0;
        public const double ToDeg = 180.0 / Math.PI;

        public double X;
        public double Y;
        public double Z;

        public Point3d() {
        }

        public Point3d(Point3d p) {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        public Point3d(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }

        public double Distance(Point3d v) {
            double dx = X - v.X;
            double dy = Y - v.Y;
            double dz = Z - v.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);

            //return Math.Sqrt(Math.Pow((this.X - v.X), 2.0) +
            //                 Math.Pow((this.Y - v.Y), 2.0) +
            //                 Math.Pow((this.Z - v.Z), 2.0));
        }

        public double Distance2d(Point3d v) {
            return Math.Sqrt(Math.Pow((this.X - v.X), 2.0) +
                             Math.Pow((this.Y - v.Y), 2.0));
        }

        public double Length() {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public double Dot(Point3d v) {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public Point3d Cross(Point3d v) {
            return new Point3d((Y * v.Z) - (v.Y * Z),
                               (Z * v.X) - (v.Z * X),
                               (X * v.Y) - (v.X * Y));
        }

        public void Normalize() {
            double len = this.Length();
            if(len > 0) {
                X /= len;
                Y /= len;
                Z /= len;
            } else {
                X = 1.0;
                Y = 0.0;
                Z = 0.0;
            }
        }

        public Point3d Normalized() {
            Point3d p = new Point3d(this);
            p.Normalize();
            return p;
        }

        public Point3d RotateX(double angle) {
            double rad = angle * ToRad;
            double cosa = Math.Cos(rad);
            double sina = Math.Sin(rad);
            double yn = Y * cosa - Z * sina;
            double zn = Y * sina + Z * cosa;

            return new Point3d(X, yn, zn);
        }

        public Point3d RotateY(double angle) {
            double rad = angle * ToRad;
            double cosa = Math.Cos(rad);
            double sina = Math.Sin(rad);
            double xn = Z * sina + X * cosa;
            double zn = Z * cosa - X * sina;

            return new Point3d(xn, Y, zn);
        }

        public Point3d RotateZ(double angle) {
            double rad = angle * ToRad;
            double cosa = Math.Cos(rad);
            double sina = Math.Sin(rad);
            double xn = X * cosa - Y * sina;
            double yn = X * sina + Y * cosa;

            return new Point3d(xn, yn, Z);
        }

        public Point3d Project(int viewWidth, int viewHeight, double fov, double viewDistance) {
            double factor = (viewDistance == -Z ? 999 : fov / (viewDistance + Z));
            return new Point3d(X * factor + viewWidth / 2, Y * factor + viewHeight / 2, Z);
        }

        public Point3d UnProject(int viewWidth, int viewHeight, double fov, double viewDistance) {
            double factor = (viewDistance == -Z ? 999 : fov / (viewDistance + Z));
            return new Point3d((X - viewWidth / 2) / factor, (Y - viewHeight / 2) / factor, Z);
        }

        public System.Drawing.PointF ToPointF() {
            return new System.Drawing.PointF((float)this.X, (float)this.Y);
        }

        public override bool Equals(object obj) {
            if(obj is Point3d) {
                Point3d p = (Point3d)obj;
                return (this.X == p.X && this.Y == p.Y && this.Z == p.Z);
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public bool IsSimilar(Point3d v, double epsilon = 0.0001) {
            return Math.Abs(X - v.X) <= epsilon &&
                   Math.Abs(Y - v.Y) <= epsilon &&
                   Math.Abs(Z - v.Z) <= epsilon;
        }

        public static bool operator ==(Point3d p1, Point3d p2) {
            //if(p1 == null && p2 != null) return false;
            //if(p2 == null && p1 != null) return false;
            //if(p1 == null && p2 == null) return true;
            return (p1?.X == p2?.X && p1?.Y == p2?.Y && p1?.Z == p2?.Z);
        }

        public static bool operator !=(Point3d p1, Point3d p2) {
            return !(p1 == p2);
        }

        public static Point3d operator +(Point3d v1, Point3d v2) {
            return new Point3d(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Point3d operator -(Point3d v1, Point3d v2) {
            return new Point3d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Point3d operator -(Point3d p1) {
            return new Point3d(p1.X, p1.Y, -p1.Z);
        }

        public static Point3d operator *(Point3d v1, double scalar) {
            return new Point3d(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
        }

        public static Point3d operator *(double scalar, Point3d v1) {
            return new Point3d(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
        }

        public static double operator *(Point3d v1, Point3d v2) {
            return v1.Dot(v2);
        }

        public static Point3d operator /(Point3d v1, double scalar) {
            return v1 * (1 / scalar);
        }

        public override string ToString() {
            return string.Format("({0:F2}, {1:F2}, {2:F2})", X, Y, Z);
        }

        public Point3d AsInt(int padding = 0) {
            int x = (int)Math.Round(this.X);
            int y = (int)Math.Round(this.Y);
            int z = (int)Math.Round(this.Z);

            if(padding > 1) {
                x -= x % padding;
                y -= y % padding;
                z -= z % padding;
            }

            return new Point3d(x, y, z);
        }

        public double AngleXY(Point3d v1) {
            double dx = v1.X - this.X;
            double dy = v1.Y - this.Y;

            double a = Math.Atan2(dy, dx) * ToDeg;
            if(a < 0) a += 360;
            return a;
        }

        public double AngleXZ(Point3d v1) {
            double dx = v1.X - this.X;
            double dz = v1.Z - this.Z;

            double a = Math.Atan2(dz, dx) * ToDeg;
            if(a < 0) a += 360;
            return a;
        }

        public double AngleYZ(Point3d v1) {
            double dy = v1.Y - this.Y;
            double dz = v1.Z - this.Z;

            double a = Math.Atan2(dz, dy) * ToDeg;
            if(a < 0) a += 360;
            return a;
        }
    }
}
