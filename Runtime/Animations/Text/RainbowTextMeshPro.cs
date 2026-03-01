using TMPro;
using UnityEngine;

namespace AdriKat.Toolkit.Animations
{
    /// <summary>
    /// This script applies a rainbow effect to a TextMeshPro component.
    /// </summary>
    public class RainbowTextMeshPro : MonoBehaviour
    {
        private const int WIDTH_TEXT_REFERENCE = 20;

        public TMP_Text textMeshPro;
        [Tooltip("If true, the rainbow effect will be applied on Awake.\n" +
            "If false, you need to enable the component manually with the 'enabled' variable.\n" +
            "For example: rainbowText.enabled = true;")]
        public bool activateOnAwake = true;

        [Header("Color")]
        [Range(0f, 1f)]
        public float saturation = 1f;
        [Range(0f, 1f)]
        public float brightness = 1f;

        [Header("Rainbow effect")]
        [Range(0f, 5f)]
        public float speed = 2f;
        public bool positionBased = false;
        public bool scrollEffect = true;
        [Tooltip("The higher the value, the more compact the colors will be in each letters.\n" +
            "The lower the value, the more spread the rainbow effect will be (zero is the same as deactivating the scroll effect).")]
        [Range(0f, 5f)]
        public float letterColorDelta = 1f;
        public bool reverseScrollDirection;

        [Header("Special behaviours")]
        public bool restoreColorOnDisabled = true;
        public Color defaultColor = Color.white;
        [Tooltip("If true, the component will remember the last color of the text when it is enabled, " +
            "and replace the default color with it.")]
        public bool rememberLastColorOnEnabled;

        private void Awake()
        {
            if (!activateOnAwake)
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (textMeshPro == null)
                return;
            if (rememberLastColorOnEnabled)
            {
                defaultColor = textMeshPro.color;
            }
            textMeshPro.color = defaultColor;
        }

        private void OnDisable()
        {
            if (textMeshPro == null)
                return;

            if (restoreColorOnDisabled)
            {
                textMeshPro.color = defaultColor;
                textMeshPro.ForceMeshUpdate();
            }
        }

        private void Update()
        {
            if (textMeshPro == null)
                return;

            ApplyRainbowEffect();
        }

        private void ApplyRainbowEffect()
        {
            textMeshPro.ForceMeshUpdate();
            TMP_TextInfo textInfo = textMeshPro.textInfo;

            // If the speed is negative, the colors will go backwards
            float timeOffset = speed * Time.time;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                ApplyColorToCharacter(textInfo, i, timeOffset);
            }

            UpdateMeshColors(textInfo);
        }

        private void ApplyColorToCharacter(TMP_TextInfo textInfo, int index, float timeOffset)
        {
            int vertexIndex = textInfo.characterInfo[index].vertexIndex;
            int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;
            Color32[] newVertexColors = textInfo.meshInfo[materialIndex].colors32;

            float hue = timeOffset * (reverseScrollDirection ? -1 : 1);

            if (scrollEffect && positionBased)
            {
                hue += textMeshPro.mesh.vertices[vertexIndex].x * letterColorDelta / WIDTH_TEXT_REFERENCE;
            }
            else if (scrollEffect)
            {
                hue += (index / (float)textInfo.characterCount) * letterColorDelta;
            }

            // Make sure the hue stays between 0 and 1
            hue = Mathf.Repeat(hue, 1f);

            Color color = Color.HSVToRGB(hue, saturation, brightness);

            newVertexColors[vertexIndex + 0] = color;
            newVertexColors[vertexIndex + 1] = color;
            newVertexColors[vertexIndex + 2] = color;
            newVertexColors[vertexIndex + 3] = color;
        }

        private void UpdateMeshColors(TMP_TextInfo textInfo)
        {
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                textMeshPro.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}
