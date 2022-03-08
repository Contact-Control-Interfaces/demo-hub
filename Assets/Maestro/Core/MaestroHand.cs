using Maestro;
using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

namespace Maestro
{
    public class MaestroHand : IMaestroHand
    {
        public MaestroManager manager;
        public bool settingsOverride = false;

        //override values
        public HandSize handSizeOverride;
        public bool interactablesOnlyOverride = false;
        public HapticEffect defaultEffectOverride = new HapticEffect { Amplitude = 115, Vibration = new StrongClick(FourOptions._100) };
        public FlatnessChecker flatnessCheckerOverride;
        public LayerMask objectLayerOverride;
        public float palmMeshWaitOverride = 0.1f;
        public float tooCloseOverride = 0.1f;
        public float tooFastOverride = 0.2f;
        //whichHand from IMaestroHand
        public MaestroHand otherHandOverride;
        public IGrabManager grabManager;
        public GrabType grabTypeOverride = GrabType.Arcade;

        private bool Inheriting { get { return !(settingsOverride || manager == null); } }

        #region Inherited from MaestroManager
        public GrabType grabType {
            get {
                return Inheriting ? manager.grabType : grabTypeOverride;
            }
        }

        // TODO do this much cleaner
        public byte? ForcedMotorAmplitude
        {
            get;
            set;
        }

        // TODO do this much cleaner
        [CanBeNull]
        public VibrationEffect ForcedVibrationEffect
        {
            get;
            set;
        }
        
        private HandSize handSize {
            get {
                return Inheriting ? manager.handSize : handSizeOverride;
            }
        }

        private bool interactablesOnly {
            get {
                return Inheriting ? manager.InteractablesOnly : interactablesOnlyOverride;
            }
        }

        private HapticEffect defaultEffect {
            get {
                return Inheriting ? manager.DefaultEffect : defaultEffectOverride;
            }
        }

        private FlatnessChecker flatnessChecker {
            get {
                return Inheriting ? manager.flatnessChecker : flatnessCheckerOverride;
            }
        }

        private LayerMask objectLayer {
            get {
                return Inheriting ? manager.objectLayer : objectLayerOverride;
            }
        }

        private float palmMeshWait {
            get {
                return Inheriting ? manager.palmMeshWait : palmMeshWaitOverride;
            }
        }

        private float tooClose {
            get {
                return Inheriting ? manager.tooClose : tooCloseOverride;
            }
        }

        private float tooFast {
            get {
                return Inheriting ? manager.tooFast : tooFastOverride;
            }
        }

        private WhichHand whichHandInherited {
            get {
                if (!Inheriting && (this == manager.LeftHand || this == manager.RightHand))
                    return whichHand;
                else return this == manager.RightHand ? WhichHand.RightHand : WhichHand.LeftHand;
            }
        }

        public MaestroHand otherHand {
            get {
                return Inheriting ? (whichHandInherited == WhichHand.RightHand ? manager.LeftHand : manager.RightHand)
                    : otherHandOverride;
            }
        }
        #endregion

        public HandTransforms transforms;

        public override Transform Palm { get { return transforms.PalmBase; } }

        //TODO check all things we're holding, instead of just one
        public MaestroInteractable grabTarget { get { return grabManager.grabTarget; } }

        public bool showPalmMesh;

        public bool grabbing { get { return grabManager == null ? false : grabManager.isGrabbing; } }

        public bool twoHandGrabbing;

        // Time variables
        public float timeSinceTwoHandGrabbing = 1.0f;

        // DFROST
        public bool DestroyFingerRenderersOnSpawn;
        public bool RenderOnTop;

        // Flatness
        public bool isFlat { get { return flatnessChecker != null && flatnessChecker.isFlat(); } }

        /*************
         *  PRIVATE  *
         *************/

        // Grab Targets
        MaestroInteractable twoHandGrabTarget;
        private MaestroIndex twoHandIndex;

        // Put everything here instead of somewhere random
        public MaestroContainer mc;

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

        // Grab bools
        private bool wasTwoHandGrabbing, twoHandGrabStarted, initiatedTwoHandGrab;
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
                        
            grabManager = new ArcadeGrabManager(mc);

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
            PointOnHand fingerTip = SpawnPointOnHand(tip, handSize.TipSize);
            PointOnHand fingerMiddle = SpawnPointOnHand(middle, handSize.MiddleSize);
            PointOnHand fingerBase = SpawnPointOnHand(knuckle, handSize.TipSize);

            CapsuleCollider distal = SpawnCapsule(tip, middle, (handSize.TipSize + handSize.MiddleSize) / 2);
            CapsuleCollider proximal = SpawnCapsule(middle, knuckle, (handSize.MiddleSize + handSize.KnuckleSize) / 2);
            CapsuleCollider metacarpal = SpawnCapsule(knuckle, transforms.PalmBase, handSize.KnuckleSize);

            // Don't collide finger with itself
            Physics.IgnoreCollision(distal, proximal);
            Physics.IgnoreCollision(proximal, metacarpal);

