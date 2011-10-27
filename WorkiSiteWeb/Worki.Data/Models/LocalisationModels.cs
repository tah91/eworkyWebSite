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

namespace Worki.Data.Models
{
	[MetadataType(typeof(Localisation_Validation))]
	public partial class Localisation : IJsonProvider<LocalisationJson>, IPictureDataProvider, IMapModelProvider, IFeatureContainer// : IDataErrorInfo
	{
		#region Data Container Ctor

		//public void FillFromPictureData(PictureData picData)
		//{
		//    if (picData == null)
		//        return;
		//    LocalisationFiles.Clear();
		//    foreach (var item in picData.Files)
		//    {
		//        LocalisationFiles.Add(item);
		//    }
		//}

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

		#region Static Members

		//public static Dictionary<int, string> LocalisationFeatureDict = MiscHelpers.GetEnumDescriptors(typeof(Feature));

		public static string GetFeatureDisplayName(Feature feature)
		{
			var enumType = typeof(Feature);
			var enumResxType = typeof(Worki.Resources.Models.Localisation.LocalisationFeatures);
			var enumStr = Enum.GetName(enumType, feature);

			var nameProperty = enumResxType.GetProperty(enumStr, BindingFlags.Static | BindingFlags.Public);
			if (nameProperty != null)
			{
				enumStr = (string)nameProperty.GetValue(nameProperty.DeclaringType, null);
			}
			return enumStr;
		}


		//to remove
		//public static Dictionary<FeatureType, string> LocalisationFeatureTypes = new Dictionary<FeatureType, string>()
		//{
		//    { FeatureType.General, string.Empty},
		//    { FeatureType.WorkingPlace, Worki.Resources.Models.Localisation.Localisation.WorkingPlace},
		//    { FeatureType.MeetingRoom, Worki.Resources.Models.Localisation.Localisation.MeetingRoom},
		//    { FeatureType.SeminarRoom, Worki.Resources.Models.Localisation.Localisation.SeminarRoom},
		//    { FeatureType.VisioRoom, Worki.Resources.Models.Localisation.Localisation.VisioRoom}
		//};

		//public static string GetFeatureDesc(Feature feat, FeatureType featType)
		//{
		//    var first = Enum.GetName(typeof(Feature), feat);
		//    var sec = Enum.GetName(typeof(FeatureType), featType);
		//    return first + "-" + sec;
		//}

		//public static bool GetFeatureDesc(string str, out KeyValuePair<int, int> toFill)
		//{
		//    toFill = new KeyValuePair<int, int>(0, 0);
		//    var strs = str.Split('-');
		//    if (strs.Length != 2)
		//        return false;
		//    var first = strs[0];
		//    var sec = strs[1];
		//    if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(sec))
		//        return false;
		//    try
		//    {
		//        var firstVal = (int)Enum.Parse(typeof(Feature), first);
		//        var secVal = (int)Enum.Parse(typeof(FeatureType), sec);
		//        toFill = new KeyValuePair<int, int>(firstVal, secVal);
		//    }
		//    catch (ArgumentException)
		//    {
		//        return false;
		//    }
		//    return true;
		//}

		#endregion

		#region IFeatureContainer

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

		public static List<Feature> AvoidPeriods = new List<Feature>()
		{
			Feature.AvoidMorning,
			Feature.AvoidLunch, 
			Feature.AvoidAfternoom, 
			Feature.AvoidEvening
		};

		public static List<Feature> Characteristics = new List<Feature>()
		{
			Feature.Handicap,
			Feature.Wifi_Free, 
			Feature.Wifi_Not_Free, 
			Feature.Parking,
			Feature.Outlet,
			Feature.FastInternet, 
			Feature.SafeStorage, 
			Feature.Coffee,
			Feature.Restauration,
			Feature.AC, 
			Feature.ErgonomicFurniture, 
			Feature.Shower,
			Feature.Newspaper,
			Feature.TV
		};

