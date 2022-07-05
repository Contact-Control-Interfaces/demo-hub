using UnityEditor;
using UnityEngine;
using System.IO;

namespace ContactCi.Android
{
    public class ProjectSettings : ScriptableObject
    {
        public const string AssetFilePath = "Assets/Maestro/ProjectSettings.asset";
        
        [SerializeField]
        private bool shouldPatchManifest;

        public bool ShouldPatchManifest => shouldPatchManifest;

        private static ProjectSettings GetDefaultSettings()
        {
            var settings = CreateInstance<ProjectSettings>();
            
            settings.shouldPatchManifest = true;
            
            return settings;
        }

        public static ProjectSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ProjectSettings>(AssetFilePath);

            // No settings saved; use default
            if (settings == null)
            {
                settings = GetDefaultSettings();

                string assetFileFolder = Path.GetDirectoryName(AssetFilePath);

                if (!AssetDatabase.IsValidFolder(assetFileFolder))
                {
                    Directory.CreateDirectory(assetFileFolder);
                }

                AssetDatabase.CreateAsset(settings, AssetFilePath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }
        
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}