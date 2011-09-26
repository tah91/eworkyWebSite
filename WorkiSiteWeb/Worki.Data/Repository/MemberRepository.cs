using System;
using System.Linq;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using System.Web.Security;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public interface IMemberRepository : IRepository<Member>
	{
		Member GetMember(string key);
		string GetUserName(string email);
		bool ActivateMember(string username, string key);
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
			Member member = _Context.Members.SingleOrDefault(m => m.Username == key);

			return member;
		}

		public bool ActivateMember(string username, string key)
		{
			var member = _Context.Members.SingleOrDefault(m => m.Email == username);
			if (member == null)
				return false;
			try
			{
				if (string.Compare(key, member.EmailKey) == 0)
				{
					member.IsApproved = true;
					member.LastActivityDate = DateTime.Now;
					member.EmailKey = null;
					_Context.Commit();
					return true;
				}
				else
				{
					_Context.Complete();
					return false;
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("ActivateMember", ex);
				return false;
			}			
		}

		#endregion

		#region IRepository

        public override void Delete(params object[] keys)
        {
            var id = (int)keys[0];
            Member member = _Context.Members.SingleOrDefault(m => m.MemberId == id);
            if (member == null)
                return;
            var admin = _Context.Members.SingleOrDefault(m => m.Username == MiscHelpers.AdminUser);
            //set member localisation to admin
            foreach (var item in member.Localisations.ToList())
            {
                item.OwnerID = admin.MemberId;
            }
            //set member comment to admin
            foreach (var item in member.Comments.ToList())
            {
                item.PostUserID = admin.MemberId;
            }
            _Context.Members.Remove(member);
        }

		#endregion
	}
}