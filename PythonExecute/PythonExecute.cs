using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RevitReactionImporter
{
    public class PythonExecute
    {
        public static void Main()
        {
            //string[] args
            string python = @"C:\Program Files\Anaconda3\python.exe";
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(python);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Program Files\Anaconda3\python.exe";
            //startInfo.Arguments = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer\hello.py";
            startInfo.Arguments = "hello.py";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            Console.WriteLine(output);

            Console.ReadLine();
            process.Close();
        }
    }
}
