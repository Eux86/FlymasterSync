using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace UpdateChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            //Updater updater = new Updater(new Uri("http://flymastersync.altervista.org/version.info"));
            //Console.WriteLine("Checking..");
            //if (updater.NeedUpdate())
            //{
                
            //}

            string uninstallString = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{2558E6B9-69D2-45F1-9C39-2D5098C458B2}", "UninstallString", "");
            uninstallString = uninstallString.Replace("/I", "/x")+" /quiet";
            Console.WriteLine(uninstallString);

            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + uninstallString);
             // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            // Get the output into a string
            string result = proc.StandardOutput.ReadToEnd();
            // Display the command output.
            Console.WriteLine(result);
            Console.WriteLine("Uninstalled");
            Console.ReadLine();
        }
    }
}
