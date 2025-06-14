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
using System.Windows.Shapes;

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

            Dispatcher.InvokeAsync(async () => await GithubUpdater.CheckForUpdatesAsync());
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
