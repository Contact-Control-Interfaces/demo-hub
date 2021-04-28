using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maestro.Haptics.ForceFeedback;
using Maestro.Haptics.Vibration;
using System;
using Assets;
using Plane = UnityEngine.Plane;

public class MaestroHand : IMaestroHand
{

    public GameObject ThumbMiddle { get { return colliders[0]; } set { colliders[0] = value; } }
    public GameObject ThumbTip { get { return colliders[1]; } set { colliders[1] = value; } }
    public GameObject ThumbMeta { get { return extras[0]; } set { extras[0] = value; } }

    public GameObject IndexTip { get { return colliders[3]; } set { colliders[3] = value; } }
    public GameObject IndexMiddle { get { return colliders[2]; } set { colliders[2] = value; } }

    public GameObject MiddleTip { get { return colliders[5]; } set { colliders[5] = value; } }
    public GameObject MiddleMiddle { get { return colliders[4]; } set { colliders[4] = value; } }

    public GameObject RingTip { get { return colliders[7]; } set { colliders[7] = value; } }
    public GameObject RingMiddle { get { return colliders[6]; } set { colliders[6] = value; } }

    public GameObject LittleTip { get { return colliders[9]; } set { colliders[9] = value; } }
    public GameObject LittleMiddle { get { return colliders[8]; } set { colliders[8] = value; } }

    public GameObject Palm { get { return colliders[10]; } set { colliders[10] = value; } }

    [Header("Hand Configuration")]
    public WhichHand whichHand = WhichHand.RightHand;

    public MaestroHand otherHand;
    public string objectLayer = "GrippedObject";
    public string ignoreLayer = "IgnoreWhileGripped";

    public GameObject[] colliders = new GameObject[11];
    public GameObject[] extras = new GameObject[1];

    public MaestroManager manager;
    public FlatnessChecker flatnessChecker;
    private bool isFlat { get { return flatnessChecker != null && flatnessChecker.isFlat(); } }

    public float tooClose = 0.1f;
    public float tooFast = 0.2f;

    private FingerCollider[] phystips = new FingerCollider[12];
    private PullOnCollideBehaviour[] pulls = new PullOnCollideBehaviour[5];
    private VibrateOnCollideBehaviour[] vibs = new VibrateOnCollideBehaviour[5];

    MaestroInteractable grabTarget, twoHandGrabTarget;
    private bool lastTargetWasTool = false;
    private GameObject grabPos;

    private Transform oldParent = null;

    private bool wasgrabbing;
    private bool grabStarted;
    private bool regrabbed;

    private bool oldGravity;
    private bool oldKinematic;

    private bool twoHandGrabStarted;
    private bool wasTwoHandGrabbing;
    public bool twoHandGrabbing;
    private int twoHandIndex;

    private RigidbodyConstraints oldConstraints;

    private Quaternion planeOffset;
    //private Insphere insphere;
    //private GameObject insphereDisplay;

    [Header("Debug")]
    public bool showColliders;
    public bool[] contacts;
    public PhysicMaterial fingertipPhysicMaterial;

    public bool grabbing;
    public float timeSinceGrabbing = 1.0f;
    public float timeSinceTwoHandGrabbing = 1.0f;
    public float timeSinceRelease = 1.0f;

    public int f1, f2;
    public float dist1, dist2;
    public float ratio1, ratio2;
    public float ToolRatio = 1.3f;
    public float releaseRatio = 1.2f;
    public float scaler = 1.0f;

    [Header("Collider settings")]
    public Vector3 palmSize = new Vector3(0.08f, 0.02f, 0.08f);
    public Vector3 palmOffset = new Vector3(0.06f, 0f, 0f);
    public float fingertipRadius = 0.008f;

    [Header("Haptic defaults")]
    [Range(0, 255)]
    public byte DefaultPull = 30;
    public byte DefaultVib = 0;

    public GameObject debugPlane;

    private AudioSource audioSource;
    private bool initiatedTwoHandGrab;

    private List<Vector3> palmLocations;
    private int throwHistory = 3;

    private float minTwoHandThrowSpeed = 0.3f;
    private float twoHandDropDelay = 0.1f;
    private float timeSinceDropSatisfied = 0.0f;

    //[HideInInspector]
    //public Texture2D handTexture;

    //private bool HandOpen { get { return insphere != null && insphere.radius < FlatnessThreshold(fingertipRadius); } }

