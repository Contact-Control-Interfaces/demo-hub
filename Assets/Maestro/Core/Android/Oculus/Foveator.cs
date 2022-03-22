using UnityEngine;

#if UNITY_ANDROID

namespace Maestro
{
    public class Foveator : MonoBehaviour
    {
        public OVRManager.FixedFoveatedRenderingLevel FixedFoveatedRenderingLevel;
        
        void Start()
        {
            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
            OVRManager.useDynamicFixedFoveatedRendering = true;
        }
    }
}

#endif