using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Runtime.Serialization;
using Worki.Infrastructure.Helpers;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;

namespace Worki.Data.Models
{
	[MetadataType(typeof(Localisation_Validation))]
    public partial class Localisation : IJsonProvider<LocalisationJson>, IPictureDataProvider, IMapModelProvider, IFeatureProvider, IValidatableObject// : IDataErrorInfo
    {
        #region Ctor

        #endregion

        #region IJsonProvider

        public LocalisationJson GetJson()
        {
            var json = new LocalisationJson
            {
                id = ID,
                latitude = Latitude,
                longitude = Longitude,
                name = Name,
				description = GetDescription(),
                address = Adress,
                postalCode = PostalCode,
                city = City,
                rating = GetRatingAverage(RatingType.General),
				type = Localisation.GetLocalisationType(TypeValue),
                isFree = IsFreeLocalisation()
            };

            //get comments
            foreach (var item in Comments)
            {
                json.comments.Add(item.GetJson());
            }

            //get fans
            foreach (var item in FavoriteLocalisations)
            {
                json.fans.Add(item.Member.GetJson());
            }

            //get all offer types
            foreach (var item in GetOfferTypes())
            {
                var minPrice = GetMinPrice((int)item);
                json.prices.Add(new PriceJson
                {
                    offerType = (int)item,
                    price = minPrice != null ? minPrice.GetPriceDisplay() : string.Empty
                });
            }

            //get offers
            foreach (var item in GetAllOffers())
            {
                json.offers.Add(item.GetJson());
            }

            //get amenities
            foreach (var item in LocalisationFeatures)
            {
                json.features.Add(new FeatureJson { featureId = item.FeatureID, featureDisplay = FeatureHelper.GetFeatureDisplayName((Feature)item.FeatureID) });
            }

            //get openning time
            json.openingTimes = new OpeningTimesJson
            {
                monday = GetOpenningTime(DayOfWeek.Monday),
                tuesday = GetOpenningTime(DayOfWeek.Tuesday),
                wednesday = GetOpenningTime(DayOfWeek.Wednesday),
                thursday = GetOpenningTime(DayOfWeek.Thursday),
                friday = GetOpenningTime(DayOfWeek.Friday),
                saturday = GetOpenningTime(DayOfWeek.Saturday),
                sunday = GetOpenningTime(DayOfWeek.Sunday)
            };

            json.access = new AccessJson
            {
                publicTransport = PublicTransportation,
                roadAccess = RoadAccess,
                station = Station
            };

            return json;
        }

		#endregion

		#region Features

		#region IFeatureProvider

        public string GetPrefix()
        {
            return FeatureHelper.LocalisationPrefix;
        }

		public bool HasFeature(Feature feature)
		{
			var equalityComparer = new LocalisationFeatureEqualityComparer();
			return LocalisationFeatures.Contains(new LocalisationFeature { FeatureID = (int)feature }, equalityComparer);
		}

		public string GetStringFeature(Feature feature)
		{
			var obj = LocalisationFeatures.FirstOrDefault(o => o.FeatureID == (int)feature);
			if (obj == null)
				return null;
			return obj.StringValue;
		}

		public decimal? GetNumberFeature(Feature feature)
		{
			var obj = LocalisationFeatures.FirstOrDefault(o => o.FeatureID == (int)feature);
			if (obj == null)
				return null;
			return obj.DecimalValue;
		}

		#endregion

		#region Features

		public List<IFeatureContainer> GetFeaturesWithin(IEnumerable<Feature> toInclude)
		{
			var toRet = new List<IFeatureContainer>();
			return (from item
						in LocalisationFeatures
					where toInclude.Contains((Feature)item.FeatureID)
					select (IFeatureContainer)item).ToList();
		}

		public bool HasFeatureIn(List<Feature> features)
		{
			foreach (var item in features)
			{
				if (HasFeature(item))
					return true;
			}
			return false;
		}

		#endregion

		#endregion

		#region Localisation Types

		public static List<int> LocalisationTypes = new List<int>()
        {
            (int)LocalisationType.SpotWifi,
            (int)LocalisationType.CoffeeResto,
            (int)LocalisationType.Biblio,
            (int)LocalisationType.PublicSpace,
			(int)LocalisationType.TravelerSpace,
			(int)LocalisationType.Hotel,
            (int)LocalisationType.Telecentre,
            (int)LocalisationType.BuisnessCenter,
            (int)LocalisationType.CoworkingSpace,
 			(int)LocalisationType.WorkingHotel,
			(int)LocalisationType.PrivateArea,
			(int)LocalisationType.SharedOffice
        };

        public static LocalisationType GetLocalisationType(string seoType)
        {
            switch (seoType)
            {
                case MiscHelpers.SeoConstants.SpotWifi:
                    return LocalisationType.SpotWifi;
                case MiscHelpers.SeoConstants.CoffeeResto:
                    return LocalisationType.CoffeeResto;
                case MiscHelpers.SeoConstants.Biblio:
                    return LocalisationType.Biblio;
                case MiscHelpers.SeoConstants.PublicSpace:
                    return LocalisationType.PublicSpace;
                case MiscHelpers.SeoConstants.TravelerSpace:
                    return LocalisationType.TravelerSpace;
                case MiscHelpers.SeoConstants.Hotel:
                    return LocalisationType.Hotel;
                case MiscHelpers.SeoConstants.Telecentre:
                    return LocalisationType.Telecentre;
                case MiscHelpers.SeoConstants.BuisnessCenter:
                    return LocalisationType.BuisnessCenter;
                case MiscHelpers.SeoConstants.CoworkingSpace:
                    return LocalisationType.CoworkingSpace;
                case MiscHelpers.SeoConstants.WorkingHotel:
                    return LocalisationType.WorkingHotel;
                case MiscHelpers.SeoConstants.PrivateArea:
                    return LocalisationType.PrivateArea;
                case MiscHelpers.SeoConstants.SharedOffice:
                    return LocalisationType.SharedOffice;
                default:
                    return LocalisationType.SpotWifi;
            }
        }

        public static string GetLocalisationSeoType(int type)
        {
            var enumType = (LocalisationType)type;
            switch (enumType)
            {
                case LocalisationType.SpotWifi:
                    return MiscHelpers.SeoConstants.SpotWifi;
                case LocalisationType.CoffeeResto:
                    return MiscHelpers.SeoConstants.CoffeeResto;
                case LocalisationType.Biblio:
                    return MiscHelpers.SeoConstants.Biblio;
                case LocalisationType.PublicSpace:
                    return MiscHelpers.SeoConstants.PublicSpace;
                case LocalisationType.TravelerSpace:
                    return MiscHelpers.SeoConstants.TravelerSpace;
                case LocalisationType.Hotel:
                    return MiscHelpers.SeoConstants.Hotel;
                case LocalisationType.Telecentre:
                    return MiscHelpers.SeoConstants.Telecentre;
                case LocalisationType.BuisnessCenter:
                    return MiscHelpers.SeoConstants.BuisnessCenter;
                case LocalisationType.CoworkingSpace:
                    return MiscHelpers.SeoConstants.CoworkingSpace;
                case LocalisationType.WorkingHotel:
                    return MiscHelpers.SeoConstants.WorkingHotel;
                case LocalisationType.PrivateArea:
                    return MiscHelpers.SeoConstants.PrivateArea;
                case LocalisationType.SharedOffice:
                    return MiscHelpers.SeoConstants.SharedOffice;
                default:
                    return string.Empty;
            }
        }

		public static string GetLocalisationType(int type)
		{
			var enumType = (LocalisationType)type;
			switch (enumType)
			{
				case LocalisationType.SpotWifi:
					return Worki.Resources.Models.Localisation.Localisation.SpotWifi;
				case LocalisationType.CoffeeResto:
					return Worki.Resources.Models.Localisation.Localisation.CoffeeResto;
				case LocalisationType.Biblio:
					return Worki.Resources.Models.Localisation.Localisation.Biblio;
				case LocalisationType.PublicSpace:
					return Worki.Resources.Models.Localisation.Localisation.PublicSpace;
				case LocalisationType.TravelerSpace:
					return Worki.Resources.Models.Localisation.Localisation.TravelerSpace;
				case LocalisationType.Hotel:
					return Worki.Resources.Models.Localisation.Localisation.Hotel;
				case LocalisationType.Telecentre:
					return Worki.Resources.Models.Localisation.Localisation.Telecentre;
				case LocalisationType.BuisnessCenter:
					return Worki.Resources.Models.Localisation.Localisation.BuisnessCenter;
				case LocalisationType.CoworkingSpace:
					return Worki.Resources.Models.Localisation.Localisation.CoworkingSpace;
				case LocalisationType.WorkingHotel:
					return Worki.Resources.Models.Localisation.Localisation.WorkingHotel;
				case LocalisationType.PrivateArea:
					return Worki.Resources.Models.Localisation.Localisation.PrivateArea;
				case LocalisationType.SharedOffice:
					return Worki.Resources.Models.Localisation.Localisation.SharedOffice;
				default:
					return string.Empty;
			}
		}

