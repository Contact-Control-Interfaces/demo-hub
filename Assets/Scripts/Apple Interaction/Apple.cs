using Leap.Unity;
using Leap.Unity.PhysicalHands;
using Maestro;
using Maestro.Vibration;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Apple : MonoBehaviour
{
    //Assigned In Inspector
    [Header("Bloom Variables")]
    public BloomManager bloomManager;
    [Space(10)]

    [Header("Shaders")]
    public Material glowOff;
    public Material glowOn;
    public Material hologramGlow;
    [Space(10)]

    [Header("Effected Objects")]
    public GameObject panelDisplay;
    public GameObject wristDisplay;
    public GameObject tree;
    public GameObject touchTrigger;
    public AudioClipSettings biteSoundSettings;
    public AudioClipSettings pluckSoundSettings;

    [Header("Lock Points")]
    public Transform resetPoint;
    public float resetHeight = 2.2f;

    public Transform mouthTransform;
    public SphereCollider failsafeBubble;

    [Header("Ultraleap Hands")]
    public HandModelBase leftHand;
    public HandModelBase rightHand;

    [SerializeField] private TextType textType;

    //Variables
    private AudioSource audioSource;
    private AppleHapticsController appleHaptics;
    private Rigidbody rb;
    private AppleDrop appleDrop;
    public Renderer renderer { get; private set; }
    private Quaternion originalRotation;
    private float originalMaxLinearVelocity;

    private bool bitten = false;
    private Transform appleOwner;
    public bool isGrabbed;

    public bool StillAttachedToTree => rb != null && rb.constraints == RigidbodyConstraints.FreezeAll;

    private void Start()
    {
        panelDisplay.SetActive(false);

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        appleHaptics = GetComponent<AppleHapticsController>();
        renderer = GetComponentInChildren<Renderer>();
        appleDrop = FindObjectOfType<AppleDrop>(true);

        originalRotation = transform.rotation;
        originalMaxLinearVelocity = rb.maxLinearVelocity;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Pluck();
        }

        if (transform.position.y <= resetHeight && !bitten)
        {
            ResetApple();
        }
    }

    public void OnFullBloom()
    {
        wristDisplay.SetActive(true);
        panelDisplay.SetActive(true);
        tree.SetActive(false);
    }

    public void OnBloomEnd()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!appleHaptics.isDropping)
        {
            StartCoroutine(appleHaptics.DropHaptics());
            StartCoroutine(ReduceVelocityOnContact());
            //Check if we're colliding with the palm and set our owner to the palm if so
            if (other.TryGetComponent(out HardContactBone contactBone)
                && other.TryGetComponent(out BoxCollider palmCollider)
                && !isGrabbed)
            {
                appleOwner = other.transform;
            }
        }

        if (other.gameObject.tag == "MainCamera" && !appleHaptics.isBiting && !StillAttachedToTree)
        {
            Bite();
            touchTrigger.SetActive(false);
        }
    }

    public void OnPhysicalHandContact(ContactHand hand, Rigidbody rb)
    {
        if (rb != this.rb) return; //Exit if this is the wrong Rigidbody
        if (!isGrabbed && appleOwner == null)
        {
            appleOwner = hand.transform;
            Debug.Log("Hitting hand, with no owner assigned, assigning owner to: " + hand.transform.gameObject.name);
            if (hand.Handedness == Chirality.Left)
            {
                leftHand.OnFinish += FreezeApple;
                leftHand.OnBegin += UnfreezeApple;
            }
            else
            {
                rightHand.OnFinish += FreezeApple;
                rightHand.OnBegin += UnfreezeApple;
            }
        }
    }

    public void OnPhysicalHandContactExit(ContactHand hand, Rigidbody rb)
    {
        if (rb != this.rb) return; //Exit if this is the wrong Rigidbody
        if (appleOwner == hand.transform)
        {
            Debug.Log("No longer hitting hand, removing owner");
            appleOwner = null;
            if (hand.Handedness == Chirality.Left)
            {
                leftHand.OnFinish -= FreezeApple;
                leftHand.OnBegin -= UnfreezeApple;
            }
            else
            {
                rightHand.OnFinish -= FreezeApple;
                rightHand.OnBegin -= UnfreezeApple;
            }
        }
    }
    private IEnumerator ReduceVelocityOnContact()
    {
        rb.velocity = Vector3.zero;
        rb.maxLinearVelocity = 0.05f;
        yield return new WaitForSeconds(0.2f);
        rb.velocity = Vector3.zero;
        rb.maxLinearVelocity = originalMaxLinearVelocity;
    }

    public void FreezeApple()
    {
        Debug.Log("Freezing apple");
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
    public void UnfreezeApple()
    {
        Debug.Log("Unfreezing apple");
        rb.constraints = RigidbodyConstraints.None;
    }
    public void ResetLinearVelocity()
    {
        panelDisplay.SetActive(true);
        wristDisplay.SetActive(true);
    }

    void TreeDisable()
    {
        tree.SetActive(false);
        rb.maxLinearVelocity = originalMaxLinearVelocity;
    }

    public void Pluck()
    {
        if (StillAttachedToTree)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;

            renderer.material = glowOn;

            var interactable = GetComponent<MaestroInteractable>();
            interactable.type = InteractionType.OneHandGrab;

            audioSource.PlayAudioClipWithCustomSettings(pluckSoundSettings);

            textType.TextGen("Take a bite.", true);
            touchTrigger.SetActive(false);
        }
    }

    public void ResetApple()
    {
        transform.SetParent(null);
        GetComponent<ReturnToSpawn>().Poof();

        renderer.material = hologramGlow;
        rb.drag = 0f;
        rb.maxLinearVelocity = originalMaxLinearVelocity;
        touchTrigger.SetActive(true);

        var inter = GetComponent<MaestroInteractable>();
        inter.type = InteractionType.Static;

        transform.position = resetPoint.position;
        transform.rotation = originalRotation;

        // Gross but easiest way to trigger another drop if the hand hasn't left the box
        appleDrop.ResetAppleDrop();
        appleDrop.TryStartTime();
    }

    public void Bite()
    {
        touchTrigger.SetActive(false);

        // Don't allow user to drop the apple at this point
        failsafeBubble.radius = 10f;

        audioSource.PlayAudioClipWithCustomSettings(biteSoundSettings);
        StartCoroutine(appleHaptics.BiteHaptics());

        bitten = true;

        // Lock apple in place relative to camera
        transform.parent = Camera.main.transform;
        rb.isKinematic = true;

        // Lerp the apple toward the mouth during bite
        StartCoroutine(LerpAppleToMouthPosition());

        bloomManager.StartBloom();
    }

    IEnumerator LerpAppleToMouthPosition()
    {
        float totalWait = appleHaptics.BiteDuration;
        float elapsed = 0f;

        Vector3 startPosition = transform.position;

        while (elapsed < totalWait)
        {
            elapsed += Time.fixedDeltaTime;

            transform.position = Vector3.Lerp(startPosition, mouthTransform.position, elapsed / totalWait);

            yield return new WaitForFixedUpdate();
        }
        renderer.enabled = false;
    }
}