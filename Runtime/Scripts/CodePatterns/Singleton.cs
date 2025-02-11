using UnityEngine;

namespace AdriKat.Utils.CodePatterns
{
    /// <summary>
    /// Singleton pattern implementation.<br/>
    /// Make your class inherit from this class to make it a singleton.<br/>
    /// You can choose the policy of the singleton in the inspector.<br/>
    /// You can also enable or disable lazy instantiation globally.<br/>
    /// </summary>
    /// <typeparam name="T">The class you want to have as a singleton.</typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Allow or not lazy instantiation of the singleton.<br/>
        /// If true, the singleton can be created at runtime if it doesn't exist.<br/>
        /// If false, getter returns null and throws a warning if the singleton doesn't exist.<br/>
        /// </summary>
        public static bool AllowLazyInstantiation { get; set; } = false;

        [Header("Singleton Settings")]
        [SerializeField] private bool _dontDestroyOnLoad = true;
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
                        if (AllowLazyInstantiation) // Only create if lazy instantiation is enabled
                        {
                            Debug.Log($"[Singleton] Creating a new instance of {typeof(T)}.");
                            GameObject singletonObject = new GameObject(typeof(T).Name);
                            _instance = singletonObject.AddComponent<T>();
                        }
                        else
                        {
                            Debug.LogWarning($"[Singleton] Instance of {typeof(T)} not found and lazy instantiation is disabled.");
                        }
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
                    if (_dontDestroyOnLoad)
                    {
                        DontDestroyOnLoad(gameObject);
                    }
                }
            }
            else
            {
                _instance = this as T;
                if (_dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
