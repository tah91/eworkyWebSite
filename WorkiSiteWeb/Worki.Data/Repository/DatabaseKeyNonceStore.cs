using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.UnitOfWork;
using Worki.Infrastructure.Helpers;

namespace Worki.Data.Models
{
	public interface INonceRepository : IRepository<ApiNonce>
	{

	}

	/// <summary>
	/// A database-persisted nonce store.
	/// </summary>
	public class DatabaseKeyNonceStore : RepositoryBase<ApiNonce>, INonceRepository, INonceStore, ICryptoKeyStore
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseKeyNonceStore"/> class.
		/// </summary>
		public DatabaseKeyNonceStore(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

		#region INonceStore Members

		/// <summary>
		/// Stores a given nonce and timestamp.
		/// </summary>
		/// <param name="context">The context, or namespace, within which the
		/// <paramref name="nonce"/> must be unique.
		/// The context SHOULD be treated as case-sensitive.
		/// The value will never be <c>null</c> but may be the empty string.</param>
		/// <param name="nonce">A series of random characters.</param>
		/// <param name="timestampUtc">The UTC timestamp that together with the nonce string make it unique
		/// within the given <paramref name="context"/>.
		/// The timestamp may also be used by the data store to clear out old nonces.</param>
		/// <returns>
		/// True if the context+nonce+timestamp (combination) was not previously in the database.
		/// False if the nonce was stored previously with the same timestamp and context.
		/// </returns>
		/// <remarks>
		/// The nonce must be stored for no less than the maximum time window a message may
		/// be processed within before being discarded as an expired message.
		/// This maximum message age can be looked up via the
		/// <see cref="DotNetOpenAuth.Configuration.MessagingElement.MaximumMessageLifetime"/>
		/// property, accessible via the <see cref="DotNetOpenAuth.Configuration.DotNetOpenAuthSection.Configuration"/>
		/// property.
		/// </remarks>
		public bool StoreNonce(string context, string nonce, DateTime timestampUtc)
		{
			try
			{
				var dbSet = _Context.Set<ApiNonce>();
				dbSet.Add(new ApiNonce { Context = context, Code = nonce, Timestamp = timestampUtc });
				_Context.Commit();
				return true;
			}
			catch (Exception ex)
			{
				_Context.Complete();
				_Logger.Error("StoreNonce", ex);
				return false;
			}
		}

		#endregion

		#region ICryptoKeyStore Members

		public CryptoKey GetKey(string bucket, string handle)
		{
			// It is critical that this lookup be case-sensitive, which can only be configured at the database.
			var matches = from key in _Context.ApiSymmetricCryptoKeys
						  where key.Bucket == bucket && key.Handle == handle
						  select new CryptoKey(key.Secret, key.ExpiresUtc.AsUtc());

			return matches.FirstOrDefault();
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
		{
			return from key in _Context.ApiSymmetricCryptoKeys
				   where key.Bucket == bucket
				   orderby key.ExpiresUtc descending
				   select new KeyValuePair<string, CryptoKey>(key.Handle, new CryptoKey(key.Secret, key.ExpiresUtc.AsUtc()));
		}

		public void StoreKey(string bucket, string handle, CryptoKey key)
		{
			try
			{
				var keyRow = new ApiSymmetricCryptoKey()
				{
					Bucket = bucket,
					Handle = handle,
					Secret = key.Key,
					ExpiresUtc = key.ExpiresUtc,
				};

				var dbSet = _Context.Set<ApiSymmetricCryptoKey>();
				dbSet.Add(keyRow);
				_Context.Commit();
			}
			catch (Exception ex)
			{
				_Context.Complete();
				_Logger.Error("StoreKey", ex);
			}
		}

		public void RemoveKey(string bucket, string handle)
		{
			try
			{
				var dbSet = _Context.Set<ApiSymmetricCryptoKey>();
				var toRemove = dbSet.FirstOrDefault(k => k.Bucket == bucket && k.Handle == handle);
				dbSet.Remove(toRemove);
			}
			catch (Exception ex)
			{
				_Logger.Error("RemoveKey", ex);
			}
		}

		#endregion
	}

	public interface IApiClientRepository : IRepository<ApiClient>
	{

	}

	public class ApiClientRepository : RepositoryBase<ApiClient>, IApiClientRepository
	{
		public ApiClientRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}
	}

	public interface IApiClientAuthorizationRepository : IRepository<ApiClientAuthorization>
	{

	}

	public class ApiClientAuthorizationRepository : RepositoryBase<ApiClientAuthorization>, IApiClientAuthorizationRepository
	{
		public ApiClientAuthorizationRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}
	}
}