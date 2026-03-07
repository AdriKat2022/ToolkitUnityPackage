using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Debugging
{
    public class DictionaryViewerWindow : EditorWindow
    {
        private UnityEngine.Object targetObject;
        private Vector2 scroll;
        private bool repaintContinuously;
        
        private List<FieldInfo> dictionaryFieldInfosCache;
        private bool[] foldouts;
        
        [MenuItem("Toolkit/Debugging/Dictionary Viewer")]
        public static void ShowWindow()
        {
            GetWindow<DictionaryViewerWindow>("Dictionary Viewer");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dictionary Viewer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawRepaintToggle();
            
            EditorGUILayout.Space();
            
            targetObject = EditorGUILayout.ObjectField("Target Object", targetObject, typeof(UnityEngine.Object), true);

            if (targetObject == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Drag an object to display its dictionaries here.", MessageType.Info, true);
                return;
            }

            EditorGUILayout.Space();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            
            if (dictionaryFieldInfosCache == null)
            {
                ExtractDictionariesFromObject(targetObject, out dictionaryFieldInfosCache);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Dictionaries are only scanned on the first layer depth. If a dictionary lies deeper in the hierarchy of that object, it won't be detected by this tool yet.", EditorStyles.helpBox);
            
            EditorGUILayout.Space();
            
            EditorGUI.indentLevel++;
            
            DisplayDictionaries(targetObject, dictionaryFieldInfosCache, foldouts);
            
            EditorGUI.indentLevel--;

            if (dictionaryFieldInfosCache.Count == 0)
            {
                EditorGUILayout.LabelField($"No dictionary was found in {targetObject.name}.");
            }
            
            EditorGUILayout.EndScrollView();
        }

        private static void DisplayDictionaries(object obj, List<FieldInfo> fieldInfosCache, bool[] foldoutBools)
        {
            foldoutBools ??= new bool[fieldInfosCache.Count];

            for (var index = 0; index < fieldInfosCache.Count; index++)
            {
                FieldInfo field = fieldInfosCache[index];
                object value = field.GetValue(obj);

                EditorDrawUtils.HorizontalLine();

                EditorGUILayout.Space();

                if (value == null)
                {
                    EditorGUILayout.LabelField($"{field.Name}", EditorStyles.largeLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"This dictionary is NULL.", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.indentLevel--;

                    continue;
                }

                IDictionary dictionary = value as IDictionary;

                EditorGUILayout.LabelField($"{field.Name}", EditorStyles.largeLabel);

                EditorGUI.indentLevel++;

                bool hasAnEntry = false;

                foreach (DictionaryEntry entry in dictionary)
                {
                    string keyString = entry.Key != null ? entry.Key.ToString() : "NULL";
                    string valueString = entry.Value != null ? entry.Value.ToString() : "NULL";

                    hasAnEntry = true;

                    EditorGUILayout.LabelField(keyString, valueString);
                }

                if (!hasAnEntry) EditorGUILayout.LabelField("No entry was found in this dictionary.", EditorStyles.miniLabel);

                EditorGUILayout.Space(20);

                EditorGUI.indentLevel--;
            }
        }

        private static void ExtractDictionariesFromObject(object obj, out List<FieldInfo> fieldInfosCache)
        {
            fieldInfosCache = new List<FieldInfo>();
                
            Type type = obj.GetType();

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (typeof(IDictionary).IsAssignableFrom(field.FieldType))
                {
                    fieldInfosCache.Add(field);
                }
            }
        }

        private void DrawRepaintToggle()
        {
            EditorGUI.BeginChangeCheck();

            repaintContinuously = EditorGUILayout.ToggleLeft(
                "Force Update Off Focus (Affects performances)",
                repaintContinuously);
            
            // repaintContinuously = EditorGUILayout.Popup(repaintContinuously ? 1 : 0,
            //     new[] { "Update On Focus", "Update Live (Costs More Performance)" },
            //     EditorStyles.layerMaskField) == 1;

            if (EditorGUI.EndChangeCheck())
            {
                UpdateRepaintSubscription();
            }
        }
        
        private void UpdateRepaintSubscription()
        {
            EditorApplication.update -= ForceRepaint;

            if (repaintContinuously)
            {
                EditorApplication.update += ForceRepaint;
            }
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= ForceRepaint;
        }
        
        private void ForceRepaint()
        {
            if (this == null) return;

            Repaint();
        }
    }
}