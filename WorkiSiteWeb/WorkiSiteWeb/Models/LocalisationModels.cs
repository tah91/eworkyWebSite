using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Worki.Web.Helpers;
using Worki.Web.Infrastructure;
using System.Runtime.Serialization;

namespace Worki.Web.Models
{
	[MetadataType(typeof(Localisation_Validation))]
	public partial class Localisation// : IDataErrorInfo
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

		public LocalisationJson GetJson()
		{
			var image = LocalisationFiles.Where(f => f.IsDefault == true).FirstOrDefault();
			var imageUrl = image == null ? string.Empty : MiscHelpers.GetUserImagePath(image.FileName);
			return new LocalisationJson
			{
				ID = ID,
				Latitude = Latitude,
				Longitude = Longitude,
				Name = Name,
				Description = Description,
				MainPic = imageUrl,
				Address = Adress,
				City = City,
				TypeString = LocalisationTypes[TypeValue]
			};
		}

		#endregion

		#region Features

		public List<Feature> GetFeaturesOfType(FeatureType featureType, IEnumerable<Feature> toExclude = null)
		{
			var toRet = new List<Feature>();
			var featureToCheck = toExclude == null ? LocalisationFeatures : LocalisationFeatures.Where(f => !toExclude.Contains((Feature)f.FeatureID));
			if (featureToCheck == null)
				return toRet;
			return (from item in featureToCheck where item.OfferID == (int)featureType select (Feature)item.FeatureID).ToList();
		}

		public bool HasFeatureIn(List<Feature> features, FeatureType featureType)
		{
			foreach (var item in features)
			{
				if (HasFeature(item, featureType))
					return true;
			}
			return false;
		}

		public bool HasFeature(Feature feature, FeatureType featureType)
		{
			return HasFeature((int)feature, (int)featureType);
		}

		public bool HasFeature(int feature, int featureType)
		{
			var equalityComparer = new LocalisationFeatureEqualityComparer();
			return LocalisationFeatures.Contains(new LocalisationFeature { FeatureID = feature, OfferID = featureType }, equalityComparer);
		}

		public bool HasFeatureType(FeatureType type, IEnumerable<Feature> toExclude)
		{
			var featureToCheck = LocalisationFeatures.Where(f => !toExclude.Contains((Feature)f.FeatureID));
			if (featureToCheck == null)
				return false;
			var intType = (int)type;
			return featureToCheck.Where(f => f.OfferID == intType).Count() > 0;
		}

		public bool HasOffer(LocalisationOffer offer)
		{
			var featureType = (FeatureType)GetFeatureTypeFromOfferType((int)offer);
			var feature = GetFeatureFromOfferType((int)offer);
			return HasFeature(feature, featureType);
		}

		/// <summary>
		/// Check if localisation have an offer
		/// </summary>
		/// <returns>true if it is the case</returns>
		public bool HasOffer()
		{
			return LocalisationFeatures.Where(f => LocalisationBinder.OfferTypes.Contains(f.FeatureID)).Count() > 0;
		}

		/// <summary>
		/// Check if localisation have offer except a given offer
		/// </summary>
		/// <param name="offer">offer to exclude</param>
		/// <returns>true if it is the case</returns>
		public bool HasOfferExcept(LocalisationOffer offer)
		{
			var feature = GetFeatureFromOfferType((int)offer);
			var types = LocalisationBinder.OfferTypes.Except(new List<int> { (int)feature }).ToList();
			return LocalisationFeatures.Where(f => types.Contains(f.FeatureID)).Count() > 0;
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
			var avoidList = new List<Feature> { Feature.AvoidMorning, Feature.AvoidLunch, Feature.AvoidAfternoom, Feature.AvoidEvening };
			if (!HasFeatureIn(avoidList, FeatureType.General))
				return toRet;
			foreach (var item in avoidList)
			{
				if (HasFeature(item, FeatureType.General))
					toRet += LocalisationBinder.LocalisationFeatureDict[(int)item].ToLower() + ", ";
			}
			var last = toRet.LastIndexOf(",");
			toRet = toRet.Remove(last, 1).Insert(last, ".");
			return toRet;
		}


		#endregion

		#region Localisation Types

