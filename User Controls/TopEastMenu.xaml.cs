using System;
using System.Collections.Generic;
using System.Linq;
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

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TopEastMenu.xaml
    /// </summary>
    public partial class TopEastMenu : UserControl
    {
        public TopEastMenu()
        {
            InitializeComponent();

            #if DEBUG

            #else
            HUDReshade.Visibility = Visibility.Collapsed; 
            #endif
        }

        private void OpenReshade(object sender, RoutedEventArgs e)
        {
            Reshade f2 = new();
            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            f2.Show();
        }

        private void OpenWiki(object sender, RoutedEventArgs e)
        {
            #if DEBUG
            Tutorial f2 = new Tutorial();
            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            f2.Show();                                 
            #endif
            
        }

        private void OpenDiscord(object sender, RoutedEventArgs e)
        {
            string url = "https://discord.gg/mhrZqjRyKx";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        private void WebsiteButton(object sender, RoutedEventArgs e)
        {
            string url = "https://www.crystalmods.com/";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        
        }

        private void GithubButton(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/dawnbomb/GameEditorStudio";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        
    }
}
