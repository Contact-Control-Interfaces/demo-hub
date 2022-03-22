using UnityEngine;

#if UNITY_ANDROID

namespace Maestro
{
    public class Foveator : MonoBehaviour
    {
        public OVRManager.FixedFoveatedRenderingLevel FixedFoveatedRenderingLevel;
        
        void Start()
        {
            OVRManager.fixedFoveatedRenderingLevel = FixedFoveatedRenderingLevel;
            OVRManager.useDynamicFixedFoveatedRendering = FixedFoveatedRenderingLevel != OVRManager.FixedFoveatedRenderingLevel.Off;
        }
    }
}

#endif