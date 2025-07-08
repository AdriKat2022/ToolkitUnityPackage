using System;
using UnityEngine;

namespace AdriKat.Toolkit.Utils
{
    public static class EditorDrawUtils
    {
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
    }
}