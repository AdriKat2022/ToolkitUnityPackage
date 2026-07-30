using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AdriKat.Toolkit.Attributes
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class StandaloneButtonDrawer : Editor
    {
        #region UI Toolkit
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Default inspector
            InspectorElement.FillDefaultInspector(
                root,
                serializedObject,
                this
            );

            MonoBehaviour targetScript = (MonoBehaviour)target;
            Type type = targetScript.GetType();

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public
            );

            foreach (MethodInfo method in methods)
            {
                var attribute = method.GetCustomAttribute<InspectorButton>();

                if (attribute == null)
                    continue;

                var button = new Button(() =>
                {
                    foreach (var obj in targets)
                    {
                        method.Invoke(obj, null);
                    }
                })
                {
                    text = attribute.Name
                };

                root.Add(button);
            }

            return root;
        }
        
        #endregion
        
        #region IMGUI
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MonoBehaviour targetScript = (MonoBehaviour)target;
            Type type = targetScript.GetType();

            // Find methods with the [Button] attribute
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (MethodInfo method in methods)
            {
                InspectorButton inspectorButtonAttribute = (InspectorButton)Attribute.GetCustomAttribute(method, typeof(InspectorButton));

                if (inspectorButtonAttribute == null)
                {
                    continue;
                }

                GUILayout.Space(2);
                if (GUILayout.Button(inspectorButtonAttribute.Name))
                {
                    method.Invoke(targetScript, null);
                }
            }
        }
        
        #endregion
    }
}
