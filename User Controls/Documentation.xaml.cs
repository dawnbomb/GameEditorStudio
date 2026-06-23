using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Xml.Linq;
//using System.Windows.Shapes;

namespace GameEditorStudio
{
    public class Document
    {
        public string Name { get; set; } = "New Document";
        public string Note { get; set; } = "";
        public string Text { get; set; } = "";

        public DocumentTypes DocumentType { get; set; } = DocumentTypes.Project;

        public enum DocumentTypes {Workshop, Project }
    }


    public partial class DocumentsUserControl : UserControl
    {
        //public string ExePath = "";
        public WorkshopData WorkshopData { get; set; }

        public Workshop TheWorkshop { get; set; }
        TreeViewItem ?CurrentTreeItem { get; set; } //The currently selected document
       
                

        //TODO:
        //Menu Save Workshop Documents from workshop
        //Menu Save Project Documents from workshop
        //Save document name (rename)
        //Test if new docs appear that were not in load order
        //delete document
        //new document

        public DocumentsUserControl()
        {
            InitializeComponent();            
            
        }

        public void TabClicked()
        {
            
            WorkshopDocumentsTreeView.Items.Clear();
            ProjectDocumentsTreeView.Items.Clear();

            foreach (Document WorkDoc in WorkshopData.WorkshopDocumentsList)
            {
                CreateTreeItem(WorkDoc, WorkshopDocumentsTreeView, ProjectDocumentsTreeView);
            }

            if (TheWorkshop.IsPreviewMode == true) { return; }
            
            foreach (Document ProjDoc in WorkshopData.ProjectDocumentsList)
            {
                CreateTreeItem(ProjDoc, ProjectDocumentsTreeView, WorkshopDocumentsTreeView);
            }


        }

        

        private void ButtonNewWorkshopDocument(object sender, RoutedEventArgs e)
        {
            Document Document = new();
            WorkshopData.WorkshopDocumentsList.Add(Document);
            CreateTreeItem(Document, WorkshopDocumentsTreeView, ProjectDocumentsTreeView);

            foreach (TreeViewItem Item in WorkshopDocumentsTreeView.Items)
            {
                if (Item.Tag as Document == Document)
                {
                    Item.IsSelected = true;
                }
            }
        }

        private void ButtonNewProjectDocument(object sender, RoutedEventArgs e)
        {
            Document Document = new();
            WorkshopData.ProjectDocumentsList.Add(Document);
            CreateTreeItem(Document, ProjectDocumentsTreeView, WorkshopDocumentsTreeView);

            foreach (TreeViewItem Item in ProjectDocumentsTreeView.Items)
            {
                if (Item.Tag as Document == Document)
                {
                    Item.IsSelected = true;
                }
            }
        }




