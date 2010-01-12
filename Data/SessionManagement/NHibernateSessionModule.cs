using System;
using System.Web;

namespace Data.SessionManagement
{
    /// <summary>
    /// Implements the Open-Session-In-View pattern using <see cref="NHibernateSessionManager" />.
    /// </summary>
    /// <remarks>See http://www.hibernate.org/43.html or Chapter 16 of "Java Persistence with Hibernate"
    /// for a discussion of the Open-Session-In-View pattern.</remarks>
    public class NHibernateSessionModule : IHttpModule
    {
        private SessionFactoryConfig _config;

        private SessionFactoryConfig FactoryConfig
        {
            get
            {
                if (_config == null)
                {
                    _config = IoC.Windsor.IoC.Resolve<SessionFactoryConfig>();
                }
                return _config;
            }
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"></see> that provides access to the methods, properties, and events common to
        /// all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginTransaction;
            context.EndRequest += CommitAndCloseSession;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that
        /// implements <see cref="T:System.Web.IHttpModule"></see>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Opens a session within a transaction at the beginning of the HTTP request.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">An <see cref="EventArgs" /> instance that contains no data.</param>
        private void BeginTransaction(object sender, EventArgs e)
        {
            foreach (string factoryConfig in FactoryConfig)
            {
                new NHibernateSessionManager(factoryConfig).BeginTransaction();
            }
        }

        /// <summary>
        /// Commits and closes the NHibernate session provided by the supplied <see cref="NHibernateSessionManager"/>.
        /// Assumes a transaction was begun at the beginning of the request; but a transaction or session does
        /// not *have* to be opened for this to operate successfully.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">An <see cref="EventArgs" /> instance that contains no data.</param>
        private void CommitAndCloseSession(object sender, EventArgs e)
        {
            try
            {
                // Commit every open session factory
                foreach (string factoryConfig in FactoryConfig)
                {
                    new NHibernateSessionManager(factoryConfig).CommitTransaction();
                }
            }
            finally
            {
                // No matter what happens, make sure all the sessions get closed
                foreach (string factoryConfig in FactoryConfig)
                {
                    new NHibernateSessionManager(factoryConfig).CloseSession();
                }
            }
        }
    }
}