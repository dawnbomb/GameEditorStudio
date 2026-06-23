using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using GameEditorStudio.Loading;
using Microsoft.VisualBasic;
using Ookii.Dialogs.Wpf;
using WpfHexEditor;
using Path = System.IO.Path;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Workshop WorkshopXaml { get; set; } 
        public WorkshopData WorkshopData { get; set; }

        public Home()
        {
            InitializeComponent();

            //RefreshProjectsTreeView();
        }

        public void HomeSetup(WorkshopData TheWorkshopData)
        {
            WorkshopData = TheWorkshopData;
            WorkshopXaml = WorkshopData.WorkshopXaml;

            if (WorkshopData.LoadedProject == null) { LabelCurrentProject.Content = "NONE"; }
            if (WorkshopData.LoadedProject != null) { LabelCurrentProject.Content = TheWorkshopData.LoadedProject.ProjectName; }
            RefreshProjectsTreeView();

            WorkshopXaml.ButtonHome.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); //Click the home tab, triggers the setting of the tab styles. 
            if (WorkshopData.IsProjectLoaded == false) 
            {
                HomeUnloadProjectButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }

            
        }
        

        public void LoadProject(Project ProjectData)
        {
            LoadingPanel.Visibility = Visibility.Visible;

            if (ProjectData == null)
            {
                WorkshopData.LoadedProject = null;
                WorkshopData.IsProjectLoaded = false;
                WorkshopData.WorkshopXaml.IsPreviewMode = true;

                LabelCurrentProject.Content = "NONE (Preview Mode)";
                FileManager.IsEnabled = false;
            }

            if (ProjectData != null)
            {
                WorkshopData.LoadedProject = ProjectData;
                WorkshopData.IsProjectLoaded = true;
                WorkshopData.WorkshopXaml.IsPreviewMode = false;

                LabelCurrentProject.Content = "Loading...";
                FileManager.IsEnabled = true;
            }

            


            FileLoading fileLoading = new();
            fileLoading.TryLoadAllGameFilesIntoWorkshopDatabase(WorkshopData); //First we load workshop files into the database. }

            FileManager.RefreshFileTree();

            LoadWorkshopDatabaseCode LoadDatabase = new();
            LoadDatabase.LoadAllProjectDocuments(WorkshopData);

            WorkshopData.WorkshopXaml.MenusForToolsAndEvents.SetupTopMenuForProject(WorkshopData.WorkshopXaml);            


            foreach (DataTableEditorData DTEData in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                DTEData.DTEXaml.ProjectLoadDTETables(ProjectData);
            }

            //Loading bar code
            double max = WorkshopData.GameEditors.Count;
            double loadcounter = 0;
            LoadingProgressBarUI.Maximum = 100;
            LoadingProgressBarUI.Value = 0;
            LoadingPartTextUI.Content = "Part 2: Generating Game Editors...";
            LoadUIUI.Visibility = Visibility.Visible;

            //Load / regenerate the editors with the game files.
            foreach (DataTableEditorData DTEData in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                LoadingStatusTextUI.Content = DTEData.EditorName + " (" + (loadcounter + 1).ToString() + "/" + WorkshopData.GameEditors.Count + ")";
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

                DTEData.DTEXaml.GenerateUI();

                //Loading bar code

                loadcounter++;
                double percent = (loadcounter / max) * 100;
                int calc = (int)percent;
                LoadingProgressBarUI.Value = calc;

            }
            foreach (TextEditorData TEXTData in WorkshopData.GameEditors.OfType<TextEditorData>())
            {
                LoadingStatusTextUI.Content = TEXTData.EditorName + " (" + (loadcounter + 1).ToString() + "/" + WorkshopData.GameEditors.Count + ")";
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

                TEXTData.TextEditorXaml.GenerateUI();

                //Loading bar code
                loadcounter++;
                double percent = (loadcounter / max) * 100;
                int calc = (int)percent;
                LoadingProgressBarUI.Value = calc;
                
            }
            //LoadingPartText.Content = "DONE~";
            LoadingStatusTextUI.Content = "DONE!";
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));


            foreach (DataTableEditorData TheEditor in WorkshopData.GameEditors.OfType<DataTableEditorData>()) //Sync Entry Decorations state & Sets Symbology for all DTE editors. 
            {//Fix this later, as im doing for all DTE, for all DTE. 
                TheEditor.DataTableEditorData.DTEXaml.UpdateEntryDecorationsForAllEditors();
                break;
            }

            //RefreshUI();
            //Thread.Sleep(10);
            //Thread.Sleep(10000);

            if (ProjectData != null)
            {
                LabelCurrentProject.Content = ProjectData.ProjectName;
            }

            LoadingPanel.Visibility = Visibility.Collapsed;
            LoadUIUI.Visibility = Visibility.Collapsed;
        }





        



        public void RefreshProjectsTreeView()
        {
            ProjectsTreeView.Items.Clear();
            if (WorkshopData.ProjectsList == null) { return; }
            foreach (Project ProjectData in WorkshopData.ProjectsList)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = ProjectData.ProjectName;
                item.Tag = ProjectData;
                ProjectsTreeView.Items.Add(item);
                if (WorkshopData.LoadedProject == null && WorkshopData.SelectedProject == ProjectData) { item.IsSelected = true; }
                else if (WorkshopData.LoadedProject == ProjectData ) { item.IsSelected = true; }
                
            }
        }

        private void ProjectsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            WorkshopData.SelectedProject = GetSelectedProject();

            Project GetSelectedProject()
            {
                TreeViewItem treeViewItem = ProjectsTreeView.SelectedItem as TreeViewItem;
                if (treeViewItem == null) { return null; }
                Project projectdata = treeViewItem?.Tag as Project;
                if (projectdata != null)
                {
                    LabelSelectedProject.Content = projectdata.ProjectName + " (Events use this)";
                    return projectdata;
                }
                LabelSelectedProject.Content = "NONE" + " (Events use this)";
                return null;
            }

            RefreshProjectEventResourcesUI(WorkshopData.SelectedProject);

            TreeViewItem treeViewItem = ProjectsTreeView.SelectedItem as TreeViewItem;
            if (treeViewItem == null)
            {   
                ProjectNameTextbox.Text = "";
                TextBoxInputDirectory.Text = "";
                BorderInputFolder.ToolTip = null;
                TextBoxOutputDirectory.Text = "";
                BorderOutputFolder.ToolTip = null;
            }
            Project projectdata = treeViewItem?.Tag as Project;            
            if (projectdata != null)
            {
                
                ProjectNameTextbox.Text = projectdata.ProjectName;
                TextBoxInputDirectory.Text = projectdata.ProjectInputDirectory;
                BorderInputFolder.ToolTip = projectdata.ProjectInputDirectory;
                TextBoxOutputDirectory.Text = projectdata.ProjectOutputDirectory;
                BorderOutputFolder.ToolTip = projectdata.ProjectOutputDirectory;
            }
        }


        private void OpenSelectedProjectFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenSelectedProjectFolder(MethodData);
        }

        private void OpenSelectedProjectInputFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenSelectedProjectInputFolder(MethodData);
        }
        private void OpenSelectedProjectOutputFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenSelectedProjectOutputFolder(MethodData);
        }

        private void CopyProjectInputFolderTextButton(object sender, RoutedEventArgs e)
        {
            string textToCopy = TextBoxInputDirectory.Text;
            if (string.IsNullOrEmpty(textToCopy)) return;

            Clipboard.Clear(); // Clear first
            System.Threading.Thread.Sleep(200);

            for (int i = 0; i < 2; i++)
            {
                try
                {                    
                    Clipboard.SetDataObject(textToCopy, true);
                    return;
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    if ((uint)ex.ErrorCode != 0x800401D0) throw;
                    System.Threading.Thread.Sleep(100);
                }
            }

            PixelWPF.LibraryPixel.NotificationNegative("Error copying to clipboard",
                "I have no fucking idea why, but copying text is buggy in my program. \n\nAtleast i'm telling you it failed? >_>\n\nIt works like 1 in every 10 tries. This problem drove me mad until i gave up. Fuck this >:(");
        }

        private void CopyProjectOutputFolderTextButton(object sender, RoutedEventArgs e)
        {
            string textToCopy = TextBoxOutputDirectory.Text;
            if (string.IsNullOrEmpty(textToCopy)) return;

            // Try up to 10 times to open the clipboard
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Clipboard.SetText(textToCopy);
                    return; // Success! Exit the method.
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    // 0x800401D0 is the "CLIPBRD_E_CANT_OPEN" error code
                    if ((uint)ex.ErrorCode != 0x800401D0) throw;

                    // Wait 10 milliseconds before trying again
                    System.Threading.Thread.Sleep(30);
                }
            }
            PixelWPF.LibraryPixel.NotificationNegative("Error copying to clipboard", "I have no fucking idea why, but copying text is buggy in my program. \n\nAtleast i'm telling you it failed? >_>\n\nIt works like 1 in every 10 tries. This problem drove me mad until i gave up. Fuck this >:(");
        }

        private void ButtonLoadProject(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = ProjectsTreeView.SelectedItem as TreeViewItem;
            if (treeViewItem == null) { return; }
            Project projectdata = treeViewItem?.Tag as Project;
            if (projectdata != null)
            {
                LoadProject(projectdata);  
            }
        }

        private void ButtonUnloadProject(object sender, RoutedEventArgs e)
        {
            WorkshopData.SelectedProject = null;
            LoadProject(null);
        }
        

        private void ChangeProjectName(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (WorkshopData.SelectedProject == null) { return; }

                Project TheProject = WorkshopData.SelectedProject;
                string oldFolderPath = LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + TheProject.ProjectName;
                string newFolderPath = LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + ProjectNameTextbox.Text;

                if (oldFolderPath == newFolderPath) { return; }

                //Rename the project
                try 
                {
                    Directory.Move(oldFolderPath, newFolderPath);// Rename the folder at the old path to the new path
                } 
                catch 
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Cannot Rename / Folder in Use", "Renaming was stopped because windows is saying the folder is in use. \n\nIn my testing, this mosty happens is if the folder is open in your windows file explorer. I was able to do this even when files were open in notepad or notepad++, but not if the folder is open. \n\nOf corse, some files could actually be in important use, but atleast for now i only store folders and .txt files in the project folder, sooo...\n\nKinda ridiculous... >_>");
                    return;
                }
                

                TheProject.ProjectName = ProjectNameTextbox.Text;

                CommandMethodsClass.SaveProjectXML(TheProject, WorkshopData);

                //Now update the UI.
                TreeViewItem SelectedTreeViewItem = ProjectsTreeView.SelectedItem as TreeViewItem;
                if (SelectedTreeViewItem == null) { return; }

                SelectedTreeViewItem.Header = ProjectNameTextbox.Text;                
            }
        }

        private void SetSelectedProjectInputFolder(object sender, RoutedEventArgs e)
        {
            if (WorkshopData.SelectedProject == null) { return; }

            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog();//This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select the folder named " + WorkshopData.WorkshopInputDirectory; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.
            {   //Smart seleting the folder to start in.
                string inputPath = TextBoxInputDirectory.Text + "\\";
                DirectoryInfo? current = new DirectoryInfo(inputPath);
                while (current != null && !current.Exists)
                {
                    current = current.Parent;
                }
                if (current != null)
                {
                    FolderSelect.SelectedPath = current.FullName + "\\";
                }
            }

            if ((bool)FolderSelect.ShowDialog(Window.GetWindow(this))) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                                
                if (WorkshopData.ProjectsRequireSameFolderName == false)
                {
                    PixelWPF.LibraryPixel.NotificationPositive("You MAYBE selected the correct folder?",
                        "This workshop doesn't require a specific folder name to be selected. " +
                        "This is usually set when the input folder is one that users commonly want to be able to rename. " +
                        "\n\n" +
                        "If your not sure, I STRONGLY recommend checking the readme, as well as asking around. \n\nOr you can just load your project and see if you get a ton of errors. :p"
                    );

                    WorkshopData.SelectedProject.ProjectInputDirectory = FolderSelect.SelectedPath;
                    TextBoxInputDirectory.Text = FolderSelect.SelectedPath;

                    CommandMethodsClass.SaveProjectXML(WorkshopData.SelectedProject, WorkshopData);
                }
                else if (WorkshopData.WorkshopInputDirectory == Path.GetFileName(FolderSelect.SelectedPath) && WorkshopData.ProjectsRequireSameFolderName == true)
                {
                    PixelWPF.LibraryPixel.NotificationPositive("You selected the correct folder!",
                        "The folder name you selected is the same as the one this workshop is looking for. " +
                        "This can only be wrong if you selected a folder with the exact same name, but a diffrent location."
                    );
                    
                    WorkshopData.SelectedProject.ProjectInputDirectory = FolderSelect.SelectedPath;
                    TextBoxInputDirectory.Text = FolderSelect.SelectedPath;

                    CommandMethodsClass.SaveProjectXML(WorkshopData.SelectedProject, WorkshopData);
                }
                else
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Wrong folder selected!",
                        "This workshop is looking for you to select a folder named \"" + WorkshopData.WorkshopInputDirectory + "\"." +
                        "\n\n" +
                        "If your confused, check the README, or see if there are any helpful discords.");

                }




            }
        }

        
        private void SetSelectedProjectOutputFolder(object sender, RoutedEventArgs e)
        {
            if (WorkshopData.SelectedProject == null) { return; }

            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select where files will save to."; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            {   //Smart seleting the folder to start in.
                string outputPath = TextBoxOutputDirectory.Text + "\\";
                DirectoryInfo? current = new DirectoryInfo(outputPath);
                while (current != null && !current.Exists)
                {
                    current = current.Parent;
                }
                if (current != null)
                {
                    FolderSelect.SelectedPath = current.FullName + "\\";
                }
            }
            if ((bool)FolderSelect.ShowDialog(Window.GetWindow(this))) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                WorkshopData.SelectedProject.ProjectOutputDirectory = FolderSelect.SelectedPath;
                TextBoxOutputDirectory.Text = FolderSelect.SelectedPath;

                //UpdateProjectXML(WorkshopData.SelectedProject);//ProjectName, Input, Output     

                CommandMethodsClass.SaveProjectXML(WorkshopData.SelectedProject, WorkshopData);
            }
        }

        private void UpdateProjectXML(Project ProjectData)
        {
            //if (ProjectData.ProjectEventResources == null) { ProjectData.ProjectEventResources = new(); }

            //try
            //{
            //    XmlWriterSettings settings = new XmlWriterSettings();
            //    settings.Indent = true;
            //    settings.IndentChars = ("    ");
            //    settings.CloseOutput = true;
            //    settings.OmitXmlDeclaration = true;
            //    using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\" + ProjectData.ProjectName + "\\" + "Project.xml", settings))
            //    {
            //        writer.WriteStartElement("Project"); //This is the root of the XML
            //        writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
            //        writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
            //        writer.WriteElementString("NOTE", "The resources are (Project Event Resources) with a key matching (Workshop Event Resources).");
            //        writer.WriteElementString("Seperator", "====================================================================================");
            //        writer.WriteElementString("Name", ProjectData.ProjectName);
            //        writer.WriteElementString("InputLocation", ProjectData.ProjectInputDirectory);
            //        writer.WriteElementString("OutputLocation", ProjectData.ProjectOutputDirectory);

            //        writer.WriteStartElement("ResourceList");
            //        foreach (ProjectEventResource ProjectEventData in ProjectData.ProjectEventResources)
            //        {
            //            writer.WriteStartElement("Resource");
            //            writer.WriteElementString("Name", WorkshopEventResources.Find(thing => thing.WorkshopResourceKey == ProjectEventData.ResourceKey).Name);
            //            writer.WriteElementString("Key", ProjectEventData.ResourceKey);
            //            writer.WriteElementString("Location", ProjectEventData.Location);
            //            writer.WriteEndElement(); //End Event Resources
            //        }
            //        writer.WriteEndElement(); //End Event Resources

            //        writer.WriteEndElement(); //End Project  AKA the Root of the XML   
            //        writer.Flush(); //Ends the XML
            //    }
            //}
            //catch
            //{

            //}

            ////DataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
            //string TheProjectFolder = LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\" + ProjectData.ProjectName + "\\";

            //if (Directory.Exists(TheProjectFolder + "\\" + "Documents" + "\\"))  //Documents Folder
            //{
            //}
            //else
            //{
            //    Directory.CreateDirectory(TheProjectFolder + "\\" + "Documents" + "\\");
            //}

            //if (Directory.Exists(TheProjectFolder + "\\" + "\\Documents\\LoadOrder.txt")) //LoadOrder.txt file
            //{

            //}
            //else
            //{
            //    System.IO.File.WriteAllText(TheProjectFolder + "\\" + "\\Documents\\LoadOrder.txt", " ");
            //}



        }








        public void RefreshProjectEventResourcesUI(Project UserProject)
        {
            LabelForMissingProjectResources.Visibility = Visibility.Collapsed;
            ProjectEventResourcesPanel.Children.Clear();

            if (UserProject == null)
            {
                return;
            }

            foreach (EventResource WorkshopEventResource in WorkshopData.WorkshopEventResources)
            {
                if (WorkshopEventResource.IsChild == true)
                {
                    continue;
                }
                if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.CMDText)
                {
                    continue;
                }




                DockPanel MainPanel = new();
                ProjectEventResourcesPanel.Children.Add(MainPanel);
                DockPanel.SetDock(MainPanel, Dock.Top);
                MainPanel.Margin = new Thickness(4, 2, 4, 7);
                MainPanel.Background = Brushes.Transparent;

                DockPanel TopPanel = new();
                DockPanel.SetDock(TopPanel, Dock.Top);
                MainPanel.Children.Add(TopPanel);
                TopPanel.LastChildFill = false;
                TopPanel.Background = Brushes.Transparent;

                DockPanel BottomPanel = new();
                DockPanel.SetDock(BottomPanel, Dock.Top);
                MainPanel.Children.Add(BottomPanel);
                BottomPanel.Background = Brushes.Transparent;



                Label Label = new();
                TopPanel.Children.Add(Label);
                DockPanel.SetDock(Label, Dock.Left);
                if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.File && WorkshopEventResource.IsChild == false)
                { Label.Content = "🗎   " + WorkshopEventResource.Name; }
                if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.Folder && WorkshopEventResource.IsChild == false)
                { Label.Content = "📁 " + WorkshopEventResource.Name; }
                if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.CMDText && WorkshopEventResource.IsChild == false)
                { Label.Content = "✎ " + WorkshopEventResource.Name; }

                Button OpenButton = new();
                TopPanel.Children.Add(OpenButton);
                DockPanel.SetDock(OpenButton, Dock.Right);
                OpenButton.Height = 30;
                OpenButton.Content = " Open ";

                Button BrowseButton = new();
                TopPanel.Children.Add(BrowseButton);
                DockPanel.SetDock(BrowseButton, Dock.Right);
                BrowseButton.Width = 100;
                BrowseButton.Height = 30;
                BrowseButton.Margin = new Thickness(0, 0, 4, 0);
                BrowseButton.Content = "Browse...";



                //< Border CornerRadius = "8" BorderBrush = "Black"  BorderThickness = "1.5" Padding = "2" Background = "#FF18191B" >  < !--Background = "White"-- >
                //        < TextBox x: Name = "ProjectNameTextbox" DockPanel.Dock = "Top" Margin = "0,0,0,0" Text = "New Project" KeyDown = "ChangeProjectName" Padding = "4"  BorderThickness = "0" />
                // </ Border >

                Border TextBorder = new Border();
                BottomPanel.Children.Add(TextBorder);
                TextBorder.CornerRadius = new CornerRadius(8);
                TextBorder.BorderBrush = Brushes.Black;
                TextBorder.BorderThickness = new Thickness(1.5);
                TextBorder.Padding = new Thickness(2);
                TextBorder.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF18191B"));

                TextBox Textbox = new TextBox();
                TextBorder.Child = Textbox;
                Textbox.Padding = new Thickness(4);
                Textbox.BorderThickness = new Thickness(0);
                Textbox.Margin = new Thickness(0);
                DockPanel.SetDock(Textbox, Dock.Left);
                Textbox.IsEnabled = false;
                foreach (ProjectEventResource ProjectEventData in UserProject.ProjectEventResources) //Copy 3
                {
                    if (WorkshopEventResource.Key == ProjectEventData.Key)
                    {
                        Textbox.Text = ProjectEventData.Location;
                        TextBorder.ToolTip = ProjectEventData.Location;
                    }

                }


                OpenButton.Click += (sender, e) =>
                {
                    if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.File && WorkshopEventResource.IsChild == false)
                    {
                        LibraryGES.OpenFileFolder(Textbox.Text);
                    }
                    else if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.Folder && WorkshopEventResource.IsChild == false)
                    {
                        LibraryGES.OpenFolder(Textbox.Text);
                    }
                };

                Label MissingLabel = new();
                TopPanel.Children.Add(MissingLabel);
                MissingLabel.Content = "Location Error!";
                MissingLabel.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF440A0A"));
                MissingLabel.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFF1800"));
                MissingLabel.Padding = new Thickness(3, 1, 3, 3);
                MissingLabel.BorderThickness = new Thickness(0);
                MissingLabel.Height = 25;
                MissingLabel.Margin = new Thickness(10, 0, 0, 0);
                MissingLabel.Visibility = Visibility.Collapsed;
                DockPanel.SetDock(Textbox, Dock.Left);
                {
                    //This all deals with making it clear to the user that resources are not set properly. 
                    if (Textbox.Text != "" && !File.Exists(Textbox.Text) && WorkshopEventResource.ResourceType == EventResource.ResourceTypes.File && WorkshopEventResource.IsChild == false) //If file does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                    if (Textbox.Text != "" && !Directory.Exists(Textbox.Text) && WorkshopEventResource.ResourceType == EventResource.ResourceTypes.Folder && WorkshopEventResource.IsChild == false) //If folder does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                    string finalPart = Path.GetFileName(Textbox.Text);
                    if (Textbox.Text != "" && File.Exists(Textbox.Text) && WorkshopEventResource.RequiredName == true && finalPart != WorkshopEventResource.Location && WorkshopEventResource.ResourceType == EventResource.ResourceTypes.File && WorkshopEventResource.IsChild == false) //If file does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                    if (Textbox.Text != "" && Directory.Exists(Textbox.Text) && WorkshopEventResource.RequiredName == true && finalPart != WorkshopEventResource.Location && WorkshopEventResource.ResourceType == EventResource.ResourceTypes.Folder && WorkshopEventResource.IsChild == false) //If folder does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                }



                BrowseButton.Click += (sender, e) =>
                {
                    string TheString = "";

                    if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.File && WorkshopEventResource.IsChild == false)
                    { TheString = LibraryGES.GetSelectedFilePath("Select a File"); }  //TYPE IF
                    if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.Folder && WorkshopEventResource.IsChild == false)
                    { TheString = LibraryGES.GetSelectedFolderPath("Select a Folder"); }  //TYPE IF



                    if (TheString != null && TheString != "")
                    {
                        if (WorkshopEventResource.RequiredName == true)
                        {
                            if (Path.GetFileName(TheString) == WorkshopEventResource.Location)
                            {
                                Textbox.Text = TheString;


                                foreach (ProjectEventResource ProjectEventResource in UserProject.ProjectEventResources) //Copy 1
                                {
                                    if (WorkshopEventResource.Key == ProjectEventResource.Key)
                                    {
                                        ProjectEventResource.Location = TheString;
                                        MissingLabel.Visibility = Visibility.Collapsed;

                                        CommandMethodsClass.SaveProjectXML(UserProject, WorkshopData);
                                    }

                                }


                            }
                            else
                            {
                                PixelWPF.LibraryPixel.NotificationNegative("Wrong File/Folder Selected", "This resource is set to require a specific name." +
                                    "\n\nRequired Name:" +
                                    "\n" + WorkshopEventResource.Location);
                                //MessageBox.Show("You selected the wrong resource." +
                                //    "\nSometimes a resource can require an exact matching name, this is one of those times." +
                                //"\nYou must " + TheString + " with the name " + WorkshopEventResource.RequiredName, "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            
                            foreach (ProjectEventResource ProjectEventResource in UserProject.ProjectEventResources) //Copy 2
                            {
                                if (WorkshopEventResource.Key == ProjectEventResource.Key)
                                {
                                    Textbox.Text = TheString;

                                    ProjectEventResource.Location = TheString;
                                    MissingLabel.Visibility = Visibility.Collapsed;
                                    CommandMethodsClass.SaveProjectXML(UserProject, WorkshopData);

                                    break;
                                }

                            }
                        }


                    }

                };

            }
        }

        private void OpenProjectFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            //MethodData.GameLibrary = this;
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenSelectedProjectFolder(MethodData);
        }        

        private void DeleteProject(object sender, RoutedEventArgs e)
        {
            //if (ProjectsSelector.SelectedIndex < 0)
            //    return;

            //var result = MessageBox.Show(
            //    "Are you sure you want to delete this project?\nThis action cannot be undone.",
            //    "Confirm Delete",
            //    MessageBoxButton.YesNo,
            //    MessageBoxImage.Warning
            //);

            //if (result != MessageBoxResult.Yes)
            //    return;

            //Project UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];

            //Directory.Delete(
            //    Path.Combine(LibraryGES.ApplicationLocation, "Projects", SelectedWorkshop.WorkshopName, UserProject.ProjectName),
            //    true
            //);

            //SelectedWorkshop.ProjectsList.RemoveAt(ProjectsSelector.SelectedIndex);

            //CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();

            //PixelWPF.LibraryPixel.Notification("Project Deleted",
            //    "Just a reminder that even when a project is deleted, the output folder the files were being saved to will still exist."
            //);
        }

        private void OpenProjectInputFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenSelectedProjectInputFolder(MethodData);
        }

        private void OpenProjectOutputFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenSelectedProjectOutputFolder(MethodData);
        }

        private void ProjectRowDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ProjectSelected(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NewProjectButton(object sender, RoutedEventArgs e)
        {
            string NewProjectName = "New Project";


            {   //This chunk makes sure a new project will never have the same name as an existing project. Inelegant, but it works.
                int i = 0; 
                int goal = 1; 
                for (; i < goal; i++)
                {
                    foreach (Project Aproject in WorkshopData.ProjectsList)
                    {
                        if (Aproject.ProjectName == NewProjectName)
                        {
                            NewProjectName = NewProjectName + "2";
                            goal++;
                        }
                    }
                }
            }
            

            string TheProjectFolder = LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + NewProjectName + "\\";

            if (Directory.Exists(TheProjectFolder)) //One last failsafe just incase. 
            {
                return;
            }

            Directory.CreateDirectory(TheProjectFolder);

            Project NewProject = new();
            NewProject.ProjectName = NewProjectName;
            WorkshopData.ProjectsList.Add(NewProject); //Add to the list of projects for this workshop.
            NewProject.CreatedDate = DateTime.Now.ToString("MMM dd yyyy");
            NewProject.CreatedVersion = LibraryGES.VersionNumber;

            foreach (EventResource EventResource in WorkshopData.WorkshopEventResources) 
            {
                ProjectEventResource NewProjectEventData = new();
                NewProjectEventData.Key = EventResource.Key;
                NewProject.ProjectEventResources.Add(NewProjectEventData);
            }


            CommandMethodsClass.SaveProjectXML(NewProject, WorkshopData);

            RefreshProjectsTreeView();
            
        }

        
    }
}