        public static Dictionary<int, string> GetLocalisationSeoTypes()
		{
            return LocalisationTypes.ToDictionary(t => t, t => GetLocalisationSeoType(t));
		}

        public static Dictionary<int, string> GetLocalisationTypes()
        {
            return LocalisationTypes.ToDictionary(t => t, t => GetLocalisationType(t));
        }

		public static Dictionary<int, string> GetFreeLocalisationTypes()
		{
			return FreeLocalisationTypes.ToDictionary(t => t, t => GetLocalisationType(t));
		}

		public static Dictionary<int, string> GetNotFreeLocalisationTypes(bool sharedOffice = false)
		{
			var notFree = LocalisationTypes.Except(FreeLocalisationTypes);
			if (!sharedOffice)
			{
				notFree = notFree.Except(new List<int> { (int)LocalisationType.SharedOffice });
			}
			return notFree.ToDictionary(t => t, t => GetLocalisationType(t));
		}

		public static List<int> FreeLocalisationTypes = new List<int>()
        {
            (int)LocalisationType.SpotWifi,
            (int)LocalisationType.CoffeeResto,
            (int)LocalisationType.Biblio,
            (int)LocalisationType.PublicSpace 
        };

		public bool IsFreeLocalisation()
		{
			return FreeLocalisationTypes.Contains(TypeValue);
		}

		public bool IsSharedOffice()
		{
			return (int)LocalisationType.SharedOffice == TypeValue;
		}

        public bool IsPaidLocalisation()
        {
            return !IsFreeLocalisation() && !IsSharedOffice();
        }

		#endregion

		#region Localisation Offers

		public static List<int> OfferTypes = new List<int>()
        {
            (int)LocalisationOffer.AllOffers,
            (int)LocalisationOffer.FreeArea,
            (int)LocalisationOffer.BuisnessLounge,
            (int)LocalisationOffer.Workstation,
			(int)LocalisationOffer.Desktop,
            (int)LocalisationOffer.MeetingRoom,
            (int)LocalisationOffer.SeminarRoom,
            (int)LocalisationOffer.VisioRoom
        };

		public static string GetOfferType(int type, bool single = true, bool forSearch = false)
		{
			var enumType = (LocalisationOffer)type;
			switch (enumType)
			{
				case LocalisationOffer.AllOffers:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.AllOffers;
				case LocalisationOffer.FreeArea:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.FreeArea;
				case LocalisationOffer.BuisnessLounge:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.BuisnessLounge;
				case LocalisationOffer.Workstation:
					return forSearch ? Worki.Resources.Models.Localisation.LocalisationFeatures.WorkstationSearch
							: single ? Worki.Resources.Models.Localisation.LocalisationFeatures.SingleWorkstation : Worki.Resources.Models.Localisation.LocalisationFeatures.Workstation;
				case LocalisationOffer.Desktop:
					return forSearch ? Worki.Resources.Models.Localisation.LocalisationFeatures.DesktopSearch
							: single ? Worki.Resources.Models.Localisation.LocalisationFeatures.SingleDesktop : Worki.Resources.Models.Localisation.LocalisationFeatures.Desktop;
				case LocalisationOffer.MeetingRoom:
					return single ? Worki.Resources.Models.Localisation.LocalisationFeatures.SingleMeetingRoom : Worki.Resources.Models.Localisation.LocalisationFeatures.MeetingRoom;
				case LocalisationOffer.SeminarRoom:
					return single ? Worki.Resources.Models.Localisation.LocalisationFeatures.SingleSeminarRoom : Worki.Resources.Models.Localisation.LocalisationFeatures.SeminarRoom;
				case LocalisationOffer.VisioRoom:
					return single ? Worki.Resources.Models.Localisation.LocalisationFeatures.SingleVisioRoom : Worki.Resources.Models.Localisation.LocalisationFeatures.VisioRoom;
				default:
					return string.Empty;
			}
		}

        public static int GetOfferTypeFromSeoString(string offerType)
        {
            switch (offerType)
            {
                case MiscHelpers.SeoConstants.FreeArea:
                    return (int)LocalisationOffer.FreeArea;
                case MiscHelpers.SeoConstants.BuisnessLounge:
                    return (int)LocalisationOffer.BuisnessLounge;
                case MiscHelpers.SeoConstants.Workstation:
                    return (int)LocalisationOffer.Workstation;
                case MiscHelpers.SeoConstants.Desktop:
                    return (int)LocalisationOffer.Desktop;
                case MiscHelpers.SeoConstants.MeetingRoom:
                    return (int)LocalisationOffer.MeetingRoom;
                case MiscHelpers.SeoConstants.SeminarRoom:
                    return (int)LocalisationOffer.SeminarRoom;
                case MiscHelpers.SeoConstants.VisioRoom:
                    return (int)LocalisationOffer.VisioRoom;
                default:
                    return -1;
            }
        }

        public static string GetSeoStringOfferFromType(int offerType)
        {
            var offerTypeEnum = (LocalisationOffer)offerType;
            switch (offerTypeEnum)
            {
                case LocalisationOffer.FreeArea:
                    return MiscHelpers.SeoConstants.FreeArea;
                case LocalisationOffer.BuisnessLounge:
                    return MiscHelpers.SeoConstants.BuisnessLounge;
                case LocalisationOffer.Workstation:
                    return MiscHelpers.SeoConstants.Workstation;
                case LocalisationOffer.Desktop:
                    return MiscHelpers.SeoConstants.Desktop;
                case LocalisationOffer.MeetingRoom:
                    return MiscHelpers.SeoConstants.MeetingRoom;
                case LocalisationOffer.SeminarRoom:
                    return MiscHelpers.SeoConstants.SeminarRoom;
                case LocalisationOffer.VisioRoom:
                    return MiscHelpers.SeoConstants.VisioRoom;
                default:
                    return string.Empty;
            }
        }

		public static Dictionary<int, string> GetOfferTypes(bool forSearch = false)
		{
			return OfferTypes.ToDictionary(o => o, o => GetOfferType(o, true, forSearch));
		}

		public static Dictionary<int, string> GetOfferTypeDict(IEnumerable<LocalisationOffer> except, bool forSearch = false)
		{
			var toRet = (from item in GetOfferTypes(forSearch) where !except.Contains((LocalisationOffer)item.Key) select item).ToDictionary(k => k.Key, k => k.Value);
			return toRet;
		}

        public static Dictionary<int, string> GetOfferTypeDict(bool isShared)
        {
            var offersToExclude = isShared ? new List<LocalisationOffer> { LocalisationOffer.AllOffers, LocalisationOffer.FreeArea, LocalisationOffer.BuisnessLounge, LocalisationOffer.SeminarRoom, LocalisationOffer.VisioRoom }
                                           : new List<LocalisationOffer> { LocalisationOffer.AllOffers, LocalisationOffer.FreeArea };

            return Localisation.GetOfferTypeDict(offersToExclude);
        }

		public bool HasOffer(LocalisationOffer offer)
		{
			return Offers.Where(o => o.IsOnline && o.Type == (int)offer).Count() != 0;
		}

		public int OfferCount(LocalisationOffer offer)
		{
            return Offers.Where(o => o.IsOnline &&  o.Type == (int)offer).Count();
		}

        public IEnumerable<Offer> GetAllOffers()
        {
            return Offers.Where(o => o.IsOnline);
        }

		public IEnumerable<Offer> GetOffers(LocalisationOffer offerType)
		{
			return Offers.Where(o => o.IsOnline && o.Type == (int)offerType);
		}

        public IEnumerable<Offer> GetBookableOffers()
        {
            return Offers.Where(o => o.IsOnline && o.IsReallyBookable());
        }

