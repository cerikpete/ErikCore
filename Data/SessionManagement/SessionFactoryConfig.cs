using System.Collections;

namespace Data.SessionManagement
{
    /// <summary>
    /// Encapsulates multiple session factory configuration files.
    /// </summary>
    public class SessionFactoryConfig : IEnumerable
    {
        private readonly IList _configList;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionFactoryConfig"/> class.
        /// </summary>
        /// <param name="configList">The list of configuration files.</param>
        public SessionFactoryConfig(IList configList)
        {
            _configList = configList;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            foreach (string config in _configList)
            {
                yield return config;
            }
        }
    }
}