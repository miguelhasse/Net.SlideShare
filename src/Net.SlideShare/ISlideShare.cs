using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.SlideShare
{
	public interface ISlideShareShow
	{
		/// <summary>
		/// Returns slideshow by ID.
		/// </summary>
		Task<Slideshow> GetAsync(int slideshowId, bool excludeTags = false, bool includeTranscript = false, bool detailed = false, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns slideshow by URL.
		/// </summary>
		Task<Slideshow> GetAsync(string slideshowUrl, bool excludeTags = false, bool includeTranscript = false, bool detailed = false, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns slideshows that contain the specified tag.
		/// </summary>
		Task<IEnumerable<Slideshow>> GetByTagAsync(string tags, bool detailed = false, int? offset = null, int? limit = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns slideshows that are a part of the specified group.
		/// </summary>
		Task<IEnumerable<Slideshow>> GetByGroupAsync(string groupname, bool detailed = false, int? offset = null, int? limit = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns user slideshows.
		/// </summary>
		Task<IEnumerable<Slideshow>> GetByUserAsync(string username, bool includeUnconverted = false, bool detailed = false, int? offset = null, int? limit = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns slideshows according to the search criteria.
		/// </summary>
		Task<IEnumerable<Slideshow>> SearchAsync(string query, SlideshowSearchOptions options, bool includeTranscript = false, bool detailed = false, int? offset = null, int? limit = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Upload a slideshow.
		/// </summary>
		Task<int> UploadAsync(string filepath, SlideshowDetailOptions options, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Edit a slideshow.
		/// </summary>
		Task EditAsync(int slideshowId, SlideshowDetailOptions options, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Delete a slideshow.
		/// </summary>
		Task DeleteAsync(int slideshowId, CancellationToken cancellationToken = default(CancellationToken));
	}

	public interface ISlideShareUser
	{
		/// <summary>
		/// Returns user groups.
		/// </summary>
		Task<IEnumerable<Group>> GetGroupsAsync(string username, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns user contacts.
		/// </summary>
		Task<IEnumerable<Contact>> GetContactsAsync(string username, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns user tags.
		/// </summary>
		Task<IEnumerable<string>> GetTagsAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
	
	public interface ISlideShareFavorite
	{
		/// <summary>
		/// Returns user favorites.
		/// </summary>
		Task<IEnumerable<Favorite>> GetAsync(string username, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Favorites slideshow (identified by slideshow_id).
		/// </summary>
		Task<bool> AddAsync(int slideshowId, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Check user favorites.
		/// </summary>
		Task<bool> CheckAsync(int slideshowId, CancellationToken cancellationToken = default(CancellationToken));
	}
	
	public interface ISlideShareMarketing
	{
		/// <summary>
		/// Get user campaigns.
		/// </summary>
		Task<IEnumerable<Campaign>> GetCampaignsAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Get user leads.
		/// </summary>
		Task<IEnumerable<Lead>> GetLeadsAsync(DateTime? begin = null, DateTime? end = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Get user campaign leads.
		/// </summary>
		Task<IEnumerable<Lead>> GetCampaignLeadsAsync(string campaignId, DateTime? begin = null, DateTime? end = null, CancellationToken cancellationToken = default(CancellationToken));
	}
	
	public interface ISlideShareMetadata // Embeddable Media
	{
		Task<SlideshowEmbed> GetAsync(string url, CancellationToken cancellationToken = default(CancellationToken));
	}
	
	public interface ISlideShareSyndication // Feeds
	{
		Task<IEnumerable<FeedItem>> GetAsync(SlideShareFeed feed, CancellationToken cancellationToken = default(CancellationToken));

		Task<IEnumerable<FeedItem>> GetFeaturedAsync(SlideShareCategory category, CancellationToken cancellationToken = default(CancellationToken));
	}

	public sealed class SlideshowSearchOptions
	{
		public SlideshowLanguage Language { get; set; }
		
		public SlideshowSortOrder SortOrder { get; set; }
		
		public SlideshowTimePeriod FilterPeriod { get; set; }
		
		public SlideshowFileFormat FilterFileFormat { get; set; }
		
		public SlideshowFileType FilterFileType { get; set; }
		
		public bool SearchTags { get; set; }
		
		public bool DownloadAvailable { get; set; }
		
		public bool UnderLicense { get; set; }
		
		public bool AdaptionLicense { get; set; }
		
		public bool CommercialLicense { get; set; }
		
		internal void WriteTo(IDictionary<string, object> parameters)
		{
			if (Language != SlideshowLanguage.English)
				parameters.Add("lang", this.Language);

			if (SortOrder != SlideshowSortOrder.Relevance)
				parameters.Add("sort", this.SortOrder);

			if (FilterPeriod != SlideshowTimePeriod.Any)
				parameters.Add("upload_date", this.FilterPeriod.ToString().ToLower());

			if (FilterFileFormat != SlideshowFileFormat.All)
				parameters.Add("fileformat", this.FilterFileFormat);

			if (FilterFileType != SlideshowFileType.All)
				parameters.Add("file_type", this.FilterFileType);

			if (SearchTags)
				parameters.Add("what", "tag");
				
			if (DownloadAvailable)
				parameters.Add("download", 1);

			if (UnderLicense)
			{
				parameters.Add("cc", 1);

				if (AdaptionLicense)
					parameters.Add("cc_adapt", 1);

				if (CommercialLicense)
					parameters.Add("cc_commercial", 1);
			}
		}
	}

	public sealed class SlideshowDetailOptions
	{
		public string Title { get; set; }
		
		public string Description { get; set; }
		
		public string Tags { get; set; }

		public bool AllowDownload { get; set; }

		public bool MakePrivate { get; set; }
		
		public bool GenerateSecretUrl { get; set; }
		
		public bool AllowEmbeds { get; set; }
		
		public bool ShareWithContacts { get; set; }
		
		internal void WriteTo(IDictionary<string, object> parameters)
		{
            parameters.Add("slideshow_title", this.Title);
            parameters.Add("slideshow_description", this.Description);
            parameters.Add("slideshow_tags", this.Tags);

            if (MakePrivate)
                parameters.Add("make_src_public", "Y");

            if (AllowEmbeds)
                parameters.Add("allow_embeds", "Y");

            if (ShareWithContacts)
                parameters.Add("share_with_contacts", "Y");

            if (AllowDownload)
            {
                parameters.Add("make_slideshow_private", "Y");

                if (GenerateSecretUrl)
                    parameters.Add("generate_secret_url", "Y");
            }
        }
	}
	
	public enum SlideshowSortOrder
	{
        [DefaultValue("relevance")]
        Relevance,
        [DefaultValue("mostviewed")]
        MostViewed,
        [DefaultValue("mostdownloaded")]
        MostDownloaded,
        [DefaultValue("latest")]
        Latest
	}

	public enum SlideshowLanguage
	{
        [DefaultValue("**")]
		All,
        [DefaultValue("en")]
        English,
        [DefaultValue("es")]
        Spanish,
        [DefaultValue("pt")]
        Portuguese,
        [DefaultValue("fr")]
        French,
        [DefaultValue("it")]
        Italian,
        [DefaultValue("nl")]
        Dutch,
        [DefaultValue("de")]
        German,
        [DefaultValue("zh")]
        Chinese,
        [DefaultValue("jp")]
        Japanese,
        [DefaultValue("ko")]
        Korean,
        [DefaultValue("ro")]
        Romanian,
        [DefaultValue("!!")]
        Other
	}

	public enum SlideshowTimePeriod
	{
        [DefaultValue("any")]
        Any,
        [DefaultValue("week")]
        Week,
        [DefaultValue("month")]
        Month,
        [DefaultValue("year")]
        Year
	}

	public enum SlideshowFileFormat
	{
        [DefaultValue("all")]
        All,
        [DefaultValue("pdf")]
        Acrobat,
        [DefaultValue("odp")]
        PowerPoint,
        [DefaultValue("ppt")]
        OpenOffice,
        [DefaultValue("pps")]
        Slideshow,
        [DefaultValue("pot")]
        Template
	}

	public enum SlideshowFileType
	{
		All,
        Presentations,
        Documents,
        Webinars,
        Videos
	}

	public enum SlideShareFeed
	{
		[DefaultValue("featured")]
        Featured,
		[DefaultValue("popular")]
        Popular,
		[DefaultValue("downloaded")]
        Downloaded,
		[DefaultValue("favorited")]
        Liked,
		[DefaultValue("videos")]
        Videos,
		[DefaultValue("latest")]
        Latest,
		[DefaultValue("facebooked")]
        Facebook,
		[DefaultValue("linked_in")]
        LinkedIn
	}

	public enum SlideShareCategory
	{
        [DefaultValue("data-analytics")]
        Analytics,
        [DefaultValue("automotive")]
        Automotive,
        [DefaultValue("business")]
        Business,
        [DefaultValue("career")]
        Career,
        [DefaultValue("design")]
        Design,
        [DefaultValue("economy-finance")]
        Economy,
        [DefaultValue("education")]
        Education,
        [DefaultValue("engineering")]
        Engineering,
        [DefaultValue("entertainment-humor")]
        Entertainment,
        [DefaultValue("small-business-entrepreneurship")]
        Entrepreneurship,
        [DefaultValue("environment")]
        Environment,
        [DefaultValue("food")]
        Food,
        [DefaultValue("government-nonprofit")]
        Government,
        [DefaultValue("devices-hardware")]
        Hardware,
        [DefaultValue("healthcare")]
        Healthcare,
        [DefaultValue("internet")]
        Internet,
        [DefaultValue("investor-relations")]
        InvestorRelations,
        [DefaultValue("law")]
        Law,
        [DefaultValue("leadership-management")]
        Leadership,
        [DefaultValue("lifestyle")]
        Lifestyle,
        [DefaultValue("marketing")]
        Marketing,
        [DefaultValue("health-medicine")]
        Medicine,
        [DefaultValue("mobile")]
        Mobile,
        [DefaultValue("art-photos")]
        Photos,
        [DefaultValue("news-politics")]
        Politics,
        [DefaultValue("presentations-public-speaking")]
        Presentations,
        [DefaultValue("real-estate")]
        RealEstate,
        [DefaultValue("recruiting-hr")]
        Recruiting,
        [DefaultValue("retail")]
        Retail,
        [DefaultValue("sales")]
        Sales,
        [DefaultValue("science")]
        Science,
        [DefaultValue("self-improvement")]
        SelfImprovement,
        [DefaultValue("services")]
        services,
        [DefaultValue("social-media")]
        SocialMedia,
        [DefaultValue("software")]
        Software,
        [DefaultValue("spiritual")]
        Spiritual,
        [DefaultValue("sports")]
        Sports,
        [DefaultValue("technology")]
        Technology,
        [DefaultValue("travel")]
        Travel
	}
}
