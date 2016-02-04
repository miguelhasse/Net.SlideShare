using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace System.Net.SlideShare
{
	public class Slideshow
	{
		internal Slideshow(XElement root)
		{
			this.ID = root.AsString("ID");
			this.Title = root.AsString("Title");
			this.Description = root.AsString("Description");
			this.Status = root.AsEnum<SlideshowStatus>("Status");
			this.Username = root.AsString("Username");
			this.SlideshowUrl = root.AsUri("URL");
			this.ThumbnailUrl = root.AsUri("ThumbnailURL");
			this.ThumbnailSmallUrl = root.AsUri("ThumbnailSmallURL");
			this.Created = root.AsDateTime("Created");
			this.Updated = root.AsDateTime("Updated");
			this.Language = root.AsString("Language");
			this.Format = root.AsString("Format");
			this.DownloadAvailable = root.AsInteger("Download") > 0;
			this.DownloadUrl = root.AsUri("DownloadURL");
			this.SlideshowType = root.AsEnum<SlideshowType>("SlideshowType");
			this.InContest = root.AsInteger("InContest") > 0;
		}

		public string ID { get; private set; }

		public string Title { get; private set; }

		public string Description { get; private set; }

		public SlideshowStatus Status { get; private set; }

		public string Username { get; private set; }

		public Uri SlideshowUrl { get; private set; }

		public Uri ThumbnailUrl { get; private set; }

		public Uri ThumbnailSmallUrl { get; private set; }

		public DateTime Created { get; private set; }

		public DateTime Updated { get; private set; }

		public string Language { get; private set; }

		public string Format { get; private set; }

		public bool DownloadAvailable { get; private set; }

		public Uri DownloadUrl { get; private set; }

		public SlideshowType SlideshowType { get; private set; }

		public bool InContest { get; private set; }
	}
	
	public class SlideshowDetailed : Slideshow
	{
		internal SlideshowDetailed(XElement root) : base(root)
		{
			this.UserID = root.AsInteger("UserID");
			this.PresentationLocation = root.AsString("PPTLocation");
			this.StrippedTitle = root.AsString("StrippedTitle");
			this.Transcript = root.AsString("Transcript");
			this.Tags = root.Element("Tags").Elements("Tag")
				.Select(s => s.Value).ToArray();
			this.Audio = root.AsInteger("Audio");
			this.DownloadCount = root.AsInteger("NumDownloads");
			this.ViewCount = root.AsInteger("NumViews");
			this.CommentCount = root.AsInteger("NumComments");
			this.FavoriteCount = root.AsInteger("NumFavorites");
			this.SlideCount = root.AsInteger("NumSlides");
			this.RelatedSlideshows = root.Element("RelatedSlideshows").Elements("RelatedSlideshowID")
				.Select(s => s.Value).ToArray();
			this.PrivacyLevel = root.AsInteger("PrivacyLevel");
			this.FlagVisible = root.AsInteger("FlagVisible") > 0;
			this.ShowOnSlideShare = root.AsInteger("ShowOnSS") > 0;
			this.SecretURL = root.AsInteger("SecretURL") > 0;
			this.AllowEmbed = root.AsInteger("AllowEmbed") > 0;
			this.ShareWithContacts = root.AsInteger("ShareWithContacts") > 0;
		}

		public int UserID { get; private set; }

		public string PresentationLocation { get; private set; }

		public string StrippedTitle { get; private set; }

		public string Transcript { get; private set; }

		public string[] Tags { get; private set; }

		public int Audio { get; private set; }

		public int DownloadCount { get; private set; }

		public int ViewCount { get; private set; }

		public int CommentCount { get; private set; }

		public int FavoriteCount { get; private set; }

		public int SlideCount { get; private set; }

		public string[] RelatedSlideshows { get; private set; }

		public int PrivacyLevel { get; private set; }

		public bool FlagVisible { get; private set; }

		public bool ShowOnSlideShare { get; private set; }

		public bool SecretURL { get; private set; }

		public bool AllowEmbed { get; private set; }

		public bool ShareWithContacts { get; private set; }
	}

	internal class Slideshows : IEnumerable<Slideshow>
	{
		internal IEnumerable<Slideshow> Results { get; set; }

		public int Offset { get; internal set; }

		public int TotalCount { get; internal set; }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)Results).GetEnumerator();
		}

		public IEnumerator<Slideshow> GetEnumerator()
		{
			return Results.GetEnumerator();
		}
	}

	public class Contact
	{
		internal Contact(XElement root)
		{
			this.Username = root.AsString("Username");
			this.SlideshowCount = root.AsInteger("NumSlideshows");
			this.CommentCount = root.AsInteger("NumComments");
		}

		public string Username { get; private set; }

		public int SlideshowCount { get; private set; }

		public int CommentCount { get; private set; }
	}

	public class Group
	{
		internal Group(XElement root)
		{
			this.Name = root.AsString("Name");
			this.PostCount = root.AsInteger("NumPosts");
			this.SlideshowCount = root.AsInteger("NumSlideshows");
			this.MemberCount = root.AsInteger("NumMembers");
			this.QueryName = root.AsString("QueryName");
			this.Created = root.AsDateTime("Created");
			this.GroupUrl = root.AsUri("URL");
		}

		public string Name { get; private set; }

		public int PostCount { get; private set; }

		public int SlideshowCount { get; private set; }

		public int MemberCount { get; private set; }

		public string QueryName { get; private set; }

		public DateTime Created { get; private set; }

		public Uri GroupUrl { get; private set; }
	}

	public class Favorite
	{
		internal Favorite(XElement root)
		{
			this.ID = root.AsInteger("slideshow_id");
			this.Tags = root.AsString("tag_text");
		}

		public int ID { get; private set; }

		public string Tags { get; private set; }
	}

	public class Lead
	{
		internal Lead(XElement root)
		{
			this.SlideshowID = root.AsInteger("SlideshowId");
			this.LeadCampaignID = root.AsInteger("LeadCampaignId");
			this.FirstName = root.AsString("FirstName");
			this.LastName = root.AsString("LastName");
			this.Email = root.AsString("Email");
			this.PhoneNo = root.AsString("PhoneNo");
			this.Address = root.AsString("Address");
			this.City = root.AsString("City");
			this.Country = root.AsString("Country");
			this.Zipcode = root.AsString("Zipcode");
			this.State = root.AsString("State");
			this.GeoData = new GeoData(root.Element("GeoData"));
			this.UserComment = root.AsString("UserComment");
			this.PaidAt = root.AsDateTime("PaidAt");
			this.ReadAt = root.AsDateTime("ReadAt");
			this.DeletedAt = root.AsDateTime("DeletedAt");
			this.CreatedAt = root.AsDateTime("CreatedAt");
			this.UpdatedAt = root.AsDateTime("UpdatedAt");
			this.Rating = root.AsInteger("Rating");
			this.Message = root.AsString("Message");
			this.Mechanism = root.AsString("Mechanism");
			this.Cost = root.AsDecimal("Cost");
		}

		public int SlideshowID { get; private set; }
		
		public int LeadCampaignID { get; private set; }

		public string FirstName { get; private set; }

		public string LastName { get; private set; }
		
		public string Email { get; private set; }
		
		public string PhoneNo { get; private set; }
		
		public string Address { get; private set; }
		
		public string City { get; private set; }
		
		public string Country { get; private set; }
		
		public string Zipcode { get; private set; }
		
		public string State { get; private set; }
		
		public GeoData GeoData { get; private set; }
		
		public string UserComment { get; private set; }
		
		public DateTime PaidAt { get; private set; }
		
		public DateTime ReadAt { get; private set; }
		
		public DateTime DeletedAt { get; private set; }
		
		public DateTime CreatedAt { get; private set; }
		
		public DateTime UpdatedAt { get; private set; }
		
		public int Rating { get; private set; }
		
		public string Message { get; private set; }
		
		public string Mechanism { get; private set; }
		
		public decimal Cost { get; private set; }
	}
		
	public class Campaign
	{
		internal Campaign(XElement root)
		{
			this.ID = root.AsInteger("Id");
			this.Name = root.AsString("Name");
			this.SlideshowID = root.AsInteger("SlideshowId");
			this.TargetedRegion = root.AsString("TargetedRegion");
			this.ForTagged = root.AsString("ForTagged");
			this.Started = root.AsDateTime("StartedAt");
			this.Paused = root.AsDateTime("PausedAt");
			this.Created = root.AsDateTime("CreatedAt");
			this.Updated = root.AsDateTime("UpdatedAt");
			this.Ended = root.AsDateTime("EndedAt");
			this.DailySpend = root.AsDecimal("MaximumDailySpend");
			this.State = root.AsString("StateOfCampaign");
		}

		public int ID { get; private set; }

		public string Name { get; private set; }

		public int SlideshowID { get; private set; }

		public string TargetedRegion { get; private set; }

		public string ForTagged { get; private set; }

		public DateTime Started { get; private set; }

		public DateTime Paused { get; private set; }

		public DateTime Created { get; private set; }

		public DateTime Updated { get; private set; }

		public DateTime Ended { get; private set; }
		
		public decimal DailySpend { get; private set; }
		
		public string State { get; private set; }
	}

	public class LeadCampaign : Campaign
	{
		internal LeadCampaign(XElement root) : base(root)
		{
			this.RequiresPhone = root.AsBoolean("RequiresPhone");
			this.RequiresAddress = root.AsBoolean("RequiresAddress");
			this.CallToAction = root.AsString("CallToAction");
			this.OfferText = root.AsString("OfferText");
			this.CostPerLead = root.AsDecimal("CostPerLead");
			this.FormPosition = root.AsString("FormPosition");
			this.FormBlocking = root.AsBoolean("FormBlocking");
			this.ForDownload = root.AsBoolean("ForDownload");
			this.ForSidebar = root.AsBoolean("ForSidebar");
		}

		public bool RequiresPhone { get; private set; }

		public bool RequiresAddress { get; private set; }

		public string CallToAction { get; private set; }

		public string OfferText { get; private set; }
		
		public decimal CostPerLead { get; private set; }

		public string FormPosition { get; private set; }
		
		public bool FormBlocking { get; private set; }
		
		public bool ForDownload { get; private set; }
		
		public bool ForSidebar { get; private set; }
	}

	public class PromotionCampaign : Campaign
	{
		internal PromotionCampaign(XElement root) : base(root)
		{
			this.SlideshareCategory = root.AsString("SlideshareCategory");
			this.PromotionType = root.AsString("PromotionType");
			this.CostPerClick = root.AsDecimal("Cpc");
			this.ClicksToday = root.AsInteger("ClicksToday");
			this.ClicksAlltime = root.AsInteger("ClicksAlltime");
			this.ImpressionsToday = root.AsInteger("ImpressionsToday");
			this.ImpressionsAlltime = root.AsInteger("ImpressionsAlltime");
			this.ClickthroughRate = root.AsDecimal("Ctr");
			this.PauseCause = root.AsString("PauseCause");
		}

		public string SlideshareCategory { get; private set; }

		public string PromotionType { get; private set; }
		
		public decimal CostPerClick { get; private set; }

		public int ClicksToday { get; private set; }

		public int ClicksAlltime { get; private set; }

		public int ImpressionsToday { get; private set; }

		public int ImpressionsAlltime { get; private set; }
		
		public decimal  ClickthroughRate { get; private set; }

		public string PauseCause { get; private set; }
	}

	public class GeoData
	{
		internal GeoData(XElement root)
		{
			this.Region = root.AsString("Region");
			this.Country = root.AsString("Country");
			this.City = root.AsString("City");
		}

		public string Region { get; private set; }

		public string Country { get; private set; }

		public string City { get; private set; }
	}

	public class SlideshowEmbed : IEnumerable<Uri>
	{
		internal SlideshowEmbed(XElement root)
		{
			this.ID = root.AsString("slideshow-id");
			this.Title = root.AsString("title");
			this.SlideshowType = root.AsString("type");
			this.SlideBaseUrl = root.AsUri("slide-image-baseurl");
			this.SlideSuffix = root.AsString("slide-image-baseurl-suffix");
			this.SlideCount = root.AsInteger("total-slides");
			this.SlideWidth = root.AsInteger("width");
			this.SlideHeight = root.AsInteger("height");
			this.ThumbnailUrl = root.AsUri("thumbnail");
			this.ThumbnailWidth = root.AsInteger("thumbnail-width");
			this.ThumbnailHeight = root.AsInteger("thumbnail-height");

			Uri authorUri;
			if (Uri.TryCreate(root.AsString("author-url"), UriKind.Absolute, out authorUri))
				this.Username = authorUri.AbsolutePath.Split('/').Last(s => s.Length > 0);
		}

		public string ID { get; private set; }

		public string Title { get; private set; }

		public string Username { get; private set; }

		public string SlideshowType { get; private set; }

		public Uri SlideBaseUrl { get; private set; }

		public string SlideSuffix { get; private set; }

		public int SlideCount { get; private set; }

		public int SlideWidth { get; private set; }

		public int SlideHeight { get; private set; }

		public Uri ThumbnailUrl { get; private set; }

		public int ThumbnailWidth { get; private set; }

		public int ThumbnailHeight { get; private set; }

		public Uri GetSlideUrl(int index)
		{
			if (index < 1 || index > SlideCount) throw new ArgumentOutOfRangeException("index");
			return new Uri(this.SlideBaseUrl, String.Concat(this.SlideBaseUrl.AbsolutePath, index, this.SlideSuffix));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new SlideEnumerator(this);
		}

		public IEnumerator<Uri> GetEnumerator()
		{
			return new SlideEnumerator(this);
		}

		private sealed class SlideEnumerator : IEnumerator<Uri>
		{
			private SlideshowEmbed oembed;
			private Uri current;
			private int index;

			public SlideEnumerator(SlideshowEmbed oembed)
			{
				this.oembed = oembed;
				this.index = 0;
			}

			object IEnumerator.Current
			{
				get { return this.Current; }
			}

			public Uri Current
			{
				get
				{
					if (this.index > 0) return this.current;
					throw new InvalidOperationException();
				}
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				int position = this.index + 1;
				if (position < oembed.SlideCount)
				{
					this.index = position;
					this.current = oembed.GetSlideUrl(position);
					return true;
				}
				return false;
			}

			void IEnumerator.Reset()
			{
				this.index = 0;
				this.current = null;
			}
		}
	}

	public class FeedItem
	{
		private static class SlideShareRssNames
		{
			internal static readonly XName Media_Content;
			internal static readonly XName Media_Description;
			internal static readonly XName Media_Credit;
			internal static readonly XName Media_Thumbnail;

			internal static readonly XName Slideshare_Meta;
			internal static readonly XName Slideshare_Views;
			internal static readonly XName Slideshare_Comments;
			internal static readonly XName Slideshare_Type;

			private const string MediaRssNamespace = "http://search.yahoo.com/mrss/";
			private const string SlideshareNamespace = "http://slideshare.net/api/1";

			static SlideShareRssNames()
			{
				Media_Content = XName.Get("content", MediaRssNamespace);
				Media_Description = XName.Get("description", MediaRssNamespace);
				Media_Credit = XName.Get("credit", MediaRssNamespace);
				Media_Thumbnail = XName.Get("thumbnail", MediaRssNamespace);
				Slideshare_Meta = XName.Get("meta", SlideshareNamespace);
				Slideshare_Views = XName.Get("views", SlideshareNamespace);
				Slideshare_Comments = XName.Get("comments", SlideshareNamespace);
				Slideshare_Type = XName.Get("type", SlideshareNamespace);
			}
		}

		internal FeedItem(XElement root)
		{
			XElement metadataElement = root.Element(SlideShareRssNames.Slideshare_Meta);
			XElement contentElement = root.Element(SlideShareRssNames.Media_Content);

			this.Title = root.AsString("title");
			this.SlideshowUrl = root.AsUri("link");
			this.PresentationLocation = root.AsString("doc");
			this.Created = root.AsDateTime("pubDate");
			this.Description = contentElement.AsString(SlideShareRssNames.Media_Description);
			this.Username = contentElement.AsString(SlideShareRssNames.Media_Credit);
			this.ViewCount = metadataElement.AsInteger(SlideShareRssNames.Slideshare_Views);
			this.CommentCount = metadataElement.AsInteger(SlideShareRssNames.Slideshare_Comments);
			this.SlideshowType = metadataElement.AsString(SlideShareRssNames.Slideshare_Type);

			var thumbnails = contentElement.Descendants(SlideShareRssNames.Media_Thumbnail)
				.Where(s => s.Attribute("width") == null || s.Attribute("width").Value != null)
				.OrderBy(s => Convert.ToInt32(s.Attribute("width").Value, CultureInfo.InvariantCulture))
				.Select(s => s.Attribute("url").Value).ToList();

			if (thumbnails.Count > 0)
			{
				this.ThumbnailSmallUrl = new Uri(new UriBuilder().Uri, thumbnails[0]);
				if (thumbnails.Count > 1) this.ThumbnailUrl = new Uri(thumbnails[1]);
			}
			this.StrippedTitle = this.SlideshowUrl.AbsolutePath.Split('/').Last(s => s.Length > 0);
		}

		public string Title { get; private set; }

		public Uri SlideshowUrl { get; private set; }

		public string PresentationLocation { get; private set; }

		public string StrippedTitle { get; private set; }

		public string Description { get; private set; }

		public string Username { get; private set; }

		public DateTime Created { get; private set; }

		public Uri ThumbnailUrl { get; private set; }

		public Uri ThumbnailSmallUrl { get; private set; }

		public int ViewCount { get; private set; }

		public int CommentCount { get; private set; }

		public string SlideshowType { get; private set; }
	}

	public enum SlideshowStatus : int
	{
		Queued = 0, Converting = 1, Converted = 2, Failed = 3
	}

	public enum SlideshowType : int
	{
		Presentation = 0, Document = 1, Portfolio = 2, Video = 3
	}
}
