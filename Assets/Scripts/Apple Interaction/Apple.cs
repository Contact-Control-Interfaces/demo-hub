using Leap.Unity.Interaction.PhysicsHands;
using Maestro;
using Maestro.Vibration;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Apple : MonoBehaviour
{
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
    public GameObject tree;
    public GameObject touchTrigger;
    public AudioSource biteSound;
    public AudioSource pluckSound;

    [Header("Lock Points")]
    public Transform resetPoint;
    public float resetHeight = 2.2f;

    public Transform mouthTransform;
    public SphereCollider failsafeBubble;

    [Header("Bite timing")]
    public float firstBite = 0.15f;
    public float secondBite = 0.8f;
    public float eachBiteDuration = 0.1f;

    private float BiteDuration => secondBite + eachBiteDuration;
    
    public HapticEffect DropEffect = new HapticEffect(){Amplitude = 255, Vibration = new SoftBump(WideThreeOptions._100){OneShot = true}};
    public float DropEffectDuration = 500; //ms

    [SerializeField]private TextType textType;

    private Rigidbody rb;
    private Quaternion originalRotation;

    private Coroutine biteCouroutine;
    private Coroutine dropCoroutine;

    private bool bitten = false;

    public bool StillAttachedToTree => rb != null && rb.constraints == RigidbodyConstraints.FreezeAll;

    private void Start()
    {
        panelDisplay.SetActive(false);

        rb = GetComponent<Rigidbody>();

        originalRotation = transform.rotation;

        var sources = GetComponents<AudioSource>().AsEnumerable().GetEnumerator();
        if (biteSound == null && sources.MoveNext()) {
            biteSound = sources.Current;
        }
        if (pluckSound == null && sources.MoveNext()) {
            pluckSound = sources.Current;
        }
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
        TreeDisable();
        DisplayPanel();
    }

    public void OnBloomEnd()
    {
        MaterialChange();

        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (biteCouroutine != null) {
            StopCoroutine(biteCouroutine);
            biteCouroutine = null;
        }

        if (dropCoroutine != null)
        {
            StopCoroutine(dropCoroutine);
            dropCoroutine = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dropCoroutine == null) {
            dropCoroutine = StartCoroutine(DropHaptics());
        }

        if (other.gameObject.tag == "MainCamera" && biteCouroutine == null && !StillAttachedToTree)
        {
            Bite();
            touchTrigger.SetActive(false);
        }
    }

    void MaterialChange()
    {
        GetComponent<Renderer>().material = glowOff;
    }

    void DisplayPanel()
    {
        panelDisplay.SetActive(true);
    }

    void TreeDisable()
    {
        tree.SetActive(false);
    }

    public void Pluck()
    {
        if (StillAttachedToTree) {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;

            GetComponent<Renderer>().material = glowOn;

            var interactable = GetComponent<MaestroInteractable>();
            interactable.type = InteractionType.OneHandGrab;

            if (pluckSound != null)
                pluckSound.Play();

            textType.TextGen("Take a bite.", true);
        }
    }

    public void ResetApple()
    {
        transform.SetParent(null);
        GetComponent<ReturnToSpawn>().Poof();

        GetComponent<Renderer>().material = hologramGlow;
        rb.isKinematic = true;
        rb.drag = 0f;

        var inter = GetComponent<MaestroInteractable>();
        inter.type = InteractionType.Static;

        transform.position = resetPoint.position;
        transform.rotation = originalRotation;
        this.GetComponent<MeshRenderer>().enabled = false;

        // Gross but easiest way to trigger another drop if the hand hasn't left the box
        AppleDrop drop = FindObjectOfType<AppleDrop>();
        drop.TryStartTime();
    }

    public void Bite()
    {
        touchTrigger.SetActive(false);

        // Don't allow user to drop the apple at this point
        failsafeBubble.radius = 10f;

        biteSound.Play();
        biteCouroutine = StartCoroutine(BiteHaptics());

        bitten = true;

        // Lock apple in place relative to camera
        transform.parent = Camera.main.transform;
        rb.isKinematic = true;

        // Lerp the apple toward the mouth during bite
        StartCoroutine(LerpRoutine());

        bloomManager.StartBloom();
    }

    IEnumerator LerpRoutine()
    {
        float totalWait = BiteDuration;
        float elapsed = 0f;

        Vector3 startPosition = transform.position;

        while (elapsed < totalWait) {
            elapsed += Time.fixedDeltaTime;

            transform.position = Vector3.Lerp(startPosition, mouthTransform.position, elapsed / totalWait);
            
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator BiteHaptics()
    {
        MaestroInteractable interactable = GetComponent<MaestroInteractable>();

        interactable.SendHapticsToWholeHand = true;

        yield return new WaitForSeconds(firstBite);
        interactable.stayHaptics.Vibration = new SharpTick(NarrowThreeOptions._100);
        interactable.stayHaptics.Vibration.OneShot = true;
        yield return new WaitForSeconds(eachBiteDuration);
        interactable.stayHaptics.Vibration = VibrationEffect.None;

        yield return new WaitForSeconds(secondBite - (firstBite + eachBiteDuration));
        interactable.stayHaptics.Vibration = new DoubleSharpTick(TickDuration.Short, NarrowThreeOptions._100);
        interactable.stayHaptics.Vibration.OneShot = true;
        yield return new WaitForSeconds(eachBiteDuration);
        interactable.stayHaptics.Vibration = VibrationEffect.None;

        interactable.SendHapticsToWholeHand = false;

        // Stop showing apple after bite
        var rend = GetComponent<Renderer>();
        rend.enabled = false;
    }

    IEnumerator DropHaptics()
    {
        var interactable = GetComponent<MaestroInteractable>();
        interactable.SendHapticsToWholeHand = true;
        interactable.SetHapticOverride(DropEffect);
        yield return new WaitForSeconds(DropEffectDuration / 1000f);

        interactable.ResetOverride();
        interactable.SendHapticsToWholeHand = false;
        dropCoroutine = null;
    }
}
