using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Worki.Infrastructure.Repository
{
    /// <summary>
    /// Generic interface to represent basics tasks that can be performed in a data store
    /// Repository pattern
    /// </summary>
    /// <typeparam name="T">type of data in the repository</typeparam>
	public interface IRepository<T>
	{
		/// <summary>
		/// Get element for a given key
		/// </summary>
		/// <param name="key">key</param>
		/// <returns>object instance if key is present</returns>
        T Get(params object[] keys);

		/// <summary>
		/// add an element to the repository
		/// </summary>
		/// <param name="toAdd">element to add</param>
		void Add(T toAdd);

		/// <summary>
		/// remove an element from the repository
		/// </summary>
		/// <param name="key">key of element to remove</param>
        void Delete(params object[] keys);

		/// <summary>
		/// remove elements from the repository that satisfy a where clause
		/// </summary>
		/// <param name="where">clause to satisfy</param>
		void Delete(Expression<Func<T, bool>> where);

		/// <summary>
		/// perform modifications on a element of the repository
		/// </summary>
		/// <param name="key">key of thye element to modify</param>
		/// <param name="actionToPerform">action to perform</param>
		//void Update(int key, Action<T> actionToPerform);

		/// <summary>
		/// Get all the element from repository
		/// </summary>
		/// <returns>list of the elements</returns>
		IList<T> GetAll();

		/// <summary>
		/// Get paginated results from repository
		/// </summary>
		/// <param name="start">the index of the first element to take</param>
		/// <param name="pageSize">the number of element to take</param>
		/// <param name="keySelector">the key to order by when selecting items</param>
		/// <returns>list of the elements</returns>
		IList<T> Get(int start, int pageSize, Expression<Func<T, int>> keySelector);

		/// <summary>
		/// Get the number of element in repository
		/// </summary>
		/// <returns>number of element in repository</returns>
		int GetCount();

		/// <summary>
		/// Get list of items satisfying a where clause
		/// </summary>
		/// <param name="where">clause to satisfy</param>
		/// <returns>items that are ok</returns>
		IList<T> GetMany(Expression<Func<T, bool>> where);

		/// <summary>
		/// Get first item satisfying a where clause
		/// </summary>
		/// <param name="where">clause to satisfy</param>
		/// <returns>first item that is ok</returns>
		T Get(Expression<Func<T, bool>> where);
	}
}
