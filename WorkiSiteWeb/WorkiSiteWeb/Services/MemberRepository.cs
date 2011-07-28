using System;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using System.Collections.Generic;
using System.Data.Objects;
using WorkiSiteWeb.Infrastructure.Logging;
using System.Web.Security;
using WorkiSiteWeb.Helpers;

namespace WorkiSiteWeb.Models
{
    public class MemberRepository : IMemberRepository
    {
        #region Private

        #region Complied Queries

        Func<WorkiDBEntities, string, IQueryable<Member>> _GetMemberFromUserName = CompiledQuery.Compile<WorkiDBEntities, string, IQueryable<Member>>(
            (db, username) => from members in db.Members where members.Username == username select members
            );

        Func<WorkiDBEntities, string, IQueryable<Member>> _GetMemberFromEmail = CompiledQuery.Compile<WorkiDBEntities, string, IQueryable<Member>>(
            (db, email) => from members in db.Members where members.Email == email select members
            );

        Func<WorkiDBEntities, int, IQueryable<Member>> _GetMemberFromId = CompiledQuery.Compile<WorkiDBEntities, int, IQueryable<Member>>(
            (db, id) => from members in db.Members where members.MemberId == id select members
            );

		ILogger _Logger; 

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
        //WorkiDBEntities db = new WorkiDBEntities();

		public MemberRepository(ILogger logger)
		{
			_Logger = logger;

			//initialise admin data
			Initialise();
		}

        #endregion

        #region IMemberRepository

        public String GetUserName(string email)
        {
            using (var db = new WorkiDBEntities())
            {
                Member m = _GetMemberFromEmail(db, email).SingleOrDefault();
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
                var member = _GetMemberFromEmail(db, username).SingleOrDefault();
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

		public Member Get(int key)
		{
			var db = new WorkiDBEntities();
			//using (var db = new WorkiDBEntities())
			{
				Member member = db.Members.SingleOrDefault(m => m.MemberId == key);

				return member;
			}
		}

        public void Add(Member toAdd)
        {
            using (var db = new WorkiDBEntities())
            {
                db.Members.AddObject(toAdd);
                db.SaveChanges();
            }
        }

        public void Delete(int key)
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
				db.Members.DeleteObject(member);
                db.SaveChanges();
            }
        }

        public void Update(int id, Action<Member> actionToPerform)
        {
            using (var db = new WorkiDBEntities())
            {
				Member member = db.Members.SingleOrDefault(m => m.MemberId == id);
                if (member != null)
                {
                    actionToPerform.Invoke(member);
                    db.SaveChanges();
                }
            }
        }

        public IList<Member> GetAll()
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                return db.Members.ToList();
            }
        }

        public IList<Member> Get(int start, int pageSize)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
				return db.Members.OrderByDescending(m => m.MemberId).Skip(start).Take(pageSize).ToList();
            }
        }

        public int GetCount()
        {
            using (var db = new WorkiDBEntities())
            {
                return db.Members.Count();
            }
        }

        #endregion
    }
}