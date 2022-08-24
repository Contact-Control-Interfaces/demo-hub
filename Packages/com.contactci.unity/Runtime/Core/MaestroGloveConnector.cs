using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class MaestroGloveConnector
    {
        private static MaestroGloveConnector _singleton;

        public static MaestroGloveConnector Instance => _singleton ??= new MaestroGloveConnector();

        public event EventHandler<bool> OnDetectionStarted;
        
        public void StartScanningForGloves()
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            OnDetectionStarted?.Invoke(this, MaestroNativeWrapper.start_maestro_detection_service());
#elif UNITY_ANDROID
            var bluetoothPermissionRequester = new OculusBluetoothLocationPermissionRequester();
            
            bluetoothPermissionRequester.PermissionDenied += () => OnDetectionStarted?.Invoke(this, false);
            bluetoothPermissionRequester.PermissionGranted += () =>
            {
                MaestroAndroidWrapper.Start();
                OnDetectionStarted?.Invoke(this, true);
            };

            bluetoothPermissionRequester.RequestPermission();
#endif
        }

        public IntPtr GetRightGlovePointer()
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.get_right_glove_pointer();
#elif UNITY_ANDROID
            return MaestroAndroidWrapper.getRightGloveIdentifier();
#endif
        }

        public IntPtr GetLeftGlovePointer()
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.get_left_glove_pointer();
#elif UNITY_ANDROID
            return MaestroAndroidWrapper.getLeftGloveIdentifier();
#endif
        }

        public bool isGloveConnected(IntPtr glovePointer)
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.is_glove_connected(glovePointer);
#elif UNITY_ANDROID
            return MaestroAndroidWrapper.checkIsGloveConnected(glovePointer);
#endif
        }
    }
}