    #region MonoBehaviour functions
    public void Start() {
        palmLocations = new List<Vector3>();

        /* Note:
         * ODD indices - finger tips
         * EVENS indices - middle knuckles
         */

        // Init pickup bools to false
        wasTwoHandGrabbing = twoHandGrabStarted = initiatedTwoHandGrab = false;

        // Get pickup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning("MaestroHand does not have an attached AudioSource! No sound will be played on pickup.");

        // Flip left hand's box offset so both hands can have the same offset
        if (whichHand == WhichHand.LeftHand)
            palmOffset = Vector3.Scale(palmOffset, new Vector3(-1, 1, 1));

        // Attach vibration scripts and store in array
        int k = 0;
        if (!ThumbTip.gameObject.GetComponent<ThumbVibrateOnCollideBehaviour>())
            vibs[k++] = ThumbTip.gameObject.AddComponent<ThumbVibrateOnCollideBehaviour>();

        if (!IndexTip.gameObject.GetComponent<IndexVibrateOnCollideBehaviour>())
            vibs[k++] = IndexTip.gameObject.AddComponent<IndexVibrateOnCollideBehaviour>();

        if (!MiddleTip.gameObject.GetComponent<MiddleVibrateOnCollideBehaviour>())
            vibs[k++] = MiddleTip.gameObject.AddComponent<MiddleVibrateOnCollideBehaviour>();

        if (!RingTip.gameObject.GetComponent<RingVibrateOnCollideBehaviour>())
            vibs[k++] = RingTip.gameObject.AddComponent<RingVibrateOnCollideBehaviour>();

        if (!LittleTip.gameObject.GetComponent<LittleVibrateOnCollideBehaviour>())
            vibs[k++] = LittleTip.gameObject.AddComponent<LittleVibrateOnCollideBehaviour>();

        // Attach force feedback scripts and store in array
        k = 0;
        if (!ThumbTip.gameObject.GetComponent<ThumbPullOnCollideBehaviour>())
            pulls[k++] = ThumbTip.gameObject.AddComponent<ThumbPullOnCollideBehaviour>();

        if (!IndexTip.gameObject.GetComponent<IndexPullOnCollideBehaviour>())
            pulls[k++] = IndexTip.gameObject.AddComponent<IndexPullOnCollideBehaviour>();

        if (!MiddleTip.gameObject.GetComponent<MiddlePullOnCollideBehaviour>())
            pulls[k++] = MiddleTip.gameObject.AddComponent<MiddlePullOnCollideBehaviour>();

        if (!RingTip.gameObject.GetComponent<RingPullOnCollideBehaviour>())
            pulls[k++] = RingTip.gameObject.AddComponent<RingPullOnCollideBehaviour>();

        if (!LittleTip.gameObject.GetComponent<LittlePullOnCollideBehaviour>())
            pulls[k++] = LittleTip.gameObject.AddComponent<LittlePullOnCollideBehaviour>();

        foreach (PullOnCollideBehaviour temp in pulls) {
            if (temp != null)
                temp.Idle = DefaultPull;
        }

        // Add appropriate layers
        int toSet = LayerMask.NameToLayer(ignoreLayer);
        int objectLayerInt = LayerMask.NameToLayer(objectLayer);

        if (toSet >= 0 && objectLayerInt >= 0) {
            foreach (Collider c in this.GetComponentsInChildren<Collider>(true)) {
                c.gameObject.layer = toSet;
            }
            Physics.IgnoreLayerCollision(toSet, objectLayerInt, true);
        } else {
            if (toSet < 0) {
                Debug.LogWarning(String.Format("Layer '{0}' does not exist! Please define it for proper functionality", ignoreLayer));
            }
            if (objectLayerInt < 0) {
                Debug.LogWarning(String.Format("Layer '{0}' does not exist! Please define it for proper functionality", objectLayer));
            }
        }

        // Init contact bools
        contacts = new bool[11];

        //Debug.Log(Physics.defaultContactOffset);
        //Physics.defaultContactOffset = 0.001f;
        //Physics.defaultContactOffset = 0.0000001f;

        // Generate all FingerColliders
        for (int i = 0; i < colliders.Length; i++) {
            // Create new FingerCollider and start setup
            GameObject g = new GameObject();
            g.AddComponent<Rigidbody>();
            phystips[i] = g.AddComponent<FingerCollider>();

            phystips[i].index = i;
            phystips[i].hpi = this;
            phystips[i].rb = phystips[i].GetComponent<Rigidbody>();
            contacts[i] = false;

            bool isPalm = i == colliders.Length - 1;

            //Disable collision between finger tips and debug spheres
            //Physics.IgnoreCollision(phystips[i].GetComponent<Collider>(), colliders[i].transform.GetComponent<Collider>());

            // Make the palm collider a box with special properties instead of a sphere
            if (isPalm) {
                phystips[i].makeRend(palmSize);
                phystips[i].col.transform.localPosition = palmOffset;
                //phystips[i].releaseCol.transform.localPosition = palmOffset;

                //phystips[i].vocbs = vibs;
                //phystips[i].pocbs = pulls;
                //phystips[i].setAudio(soundPickup);
            } else
                phystips[i].makeRend(fingertipRadius);

            // Apply physics material to FCs if applicable
            if (fingertipPhysicMaterial != null)
                phystips[i].SetPhysicMaterial(fingertipPhysicMaterial);

            // Set up rigidbody
            phystips[i].rb.mass = isPalm ? 5.0f : (i % 2 == 0 ? 5.0f : 10.0f); //( i == colliders.Length - 1 ? 1.0f : 10.0f); //0.1f;
            phystips[i].rb.useGravity = false;
            phystips[i].rb.freezeRotation = true;

            // Only define index finger as being able to paint
            //TODO do some better way
            phystips[i].isTip = i == 3; //i < 10 && i % 2 == 1;

            // Set up tap sound effects for finger tips
            if (i < 10 && i % 2 == 1) {
                phystips[i].AddAudioSource();
            }
        }

        // Set finger tip FCs haptic controllers
        for (int i = 0; i < vibs.Length; i++) {
            //phystips[i * 2 + 1].vocb = vibs[i];
            //phystips[i * 2 + 1].pocb = pulls[i];
        }

        // Setup temporary FCs
        for (int j = 0; j < extras.Length; j++) {
            int i = j + colliders.Length;
            //making a new fingertip collider gameObject and naming it
            GameObject g = new GameObject();
            g.AddComponent<Rigidbody>();
            phystips[i] = g.AddComponent<FingerCollider>();

            phystips[i].index = i;
            phystips[i].hpi = this;
            phystips[i].rb = phystips[i].GetComponent<Rigidbody>();

            phystips[i].makeRend(fingertipRadius);
            if (fingertipPhysicMaterial != null) {
                phystips[i].SetPhysicMaterial(fingertipPhysicMaterial);
            }
            phystips[i].rb.mass = 5.0f;
            phystips[i].rb.useGravity = false;
            phystips[i].rb.freezeRotation = true;
        }
    }

