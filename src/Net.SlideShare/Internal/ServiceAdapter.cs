using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.SlideShare.Internal
{
	internal static class ServiceAdapter
	{
		public static string ComputeHash(string text)
		{
			using (var cryptoTransform = new SHA1Managed())
			{
				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
				return cryptoTransform.ComputeHash(buffer).ToHexString();
			}
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