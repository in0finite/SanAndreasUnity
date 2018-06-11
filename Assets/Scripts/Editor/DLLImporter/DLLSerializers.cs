using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DLLSerializers
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
            stream.WriteAsync(arr, 0, arr.Length);
            stream.Position = 0;

            return (T)binaryFormatter.Deserialize(stream);
        }
    }

    /// <summary>
    /// Serializes to file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">The path.</param>
    /// <param name="value">The value.</param>
    /// <param name="pretty">if set to <c>true</c> [pretty].</param>
    public static void SerializeToFile<T>(this T value, string path)
    {
        File.WriteAllBytes(path, Serialize(value));
    }

    /// <summary>
    /// Deserializes from file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">The path.</param>
    /// <returns>T.</returns>
    public static T DeserializeFromFile<T>(this string path)
    {
        return Deserialize<T>(File.ReadAllBytes(path));
    }

    public static string JsonSerialize<T>(this T objectToWrite, bool pretty)
    {
        return JsonConvert.SerializeObject(objectToWrite, pretty ? Formatting.Indented : Formatting.None);
    }

    public static T JsonDeserialize<T>(this string jsonStr)
    {
        return (T)JsonConvert.DeserializeObject(jsonStr);
    }

    /// <summary>
    /// Serializes to file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">The path.</param>
    /// <param name="value">The value.</param>
    /// <param name="pretty">if set to <c>true</c> [pretty].</param>
    public static void JsonSerializeToFile<T>(this T value, string path, bool pretty = true)
    {
        File.WriteAllText(path, JsonSerialize(value, pretty));
    }

    /// <summary>
    /// Deserializes from file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">The path.</param>
    /// <returns>T.</returns>
    public static T JsonDeserializeFromFile<T>(this string path)
    {
        return JsonDeserialize<T>(File.ReadAllText(path));
    }

    /// <summary>
    /// Determines whether the specified input is json.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns><c>true</c> if the specified input is json; otherwise, <c>false</c>.</returns>
    public static bool IsJson(this string input)
    {
        input = input.Trim();
        return input.StartsWith("{") && input.EndsWith("}")
               || input.StartsWith("[") && input.EndsWith("]");
    }
}