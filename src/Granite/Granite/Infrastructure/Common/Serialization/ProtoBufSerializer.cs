using System.IO;
using ProtoBuf;

namespace Granite.Infrastructure.Common.Serialization
{
    public class ProtoBufSerializer : IProtoBufSerializer
    {
        public byte[] Serialize<T>(T source)
        {
            using (var memStream = new MemoryStream())
            {
                Serializer.Serialize(memStream, source);
                return memStream.ToArray();
            }
        }

        public void Serialize<T>(Stream destination, T source)
        {
            Serializer.Serialize(destination, source);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            using (var memStream = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(memStream);
            }
        }

        public T Deserialize<T>(Stream source) 
        {
            return Serializer.Deserialize<T>(source);
        }
    }
}