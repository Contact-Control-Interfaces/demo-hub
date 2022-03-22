using UnityEngine;
using UnityEngine.Android;

#if UNITY_ANDROID

namespace Maestro
{
    public class OculusBluetoothLocationPermissionRequester : MonoBehaviour
    {
        void Start()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                StartScanningForGloves();
            }
            else
            {
                Permission.RequestUserPermission(Permission.FineLocation, CreatePermissionCallbacks());
            }
        }

        private PermissionCallbacks CreatePermissionCallbacks()
        {
            var callbacks = new PermissionCallbacks();
                
            callbacks.PermissionGranted += permission =>
            {
                // We only care about results of `Permission.FineLocation`
                if (permission != Permission.FineLocation)
                {
                    return;
                }
                
                StartScanningForGloves();
            };
                
            callbacks.PermissionDenied += permission =>
            {
                if (permission == Permission.FineLocation)
                {
                   // Don't start the glove detection
                } 
            };

            return callbacks;
        }

        private void StartScanningForGloves()
        {
            bool detectionStarted = MaestroGloveConnector.StartScanningForGloves();
        
            if (detectionStarted)
            {
                Debug.Log("Maestro detection service was started.");
            }
            else
            {
                Debug.LogError("Maestro detection service failed to start!");
            }
        }
    }
}

#endif
