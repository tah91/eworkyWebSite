using System;
using Worki.Web.Models;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using Worki.Web.Infrastructure.Logging;

namespace Worki.Web.Infrastructure.Repository
{
	public abstract class RepositoryBase<T> where T : class
	{
		protected ILogger _Logger;

		public RepositoryBase(ILogger logger)
		{
			_Logger = logger;
		}

		public virtual void Add(T entity)
		{
			try
			{
				using (var db = new WorkiDBEntities())
				{
					var dbSet = db.Set<T>();
					dbSet.Add(entity);
					db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Add", ex);
			}
		}

		public virtual void Delete(int key)
		{
			try
			{
				using (var db = new WorkiDBEntities())
				{
					var dbSet = db.Set<T>();
					var entity = dbSet.Find(key);
					dbSet.Remove(entity);
					db.SaveChanges();
				}
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
				using (var db = new WorkiDBEntities())
				{
					var dbSet = db.Set<T>();
					IEnumerable<T> toRemove = dbSet.Where<T>(where).AsEnumerable();
					foreach (T obj in toRemove)
					{
						dbSet.Remove(obj);
					}
					db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Delete", ex);
			}
		}


		public virtual T Get(int key)
		{
			try
			{
				var db = new WorkiDBEntities();
				var dbSet = db.Set<T>();
				var entity = dbSet.Find(key);
				return entity;
			}
			catch (Exception ex)
			{
				_Logger.Error("Get", ex);
				return null;
			}
		}

		public virtual void Update(int key, Action<T> actionToPerform)
		{
			try
			{
				using (var db = new WorkiDBEntities())
				{
					var dbSet = db.Set<T>();
					var entity = dbSet.Find(key);
					if (entity != null)
					{
						actionToPerform.Invoke(entity);
						db.SaveChanges();
					}
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Update", ex);
			}
		}

		public virtual IList<T> GetAll()
		{
			try
			{
				var db = new WorkiDBEntities();
				var dbSet = db.Set<T>();
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
				var db = new WorkiDBEntities();
				var dbSet = db.Set<T>();
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
				using (var db = new WorkiDBEntities())
				{
					var dbSet = db.Set<T>();
					return dbSet.Count();
				}
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
				var db = new WorkiDBEntities();
				var dbSet = db.Set<T>();
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
				var db = new WorkiDBEntities();
				var dbSet = db.Set<T>();
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
