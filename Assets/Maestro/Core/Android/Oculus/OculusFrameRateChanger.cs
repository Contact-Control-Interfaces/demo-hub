using UnityEngine;

#if UNITY_ANDROID

namespace Maestro
{
    public class OculusFrameRateChanger : MonoBehaviour
    {
        public bool Enable90fps; 

        void Start()
        {
            if (Enable90fps) OVRPlugin.systemDisplayFrequency = 90.0f;
        }
    }
}

#endif
