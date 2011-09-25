
namespace Worki.Infrastructure.UnitOfWork
{
    /// <summary>
	/// Maintains a list of objects affected by a business transaction and coordinates 
	/// the writing out of changes and the resolution of concurrency problems
    /// </summary>
	public interface IUnitOfWork
	{
		/// <summary>
		/// comit the operation to the database
		/// </summary>
		void Commit();

		/// <summary>
		/// complete the operation and flush any database goo without sending changes over the wire
		/// </summary>
		void Complete();
	}
}
