using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using System.Linq;

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
	}

	[Bind(Exclude = "Id,LocalisationPicture")]
	public class WelcomePeople_Validation
	{

	}

	public class WelcomePeopleFormViewModel
	{
		public WelcomePeopleFormViewModel()
		{
			WelcomePeople = new WelcomePeople();
		}

		public WelcomePeopleFormViewModel(WelcomePeople people)
		{
			WelcomePeople = people;
			LocalisationName = people.Offer.Localisation.Name;
		}

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