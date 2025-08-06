using System.IO;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Settings
{
    /// <summary>
    /// Create a class extending this with the scriptable object of your choice to have a ready-to-use working settings provider.<br/>
    /// See example use in AdriKat.Toolkit.Settings.AudioSettings.cs
    /// </summary>
    /// <typeparam name="T">The ScriptableObject storing your settings.</typeparam>
    public class SettingsProvider<T> where T : ScriptableObject, IDefaultInitializable
    {
        private const string SETTINGS_FOLDER = "Assets/Settings";

        private static T _settings;

        /// <summary>
        /// Retrieves an instance of the settings object of type <typeparamref name="T"/>. If the settings object
        /// does not exist in the designated path, it will create a new instance, set it to its default values,
        /// store it as an asset, and return it. Later calls will return the cached instance.
        /// </summary>
        /// <typeparam name="T">The type of the settings object, which must be a ScriptableObject implementing IDefaultInitializable.</typeparam>
        /// <returns>The existing or newly created settings object of type <typeparamref name="T"/>.</returns>
        public static T GetOrCreateSettings()
        {
            // Return from cache if existing.
            if (_settings != null) return _settings;
            
            string fullPath = Path.Combine(SETTINGS_FOLDER, $"{typeof(T).Name}.asset");
            
            _settings = AssetDatabase.LoadAssetAtPath<T>(fullPath);

            // Return if just found it.
            if (_settings != null) return _settings;
            
            // Create a new one.
            if (!Directory.Exists(SETTINGS_FOLDER))
            {
                Directory.CreateDirectory(SETTINGS_FOLDER);
            }

            _settings = ScriptableObject.CreateInstance<T>();
            _settings.SetDefault();
            AssetDatabase.CreateAsset(_settings, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return _settings;
        }

        
        public static void OpenSettingsForUser()
        {
            EditorUtility.OpenPropertyEditor(GetOrCreateSettings());
        }
    }
}