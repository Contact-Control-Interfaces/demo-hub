using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public abstract class KeyboardGenerator : MonoBehaviour
{
    public bool generate;
    private bool last_generate;

    public Scale[] notes;
    public Transform target;

    [Space]
    public float DbRatio = 0.35f;
    public float EbRatio = 0.65f;
    public float GbRatio = 0.25f;
    public float AbRatio = 0.5f;
    public float BbRatio = 0.75f;

    [Space]
    public GameObject KeyPrefab;
    public GameObject OtherKeyPrefab;

    public float KeyGap = 0.001f;
    public float PressDepth = 0.0102f;
    public Vector3 KeyDimensions = new Vector3(0.0225f, 0.0204f, 0.1524f);
    public Vector3 OtherKeyDimensions = new Vector3(0.0127f, 0.0127f, 0.09f);

    public List<GameObject> naturals;
    public List<GameObject> others;

    public virtual float OctaveSpan { get { return 7 * (KeyDimensions.x + KeyGap); } }

    public float FullSpan { get; protected set; }
    protected float CurrentOctaveSpan;

    public virtual bool Generate()
    {
        FullSpan = 0f;

        Debug.Log("Generating...");
        if (notes.Length <= 0) {
            Debug.LogError("No notes!");
        }

        naturals = new List<GameObject>();
        others = new List<GameObject>();

        for (int i = 0; i < notes.Length; i++) {
            GenerateOctave(i, notes[i]);
        }

        return true;
    }

    public virtual GameObject GenerateOctave(int num, Scale scale)
    {
        CurrentOctaveSpan = 0f;

        GameObject result = new GameObject($"Octave {num}");

        result.transform.parent = target;

        naturals.AddRange(GenerateNaturalKeys(num, result.transform, scale.white));
        others.AddRange(GenerateOtherKeys(num, result.transform, scale.black));

        // Center keyboard horizontally
        result.transform.localPosition = new Vector3(FullSpan, 0, 0);

        FullSpan += CurrentOctaveSpan;

        return result;
    }

    protected virtual GameObject[] GenerateOtherKeys(int octave, Transform parent, BlackScale scale)
    {
        List<GameObject> result = new List<GameObject>();

        IEnumerator<AudioClip> enumerator = (IEnumerator<AudioClip>)scale.GetEnumerator();
        enumerator.MoveNext();

        float distanceToSurface = (KeyDimensions.y - OtherKeyDimensions.y) / 2f;
        float distanceToTop = (KeyDimensions.z - OtherKeyDimensions.z) / 2f;

        float[] ratios = { DbRatio, EbRatio, 0, GbRatio, AbRatio, BbRatio };
        for (int i = 0; i < 6; i++) {
            if (i == 2) continue;

            result.Add(GenerateOtherKey(octave, i, i + 1, ratios[i], parent, enumerator.Current, OtherKeyDimensions, PressDepth + distanceToSurface, distanceToTop));

            enumerator.MoveNext();
        }

        return result.ToArray();
    }

    protected virtual GameObject GenerateOtherKey(int octave, int index1, int index2, float ratio, Transform parent, AudioClip sound, Vector3 scale, float raise, float pushback)
    {
        GameObject key = Instantiate(OtherKeyPrefab == null ? KeyPrefab : OtherKeyPrefab);

        var source = key.GetComponent<AudioSource>();
        source.clip = sound;

        float delta = OtherKeyDimensions.x / 2;
        float position1 = parent.GetChild(index1).transform.localPosition.x;
        float position2 = parent.GetChild(index2).transform.localPosition.x;
        float midpoint = (position1 + position2) / 2f;

        key.transform.parent = parent;
        key.transform.localPosition = new Vector3(Mathf.Lerp(midpoint - delta, midpoint + delta, ratio), raise, pushback);
        key.transform.localScale = scale;

        key.gameObject.layer = LayerMask.NameToLayer("black");

        return key;
    }

    protected virtual GameObject[] GenerateNaturalKeys(int octave, Transform parent, WhiteScale scale)
    {
        List<GameObject> result = new List<GameObject>();

        int i = 0;
        foreach (AudioClip clip in scale) {
            result.Add(GenerateNaturalKey(octave, i, parent, clip, KeyDimensions));

            i++;
        }

        return result.ToArray();
    }

    protected virtual GameObject GenerateNaturalKey(int octave, int index, Transform parent, AudioClip sound, Vector3 scale)
    {
        GameObject key = Instantiate(KeyPrefab);
        key.transform.parent = parent;

        AudioSource source = key.GetComponent<AudioSource>();
        source.clip = sound;

        key.transform.localScale = scale;
        key.transform.localPosition = new Vector3(CurrentOctaveSpan, 0, 0);

        key.gameObject.layer = LayerMask.NameToLayer("white");

        CurrentOctaveSpan += scale.x + KeyGap;

        return key;
    }

    void Update()
    {
        if (generate && !last_generate) {
            this.Generate();
        }

        last_generate = generate;
    }
}
