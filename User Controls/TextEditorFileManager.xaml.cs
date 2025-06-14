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
    /// Interaction logic for TextEditorFileManager.xaml
    /// </summary>
    public partial class TextEditorFileManager : UserControl
    {
        Editor TheEditor { get; set; }

        public TextEditorFileManager(Editor AEditor)
        {
            InitializeComponent();

            TheEditor = AEditor;

            TextFileManager.IsTextEditor = true;
            TextFileManager.ThisEditor = AEditor;

            TextFileManager.AddFileButton.IsEnabled = false;
            TextFileManager.AddFileButton.Visibility = Visibility.Collapsed;

            //WaitAMoment();            

            AllFileManager.Loaded += TESTTHING;
        }

        private void TESTTHING(object sender, RoutedEventArgs e)
        {
            List<TreeViewItem> items = new();

            foreach (TreeViewItem Item in AllFileManager.TreeGameFiles.Items)
            {
                GameFile GameFile = Item.Tag as GameFile;
                if (TheEditor.TextEditorData.ListOfGameFiles.Contains(GameFile))
                {
                    items.Add(Item);
                }
            }

            foreach (TreeViewItem Item in items)
            {                
                AllFileManager.TreeGameFiles.Items.Remove(Item);
            }
        }



        private async Task WaitAMoment() 
        {
            await Task.Delay(1);
        }

        private void ButtonClose(object sender, RoutedEventArgs e)
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
                TheEditor.TextEditorData.MainGrid.Visibility = Visibility.Visible;
            }

            TheEditor.TextEditorData.TextFileManager.RefreshFileTree();
        }

        private void ButtonAddFile(object sender, RoutedEventArgs e)
        {            
            TreeViewItem Item = AllFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            GameFile GameFile = Item.Tag as GameFile;
            if (GameFile == null) { return; }

            TheEditor.TextEditorData.ListOfGameFiles.Add(GameFile);
            AllFileManager.TreeGameFiles.Items.Remove(Item);
            TextFileManager.TreeGameFiles.Items.Add(Item);
        }

        private void ButtonRemoveFile(object sender, RoutedEventArgs e)
        {
            TreeViewItem Item = TextFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            GameFile GameFile = Item.Tag as GameFile;
            if (GameFile == null) { return; }

            TheEditor.TextEditorData.ListOfGameFiles.Remove(GameFile);
            TextFileManager.TreeGameFiles.Items.Remove(Item);
            AllFileManager.TreeGameFiles.Items.Add(Item);

        }
    }
}
