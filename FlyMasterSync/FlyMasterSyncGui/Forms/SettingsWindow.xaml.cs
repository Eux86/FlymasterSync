using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlyMasterSyncGui.Annotations;

namespace FlyMasterSyncGui.Forms
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private string _tracksFolder;
        public string TracksFolder
        {
            get { return _tracksFolder; }
            set
            {
                _tracksFolder = value;
                OnPropertyChanged("TracksFolder");
            }
        }

        private string _doaramaKey;
        public string DoaramaKey
        {
            get { return _doaramaKey; }
            set
            {
                _doaramaKey = value; 
                OnPropertyChanged("DoaramaKey");
            }
        }




        private static SettingsWindow _instance;
        private static bool _visible;

        
        public new static SettingsWindow Show()
        {
            if (_instance == null) _instance = new SettingsWindow();
            try
            {
                ((Window)_instance).Show();
            }
            catch (InvalidOperationException ex)
            {
                _instance = new SettingsWindow();
                ((Window)_instance).Show();
            }

            _visible = true;
            return _instance;
        }

        public new static SettingsWindow Hide()
        {
            if (_instance != null) ((Window)_instance).Hide();
            _visible = false;
            return _instance;
        }

        public SettingsWindow()
        {
            InitializeComponent();
            DoaramaKey = Properties.Settings.Default.UserDoaramaApiKey;
            TracksFolder = System.IO.Path.GetFullPath(Properties.Settings.Default.TracksFolder);
        }


        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UserDoaramaApiKey = DoaramaKey;
            //Properties.Settings.Default.TracksFolder = TracksFolder;
            Properties.Settings.Default.Save();
            this.Close();
        }
        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
