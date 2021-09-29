using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickHull3D;

namespace QuickHull3DTest {
    [TestClass]
    public class QuickHull {

        private const double DOUBLE_PREC = 2.2204460492503131e-16;
        private static readonly Random rand = new Random(0x1234); // random number generator
        static bool debugEnable = false;
        static bool triangulate = false;

        const int NO_DEGENERACY = 0;
        const int EDGE_DEGENERACY = 1;
        const int VERTEX_DEGENERACY = 2;

        static bool testRotation = true;
        static int degeneracyTest = VERTEX_DEGENERACY;
        static double epsScale = 2.0;

        private void randomlyPerturb(Point3d pnt, double tol) {
            pnt.x += tol * (rand.NextDouble() - 0.5);
            pnt.y += tol * (rand.NextDouble() - 0.5);
            pnt.z += tol * (rand.NextDouble() - 0.5);
        }


        /**
         * Returns the coordinates for <code>num</code> randomly
         * chosen points which are degenerate which respect
         * to the specified dimensionality.
         *
         * @param num number of points to produce
         * @param dimen dimensionality of degeneracy: 0 = coincident,
         * 1 = colinear, 2 = coplaner.
         * @return array of coordinate values
         */
        private double[] randomDegeneratePoints(int num, int dimen) {
            double[] coords = new double[num * 3];
            Point3d pnt = new Point3d();

            Point3d _base = new Point3d();
            _base.SetRandom(-1, 1, rand);

            double tol = DOUBLE_PREC;

            if(dimen == 0) {
                for(int i = 0; i < num; i++) {
                    pnt.Set(_base);
                    randomlyPerturb(pnt, tol);
                    coords[i * 3 + 0] = pnt.x;
                    coords[i * 3 + 1] = pnt.y;
                    coords[i * 3 + 2] = pnt.z;
                }
            } else if(dimen == 1) {
                Vector3d u = new Vector3d();
                u.SetRandom(-1, 1, rand);
                u.Normalize();
                for(int i = 0; i < num; i++) {
                    double a = 2 * (rand.NextDouble() - 0.5);
                    pnt.Scale(a, u);
                    pnt.Add(_base);
                    randomlyPerturb(pnt, tol);
                    coords[i * 3 + 0] = pnt.x;
                    coords[i * 3 + 1] = pnt.y;
                    coords[i * 3 + 2] = pnt.z;
                }
            } else // dimen == 2
              {
                Vector3d nrm = new Vector3d();
                nrm.SetRandom(-1, 1, rand);
                nrm.Normalize();
                for(int i = 0; i < num; i++) { // compute a random point and project it to the plane
                    Vector3d perp = new Vector3d();
                    pnt.SetRandom(-1, 1, rand);
                    perp.Scale(pnt.Dot(nrm), nrm);
                    pnt.Sub(perp);
                    pnt.Add(_base);
                    randomlyPerturb(pnt, tol);
                    coords[i * 3 + 0] = pnt.x;
                    coords[i * 3 + 1] = pnt.y;
                    coords[i * 3 + 2] = pnt.z;
                }
            }
            return coords;
        }

        private double[] shuffleCoords(double[] coords) {
            int num = coords.Length / 3;

            for(int i = 0; i < num; i++) {
                int i1 = rand.Next(num);
                int i2 = rand.Next(num);
                for(int k = 0; k < 3; k++) {
                    double tmp = coords[i1 * 3 + k];
                    coords[i1 * 3 + k] = coords[i2 * 3 + k];
                    coords[i2 * 3 + k] = tmp;
                }
            }
            return coords;
        }

        private double[] addDegeneracy(int type, double[] coords, QuickHull3D.Hull hull) {
            int numv = coords.Length / 3;
            int[][] faces = hull.GetFaces();
            double[] coordsx = new double[coords.Length + faces.Length * 3];
            for(int i = 0; i < coords.Length; i++) {
                coordsx[i] = coords[i];
            }

            double[] lam = new double[3];
            double eps = hull.DistanceTolerance;

            for(int i = 0; i < faces.Length; i++) {
                // random point on an edge
                lam[0] = rand.NextDouble();
                lam[1] = 1 - lam[0];
                lam[2] = 0.0;

                if(type == VERTEX_DEGENERACY && (i % 2 == 0)) {
                    lam[0] = 1.0;
                    lam[1] = lam[2] = 0;
                }

                for(int j = 0; j < 3; j++) {
                    int vtxi = faces[i][j];
                    for(int k = 0; k < 3; k++) {
                        coordsx[numv * 3 + k] +=
                           lam[j] * coords[vtxi * 3 + k] +
                           epsScale * eps * (rand.NextDouble() - 0.5);
                    }
                }
                numv++;
            }
            shuffleCoords(coordsx);
            return coordsx;
        }




