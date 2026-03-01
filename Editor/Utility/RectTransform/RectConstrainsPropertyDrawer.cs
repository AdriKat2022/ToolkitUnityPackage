using UnityEditor;
using UnityEngine;
using static AdriKat.Toolkit.Utility.EditorDrawUtils;

namespace AdriKat.Toolkit.Utility
{

    /// <summary>
    /// Offers more control for constraining: <br/>
    /// Toggles to enable/disable the constraints (disabling will set the values to -infinity for min values, and + infinity for max values)<br/>
    /// </summary>
    [CustomPropertyDrawer(typeof(RectConstraints))]
    public class RectConstraintsPropertyDrawer : PropertyDrawer
    {
        private const float ToggleWidth = 100;

        private float cachedMinWidth = 0;
        private float cachedMinHeight = 0;
        private float cachedMaxWidth = 100;
        private float cachedMaxHeight = 100;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool anyChange = false;

            var fitInRectTransform = property.FindPropertyRelative(nameof(RectConstraints.fitInRectTransform));
            var useRectTransformWidthAsMin = property.FindPropertyRelative(nameof(RectConstraints.useRectTransformWidthAsMin));
            var useRectTransformWidthAsMax = property.FindPropertyRelative(nameof(RectConstraints.useRectTransformWidthAsMax));
            var useRectTransformWidthToFit = property.FindPropertyRelative(nameof(RectConstraints.useRectTransformWidthToFit));
            var useRectTransformHeightAsMin = property.FindPropertyRelative(nameof(RectConstraints.useRectTransformHeightAsMin));
            var useRectTransformHeightAsMax = property.FindPropertyRelative(nameof(RectConstraints.useRectTransformHeightAsMax));
            var useRectTransformHeightToFit = property.FindPropertyRelative(nameof(RectConstraints.useRectTransformHeightToFit));
            var minW = property.FindPropertyRelative(nameof(RectConstraints.minWidth));
            var maxW = property.FindPropertyRelative(nameof(RectConstraints.maxWidth));
            var minH = property.FindPropertyRelative(nameof(RectConstraints.minHeight));
            var maxH = property.FindPropertyRelative(nameof(RectConstraints.maxHeight));

            // Header
            EditorGUI.BeginProperty(position, label, property);
            Rect head = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            //EditorGUI.LabelField(head, label);

            string foldoutKey = $"{property.serializedObject}{property.propertyPath}";
            
            bool isExpanded = EditorPrefs.GetBool(foldoutKey, false);
            isExpanded = EditorGUI.Foldout(head, isExpanded, label, true);
            EditorPrefs.SetBool(foldoutKey, isExpanded);

