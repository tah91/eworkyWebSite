﻿using System;
using System.Linq;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using System.Web.Security;

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
        #region Private

        #region Complied Queries

		//Func<WorkiDBEntities, string, IQueryable<Member>> _GetMemberFromUserName = CompiledQuery.Compile<WorkiDBEntities, string, IQueryable<Member>>(
		//    (db, username) => from members in db.Members where members.Username == username select members
		//    );

		//Func<WorkiDBEntities, string, IQueryable<Member>> _GetMemberFromEmail = CompiledQuery.Compile<WorkiDBEntities, string, IQueryable<Member>>(
		//    (db, email) => from members in db.Members where members.Email == email select members
		//    );

		//Func<WorkiDBEntities, int, IQueryable<Member>> _GetMemberFromId = CompiledQuery.Compile<WorkiDBEntities, int, IQueryable<Member>>(
		//    (db, id) => from members in db.Members where members.MemberId == id select members
		//    );

        #endregion		

		static bool _Initialized = false;

		void Initialise()
		{
			if (_Initialized)
				return;

			try
			{
				using (var db = new WorkiDBEntities())
				{
					//create roles
					if (!Roles.RoleExists(MiscHelpers.AdminRole))
					{
						Roles.CreateRole(MiscHelpers.AdminRole);
					}

					//create admin
					var user = db.Members.FirstOrDefault(m => m.Username == MiscHelpers.AdminUser);

					//create admin
					if (user == null)
					{
						MembershipCreateStatus status;
						Membership.Provider.CreateUser(MiscHelpers.AdminUser, MiscHelpers.AdminPass, MiscHelpers.AdminMail, null, null, true, null, out status);
						//add role
						if (!Roles.IsUserInRole(MiscHelpers.AdminUser, MiscHelpers.AdminRole))
							Roles.AddUserToRole(MiscHelpers.AdminUser, MiscHelpers.AdminRole);
					}
					//add member data
					if (user.MemberMainData == null)
					{
						user.MemberMainData = new MemberMainData { FirstName = MiscHelpers.AdminUser, LastName = MiscHelpers.AdminUser, Civility = (int)CivilityType.Mr };
					}

					db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Initialise", ex);
				return;
			}
			_Initialized = true;
		}

		public MemberRepository(ILogger logger)
			: base(logger)
		{
			//initialise admin data
			Initialise();
		}

        #endregion

        #region IMemberRepository

        public String GetUserName(string email)
        {
            using (var db = new WorkiDBEntities())
            {
				Member m = (from members in db.Members where members.Email == email select members).SingleOrDefault();
                return m == null ? null : m.Username;
            }
        }

        public Member GetMember(string key)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
				Member member = db.Members.SingleOrDefault(m => m.Username == key);

                return member;
            }
        }

        public bool ActivateMember(string username, string key)
        {
            using (var db = new WorkiDBEntities())
            {
                var member = db.Members.SingleOrDefault(m => m.Email == username);
                if (member == null)
                    return false;
                if (string.Compare(key, member.EmailKey) == 0)
                {
                    member.IsApproved = true;
                    member.LastActivityDate = DateTime.Now;
                    member.EmailKey = null;
                    db.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region IRepository

		public override void Delete(int key)
		{
			using (var db = new WorkiDBEntities())
			{
				Member member = db.Members.SingleOrDefault(m => m.MemberId == key);
				if (member == null)
					return;
				var admin = db.Members.SingleOrDefault(m => m.Username == MiscHelpers.AdminUser);
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
				db.Members.Remove(member);
				db.SaveChanges();
			}
		}

        #endregion
    }
}