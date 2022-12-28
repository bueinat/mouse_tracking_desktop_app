using mouse_tracking_web_app.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace mouse_tracking_web_app.Models
{
    /// <summary>
    /// Class <c>OuterCodeRunner</c> is helpout class for running Python scripts.
    /// </summary>
    public class OuterCodeRunner
    {
        /// <summary>
        /// Property <c>SM</c> is a <paramref name="SettingsManager"/> from which you get the relevant settings properties.
        /// </summary>
        public SettingsManager SM;

        /// <summary>
        /// This constructor only gets a <paramref name="SettingsManager"/>.
        /// </summary>
        /// <param name="sManager"></param>
        public OuterCodeRunner(SettingsManager sManager)
        {
            SM = sManager;
        }

        /// <summary>
        /// Method <c>RunCmd</c> run a given python code and process its output and error
        /// </summary>
        /// <param name="scriptName">the script which you want to run</param>
        /// <param name="argv">dictionary of arguments which will be passed to the script</param>
        /// <param name="outputHandler">handler which will treat the standard output</param>
        /// <param name="errorHandler">handler which will treat the standard error</param>
        /// <param name="cToken">cancellation token that will kill the script</param>
        public void RunCmd(string scriptName, Dictionary<string, string> argv,
                    DataReceivedEventHandler outputHandler, DataReceivedEventHandler errorHandler, CancellationToken cToken)
        {
            // set variables for process
            string startupPath = Utils.UtilMethods.TryGetSolutionDirectoryInfo().FullName;
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
            cToken.Register(() => process.Kill());

            // start the process
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            // wait for the process to exit
            process.WaitForExit();
        }

        /// <summary>
        /// Method <c>WriteDictToCSV</c> write a dictionary to <c>csv</c> file.
        /// </summary>
        /// <param name="data">dictionary to be saved.</param>
        /// <returns>Path of the file in which the data was saved.</returns>
        private static string WriteDictToCSV(Dictionary<string, string> data)
        {
            // convert dictionary to string
            string csv = string.Join(
                Environment.NewLine,
                data.Select(d => $"{d.Key},{d.Value}")
            );

            // create a temp file and write to it
            string fileName = CreateTmpFile();
            File.WriteAllText(fileName, csv);
            return fileName;
        }

        /// <summary>
        /// Method <c>CreateTmpFile</c> create a temporary file and returns its path.
        /// </summary>
        /// <returns>Path of the generated file.</returns>
        private static string CreateTmpFile()
        {
            string fileName = string.Empty;

            try
            {
                // create a temp file
                fileName = Path.GetTempFileName();
                FileInfo fileInfo = new FileInfo(fileName)
                {
                    Attributes = FileAttributes.Temporary
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create TEMP file or set its attributes: " + ex.Message);
            }

            // return the name of the created file.
            return fileName;
        }
    }
}