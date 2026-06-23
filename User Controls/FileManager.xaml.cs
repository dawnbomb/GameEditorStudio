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
using System.Windows.Forms; //Im actually using this for the file picker because VistaOpenFileDialog refuses to respect setting an initil directory.
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
//using static System.Net.WebRequestMethods;
using static System.Windows.Forms.LinkLabel;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Threading;

//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Path = System.IO.Path;
using MenuItem = System.Windows.Controls.MenuItem;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for FileManager.xaml
    /// </summary>
    public partial class FileManager : System.Windows.Controls.UserControl
    {
        public Workshop WorkshopXaml { get; set; }
        public bool IsTextEditor { get; set; } = false;        
        public TextEditorData TextEditorData {  get; set; }
       



        public FileManager()
        {
            InitializeComponent();
           

            this.Loaded += new RoutedEventHandler(LoadEvent);

        }

        

        public void LoadEvent(object sender, RoutedEventArgs e)
        {
            var workshopWindow = VisualTreeHelper.GetParent(this);
            while (workshopWindow != null && workshopWindow is not Workshop)
            {
                workshopWindow = VisualTreeHelper.GetParent(workshopWindow);
            }

            if (workshopWindow is Workshop workshopControl)
            {
                WorkshopXaml = workshopControl;
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
                foreach (GameFile GameFile in WorkshopXaml.WorkshopData.GameFiles)//Error as part of the All 1 Window update
                {
                    TreeViewItem TreeViewItem = new TreeViewItem();

                    TreeViewItem.Tag = GameFile;                    
                        
                    TreeGameFiles.Items.Add(TreeViewItem);

                    FileItemNameBuilder(TreeViewItem);

                    SetupContextMenu(TreeViewItem, GameFile);




                }
            }
            else if (IsTextEditor == true)
            {
                //First we load the text editor's list of game files. 
                TextEditorData.ListOfGameFiles.Clear();
                foreach (string filelocation in TextEditorData.GameFileLocations) 
                {
                    foreach (GameFile gameFile in WorkshopXaml.WorkshopData.GameFiles)
                    {
                        if (gameFile.FileLocation == filelocation)
                        {
                            TextEditorData.ListOfGameFiles.Add(gameFile);
                        }
                    }
                }

                
                //Then we load the text editor's game files into the text editor's tree view. 
                foreach (GameFile GameFile in TextEditorData.ListOfGameFiles) 
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
            OpenInputFileLocation.Header = "Open File Location (Input Folder)  ";
            OpenInputFileLocation.Click += new RoutedEventHandler(OpenFileInputLocationFunction);
            void OpenFileInputLocationFunction(object sender, RoutedEventArgs e)
            {
                LibraryGES.OpenFileFolder(WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + GameFile.FileLocation);
            }

            MenuItem OpenOutputFileLocation = new MenuItem();
            ContextMenu.Items.Add(OpenOutputFileLocation);
            OpenOutputFileLocation.Header = "Open File Location (Output Folder)  ";
            OpenOutputFileLocation.Click += new RoutedEventHandler(OpenFileOutputLocationFunction);
            void OpenFileOutputLocationFunction(object sender, RoutedEventArgs e)
            {
                LibraryGES.OpenFileFolder(WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + GameFile.FileLocation);
            }
            string TheOutputPath = Path.Combine(WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory, GameFile.FileLocation);
            if (File.Exists(TheOutputPath))
            {

            }
            else
            {
                OpenOutputFileLocation.IsEnabled = false;
            }

            MenuItem OpenFileFromInput = new MenuItem();
            OpenFileFromInput.Header = "Open File (From Input)";
            ContextMenu.Items.Add(OpenFileFromInput);
            OpenFileFromInput.Click += OpenTheFileFromInput; // Event handler for click action
            void OpenTheFileFromInput(object sender, RoutedEventArgs e)
            {
                LibraryGES.OpenFile(WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + GameFile.FileLocation);

            }

            MenuItem OpenFileFromOutput = new MenuItem();
            OpenFileFromOutput.Header = "Open File (From Output)";
            ContextMenu.Items.Add(OpenFileFromOutput);
            OpenFileFromOutput.Click += OpenTheFileFromOutput; // Event handler for click action
            void OpenTheFileFromOutput(object sender, RoutedEventArgs e)
            {
                LibraryGES.OpenFile(WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + GameFile.FileLocation);

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
            FileNote.Text = " " + GameFile.FileNote;
            FileNote.Foreground = Brushes.Orange;

            TextBlockItem.Inlines.Add(FileNameText);
            TextBlockItem.Inlines.Add(FileNote);
            TreeViewItem.Header = TextBlockItem;


            TreeViewItem.Tag = GameFile;
            if (GameFile.FileWorkshopTooltip == "")
            {
                TreeViewItem.ToolTip = null;
                FileNameText.TextDecorations = null;
            }
            else
            {
                TreeViewItem.ToolTip = GameFile.FileWorkshopTooltip;
                FileNameText.TextDecorations = TextDecorations.Underline;
            }
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
            FileNotepadTextbox.Text = GameFile.FileWorkshopTooltip;
            FileLocationTextbox.Text = GameFile.FileLocation;

            {
                DependencyObject? current = this;

                while (current != null && current is not TextTableManager)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                //while (current != null && current is not DataTableManager)
                //{
                //    current = VisualTreeHelper.GetParent(current);
                //}
               

                if (current is TextTableManager)
                {
                    TextTableManager MyParent = (TextTableManager)current;

                    if (this == MyParent.FileManagerForTextFiles) 
                    {
                        TextTableManager TheThingy = (TextTableManager)current;
                        TheThingy.UpdateTextFileNameListPreview();
                        //string fullText = Encoding.UTF8.GetString(GameFile.FileBytes);
                        //string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                        //TheThingy.TextFilePreviewTextbox.Text = ""; // Clear it first if needed

                        //for (int i = 0; i < lines.Length; i++)
                        //{
                        //    TheThingy.TextFilePreviewTextbox.Text += (i) + ": " + lines[i] + "\n";
                        //}
                    }

                    if (this == MyParent.DataFileManager)
                    {
                        MyParent.UpdateDataFileNamesPreview();
                    }


                }

                if (current is DataTableManager)
                {
                    DataTableManager MyParent = (DataTableManager)current;

                    if (this == MyParent.JSONFileManager)
                    {
                        MyParent.UpdateJSONTree();
                    }

                }
            }
            
        }

        private void ButtonManageTextEditorFiles() 
        {            
            TextEditorFileManager textEditorFileManager = new TextEditorFileManager(TextEditorData);
            TextEditorData.MainGrid.Visibility = Visibility.Collapsed;
            if (TextEditorData.EditorVisual is TextEditor TE) 
            {
                TE.BackPanel.Children.Add(textEditorFileManager);
            }            
        }

        private void TreeGameFilesDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

                foreach (string fullPath in droppedFiles)
                {
                    AddWorkshopFile(fullPath, null); // Pass null since no dialog was used
                }
            }
        }



        private void ButtonAddFileToWorkshop(object sender, RoutedEventArgs e)
        {
            if (WorkshopXaml.WorkshopData == null) { return; }

            //VistaOpenFileDialog
            OpenFileDialog openFileDialog = new();
            string inputDir = WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory;
            if (Directory.Exists(inputDir))
            {
                openFileDialog.InitialDirectory = inputDir;
            }

            //if (openFileDialog.ShowDialog() == true)
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //string Testa = openFileDialog.FileName.Substring(TheWorkshop.ProjectDataItem.ProjectInputDirectory.Length).TrimStart('\\');
                string fullPath = openFileDialog.FileName;



                AddWorkshopFile(fullPath, openFileDialog);
            }
        }

        private void AddWorkshopFile(string fullPath, OpenFileDialog openFileDialog)
        {
            string inputDir = WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory;

            // Make sure the dropped file is inside the input directory
            if (!fullPath.StartsWith(inputDir, StringComparison.OrdinalIgnoreCase))
            {
                PixelWPF.LibraryPixel.NotificationNegative("Error: File outside the input folder!",
                    "You can only add files that are INSIDE the project's input directory!\n\n" +
                    $"Input folder: {inputDir}\n\n" +
                    $"Attempted: {fullPath}");
                return;
            }

            string relativePath = fullPath.Substring(inputDir.Length).TrimStart('\\');

            foreach (GameFile gamefile in WorkshopXaml.WorkshopData.GameFiles)
            {
                if (gamefile.FileLocation == relativePath)
                {
                    PixelWPF.LibraryPixel.Notification("Notice: File already in workshop",
                        "FYI: Yes i'm aware that sometimes games will have diffrent folders with identical file names inside them, causing those files to be hard to work with. " +
                        "To deal with this problem, Game Editor Studio allows users to give a Nickname. Files with nicknames are shown as if their Nickname IS their filename. " +
                        "To better understand what just happened involving the file you tried adding to the workshop, here is the workshops information on that file. " +
                        "\n" +
                        "\nRealname: " + gamefile.FileName +
                        "\nNickname: " + gamefile.FileNote +
                        "\nFilepath: " + gamefile.FileLocation +
                        "\n" +
                        "\n*The file path is relative, based on the input directory of this project and is not an absolute location on your computer. " +
                        "You can access the input directory from Shortcuts -> open input directory."
                        );

                    return;
                }
            }

            try
            {
                GameFile Agamefile = new();
                Agamefile.FileName = Path.GetFileName(fullPath);
                Agamefile.FileLocation = relativePath;
                Agamefile.FileBytes = File.ReadAllBytes(fullPath);               

                WorkshopXaml.WorkshopData.GameFiles.Add(Agamefile);
                
                {   //Automatically select the newly added file in the tree view
                    TreeViewItem TreeViewItem = new TreeViewItem();
                    TreeViewItem.Tag = Agamefile;
                    TreeGameFiles.Items.Add(TreeViewItem);
                    TreeViewItem.IsSelected = true;
                }
                
            }
            catch (Exception ex)
            {
                PixelWPF.LibraryPixel.Notification("Error Details", ex.Message + "\n" + ex.StackTrace);

                PixelWPF.LibraryPixel.Notification("Notice: File not found?",
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

            WorkshopXaml.HomeControl.FileManager.RefreshFileTree();
            RefreshFileTree();
        }



        private void FileNoteBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            
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
            GameFile.FileWorkshopTooltip = FileNotepadTextbox.Text;
            FileItemNameBuilder(treeViewItem);
        }

        public void SelectFile(GameFile GameFile) //Sometimes i can't select a item cause this didn't load yet, so this event makes sure it can.
        {
            this.Loaded += new RoutedEventHandler(LoadEvent); //This is the event that's called when the window is loaded. 
            void LoadEvent(object sender, RoutedEventArgs e)
            {
                foreach (TreeViewItem TheItem in TreeGameFiles.Items)
                {
                    if (GameFile == TheItem.Tag)
                    {
                        TheItem.IsSelected = true;
                    }
                }
            }
            
        }

        private void FileLocationTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (WorkshopXaml.WorkshopData.IsProjectLoaded == false) { FileLocationTextbox.ToolTip = null; return; }

            FileLocationTextbox.ToolTip = "" +
                "Location: " +
                "\n" + FileLocationTextbox.Text +
                "\n\n" +
                "From Project Input: " +
                "\n" + WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory + "\\"+ FileLocationTextbox.Text  +
                "\n\n" +
                "From Project Output: " +
                "\n" + WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\"+ FileLocationTextbox.Text; // Update the tooltip to show the file location text
        }

        
    }
}
