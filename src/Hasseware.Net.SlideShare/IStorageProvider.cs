using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.SlideShare
{
	/// <summary>
	/// Local storage provider
	/// </summary>
	public interface IStorageProvider
	{
		Task<Stream> OpenAsync(string filepath, CancellationToken cancellationToken);
	}
}