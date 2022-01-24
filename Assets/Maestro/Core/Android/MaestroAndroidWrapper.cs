using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID

namespace Maestro
{
    public class MaestroAndroidWrapper
    {
        private static AndroidJavaObject androidAdapterInstance;
        private static AndroidJavaObject AndroidAdapterInstance
        {
            get
            {
                if (androidAdapterInstance == null)
                {
                    Debug.Log("Trying to create instance of AndroidAdapter...");

                    androidAdapterInstance = new AndroidJavaObject("com.cci.maestro.AndroidAdapter");

                    Debug.Log("Created instance of AndroidAdapter.");
                }

                return androidAdapterInstance;
            }
        }

        public static void Start()
        {
            AndroidAdapterInstance.Call("start");
        }

        public static void Stop()
        {
            AndroidAdapterInstance.Call("stop");
        }

        public static bool IsBleProcessing()
        {
            return AndroidAdapterInstance.Call<bool>("isBleScanning");
        }

        public static IntPtr getLeftGloveIdentifier()
        {
            return (IntPtr)AndroidAdapterInstance.Call<int>("getLeftGloveIdentifier");
        }

        public static IntPtr getRightGloveIdentifier()
        {
            return (IntPtr)AndroidAdapterInstance.Call<int>("getRightGloveIdentifier");
        }

        public static bool checkIsGloveConnected(IntPtr gloveIdentifier)
        {
            return AndroidAdapterInstance.Call<bool>("isGloveConnected", (int)gloveIdentifier);
        }

        public static void ApplyHaptics(IntPtr maestroPtr, MaestroHapticContext newContext, MaestroHapticContext lastContext)
        {
            sbyte sbyteOrZero(byte? maybeByte)
            {
                return (sbyte)(maybeByte ?? 0);
            }

            int maestroPtrInt = (int)maestroPtr;
            AndroidJavaObject adapter = AndroidAdapterInstance;

            if (adapter == null)
            {
                Debug.Log("Adapter is null");
            }

            adapter.Call("applyThumbForceFeedback", maestroPtrInt, sbyteOrZero(newContext.ThumbAmplitude));
            adapter.Call("applyIndexForceFeedback", maestroPtrInt, sbyteOrZero(newContext.IndexAmplitude));
            adapter.Call("applyMiddleForceFeedback", maestroPtrInt, sbyteOrZero(newContext.MiddleAmplitude));
            adapter.Call("applyRingForceFeedback", maestroPtrInt, sbyteOrZero(newContext.RingAmplitude));
            adapter.Call("applyLittleForceFeedback", maestroPtrInt, sbyteOrZero(newContext.LittleAmplitude));

            adapter.Call("applyThumbVibration", maestroPtrInt, sbyteOrZero(newContext.ThumbVibrationEffect?.Value));
            adapter.Call("applyIndexVibration", maestroPtrInt, sbyteOrZero(newContext.IndexVibrationEffect?.Value));
            adapter.Call("applyMiddleVibration", maestroPtrInt, sbyteOrZero(newContext.MiddleVibrationEffect?.Value));
            adapter.Call("applyRingVibration", maestroPtrInt, sbyteOrZero(newContext.RingVibrationEffect?.Value));
            adapter.Call("applyLittleVibration", maestroPtrInt, sbyteOrZero(newContext.LittleVibrationEffect?.Value));
        }
    }
}

#endif