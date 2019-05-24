using System.IO;

namespace Granite.Infrastructure.Common.Serialization
{
    public interface IProtoBufSerializer
    {
        byte[] Serialize<T>(T source);
        void Serialize<T>(Stream destination, T source);
        T Deserialize<T>(byte[] bytes);
        T Deserialize<T>(Stream source);
    }
}