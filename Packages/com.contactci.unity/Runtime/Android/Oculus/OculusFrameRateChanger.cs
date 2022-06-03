using UnityEngine;

namespace Maestro
{
    public class OculusFrameRateChanger : MonoBehaviour
    {
#if UNITY_ANDROID

        public bool Enable90fps;

        void Start()
        {
            if (Enable90fps) OVRPlugin.systemDisplayFrequency = 90.0f;
        }
#endif
    }
}
