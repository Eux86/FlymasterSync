using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace UpdateChecker
{
    public class Updater
    {
        private const string UpdateTempDirectoryPath = "Update";


        private readonly Uri _updateFileUrl;

        private Version _onlineVersion;
        private Uri _updateDownloadUri;

        public Updater(Uri updateFileUrl)
        {
            _updateFileUrl = updateFileUrl;

            if (!Directory.Exists(UpdateTempDirectoryPath))
                Directory.CreateDirectory(UpdateTempDirectoryPath);
            foreach (var file in Directory.GetFiles(UpdateTempDirectoryPath))
            {
                File.Delete(file);
            }
        }

        public Uri UpdateDownloadUri
        {
            get { return _updateDownloadUri; }
            private set { _updateDownloadUri = value; }
        }

        public Version OnlineVersion
        {
            get { return _onlineVersion; }
            private set { _onlineVersion = value; }
        }

        public async Task<bool> NeedUpdateAsync(Version currentVersion)
        {
            return await Task.Run(() => NeedUpdate(currentVersion));
        }

        public bool NeedUpdate(Version currentVersion)
        {
            try
            {
                var webRequest = WebRequest.Create(_updateFileUrl);
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    try
                    {
                        _onlineVersion = new Version(reader.ReadLine());
                        _updateDownloadUri = new Uri(reader.ReadLine());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Something went wrong trying to download information about a new version",
                            ex);
                    }
                }
                
                return _onlineVersion.CompareTo(currentVersion) > 0;
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            
        }

        public async Task<string> DownloadNewVersionAsync()
        {
            return await Task.Run(() => DownloadNewVersion());
        }

        public string DownloadNewVersion()
        {
            WebClient myWebClient = new WebClient();
            string updateFilePath = UpdateTempDirectoryPath +"\\"+ Path.GetFileName(_updateDownloadUri.LocalPath);
            myWebClient.DownloadFile(UpdateDownloadUri,updateFilePath);
            return updateFilePath;
        }

        public void InstallUpdate()
        {
            string uninstallString = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{2558E6B9-69D2-45F1-9C39-2D5098C458B2}", "UninstallString", "");
            if (uninstallString != null)
            {
                uninstallString = uninstallString.Replace("/I", "/x") + " /quiet";
            }

            string installationPath = Assembly.GetEntryAssembly().Location;
            installationPath = Path.GetDirectoryName(installationPath);

            string reinstallerPath = UpdateTempDirectoryPath + "\\" + "reinstaller.bat";
            using (StreamWriter writer = new StreamWriter(reinstallerPath))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine("cd " + UpdateTempDirectoryPath);
                writer.WriteLine(uninstallString);
                writer.WriteLine("msiexec /i %1 TARGETDIR=\""+installationPath+"\" /qb");
                writer.WriteLine("cd ..");
                writer.WriteLine("start FlymasterSyncGui.exe");
                writer.WriteLine("exit");
            }
            Process.Start(Path.GetFullPath(reinstallerPath), Path.GetFileName(_updateDownloadUri.LocalPath));
        }

        private void Uninstall()
        {
            string uninstallString = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{2558E6B9-69D2-45F1-9C39-2D5098C458B2}", "UninstallString", "");
            uninstallString = uninstallString.Replace("/I", "/x") + " /quiet";
            Console.WriteLine(uninstallString);

            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + uninstallString);
            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            // Get the output into a string
            string result = proc.StandardOutput.ReadToEnd();
            // Display the command output.
            Console.WriteLine(result);
        }

    }
}
