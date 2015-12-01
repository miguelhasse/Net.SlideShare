using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace System.Net.SlideShare.Internal
{
	internal static class ServiceAdapter
	{
		public static string ComputeHash(string text)
		{
            var buffer = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
            var hashProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            return CryptographicBuffer.EncodeToHexString(hashProvider.HashData(buffer));
		}

		public static IStorageProvider DefaultStorageProvider()
		{
			return new FileSystemStorage();
		}

	    private sealed class FileSystemStorage : IStorageProvider
		{
			public Task<System.IO.Stream> OpenAsync(string filepath, CancellationToken cancellationToken)
			{
				return Task.Factory.StartNew(() => (System.IO.Stream)new System.IO.FileStream(filepath,
					System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None, 4096, true));
			}
		}
	}
}