            return new FingerContainer(fingerTip, fingerMiddle, fingerBase, distal, proximal, metacarpal);
        }

        private void InitContainer()
        {
            // Create and assign all fingers
            mc[WhichFinger.Thumb] = InitFingerContainer(transforms.ThumbTip, transforms.ThumbMiddle, transforms.ThumbKnuckle);
            mc[WhichFinger.Index] = InitFingerContainer(transforms.IndexTip, transforms.IndexMiddle, transforms.IndexKnuckle);
            mc[WhichFinger.Middle] = InitFingerContainer(transforms.MiddleTip, transforms.MiddleMiddle, transforms.MiddleKnuckle);
            mc[WhichFinger.Ring] = InitFingerContainer(transforms.RingTip, transforms.RingMiddle, transforms.RingKnuckle);
            mc[WhichFinger.Little] = InitFingerContainer(transforms.LittleTip, transforms.LittleMiddle, transforms.LittleKnuckle);
            mc.PalmBase = SpawnPointOnHand(transforms.PalmBase, handSize.KnuckleSize);

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
                persistTimes[index] = interactable.persistenceDuration;
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
            // Update all time variables
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

                poh.fc.rb.MoveRotation(poh.transform.rotation.normalized);
            }

            // Make sure we're using the correct GrabManager
            if (grabManager == null || grabManager.grabType != grabType) {
                grabManager = new ArcadeGrabManager(mc);
            }

            grabManager.FixedUpdate();
        }

        protected override MaestroHapticContext ProcessHaptics()
        {
            MaestroHapticContext nextHaptics = new MaestroHapticContext();
            bool palmTouch = mc.PalmBase.Contacting;

            // TODO do this much cleaner
            if (ForcedMotorAmplitude != null || ForcedVibrationEffect != null)
            {
                if (ForcedMotorAmplitude != null)
                {
                    nextHaptics.SetAllAmplitudes(ForcedMotorAmplitude);
                }

                if (ForcedVibrationEffect != null)
                {
                    nextHaptics.SetAllVibrationEffects(ForcedVibrationEffect);
                }
            } else if (grabTarget != null && grabTarget.SendHapticsToWholeHand) {
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
                            persistTimes[tip.index] = interactable.persistenceDuration;
                        }

                    } else if (persistInteractables.ContainsKey(tip.index) && persistInteractables[tip.index] != null && persistTimes[tip.index] > Time.fixedDeltaTime) {
                        nextHaptics.SetAmplitudeFromIndex(tip.fc.index, persistInteractables[tip.index].getMotorAmplitude());
                        nextHaptics.SetVibrationEffectFromIndex(tip.fc.index, persistInteractables[tip.index].getVibrationEffect());
                        persistTimes[tip.index] -= Time.fixedDeltaTime;
                        if (persistTimes[tip.index] <= 0) {
                            persistTimes.Remove(tip.index);
                            persistInteractables.Remove(tip.index);
                        }

                    } else if (tip.Contacting && !interactablesOnly) {
                        nextHaptics.SetAmplitudeFromIndex(tip.index, defaultEffect.Amplitude);
                        nextHaptics.SetVibrationEffectFromIndex(tip.index, defaultEffect.Vibration);

                    } else if (inheritFromPalm) {
                        // Inherit a portion of palm haptics if applicable
                        byte? amp = mc.PalmBase.fc.touching.getMotorAmplitude();
                        if (amp.HasValue) {
                            //Debug.Log(mc.PalmBase.fc.touching.gameObject.name);
                            nextHaptics.SetAmplitudeFromIndex(tip.fc.index, (byte)(amp.Value * palmDiffusion));
                        }

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
                        verts[k] *= (handSize.KnuckleSize);

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
                        verts[k] *= (handSize.KnuckleSize);
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

            InitRenderer(temp.GetComponent<Renderer>());

            return result;
        }

        private FingerCollider SpawnAtTip(Transform t)
        {
            return Spawn(t, handSize.TipSize);
        }

        private FingerCollider SpawnAtMiddle(Transform t)
        {
            return Spawn(t, handSize.MiddleSize);
        }

        private FingerCollider SpawnAtKnuckle(Transform t)
        {
            return Spawn(t, handSize.KnuckleSize);
        }

        private void InitRenderer(Renderer renderer)
        {
            if (renderer == null)
                return;

            if (DestroyFingerRenderersOnSpawn)
                Destroy(renderer);
            else if (RenderOnTop) {
                Shader showAlwaysShader = Shader.Find("GUI/Text Shader");

                if (renderer != null && showAlwaysShader != null) {
                    renderer.material.shader = showAlwaysShader;
                    renderer.material.color = Color.red;
                }
            }
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

            rb.mass = 10;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            CylinderBetween cb = temp.AddComponent<CylinderBetween>();
            cb.a = a;
            cb.b = b;
            cb.size = size;
            //FingerCollider?? TODO

            InitRenderer(temp.GetComponent<Renderer>());

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

        #region Helper functions

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
