using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Maestro.EditorExtensions
{
    static class StructEditor
    {
        public static SerializedObject HandTransformsOnGUI(this SerializedObject property)
        {
            /* Inspector stuff */
            
            foreach (SerializedProperty sp in property.FindProperty("transforms")) {
                /* Don't know if foreach actually iterates through child properties */
            }

            EditorGUILayout.HelpBox("HandTransforms OnGUI goes here", MessageType.Info);

            return property;
        }

        public static SerializedObject HandTransformsOnGUI(this SerializedObject property, int heck)
        {
            /* this can be called like serializedObject.HandTransformsOnGUI(10); */
            /* first argument is the specific instance, kind of like __self in python */

            return property;
        }
    }
}
