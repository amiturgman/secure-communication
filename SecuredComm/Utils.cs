using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SecuredCommunication
{
    /// <summary>
    /// Provides utility helper methods
    /// </summary>
    public static class Utils
    {
        public static byte[] ToByteArray<T>(T source)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }

        public static T FromByteArray<T>(byte[] data)
        {
            if ((data == null) || (data.Length == 0))
            {
                return default(T);
            }
             
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
    }
}
