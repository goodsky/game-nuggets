using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    public static class GameDataSerializer
    {
        /// <summary>
        /// Serialize GameData to a string.
        /// </summary>
        /// <typeparam name="T">The GameData contract type.</typeparam>
        /// <returns>The XML representation of the game data.</returns>
        public static string Save<T>(T gameData)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, gameData);
                stream.Position = 0L;

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Deserialize GameData from a Unity configuration file.
        /// Throws SerializationException on a malconfigured XML.
        /// </summary>
        /// <typeparam name="T">The GameData contract type.</typeparam>
        /// <param name="config">Name of the Unity TextAsset configuration without extension.</param>
        /// <returns>The serialized game data.</returns>
        public static T Load<T>(string config) where T : class
        {
            var configFile = Resources.Load<TextAsset>(config);

            var serializer = new XmlSerializer(typeof(MenuData));
            using (var configStream = new MemoryStream(configFile.bytes))
            {
                return serializer.Deserialize(configStream) as T;
            }
        }
    }
}
