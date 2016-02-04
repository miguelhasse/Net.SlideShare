using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace System.Net.SlideShare
{
	public static partial class Extensions
	{
		static readonly string[] DateTimeFormats = new string[]
		{ 
			"yyyy-MM-dd HH:mm:ss 'UTC'",
			CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern,
			CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern
		};
		static readonly Uri BaseUri = new UriBuilder().Uri;

		public static int Offset(this IEnumerable<Slideshow> source)
		{
			return (source is Slideshows) ? ((Slideshows)source).Offset : 0;
		}

		public static int TotalCount(this IEnumerable<Slideshow> source)
		{
			return (source is Slideshows) ? ((Slideshows)source).TotalCount : source.Count();
		}
		
		internal static bool AsBoolean(this XElement element, XName name)
		{
			element = element.Element(name);
			return Boolean.Parse(element.Value);
		}

		internal static decimal AsDecimal(this XElement element, XName name)
		{
			element = element.Element(name);
			return Decimal.Parse(element.Value, CultureInfo.InvariantCulture);
		}

		internal static int AsInteger(this XElement element, XName name)
		{
			element = element.Element(name);
			return Int32.Parse(element.Value, CultureInfo.InvariantCulture);
		}

		internal static string AsString(this XElement element, XName name)
		{
			element = element.Element(name);
			return (element == null) ? null : element.Value;
		}

		internal static Uri AsUri(this XElement element, XName name)
		{
			element = element.Element(name);
			if (element != null)
			{
				return Uri.IsWellFormedUriString(element.Value, UriKind.Absolute) ?
					new Uri(element.Value, UriKind.Absolute) : new Uri(BaseUri, element.Value);
			}
			return null;
		}

		internal static DateTime AsDateTime(this XElement element, XName name)
		{
			element = element.Element(name);
			return XmlConvert.ToDateTimeOffset(element.Value, DateTimeFormats).DateTime.ToUniversalTime();
		}

		internal static T AsEnum<T>(this XElement element, XName name) where T : struct
		{
			element = element.Element(name);
			return (T)Enum.Parse(typeof(T), element.Value, true);
		}
	}
}