		public static List<Feature> Services = new List<Feature>()
		{
			Feature.Domiciliation,
			Feature.Secretariat, 
			Feature.Courier, 
			Feature.Printer,
			Feature.Computers,
			Feature.Archiving, 
			Feature.Concierge, 
			Feature.Pressing,
			Feature.ComputerHelp,
			Feature.RoomService, 
			Feature.Community,
			Feature.RelaxingArea
		};

		public static List<Feature> BuisnessLounge = new List<Feature>()
        {
            Feature.OpenToAll,
            Feature.ReservedToClients,
            Feature.ForCardOwner
        };

        public static List<Feature> Workstation = new List<Feature>()
        {
            Feature.Sector,
            Feature.NotSectorial,
            Feature.CoworkingVibe
        };

        public static List<Feature> Desktop = new List<Feature>()
        {
            Feature.Desktop10_25,
            Feature.Desktop25_50,
            Feature.Desktop50_100,
            Feature.Desktop100Plus,
            Feature.Equipped,
            Feature.AvailableNow,
            Feature.AllInclusive,
            Feature.FlexibleContract,
            Feature.MinimalPeriod
        };

        public static List<Feature> MeetingRoom = new List<Feature>() 
        {
            Feature.Room2_7,
            Feature.Room7_20,
            Feature.Room20_plus,
            Feature.Audio,
            Feature.Visio,
            Feature.Projector,
            Feature.VideoProj,
            Feature.Paperboard,
            Feature.Internet,
            Feature.PhoneJack,
            Feature.Drinks
        };

        public static List<Feature> SeminarRoom = new List<Feature>() 
        {
            Feature.Room20_100,
            Feature.Room100_250,
            Feature.Room250_1000,
            Feature.Room1000_plus,
            Feature.Scene,
            Feature.Micro,
            Feature.Picturesque,
            Feature.Welcoming,
            Feature.Accommodation,
            Feature.Lighting,
            Feature.Sound,
            Feature.Catering
        };

        public static List<Feature> VisioRoom = new List<Feature>() 
        {
            Feature.Room1_4,
            Feature.Room5_10,
            Feature.Room10_Plus,
            Feature.Visio,
            Feature.VisioHD,
            Feature.Telepresence,
            Feature.Paperboard,
            Feature.Internet,
            Feature.PhoneJack,
            Feature.Drinks
        };

		public List<Feature> GetFeaturesWithin(IEnumerable<Feature> toInclude)
		{
			var toRet = new List<Feature>();
			return (from item
						in LocalisationFeatures
					where toInclude.Contains((Feature)item.FeatureID)
					select (Feature)item.FeatureID).ToList();
		}

		//public List<Feature> GetFeaturesOfType(FeatureType featureType, IEnumerable<Feature> toExclude = null)
		//{
		//    var toRet = new List<Feature>();
		//    var featureToCheck = toExclude == null ? LocalisationFeatures : LocalisationFeatures.Where(f => !toExclude.Contains((Feature)f.FeatureID));
		//    if (featureToCheck == null)
		//        return toRet;
		//    return (from item in featureToCheck where item.OfferID == (int)featureType select (Feature)item.FeatureID).ToList();
		//}

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

		//public bool HasFeature(Feature feature, FeatureType featureType)
		//{
		//    return HasFeature((int)feature, (int)featureType);
		//}

		//public bool HasFeature(int feature, int featureType)
		//{
		//    var equalityComparer = new LocalisationFeatureEqualityComparer();
		//    return LocalisationFeatures.Contains(new LocalisationFeature { FeatureID = feature, OfferID = featureType }, equalityComparer);
		//}

		/// <summary>
		/// Check if localisation have offer except a given offer
		/// </summary>
		/// <param name="offer">offer to exclude</param>
		/// <returns>true if it is the case</returns>
		//public bool HasOfferExcept(LocalisationOffer offer)
		//{
		//    var feature = GetFeatureFromOfferType((int)offer);
		//    var types = Localisation.OfferTypes.Except(new List<int> { (int)feature }).ToList();
		//    return LocalisationFeatures.Where(f => types.Contains(f.FeatureID)).Count() > 0;
		//}

