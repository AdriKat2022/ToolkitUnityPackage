using TMPro;
using UnityEngine;

namespace AdriKat.AnimationScripts.Text
{
    /// <summary>
    /// This script applies a rainbow effect to a TextMeshPro component.
    /// </summary>
    public class RainbowTextMeshPro : MonoBehaviour
    {
        private const int WidthTextReference = 20;

        [SerializeField] private TextMeshPro textMeshPro;
        [Tooltip("If true, the rainbow effect will be applied on Awake.\n" +
            "If false, you need to enable the component manually with the 'enabled' variable.\n" +
            "For example: rainbowText.enabled = true;")]
        [SerializeField] private bool activateOnAwake = true;

        [Header("Color")]
        [Range(0f, 1f)]
        [SerializeField] private float saturation = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float brightness = 1f;

        [Header("Rainbow effect")]
        [Range(0f, 5f)]
        [SerializeField] private float speed = 2f;
        [SerializeField] private bool positionBased = false;
        [SerializeField] private bool scrollEffect = true;
        [Tooltip("The higher the value, the more compact the colors will be in each letters.\n" +
            "The lower the value, the more spread the rainbow effect will be (zero is the same as deactivating the scroll effect).")]
        [Range(0f, 5f)]
        [SerializeField] private float letterColorDelta = 1f;
        [SerializeField] private bool reverseScrollDirection = false;

        [Header("Special behaviours")]
        [SerializeField] private bool restoreColorOnDisabled = true;
        [SerializeField] private Color defaultColor = Color.white;
        [Tooltip("If true, the component will remember the last color of the text when it is enabled, " +
            "and replace the default color with it.")]
        [SerializeField] private bool rememberLastColorOnEnabled = false;

        private float textWidthReference;

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
                hue += textMeshPro.mesh.vertices[vertexIndex].x * letterColorDelta / WidthTextReference;
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
