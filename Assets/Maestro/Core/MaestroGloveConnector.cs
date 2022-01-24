using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public static class MaestroGloveConnector
    {
        public static bool StartScanningForGloves()
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.start_maestro_detection_service();
#elif UNITY_ANDROID
            MaestroAndroidWrapper.Start();
            return true;
#endif
        }

        public static IntPtr GetRightGlovePointer()
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.get_right_glove_pointer();
#elif UNITY_ANDROID
            return MaestroAndroidWrapper.getRightGloveIdentifier();
#endif
        }

        public static IntPtr GetLeftGlovePointer()
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.get_left_glove_pointer();
#elif UNITY_ANDROID
            return MaestroAndroidWrapper.getLeftGloveIdentifier();
#endif
        }

        public static bool isGloveConnected(IntPtr glovePointer)
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.is_glove_connected(glovePointer);
#elif UNITY_ANDROID
            return MaestroAndroidWrapper.checkIsGloveConnected(glovePointer);
#endif
        }
    }
}