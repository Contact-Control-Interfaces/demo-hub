using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public static class HapticsApplicator
    {
        public static void ApplyHaptics(IntPtr maestroPtr, MaestroHapticContext newContext, MaestroHapticContext lastContext)
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            MaestroNativeWrapper.SetHapticsFromContexts(maestroPtr, newContext, lastContext);
#elif UNITY_ANDROID
            MaestroAndroidWrapper.ApplyHaptics(maestroPtr, newContext, lastContext);
#endif
        }
    }
}