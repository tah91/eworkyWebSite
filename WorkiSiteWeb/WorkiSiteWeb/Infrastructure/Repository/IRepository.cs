using System;
using System.Collections.Generic;

namespace WorkiSiteWeb.Infrastructure.Repository
{
    /// <summary>
    /// Generic interface to represent basics tasks that can be performed in a data store
    /// Repository pattern
    /// </summary>
    /// <typeparam name="T">type of data in the repository</typeparam>
    /// <typeparam name="TKey">type of key in order to get a element</typeparam>
    public interface IRepository<T,TKey>
    {
        /// <summary>
        /// Get element for a given key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>object instance if key is present</returns>
        T Get(TKey key);

        /// <summary>
        /// add an element to the repository
        /// </summary>
        /// <param name="toAdd">element to add</param>
        void Add(T toAdd);

        /// <summary>
        /// remove an element from the repository
        /// </summary>
        /// <param name="key">key of element to remove</param>
        void Delete(TKey key);

        /// <summary>
        /// perform modifications on a element of the repository
        /// </summary>
        /// <param name="key">key of thye element to modify</param>
        /// <param name="actionToPerform">action to perform</param>
        void Update(TKey key, Action<T> actionToPerform);

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
        /// <returns>list of the elements</returns>
        IList<T> Get(int start, int pageSize);

        /// <summary>
        /// Get the number of element in repository
        /// </summary>
        /// <returns>number of element in repository</returns>
        int GetCount();
    }
}
