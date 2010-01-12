using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Core;
using Castle.Core.Resource;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Utilities;

namespace IoC.Windsor
{
    public static class IoC
    {
        /// <summary>
        /// The lifetime of a component retrieved from the IOC container.
        /// </summary>
        public enum LifeStyle
        {
            /// <summary>
            /// A new instance is created each time the component is resolved.
            /// </summary>
            Transient,
            /// <summary>
            /// The same instance is returned each time the component is resolved.
            /// </summary>
            Singleton
        }

        private static IDictionary<string, IWindsorContainer> _containerDictionary;
        private static IWindsorContainer _defaultContainer;

        static IoC()
        {
            InitializeConfiguration();
        }

        /// <summary>
        /// Returns the default container
        /// </summary>
        public static IWindsorContainer Container
        {
            get { return _defaultContainer; }
        }

        /// <summary>
        /// Uses the underlying IOC container to locate the concrete instance
        /// of the specified <typeparamref name="T">type</typeparamref>.
        /// </summary>
        /// <typeparam name="T">The service (interface) to create a concrete type for.</typeparam>
        /// <returns>The concrete type from the IOC container.</returns>
        public static T Resolve<T>()
        {
            return _defaultContainer.Resolve<T>();
        }

        /// <summary>
        /// Uses the underlying IOC container to locate the concrete instance
        /// of the specified <typeparamref name="T">type</typeparamref>.
        /// </summary>
        /// <typeparam name="T">The service (interface) to create a concrete type for.</typeparam>
        /// <param name="constructorArgs">The arguments to be passed to the class's constructor.</param>
        /// <returns>The concrete type from the IOC container.</returns>
        public static T Resolve<T>(IDictionary<string, object> constructorArgs)
        {
            IDictionary windsorArgs = new Hashtable(constructorArgs.Count);

            foreach (var entry in constructorArgs)
            {
                windsorArgs.Add(entry.Key, entry.Value);
            }
            return _defaultContainer.Resolve<T>(windsorArgs);
        }

        /// <summary>
        /// Uses the underlying IOC container to locate the concrete instance
        /// of the specified type.
        /// </summary>
        /// <param name="objectType">The type to retrieve a concrete instance of.</param>
        /// <returns>The concrete type from the IOC container.</returns>
        public static object Resolve(Type objectType)
        {
            return _defaultContainer.Resolve(objectType);
        }

        /// <summary>
        /// Uses the underlying IOC container to locate the concrete instance
        /// of the specified <typeparamref name="T">type</typeparamref>.
        /// </summary>
        /// <typeparam name="T">The service (interface) to create a concrete type for.</typeparam>
        /// <param name="constructorArgs">The arguments to be passed to the class's constructor.</param>
        /// <param name="instance">The concrete type from the IOC container or <see langword="null"/> if no suitable implementation
        /// can be found.</param>
        /// <returns><see langword="true" /> if an implentation can be found; otherwise <see langword="false" />.</returns>
        public static bool TryResolve<T>(IDictionary<string, object> constructorArgs, out T instance)
        {
            instance = default(T);
            bool success = false;

            if (_defaultContainer.Kernel.HasComponent(typeof (T)))
            {
                instance = Resolve<T>(constructorArgs);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Uses the underlying IOC container to locate the concrete instance
        /// of the specified <typeparamref name="T">type</typeparamref>.
        /// </summary>
        /// <typeparam name="T">The service (interface) to create a concrete type for.</typeparam>
        /// <param name="instance">The concrete type from the IOC container or <see langword="null"/> if no suitable implementation
        /// can be found.</param>
        /// <returns><see langword="true" /> if an implentation can be found; otherwise <see langword="false" />.</returns>
        public static bool TryResolve<T>(out T instance)
        {
            instance = default(T);
            bool success = false;

            if (_defaultContainer.Kernel.HasComponent(typeof (T)))
            {
                instance = Resolve<T>();
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Adds the type to the IOC container using the specified key and lifestyle.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type.</param>
        /// <param name="lifestyle">The lifestyle.</param>
        public static void AddType(string key, Type type, LifeStyle lifestyle)
        {
            _defaultContainer.AddComponentLifeStyle(key, type, GetWindsorLifestyle(lifestyle));
        }

        /// <summary>
        /// Adds the type to the IOC container using the specified key and lifestyle.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="serviceType">Type of the service (interface) to add.</param>
        /// <param name="classType">Type of the class implementing the service.</param>
        /// <param name="lifestyle">The lifestyle.</param>
        public static void AddType(string key, Type serviceType, Type classType, LifeStyle lifestyle)
        {
            _defaultContainer.AddComponentLifeStyle(key, serviceType, classType, GetWindsorLifestyle(lifestyle));
        }

        /// <summary>
        /// Resets the configuration to that contained in the configuration files.
        /// </summary>
        /// <remarks>This method is valuable primarily in testing situations.</remarks>
        public static void ResetConfiguration()
        {
            InitializeConfiguration();
        }

        /// <summary>
        /// Releases the specified instance from the container, performing any necessary
        /// cleanup.
        /// </summary>
        /// <param name="instance">The object instance to release.</param>
        public static void Release(object instance)
        {
            _defaultContainer.Release(instance);
        }

        private static void InitializeConfiguration()
        {
            if (_defaultContainer != null)
            {
                _defaultContainer.Dispose();
            }
            var file = new FileResource(PathGenerator.GetExecutionPath(WindsorConfig.DefaultConfig));
            _defaultContainer = new WindsorContainer(new XmlInterpreter(file));

            if (_containerDictionary == null)
            {
                _containerDictionary = new Dictionary<string, IWindsorContainer>();
                _containerDictionary.Add(WindsorConfig.DefaultConfig, _defaultContainer);
            }
        }

        private static LifestyleType GetWindsorLifestyle(LifeStyle lifestyle)
        {
            switch (lifestyle)
            {
                case LifeStyle.Transient:
                    return LifestyleType.Transient;
                case LifeStyle.Singleton:
                    return LifestyleType.Singleton;
                default:
                    throw new ArgumentOutOfRangeException("lifestyle", lifestyle.ToString());
            }
        }
    }

    public static class WindsorConfig
    {
        public const string DefaultConfig = "WindsorConfig.config";
    }
}