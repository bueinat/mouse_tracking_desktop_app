using mouse_tracking_web_app.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

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
                    DataReceivedEventHandler outputHandler, DataReceivedEventHandler errorHandler, CancellationToken cToken)
        {
            // set variables for process
            string startupPath = VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName;
            string pythonScript = $"{startupPath}\\{scriptName}";

            string fileName = WriteDictToCSV(argv);
            string args = $"-u {pythonScript} \"{fileName}\"";

            // create process
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

            // registrations
            process.ErrorDataReceived += errorHandler;
            process.OutputDataReceived += outputHandler;
            _ = cToken.Register(() => process.Kill());

            // start the process
            _ = process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            // wait for the process to exit
            process.WaitForExit();
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