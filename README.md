## SlideShare Client for .NET ##

Implements asynchronous operations and includes support for portable class libraries.

### Usage samples ###

	static async Task FeaturedSample(CancellationToken cancellationToken)
	{
		using (var client = new ServiceClient("<ApiKey>", "<SharedSecret>", null))
		{
			foreach (var feeditem in await ServiceClient.Feeds.GetAsync(SlideShareFeed.Featured, cancellationToken).ConfigureAwait(false))
			{
				var ss = await client.Slideshow.GetAsync(feeditem.SlideshowUrl.AbsoluteUri, cancellationToken: cancellationToken).ConfigureAwait(false);
				var md = await ServiceClient.Metadata.GetAsync(ss.SlideshowUrl.AbsoluteUri, cancellationToken).ConfigureAwait(false);

				foreach (var uri in md)
				{
					Trace.TraceInformation(uri.AbsoluteUri);
				}
			}
		}
	}
	
	static async Task UploadSample(CancellationToken cancellationToken)
	{
		var credentials = new System.Net.NetworkCredential("<Username>", "<Password>");
	    using (var client = new ServiceClient("<ApiKey>", "<SharedSecret>", credentials, () => new FileSystemStorage()))
		{
			int id = await client.Slideshow.UploadAsync("slideshare_presentation.pptx",
        		new SlideshowDetailOptions { Title = "Sample Presentation" }, cancellationToken);
		}
	}

	public sealed class FileSystemStorage : IStorageProvider
	{
		public Task<System.IO.Stream> OpenAsync(string filepath, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => (System.IO.Stream)new System.IO.FileStream(filepath,
				System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None, 4096, true));
		}
	}
