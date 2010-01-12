using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;

namespace Data.Repositories
{
    public interface IRepository<T>
    {
        /// <summary>
        /// Retrieves all of the objects of the specified type.
        /// </summary>
        /// <returns>An <see cref="IList{T}" /> containing all of the instances of the specified type.</returns>
        IList<T> GetAll();

        /// <summary>
        /// Saves the specified object to the database.
        /// </summary>
        /// <param name="entity">The object to save.</param>
        /// <returns>The saved version of the object.</returns>
        T Save(T entity);

        /// <summary>
        /// Deletes the specified object from the database.
        /// </summary>
        /// <param name="entity">Object to delete</param>
        void Delete(T entity);

        /// <summary>
        /// Retrieves an object from the database using the specified Id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// An instance of the specified object with the specified Id.
        /// </returns>
        /// <remarks>This method will not return a proxy object; it always returns the real object.</remarks>
        T GetById(int id);

        /// <summary>
        /// Retrieves an object from the database using the specified Id.
        /// </summary>
        /// <param name="id">The id of the object to retrieve</param>
        /// <returns>An instance of the specified object with the specified Id.</returns>
        /// <remarks>This method will return a proxy if the object is not already loaded
        /// in the session.</remarks>
        T LoadById(int id);

        /// <summary>
        /// Creates an <see cref="ICriteria" /> for the current session and type.
        /// </summary>
        /// <param name="criterion">The criterion to be added to the criteria object.</param>
        /// <returns>An <see cref="ICriteria" /> with the specified criterion objects added.</returns>
        ICriteria MakeCriteria(params ICriterion[] criterion);
    }
}