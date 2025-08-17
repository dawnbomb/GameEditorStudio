using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Tracing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
//using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Windows.System.Preview;
using static Microsoft.IO.RecyclableMemoryStreamManager;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Path = System.IO.Path;
//using System.Windows.Shapes;

namespace GameEditorStudio
{
    //Oddly only after installing arm64. x64, and x86 could i load this project, or even make a new one. WTF?   https://dotnet.microsoft.com/en-us/download/dotnet/8.0

    // This is the "start" of the program. Stuff here is kind of unorganized, but it's not that bad.
    // This file doesn't really interact with other files for the most part.
    // Click the wiki launches the Tutorial.xaml in the Tutorial folder. It's a wiki of everything to do with crystal editor and related, and teach stuff. 
    // the wiki is extremely under-developed, and should be probably entirely overhauled. The wiki does not interact with other files. (other then in the tutorial folder) (i should rename folder to wiki...)

    // When this program starts, it scans the workshops folder for every folder name. The list of workshops is just the direct folder names.
    // Clicking a workshop loads information from that workshop folder / LibraryInfo.xml. This window entirely handles saving and loading info for that file.
    // Clicking a workshop also loads info from Projects/WorkshopName/ for every folder inside, loads ProjectInfo into a list onscreen. Info in ProjectInfo.xml is used for this screen, NOT the actual project / workshop. 
    // Clicking to launch a project, opens workshop.xaml.cs and is the main part of the program, and extremely unorganized and messy.


    //disabled a method in setup text editor

    //OLD PUBLISH METHOD
    //1 open command prompt
    //2 navigate to the .csproj file folder  FOR EXAMPLE:  cd D:\Crystal Studio
    //3 copy paste this and hit enter
    //dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true 

    //NEW ONE
    //now i have, in Game Editor Studio.csproj, if you open it in notepad, there are these lines that publish in mostly one file. From there, just take the exe and ignore the rest. No more command prompt, hell yes!
    //<!-- Publish settings -->
    //<PublishSingleFile>true</PublishSingleFile>
    //<PublishTrimmed>false</PublishTrimmed> <!-- Leave off unless you're sure -->
    //<SelfContained>true</SelfContained>
    //<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
    //<RuntimeIdentifier>win-x64</RuntimeIdentifier> <!-- Or win-x86 if needed -->
    //<DebugType>none</DebugType>
    //<DebugSymbols>false</DebugSymbols>
    //These last two debug lines, remove the Game Editor Studio.pdb from publishing, which is a debugging file. Hopefully unnecessary for release versions.

    public partial class GameLibrary : Window
    {        
        public WorkshopData? SelectedWorkshop { get; set; }
        public TopMenu MainMenu { get; set; }

        //Order of operations is...
        //1: Window Initalize
        //2: User Control Initalize
        //3: Window.Loaded
        //4: User Control.Loaded

