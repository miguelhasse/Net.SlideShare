using System.Net.Http;
using System.Net.SlideShare.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.SlideShare
{
	public sealed class SlideShareHandler : DelegatingHandler
	{
		static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly string apiKey, sharedSecret;

		public SlideShareHandler(string apiKey, string sharedSecret)
			: this(new HttpClientHandler(), apiKey, sharedSecret)
		{
		}

		public SlideShareHandler(HttpClientHandler innerHandler, string apiKey, string sharedSecret)
			: base(innerHandler)
		{
			if (innerHandler != null)
			{
				innerHandler.AllowAutoRedirect = false;
				innerHandler.UseCookies = false;
			}
			this.apiKey = apiKey;
			this.sharedSecret = sharedSecret;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string credentials = CreateValidationParameters(this.apiKey, this.sharedSecret);

			var builder = new UriBuilder(request.RequestUri);
			builder.Query = (builder.Query.Length == 0) ? credentials :
				String.Join("&", credentials, builder.Query.Trim('?', '&'));
			request.RequestUri = builder.Uri;

			return base.SendAsync(request, cancellationToken);
		}

		private static string CreateValidationParameters(string apiKey, string sharedSecret)
		{
			string timestamp = DateTime.UtcNow.ToUnixTime().ToString();
			var hash = ServiceAdapter.ComputeHash(String.Concat(sharedSecret, timestamp));
			return String.Concat("api_key=", apiKey, "&hash=", hash, "&ts=", timestamp);
		}
	}
}
