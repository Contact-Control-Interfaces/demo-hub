using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BlackScale", order = 3)]
public class BlackScale : ScriptableObject, IEnumerable, IEnumerable<AudioClip>
{
    public AudioClip Db, Eb, Gb, Ab, Bb;

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable<AudioClip>)this).GetEnumerator();
    }

    IEnumerator<AudioClip> IEnumerable<AudioClip>.GetEnumerator()
    {
        yield return Db; 
        yield return Eb; 
        yield return Gb; 
        yield return Ab; 
        yield return Bb;
    }
}
