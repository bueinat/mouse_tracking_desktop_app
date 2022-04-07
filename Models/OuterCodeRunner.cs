using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace mouse_tracking_web_app.Models
{
    public static class VisualStudioProvider
    {
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(
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
            delegate (Object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("OCR_" + e.PropertyName);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string PatchParameter(string parameter, int serviceid)
        {
            var engine = Python.CreateEngine(); // Extract Python language engine from their grasp
            var scope = engine.CreateScope(); // Introduce Python namespace (scope)
            var d = new Dictionary<string, object>
            {
                {"serviceid", serviceid},
                {"parameter", parameter}
            }; // Add some sample parameters. Notice that there is no need in specifically setting the object type, interpreter will do that part for us in the script properly with high probability

            scope.SetVariable("params", d); // This will be the name of the dictionary in python script, initialized with previously created .NET Dictionary
            ScriptSource source = engine.CreateScriptSourceFromFile("C:\\Users\\buein\\Downloads\\test_code.py"); // Load the script
            object result = source.Execute(scope);
            parameter = scope.GetVariable<string>("parameter"); // To get the finally set variable 'parameter' from the python script
            return parameter;
        }

        public string RunCmd(string scriptName, List<string> argv)
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
                // make sure we can read the output from stdout
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,

                // start python app with 3 arguments
                // 1st arguments is pointer to itself,
                // 2nd and 3rd are actual arguments we want to send
                Arguments = args
            };
            Console.WriteLine(args);

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string stderr = process.StandardError.ReadToEnd();  // Here are the exceptions from our Python script
                    Console.WriteLine(stderr);
                    string result = reader.ReadToEnd();                 // Here is the result of StdOut(for example: print "test")
                    return result;
                }
            }
        }

        public void RunScriptWithCMDArgs(string scriptName, List<string> argv)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            engine.GetSysModule().SetVariable("argv", argv);
            string startupPath = VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName;
            ICollection<string> Paths = SetPaths(engine); //THIS SETS THE PYTHON LIBRARY PATHS
            engine.SetSearchPaths(Paths);

            //string startupPath = Path.getModulePath();
            //Assembly.getExecutiveAssebly.getdirectory path
            engine.ExecuteFile(@"C:/Users/buein/Downloads/test_code.py", scope);
            engine.ExecuteFile($"{startupPath}\\{scriptName}", scope);
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static ICollection<string> SetPaths(ScriptEngine engine)
        {
            var Paths = engine.GetSearchPaths();
            Paths.Add(@"C:\Users\buein\anaconda3");
            Paths.Add(@"C:\Users\buein\anaconda3\python37.zip");
            Paths.Add(@"C:\Users\buein\anaconda3\DLLs");
            Paths.Add(@"C:\Users\buein\anaconda3\Lib");
            Paths.Add(@"C:\Users\buein\anaconda3");
            Paths.Add(@"C:\Users\buein\anaconda3\Lib\site-packages");
            Paths.Add(@"C:\Users\buein\anaconda3\Lib\site-packages\win32");
            Paths.Add(@"C:\Users\buein\anaconda3\Lib\site-packages\win32\lib");
            Paths.Add(@"C:\Users\buein\anaconda3\Lib\site-packages\Pythonwin");
            Paths.Add(@"C:\Users\buein\anaconda3\Lib\site-packages\IPython\extensions");
            //Paths.Add(@"C:\Users\lee.williams.ipython");
            return Paths;
        }
    }
}