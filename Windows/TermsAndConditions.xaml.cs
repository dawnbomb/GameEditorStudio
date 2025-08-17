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
        }

        private void ButtonIAgreeToToS(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        
    }
}