    private void FixedUpdate() {
        // Update contact bools
        regrabbed = false;

        for (int i = 0; i < contacts.Length; i++)
            contacts[i] = phystips[i].Contacting;

        bool palmTouch = contacts[colliders.Length - 1];

        // Update all haptics
        for (int i = 0; i < pulls.Length; i++) {
            if (i < 2 && manager != null && (manager.VibrationEffect != null/* || manager.Amplitude != null*/)) {
                vibs[i].VibrationEffect = manager.VibrationEffect;
                //TODO do this properly
                if (manager.VibrationEffect == 6) {
                    pulls[i].Amplitude = 100;
                }
            } else if (pulls[i].DribbleOverride) {
                pulls[i].Amplitude = 255;
                vibs[i].VibrationEffect = 5;

                pulls[i].DribbleWait += Time.fixedDeltaTime;
                //wait some time
                if (pulls[i].DribbleWait >= DribbleLengthener.MinDribbleWait) {
                    pulls[i].DribbleOverride = false;
                }
            } else {
                if (grabTarget != null && grabTarget.SendHapticsToWholeHand) {
                    pulls[i].Amplitude = grabTarget.getMotorAmplitude();
                    vibs[i].VibrationEffect = grabTarget.getVibrationEffect();
                } else {
                    MaestroInteractable mi = phystips[i * 2 + 1].touching;

                    if (mi != null) {
                        pulls[i].Amplitude = mi.getMotorAmplitude();
                    } else if (palmTouch && phystips[colliders.Length - 1].touching != null) {
                        byte? amp = phystips[colliders.Length - 1].touching.getMotorAmplitude();
                        if (amp.HasValue) {
                            pulls[i].Amplitude = (byte)(amp.Value * 0.65f);
                        }
                    } else {
                        mi = phystips[i * 2].touching; //test knuckle
                        if (!(palmTouch && phystips[colliders.Length - 1].touching != null)) {
                            if (mi != null)
                                pulls[i].Amplitude = mi.getMotorAmplitude();
                            else
                                pulls[i].Amplitude = null;
                        } else {
                            byte? amp = phystips[colliders.Length - 1].touching.getMotorAmplitude();
                            if (amp.HasValue) {
                                pulls[i].Amplitude = (byte)(amp.Value * 0.65f);
                            }
                        }
                        //pulls[i].Amplitude = null;

                        mi = phystips[i * 2 + 1].touching;
                    }

                    if (mi != null) {
                        vibs[i].VibrationEffect = mi.getVibrationEffect();
                    } else {
                        vibs[i].VibrationEffect = null;
                    }

                    //TODO REMOVE
                    /*if (i == 1 && whichHand == WhichHand.RightHand)
                    {
                        MaestroInteractable herk = phystips[i * 2 + 1].lastTouching;
                        MaestroBLEUI.SetRightText(String.Format("{0}\r\n{1}\r\n{2}",
                            mi != null ? mi.name : "NULL",
                            herk != null ? herk.name : "NULL",
                            vibs[i].VibrationEffect ?? 0));
                    }*/

                    /*else if (palmTouch && phystips[colliders.Length - 1].touching != null)
                    {
                        vibs[i].VibrationEffect = phystips[colliders.Length - 1].touching.getVibrationEffect();
                    }*/
                }
            }


        }

        // Update all time variables
        timeSinceGrabbing += Time.fixedDeltaTime;
        timeSinceRelease += Time.fixedDeltaTime;
        timeSinceTwoHandGrabbing += Time.fixedDeltaTime;

        // Move all temp FCs and check what they're touching
        // TODO merge with others
        for (int j = 0; j < extras.Length; j++) {
            int i = j + colliders.Length;

            // turn off finger colliders if I'm grabbing something or just let go of it
            phystips[i].col.enabled = !lastTargetWasTool || timeSinceRelease > 0.5f;

            //move the fingertip + palm colliders to the right place. Because I'm setting velocity, there's no guarantee they'll actually get there (ex. if there's something heavy in the way)
            Vector3 dist = (extras[j].transform.position - phystips[i].rb.position);
            Vector3 lastLocation = phystips[i].lastLocation;

            if (dist.magnitude > tooFast && (lastLocation - phystips[i].rb.position).magnitude < tooClose) {
                phystips[i].rb.transform.position = extras[j].transform.position;
            } else {
                phystips[i].rb.velocity = dist / Time.deltaTime;
            }

            phystips[i].rb.MoveRotation(extras[j].transform.rotation);

            phystips[i].rend.enabled = showColliders;

            //if the fingertip is touching something, and I'm not currently holding anything, check if there's more than one finger holding onto it.
            if (phystips[i].touching != null && !grabbing) {
                checkFingerGrabbing(i);
            }

            /*if (otherHand != null && phystips[i].touching != null && !twoHandGrabbing && phystips[i].touching.type == InteractionType.TwoHand)
            {
                checkTwoHandGrabbing(i);
            }*/
        }

        // Move all FCs and check what they're touching
        for (int i = 0; i < colliders.Length; i++) {
            phystips[i].col.enabled = !lastTargetWasTool || timeSinceRelease > 0.5f; // turn off finger colliders if I'm grabbing something or just let go of it

            //move the fingertip + palm colliders to the right place. Because I'm setting velocity, there's no guarantee they'll actually get there (ex. if there's something heavy in the way)

            Vector3 dist = (colliders[i].transform.position - phystips[i].rb.position);
            Vector3 lastLocation = phystips[i].lastLocation;

            if (dist.magnitude > tooFast && (lastLocation - phystips[i].rb.position).magnitude < tooClose) {
                phystips[i].rb.transform.position = colliders[i].transform.position;
            } else {
                phystips[i].rb.velocity = dist / Time.deltaTime;
            }

            //phystips[i].rb.velocity = (colliders[i].transform.position - phystips[i].rb.position) / Time.deltaTime;
            phystips[i].rb.MoveRotation(colliders[i].transform.rotation);

            phystips[i].rend.enabled = (phystips[i].isTip && !phystips[i].PaintColor.Equals(Color.clear)) || (showColliders && contacts[i]);

            //if the fingertip is touching something, and I'm not currently holding anything, check if there's more than one finger holding onto it.
            if (phystips[i].touching != null && !grabbing) {
                checkFingerGrabbing(i);
            }
        }

        recordPalmLocation(phystips[colliders.Length - 1].rb.transform.position);

        // If the user has two hands defined, check if two-hand grab has started
        if (otherHand != null && !twoHandGrabbing && !otherHand.twoHandGrabbing)
            checkTwoHandGrabbing();

        // Update started bools
        grabStarted = !wasgrabbing && grabbing;
        twoHandGrabStarted = !wasTwoHandGrabbing && twoHandGrabbing;

        // Play pickup noise if necessary
        if (grabStarted && audioSource != null) {
            audioSource.pitch = 2 + UnityEngine.Random.value;
            audioSource.Play();
        }

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
            Vector3 temp = Vector3.Lerp(grabPos.transform.position, centroid, Time.fixedDeltaTime * scaler);
            if (!(temp.Equals(Vector3.negativeInfinity) || grabTarget.maintainPosition))
                grabPos.transform.position = temp;
        }

