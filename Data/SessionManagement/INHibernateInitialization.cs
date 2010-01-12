using NHibernate.Cfg;

namespace Data.SessionManagement
{
    /// <summary>
    /// Provides a callback to perform additional NHibernate configuration.  Based on the
    /// same function from Rhino.Commons.
    /// </summary>
    public interface INHibernateInitialization
    {
        /// <summary>
        /// Called when the <see cref="Configuration" /> has been created based on
        /// the config file.
        /// </summary>
        /// <param name="cfg">The <see cref="Configuration" /> instance be initialized.</param>
        void Configured(Configuration cfg);
    }
}