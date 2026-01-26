using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IWshRuntimeLibrary;
using static System.Windows.Forms.LinkLabel;
using File = System.IO.File;

namespace GameEditorStudio
{
    
    public class WikiDataBase
    {
        public List<WikiFolder> Folders { get; set; } = new();
    }


    public class WikiFolder
    {
        public string Name { get; set; } = "New Category";
        public List<WikiDocument> Documents { get; set; } = new();
    }

    public class WikiDocument
    {
        public string Name { get; set; } = "New Document";
        public string Text { get; set; } = "";
        public List<WikiImage> ImagesList { get; set; } = new();
    }
    public class WikiImage
    {
        public string FileName { get; set; } = "";
        public BitmapImage Bitmap { get; set; } = new();
    }

    public partial class Tutorial : Window
    {     



        public Tutorial()
        {            
            InitializeComponent();

            CategoryNameBox.Visibility = Visibility.Collapsed;
            DocumentNameBox.Visibility = Visibility.Collapsed;
            ButtonNewCategory.Visibility = Visibility.Collapsed;
            ButtonNewDocument.Visibility = Visibility.Collapsed;
            SaveWikiButton.Visibility = Visibility.Collapsed;
            DocumentEditBox.Visibility = Visibility.Collapsed;

            foreach (WikiFolder folder in LibraryGES.Wiki.Folders) 
            {
                GenerateCategory(folder);
            }
        }

        private void GenerateCategory(WikiFolder Category)
        {
            TreeViewItem Categoryitem = new();
            Categoryitem.Header = Category.Name;
            Categoryitem.Tag = Category;
            TreeOfCategorys.Items.Add(Categoryitem);

            ContextMenu contextMenu = new();
            Categoryitem.ContextMenu = contextMenu;
            MenuItem favoriteMenuitem = new();
            favoriteMenuitem.Header = "Favorite";
            contextMenu.Items.Add(favoriteMenuitem);
            MenuItem deleteMenuitem = new();
            deleteMenuitem.Header = "Delete";
            contextMenu.Items.Add(deleteMenuitem);
        }
        private void GenerateDocument(WikiDocument Document)
        {            

            TreeViewItem Documentitem = new();            
            Documentitem.Header = Document.Name;
            Documentitem.Tag = Document;
            TreeOfDocuments.Items.Add(Documentitem);
            Documentitem.IsSelected = true;


            ContextMenu contextMenu = new();
            Documentitem.ContextMenu = contextMenu;


            MenuItem favoriteMenuitem = new();
            favoriteMenuitem.Header = "Favorite";
            contextMenu.Items.Add(favoriteMenuitem);

            MenuItem deleteMenuitem = new();
            deleteMenuitem.Header = "Delete";
            contextMenu.Items.Add(deleteMenuitem);


        }

        private void NewWikiCategory(object sender, RoutedEventArgs e)
        {
            WikiFolder Category = new();
            LibraryGES.Wiki.Folders.Add(Category);
            GenerateCategory(Category);
        }

        private void NewWikiTopic(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = TreeOfCategorys.SelectedItem as TreeViewItem;
            if (item != null)
            {
                WikiFolder category = item.Tag as WikiFolder;

                WikiDocument Document = new();
                category.Documents.Add(Document);

                {
                    string baseName = "New Document";
                    string newName = baseName;
                    int counter = 1;

                    // Keep checking until we find a unique name
                    while (TreeOfDocuments.Items.Cast<TreeViewItem>()
                               .Any(item => (item.Header?.ToString() ?? "") == newName))
                    {
                        counter++;
                        newName = $"{baseName} {counter}";
                    }
                    Document.Name = newName;
                }
                

                GenerateDocument(Document);
            }

            
        }

        private void ItemNameBuilder(TreeViewItem item) 
        {
            
        }

        

        private void CategoryNameBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            TreeViewItem item = TreeOfCategorys.SelectedItem as TreeViewItem;
            if (item != null) 
            {
                WikiFolder category = item.Tag as WikiFolder;
                category.Name = CategoryNameBox.Text;
                item.Header = "📁 " + category.Name;
            }
        }

