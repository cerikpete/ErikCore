using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Data.SessionManagement
{
    /// <summary>
    /// Represents an abstract storage location for application data.
    /// </summary>
    /// <typeparam name="T">The type of application data to store.</typeparam>
    public interface IStorageContext<T>
    {
        /// <summary>
        /// Returns the value at the provided key.
        /// </summary>
        /// <returns>An <see cref="IDictionary{TKey,TValue}" /> of the stored data.</returns>
        IDictionary<string, T> GetStored();

        /// <summary>
        /// Stores the specified value at the specified key.
        /// </summary>
        /// <param name="data">The dictionary to store.</param>
        void SetStored(IDictionary<string, T> data);

        /// <summary>
        /// Clears the storage context.
        /// </summary>
        void Clear();
    }

    internal class HttpStorageContext<T> : IStorageContext<T>
    {
        public IDictionary<string, T> GetStored()
        {
            return (IDictionary<string, T>)HttpContext.Current.Items[Key];
        }

        public void SetStored(IDictionary<string, T> data)
        {
            HttpContext.Current.Items[Key] = data;
        }

        public void Clear()
        {
            HttpContext.Current.Items[Key] = null;
        }

        private string Key
        {
            get { return typeof (T).FullName; }
        }
    }

    /// <summary>
    /// Stores its data in the <see cref="CallContext" />.
    /// </summary>
    /// <remarks>Discussion concerning this can be found at http://forum.springframework.net/showthread.php?t=572</remarks>
    internal class WindowsStorageContext<T> : IStorageContext<T>
    {
        public IDictionary<string, T> GetStored()
        {
            return (IDictionary<string, T>)CallContext.GetData(Key);
        }

        public void SetStored(IDictionary<string, T> data)
        {
            CallContext.SetData(Key, data);
        }

        public void Clear()
        {
            CallContext.SetData(Key, null);
        }

        private string Key
        {
            get { return typeof (T).FullName; }
        }
    }
}