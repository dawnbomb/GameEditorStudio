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
using static OfficeOpenXml.ExcelErrorValue;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for GES.xaml
    /// </summary>
    public partial class GES : Window
    {
        public GES()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Title = "Game Editor Studio     Version: " + LibraryGES.VersionNumber + "   ( " + LibraryGES.VersionDate + " )";
            Database.GESMain = this;

            #if DEBUG
            LibraryGES.DebugMode = true;
            //Now we set where the .exe is supposed to be. For use when in visual studio.
            LibraryGES.ApplicationLocation = "D:\\Game Editor Studio";
            //"O:\\Teddy\\Game Editor Studio\\Game Editor Studio"; 
            //LibraryGES.ApplicationLocation = "O:\\Teddy\\Work\\Game Editor Studio";
            #else
            LibraryGES.ApplicationLocation = AppDomain.CurrentDomain.BaseDirectory;
            #endif

            ReadRecentWorkshopNames(); //Has to be before Game Library is added.

            GameLibrary Library = new();
            GESGrid.Children.Add(Library);


            TermsAndConditions tos = new();
            GESGrid.Children.Add(tos);
            Grid.SetRowSpan(tos, 15);
            Grid.SetColumnSpan(tos, 15);



            if (Properties.Settings.Default.FullscreenGESOnLaunch == true)
            {
                this.WindowState = WindowState.Maximized;
            }

            

        }

        private void ReadRecentWorkshopNames() 
        {
            LibraryGES.RecentWorkshops.Clear();
            string RecentWorkshopNames = Properties.Settings.Default.RecentWorkshops;
            if (!string.IsNullOrEmpty(RecentWorkshopNames))
            {
                string[] workshops = RecentWorkshopNames.Split('|');

                foreach (string workshop in workshops)
                {
                    if (!string.IsNullOrWhiteSpace(workshop))
                    {
                        LibraryGES.RecentWorkshops.Add(workshop);
                    }
                }
            }
        }
    }
}
