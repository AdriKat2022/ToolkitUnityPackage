using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdriKat.Utils.Debugging
{
    /// <summary>
    /// Helper class to log messages on the screen. Useful for debugging purposes, especially on builds where the log might not be visible.<br/>
    /// Warning as is may be expensive to spam Log calls when active.
    /// </summary>
    public class LiveLogger : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textPrefab;
        [SerializeField] private VerticalLayoutGroup _layoutGroup;
        [SerializeField] private float _logTTL = 5f;
        [SerializeField] private bool _showWarning = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private bool _showLogs = true;

        #region Singleton
        private static LiveLogger _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Debug.LogWarning("There is already an instance of LiveLogger in the scene. Deleting this one.");
                Destroy(this);
                return;
            }

            _instance = this;

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        #endregion

        private void Start()
        {
            if (_textPrefab == null)
            {
                if (_showWarning)
                {
                    Debug.LogWarning("Consider having a ready instanced live logger for more customization on the text prefab.");
                }

                InstantiateDefaultConfiguration();
            }
        }

        private void InstantiateDefaultConfiguration()
        {
            Canvas canvas;

            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            if (_layoutGroup == null)
            {
                RectTransform container = new GameObject("LogsContainer").AddComponent<RectTransform>();
                container.transform.SetParent(canvas.transform, false);
                container.anchorMin = new Vector2(1f, 0f);
                container.anchorMax = new Vector2(1f, 1f);
                container.pivot = new Vector2(1f, 1f);
                container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600);

                _layoutGroup = container.gameObject.AddComponent<VerticalLayoutGroup>();
                _layoutGroup.childControlWidth = true;
                _layoutGroup.childControlHeight = false;
                _layoutGroup.childAlignment = TextAnchor.UpperRight;
                _layoutGroup.childForceExpandHeight = false;
                _layoutGroup.spacing = 30;
                _layoutGroup.padding = new RectOffset(0, 20, 20, 0);

                _textPrefab = new GameObject("LiveLoggerTextTemplate").AddComponent<TextMeshProUGUI>();
                _textPrefab.alignment = TextAlignmentOptions.TopRight;
                ContentSizeFitter fitter = _textPrefab.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                if (_dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(_textPrefab.gameObject);
                    DontDestroyOnLoad(canvasGo);
                }
            }
        }

        /// <summary>
        /// Logs a message on the screen.<br/>
        /// Creates a text object with the message and destroys it after a set amount of time.<br/>
        /// If no instances of LiveLogger are found in the scene, a new one will be created with a default configuration
        /// and used for the rest of the game.
        /// </summary>
        /// <param name="text">Log text</param>
        public static void Log(string text)
        {
            // If the instance is null, create a new GameObject with the LiveLogger component attached to it
            if (_instance == null)
            {
                GameObject go = new GameObject("LiveLogger");
                _instance = go.AddComponent<LiveLogger>();
            }

            if (_instance._showLogs)
            {
                _ = _instance.LogInternal(text);
            }
        }

        private async Task LogInternal(string text)
        {
            while (_layoutGroup == null)
            {
                // Wait for the layout group to be assigned
                await Task.Yield();
            }

            TextMeshProUGUI textInstance = Instantiate(_textPrefab, _layoutGroup.transform);
            textInstance.text = text;
            textInstance.transform.SetAsFirstSibling();
            Destroy(textInstance.gameObject, _logTTL);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.GetComponent<RectTransform>());
        }
    }
}