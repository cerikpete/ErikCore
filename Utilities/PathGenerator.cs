using System.Web;

namespace Utilities
{
    /// <summary>
    /// Generates the path information based on the current execution environment.
    /// </summary>
    public static class PathGenerator
    {
        /// <summary>
        /// Gets the full path to the configuration file based on the current execution environment.
        /// </summary>
        /// <param name="configFilename">The name of the configuration file.</param>
        /// <returns>The absolute path to the configuration file.</returns>
        public static string GetExecutionPath(string configFilename)
        {
            if (IsWeb)
            {
                return System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~"), configFilename);
            }
            string assemblyPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));
            return System.IO.Path.Combine(assemblyPath, configFilename);
        }

        /// <summary>
        /// Gets a value indicating whether the current execution environment is a web environment.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if the current environment is a web one; otherwise, <see langword="false"/>.
        /// </value>
        private static bool IsWeb
        {
            get { return HttpContext.Current != null; }
        }
    }
}