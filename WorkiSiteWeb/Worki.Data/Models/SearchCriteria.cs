using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Worki.Infrastructure;

namespace Worki.Data.Models
{
    public class SearchCriteria
    {
        #region Ctor

        public SearchCriteria()
        {
			Init();
        }

		public SearchCriteria(bool wifi)
		{
			Init(wifi);
		}

		void Init(bool wifi=false)
		{
			LocalisationData = new Localisation();
			Everything = true;
			LocalisationOffer = -1;
			if (wifi)
				LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });
            SearchType = eSearchType.ePerOffer;
            DirectAccessType = eDirectAccessType.eNone;

            /* Initialize it with the distance criteria */
            OrderBy = eOrderBy.Distance;
		}

        #region Direct Access

        public static SearchCriteria CreateSearchCriteria(eDirectAccessType directAccessType)
        {
            SearchCriteria criteria = null;
            switch (directAccessType)
            {
                case eDirectAccessType.eStudent:
                    criteria = new SearchCriteria { SpotWifi = true, CoffeeResto = true, Biblio = true, PublicSpace = true };
                    break;
                case eDirectAccessType.eTeleworker:
                    criteria = new SearchCriteria { PublicSpace = true, Telecentre = true, BuisnessCenter = true, CoworkingSpace = true };
                    break;
                case eDirectAccessType.eStartUp:
                    criteria = new SearchCriteria { BuisnessCenter = true, CoworkingSpace = true, WorkingHotel = true };
                    break;
                case eDirectAccessType.eNomade:
                    criteria = new SearchCriteria { SpotWifi = true, CoffeeResto = true, Biblio = true, TravelerSpace = true, Hotel = true, Telecentre = true, BuisnessCenter = true, CoworkingSpace = true };
                    break;
                case eDirectAccessType.eEntreprise:
                    criteria = new SearchCriteria { Hotel = true, Telecentre = true, BuisnessCenter = true, WorkingHotel = true, PrivateArea = true };
                    break;
                case eDirectAccessType.eIndependant:
                    criteria = new SearchCriteria { Telecentre = true, BuisnessCenter = true, CoworkingSpace = true, WorkingHotel = true };
                    break;
                case eDirectAccessType.eNone:
                default:
                    criteria = new SearchCriteria { SearchType = eSearchType.ePerType };
                    break;
            }

            if (directAccessType != eDirectAccessType.eNone)
                criteria.Everything = false;
            criteria.SearchType = eSearchType.ePerType;
            criteria.DirectAccessType = directAccessType;

            return criteria;
        }

        public static string GetDirectAccessTitle(eDirectAccessType type)
        {
            switch (type)
            {
                case eDirectAccessType.eStudent:
                    return Worki.Resources.Models.Profile.Profile.Student;
                case eDirectAccessType.eTeleworker:
                    return Worki.Resources.Models.Profile.Profile.Teleworker;
                case eDirectAccessType.eStartUp:
                    return Worki.Resources.Views.Search.SearchString.Entrepreneur;
                case eDirectAccessType.eNomade:
                    return Worki.Resources.Views.Search.SearchString.Nomad;
                case eDirectAccessType.eIndependant:
                    return Worki.Resources.Views.Search.SearchString.Independant;
                case eDirectAccessType.eEntreprise:
                    return Worki.Resources.Models.Profile.Profile.Company;
                case eDirectAccessType.eNone:
                default:
                    return Worki.Resources.Views.Search.SearchString.Search;
            }
        }

        #endregion

        #endregion

        #region Properties

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Place", ResourceType = typeof(Worki.Resources.Models.Search.SearchCriteria))]
        public string Place { get; set; }

        public Localisation LocalisationData { get; set; }

        /* Add the property OderBy */
        public eOrderBy OrderBy { get; set; }

        public int LocalisationOffer { get; set; }

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

        public eSearchType SearchType { get; set; }
        public eDirectAccessType DirectAccessType { get; set; }

        #endregion
    }

    public enum eSearchType
    {
        ePerOffer,
        ePerType
    }

    public enum eDirectAccessType
    {
        eNone,
        eStudent,
        eNomade,
        eTeleworker,
        eStartUp,
        eIndependant,
        eEntreprise
    }

    /* Enum of the different ordered type */
    public enum eOrderBy
    {
        Rating,
        Distance
    }

    public class SearchCriteriaFormViewModel
    {
        #region Properties

        public SearchCriteria Criteria { get; private set; }
        public SelectList Offers { get; private set; }
        public Dictionary<int, double> DistanceFromLocalisation { get; private set; }
        public IList<Localisation> Results { get; set; }
        public PagingInfo PagingInfo { get; set; }

        public IList<Localisation> PageResults
        {
            get 
			{
				if (Results.Count == 0)
					return Results;
				return Results.Skip((PagingInfo.CurrentPage - 1) * PagingInfo.ItemsPerPage).Take(PagingInfo.ItemsPerPage).ToList(); 
			}
        }

        #endregion

        #region Ctor

        void Init(bool allOffers = false)
        {
            Results = new List<Localisation>();
            DistanceFromLocalisation = new Dictionary<int, double>();
            Criteria = new SearchCriteria();
            var offers = allOffers ? Localisation.LocalisationOfferTypes : Localisation.GetOfferTypeDict(new List<LocalisationOffer> { LocalisationOffer.AllOffers });
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

		public const int PageSize = 5; // Will change this later

		/// <summary>
		/// fill page info from page index
		/// </summary>
		public void FillPageInfo(int pageValue = 1, int pageSize = PageSize)
		{
			PagingInfo = new PagingInfo
			{
				CurrentPage = pageValue,
				ItemsPerPage = pageSize,
				TotalItems = Results.Count()
			};
		}

		public SearchSingleResultViewModel GetSingleResult(int index)
		{
			if (index < 0 || index >= Results.Count)
				return null;

			var detailModel = new SearchSingleResultViewModel
			{
				Localisation = Results[index],
				Index = index,
				TotalItems = PagingInfo.TotalItems,
				Distance = DistanceFromLocalisation[Results[index].ID],
				FromSearch = true
			};
			return detailModel;
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

        #endregion

		public SearchSingleResultViewModel()
		{
			FromSearch = false;
		}
    }
}
