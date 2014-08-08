using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace System.Net.SlideShare.Internal
{
	internal static class ServiceAdapter
	{
		public static string ComputeHash(string text)
		{
			var textBuffer = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
			var cryptoTransform = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
			var hash = cryptoTransform.CreateHash(textBuffer);	

			byte[] hashBuffer;
			CryptographicBuffer.CopyToByteArray(hash.GetValueAndReset(), out hashBuffer);	
			return hashBuffer.ToHexString();
		}

		public static IStorageProvider DefaultStorageProvider()
		{
			return null;
		}
	}
}