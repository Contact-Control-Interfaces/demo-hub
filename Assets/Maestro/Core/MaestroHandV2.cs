using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Maestro
{
    public class MaestroHandV2 : IMaestroHand
    {
        public MaestroHandV2 otherHand;
        public MaestroManager manager;

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

        public override Transform Palm { get { return PalmBase; } }

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
        public MaestroIndex f1, f2;
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

        // Default haptic interaction
        public HapticEffect defaultEffect = new HapticEffect { Amplitude = 115, Vibration = 3 };
        public bool interactablesOnly = false;

        public bool settingsOverride = false;

        /*************
         *  PRIVATE  *
         *************/

        // Grab Targets
        MaestroInteractable grabTarget, twoHandGrabTarget;
        private MaestroIndex twoHandIndex;
        private bool lastTargetWasTool = false;
        private GameObject grabPos;

        // Put everything here instead of somewhere random
        private MaestroContainer mc;

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
        private GameObject thumbMeshObject, palmMeshObject;

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

        // Persistance storage
        private Dictionary<MaestroIndex, MaestroInteractable> persistInteractables;
        private Dictionary<MaestroIndex, float> persistTimes;

        private float timeSinceDropSatisfied;

        private bool arePalmMeshesTrigger = true;

        #region Monobehaviour functions
        public override void Start()
        {
            // Init collections
            persistInteractables = new Dictionary<MaestroIndex, MaestroInteractable>();
            persistTimes = new Dictionary<MaestroIndex, float>();
            palmLocations = new List<Vector3>();

            // Init pickup bools to false
            wasTwoHandGrabbing = twoHandGrabStarted = initiatedTwoHandGrab = false;

            // Create and initialize container
            GameObject temp = new GameObject(string.Format("{0} Maestro container", whichHand == WhichHand.LeftHand ? "Left" : "Right"));
            mc = temp.AddComponent<MaestroContainer>();
            mc.parent = this;
            InitContainer();

            // Initialize palm meshes
            InitPalmMeshes();

            // Turn off knuckle visibility/collision to see progress
            IEnumerable<PointOnHand> fingerknuckles = mc.Where(x => x.fc.isFingerBase);
            foreach (PointOnHand poh in fingerknuckles) {
                ToggleVisibility(poh.fc.transform, true);
            }

            // Turn off palmBox visibility/collision to see progress
            IEnumerable<CapsuleCollider> metacarpals = mc.GetFingers().Select(x => x.Metacarpal);
            foreach (CapsuleCollider cc in metacarpals) {
                ToggleVisibility(cc.transform, true);
            }

            // Ignore FCS with palm meshes
            foreach (PointOnHand poh in mc) {
                foreach (Collider c in poh.fc.GetComponents<Collider>()) {
                    Physics.IgnoreCollision(c, thumbMC, true);
                    Physics.IgnoreCollision(c, PalmCollider, true);
                }
            }

            // Ignore proximals with palm meshes
            IEnumerable<CapsuleCollider> proximals = mc.GetFingers().Select(x => x.Proximal);
            foreach (CapsuleCollider c in proximals) {
                Physics.IgnoreCollision(c, thumbMC, true);
                Physics.IgnoreCollision(c, PalmCollider, true);
            }

            // Call IMaestroHand's start
            base.Start();
        }

        private void InitPalmMeshes()
        {
            // Create both meshes
            m = new Mesh();
            thumbM = new Mesh();
            m.name = "PALM MESH";
            thumbM.name = "THUMB MESH";

            // Make GameObjects for both meshes
            thumbMeshObject = new GameObject("ThumbMesh");
            thumbMeshObject.transform.parent = mc.PalmBase.fc.transform;

            palmMeshObject = new GameObject("PalmMesh");
            palmMeshObject.transform.parent = mc.PalmBase.fc.transform;

            // Retrieve or add MeshFilters
            this.mf = palmMeshObject.GetComponent<MeshFilter>(); 
            if (!mf)
                mf = palmMeshObject.AddComponent<MeshFilter>();

            thumbMF = thumbMeshObject.GetComponent<MeshFilter>();
            if (!thumbMF)
                thumbMF = thumbMeshObject.AddComponent<MeshFilter>();

            // Get MeshRenderer
            palmMeshRenderer = palmMeshObject.GetComponent<MeshRenderer>();

            // Assign meshes to MeshFilters
            mf.mesh = m;
            thumbMF.mesh = thumbM;

            palmMeshFilter = mf;

            // Generate initial palm meshes
            CalculatePalmMeshes(mf, false);

            // Start coroutine to recalculate palm meshes
            StartCoroutine("RecalculatePalmVertices");
        }

        private FingerContainer InitFingerContainer(Transform tip, Transform middle, Transform knuckle)
        {
            PointOnHand fingerTip = SpawnPointOnHand(tip, TipSize);
            PointOnHand fingerMiddle = SpawnPointOnHand(middle, MiddleSize);
            PointOnHand fingerBase = SpawnPointOnHand(knuckle, TipSize);

            CapsuleCollider distal = SpawnCapsule(tip, middle, (TipSize + MiddleSize) / 2);
            CapsuleCollider proximal = SpawnCapsule(middle, knuckle, (MiddleSize + KnuckleSize) / 2);
            CapsuleCollider metacarpal = SpawnCapsule(knuckle, PalmBase, KnuckleSize);

            // Don't collide finger with itself
            Physics.IgnoreCollision(distal, proximal);
            Physics.IgnoreCollision(proximal, metacarpal);

            return new FingerContainer(fingerTip, fingerMiddle, fingerBase, distal, proximal, metacarpal);
        }

        private void InitContainer()
        {
            // Create and assign all fingers
            mc[WhichFinger.Thumb] = InitFingerContainer(ThumbTip, ThumbMiddle, ThumbKnuckle);
            mc[WhichFinger.Index] = InitFingerContainer(IndexTip, IndexMiddle, IndexKnuckle);
            mc[WhichFinger.Middle] = InitFingerContainer(MiddleTip, MiddleMiddle, MiddleKnuckle);
            mc[WhichFinger.Ring] = InitFingerContainer(RingTip, RingMiddle, RingKnuckle);
            mc[WhichFinger.Little] = InitFingerContainer(LittleTip, LittleMiddle, LittleKnuckle);
            mc.PalmBase = SpawnPointOnHand(PalmBase, KnuckleSize);

            // Set FC mass
            mc.ToList().ForEach(x => x.fc.rb.mass = (x.fc.isPalmBase ? 10.0f : 5.0f));

            List<FingerContainer> fingers = mc.GetFingers();
            IEnumerable<CapsuleCollider> distals = fingers.Select(x => x.Distal);
            IEnumerable<CapsuleCollider> proximals = fingers.Select(x => x.Proximal);
            IEnumerable<CapsuleCollider> metacarpals = fingers.Select(x => x.Metacarpal);

            // Ignore self collision within the hand
            foreach (PointOnHand contained in mc) {
                foreach (Collider c in contained.fc.GetComponents<Collider>()) {
                    foreach(CapsuleCollider distal in distals) {
                        Physics.IgnoreCollision(c, distal);
                    }
                    foreach (CapsuleCollider proximal in proximals) {
                        Physics.IgnoreCollision(c, proximal);
                    }
                    foreach (CapsuleCollider metacarpal in metacarpals) {
                        Physics.IgnoreCollision(c, metacarpal);
                    }
                }
            }
        }

        private void OnDisable()
        {
            // Disable all FCs when the hand itself is disabled
            SetAllFCs(false);
        }

        private void OnEnable()
        {
            // Enable all FCs when the hand itself is enabled
            SetAllFCs(true);
        }

        private void SetAllFCs(bool enabled)
        {
            if (mc != null) {
                foreach (PointOnHand poh in mc) {
                    if (poh.fc)
                        poh.fc.enabled = enabled;
                }
            }
        }

        private PointOnHand SpawnPointOnHand(Transform t, float size)
        {
            return new PointOnHand(t, Spawn(t, size));
        }

        private void ApplyGlobalInteractable(MaestroInteractable interactable, MaestroIndex index)
        {
            if (interactable.isPersistent) {
                persistInteractables[index] = interactable;
                persistTimes[index] = interactable.persistanceDuration;
            }
        }

        public void ApplyAllGlobalInteractable(MaestroInteractable interactable)
        {
            foreach (PointOnHand poh in mc.Where(x => !x.fc.isPalmBase)) {
                ApplyGlobalInteractable(interactable, poh.index);
            }
        }

        public void ApplyThumbGlobalInteractable(MaestroInteractable interactable)
        {
            ApplyGlobalInteractable(interactable, new MaestroIndex(WhichFinger.Thumb, PointOnFinger.Tip));
        }

        public void ApplyIndexGlobalInteractable(MaestroInteractable interactable)
        {
            ApplyGlobalInteractable(interactable, new MaestroIndex(WhichFinger.Index, PointOnFinger.Tip));
        }

        public void ApplyMiddleGlobalInteractable(MaestroInteractable interactable)
        {
            ApplyGlobalInteractable(interactable, new MaestroIndex(WhichFinger.Middle, PointOnFinger.Tip));
        }

        public void ApplyRingGlobalInteractable(MaestroInteractable interactable)
        {
            ApplyGlobalInteractable(interactable, new MaestroIndex(WhichFinger.Ring, PointOnFinger.Tip));
        }

        public void ApplyLittleGlobalInteractable(MaestroInteractable interactable)
        {
            ApplyGlobalInteractable(interactable, new MaestroIndex(WhichFinger.Little, PointOnFinger.Tip));
        }

        public void FixedUpdate()
        {
            // Update contact bools
            regrabbed = false;

            // Update all time variables
            timeSinceGrabbing += Time.fixedDeltaTime;
            timeSinceRelease += Time.fixedDeltaTime;
            timeSinceTwoHandGrabbing += Time.fixedDeltaTime;

            // Move all FCs
            foreach (PointOnHand poh in mc) {
                Vector3 dist = (poh.transform.position - poh.fc.rb.position);
                Vector3 lastLocation = poh.fc.lastLocation;

                // Teleport the FC back into place if we think it's stuck somewhere
                if (dist.magnitude > tooFast && (lastLocation - poh.fc.rb.position).magnitude < tooClose) {
                    poh.fc.rb.transform.position = poh.transform.position;
                } else {
                    poh.fc.rb.velocity = dist / Time.deltaTime;
                }

                poh.fc.rb.MoveRotation(poh.transform.rotation);

                //if the fingertip is touching something, and I'm not currently holding anything, check if there's more than one finger holding onto it.
                if (poh.fc.touching != null && !grabbing) {
                    CheckFingerGrabbing(poh.fc.index);
                } else {
                    //check the appropriate 
                }
            }

            // Record palm
            recordPalmLocation(mc.PalmBase.fc.rb.transform.position);

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
                Vector3 centroid = GetCentroid(mc);
                Vector3 temp = Vector3.Lerp(grabPos.transform.position, centroid, Time.fixedDeltaTime * 0.5f);
                if (!(temp.Equals(Vector3.negativeInfinity) || grabTarget.maintainPosition))
                    grabPos.transform.position = temp;
            }
        }

        protected override MaestroHapticContext ProcessHaptics()
        {
            MaestroHapticContext nextHaptics = new MaestroHapticContext();
            bool palmTouch = mc.PalmBase.Contacting;

            if (grabTarget != null && grabTarget.SendHapticsToWholeHand) {
                nextHaptics.SetAllAmplitudes(grabTarget.getMotorAmplitude());
                nextHaptics.SetAllVibrationEffects(grabTarget.getVibrationEffect());
            } else {
                // Check each finger specifically

                IEnumerable<PointOnHand> tips = mc.Where(x => x.fc.isTip);
                foreach (PointOnHand tip in tips) {
                    bool inheritFromPalm = palmTouch && mc.PalmBase.fc.touching != null;
                    float palmDiffusion = 0.65f;

                    MaestroInteractable interactable = tip.fc.touching;
                    if (interactable != null) {
                        nextHaptics.SetAmplitudeFromIndex(tip.fc.index, interactable.getMotorAmplitude());
                        nextHaptics.SetVibrationEffectFromIndex(tip.fc.index, interactable.getVibrationEffect());
                        if (interactable.isPersistent) {
                            persistInteractables[tip.index] = interactable;
                            persistTimes[tip.index] = interactable.persistanceDuration;
                        }
                    } else if (persistInteractables.ContainsKey(tip.index) && persistInteractables[tip.index] != null && persistTimes[tip.index] > Time.fixedDeltaTime) {
                        nextHaptics.SetAmplitudeFromIndex(tip.fc.index, persistInteractables[tip.index].getMotorAmplitude());
                        nextHaptics.SetVibrationEffectFromIndex(tip.fc.index, persistInteractables[tip.index].getVibrationEffect());
                        persistTimes[tip.index] -= Time.fixedDeltaTime;
                        if (persistTimes[tip.index] <= 0) {
                            persistTimes.Remove(tip.index);
                            persistInteractables.Remove(tip.index);
                        }
                    } else if (inheritFromPalm) {
                        // Inherit a portion of palm haptics if applicable
                        byte? amp = mc.PalmBase.fc.touching.getMotorAmplitude();
                        if (amp.HasValue) nextHaptics.SetAmplitudeFromIndex(tip.fc.index, (byte)(amp.Value * palmDiffusion));
                        
                    } else {
                        // Check middle joint
                        PointOnHand matchingMiddle = mc[tip.index.finger][PointOnFinger.Middle];

                        interactable = matchingMiddle.fc.touching;
                        if (inheritFromPalm) {
                            byte? amp = mc.PalmBase.fc.touching.getMotorAmplitude();
                            if (amp.HasValue) nextHaptics.SetAmplitudeFromIndex(tip.fc.index, (byte)(amp.Value * palmDiffusion));
                        } else if (interactable != null) {
                            nextHaptics.SetAmplitudeFromIndex(tip.fc.index, interactable.getMotorAmplitude());
                        }
                    }
                }
            }
            return nextHaptics;
        }
        #endregion

        #region Palm Mesh functions
        private void CalculatePrimaryPalmMesh(bool onlyVerts)
        {
            // Preform first time mesh setup
            if (!onlyVerts) {
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
            IEnumerable<PointOnHand> knuckles = mc.Where(x => x.fc.isFingerBase);
            foreach (PointOnHand poh in knuckles) {
                MeshFilter current = poh.fc.gameObject.GetComponent<MeshFilter>();

                if (current) {
                    // Vertices
                    Vector3[] verts = current.mesh.vertices;
                    for (int k = 0; k < verts.Length; k++) {
                        // Scale all these vertices by knuckle size
                        verts[k] *= (KnuckleSize);

                        // Shift them to their finger position if necessary (not palm base)
                        //if (i < 15)
                        if (poh.index.finger != WhichFinger.Thumb) //handled in other mesh
                            verts[k] += mf.transform.InverseTransformPoint(poh.fc.transform.position);
                    }
                    newVertices.AddRange(verts);

                    // UVs
                    if (!onlyVerts) {
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
            if (!onlyVerts) {
                m.uv = newUV.ToArray();

                // Set all triangles
                for (int i = 0; i < subs.Count; i++)
                    m.SetTriangles(subs[i], i);
            }
        }

        private void CalculateSecondaryPalmMesh(bool onlyVerts)
        {
            // Set clear Thumb mesh on new mesh filter 
            if (!onlyVerts) {
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
            WhichFinger[] toInclude = new WhichFinger[] { WhichFinger.Thumb, WhichFinger.Index, WhichFinger.Palm };
            IEnumerable<PointOnHand> thumbIndexPalm = mc.Where(x => (x.fc.isFingerBase || x.fc.isPalmBase) && toInclude.Contains(x.index.finger));

            foreach (PointOnHand poh in thumbIndexPalm) {
                MeshFilter current = poh.fc.gameObject.GetComponent<MeshFilter>();
                if (current) {
                    // Vertices
                    Vector3[] verts = current.mesh.vertices;
                    for (int k = 0; k < verts.Length; k++) {
                        // Scale each submesh down by the knuckle size, also shift to their rational position in world space. 
                        verts[k] *= (KnuckleSize);
                        verts[k] += mf.transform.InverseTransformPoint(poh.fc.transform.position);
                    }
                    newVertices.AddRange(verts);

                    // UVs
                    if (!onlyVerts) {
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
            if (!onlyVerts) {
                thumbM.uv = newUV.ToArray();

                // Set all triangles
                for (int i = 0; i < subs.Count; i++)
                    thumbM.SetTriangles(subs[i], i);
            }
        }

        private void TogglePalmMeshes()
        {
            if (PalmCollider == null) {
                PalmCollider = palmMeshObject.AddComponent<MeshCollider>();
                SetupMeshCollider(PalmCollider);
            }
            ToggleMeshCollider(PalmCollider);

            if (thumbMC == null) {
                thumbMC = thumbMeshObject.AddComponent<MeshCollider>();
                SetupMeshCollider(thumbMC);
            }
            ToggleMeshCollider(thumbMC);
        }

        private void SetupMeshCollider(MeshCollider meshCollider)
        {
            meshCollider.convex = true;
            meshCollider.isTrigger = arePalmMeshesTrigger;
        }

        private void ToggleMeshCollider(MeshCollider meshCollider)
        {
            if (meshCollider != null) {
                // Toggle convex off/on, which will force it to recalculate a convex collider.
                meshCollider.isTrigger = arePalmMeshesTrigger;
                meshCollider.convex = true;
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
            temp.transform.parent = mc.gameObject.transform;

            FingerCollider result = temp.AddComponent<FingerCollider>();
            result.rb = temp.AddComponent<Rigidbody>();
            result.rb.useGravity = false;
            result.rb.freezeRotation = true;
            result.hpi = this;

            if (DestroyFingerRenderersOnSpawn)
                Destroy(temp.GetComponent<Renderer>());

            return result;
        }

        private FingerCollider SpawnAtTip(Transform t)
        {
            return Spawn(t, TipSize);
        }

        private FingerCollider SpawnAtMiddle(Transform t)
        {
            return Spawn(t, MiddleSize);
        }

        private FingerCollider SpawnAtKnuckle(Transform t)
        {
            return Spawn(t, KnuckleSize);
        }

        private CapsuleCollider SpawnCapsule(Transform a, Transform b, float size)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            temp.name = a.gameObject.name + " + " + b.gameObject.name;

            temp.transform.localScale = new Vector3(size, (b.position - a.position).magnitude / 2, size);
            temp.transform.parent = mc.gameObject.transform;

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

        #region Two handed grab functions
        private void CheckTwoHandGrabbing()
        {
            List<LineSegment> possibilities = new List<LineSegment>();

            foreach (PointOnHand i in mc) {
                foreach (PointOnHand j in otherHand.mc) {
                    if (i.Contacting && j.Contacting) {
                        if (i.fc.touching != null && i.fc.touching.type == InteractionType.TwoHand && i.fc.touching == j.fc.touching) {
                            possibilities.Add(new LineSegment(i.fc.transform.position, j.fc.transform.position, i.fc.touching, i.index, j.index));
                        }
                    }
                }
            }

            if (possibilities.Count > 0) {
                possibilities.Sort(possibilities[0]);

                LineSegment last = possibilities[0];

                TwoHandGrabStart(last.interactable, last.index, last.otherIndex);
            }
        }

        private void TwoHandGrabStart(MaestroInteractable interactable, MaestroIndex index, MaestroIndex otherIndex)
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
            if (CheckTwoHandGrabDone()) {
                TwoHandGrabEnd(true);
            } else {
                timeSinceDropSatisfied = -0.5f;
                ApplyTwoHandFollowForce();
            }
        }

        private bool CheckTwoHandGrabDone()
        {
            // Check if either has at least one FC touching the target
            bool hasOne = mc.Where(x => x.fc.touching == twoHandGrabTarget).FirstOrDefault() != null;
            bool otherHasOne = otherHand.mc.Where(x => x.fc.touching == twoHandGrabTarget).FirstOrDefault() != null;

            return timeSinceTwoHandGrabbing > 0.1f && !(hasOne && otherHasOne);
        }

        private void TwoHandGrabEnd(bool drop)
        {
            Debug.Log("Ending 2 Hand");

            if (drop) {
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
            if (twoHandGrabbing && twoHandGrabTarget != null) {

                Vector3 toInBetween = (((mc[twoHandIndex].transform.position + otherHand.mc[otherHand.twoHandIndex].transform.position) / 2f) - twoHandGrabTarget.getFollowPoint());
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
            public MaestroIndex index, otherIndex;

            public LineSegment(Vector3 start, Vector3 end, MaestroInteractable interactable, MaestroIndex index, MaestroIndex otherIndex)
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
        private bool CheckFingerGrabbing(MaestroIndex position)
        {
            PointOnHand grabbed = mc[position];

            MaestroIndex? other = ShouldStartGrab(position);
            bool result = other.HasValue
                && !isFlat
                && grabbed.fc.lastTouching != null
                && grabbed.fc.lastTouching.type != InteractionType.Static
                && grabbed.fc.lastTouching.type != InteractionType.TwoHand
                && grabbed.fc.lastTouching.Equals(mc[other.Value].fc.lastTouching)
                && timeSinceRelease > 0.5f;

            if (result) {
                if (grabbing)
                    Regrab(grabbed.fc.lastTouching, position, other.Value);
                else
                    GrabStart(grabbed.fc.lastTouching, position, other.Value);
            }
            return result;
        }

        private MaestroIndex? ShouldStartGrab(MaestroIndex index)
        {
            switch (index.finger) {
                case WhichFinger.Thumb:

                    /* Thumb checks for finger tips or palm */
                    MaestroIndex? result = firstContactTip(includeThumb: false);
                    if (result == null && mc.PalmBase.Contacting) //Palm override
                        result = new MaestroIndex(WhichFinger.Palm, PointOnFinger.Base);
                    return result;

                case WhichFinger.Index: 
                case WhichFinger.Middle: 
                case WhichFinger.Ring:
                case WhichFinger.Little:

                    /* Fingers check for thumb or palm */
                    if (firstContactTip() != null) {
                        return new MaestroIndex(WhichFinger.Thumb, PointOnFinger.Tip);
                    } else if (mc.PalmBase.Contacting) {
                        return new MaestroIndex(WhichFinger.Palm, PointOnFinger.Base);
                    }
                    return null;

                default:
                    /* Palm checks all finger tips */
                    return firstContactTip();
            }
        }

        private bool CheckGrabDone()
        {
            float tempDist1 = GetDist(f1);
            float tempDist2 = GetDist(f2);

            ratio1 = tempDist1 / dist1; //grabPos
            ratio2 = tempDist2 / dist2; //grabPos

            bool tipOrMiddleTouching = mc.Where(x => (x.fc.isTip || x.fc.isMiddleJoint)).Any(x => x.fc.Contacting);

            return (grabTarget.isTool ? Mathf.Max(tempDist1, tempDist2) > 0.05f : Mathf.Min(ratio1, ratio2) > releaseRatio) || !tipOrMiddleTouching;
        }

        private void GrabStart(MaestroInteractable r, MaestroIndex finger0, MaestroIndex finger1)
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

            dist1 = GetDist(f1);
            dist2 = GetDist(f2);

            // Tell the Interactable it's been grabbed by this script
            r.Grab(objectLayer);
        }

        private float GetDist(MaestroIndex index)
        {
            Vector3 followPoint = grabTarget.getFollowPoint();
            if (grabTarget.gripCollider != null)
                followPoint = ClosestPointOnGripCollider(index, grabTarget.gripCollider);

            return (mc[index].transform.position - followPoint).magnitude;
        }

        private Vector3 ClosestPointOnGripCollider(MaestroIndex index, Collider grip)
        {
            return Physics.ClosestPoint(mc[index].transform.position, grip, grip.transform.position, grip.transform.rotation);
        }

        private void OnGrabbing()
        {
            if (isFlat || (timeSinceGrabbing > 0.25f && (CheckGrabDone() /*|| (!grabTarget.isTool && SnowconeDetected())*/)))
                GrabEnd(true);
            else
                ApplyFollowForce();
        }

        private void Regrab(MaestroInteractable r, MaestroIndex finger0, MaestroIndex finger1)
        {
            if (r == null)
                return;

            Debug.Log("Regrabbing: " + finger0 + ", " + finger1);

            grabbing = true;
            regrabbed = true;

            Vector3 centroid = GetCentroid(mc);
            if (!centroid.Equals(Vector3.negativeInfinity)) {
                Vector3 worldPos = grabTarget.transform.position;
                Quaternion worldRot = grabTarget.transform.rotation;
                grabPos.transform.position = centroid;
                grabTarget.transform.SetPositionAndRotation(worldPos, worldRot);
            }

            f1 = finger0;
            f2 = finger1;

            dist1 = GetDist(f1);
            dist2 = GetDist(f2);
        }

        private void GrabEnd(bool drop)
        {
            if (drop) {
                // Try regrab
                bool grabStarted = mc.Any(x => x.fc.touching != null && CheckFingerGrabbing(x.index));

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
                return mc.PalmBase.fc.rb.velocity;
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
        private MaestroIndex? firstContact(params MaestroIndex[] positions)
        {
            PointOnHand firstContact = mc.Where(x => positions.Contains(x.fc.index)).FirstOrDefault(x => x.Contacting);
            if (firstContact != null)
                return firstContact.fc.index;
            else return null;
        }

        private MaestroIndex? firstContactTip(bool includeThumb = true)
        {
            IEnumerable<PointOnHand> tips = mc.Where(x => x.fc.isTip);

            if (!includeThumb)
                tips = tips.Where(x => x.index.finger != WhichFinger.Thumb);

            PointOnHand firstFound = tips.FirstOrDefault(x => x.Contacting);

            if (firstFound != null)
                return firstFound.index;
            else return null;
        }

        private bool SnowconeDetected()
        {
            WhichFinger[] toInclude = new WhichFinger[] { WhichFinger.Thumb, WhichFinger.Index };
            WhichFinger[] toExclude = new WhichFinger[] { WhichFinger.Middle, WhichFinger.Ring, WhichFinger.Little };

            PointOnHand firstIncluded = mc.Where(x => toInclude.Contains(x.index.finger)
                && (x.index.point == PointOnFinger.Tip || x.index.point == PointOnFinger.Middle)).FirstOrDefault();

            PointOnHand firstExcluded = mc.Where(x => toExclude.Contains(x.index.finger)
                && (x.index.point == PointOnFinger.Tip || x.index.point == PointOnFinger.Middle)).FirstOrDefault();

            return !(mc[WhichFinger.Thumb][PointOnFinger.Tip].Contacting && mc[WhichFinger.Index][PointOnFinger.Tip].Contacting)
                && firstIncluded != null
                && firstExcluded == null;
        }

        private float FlatnessThreshold(float radius)
        {
            return radius / 2;
        }

        private Vector3 GetCentroid(MaestroContainer container)
        {
            Vector3 result = Vector3.zero;
            int count = 0;

            mc.ToList().ForEach(x => {
                if (!(x.fc.isPalmBase || x.index.finger == WhichFinger.Thumb) && x.Contacting) {
                    result += x.transform.position;
                    count++;
                }
            });

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
            fj.connectedBody = mc.PalmBase.fc.rb;
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
    }
}
