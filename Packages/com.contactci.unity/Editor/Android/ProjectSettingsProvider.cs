using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContactCi.Android
{
    public class ProjectSettingsProvider
    {
        public static readonly string SettingsPath = "Project/Contact CI/Maestro";
        
        private static string[] Keywords = {"contact", "maestro", "build", "AndroidManifest.xml", "android", "manifest", "patch"};

        [SettingsProvider]
        public static SettingsProvider MaestroAndroidPluginSettingsProvider()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.Project)
            {
                label = "Android Build Settings",
                guiHandler = (searchContext) =>
                { 
                    var settings = ProjectSettings.GetSerializedSettings();

                    EditorGUIUtility.labelWidth = 200;
                    EditorGUILayout.PropertyField(settings.FindProperty("shouldPatchManifest"),
                        new GUIContent(
                            "Patch AndroidManifest.xml", 
                            "Toggle whether we should automatically patch the AndroidManifest.xml"
                            + " with correct permissions to enable Bluetooth Low Energy. This is required for"
                            + " Maestro and other Contact CI haptics devices to function, unless you patch the"
                            + " AndroidManifest.xml yourself manually by exporting the Android Studio project."
                        )
                    );

                    settings.ApplyModifiedProperties();
                },
                keywords = new HashSet<string>(Keywords)
            };
        }
    }
}