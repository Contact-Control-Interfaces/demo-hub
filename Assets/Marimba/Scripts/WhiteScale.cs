using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WhiteScale", order = 2)]
public class WhiteScale : ScriptableObject, IEnumerable, IEnumerable<AudioClip>
{
    public AudioClip A, B, C, D, E, F, G;

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable<AudioClip>)this).GetEnumerator();
    }

    IEnumerator<AudioClip> IEnumerable<AudioClip>.GetEnumerator()
    {
        yield return C;
        yield return D;
        yield return E;
        yield return F;
        yield return G;
        yield return A;
        yield return B;
    }
}
