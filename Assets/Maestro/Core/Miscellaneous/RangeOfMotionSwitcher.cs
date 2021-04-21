using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class RangeOfMotionSwitcher : MonoBehaviour {

    private float elapsed;
    private SteamVR_Behaviour_Skeleton parent;

    public float rate = 0.1f;
    public Vector3 direction = Vector3.forward;
    public float distance;
    public Vector3 offset = new Vector3(-0.5f, 0, 0);
    public bool ignoreTwoHand;

	// Use this for initialization
	void Start () {
        elapsed = 0.0f;
        parent = GetComponent<SteamVR_Behaviour_Skeleton>();
    }

    private Vector3 getOffset(Vector3 scales)
    {
        return gameObject.transform.right * scales.x + gameObject.transform.up * scales.y + gameObject.transform.forward * scales.z;
    }
	
	// Update is called once per frame
	void Update () {
        elapsed += Time.deltaTime;
        if (elapsed > rate)
        {
            Vector3 dir = -gameObject.transform.right.normalized;
            if (parent.inputSource == SteamVR_Input_Sources.LeftHand)
                dir *= -1;

            elapsed %= rate;
            RaycastHit[] hits = Physics.SphereCastAll(gameObject.transform.position + getOffset(offset), 0.08f, dir, distance);

            MaestroInteractable found = null;
            bool change = false;
            foreach (RaycastHit hit in hits)
            {
                if ((found = getInteractable(hit)) != null && found.type != InteractionType.Static)
                {
                    if (ignoreTwoHand && found.type == InteractionType.TwoHand)
                        continue;

                    change = true;
                    break;
                }
            }

            if (change)
                parent.rangeOfMotion = EVRSkeletalMotionRange.WithController;
            else
                parent.rangeOfMotion = EVRSkeletalMotionRange.WithoutController;

            Debug.DrawRay(gameObject.transform.position+ getOffset(offset), dir * distance, Color.red, rate);
        }

	}

    private MaestroInteractable getInteractable(RaycastHit hit)
    {
        return hit.collider.GetComponentInParent<MaestroInteractable>();
    }
}
