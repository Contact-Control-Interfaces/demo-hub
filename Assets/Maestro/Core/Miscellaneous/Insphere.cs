using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    class Insphere : IComparable<Insphere>
    {
        public Vector3 center;
        public float radius;

        public Insphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public Insphere() : this(Vector3.zero, 1f) { }


        /*public static Insphere Get(List<Vector3> points)
        {
            if (points.Count < 4)
                return null;

            //Calculate all planes
            List<Plane> allPlanes = new List<Plane>();
            for (int i = 0; i < points.Count - 2; i++)
            {
                for (int j = i+1; j < points.Count - 1; j++)
                {
                    for (int k = j+1; k < points.Count; k++)
                    {
                        allPlanes.Add(from3(points[i], points[j], points[k]));
                    }
                }
            }

            //Get first 4 points
            Vector3[] tetra = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                tetra[i] = points[0];
                points.RemoveAt(0);
            }

            return Get(tetra);
        }*/

        public static Insphere Get(List<Plane> planes)
        {
            List<Insphere> result = new List<Insphere>();

            for (int i = 0; i < planes.Count-3; i++)
            {
                for (int j = i+1; j < planes.Count-2; j++)
                {
                    for (int k = j+1; k < planes.Count-1; k++)
                    {
                        for (int l = k+1; l < planes.Count; l++)
                        {
                            result.Add(Get(planes[i], planes[j], planes[k], planes[l]));
                        }
                    }
                }
            }

            return result.Max();
        }

        public static Insphere Get(Plane a, Plane b, Plane c, Plane d)
        {
            Plane[] planes = new Plane[] { a, b, c, d };

            float[,] A = new float[6, 3];
            //float[,] X = new float[3, 1];
            float[,] B = new float[6, 1];

            //Populate the matrix A with every possible bisector (4 choose 2 = 6)
            int index = 0;
            for (int i = 0; i < planes.Length - 1; i++)
            {
                for (int j = i + 1; j < planes.Length; j++)
                {
                    Plane bisector = bisect(planes[i], planes[j]);

                    A[index, 0] = bisector.a;
                    A[index, 1] = bisector.b;
                    A[index, 2] = bisector.c;
                    B[index++, 0] = -bisector.d; //TODO?
                }
            }

            //Plane a = from3(points[0], points[1], points[2]);
            //Plane b = from3(points[0], points[1], points[3]);
            //Plane bisector = bisect(a, b);

            float[,] At = transpose(A);
            float[,] AtA = multiply(At, A);
            float[,] AtB = multiply(At, B);

            float denominator = determinant3x3(AtA);

            //Get x coordinate
            /*float temp1 = AtA[0, 0];
            float temp2 = AtA[1, 0];
            float temp3 = AtA[2, 0];
            AtA[0, 0] = AtB[0, 0];
            AtA[1, 0] = AtB[1, 0];
            AtA[2, 0] = AtB[2, 0];
            float x = determinant3x3(AtA);
            AtA[0, 0] = temp1;
            AtA[1, 0] = temp2;
            AtA[2, 0] = temp3;*/
            float x = getCoordinate(AtA, AtB, 0) / denominator;
            float y = getCoordinate(AtA, AtB, 1) / denominator;
            float z = getCoordinate(AtA, AtB, 2) / denominator;

            float radius = Mathf.Abs(
                ((planes[0].a * x) + (planes[0].b * y) + (planes[0].c * z) + planes[0].d) /
                Mathf.Sqrt(Mathf.Pow(planes[0].a, 2) + Mathf.Pow(planes[0].b, 2) + Mathf.Pow(planes[0].c, 2))
            );

            return new Insphere(new Vector3(x, y, z), radius); //TODO
        }

        /*public static Insphere Get(Vector3[] points)
        {
            //REQUIRES 4 POINTS

            //Generate all 4 planes in the tetrahedron defined by points
            Plane[] planes = new Plane[] {
                from3(points[0], points[1], points[2]),
                from3(points[1], points[2], points[3]),
                from3(points[0], points[2], points[3]),
                from3(points[0], points[1], points[3])
            };

            float[,] A = new float[6, 3];
            //float[,] X = new float[3, 1];
            float[,] B = new float[6, 1];

            //Populate the matrix A with every possible bisector (4 choose 2 = 6)
            int index = 0;
            for (int i = 0; i < planes.Length - 1; i++)
            {
                for (int j = i + 1; j < planes.Length; j++)
                {
                    Plane bisector = bisect(planes[i], planes[j]);

                    A[index, 0] = bisector.a;
                    A[index, 1] = bisector.b;
                    A[index, 2] = bisector.c;
                    B[index++, 0] = -bisector.d; //TODO?
                }
            }

            //Plane a = from3(points[0], points[1], points[2]);
            //Plane b = from3(points[0], points[1], points[3]);
            //Plane bisector = bisect(a, b);

            float[,] At = transpose(A);
            float[,] AtA = multiply(At, A);
            float[,] AtB = multiply(At, B);

            float denominator = determinant3x3(AtA);

            //Get x coordinate
            /*float temp1 = AtA[0, 0];
            float temp2 = AtA[1, 0];
            float temp3 = AtA[2, 0];
            AtA[0, 0] = AtB[0, 0];
            AtA[1, 0] = AtB[1, 0];
            AtA[2, 0] = AtB[2, 0];
            float x = determinant3x3(AtA);
            AtA[0, 0] = temp1;
            AtA[1, 0] = temp2;
            AtA[2, 0] = temp3; *
            float x = getCoordinate(AtA, AtB, 0) / denominator;
            float y = getCoordinate(AtA, AtB, 1) / denominator;
            float z = getCoordinate(AtA, AtB, 2) / denominator;

            float radius = Mathf.Abs(
                ((planes[0].a * x) + (planes[0].b * y) + (planes[0].c * z) + planes[0].d) /
                Mathf.Sqrt(Mathf.Pow(planes[0].a, 2) + Mathf.Pow(planes[0].b, 2) + Mathf.Pow(planes[0].c, 2))
            );

            return new Insphere(new Vector3(x, y, z), radius); //TODO
        }*/

        private static float getCoordinate(float[,] matrixA, float[,] matrixB, int column)
        {
            float temp1 = matrixA[0, column];
            float temp2 = matrixA[1, column];
            float temp3 = matrixA[2, column];
            matrixA[0, column] = matrixB[0, 0];
            matrixA[1, column] = matrixB[1, 0];
            matrixA[2, column] = matrixB[2, 0];
            float result = determinant3x3(matrixA);
            matrixA[0, column] = temp1;
            matrixA[1, column] = temp2;
            matrixA[2, column] = temp3;

            return result;
        }

        private static float[,] transpose(float[,] matrix)
        {
            float[,] result = new float[matrix.GetLength(1), matrix.GetLength(0)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }

            return result;
        }

        private static float[,] multiply(float[,] a, float[,] b)
        {
            float[,] result = new float[a.GetLength(0), b.GetLength(1)];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    float total = 0.0f;

                    for (int k = 0; k < a.GetLength(1); k++)
                    {
                        total += a[i, k] * b[k, j];
                    }

                    result[i, j] = total;
                }
            }

            return result;
        }

        public static Plane from3(Vector3 a, Vector3 b, Vector3 c)
        {
            float[,] matrix = new float[3, 3];
            matrix[0, 0] = a.x;
            matrix[0, 1] = a.y;
            matrix[0, 2] = a.z;
            matrix[1, 0] = b.x;
            matrix[1, 1] = b.y;
            matrix[1, 2] = b.z;
            matrix[2, 0] = c.x;
            matrix[2, 1] = c.y;
            matrix[2, 2] = c.z;
            float D = -1f / determinant3x3(matrix);

            matrix[0, 0] = matrix[1, 0] = matrix[2, 0] = 1f;
            float A = D * determinant3x3(matrix);
            matrix[0, 0] = a.x;
            matrix[1, 0] = b.x;
            matrix[2, 0] = c.x;

            matrix[0, 1] = matrix[1, 1] = matrix[2, 1] = 1f;
            float B = D * determinant3x3(matrix);
            matrix[0, 1] = a.y;
            matrix[1, 1] = b.y;
            matrix[2, 1] = c.y;

            matrix[0, 2] = matrix[1, 2] = matrix[2, 2] = 1f;
            float C = D * determinant3x3(matrix);

            return new Plane(A, B, C, 1f);
        }

        private static Plane bisect(Plane a, Plane b)
        {
            float A = Mathf.Sqrt((a.a * a.a) + (a.b * a.b) + (a.c * a.c));
            float B = Mathf.Sqrt((b.a * b.a) + (b.b * b.b) + (b.c * b.c));

            Plane pos = new Plane((A * b.a) - (B * a.a), (A * b.b) - (B * a.b), (A * b.c) - (B * a.c), (A * b.d) - (B * a.d));
            //Plane neg = new Plane((-A * b.a) - (B * a.a), (-A * b.b) - (B * a.b), (-A * b.c) - (B * a.c), (-A * b.d) - (B * a.d));
            Plane neg = new Plane((A * b.a) + (B * a.a), (A * b.b) + (B * a.b), (A * b.c) + (B * a.c), (A * b.d) + (B * a.d));

            Vector3 temp = a.Normal;
            float between = Vector3.Angle(temp, b.Normal);
            //float betweenPos = Vector3.Angle(temp, pos.Normal);
            float betweenNeg = Vector3.Angle(temp, neg.Normal);

            if (between <= 90)
            {
                //bisector must be less than 45 degrees
                return betweenNeg <= 45 ? neg : pos;
            }
            else
            {
                //bisector must be greater than 45 degrees
                return betweenNeg > 45 ? neg : pos;
            }

        }

        private static float determinant3x3(float[,] matrix)
        {
            float first = matrix[0, 0] * determinant2x2(matrix[1, 1], matrix[1, 2], matrix[2, 1], matrix[2, 2]);
            float second = matrix[0, 1] * determinant2x2(matrix[1, 0], matrix[1, 2], matrix[2, 0], matrix[2, 2]);
            float third = matrix[0, 2] * determinant2x2(matrix[1, 0], matrix[1, 1], matrix[2, 0], matrix[2, 1]);

            return first - second + third;
        }

        private static float determinant2x2(float a, float b, float c, float d)
        {
            return (a * d) - (b * c);
        }

        public int CompareTo(Insphere other)
        {
            return this.radius.CompareTo(other.radius);
        }
    }

    struct Plane
    {
        public float a, b, c, d;

        public Plane(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public Vector3 Normal { get { return new Vector3(a, b, c).normalized; } }
    }
}
