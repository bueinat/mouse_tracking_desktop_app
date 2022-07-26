using System;
using System.IO;
using System.Linq;

namespace mouse_tracking_web_app.Utils
{
    /// <summary>
    /// Class <c>UtilMethod</c> contains a collection of static method which are used in the code.
    /// </summary>
    public static class UtilMethods
    {
        /// <summary>
        /// Method <c>MakeRelative</c> gets two paths and return the first one as related to the second one.
        /// </summary>
        /// <param name="filePath">Full path of a file.</param>
        /// <param name="referencePath">A reference path to be related to.</param>
        /// <returns>Relative path of <paramref name="filePath"/> starting from <paramref name="referencePath"/>.</returns>
        public static string MakeRelative(string filePath, string referencePath)
        {
            Uri fileUri = new Uri(filePath);
            Uri referenceUri = new Uri(referencePath);
            string relativePath =  Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
            return relativePath.Substring(relativePath.IndexOf('\\') + 1);
        }

        /// <summary>
        /// Method <c>TryGetSolutionDirectoryInfo</c> returns the directory in which the solution exists.
        /// </summary>
        /// <param name="currentPath"><c>pwd</c></param>
        /// <returns>Path of the solution.</returns>
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            // get current directory
            DirectoryInfo directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());

            // go to parent until you find the solution path
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }
    }
}