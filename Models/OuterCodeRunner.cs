using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using RunProcessAsTask;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class OuterCodeRunner : INotifyPropertyChanged
    {
        private readonly MainControllerModel model;

        public OuterCodeRunner(MainControllerModel model)
        {
            this.model = model;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("OCR_" + e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<string> RunCmd(string scriptName, List<string> argv)
        {
            string startupPath = VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName;

            // full path of python interpreter
            // TODO: move it to some appsetting file
            string pythonPath = @"C:\Users\buein\AppData\Local\Microsoft\WindowsApps\python.exe";

            // python app to call
            string pythonScript = $"{startupPath}\\{scriptName}";

            List<string> new_argv = argv.Select(s => "\"" + s + "\"").ToList();
            string args = pythonScript + " " + string.Join(" ", new_argv);
            // Create new process start info
            ProcessStartInfo start = new ProcessStartInfo(pythonPath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = args
            };

            ProcessResults processResults = await ProcessEx.RunAsync(start);

            Console.WriteLine("run successful");
            return JoinStringArray(processResults.StandardOutput);
        }

        private string JoinStringArray(string[] stringArr)
        {
            string s = "";
            for (int i = 0; i < stringArr.Length; i++)
            {
                s = i == 0 ? stringArr[i] : s + "\r\n" + stringArr[i];
            }
            return s;
        }
    }
}