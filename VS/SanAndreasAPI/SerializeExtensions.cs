using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SanAndreasAPI
{
    public static class SerializerExtensions
    {
        public static byte[] Serialize<T>(this T objectToWrite)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);

                return stream.GetBuffer();
            }
        }

        public static T Deserialize<T>(this byte[] arr)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                stream.Write(arr, 0, arr.Length);
                stream.Position = 0;

                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
}