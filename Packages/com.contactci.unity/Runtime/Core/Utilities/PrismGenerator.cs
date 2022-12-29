using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Maestro
{
    public class PrismGenerator : MonoBehaviour
    {
        // Every group of 3 points is a face of a triangular prism
        public Transform[] points;
        public Color[] colors;

        public float thickness = 0.003f;
        public bool center = true;
        public bool destroyRenderers = true;

        public MaestroContainer container;
        public Transform parent;
        public GameObject[] prisms;
        private FingerCollider[] fcs;
        private Rigidbody[] rigidbodies;

        private static ushort[] triangles = {
            /* top/bottom */
            0,1,2,
            5,4,3,
            /* extrude BC */
            1,4,5,
            5,2,1,
            /* extrude AC */
            2,5,3,
            3,0,2,
            /* extrude AB */
            0,3,4,
            4,1,0
        };

        public bool AnyTouching { get { return fcs != null && fcs.Any(x => x.Contacting); } }

        public MaestroInteractable Touching { get; private set; }

        public void Init(PhysicMaterial material = null)
        {
            int prismCount = points.Length / 3;

            if (points.Length % 3 != 0) {
                Debug.LogError("This isn't configured properly! Not enough points!");
                this.enabled = false;
            } else {

                // Generate random colors if we don't have any set
                if (colors == null || colors.Length < prismCount) {
                    colors = new Color[prismCount];
                    for (int i = 0; i < prismCount; i++) {
                        colors[i] = new Color(Random.value, Random.value, Random.value);
                    }
                }

                // Generate each prism
                prisms = new GameObject[prismCount];
                fcs = new FingerCollider[prismCount];
                rigidbodies = new Rigidbody[prismCount];
                for (int i = 0; i < prismCount; i++) {

                    GameObject newObject = new GameObject();
                    newObject.name = "Prism " + (i+1);
                    newObject.transform.parent = this.transform;

                    // Create mesh
                    Mesh newMesh = MakeTriangularPrism(newObject, points[3 * i], points[3 * i + 1], points[3 * i + 2], thickness);
                    MeshFilter mf = newObject.AddComponent<MeshFilter>();
                    mf.mesh = newMesh;

                    // Render these prisms on top
                    MeshRenderer mr = newObject.AddComponent<MeshRenderer>();
                    if (destroyRenderers) {
                        mr.enabled = false;
                        Destroy(mr);
                    } else {
                        mr.material.shader = Shader.Find("GUI/Text Shader");
                        mr.material.color = colors[i];
                    }

                    // Add collision
                    Rigidbody rb = newObject.AddComponent<Rigidbody>();
                    rb.useGravity = false;
                    rb.isKinematic = true;
                    MeshCollider mc = newObject.AddComponent<MeshCollider>();
                    mc.convex = true;

                    if (material != null)
                        mc.sharedMaterial = material;

                    FingerCollider fc = newObject.AddComponent<FingerCollider>();
                    fc.SetParentHPI(container.parent);

                    prisms[i] = newObject;
                    fcs[i] = fc;
                    rigidbodies[i] = rb;
                }
            }
        }

        void FixedUpdate()
        {
            AdjustPrisms();

            Touching = GetCurrentTouching();
        }

        private void AdjustPrisms()
        {
            for (int i = 0; i < prisms.Length; i++) {
                Transform a = points[i * 3];
                Transform b = points[i * 3 + 1];
                Transform c = points[i * 3 + 2];

                prisms[i].transform.position = Centroid(a, b, c);
                rigidbodies[i].velocity = Vector3.zero;

                Vector3 forward = Normal(a, b, c);

                if (forward.sqrMagnitude > 0)
                    prisms[i].transform.rotation = Quaternion.LookRotation(forward, OrientTo(a, b, c));
            }
        }

        private MaestroInteractable GetCurrentTouching()
        {
            MaestroInteractable touching = null;
            int max = 0;
            if (fcs != null) {
                foreach (var mi in fcs.Where(x => x.Contacting).Select(x => x.touching))
                {
                    if (mi && mi.interactionPriority > max)
                    {
                        max = mi.interactionPriority;
                        touching = mi;
                    }
                }
            }
            if(max == 0)
                return null;
            return touching;
        }

        /*
         * Triangles are specified in order clockwise, as Unity prefers the Left hand rule. 
         * curling your left hand in the same order will give the triangle's normal as your thumb.
         */
        private static Mesh MakeTriangularPrism(GameObject parent, Transform a, Transform b, Transform c, float depth = 1f, bool center = true)
        {
            Vector3 centroid = Centroid(a, b, c);
            Vector3 normal = Normal(a, b, c);

            parent.transform.position = centroid;
            parent.transform.rotation = Quaternion.LookRotation(normal, OrientTo(a, b, c));

            Vector3 firstTriangleOffset = Vector3.zero;
            Vector3 secondTriangleOffset = depth * normal;
            if (center) {
                // offset both triangles evenly instead of just the second
                secondTriangleOffset /= 2;
                firstTriangleOffset = -secondTriangleOffset;
            }

            Vector3[] vertices = {
                a.position + firstTriangleOffset,
                b.position + firstTriangleOffset,
                c.position + firstTriangleOffset,
                a.position + secondTriangleOffset,
                b.position + secondTriangleOffset,
                c.position + secondTriangleOffset
            };

            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = parent.transform.InverseTransformPoint(vertices[i]);
            }

            Mesh result = new Mesh();
            result.subMeshCount = 1;
            result.SetVertices(vertices);
            result.SetTriangles(triangles, 0);

            return result;
        }

        private static Vector3 Centroid(params Transform[] points)
        {
            if (points.Length == 0)
                return Vector3.zero;

            Vector3 total = Vector3.zero;
            foreach (Transform t in points)
                total += t.position;
            return total / points.Length;
        }

        private static Vector3 Normal(Transform a, Transform b, Transform c)
        {
            return Vector3.Cross(a.position - b.position, c.position - b.position).normalized;
        }

        private static Vector3 OrientTo(Transform a, Transform b, Transform c)
        {
            // Just another vector to properly orient the prisms
            return b.position - a.position;
        }
    }
}