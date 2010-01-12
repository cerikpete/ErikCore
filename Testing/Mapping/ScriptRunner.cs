using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Subtext.Scripting;

namespace Testing.Mapping
{
    /// <summary>
    /// Class containing the functionality to run set up and tear down scripts for mapping tests.
    /// </summary>
    public class ScriptRunner
    {
        private readonly string connectionString;
        private const string SetupFile = @"Files\CreateObjects.sql";
        private const string TearDownFile = @"Files\DropObjects.sql";

        public ScriptRunner(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void RunSetUpFile()
        {
            RunSetUpFile(SetupFile);
        }

        public void RunSetUpFile(string setUpFile)
        {
            SqlScriptRunner runner = new SqlScriptRunner(GetCommandText(setUpFile));
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    runner.Execute(trans);
                    trans.Commit();
                }
            }
        }

        public void RunTearDownFile()
        {
            SqlScriptRunner runner = new SqlScriptRunner(GetCommandText(TearDownFile));
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    runner.Execute(trans);
                    trans.Commit();
                }
            }
        }

        private string GetCommandText(string fileName)
        {
            string command;
            using (FileStream fs = new FileStream(MakeFullPath(fileName), FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                command = sr.ReadToEnd();
            }
            return command;

        }

        private string MakeFullPath(string fileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));
            assemblyPath = System.Text.RegularExpressions.Regex.Replace(assemblyPath, @"bin\\.*", "");
            return Path.Combine(assemblyPath, fileName);
        }
    }
}