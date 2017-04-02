using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlyMasterSyncGui.Forms
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {

        private static About _instance;
        private static bool _visible;

        public new static About Show()
        {
            if (_instance == null) _instance = new About();
            try
            {
                ((Window)_instance).Show();
            }
            catch (InvalidOperationException ex)
            {
                _instance = new About();
                ((Window)_instance).Show();
            }

            _visible = true;
            return _instance;
        }

        public new static About Hide()
        {
            if (_instance != null) ((Window)_instance).Hide();
            _visible = false;
            return _instance;
        }

        public About()
        {
            InitializeComponent();
            VersionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
