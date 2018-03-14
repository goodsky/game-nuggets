using UnityEngine;

namespace Common
{
    /// <summary>
    /// Singleton behaviour class, used for components that should only have one instance.
    /// </summary>
    /// <typeparam name="T">The Singleton Type</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;

        /// <summary>
        /// Returns the Singleton instance of the classes type.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!IsInitialized)
                {
                    Debug.LogErrorFormat("Singleton {0} is not initialized.", typeof(T).Name);
                }

                return instance;
            }
        }

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return instance != null;
            }
        }

        /// <summary>
        /// Sets the singleton instance.
        /// Singletons should be sure to call base.Awake().
        /// </summary>
        protected virtual void Awake()
        {
            if (!enabled)
                return;

            if (IsInitialized && instance != this)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(this);
                }
                else
                {
                    Destroy(this);
                }

                Debug.LogErrorFormat("Trying to instantiate a second instance of singleton class {0}. Additional Instance was destroyed", GetType().Name);
            }
            else if (!IsInitialized)
            {
                instance = (T)this;
            }
        }

        /// <summary>
        /// Desets the singleton instance.
        /// Singletons should be sure to call base.OnDestroy().
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
