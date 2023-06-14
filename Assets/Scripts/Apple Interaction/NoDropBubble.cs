using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NoDropBubble : MonoBehaviour
{
    List<FingerCollider> inside;

    private IMaestroHand[] lastHands;

    void Awake()
    {
        inside = new List<FingerCollider>();
        lastHands = new IMaestroHand[0];
    }

    public void Register(FingerCollider fc)
    {
        inside.Add(fc);

        IMaestroHand[] hands = GetHands(inside);
        if (hands.Length > lastHands.Length) {
            SetDisallowDropping(hands, lastHands, true);
        }

        lastHands = hands;
    }

    public void Deregister(FingerCollider fc)
    {
        inside.Remove(fc);

        IMaestroHand[] hands = GetHands(inside);
        if (hands.Length < lastHands.Length) {
            SetDisallowDropping(lastHands, hands, false);
        }

        lastHands = hands;
    }

    private IMaestroHand[] GetHands(List<FingerCollider> fcs)
    {
        return fcs.Select(x => x.hpi).Distinct().ToArray();
    }

    private void SetDisallowDropping(IEnumerable<IMaestroHand> big, IEnumerable<IMaestroHand> small, bool toSet)
    {
        var handsToModify = big.Except(small);
        foreach (IMaestroHand hand in handsToModify)
        {
            if (hand.grabManager == null) return;
            hand.grabManager.DisallowDropping = toSet;
            Debug.Log($"{hand.gameObject.name} set to {toSet.ToString()}");
        }
    }
}
