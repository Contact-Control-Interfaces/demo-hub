using UnityEngine;

namespace Maestro
{
    public class Foveator : MonoBehaviour
    {
#if UNITY_ANDROID

        public OVRManager.FixedFoveatedRenderingLevel FixedFoveatedRenderingLevel;
        
        void Start()
        {
            OVRManager.fixedFoveatedRenderingLevel = FixedFoveatedRenderingLevel;
            OVRManager.useDynamicFixedFoveatedRendering = FixedFoveatedRenderingLevel != OVRManager.FixedFoveatedRenderingLevel.Off;
        }
#endif
    }
}
