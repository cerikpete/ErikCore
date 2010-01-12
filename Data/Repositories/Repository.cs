using System.Collections.Generic;
using Data.SessionManagement;
using NHibernate;
using NHibernate.Criterion;

namespace Data.Repositories
{
    public class Repository<T> : IRepository<T>
    {
        private readonly string _configPath;

        public Repository(string configPath)
        {
            _configPath = configPath;
        }

        /// <summary>
        /// Retrieves all of the objects of the specified type.
        /// </summary>
        /// <returns>An <see cref="IList{T}" /> containing all of the instances of the specified type.</returns>
        public IList<T> GetAll()
        {
            return GetByCriteria();
        }

        /// <summary>
        /// Saves the specified object to the database.
        /// </summary>
        /// <param name="entity">The object to save.</param>
        /// <returns>The saved version of the object.</returns>
        public T Save(T entity)
        {
            NHibernateSession.Save(entity);
            return entity;
        }

        /// <summary>
        /// Deletes the specified object from the database.
        /// </summary>
        /// <param name="entity">Object to delete</param>
        public void Delete(T entity)
        {
            NHibernateSession.Delete(entity);
        }

        /// <summary>
        /// Retrieves objects of the specified type using the provided criteria.
        /// </summary>
        /// <param name="criterion">A list of <see cref="ICriterion"/>s to include in the filtering.</param>
        /// <returns>An <see cref="IList{T}" /> containing the matching instances.</returns>
        private IList<T> GetByCriteria(params ICriterion[] criterion)
        {
            ICriteria criteria = MakeCriteria(criterion);

            return criteria.List<T>();
        }

        /// <summary>
        /// Creates an HQL query using the specified HQL string.
        /// </summary>
        /// <param name="hql">The HQL string.</param>
        /// <returns>An <see cref="IQuery" /> for the specified HQL.</returns>
        protected IQuery GetQuery(string hql)
        {
            return NHibernateSession.CreateQuery(hql);
        }

        /// <summary>
        /// Creates an <see cref="ICriteria" /> for the current session and type.
        /// </summary>
        /// <param name="criterion">The criterion to be added to the criteria object.</param>
        /// <returns>An <see cref="ICriteria" /> with the specified criterion objects added.</returns>
        public ICriteria MakeCriteria(params ICriterion[] criterion)
        {
            ICriteria criteria = NHibernateSession.CreateCriteria(typeof(T));
            foreach (ICriterion crit in criterion)
            {
                criteria.Add(crit);
            }
            return criteria;
        }

        /// <summary>
        /// Retrieves an object from the database using the specified Id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// An instance of the specified object with the specified Id.
        /// </returns>
        /// <remarks>This method will not return a proxy object; it always returns the real object.</remarks>
        public virtual T GetById(int id)
        {
            return NHibernateSession.Get<T>(id);
        }

        /// <summary>
        /// Retrieves an object from the database using the specified Id.
        /// </summary>
        /// <param name="id">The id of the object to retrieve</param>
        /// <returns>An instance of the specified object with the specified Id.</returns>
        /// <remarks>This method will return a proxy if the object is not already loaded
        /// in the session.</remarks>
        public virtual T LoadById(int id)
        {
            return NHibernateSession.Load<T>(id);
        }

        /// <summary>
        /// Exposes the ISession used within the repository.
        /// </summary>
        protected ISession NHibernateSession
        {
            get
            {
                return new NHibernateSessionManager(_configPath).GetSession();
            }
        }

        /// <summary>
        /// Gets the named query from the current session.
        /// </summary>
        /// <param name="queryName">Name of the query.</param>
        /// <returns>An <see cref="IQuery" /> representing the named query.</returns>
        protected IQuery GetNamedQuery(string queryName)
        {
            return NHibernateSession.GetNamedQuery(queryName);
        }
    }
}