using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.XPath;
using Data.Repositories;
using Data.SessionManagement;
using NUnit.Framework;
using Utilities;

namespace Testing.Mapping
{
    [Category("Mapping Tests")]
    public abstract class MappingTestFor<SystemUnderTest, RepositoryForSystemUnderTest> where RepositoryForSystemUnderTest : IRepository<SystemUnderTest>
    {
        protected SystemUnderTest systemUnderTest;
        private DataRow testDataRow;
        private readonly NHibernateSessionManager _manager;
        private readonly ReflectiveMapper _reflectiveMapper;
        private readonly ScriptRunner _scriptRunner;

        private const string ConfigFileName = "nhibernate.config";

        public MappingTestFor()
        {
            _manager = new NHibernateSessionManager(ConfigFileName);
            _reflectiveMapper = new ReflectiveMapper();
            _scriptRunner = new ScriptRunner(ExtractConnectionString());
        }

        /// <summary>
        /// Method to save the test data to the data store using the appropriate Dao.
        /// </summary>
        private void SaveSystemUnderTest()
        {
            // Clear out any objects that may be in the session
            _manager.GetSession().Clear();

            StartTransaction();

            // Save the system under test with the correct Dao
            var repositoryForSystemUnderTest = InstantiateRepositoryForSystemUnderTest();
            repositoryForSystemUnderTest.Save(systemUnderTest);
            ResetSession();
        }

        private void LoadSystemUnderTestUsingReflection()
        {
            systemUnderTest = LoadObjectUsingReflection<SystemUnderTest>();
        }

        protected TestObjectType LoadObjectUsingReflection<TestObjectType>()
        {
            return _reflectiveMapper.LoadObjectUsingReflection<TestObjectType>();
        }

        private RepositoryForSystemUnderTest InstantiateRepositoryForSystemUnderTest()
        {
            return (RepositoryForSystemUnderTest)Activator.CreateInstance(typeof(RepositoryForSystemUnderTest), new[] { FullyQualifiedConfigFileName });
        }

        protected abstract string SqlToRetrieveTestDataRow { get; }

        private void GetDataRowWithTestData()
        {
            if (testDataRow == null)
            {
                SqlConnection conn = new SqlConnection(ExtractConnectionString());
                SqlCommand cmd = new SqlCommand(SqlToRetrieveTestDataRow, conn);
                cmd.CommandType = CommandType.Text;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                conn.Open();
                try
                {
                    DataSet ds = new DataSet();
                    dataAdapter.Fill(ds);
                    testDataRow = ds.Tables[0].Rows[0];
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        protected void SetField(object target, string fieldName, object value)
        {
            _reflectiveMapper.SetField(target, fieldName, value);
        }

        protected string ExtractConnectionString()
        {
            XPathDocument doc = new XPathDocument(PathGenerator.GetExecutionPath(ConfigFileName));
            XPathNavigator xpath = doc.CreateNavigator();
            XmlNamespaceManager manager = new XmlNamespaceManager(xpath.NameTable);
            manager.AddNamespace("h", "urn:nhibernate-configuration-2.2");
            string connectionString =
                xpath.SelectSingleNode(
                    "/h:hibernate-configuration/h:session-factory/h:property[@name='connection.connection_string']",
                    manager).InnerXml;
            return connectionString;
        }

        protected ValueChecker EnsureValueIn(Func<SystemUnderTest, object> property)
        {
            object valueToCheck = property.Invoke(systemUnderTest);
            return EnsureThat(valueToCheck);
        }

        protected ValueChecker EnsureThat(object valueToCheck)
        {
            GetDataRowWithTestData();
            return new ValueChecker(testDataRow, valueToCheck);
        }

        /// <summary>
        /// Saves test objects to the database before setting up the system under test.  Used if the system under test
        /// depends on other objects existing before it can be created.
        /// </summary>
        /// <param name="objectsToSave">Set of objects to save before setting up the system under test.</param>
        protected virtual void SaveDependentObjectsNeededBySystemUnderTest(params object[] objectsToSave)
        {
            foreach (var testObject in objectsToSave)
            {
                SaveObject(testObject);
            }
        }

        /// <summary>
        /// Virtual method used to set up test values on the system under test when the values inserted via reflection are not sufficient.  Also
        /// optionally will save any objects needed to correctly set up the system under test.
        /// </summary>
        protected virtual void SetUpTestDataOnSystemUnderTest()
        {
            SaveDependentObjectsNeededBySystemUnderTest();
        }

        /// <summary>
        /// Sets up the system under test with test data.
        /// </summary>
        private void SetUpSystemUnderTest()
        {
            LoadSystemUnderTestUsingReflection();
            SetUpTestDataOnSystemUnderTest();
        }

        protected string FullyQualifiedConfigFileName
        {
            get { return PathGenerator.GetExecutionPath(ConfigFileName); }
        }

        protected void SaveObject<T>(T objectToSave)
        {
            try
            {
                IRepository<T> repository = new Repository<T>(FullyQualifiedConfigFileName);
                repository.Save(objectToSave);
            }
            catch (NHibernate.NonUniqueObjectException)
            {
                // Suppress this exception since we don't care for testing if the same object was somehow added twice to the session
                // across tests.
            }
        }

        protected T GetObject<T>(int id)
        {
            IRepository<T> repository = new Repository<T>(FullyQualifiedConfigFileName);            
            return repository.GetById(id);
        }

        #region Transaction/Session Methods

        private void StartTransaction()
        {
            _manager.BeginTransaction();
        }

        private void CommitTransaction()
        {
            _manager.CommitTransaction();
        }

        private void CloseSession()
        {            
            _manager.CloseSession();
        }

        private void ResetSession()
        {
            CommitTransaction();
            CloseSession();
        }

        #endregion

        #region Setup/Teardown

        [TestFixtureSetUp]
        public void BaseSetup()
        {
            try
            {
                _scriptRunner.RunSetUpFile();
                SetUpSystemUnderTest();
                SaveSystemUnderTest();
                CoreSetup();
            }
            catch
            {
                // Errors don't hit tear down, so ensure we close the session, then throw the exception so we don't swallow it
                CloseSession();
                throw;
            }
        }

        [TestFixtureTearDown]
        public void BaseTearDown()
        {
            CleanUpTestData();
        }

        private void CleanUpTestData()
        {
            _scriptRunner.RunTearDownFile();
            CoreTearDown();
        }

        protected virtual void CoreSetup()
        {
        }

        protected virtual void CoreTearDown()
        {
        }

        #endregion
    }
}