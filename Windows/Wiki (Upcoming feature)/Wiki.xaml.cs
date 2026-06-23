using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
using OfficeOpenXml.Style;
using static System.Windows.Forms.LinkLabel;
using File = System.IO.File;
using Path = System.IO.Path;

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
        bool EditMode { get; set; } = false;


        public Tutorial()
        {            
            InitializeComponent();

            //Category
            ButtonNewCategory.Visibility = Visibility.Collapsed;
            ButtonDelCategory.Visibility = Visibility.Collapsed;
            CategoryEditGrid.Visibility = Visibility.Collapsed;
            CategoryNameBox.Visibility = Visibility.Collapsed;
            ButtonMovCatUp.Visibility = Visibility.Collapsed;
            ButtonMovCatDown.Visibility = Visibility.Collapsed;


            //Document
            DocumentNameBox.Visibility = Visibility.Collapsed;
            ButtonDelDocument.Visibility = Visibility.Collapsed;
            ButtonNewDocument.Visibility = Visibility.Collapsed;                        
            DocumentEditGrid.Visibility = Visibility.Collapsed;
            ButtonMovDocUp.Visibility = Visibility.Collapsed;
            ButtonMovDocDown.Visibility = Visibility.Collapsed;

            //Right editors bar
            EditorView.Visibility = Visibility.Collapsed;
            SaveWikiButton.Visibility = Visibility.Collapsed;
            DocumentEditBox.IsEnabled = false;
            EditColumn.Width = new GridLength(0);

            //Load Wiki IF not yet loaded.  (Loading was moved here from GameLibrary to reduce startup loading time.)
            if (LibraryGES.Wiki.Folders.Count == 0) 
            {
                LoadWiki(); //Origonally i made this, but when adding LoadOrder support, now it's vibe coaded because im lazy.
            }
            
        }

        private void LoadWiki()
        {
            string wikiBase = Path.Combine(LibraryGES.ApplicationLocation, "Other", "Wiki");
            if (!Directory.Exists(wikiBase)) return;

            // --- 1. Load Categories ---
            HashSet<string> loadedCategories = new();
            string catOrderPath = Path.Combine(wikiBase, "LoadOrder.txt");

            // Load from file first
            if (File.Exists(catOrderPath))
            {
                foreach (string line in File.ReadAllLines(catOrderPath).Where(l => !string.IsNullOrWhiteSpace(l)))
                {
                    if (LoadFolderIntoWiki(Path.Combine(wikiBase, line)))
                        loadedCategories.Add(line.ToLower());
                }
            }

            // Load remaining discovered directories
            foreach (string dir in Directory.GetDirectories(wikiBase))
            {
                string name = Path.GetFileName(dir);
                if (!loadedCategories.Contains(name.ToLower()))
                    LoadFolderIntoWiki(dir);
            }

            // Finally, build the UI
            foreach (WikiFolder folder in LibraryGES.Wiki.Folders)
            {
                GenerateCategory(folder);
            }

            bool LoadFolderIntoWiki(string categoryPath)
            {
                if (!Directory.Exists(categoryPath)) return false;

                WikiFolder wikiFolder = new() { Name = Path.GetFileName(categoryPath) };
                LibraryGES.Wiki.Folders.Add(wikiFolder);

                HashSet<string> loadedDocs = new();
                string docOrderPath = Path.Combine(categoryPath, "LoadOrder.txt");

                // Load Documents from file
                if (File.Exists(docOrderPath))
                {
                    foreach (string line in File.ReadAllLines(docOrderPath).Where(l => !string.IsNullOrWhiteSpace(l)))
                    {
                        if (LoadDocumentIntoFolder(wikiFolder, Path.Combine(categoryPath, line)))
                            loadedDocs.Add(line.ToLower());
                    }
                }

                // Load remaining discovered document folders
                foreach (string dir in Directory.GetDirectories(categoryPath))
                {
                    string name = Path.GetFileName(dir);
                    if (!loadedDocs.Contains(name.ToLower()))
                        LoadDocumentIntoFolder(wikiFolder, dir);
                }

                return true;
            }

            bool LoadDocumentIntoFolder(WikiFolder folder, string docPath)
            {
                if (!Directory.Exists(docPath)) return false;

                string docName = Path.GetFileName(docPath);
                WikiDocument document = new() { Name = docName };
                folder.Documents.Add(document);

                // Load Text (Looking for docName.txt or any .txt)
                string specificTxt = Path.Combine(docPath, docName + ".txt");
                if (File.Exists(specificTxt))
                    document.Text = File.ReadAllText(specificTxt);
                else
                {
                    string? anyTxt = Directory.GetFiles(docPath, "*.txt").FirstOrDefault();
                    if (anyTxt != null) document.Text = File.ReadAllText(anyTxt);
                }

                // Load Images
                foreach (string imgFile in Directory.GetFiles(docPath).Where(f => f.EndsWith(".png") || f.EndsWith(".jpg")))
                {
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imgFile, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    document.ImagesList.Add(new WikiImage { FileName = Path.GetFileName(imgFile), Bitmap = bitmap });
                }

                return true;
            }
        }

        

        

        //private void LoadWiki() 
        //{
        //    string CategoryLoadOrderPath = LibraryGES.ApplicationLocation + "\\Other\\Wiki\\LoadOrder.txt";
        //    if (File.Exists(CategoryLoadOrderPath))
        //    {
        //        string[] CategoryLoadOrder = File.ReadAllLines(CategoryLoadOrderPath);
        //        foreach (string categoryName in CategoryLoadOrder)
        //        {
        //            WikiFolder folder = new() { Name = categoryName };
        //            LibraryGES.Wiki.Folders.Add(folder);
        //            string DocumentLoadOrderPath = LibraryGES.ApplicationLocation + "\\Other\\Wiki\\" + categoryName + "\\LoadOrder.txt";
        //            if (File.Exists(DocumentLoadOrderPath))
        //            {
        //                string[] DocumentLoadOrder = File.ReadAllLines(DocumentLoadOrderPath);
        //                foreach (string documentName in DocumentLoadOrder)
        //                {
        //                    WikiDocument document = new() { Name = documentName };
        //                    folder.Documents.Add(document);
        //                    string docTextPath = LibraryGES.ApplicationLocation + "\\Other\\Wiki\\" + categoryName + "\\" + documentName + "\\" + documentName + ".txt";
        //                    if (File.Exists(docTextPath))
        //                    {
        //                        document.Text = File.ReadAllText(docTextPath);
        //                    }
        //                    string docFolderPath = LibraryGES.ApplicationLocation + "\\Other\\Wiki\\" + categoryName + "\\" + documentName;
        //                    if (Directory.Exists(docFolderPath))
        //                    {
        //                        string[] imageFiles = Directory.GetFiles(docFolderPath).Where(f => f.EndsWith(".png") || f.EndsWith(".jpg")).ToArray();
        //                        foreach (string imageFile in imageFiles)
        //                        {
        //                            BitmapImage bitmap = new();
        //                            bitmap.BeginInit();
        //                            bitmap.UriSource = new Uri(imageFile);
        //                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //                            bitmap.EndInit();
        //                            WikiImage wikiImage = new() { FileName = System.IO.Path.GetFileName(imageFile), Bitmap = bitmap };
        //                            document.ImagesList.Add(wikiImage);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    foreach (WikiFolder folder in LibraryGES.Wiki.Folders)
        //    {
        //        GenerateCategory(folder);
        //    }
        //}

        private void GenerateCategory(WikiFolder Category)
        {
            TreeViewItem Categoryitem = new();
            Categoryitem.Header = Category.Name;
            Categoryitem.Tag = Category;
            TreeOfCategorys.Items.Add(Categoryitem);
            
            {
                ContextMenu contextMenu = new();
                Categoryitem.ContextMenu = contextMenu;

                MenuItem openCatFolder = new();
                openCatFolder.Header = "Open Folder";
                contextMenu.Items.Add(openCatFolder);
                openCatFolder.Click += OpenCatFolderClick;

                void OpenCatFolderClick(object sender, RoutedEventArgs e)
                {
                    string folderPath = LibraryGES.ApplicationLocation + "\\Other\\Wiki\\" + Category.Name;
                    if (Directory.Exists(folderPath))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = folderPath,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                    else
                    {
                        MessageBox.Show("Folder does not exist on disk.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }
        private void GenerateDocument(WikiDocument Document)
        {            

            TreeViewItem Documentitem = new();            
            Documentitem.Header = Document.Name;
            Documentitem.Tag = Document;
            TreeOfDocuments.Items.Add(Documentitem);
            Documentitem.IsSelected = true;

            {
                ContextMenu contextMenu = new();
                Documentitem.ContextMenu = contextMenu;

                MenuItem openDocFolder = new();
                openDocFolder.Header = "Open Folder";
                contextMenu.Items.Add(openDocFolder);

                openDocFolder.Click += OpenDocFolderClick;

                void OpenDocFolderClick(object sender, RoutedEventArgs e)
                {
                    TreeViewItem catItem = TreeOfCategorys.SelectedItem as TreeViewItem;
                    if (catItem == null)
                    {
                        MessageBox.Show("No category selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    WikiFolder category = catItem.Tag as WikiFolder;
                    string folderPath = LibraryGES.ApplicationLocation + "\\Other\\Wiki\\" + category.Name + "\\" + Document.Name;
                    if (Directory.Exists(folderPath))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = folderPath,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                    else
                    {
                        MessageBox.Show("Folder does not exist on disk.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                //MenuItem favoriteMenuitem = new();
                //favoriteMenuitem.Header = "Favorite";
                //contextMenu.Items.Add(favoriteMenuitem);

                //MenuItem deleteMenuitem = new();
                //deleteMenuitem.Header = "Delete";
                //contextMenu.Items.Add(deleteMenuitem);
            }
            


        }

        private void NewWikiCategory(object sender, RoutedEventArgs e)
        {
            WikiFolder Category = new();
            LibraryGES.Wiki.Folders.Add(Category);
            GenerateCategory(Category);
        }

        private void DeleteWikiCategory(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = TreeOfCategorys.SelectedItem as TreeViewItem;
            if (item != null)
            {
                WikiFolder category = item.Tag as WikiFolder;

                if (category.Documents.Count == 0) 
                {
                    bool delete = PixelWPF.LibraryPixel.NotificationConfirm("Delete Category?", "Are you sure you want to delete this category? \n\n(You still have to save afterward)");
                    if (delete == false) { return; }

                    LibraryGES.Wiki.Folders.Remove(category);
                    TreeOfCategorys.Items.Remove(item);
                }
                else
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Category is not empty", "Please delete everything inside first.");
                }
                    
            }
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

        private void DeleteWikiTopic(object sender, RoutedEventArgs e)
        {
            bool delete = PixelWPF.LibraryPixel.NotificationConfirm("Delete Document?","Are you sure you want to delete this document? \n\n(You still have to save afterward)");
            if (delete == false) { return; }

            TreeViewItem item = TreeOfDocuments.SelectedItem as TreeViewItem;
            if (item != null)
            {
                WikiDocument document = item.Tag as WikiDocument;
                TreeViewItem catItem = TreeOfCategorys.SelectedItem as TreeViewItem;
                if (catItem != null)
                {
                    WikiFolder category = catItem.Tag as WikiFolder;
                    category.Documents.Remove(document);
                    TreeOfDocuments.Items.Remove(item);
                }
            }
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

        private void MovCatUp(object sender, RoutedEventArgs e)
        {
            MoveTreeViewItem(TreeOfCategorys, -1, LibraryGES.Wiki.Folders);
        }

        private void MovCatDown(object sender, RoutedEventArgs e)
        {
            MoveTreeViewItem(TreeOfCategorys, 1, LibraryGES.Wiki.Folders);
        }

        private void MovDocUp(object sender, RoutedEventArgs e)
        {
            // Documents are children of the selected Category
            var catItem = TreeOfCategorys.SelectedItem as TreeViewItem;
            if (catItem?.Tag is WikiFolder category)
            {
                MoveTreeViewItem(TreeOfDocuments, -1, category.Documents);
            }
        }

        private void MovDocDown(object sender, RoutedEventArgs e)
        {
            var catItem = TreeOfCategorys.SelectedItem as TreeViewItem;
            if (catItem?.Tag is WikiFolder category)
            {
                MoveTreeViewItem(TreeOfDocuments, 1, category.Documents);
            }
        }

        //Helper method to move items up/down in a TreeView and the corresponding backend list
        private void MoveTreeViewItem<T>(TreeView treeView, int direction, List<T> backendList)
        {
            if (treeView.SelectedItem is not TreeViewItem selectedItem) return;
            if (selectedItem.Tag is not T dataObject) return;

            // 1. Get current index in the TreeView
            ItemsControl parentContainer = ItemsControl.ItemsControlFromItemContainer(selectedItem);
            if (parentContainer == null) return;

            int oldIndex = parentContainer.Items.IndexOf(selectedItem);
            int newIndex = oldIndex + direction;

            // 2. Check boundaries
            if (newIndex < 0 || newIndex >= parentContainer.Items.Count) return;

            // 3. Move in Backend List
            backendList.Remove(dataObject);
            backendList.Insert(newIndex, dataObject);

            // 4. Move in UI TreeView
            parentContainer.Items.RemoveAt(oldIndex);
            parentContainer.Items.Insert(newIndex, selectedItem);

            // 5. Keep the item selected and focused after the move
            selectedItem.IsSelected = true;
            selectedItem.Focus();
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

           // DocumentReadBox.Text = DocumentEditBox.Text;


            UpdateWikiPagePanel();
        }

        private void UpdateWikiPagePanel() 
        {
            TreeViewItem item = TreeOfDocuments.SelectedItem as TreeViewItem;
            WikiDocument document = item.Tag as WikiDocument;

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

            UpdateWikiPagePanel();
        }

        private void ButtonEditModeClick(object sender, RoutedEventArgs e)
        {
            Window pwDialog = new Window
            {
                //Title = "Enter Password",
                Width = 320,
                Height = 160, // Increased slightly for better spacing
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this,
                // Optional: Makes it look more like a standard dialog
                WindowStyle = WindowStyle.ToolWindow
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
                IsDefault = true, // Pressing Enter now triggers this!
                Margin = new Thickness(0, 0, 10, 0)
            };

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Width = 70,
                IsCancel = true // Pressing Esc or the 'X' now triggers this!
            };

            // Button container
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            StackPanel mainPanel = new StackPanel();
            mainPanel.Children.Add(new Label { Content = "Please enter the password:", Margin = new Thickness(10, 5, 0, 0) });
            mainPanel.Children.Add(passwordBox);
            mainPanel.Children.Add(buttonPanel);

            pwDialog.Content = mainPanel;

            bool result = false;

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
                    passwordBox.Clear();
                    passwordBox.Focus();
                }
            };

            // Note: cancelButton doesn't need a Click handler because IsCancel=true 
            // automatically closes the window when clicked.

            pwDialog.ShowDialog();

            // If password was correct, continue
            if (result == true)
            {
                //General
                EditMode = true;
                ButtonEditMode.Visibility = Visibility.Collapsed;

                //Category
                ButtonNewCategory.Visibility = Visibility.Visible;
                ButtonDelCategory.Visibility = Visibility.Visible;
                CategoryEditGrid.Visibility = Visibility.Visible;
                CategoryNameBox.Visibility = Visibility.Visible;
                ButtonMovCatUp.Visibility = Visibility.Visible;
                ButtonMovCatDown.Visibility = Visibility.Visible;

                //Document
                ButtonNewDocument.Visibility = Visibility.Visible;
                ButtonDelDocument.Visibility = Visibility.Visible;
                DocumentEditGrid.Visibility = Visibility.Visible;
                DocumentNameBox.Visibility = Visibility.Visible;
                ButtonMovDocUp.Visibility = Visibility.Visible;
                ButtonMovDocDown.Visibility = Visibility.Visible;

                //Right Bar
                EditorView.Visibility = Visibility.Visible;
                SaveWikiButton.Visibility = Visibility.Visible;
                EditColumn.Width = new GridLength(700);

                if (this.Width < 1600)
                {
                    this.Width = 1600;
                }
                DocumentEditBox.IsEnabled = true;
            }
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

                string CategoryLoadOrder = "";

                foreach (WikiFolder folder in LibraryGES.Wiki.Folders)
                {
                    CategoryLoadOrder = CategoryLoadOrder + folder.Name + "\n";
                    string folderPath = WikiPath + "\\Wiki\\" + folder.Name;
                    Directory.CreateDirectory(folderPath);

                    string DocumentLoadOrder = "";

                    foreach (WikiDocument document in folder.Documents)
                    {
                        DocumentLoadOrder = DocumentLoadOrder + document.Name + "\n";
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

                    File.WriteAllText(WikiPath + "\\Wiki\\" + folder.Name + "\\LoadOrder.txt", DocumentLoadOrder);
                }

                File.WriteAllText(WikiPath + "\\Wiki" + "\\LoadOrder.txt", CategoryLoadOrder);
            }
            

            
        }
        

        
    }
}