        private void rotateCoords(double[] res, double[] xyz, double roll, double pitch, double yaw) {
            double sroll = Math.Sin(roll);
            double croll = Math.Cos(roll);
            double spitch = Math.Sin(pitch);
            double cpitch = Math.Cos(pitch);
            double syaw = Math.Sin(yaw);
            double cyaw = Math.Cos(yaw);

            double m00 = croll * cpitch;
            double m10 = sroll * cpitch;
            double m20 = -spitch;

            double m01 = croll * spitch * syaw - sroll * cyaw;
            double m11 = sroll * spitch * syaw + croll * cyaw;
            double m21 = cpitch * syaw;

            double m02 = croll * spitch * cyaw + sroll * syaw;
            double m12 = sroll * spitch * cyaw - croll * syaw;
            double m22 = cpitch * cyaw;

            for(int i = 0; i < xyz.Length - 2; i += 3) {
                res[i + 0] = m00 * xyz[i + 0] + m01 * xyz[i + 1] + m02 * xyz[i + 2];
                res[i + 1] = m10 * xyz[i + 0] + m11 * xyz[i + 1] + m12 * xyz[i + 2];
                res[i + 2] = m20 * xyz[i + 0] + m21 * xyz[i + 1] + m22 * xyz[i + 2];
            }
        }



        /**
         * Returns true if two face index sets are equal,
         * modulo a cyclical permuation.
         *
         * @param indices1 index set for first face
         * @param indices2 index set for second face
         * @return true if the index sets are equivalent
         */
        private bool faceIndicesEqual(int[] indices1, int[] indices2) {
            if(indices1.Length != indices2.Length) {
                return false;
            }
            int len = indices1.Length;
            int j;
            for(j = 0; j < len; j++) {
                if(indices1[0] == indices2[j]) {
                    break;
                }
            }
            if(j == len) {
                return false;
            }
            for(int i = 1; i < len; i++) {
                if(indices1[i] != indices2[(j + i) % len]) {
                    return false;
                }
            }
            return true;
        }


        private void explicitFaceCheck(QuickHull3D.Hull hull, int[][] checkFaces) {

            int[][] faceIndices = hull.GetFaces();
            Assert.AreEqual(faceIndices.Length, checkFaces.Length, $"Error: {faceIndices.Length} faces vs. {checkFaces.Length}");

            // translate face indices back into original indices
            Point3d[] pnts = hull.GetVertices();
            int[] vtxIndices = hull.GetVertexPointIndices();

            for(int j = 0; j < faceIndices.Length; j++) {
                int[] idxs = faceIndices[j];
                for(int k = 0; k < idxs.Length; k++) {
                    idxs[k] = vtxIndices[idxs[k]];
                }
            }
            for(int i = 0; i < checkFaces.Length; i++) {
                int[] cf = checkFaces[i];
                int j;
                for(j = 0; j < faceIndices.Length; j++) {
                    if(faceIndices[j] != null) {
                        if(faceIndicesEqual(cf, faceIndices[j])) {
                            faceIndices[j] = null;
                            break;
                        }
                    }
                }
                Assert.AreNotEqual(j, faceIndices.Length, $"Error: face {string.Join(" ", cf)} not found");
            }
        }


        private void degenerateTest(QuickHull3D.Hull hull, double[] coords) {
            double[] coordsx = addDegeneracy(degeneracyTest, coords, hull);

            QuickHull3D.Hull xhull = new QuickHull3D.Hull();
            xhull.Debug = debugEnable;

            try {
                xhull.Build(coordsx, coordsx.Length / 3);
                if(triangulate) {
                    xhull.Triangulate();
                }
            } catch(Exception) {
                for(int i = 0; i < coordsx.Length / 3; i++) {
                    Console.Out.WriteLine($"{coordsx[i * 3 + 0]}, {coordsx[i * 3 + 1]}, {coordsx[i * 3 + 2]}, ");
                }
            }

            Assert.IsTrue(xhull.Check(Console.Out));
        }


