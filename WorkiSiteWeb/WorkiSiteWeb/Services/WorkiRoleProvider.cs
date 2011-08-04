using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Security;
using WorkiSiteWeb.Infrastructure.Repository;
using Ninject;

namespace WorkiSiteWeb.Models
{
    public sealed class WorkiRoleProvider : RoleProvider
    {
		[Inject]
		public IGroupRepository GroupRepository { get; set; }
		[Inject]
		public IMemberRepository MemberRepository { get; set; }

        /*************************************************************************
         * Initialization
         *************************************************************************/

        /// <summary>
        /// Initialize the RoleProvider
        /// </summary>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "WorkiRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Worki Role Provider");
            }

            // Initialize base class
            base.Initialize(name, config);

            _applicationName = GetConfigValue(config["applicationName"],
                System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);

        }

        /*************************************************************************
         * General settings
         *************************************************************************/

        private string _applicationName;
        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        /*************************************************************************
         * Retrieval methods
         *************************************************************************/

        /// <summary>
        /// Gets all available user roles
        /// </summary>
        /// <returns>Array of all available roles</returns>
        public override string[] GetAllRoles()
        {
            string[] roles = null;
            roles = (from groups in GroupRepository.GetAll()
                     select groups.Title).ToArray();

            return roles;
        }

        /// <summary>
        /// Gets the assigned roles for a particular user.
        /// </summary>
        /// <param name="username">Matching username</param>
        /// <returns>Array of assigned roles</returns>
        public override string[] GetRolesForUser(string username)
        {
            string[] roles = new string[] { "" };

            try
            {
                roles = GroupRepository.GetGroupsForUser(username).ToArray();
            }
            catch (Exception)
            {

            }
            return roles;
        }

        /// <summary>
        /// Gets all the users in a particular role
        /// </summary>
        public override string[] GetUsersInRole(string roleName)
        {
            // Without paging, this function seems pointless to me,
            // so I didn't implement it. Should be simple enough using the previous code though.
            throw new NotImplementedException();
        }

        /*************************************************************************
         * Create and Delete methods
         *************************************************************************/

        /// <summary>
        /// Creates a new role
        /// </summary>
        public override void CreateRole(string roleName)
        {
            // No need to add if it already exists
            if (!RoleExists(roleName))
            {
                Group g = new Group();
                g.Title = roleName;
                GroupRepository.Add(g);
                //GroupRepository.Save();
            }
        }

        /// <summary>
        /// Deletes a given role
        /// </summary>
        /// <param name="roleName">Role name to delete</param>
        /// <param name="throwOnPopulatedRole">Specifies whether the function should throw
        /// if there are assigned users to this role</param>
        /// <returns>True if successful. Defaults to false</returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            // Return status. Defaults to false.
            bool ret = false;

            // You can only delete an existing role
            if (RoleExists(roleName))
            {
                try
                {
                    if (throwOnPopulatedRole)
                    {
                        int[] users = (from mg in GroupRepository.GetAllMembersInGroups()
                                       where mg.Group.Title == roleName
                                       select mg.Member.MemberId).ToArray();

                        if (users.Count() > 0)
                            throw new ProviderException("Cannot delete roles with users assigned to them");
                    }

                    GroupRepository.DeleteRole(roleName);

                    ret = true;
                }
                catch { }
            }

            return ret;
        }

        /*************************************************************************
         * Assign/Remove methods
         *************************************************************************/

        /// <summary>
        /// Adds a collection of users to a collection of corresponding roles
        /// </summary>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            // Get the actual available roles
            string[] allRoles = GetAllRoles();

            // See if any of the given roles match the available roles
            IEnumerable<string> roles = allRoles.Intersect(roleNames);

            // There were some roles left after removing non-existent ones
            if (roles.Count() > 0)
            {
                // Cleanup duplicates first
                RemoveUsersFromRoles(usernames, roleNames);

                // Get the user IDs
                List<int> mlist = (from members in MemberRepository.GetAll()
                                   where usernames.Contains(members.Username)
                                   select members.MemberId).ToList();

                // Get the group IDs
                List<int> glist = (from groups in GroupRepository.GetAll()
                                   where roleNames.Contains(groups.Title)
                                   select groups.GroupId).ToList();

                // Fresh list of user-role assignments
                List<MembersInGroup> mglist = new List<MembersInGroup>();
                foreach (int m in mlist)
                {
                    foreach (int g in glist)
                    {
                        MembersInGroup mg = new MembersInGroup();
                        mg.MemberId = m;
                        mg.GroupId = g;
                        mglist.Add(mg);
                    }
                }

                GroupRepository.AddMembersInGroup(mglist);
                //GroupRepository.Save();
            }
        }

        /// <summary>
        /// Remove a collection of users from a collection of corresponding roles
        /// </summary>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            // Get the actual available roles
            string[] allRoles = GetAllRoles();

            // See if any of the given roles match the available roles
            IEnumerable<string> roles = allRoles.Intersect(roleNames);

            // There were some roles left after removing non-existent ones
            if (roles.Count() > 0)
            {
                    List<MembersInGroup> mg = (from members in GroupRepository.GetAllMembersInGroups()
                                               where usernames.Contains(members.Member.Username) &&
                                               roleNames.Contains(members.Group.Title)
                                               select members).ToList();

                    GroupRepository.DeleteMembersInGroup(mg);
                    //GroupRepository.Save();
            }
        }

        /*************************************************************************
         * Searching methods
         *************************************************************************/

        /// <summary>
        /// Checks if a given username is in a particular role
        /// </summary>
        public override bool IsUserInRole(string username, string roleName)
        {
            // Return status defaults to false
            bool ret = false;

            if (RoleExists(roleName))
            {
                int c = (from m in GroupRepository.GetAllMembersInGroups()
                         where m.Member.Username == username &&
                         m.Group.Title == roleName
                         select m).Count();

                if (c > 0)
                    ret = true;
            }

            return ret;
        }

        /// <summary>
        /// Finds a set of users in a given role
        /// </summary>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            // Here's another function that doesn't make sense without paging
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a given role already exists in the database
        /// </summary>
        /// <param name="roleName">Role name to search</param>
        /// <returns>True if the role exists. Defaults to false.</returns>
        public override bool RoleExists(string roleName)
        {
            bool ret = false;

            // If the specified role exist
            if (GetAllRoles().Contains(roleName))
                ret = true;

            return ret;
        }

        /*************************************************************************
         * Private helper methods
         *************************************************************************/

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }
    }
}
