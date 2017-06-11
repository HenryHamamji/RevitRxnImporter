using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace RevitReactionImporter
{
    public class RAMModel
    {
        public static void ExecutePythonScript()
        {
            //string python = @"C:\Program Files\Anaconda3\python.exe";
            Process process = new Process();
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            // startInfo.FileName = @"C:\Program Files\Anaconda3\python.exe";
            //startInfo.FileName = @"C:\Users\Owner\Anaconda3\envs\firstEnv\python.exe";
            // startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = @" python C:\dev\RAM Reaction Importer\RAM-Reaction-Importer\hello.py";


           // startInfo.Arguments = "python hello.py";
            //startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            process.StartInfo = startInfo;
            process.Start();
            process.StandardInput.WriteLine(@"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer");
            process.StandardInput.WriteLine("python getLevels.py");

            // Read the standard output of the app we called.  
            // in order to avoid deadlock we will read output first 
            // and then wait for process terminate: 
            StreamReader myStreamReader = process.StandardOutput;
            string myString = myStreamReader.ReadLine();

            /*if you need to read multiple lines, you might use: 
                string myString = myStreamReader.ReadToEnd() */

            // wait exit signal from the app we called and then close it. 
            //process.WaitForExit();
            process.Close();

            // write the output we got from python app 
            Console.WriteLine("Value received from script: " + myString);            
        }


    }
}
