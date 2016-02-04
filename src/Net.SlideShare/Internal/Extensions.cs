using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace System.Net.SlideShare.Internal
{
	internal static class Extensions
	{
		static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long ToUnixTime(this DateTime source)
		{
			return Convert.ToInt64(Math.Floor((source - unixEpoch).TotalSeconds));
		}

		public static string Wrap(this string source, string wrapper)
		{
			return String.Concat(wrapper, source, wrapper);
		}

        public static object DefaultValue(this Enum source)
        {
#if NETFX_CORE
            var fi = source.GetType().GetTypeInfo().GetDeclaredField(source.ToString());
#elif PORTABLE
            var fi = source.GetType().GetRuntimeField(source.ToString());
#else
            var fi = source.GetType().GetField(source.ToString());
#endif
            DefaultValueAttribute[] attributes = (DefaultValueAttribute[])
                fi.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Value : source;
        }

		public static string ToString<T>(this IEnumerable<T> source, string separator)
		{
			return String.Join(separator, source);
		}

		public static string ToHexString(this byte[] source)
		{
			int b, length = source.Length;
			char[] c = new char[length * 2];

			for (int i = 0; i < length; i++)
			{
				b = source[i] >> 4;
				c[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));

				b = source[i] & 0xF;
				c[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
			}
			return new string(c);
		}
	}
}