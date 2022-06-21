using UnityEngine;
using UnityEngine.Android;

namespace Maestro
{
    public class OculusBluetoothLocationPermissionRequester : MonoBehaviour
    {
#if UNITY_ANDROID

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
            MaestroAndroidWrapper.Start();
        }
#endif
    }
}
