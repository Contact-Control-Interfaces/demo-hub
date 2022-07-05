using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEditor.Android;
using UnityEditor.XR.Oculus;

#if UNITY_ANDROID

namespace ContactCi.Android {
    
    internal class AndroidManifestPermissionPatcher : IPostGenerateGradleAndroidProject
    {
        // Last time we checked the Oculus XR Plugin source, the callbackOrder was 10000
        private static readonly int FallbackCallbackOrder = 10001;
        private static readonly string AndroidUri = "http://schemas.android.com/apk/res/android";
        private static readonly string AndroidManifestPath = "/src/main/AndroidManifest.xml";
        private static readonly List<string> AndroidPermissions = new List<string>
        {
            "android.permission.ACCESS_COARSE_LOCATION",
            "android.permission.ACCESS_FINE_LOCATION",
            "android.permission.BLUETOOTH",
            "android.permission.BLUETOOTH_SCAN",
            "android.permission.BLUETOOTH_CONNECT",
            "android.permission.BLUETOOTH_ADMIN"
        }.Distinct().ToList();
        
        // Just after the Oculus build processor
        public int callbackOrder
        {
            get
            {
                Debug.Log("Determining callbackOrder value of Oculus XR Plugin to ensure we patch the AndroidManifest.xml");

                // We will try to determine it at runtime below, and if we can't we will have to fall back
                // on the assumption that it hasn't changed.
                int order = FallbackCallbackOrder;

                try
                {
                    int oculusXrPluginCallbackOrder = GetOculusXrPluginCallbackOrder();
                    
                    Debug.Log($"Oculus XR Plugin callbackOrder is {oculusXrPluginCallbackOrder}");
                    
                    order = oculusXrPluginCallbackOrder + 1;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to obtain Oculus XR Plugin callbackOrder. Falling back to {order}. {e}");
                }
                
                Debug.Log($"Using callbackOrder {order}");
                
                return order;
            }
        }

        private int GetOculusXrPluginCallbackOrder()
        {
            var oculusPackageAssembly = typeof(OculusBuildProcessor).Assembly;
            var internalTypeInstance = oculusPackageAssembly.CreateInstance("UnityEditor.XR.Oculus.OculusManifest")
                                       ?? throw new ApplicationException("Failed to instantiate OculusManifest.");
            var internalType = internalTypeInstance.GetType();
            var property = internalType.GetProperty("callbackOrder", BindingFlags.Instance | BindingFlags.Public)
                           ?? throw new ApplicationException("Failed to find 'callbackOrder' property on OculusManifest.");

            return Convert.ToInt32(property.GetValue(internalTypeInstance));
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var projectSettings = ProjectSettings.GetOrCreateSettings();

            if (!projectSettings.ShouldPatchManifest)
            {
                Debug.LogWarning("AndroidManifest.xml patching disabled via Project "
                        + $"Settings/{ProjectSettingsProvider.SettingsPath}/Patch AndroidManifest.xml toggle.");
                return;
            }
            
            var manifestPath = path + AndroidManifestPath;
            var manifestDoc = new XmlDocument();
            
            manifestDoc.Load(manifestPath);

            Debug.Log($"Patching Android permissions in ${manifestPath}");

            List<string> existingPermissions = GetExistingPermissions(manifestDoc);
            List<string> duplicatePermissions = existingPermissions.Intersect(AndroidPermissions).ToList();

            duplicatePermissions
                .ForEach(duplicatePermission => Debug.Log($"Android permission {duplicatePermission} already exists."));
            
            AndroidPermissions
                .Except(duplicatePermissions).ToList()
                .ForEach(permission => CreateNewPermission(manifestDoc, permission));
            
            manifestDoc.Save(manifestPath);
            
            Debug.Log("Completed AndroidManifest.xml modification");
        }

        private List<string> GetExistingPermissions(XmlDocument doc)
        {
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("android", AndroidUri);
            
            var xmlNodeList = doc.SelectNodes("/manifest/uses-permission/@android:name", namespaceManager);
            
            return xmlNodeList?.Cast<XmlNode>().Select(node => node.Value).ToList();
        }

        private void CreateNewPermission(XmlDocument doc, string permission)
        {
            var attributes = new Dictionary<string, string> { {"name", permission} };
            
            Debug.Log($"Creating Android permission {permission}");

            CreateXmlTagWithAttributes(doc, "/manifest", "uses-permission", attributes);
        }
        
        private void CreateXmlTagWithAttributes(XmlDocument doc, string parentPath, string tag, IDictionary<string, string> attributes)
        {
            var childElement = doc.CreateElement(tag);
            
            attributes.ToList().ForEach(attributePair =>
            {
                childElement.SetAttribute(attributePair.Key, AndroidUri, attributePair.Value);
            });
            
            doc.SelectSingleNode(parentPath)?.AppendChild(childElement);
        }
    }
}

#endif