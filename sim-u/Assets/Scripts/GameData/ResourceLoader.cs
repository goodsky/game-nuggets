using Common;
using System;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    public enum ResourceType
    {
        Materials,
        Models,
        Prefabs
    }

    public enum ResourceCategory
    {
        Buildings,
        Terrain,
        Toolbar,
        UI
    }

    /// <summary>
    /// Manager for loading runtime resources.
    /// The benefits of this loader are:
    ///    1) Schematizes the folders. Fewer typos on 'Model' vs 'Models'.
    ///    2) Throws a fatal error if it fails to load instead of silently continuing.
    /// </summary>
    public static class ResourceLoader
    {
        public static T[] LoadAll<T>(ResourceType type, ResourceCategory category) where T : UnityEngine.Object
        {
            string resourcePath = string.Format("{0}/{1}", type.ToString(), category.ToString());
            return Resources.LoadAll<T>(resourcePath);
        }

        public static T Load<T>(ResourceType type, ResourceCategory category, string name) where T : UnityEngine.Object
        {
            UnityEngine.Object resourceObj = Load(type, category, name, typeof(T));
            return resourceObj as T;
        }

        public static UnityEngine.Object Load(ResourceType type, ResourceCategory category, string name, Type returnType)
        {
            string resourcePath = string.Format("{0}/{1}/{2}", type.ToString(), category.ToString(), name);
            UnityEngine.Object resource = Resources.Load(resourcePath, returnType);
            if (resource == null)
            {
                GameLogger.FatalError("ResourceLoader could not find resource {0}", resourcePath);
            }

            return resource;
        }
    }

    /// <summary>
    /// Attribute to allow the <see cref="GameDataLoader{T}"/> to load resources.
    /// </summary>
    public class ResourceLoaderAttribute : XmlIgnoreAttribute
    {
        public ResourceLoaderAttribute(ResourceType type, ResourceCategory category, string propertyName = null, string resourceName = null)
        {
            Type = type;
            Category = category;
            PropertyName = propertyName;
            ResourceName = resourceName;
        }

        public ResourceType Type { get; set; }

        public ResourceCategory Category { get; set; }

        public string PropertyName { get; set; }

        public string ResourceName { get; set; }
    }
}
