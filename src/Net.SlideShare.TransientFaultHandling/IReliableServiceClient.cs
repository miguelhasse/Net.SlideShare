namespace System.Net.SlideShare
{
    public interface IReliableServiceClient
    {
        ISlideShareShow Slideshow { get; }

        ISlideShareUser User { get; }

        ISlideShareFavorite Favorite { get; }

        ISlideShareMarketing Marketing { get; }

        ISlideShareMetadata Metadata { get; }

        ISlideShareSyndication Feeds { get; }
    }
}
