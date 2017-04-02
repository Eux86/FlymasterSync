using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlyMasterSyncGui.Forms;
using UpdateChecker;

namespace FlyMasterSyncGui
{
    class UpdateManager
    {
        public UpdateManager()
        {
            CheckAndDownload();
        }

        private async void CheckAndDownload()
        {
            Updater updater = new Updater(new Uri("http://flymastersync.altervista.org/version.info"));
            if (await updater.NeedUpdateAsync(Assembly.GetExecutingAssembly().GetName().Version))
            {
                var result = MessageBox.Show(
                    "A new update is available!\nVersion: " + updater.OnlineVersion + "\nDo you want to install it?",
                    "Update available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    UpdateInfo ui = new UpdateInfo();
                    ui.Show();
                    await updater.DownloadNewVersionAsync();
                    updater.InstallUpdate();
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
