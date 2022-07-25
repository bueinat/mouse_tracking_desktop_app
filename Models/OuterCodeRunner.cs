using mouse_tracking_web_app.ViewModels;
using RunProcessAsTask;
using System;
using System.Collections.Generic;
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
        public SettingsManager SM;

        public OuterCodeRunner(SettingsManager sManager)
        {
            SM = sManager;
        }

        public static string WriteDictToCSV(Dictionary<string, string> data)
        {
            string csv = string.Join(
                Environment.NewLine,
                data.Select(d => $"{d.Key},{d.Value}")
            );
            string fileName = CreateTmpFile();
            File.WriteAllText(fileName, csv);
            return fileName;
        }

        public void RunCmd(string scriptName, Dictionary<string, string> argv,
                    DataReceivedEventHandler outputHandler, DataReceivedEventHandler errorHandler)
        {
            string startupPath = VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName;

            // python app to call
            string pythonScript = $"{startupPath}\\{scriptName}";

            string fileName = WriteDictToCSV(argv);
            string args = $"-u {pythonScript} \"{fileName}\"";

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = SM.PythonPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.ErrorDataReceived += errorHandler;
            process.OutputDataReceived += outputHandler;

            _ = process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        public List<string> RunCmd_(string scriptName, Dictionary<string, string> argv)
        {
            string startupPath = VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName;

            // python app to call
            string pythonScript = $"{startupPath}\\{scriptName}";

            string fileName = WriteDictToCSV(argv);
            string args = $"{pythonScript} \"{fileName}\"";

            // Create new process start info
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(SM.PythonPath)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = args
                }
            };

            _ = process.Start();

            // Synchronously read the standard output of the spawned process.
            StreamReader reader = process.StandardOutput;
            string line = reader.ReadLine();
            List<string> output = new List<string>();
            while (!string.IsNullOrEmpty(line))
            {
                output.Add(line);
                line = reader.ReadLine();
            }
            //string output = reader.ReadToEnd();

            // Write the redirected output to this application's window.
            //Console.WriteLine(output);

            process.WaitForExit();

            return output;
            //return processResults.StandardOutput;
        }

        // the following method was taken from here: https://www.daveoncsharp.com/2009/09/how-to-use-temporary-files-in-csharp/
        private static string CreateTmpFile()
        {
            string fileName = string.Empty;

            try
            {
                // Get the full name of the newly created Temporary file.
                // Note that the GetTempFileName() method actually creates
                // a 0-byte file and returns the name of the created file.
                fileName = Path.GetTempFileName();

                // Craete a FileInfo object to set the file's attributes
                FileInfo fileInfo = new FileInfo(fileName)
                {
                    // Set the Attribute property of this file to Temporary.
                    // Although this is not completely necessary, the .NET Framework is able
                    // to optimize the use of Temporary files by keeping them cached in memory.
                    Attributes = FileAttributes.Temporary
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create TEMP file or set its attributes: " + ex.Message);
            }

            return fileName;
        }
    }
}