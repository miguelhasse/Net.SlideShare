using System.Security.Cryptography;

namespace System.Net.SlideShare.Internal
{
	internal static class ServiceAdapter
	{
		public static string ComputeHash(string text)
		{
			var cryptoTransform = new SHA1Internal();
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
			cryptoTransform.HashCore(buffer, 0, buffer.Length);
			return cryptoTransform.HashFinal().ToHexString();
		}

		public static IStorageProvider DefaultStorageProvider()
		{
			return null;
		}
	}
}