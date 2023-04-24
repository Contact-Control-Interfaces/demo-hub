using Leap;
using Leap.Unity;
using Maestro;
using Maestro.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraleapFlatnessChecker : FlatnessChecker
{
    public HandModelBase handModel;

    protected float flatness;

    public override bool isFlat()
    {
        return flatness > 0.99f;
    }

    void Start()
    {
        if (handModel == null)
            handModel = this.GetComponentInParent<HandModelBase>();
    }

    void Update()
    {
        var hand = handModel.GetLeapHand();
        if (hand != null) {
            flatness = 1f - hand.GrabStrength;
        }
    }
}