        private void singleTest(double[] coords, int[][] checkFaces) {
            QuickHull3D.Hull hull = new QuickHull3D.Hull();
            hull.Debug = debugEnable;

            hull.Build(coords, coords.Length / 3);
            if(triangulate) {
                hull.Triangulate();
            }

            Assert.IsTrue(hull.Check(Console.Out));

            if(checkFaces != null) {
                explicitFaceCheck(hull, checkFaces);
            }
            if(degeneracyTest != NO_DEGENERACY) {
                degenerateTest(hull, coords);
            }
        }

        private static double toRadians(double angle)
            => (2.0 * Math.PI * angle) / 360.0;

        private void test(double[] coords, int[][] checkFaces) {
            singleTest(coords, checkFaces);

            if(testRotation) {
                double[][] rpyList = new double[][]
                {
                    new double[] {  0,  0,  0},
                    new double[] { 10, 20, 30},
                    new double[] { -45, 60, 91},
                    new double[] { 125, 67, 81}
                };

                double[] xcoords = new double[coords.Length];
                for(int i = 0; i < rpyList.Length; i++) {
                    double[] rpy = rpyList[i];
                    rotateCoords(xcoords, coords,
                              toRadians(rpy[0]),
                              toRadians(rpy[1]),
                              toRadians(rpy[2]));
                    singleTest(xcoords, checkFaces);
                }
            }
        }


        /**
         * Returns the coordinates for <code>num</code> points whose x, y, and
         * z values are randomly chosen within a given range.
         *
         * @param num number of points to produce
         * @param range coordinate values will lie between -range and range
         * @return array of coordinate values
         */
        private double[] randomPoints(int num, double range) {
            double[] coords = new double[num * 3];

            for(int i = 0; i < num; i++) {
                for(int k = 0; k < 3; k++) {
                    coords[i * 3 + k] = 2 * range * (rand.NextDouble() - 0.5);
                }
            }
            return coords;
        }

        /**
         * Returns the coordinates for <code>num</code> points whose x, y, and
         * z values are randomly chosen to lie within a sphere.
         *
         * @param num number of points to produce
         * @param radius radius of the sphere
         * @return array of coordinate values
         */
        public double[] randomSphericalPoints(int num, double radius) {
            double[] coords = new double[num * 3];
            Point3d pnt = new Point3d();

            for(int i = 0; i < num;) {
                pnt.SetRandom(-radius, radius, rand);
                if(pnt.Norm <= radius) {
                    coords[i * 3 + 0] = pnt.x;
                    coords[i * 3 + 1] = pnt.y;
                    coords[i * 3 + 2] = pnt.z;
                    i++;
                }
            }
            return coords;
        }

        /**
         * Returns the coordinates for <code>num</code> points whose x, y, and
         * z values are each randomly chosen to lie within a specified
         * range, and then clipped to a maximum absolute
         * value. This means a large number of points
         * may lie on the surface of cube, which is useful
         * for creating degenerate convex hull situations.
         *
         * @param num number of points to produce
         * @param range coordinate values will lie between -range and
         * range, before clipping
         * @param max maximum absolute value to which the coordinates
         * are clipped
         * @return array of coordinate values
         */
        public double[] randomCubedPoints(int num, double range, double max) {
            double[] coords = new double[num * 3];

            for(int i = 0; i < num; i++) {
                for(int k = 0; k < 3; k++) {
                    double x = 2 * range * (rand.NextDouble() - 0.5);
                    if(x > max) {
                        x = max;
                    } else if(x < -max) {
                        x = -max;
                    }
                    coords[i * 3 + k] = x;
                }
            }
            return coords;
        }