        public IEnumerable<LocalisationOffer> GetOfferTypes()
        {
            return Offers.Where(o => o.IsOnline).GroupBy(o => o.Type).Select(g => (LocalisationOffer)g.Key);
        }

		public IEnumerable<string> GetOfferTypeList()
		{
			return Offers.Where(o => o.IsOnline).GroupBy(o => o.Type).Select(g => Localisation.GetOfferType(g.Key));
		}

		/// <summary>
		/// Check if localisation have an offer
		/// </summary>
		/// <returns>true if it is the case</returns>
		public bool HasOffer()
		{
			return Offers.Where(o => o.IsOnline).Count() != 0;
		}

		/// <summary>
		/// Get all files of a certain type of offer
		/// </summary>
		/// <param name="offerType">the type of offer</param>
		/// <returns>the files</returns>
		public IEnumerable<OfferFile>  GetOfferFiles(LocalisationOffer offerType)
		{
			var offers = GetOffers(offerType);
			var toRet = new List<OfferFile>();
			foreach (var item in offers)
			{
				foreach (var file in item.OfferFiles)
				{
					toRet.Add(file);
				}
			}
			return toRet;
		}

		/// <summary>
		/// Get all features of a certain type of offer
		/// </summary>
		/// <param name="offerType">the type of offer</param>
		/// <returns>the features</returns>
		public IEnumerable<IFeatureContainer> GetOfferFeatures(LocalisationOffer offerType)
		{
			var offers = GetOffers(offerType);
			var set = new HashSet<IFeatureContainer>();
			foreach (var item in offers)
			{
				foreach (var feature in item.OfferFeatures)
				{
					if (set.Where(f => f.Feature == feature.Feature && f.DecimalValue == feature.DecimalValue && f.StringValue == feature.StringValue).Count() != 0)
						continue;
					set.Add(feature);
				}
			}
			return set.ToList();
		}

		#endregion

		//#region IDataErrorInfo

		//public string Error
		//{
		//    get { return string.Empty; }
		//}

		//public string this[string columnName]
		//{
		//    get
		//    {
		//        switch (columnName)
		//        {
		//            default:
		//                return string.Empty;
		//        }
		//    }
		//}
		//#endregion

		#region Comments

        public IEnumerable<Comment> GetOrderedComments(Culture culture)
		{
            var withComm = (from item
                           in Comments
                            where item.HasPost()
                             orderby item.HasPost(culture) descending, item.Date descending
                            select item);

			var withoutComm = (from item
								 in Comments
							   where !item.HasPost()
							   orderby item.Date descending
							   select item);

			var toRet = withComm.Concat(withoutComm);
			return toRet;
		}

		public IEnumerable<Comment> GetCommentSummary(Culture culture)
		{
			var toRet = (from item
						   in Comments
                         where item.Member != null && item.HasPost()
                         orderby item.HasPost(culture) descending, item.Date descending
						 select item).Take(2);
			return toRet;
		}

		public double GetRatingAverage(RatingType ratingType)
		{
			if (Comments.Count == 0)
				return 0;
			try
			{
				switch (ratingType)
				{
					case RatingType.Price:
						return Comments.Where(c => c.RatingPrice >= 0).Average(c => c.RatingPrice);
					case RatingType.Wifi:
						return Comments.Where(c => c.RatingWifi >= 0).Average(c => c.RatingWifi);
					case RatingType.Dispo:
						return Comments.Where(c => c.RatingDispo >= 0).Average(c => c.RatingDispo);
					case RatingType.Welcome:
						return Comments.Where(c => c.RatingWelcome >= 0).Average(c => c.RatingWelcome);
					case RatingType.General:
					default:
						{
							if (!IsFreeLocalisation())
								return Comments.Where(c => c.Rating >= 0).Average(c => c.Rating);
							else
							{
								var ratings = new List<double>
                                { 
                                    GetRatingAverage(RatingType.Price),
                                    GetRatingAverage(RatingType.Wifi), 
                                    GetRatingAverage(RatingType.Dispo), 
                                    GetRatingAverage(RatingType.Welcome) 
                                };
								return ratings.Where(d => d >= 0).Average();
							}
						}
				}
			}
			catch (Exception)
			{
				return -1;
			}
		}

		#endregion

		#region Has Field

		public bool HasOpenningTimes(DayOfWeek day)
		{
			switch (day)
			{
				case DayOfWeek.Monday:
					return LocalisationData.MonOpenMorning != null || LocalisationData.MonCloseMorning != null || LocalisationData.MonOpenAfter != null || LocalisationData.MonCloseAfter != null;
				case DayOfWeek.Tuesday:
					return LocalisationData.TueOpenMorning != null || LocalisationData.TueCloseMorning != null || LocalisationData.TueOpenAfter != null || LocalisationData.TueCloseAfter != null;
				case DayOfWeek.Wednesday:
					return LocalisationData.WedOpenMorning != null || LocalisationData.WedCloseMorning != null || LocalisationData.WedOpenAfter != null || LocalisationData.WedCloseAfter != null;
				case DayOfWeek.Thursday:
					return LocalisationData.ThuOpenMorning != null || LocalisationData.ThuCloseMorning != null || LocalisationData.ThuOpenAfter != null || LocalisationData.ThuCloseAfter != null;
				case DayOfWeek.Friday:
					return LocalisationData.FriOpenMorning != null || LocalisationData.FriCloseMorning != null || LocalisationData.FriOpenAfter != null || LocalisationData.FriCloseAfter != null;
				case DayOfWeek.Saturday:
					return LocalisationData.SatOpenMorning != null || LocalisationData.SatCloseMorning != null || LocalisationData.SatOpenAfter != null || LocalisationData.SatCloseAfter != null;
				case DayOfWeek.Sunday:
					return LocalisationData.SunOpenMorning != null || LocalisationData.SunCloseMorning != null || LocalisationData.SunOpenAfter != null || LocalisationData.SunCloseAfter != null;
				default:
					return false;
			}
		}

		public bool HasOpenningTimes()
		{
			return HasOpenningTimes(DayOfWeek.Monday)
				|| HasOpenningTimes(DayOfWeek.Tuesday)
				|| HasOpenningTimes(DayOfWeek.Wednesday)
				|| HasOpenningTimes(DayOfWeek.Thursday)
				|| HasOpenningTimes(DayOfWeek.Friday)
				|| HasOpenningTimes(DayOfWeek.Saturday)
				|| HasOpenningTimes(DayOfWeek.Sunday);
		}

		public bool HasAccesField()
		{
			return !string.IsNullOrEmpty(PublicTransportation)
				|| !string.IsNullOrEmpty(RoadAccess)
				|| !string.IsNullOrEmpty(Station);
		}

		public bool ContactEmpty()
		{
            return (string.IsNullOrEmpty(Fax) && string.IsNullOrEmpty(PhoneNumber) && string.IsNullOrEmpty(WebSite) && string.IsNullOrEmpty(Mail) && string.IsNullOrEmpty(Facebook) && string.IsNullOrEmpty(Twitter));
		}

        /// <summary>
        /// show contact or not
        /// </summary>
        /// <returns>true if free, or shared, or paid with no owner</returns>
        public bool ShowContactInfo()
        {
            return IsFreeLocalisation() || IsSharedOffice() || (IsPaidLocalisation() && !HasOwner());
        }

		#endregion

		#region Render strings

		/// <summary>
		/// Render datetime to readable string
		/// </summary>
		/// <param name="date">the date to render</param>
		/// <returns>the date string</returns>
		public string RenderDate(DateTime date)
		{
			return CultureHelpers.GetSpecificFormat(date, CultureHelpers.TimeFormat.Time);
			//var temp = string.Format("{0:H:mm}", date);
			//return temp;
		}

