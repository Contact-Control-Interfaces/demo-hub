using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarimbaGenerator : KeyboardGenerator
{
    public Vector3 MaxKeyDimensions = new Vector3(0.094488f, 0.03937f, 0.586581f);
    public float KeyOverlap = 0.1f;

    protected override GameObject GenerateNaturalKey(int octave, int index, Transform parent, AudioClip sound, Vector3 scale)
    {
        GameObject result = base.GenerateNaturalKey(octave, index, parent, sound, scale);

        CurrentOctaveSpan -= result.transform.localScale.x;

        int totalIndex = (7 * octave) + index;
        int maxIndex = (7 * notes.Length) - 1;

        float lerp = (float)totalIndex / maxIndex;

        result.transform.localScale = Vector3.Lerp(MaxKeyDimensions, KeyDimensions, lerp);

        Vector3 newPosition = result.transform.localPosition;
        newPosition.z = (MaxKeyDimensions.z - result.transform.localScale.z) / 2;

        result.transform.localPosition = newPosition;

        result.GetComponent<Rigidbody>().mass = lerp;
        
        CurrentOctaveSpan += result.transform.localScale.x;

        return result;
    }

    protected override GameObject GenerateOtherKey(int octave, int index1, int index2, float ratio, Transform parent, AudioClip sound, Vector3 scale, float raise, float pushback)
    {
        GameObject result = base.GenerateOtherKey(octave, index1, index2, ratio, parent, sound, scale, raise, pushback);

        float totalIndex = (7 * octave) + (index1 + index2)/2f;
        int maxIndex = (7 * notes.Length) - 1;

        float lerp = totalIndex / maxIndex;

        result.transform.localScale = Vector3.Lerp(MaxKeyDimensions, KeyDimensions, lerp);

        Vector3 newPosition = result.transform.localPosition;
        newPosition.z = (MaxKeyDimensions.z + result.transform.localScale.z) / 2f - KeyOverlap;
        result.transform.localPosition = newPosition;

        return result;
    }
}
