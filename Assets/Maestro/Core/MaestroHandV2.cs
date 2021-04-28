using Assets;
using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MaestroHandV2 : IMaestroHand
{
    public WhichHand whichHand = WhichHand.RightHand;
    public MaestroHandV2 otherHand;
    private MaestroGloveBehaviour parentGloveBehavior;

    #region Transforms
    public Transform ThumbTip;
    public Transform IndexTip;
    public Transform MiddleTip;
    public Transform RingTip;
    public Transform LittleTip;
    public Transform ThumbMiddle;
    public Transform IndexMiddle;
    public Transform MiddleMiddle;
    public Transform RingMiddle;
    public Transform LittleMiddle;
    public Transform ThumbKnuckle;
    public Transform IndexKnuckle;
    public Transform MiddleKnuckle;
    public Transform RingKnuckle;
    public Transform LittleKnuckle;
    public Transform PalmBase;
    #endregion

    // Sizes
    public float TipSize;
    public float MiddleSize;
    public float KnuckleSize;

    public bool showPalmMesh;
    public float palmMeshWait = 0.1f;

    public bool grabbing;

    public bool twoHandGrabbing;
    
    // Time variables
    public float timeSinceGrabbing = 1.0f;
    public float timeSinceTwoHandGrabbing = 1.0f;
    public float timeSinceRelease = 1.0f;

    // Pickup Data
    public int f1, f2;
    public float dist1, dist2;
    public float ratio1, ratio2;

    // Pickup Ratios
    public float ToolRatio = 1.3f;
    public float releaseRatio = 1.2f;

    // Respawn variables
    public float tooClose = 0.1f;
    public float tooFast = 0.2f;

    // DFROST
    public bool DestroyFingerRenderersOnSpawn;

    // Flatness
    public FlatnessChecker flatnessChecker;
    private bool isFlat { get { return flatnessChecker != null && flatnessChecker.isFlat(); } }

    // Layers
    public LayerMask objectLayer;

    /*************
     *  PRIVATE  *
     *************/

    // Contacts
    private bool[] contacts = new bool[16];
    private bool PalmContact { get { if (contacts.Length > 14) return contacts[15]; else return false; } }

    // FCs
    private FingerCollider[] fcs;
    private FingerCollider PalmFC { get { if (fcs.Length > 14) return fcs[15]; else return null; } }

    // Arrays
    private Transform[] tips, middles, knuckles, transforms;
    private CapsuleCollider[] distal, proximal, palmBox;

    // Grab Targets
    MaestroInteractable grabTarget, twoHandGrabTarget;
    private int twoHandIndex;
    private bool lastTargetWasTool = false;
    private GameObject grabPos;

    // Put everything here instead of somewhere random
    private GameObject container;

    // Why not have this
    private MeshRenderer palmMeshRenderer;

    // Mesh generation lists    
    private List<Vector3> newVertices = new List<Vector3>();
    private List<Vector2> newUV = new List<Vector2>();
    private List<int> newTriangles = new List<int>();

    // Mesh generation meshes
    private Mesh m, thumbM, palmMesh;
    private MeshFilter palmMeshFilter, mf, thumbMF;
    private MeshCollider PalmCollider, thumbMC;
    private GameObject thumbMeshObject;

    // Throw estimation
    private List<Vector3> palmLocations;
    private int throwHistory = 3;

    // Grab bools
    private bool wasgrabbing, grabStarted, regrabbed;
    private bool wasTwoHandGrabbing, twoHandGrabStarted, initiatedTwoHandGrab;

    // Grab storage
    private RigidbodyConstraints oldConstraints;
    private bool oldGravity, oldKinematic;
    private Transform oldParent = null;

    // persistance storage
    private MaestroInteractable[] persist = new MaestroInteractable[16];
    private float[] persistTimeLeft = new float[16];


    private float timeSinceDropSatisfied;

    #region Monobehaviour functions
    public void Start()
    {
        // Get parent glove behavior to retrieve pointer
        parentGloveBehavior = GetComponentInParent<MaestroGloveBehaviour>();


        palmLocations = new List<Vector3>();

        // Init pickup bools to false
        wasTwoHandGrabbing = twoHandGrabStarted = initiatedTwoHandGrab = false;


        // Sort Transforms into arrays
        tips = new Transform[] { ThumbTip,
                                  IndexTip,
                                  MiddleTip,
                                  RingTip,
                                  LittleTip };

        middles = new Transform[] { ThumbMiddle,
                                    IndexMiddle,
                                    MiddleMiddle,
                                    RingMiddle,
                                    LittleMiddle };

        knuckles = new Transform[] { ThumbKnuckle,
                                     IndexKnuckle,
                                     MiddleKnuckle,
                                     RingKnuckle,
                                     LittleKnuckle };


        // Init container
        container = new GameObject((whichHand == WhichHand.LeftHand ? "Left" : "Right") + " Maestro container");

        // Spawn FCs
        List<FingerCollider> temps = new List<FingerCollider>();
        for (int i = 0; i < 16; i++) {
            temps.Add(Spawn(i < 5 ? tips[i] : (i < 10 ? middles[i - 5] : (i < 15 ? knuckles[i - 10] : PalmBase)), i < 5 ? TipSize : (i < 10 ? MiddleSize : KnuckleSize )));
        }
        fcs = temps.ToArray();

        // Init contact bools
        ResetContacts();

        // Set FC mass
        for (int i = 0; i < fcs.Length; i++) {
            fcs[i].rb.mass = i == fcs.Length - 1 ? 10.0f : 5.0f;
        }

        // Set index to be a tip for painting
        fcs[1].isTip = true;

        // TODO add AudioSources for sound effects

        // Init all fingers
        distal = new CapsuleCollider[5];
        proximal = new CapsuleCollider[5];
        palmBox = new CapsuleCollider[5];

        for (int i = 0; i < 5; i++) {
            distal[i] = Spawn(tips[i], middles[i], (TipSize + MiddleSize) / 2);
            proximal[i] = Spawn(middles[i], knuckles[i], (MiddleSize + KnuckleSize) / 2);
            palmBox[i] = Spawn(knuckles[i], PalmBase, KnuckleSize);

            //Remove collision TODO too much?
            for (int j = 0; j < fcs.Length; j++) {
                foreach(Collider c in fcs[j].GetComponents<Collider>()){
                    Physics.IgnoreCollision(c, distal[i]);
                    Physics.IgnoreCollision(c, proximal[i]);
                    Physics.IgnoreCollision(c, palmBox[i]);
                }
            }
        }

        // Ignore capsules collision with themselves
        for (int i = 0; i < 5; i++) {
            Physics.IgnoreCollision(distal[i], proximal[i]);
            Physics.IgnoreCollision(palmBox[i], proximal[i]);
        }

        // Pull all transforms in one array
        List<Transform> trans = new List<Transform>();
        trans.AddRange(tips);
        trans.AddRange(middles);
        trans.AddRange(knuckles);
        trans.Add(PalmBase);
        transforms = trans.ToArray();

        // Generate Palm Mesh
        m = new Mesh();
        thumbM = new Mesh();
        m.name = "PALM MESH";
        thumbM.name = "THUMB MESH";

        // Get MeshFilter
        this.mf = PalmBase.gameObject.AddComponent<MeshFilter>();
        if (!mf)
            mf = PalmBase.gameObject.GetComponent<MeshFilter>();

        // Get MeshRenderer
        palmMeshRenderer = PalmBase.gameObject.GetComponent<MeshRenderer>();

        // Make Thumb Mesh
        thumbMeshObject = new GameObject("empty");
        thumbMeshObject.transform.parent = PalmBase;
        thumbMeshObject.transform.position = Vector3.zero;

        thumbMF = thumbMeshObject.GetComponent<MeshFilter>();
        if (!thumbMF)
            thumbMF = thumbMeshObject.AddComponent<MeshFilter>();

        // Turn off knuckle visibility/collision to see progress
        for (int i = 10; i < 16; i++) {
            ToggleVisibility(fcs[i].transform, true);
        }

        // Turn off palmBox visibility/collision to see progress
        foreach (CapsuleCollider cc in palmBox) {
            ToggleVisibility(cc.transform, true);
        }

        mf.mesh = m;
        thumbMF.mesh = thumbM;

        palmMeshFilter = mf;
        mf.gameObject.transform.localScale = Vector3.one;

        // Generate initial palm meshes
        CalculatePalmMeshes(mf, false);

        // Start coroutine to recalculate palm meshes
        StartCoroutine("RecalculatePalmVertices");

        // Ignore FCS with palm meshes
        for (int i = 0; i < fcs.Length; i++) {
            foreach (Collider c in fcs[i].GetComponents<Collider>()) {
                Physics.IgnoreCollision(c, thumbMC, true);
                Physics.IgnoreCollision(c, PalmCollider, true);
            }
        }
        
        // Ignore proximals with palm meshes
        for (int i = 0; i < 5; i++) {
            Physics.IgnoreCollision(proximal[i], thumbMC, true);
            Physics.IgnoreCollision(proximal[i], PalmCollider, true);
        }
    }

    private void OnDisable()
    {
        // Disable all FCs when the hand itself is disabled
        if (fcs != null) {
            foreach (FingerCollider fc in fcs) {
                if (fc)
                    fc.enabled = false;
            }
        }
        
    }

    private void OnEnable()
    {
        // Enable all FCs when the hand itself is enabled
        if (fcs != null) {
            foreach (FingerCollider fc in fcs) {
                if (fc)
                    fc.enabled = true;
            }
        }
        
    }

    public void FixedUpdate()
    {
        MaestroHapticContext currentHaptics = new MaestroHapticContext();

        // Update contact bools
        regrabbed = false;
        for (int i = 0; i < contacts.Length; i++)
            contacts[i] = fcs[i].TriggerTouching;

        /* 10 - 15 useless */

        bool palmTouch = PalmFC.TriggerTouching;

        if (grabTarget != null && grabTarget.SendHapticsToWholeHand) {
            currentHaptics.SetAllAmplitudes(grabTarget.getMotorAmplitude());
            currentHaptics.SetAllVibrationEffects(grabTarget.getVibrationEffect());
        } else {
            // Check each finger specifically
            for (int i = 0; i < tips.Length; i++) {
                

                // TODO check minimum duration

                bool inheritFromPalm = palmTouch && PalmFC.touching != null;
                float palmDiffusion = 0.65f;

                MaestroInteractable interactable = fcs[i].touching;
                if (interactable != null)
                {
                    currentHaptics.SetAmplitudeFromIndex(i, interactable.getMotorAmplitude());
                    currentHaptics.SetVibrationEffectFromIndex(i, interactable.getVibrationEffect());
                    if (interactable.isPersistent)
                    {
                        persist[i] = interactable;
                        persistTimeLeft[i] = interactable.persistanceDuration;
                    }

                } else if (persist[i] && persistTimeLeft[i] > Time.fixedDeltaTime)
                {
                    currentHaptics.SetAmplitudeFromIndex(i, persist[i].getMotorAmplitude());
                    currentHaptics.SetVibrationEffectFromIndex(i, persist[i].getVibrationEffect());
                    persistTimeLeft[i] -= Time.fixedDeltaTime;
                } else if (inheritFromPalm) {
                    // Inherit a portion of palm haptics if applicable
                    byte? amp = PalmFC.touching.getMotorAmplitude();
                    if (amp.HasValue) currentHaptics.SetAmplitudeFromIndex(i, (byte)(amp.Value * palmDiffusion));
                } else {
                    // Check middle joint
                    interactable = fcs[i + tips.Length].touching;
                    if (inheritFromPalm) {
                        byte? amp = PalmFC.touching.getMotorAmplitude();
                        if (amp.HasValue) currentHaptics.SetAmplitudeFromIndex(i, (byte)(amp.Value * palmDiffusion));
                    } else if (interactable != null) {
                        currentHaptics.SetAmplitudeFromIndex(i, interactable.getMotorAmplitude());
                    }
                }
            }
        }

        // Update all time variables
        timeSinceGrabbing += Time.fixedDeltaTime;
        timeSinceRelease += Time.fixedDeltaTime;
        timeSinceTwoHandGrabbing += Time.fixedDeltaTime;

        // Move all FCs
        for (int i = 0; i < fcs.Length; i++){

            Vector3 dist = (transforms[i].position - fcs[i].rb.position);
            Vector3 lastLocation = fcs[i].lastLocation;

            if (dist.magnitude > tooFast && (lastLocation - fcs[i].rb.position).magnitude < tooClose){
                fcs[i].rb.transform.position = transforms[i].position;
            } else {
                fcs[i].rb.velocity = dist / Time.deltaTime;
            }

            fcs[i].rb.MoveRotation(transforms[i].rotation);

            //if the fingertip is touching something, and I'm not currently holding anything, check if there's more than one finger holding onto it.
            if (fcs[i].touching != null && !grabbing) {
                CheckFingerGrabbing(i);
            } else {
                //check the appropriate 
            }
        }

        // Record palm
        recordPalmLocation(PalmFC.rb.transform.position);

        // If the user has two hands defined, check if two-hand grab has started
        if (otherHand != null && !twoHandGrabbing && !otherHand.twoHandGrabbing)
            CheckTwoHandGrabbing();

        // Update started bools
        grabStarted = !wasgrabbing && grabbing;
        twoHandGrabStarted = !wasTwoHandGrabbing && twoHandGrabbing;

        // Update one-hand grabs if necessary
        if (grabbing)
            OnGrabbing();
        else
            grabStarted = false;

        // Update two-hand grabs if necessary
        if (twoHandGrabbing && initiatedTwoHandGrab)
            OnTwoHandGrabbing();
        else
            twoHandGrabStarted = false;

        // Update was bools
        wasgrabbing = grabbing;
        wasTwoHandGrabbing = twoHandGrabbing;

        // Move grab anchor if necessary
        if (grabbing && grabPos != null && !grabTarget.isTool && !grabStarted) {
            Vector3 centroid = GetCentroid(contacts);
            Vector3 temp = Vector3.Lerp(grabPos.transform.position, centroid, Time.fixedDeltaTime * 0.5f);
            if (!(temp.Equals(Vector3.negativeInfinity) || grabTarget.maintainPosition))
                grabPos.transform.position = temp;
        }

        // Send haptics updates to glove
        MaestroNativeWrapper.SetHapticsFromContexts(parentGloveBehavior.GetPointer(), currentHaptics, lastHaptics);

        // Set variable for last set haptics
        lastHaptics = currentHaptics;
    }
    #endregion

    #region Palm Mesh functions
    private void CalculatePrimaryPalmMesh(bool onlyVerts)
    {
        // Preform first time mesh setup
        if (!onlyVerts)
        {
            m.Clear();
            mf.mesh = m;
            mf.mesh.subMeshCount = 6;
        }

        // Clear 
        newVertices.Clear();
        newUV.Clear();
        newTriangles.Clear();

        // Init variables
        List<int[]> subs = new List<int[]>();
        int subMesh = 0;
        int vertexOffset = 0;

        /* Get finger knuckles to palm */
        for (int i = 11; i < 16; i++)
        {
            MeshFilter current = fcs[i].gameObject.GetComponent<MeshFilter>();
            if (current)
            {
                // Vertices
                Vector3[] verts = current.mesh.vertices;
                for (int k = 0; k < verts.Length; k++)
                {
                    // Scale all these vertices by knuckle size
                    verts[k] *= (KnuckleSize);

                    // Shift them to their finger position if necessary (not palm base)
                    if (i < 15)
                        verts[k] += mf.transform.InverseTransformPoint(fcs[i].transform.position);
                }
                newVertices.AddRange(verts);

                // UVs
                if (!onlyVerts)
                {
                    newUV.AddRange(current.mesh.uv);

                    // Triangles, shift all indices by last batch size
                    int[] tris = current.mesh.triangles;
                    for (int k = 0; k < tris.Length; k++)
                        tris[k] += vertexOffset;
                    newTriangles.AddRange(tris);

                    // Insert this submesh into temp array (vertices must go first, but we don't have them all yet)
                    int[] result = newTriangles.ToArray();
                    subs.Insert(subMesh, result);

                    // Clear for next submesh
                    newTriangles.Clear();
                }

                // Set vertex offset to add to triangles
                vertexOffset = newVertices.Count;
            }
            subMesh++;
        }

        // Set vertices
        m.vertices = newVertices.ToArray();

        // Set uv/triangles if allowed
        if (!onlyVerts)
        {
            m.uv = newUV.ToArray();

            // Set all triangles
            for (int i = 0; i < subs.Count; i++)
                m.SetTriangles(subs[i], i);
        }
    }

    private void CalculateSecondaryPalmMesh(bool onlyVerts)
    {
        // Set clear Thumb mesh on new mesh filter 
        if (!onlyVerts)
        {
            thumbM.Clear();
            thumbMF.mesh = thumbM;
            thumbMF.mesh.subMeshCount = 4;
        }

        // Clear 
        newVertices.Clear();
        newUV.Clear();
        newTriangles.Clear();

        // Init submesh variables
        List<int[]> subs = new List<int[]>();
        int subMesh = 0;
        int vertexOffset = 0;

        /* Get Index - Thumb - Palm */
        int[] indices = new int[] { 10, 11, 15 };
        for (int i = 0; i < indices.Length; i++)
        {
            MeshFilter current = fcs[indices[i]].gameObject.GetComponent<MeshFilter>();
            if (current)
            {
                // Vertices
                Vector3[] verts = current.mesh.vertices;
                for (int k = 0; k < verts.Length; k++)
                {
                    // Scale each submesh down by the knuckle size, also shift to their rational position in world space. 
                    verts[k] *= (KnuckleSize);
                    verts[k] += mf.transform.InverseTransformPoint(fcs[indices[i]].transform.position);
                }
                newVertices.AddRange(verts);

                // UVs
                if (!onlyVerts)
                {
                    newUV.AddRange(current.mesh.uv);

                    // Triangles, so shift each batch by last batch's size
                    int[] tris = current.mesh.triangles;
                    for (int k = 0; k < tris.Length; k++)
                        tris[k] += vertexOffset;
                    newTriangles.AddRange(tris);

                    // Store the result for later, as we don't have the full result yet.
                    int[] result = newTriangles.ToArray();
                    subs.Insert(subMesh, result);
                    newTriangles.Clear();
                }

                // Set vertexOffset as current size;
                vertexOffset = newVertices.Count;
            }
            subMesh++;
        }

        // Set vertices / uv
        thumbM.vertices = newVertices.ToArray();
        if (!onlyVerts)
        {
            thumbM.uv = newUV.ToArray();

            // Set all triangles
            for (int i = 0; i < subs.Count; i++)
                thumbM.SetTriangles(subs[i], i);
        }
    }

    private void TogglePalmMeshes()
    {
        // Make sure mesh collider exists between knuckles and palm base
        if (!PalmCollider)
            PalmCollider = PalmBase.gameObject.AddComponent<MeshCollider>();

        ToggleMeshCollider(PalmCollider);

        // Make sure mesh collider exists between thumb and palm base
        if (!thumbMC)
            thumbMC = thumbMeshObject.AddComponent<MeshCollider>();

        ToggleMeshCollider(thumbMC);
    }

    private void ToggleMeshCollider(MeshCollider mc)
    {
        if (mc != null)
        {
            // Toggle convex off/on, which will force it to recalculate a convex collider.
            mc.convex = false;
            mc.convex = true;
        }
    }

    private void CalculatePalmMeshes(MeshFilter mf, bool onlyVerts)
    {
        CalculatePrimaryPalmMesh(onlyVerts);
        CalculateSecondaryPalmMesh(onlyVerts);

        TogglePalmMeshes();

        // Remove all local rotation from secondary mesh parent
        thumbMeshObject.transform.localPosition = Vector3.zero;
        thumbMeshObject.transform.localRotation = Quaternion.identity;
    }

    private IEnumerator RecalculatePalmVertices()
    {
        for (; ; )
        {
            CalculatePalmMeshes(mf, true);
            yield return new WaitForSeconds(palmMeshWait);
        }
    }
    #endregion

    private void ToggleVisibility(Transform t, bool includeCollision)
    {
        if (includeCollision) {
            Collider c = t.GetComponent<Collider>();
            c.enabled = !c.enabled;
        }

        Renderer rend = t.GetComponent<Renderer>();
        rend.enabled = !rend.enabled;
    }

    private FingerCollider Spawn(Transform t, float size)
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        temp.name = "FC: " + t.gameObject.name;
        temp.transform.localScale = size * Vector3.one;
        if (container)
            temp.transform.parent = container.transform;
        
        FingerCollider result = temp.AddComponent<FingerCollider>();
        result.rb = temp.AddComponent<Rigidbody>();
        result.rb.useGravity = false;
        result.rb.freezeRotation = true;
        result.hpi = this;

        if (DestroyFingerRenderersOnSpawn)
            Destroy(temp.GetComponent<Renderer>());

        return result;
    }

    private CapsuleCollider Spawn(Transform a, Transform b, float size)
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        temp.name = a.gameObject.name + " + " + b.gameObject.name;

        temp.transform.localScale = new Vector3(size, (b.position - a.position).magnitude / 2, size);
        if (container)
            temp.transform.parent = container.transform;

        CapsuleCollider result = temp.GetComponent<CapsuleCollider>();

        Rigidbody rb = temp.AddComponent<Rigidbody>();
        rb.useGravity = false;

        CylinderBetween cb = temp.AddComponent<CylinderBetween>();
        cb.a = a;
        cb.b = b;
        cb.size = size;
        //FingerCollider?? TODO

        if (DestroyFingerRenderersOnSpawn)
            Destroy(temp.GetComponent<Renderer>());

        return result;
    }

    private void ResetContacts()
    {
        contacts = new bool[16];
    }

    #region Two handed grab functions
    private void CheckTwoHandGrabbing()
    {
        List<LineSegment> possibilities = new List<LineSegment>();

        for (int i = 0; i < contacts.Length; i++)
        {
            for (int j = 0; j < otherHand.contacts.Length; j++)
            {
                if (contacts[i] && otherHand.contacts[j])
                {
                    if (fcs[i].touching != null && fcs[i].touching.type == InteractionType.TwoHand && fcs[i].touching == otherHand.fcs[j].touching /*phystips[i].touching.type != InteractionType.Static*/ )
                    {
                        possibilities.Add(new LineSegment(fcs[i].transform.position, otherHand.fcs[j].transform.position, fcs[i].touching, i, j));
                    }
                }
            }
        }

        if (possibilities.Count > 0)
        {
            possibilities.Sort(possibilities[0]);

            LineSegment last = possibilities[0];

            TwoHandGrabStart(last.interactable, last.index, last.otherIndex);
        }
    }

    private void TwoHandGrabStart(MaestroInteractable interactable, int index, int otherIndex)
    {
        if (interactable == null)
            return;

        twoHandGrabTarget = interactable;
        twoHandIndex = index;
        otherHand.twoHandGrabTarget = interactable;
        otherHand.twoHandIndex = otherIndex;
        twoHandGrabbing = true;
        initiatedTwoHandGrab = true;
        otherHand.twoHandGrabbing = true;
        otherHand.initiatedTwoHandGrab = false;
        timeSinceTwoHandGrabbing = 0.0f;

        interactable.rb.useGravity = false;

        if (interactable.maintainOrientation)
            interactable.rb.constraints = RigidbodyConstraints.FreezeRotation;

        interactable.Grab(objectLayer);
    }

    private void OnTwoHandGrabbing()
    {
        if (CheckTwoHandGrabDone())
        {
            TwoHandGrabEnd(true);
        }
        else
        {


            timeSinceDropSatisfied = -0.5f;
            ApplyTwoHandFollowForce();
        }
    }

    private bool CheckTwoHandGrabDone()
    {
        bool hasOne = false, otherHasOne = false;

        for (int i = 1; i < fcs.Length; i += 2)
        {
            if (fcs[i].touching == twoHandGrabTarget)
            {
                hasOne = true;
                break;
            }
        }
        hasOne |= PalmFC.touching == twoHandGrabTarget;

        for (int i = 1; i < otherHand.fcs.Length; i += 2)
        {
            if (otherHand.fcs[i].touching == twoHandGrabTarget)
            {
                otherHasOne = true;
                break;
            }
        }
        otherHasOne |= otherHand.PalmFC.touching == twoHandGrabTarget;

        return timeSinceTwoHandGrabbing > 0.1f && !(hasOne && otherHasOne);
    }

    private void TwoHandGrabEnd(bool drop)
    {
        Debug.Log("Ending 2 Hand");

        if (drop)
        {
            twoHandGrabTarget.rb.constraints = RigidbodyConstraints.None;
            twoHandGrabTarget.transform.parent = oldParent;
            oldParent = null;

            //release the thing

            twoHandGrabTarget.rb.useGravity = true;
            twoHandGrabTarget.rb.isKinematic = false;
            twoHandGrabTarget.rb.freezeRotation = false;

            twoHandGrabTarget.Release();//tell it it got dropped
        }

        //contacts = new bool[11]; TODO contacts remade

        //lastTargetWasTool = grabTarget.isTool;
        twoHandGrabTarget = null;
        otherHand.twoHandGrabTarget = null;

        twoHandGrabbing = false;
        otherHand.twoHandGrabbing = false;
    } 

    private void ApplyTwoHandFollowForce()
    {
        if (twoHandGrabbing && twoHandGrabTarget != null)
        {

            Vector3 toInBetween = (((fcs[twoHandIndex].transform.position + otherHand.fcs[otherHand.twoHandIndex].transform.position) / 2f) - twoHandGrabTarget.getFollowPoint());
            // Square vector
            toInBetween.Scale(new Vector3(Mathf.Abs(toInBetween.x), Mathf.Abs(toInBetween.y), Mathf.Abs(toInBetween.z)));

            // Dampen velocity
            twoHandGrabTarget.rb.velocity = twoHandGrabTarget.rb.velocity * 0.2f;

            // Accelerate toward center point
            twoHandGrabTarget.rb.velocity += toInBetween * Time.fixedDeltaTime * 100;

            twoHandGrabTarget.rb.angularVelocity = Vector3.zero;
        }
    }

    private struct LineSegment : IComparer<LineSegment>
    {
        public Vector3 start, end;
        public float length;
        public MaestroInteractable interactable;
        public int index, otherIndex;

        public LineSegment(Vector3 start, Vector3 end, MaestroInteractable interactable, int index, int otherIndex)
        {
            this.length = (start - end).magnitude;
            this.start = start;
            this.end = end;
            this.interactable = interactable;
            this.index = index;
            this.otherIndex = otherIndex;
        }

        public int Compare(LineSegment x, LineSegment y)
        {
            return x.length.CompareTo(y.length);
        }
    }
    #endregion

    #region One handed grab functions
    private bool CheckFingerGrabbing(int j)
    {
        int index = j / 2;
        //bool isPalm = index == 5;
        //if (isPalm) return false; //TODO?

        int other = ShouldStartGrab(index);
        bool result = other >= 0
            && !isFlat
            && fcs[j].lastTouching != null
            && fcs[j].lastTouching.type != InteractionType.Static
            && fcs[j].lastTouching.type != InteractionType.TwoHand
            && fcs[j].lastTouching.Equals(fcs[other].lastTouching)
            && timeSinceRelease > 0.5f;

        if (result)
        {
            if (grabbing)
                Regrab(fcs[j].lastTouching, j, other);
            else
                GrabStart(fcs[j].lastTouching, j, other);
        }
        return result;
    }

    private int ShouldStartGrab(int index)
    {
        switch (index)
        {
            case 0: // Thumb
                int result = firstContactTipInRange(1,4);
                if (result < 0 && PalmContact) //Palm override
                    result = 15;

                return result;
            case 1:
            case 2:
            case 3:
            case 4: // Fingers
                if (contacts[0] || contacts[5]) {
                    return contacts[0] ? 0 : 5;
                } else if (PalmContact) {
                    return 15;
                }
                return -1;

            case 5: //palm
                return firstContactTipInRange(0, 4);
            default:
                return -1;
        }
    }

    private bool CheckGrabDone()
    {

        float tempDist1 = ((fcs[f1].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(fcs[f1].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude);

        float tempDist2 = ((fcs[f2].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(fcs[f2].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude);

        ratio1 = tempDist1 / dist1; //grabPos
        ratio2 = tempDist2 / dist2; //grabPos

        bool someTips = false;
        for (int i = 0; i < 10; i++) // < 5
            someTips |= contacts[i]; // i * 2 + 1

        return (grabTarget.isTool ? Mathf.Max(tempDist1, tempDist2) > 0.05f : Mathf.Min(ratio1, ratio2) > releaseRatio) || /*(grabTarget.isTool ? false :*/ !someTips;
    }

    private void GrabStart(MaestroInteractable r, int finger0, int finger1)
    {
        if (r == null)
            return;

        Debug.Log("Start grab: " + finger0 + ", " + finger1);

        grabTarget = r;
        timeSinceGrabbing = 0;

        grabbing = true;

        grabPos = generateAnchor(grabTarget.isTool ? PalmBase.position : grabTarget.getFollowPoint());
        //grabPos.transform.rotation = Quaternion.identity;

        if (otherHand != null && otherHand.grabbing && otherHand.grabTarget == this.grabTarget) {
            oldParent = otherHand.oldParent;
            oldGravity = otherHand.oldGravity;
            oldKinematic = otherHand.oldKinematic;
            otherHand.GrabEnd(false);
        } else {
            oldParent = grabTarget.transform.parent;

            oldGravity = grabTarget.rb.useGravity;

            oldKinematic = grabTarget.rb.isKinematic;
        }

        grabTarget.rb.useGravity = false;

        oldConstraints = grabTarget.rb.constraints;
        if (oldConstraints == RigidbodyConstraints.None) {
            Vector3 worldPos = grabTarget.transform.position;
            Quaternion worldRot = grabTarget.transform.rotation;
            grabTarget.transform.parent = grabPos.transform;
            grabTarget.transform.SetPositionAndRotation(worldPos, worldRot);
            //grabTarget.transform.localPosition = Vector3.zero;
        }

        if (grabTarget.maintainOrientation)
            grabTarget.rb.constraints |= RigidbodyConstraints.FreezeRotation;

        grabTarget.rb.isKinematic = false;

        f1 = finger0;
        f2 = finger1;
        dist1 = (fcs[f1].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(fcs[f1].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos
        dist2 = (fcs[f2].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(fcs[f2].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos

        // Tell the Interactable it's been grabbed by this script
        r.Grab(objectLayer);
    }

    private void OnGrabbing()
    {
        if (isFlat || (timeSinceGrabbing > 0.25f && (CheckGrabDone() /*|| (!grabTarget.isTool && SnowconeDetected())*/)))
            GrabEnd(true);
        else
            ApplyFollowForce();
    }

    private void Regrab(MaestroInteractable r, int finger0, int finger1)
    {
        if (r == null)
            return;

        Debug.Log("Regrabbing: " + finger0 + ", " + finger1);

        grabbing = true;
        regrabbed = true;

        Vector3 centroid = GetCentroid(contacts);
        if (!centroid.Equals(Vector3.negativeInfinity)) {
            Vector3 worldPos = grabTarget.transform.position;
            Quaternion worldRot = grabTarget.transform.rotation;
            grabPos.transform.position = centroid;
            grabTarget.transform.SetPositionAndRotation(worldPos, worldRot);
        }

        f1 = finger0;
        f2 = finger1;

        dist1 = (fcs[f1].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
             Physics.ClosestPoint(fcs[f1].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos
        dist2 = (fcs[f2].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(fcs[f2].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos
    }

    private void GrabEnd(bool drop)
    {
        if (drop) {
            // Try regrab
            bool grabStarted = false;

            for (int i = 0; i < contacts.Length; i++) {
                if (fcs[i].touching != null && !grabStarted)
                    grabStarted |= CheckFingerGrabbing(i);
            }

            if (grabStarted)
                ResetContacts();

            // Drop only if regrab failed
            if (!grabStarted) {
                Debug.Log("Dropping" + timeSinceGrabbing);

                grabTarget.rb.constraints = oldConstraints;
                grabTarget.rb.AddForce(getThrowVelocity(), ForceMode.Force);
                grabTarget.transform.parent = oldParent;
                oldParent = null;

                //release the thing

                grabTarget.rb.useGravity = oldGravity;
                grabTarget.rb.isKinematic = oldKinematic;

                // Tell the MaestroInteractable it was released
                grabTarget.Release();

                // Reset grab position and contacts
                grabPos.transform.DetachChildren();
                Destroy(grabPos);
                ResetContacts();
                lastTargetWasTool = grabTarget.isTool;
                grabTarget = null;

                timeSinceRelease = 0.0f;
                grabbing = false;
            }
        } else {
            //if drop is false, a different hand has taken control of the thing and we don't need to worry about detaching it
            grabPos.transform.DetachChildren();
            lastTargetWasTool = grabTarget.isTool;
            Destroy(grabPos);
            grabbing = false;
            timeSinceRelease = 0.0f;
            grabTarget = null;
            ResetContacts();
        }
    }

    private void ApplyFollowForce()
    {
        if (grabbing && grabTarget != null) {
            if (grabTarget.stayInHand) {
                grabTarget.gameObject.transform.Translate(grabPos.transform.position - grabTarget.getFollowPoint(), Space.World);

                grabTarget.rb.angularVelocity = Vector3.zero;
            } else {
                if (!grabTarget.isTool) {
                    Vector3 dis = grabPos.transform.position - grabTarget.getFollowPoint();

                    //dampen
                    Vector3 current = grabTarget.rb.velocity;
                    current *= 0.5f;
                    grabTarget.rb.velocity = current;

                    grabTarget.rb.velocity += dis * Mathf.Pow(dis.magnitude * 750, 2) * Time.fixedDeltaTime; //prev. 500

                } else
                    grabTarget.rb.velocity = Vector3.zero;

                grabTarget.rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private Vector3 getThrowVelocity()
    {
        if (palmLocations.Count <= 1)
            return PalmFC.rb.velocity;
        else {
            Vector3 result = Vector3.zero;
            float scalar = 1f;

            //Get weighted average of palm history
            for (int i = palmLocations.Count - 1; i > 0; i--) {
                result += (palmLocations[i] - palmLocations[i - 1]) * scalar;
                scalar *= 0.5f;
            }

            return result;
        }
    }

    private void recordPalmLocation(Vector3 location)
    {
        palmLocations.Add(location);
        while (palmLocations.Count > throwHistory)
            palmLocations.RemoveAt(0);
    }
    #endregion

    #region Helper functions

    private int firstContactInRange(int start, int end)
    {
        int index = -1;
        for (int i = start; i <= end; i++) {
            if (contacts[i]) {
                index = i;
                break;
            }
        }

        return index;
    }

    private int firstContactTipInRange(int start, int end)
    {
        int index = -1;
        //if (start % 2 == 0)
        //    start++;

        for (int i = start; i <= end && i < 5; i++) {
            if (contacts[i]) {
                index = i;
                break;
            }
        }

        return index;
    }

    private bool SnowconeDetected()
    {
        return !(contacts[1] && contacts[3]) && firstContactInRange(0, 3) >= 0 && firstContactInRange(4, 9) < 0;
    }

    private float FlatnessThreshold(float radius)
    {
        return radius / 2;
    }

    public Vector3 GetCentroid(bool[] whichColliders)
    {
        Vector3 result = Vector3.zero;
        int count = 0;



        for (int i = 2 /*ignore thumb*/; i < whichColliders.Length - 1 /*Palm pulls the object down too much*/; i++) {
            if (whichColliders[i]) {
                result += fcs[i].transform.position;
                count++;
            }
        }

        if (count > 1)
            return result / count;
        else
            return Vector3.negativeInfinity;
    }

    public GameObject generateAnchor(Vector3 position)
    {
        GameObject anchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);


        anchor.transform.SetPositionAndRotation(position, Quaternion.identity);

        anchor.transform.localScale = Vector3.one * 0.01f;
        FixedJoint fj = anchor.AddComponent<FixedJoint>();
        fj.connectedBody = PalmFC.rb;
        fj.massScale = 100;
        fj.connectedMassScale = 100;

        Rigidbody rb = anchor.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0;
        rb.mass = 10;

        Destroy(anchor.GetComponent<Collider>());
        //Destroy(anchor.GetComponent<Rigidbody>());
        Destroy(anchor.GetComponent<Renderer>());

        return anchor;
    }

    private Vector3 OffsetToAngular(Quaternion a, Quaternion b, float timestep)
    {
        // Turn the difference between two quaternions to an angular velocity
        Quaternion deltaRot = b * Quaternion.Inverse(a);
        Vector3 vel = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
        vel = vel / timestep;
        vel = vel * Mathf.Deg2Rad;
        return vel;
    }
    #endregion

    #region Paint functions (temporary)
    public void ClearPaint()
    {
        if (fcs != null)
        {
            foreach (FingerCollider fc in fcs)
            {
                if (fc != null && fc.isTip && !fc.PaintColor.Equals(Color.clear))
                {
                    fc.PaintColor = Color.clear;
                }
            }
        }
    }
    #endregion
}
