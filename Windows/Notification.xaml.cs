using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {
        public Notification(string Error)
        {
            InitializeComponent();

            MessageBox.Text = Error;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.ShowDialog();
        }
        

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



       
        /////////////////////////// EXAMPLE /////////////////////////////////////
       
        public void Example()
        {
            Notification Notification = new("This is an example error message." +
                        "\n" +
                        "\nList of possible causes..." +
                        "\n1: You." +
                        "\n2: Me." +
                        "\n3: Society." +
                        "\n" +
                        "\nNote: :D");
                        //Environment.FailFast(null); //Kills program instantly. 
            return;
            
        }

    }
}
