using System;
using Worki.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Repository
{
	public abstract class RepositoryBase<T> where T : class
	{
		protected ILogger _Logger;
		protected WorkiDBEntities _Context;

		public RepositoryBase(ILogger logger, IUnitOfWork context)
		{
			_Logger = logger;
			_Context = context as WorkiDBEntities;
		}

		public virtual void Add(T entity)
		{
			try
			{
				var dbSet = _Context.Set<T>();
				dbSet.Add(entity);
			}
			catch (Exception ex)
			{
				_Logger.Error("Add", ex);
			}
		}

		public virtual void Delete(params object[] keys)
		{
			try
			{
				var dbSet = _Context.Set<T>();
                var entity = dbSet.Find(keys);
				dbSet.Remove(entity);
			}
			catch (Exception ex)
			{
				_Logger.Error("Delete", ex);
			}
		}

		public virtual void Delete(Expression<Func<T, bool>> where)
		{
			try
			{
				var dbSet = _Context.Set<T>();
				IEnumerable<T> toRemove = dbSet.Where<T>(where).AsEnumerable();
				foreach (T obj in toRemove)
				{
					dbSet.Remove(obj);
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Delete", ex);
			}
		}

        public virtual T Get(params object[] keys)
		{
			try
			{
				var dbSet = _Context.Set<T>();
				var entity = dbSet.Find(keys);
				return entity;
			}
			catch (Exception ex)
			{
				_Logger.Error("Get", ex);
				return null;
			}
		}

		public virtual IList<T> GetAll()
		{
			try
			{
				var dbSet = _Context.Set<T>();
				return dbSet.ToList();
			}
			catch (Exception ex)
			{
				_Logger.Error("GetAll", ex);
				return null;
			}
		}

		public virtual IList<T> Get(int start, int pageSize, Expression<Func<T, int>> keySelector)
		{
			try
			{
				var dbSet = _Context.Set<T>();
				return dbSet.OrderByDescending(keySelector).Skip(start).Take(pageSize).ToList();
			}
			catch (Exception ex)
			{
				_Logger.Error("Get", ex);
				return null;
			}
		}

		public virtual int GetCount()
		{
			try
			{
				var dbSet = _Context.Set<T>();
				return dbSet.Count();
			}
			catch (Exception ex)
			{
				_Logger.Error("GetCount", ex);
				return 0;
			}
		}

		public virtual IList<T> GetMany(Expression<Func<T, bool>> where)
		{
			try
			{
				var dbSet = _Context.Set<T>();
				return dbSet.Where(where).ToList();
			}
			catch (Exception ex)
			{
				_Logger.Error("GetMany", ex);
				return null;
			}
		}

		public virtual T Get(Expression<Func<T, bool>> where)
		{
			try
			{
				var dbSet = _Context.Set<T>();
				return dbSet.Where(where).FirstOrDefault();
			}
			catch (Exception ex)
			{
				_Logger.Error("Get", ex);
				return null;
			}
		}
	}
}
