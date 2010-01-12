using NHibernate;
using NHibernate.Cfg;

namespace Data.SessionManagement
{
    /// <summary>
    /// Represents a method of obtaining NHibernate's configuration.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Gets an <see cref="ISessionFactory" /> for the specified configuration file.
        /// </summary>
        /// <param name="configPath">The path to the configuration file containing the NHibernate configuration.</param>
        /// <returns>An <see cref="ISessionFactory" /> based on the supplied configuration file.</returns>
        ISessionFactory GetSessionFactory(string configPath);
    }

    /// <summary>
    /// An <see cref="IConfiguration" /> the gets its configuration from an NHibernate configuration file.
    /// </summary>
    public class NHibernateConfiguration : IConfiguration
    {
        /// <summary>
        /// Gets an <see cref="ISessionFactory"/> for the specified configuration file.
        /// </summary>
        /// <param name="configPath">The path to the configuration file containing the NHibernate configuration.</param>
        /// <returns>
        /// An <see cref="ISessionFactory"/> based on the supplied configuration file.
        /// </returns>
        public ISessionFactory GetSessionFactory(string configPath)
        {
            Configuration cfg = new Configuration();
            cfg.Configure(configPath);

            INHibernateInitialization initialization;
            if (IoC.Windsor.IoC.TryResolve(out initialization))
            {
                initialization.Configured(cfg);
            }

            return cfg.BuildSessionFactory();
        }
    }
}