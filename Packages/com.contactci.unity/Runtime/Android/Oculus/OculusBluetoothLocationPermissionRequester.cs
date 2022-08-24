using System;
using UnityEngine;
using UnityEngine.Android;

namespace Maestro
{
    public class OculusBluetoothLocationPermissionRequester
    {
#if UNITY_ANDROID
        public event Action PermissionGranted;
        public event Action PermissionDenied;

        public void RequestPermission()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("Already has FineLocation permission");
                
                PermissionGranted?.Invoke();
            }
            else
            {
                Debug.Log("Requesting FineLocation permission");

                Permission.RequestUserPermission(Permission.FineLocation, CreatePermissionCallbacks());
            }
        }

        private PermissionCallbacks CreatePermissionCallbacks()
        {
            var callbacks = new PermissionCallbacks();
                
            callbacks.PermissionGranted += permission =>
            {
                Debug.Log("Permission granted: " + permission);
                
                // We only care about results of `Permission.FineLocation`
                if (permission == Permission.FineLocation)
                {
                    PermissionGranted?.Invoke();
                }
            };
                
            callbacks.PermissionDenied += permission =>
            {
                Debug.Log("Permission denied: " + permission);
                
                // We only care about results of `Permission.FineLocation`
                if (permission == Permission.FineLocation)
                {
                    PermissionDenied?.Invoke();
                } 
            };

            return callbacks;
        }
#endif
    }
}
