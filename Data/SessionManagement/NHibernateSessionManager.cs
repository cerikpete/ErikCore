using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using NHibernate;
using Utilities;

namespace Data.SessionManagement
{
    /// <summary>
    /// Handles the creation and management of the <see cref="ISessionFactory" />.
    /// </summary>
    public sealed class NHibernateSessionManager
    {
        private static readonly object _lock = new object();
        private readonly string _configPath;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateSessionManager"/> class.
        /// </summary>
        /// <param name="configPath">The path to the configuration file containing the NHibernate configuration.</param>
        public NHibernateSessionManager(string configPath) : this(configPath, new NHibernateConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateSessionManager"/> class.
        /// </summary>
        /// <param name="configPath">The path to the configuration file containing the NHibernate configuration.</param>
        /// <param name="configuration">The <see cref="IConfiguration" /> instance to obtain the
        /// <see cref="ISessionFactory" /> from.</param>
        public NHibernateSessionManager(string configPath, IConfiguration configuration)
        {
            _configPath = PathGenerator.GetExecutionPath(configPath);
            _configuration = configuration;
        }

        /// <summary>
        /// This method attempts to find a session factory in the <see cref="HttpRuntime.Cache" /> 
        /// via its name; if it can't be found it creates a new one and adds it to the cache.
        ///  </summary>
        /// <remarks>Even though this uses <see cref="HttpRuntime.Cache" />, it works in Windows
        /// applications; see http://www.codeproject.com/csharp/cacheinwinformapps.asp for an
        /// examination of this.</remarks>
        public ISessionFactory GetSessionFactory()
        {
            //  Attempt to retrieve a cached SessionFactory from the HttpRuntime's cache.
            var sessionFactory = (ISessionFactory) HttpRuntime.Cache.Get(_configPath);

            //  Failed to find a cached SessionFactory so make a new one.
            if (sessionFactory == null)
            {
                lock (_lock)
                {
                    sessionFactory = (ISessionFactory) HttpRuntime.Cache.Get(_configPath);

                    if (sessionFactory == null)
                    {
                        sessionFactory = _configuration.GetSessionFactory(_configPath);

                        if (sessionFactory == null)
                        {
                            throw new InvalidOperationException("cfg.BuildSessionFactory() returned null.");
                        }

                        HttpRuntime.Cache.Add(_configPath, sessionFactory, null, Cache.NoAbsoluteExpiration,
                                              TimeSpan.FromHours(1), CacheItemPriority.Normal, null);
                    }
                }
            }

            return sessionFactory;
        }

        /// <summary>
        /// Resets the session factory.
        /// </summary>
        /// <remarks>This method will remove the cached <see cref="ISessionFactory" /> so that the next method
        /// call will result in a new ISessionFactory being created.  Calls to this method should be considered
        /// carefully since construction of an ISessionFactory is expensive.</remarks>
        public void ResetSessionFactory()
        {
            HttpRuntime.Cache.Remove(_configPath);
            ContextData<ISession>().Clear();
            ContextData<ITransaction>().Clear();
        }

        /// <summary>
        /// Gets a session from the specified ISessionFactory.
        /// </summary>
        /// <returns>An <see cref="ISession" /> from the specified factory.</returns>
        public ISession GetSession()
        {
            return GetSession(null);
        }

        /// <summary>
        /// Gets a session from the specified ISessionFactory.
        /// </summary>
        /// <param name="interceptor">The <see cref="IInterceptor" /> to register with the session.  This parameter should be null
        /// for no interceptor.</param>
        /// <returns>An <see cref="ISession" /> from the specified factory.</returns>
        public ISession GetSession(IInterceptor interceptor)
        {
            ISession session;

            if (!ContextData<ISession>().TryGetValue(_configPath, out session))
            {
                if (interceptor != null)
                {
                    session = GetSessionFactory().OpenSession(interceptor);
                }
                else
                {
                    session = GetSessionFactory().OpenSession();
                }

                ContextData<ISession>()[_configPath] = session;
            }

            return session;
        }

        /// <summary>
        /// Closes the specified Session.
        /// </summary>
        public void CloseSession()
        {
            ISession session;
            ContextData<ISession>().TryGetValue(_configPath, out session);

            ContextData<ISession>().Remove(_configPath);
            if (session != null && session.IsOpen)
            {
                session.Close();
            }
        }

        /// <summary>
        /// Starts a transaction using the specified Session.
        /// </summary>
        public void BeginTransaction()
        {
            ITransaction transaction;

            if (!ContextData<ITransaction>().TryGetValue(_configPath, out transaction))
            {
                transaction = GetSession().BeginTransaction();
                ContextData<ITransaction>().Add(_configPath, transaction);
            }
        }

        /// <summary>
        /// Commits any open transaction in the specified Session.
        /// </summary>
        public void CommitTransaction()
        {
            ITransaction transaction = ContextData<ITransaction>()[_configPath];

            try
            {
                if (HasOpenTransaction(transaction))
                {
                    transaction.Commit();
                }
                ContextData<ITransaction>().Remove(_configPath);
            }            
            catch (HibernateException)
            {
                RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// Checks for an open transaction in the specified Session.
        /// </summary>
        /// <returns><see langword="true" /> if an open transaction exists; otherwise <see langword="false" />.</returns>
        public bool HasOpenTransaction()
        {
            ITransaction transaction;
            ContextData<ITransaction>().TryGetValue(_configPath, out transaction);

            return HasOpenTransaction(transaction);
        }

        /// <summary>
        /// Checks for an open transaction in the specified Session.
        /// </summary>
        /// <param name="transaction">The transaction to check.</param>
        /// <returns>
        /// 	<see langword="true"/> if the <paramref name="transaction" /> is not null and open; otherwise, <see langword="false"/>.
        /// </returns>
        private bool HasOpenTransaction(ITransaction transaction)
        {
            return transaction != null && !transaction.WasCommitted && !transaction.WasRolledBack;
        }

        /// <summary>
        /// Rolls back any open transaction in the specified Session.
        /// </summary>
        public void RollbackTransaction()
        {
            ITransaction transaction = ContextData<ITransaction>()[_configPath];

            try
            {
                if (HasOpenTransaction(transaction))
                {
                    transaction.Rollback();
                }
                ContextData<ITransaction>().Remove(_configPath);
            }
            finally
            {
                CloseSession();
            }
        }

        /// <summary>
        /// Returns an <see cref="IDictionary{TKey,TValue}" /> of open items keyed by the location
        /// of the NHibernate configuration file.
        /// </summary>
        /// <returns>An IDictionary of the currently open items.</returns>
        /// <remarks>
        /// Since multiple databases may be in use, there may be one item per database 
        /// open at any one time.  They are stored in a dictionary keyed by the location of the NHibernate configuration file.
        /// </remarks>
        private IDictionary<string, T> ContextData<T>()
        {
            IStorageContext<T> storage = GetStorageContext<T>();
            IDictionary<string, T> values = storage.GetStored();
            if (values == null)
            {
                values = new Dictionary<string, T>();
                storage.SetStored(values);
            }
            return values;
        }

        /// <summary>
        /// Returns an <see cref="IStorageContext{T}" /> based on the current environment.
        /// </summary>
        /// <returns>An IStorageContext appropriate to the current environment.</returns>
        private IStorageContext<T> GetStorageContext<T>()
        {
            IStorageContext<T> current;
            if (HttpContext.Current != null)
            {
                current = new HttpStorageContext<T>();
            }
            else
            {
                current = new WindowsStorageContext<T>();
            }
            return current;
        }
    }
}