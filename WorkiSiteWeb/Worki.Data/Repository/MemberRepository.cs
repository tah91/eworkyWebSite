using System;
using System.Linq;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using System.Web.Security;
using System.Collections.Generic;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public interface IMemberRepository : IRepository<Member>
	{
		Member GetMember(string key);
		string GetUserName(string email);
        IList<MemberAdminModel> GetLeaders();
		IList<Member> GetMembers(string role);
        IList<Member> GetClients(int ownerId);
		IEnumerable<Member> GetOwners();
		int GetAdminId();
	}

	public class MemberRepository : RepositoryBase<Member>, IMemberRepository
	{
		public MemberRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{

		}

		#region IMemberRepository

		public String GetUserName(string email)
		{
			Member m = (from members in _Context.Members where members.Email == email select members).SingleOrDefault();
			return m == null ? null : m.Username;
		}

		public Member GetMember(string key)
		{
            Member member = _Context.Members.SingleOrDefault(m => string.Compare(m.Email, key, StringComparison.InvariantCultureIgnoreCase) == 0);

			return member;
		}



		#endregion

		#region IRepository

        public override void Delete(params object[] keys)
        {
            var id = (int)keys[0];
            Member member = _Context.Members.SingleOrDefault(m => m.MemberId == id);
            if (member == null)
                return;
			var adminId = GetAdminId();
            //set member localisation to admin
            foreach (var item in member.Localisations.ToList())
            {
				item.SetOwner(adminId);
            }
            //set member comment to admin
            foreach (var item in member.Comments.ToList())
            {
				item.PostUserID = adminId;
            }
            _Context.Members.Remove(member);
        }

		#endregion

        #region Admin List

        public IList<MemberAdminModel> GetLeaders()
        {
            var leaders = from item in _Context.Members
                          where item.MemberEditions.Count > 2
                          orderby item.MemberEditions.Count descending
                         select new MemberAdminModel
                         {
                             MemberId = item.MemberId,
                             UserName = item.Username,
                             LastName = item.MemberMainData.LastName,
                             Score = 0
                         };

            return leaders.Take(50).ToList();
        }

        #endregion

        public IList<Member> GetClients(int ownerId)
        {
			var clientIds = from item in _Context.MemberBookings
							where item.Offer.Localisation.OwnerID == ownerId && item.StatusId == (int)MemberBooking.Status.Paid
							group item by item.MemberId into clients
							where clients.Count() > 0
							orderby clients.Count()
							select clients.Key;

			clientIds = clientIds.Concat(from item in _Context.MemberQuotations
										 where item.Offer.Localisation.OwnerID == ownerId && item.StatusId == (int)MemberQuotation.Status.Paid
										 group item by item.MemberId into clients
										 where clients.Count() > 0
										 orderby clients.Count()
										 select clients.Key);

            return GetMany(m => clientIds.Contains(m.MemberId));
        }

		public int GetAdminId()
		{
			Member member = _Context.Members.SingleOrDefault(m => string.Compare(m.Email, MiscHelpers.AdminConstants.AdminMail, StringComparison.InvariantCultureIgnoreCase) == 0);

			return member != null ? member.MemberId : -1;
		}

		#region Role

		public IList<Member> GetMembers(string role)
		{
			var members = from item in _Context.MembersInGroups
						 where item.Group.Title == role
						 orderby item.MemberId descending
						 select item.Member;

			return members.ToList();
		}

		public IEnumerable<Member> GetOwners()
		{
			var ownerIds = (from item in _Context.Localisations
							group item by item.OwnerID into owners
							where owners.Count() > 0
							select owners).ToDictionary(o => o.Key.Value, o => o.Count());

			var ids = ownerIds.Select(o => o.Key).ToList();
			return GetMany(m => ids.Contains(m.MemberId)).OrderByDescending(m => ownerIds[m.MemberId]);
		}

		#endregion
	}
}