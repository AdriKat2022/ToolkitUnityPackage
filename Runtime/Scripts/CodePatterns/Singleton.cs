using UnityEngine;

namespace AdriKat.Utils.CodePatterns
{
    /// <summary>
    /// Singleton pattern implementation.<br/>
    /// Make your class inherit from this class to make it a singleton.<br/>
    /// You can choose the policy of the singleton in the inspector.<br/>
    /// </summary>
    /// <typeparam name="T">The class you want to have as a singleton.</typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _applicationIsQuitting = false;

        [SerializeField] private SingletonPolicy singletonPolicy = SingletonPolicy.FirstStays;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed. Returning null.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (singletonPolicy == SingletonPolicy.FirstStays)
                {
                    Destroy(gameObject); // New instance gets destroyed.
                }
                else if (singletonPolicy == SingletonPolicy.LastStays)
                {
                    Destroy(_instance.gameObject); // Old instance gets replaced.
                    _instance = this as T;
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}