using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class StandaloneButtonDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MonoBehaviour targetScript = (MonoBehaviour)target;
            Type type = targetScript.GetType();

            // Find methods with the [Button] attribute
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (MethodInfo method in methods)
            {
                StandaloneButtonActionAttribute standaloneButtonAttribute = (StandaloneButtonActionAttribute)Attribute.GetCustomAttribute(method, typeof(StandaloneButtonActionAttribute));

                if (standaloneButtonAttribute == null)
                {
                    continue;
                }

                GUILayout.Space(2);
                if (GUILayout.Button(standaloneButtonAttribute.Name))
                {
                    method.Invoke(targetScript, null);
                }
            }
        }
    }
}