        public GameLibrary()
        {
            InitializeComponent();   
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Title = "Game Editor Studio     Version: " + LibraryGES.VersionNumber + "   ( " + LibraryGES.VersionDate + " )";
            LibraryGES.ApplicationLocation = AppDomain.CurrentDomain.BaseDirectory;
            Database.GameLibrary = this; 

            #if DEBUG
            LibraryGES.ApplicationLocation = "D:\\Game Editor Studio"; //Where the .exe is supposed to be.                     
            #endif

            LoadDatabase LoadDatabase = new(); //Must happen before Setup Commands, because commands use tools.   
            LoadDatabase.LoadThemes(); //Loads from Other/Themes - A theme is a list of colors for the UI. Users can create their own color themes. 
            LoadDatabase.LoadToolsList(); //Loads from Other/Tools.xml.            
            LoadDatabase.LoadCommandsList(this); //Loads from Other/Commands.xml.            
            LoadDatabase.LoadCommonEventsList(); //Loads from Other/Common Events.xml.   
            //NOTE: LoadCommonEventsForWorkshop happens when the tools menu itself is opened,
            //as i don't currently support pre-loading every workshops data from the library, but common events are still for the "CURRENT" workshop.
            LoadDatabase.LoadToolLocations(); //Load user's last known tool locations.
            LoadDatabase.LoadEnabledCommonEvents(); //Loads from Settings/Common Events.xml the user's enabled common events.
            LoadDatabase.LoadWorkshops(); //Events are loaded here. - - -  Does not fully load the workshops, that happens when one is launched. 
            
            RefreshWorkshopTree();

            

            Dispatcher.InvokeAsync(async () => await PixelWPF.GithubUpdater.CheckForUpdatesAsync("GameEditorStudio", "dawnbomb/GameEditorStudio/releases/latest", LibraryGES.VersionNumber));

            TermsAndConditions tos = new();
            LibraryGrid.Children.Add(tos);
            Grid.SetRowSpan(tos, 15);
            Grid.SetColumnSpan(tos, 15);

            this.Topmost = true;    // Temporarily set topmost to ensure visibility
            this.Activate();        // Try to bring to foreground
            this.Focus();           // Set keyboard focus
            this.Topmost = false;   // Reset topmost if undesired permanently

        }

        



        
        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
             LaunchWorkshop();
        }
        

        
        public void RefreshWorkshopTree()
        {
            LibraryTreeOfWorkshops.Items.Clear();

            if (!Directory.Exists(LibraryGES.ApplicationLocation + "\\Workshops")) 
            {
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops");

                MessageBox.Show("The Workshops folder did not exist. A new one was created.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (Directory.GetDirectories(LibraryGES.ApplicationLocation + "\\Workshops").Length == 0)
            {
                MessageBox.Show("There seems to be no workshops in the workshops folder. This should never happen unless you manually deleted them all. It's strongly recommended you go find the latest workshops list.", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            foreach (WorkshopData workshopData in Database.Workshops) 
            {
                TreeViewItem treeItem = new TreeViewItem { Header = workshopData.WorkshopName };
                treeItem.Tag = workshopData; // Store the WorkshopData in the Tag property for later use

                // Create context menu for this tree item
                ContextMenu contextMenu = new ContextMenu();
                treeItem.ContextMenu = contextMenu;

                MenuItem option1 = new MenuItem { Header = "New Workshop" };
                option1.Click += ButtonCreateWorkshop2; // You can add click event handlers here
                contextMenu.Items.Add(option1);

                MenuItem option4 = new MenuItem { Header = "Open Workshop Folder" };
                option4.Click += OpenWorkshopFolder;
                contextMenu.Items.Add(option4);

                MenuItem option2 = new MenuItem { Header = "Edit Workshop" };
                option2.Click += ButtonEditWorkshop2;
                contextMenu.Items.Add(option2);

                MenuItem option3 = new MenuItem { Header = "Delete Workshop" };
                option3.Click += DeleteWorkshop;
                contextMenu.Items.Add(option3);

                

                treeItem.MouseRightButtonDown += (s, e) =>
                {
                    treeItem.IsSelected = true; //If i ever remove this, make sure all right click options function properly (Especially Open Workshop Folder as that invokes a CommandMethod.)
                };


                LibraryTreeOfWorkshops.Items.Add(treeItem);
            }

            

            
        }
       
        

        private void LibraryTreeOfWorkshops_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            EditorsTree.Items.Clear();
            LibraryDocumentsTree.Items.Clear();
            ProjectNameTextbox.Text = "";
            TextBoxInputDirectory.Text = "You must select something to launch the workshop.";
            TextBoxOutputDirectory.Text = "If not set, defaults to the Input Directory.";


            if (LibraryTreeOfWorkshops.SelectedItem == null)
            {
                SelectedWorkshop = null;
                return;
            }

            TreeViewItem treeItem = LibraryTreeOfWorkshops.SelectedItem as TreeViewItem;
            SelectedWorkshop = treeItem.Tag as WorkshopData;

            ProjectsSelector.ItemsSource = SelectedWorkshop.ProjectsList; // Bind the collection to the ItemsSource property of the DataGrid control   
            CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();  

            if (ProjectsSelector.Items.Count > 0)
            {
                ProjectsSelector.SelectedItem = null;
                // Select the first item
                ProjectsSelector.SelectedItem = ProjectsSelector.Items[0];

                // Optionally, scroll the selected item into view
                ProjectsSelector.ScrollIntoView(ProjectsSelector.SelectedItem);
            }


            ButtonSelectInputDirectory.ToolTip = "This workshop does not require a specific name for it's input folder. \nCheck the readme document for info on what the input folder is supposed to be.";
            if (SelectedWorkshop.ProjectsRequireSameFolderName == true)
            {
                ButtonSelectInputDirectory.ToolTip = "This workshop is looking for a folder by the name of...\n" + SelectedWorkshop.WorkshopInputDirectory;
            }



            { //Documents and Editors

                //WorkshopInfoDocuments.Content = "Documents: " + Convert.ToString(System.IO.Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Documentation", "*", SearchOption.TopDirectoryOnly).Count());

                string documentationPath = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", SelectedWorkshop.WorkshopName, "Documents");
                string[] folderPaths = Directory.GetDirectories(documentationPath);
                foreach (string folderPath in folderPaths)
                {
                    TreeViewItem item = new();
                    item.Header = new DirectoryInfo(folderPath).Name;
                    item.Tag = System.IO.File.ReadAllText(folderPath + "\\Text.txt");
                    LibraryDocumentsTree.Items.Add(item);

                    string headerText = (item.Header as string)?.Replace(" ", "").ToLower(); //Check if a readme exists, but ignore caps and spaces, so it will always find it.
                    if (headerText == "readme")
                    {
                        item.IsSelected = true;
                    }
                }



                string WorkshopEditorsFolder = LibraryGES.ApplicationLocation + "\\Workshops\\" + SelectedWorkshop.WorkshopName + "\\Editors\\";
                string[] EditorFoldersList = Directory.GetDirectories(WorkshopEditorsFolder);

                // Create TreeViewItems for each folder and add them to the EditorsTree
                foreach (string folder in EditorFoldersList)
                {
                    string folderName = new DirectoryInfo(folder).Name;
                    TreeViewItem folderItem = new TreeViewItem { Header = folderName };
                    EditorsTree.Items.Add(folderItem);
                }
            }

        }

        

        private void ButtonLaunchWorkshop_Click(object sender, RoutedEventArgs e)
        {
            LaunchWorkshop();
        }

        private void LaunchWorkshop() 
        {
            if (SelectedWorkshop.ProjectDataItem != null || SelectedWorkshop.WorkshopXaml != null) 
            {
                PixelWPF.LibraryPixel.NotificationNegative("Sorry - Please restart Game Editor Studio D;",
                        "As part of adding an upcoming feature to let users select a project AFTER a workshop is loaded and swap between them, i added a crash that happens if you try to open a workshop you previously opened. " +
                        "It happened because i got sidetracked and never finished adding the new feature. " +
                        "\n\nAnyway if you restart GES it will be fine. I'll finish adding the feature sometime in the next 2-3 months, as a huge code rewrite is required. But it would make creating multiple mods SUPER easy so i'm not backing down! "
                        );
                return;
            }

            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }
            ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];

            if (!Directory.Exists(UserProject.ProjectInputDirectory)) 
            {
                PixelWPF.LibraryPixel.Notification("Huh?",
                    "It seems the input folder for this project ... doesn't exist?"
                    );   
                return;
            }
            if (!Directory.Exists(UserProject.ProjectOutputDirectory))
            {
                PixelWPF.LibraryPixel.Notification("Huh?",
                    "It seems the output folder for this project ... doesn't exist?"
                    );
                return;
            }

            Workshop TheWorkshop = new Workshop(SelectedWorkshop, UserProject); //Thing One, the workshop

            DependencyObject parent = this;
            while (parent != null && !(parent is Window))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            Window currentWindow = parent as Window;
            if (currentWindow != null)
            {
                // Set the position of the new window to the position of the current window
                TheWorkshop.Left = currentWindow.Left;
                TheWorkshop.Top = currentWindow.Top + 30;
            }

            TheWorkshop.Show();

            //Properties.Settings.Default.LastWorkshop = WorkshopName; //Set the workshop name in settings, so it can be used by other parts of the program. 
        }

        private void LaunchWorkshopPreviewMode(object sender, RoutedEventArgs e)
        {            
            bool IsPreviewModeActive = true;

            Workshop TheWorkshop = new Workshop(SelectedWorkshop, null, IsPreviewModeActive); //Thing One, the workshop

            DependencyObject parent = this;
            while (parent != null && !(parent is Window))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            Window currentWindow = parent as Window;
            if (currentWindow != null)
            {
                // Set the position of the new window to the position of the current window
                TheWorkshop.Left = currentWindow.Left;
                TheWorkshop.Top = currentWindow.Top;
            }

            TheWorkshop.Show();

            //LoadEditor.CreateEditor(f2); 
            //If auto mode, run load auto mode
            //this.Close();
        }

















        private void ButtonCreateWorkshop2(object sender, RoutedEventArgs e)
        {
            WorkshopData newworkshopdata = new();
            WorkshopMaker TheUserControl = new("New", newworkshopdata);

            Grid.SetRow(TheUserControl, 2);
            Grid.SetColumn(TheUserControl, 1);
            Grid.SetRowSpan(TheUserControl, 3);
            Grid.SetColumnSpan(TheUserControl, 5);
            LibraryGrid.Children.Add(TheUserControl);
        }

        private void ButtonEditWorkshop2(object sender, RoutedEventArgs e)
        {
            if (SelectedWorkshop == null) { return; }

            WorkshopMaker TheUserControl = new("Edit", SelectedWorkshop);

            //WorkshopInfoGrid.Children.Clear();
            //WorkshopInfoGrid.Children.Add(TheUserControl);
            //MainTabControl.SelectedItem = TabWorkshopMaker;

            Grid.SetRow(TheUserControl, 2);
            Grid.SetColumn(TheUserControl, 1);
            Grid.SetRowSpan(TheUserControl, 3);
            Grid.SetColumnSpan(TheUserControl, 5);
            LibraryGrid.Children.Add(TheUserControl);
        }

        private void DeleteWorkshop(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This doesn't work yet, sorry! For now, just go to the workshops folder and delete the one you want. This is a low priority feature, will be added...eventually.");
        }

       

        
                

         
        
                
               


        private void ProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            LabelForMissingProjectInput.Visibility = Visibility.Collapsed;
            LabelForMissingProjectOutput.Visibility = Visibility.Collapsed;

            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null || SelectedWorkshop == null)
            {
                ProjectNameTextbox.Text = "";
                TextBoxInputDirectory.Text = "";
                TextBoxOutputDirectory.Text = "";
                RefreshProjectEventResourcesUI();
                return;
            }


            
            ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
            MainMenu.ProjectDataItem = UserProject;