		string GetOpenningTime(DateTime? openAM, DateTime? closeAM, DateTime? openPM, DateTime? closePM)
		{
			var toRet = string.Empty;
			//open morning
			if (openAM.HasValue)
			{
				toRet += RenderDate(openAM.Value);
				toRet += Worki.Resources.Models.Localisation.Localisation.To;
				//close for lunch
				if (closeAM.HasValue)
				{
					toRet += RenderDate(closeAM.Value);
				}
				//close at evening
				else if (closePM.HasValue)
				{
					toRet += RenderDate(closePM.Value);
				}
				else
				{
					toRet += "0:00";
				}
			}

			if (openPM.HasValue)
			{
				if (openAM.HasValue)
					toRet += " - ";
				toRet += RenderDate(openPM.Value);
				if (closePM.HasValue)
				{
					toRet += Worki.Resources.Models.Localisation.Localisation.To;
					toRet += RenderDate(closePM.Value);
				}
				else
				{
					toRet += Worki.Resources.Models.Localisation.Localisation.To;
					toRet += "0:00";
				}
			}

			return toRet;//.Replace(':', 'h');
		}

		public string GetOpenningTime(DayOfWeek day)
		{
			switch (day)
			{
				case DayOfWeek.Monday:
					return GetOpenningTime(LocalisationData.MonOpenMorning, LocalisationData.MonCloseMorning, LocalisationData.MonOpenAfter, LocalisationData.MonCloseAfter);
				case DayOfWeek.Tuesday:
					return GetOpenningTime(LocalisationData.TueOpenMorning, LocalisationData.TueCloseMorning, LocalisationData.TueOpenAfter, LocalisationData.TueCloseAfter);
				case DayOfWeek.Wednesday:
					return GetOpenningTime(LocalisationData.WedOpenMorning, LocalisationData.WedCloseMorning, LocalisationData.WedOpenAfter, LocalisationData.WedCloseAfter);
				case DayOfWeek.Thursday:
					return GetOpenningTime(LocalisationData.ThuOpenMorning, LocalisationData.ThuCloseMorning, LocalisationData.ThuOpenAfter, LocalisationData.ThuCloseAfter);
				case DayOfWeek.Friday:
					return GetOpenningTime(LocalisationData.FriOpenMorning, LocalisationData.FriCloseMorning, LocalisationData.FriOpenAfter, LocalisationData.FriCloseAfter);
				case DayOfWeek.Saturday:
					return GetOpenningTime(LocalisationData.SatOpenMorning, LocalisationData.SatCloseMorning, LocalisationData.SatOpenAfter, LocalisationData.SatCloseAfter);
				case DayOfWeek.Sunday:
					return GetOpenningTime(LocalisationData.SunOpenMorning, LocalisationData.SunCloseMorning, LocalisationData.SunOpenAfter, LocalisationData.SunCloseAfter);
				default:
					return string.Empty;
			}
		}

		public string GetAvoidString()
		{
			var toRet = string.Empty;
			if (!HasFeatureIn(FeatureHelper.AvoidPeriods))
				return toRet;
			foreach (var item in FeatureHelper.AvoidPeriods)
			{
				if (HasFeature(item))
					toRet += FeatureHelper.GetFeatureDisplayName(item).ToLower() + ", ";
			}
			var last = toRet.LastIndexOf(",");
			toRet = toRet.Remove(last, 1).Insert(last, ".");
			return toRet;
		}

		#endregion

        #region IPictureDataProvider

        public int GetId()
        {
            return ID;
        }

		public ProviderType GetProviderType()
		{
			return ProviderType.Localisation;
		}

        public List<PictureData> GetPictureData()
        {
            if (LocalisationFiles != null)
                return (from item in LocalisationFiles select new PictureData { FileName = item.FileName, IsDefault = item.IsDefault, IsLogo = item.IsLogo }).ToList();
            return new List<PictureData>();
        }

        public string GetMainPic()
        {
            var main = (from item in LocalisationFiles where item.IsDefault orderby item.ID select item.FileName).FirstOrDefault();
            return main;
        }

        public string GetPic(int index)
        {
            var list = (from item in LocalisationFiles where !item.IsDefault && !item.IsLogo orderby item.ID select item.FileName).ToList();
            var count = list.Count();
            if (count == 0 || index < 0 || index >= count)
                return string.Empty;
            return list[index];
        }

        public string GetLogoPic()
        {
            var logo = (from item in LocalisationFiles where item.IsLogo orderby item.ID select item.FileName).FirstOrDefault();
            return logo;
        }

        public string GetDisplayName()
        {
			return string.Format(Worki.Resources.Models.Localisation.Localisation.DisplayName, GetFullName(), Localisation.GetLocalisationType(TypeValue), City);
        }

        public string GetDescription()
        {
			switch(Thread.CurrentThread.CurrentUICulture.Name)
			{
				case "en":
					return DescriptionEn;
				case "es":
					return DescriptionEs;
                case "de":
                    return DescriptionDe;
				default:
					return Description;
			}
        }

        public string GetDescriptionName()
        {
            switch (Thread.CurrentThread.CurrentUICulture.Name)
            {
                case "en":
                    return "DescriptionEn";
                case "es":
                    return "DescriptionEs";
                case "de":
                    return "DescriptionDe";
                default:
                    return "Description";
            }
        }

        #endregion

		#region Localisation Edition

		public MemberEdition GetLastEdition()
		{
			var last = (from item in MemberEditions where item.ModificationType == (int)EditionType.Edition orderby item.ModificationDate descending select item).FirstOrDefault();
			return last;
		}

		public MemberEdition GetCreation()
		{
			var creation = (from item in MemberEditions where item.ModificationType == (int)EditionType.Creation select item).FirstOrDefault();
			return creation;
		}

		/// <summary>
		/// Get last modification date
		/// </summary>
		/// <returns>last modification date</returns>
		public DateTime GetLastModificationDate()
		{
			var last = GetLastEdition() ?? GetCreation();
			if (last != null)
				return last.ModificationDate;

			return DateTime.UtcNow;
		}

		#endregion

        #region IMapModelProvider

        public MapModel GetMapModel()
        {
            return new MapModel
            {
                Latitude = Latitude,
                Longitude = Longitude,
                Name = Name
            };
        }

        #endregion

        #region Owner

        /// <summary>
        /// tell if the localisation has an owner
        /// </summary>
        /// <returns>true of has owner</returns>
        public bool HasOwner()
        {
            return Member != null && Member.Username != MiscHelpers.AdminConstants.AdminMail;
        }

        /// <summary>
        /// set ownership to on member
        /// </summary>
        /// <param name="ownerid">Ownerid</param>
        public void SetOwner(int ownerid)
        {
            if (ownerid != -1)
                OwnerID = ownerid;
        }

        public bool IsOwner(int memberid)
        {
            return (OwnerID == memberid);
        }

		/// <summary>
		/// tell if the owner has the specified client
		/// </summary>
		/// <param name="clientId">id of the client</param>
		/// <returns>true if is already a client</returns>
		public bool HasClient(int clientId)
		{
			return LocalisationClients.Where(mc => mc.ClientId == clientId).Count() > 0;
		}

        #endregion

        #region MainLocalisation

        public bool IsMain
        {
            get
            {
                return MainLocalisation != null && MainLocalisation.IsMain;
            }
        }

        public bool IsOffline
        {
            get
            {
                return MainLocalisation != null && MainLocalisation.IsOffline;
            }
        }

        #endregion

        #region Booking

        public IList<MemberBooking> GetBookings()
        {
            var toRet = new List<MemberBooking>();
            foreach (var offer in Offers)
            {
                toRet.AddRange(offer.MemberBookings);
            }

            return toRet;
        }

        public IList<MemberBooking> GetPaidBookings()
        {
            var bookings = GetBookings();
            return bookings.Where(b => b.StatusId == (int)MemberBooking.Status.Paid).ToList();
        }

		/// <summary>
		/// true if one offer is bookable
		/// </summary>
		/// <returns></returns>
        public bool AcceptBooking()
		{
            return Offers.Count(o => o.IsReallyBookable()) != 0 && HasOwner();
		}

		/// <summary>
		/// true if one offer is quotable
		/// </summary>
		/// <returns></returns>
        public bool AcceptQuotation()
		{
            return Offers.Count(o => o.AcceptQuotation()) != 0 && HasOwner();
		}

        public bool AcceptProduct()
        {
            return AcceptBooking() || AcceptQuotation();
        }

        #endregion

        #region Quotation

        public IList<MemberQuotation> GetQuotations()
        {
            var toRet = new List<MemberQuotation>();
            foreach (var offer in Offers)
            {
                toRet.AddRange(offer.MemberQuotations);
            }

            return toRet;
        }

        #endregion

		#region Prices

