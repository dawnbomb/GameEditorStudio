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
    public partial class TermsAndConditions : Window
    {
        public TermsAndConditions()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            

            this.Topmost = true;    // Temporarily set topmost to ensure visibility
            this.Activate();        // Try to bring to foreground
            this.Focus();           // Set keyboard focus
            this.Topmost = false;   // Reset topmost if undesired permanently
        }

        private void ButtonIAgreeToToS(object sender, RoutedEventArgs e)
        {
            OpenCrystalEditor();
        }

        public void OpenCrystalEditor()
        {
            GameLibrary GameLibrary = new GameLibrary();
            GameLibrary.Show();

            this.Close();

            //foreach (TreeViewItem item in GameLibrary.LibraryTreeOfWorkshops.Items) //Select the last workshop used, if it exists.
            //{                
            //    if (item.Header as string == Properties.Settings.Default.LastWorkshop)
            //    {
            //        item.IsSelected = true;
            //    }
            //}
        }
    }
}
