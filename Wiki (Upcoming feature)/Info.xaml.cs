using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
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

namespace GameEditorStudio
{
    
    public class WikiDataBase
    {
        public List<WikiCategory> Categories { get; set; } = new();
    }

    public class WikiCategory
    {
        public string Name { get; set; } = "New Category";
        public List<WikiDocument> Documents { get; set; } = new();
    }

    public class WikiDocument
    {
        public string Name { get; set; } = "New Document";
        public string Text { get; set; } = "";
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
        }

        private void GenerateCategory(WikiCategory Category)
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
            //if () { } //If category tree isn't selecting anything, then return.


            TreeViewItem Documentitem = new();
            Documentitem.Header = Document.Name;
            Documentitem.Tag = Document;
            TreeOfDocuments.Items.Add(Documentitem);


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
            WikiCategory Category = new();
            LibraryGES.Wiki.Categories.Add(Category);  
            TreeViewItem item = new();
            item.Tag = Category;
            item.Header = Category.Name;
            TreeOfCategorys.Items.Add(item);
        }

        private void NewWikiTopic(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = TreeOfCategorys.SelectedItem as TreeViewItem;
            if (item != null)
            {
                WikiCategory category = item.Tag as WikiCategory;

                WikiDocument Document = new();
                category.Documents.Add(Document);

                GenerateDocument(Document);
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
                WikiCategory category = item.Tag as WikiCategory;
                category.Name = CategoryNameBox.Text;
                item.Header = category.Name;
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
                item.Header = document.Name;
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

            WikiCategory category = item.Tag as WikiCategory;
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
            DocumentTextbox.Text = document.Text;



        }

        private void TextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewItem item = TreeOfDocuments.SelectedItem as TreeViewItem;
            if (item == null)
            {
                return;
            }

            WikiDocument document = item.Tag as WikiDocument;
            document.Text = DocumentTextbox.Text;
        }

        private void ButtonEditModeClick(object sender, RoutedEventArgs e)
        {
            ButtonEditMode.Visibility = Visibility.Collapsed;
            ButtonWriteMode.Visibility = Visibility.Visible;

            CategoryNameBox.Visibility = Visibility.Visible;
            DocumentNameBox.Visibility = Visibility.Visible;
            ButtonNewCategory.Visibility = Visibility.Visible;
            ButtonNewDocument.Visibility = Visibility.Visible;
        }
        private void ButtonWriteModeClick(object sender, RoutedEventArgs e)
        {
            ButtonWriteMode.Visibility = Visibility.Collapsed;
            ButtonReadMode.Visibility = Visibility.Visible;
        }
        private void ButtonReadModeClick(object sender, RoutedEventArgs e)
        {
            ButtonReadMode.Visibility = Visibility.Collapsed;
            ButtonWriteMode.Visibility = Visibility.Visible;
        }

        
    }
}
