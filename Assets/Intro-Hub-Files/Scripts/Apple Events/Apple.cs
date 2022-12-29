using Leap.Unity;
using Leap.Unity.HandsModule;
using Maestro;
using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;
using Bloom = UnityEngine.Rendering.Universal.Bloom;

public class Apple : MonoBehaviour
{
    [Header("Bloom Variables")]
    public VolumeProfile universalBloom;
    public Bloom bloomEffect;
    [Range(1f, 1000f)]
    public float bloomIntensity;
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
    public GameObject dropObject;
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

    private bool BloomUp;
    private bool BloomDown;
    private bool materialChange;
    private bool EffectDone;
    private TextType textType;

    private Rigidbody rb;
    private Quaternion originalRotation;
    private Vector3 originalLocalScale;

    private Coroutine biteCouroutine;
    private Coroutine dropCoroutine;

    private bool bitten = false;

    public bool StillAttachedToTree => rb != null && rb.constraints == RigidbodyConstraints.FreezeAll;



    private void Start()
    {
        panelDisplay.SetActive(false);
        textType = FindObjectOfType<TextType>();

        rb = dropObject.GetComponent<Rigidbody>();

        originalRotation = dropObject.transform.rotation;
        originalLocalScale = dropObject.transform.localScale;

        var sources = dropObject.GetComponents<AudioSource>().AsEnumerable().GetEnumerator();
        if (biteSound == null && sources.MoveNext()) {
            biteSound = sources.Current;
        }
        if (pluckSound == null && sources.MoveNext()) {
            pluckSound = sources.Current;
        }
    }

    void Update()
    {
        if (dropObject.transform.position.y <= resetHeight && !bitten)
        {
            ResetApple();
        }

        if (!EffectDone)
        {
            TestBloom();
            if (BloomUp)
            {
                if (bloomEffect.intensity.value <= bloomIntensity)
                {
                    bloomEffect.intensity.value += 5f;
                }
                else
                {
                    TreeDisable();
                    DisplayPanel();

                    BloomDown = true;
                    BloomUp = false;
                }
            }
            else if (BloomDown)
            {
                if (bloomEffect.intensity.value > 0f)
                {
                    bloomEffect.intensity.value -= 10f;
                }
                else
                {
                    Debug.Log("BloomDone");
                    if (!materialChange)
                    {
                        BloomDown = false;
                        bloomEffect.intensity.value = 100f;
                        MaterialChange();
                        EffectDone = true;

                        Destroy(gameObject);
                    }
                }
            }
        }
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
        dropCoroutine = StartCoroutine(DropHaptics());

        if (other.gameObject.tag == "MainCamera" && biteCouroutine == null)
        {
            Bite();
            touchTrigger.SetActive(false);
        }
    }

    private void TestBloom()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (universalBloom.TryGet<Bloom>(out bloomEffect))
            {
                bloomEffect.intensity.value = 0f;
                Debug.Log("Bloom");
                BloomUp = true;
                biteSound.Play();
            }
        }
    }

    void MaterialChange()
    {
        materialChange = true;
        bloomEffect.intensity.value = 5f;
        this.GetComponent<Renderer>().material = glowOff;
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
            rb.constraints = RigidbodyConstraints.None;

            dropObject.GetComponent<Renderer>().material = glowOn;

            if (pluckSound != null)
                pluckSound.Play();

            textType.TextGen("Take a bite.", true);
        }
    }

    private void ResetApple()
    {
        this.transform.SetParent(null);

        dropObject.GetComponent<Renderer>().material = hologramGlow;

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        dropObject.transform.position = resetPoint.position;
        dropObject.transform.rotation = originalRotation;
        dropObject.transform.localScale = originalLocalScale;

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
        dropObject.transform.parent = Camera.main.transform;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Lerp the apple toward the mouth during bite
        StartCoroutine(LerpRoutine());

        if (universalBloom.TryGet<Bloom>(out bloomEffect))
        {
            bloomEffect.intensity.value = 0f;
            BloomUp = true;
        }
    }

    IEnumerator LerpRoutine()
    {
        float totalWait = BiteDuration;
        float elapsed = 0f;

        Vector3 startPosition = dropObject.transform.position;

        while (elapsed < totalWait) {
            elapsed += Time.fixedDeltaTime;

            dropObject.transform.position = Vector3.Lerp(startPosition, mouthTransform.position, elapsed / totalWait);
            
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator BiteHaptics()
    {
        MaestroInteractable interactable = dropObject.GetComponent<MaestroInteractable>();

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
        var rend = dropObject.GetComponent<Renderer>();
        rend.enabled = false;
    }

    IEnumerator DropHaptics()
    {
        var interactable = dropObject.GetComponent<MaestroInteractable>();
        interactable.SendHapticsToWholeHand = true;
        interactable.SetHapticOverride(DropEffect);
        yield return new WaitForSeconds(DropEffectDuration / 1000f);
        interactable.ResetOverride();
        interactable.SendHapticsToWholeHand = false;
    }
}