        /// <summary>
        /// Get list of all offers prices
        /// </summary>
        /// <returns>list of all offerprices</returns>
        public IEnumerable<OfferPrice> GetAllPrices(int offerType = -1)
        {
            var toRet = new List<OfferPrice>();
            var offers = offerType == -1 ? Offers : Offers.Where(o => o.Type == offerType);
            if (offers.Count() == 0)
                return toRet;

            foreach (var offer in offers)
            {
                toRet = toRet.Concat(offer.OfferPrices).ToList();
            }

            return toRet;
        }

        /// <summary>
        /// Get the min price of the localisation, empty if no price
        /// filter by offerType if needed
        /// </summary>
        /// <returns>the min price string</returns>
        public OfferPrice GetMinPrice(int offerType = -1)
        {
            var offerPrices = GetAllPrices(offerType);

            if (offerPrices.Count() == 0)
                return null;

            var minPrice = offerPrices.Min();
            return minPrice;
        }

        /// <summary>
        /// Get the min price of the localisation, empty if no price
        /// filter by offerType if needed
        /// </summary>
        /// <returns>the min price string</returns>
        public string GetMinPriceString(int offerType = -1)
        {
            var price = GetMinPrice(offerType);
            if (price == null)
                return string.Empty;

            return string.Format(Worki.Resources.Models.Offer.Offer.PriceFrom, price.GetPriceDisplay());
        }

        static IEnumerable<LocalisationOffer> _OfferTypes = new List<LocalisationOffer> 
        {
            LocalisationOffer.BuisnessLounge,
		    LocalisationOffer.Workstation,
		    LocalisationOffer.Desktop,
		    LocalisationOffer.MeetingRoom,
		    LocalisationOffer.SeminarRoom,
		    LocalisationOffer.VisioRoom
        };

        /// <summary>
        /// Get the min price list, for each type of offer
        /// </summary>
        /// <returns>the min price list</returns>
        public IEnumerable<OfferPrice> GetMinPrices()
        {
            var toRet = new List<OfferPrice> ();
            foreach (var item in _OfferTypes)
            {
                var price = GetMinPrice((int)item);
                if (price != null)
                    toRet.Add(price);
            }

            return toRet;
        }

		/// <summary>
		/// Get all offers which have prices
		/// </summary>
		/// <returns>list of prices offers</returns>
		public IEnumerable<Offer> GetPricedOffers()
		{
			return Offers.Where(o => o.Price != 0);
		}

		/// <summary>
		/// Get amounts after commission processing
		/// </summary>
		/// <param name="totalAmount">total amount</param>
		/// <param name="ownerAmount">amount for owner</param>
		/// <param name="eworkyAmount">amount for eworky</param>
		public void GetAmounts(decimal totalAmount, out decimal ownerAmount, out decimal eworkyAmount)
		{
			ownerAmount = (1 - BookingCom / 100) * totalAmount;
			eworkyAmount = BookingCom / 100 * totalAmount;
		}

		/// <summary>
		/// Tell if Quotation should be paid
		/// </summary>
		public bool ShouldPayQuotation()
		{
			return QuotationPrice != 0;
		}

		/// <summary>
		/// Get price of a quotation
		/// </summary>
        public decimal GetQuotationPrice()
        {
            if (QuotationPrice != 0)
                return QuotationPrice;

            switch ((LocalisationType)TypeValue)
            {
                case LocalisationType.BuisnessCenter:
                    return 20;
                case LocalisationType.CoworkingSpace:
                default:
                    return 5;
            }
        }

		#endregion

        #region Availability

        /// <summary>
        /// Get the closer availability
        /// </summary>
        /// <param name="offerType">offer type to filter</param>
        /// <returns>the availability display</returns>
        public string GetMinAvailability(int offerType = -1)
        {
            if (!IsSharedOffice())
                return string.Empty;

            var offers = offerType == -1 ? Offers.Where(o => o.AvailabilityPeriod != 0) : Offers.Where(o => o.Type == offerType && o.AvailabilityPeriod != 0);

			if (offers.Count() == 0)
				return string.Empty;

            var min = offers.Min(o => o.AvailabilityPeriod);
            var minOffer = offers.FirstOrDefault(o => o.AvailabilityPeriod == min);

            if (minOffer == null)
                return string.Empty;

            return minOffer.GetAvailabilityDisplay(true);
        }

        #endregion

		#region Shared Office

		public const string DefaultSharedOfficeName = "Shared Office";

		public static List<int> CompanyTypes = new List<int>()
        {
			(int)eCompanyType.StartUp,
            (int)eCompanyType.CreativeAgency,
            (int)eCompanyType.Consulting,
            (int)eCompanyType.BigCompany,
            (int)eCompanyType.Independent,
			(int)eCompanyType.Association,
			(int)eCompanyType.SmallBusiness,
            (int)eCompanyType.Attorney,
            (int)eCompanyType.Medical
        };

		public static string GetCompanyType(int type)
		{
			var enumType = (eCompanyType)type;
			switch (enumType)
			{
				case eCompanyType.StartUp:
					return Worki.Resources.Models.Localisation.Localisation.StartUp;
				case eCompanyType.CreativeAgency:
					return Worki.Resources.Models.Localisation.Localisation.CreativeAgency;
				case eCompanyType.Consulting:
					return Worki.Resources.Models.Localisation.Localisation.Consulting;
				case eCompanyType.BigCompany:
					return Worki.Resources.Models.Localisation.Localisation.BigCompany;
				case eCompanyType.Independent:
					return Worki.Resources.Models.Localisation.Localisation.Independent;
				case eCompanyType.Association:
					return Worki.Resources.Models.Localisation.Localisation.Association;
				case eCompanyType.SmallBusiness:
					return Worki.Resources.Models.Localisation.Localisation.SmallBusiness;
                case eCompanyType.Attorney:
                    return Worki.Resources.Models.Localisation.Localisation.Attorney;
                case eCompanyType.Medical:
                    return Worki.Resources.Models.Localisation.Localisation.Medical;
				default:
					return string.Empty;
			}
		}

		public static string GetInCompanyType(eCompanyType type)
		{
			switch (type)
			{
				case eCompanyType.StartUp:
					return Worki.Resources.Models.Localisation.Localisation.InStartUp;
				case eCompanyType.CreativeAgency:
					return Worki.Resources.Models.Localisation.Localisation.InCreativeAgency;
				case eCompanyType.Consulting:
					return Worki.Resources.Models.Localisation.Localisation.InConsulting;
				case eCompanyType.BigCompany:
					return Worki.Resources.Models.Localisation.Localisation.InBigCompany;
				case eCompanyType.Independent:
					return Worki.Resources.Models.Localisation.Localisation.InIndependent;
				case eCompanyType.Association:
					return Worki.Resources.Models.Localisation.Localisation.InAssociation;
				case eCompanyType.SmallBusiness:
					return Worki.Resources.Models.Localisation.Localisation.InSmallBusiness;
                case eCompanyType.Attorney:
                    return Worki.Resources.Models.Localisation.Localisation.InAttorney;
                case eCompanyType.Medical:
                    return Worki.Resources.Models.Localisation.Localisation.InMedical;
				default:
					return string.Empty;
			}
		}

		public static Dictionary<int, string> GetCompanyTypes()
		{
			return CompanyTypes.ToDictionary(t => t, t => GetCompanyType(t));
		}

        /// <summary>
        /// Get name of the place
        /// </summary>
        /// <returns>name</returns>
		public string GetFullName()
		{
			if (!IsSharedOffice())
				return Name;
			else
			{
				var offerPart = string.Empty;
				var atPart = string.Empty;
				var offer = Offers.FirstOrDefault();
				if (offer != null)
				{
					offerPart = GetOfferType(offer.Type);
				}
				else
				{
					offerPart = GetOfferType((int)LocalisationOffer.Workstation);
				}

				atPart = GetInCompanyType((eCompanyType)CompanyType);

                if (string.IsNullOrEmpty(CompanyName))
                {
                    atPart = GetInCompanyType((eCompanyType)CompanyType);
                }
                else
                {
                    atPart = string.Format(Worki.Resources.Models.Localisation.Localisation.InCompany, CompanyName);
                }

				return offerPart + " " + atPart;
			}
		}

		#endregion

        #region Countries