        /**
         * Returns randomly shuffled coordinates for points on a
         * three-dimensional grid, with a presecribed width between each point.
         *
         * @param gridSize number of points in each direction,
         * so that the total number of points produced is the cube of
         * gridSize.
         * @param width distance between each point along a particular
         * direction
         * @return array of coordinate values
         */
        public double[] randomGridPoints(int gridSize, double width) {
            // gridSize gives the number of points across a given dimension
            // any given coordinate indexed by i has value
            // (i/(gridSize-1) - 0.5)*width

            int num = gridSize * gridSize * gridSize;

            double[] coords = new double[num * 3];

            int idx = 0;
            for(int i = 0; i < gridSize; i++) {
                for(int j = 0; j < gridSize; j++) {
                    for(int k = 0; k < gridSize; k++) {
                        coords[idx * 3 + 0] = (i / (double)(gridSize - 1) - 0.5) * width;
                        coords[idx * 3 + 1] = (j / (double)(gridSize - 1) - 0.5) * width;
                        coords[idx * 3 + 2] = (k / (double)(gridSize - 1) - 0.5) * width;
                        idx++;
                    }
                }
            }
            shuffleCoords(coords);
            return coords;
        }

        /**
         * Runs a set of tests on the QuickHull3D class, and
         * prints <code>Passed</code> if all is well.
         * Otherwise, an error message and stack trace
         * are printed.
         *
         * <p>If the option <code>-timing</code> is supplied,
         * then timing information is produced instead.
         */
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Input points appear to be coincident")]
        public void TestDegenerateInput0() {
            QuickHull3D.Hull hull = new QuickHull3D.Hull();

            for(int i = 0; i < 10; i++) {
                double[] coords = randomDegeneratePoints(10, 0);
                hull.Build(coords);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Input points appear to be colinear")]
        public void TestDegenerateInput1() {
            QuickHull3D.Hull hull = new QuickHull3D.Hull();

            for(int i = 0; i < 10; i++) {
                double[] coords = randomDegeneratePoints(10, 1);
                hull.Build(coords);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Input points appear to be coplanar")]
        public void TestDegenerateInput2() {
            QuickHull3D.Hull hull = new QuickHull3D.Hull();

            for(int i = 0; i < 10; i++) {
                double[] coords = randomDegeneratePoints(10, 2);
                hull.Build(coords);
            }
        }

        [TestMethod]
        public void TestExplicit() {
            double[] coords = new double[]
            {
                    21, 0, 0,
                    0, 21, 0,
                    0, 0, 0,
                    18, 2, 6,
                    1, 18, 5,
                    2, 1, 3,
                    14, 3, 10,
                    4, 14, 14,
                    3, 4, 10,
                    10, 6, 12,
                    5, 10, 15,
            };

            test(coords, null);

            coords = new double[]
            {
                    0.0 , 0.0 , 0.0,
                    21.0, 0.0 , 0.0,
                    0.0 , 21.0, 0.0,
                    2.0 , 1.0 , 2.0,
                    17.0, 2.0 , 3.0,
                    1.0 , 19.0, 6.0,
                    4.0 , 3.0 , 5.0,
                    13.0, 4.0 , 5.0,
                    3.0 , 15.0, 8.0,
                    6.0 , 5.0 , 6.0,
                    9.0 , 6.0 , 11.0,
            };

            test(coords, null);

            Console.Out.WriteLine("Testing 20 to 200 random points ...");
            for(int n = 20; n < 200; n += 10) { // Console.Out.WriteLine (n);
                for(int i = 0; i < 10; i++) {
                    coords = randomPoints(n, 1.0);
                    test(coords, null);
                }
            }

            Console.Out.WriteLine("Testing 20 to 200 random points in a sphere ...");
            for(int n = 20; n < 200; n += 10) { // Console.Out.WriteLine (n);
                for(int i = 0; i < 10; i++) {
                    coords = randomSphericalPoints(n, 1.0);
                    test(coords, null);
                }
            }

            Console.Out.WriteLine("Testing 20 to 200 random points clipped to a cube ...");
            for(int n = 20; n < 200; n += 10) { // Console.Out.WriteLine (n);
                for(int i = 0; i < 10; i++) {
                    coords = randomCubedPoints(n, 1.0, 0.5);
                    test(coords, null);
                }
            }

            Console.Out.WriteLine("Testing 8 to 1000 randomly shuffled points on a grid ...");
            for(int n = 2; n <= 10; n++) { // Console.Out.WriteLine (n*n*n);
                for(int i = 0; i < 10; i++) {
                    coords = randomGridPoints(n, 4.0);
                    test(coords, null);
                }
            }

        }



    }
}