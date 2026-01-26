using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TermsAndConditions.xaml
    /// </summary>
    public partial class TermsAndConditions : UserControl
    {
        public TermsAndConditions()
        {
            InitializeComponent();
            VNum.Content = "v" + Properties.Settings.Default.ToSVersion;

            if (Properties.Settings.Default.ToSVersion == LibraryGES.VersionNumber.ToString()) 
            {
                CloseToS();
            }
        }

        private void ButtonIAgreeToToS(object sender, RoutedEventArgs e)
        {   
            Properties.Settings.Default.ToSVersion = LibraryGES.VersionNumber.ToString();
            Properties.Settings.Default.Save();

            CloseToS();
        }

        private void CloseToS() 
        {
            this.Visibility = Visibility.Collapsed;
        }
        
    }
}