		#endregion

		#region Localisation Types

		public static List<int> LocalisationTypes = new List<int>()
        {
            (int)LocalisationType.SpotWifi,
            (int)LocalisationType.CoffeeResto,
            (int)LocalisationType.Biblio,
            (int)LocalisationType.PublicSpace,
			(int)LocalisationType.TravelerSpace,
            (int)LocalisationType.Telecentre,
            (int)LocalisationType.BuisnessCenter,
            (int)LocalisationType.CoworkingSpace,
 			(int)LocalisationType.WorkingHotel,
			(int)LocalisationType.PrivateArea
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
			var notFree = LocalisationTypes.Except(FreeLocalisationTypes);
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

		public static string GetOfferType(int type)
		{
			var enumType = (LocalisationOffer)type;
			switch (enumType)
			{
				case LocalisationOffer.AllOffers:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.AllOffers;
				case LocalisationOffer.FreeArea:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.FreeArea;
				case LocalisationOffer.BuisnessLounge:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.BuisnessRoom;
				case LocalisationOffer.Workstation:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.SingleWorkstation;
				case LocalisationOffer.Desktop:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.SingleSingleDesk;
				case LocalisationOffer.MeetingRoom:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.SingleMeetingRoom;
				case LocalisationOffer.SeminarRoom:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.SingleSeminarRoom;
				case LocalisationOffer.VisioRoom:
					return Worki.Resources.Models.Localisation.LocalisationFeatures.SingleVisioRoom;
				default:
					return string.Empty;
			}
		}

		//public static List<int> OfferTypes = new List<int>()
		//{
		//    (int)Feature.BuisnessLounge,
		//    (int)Feature.Workstation,
		//    (int)Feature.MeetingRoom,
		//    (int)Feature.SeminarRoom,
		//    (int)Feature.VisioRoom,
		//    (int)Feature.Desktop,
		//    (int)Feature.FreeArea
		//};

		public static Dictionary<int, string> GetOfferTypes()
		{
			return OfferTypes.ToDictionary(o => o, o => GetOfferType(o));
		}

		public static Dictionary<int, string> GetOfferTypeDict(IEnumerable<LocalisationOffer> except)
		{
			var toRet = (from item in GetOfferTypes() where !except.Contains((LocalisationOffer)item.Key) select item).ToDictionary(k => k.Key, k => k.Value);
			return toRet;
		}

		public bool HasOffer(LocalisationOffer offer)
		{
			return Offers.Where(o => o.Type == (int)offer).Count() != 0;
			//var featureType = (FeatureType)GetFeatureTypeFromOfferType((int)offer);
			//var feature = GetFeatureFromOfferType((int)offer);
			//return HasFeature(feature, featureType);
		}

		/// <summary>
		/// Check if localisation have an offer
		/// </summary>
		/// <returns>true if it is the case</returns>
		public bool HasOffer()
		{
			return Offers.Count != 0;
			//return LocalisationFeatures.Where(f => Localisation.OfferTypes.Contains(f.FeatureID)).Count() > 0;
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
					return MonOpen != null || MonOpen2 != null || MonClose != null || MonClose2 != null;
				case DayOfWeek.Tuesday:
					return TueOpen != null || TueOpen2 != null || TueClose != null || TueClose2 != null;
				case DayOfWeek.Wednesday:
					return WedOpen != null || MonOpen2 != null || WedClose != null || WedClose2 != null;
				case DayOfWeek.Thursday:
					return ThuOpen != null || ThuOpen2 != null || ThuClose != null || ThuClose2 != null;
				case DayOfWeek.Friday:
					return FriOpen != null || FriOpen2 != null || FriClose != null || FriClose2 != null;
				case DayOfWeek.Saturday:
					return SatOpen != null || SatOpen2 != null || SatClose != null || SatClose2 != null;
				case DayOfWeek.Sunday:
					return SunOpen != null || SunOpen2 != null || SunClose != null || SunClose2 != null;
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
			return (string.IsNullOrEmpty(Fax) && string.IsNullOrEmpty(PhoneNumber) && string.IsNullOrEmpty(WebSite) && string.IsNullOrEmpty(Mail));
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
					return GetOpenningTime(MonOpen, MonClose2, MonOpen2, MonClose);
				case DayOfWeek.Tuesday:
					return GetOpenningTime(TueOpen, TueClose2, TueOpen2, TueClose);
				case DayOfWeek.Wednesday:
					return GetOpenningTime(WedOpen, WedClose2, WedOpen2, WedClose);
				case DayOfWeek.Thursday:
					return GetOpenningTime(ThuOpen, ThuClose2, ThuOpen2, ThuClose);
				case DayOfWeek.Friday:
					return GetOpenningTime(FriOpen, FriClose2, FriOpen2, FriClose);
				case DayOfWeek.Saturday:
					return GetOpenningTime(SatOpen, SatClose2, SatOpen2, SatClose);
				case DayOfWeek.Sunday:
					return GetOpenningTime(SunOpen, SunClose2, SunOpen2, SunClose);
				default:
					return string.Empty;
			}
		}

