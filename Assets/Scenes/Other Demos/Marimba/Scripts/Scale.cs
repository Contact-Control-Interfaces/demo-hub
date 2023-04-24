using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Scale", order = 1)]
public class Scale : ScriptableObject
{
    public WhiteScale white;
    public BlackScale black;
}

