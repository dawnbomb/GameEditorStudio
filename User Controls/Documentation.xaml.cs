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
        WorkshopData Database;

        public string WorkshopName = "";
        Workshop TheWorkshop;
        TreeViewItem CurrentTreeItem; //The currently selected document
        Document CurrentDocument;

        public string Mode = "WorkshopDocuments";
        List<Document> DocumentsWorkshop { get; set; }
        List<Document> DocumentsProject { get; set; }
        //string[] WorkshopDocumentOrder;
        //string[] WorkshopDocumentFolderNames;

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
            
            this.Loaded += new RoutedEventHandler(LoadEvent);
        }

        public void LoadEvent(object sender, RoutedEventArgs e)
        {
            if (DocumentsWorkshop != null || DocumentsProject != null) 
            {
                return;
            }

            DocumentsWorkshop = new List<Document>();
            DocumentsProject = new List<Document>();

            var parentWindow = Window.GetWindow(this);

            if (parentWindow is Workshop workshopWindow)
            {
                Database = workshopWindow.MyDatabase;

                if (workshopWindow.IsPreviewMode == true) 
                {
                    return;
                }

                WorkshopName = workshopWindow.WorkshopName;
                TheWorkshop = workshopWindow;

                Database.Workshop.TheDocumentsUserControl = this;


                string[] WorkshopDocumentOrder = File.ReadLines(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents\\" + "LoadOrder.txt").ToArray();
                string[] WorkshopDocumentFolderNames = Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
                string[] ProjectDocumentOrder = File.ReadLines(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents\\" + "LoadOrder.txt").ToArray();
                string[] ProjectDocumentFolderNames = Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();


                foreach (string name in WorkshopDocumentOrder)
                {
                    if (WorkshopDocumentFolderNames.Contains(name))
                    {
                        Document TheDocument = new Document { Name = name, Text = System.IO.File.ReadAllText(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents\\" + name + "\\Text.txt") };
                        DocumentsWorkshop.Add(TheDocument); // Adding the document object to the list
                    }
                }

                foreach (string name in WorkshopDocumentFolderNames)//The, add any new documents to the document tree in alphabetical order.
                {
                    if (!WorkshopDocumentOrder.Contains(name))
                    {
                        Document TheDocument = new Document { Name = name, Text = System.IO.File.ReadAllText(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents\\" + name + "\\Text.txt") };
                        DocumentsWorkshop.Add(TheDocument); // Adding the document object to the list                 

                    }
                }


                foreach (string name in ProjectDocumentOrder)//The last known list of documents for this workshop, in the order they were saved in.
                {
                    if (ProjectDocumentFolderNames.Contains(name))
                    {
                        Document TheDocument = new Document { Name = name, Text = System.IO.File.ReadAllText(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents\\" + name + "\\Text.txt") };
                        DocumentsProject.Add(TheDocument); // Adding the document object to the list

                    }
                }

                foreach (string name in ProjectDocumentFolderNames)//The, add any new documents to the document tree in alphabetical order.
                {
                    if (!ProjectDocumentOrder.Contains(name))
                    {
                        Document TheDocument = new Document { Name = name, Text = System.IO.File.ReadAllText(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents\\" + name + "\\Text.txt") };
                        DocumentsProject.Add(TheDocument); // Adding the document object to the list

                    }
                }

            }



            ShowDocuments();

        }




        

        private void ButtonNewWorkshopDocument(object sender, RoutedEventArgs e)
        {
            Document Document = new();
            DocumentsWorkshop.Add(Document);
            CreateTreeItem(Document, WorkshopDocumentsTreeView, ProjectDocumentsTreeView);

        }

        private void ButtonNewProjectDocument(object sender, RoutedEventArgs e)
        {
            Document Document = new();
            DocumentsProject.Add(Document);
            CreateTreeItem(Document, ProjectDocumentsTreeView, WorkshopDocumentsTreeView);

        }



        private void ShowDocuments() 
        {
            foreach (Document WorkDoc in DocumentsWorkshop)
            {
                CreateTreeItem(WorkDoc, WorkshopDocumentsTreeView, ProjectDocumentsTreeView);
            }
            foreach (Document WorkDoc in DocumentsProject)
            {
                CreateTreeItem(WorkDoc, ProjectDocumentsTreeView, WorkshopDocumentsTreeView);
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
            tree.Items.Add(TreeViewItem);

            TreeViewItem.Selected += (sender, e) =>
            {
                if (CurrentDocument != null)
                {
                    SaveDocumentToMemory();
                }


                DocumentNameBox.Text = TheDocument.Name;
                DocumentTextBox.Text = TheDocument.Text;
                CurrentDocument = TheDocument;
                CurrentTreeItem = TreeViewItem;

                TreeViewItem selectedItem = OtherTree.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    selectedItem.IsSelected = false;
                }

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
                DocumentsWorkshop.Remove(TheDocument);
                DocumentsProject.Remove(TheDocument);
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


        }


        public void SaveDocumentToMemory()
        {
            if (CurrentDocument != null) 
            {
                CurrentDocument.Text = DocumentTextBox.Text;
            }
            
        }





        public void ButtonNewSaveAll(object sender, RoutedEventArgs e)
        {
            SaveAllDocumentsWorkshop();
            SaveAllDocumentsProject();
        }

        public void SaveAllDocumentsWorkshop() 
        {
            SaveDocumentToMemory();
            try
            {
                Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest");
                foreach (Document Document in DocumentsWorkshop)
                {
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest\\" + Document.Name);
                    System.IO.File.WriteAllText(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.

                }
                Directory.Delete(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest", true);

                //Assuming there was no errors, the next chunk actually saves the documents.
                //Instead of trying to properly deal with the logistics of renamed document folders, we just blow up the entire Documents folder and recreate it.       

                LibraryMan.NukeDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents");

                string DocumentOrder = "";
                foreach (Document Document in DocumentsWorkshop)
                {
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents\\" + Document.Name);
                    System.IO.File.WriteAllText(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    DocumentOrder = DocumentOrder + Document.Name + "\n";
                }
                System.IO.File.WriteAllText(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents\\" + "LoadOrder.txt", DocumentOrder);


            }
            catch
            {
                LibraryMan.NukeDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest");


                string Error = "Error: WORKSHOP Documentation not saved." +
                    "\n" +
                    "\nAn error occured during the \"Saving Documentation\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
                    "\n" +
                    "\nAs you were probably saving more then only your documentation, you'll be happy to hear that each part of saving is handled seperately. " +
                    "This means there is NO CHANCE that any other parts of the saving operation are affected, such as when also saving editors or saving workshop files. " +
                    "\n" +
                    "\nDocumentats are saved using the names you give them to actual folders. " +
                    "This can cause problems as each operating system has a diffrent list of symbols it doesn't allow folder names to use. " +
                    "For safety, we first simulate what would happen IF it actually saved anything, " +
                    "by creating a temporary dummy folder and saving everything to that temporary folder. " +
                    "This way there is no chance your actual document folder will get corrupted or result in any other serious error. :)" +
                    "\n" +
                    "\nAs your seeing this error, it means your operating system doesn't like atleast one of the symbols you tried using in a documents name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n" +
                    "\nTry changing the names of any documents you think might have caused the error, and then try saving your documents again. " +
                    "\n" +
                    "\nYes, the problem is the document NAMES, not the text inside them.";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
            }
        }
        public void SaveAllDocumentsProject() 
        {
            SaveDocumentToMemory();
            try
            {

                Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\ProjectFolderTestSave");
                foreach (Document Document in DocumentsProject)
                {
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\ProjectFolderTestSave\\" + Document.Name);
                    File.WriteAllText(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\ProjectFolderTestSave\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.

                }

                Directory.Delete(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\ProjectFolderTestSave", true);
                LibraryMan.NukeDirectory(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents");
                //Assuming there was no errors, the next chunk actually saves the documents.
                //Instead of trying to properly deal with the logistics of renamed document folders, we just blow up the entire Documents folder and recreate it.       


                //Directory.CreateDirectory(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents");
                string DocumentOrder = "";
                foreach (Document Document in DocumentsProject)
                {
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents\\" + Document.Name);
                    File.WriteAllText(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    DocumentOrder = DocumentOrder + Document.Name + "\n";

                }
                File.WriteAllText(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\Documents\\" + "LoadOrder.txt", DocumentOrder);




            }
            catch
            {
                LibraryMan.NukeDirectory(LibraryMan.ApplicationLocation + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectDataItem.ProjectName + "\\ProjectFolderTestSave");


                string Error = "Error: PROJECT Documentation not saved." +
                    "\n" +
                    "\nAn error occured during the \"Saving Documentation\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
                    "\n" +
                    "\nAs you were probably saving more then only your documentation, you'll be happy to hear that each part of saving is handled seperately. " +
                    "This means there is NO CHANCE that any other parts of the saving operation are affected, such as when also saving editors or saving workshop files. " +
                    "\n" +
                    "\nDocumentats are saved using the names you give them to actual folders. " +
                    "This can cause problems as each operating system has a diffrent list of symbols it doesn't allow folder names to use. " +
                    "For safety, we first simulate what would happen IF it actually saved anything, " +
                    "by creating a temporary dummy folder and saving everything to that temporary folder. " +
                    "This way there is no chance your actual document folder will get corrupted or result in any other serious error. :)" +
                    "\n" +
                    "\nAs your seeing this error, it means your operating system doesn't like atleast one of the symbols you tried using in a documents name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n" +
                    "\nTry changing the names of any documents you think might have caused the error, and then try saving your documents again. " +
                    "\n" +
                    "\nYes, the problem is the document NAMES, not the text inside them.";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
            }
        }



        private void DocumentTreePreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyUp(Key.LeftShift))
            //{
            //    var treeView = (TreeView)sender;
            //    var treeViewItem = GetTreeViewItemAtPoint(treeView, e.GetPosition(treeView));
            //    if (treeViewItem != null)
            //    {
            //        var data = new DataObject("MoveDocumentItem", treeViewItem);
            //        DragDrop.DoDragDrop(treeViewItem, data, DragDropEffects.Move);
            //    }
            //}

            //TreeViewItem GetTreeViewItemAtPoint(ItemsControl control, Point point)
            //{
            //    var hitTestResult = VisualTreeHelper.HitTest(control, point);
            //    var visualHit = hitTestResult?.VisualHit;
            //    while (visualHit != null)
            //    {
            //        if (visualHit is TreeViewItem treeViewItem)
            //        {
            //            return treeViewItem;
            //        }

            //        visualHit = VisualTreeHelper.GetParent(visualHit);
            //    }

            //    return null;
            //}
        }

        private void DocumentNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (CurrentTreeItem != null) 
                {
                    CurrentTreeItem.Header = DocumentNameBox.Text;
                    CurrentDocument.Name = DocumentNameBox.Text;
                }               


            }
        }
    }







    





}
