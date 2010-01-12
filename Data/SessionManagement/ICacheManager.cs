using System;

namespace Data.SessionManagement
{
    /// <summary>
    /// Represents actions that can be taken on the cache.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Clears the second level cache for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        void ClearSecondLevelCache(Type type);
        /// <summary>
        /// Clears the query cache.
        /// </summary>
        void ClearQueryCache();
    }

    /// <summary>
    /// An implementation of <see cref="ICacheManager" /> dealing with the NHibernate cache.
    /// </summary>
    public class NHibernateCacheManager : ICacheManager
    {
        private readonly string _configPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateCacheManager"/> class.
        /// </summary>
        /// <param name="configPath">The path to the configuration file containing the NHibernate configuration.</param>
        public NHibernateCacheManager(string configPath)
        {
            _configPath = configPath;
        }

        /// <summary>
        /// Clears the second level cache for the specified entity type.
        /// </summary>
        /// <param name="type">The type.</param>
        public void ClearSecondLevelCache(Type type)
        {
            var sessionFactory = new NHibernateSessionManager(_configPath).GetSessionFactory();
            sessionFactory.Evict(type);
        }

        /// <summary>
        /// Clears the query cache.
        /// </summary>
        public void ClearQueryCache()
        {
            var sessionFactory = new NHibernateSessionManager(_configPath).GetSessionFactory();
            sessionFactory.EvictQueries();
        }
    }
}