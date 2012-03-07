using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using System.Linq;
using System.ComponentModel;
using Worki.Infrastructure;

namespace Worki.Data.Models
{
	
	#region Admin User

	public class User
	{
		public string UserName { get; set; }
	}

    public class MemberAdminModel
    {
        public int MemberId { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public int Score { get; set; }
        public bool Locked { get; set; }
        public string LastName { get; set; }
    }

    public enum eBOStatus
    {
        None,
        Pending,
        Done
    };

    public class BOAccept : IDataErrorInfo
    {
        public bool IsRead { get; set; }

        public BOAccept()
        {
           IsRead = true;
        }

        #region IDataErrorInfo

        public string Error
        {
            get { return string.Empty; }
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "IsRead":
                        {
                            if (!IsRead)
                            {
                                return @Worki.Resources.Views.Home.CguString.MustAcceptCGU;
                            }
                            else
                                return string.Empty;
                        }
                    default:
                        return string.Empty;
                }
            }
        }

        #endregion
    }

	#endregion

	#region Admin Import

	public class AdminImportViewModel
	{
		public string resultMessage { get; set; }
		public string localisationsAlreadyInDB { get; set; }
	}   

	#endregion

	#region Admin WelcomePepole

	[MetadataType(typeof(WelcomePeople_Validation))]
	public partial class WelcomePeople
	{
		public static Dictionary<int, string> SiteVersions = new Dictionary<int, string> 
		{ 
			{ (int)eSiteVersion.fr, "fr" }, 
			{ (int)eSiteVersion.com, "com" }, 
			{ (int)eSiteVersion.es, "es" } 
		};

		public static eSiteVersion GetVersion(Culture culture)
		{
			switch(culture)
			{
				case Culture.fr:
					return eSiteVersion.fr;
				case Culture.es:
					return eSiteVersion.es;
				case Culture.en:
				default:
					return eSiteVersion.com;
			}
		}
	}

	[Bind(Exclude = "Id,LocalisationPicture")]
	public class WelcomePeople_Validation
	{

	}

	public enum eSiteVersion
	{
		fr,
		com,
		es
	}

	public class WelcomePeopleFormViewModel
	{
		public WelcomePeopleFormViewModel()
		{
			WelcomePeople = new WelcomePeople();
			SiteVersions = new SelectList(WelcomePeople.SiteVersions, "Key", "Value");
		}

		public WelcomePeopleFormViewModel(WelcomePeople people)
		{
			WelcomePeople = people;
			LocalisationName = people.Offer.Localisation.Name;
			OfferName = people.Offer.Name;
			SiteVersions = new SelectList(WelcomePeople.SiteVersions, "Key", "Value");
		}

		public SelectList SiteVersions { get; private set; }
		public WelcomePeople WelcomePeople { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string OfferName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string LocalisationName { get; set; }
	}

	#endregion

	#region Admin Booking

	#endregion

    #region Admin Quotation

    #endregion

    #region Admin Press

    [MetadataType(typeof(Press_Validation))]
    public partial class Press
    {
        public Press()
        {
            Date = DateTime.UtcNow;
        }
    }

    [Bind(Exclude = "Id")]
    public class Press_Validation
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Url { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Description { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public DateTime Date { get; set; }
    }

    #endregion
    
    #region Admin Rental

    #endregion

    #region Admin Stat

    public class StateItem
    {
        public string Country_Name { get; set; }
        public int SpotWifi { get; set; }
        public int CoffeeResto { get; set; }
        public int Biblio { get; set; }
        public int PublicSpace { get; set; }
        public int TravelerSpace { get; set; }
        public int Hotel { get; set; }
        public int Telecentre { get; set; }
        public int BuisnessCenter { get; set; }
        public int CoworkingSpace { get; set; }
        public int WorkingHotel { get; set; }
        public int PrivateArea { get; set; }
		public int SharedOffice { get; set; }
        public int Total { get; set; }

        public StateItem(string name, int total)
        {
            Country_Name = name;
            Total = total;
        }
    }

    #endregion

}