using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_ANDROID
using Valve.VR;
#endif

namespace Maestro
{
    public class IndexFlatnessChecker : FlatnessChecker
    {
        private bool flat;

        public override bool isFlat()
        {
            return flat;
        }

        private float sum(float[] floats)
        {
            float result = 0.0f;

            for (int i = 0; i < floats.Length; i++)
                result += floats[i];

            return result;
        }

#if !UNITY_ANDROID
        SteamVR_Behaviour_Skeleton parent;

        public float total;
        public float threshold = 0.0f;

        // Start is called before the first frame update
        void Start()
        {
            parent = this.GetComponent<SteamVR_Behaviour_Skeleton>();
        }

        // Update is called once per frame
        void Update()
        {
            total = sum(parent.fingerCurls);
            flat = total <= threshold;
        }
#endif
    }
}