            ProjectNameTextbox.Text = UserProject.ProjectName;
            TextBoxInputDirectory.Text = UserProject.ProjectInputDirectory;
            TextBoxOutputDirectory.Text = UserProject.ProjectOutputDirectory;

            ButtonOpenInputFolder.ToolTip = UserProject.ProjectInputDirectory;
            ButtonOpenOutputFolder.ToolTip = UserProject.ProjectOutputDirectory;
            BorderInputFolder.ToolTip = UserProject.ProjectInputDirectory;
            BorderOutputFolder.ToolTip = UserProject.ProjectOutputDirectory;
            

            if (TextBoxInputDirectory.Text == "") 
            {
                TextBoxInputDirectory.Text = "Where new projects read files from. :)";
                ButtonOpenInputFolder.ToolTip = null;
                BorderInputFolder.ToolTip = null;
            }
            if (TextBoxOutputDirectory.Text == "") 
            {
                TextBoxOutputDirectory.Text = "Where files will be saved to. :)";
                ButtonOpenOutputFolder.ToolTip = null;
                BorderOutputFolder.ToolTip = null;
            }

            
            if (UserProject.ProjectOutputDirectory != "" && !Directory.Exists(TextBoxOutputDirectory.Text))
            {
                LabelForMissingProjectOutput.Visibility = Visibility.Visible;
            }
            if (UserProject.ProjectInputDirectory != "" && !Directory.Exists(TextBoxInputDirectory.Text))
            {
                LabelForMissingProjectInput.Visibility = Visibility.Visible;
            }