        public static Dictionary<string, string> GetCountries()
        {
            return MiscHelpers.GetEnumDisplayName(typeof(CountryId), typeof(Worki.Resources.Models.Localisation.LocalisationCountry), Worki.Resources.Models.Localisation.LocalisationCountry.PickOne).OrderBy(kp => kp.Key == "" ? "" : kp.Value).ToDictionary(kp => kp.Key, kp => kp.Value);
        }

        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(GetDescription()))
            {
                yield return new ValidationResult(string.Format(Worki.Resources.Validation.ValidationString.Required, Worki.Resources.Models.Localisation.Localisation.Description), new[] { GetDescriptionName() });
            }
            else
            {
               var validateDescription = FormValidation.ValidateDescription(GetDescription(), string.Format(Worki.Resources.Validation.ValidationString.ProhibitedString, Worki.Resources.Models.Localisation.Localisation.Description), GetDescriptionName());
               if (validateDescription != null)
               {
                   yield return validateDescription;
               }
            }

        }

    }

	[Bind(Exclude = "Id,OwnerId")]
	public class Localisation_Validation
	{
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Name", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Name { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "TypeValue", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int TypeValue { get; set; }

		[Display(Name = "CompanyName", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public string CompanyName { get; set; }

		[Display(Name = "CompanyType", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int CompanyType { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Adress", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Adress { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "PostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string PostalCode { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "City", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string City { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Country", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [SelectStringValidation(ErrorMessageResourceName = "SelectOne", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string CountryId { get; set; }

		[Display(Name = "PhoneNumber", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(50, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string PhoneNumber { get; set; }

		[Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
		[Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Mail { get; set; }

		[StringLength(50, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Fax", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public string Fax { get; set; }

		[Display(Name = "WebSite", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		//[WebSite]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string WebSite { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Description { get; set; }

		[Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string DescriptionEn { get; set; }

		[Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string DescriptionEs { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		[Display(Name = "PublicTransportation", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string PublicTransportation { get; set; }

		[Display(Name = "Station", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Station { get; set; }

		[Display(Name = "RoadAccess", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string RoadAccess { get; set; }

        [Display(Name = "Facebook", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public string Facebook { get; set; }

        [Display(Name = "Twitter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public string Twitter { get; set; }

        [Display(Name = "BookingCom", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public decimal BookingCom { get; set; }

        [Display(Name = "QuotationPrice", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public decimal QuotationPrice { get; set; }

         [Display(Name = "DirectlyReceiveQuotation", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Boolean DirectlyReceiveQuotation { get; set; }
	}

    public class CommentProjection
    {
        public int Price { get; set; }
        public int Wifi { get; set; }
        public int Dispo { get; set; }
        public int Welcome { get; set; }
        public int Rating { get; set; }
    }

    public class LocalisationProjection :  IJsonProvider<LocalisationJson>
    {
        public int ID { get; set; }
        public int LocalisationType { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string LocalisationName { get; set; }
        public IEnumerable<int> Features { get; set; }
        public IEnumerable<int> OfferTypes { get; set; }
        public IEnumerable<CommentProjection> Ratings { get; set; }

        #region IJsonProvider

        public LocalisationJson GetJson()
        {
            return new LocalisationJson
            {
                id = ID,
                latitude = Latitude,
                longitude = Longitude,
                name = LocalisationName
            };
        }

        #endregion
    }

	public class LocalisationFormViewModel
	{
		#region Properties

		public Localisation Localisation { get; private set; }
		public SelectList Types { get; private set; }
		public SelectList CompanyTypes { get; private set; }
        public SelectList Countries { get; private set; }
		public bool IsFreeLocalisation { get; set; }
        public bool IsOwner { get; set; }
		public int NewOfferType { get; set; }
		public SelectList Offers { get; set; }
		public bool IsSharedOffice { get; set; }

        [Display(Name = "SendMailToOwner", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool SendMailToOwner { get; set; }

		#endregion

		#region Ctor

		public LocalisationFormViewModel()
		{
			Localisation = new Localisation();
			var offers = Localisation.GetOfferTypeDict(new List<LocalisationOffer> { LocalisationOffer.FreeArea });
			Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.BuisnessLounge);
			Localisation.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });
			Init();
		}

		public LocalisationFormViewModel(bool isFree, bool isShared = false)
		{
			Localisation = new Localisation();
			Localisation.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });
			Init(isFree, isShared);
		}

		public LocalisationFormViewModel(Localisation localisation)
		{
			Localisation = localisation;
			Init(localisation.IsFreeLocalisation(), localisation.IsSharedOffice());
		}

		void Init(bool isFree = true, bool isShared = false)
		{
            SendMailToOwner = true;
			IsFreeLocalisation = isFree;
			IsSharedOffice = isShared;
            IsOwner = true;
			if (isShared)
			{
				Localisation.TypeValue = (int)LocalisationType.SharedOffice;
			}

			var dict = isFree ? Localisation.GetFreeLocalisationTypes() : Localisation.GetNotFreeLocalisationTypes(isShared);
			Types = new SelectList(dict, "Key", "Value", LocalisationType.SpotWifi);
			var offers = Localisation.GetOfferTypeDict(isShared);
			Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.AllOffers);
			CompanyTypes = new SelectList(Localisation.GetCompanyTypes(), "Key", "Value", eCompanyType.StartUp);
            Countries = new SelectList(Localisation.GetCountries(), "Key", "Value");
            if (Localisation.LocalisationData == null)
                Localisation.LocalisationData = new LocalisationData();
            if (Localisation.MainLocalisation == null)
                Localisation.MainLocalisation = new MainLocalisation();
		}

		#endregion
	}

	public class LocalisationOfferViewModel
	{
		#region Properties

		public Localisation Localisation { get; set; }
		public IEnumerable<Offer> Offers { get; set; }
        public string Title { get; set; }
		public string Message { get; set; }

		#endregion

		#region Ctor

		public LocalisationOfferViewModel()
		{

		}

		public LocalisationOfferViewModel(Localisation localisation, LocalisationOffer offerType)
		{
			Localisation = localisation;
			Offers = Localisation.GetOffers(offerType);
            Title = Localisation.Name + " - " + Localisation.GetOfferType((int)offerType, false);
		}

		#endregion
	}

	public class LocalisationAskBookingFormModel
	{
		#region Properties

		public Localisation Localisation { get; set; }
		public Contact Contact { get; set; }

		#endregion

		#region Ctor

		public LocalisationAskBookingFormModel()
		{

		}

		public LocalisationAskBookingFormModel(Localisation localisation, Member member)
		{
			Localisation = localisation;
			Contact = new Contact { FirstName = member.MemberMainData.FirstName, LastName = member.MemberMainData.LastName, EMail = member.Email };
		}

		#endregion
	}

	#region Data Containers

	#endregion

	#region Equality Comparer

	public class LocalisationFeatureEqualityComparer : IEqualityComparer<LocalisationFeature>
	{
		#region IEqualityComparer<LocalisationFeature> Members

		public bool Equals(LocalisationFeature x, LocalisationFeature y)
		{
			//case of general feature
			if (x.OfferID == 0 || y.OfferID == 0)
				return x.FeatureID == y.FeatureID;
			else
				return x.FeatureID == y.FeatureID && x.OfferID == y.OfferID;
		}

		public int GetHashCode(LocalisationFeature obj)
		{
			return base.GetHashCode();
		}

		#endregion
	}

	#endregion

	#region Enums

	/// <summary>
	/// Describe comment rating types
	/// </summary>
	public enum RatingType
	{
		General,
		Price,
		Wifi,
		Dispo,
		Welcome
	}

	/// <summary>
	/// Localisation Type, correspond to the field TypeValue of Localisation
	/// </summary>
	public enum LocalisationType
	{
		SpotWifi,           //0
		CoffeeResto,
		Biblio,
		PublicSpace,
		TravelerSpace,
		Hotel,              //5
		Telecentre,
		BuisnessCenter,
		CoworkingSpace,
		WorkingHotel,
		PrivateArea,        //10
		SharedOffice
	}

	/// <summary>
	/// Company Type, correspond to the field CompanyType of Localisation, for shared office
	/// </summary>
	public enum eCompanyType
	{
		StartUp,
		CreativeAgency,
		Consulting,
		BigCompany,
		Independent,
		Association,
		SmallBusiness,
        Attorney,
        Medical
	}

	/// <summary>
	/// "Grouping" of Offer type, correspond to the field OfferId of LocalisationFeature
	/// </summary>
	public enum FeatureType
	{

		General,
		WorkingPlace,
		MeetingRoom,
		SeminarRoom,
		VisioRoom
	}

	/// <summary>
	/// Feature type, correspond to the field FeatureId of LocalisationFeature
	/// </summary>
	//[LocalizedEnum(ResourceType = typeof(Worki.Resources.Models.Localisation.LocalisationFeatures))]
	public enum Feature
	{
		Access24,
		LunchClose,
		BuisnessLounge,
		Workstation,
		Desktop,
		MeetingRoom,
		VisioRoom,
		SeminarRoom,
		Wifi_Free,
		Parking,
		Handicap,
		Restauration,
		Coffee,
		Visio,
		Concierge,
		Secretariat,
		Community,
		Domiciliation,
		Newspaper,
		Computers,
		Courier,
		Printer,
		ComputerHelp,
		Pressing,
		Outlet,
		FastInternet,
		TV,
		SafeStorage,
		RoomService,
		Archiving,
		RelaxingArea,
		AC,
		ErgonomicFurniture,
		Room2_7,
		Room7_20,
		Room20_plus,
		Room20_100,
		Room100_250,
		Room250_1000,
		Room1000_plus,
		Audio,
		Projector,
		VideoProj,
		Paperboard,
		Internet,
		PhoneJack,
		Drinks,
		Accommodation,
		Lighting,
		Sound,
		Catering,
		Scene,
		Micro,
		Picturesque,
		Welcoming,
		Wifi_Not_Free,
		Shower,
		Room1_4,
		Room5_10,
		Room10_Plus,
		VisioHD,
		Telepresence,
		LocalisationOwner,
		FreeArea,
		AvoidMorning,
		AvoidLunch,
		AvoidAfternoom,
		AvoidEvening,
		Desktop10_25,
		Desktop25_50,
		Desktop50_100,
		Desktop100Plus,
		Equipped,
		AvailableNow,
		AllInclusive,
		FlexibleContract,
		NotSectorial,
		CoworkingVibe,
		OpenToAll,
		ReservedToClients,
		PhoneLine,
		Kitchen,
		SharedMeetingRoom,
		Lift,
        Heater,
        Architects,
        Associative,
        Artists,
        Lawyers,
        BusinessDevelopers,
        Commercial,
        CommunicationMedia,
        Accountants,
        Consultants,
        Designers,
        Developers,
        Writers,
        Contractors,
        Independent,
        Investors,
        Journalists,
        Photographers,
        Nomads,

		//string features start from 1000, put bool features before this
		Sector = 1000,
		MinimalPeriod,
		ForCardOwner,

		//number features start from 2000, put string features before this
		CoffeePrice = 2000
	}

	/// <summary>
	/// Offer Type, kind of super feature types...
	/// </summary>
	public enum LocalisationOffer
	{
        AllOffers = -1,
		FreeArea,
		BuisnessLounge,
		Workstation,
		Desktop,
		MeetingRoom,
		SeminarRoom,
		VisioRoom
	}

    public enum CountryId
    {
        AF,     //Afghanistan
        AX,     //Åland Islands
        AL,		//Albania
        DZ,		//Algeria
        AS,		//American Samoa
        AD,		//Andorra
        AO,		//Angola
        AI,		//Anguilla
        AQ,		//Antarctica
        AG,		//Antigua and Barbuda
        AR,		//Argentina
        AM,		//Armenia
        AW,		//Aruba
        AU,		//Australia
        AT,		//Austria
        AZ,		//Azerbaijan
        BS,		//Bahamas
        BH,		//Bahrain
        BD,		//Bangladesh
        BB,		//Barbados
        BY,		//Belarus
        BE,		//Belgium
        BZ,		//Belize
        BJ,		//Benin
        BM,		//Bermuda
        BT,		//Bhutan
        BO,		//Bolivia
        BA,		//Bosnia and Herzegovina
        BW,		//Botswana
        BV,		//Bouvet Island
        BR,		//Brazil
        IO,		//British Indian Ocean Territory
        BN,		//Brunei Darussalam
        BG,		//Bulgaria
        BF,		//Burkina Faso
        BI,		//Burundi
        KH,		//Cambodia
        CM,		//Cameroon
        CA,		//Canada
        CV,		//Cape Verde
        KY,		//Cayman Islands
        CF,		//Central African Republic
        TD,		//Chad
        CL,		//Chile
        CN,		//China
        CX,		//Christmas Island
        CC,		//Cocos (Keeling) Islands
        CO,		//Colombia
        KM,		//Comoros
        CG,		//Congo
        CD,		//Congo, The Democratic Republic of The
        CK,		//Cook Islands
        CR,		//Costa Rica
        CI,		//Cote D'ivoire
        HR,		//Croatia
        CU,		//Cuba
        CY,		//Cyprus
        CZ,		//Czech Republic
        DK,		//Denmark
        DJ,		//Djibouti
        DM,		//Dominica
        DO,		//Dominican Republic
        EC,		//Ecuador
        EG,		//Egypt
        SV,		//El Salvador
        GQ,		//Equatorial Guinea
        ER,		//Eritrea
        EE,		//Estonia
        ET,		//Ethiopia
        FK,		//Falkland Islands (Malvinas)
        FO,		//Faroe Islands
        FJ,		//Fiji
        FI,		//Finland
        FR,		//France
        GF,		//French Guiana
        PF,		//French Polynesia
        TF,		//French Southern Territories
        GA,		//Gabon
        GM,		//Gambia
        GE,		//Georgia
        DE,		//Germany
        GH,		//Ghana
        GI,		//Gibraltar
        GR,		//Greece
        GL,		//Greenland
        GD,		//Grenada
        GP,		//Guadeloupe
        GU,		//Guam
        GT,		//Guatemala
        GG,		//Guernsey
        GN,		//Guinea
        GW,		//Guinea-bissau
        GY,		//Guyana
        HT,		//Haiti
        HM,		//Heard Island and Mcdonald Islands
        VA,		//Holy See (Vatican City State)
        HN,		//Honduras
        HK,		//Hong Kong
        HU,		//Hungary
        IS,		//Iceland
        IN,		//India
        ID,		//Indonesia
        IR,		//Iran, Islamic Republic of
        IQ,		//Iraq
        IE,		//Ireland
        IM,		//Isle of Man
        IL,		//Israel
        IT,		//Italy
        JM,		//Jamaica
        JP,		//Japan
        JE,		//Jersey
        JO,		//Jordan
        KZ,		//Kazakhstan
        KE,		//Kenya
        KI,		//Kiribati
        KP,		//Korea, Democratic People's Republic of
        KR,		//Korea, Republic of
        KW,		//Kuwait
        KG,		//Kyrgyzstan
        LA,		//Lao People's Democratic Republic
        LV,		//Latvia
        LB,		//Lebanon
        LS,		//Lesotho
        LR,		//Liberia
        LY,		//Libyan Arab Jamahiriya
        LI,		//Liechtenstein
        LT,		//Lithuania
        LU,		//Luxembourg
        MO,		//Macao
        MK,		//Macedonia, The Former Yugoslav Republic of
        MG,		//Madagascar
        MW,		//Malawi
        MY,		//Malaysia
        MV,		//Maldives
        ML,		//Mali
        MT,		//Malta
        MH,		//Marshall Islands
        MQ,		//Martinique
        MR,		//Mauritania
        MU,		//Mauritius
        YT,		//Mayotte
        MX,		//Mexico
        FM,		//Micronesia, Federated States of
        MD,		//Moldova, Republic of
        MC,		//Monaco
        MN,		//Mongolia
        ME,		//Montenegro
        MS,		//Montserrat
        MA,		//Morocco
        MZ,		//Mozambique
        MM,		//Myanmar
        NA,		//Namibia
        NR,		//Nauru
        NP,		//Nepal
        NL,		//Netherlands
        AN,		//Netherlands Antilles
        NC,		//New Caledonia
        NZ,		//New Zealand
        NI,		//Nicaragua
        NE,		//Niger
        NG,		//Nigeria
        NU,		//Niue
        NF,		//Norfolk Island
        MP,		//Northern Mariana Islands
        NO,		//Norway
        OM,		//Oman
        PK,		//Pakistan
        PW,		//Palau
        PS,		//Palestinian Territory, Occupied
        PA,		//Panama
        PG,		//Papua New Guinea
        PY,		//Paraguay
        PE,		//Peru
        PH,		//Philippines
        PN,		//Pitcairn
        PL,		//Poland
        PT,		//Portugal
        PR,		//Puerto Rico
        QA,		//Qatar
        RE,		//Reunion
        RO,		//Romania
        RU,		//Russian Federation
        RW,		//Rwanda
        SH,		//Saint Helena
        KN,		//Saint Kitts and Nevis
        LC,		//Saint Lucia
        PM,		//Saint Pierre and Miquelon
        VC,		//Saint Vincent and The Grenadines
        WS,		//Samoa
        SM,		//San Marino
        ST,		//Sao Tome and Principe
        SA,		//Saudi Arabia
        SN,		//Senegal
        RS,		//Serbia
        SC,		//Seychelles
        SL,		//Sierra Leone
        SG,		//Singapore
        SK,		//Slovakia
        SI,		//Slovenia
        SB,		//Solomon Islands
        SO,		//Somalia
        ZA,		//South Africa
        GS,		//South Georgia and The South Sandwich Islands
        ES,		//Spain
        LK,		//Sri Lanka
        SD,		//Sudan
        SR,		//Suriname
        SJ,		//Svalbard and Jan Mayen
        SZ,		//Swaziland
        SE,		//Sweden
        CH,		//Switzerland
        SY,		//Syrian Arab Republic
        TW,		//Taiwan, Province of China
        TJ,		//Tajikistan
        TZ,		//Tanzania, United Republic of
        TH,		//Thailand
        TL,		//Timor-leste
        TG,		//Togo
        TK,		//Tokelau
        TO,		//Tonga
        TT,		//Trinidad and Tobago
        TN,		//Tunisia
        TR,		//Turkey
        TM,		//Turkmenistan
        TC,		//Turks and Caicos Islands
        TV,		//Tuvalu
        UG,		//Uganda
        UA,		//Ukraine
        AE,		//United Arab Emirates
        GB,		//United Kingdom
        US,		//United States
        UM,		//United States Minor Outlying Islands
        UY,		//Uruguay
        UZ,		//Uzbekistan
        VU,		//Vanuatu
        VE,		//Venezuela
        VN,		//Viet Nam
        VG,		//Virgin Islands, British
        VI,		//Virgin Islands, U.S.
        WF,		//Wallis and Futuna
        EH,		//Western Sahara
        YE,		//Yemen
        ZM,		//Zambia
        ZW,		//Zimbabwe
    }

	#endregion

	[MetadataType(typeof(Comment_Validation))]
	public partial class Comment : IJsonProvider<CommentJson>// : IDataErrorInfo, IValidatableObject
	{
		public const string DisplayRelatedLocalisation = "DisplayRelatedLocalisation";

		public Comment()
		{
			Rating = -1;
			RatingPrice = -1;
			RatingWifi = -1;
			RatingWelcome = -1;
			RatingDispo = -1;
		}

		public CommentJson GetJson()
		{
			return new CommentJson
			{
				id = ID,
				date = Date,
				post = GetPost(),
				rating = Rating,
				ratingWifi = RatingWifi,
				ratingDispo = RatingDispo,
				ratingPrice = RatingPrice,
				ratingWelcome = RatingWelcome,
                author = Member.GetJson()
			};
		}

		//#region IDataErrorInfo

		//public string Error
		//{
		//    get { return string.Empty; }
		//}

		//public string this[string columnName]
		//{
		//    get
		//    {
		//        switch (columnName)
		//        {
		//            //case "Rating":
		//            //    if (Rating < 0)
		//            //        return "Oups, il semble que vous ayez oublié d'attribuer une note !";
		//            //    else
		//            //        return string.Empty;
		//            default:
		//                return string.Empty;
		//        }
		//    }
		//}

		//#endregion

		public double GetRating()
		{
			if (Localisation == null)
				return -1;
			try
			{

				if (!Localisation.IsFreeLocalisation())
					return Rating;
				else
				{
					var ratings = new List<double>
                                { 
                                    RatingPrice,
                                    RatingWifi, 
                                    RatingDispo, 
                                    RatingWelcome 
                                };
					return ratings.Where(d => d >= 0).Average();
				}

			}
			catch (Exception)
			{
				return -1;
			}
		}

        public string GetPost()
        {
            return Post;
        }

        public bool HasPost(Culture culture)
        {
            return PostLanguage == (int)culture;
        }

		public bool HasPost()
		{
			return !string.IsNullOrEmpty(Post);
		}
		/// <summary>
		/// Validate object, thow exception if not valid
		/// </summary>
		public void Validate(ref string error)
		{
			string commentError = Worki.Resources.Validation.ValidationString.AlreadyRateThis;
			if (!Localisation.IsFreeLocalisation())
			{
				if (Rating < 0 && !HasPost())
				{
					error = commentError;
					throw new Exception(error);
				}
			}
			else
			{
                if (RatingDispo < 0 && RatingPrice < 0 && RatingWelcome < 0 && RatingWifi < 0 && !HasPost())
				{
					error = commentError;
					throw new Exception(error);
				}
			}
		}
	}

	[Bind(Exclude = "Id,PostUserID")]
	public class Comment_Validation
	{
		[Display(Name = "Post", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Post { get; set; }

		[Range(-1, 5)]
		[Display(Name = "Rating", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int Rating { get; set; }

		[Range(-1, 5)]
		[Display(Name = "RatingPrice", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int RatingPrice { get; set; }

		[Range(-1, 5)]
		[Display(Name = "RatingWifi", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int RatingWifi { get; set; }

		[Range(-1, 5)]
		[Display(Name = "RatingDispo", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int RatingDispo { get; set; }

		[Range(-1, 5)]
		[Display(Name = "RatingWelcome", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int RatingWelcome { get; set; }
	}

	[MetadataType(typeof(LocalisationData_Validation))]
	public partial class LocalisationData
	{

	}

	public class LocalisationData_Validation
	{
		[Display(Name = "MonOpenMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public Nullable<System.DateTime> MonOpenMorning { get; set; }

        [Display(Name = "MonCloseMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> MonCloseMorning { get; set; }

        [Display(Name = "MonOpenAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> MonOpenAfter { get; set; }

        [Display(Name = "MonCloseAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> MonCloseAfter { get; set; }

        [Display(Name = "TueOpenMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> TueOpenMorning { get; set; }

        [Display(Name = "TueCloseMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> TueCloseMorning { get; set; }

        [Display(Name = "TueOpenAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> TueOpenAfter { get; set; }

        [Display(Name = "TueCloseAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> TueCloseAfter { get; set; }

        [Display(Name = "WedOpenMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> WedOpenMorning { get; set; }

        [Display(Name = "WedCloseMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> WedCloseMorning { get; set; }

        [Display(Name = "WedOpenAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> WedOpenAfter { get; set; }

        [Display(Name = "WedCloseAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> WedCloseAfter { get; set; }

        [Display(Name = "ThuOpenMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> ThuOpenMorning { get; set; }

        [Display(Name = "ThuCloseMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> ThuCloseMorning { get; set; }

        [Display(Name = "ThuOpenAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> ThuOpenAfter { get; set; }

        [Display(Name = "ThuCloseAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> ThuCloseAfter { get; set; }

        [Display(Name = "FriOpenMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> FriOpenMorning { get; set; }

        [Display(Name = "FriCloseMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> FriCloseMorning { get; set; }

        [Display(Name = "FriOpenAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> FriOpenAfter { get; set; }

        [Display(Name = "FriCloseAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> FriCloseAfter { get; set; }

        [Display(Name = "SatOpenMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SatOpenMorning { get; set; }

        [Display(Name = "SatCloseMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SatCloseMorning { get; set; }

        [Display(Name = "SatOpenAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SatOpenAfter { get; set; }

        [Display(Name = "SatCloseAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SatCloseAfter { get; set; }

        [Display(Name = "SunOpenMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SunOpenMorning { get; set; }

        [Display(Name = "SunCloseMorning", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SunCloseMorning { get; set; }

        [Display(Name = "SunOpenAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SunOpenAfter { get; set; }

        [Display(Name = "SunCloseAfter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public Nullable<System.DateTime> SunCloseAfter { get; set; }
	}

	public partial class LocalisationFeature : IFeatureContainer
	{
		public Feature Feature
		{
			get { return (Feature)FeatureID; }
		}
	}
}
