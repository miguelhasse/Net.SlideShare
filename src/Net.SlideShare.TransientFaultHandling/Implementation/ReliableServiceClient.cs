using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Net.SlideShare.TransientFaultHandling.Implementation

{
    internal sealed class ReliableServiceClient : IReliableServiceClient, IDisposable
    {
        private ServiceClient client;

        private readonly Lazy<ISlideShareShow> slideshow;
        private readonly Lazy<ISlideShareUser> user;
        private readonly Lazy<ISlideShareFavorite> favorite;
        private readonly Lazy<ISlideShareMarketing> marketing;
        private readonly Lazy<ISlideShareMetadata> metadata;
        private readonly Lazy<ISlideShareSyndication> feeds;

        public ReliableServiceClient(ServiceClient client, RetryPolicy readRetryPolicy, RetryPolicy writeRetryPolicy)
        {
            this.client = client;

            this.slideshow = new Lazy<ISlideShareShow>(() => new SlideshowContext(this.client.Slideshow, readRetryPolicy, writeRetryPolicy));
            this.user = new Lazy<ISlideShareUser>(() => new SlideShareUser(this.client.User, readRetryPolicy));
            this.favorite = new Lazy<ISlideShareFavorite>(() => new SlideShareFavorite(this.client.Favorite, readRetryPolicy, writeRetryPolicy));
            this.marketing = new Lazy<ISlideShareMarketing>(() => new SlideShareMarketing(this.client.Marketing, readRetryPolicy));
            this.metadata = new Lazy<ISlideShareMetadata>(() => new SlideShareMetadata(ServiceClient.Metadata, readRetryPolicy));
            this.feeds = new Lazy<ISlideShareSyndication>(() => new SlideShareSyndication(ServiceClient.Feeds, readRetryPolicy));
        }

        public ISlideShareShow Slideshow
        {
            get { return slideshow.Value; }
        }

        public ISlideShareUser User
        {
            get { return user.Value; }
        }

        public ISlideShareFavorite Favorite
        {
            get { return favorite.Value; }
        }

        public ISlideShareMarketing Marketing
        {
            get { return marketing.Value; }
        }

        public ISlideShareMetadata Metadata
        {
            get { return metadata.Value; }
        }

        public ISlideShareSyndication Feeds
        {
            get { return feeds.Value; }
        }

        public void Dispose()
        {
            ServiceClient local = Interlocked.Exchange<ServiceClient>(ref client, default(ServiceClient));

            if (local != null)
            {
                try
                {
                    local.Dispose();
                }
                catch (Exception ex)
                {
#if PORTABLE
                    Debug.WriteLine("Exception on dispose: {0}", ex);
#else
                    Trace.TraceWarning("Exception on dispose: {0}", ex);
#endif
                }
            }
        }

#region Nested Classes

        private sealed class SlideshowContext : ISlideShareShow
        {
            private readonly ISlideShareShow client;
            private readonly RetryPolicy readRetryPolicy;
            private readonly RetryPolicy writeRetryPolicy;

            internal SlideshowContext(ISlideShareShow client, RetryPolicy readRetryPolicy, RetryPolicy writeRetryPolicy)
            {
                this.client = client;
                this.readRetryPolicy = readRetryPolicy;
                this.writeRetryPolicy = writeRetryPolicy;
            }

            public Task DeleteAsync(int slideshowId, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.DeleteAsync(slideshowId, cancellationToken), cancellationToken);
            }

            public Task EditAsync(int slideshowId, SlideshowDetailOptions options, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.EditAsync(slideshowId, options, cancellationToken), cancellationToken);
            }

            public Task<Slideshow> GetAsync(string slideshowUrl, bool excludeTags = false, bool includeTranscript = false, bool detailed = false, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetAsync(slideshowUrl, excludeTags, includeTranscript, detailed, cancellationToken), cancellationToken);
            }

            public Task<Slideshow> GetAsync(int slideshowId, bool excludeTags = false, bool includeTranscript = false, bool detailed = false, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetAsync(slideshowId, excludeTags, includeTranscript, detailed, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Slideshow>> GetByGroupAsync(string groupname, bool detailed = false, int? offset = default(int?), int? limit = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetByGroupAsync(groupname, detailed, offset, limit, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Slideshow>> GetByTagAsync(string tags, bool detailed = false, int? offset = default(int?), int? limit = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetByTagAsync(tags, detailed, offset, limit, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Slideshow>> GetByUserAsync(string username, bool includeUnconverted = false, bool detailed = false, int? offset = default(int?), int? limit = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetByUserAsync(username, includeUnconverted, detailed, offset, limit, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Slideshow>> SearchAsync(string query, SlideshowSearchOptions options, bool includeTranscript = false, bool detailed = false, int? offset = default(int?), int? limit = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.SearchAsync(query, options, includeTranscript, detailed, offset, limit, cancellationToken), cancellationToken);
            }

            public Task<int> UploadAsync(string filepath, SlideshowDetailOptions options, CancellationToken cancellationToken = default(CancellationToken))
            {
                return writeRetryPolicy.ExecuteAsync(() => client.UploadAsync(filepath, options, cancellationToken), cancellationToken);
            }
        }

        private sealed class SlideShareUser : ISlideShareUser
        {
            private readonly ISlideShareUser client;
            private readonly RetryPolicy readRetryPolicy;

            internal SlideShareUser(ISlideShareUser client, RetryPolicy readRetryPolicy)
            {
                this.client = client;
                this.readRetryPolicy = readRetryPolicy;
            }

            public Task<IEnumerable<Contact>> GetContactsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetContactsAsync(username, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Group>> GetGroupsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetGroupsAsync(username, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<string>> GetTagsAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetTagsAsync(cancellationToken), cancellationToken);
            }
        }

        private sealed class SlideShareFavorite : ISlideShareFavorite
        {
            private readonly ISlideShareFavorite client;
            private readonly RetryPolicy readRetryPolicy;
            private readonly RetryPolicy writeRetryPolicy;

            internal SlideShareFavorite(ISlideShareFavorite client, RetryPolicy readRetryPolicy, RetryPolicy writeRetryPolicy)
            {
                this.client = client;
                this.readRetryPolicy = readRetryPolicy;
                this.writeRetryPolicy = writeRetryPolicy;
            }

            public Task<bool> AddAsync(int slideshowId, CancellationToken cancellationToken = default(CancellationToken))
            {
                return writeRetryPolicy.ExecuteAsync(() => client.AddAsync(slideshowId, cancellationToken), cancellationToken);
            }

            public Task<bool> CheckAsync(int slideshowId, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.CheckAsync(slideshowId, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Favorite>> GetAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetAsync(username, cancellationToken), cancellationToken);
            }
        }

        private sealed class SlideShareMarketing : ISlideShareMarketing
        {
            private readonly ISlideShareMarketing client;
            private readonly RetryPolicy readRetryPolicy;

            internal SlideShareMarketing(ISlideShareMarketing client, RetryPolicy readRetryPolicy)
            {
                this.client = client;
                this.readRetryPolicy = readRetryPolicy;
            }

            public Task<IEnumerable<Lead>> GetCampaignLeadsAsync(string campaignId, DateTime? begin = default(DateTime?), DateTime? end = default(DateTime?), CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetCampaignLeadsAsync(campaignId, begin, end, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Campaign>> GetCampaignsAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetCampaignsAsync(cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<Lead>> GetLeadsAsync(DateTime? begin = default(DateTime?), DateTime? end = default(DateTime?), CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetLeadsAsync(begin, end, cancellationToken), cancellationToken);
            }
        }

        private sealed class SlideShareMetadata : ISlideShareMetadata
        {
            private readonly ISlideShareMetadata client;
            private readonly RetryPolicy readRetryPolicy;

            internal SlideShareMetadata(ISlideShareMetadata client, RetryPolicy readRetryPolicy)
            {
                this.client = client;
                this.readRetryPolicy = readRetryPolicy;
            }

            public Task<SlideshowEmbed> GetAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetAsync(url, cancellationToken), cancellationToken);
            }
        }

        private sealed class SlideShareSyndication : ISlideShareSyndication
        {
            private readonly ISlideShareSyndication client;
            private readonly RetryPolicy readRetryPolicy;

            internal SlideShareSyndication(ISlideShareSyndication client, RetryPolicy readRetryPolicy)
            {
                this.client = client;
                this.readRetryPolicy = readRetryPolicy;
            }

            public Task<IEnumerable<FeedItem>> GetAsync(SlideShareFeed feed, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetAsync(feed, cancellationToken), cancellationToken);
            }

            public Task<IEnumerable<FeedItem>> GetFeaturedAsync(SlideShareCategory category, CancellationToken cancellationToken = default(CancellationToken))
            {
                return readRetryPolicy.ExecuteAsync(() => client.GetFeaturedAsync(category, cancellationToken), cancellationToken);
            }
        }

#endregion
    }
}