            RefreshProjectEventResourcesUI();

        }

        public void RefreshProjectEventResourcesUI() 
        {
            LabelForMissingProjectResources.Visibility = Visibility.Collapsed;
            ProjectEventResourcesPanel.Children.Clear();

            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null || SelectedWorkshop == null)
            {                
                return;
            }
            ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
            if (UserProject == null) 
            {
                return;
            }

            foreach (EventResource WorkshopEventResource in SelectedWorkshop.WorkshopEventResources)
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

                DockPanel TopPanel = new();
                DockPanel.SetDock(TopPanel, Dock.Top);
                MainPanel.Children.Add(TopPanel);
                TopPanel.LastChildFill = false;

                DockPanel BottomPanel = new();
                DockPanel.SetDock(BottomPanel, Dock.Top);
                MainPanel.Children.Add(BottomPanel);

                

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
                BrowseButton.Margin = new Thickness(0,0,4,0);
                BrowseButton.Content = "Browse...";

                

                //< Border CornerRadius = "8" BorderBrush = "Black"  BorderThickness = "1.5" Padding = "2" Background = "#FF18191B" >  < !--Background = "White"-- >
                //        < TextBox x: Name = "ProjectNameTextbox" DockPanel.Dock = "Top" Margin = "0,0,0,0" Text = "New Project" KeyDown = "ChangeProjectName" Padding = "4"  BorderThickness = "0" />
                // </ Border >

                Border TextBorder = new Border();
                BottomPanel.Children.Add(TextBorder);
                TextBorder.CornerRadius = new CornerRadius(8);
                TextBorder.BorderBrush = Brushes.Black ;
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
                MissingLabel.Padding = new Thickness(3,1,3,3);
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

                                        CommandMethodsClass.SaveProjectXML(UserProject, SelectedWorkshop);
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
                            Textbox.Text = TheString;
                            foreach (ProjectEventResource ProjectEventResource in UserProject.ProjectEventResources) //Copy 2
                            {
                                if (WorkshopEventResource.Key == ProjectEventResource.Key)
                                {
                                    ProjectEventResource.Location = TheString;
                                    MissingLabel.Visibility = Visibility.Collapsed;
                                    CommandMethodsClass.SaveProjectXML(UserProject, SelectedWorkshop);
                                }

                            }
                        }


                    }

                };

            }
        }

        private void CreateNewProject(object sender, RoutedEventArgs e) //THE NEW PROJECT sdkjfsdjfklsdjfklsjfklsjfklsjkflsjklfjklfsdjklfsjlfkjfklsda
        {            
            string TheProjectFolder = LibraryGES.ApplicationLocation + "\\Projects\\" + SelectedWorkshop.WorkshopName + "\\" + "New Project" + "\\";
            if (Directory.Exists(TheProjectFolder) || LibraryTreeOfWorkshops.SelectedItem == null )
            {
                return;
            }            
                                    
            Directory.CreateDirectory(TheProjectFolder);

            ProjectData NewDataItem = new();
            SelectedWorkshop.ProjectsList.Add(NewDataItem); //Add to the list of projects for this workshop.
            CommandMethodsClass.SaveProjectXML(NewDataItem, SelectedWorkshop);       

            CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();
            ProjectsSelector.SelectedItem = NewDataItem;
        }
        


        //=================Button inputs==================

        

        private void ChangeProjectName(object sender, KeyEventArgs e)
        {   
            if (e.Key == Key.Enter)
            {                

                if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null|| ProjectNameTextbox.Text == "")   {return;}

                ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
                string oldFolderPath = LibraryGES.ApplicationLocation + "\\Projects\\" + SelectedWorkshop.WorkshopName + "\\" + UserProject.ProjectName;
                string newFolderPath = LibraryGES.ApplicationLocation + "\\Projects\\" + SelectedWorkshop.WorkshopName + "\\" + ProjectNameTextbox.Text;

                if (oldFolderPath == newFolderPath) {return;}

                Directory.Move(oldFolderPath, newFolderPath);// Rename the folder at the old path to the new path

                UserProject.ProjectName = ProjectNameTextbox.Text;

                CommandMethodsClass.SaveProjectXML(UserProject, SelectedWorkshop);




                TreeViewItem selectedItem = LibraryTreeOfWorkshops.ItemContainerGenerator.ContainerFromItem(LibraryTreeOfWorkshops.SelectedItem) as TreeViewItem;
                if (selectedItem != null) //This stuff makes it so the data grid updates.
                {
                    selectedItem.IsSelected = false;
                    selectedItem.IsSelected = true;
                }
                               

                foreach (var item in ProjectsSelector.Items)
                {
                    if (item is ProjectData dataItem && dataItem.ProjectName.Equals(ProjectNameTextbox.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        // Found the project, select the row
                        ProjectsSelector.SelectedItem = item;
                        ProjectsSelector.ScrollIntoView(item); // Optional: Scroll to the item if it's not visible
                        break; // Stop the loop as we found the item
                    }
                }
            }
            

        }

        private void ButtonSelectInputDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }

            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog();//This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select the folder named " + SelectedWorkshop.WorkshopInputDirectory; //This sets a description to help remind the user what their looking for.
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
            
            if ((bool)FolderSelect.ShowDialog(this)) //This triggers the folder selection screen, and if the user does not cancel out...
            {

                //if (System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\" +  WorkshopInputDirectory) == Path.GetFileName(FolderSelect.SelectedPath))
                if (SelectedWorkshop.ProjectsRequireSameFolderName == false)
                {
                    PixelWPF.LibraryPixel.NotificationPositive("You MAYBE selected the correct folder?",
                        "This workshop doesn't require a specific folder name to be selected. " +
                        "This is usually set when the input folder is one that users commonly want to be able to rename. " +
                        "\n\n" +
                        "If your not sure, I STRONGLY recommend checking the readme, as well as asking around. Well, i mean, you'll know if something is wrong if you launch your project and get a ton of errors. >_>;"
                    );

                    ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
                    UserProject.ProjectInputDirectory = FolderSelect.SelectedPath;
                    TextBoxInputDirectory.Text = FolderSelect.SelectedPath;

                    CommandMethodsClass.SaveProjectXML(UserProject, SelectedWorkshop);

                }
                else if (SelectedWorkshop.WorkshopInputDirectory == Path.GetFileName(FolderSelect.SelectedPath) && SelectedWorkshop.ProjectsRequireSameFolderName == true)
                {
                    PixelWPF.LibraryPixel.NotificationPositive("You selected the correct folder!",
                        "The folder name you selected is the same as the one this workshop is looking for. " +
                        "This can only be wrong if you selected a folder with the exact same name, but a diffrent location."
                    );

                    ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
                    UserProject.ProjectInputDirectory = FolderSelect.SelectedPath;
                    TextBoxInputDirectory.Text = FolderSelect.SelectedPath;

                    CommandMethodsClass.SaveProjectXML(UserProject, SelectedWorkshop);


                }
                else
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Wrong folder selected!",
                        "This workshop is looking for you to select a folder named \"" + SelectedWorkshop.WorkshopInputDirectory + "\"." +
                        "\n\n" +
                        "If your confused, check the README, or see if there are any helpful discords.");


                }




            }
        }

        private void ButtonSelectOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }


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
            if ((bool)FolderSelect.ShowDialog(this)) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
                UserProject.ProjectOutputDirectory = FolderSelect.SelectedPath;
                TextBoxOutputDirectory.Text = FolderSelect.SelectedPath;

                CommandMethodsClass.SaveProjectXML(UserProject, SelectedWorkshop);


            }


        }



        

        

        

        private void DocumentsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem Item = LibraryDocumentsTree.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            TextBoxWorkshopReadMe.Text = Item.Tag as string;
            DocumentNameLabel.Content = Item.Header as string;
        }
        private void OpenProjectFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = this;
            MethodData.WorkshopData = SelectedWorkshop;
            CommandMethodsClass.OpenProjectFolder(MethodData);
        }

        private void OpenInput(object sender, RoutedEventArgs e)
        {

            MethodData MethodData = new();
            MethodData.GameLibrary = this;
            CommandMethodsClass.OpenInputFolder(MethodData);

        }

        private void OpenOutput(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = this;
            CommandMethodsClass.OpenOutputFolder(MethodData);

        }

        private void DeleteProject(object sender, RoutedEventArgs e)
        {
            if (ProjectsSelector.SelectedIndex < 0)
                return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this project?\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            ProjectData UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];

            Directory.Delete(
                Path.Combine(LibraryGES.ApplicationLocation, "Projects", SelectedWorkshop.WorkshopName, UserProject.ProjectName),
                true
            );

            SelectedWorkshop.ProjectsList.RemoveAt(ProjectsSelector.SelectedIndex);

            CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();

            PixelWPF.LibraryPixel.Notification("Project Deleted",
                "Just a reminder that even when a project is deleted, the output folder the files were being saved to will still exist."
            );
        }

        private void OpenWorkshopFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = this;
            CommandMethodsClass.OpenWorkshopFolder(MethodData);

        }

        
    }

    public class LocationToColorConverter : IValueConverter //Ignore this, used for binding the color of a tool in the general or workshop tools menus.
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var location = value as string;
            if (string.IsNullOrEmpty(location))
            {
                return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
