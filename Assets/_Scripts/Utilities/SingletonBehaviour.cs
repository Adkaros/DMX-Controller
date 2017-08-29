using UnityEngine;
using System.Collections.Generic;

namespace M1.Utilities
{
    // Make a subclass of this class with T as the subclass to make a singleton
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<T>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = typeof(T).Name;
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        public static bool DoesExist()
        {
            return instance != null;
        }

        // Makes this object a persistent singleton unless the singleton already exists in which case
        // the this object is destroyed
        protected void DontDestroy()
        {
            if (this == Instance)
            {
                // needs to be a root object before calling DontDestroyOnLoad
                Instance.transform.parent = null;
                MonoBehaviour.DontDestroyOnLoad(Instance.gameObject);
            }
            else
            {
                MonoBehaviour.Destroy(this);
            }
        }
    }
}