using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using System.Linq;

namespace Worki.Data.Models
{
	#region Admin Visitor
	
	public class VisitorListViewModel
	{
		public Dictionary<Visitor, bool> ListVisitor { get; set; }
		public PagingInfo PagingInfo { get; set; }
	}    

	#endregion

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
    }

	public class UserListViewModel
	{
        public IEnumerable<MemberAdminModel> ListMemberShip { get; set; }
		public PagingInfo PagingInfo { get; set; }
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

	public class WelcomePeopleListViewModel
	{
		public IList<WelcomePeople> WelcomePeople { get; set; }
		public PagingInfo PagingInfo { get; set; }
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
			Email = people.Member.Username;
			LocalisationName = people.Localisation.Name;
		}

		public WelcomePeople WelcomePeople { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Email { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string LocalisationName { get; set; }
	}

	#endregion

	#region Admin Booking

	public class MemberBookingListViewModel
	{
		public IList<MemberBooking> MemberBooking { get; set; }
		public PagingInfo PagingInfo { get; set; }
	}

	#endregion

    #region Admin Press

    [MetadataType(typeof(Press_Validation))]
    public partial class Press
    {
        public Press()
        {
            Date = DateTime.Now;
        }
    }

    public class PressListViewModel
    {
        public IList<Press> Press { get; set; }
        public PagingInfo PagingInfo { get; set; }
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

    public class RentalListViewModel
    {
        public IList<Rental> Rentals { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }

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
        public int Total { get; set; }

        public StateItem(string name, int total)
        {
            Country_Name = name;
            SpotWifi = 0;
            CoffeeResto = 0;
            Biblio = 0;
            PublicSpace = 0;
            TravelerSpace = 0;
            Hotel = 0;
            Telecentre = 0;
            BuisnessCenter = 0;
            CoworkingSpace = 0;
            WorkingHotel = 0;
            PrivateArea = 0;
            Total = total;
        }
    }

    #endregion

}