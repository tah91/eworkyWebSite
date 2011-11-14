using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

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

	#region Admin Localisation

	public class AdminLocalisation : LocalisationListViewModel
	{
		public Dictionary<Localisation, bool> AdminDict { get; set; }

		public AdminLocalisation(IList<Localisation> localisations)
		{
			AdminDict = new Dictionary<Localisation, bool>();
			foreach (Localisation localisation in localisations)
			{
				if (localisation.MainLocalisation != null)
				{
					AdminDict[localisation] = true;
				}
				else
				{
					AdminDict[localisation] = false;
				}
			}
		}
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
        public int nb_SpotWifi { get; set; }
        public int nb_CoffeeResto { get; set; }
        public int nb_Biblio { get; set; }
        public int nb_PublicSpace { get; set; }
        public int nb_TravelerSpace { get; set; }
        public int nb_Hotel { get; set; }
        public int nb_Telecentre { get; set; }
        public int nb_BuisnessCenter { get; set; }
        public int nb_CoworkingSpace { get; set; }
        public int nb_WorkingHotel { get; set; }
        public int nb_PrivateArea { get; set; }
        public int nb_Total { get; set; }

        public StateItem(string name)
        {
            Country_Name = name;
            nb_SpotWifi = 0;
            nb_CoffeeResto = 0;
            nb_Biblio = 0;
            nb_PublicSpace = 0;
            nb_TravelerSpace = 0;
            nb_Hotel = 0;
            nb_Telecentre = 0;
            nb_BuisnessCenter = 0;
            nb_CoworkingSpace = 0;
            nb_WorkingHotel = 0;
            nb_PrivateArea = 0;
            nb_Total = 0;
        }

        public void incr_nb_type(int type)
        {
            var enumType = (LocalisationType)type;
            switch (enumType)
            {
                case LocalisationType.SpotWifi:
                    {
                        nb_SpotWifi++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.CoffeeResto:
                    {
                        nb_CoffeeResto++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.Biblio:
                    {
                        nb_Biblio++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.PublicSpace:
                    {
                        nb_PublicSpace++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.TravelerSpace:
                    {
                        nb_TravelerSpace++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.Hotel:
                    {
                        nb_Hotel++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.Telecentre:
                    {
                        nb_Telecentre++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.BuisnessCenter:
                    {
                        nb_BuisnessCenter++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.CoworkingSpace:
                    {
                        nb_CoworkingSpace++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.WorkingHotel:
                    {
                        nb_WorkingHotel++;
                        nb_Total++;
                        break;
                    }
                case LocalisationType.PrivateArea:
                    {
                        nb_PrivateArea++;
                        nb_Total++;
                        break;
                    }
                default:
                    break;
            }
        }

        public void GetTotal(ILocalisationRepository lRepo)
        {
            nb_SpotWifi = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.SpotWifi)).Count;
            nb_CoffeeResto = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.CoffeeResto)).Count;
            nb_Biblio = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.Biblio)).Count;
            nb_PublicSpace = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.PublicSpace)).Count;
            nb_TravelerSpace = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.TravelerSpace)).Count;
            nb_Hotel = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.Hotel)).Count;
            nb_Telecentre = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.Telecentre)).Count;
            nb_BuisnessCenter = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.BuisnessCenter)).Count;
            nb_CoworkingSpace = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.CoworkingSpace)).Count;
            nb_WorkingHotel = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.WorkingHotel)).Count;
            nb_PrivateArea = lRepo.GetMany(x => (x.TypeValue == (int)LocalisationType.PrivateArea)).Count;
            nb_Total = nb_SpotWifi + nb_CoffeeResto + nb_Biblio + nb_PublicSpace + nb_TravelerSpace + nb_Hotel + nb_Telecentre + nb_BuisnessCenter + nb_CoworkingSpace + nb_WorkingHotel + nb_PrivateArea;
        }
    }

    #endregion

}