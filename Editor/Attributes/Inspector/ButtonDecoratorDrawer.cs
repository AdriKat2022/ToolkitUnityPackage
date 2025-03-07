using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Utils.Attributes
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonDecoratorDrawer : Editor
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
                ButtonActionAttribute buttonAttribute = (ButtonActionAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonActionAttribute));

                if (buttonAttribute == null)
                {
                    continue;
                }

                GUILayout.Space(2);
                if (GUILayout.Button(buttonAttribute.Name))
                {
                    method.Invoke(targetScript, null);
                }
            }
        }
    }
}
