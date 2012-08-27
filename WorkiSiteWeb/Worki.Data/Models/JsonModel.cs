using System;
using System.Collections.Generic;

namespace Worki.Data.Models
{
	public interface IJsonProvider<T>
	{
		T GetJson();
	}

    public class LocalisationsContainer
    {
        public LocalisationsContainer()
        {
            list = new List<LocalisationJson>();
        }

        public List<LocalisationJson> list;
        public int maxCount;
        public double latitude;
        public double longitude;
    }

	public class LocalisationJson
	{
        public LocalisationJson()
        {
            comments = new List<CommentJson>();
            fans = new List<MemberJson>();
            features = new List<FeatureJson>();
            offers = new List<OfferJson>();
            images = new List<ImageJson>();
            minPrices = new List<MinPriceJson>();
        }

		public int id { get; set; }
		public string name { get; set; }
		public double latitude { get; set; }
		public double longitude { get; set; }
		public string description { get; set; }
		public string address { get; set; }
        public string postalCode { get; set; }
		public string city { get; set; }
        public double distance { get; set; }
		public string type { get; set; }
        public bool isFree { get; set; }
		public string url { get; set; }
        public double rating { get; set; }
        public OpeningTimesJson openingTimes { get; set; }
        public AccessJson access { get; set; }
        public List<MinPriceJson> minPrices { get; set; }
        public List<ImageJson> images { get; set; }
        public List<FeatureJson> features { get; set; }
        public List<OfferJson> offers { get; set; }
		public List<CommentJson> comments { get; set; }
        public List<MemberJson> fans { get; set; }
	}

	public class CommentJson
	{
		public int id { get; set; }
		public string date { get; set; }
        public MemberJson author { get; set; }
		public string post { get; set; }
		public int rating { get; set; }
		public int ratingPrice { get; set; }
		public int ratingWifi { get; set; }
		public int ratingDispo { get; set; }
		public int ratingWelcome { get; set; }
	}

    public class MemberJson
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public ImageJson avatar { get; set; }
        public string companyName { get; set; }
        public string city { get; set; }
        public string profile { get; set; }
        public string description { get; set; }
        public string twitter { get; set; }
        public string facebook { get; set; }
        public string linkedin { get; set; }
        public string viadeo { get; set; }
    }

    public class PriceJson
    {
        public string price { get; set; }
        public string frequency { get; set; }
    }

    public class MinPriceJson
    {
        public int offerType { get; set; }
        public PriceJson price { get; set; }
    }

    public class AccessJson
    {
        public string publicTransport { get; set; }
        public string station { get; set; }
        public string roadAccess { get; set; }
    }

    public class OfferJson
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<ImageJson> images { get; set; }
        public List<PriceJson> prices { get; set; }
        public List<FeatureJson> features { get; set; }
        public string availability { get; set; }
        public int offerType { get; set; }
    }

	public class ImageJson
	{
		const string DeleteType = "POST";

		public ImageJson()
        {
            delete_type = DeleteType;
        }

		public string name { get; set; }
		public int size { get; set; }
		public string url { get; set; }
		public string thumbnail_url { get; set; }
		public string delete_url { get; set; }
		public string delete_type { get; set; }
		public string is_default { get; set; }
		public string is_logo { get; set; }
	}

    public class OpeningTimesJson
    {
        public string monday { get; set; }
        public string tuesday { get; set; }
        public string wednesday { get; set; }
        public string thursday { get; set; }
        public string friday { get; set; }
        public string saturday { get; set; }
        public string sunday { get; set; }
    }

    public class AuthJson
    {
        public string       token { get; set; }
        public string       email { get; set; }
        public string       firstName { get; set; }
        public string       lastName { get; set; }
        public string       birthDate {get;set;}
        public string       phoneNumber { get; set; }
        public int          profile { get; set; }
        public int          civility { get; set; }
        public string       avatar { get; set; }
        public string       description { get; set; }
        public string       city { get; set; }
        public string       postalCode { get; set; }

    }

    public class FeatureJson
    {
        public int featureId { get; set; }
        public string featureDisplay { get; set; }
    }
}