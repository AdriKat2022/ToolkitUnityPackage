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
    public class EditorSettingsProvider<T> where T : ScriptableObject, IDefaultInitializable
    {
        private const string SETTINGS_FOLDER = "Assets/Settings";
        
        private static T _settings;
        
        private static string FullPath => Path.Combine(SETTINGS_FOLDER, $"{typeof(T).Name}.asset");

        /// <summary>
        /// Retrieves an instance of the settings object of type <typeparamref name="T"/>. If the settings object
        /// does not exist in the designated path, it will create a new instance, set it to its default values,
        /// store it as an asset, and return it. Later calls will return the cached instance.
        /// </summary>
        /// <typeparam name="T">The type of the settings object, which must be a ScriptableObject implementing IDefaultInitializable.</typeparam>
        /// <returns>The existing or newly created settings object of type <typeparamref name="T"/>.</returns>
        public static T GetOrCreateSettings()
        {
            T settings = GetSettings();
            
            return settings != null ? settings : CreateSettings();
        }

        public static T CreateSettings()
        {
            // Create a new one.
            if (!Directory.Exists(SETTINGS_FOLDER))
            {
                Directory.CreateDirectory(SETTINGS_FOLDER);
            }

            _settings = ScriptableObject.CreateInstance<T>();
            _settings.SetDefault();
            
            AssetDatabase.CreateAsset(_settings, FullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return _settings;
        }

        public static T GetSettings()
        {
            // Return from cache if existing.
            if (_settings != null) return _settings;
            
            _settings = AssetDatabase.LoadAssetAtPath<T>(FullPath);

            return _settings;
        }
        
        public static bool DoesSettingsExist()
        {
            return GetSettings() != null;
        }
        
        public static void OpenSettingsForUser()
        {
            EditorUtility.OpenPropertyEditor(GetOrCreateSettings());
        }
    }
}