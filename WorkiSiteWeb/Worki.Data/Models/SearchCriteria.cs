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
		}

        #endregion

        #region Properties

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Place", ResourceType = typeof(Worki.Resources.Models.Search.SearchCriteria))]
        public string Place { get; set; }

        public Localisation LocalisationData { get; set; }

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

    public class SearchCriteriaFormViewModel
    {
        #region Properties

        public SearchCriteria Criteria { get; private set; }
        public SelectList Offers { get; private set; }
        public eSearchType SearchType { get; private set; }
        public eDirectAccessType DirectAccessType { get;private set; }
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

        void Init()
        {
            Results = new List<Localisation>();
            DistanceFromLocalisation = new Dictionary<int, double>();
            Criteria = new SearchCriteria();
            Offers = new SelectList(Localisation.LocalisationOfferTypes, "Key", "Value", LocalisationOffer.FreeArea);
            SearchType = eSearchType.ePerOffer;
            DirectAccessType = eDirectAccessType.eNone;
        }

        public SearchCriteriaFormViewModel(SearchCriteria criteria, eSearchType searchType = eSearchType.ePerOffer, eDirectAccessType directAccessType = eDirectAccessType.eNone)
        {
            Init();
            Criteria = criteria;
            SearchType = searchType;
            DirectAccessType = directAccessType;
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
		public void FillPageInfo(int pageValue = 1)
		{
			PagingInfo = new PagingInfo
			{
				CurrentPage = pageValue,
				ItemsPerPage = PageSize,
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
