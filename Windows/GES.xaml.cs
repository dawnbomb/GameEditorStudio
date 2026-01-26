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


            GameLibrary Library = new();
            GESGrid.Children.Add(Library);


            TermsAndConditions tos = new();
            GESGrid.Children.Add(tos);
            Grid.SetRowSpan(tos, 15);
            Grid.SetColumnSpan(tos, 15);
        }
    }
}
