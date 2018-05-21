using Cadenza.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace SanAndreasAPI
{
    public class MinMax<T>
    {
        public T min, max;

        private MinMax()
        {
        }

        public MinMax(T mn, T mx)
        {
            min = mn;
            max = mx;
        }
    }

    public static class ArrayExtensions
    {
        public static IEnumerable<T> JoinMultipleArray<T>(this IEnumerable<T[]> array)
        {
            foreach (var el in array)
                for (int i = 0; i < el.Length; ++i)
                    yield return el[i];
        }
    }

    public enum ImageFormats
    {
        JPG,
        PNG,
        BMP,
        TIFF,
        GIF,
        ICO
    }

    /*public static class ImageExtensions
    {
        public static Image GetCompressedBitmap(this Bitmap bmp, ImageFormats imageFormats = ImageFormats.PNG, long quality = 100L) //[0-100]
        {
            using (MemoryStream mss = new MemoryStream())
            {
                EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);
                ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(o => o.FormatID == GetFormatGuidFromEnum(imageFormats));
                EncoderParameters parameters = new EncoderParameters(1);
                parameters.Param[0] = qualityParam;
                bmp.Save(mss, imageCodec, parameters);
                return Image.FromStream(mss);
            }
        }

        private static Guid GetFormatGuidFromEnum(ImageFormats format)
        {
            switch (format)
            {
                case ImageFormats.JPG:
                    return ImageFormat.Jpeg.Guid;

                case ImageFormats.PNG:
                    return ImageFormat.Png.Guid;

                case ImageFormats.BMP:
                    return ImageFormat.Bmp.Guid;

                case ImageFormats.GIF:
                    return ImageFormat.Gif.Guid;

                case ImageFormats.TIFF:
                    return ImageFormat.Tiff.Guid;

                case ImageFormats.ICO:
                    return ImageFormat.Icon.Guid;
            }
            return default(Guid);
        }
    }*/

    public static class SocketExtensions
    {
        public static IPAddress GetLocalIPAddress()
        { //Get my Local IP (192.168.x.x)
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static int GetObjectSize(this object TestObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] Array;
                bf.Serialize(ms, TestObject);
                Array = ms.ToArray();
                return Array.Length;
            }
        }

        public static bool FindFirstMissingNumberFromSequence<T>(this IEnumerable<T> arr, out T n, MinMax<T> mnmx = null)
        {
            //Dupe

            if (!arr.GetItemType().IsNumericType())
            {
                Console.WriteLine("Type '{0}' can't be used as a numeric type!", typeof(T).Name);
                n = default(T);
                return false;
            }

            if (mnmx != null)
            {
                arr.Add(mnmx.min);
                arr.Add(mnmx.max);
            }

            IOrderedEnumerable<T> list = arr.OrderBy(x => x);

            //End dupe

            bool b = false;
            n = default(T);

            foreach (object num in list)
            {
                b = ExpressionMath<T>.Default.GreaterThan(ExpressionMath<T>.Default.Subtract((T)num, n), 1.ConvertGeneric<T>());
                if (b)
                    break;
                else
                    n = (T)num;
            }

            n = ExpressionMath<T>.Default.Add(n, 1.ConvertGeneric<T>());

            return b;
        }

        public static IEnumerable<T> FindMissingNumbersFromSequence<T>(IEnumerable<T> arr, MinMax<T> mnmx = null) where T : struct
        {
            if (!arr.GetItemType().IsNumericType())
            {
                Console.WriteLine("Type '{0}' can't be used as a numeric type!", typeof(T).Name);
                yield break;
            }

            if (mnmx != null)
            {
                arr.Add(mnmx.min);
                arr.Add(mnmx.max);
            }

            IOrderedEnumerable<T> list = arr.OrderBy(x => x);
            T n = default(T);

            foreach (object num in list)
            {
                T op = (T)ExpressionMath<object>.Default.Subtract(num, n);
                if (ExpressionMath<T>.Default.GreaterThan(op, 1.ConvertGeneric<T>()))
                {
                    int max = op.ConvertValue<int>();
                    for (int l = 1; l < max; ++l)
                        yield return ExpressionMath<T>.Default.Add(n, l.ConvertValue<T>());
                }
                n = (T)num;
            }
        }

        public static Type GetItemType<T>(this IEnumerable<T> enumerable)
        {
            return typeof(T);
        }

        public static T ConvertGeneric<T>(this object o)
        {
            return (T)o;
        }

        //This is necessary because we cannot cast (using C# expression (Type)var, or var as Type) => Errors: CS0077, CS0030, CS0413
        public static T ConvertValue<T>(this object o) where T : struct
        {
            return (T)Convert.ChangeType(o, typeof(T));
        }

        public static bool IsNumericType<T>(this T o)
        {
            return typeof(T).IsNumericType();
        }

        public static bool IsNumericType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;

                default:
                    return false;
            }
        }

        public static IEnumerable<T> Add<T>(this IEnumerable<T> e, T value)
        {
            foreach (var cur in e)
            {
                yield return cur;
            }
            yield return value;
        }
    }
}