        // Propagate to fingers
        Prop();

        // Collision debugger
        /*char yep = '+', nope = '_';
        string helpme = "";
        helpme += contacts[1] ? yep : nope;
        helpme += contacts[3] ? yep : nope;
        helpme += contacts[5] ? yep : nope;
        helpme += contacts[7] ? yep : nope;
        helpme += contacts[9] ? yep : nope;

        if (this.whichHand == WhichHand.LeftHand)
            MaestroBLEUI.SetLeftText(helpme);
        else
            MaestroBLEUI.SetRightText(helpme);*/
    }

    private void Prop() {
        foreach (PullOnCollideBehaviour p in pulls) {
            if (p != null)
                p.Prop();
        }

        foreach (VibrateOnCollideBehaviour v in vibs) {
            if (v != null)
                v.Prop();
        }
    }

    private void OnDisable() {
        // Disable all haptics
        foreach (PullOnCollideBehaviour p in pulls) {
            if (p != null) {
                p.Amplitude = 0;
                p.PaintAmp = 0;
            }
        }

        foreach (VibrateOnCollideBehaviour v in vibs) {
            if (v != null) {
                v.VibrationEffect = 0;
                v.PulseEffect = 0;
            }
        }

        Prop();

        //Debug.Log("ZERRRRRRR");

        // Disable all FCs when the hand itself is disabled
        foreach (FingerCollider fc in phystips) {
            if (fc)
                fc.enabled = false;
        }
    }

    private void OnEnable() {
        // Disable all haptics
        foreach (PullOnCollideBehaviour p in pulls) {
            if (p != null) {
                p.Amplitude = 0;
                p.PaintAmp = null;
            }
        }

        foreach (VibrateOnCollideBehaviour v in vibs) {
            if (v != null) {
                v.VibrationEffect = 0;
                v.PulseEffect = null;
            }
        }

        Prop();

        // Enable all FCs when the hand itself is enabled
        foreach (FingerCollider fc in phystips) {
            if (fc)
                fc.enabled = true;
        }
    }
    #endregion 

    #region Two handed grab functions
    private void OnTwoHandGrabbing() {
        if (CheckTwoHandGrabDone()) {
            /*float delay = Mathf.Max(getThrowVelocity().magnitude, otherHand.getThrowVelocity().magnitude) > minTwoHandThrowSpeed ? -1f : twoHandDropDelay;

            if (timeSinceDropSatisfied < 0)
                timeSinceDropSatisfied = 0.0f;
            else
                timeSinceDropSatisfied += Time.fixedDeltaTime;

            if (timeSinceDropSatisfied > delay)
            {*/
            TwoHandGrabEnd(true);
            //    timeSinceDropSatisfied = -0.5f;
            //}
        } else {
            timeSinceDropSatisfied = -0.5f;
            ApplyTwoHandFollowForce();
        }
    }

    private void TwoHandGrabEnd(bool drop) {
        Debug.Log("Ending 2 Hand");

        if (drop) {
            twoHandGrabTarget.rb.constraints = RigidbodyConstraints.None;
            twoHandGrabTarget.transform.parent = oldParent;
            oldParent = null;

            //release the thing

            twoHandGrabTarget.rb.useGravity = true;
            //grabTarget.rb.ResetCenterOfMass();
            twoHandGrabTarget.rb.isKinematic = false;
            twoHandGrabTarget.rb.freezeRotation = false;

            twoHandGrabTarget.Release();//tell it it got dropped
        }

        contacts = new bool[11];
        //lastTargetWasTool = grabTarget.isTool;
        twoHandGrabTarget = null;
        otherHand.twoHandGrabTarget = null;

        twoHandGrabbing = false;
        otherHand.twoHandGrabbing = false;
    }

    private void ApplyTwoHandFollowForce() {
        if (twoHandGrabbing && twoHandGrabTarget != null) {

            /*
             OLD 
            twoHandGrabTarget.rb.velocity += (((phystips[twoHandIndex].transform.position + otherHand.phystips[otherHand.twoHandIndex].transform.position) / 2f) - twoHandGrabTarget.getFollowPoint()) * (Time.fixedDeltaTime*10);

            twoHandGrabTarget.rb.angularVelocity = Vector3.zero;
            */

            Vector3 toInBetween = (((phystips[twoHandIndex].transform.position + otherHand.phystips[otherHand.twoHandIndex].transform.position) / 2f) - twoHandGrabTarget.getFollowPoint());
            // Square vector
            toInBetween.Scale(new Vector3(Mathf.Abs(toInBetween.x), Mathf.Abs(toInBetween.y), Mathf.Abs(toInBetween.z)));

            // Dampen velocity
            twoHandGrabTarget.rb.velocity *= 0.2f;

            // Accelerate toward center point
            twoHandGrabTarget.rb.velocity += toInBetween * Time.fixedDeltaTime * 100;

            twoHandGrabTarget.rb.angularVelocity = Vector3.zero;
        }
    }

    private void checkTwoHandGrabbing() {
        List<LineSegment> possibilities = new List<LineSegment>();

        for (int i = 0; i < contacts.Length; i++) {
            for (int j = 0; j < otherHand.contacts.Length; j++) {
                if (contacts[i] && otherHand.contacts[j]) {
                    if (phystips[i].touching != null && phystips[i].touching.type == InteractionType.TwoHand && phystips[i].touching == otherHand.phystips[j].touching /*phystips[i].touching.type != InteractionType.Static*/ ) {
                        possibilities.Add(new LineSegment(phystips[i].transform.position, otherHand.phystips[j].transform.position, phystips[i].touching, i, j));
                    }
                }
            }
        }

        if (possibilities.Count > 0) {
            possibilities.Sort(possibilities[0]);

            //LineSegment last = possibilities[possibilities.Count - 1];
            LineSegment last = possibilities[0];

            TwoHandGrabStart(last.interactable, last.index, last.otherIndex);
        }
    }

    private bool CheckTwoHandGrabDone() {
        bool hasOne = false, otherHasOne = false;

        for (int i = 1; i < phystips.Length; i += 2) {
            if (phystips[i].touching == twoHandGrabTarget) {
                hasOne = true;
                break;
            }
        }
        hasOne |= phystips[10].touching == twoHandGrabTarget;

        for (int i = 1; i < otherHand.phystips.Length; i += 2) {
            if (otherHand.phystips[i].touching == twoHandGrabTarget) {
                otherHasOne = true;
                break;
            }
        }
        otherHasOne |= otherHand.phystips[10].touching == twoHandGrabTarget;

        return timeSinceTwoHandGrabbing > 0.1f && !(hasOne && otherHasOne);
    }

    private void TwoHandGrabStart(MaestroInteractable interactable, int index, int otherIndex) {
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

        interactable.Grab(LayerMask.NameToLayer(objectLayer));
    }
    #endregion

    #region One handed grab functions
    //this grabs an object if both finger (i) and the opposite finger are both touching it. So, thumb + index finger can grab stuff, the middle/ring/pinky + palm can grab stuff
    private bool checkFingerGrabbing(int j) {
        int index = j / 2;
        //bool isPalm = index == 5;
        //if (isPalm) return false; //TODO?

        int other = shouldStartGrab(index);
        bool result = other >= 0
            && !isFlat
            && phystips[j].touching != null
            && phystips[j].touching.type != InteractionType.Static
            && phystips[j].touching.type != InteractionType.TwoHand
            && phystips[j].touching.Equals(phystips[other].touching)
            && timeSinceRelease > 0.5f;

        if (result) {
            if (grabbing)
                Regrab(phystips[j].touching, j, other);
            else
                GrabStart(phystips[j].touching, j, other);
        }
        return result;
    }

    private int shouldStartGrab(int index) {
        switch (index) {
            case 0: // Thumb
                int result = firstContactTipInRange(2, 9);
                if (result < 0 && contacts[10])
                    result = 10;

                return result;
            case 1:
            case 2:
            case 3:
            case 4: // Fingers
                int temp = firstContactInRange(0, 1);
                if (temp < 0 && contacts[10])
                    temp = 10;

                return temp;

            case 5: //palm
                int finger = firstContactTipInRange(2, 9);
                int thumb = firstContactInRange(0, 1);

                return finger >= 0 ? finger : thumb;
            default:
                return -1;
        }
    }

    private bool CheckGrabDone() {
        //ratio1 = (phystips[f1].transform.position - grabTarget.getFollowPoint()).magnitude / dist1; //grabPos before it moved
        //ratio2 = (phystips[f2].transform.position - grabTarget.getFollowPoint()).magnitude / dist2;

        float tempDist1 = ((phystips[f1].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(phystips[f1].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude);

        float tempDist2 = ((phystips[f2].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(phystips[f2].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude);

        ratio1 = tempDist1 / dist1; //grabPos
        ratio2 = tempDist2 / dist2; //grabPos

        bool someTips = false;
        for (int i = 0; i < 10; i++) // < 5
            someTips |= contacts[i]; // i * 2 + 1

        //return (timeFlat > flatDelay) || (timeSinceGrabbing > 0.1f && (Mathf.Min(ratio1, ratio2) > (grabTarget.isTool ? ToolRatio : releaseRatio) ||  (grabTarget.isTool ? false : !someTips)));
        return (grabTarget.isTool ? Mathf.Max(tempDist1, tempDist2) > 0.05f : Mathf.Min(ratio1, ratio2) > releaseRatio) || /*(grabTarget.isTool ? false :*/ !someTips;
    }

    private void GrabStart(MaestroInteractable r, int finger0, int finger1) {
        if (r == null)
            return;


        Debug.Log("Start grab: " + finger0 + ", " + finger1);

        grabTarget = r;
        timeSinceGrabbing = 0;

        grabbing = true;


        grabPos = generateAnchor(grabTarget.isTool ? colliders[10].transform.position : grabTarget.getFollowPoint());
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
        dist1 = (phystips[f1].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(phystips[f1].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos
        dist2 = (phystips[f2].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(phystips[f2].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos

        // Tell the Interactable it's been grabbed by this script
        r.Grab(LayerMask.NameToLayer(objectLayer));
    }

    private void Regrab(MaestroInteractable r, int finger0, int finger1) {
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

        dist1 = (phystips[f1].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
             Physics.ClosestPoint(phystips[f1].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos
        dist2 = (phystips[f2].transform.position - (grabTarget.gripCollider == null ? grabTarget.getFollowPoint() :
            Physics.ClosestPoint(phystips[f2].transform.position, grabTarget.gripCollider, grabTarget.gripCollider.transform.position, grabTarget.gripCollider.transform.rotation))).magnitude; //grabPos
    }

    private void OnGrabbing() {
        if (isFlat || (timeSinceGrabbing > 0.25f && (CheckGrabDone() /*|| (!grabTarget.isTool && SnowconeDetected())*/)))
            GrabEnd(true);
        else
            ApplyFollowForce();
    }

    /**
     * Makes the held object follow the hand
     */
    private void ApplyFollowForce() {
        if (grabbing && grabTarget != null) {
            if (grabTarget.stayInHand) {
                grabTarget.gameObject.transform.Translate(grabPos.transform.position - grabTarget.getFollowPoint(), Space.World);

                grabTarget.rb.angularVelocity = Vector3.zero;
            } else {
                if (!grabTarget.isTool) {
                    Vector3 dis = grabPos.transform.position - grabTarget.getFollowPoint();
                    //Vector3 forScaling = new Vector3(Mathf.Abs(dis.x), Mathf.Abs(dis.y), Mathf.Abs(dis.z));

                    //;

                    //dampen
                    Vector3 current = grabTarget.rb.velocity;
                    current *= 0.5f;
                    grabTarget.rb.velocity = current;

                    grabTarget.rb.velocity += dis * Mathf.Pow(dis.magnitude * 750, 2) * Time.fixedDeltaTime; //prev. 500
                    //grabTarget.rb.velocity += (grabPos.transform.position - grabTarget.getFollowPoint()) * (Time.fixedDeltaTime * 1000);

                    //grabTarget.rb.velocity = (grabPos.transform.position - grabTarget.getFollowPoint()) * (Time.fixedDeltaTime * 10000f);
                } else
                    grabTarget.rb.velocity = Vector3.zero;

                grabTarget.rb.angularVelocity = Vector3.zero;

            }
        }
    }

    private Vector3 getThrowVelocity() {
        if (palmLocations.Count <= 1)
            return phystips[10].rb.velocity;
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

    private void recordPalmLocation(Vector3 location) {
        palmLocations.Add(location);
        while (palmLocations.Count > throwHistory)
            palmLocations.RemoveAt(0);
    }

    private void GrabEnd(bool drop) {
        if (drop) {
            // Try regrab
            bool grabStarted = false;

            for (int i = 0; i < contacts.Length; i++) {
                if (phystips[i].touching != null && !grabStarted)
                    grabStarted |= checkFingerGrabbing(i);
            }

            if (grabStarted)
                contacts = new bool[11];

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
                //grabTarget.rb.freezeRotation = false;

                // Tell the MaestroInteractable it was released
                grabTarget.Release();

                // Reset grab position and contacts
                grabPos.transform.DetachChildren();
                Destroy(grabPos);
                contacts = new bool[11];
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
            contacts = new bool[11];
        }
    }
    #endregion

    #region Helper functions
    public int IndexOf(Collider collider) {
        return System.Array.IndexOf(colliders, collider);
    }

    private int firstContactInRange(int start, int end) {
        int index = -1;
        for (int i = start; i <= end; i++) {
            if (contacts[i]) {
                index = i;
                break;
            }
        }

        return index;
    }

    private int firstContactTipInRange(int start, int end) {
        int index = -1;
        if (start % 2 == 0)
            start++;

        for (int i = start; i <= end; i += 2) {
            if (contacts[i]) {
                index = i;
                break;
            }
        }

        return index;
    }

    private bool SnowconeDetected() {
        return !(contacts[1] && contacts[3]) && firstContactInRange(0, 3) >= 0 && firstContactInRange(4, 9) < 0;
    }

    private float FlatnessThreshold(float radius) {
        return radius / 2;
    }

    public Vector3 GetCentroid(bool[] whichColliders) {
        Vector3 result = Vector3.zero;
        int count = 0;

        /*int chosenOne = 4;
        bool temp = whichColliders[chosenOne];
        whichColliders[chosenOne] = true;*/

        for (int i = 2 /*ignore thumb*/; i < whichColliders.Length - 1 /*Palm pulls the object down too much*/; i++) {
            if (whichColliders[i]) {
                result += colliders[i].transform.position;
                count++;
            }
        }

        if (count > 1)
            return result / count;
        else
            return Vector3.negativeInfinity;
    }

    public GameObject generateAnchor(Vector3 position) {
        GameObject anchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);


        anchor.transform.SetPositionAndRotation(position, Quaternion.identity);

        anchor.transform.localScale = Vector3.one * 0.01f;
        FixedJoint fj = anchor.AddComponent<FixedJoint>();
        fj.connectedBody = phystips[10].rb;
        fj.massScale = 100;
        fj.connectedMassScale = 100;

        Rigidbody rb = anchor.GetComponent<Rigidbody>();
        //rb.isKinematic = true;
        rb.useGravity = false;
        rb.drag = 0;
        rb.mass = 10;
        //anchor.transform.parent = colliders[10].transform; //Attach to palm

        Destroy(anchor.GetComponent<Collider>());
        //Destroy(anchor.GetComponent<Rigidbody>());
        Destroy(anchor.GetComponent<Renderer>());

        return anchor;
    }

    private Vector3 OffsetToAngular(Quaternion a, Quaternion b, float timestep) {
        // Turn the difference between two quaternions to an angular velocity
        Quaternion deltaRot = b * Quaternion.Inverse(a);
        Vector3 vel = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
        vel /= timestep;
        vel *= Mathf.Deg2Rad;
        return vel;
    }
    #endregion

    #region Paint functions (temporary)
    public void setAll(byte? b, byte? c) {
        foreach (PullOnCollideBehaviour pocb in pulls) {
            try {
                pocb.PaintAmp = b;
            } catch (Exception) { }
        }

        foreach (VibrateOnCollideBehaviour vocb in vibs) {
            try {

                vocb.PaintEffect = c;
            } catch (Exception) { }
        }
    }

    public void ClearPaint() {
        if (phystips != null) {
            foreach (FingerCollider fc in phystips) {
                if (fc != null && fc.isTip && !fc.PaintColor.Equals(Color.clear)) {
                    fc.PaintColor = Color.clear;
                }
            }
        }
    }
    #endregion

    #region Insphere generation (currently unused)
    private UnityEngine.Plane? PlaneFromPoints(List<Vector3> points) {
        UnityEngine.Plane? result = null;

        if (points.Count > 1) {
            Vector3 centroid = Vector3.zero;
            foreach (Vector3 v in points)
                centroid += v;

            centroid *= 1f / points.Count;

            if (points.Count == 3)
                result = new UnityEngine.Plane(points[0], points[1], points[2]);
            else if (points.Count > 3) {
                Vector3 direction;

                float[,] matrix = new float[3, 3];
                foreach (Vector3 v in points) {
                    matrix[0, 0] += v.x * v.x;
                    matrix[0, 1] += v.x * v.y;
                    matrix[0, 2] += v.x * v.z;
                    matrix[1, 1] += v.y * v.y;
                    matrix[1, 2] += v.y * v.z;
                    matrix[2, 2] += v.z * v.z;
                }

                Vector3 determinate = new Vector3(
                    (matrix[1, 1] * matrix[2, 2]) - (matrix[1, 2] * matrix[1, 2]),
                    (matrix[0, 0] * matrix[2, 2]) - (matrix[0, 2] * matrix[0, 2]),
                    (matrix[0, 0] * matrix[1, 1]) - (matrix[0, 1] * matrix[0, 1])
                );
                float max = Mathf.Max(determinate.x, determinate.y, determinate.z);
                if (max > 0) {
                    if (max == determinate.x) {
                        direction = new Vector3(
                            max,
                            (matrix[0, 2] * matrix[1, 2]) - (matrix[0, 1] * matrix[2, 2]),
                            (matrix[0, 1] * matrix[1, 2]) - (matrix[0, 2] * matrix[1, 1])
                        );
                    } else if (max == determinate.y) {
                        direction = new Vector3(
                            (matrix[0, 2] * matrix[1, 2]) - (matrix[0, 1] * matrix[2, 2]),
                            max,
                            (matrix[0, 1] * matrix[0, 2]) - (matrix[1, 2] * matrix[0, 0])
                        );
                    } else {
                        direction = new Vector3(
                            (matrix[0, 1] * matrix[1, 2]) - (matrix[0, 2] * matrix[1, 1]),
                            (matrix[0, 1] * matrix[0, 2]) - (matrix[1, 2] * matrix[0, 0]),
                            max
                        );
                    }

                    result = new UnityEngine.Plane(direction.normalized, centroid);
                }
            }
        }

        return result;
    }

    private List<Assets.Plane> GetPlanes() {
        List<Assets.Plane> result = new List<Assets.Plane>();

        for (int i = 0; i < 4; i++)
            result.AddRange(GetPlanesForFinger(i));

        //Extra plane for extra thumb digit
        //result.Add(Insphere.from3(extras[0].transform.position, colliders[0].transform.position, colliders[colliders.Length - 1].transform.position));

        return result;
    }

    /**
     * Generates 4 planes between two fingers, 0 being thumb and 4 being pinky.
     * 
     * Should never be called with index > 3, as it automatically gets all planes between palm, index, and (index + 1)
     */
    private List<Assets.Plane> GetPlanesForFinger(int index) {
        List<Assets.Plane> result = new List<Assets.Plane>();

        int middle = (index * 2);
        int tip = middle + 1;
        int nextMiddle = ((index + 1) * 2);
        int nextTip = nextMiddle + 1;

        int palm = colliders.Length - 1;

        result.Add(From3Indices(middle, tip, nextTip));
        result.Add(From3Indices(nextMiddle, middle, nextTip));
        result.Add(From3Indices(palm, middle, nextMiddle));
        result.Add(From3Indices(palm, tip, nextTip));

        return result;
    }

    private Assets.Plane From3Indices(int a, int b, int c) {
        return Insphere.from3(colliders[c].transform.position, colliders[b].transform.position, colliders[a].transform.position);
    }

    private struct LineSegment : IComparer<LineSegment>
    {
        public Vector3 start, end;
        public float length;
        public MaestroInteractable interactable;
        public int index, otherIndex;

        public LineSegment(Vector3 start, Vector3 end, MaestroInteractable interactable, int index, int otherIndex) {
            this.length = (start - end).magnitude;
            this.start = start;
            this.end = end;
            this.interactable = interactable;
            this.index = index;
            this.otherIndex = otherIndex;
        }

        public int Compare(LineSegment x, LineSegment y) {
            return x.length.CompareTo(y.length);
        }
    }
    #endregion
}
