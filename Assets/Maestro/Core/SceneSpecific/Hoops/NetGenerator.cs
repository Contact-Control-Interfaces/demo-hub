using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetGenerator : MonoBehaviour
{
    public float[] heights;
    public float[] radii;
    public int pointsPerTier;
    public float size;
    public float spring;
    public float damper;
    public float drag;
    public float mass;
    public float gravity;

    private GameObject[] tiers;
    private List<GameObject> cylinders;
    private List<Rigidbody> allRBs;

    private int layer;

    private string NetLayerName = "IgnoreNet";

    // Start is called before the first frame update
    void Start()
    {
        layer = LayerMask.NameToLayer(NetLayerName);
        if (layer < 0)
            Debug.LogWarning(string.Format("Layer '{0}' is not defined! Please define it to enable the net to generate properly!", NetLayerName));
        else
            Physics.IgnoreLayerCollision(layer, layer, true);

        Generate();
    }

    private void Generate()
    {
        if (layer >= 0) {
            if (heights.Length + 1 == radii.Length) {
                allRBs = new List<Rigidbody>();

                if (cylinders != null) {
                    foreach (GameObject go in cylinders) {
                        Destroy(go);
                    }
                }
                cylinders = new List<GameObject>();


                if (tiers != null) {
                    foreach (GameObject go in tiers) {
                        Destroy(go);
                    }
                }
                tiers = new GameObject[radii.Length];

                // Generate each tier of spheres;
                float currentHeight = 0f;
                int currentTier = 0;
                do {
                    tiers[currentTier] = generateTier(currentTier, currentHeight);
                    tiers[currentTier].transform.parent = this.transform;
                    if (currentTier < heights.Length)
                        currentHeight += heights[currentTier++];
                    else
                        break;
                } while (currentTier <= heights.Length);


                for (int j = 0; j < pointsPerTier; j++) {
                    //tiers[0].transform.GetChild(j).gameObject.AddComponent<FixedJoint>();
                    Destroy(tiers[0].transform.GetChild(j).gameObject.GetComponent<Collider>());
                    tiers[0].transform.GetChild(j).gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }


                // Connect with cylinders (and springs)
                for (int i = 1; i < tiers.Length; i++) {
                    for (int j = 0; j < pointsPerTier; j++) {
                        GameObject temp = getCylinderBetween(tiers[i].transform.GetChild(j), tiers[i - 1].transform.GetChild(j));
                        if (i == 1)
                            Destroy(temp.GetComponent<Collider>());

                        if (i % 2 == 0) {
                            temp = getCylinderBetween(tiers[i].transform.GetChild(j), tiers[i - 1].transform.GetChild(j - 1 >= 0 ? j - 1 : pointsPerTier - 1));
                            if (i == 1)
                                Destroy(temp.GetComponent<Collider>());
                        } else {
                            temp = getCylinderBetween(tiers[i].transform.GetChild(j), tiers[i - 1].transform.GetChild(j + 1 < pointsPerTier ? j + 1 : 0));
                            if (i == 1)
                                Destroy(temp.GetComponent<Collider>());
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) {
            Generate();
        } else if (allRBs != null) {
            foreach (Rigidbody rb in allRBs) {
                rb.velocity += gravity * Vector3.up;
            }
        }
    }

    private GameObject getCylinderBetween(Transform a, Transform b)
    {
        GameObject result = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Rigidbody rb = result.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.drag = drag;
        rb.useGravity = false;
        result.layer = layer;
        cylinders.Add(result);
        //Collider temp = result.GetComponent<Collider>(); //TODO remove
        CylinderBetween cb = result.AddComponent<CylinderBetween>();
        MaestroInteractable mi = result.AddComponent<MaestroInteractable>();
        mi.type = InteractionType.Static;
        mi.IgnoreTaps = true;
        mi.Amplitude = 255;
        mi.VibrationEffect = 2;
        //Physics.IgnoreCollision(temp, a.GetComponent<Collider>(), true);
        //Physics.IgnoreCollision(temp, b.GetComponent<Collider>(), true);
        cb.a = a;
        cb.b = b;
        cb.size = size;
        //cb.Init();

        SpringJoint sj = a.gameObject.AddComponent<SpringJoint>();
        sj.damper = damper;
        sj.spring = spring;
        sj.connectedBody = b.gameObject.GetComponent<Rigidbody>();
        sj.minDistance = 0.001f;
        sj.maxDistance = 0.05f;
        sj.enablePreprocessing = false;

        return result;
    }

    private GameObject generateTier(int tier, float height)
    {
        GameObject result = new GameObject("Tier " + tier);
        float radius = radii[tier];
        float angleOffset = (Mathf.PI * 2f) / pointsPerTier;
        float start = 0;
        if (tier % 2 == 1)
            start = angleOffset / 2;

        for (int i = 0; i < pointsPerTier; i++) {
            getSphere(this.transform.position + new Vector3(radius * Mathf.Cos(start + (i * angleOffset)), height, radius * Mathf.Sin(start + (i * angleOffset)))).transform.parent = result.transform;
        }

        return result;
    }

    private GameObject getSphere(Vector3 location)
    {
        GameObject result = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        result.layer = layer;
        Rigidbody rb = result.AddComponent<Rigidbody>();
        rb.drag = drag;
        rb.mass = mass;
        rb.useGravity = false;
        allRBs.Add(rb);
        result.transform.localScale = size * Vector3.one;
        result.transform.position = location;
        return result;
    }
}