        public void CreateTreeItem(Document TheDocument, TreeView tree, TreeView OtherTree)
        {
            TreeViewItem TreeViewItem = new();

            int i = 0; //This chunk makes sure a new document will never have the same name as an existing document.
            int goal = 1; //without the user getting an error, interrupting the flow of concentration.
            for (; i < goal; i++)
            {
                foreach (TreeViewItem Item in tree.Items)
                {
                    if (Item.Header as string == TheDocument.Name)
                    {
                        TheDocument.Name = TheDocument.Name + "2";
                        goal++;
                    }
                }

            }

            TreeViewItem.Header = TheDocument.Name;
            TreeViewItem.Tag = TheDocument;
            tree.Items.Add(TreeViewItem);

            TreeViewItem.Selected += (sender, e) =>
            {
                //if (CurrentDocument != null)
                //{
                //    SaveCurrentDocumentToMemory();
                //}

                //CurrentDocument = TheDocument;
                //CurrentTreeItem = TreeViewItem;

                //if (CurrentDocument == null) 
                //{
                //    DocumentNameBox.IsEnabled = false;
                //    DocumentTextBox.IsEnabled = false;
                //    DocumentNameBox.Text = "";
                //    DocumentTextBox.Text = "";
                //}

                //DocumentNameBox.Text = TheDocument.Name;
                //DocumentTextBox.Text = TheDocument.Text;
                
                

                //TreeViewItem selectedItem = OtherTree.SelectedItem as TreeViewItem;
                //if (selectedItem != null)
                //{
                //    selectedItem.IsSelected = false;
                //}

            };



            // Create the ContextMenu
            ContextMenu contextMenu = new ContextMenu();

            // Create a MenuItem
            MenuItem menuItem1 = new MenuItem();
            menuItem1.Header = "Delete Document";
            menuItem1.Click += MenuItem1_Click; // Event handler for click action
            contextMenu.Items.Add(menuItem1);

            TreeViewItem.ContextMenu = contextMenu;

            void MenuItem1_Click(object sender, RoutedEventArgs e)
            {
                // Handle the click event for MenuItem1
                WorkshopData.WorkshopDocumentsList.Remove(TheDocument);
                WorkshopData.ProjectDocumentsList.Remove(TheDocument);
                tree.Items.Remove(TreeViewItem); //DocumentsTreeView.SelectedItem
            }

           

            //This chunk handles moving the documents via mouse click and drag.

            TreeViewItem.AllowDrop = true;
            TreeViewItem.Drop += DocumentDrop;
            void DocumentDrop(object sender, DragEventArgs e)
            {

                if (e.Data.GetDataPresent("MoveDocumentItem") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                {
                    TreeViewItem DropTreeViewItem = (TreeViewItem)e.Data.GetData("MoveDocumentItem");

                    if (TreeViewItem != DropTreeViewItem)
                    {
                        tree.Items.Remove(DropTreeViewItem);
                        int ToIndex = tree.Items.IndexOf(TreeViewItem) + 1;

                        if (ToIndex == tree.Items.Count && DropTreeViewItem != TreeViewItem)
                        {

                            tree.Items.Add(DropTreeViewItem);
                            DropTreeViewItem.IsSelected = true;
                        }
                        else if (DropTreeViewItem != TreeViewItem)
                        {
                            tree.Items.Insert(ToIndex, DropTreeViewItem);
                            DropTreeViewItem.IsSelected = true;
                        }
                    }



                }


            }

            if(WorkshopData.CurrentDocument == TheDocument) { TreeViewItem.IsSelected = true; }
        }

        private void WTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (WorkshopDocumentsTreeView.SelectedItem == null && ProjectDocumentsTreeView.SelectedItem == null) { SelectDocument(null); }
            if (WorkshopDocumentsTreeView.SelectedItem == null)  { return; }

            TreeViewItem OtherSelectedItem = ProjectDocumentsTreeView.SelectedItem as TreeViewItem;
            if (OtherSelectedItem != null) { OtherSelectedItem.IsSelected = false; }
            TreeViewItem MySelectedItem = WorkshopDocumentsTreeView.SelectedItem as TreeViewItem;
            CurrentTreeItem = MySelectedItem;
            SelectDocument(MySelectedItem.Tag as Document);  //e.NewValue;
        }

        private void PTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (WorkshopDocumentsTreeView.SelectedItem == null && ProjectDocumentsTreeView.SelectedItem == null) { SelectDocument(null); }
            if (ProjectDocumentsTreeView.SelectedItem == null) { return; }

            TreeViewItem OtherSelectedItem = WorkshopDocumentsTreeView.SelectedItem as TreeViewItem;
            if (OtherSelectedItem != null) { OtherSelectedItem.IsSelected = false; }
            TreeViewItem MySelectedItem = ProjectDocumentsTreeView.SelectedItem as TreeViewItem;
            CurrentTreeItem = MySelectedItem;
            SelectDocument(MySelectedItem.Tag as Document);
        }

        private void SelectDocument(Document newdocument)
        {
            if (newdocument == null) 
            {
                CurrentTreeItem = null;
                WorkshopData.CurrentDocument = null;
                DocumentNameBox.IsEnabled = false;
                DocumentTextBox.IsEnabled = false;
                DocumentNameBox.Text = "";
                DocumentTextBox.Text = "";
                return;
            }
            if (newdocument != null) 
            {
                WorkshopData.CurrentDocument = newdocument;
                DocumentNameBox.IsEnabled = true;
                DocumentTextBox.IsEnabled = true;
                DocumentNameBox.Text = newdocument.Name;
                DocumentTextBox.Text = newdocument.Text;
                return;
            }  

        }



        private void DocumentNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.CurrentDocument == null) { return; }

            if (e.Key == Key.Enter)
            {
                if (CurrentTreeItem != null)
                {
                    CurrentTreeItem.Header = DocumentNameBox.Text;
                    SaveCurrentDocumentToMemory();
                }

            }
        }

        private void DocumentTextChanged(object sender, TextChangedEventArgs e)
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.CurrentDocument == null) { return; }
            SaveCurrentDocumentToMemory();
        }

        public void SaveCurrentDocumentToMemory()
        {
            if (WorkshopData.CurrentDocument == null) { return; }

            WorkshopData.CurrentDocument.Name = DocumentNameBox.Text;
            WorkshopData.CurrentDocument.Text = DocumentTextBox.Text;
        }

        
    }


}
