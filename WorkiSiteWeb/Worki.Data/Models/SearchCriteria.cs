using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;
using System;

namespace Worki.Data.Models
{
    public class SearchCriteria
    {
        /// <summary>
        /// class used to prefilter widget
        /// </summary>
        public class Filter
        {
            public IEnumerable<string> GetCountries()
            {
                if (string.IsNullOrEmpty(Countries))
                    return new List<string>();
                return Countries.Split(',').ToList();
            }

            public IEnumerable<int> GetTypes()
            {
                if (string.IsNullOrEmpty(Types))
                    return new List<int>();
                return Types.Split(',').Select(t => (int)Localisation.GetLocalisationType(t)).ToList();
            }

            public string Countries { get; set; }
            public string Types { get; set; }
        }

        #region Ctor

        public SearchCriteria()
        {
            Init();
        }

		public SearchCriteria(bool wifi = false, eSearchType searchType = eSearchType.ePerType, eOrderBy orderBy = eOrderBy.Distance)
		{
            Init(wifi, searchType, orderBy);
		}

        void Init(bool wifi = false, eSearchType searchType = eSearchType.ePerType, eOrderBy orderBy = eOrderBy.Distance)
        {
            LocalisationData = new Localisation();
            OfferData = new Offer { Type = -1 };
            Everything = true;
            //LocalisationOffer = -1;

            if (wifi)
                LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });

            SearchType = searchType;
            OrderBy = orderBy;
			ResultView = eResultView.List;
            GlobalType = eGlobalType.None;
        }

        public Dictionary<string, object> GetDictionnary()
        {
            var toRet = new Dictionary<string, object>();

            if (Page > 0)
                toRet[MiscHelpers.SeoConstants.Page] = Page;

            toRet[MiscHelpers.SeoConstants.Place] = Place;
            toRet[MiscHelpers.SeoConstants.SearchOfferType] = Localisation.GetSeoStringOfferFromType(OfferData.Type);
            toRet[MiscHelpers.SeoConstants.GlobalType] = (int)GlobalType;
            toRet[MiscHelpers.SeoConstants.Latitude] = (float)LocalisationData.Latitude;
            toRet[MiscHelpers.SeoConstants.Longitude] = (float)LocalisationData.Longitude;
            toRet[MiscHelpers.SeoConstants.PlaceName] = LocalisationData.Name;

            toRet[MiscHelpers.SeoConstants.Order] = (int)OrderBy;
            toRet[MiscHelpers.SeoConstants.Search] = (int)SearchType;
            toRet[MiscHelpers.SeoConstants.View] = (int)ResultView;
            toRet[MiscHelpers.SeoConstants.Boundary] = (float)Boundary;

            if (FreeAreas)
            {
                SpotWifi = true;
                CoffeeResto = true;
                Biblio = true;
                TravelerSpace = true;
            }

            if (OtherTypes)
            {
                PrivateArea = true;
                WorkingHotel = true;
                PublicSpace = true;
                Hotel = true;
            }

            var localisationTypes = new List<string>();
            if (Telecentre)
                localisationTypes.Add(MiscHelpers.SeoConstants.Telecentre);
            if (BuisnessCenter)
                localisationTypes.Add(MiscHelpers.SeoConstants.BuisnessCenter);
            if (CoworkingSpace)
                localisationTypes.Add(MiscHelpers.SeoConstants.CoworkingSpace);
            if (SharedOffice)
                localisationTypes.Add(MiscHelpers.SeoConstants.SharedOffice);
            if (SpotWifi)
                localisationTypes.Add(MiscHelpers.SeoConstants.SpotWifi);
            if (CoffeeResto)
                localisationTypes.Add(MiscHelpers.SeoConstants.CoffeeResto);
            if (Biblio)
                localisationTypes.Add(MiscHelpers.SeoConstants.Biblio);
            if (TravelerSpace)
                localisationTypes.Add(MiscHelpers.SeoConstants.TravelerSpace);
            if (WorkingHotel)
                localisationTypes.Add(MiscHelpers.SeoConstants.WorkingHotel);
            if (PrivateArea)
                localisationTypes.Add(MiscHelpers.SeoConstants.PrivateArea);
            if (PublicSpace)
                localisationTypes.Add(MiscHelpers.SeoConstants.PublicSpace);
            if (Hotel)
                localisationTypes.Add(MiscHelpers.SeoConstants.Hotel);

            toRet[MiscHelpers.SeoConstants.LocalisationType] = string.Join(",", localisationTypes);

            foreach (var neededFeature in LocalisationData.LocalisationFeatures)
            {
                var display = FeatureHelper.FeatureToString(neededFeature.FeatureID, FeatureHelper.LocalisationPrefix);
                toRet[display] = true;
            }

            foreach (var offerFeature in OfferData.OfferFeatures)
            {
                var display = FeatureHelper.FeatureToString(offerFeature.FeatureId, FeatureHelper.OfferPrefix);
                toRet[display] = true;
            }

            if (PreFilter != null)
            {
                toRet[MiscHelpers.WidgetConstants.Country] = PreFilter.Countries;
                toRet[MiscHelpers.WidgetConstants.Type] = PreFilter.Types;
            }

            return toRet;
        }

		public bool HasBounds()
		{
			return NorthEastLat != 0 &&
				   NorthEastLng != 0 &&
				   SouthWestLat != 0 &&
				   SouthWestLng != 0;
		}

        #endregion

        #region Properties

        //[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Place", ResourceType = typeof(Worki.Resources.Models.Search.SearchCriteria))]
        public string Place { get; set; }

        public Localisation LocalisationData { get; set; }
		public Offer OfferData { get; set; }

        [Display(Name = "Everything", ResourceType = typeof(Worki.Resources.Models.Search.SearchCriteria))]
        public bool Everything { get; set; }

        [Display(Name = "SpotWifi", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool SpotWifi { get; set; }

        [Display(Name = "CoffeeResto", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool CoffeeResto { get; set; }

        [Display(Name = "Biblio", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool Biblio { get; set; }

        [Display(Name = "PublicSpace", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool PublicSpace { get; set; }

        [Display(Name = "TravelerSpace", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool TravelerSpace { get; set; }

        [Display(Name = "Hotel", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool Hotel { get; set; }

        [Display(Name = "Telecentre", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool Telecentre { get; set; }

        [Display(Name = "BuisnessCenter", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool BuisnessCenter { get; set; }

        [Display(Name = "CoworkingSpace", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool CoworkingSpace { get; set; }

        [Display(Name = "WorkingHotel", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool WorkingHotel { get; set; }

        [Display(Name = "PrivateArea", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        public bool PrivateArea { get; set; }

		[Display(Name = "SharedOffice", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public bool SharedOffice { get; set; }

		[Display(Name = "FreeAreas", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public bool FreeAreas { get; set; }

		[Display(Name = "OtherTypes", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
		public bool OtherTypes { get; set; }

		public eOrderBy OrderBy { get; set; }
        public eSearchType SearchType { get; set; }
		public eResultView ResultView { get; set; }
        public eGlobalType GlobalType { get; set; } 
        public int Page { get; set; }
        public IEnumerable<LocalisationProjection> Projection { get; set; }
        public Filter PreFilter { get; set; }

		public float NorthEastLat { get; set; }
		public float NorthEastLng { get; set; }
		public float SouthWestLat { get; set; }
		public float SouthWestLng { get; set; }

        public float Boundary { get; set; }

        public string OfferTypes { get; set; }

        #endregion
    }

    public class SearchCriteriaApi
    {
        public SearchCriteriaApi()
        {
            latitude = 0;
            longitude = 0;
            neLat = 0;
            neLng = 0;
            swLat = 0;
            swLng = 0;
            boundary = 50;
            offerTypes = null;
            types = null;
            features = null;
            orderBy = 1;
            page = 1;
        }

        public string place { get; set; }
        public string name { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public float neLat { get; set; }
        public float neLng { get; set; }
        public float swLat { get; set; }
        public float swLng { get; set; }
        public float boundary { get; set; }
        public string offerTypes { get; set; }
        public string types { get; set; }
        public string features { get; set; }
        public int orderBy { get; set; }
        public int page { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(place) && (latitude == 0 || longitude == 0) && string.IsNullOrEmpty(name);
        }

        public void FillCriteria(ref SearchCriteria criteria)
        {
            //types
            if (!string.IsNullOrEmpty(types))
            {
                try
                {
                    var typesArray = types.SplitAndParse();
                    foreach (var item in typesArray)
                    {
                        var intType = (LocalisationType)item;
                        switch (intType)
                        {
                            case LocalisationType.SpotWifi:
                                criteria.SpotWifi = true;
                                break;
                            case LocalisationType.CoffeeResto:
                                criteria.CoffeeResto = true;
                                break;
                            case LocalisationType.Biblio:
                                criteria.Biblio = true;
                                break;
                            case LocalisationType.PublicSpace:
                                criteria.PublicSpace = true;
                                break;
                            case LocalisationType.TravelerSpace:
                                criteria.TravelerSpace = true;
                                break;
                            case LocalisationType.Hotel:
                                criteria.Hotel = true;
                                break;
                            case LocalisationType.Telecentre:
                                criteria.Telecentre = true;
                                break;
                            case LocalisationType.BuisnessCenter:
                                criteria.BuisnessCenter = true;
                                break;
                            case LocalisationType.CoworkingSpace:
                                criteria.CoworkingSpace = true;
                                break;
                            case LocalisationType.WorkingHotel:
                                criteria.WorkingHotel = true;
                                break;
                            case LocalisationType.PrivateArea:
                                criteria.PrivateArea = true;
                                break;
                            case LocalisationType.SharedOffice:
                                criteria.SharedOffice = true;
                                break;
                            default:
                                break;
                        }
                    }
                    criteria.Everything = false;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            //features
            if (!string.IsNullOrEmpty(features))
            {
                try
                {
                    //var offerId = Localisation.GetFeatureTypeFromOfferType(criteria.LocalisationOffer);
                    var featuresArray = features.SplitAndParse();
                    foreach (var item in featuresArray)
                    {
                        criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = item });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            //offer types
            criteria.OfferTypes = offerTypes;

            criteria.OfferData.Type = -1;
        }
    }

    public enum eSearchType
    {
        ePerOffer,
        ePerType,
        ePerName
    }

    public enum eOrderBy
    {
        Rating,
        Distance
    }

	public enum eResultView
	{
		List,
		Map
	}

    public enum eGlobalType
    {
        None,
        BuisnessCenter_Smartworkcenter,
        Coworking_SharedOffice,
        MeetingRoom
    }

    public class SearchCriteriaFormViewModel : PagingList<Localisation>
    {
        #region Properties

        public SearchCriteria Criteria { get; private set; }
        public SelectList Offers { get; private set; }
        public SelectList GlobalTypes { get; private set; }
        public Dictionary<int, double> DistanceFromLocalisation { get; private set; }

        public IList<Localisation> PageResults
        {
            get 
			{
				if (List.Count == 0)
					return List;
				return List.Skip((PagingInfo.CurrentPage - 1) * PagingInfo.ItemsPerPage).Take(PagingInfo.ItemsPerPage).ToList(); 
			}
        }

        #endregion

        #region Ctor

        public static List<int> GlobalTypesList = new List<int>()
        {
            (int)eGlobalType.BuisnessCenter_Smartworkcenter,
            (int)eGlobalType.Coworking_SharedOffice,
            (int)eGlobalType.MeetingRoom
        };

        public static string GetGlobalType(int globalTypeInt)
        {
            var globalType = (eGlobalType)globalTypeInt;
            switch (globalType)
            {
                case eGlobalType.BuisnessCenter_Smartworkcenter:
                    return  Worki.Resources.Models.Search.SearchCriteria.BuisnessCenter_Smartworkcenter;
                case eGlobalType.Coworking_SharedOffice:
                    return  Worki.Resources.Models.Search.SearchCriteria.Coworking_SharedOffice;
                case eGlobalType.MeetingRoom:
                    return  Worki.Resources.Models.Search.SearchCriteria.MeetingRoom;
                default:
                    return "";
            }
        }

        public static Dictionary<int, string> GetGlobalTypes()
        {
            return GlobalTypesList.ToDictionary(o => o, o => GetGlobalType(o));
        }

        void Init(bool allOffers = false)
        {
            List = new List<Localisation>();
            DistanceFromLocalisation = new Dictionary<int, double>();
            Criteria = new SearchCriteria();
            var toExclude = allOffers ? new List<LocalisationOffer> ()
                : new List<LocalisationOffer> { LocalisationOffer.BuisnessLounge, LocalisationOffer.SeminarRoom, LocalisationOffer.VisioRoom };
            var offers = Localisation.GetOfferTypeDict(toExclude, true);
            Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.FreeArea);
            GlobalTypes = new SelectList(GetGlobalTypes(), "Key", "Value", eGlobalType.BuisnessCenter_Smartworkcenter);
        }

        public SearchCriteriaFormViewModel(SearchCriteria criteria, bool allOffers = false)
        {
            Init(allOffers);
            Criteria = criteria;
        }

        public SearchCriteriaFormViewModel()
        {
            Init();
        }

        #endregion

		public const int PageSize = 15; // Will change this later

		/// <summary>
		/// fill page info from page index
		/// </summary>
		public void FillPageInfo(int pageValue = 1, int pageSize = PageSize)
		{
			PagingInfo = new PagingInfo
			{
				CurrentPage = pageValue,
				ItemsPerPage = pageSize,
				TotalItems = List.Count()
			};
		}

		public SearchSingleResultViewModel GetSingleResult(int index)
		{
			if (index < 0 || index >= List.Count)
				return null;

			var detailModel = new SearchSingleResultViewModel
			{
				Localisation = List[index],
				Index = index,
				TotalItems = PagingInfo.TotalItems,
				Distance = DistanceFromLocalisation[List[index].ID],
				FromSearch = true,
                Criteria = this.Criteria
			};
			return detailModel;
		}

		public string GetOrderName()
		{
			switch ((eOrderBy)Criteria.OrderBy)
			{
				case eOrderBy.Distance:
					return Worki.Resources.Views.Search.SearchString.DistanceOrdered;
				case eOrderBy.Rating:
					return Worki.Resources.Views.Search.SearchString.RateOrdered;
				default:
					return string.Empty;
			}
		}
    }

    public class SearchSingleResultViewModel
    {
        #region Properties

		public bool FromSearch { get; set; }
        public int Index { get; set; }
		public int TotalItems { get; set; }
		public double Distance { get; set; }
		public Localisation Localisation { get; set; }
        public SearchCriteria Criteria { get; set; }

        #endregion

		public SearchSingleResultViewModel()
		{
			FromSearch = false;
		}
    }
}