        private void DocumentNameBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) 
            {
                return;
            }

            TreeViewItem item = TreeOfDocuments.SelectedItem as TreeViewItem;
            if (item != null)
            {
                WikiDocument document = item.Tag as WikiDocument;
                document.Name = DocumentNameBox.Text;                
                item.Header = "🗎 " + document.Name;
                DocumentLabel.Content = document.Name;
            }
        }

        private void CategoryChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeOfDocuments.Items.Clear();

            TreeViewItem item = TreeOfCategorys.SelectedItem as TreeViewItem;
            if (item == null)
            {
                return;
            }

            WikiFolder category = item.Tag as WikiFolder;
            CategoryNameBox.Text = category.Name;
            

            foreach (WikiDocument Document in category.Documents) 
            {
                GenerateDocument(Document);
            }

            if (TreeOfDocuments.Items.Count > 0)
            {
                TreeViewItem DItem = TreeOfDocuments.Items[0] as TreeViewItem;
                DItem.IsSelected = true;
            }
        }

        private void DocumentChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = TreeOfDocuments.SelectedItem as TreeViewItem;
            if (item == null)
            {
                DocumentNameBox.Text = "";
                DocumentLabel.Content = "";
                return;
            }

            WikiDocument document = item.Tag as WikiDocument;
            DocumentNameBox.Text = document.Name;
            DocumentLabel.Content = document.Name;
            DocumentEditBox.Text = document.Text;

            DocumentReadBox.Text = DocumentEditBox.Text;

            PatchnotePanel.Children.Clear();

            StackPanel thepanel = PatchnotePanel;
            string[] lines = document.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Dictionary<string, BitmapImage> images = new();
            foreach (WikiImage wimage in document.ImagesList) { images.Add(wimage.FileName, wimage.Bitmap); }
            
            
            PixelWPF.LibraryText.TextToStackPanel(document.Text, thepanel, images); 

        }
        

        private void TextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewItem item = TreeOfDocuments.SelectedItem as TreeViewItem;
            if (item == null)
            {
                return;
            }

            WikiDocument document = item.Tag as WikiDocument;
            document.Text = DocumentEditBox.Text;
        }

        private void ButtonEditModeClick(object sender, RoutedEventArgs e)
        {
            // Create a simple password dialog
            Window pwDialog = new Window
            {
                Title = "Enter Password",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this, // set owner so it’s modal to your main window
            };

            PasswordBox passwordBox = new PasswordBox
            {
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center
            };

            Button okButton = new Button
            {
                Content = "OK",
                Width = 70,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            StackPanel panel = new StackPanel();
            panel.Children.Add(passwordBox);
            panel.Children.Add(okButton);
            pwDialog.Content = panel;

            bool? result = null;
            okButton.Click += (s, ev) =>
            {
                if (passwordBox.Password == "123")
                {
                    result = true;
                    pwDialog.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            pwDialog.ShowDialog();

            // If password was correct, continue
            if (result == true)
            {
                ButtonEditMode.Visibility = Visibility.Collapsed;
                ButtonWriteMode.Visibility = Visibility.Visible;

                CategoryNameBox.Visibility = Visibility.Visible;
                DocumentNameBox.Visibility = Visibility.Visible;
                ButtonNewCategory.Visibility = Visibility.Visible;
                ButtonNewDocument.Visibility = Visibility.Visible;
                SaveWikiButton.Visibility = Visibility.Visible;
            }
        }
        private void ButtonWriteModeClick(object sender, RoutedEventArgs e)
        {
            DocumentEditBox.Visibility = Visibility.Visible;
            PatchnotePanel.Visibility = Visibility.Collapsed;

            ButtonWriteMode.Visibility = Visibility.Collapsed;
            ButtonReadMode.Visibility = Visibility.Visible;            
        }
        private void ButtonReadModeClick(object sender, RoutedEventArgs e)
        {
            DocumentEditBox.Visibility = Visibility.Collapsed;
            PatchnotePanel.Visibility = Visibility.Visible;

            ButtonReadMode.Visibility = Visibility.Collapsed;
            ButtonWriteMode.Visibility = Visibility.Visible;

            TreeViewItem item = TreeOfDocuments.SelectedItem as TreeViewItem;
            if (item == null)
            {
                return;
            }
            item.IsSelected = false;
            item.IsSelected = true;
        }

        private void ButtonSaveWiki(object sender, RoutedEventArgs e)
        {            
            try 
            {        
                //First we do a test save to make sure everything is okay. If it works, were delete it, then delete the real wiki, and save over it. 
                SaveWiki(LibraryGES.ApplicationLocation + "\\Other\\FakeWiki"); //Test Save
                if (Directory.Exists(LibraryGES.ApplicationLocation + "\\Other\\FakeWiki"))  { Directory.Delete(LibraryGES.ApplicationLocation + "\\Other\\FakeWiki", true); } // Delete test save wiki
                if (Directory.Exists(LibraryGES.ApplicationLocation + "\\Other\\Wiki"))  { Directory.Delete(LibraryGES.ApplicationLocation + "\\Other\\Wiki", true); } //Delete real wiki
                SaveWiki(LibraryGES.ApplicationLocation + "\\Other"); //Real Save
            } 
            catch 
            {
                PixelWPF.LibraryPixel.NotificationNegative("Wiki Save Error","");
                return;
            }
            

            void SaveWiki(string WikiPath) 
            {
                Directory.CreateDirectory(WikiPath + "\\Wiki");

                foreach (WikiFolder folder in LibraryGES.Wiki.Folders)
                {
                    string folderPath = WikiPath + "\\Wiki\\" + folder.Name;
                    Directory.CreateDirectory(folderPath);

                    foreach (WikiDocument document in folder.Documents)
                    {
                        string docPath = folderPath + "\\" + document.Name;
                        Directory.CreateDirectory(docPath);

                        File.WriteAllText(docPath + "\\" + document.Name + ".txt", document.Text);

                        foreach (WikiImage img in document.ImagesList)
                        {
                            using FileStream stream = new(docPath + "\\" + img.FileName, FileMode.Create, FileAccess.Write);
                            PngBitmapEncoder encoder = new();
                            encoder.Frames.Add(BitmapFrame.Create(img.Bitmap));
                            encoder.Save(stream);
                        }
                    }
                }
            }
            

            
        }
    }
}
