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
        public GrabType grabTypeOverride = GrabType.Arcade;

        #region Inherited from MaestroManager
        public GrabType grabType {
            get {
                return Inheriting ? manager.grabType : grabTypeOverride;
            }
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

        private PhysicMaterial physicMaterial {
            get {
                return Inheriting ? manager.handPhysicMaterial : handPhysicMaterial;
            }
        }

        private FlatnessChecker flatnessChecker {
            get {
                return (Inheriting && manager.flatnessChecker != null) ? manager.flatnessChecker : flatnessCheckerOverride;
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

        public override Transform Palm { get { return transforms.PalmBaseThumb; } }

        //TODO check all things we're holding, instead of just one
        public MaestroInteractable grabTarget { get { return grabManager == null ? null : grabManager.grabTarget; } }

        public bool inflatePalm;

        public bool grabbing { get { return grabManager == null ? false : grabManager.isGrabbing; } }

        public bool twoHandGrabbing;

        // Time variables
        public float timeSinceTwoHandGrabbing = 1.0f;

        // Debug
        public bool DestroyRenderersOnSpawn;
        public bool RenderOnTop;
        public bool ShowCollidersWhileTouching = true;

        // Flatness
        public bool isFlat { get { return flatnessChecker != null && flatnessChecker.isFlat(); } }

        // Put everything here instead of somewhere random
        public MaestroContainer mc;

        public PhysicMaterial handPhysicMaterial;

        /*************
         *  PRIVATE  *
         *************/

        // Palm approximation
        private PrismGenerator prismGenerator;

        // Persistance storage
        private Dictionary<MaestroIndex, MaestroInteractable> persistInteractables;
        private Dictionary<MaestroIndex, float> persistTimes;

        private bool Inheriting { get { return !(settingsOverride || manager == null); } }
        private bool lastPalmTouch;

        #region Monobehaviour functions
        public override void Start()
        {
            // Init collections
            persistInteractables = new Dictionary<MaestroIndex, MaestroInteractable>();
            persistTimes = new Dictionary<MaestroIndex, float>();

            this.ShowCollidersWhileTouching = true;

            // Create and initialize container
            GameObject temp = new GameObject(string.Format("{0} Maestro container", whichHand == WhichHand.LeftHand ? "Left" : "Right"));
            mc = temp.AddComponent<MaestroContainer>();
            mc.parent = this;
            InitContainer();

            IEnumerable<Collider> distals = mc.GetFingers().Select(x => x.Distal.transform.GetComponent<Collider>());
            IEnumerable<Collider> proximals = mc.GetFingers().Select(x => x.Proximal.transform.GetComponent<Collider>());
            IEnumerable<Collider> metacarpals = mc.GetFingers().Select(x => x.Metacarpal.transform.GetComponent<Collider>());

            // Turn off bottom knuckles, covered by the palm meshes
            IEnumerable<PointOnHand> fingerknuckles = mc.Where(x => x.fc.isFingerBase);
            foreach (PointOnHand poh in fingerknuckles) {
                SetVisibility(poh.fc.transform, false);
            }

            // Turn off metacarpals, covered by the palm meshes
            foreach (CapsuleCollider cc in metacarpals) {
                cc.gameObject.SetActive(false);
            }

            // Make sure fingertips are seen
            IEnumerable<PointOnHand> tips = mc.GetFingers().Select(x => x.Tip);
            foreach (PointOnHand poh in tips) {
                SetVisibility(poh.fc.transform, true);
            }

            ValidateGrabManager();

            // Call IMaestroHand's start
            base.Start();
        }

        public void FixedUpdate()
        {
            // Update all time variables
            timeSinceTwoHandGrabbing += Time.fixedDeltaTime;

            // Move all FCs
            foreach (PointOnHand poh in mc) {
                // Skip digits as CylinderBetween should handle it
                if (poh.whereOnFinger == PointOnFinger.Distal || poh.whereOnFinger == PointOnFinger.Proximal)
                    continue;

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

            // Move palm
            mc.PalmContainer.transform.SetPositionAndRotation(transforms.PalmBaseThumb.position, transforms.PalmBaseThumb.rotation);

            // Make sure we're using the correct GrabManager
            ValidateGrabManager();

            if (grabManager != null) {
                grabManager.FixedUpdate();
            }
        }

        public override void OnDisable()
        {
            // Disable all FCs when the hand itself is disabled
            SetAllFCs(false);

            base.OnDisable();
        }

        private void OnEnable()
        {
            // Enable all FCs when the hand itself is enabled
            SetAllFCs(true);
        }
        #endregion

        #region Init/Spawning
        private void InitContainer()
        {
            // Create and assign all fingers
            mc[WhichFinger.Thumb] = InitFingerContainer(transforms.ThumbTip, transforms.ThumbMiddle, transforms.ThumbKnuckle);
            mc[WhichFinger.Index] = InitFingerContainer(transforms.IndexTip, transforms.IndexMiddle, transforms.IndexKnuckle);
            mc[WhichFinger.Middle] = InitFingerContainer(transforms.MiddleTip, transforms.MiddleMiddle, transforms.MiddleKnuckle);
            mc[WhichFinger.Ring] = InitFingerContainer(transforms.RingTip, transforms.RingMiddle, transforms.RingKnuckle);
            mc[WhichFinger.Little] = InitFingerContainer(transforms.LittleTip, transforms.LittleMiddle, transforms.LittleKnuckle);

            PrismGenerator prisms = InitPalm();
            mc.PalmContainer = prisms.transform;

            // Set FC mass
            mc.ToList().ForEach(x => x.fc.rb.mass = (x.fc.isPalm ? 10.0f : 5.0f));

            List<FingerContainer> fingers = mc.GetFingers();
            IEnumerable<Collider> distals = fingers.Select(x => x.Distal.transform.GetComponent<Collider>());
            IEnumerable<Collider> proximals = fingers.Select(x => x.Proximal.transform.GetComponent<Collider>());
            IEnumerable<Collider> metacarpals = fingers.Select(x => x.Metacarpal.transform.GetComponent<Collider>());

            // Ignore self collision within the hand
            foreach (PointOnHand contained in mc) {
                foreach (Collider c in contained.fc.GetComponents<Collider>()) {
                    foreach (Collider distal in distals) {
                        Physics.IgnoreCollision(c, distal);
                    }
                    foreach (Collider proximal in proximals) {
                        Physics.IgnoreCollision(c, proximal);
                    }
                    foreach (Collider metacarpal in metacarpals) {
                        Physics.IgnoreCollision(c, metacarpal);
                    }
                }
            }

            // Don't let palm and proximal digits interact
            var prismColliders = prisms.prisms.Select(x => x.GetComponent<Collider>());
            foreach (Collider p in prismColliders) {
                foreach (CapsuleCollider c in proximals) {
                    Physics.IgnoreCollision(p, c);
                }
            }
        }

        private PrismGenerator InitPalm()
        {
            // Make gameobject to house prisms
            GameObject prismsObject = new GameObject("prisms");
            prismsObject.transform.parent = mc.transform;
            prismsObject.transform.localPosition = Vector3.zero;

            // Init prisms
            prismGenerator = prismsObject.AddComponent<PrismGenerator>();
            prismGenerator.thickness = this.handSize.KnuckleSize;
            prismGenerator.destroyRenderers = this.DestroyRenderersOnSpawn;
            prismGenerator.container = mc;
            List<Transform> points = new List<Transform>();
            List<float> depths = new List<float>();

            // Main palm thumb prism
            points.Add(transforms.PalmCreaseThumb);
            points.Add(transforms.ThumbKnuckle);
            points.Add(transforms.PalmBaseThumb);

            // Main palm little prism
            points.Add(transforms.PalmBaseLittle);
            points.Add(transforms.PalmCreaseLittle);
            points.Add(transforms.PalmBaseThumb);

            // Palm middle prisms
            points.Add(transforms.PalmCreaseMiddle);
            points.Add(transforms.PalmBaseThumb);
            points.Add(transforms.PalmCreaseLittle);

            points.Add(transforms.PalmBaseThumb);
            points.Add(transforms.PalmCreaseMiddle);
            points.Add(transforms.PalmCreaseThumb);

            // Index flap
            points.Add(transforms.PalmCreaseThumb);
            points.Add(transforms.BetweenIndexMiddle);
            points.Add(transforms.IndexKnuckle);

            // Little flap
            points.Add(transforms.BetweenRingLittle);
            points.Add(transforms.PalmCreaseLittle);
            points.Add(transforms.LittleKnuckle);

            // Middle/ring flap
            points.Add(transforms.PalmCreaseMiddle);
            points.Add(transforms.BetweenRingLittle);
            points.Add(transforms.BetweenIndexMiddle);

            // Fill in remaining
            points.Add(transforms.PalmCreaseThumb);
            points.Add(transforms.PalmCreaseMiddle);
            points.Add(transforms.BetweenIndexMiddle);

            points.Add(transforms.PalmCreaseMiddle);
            points.Add(transforms.PalmCreaseLittle);
            points.Add(transforms.BetweenRingLittle);

            prismGenerator.points = points.ToArray();
            prismGenerator.Init(physicMaterial);
            return prismGenerator;
        }

        private FingerContainer InitFingerContainer(Transform tip, Transform middle, Transform knuckle)
        {
            PointOnHand fingerTip = SpawnPointOnHand(tip, handSize.TipSize);
            PointOnHand fingerMiddle = SpawnPointOnHand(middle, handSize.MiddleSize);
            PointOnHand fingerBase = SpawnPointOnHand(knuckle, handSize.TipSize);

            PointOnHand distal = SpawnPointOnHand(tip, middle, (handSize.TipSize + handSize.MiddleSize) / 2);
            PointOnHand proximal = SpawnPointOnHand(middle, knuckle, (handSize.MiddleSize + handSize.KnuckleSize) / 2);
            PointOnHand metacarpal = SpawnPointOnHand(knuckle, transforms.PalmBaseThumb, handSize.KnuckleSize);

            // Don't collide finger with itself
            Collider proximalCollider = proximal.transform.GetComponent<Collider>();
            Physics.IgnoreCollision(distal.transform.GetComponent<Collider>(), proximalCollider); ;
            Physics.IgnoreCollision(proximalCollider, metacarpal.transform.GetComponent<Collider>());

            return new FingerContainer(fingerTip, fingerMiddle, fingerBase, distal, proximal, metacarpal);
        }

        private void InitRenderer(Renderer renderer)
        {
            if (renderer == null)
                return;

            if (DestroyRenderersOnSpawn)
                Destroy(renderer);
            else if (RenderOnTop) {
                Shader showAlwaysShader = Shader.Find("GUI/Text Shader");

                if (renderer != null && showAlwaysShader != null) {
                    renderer.material.shader = showAlwaysShader;
                    renderer.material.color = Color.red;
                }
            }
        }

        private PointOnHand SpawnPointOnHand(Transform a, Transform b, float size)
        {
            CapsuleCollider capsule = SpawnCapsule(a, b, size);

            FingerCollider fc = capsule.gameObject.AddComponent<FingerCollider>();
            if (fc.rend != null)
                InitRenderer(fc.rend);
            fc.SetParentHPI(this);

            return new PointOnHand(capsule.gameObject.transform, fc);
        }

        private PointOnHand SpawnPointOnHand(Transform t, float size)
        {
            return new PointOnHand(t, Spawn(t, size));
        }

        private FingerCollider Spawn(Transform t, float size)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.name = "FC: " + t.gameObject.name;
            temp.transform.localScale = size * Vector3.one;
            temp.transform.parent = mc.gameObject.transform;

            FingerCollider result = temp.AddComponent<FingerCollider>();
            if (result.rend != null)
                InitRenderer(result.rend);
            result.rb.useGravity = false;
            result.rb.freezeRotation = true;
            result.SetParentHPI(this);

            Collider c = result.GetComponent<Collider>();
            c.sharedMaterial = physicMaterial;

            return result;
        }

        private CapsuleCollider SpawnCapsule(Transform a, Transform b, float size)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            temp.name = a.gameObject.name + " + " + b.gameObject.name;

            temp.transform.localScale = new Vector3(size, (b.position - a.position).magnitude / 2, size);
            temp.transform.parent = mc.gameObject.transform;

            CapsuleCollider result = temp.GetComponent<CapsuleCollider>();
            result.sharedMaterial = physicMaterial;

            Rigidbody rb = temp.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            rb.mass = 10;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            CylinderBetween cb = temp.AddComponent<CylinderBetween>();
            cb.a = a;
            cb.b = b;
            cb.size = size;
            //FingerCollider?? TODO

            Renderer r = temp.GetComponent<Renderer>();
            InitRenderer(r);
            r.enabled = false; //TODO

            return result;
        }
        #endregion

        private void ApplyGlobalInteractable(MaestroInteractable interactable, MaestroIndex index)
        {
            if (interactable.isPersistent) {
                persistInteractables[index] = interactable;
                persistTimes[index] = interactable.persistenceDuration;
            }
        }

        public void ApplyAllGlobalInteractable(MaestroInteractable interactable)
        {
            foreach (PointOnHand poh in mc.Where(x => !x.fc.isPalm)) {
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

        private void PersistPalm(MaestroIndex mi, MaestroInteractable interactable, float duration = 0.25f)
        {
            persistTimes[mi] = interactable.isPersistent ? interactable.persistenceDuration : duration;
            persistInteractables[mi] = interactable;
        }

        public void PersistWholeHand(MaestroInteractable interactable, float duration = 0.25f)
        {
            List<MaestroIndex> toInclude = new List<MaestroIndex>();
            toInclude.Add(new MaestroIndex(WhichFinger.Thumb, PointOnFinger.Tip));
            toInclude.Add(new MaestroIndex(WhichFinger.Index, PointOnFinger.Tip));
            toInclude.Add(new MaestroIndex(WhichFinger.Middle, PointOnFinger.Tip));
            toInclude.Add(new MaestroIndex(WhichFinger.Ring, PointOnFinger.Tip));
            toInclude.Add(new MaestroIndex(WhichFinger.Little, PointOnFinger.Tip));

            foreach (MaestroIndex mi in toInclude) {
                PersistPalm(mi, interactable, duration);
            }
        }

        protected override MaestroHapticContext ProcessHaptics()
        {
            MaestroHapticContext nextHaptics = new MaestroHapticContext();

            if (WholeHandReverb > 0) {
                nextHaptics.SetAllAmplitudes(255);
                WholeHandReverb -= Time.fixedDeltaTime;
            }

            if (grabTarget != null && grabTarget.SendHapticsToWholeHand) {
                nextHaptics.SetAllAmplitudes(grabTarget.currentHaptics.Amplitude);
                nextHaptics.SetAllVibrationEffects(grabTarget.currentHaptics.Vibration);
            } else {

                foreach (FingerContainer finger in mc.GetFingers()) {

                    MaestroIndex? touchIndex = null;
                    MaestroIndex? contactIndex = null;

                    // Traverse tip to base, get first that's touching something
                    foreach (PointOnHand poh in finger) {
                        if (!contactIndex.HasValue && poh.fc.Contacting) {
                            contactIndex = poh.index;
                        }

                        if (poh.fc.touching) {
                            touchIndex = poh.index;
                            if (!contactIndex.HasValue)
                                contactIndex = touchIndex;
                            break;
                        }
                    }

                    if (touchIndex.HasValue) {
                        // We're touching some MaestroInteractable with this finger
                        PointOnFinger source = touchIndex.Value.point;
                        MaestroInteractable touching = finger[source].fc.touching;
                        float weight = GetWeight(source);

                        if (touching != null) {
                            nextHaptics.SetAmplitudeFromIndex(touchIndex.Value, (byte)(weight * touching.currentHaptics.Amplitude));

                            if (ShouldApplyVibration(source)) {
                                nextHaptics.SetVibrationEffectFromIndex(touchIndex.Value, touching.currentHaptics.Vibration);
                            }

                            if (touching.isPersistent) {
                                SetPersistentHaptics(touchIndex.Value, touching);
                            }
                        }
                    } else if (prismGenerator.AnyTouching && prismGenerator.Touching != null) {
                        // The palm is touching some MaestroInteractable
                        float palmDiffusion = 0.75f;
                        MaestroInteractable touching = prismGenerator.Touching;
                        if (touching.isPersistent) {
                            PersistWholeHand(touching);
                        } else {
                            nextHaptics.SetAllAmplitudes((byte)(palmDiffusion * touching.currentHaptics.Amplitude));

                            // Ignore palm vibration
                            //nextHaptics.SetAllVibrationEffects(touchingInt.currentHaptics.Vibration);
                        }
                    } else if (contactIndex.HasValue && !interactablesOnly) {
                        // This finger is touching some generic collider in the scene
                        PointOnFinger where = contactIndex.Value.point;

                        nextHaptics.SetAmplitudeFromIndex(contactIndex.Value, (byte)(GetWeight(where) * defaultEffect.Amplitude));

                        if (ShouldApplyVibration(contactIndex.Value.point)) {
                            nextHaptics.SetVibrationEffectFromIndex(contactIndex.Value, defaultEffect.Vibration);
                        }
                    }
                }
            }

            return nextHaptics;
        }

        #region Helpers

        private float GetWeight(PointOnFinger where)
        {
            return where switch {
                PointOnFinger.Tip => 1.0f,
                PointOnFinger.Distal => 1.0f,
                PointOnFinger.Middle => 0.75f,
                PointOnFinger.Proximal => 0.5f,
                PointOnFinger.Base => 0.25f,
                _ => 0f
            };
        }

        private bool ShouldApplyVibration(PointOnFinger where)
        {
            return where switch {
                PointOnFinger.Tip => true,
                PointOnFinger.Distal => true,
                _ => false
            };
        }

        private bool HasPersistentHaptics(MaestroIndex index)
        {
            return persistInteractables.ContainsKey(index)
                && persistInteractables[index] != null
                && persistTimes[index] > Time.fixedDeltaTime;
        }

        private void SetPersistentHaptics(MaestroIndex index, MaestroInteractable persistent)
        {
            persistInteractables[index] = persistent;
            persistTimes[index] = persistent.persistenceDuration;
        }

        private void UpdatePersistentHaptics(MaestroIndex index)
        {
            persistTimes[index] -= Time.fixedDeltaTime;
            if (persistTimes[index] <= 0) {
                persistTimes.Remove(index);
                persistInteractables.Remove(index);
            }
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

        private void ValidateGrabManager()
        {
            if (grabType == GrabType.None) {
                grabManager = null;
            } else if (grabManager == null || grabManager.grabType != grabType) {
                switch (grabType) {
                    default: throw new ArgumentOutOfRangeException($"No grab manager defined for type {grabType}!");
                    case GrabType.Arcade: grabManager = new ArcadeGrabManager(mc); break;
                    case GrabType.Physics: grabManager = new PhysicsGrabManager(mc); break;
                }
            }
        }

        private void SetAllFCs(bool enabled)
        {
            if (mc != null) {
                foreach (PointOnHand poh in mc) {
                    if (poh.fc != null)
                        poh.fc.enabled = enabled;
                }
            }
        }

        private void SetVisibility(Transform t, bool value, bool includeCollision = true)
        {
            if (includeCollision) {
                Collider c = t.GetComponent<Collider>();
                if (c != null)
                    c.enabled = value;
            }

            Renderer rend = t.GetComponent<Renderer>();
            rend.enabled = value;
        }

        public static void SetOnlyWhileTouching(bool state)
        {
            MaestroHand[] hands = GameObject.FindObjectsOfType<MaestroHand>();
            foreach (MaestroHand mh in hands) {
                mh.ShowOnlyWhileTouching = state;
            }
        }
        #endregion

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
    }
}
