using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public partial class WorkiDBEntities : IUnitOfWork
    {
		public void Commit()
		{
			SaveChanges();
			Dispose();
		}

		public void Complete()
		{
			Dispose();
		}
	}
}