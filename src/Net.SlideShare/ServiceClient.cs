using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.SlideShare.Internal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace System.Net.SlideShare
{
	public sealed class ServiceClient : IDisposable
	{
		private readonly Lazy<IStorageProvider> storage;
		private readonly ICredentials credentials;
		private readonly HttpClient client;
		private bool disposed = false;

		#region Constructors

		public ServiceClient(string apiKey, string sharedSecret, ICredentials credentials = null)
			: this(apiKey, sharedSecret, credentials, () => ServiceAdapter.DefaultStorageProvider())
		{
		}

		public ServiceClient(string apiKey, string sharedSecret, Func<IStorageProvider> storageFactory)
			: this(apiKey, sharedSecret, null, storageFactory)
		{
		}

		public ServiceClient(string apiKey, string sharedSecret, ICredentials credentials, Func<IStorageProvider> storageFactory)
		{
			this.client = new HttpClient(new SlideShareHandler(apiKey, sharedSecret));
			this.storage = new Lazy<IStorageProvider>(storageFactory);
			this.credentials = credentials;

			this.Slideshow = new SlideshowContext(this);
			this.User = new UserContext(this);
			this.Favorite = new FavoriteContext(this);
			this.Marketing = new MarketingContext(this);
		}

		static ServiceClient()
		{
			ServiceClient.Metadata = new MetadataContext();
			ServiceClient.Feeds = new FeedsContext();
		}

		#endregion

		public ISlideShareShow Slideshow { get; private set; }

		public ISlideShareUser User { get; private set; }

		public ISlideShareFavorite Favorite { get; private set; }

		public ISlideShareMarketing Marketing { get; private set; }

		public static ISlideShareMetadata Metadata { get; private set; }

		public static ISlideShareSyndication Feeds { get; private set; }

		#region Disposal Implementation
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		private void Dispose(bool disposing)
		{
			if(!this.disposed)
			{
				if (disposing) // dispose aggregated resources
					this.client.Dispose();
				this.disposed = true; // disposing has been done
			}
		}
		
		#endregion
		
		#region Request Handling Methods

		private Task<XElement> GetAsync(string cmd, IDictionary<string, object> parameters, bool includeCredentials, CancellationToken cancellationToken)
		{
			if (includeCredentials)
			{
				if (parameters == null) parameters = new Dictionary<string, object>();
				IncludeCredentials(parameters);
			}
			TaskCompletionSource<XElement> tcs = new TaskCompletionSource<XElement>();
			this.client.GetAsync(CreateRequestUri(cmd, parameters),
				HttpCompletionOption.ResponseHeadersRead, cancellationToken)
				.ContinueWith(t => HandleResponseCompletion(t, tcs));
			return tcs.Task;
		}

		private Task<XElement> PostAsync(string cmd, IDictionary<string, object> parameters, CancellationToken cancellationToken)
		{
			var content = new MultipartFormDataContent();
			IncludeCredentials(parameters);

			foreach (var param in parameters.Where(s => s.Value != null && !(s.Value is HttpContent)))
			{
				var value = ConvertParameterValue(param.Value, false);
				content.Add(new StringContent(value), param.Key.Wrap("\""));
			}
			foreach (var param in parameters.Where(s => s.Value is HttpContent))
			{
				content.Add((HttpContent)param.Value);
			}
			TaskCompletionSource<XElement> tcs = new TaskCompletionSource<XElement>();
			this.client.PostAsync(CreateRequestUri(cmd, null), content, cancellationToken)
				.ContinueWith(t => HandleResponseCompletion(t, tcs));
			return tcs.Task;
		}

		private static void HandleResponseCompletion(Task<HttpResponseMessage> task, TaskCompletionSource<XElement> tcs)
		{
			if (task.IsCanceled) tcs.TrySetCanceled();
			else if (task.IsFaulted) tcs.TrySetException(task.Exception);
			else if (task.IsCompleted)
			{
				task.Result.EnsureSuccessStatusCode();
				var root = XElement.Load(task.Result.Content.ReadAsStreamAsync().Result);
				
				ServiceRequestException ex;
				if (ExtractServiceError(root, out ex))
				{
					tcs.TrySetException(ex);
					return;
				}
				tcs.TrySetResult(root);
			}
		}

		private Task<HttpContent> BuildFileContent(string name, string filename, CancellationToken cancellationToken)
		{
			return this.storage.Value.OpenAsync(filename, cancellationToken).ContinueWith<HttpContent>(t =>
			{
				filename = System.IO.Path.GetFileName(filename).Wrap("\"");
				name = name.Wrap("\"");

				var content = new StreamContent(t.Result);
				content.Headers.ContentLength = t.Result.Length;
				content.Headers.ContentType = new MediaTypeHeaderValue(SlideShareMediaTypes.Default);
				content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
				{
					Name = name,
					FileName = filename,
					FileNameStar = filename
				};
				return content;
			}, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
		}
		
		private Uri CreateRequestUri(string cmd, IDictionary<string, object> parameters)
		{
			var builder = new UriBuilder(String.Concat("https://www.slideshare.net/api/2/", cmd));

			if (parameters != null)
			{
				string query = String.Join("&", parameters.Where(s => s.Value != null)
					.Select(s => String.Concat(s.Key, "=", ConvertParameterValue(s.Value, true))));
#if DEBUG
				System.Diagnostics.Debug.WriteLine(query);
#endif
				builder.Query = query;
			}
			return builder.Uri;
		}

		private static string ConvertParameterValue(object value, bool escapeStrings)
		{
			Type t = value.GetType();
			t = Nullable.GetUnderlyingType(t) ?? t;

#if NETFX_CORE || PORTABLE
			if (t == typeof(Enum)) return ((Enum)value).DefaultValue().ToString();
#else
			if (t.IsEnum) return ((Enum)value).DefaultValue().ToString();
#endif
			else if (t == typeof(DateTime)) return ((DateTime)value).ToString(CultureInfo.InvariantCulture);
			else if (t == typeof(int)) return ((int)value).ToString(CultureInfo.InvariantCulture);
			else if (t == typeof(bool)) return ((bool)value) ? "1" : "0";
			
			return escapeStrings ? Uri.EscapeDataString(value.ToString()) : value.ToString();
		}

		private void IncludeCredentials(IDictionary<string, object> parameters)
		{
			NetworkCredential credential = (this.credentials == null) ? null :
				this.credentials.GetCredential(new Uri("https://www.slideshare.net"), "Basic");

			if (credential != null)
			{
				parameters.Add("username", credential.UserName);
				parameters.Add("password", credential.Password);
			}
			else throw new InvalidOperationException("User credentials are required.");
		}

		private static bool ExtractServiceError(XElement root, out ServiceRequestException exception)
		{
			if (String.Equals(root.Name.LocalName, "SlideShareServiceError"))
			{
				var el = root.Element("Message");

                int statusCode;
                exception = (Int32.TryParse(el.Attribute("ID").Value, out statusCode)) ?
                   new ServiceRequestException(statusCode, el.Value) : new ServiceRequestException(-1, el.Value);

				return true;
			}
			exception = null;
			return false;
		}

		#endregion

		#region Nested Classes

		private sealed class SlideshowContext : ISlideShareShow
		{
			private ServiceClient client;

			internal SlideshowContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<Slideshow> GetAsync(int slideshowId, bool excludeTags, bool includeTranscript, bool detailed, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "slideshow_id", slideshowId }, { "exclude_tags", excludeTags },
					{ "detailed", detailed }, { "get_transcript", includeTranscript }
				};
				var response = await client.GetAsync("get_slideshow", parameters, false, cancellationToken).ConfigureAwait(false);
				return ToSlideshow(response, detailed);
			}

			public async Task<Slideshow> GetAsync(string slideshowUrl, bool excludeTags, bool includeTranscript, bool detailed, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "slideshow_url", slideshowUrl }, { "exclude_tags", excludeTags },
					{ "detailed", detailed }, { "get_transcript", includeTranscript }
				};
				var response = await client.GetAsync("get_slideshow", parameters, false, cancellationToken).ConfigureAwait(false);
				return ToSlideshow(response, detailed);
			}

			public async Task<IEnumerable<Slideshow>> GetByTagAsync(string tags, bool detailed, int? offset, int? limit, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "tags", tags }, { "detailed", detailed },
					{ "offset", offset }, { "limit", limit ?? (offset.HasValue ? 10 : (int?)null) }
				};
				var response = await client.GetAsync("get_slideshows_by_tag", parameters, false, cancellationToken).ConfigureAwait(false);
				return ToSlideshows(response, detailed);
			}

			public async Task<IEnumerable<Slideshow>> GetByGroupAsync(string groupname, bool detailed, int? offset, int? limit, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "group_name", groupname }, { "detailed", detailed },
					{ "offset", offset }, { "limit", limit ?? (offset.HasValue ? 10 : (int?)null) }
				};
				var response = await client.GetAsync("get_slideshows_by_group", parameters, false, cancellationToken).ConfigureAwait(false);
				return ToSlideshows(response, detailed);
			}

			public async Task<IEnumerable<Slideshow>> GetByUserAsync(string username, bool includeUnconverted, bool detailed, int? offset, int? limit, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "username_for", username }, { "detailed", detailed }, { "get_unconverted", includeUnconverted },
					{ "offset", offset }, { "limit", limit ?? (offset.HasValue ? 10 : (int?)null) }
				};
				var response = await client.GetAsync("get_slideshows_by_user", parameters, false, cancellationToken).ConfigureAwait(false);
				return ToSlideshows(response, detailed);
			}

			public async Task<IEnumerable<Slideshow>> SearchAsync(string query, SlideshowSearchOptions options, bool includeTranscript, bool detailed, int? offset, int? limit, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "q", query }, { "get_transcript", includeTranscript },
					{ "page", offset }, { "items_per_page", limit ?? (offset.HasValue ? 10 : (int?)null) }
				};
				if (options != null) options.WriteTo(parameters);
				var response = await client.GetAsync("search_slideshows", parameters, false, cancellationToken).ConfigureAwait(false);
				return ToSlideshows(response, detailed);
			}

			public async Task<int> UploadAsync(string filepath, SlideshowDetailOptions options, CancellationToken cancellationToken)
			{
				Uri uri;
				if (!Uri.TryCreate(filepath, UriKind.Absolute, out uri))
				{
#if !(NETFX_CORE || PORTABLE)
					if (!Uri.TryCreate(System.IO.Path.Combine(Environment.CurrentDirectory, filepath),
						UriKind.Absolute, out uri)) throw new ArgumentException("filepath");
#else
					throw new ArgumentException("filepath");
#endif
				}
				XElement response;
				var parameters = new Dictionary<string, object>();
				options.WriteTo(parameters);

#if !(NETFX_CORE || PORTABLE)
				if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
#else
				if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
#endif
				{
					parameters.Add("upload_url", uri.AbsolutePath);
					response = await client.GetAsync("upload_slideshow", parameters, true, cancellationToken).ConfigureAwait(false);
				}
				else
				{
					var content = await client.BuildFileContent("slideshow_srcfile", uri.LocalPath, cancellationToken).ConfigureAwait(false);
					parameters.Add(content.Headers.ContentDisposition.Name, content);
					response = await client.PostAsync("upload_slideshow", parameters, cancellationToken).ConfigureAwait(false);
				}
				return response.AsInteger("SlideShowID");
			}

			public async Task EditAsync(int slideshowId, SlideshowDetailOptions options, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>();
				client.IncludeCredentials(parameters);
				options.WriteTo(parameters);

				await client.GetAsync("edit_slideshow", parameters, true, cancellationToken).ConfigureAwait(false);
			}
			
			public async Task DeleteAsync(int slideshowId, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "slideshow_id", slideshowId } };
				client.IncludeCredentials(parameters);

				await client.GetAsync("delete_slideshow", parameters, true, cancellationToken).ConfigureAwait(false);
			}

			private static Slideshow ToSlideshow(XElement root, bool expectDetails)
			{
				return expectDetails ? new SlideshowDetailed(root) : new Slideshow(root);
			}

			private static Slideshows ToSlideshows(XElement root, bool expectDetails)
			{
				Func<XElement, Slideshow> selector = expectDetails ?
					new Func<XElement, Slideshow>(s => new SlideshowDetailed(s)) :
					new Func<XElement, Slideshow>(s => new Slideshow(s));

				return new Slideshows
				{
					Results = root.Descendants("Slideshow").Select(selector).ToList(),
					TotalCount = root.AsInteger("TotalResults")
				};
			}
		}

		private sealed class UserContext : ISlideShareUser
		{
			private ServiceClient client;

			internal UserContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<IEnumerable<Group>> GetGroupsAsync(string username, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "username_for", username } };
				var response = await client.GetAsync("get_user_groups", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.Descendants("Group").Select(s => new Group(s)).ToList();
			}

			public async Task<IEnumerable<Contact>> GetContactsAsync(string username, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "username_for", username } };
				client.IncludeCredentials(parameters);

				var response = await client.GetAsync("get_user_contacts", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.Descendants("Contact").Select(s => new Contact(s)).ToList();
			}

			public async Task<IEnumerable<string>> GetTagsAsync(CancellationToken cancellationToken)
			{
				var response = await client.GetAsync("get_user_tags", null, true, cancellationToken).ConfigureAwait(false);
				return response.Descendants("Tag").Select(s => s.ToString()).ToList();
			}
		}

		private sealed class FavoriteContext : ISlideShareFavorite
		{
			private ServiceClient client;

			internal FavoriteContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<IEnumerable<Favorite>> GetAsync(string username, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "username_for", username } };
				client.IncludeCredentials(parameters);

				var response = await client.GetAsync("get_user_favorites", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.Descendants("favorite").Select(s => new Favorite(s)).ToList();
			}

			public async Task<bool> AddAsync(int slideshowId, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "slideshow_id", slideshowId } };
				client.IncludeCredentials(parameters);

				var response = await client.GetAsync("add_favorite", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.AsInteger("SlideShowID") == slideshowId;
			}

			public async Task<bool> CheckAsync(int slideshowId, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "slideshow_id", slideshowId } };
				var response = await client.GetAsync("check_favorite", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.AsBoolean("Favorited");
			}
		}

		private sealed class MarketingContext : ISlideShareMarketing
		{
			private ServiceClient client;

			internal MarketingContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<IEnumerable<Campaign>> GetCampaignsAsync(CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>();
				client.IncludeCredentials(parameters);

				var response = await client.GetAsync("get_user_campaigns", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.Descendants("LeadCampaign").Select(s => (Campaign)new LeadCampaign(s)).Union(
					response.Descendants("PromotionCampaign").Select(s => new PromotionCampaign(s))).ToList();
			}

			public async Task<IEnumerable<Lead>> GetLeadsAsync(DateTime? begin, DateTime? end, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "begin", begin }, { "end", end } };
				client.IncludeCredentials(parameters);

				var response = await client.GetAsync("get_user_leads", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.Descendants("Lead").Select(s => new Lead(s)).ToList();
			}

			public async Task<IEnumerable<Lead>> GetCampaignLeadsAsync(string campaignId, DateTime? begin, DateTime? end, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "campaign_id", campaignId }, { "begin", begin }, { "end", end } };
				client.IncludeCredentials(parameters);

				var response = await client.GetAsync("get_user_campaign_leads", parameters, false, cancellationToken).ConfigureAwait(false);
				return response.Descendants("Lead").Select(s => new Lead(s)).ToList();
			}
		}

		private sealed class MetadataContext : ISlideShareMetadata
		{
			private readonly HttpClient client;

			internal MetadataContext()
			{
				this.client = new HttpClient(new HttpClientHandler());
			}

			public async Task<SlideshowEmbed> GetAsync(string url, CancellationToken cancellationToken)
			{
				var requestUrl = String.Format("http://www.slideshare.net/api/oembed/2?url={0}&format=xml", url);
				var response = await this.client.GetAsync(requestUrl, CancellationToken.None).ConfigureAwait(false);
				var content = response.EnsureSuccessStatusCode().Content;
				var root = XElement.Load(await content.ReadAsStreamAsync());
				return new SlideshowEmbed(root);
			}
		}

		private sealed class FeedsContext : ISlideShareSyndication
		{
			private HttpClient client;

			internal FeedsContext()
			{
				this.client = new HttpClient(new HttpClientHandler());
			}

			public async Task<IEnumerable<FeedItem>> GetAsync(SlideShareFeed feed, CancellationToken cancellationToken)
			{
				var requestUrl = CreateRequestUrl(feed, null);
				var response = await this.client.GetAsync(requestUrl, CancellationToken.None).ConfigureAwait(false);
				var content = response.EnsureSuccessStatusCode().Content;
				var root = XElement.Load(await content.ReadAsStreamAsync());
				return root.Descendants("item").Select(s => new FeedItem(s)).ToList();
			}

			public async Task<IEnumerable<FeedItem>> GetFeaturedAsync(SlideShareCategory category, CancellationToken cancellationToken)
			{
				var requestUrl = CreateRequestUrl(SlideShareFeed.Featured, category);
				var response = await this.client.GetAsync(requestUrl, CancellationToken.None).ConfigureAwait(false);
				var content = response.EnsureSuccessStatusCode().Content;
				var root = XElement.Load(await content.ReadAsStreamAsync());
				return root.Descendants("item").Select(s => new FeedItem(s)).ToList();
			}

			private static string CreateRequestUrl(SlideShareFeed feed, SlideShareCategory? category)
			{
				var requestBuilder = new System.Text.StringBuilder("http://www.slideshare.net/rss/");
				requestBuilder.Append(feed.DefaultValue());

				if (category.HasValue)
				{
					requestBuilder.Append("/category/");
					requestBuilder.Append(category.Value.DefaultValue());
				}
				return requestBuilder.ToString();
			}
		}

		#endregion
	}

	public sealed class ServiceRequestException : Exception
	{
		internal ServiceRequestException(int statusCode, string message) : base(message)
		{
			this.HResult = statusCode;
            try { this.Status = (ServiceExceptionStatus)Enum.ToObject(typeof(ServiceExceptionStatus), statusCode); }
            catch (ArgumentException) { this.Status = ServiceExceptionStatus.Unknown; }
		}

        public ServiceExceptionStatus Status { get; private set; }
    }

    public enum ServiceExceptionStatus
    {
        Unknown = -1,
        MissingApiKey = 0,
        FailedApiValidation = 1,
        FailedUserAuthentication = 2,
        MissingTitle = 3,
        MissingUploadFile = 4,
        BlankTitle = 5,
        InvalidFileSource = 6,
        InvalidExtension = 7,
        FileSizeExceeded = 8,
        SlideShowNotFound = 9,
        UserNotFound = 10,
        GroupNotFound = 11,
        MissingTag = 12,
        TagNotFound = 13,
        MissingParameter  = 14,
        MissingSearchQuery = 15,
        InsufficientPermissions = 16,
        InvalidParameters = 17,
        AccountAlreadyLinked = 70,
        LinkedAccountNotFound = 71,
        UserDoesNotExist = 72,
        InvalidApplicationID = 73,
        LoginAlreadyExists = 74,
        EmailAlreadyExists = 75,
        AccountUpgradeRequired = 97,
        AccountLimitExceeded = 99,
        AccountBlocked = 100
    }
}