		public static Dictionary<int, string> LocalisationTypes = new Dictionary<int, string>()
        {
            { (int)LocalisationType.SpotWifi, Worki.Resources.Models.Localisation.Localisation.SpotWifi},
            { (int)LocalisationType.CoffeeResto, Worki.Resources.Models.Localisation.Localisation.CoffeeResto},
            { (int)LocalisationType.Biblio, Worki.Resources.Models.Localisation.Localisation.Biblio},
            { (int)LocalisationType.PublicSpace, Worki.Resources.Models.Localisation.Localisation.PublicSpace},
            { (int)LocalisationType.TravelerSpace, Worki.Resources.Models.Localisation.Localisation.TravelerSpace},
            { (int)LocalisationType.Hotel, Worki.Resources.Models.Localisation.Localisation.Hotel},
            { (int)LocalisationType.Telecentre, Worki.Resources.Models.Localisation.Localisation.Telecentre},
            { (int)LocalisationType.BuisnessCenter, Worki.Resources.Models.Localisation.Localisation.BuisnessCenter},
            { (int)LocalisationType.CoworkingSpace, Worki.Resources.Models.Localisation.Localisation.CoworkingSpace},
            { (int)LocalisationType.WorkingHotel, Worki.Resources.Models.Localisation.Localisation.WorkingHotel},
            { (int)LocalisationType.PrivateArea, Worki.Resources.Models.Localisation.Localisation.PrivateArea}
        };

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

		public static Dictionary<int, string> LocalisationOfferTypes = new Dictionary<int, string>()
        {
            { (int)LocalisationOffer.FreeArea,Worki.Resources.Models.Localisation.LocalisationFeatures.FreeArea},
            { (int)LocalisationOffer.BuisnessRoom ,Worki.Resources.Models.Localisation.LocalisationFeatures.BuisnessRoom},            
            { (int)LocalisationOffer.Workstation,Worki.Resources.Models.Localisation.LocalisationFeatures.SingleWorkstation},
            { (int)LocalisationOffer.SingleDesk,Worki.Resources.Models.Localisation.LocalisationFeatures.SingleSingleDesk},
            { (int)LocalisationOffer.MeetingRoom,Worki.Resources.Models.Localisation.LocalisationFeatures.SingleMeetingRoom },            
            { (int)LocalisationOffer.SeminarRoom,Worki.Resources.Models.Localisation.LocalisationFeatures.SingleSeminarRoom },
            { (int)LocalisationOffer.VisioRoom,Worki.Resources.Models.Localisation.LocalisationFeatures.SingleVisioRoom}
        };


		public static int GetFeatureTypeFromOfferType(int offerType)
		{
			var offerEnum = (LocalisationOffer)offerType;
			switch (offerEnum)
			{
				case LocalisationOffer.FreeArea:
				case LocalisationOffer.BuisnessRoom:
				case LocalisationOffer.SingleDesk:
				case LocalisationOffer.Workstation:
					return (int)FeatureType.WorkingPlace;
				case LocalisationOffer.MeetingRoom:
					return (int)FeatureType.MeetingRoom;
				case LocalisationOffer.SeminarRoom:
					return (int)FeatureType.SeminarRoom;
				case LocalisationOffer.VisioRoom:
					return (int)FeatureType.VisioRoom;
				default:
					return (int)FeatureType.General;
			}
		}

		public static Feature GetFeatureFromOfferType(int offerType)
		{
			var offerEnum = (LocalisationOffer)offerType;
			var feature = Feature.FreeArea;
			switch (offerEnum)
			{
				case LocalisationOffer.FreeArea:
					feature = Feature.FreeArea;
					break;
				case LocalisationOffer.BuisnessRoom:
					feature = Feature.BuisnessRoom;
					break;
				case LocalisationOffer.MeetingRoom:
					feature = Feature.MeetingRoom;
					break;
				case LocalisationOffer.SeminarRoom:
					feature = Feature.SeminarRoom;
					break;
				case LocalisationOffer.SingleDesk:
					feature = Feature.SingleDesk;
					break;
				case LocalisationOffer.VisioRoom:
					feature = Feature.VisioRoom;
					break;
				case LocalisationOffer.Workstation:
					feature = Feature.Workstation;
					break;
				default:
					break;
			}
			return feature;
		}

		public static string GetOfferType(int index)
		{
			if (!LocalisationOfferTypes.ContainsKey(index))
				return null;
			return LocalisationOfferTypes[index];
		}

