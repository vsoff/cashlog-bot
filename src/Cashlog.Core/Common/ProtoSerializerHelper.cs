using ProtoBuf.Meta;

namespace Cashlog.Core.Common;

public static class ProtoSerializerHelper
{
    public static byte[] Serialize<T>(T data, RuntimeTypeModel typeModel)
    {
        using var stream = new MemoryStream();
        typeModel.Serialize(stream, data);
        return stream.ToArray();
    }

    public static T Deserialize<T>(byte[] bytes, RuntimeTypeModel typeModel)
    {
        using var stream = new MemoryStream(bytes);
        return (T)typeModel.Deserialize(stream, null, typeof(T));
    }

    public static object Deserialize(Type type, byte[] bytes, RuntimeTypeModel typeModel)
    {
        using var stream = new MemoryStream(bytes);
        return typeModel.Deserialize(stream, null, type);
    }
}