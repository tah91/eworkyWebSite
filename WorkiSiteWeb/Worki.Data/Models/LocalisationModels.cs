﻿using System;
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

namespace Worki.Data.Models
{
	[MetadataType(typeof(Localisation_Validation))]
	public partial class Localisation : IJsonProvider<LocalisationJson>, IPictureDataProvider, IMapModelProvider, IFeatureProvider// : IDataErrorInfo
    {
        #region Ctor

        #endregion

        #region IJsonProvider

        public LocalisationJson GetJson()
        {
            return new LocalisationJson
            {
                id = ID,
                latitude = Latitude,
                longitude = Longitude,
                name = Name,
                description = Description,
                address = Adress,
                city = City,
                rating = GetRatingAverage(RatingType.General),
				type = Localisation.GetLocalisationType(TypeValue)
            };
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

		public static Dictionary<int, string> GetLocalisationTypes()
		{
			return LocalisationTypes.ToDictionary(t => t, t => GetLocalisationType(t));
		}

		public static Dictionary<int, string> GetFreeLocalisationTypes()
		{
			return FreeLocalisationTypes.ToDictionary(t => t, t => GetLocalisationType(t));
		}

		public static Dictionary<int, string> GetNotFreeLocalisationTypes()
		{
			var notFree = LocalisationTypes.Except(FreeLocalisationTypes).Except(new List<int> { (int)LocalisationType.SharedOffice });
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

        public static string GetOfferType(int type, bool single = true)
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
                    return single ? Worki.Resources.Models.Localisation.LocalisationFeatures.SingleWorkstation : Worki.Resources.Models.Localisation.LocalisationFeatures.Workstation;
                case LocalisationOffer.Desktop:
                    return single ? Worki.Resources.Models.Localisation.LocalisationFeatures.SingleDesktop : Worki.Resources.Models.Localisation.LocalisationFeatures.Desktop;
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

		public static Dictionary<int, string> GetOfferTypes()
		{
			return OfferTypes.ToDictionary(o => o, o => GetOfferType(o));
		}

		public static Dictionary<int, string> GetOfferTypeDict(IEnumerable<LocalisationOffer> except)
		{
			var toRet = (from item in GetOfferTypes() where !except.Contains((LocalisationOffer)item.Key) select item).ToDictionary(k => k.Key, k => k.Value);
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

		public IEnumerable<Offer> GetOffers(LocalisationOffer offerType)
		{
			return Offers.Where(o => o.IsOnline && o.Type == (int)offerType);
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

		public IEnumerable<Comment> GetOrderedComments()
		{
			var withComm = (from item
						   in Comments
							where !string.IsNullOrEmpty(item.Post)
							orderby item.Date descending
							select item);

			var withoutComm = (from item
								 in Comments
							   where string.IsNullOrEmpty(item.Post)
							   orderby item.Date descending
							   select item);

			var toRet = withComm.Concat(withoutComm);
			return toRet;
		}

		public IEnumerable<Comment> GetCommentSummary()
		{
			var toRet = (from item
						   in Comments
						 where item.Member != null
						 orderby item.Date descending
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

		#endregion

		#region Render strings

		/// <summary>
		/// Render datetime to readable string
		/// </summary>
		/// <param name="date">the date to render</param>
		/// <returns>the date string</returns>
		public string RenderDate(DateTime date)
		{
			var temp = string.Format("{0:H:mm}", date);
			return temp;
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

			return toRet.Replace(':', 'h');
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
			return string.Format(Worki.Resources.Models.Localisation.Localisation.DisplayName, Name, Localisation.GetLocalisationType(TypeValue), City);
        }

        public string GetDescription()
        {
            return Description;
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
		/// Get the min price of the localisation, empty if no price
		/// filter by offerType if needed
		/// </summary>
		/// <returns>the min price string</returns>
		public string GetMinPrice(int offerType = -1)
		{
			var offers = offerType == -1 ? Offers.Where(o => o.Price != 0) : Offers.Where(o => o.Type == offerType && o.Price != 0);

			if (offers.Count() == 0)
				return string.Empty;

			var min = offers.Min(o => o.Price);
			var minOffer = offers.FirstOrDefault(o => o.Price == min);

			if (minOffer == null)
				return string.Empty;

			return string.Format(Worki.Resources.Models.Offer.Offer.PriceFrom, minOffer.GetPriceDisplay());
		}

		/// <summary>
		/// Get all offers which have prices
		/// </summary>
		/// <returns>list of prices offers</returns>
		public IEnumerable<Offer> GetPricedOffers()
		{
			return Offers.Where(o => o.Price != 0);
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

            var min = offers.Min(o => o.AvailabilityPeriod);
            var minOffer = offers.FirstOrDefault(o => o.AvailabilityPeriod == min);

            if (minOffer == null)
                return string.Empty;

            return minOffer.GetAvailabilityDisplay(true);
        }

        #endregion
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
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Country { get; set; }

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
	}

	public class LocalisationFormViewModel
	{
		#region Properties

		public Localisation Localisation { get; private set; }
		public SelectList Types { get; private set; }
		public bool IsFreeLocalisation { get; set; }
        public bool IsOwner { get; set; }
		public int NewOfferType { get; set; }
		public SelectList Offers { get; set; }
		public bool IsSharedOffice { get; set; }

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
			IsFreeLocalisation = isFree;
			IsSharedOffice = isShared;
			if (isShared)
			{
				Localisation.TypeValue = (int)LocalisationType.SharedOffice;
				IsOwner = true;
			}

			var dict = isFree ? Localisation.GetFreeLocalisationTypes() : Localisation.GetNotFreeLocalisationTypes();
			Types = new SelectList(dict, "Key", "Value", LocalisationType.SpotWifi);
            var offers = Localisation.GetOfferTypeDict(isShared);
			Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.AllOffers);
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

		public Localisation Localisation { get; private set; }
		public IEnumerable<Offer> Offers { get; private set; }

		#endregion

		#region Ctor

		public LocalisationOfferViewModel(Localisation localisation, LocalisationOffer offerType)
		{
			Localisation = localisation;
			Offers = Localisation.GetOffers(offerType);
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
		SpotWifi,
		CoffeeResto,
		Biblio,
		PublicSpace,
		TravelerSpace,
		Hotel,
		Telecentre,
		BuisnessCenter,
		CoworkingSpace,
		WorkingHotel,
		PrivateArea,
		SharedOffice
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
				post = Post,
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

		/// <summary>
		/// Validate object, thow exception if not valid
		/// </summary>
		public void Validate(ref string error)
		{
			string commentError = Worki.Resources.Validation.ValidationString.AlreadyRateThis;
			if (!Localisation.IsFreeLocalisation())
			{
				if (Rating < 0 && string.IsNullOrEmpty(Post))
				{
					error = commentError;
					throw new Exception(error);
				}
			}
			else
			{
                if (RatingDispo < 0 && RatingPrice < 0 && RatingWelcome < 0 && RatingWifi < 0 && string.IsNullOrEmpty(Post))
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