		public static Dictionary<int, string> GetOfferTypeDict(IEnumerable<LocalisationOffer> except)
		{
			var toRet = (from item in LocalisationOfferTypes where !except.Contains((LocalisationOffer)item.Key) select item).ToDictionary(k => k.Key, k => k.Value);
			return toRet;
		}

		#endregion

		#region Localisation Files

		/// <summary>
		/// Get filename of default image
		/// </summary>
		/// <returns>the filename</returns>
		public string GetMainPic()
		{
			var main = (from item in LocalisationFiles where item.IsDefault orderby item.ID select item.FileName).FirstOrDefault();
			return main;
		}

		/// <summary>
		/// get filename of image at an index
		/// </summary>
		/// <param name="index">the index</param>
		/// <returns>the filename</returns>
		public string GetPic(int index)
		{
			var list = (from item in LocalisationFiles where !item.IsDefault && !item.IsLogo orderby item.ID select item.FileName).ToList();
			var count = list.Count();
			if (count == 0 || index < 0 || index >= count)
				return string.Empty;
			return list[index];
		}

		/// <summary>
		/// Get filename of logo
		/// </summary>
		/// <returns>the filename</returns>
		public string GetLogoPic()
		{
			var logo = (from item in LocalisationFiles where item.IsLogo orderby item.ID select item.FileName).FirstOrDefault();
			return logo;
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
	}

	[Bind(Exclude = "Id,Createur")]
	public class Localisation_Validation
	{
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Name", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Name { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "TypeValue", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public int TypeValue { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Adress", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Adress { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "PostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string PostalCode { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "City", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string City { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Country", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Country { get; set; }

		[Display(Name = "PhoneNumber", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(50, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string PhoneNumber { get; set; }

		[Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
		[Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Mail { get; set; }

		[StringLength(50, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Fax", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public string Fax { get; set; }

		[Display(Name = "WebSite", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		//[WebSite]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
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
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string PublicTransportation { get; set; }

		[Display(Name = "Station", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Station { get; set; }

		[Display(Name = "RoadAccess", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		[StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string RoadAccess { get; set; }
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

		#endregion

		#region Ctor

		public LocalisationFormViewModel()
		{
			Localisation = new Localisation();
			Localisation.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });
			Types = new SelectList(Localisation.LocalisationTypes, "Key", "Value", LocalisationType.SpotWifi);
		}

		public LocalisationFormViewModel(Localisation localisation)
		{
			Localisation = localisation;
			Types = new SelectList(Localisation.LocalisationTypes, "Key", "Value", LocalisationType.SpotWifi);
		}

		#endregion
	}

	#region Data Containers

	public class PictureData
	{
		public string FileName { get; set; }
		public bool IsDefault { get; set; }
		public bool IsLogo { get; set; }
	}

	[DataContract]
	public class PictureDataContainer
	{
		public PictureDataContainer(Localisation loc)
		{
			if (loc.LocalisationFiles != null)
				Files = (from item in loc.LocalisationFiles select new PictureData { FileName = item.FileName, IsDefault = item.IsDefault, IsLogo = item.IsLogo }).ToList();
		}

		[DataMember]
		public List<PictureData> Files { get; set; }
	}

	public class LocalisationJson
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string Description { get; set; }
		public string MainPic { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string TypeString { get; set; }
		public string Url { get; set; }
	}

	public class ImageJson
	{
		public string name { get; set; }
		public int size { get; set; }
		public string url { get; set; }
		public string thumbnail_url { get; set; }
		public string delete_url { get; set; }
		public string delete_type { get; set; }
		public string is_default { get; set; }
		public string is_logo { get; set; }
	}

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
	[LocalizedEnum(ResourceType = typeof(Worki.Resources.Models.Localisation.LocalisationFeatures))]
	public enum Feature
	{
		Access24,
		LunchClose,
		BuisnessRoom,
		Workstation,
		SingleDesk,
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
		AvoidEvening

	}

	/// <summary>
	/// Offer Type, kind of super feature types...
	/// </summary>
	public enum LocalisationOffer
	{
		FreeArea,
		BuisnessRoom,
		Workstation,
		SingleDesk,
		MeetingRoom,
		SeminarRoom,
		VisioRoom
	}

	#endregion

	[MetadataType(typeof(Comment_Validation))]
	public partial class Comment// : IDataErrorInfo, IValidatableObject
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
