using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
//using static System.Net.WebRequestMethods;
using static System.Windows.Forms.LinkLabel;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Path = System.IO.Path;
using System.Windows.Forms; //Im actually using this for the file picker because VistaOpenFileDialog refuses to respect setting an initil directory.

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for FileManager.xaml
    /// </summary>
    public partial class FileManager : System.Windows.Controls.UserControl
    {
        Workshop TheWorkshop { get; set; }
        public bool IsTextEditor = false;
        public Editor ThisEditor {  get; set; }

        public bool IsTextFileManager = false;

        bool TESTBOOL = false;






        public FileManager()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(LoadEvent);

        }

        public void LoadEvent(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is Workshop workshopWindow)
            {
                TheWorkshop = workshopWindow;
                RefreshFileTree(); 

                if (IsTextEditor == true)
                {
                    ItemsLabel.Content = "Text Files";
                    AddFileButton.Content = "Manage";
                    AddFileButton.Click -= ButtonAddFileToWorkshop;
                    AddFileButton.Click += (s, e) => ButtonManageTextEditorFiles();
                }
            }
        }

        

        public void SetupForTextEditor() 
        {
            IsTextEditor = true;
        }

        public void RefreshFileTree() //System.Windows.Controls.
        {
            GameFile TheSelectedFile = null;

            if (TreeGameFiles.SelectedItem != null) 
            {
                TreeViewItem Itema = TreeGameFiles.SelectedItem as TreeViewItem;
                TheSelectedFile = Itema.Tag as GameFile;
            }
            
            

            TreeGameFiles.Items.Clear();

            if (IsTextEditor == false)
            {
                foreach (GameFile GameFile in TheWorkshop.MyDatabase.GameFiles.Values)//Database.GameFiles) // Update the FileTree with the current File List.
                {
                    TreeViewItem TreeViewItem = new TreeViewItem();

                    TreeViewItem.Tag = GameFile;
                    TreeViewItem.ToolTip = GameFile.FileNotepad; 
                    TreeGameFiles.Items.Add(TreeViewItem);

                    FileItemNameBuilder(TreeViewItem);

                    SetupContextMenu(TreeViewItem, GameFile);




                }
            }
            else if (IsTextEditor == true)
            {
                foreach (GameFile GameFile in ThisEditor.TextEditorData.ListOfGameFiles) 
                {
                    TreeViewItem TreeViewItem = new TreeViewItem();

                    TreeViewItem.Tag = GameFile;
                    TreeGameFiles.Items.Add(TreeViewItem);

                    FileItemNameBuilder(TreeViewItem);

                    SetupContextMenu(TreeViewItem, GameFile);
                }
            }


            foreach (TreeViewItem Item3 in TreeGameFiles.Items) //FileTreeExtraTable.Items
            {
                if (Item3.Tag == TheSelectedFile)
                {
                    Item3.IsSelected = true;
                    break;
                }
            }

        }

        public void SetupContextMenu(TreeViewItem TreeViewItem, GameFile GameFile) 
        {
            ContextMenu ContextMenu = new ContextMenu(); // THE RIGHT CLICK MENU
            TreeViewItem.ContextMenu = ContextMenu;

            MenuItem OpenInputFileLocation = new MenuItem();
            ContextMenu.Items.Add(OpenInputFileLocation);
            OpenInputFileLocation.Header = "  Open File Location (Input Folder)  ";
            OpenInputFileLocation.Click += new RoutedEventHandler(OpenFileInputLocationFunction);
            void OpenFileInputLocationFunction(object sender, RoutedEventArgs e)
            {
                LibraryMan.OpenFileFolder(TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\" + GameFile.FileLocation);
            }

            MenuItem OpenOutputFileLocation = new MenuItem();
            ContextMenu.Items.Add(OpenOutputFileLocation);
            OpenOutputFileLocation.Header = "  Open File Location (Output Folder)  ";
            OpenOutputFileLocation.Click += new RoutedEventHandler(OpenFileOutputLocationFunction);
            void OpenFileOutputLocationFunction(object sender, RoutedEventArgs e)
            {
                LibraryMan.OpenFileFolder(TheWorkshop.ProjectDataItem.ProjectOutputDirectory + "\\" + GameFile.FileLocation);
            }
            string TheOutputPath = Path.Combine(TheWorkshop.ProjectDataItem.ProjectOutputDirectory, GameFile.FileLocation);
            if (File.Exists(TheOutputPath))
            {

            }
            else
            {
                OpenOutputFileLocation.IsEnabled = false;
            }


            //MenuItem OpenInHxD = new MenuItem();
            //ContextMenu.Items.Add(OpenInHxD);
            //OpenInHxD.Header = "  Open File In HxD Hex Editor  ";
            //OpenInHxD.Click += new RoutedEventHandler(OpenFileInHxD);
            //void OpenFileInHxD(object sender, RoutedEventArgs e)
            //{                    
            //    ProcessStartInfo startInfo = new ProcessStartInfo()
            //    {
            //        FileName = toolOne.Location, // Path to the executable
            //        UseShellExecute = true,       // This allows starting a process associated with a file type (when needed)                        
            //        Arguments = $"\"{GameFile.Value.FileLocation}\""
            //    };
            //    Process.Start(startInfo);

            //}
        }

        public void FileItemNameBuilder(TreeViewItem TreeViewItem)
        {
            //TreeViewItem TreeViewItem = new TreeViewItem();
            GameFile GameFile = TreeViewItem.Tag as GameFile;

            TextBlock TextBlockItem = new TextBlock();

            Run FileNameText = new Run();
            FileNameText.Text = GameFile.FileName;

            Run FileNote = new Run();
            FileNote.Text = "   " + GameFile.FileNote;
            FileNote.Foreground = Brushes.Orange;

            TextBlockItem.Inlines.Add(FileNameText);
            TextBlockItem.Inlines.Add(FileNote);
            TreeViewItem.Header = TextBlockItem;
        }

        

        private void SelectedTreeItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = TreeGameFiles.SelectedItem as TreeViewItem;
            if (treeViewItem == null) 
            {
                FileLocationTextbox.Text = "";
                FileNoteBox.Text = "";
                FileNotepadTextbox.Text = "";
                return; 
            }
            GameFile GameFile = treeViewItem.Tag as GameFile;
            FileNoteBox.Text = GameFile.FileNote;
            FileNotepadTextbox.Text = GameFile.FileNotepad;
            FileLocationTextbox.Text = GameFile.FileLocation;

            {
                DependencyObject? current = this;

                while (current != null && current is not TextSourceManager)
                {
                    current = VisualTreeHelper.GetParent(current);
                }

                if (current is TextSourceManager)
                {
                    TextSourceManager MyParent = (TextSourceManager)current;

                    if (this == MyParent.FileManagerForTextFiles) 
                    {
                        TextSourceManager TheThingy = (TextSourceManager)current;
                        string fullText = Encoding.UTF8.GetString(GameFile.FileBytes);
                        string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                        TheThingy.TextFilePreviewTextbox.Text = ""; // Clear it first if needed

                        for (int i = 0; i < lines.Length; i++)
                        {
                            TheThingy.TextFilePreviewTextbox.Text += (i) + ": " + lines[i] + "\n";
                        }
                    }

                    if (this == MyParent.DataFileManager)
                    {
                        MyParent.UpdateDataFileNamesPreview();
                    }




                }
            }
            
        }

        private void ButtonManageTextEditorFiles() 
        {
            TESTBOOL = true;
            TextEditorFileManager textEditorFileManager = new TextEditorFileManager(ThisEditor);
            ThisEditor.TextEditorData.MainGrid.Visibility = Visibility.Collapsed;
            ThisEditor.EditorBackPanel.Children.Add(textEditorFileManager);
            //TheTextEditor.TextEditorData. ;
        }

        private void ButtonAddFileToWorkshop(object sender, RoutedEventArgs e)
        {

            //{   //Smart seleting the folder to start in.
            //    string inputPath = TextBoxInputDirectory.Text + "\\";
            //    DirectoryInfo? current = new DirectoryInfo(inputPath);
            //    while (current != null && !current.Exists)
            //    {
            //        current = current.Parent;
            //    }
            //    if (current != null)
            //    {
            //        FolderSelect.SelectedPath = current.FullName + "\\";
            //    }
            //}
            //openFileDialog.Filter = "All files (*.*)|*.*";
            //openFileDialog.RestoreDirectory = true;


            if (TheWorkshop.MyDatabase == null) { return; }

            //VistaOpenFileDialog
            OpenFileDialog openFileDialog = new();
            string inputDir = TheWorkshop.ProjectDataItem.ProjectInputDirectory;
            if (Directory.Exists(inputDir))
            {
                openFileDialog.InitialDirectory = inputDir;
            }

            //if (openFileDialog.ShowDialog() == true)
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //string Testa = openFileDialog.FileName.Substring(TheWorkshop.ProjectDataItem.ProjectInputDirectory.Length).TrimStart('\\');
                string fullPath = openFileDialog.FileName;
                string inputDir2 = TheWorkshop.ProjectDataItem.ProjectInputDirectory;
                string Testa = fullPath.Substring(inputDir2.Length).TrimStart('\\');
                foreach (KeyValuePair<string, GameFile> gamefile in TheWorkshop.MyDatabase.GameFiles)
                {
                    if (gamefile.Key == Testa)
                    {
                        LibraryMan.Notification("Notice: File already in workshop",
                            "FYI: Yes i'm aware that sometimes games will have diffrent folders with identical file names inside them, causing those files to be hard to work with. " +
                            "To deal with this problem, Game Editor Studio allows users to give a Nickname. Files with nicknames are shown as if their Nickname IS their filename. " +
                            "To better understand what just happened involving the file you tried adding to the workshop, here is the workshops information on that file. " +
                            "\n" +
                            "\nRealname: " + gamefile.Value.FileName +
                            "\nNickname: " + gamefile.Value.FileNote +
                            "\nFilepath: " + gamefile.Value.FileLocation +
                            "\n" +
                            "\n*The file path is relative, based on the input directory of this project and is not an absolute location on your computer. " +
                            "You can access the input directory from Shortcuts -> open input directory."
                            );
                                               
                        return;
                    }
                }


                try 
                {
                    GameFile FileInfo = new();

                    FileInfo.FileName = Path.GetFileName(openFileDialog.FileName);
                    FileInfo.FileLocation = openFileDialog.FileName.Substring(TheWorkshop.ProjectDataItem.ProjectInputDirectory.Length).TrimStart('\\');
                    FileInfo.FileBytes = System.IO.File.ReadAllBytes(TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\" + FileInfo.FileLocation);

                    TheWorkshop.MyDatabase.GameFiles.Add(FileInfo.FileLocation, FileInfo);
                }
                catch 
                {
                    LibraryMan.Notification("Notice: File not found?",
                        "For some reason the workshop couldn't find the file you selected." +
                        "\n\n" +
                        "Possible causes..." +
                        "\n1: You tried adding a file outside the workshops input folder." +
                        "\n2: You selected the wrong input folder when first setting up your project." +
                        "\n3: Whoever created the workshop picked a bad workshops input folder to begin with." +
                        "\n\n" +
                        "Note: You can access the input folder from Shortcuts -> open input folder."
                        );
                                   
                }

                TheWorkshop.FileManager.RefreshFileTree();
                RefreshFileTree();



            }
        }   

        private void FileNoteBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{

            //}
            TreeViewItem treeViewItem = TreeGameFiles.SelectedItem as TreeViewItem;
            if (treeViewItem == null) { return; }
            GameFile GameFile = treeViewItem.Tag as GameFile;
            GameFile.FileNote = FileNoteBox.Text;
            FileItemNameBuilder(treeViewItem);
        }
        private void FileNotepadTextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewItem treeViewItem = TreeGameFiles.SelectedItem as TreeViewItem;
            if (treeViewItem == null) { return; }
            GameFile GameFile = treeViewItem.Tag as GameFile;
            GameFile.FileNotepad = FileNotepadTextbox.Text;
            treeViewItem.ToolTip = GameFile.FileNotepad; // Update the tooltip to show the notepad text
        }

        public void SelectItem(Entry Entry) //Sometimes i can't select a item cause this didn't load yet, so this event makes sure it can.
        {
            this.Loaded += new RoutedEventHandler(LoadEvent); //This is the event that's called when the window is loaded. 
            void LoadEvent(object sender, RoutedEventArgs e)
            {
                foreach (TreeViewItem TheItem in TreeGameFiles.Items)
                {
                    if (Entry.EntryTypeMenu.GameFile == TheItem.Tag)
                    {
                        TheItem.IsSelected = true;
                    }
                }
            }
            
        }
    }
}
