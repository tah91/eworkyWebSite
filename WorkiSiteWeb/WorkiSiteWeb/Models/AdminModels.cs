using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Security;

namespace Worki.Web.Models
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

	public class UserListViewModel
	{
		public Dictionary<MembershipUser, bool> ListMemberShip { get; set; }
		public Dictionary<string, int> MemberIds { get; set; }
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
}