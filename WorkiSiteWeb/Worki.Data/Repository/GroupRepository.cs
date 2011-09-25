using System.Collections.Generic;
using System.Linq;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Logging;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public interface IGroupRepository : IRepository<Group>
	{
		void AddMembersInGroup(IEnumerable<MembersInGroup> membersInGroup);
		void DeleteMembersInGroup(IEnumerable<MembersInGroup> membersInGroup);
		IList<MembersInGroup> GetAllMembersInGroups();
		IList<string> GetGroupsForUser(string username);
	}

	public class GroupRepository : RepositoryBase<Group>, IGroupRepository
	{
		public GroupRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

		#region IGroupRepository

		public void AddMembersInGroup(IEnumerable<MembersInGroup> membersInGroup)
		{
			foreach (var item in membersInGroup)
			{
				_Context.MembersInGroups.Add(item);
			}
		}

		public void DeleteMembersInGroup(IEnumerable<MembersInGroup> membersInGroup)
		{
			var ids = from item in membersInGroup select item.RelationId;
			var toDelete = _Context.MembersInGroups.Where(mig => ids.Contains(mig.RelationId));
			foreach (var item in toDelete)
			{
				_Context.MembersInGroups.Remove(item);
			}
		}

		public IList<MembersInGroup> GetAllMembersInGroups()
		{
			return _Context.MembersInGroups.ToList();
		}

		public IList<string> GetGroupsForUser(string username)
		{
			return (from mg in _Context.MembersInGroups where mg.Member.Username == username select mg.Group.Title).ToList();
		}

		#endregion
	}
}