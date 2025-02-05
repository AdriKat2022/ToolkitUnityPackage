using TMPro;
using UnityEngine;

namespace AdriKat.AnimationScripts
{
    public class RainbowText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMeshPro;
        [SerializeField] private bool activateOnAwake = true;

        [Header("Color")]
        [SerializeField, Range(0f, 1f)] private float saturation = 1f;
        [SerializeField, Range(0f, 1f)] private float brightness = 1f;

        [Header("Behaviour")]
        [SerializeField, Range(-2f, 2)] private float spread = 1f;
        [SerializeField, Range(0f, 5f)] private float speed = 2f;
        [SerializeField] private bool scrollEffect = true;

        private void Awake()
        {
            if (!activateOnAwake)
            {
                enabled = false;
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
            float timeOffset = Time.time * speed;

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

            float hue = (scrollEffect ? (index / (float)textInfo.characterCount) * spread : 0) + timeOffset;
            hue %= 1f;
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
