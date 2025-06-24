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
        public Notification(string Icon, string Title, string MainText)
        {
            InitializeComponent();

            SymbolLabel.Content = Icon;
            if (Icon == "✔") { SymbolLabel.Foreground = Brushes.Green; }
            if (Icon == "X") { SymbolLabel.Foreground = Brushes.Red; }
            if (Icon == "X")  
            {
                System.Media.SystemSounds.Hand.Play();
                //var player = new System.Media.SoundPlayer("Resources/ErrorSound.wav");
                //player.Play();
            }
            TitleLabel.Content = Title;
            MessageBox.Text = MainText;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
        

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }



       
        /////////////////////////// EXAMPLE /////////////////////////////////////
       
        //public void Example()
        //{
        //    Notification Notification = new("This is an example error message." +
        //                "\n" +
        //                "\nList of possible causes..." +
        //                "\n1: You." +
        //                "\n2: Me." +
        //                "\n3: Society." +
        //                "\n" +
        //                "\nNote: :D");
        //                //Environment.FailFast(null); //Kills program instantly. 
        //    return;
            
        //}

    }
}
