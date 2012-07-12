﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;

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

		public SearchCriteria(bool wifi = false, eSearchType searchType = eSearchType.ePerOffer, eOrderBy orderBy = eOrderBy.Distance)
		{
            Init(wifi, searchType, orderBy);
		}

        void Init(bool wifi = false, eSearchType searchType = eSearchType.ePerOffer, eOrderBy orderBy = eOrderBy.Distance)
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
        }

        public Dictionary<string, object> GetDictionnary()
        {
            var toRet = new Dictionary<string, object>();

            if (Page > 0)
                toRet[MiscHelpers.SeoConstants.Page] = Page;

            toRet[MiscHelpers.SeoConstants.Place] = Place;
            toRet[MiscHelpers.SeoConstants.OfferType] = Localisation.GetSeoStringOfferFromType(OfferData.Type);
            toRet[MiscHelpers.SeoConstants.Latitude] = (float)LocalisationData.Latitude;
            toRet[MiscHelpers.SeoConstants.Longitude] = (float)LocalisationData.Longitude;
            toRet[MiscHelpers.SeoConstants.PlaceName] = LocalisationData.Name;

            toRet[MiscHelpers.SeoConstants.Order] = (int)OrderBy;
            toRet[MiscHelpers.SeoConstants.Search] = (int)SearchType;
            toRet[MiscHelpers.SeoConstants.View] = (int)ResultView;

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

            toRet[MiscHelpers.SeoConstants.Type] = string.Join(",", localisationTypes);

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

		public bool WithinBounds(float lat, float lng)
		{
			return SouthWestLat < lat && lat < NorthEastLat
				&& SouthWestLng < lng && lng < NorthEastLng;
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
        public int Page { get; set; }
        public IEnumerable<LocalisationProjection> Projection { get; set; }
        public Filter PreFilter { get; set; }

		public float NorthEastLat { get; set; }
		public float NorthEastLng { get; set; }
		public float SouthWestLat { get; set; }
		public float SouthWestLng { get; set; }

        public float Boundary { get; set; }

        #endregion
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

    public class SearchCriteriaFormViewModel : PagingList<Localisation>
    {
        #region Properties

        public SearchCriteria Criteria { get; private set; }
        public SelectList Offers { get; private set; }
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

        void Init(bool allOffers = false)
        {
            List = new List<Localisation>();
            DistanceFromLocalisation = new Dictionary<int, double>();
            Criteria = new SearchCriteria();
            var toExclude = allOffers ? new List<LocalisationOffer> ()
                : new List<LocalisationOffer> { LocalisationOffer.BuisnessLounge, LocalisationOffer.SeminarRoom, LocalisationOffer.VisioRoom };
            var offers = Localisation.GetOfferTypeDict(toExclude, true);
            Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.FreeArea);
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
