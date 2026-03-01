using System;
using AdriKat.Toolkit.Utility.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdriKat.Toolkit.Utility
{
    public static class EditorDrawUtils
    {
        #region Events
        
        /// <summary>
        /// Enables drag and drop support by running onDragAndDropPerformed when the user dropped something onto the reference rect.
        /// </summary>
        /// <param name="dragAndDropAreaRect">The rect that the user will drop objects onto.</param>
        /// <param name="onDragAndDropPerformed">The callback to run when the drag and drop occurs. The callback should return true if the GUI changed or false otherwise.</param>
        /// <param name="countLimit">1 only accepts a single object. A negative number accepts any count. Any other strictly positive number represents the maximum object count before refusing the drag and drop.</param>
        public static void SetDragAndDropCallback(Rect dragAndDropAreaRect, Func<Object[], bool> onDragAndDropPerformed, int countLimit = -1)
        {
            SetDragAndDropCallback<Object>(dragAndDropAreaRect, onDragAndDropPerformed, countLimit);
        }

        /// <summary>
        /// Enables drag and drop support by running onDragAndDropPerformed when the user dropped something onto the reference rect.
        /// Only allows drops of the AcceptedType parameter.
        /// </summary>
        /// <param name="dragAndDropAreaRect">The rect that the user will drop objects onto.</param>
        /// <param name="onDragAndDropPerformed">The callback to run when the drag and drop occurs. The callback should return true if the GUI changed or false otherwise.</param>
        /// <param name="countLimit">1 only accepts a single object. A negative number accepts any count. Any other strictly positive number represents the maximum object count before refusing the drag and drop.</param>
        public static void SetDragAndDropCallback<AcceptedType>(Rect dragAndDropAreaRect, Func<Object[], bool> onDragAndDropPerformed, int countLimit = -1)
        {
            Event evt = Event.current;
            
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;

            if (!dragAndDropAreaRect.Contains(evt.mousePosition)) return;

            bool isDragCountValid = countLimit < 1 || DragAndDrop.objectReferences.Length <= countLimit;
            bool isDragObjectTypeValid = DragAndDrop.objectReferences[0].GetType() == typeof(AcceptedType);

            if (isDragCountValid && isDragObjectTypeValid)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    GUI.changed = onDragAndDropPerformed(DragAndDrop.objectReferences);
                }
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            evt.Use();
        }
        #endregion
        
        #region Draw
        
        /// <summary>
        /// Draws content inside a clipped, swipe-fade group (like EditorGUILayout.BeginFadeGroup).
        /// </summary>
        /// <param name="position">The outer position rect.</param>
        /// <param name="fade">A float from 0 (hidden) to 1 (fully shown).</param>
        /// <param name="fullHeight">The full height of the content at full visibility.</param>
        /// <param name="drawContent">Callback to draw the GUI inside the group. Coordinates start at (0, 0).</param>
        /// <param name="applyAlpha">If true, alpha blending is applied based on fade.</param>
        public static void DrawClippedFadeGroup(Rect position, float fade, float fullHeight, Action<Rect> drawContent, bool applyAlpha = false)
        {
            float currentHeight = fade * fullHeight;
            if (currentHeight <= 0f) return;

            Rect groupRect = new Rect(position.x, position.y, position.width, currentHeight);
            GUI.BeginGroup(groupRect);

            if (applyAlpha)
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, fade);
                drawContent?.Invoke(new Rect(0, 0, position.width, fullHeight));
                GUI.color = oldColor;
            }
            else
            {
                drawContent?.Invoke(new Rect(0, 0, position.width, fullHeight));
            }

            GUI.EndGroup();
        }

        /// <summary>
        /// Draws a serialized object reference property with an optional button for creating or assigning
        /// the object and a foldout toggle for additional details.
        /// </summary>
        /// <param name="property">The serialized property to draw, typically an object reference.</param>
        /// <param name="label">The label to display for the object field.</param>
        /// <param name="actionName">The name of the button used to create or assign the object when the value is null.</param>
        /// <param name="onActionClicked">A callback function invoked when the action button is clicked, expected to return a new object reference.</param>
        /// <param name="foldoutState">A boolean value representing the foldout state for additional UI elements. Passed by reference to retain changes.</param>
        public static void DrawObjectWithFoldout(SerializedProperty property, GUIContent label, string actionName, Func<Object> onActionClicked, ref bool foldoutState)
        {
            if (property == null)
            {
                EditorGUILayout.HelpBox("SerializedProperty is null", MessageType.Error);
                return;
            }

            EditorGUI.BeginProperty(EditorGUILayout.GetControlRect(), label, property);

            // Begin horizontal: property + create button (if null)
            EditorGUILayout.BeginHorizontal();

            // Draw property field (without label if null)
            if (property.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(property, label);
                
                if (GUILayout.Button(new GUIContent(actionName), GUILayout.Width(60)))
                {
                    property.objectReferenceValue = onActionClicked?.Invoke();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndProperty();
                return;
            }

            // When NOT null
            EditorGUILayout.PropertyField(property, label);

            EditorGUILayout.EndHorizontal();

            // Foldout for extra fields
            foldoutState = EditorGUILayout.Foldout(foldoutState, "Details", true);

            if (foldoutState)
            {
                // Indent and draw all serialized fields of the referenced object
                EditorGUI.indentLevel++;

                SerializedObject subSerialized = new SerializedObject(property.objectReferenceValue);
                SerializedProperty iterator = subSerialized.GetIterator();

                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    // Don't draw script field
                    if (iterator.name == "m_Script")
                        continue;

                    EditorGUILayout.PropertyField(iterator, true);
                    enterChildren = false;
                }

                subSerialized.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws an object field with a foldout for additional details and an optional action button.
        /// </summary>
        /// <param name="position">The area within which to draw the GUI elements.</param>
        /// <param name="property">The serialized property representing the object reference to be displayed.</param>
        /// <param name="label">The label for the object field.</param>
        /// <param name="actionName">The text for the action button (e.g., "Create" or "Add").</param>
        /// <param name="onActionClicked">A callback invoked when the action button is clicked, expected to return a reference to the created object.</param>
        /// <param name="foldoutState">A boolean reference indicating the foldout state, which can be toggled via the foldout GUI element.</param>
        public static void DrawObjectWithFoldout(Rect position, SerializedProperty property, GUIContent label, string actionName, Func<Object> onActionClicked, ref bool foldoutState)
        {
            if (property == null) return;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect current = new Rect(position.x, position.y, position.width, lineHeight);

            EditorGUI.BeginProperty(position, label, property);
            
            float buttonWidth = 60f;
            
            // Field and Create Button side-by-side
            if (property.objectReferenceValue == null)
            {
                buttonWidth = actionName.IsNullOrEmpty() ? 0f : buttonWidth;
                
                Rect fieldRect0 = new Rect(current.x, current.y, current.width - buttonWidth - 4f, lineHeight);
                Rect buttonRect0 = new Rect(current.x + fieldRect0.width + 4f, current.y, buttonWidth, lineHeight);

                EditorGUI.PropertyField(fieldRect0, property, label);
                if (!actionName.IsNullOrEmpty() && GUI.Button(buttonRect0, actionName))
                {
                    property.objectReferenceValue = onActionClicked?.Invoke();
                }

                EditorGUI.EndProperty();
                return;
            }
            
            Rect fieldRect = new Rect(current.x, current.y, current.width - buttonWidth - 4f, lineHeight);
            Rect buttonRect = new Rect(current.x + fieldRect.width + 4f, current.y, buttonWidth, lineHeight);

            EditorGUI.PropertyField(fieldRect, property, label);
            
            Color temp = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUI.Button(buttonRect, "Remove"))
            {
                property.objectReferenceValue = null;
                EditorGUI.EndProperty();
                GUI.backgroundColor = temp;
                return;
            }
            GUI.backgroundColor = temp;
            
            // Foldout
            current.y += lineHeight + spacing;
            foldoutState = EditorGUI.Foldout(current, foldoutState, "Details", true);

            if (foldoutState)
            {
                EditorGUI.indentLevel++;

                SerializedObject subSO = new SerializedObject(property.objectReferenceValue);
                SerializedProperty iterator = subSO.GetIterator();

                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    if (iterator.name == "m_Script")
                        continue;

                    current.y += lineHeight + spacing;
                    float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                    Rect propRect = new Rect(current.x, current.y, current.width, propHeight);

                    EditorGUI.PropertyField(propRect, iterator, true);
                    current.y += propHeight - lineHeight; // account for tall properties

                    enterChildren = false;
                }

                subSO.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws a property without a foldout.<br/>
        /// Will produce the same result that EditorGUI.PropertyField if the property is not a generic.
        /// </summary>
        /// <param name="position">The space to use to draw the property.</param>
        /// <param name="property">The property to draw.</param>
        public static void DrawPropertyNoFoldout(Rect position, SerializedProperty property)
        {
            // Default unity draw if not generic or class.
            if (!property.hasVisibleChildren || property.propertyType != SerializedPropertyType.Generic)
            {
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
                return;
            }

            // Draw children manually (always expanded, no foldout)
            SerializedProperty copy = property.Copy();
            SerializedProperty end = copy.GetEndProperty();

            copy.NextVisible(true); // Enter children

            while (!SerializedProperty.EqualContents(copy, end))
            {
                float height = EditorGUI.GetPropertyHeight(copy, GUIContent.none, true);
                position.height = height;
                EditorGUI.PropertyField(position, copy, true);

                position.y += height + EditorGUIUtility.standardVerticalSpacing;

                copy.NextVisible(false);
            }
        }
        
        /// <summary>
        /// Executes a drawing action within a disabled GUI state if specified.
        /// </summary>
        /// <param name="drawAction">The action to execute for drawing the content.</param>
        /// <param name="isActive">Determines whether the GUI state is active or disabled. If false, the GUI is disabled during the execution.</param>
        public static void DrawDisabled(Action drawAction, bool isActive = false)
        {
            bool previousState = GUI.enabled;
            GUI.enabled = isActive;
            drawAction?.Invoke();
            GUI.enabled = previousState;
        }
        
        #endregion
        
        #region Height Calculation
        
        /// <summary>
        /// Calculates the height required to render the contents of a SerializedProperty
        /// that references an object, including foldout state and any sub-properties.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the object reference.</param>
        /// <param name="foldoutState">The foldout state indicating whether the object's sub-properties are expanded.</param>
        /// <returns>The total height necessary to render the object and its content.</returns>
        public static float GetPropertyHeightOfObjectContent(SerializedProperty property, bool foldoutState)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.objectReferenceValue == null)
            {
                return height;
            }
            
            // Start with 2 lines: one for the property, one for the foldout
            height = EditorGUIUtility.singleLineHeight * 2f;

            if (foldoutState)
            {
                // Add height for each visible property inside the object (excluding m_Script)
                SerializedObject subSerialized = new SerializedObject(property.objectReferenceValue);
                SerializedProperty iterator = subSerialized.GetIterator();

                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    if (iterator.name == "m_Script") continue;

                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    enterChildren = false;
                }

                height -= EditorGUIUtility.standardVerticalSpacing;
            }
            
            return height;
        }

        /// <summary>
        /// Returns the total height of a property without a foldout.<br/>
        /// Will return the same result that EditorGUI.GetPropertyHeight if the property is not a generic.
        /// </summary>
        /// <param name="property">The property to get the height from.</param>
        /// <returns>The required height including all children for this property.</returns>
        public static float GetPropertyHeightNoFoldout(SerializedProperty property)
        {
            // Default unity height if not generic or class.
            if (!property.hasVisibleChildren || property.propertyType is not SerializedPropertyType.Generic)
            {
                return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
            }

            float totalHeight = 0f;

            SerializedProperty copy = property.Copy();
            SerializedProperty end = copy.GetEndProperty();

            copy.NextVisible(true);

            while (!SerializedProperty.EqualContents(copy, end))
            {
                totalHeight += EditorGUI.GetPropertyHeight(copy, GUIContent.none, true) + EditorGUIUtility.standardVerticalSpacing;

                copy.NextVisible(false);
            }

            return totalHeight;
        }
        
        #endregion
    }
}