            if (!isExpanded)
            {
                EditorGUI.EndProperty();
                property.serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUI.indentLevel++;

            head.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(head, fitInRectTransform, new GUIContent("Use RectTransform"));

            float columnMargin = 10;
            float lh = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;

            head.y += lh * 1 + sp * 6;

            Rect row = new(head.x, head.y, head.width, lh);
            Rect divider = row;

            row.xMax /= 2;
            row.xMax -= columnMargin;


            // FIRST ROW (WIDTH)
            if (fitInRectTransform.objectReferenceValue != null)
            {
                useRectTransformWidthToFit.boolValue = EditorGUI.ToggleLeft(row, "Fit Width", useRectTransformWidthToFit.boolValue);
                row.y += lh * 2 + sp;

                if (!useRectTransformWidthToFit.boolValue)
                {
                    useRectTransformWidthAsMin.boolValue = EditorGUI.ToggleLeft(row, "Use As Min Width", useRectTransformWidthAsMin.boolValue);
                    row.y += lh + sp;

                    useRectTransformWidthAsMax.boolValue = EditorGUI.ToggleLeft(row, "Use As Max Width", useRectTransformWidthAsMax.boolValue);
                    row.y += lh * 2 + sp;


                    if (useRectTransformWidthAsMin.boolValue && !useRectTransformWidthAsMin.hasMultipleDifferentValues)
                    {
                        // Set the max height to the rect transform width
                        var rectTransform = fitInRectTransform.objectReferenceValue as UnityEngine.RectTransform;
                        minW.floatValue = rectTransform.rect.width;
                    }
                    if (useRectTransformWidthAsMax.boolValue && !useRectTransformWidthAsMax.hasMultipleDifferentValues)
                    {
                        // Set the max height to the rect transform width
                        var rectTransform = fitInRectTransform.objectReferenceValue as UnityEngine.RectTransform;
                        maxW.floatValue = rectTransform.rect.width;
                    }
                }
                else
                {
                    var rectTransform = fitInRectTransform.objectReferenceValue as UnityEngine.RectTransform;
                    minW.floatValue = rectTransform.rect.width;
                    maxW.floatValue = rectTransform.rect.width;
                    row.y += lh + sp;
                    row.y += lh * 2 + sp;
                }
                divider.y += lh + sp;
                EditorGUI.LabelField(divider, "", GUI.skin.horizontalSlider);
            }

            bool minWidthAutoDriven = useRectTransformWidthAsMin.boolValue || useRectTransformWidthToFit.boolValue;
            DrawDisabled(() =>
            {
                anyChange |= DrawFloatFieldWithToggle(row, minW, "Min Width", float.NegativeInfinity, ref cachedMinWidth, minWidthAutoDriven);
            }, fitInRectTransform.objectReferenceValue == null || !minWidthAutoDriven);

            row.y += lh + sp;

            bool maxWidthAutoDriven = useRectTransformWidthAsMax.boolValue || useRectTransformWidthToFit.boolValue;
            DrawDisabled(() =>
            {
                anyChange |= DrawFloatFieldWithToggle(row, maxW, "Max Width", float.PositiveInfinity, ref cachedMaxWidth, maxWidthAutoDriven);
            }, fitInRectTransform.objectReferenceValue == null || !maxWidthAutoDriven);

            row.x += row.width + columnMargin;
            row.y = head.y;


            // SECOND ROW (HEIGHT)
            if (fitInRectTransform.objectReferenceValue != null)
            {
                useRectTransformHeightToFit.boolValue = EditorGUI.ToggleLeft(row, "Fit Height", useRectTransformHeightToFit.boolValue);
                row.y += lh * 2 + sp;

                if (!useRectTransformHeightToFit.boolValue)
                {
                    useRectTransformHeightAsMin.boolValue = EditorGUI.ToggleLeft(row, "Use As Min Height", useRectTransformHeightAsMin.boolValue);
                    row.y += lh + sp;

                    useRectTransformHeightAsMax.boolValue = EditorGUI.ToggleLeft(row, "Use As Max Height", useRectTransformHeightAsMax.boolValue);
                    row.y += lh * 2 + sp;

                    if (useRectTransformHeightAsMin.boolValue && !useRectTransformHeightAsMin.hasMultipleDifferentValues)
                    {
                        // Set the max height to the rect transform height
                        var rectTransform = fitInRectTransform.objectReferenceValue as UnityEngine.RectTransform;
                        minH.floatValue = rectTransform.rect.height;
                    }
                    if (useRectTransformHeightAsMax.boolValue && !useRectTransformHeightAsMax.hasMultipleDifferentValues)
                    {
                        // Set the max height to the rect transform height
                        var rectTransform = fitInRectTransform.objectReferenceValue as UnityEngine.RectTransform;
                        maxH.floatValue = rectTransform.rect.height;
                    }
                }
                else
                {
                    var rectTransform = fitInRectTransform.objectReferenceValue as UnityEngine.RectTransform;
                    minH.floatValue = rectTransform.rect.height;
                    maxH.floatValue = rectTransform.rect.height;
                    row.y += lh + sp;
                    row.y += lh * 2 + sp;
                }
            }

            bool minHeightAutoDriven = useRectTransformHeightAsMin.boolValue || useRectTransformHeightToFit.boolValue;
            DrawDisabled(() =>
            {
                anyChange |= DrawFloatFieldWithToggle(row, minH, "Min Height", float.NegativeInfinity, ref cachedMinHeight, minHeightAutoDriven);
                row.y += lh + sp;
            }, fitInRectTransform.objectReferenceValue == null || !minHeightAutoDriven);

            bool maxHeightAutoDriven = useRectTransformHeightAsMax.boolValue || useRectTransformHeightToFit.boolValue;
            DrawDisabled(() =>
            {
                anyChange |= DrawFloatFieldWithToggle(row, maxH, "Max Height", float.PositiveInfinity, ref cachedMaxHeight, maxHeightAutoDriven);
            }, fitInRectTransform.objectReferenceValue == null || !maxHeightAutoDriven);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();

            if (anyChange)
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;

            string foldoutKey = $"{property.serializedObject}{property.propertyPath}";
            bool isExpanded = EditorPrefs.GetBool(foldoutKey, false);

            if (isExpanded)
            {
                totalHeight += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4 + EditorGUIUtility.standardVerticalSpacing * 3;

                if (property.FindPropertyRelative(nameof(RectConstraints.fitInRectTransform)).objectReferenceValue != null)
                {
                    totalHeight += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
                }
            }


            return totalHeight;
        }

        private bool DrawFloatFieldWithToggle(Rect position, SerializedProperty prop, string label, float defaultValue, ref float cachedValue, bool isValueDriven)
        {
            bool propertyOrToggleChanged = false;

            float toggleW = ToggleWidth;

            Rect togRect = new(position.x, position.y, toggleW, position.height);
            Rect fldRect = new(position.x + toggleW + 4, position.y, position.width - toggleW - 4, position.height);

            bool isEnabled = defaultValue < 0
                ? !float.IsNegativeInfinity(prop.floatValue)
                : !float.IsPositiveInfinity(prop.floatValue);

            EditorGUI.BeginChangeCheck();
            bool isOverriding = EditorGUI.ToggleLeft(togRect, label, isEnabled) && !isValueDriven;
            if (EditorGUI.EndChangeCheck())
            {
                propertyOrToggleChanged = true;
                if (isOverriding)
                {
                    prop.floatValue = cachedValue;
                }
                else if (!isValueDriven)
                {
                    prop.floatValue = defaultValue;
                }
            }

            bool isUnconstrained = defaultValue < 0
                ? float.IsNegativeInfinity(prop.floatValue)
                : float.IsPositiveInfinity(prop.floatValue);

            using (new EditorGUI.DisabledScope(!isOverriding))
            {
                if (isOverriding || isValueDriven)
                {
                    EditorGUI.BeginChangeCheck();
                    float newVal = EditorGUI.FloatField(fldRect, GUIContent.none, prop.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        propertyOrToggleChanged = true;
                        prop.floatValue = newVal;
                    }
                }
                else if (!isOverriding && isUnconstrained)
                {
                    //EditorGUI.FloatField(fldRect, GUIContent.none, defaultValue);
                    EditorGUI.LabelField(fldRect, GUIContent.none, new GUIContent("Unconstrained"));
                }
            }

            if (isOverriding)
            {
                cachedValue = prop.floatValue;
            }

            return propertyOrToggleChanged;
        }
    }
}