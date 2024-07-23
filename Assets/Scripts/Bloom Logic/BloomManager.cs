using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomManager : MonoBehaviour
{
    public VolumeProfile universalBloom;
    public Bloom bloomEffect;

    [Range(1f, 1000f)]
    public float bloomIntensity;

    private bool BloomUp;
    private bool BloomDown;
    private bool materialChange;
    private bool EffectDone;

    private float initialIntensity;

    public UnityEvent OnFullBloom;
    public UnityEvent OnBloomFinished;

    void Start()
    {
        if (universalBloom.TryGet<Bloom>(out bloomEffect)) {
            initialIntensity = bloomEffect.intensity.value;
        } else {
            Debug.LogError("Couldn't get Bloom!");
        }
    }

    void Update()
    {
        if (!EffectDone) {
            TestBloom();
            if (BloomUp) {
                if (bloomEffect.intensity.value < bloomIntensity) {
                    bloomEffect.intensity.value += 5f;
                } else {
                    OnFullBloom.Invoke();

                    BloomDown = true;
                    BloomUp = false;
                }
            } else if (BloomDown) {
                if (bloomEffect.intensity.value > 0f) {
                    bloomEffect.intensity.value -= 10f;
                } else {
                    Debug.Log("BloomDone");
                    if (!materialChange) {
                        BloomDown = false;
                        bloomEffect.intensity.value = initialIntensity;
                        EffectDone = true;

                        OnBloomFinished.Invoke();
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        bloomEffect.intensity.value = initialIntensity;
    }

    public void StartBloom()
    {
        bloomEffect.intensity.value = 0f;
        BloomUp = true;
    }

    private void TestBloom()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            this.StartBloom();
        }
    }
}
