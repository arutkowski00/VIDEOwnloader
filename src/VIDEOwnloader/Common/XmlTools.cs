using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using VIDEOwnloader.Model;

namespace VIDEOwnloader.Common
{
    internal class XmlTools
    {
        public static string Serialize<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stringWriter = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(writer, obj);
                    return stringWriter.ToString();
                }
            }
        }

        public static T Deserialize<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                var obj = serializer.Deserialize(reader);
                return (T)obj;
            }
        }

        public static bool TryDeserialize<T>(string xml, out T obj)
        {
            obj = default(T);
            var serializer = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    if (!serializer.CanDeserialize(xmlReader))
                        return false;
                    var deserializedObj = serializer.Deserialize(xmlReader);
                    if (!(deserializedObj is T))
                        return false;
                    obj = (T)deserializedObj;
                }
            }
            return true;
        }
    }
}