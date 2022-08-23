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
        public MaestroInteractable grabTarget { get { return grabManager.grabTarget; } }

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

            IEnumerable<CapsuleCollider> distals = mc.GetFingers().Select(x => x.Distal);
            IEnumerable<CapsuleCollider> proximals = mc.GetFingers().Select(x => x.Proximal);
            IEnumerable<CapsuleCollider> metacarpals = mc.GetFingers().Select(x => x.Metacarpal);

            // Turn off distal collision
            foreach (CapsuleCollider cc in distals) {
                cc.enabled = false;
            }

            // Turn off proximal collision
            foreach (CapsuleCollider cc in proximals) {
                cc.enabled = false;
            }

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
                        
            grabManager = new ArcadeGrabManager(mc);

            // Call IMaestroHand's start
            base.Start();
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

            // Move palm
            mc.PalmContainer.transform.SetPositionAndRotation(transforms.PalmBaseThumb.position, transforms.PalmBaseThumb.rotation);

            // Make sure we're using the correct GrabManager
            if (grabManager == null || grabManager.grabType != grabType) {
                // TODO get factory or something
                if (grabType == GrabType.Arcade) {
                    grabManager = new ArcadeGrabManager(mc);
                } else {
                    grabManager = new PhysicsGrabManager(mc);
                }
            }

            grabManager.FixedUpdate();
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
            IEnumerable<CapsuleCollider> distals = fingers.Select(x => x.Distal);
            IEnumerable<CapsuleCollider> proximals = fingers.Select(x => x.Proximal);
            IEnumerable<CapsuleCollider> metacarpals = fingers.Select(x => x.Metacarpal);

            // Ignore self collision within the hand
            foreach (PointOnHand contained in mc) {
                foreach (Collider c in contained.fc.GetComponents<Collider>()) {
                    foreach (CapsuleCollider distal in distals) {
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
            return prismGenerator;
        }

        private FingerContainer InitFingerContainer(Transform tip, Transform middle, Transform knuckle)
        {
            PointOnHand fingerTip = SpawnPointOnHand(tip, handSize.TipSize);
            PointOnHand fingerMiddle = SpawnPointOnHand(middle, handSize.MiddleSize);
            PointOnHand fingerBase = SpawnPointOnHand(knuckle, handSize.TipSize);

            CapsuleCollider distal = SpawnCapsule(tip, middle, (handSize.TipSize + handSize.MiddleSize) / 2);
            CapsuleCollider proximal = SpawnCapsule(middle, knuckle, (handSize.MiddleSize + handSize.KnuckleSize) / 2);
            CapsuleCollider metacarpal = SpawnCapsule(knuckle, transforms.PalmBaseThumb, handSize.KnuckleSize);

            // Don't collide finger with itself
            Physics.IgnoreCollision(distal, proximal);
            Physics.IgnoreCollision(proximal, metacarpal);

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

            return result;
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

            bool palmTouch = prismGenerator.AnyTouching;

            // TODO do this much cleaner
            if (ForcedMotorAmplitude != null || ForcedVibrationEffect != null)
            {
                if (ForcedMotorAmplitude != null) {
                    nextHaptics.SetAllAmplitudes(ForcedMotorAmplitude);
                }

                if (ForcedVibrationEffect != null) {
                    nextHaptics.SetAllVibrationEffects(ForcedVibrationEffect);
                }
            } else if (grabTarget != null && grabTarget.SendHapticsToWholeHand) {
                nextHaptics.SetAllAmplitudes(grabTarget.currentHaptics.Amplitude);
                nextHaptics.SetAllVibrationEffects(grabTarget.currentHaptics.Vibration);
            } else {

                // Persist palm stuff
                float palmDiffusion = 0.75f;
                MaestroInteractable touchingInt = prismGenerator.Touching;
                bool inheritFromPalm = palmTouch && touchingInt != null;
                if (inheritFromPalm && !lastPalmTouch) {
                    if (touchingInt.isPersistent) {
                        PersistWholeHand(touchingInt);
                    } else {
                        nextHaptics.SetAllAmplitudes(touchingInt.currentHaptics.Amplitude);
                        nextHaptics.SetAllVibrationEffects(touchingInt.currentHaptics.Vibration);
                    }
                }

                // Check each finger specifically
                IEnumerable<PointOnHand> tips = mc.Where(x => x.fc.isTip);
                foreach (PointOnHand tip in tips) {

                    MaestroInteractable interactable = tip.fc.touching;

                    if (interactable != null) {
                        nextHaptics.SetAmplitudeFromIndex(tip.fc.index, interactable.currentHaptics.Amplitude);
                        nextHaptics.SetVibrationEffectFromIndex(tip.fc.index, interactable.currentHaptics.Vibration);
                        if (interactable.isPersistent) {
                            persistInteractables[tip.index] = interactable;
                            persistTimes[tip.index] = interactable.persistenceDuration;
                        }

                    } else if (persistInteractables.ContainsKey(tip.index) && persistInteractables[tip.index] != null && persistTimes[tip.index] > Time.fixedDeltaTime) {
                        nextHaptics.SetAmplitudeFromIndex(tip.fc.index, persistInteractables[tip.index].currentHaptics.Amplitude);
                        nextHaptics.SetVibrationEffectFromIndex(tip.fc.index, persistInteractables[tip.index].currentHaptics.Vibration);
                        persistTimes[tip.index] -= Time.fixedDeltaTime;
                        if (persistTimes[tip.index] <= 0) {
                            persistTimes.Remove(tip.index);
                            persistInteractables.Remove(tip.index);
                        }

                    } else if (tip.Contacting && !interactablesOnly) {
                        nextHaptics.SetAmplitudeFromIndex(tip.index, defaultEffect.Amplitude);
                        nextHaptics.SetVibrationEffectFromIndex(tip.index, defaultEffect.Vibration);

                    } else {
                        // Check middle joint
                        PointOnHand matchingMiddle = mc[tip.index.finger][PointOnFinger.Middle];

                        interactable = matchingMiddle.fc.touching;
                        if (inheritFromPalm) {
                            byte? amp = prismGenerator.Touching.currentHaptics.Amplitude;
                            if (amp.HasValue) nextHaptics.SetAmplitudeFromIndex(tip.fc.index, (byte)(amp.Value * palmDiffusion));
                        } else if (interactable != null) {
                            nextHaptics.SetAmplitudeFromIndex(tip.fc.index, interactable.currentHaptics.Amplitude);
                        }
                    }
                }
            }
            lastPalmTouch = palmTouch;
            return nextHaptics;
        }

        #region Helpers
        private Vector3 OffsetToAngular(Quaternion a, Quaternion b, float timestep)
        {
            // Turn the difference between two quaternions to an angular velocity
            Quaternion deltaRot = b * Quaternion.Inverse(a);
            Vector3 vel = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
            vel = vel / timestep;
            vel = vel * Mathf.Deg2Rad;
            return vel;
        }

        private T GetOrMake<T>(GameObject obj) where T : Component
        {
            T temp = obj.GetComponent<T>();
            return temp ?? obj.AddComponent<T>();
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
