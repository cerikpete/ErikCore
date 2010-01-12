using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;

namespace Testing.Specs
{
    [Category("Specifications")]
    public abstract class SpecificationsFor<SystemUnderTest> where SystemUnderTest : class
    {
        protected SystemUnderTest sut;
        private IDictionary<Type, object> dependencies;

        [SetUp]
        public void SetUp()
        {
            dependencies = new Dictionary<Type, object>();
            sut = SetUpSystemUnderTestWithMockedDependencies();
            Set_up_context();
            And_calling();
        }

        protected InterfaceType AMockOf<InterfaceType>() where InterfaceType : class
        {
            return MockRepository.GenerateMock<InterfaceType>();
        }

        protected DependencyType AMockedDependencyOfType<DependencyType>() where DependencyType : class
        {
            if (!dependencies.ContainsKey(typeof(DependencyType)))
            {
                dependencies[typeof(DependencyType)] = AMockOf<DependencyType>();
            }
            return (DependencyType)dependencies[typeof(DependencyType)];
        }

        protected SystemUnderTest SetUpSystemUnderTestWithMockedDependencies()
        {
            ConstructorInfo greediestConstructor = GetConstructorWithMostParameters();
            return (SystemUnderTest)Activator.CreateInstance(typeof(SystemUnderTest), MockedConstructorArgs(greediestConstructor).ToArray());
        }

        private List<object> MockedConstructorArgs(ConstructorInfo constructor)
        {
            List<object> mockedParameters = new List<object>();
            foreach (var parameterInfo in constructor.GetParameters())
            {
                if (parameterInfo.ParameterType.IsInterface)
                {
                    mockedParameters.Add(MockOfParameter(parameterInfo));
                }
            }
            return mockedParameters;
        }

        private object MockOfParameter(ParameterInfo parameterToMock)
        {
            if (!dependencies.ContainsKey(parameterToMock.ParameterType))
            {
                dependencies[parameterToMock.ParameterType] = MockTheParameter(parameterToMock);
            }
            return dependencies[parameterToMock.ParameterType];
        }

        /// <summary>
        /// Mock out the parameter supplied. To do this and have it work with the new AAA syntax, we need to use the mock repository to
        /// create a dynamic mock, then replay the mock in order to get it out of record and into replay mode.
        /// </summary>
        private object MockTheParameter(ParameterInfo parameterToMock)
        {
            MockRepository mockRepository = new MockRepository();
            object mockedObject = mockRepository.DynamicMock(parameterToMock.ParameterType);
            mockRepository.Replay(mockedObject);
            return mockedObject;
        }

        private ConstructorInfo GetConstructorWithMostParameters()
        {
            ConstructorInfo[] constructors = typeof(SystemUnderTest).GetConstructors();
            int parameterCount = 0;
            ConstructorInfo constructor = constructors[0];
            foreach (var constructorInfo in constructors)
            {
                if (constructorInfo.GetParameters().Length > parameterCount)
                {
                    parameterCount = constructorInfo.GetParameters().Length;
                    constructor = constructorInfo;
                }
            }
            return constructor;
        }

        protected ExpectationBuilder<DependencyType> WhenThe<DependencyType>(DependencyType dependency) where DependencyType : class
        {
            return new ExpectationBuilder<DependencyType>(dependency);
        }

        public virtual void Set_up_context() { }

        public abstract void And_calling();
    }
}