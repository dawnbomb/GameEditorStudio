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
using static System.Net.Mime.MediaTypeNames;

namespace GameEditorStudio
{
   
    public partial class SettingsColors : UserControl
    {
       

        public SettingsColors()
        {
            InitializeComponent();

            // 2 ways to get red
            // = (Color)ColorConverter.ConvertFromString("#FF0000"); 
            // = Color.FromRgb(255, 0, 0); 

                        
            //Color ColorAlwaysMoreX = ;   

            PopulateColors(ComboBoxAlways0);
            PopulateColors(ComboBoxAlwaysSame);
            PopulateColors(ComboBoxLessThanX);
            PopulateColors(ComboBoxDisabled);
            PopulateColors(ComboBoxText);
            //PopulateColors(ComboBoxAlways0);


        }

        public void PopulateColors(ComboBox comboBox)
        {
            comboBox.Items.Add("⬤ Red");
            comboBox.Items.Add("⬤ Blue");
            comboBox.Items.Add("⬤ Green");
            comboBox.Items.Add("⬤ Pink");
            comboBox.Items.Add("⬤ Gray");


            //< ComboBoxItem >
            //< TextBlock Text = "NES: MESEN" Foreground = "Lime" />
            //</ ComboBoxItem >




        }

        public void ChangeColorB(ComboBox comboBox) 
        {
            
        }

        private void ChangeColor(object sender, EventArgs e)
        {
            if (ComboBoxName.SelectedValue.ToString() == "⬤ Blue") { LibraryMan.ValueName = LibraryMan.ColorBlue; }
        }

        
    }

}
