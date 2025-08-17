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
        public TextEditor(WorkshopData Database, Editor Editor)
        {
            InitializeComponent();
                        
            Editor.EditorBackPanel = BackPanel;
            Database.WorkshopXaml.MidGrid.Children.Add(this);

            TextFileManager.IsTextEditor = true;
            TextFileManager.ThisEditor = Editor;
            Editor.TextEditorData.TextFileManager = TextFileManager;

            MakeButton MyButton = new();
            MyButton.CreateButton(Database.WorkshopXaml, Database, Editor);

            Editor.TextEditorData.MainGrid = MainGrid;


            TextFileManager.TreeGameFiles.SelectedItemChanged += TESTTHING;
        }

        private void TESTTHING(object sender, RoutedEventArgs e)
        {
            TreeViewItem Item = TextFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { TheTextBox.Text = "";  return; }
            GameFile GameFile = Item.Tag as GameFile;
            if (GameFile == null) { TheTextBox.Text = ""; return; }

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




            //TheTextBox.Text;
        }

        private void ToggleLineID(object sender, RoutedEventArgs e)
        {
            if (TheLineBox.Visibility == Visibility.Visible) 
            {
                TheLineBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                TheLineBox.Visibility = Visibility.Visible;
            }
        }
    }
}

