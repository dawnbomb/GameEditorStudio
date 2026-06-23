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
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        WorkshopData WorkshopData { get; set; }

        public TextEditor(WorkshopData Database, TextEditorData TextEditorData)
        {
            InitializeComponent();

            //This
            WorkshopData = Database;
            
            //My Data
            TextEditorData.TextEditorXaml = this;
            TextEditorData.EditorVisual = this;
            TextEditorData.TextFileManager = TextFileManager;
            TextEditorData.MainGrid = MainGrid;

            //File Manager
            TextFileManager.IsTextEditor = true;
            TextFileManager.TextEditorData = TextEditorData;
            TextFileManager.WorkshopXaml = Database.WorkshopXaml;
            TextFileManager.TreeGameFiles.SelectedItemChanged += TESTTHING;
                        
            //Tab
            TabButtonMaker MakeEditorButton = new();
            MakeEditorButton.CreateEditorTab(TextEditorData);
            MakeEditorButton.UpdateEditorRightClickMenu(TextEditorData);

            //Finally, we appear!
            Database.WorkshopXaml.MidGrid.Children.Add(this);

            GenerateUI();
        }

        public void GenerateUI() 
        {
            TextFileManager.RefreshFileTree();

            if (WorkshopData.IsProjectLoaded == true)
            {
                TheTextBox.IsEnabled = true;
                TheLineBox.IsEnabled = true;
                TextFileManager.IsEnabled = true;
            }
            if (WorkshopData.IsProjectLoaded == false) 
            {
                TheTextBox.IsEnabled = false;
                TheLineBox.IsEnabled = false;
                TextFileManager.IsEnabled = false;
            }
            
        }

        private void TESTTHING(object sender, RoutedEventArgs e)
        {
            TreeViewItem Item = TextFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { TheTextBox.Text = ""; TheLineBox.Clear(); return; }
            GameFile GameFile = Item.Tag as GameFile;
            if (GameFile == null) { TheTextBox.Text = ""; TheLineBox.Clear(); return; }

            //TheTextBox.Text = GameFile.FileBytes;
            string fullText = Encoding.UTF8.GetString(GameFile.FileBytes);
            string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            TheTextBox.Text = fullText; // Clear it first if needed
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewItem Item = TextFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            GameFile GameFile = Item.Tag as GameFile;
            if (GameFile == null) { return; }

            TheLineBox.Clear();
            string[] lines = TheTextBox.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                TheLineBox.AppendText((i + 1).ToString() + Environment.NewLine);

            }

            GameFile.FileBytes = Encoding.UTF8.GetBytes(TheTextBox.Text);
        }

        private void ToggleLineID(object sender, RoutedEventArgs e)
        {
            if (TheLineBox.Visibility == Visibility.Visible) 
            {
                TheLineBox.Visibility = Visibility.Collapsed;
                LineIDToggle.Foreground = Brushes.Gray;
            }
            else
            {
                TheLineBox.Visibility = Visibility.Visible;
                LineIDToggle.Foreground = Brushes.White;
            }
        }
    }
}

