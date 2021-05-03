using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class WaterGenerator : MonoBehaviour
    {
        public float size;
        public int width, height;

        private Vector3[] levels;
        private int[] triangles;
        private int[] otherTriangles;
        private int[] allTriangles;

        public MeshFilter mf;
        public MeshRenderer mr;
        private Mesh mesh;

        public MeshCollider canoe;

        public float tick = 0.1f;
        private float elapsed = 0f;

        public bool bounds = false;
        public bool mend = false;
        public float threshold = 0.001f;

        // Start is called before the first frame update
        void Start()
        {
            if (!mf)
                mf = this.gameObject.AddComponent<MeshFilter>();

            if (!mr)
                mr = this.gameObject.AddComponent<MeshRenderer>();

            mesh = mf.mesh = new Mesh();
            mesh.subMeshCount = 2;

            levels = new Vector3[width * height];
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    levels[Index(i, j)] = this.transform.position + (size * (new Vector3(i - (width / 2f), 0, j - (height / 2f))));
                }
            }

            // Generate triangles
            List<int> tris = new List<int>();
            for (int i = 0; i < width - 1; i++) {
                for (int j = 0; j < height - 1; j++) {
                    // Top left of quad
                    tris.Add(Index(i, j));
                    tris.Add(Index(i, j + 1));
                    tris.Add(Index(i + 1, j));

                    //Bottom right of quad
                    tris.Add(Index(i + 1, j + 1));
                    tris.Add(Index(i + 1, j));
                    tris.Add(Index(i, j + 1));
                }
            }

            mesh.vertices = levels;

            triangles = tris.ToArray();
            otherTriangles = new int[0];


            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(otherTriangles, 1);

            allTriangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();


            List<int> adj = new List<int>();
            int start = 220;
            int x, y;
            FromIndex(220, out x, out y);
            AddAdjacent(adj, x, y);
            adj.Sort();
            adj.Reverse();
            string result = "";
            for (int i = 0; i < adj.Count; i++) {
                FromIndex(adj[i], out x, out y);
                Debug.Log("( " + x + ", " + y + ")");
                result += adj[i] + ", ";
            }
            Debug.Log(start);
            Debug.Log(result);
        }

        private int Index(int i, int j)
        {
            return (j * width) + i;
        }

        private void FromIndex(int index, out int i, out int j)
        {
            i = index % width;
            j = index / width;
        }

        // Update is called once per frame
        void Update()
        {

            CalculateVertices();
        }

        private void AddAdjacent(List<int> toMend, int x, int y)
        {
            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    int temp = Index(x + i, y + j);
                    if (!(i == 0 && j == 0) && temp >= 0 && temp < levels.Length && !toMend.Contains(temp))
                        toMend.Add(temp);
                }
            }
        }

        private void CalculateVertices()
        {

            for (int i = 0; i < levels.Length; i++) {
                float amp = 0.0125f;

                int x, y;
                FromIndex(i, out x, out y);

                levels[i] = this.transform.position + (size * (new Vector3(x - (width / 2f), 0, y - (height / 2f)))) + amp * Vector3.up * (Mathf.Sin(-Time.time * 2 + Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2))) + Mathf.Sin(-Time.time * 2 + Mathf.Sqrt(Mathf.Pow(width - x, 2) + Mathf.Pow(y, 2))));
            }




            List<int> toMend = new List<int>();
            List<int> removed = new List<int>();
            toMend.Clear();
            removed.Clear();
            List<int> tris = new List<int>();
            tris.Clear();
            List<int> otherTris = new List<int>();
            for (int k = 0; k < levels.Length; k++) {
                int i, j;
                FromIndex(k, out i, out j);
                if (i > width - 2 || j > height - 2)
                    continue;

                bool satisfied = false;
                float magnitude = ((bounds ? canoe.ClosestPointOnBounds(levels[k]) : canoe.ClosestPoint(levels[k])) - levels[k]).magnitude;
                satisfied = magnitude < threshold; //|| ( (i <= 30 && i >= 20) && (j <= 30 && j >= 20));

                if (satisfied) {
                    AddAdjacent(toMend, i, j);
                    if (!toMend.Contains(k))
                        toMend.Add(k);
                    removed.Add(k);


                    tris.Add(-1);
                    tris.Add(-1);
                    tris.Add(-1);
                    tris.Add(-1);
                    tris.Add(-1);
                    tris.Add(-1);


                } else {

                    // Top left of quad
                    tris.Add(k);
                    tris.Add(Index(i, j + 1));
                    tris.Add(Index(i + 1, j));

                    //Bottom right of quad
                    tris.Add(Index(i + 1, j + 1));
                    tris.Add(Index(i + 1, j));
                    tris.Add(Index(i, j + 1));
                }
            }


            if (mend) {
                toMend.Sort();
                toMend.Reverse();
                int lastRadix = -1;

                for (int i = 0; i < toMend.Count; i++) {
                    float y = levels[toMend[i]].y;

                    int radix = toMend[i] / 100;
                    Vector3 newPos = bounds ? canoe.ClosestPointOnBounds(levels[toMend[i]]) : canoe.ClosestPoint(levels[toMend[i]]);

                    int u, v;
                    FromIndex(toMend[i], out u, out v);

                    levels[toMend[i]] = newPos;

                    lastRadix = radix;
                }
            }

            mesh.vertices = levels;
            mesh.RecalculateNormals();

            // filter out my spacers
            tris.RemoveAll(item => item == -1);

            triangles = tris.ToArray();
            otherTriangles = otherTris.ToArray();
            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(otherTriangles, 1);
        }

        private bool Adjacent(int index, int other)
        {
            if (index == other)
                return false;

            int x, y, otherX, otherY;
            FromIndex(index, out x, out y);
            FromIndex(other, out otherX, out otherY);

            if (otherX == x) {
                return Mathf.Abs(otherY - y) == 1;
            } else if (otherY == y) {
                return Mathf.Abs(otherX - x) == 1;
            }

            return false;
        }

        public float WaterLevel(Vector3 location)
        {
            int xIndex = 0, yIndex = 0;
            if (location != null && levels != null && levels.Length > 0) {
                xIndex = (int)((location.x - levels[0].x) / size);
                yIndex = (int)((location.y - levels[0].y) / size);

                if (xIndex < width && yIndex < height && xIndex >= 0 && yIndex >= 0)
                    return levels[Index(xIndex, yIndex)].y;
            }

            return 0f; //TODO
        }
    }
}
