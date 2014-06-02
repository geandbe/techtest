using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Blurocket.TechTestShared
{
    public static class SerializationHelper
    {
        public static byte[] SerializeToBinary<T>(T instance)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize<T>(ms, instance);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserializers an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static T DeserializeFromBinary<T>(Byte[] instance)
        {
            using (var ms = new MemoryStream(instance))
            {
                ms.Position = 0;
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}