		public string GetAvoidString()
		{
			var toRet = string.Empty;
			if (!HasFeatureIn(AvoidPeriods))
				return toRet;
			foreach (var item in AvoidPeriods)
			{
				if (HasFeature(item))
					toRet += Localisation.GetFeatureDisplayName(item).ToLower() + ", ";
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
            return Name;
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

		[StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Description { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		#region Openning times

		public DateTime? MonOpen { get; set; }

		public DateTime? MonClose { get; set; }

		public DateTime? TueOpen { get; set; }

		public DateTime? TueClose { get; set; }

		public DateTime? WedOpen { get; set; }

		public DateTime? WedClose { get; set; }

		public DateTime? ThuOpen { get; set; }

		public DateTime? ThuClose { get; set; }

		public DateTime? FriOpen { get; set; }

		public DateTime? FriClose { get; set; }

		public DateTime? SatOpen { get; set; }

		public DateTime? SatClose { get; set; }

		public DateTime? SunOpen { get; set; }

		public DateTime? SunClose { get; set; }

		#endregion

		[Display(Name = "PublicTransportation", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string PublicTransportation { get; set; }

		[Display(Name = "Station", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Station { get; set; }

		[Display(Name = "RoadAccess", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string RoadAccess { get; set; }

        [Display(Name = "Bookable", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool Bookable { get; set; }
	}

	public class LocalisationListViewModel
	{
		public IList<Localisation> Localisations { get; set; }
		public PagingInfo PagingInfo { get; set; }
	}

	public class LocalisationFormViewModel
	{
		#region Properties

		public Localisation Localisation { get; private set; }
		public SelectList Types { get; private set; }
		public bool IsFreeLocalisation { get; set; }

		#endregion

		#region Ctor

		public LocalisationFormViewModel()
		{
			Localisation = new Localisation();
			Localisation.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });
			Init();
		}

		public LocalisationFormViewModel(bool isFree)
		{
			Localisation = new Localisation();
			Localisation.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });
			Init(isFree);
		}

		public LocalisationFormViewModel(Localisation localisation)
		{
			Localisation = localisation;
			Init(localisation.IsFreeLocalisation());
		}

		void Init(bool isFree = true)
		{
			IsFreeLocalisation = isFree;
			var dict = isFree ? Localisation.GetFreeLocalisationTypes() : Localisation.GetNotFreeLocalisationTypes();
			Types = new SelectList(dict, "Key", "Value", LocalisationType.SpotWifi);
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
		PrivateArea
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
				if (Rating < 0)
				{
					error = commentError;
					throw new Exception(error);
				}
			}
			else
			{
				if (RatingDispo < 0 || RatingPrice < 0 || RatingWelcome < 0 || RatingWifi < 0)
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
		[Display(Name = "CoffeePrice", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int CoffeePrice { get; set; }
	}
}
