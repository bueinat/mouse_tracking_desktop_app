using RunProcessAsTask;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.Models
{
    public static class VisualStudioProvider
    {
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            DirectoryInfo directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }
    }

    public class OuterCodeRunner
    {

        public async Task<string[]> RunCmd(string scriptName, List<string> argv)
        {
            string startupPath = VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName;

            // python app to call
            string pythonScript = $"{startupPath}\\{scriptName}";

            List<string> new_argv = argv.Select(s => "\"" + s + "\"").ToList();
            string args = pythonScript + " " + string.Join(" ", new_argv);
            // Create new process start info
            ProcessStartInfo start = new ProcessStartInfo(ConfigurationManager.AppSettings.Get("PythonPath"))
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = args
            };

            ProcessResults processResults = await ProcessEx.RunAsync(start);

            return processResults.StandardOutput;
        }
    }
}