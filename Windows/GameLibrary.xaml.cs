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

        public string WorkshopName { get; set; } //The name of the workshop (IE name of whats selected in Game Library)
        public string WorkshopInputDirectory { get; set; } //The intended InputDirectory (Folder name) for modding this game. This helps make sure end users aren't guessing what the correct one is.
        public bool WorkshopProjectsRequireSameFolderName { get; set; } = true; //If true, the project input folder must be the same name as the project. If false, it can be anything.
        public List<WorkshopResource> WorkshopEventResources { get; set; } = new(); 


        public List<ProjectDataItem> Projects { get; set; }



        public MainMenu MainMenu { get; set; }

        //Order of operations is...
        //1: Window Initalize
        //2: User Control Initalize
        //3: Window.Loaded
        //4: User Control.Loaded

        public GameLibrary()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Title = "Game Editor Studio     Version: " + LibraryMan.VersionNumber + "   ( " + LibraryMan.VersionDate + " )"; 

            string basepath = AppDomain.CurrentDomain.BaseDirectory;

            #if DEBUG //The App Location is the folder location of the executable. It's used by other parts of the program to find files.
            //I don't understand (and don't feel like spending the time) setting it up so release is the same as debug mode, so this is a temporary lazy fix.
            LibraryMan.ApplicationLocation = Path.GetFullPath(Path.Combine(basepath, @"..\..\..\..\..\Release"));
            #else
            //ExePath = basepath; //for published versions of the program to the public.
            LibraryMan.ApplicationLocation = basepath;
            //LibraryMan.ApplicationLocation = Path.GetFullPath(Path.Combine(basepath, @"..\..\..\..\Release"));

            //LibraryMan.ApplicationLocation = "D:\\Game Editor Studio\\Release";            
            #endif

            //string test = LibraryMan.ApplicationLocation;

            ImportFromGoogle TableImport = new(); //Must happen before Setup Commands, because commands use tools.
            TableImport.ImportTableFromGoogle(this);   //Imports tools, commands, and common events.
            TableImport.ImportSettings();



            ScanForWorkshops();



            Projects = new();
            ProjectsSelector.ItemsSource = Projects; // Bind the collection to the ItemsSource property of the DataGrid control           


            CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();
                        

            //Yami rpg theme


            LoadThemes();

            
            
        }

        public void LoadThemes()
        {
            //LibraryMan.ColorThemeList.Clear();
            string themesDirectory = Path.Combine(LibraryMan.ApplicationLocation, "Other\\Themes");

            if (Directory.Exists(themesDirectory))
            {
                foreach (string themeFile in Directory.GetFiles(themesDirectory, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    XElement themeXml = XElement.Load(themeFile);
                    ColorTheme theme = new ColorTheme
                    {
                        Name = (string)themeXml.Element("Name")
                    };

                    foreach (XElement xmlElement in themeXml.Descendants("Element"))
                    {
                        string elementName = (string)xmlElement.Element("Name");
                        string text = (string)xmlElement.Element("Text");
                        string back = (string)xmlElement.Element("Back");
                        string border = (string)xmlElement.Element("Border");

                        // Find the corresponding element in the theme by name
                        Element themeElement = theme.ElementList.FirstOrDefault(e => e.Name == elementName);
                        if (themeElement != null)
                        {
                            themeElement.Text = text;
                            themeElement.Back = back;
                            themeElement.Border = border;
                        }
                    }

                    LibraryMan.ColorThemeList.Add(theme);
                }
            }
            else
            {
                Console.WriteLine("No themes directory found.");
            }

            try
            {
                ColorTheme LastTheme = LibraryMan.ColorThemeList.FirstOrDefault(e => e.Name == "Asperite");
                LibraryMan.SwitchToColorTheme(LastTheme);
            }
            catch 
            {
            
            }
            
        }



        
        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
             LaunchWorkshop();
        }
        

        
        public void ScanForWorkshops()
        {
            LibraryTreeOfWorkshops.Items.Clear();

            if (!Directory.Exists(LibraryMan.ApplicationLocation + "\\Workshops")) 
            {
                Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops");

                MessageBox.Show("The Workshops folder did not exist. A new one was created.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops").Length == 0)
            {
                MessageBox.Show("There seems to be no workshops in the workshops folder. This should never happen unless you manually deleted them all. It's strongly recommended you go find the latest workshops list.", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (Directory.Exists(LibraryMan.ApplicationLocation + "\\Workshops"))
            {
                string[] allWorkshopsPathsArray = Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
                foreach (var WorkshopPath in allWorkshopsPathsArray)
                {
                    TreeViewItem treeItem = new TreeViewItem { Header = WorkshopPath };

                    // Create context menu for this tree item
                    ContextMenu contextMenu = new ContextMenu();
                    treeItem.ContextMenu = contextMenu;

                    MenuItem option1 = new MenuItem { Header = "New Workshop" };
                    option1.Click += ButtonCreateWorkshop2; // You can add click event handlers here
                    contextMenu.Items.Add(option1);

                    MenuItem option2 = new MenuItem { Header = "Edit Workshop" };
                    option2.Click += ButtonEditWorkshop2;
                    contextMenu.Items.Add(option2);

                    MenuItem option3 = new MenuItem { Header = "Delete Workshop" };
                    option3.Click += DeleteWorkshop;
                    contextMenu.Items.Add(option3);

                    MenuItem option4 = new MenuItem { Header = "Open Workshop Folder" };
                    option4.Click += OpenWorkshopFolder;
                    contextMenu.Items.Add(option4);

                    treeItem.MouseRightButtonDown += (s, e) =>
                    {
                        treeItem.IsSelected = true; // Ensure the item is selected when right-clicked
                        //If i ever remove this, put in work to make sure all the right click options function properly.
                        //Especially Open Workshop Folder, as that invokes a CommandMethod. 
                    };


                    LibraryTreeOfWorkshops.Items.Add(treeItem);
                }
            }
        }
       
        

        private void LibraryTreeOfWorkshops_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            EditorsTree.Items.Clear();
            LibraryDocumentsTree.Items.Clear();
            WorkshopEventResources.Clear();
            MainMenu.Events.Clear();
            Projects.Clear();
            MainMenu.WorkshopName = "";
            ProjectNameTextbox.Text = "";
            TextBoxInputDirectory.Text = "You must select something to launch the workshop.";
            TextBoxOutputDirectory.Text = "If not set, defaults to the Input Directory.";

            if (LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }            

             
            if (LibraryTreeOfWorkshops.SelectedItem is TreeViewItem selectedTreeItem)
            {
                WorkshopName = selectedTreeItem.Header.ToString();
            }

            using (FileStream TargetXML = new FileStream(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Workshop.xml", FileMode.Open, FileAccess.Read))
            {
                XElement xml = XElement.Load(TargetXML);                
                
                WorkshopInputDirectory = xml.Element("InputLocation")?.Value;
                WorkshopProjectsRequireSameFolderName = bool.TryParse(xml.Element("ProjectsRequireSameInputFolderName")?.Value, out bool result) && result;

            }
            ;

            { //Documents and Editors

                //WorkshopInfoDocuments.Content = "Documents: " + Convert.ToString(System.IO.Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Documentation", "*", SearchOption.TopDirectoryOnly).Count());
                
                string documentationPath = Path.Combine(LibraryMan.ApplicationLocation, "Workshops", WorkshopName, "Documents");
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


                
                string WorkshopEditorsFolder = LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Editors\\";
                string[] EditorFoldersList = Directory.GetDirectories(WorkshopEditorsFolder);

                // Create TreeViewItems for each folder and add them to the EditorsTree
                foreach (string folder in EditorFoldersList)
                {
                    string folderName = new DirectoryInfo(folder).Name;
                    TreeViewItem folderItem = new TreeViewItem { Header = folderName };
                    EditorsTree.Items.Add(folderItem);
                }
            }
            


            
            LoadEventResources();
            MainMenu.WorkshopName = WorkshopName;            
            MainMenu.LoadEventsFromXML();
            
            

            string ProjectsFolder = @LibraryMan.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\"; //"\\LibraryBannerArt.png";   
            if (Directory.Exists(ProjectsFolder))
            {
                foreach (string TheProjectFolder in Directory.GetDirectories(ProjectsFolder))
                {

                    using (FileStream fs = new FileStream(TheProjectFolder + "\\Project.xml", FileMode.Open, FileAccess.Read))
                    {
                        XElement xml = XElement.Load(fs);
                        string PName = xml.Element("Name")?.Value;
                        string PInput = xml.Element("InputLocation")?.Value;
                        string POutput = xml.Element("OutputLocation")?.Value;

                        List<ProjectEventResource> ProjectEventResources = new();
                        var xmlEventResources = xml.Element("ResourceList");

                        if (xmlEventResources != null)
                        {
                            //This oIf its empty to begin with, it blanks.
                            

                            foreach (WorkshopResource EventResource in WorkshopEventResources) 
                            {
                                if (EventResource.ResourceType == "RelativeFile" || EventResource.ResourceType == "RelativeFolder") { continue; }

                                ProjectEventResource projectEventData = new ProjectEventResource
                                {
                                    ResourceKey = EventResource.WorkshopResourceKey,
                                };
                                ProjectEventResources.Add(projectEventData);
                            }

                            foreach (XElement xmlEventResource in xmlEventResources.Elements("Resource"))
                            {
                                string resourceKey = xmlEventResource.Element("Key")?.Value;
                                string location = xmlEventResource.Element("Location")?.Value;

                                foreach (ProjectEventResource ProjectResourceData in ProjectEventResources) 
                                {
                                    if (resourceKey == ProjectResourceData.ResourceKey) 
                                    {
                                        ProjectResourceData.Location = location;
                                    }
                                }
                            }
                        }

                        Projects.Add(new ProjectDataItem
                        {
                            ProjectName = PName,
                            ProjectInputDirectory = PInput,
                            ProjectOutputDirectory = POutput,
                            ProjectEventResources = ProjectEventResources
                        });
                    }

                }

                

            }
            else //If no projects. 
            {
            
            }

            CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();
            

            if (ProjectsSelector.Items.Count > 0)
            {
                // Select the first item
                ProjectsSelector.SelectedItem = ProjectsSelector.Items[0];

                // Optionally, scroll the selected item into view
                ProjectsSelector.ScrollIntoView(ProjectsSelector.SelectedItem);
            }


        }

        public void LoadEventResources() 
        {            

            using (FileStream TargetXML = new FileStream(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Workshop.xml", FileMode.Open, FileAccess.Read))
            {
                XElement libraryxml = XElement.Load(TargetXML);

                foreach (var xmlEvent in libraryxml.Descendants("Resource"))
                {
                    WorkshopResource EventResource = new WorkshopResource
                    {
                        Name = xmlEvent.Element("Name")?.Value,
                        Location = xmlEvent.Element("Location")?.Value,
                        RequiredName = bool.TryParse(xmlEvent.Element("RequiredName")?.Value, out var result) ? result : false,
                        WorkshopResourceKey = xmlEvent.Element("Key")?.Value,
                        TargetKey = xmlEvent.Element("TargetKey")?.Value,


                        

                        //ResourceType = xmlEvent.Element("Type")?.Value,

                    };
                    if (xmlEvent.Element("ResourceType")?.Value == "File"   && xmlEvent.Element("PathType")?.Value == "FullPath")    { EventResource.ResourceType = "LocalFile"; }
                    if (xmlEvent.Element("ResourceType")?.Value == "Folder" && xmlEvent.Element("PathType")?.Value == "FullPath")    { EventResource.ResourceType = "LocalFolder"; }
                    if (xmlEvent.Element("ResourceType")?.Value == "File"   && xmlEvent.Element("PathType")?.Value == "PartialPath") { EventResource.ResourceType = "RelativeFile"; }
                    if (xmlEvent.Element("ResourceType")?.Value == "Folder" && xmlEvent.Element("PathType")?.Value == "PartialPath") { EventResource.ResourceType = "RelativeFolder"; }

                    WorkshopEventResources.Add(EventResource);
                }

            };           


        }

        private void ButtonLaunchWorkshop_Click(object sender, RoutedEventArgs e)
        {
            LaunchWorkshop();
        }

        private void LaunchWorkshop() 
        {
            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }
            ProjectDataItem UserProject = Projects[ProjectsSelector.SelectedIndex];

            if (!Directory.Exists(UserProject.ProjectInputDirectory)) 
            {
                LibraryMan.Notification("Huh?",
                    "It seems the input folder for this project ... doesn't exist?"
                    );   
                return;
            }
            if (!Directory.Exists(UserProject.ProjectOutputDirectory))
            {
                LibraryMan.Notification("Huh?",
                    "It seems the output folder for this project ... doesn't exist?"
                    );
                return;
            }

            Workshop TheWorkshop = new Workshop(WorkshopName, UserProject); //Thing One, the workshop

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

            Properties.Settings.Default.LastWorkshop = WorkshopName; //Set the workshop name in settings, so it can be used by other parts of the program. 
            Properties.Settings.Default.Save();
        }

        private void LaunchWorkshopPreviewMode(object sender, RoutedEventArgs e)
        {            
            bool IsPreviewModeActive = true;

            Workshop TheWorkshop = new Workshop(WorkshopName, null, IsPreviewModeActive); //Thing One, the workshop

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
            WorkshopMaker TheUserControl = new("New");

            Grid.SetRow(TheUserControl, 2);
            Grid.SetColumn(TheUserControl, 1);
            Grid.SetRowSpan(TheUserControl, 3);
            Grid.SetColumnSpan(TheUserControl, 5);
            LibraryGrid.Children.Add(TheUserControl);
        }

        private void ButtonEditWorkshop2(object sender, RoutedEventArgs e)
        {            
            WorkshopMaker TheUserControl = new("Edit");

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
            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null)
            {
                ProjectNameTextbox.Text = "";
                TextBoxInputDirectory.Text = "";
                TextBoxOutputDirectory.Text = "";
                GenerateProjectEventResourceUI(null);
                return;
            }


            ProjectDataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
            MainMenu.ProjectDataItem = UserProject;

            ProjectNameTextbox.Text = UserProject.ProjectName;
            TextBoxInputDirectory.Text = UserProject.ProjectInputDirectory;
            TextBoxOutputDirectory.Text = UserProject.ProjectOutputDirectory;

            if (TextBoxInputDirectory.Text == "") 
            {
                TextBoxInputDirectory.Text = "Where new projects read files from. :)";
            }
            if (TextBoxOutputDirectory.Text == "") 
            {
                TextBoxOutputDirectory.Text = "Where files will be saved to. :)";
            }

            
            //UserProject.ProjectEventResources.Clear(); 

            GenerateProjectEventResourceUI(UserProject);
            //MainMenu.ProjectDataItem = U



        }

        public void GenerateProjectEventResourceUI(ProjectDataItem UserProject) 
        {
            LabelForMissingProjectResources.Visibility = Visibility.Collapsed;
            ProjectEventResourcesPanel.Children.Clear();

            if (UserProject == null) 
            {
                return;
            }

            foreach (WorkshopResource WorkshopEventResource in WorkshopEventResources)
            {
                if (WorkshopEventResource.ResourceType == "RelativeFile" || WorkshopEventResource.ResourceType == "RelativeFolder") //TYPE IF
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
                if (WorkshopEventResource.ResourceType == "LocalFile") { Label.Content = "🗎   " + WorkshopEventResource.Name; } //TYPE IF
                if (WorkshopEventResource.ResourceType == "LocalFolder") { Label.Content = "📁 " + WorkshopEventResource.Name; }  //TYPE IF                               

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
                    if (WorkshopEventResource.WorkshopResourceKey == ProjectEventData.ResourceKey)
                    {
                        Textbox.Text = ProjectEventData.Location;
                    }

                }
                                

                OpenButton.Click += (sender, e) =>
                {
                    if (WorkshopEventResource.ResourceType == "LocalFile")
                    {
                        LibraryMan.OpenFileFolder(Textbox.Text);
                    }
                    else if (WorkshopEventResource.ResourceType == "LocalFolder") 
                    {
                        LibraryMan.OpenFolder(Textbox.Text);
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
                    if (Textbox.Text != "" && WorkshopEventResource.ResourceType == "LocalFile" && !File.Exists(Textbox.Text)) //If file does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                    if (Textbox.Text != "" && WorkshopEventResource.ResourceType == "LocalFolder" && !Directory.Exists(Textbox.Text)) //If folder does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                    string finalPart = Path.GetFileName(Textbox.Text);
                    if (Textbox.Text != "" && WorkshopEventResource.ResourceType == "LocalFile" && File.Exists(Textbox.Text) && WorkshopEventResource.RequiredName == true && finalPart != WorkshopEventResource.Location) //If file does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                    if (Textbox.Text != "" && WorkshopEventResource.ResourceType == "LocalFolder" && Directory.Exists(Textbox.Text) && WorkshopEventResource.RequiredName == true && finalPart != WorkshopEventResource.Location) //If folder does NOT exist!
                    {
                        LabelForMissingProjectResources.Visibility = Visibility.Visible;
                        MissingLabel.Visibility = Visibility.Visible;

                    }
                }
                


                BrowseButton.Click += (sender, e) =>
                {
                    string TheString = "";

                    if (WorkshopEventResource.ResourceType == "LocalFile") { TheString = LibraryMan.GetSelectedFilePath("Select a File"); }  //TYPE IF
                    if (WorkshopEventResource.ResourceType == "LocalFolder") { TheString = LibraryMan.GetSelectedFolderPath("Select a Folder"); }  //TYPE IF



                    if (TheString != null && TheString != "")
                    {
                        if (WorkshopEventResource.RequiredName == true)
                        {
                            if (Path.GetFileName(TheString) == WorkshopEventResource.Location)
                            {
                                Textbox.Text = TheString;

                                
                                foreach (ProjectEventResource ProjectEventData in UserProject.ProjectEventResources) //Copy 1
                                {
                                    if (WorkshopEventResource.WorkshopResourceKey == ProjectEventData.ResourceKey)
                                    {
                                        ProjectEventData.Location = TheString;
                                        MissingLabel.Visibility = Visibility.Collapsed;


                                        UpdateProjectXML(UserProject);
                                    }

                                }


                            }
                            else
                            {
                                MessageBox.Show("You selected the wrong resource." +
                                    "\nSometimes a resource can require an exact matching name, this is one of those times." +
                                "\nYou must " + TheString + " with the name " + WorkshopEventResource.RequiredName, "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            Textbox.Text = TheString;
                            foreach (ProjectEventResource ProjectEventData in UserProject.ProjectEventResources) //Copy 2
                            {
                                if (WorkshopEventResource.WorkshopResourceKey == ProjectEventData.ResourceKey)
                                {
                                    ProjectEventData.Location = TheString;
                                    MissingLabel.Visibility = Visibility.Collapsed;
                                    UpdateProjectXML(UserProject);
                                }

                            }
                        }


                    }

                };

            }
        }

        private void CreateNewProject(object sender, RoutedEventArgs e) //THE NEW PROJECT sdkjfsdjfklsdjfklsjfklsjfklsjkflsjklfjklfsdjklfsjlfkjfklsda
        {
            string NewProjectName = "New Project";
            string TheProjectFolder = @LibraryMan.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\" + NewProjectName + "\\";

            if (LibraryTreeOfWorkshops.SelectedItem == null || Directory.Exists(TheProjectFolder))
            {
                return;
            }
            

            ///TopTabControl.SelectedItem = TabNewProject;               
            Directory.CreateDirectory(TheProjectFolder);

            ProjectDataItem NewDataItem = new();
            NewDataItem.ProjectName = NewProjectName;
            UpdateProjectXML(NewDataItem);//ProjectName, Input, Output
            

            TreeViewItem selectedItem = LibraryTreeOfWorkshops.ItemContainerGenerator.ContainerFromItem(LibraryTreeOfWorkshops.SelectedItem) as TreeViewItem;
            if (selectedItem != null) //This stuff makes it so the data grid updates.
            {
                selectedItem.IsSelected = false;
                selectedItem.IsSelected = true;
            }

        }
        


        //=================Button inputs==================


        private void UpdateProjectXML(ProjectDataItem ProjectData) 
        {
            if (ProjectData.ProjectEventResources == null) { ProjectData.ProjectEventResources = new(); }

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("    ");
                settings.CloseOutput = true;
                settings.OmitXmlDeclaration = true;
                using (XmlWriter writer = XmlWriter.Create(LibraryMan.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\" + ProjectData.ProjectName + "\\" + "Project.xml", settings))
                {
                    writer.WriteStartElement("Project"); //This is the root of the XML
                    writer.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                    writer.WriteElementString("VersionDate", LibraryMan.VersionDate);
                    writer.WriteElementString("NOTE", "The resources are (Project Event Resources) with a key matching (Workshop Event Resources).");
                    writer.WriteElementString("Seperator", "====================================================================================");
                    writer.WriteElementString("Name", ProjectData.ProjectName);
                    writer.WriteElementString("InputLocation", ProjectData.ProjectInputDirectory);
                    writer.WriteElementString("OutputLocation", ProjectData.ProjectOutputDirectory);

                    writer.WriteStartElement("ResourceList");
                    foreach (ProjectEventResource ProjectEventData in ProjectData.ProjectEventResources)
                    {
                        writer.WriteStartElement("Resource");
                        writer.WriteElementString("Name", WorkshopEventResources.Find(thing => thing.WorkshopResourceKey == ProjectEventData.ResourceKey).Name);
                        writer.WriteElementString("Key", ProjectEventData.ResourceKey);                        
                        writer.WriteElementString("Location", ProjectEventData.Location);
                        writer.WriteEndElement(); //End Event Resources
                    }
                    writer.WriteEndElement(); //End Event Resources

                    writer.WriteEndElement(); //End Project  AKA the Root of the XML   
                    writer.Flush(); //Ends the XML
                }
            }
            catch
            {

            }

            //DataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
            string TheProjectFolder = @LibraryMan.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\" + ProjectData.ProjectName + "\\";

            if (Directory.Exists(TheProjectFolder + "\\" + "Documents" + "\\"))  //Documents Folder
            {
            }
            else
            {
                Directory.CreateDirectory(TheProjectFolder + "\\" + "Documents" + "\\");
            }

            if (Directory.Exists(TheProjectFolder + "\\" + "\\Documents\\LoadOrder.txt")) //LoadOrder.txt file
            {

            }
            else
            {
                System.IO.File.WriteAllText(TheProjectFolder + "\\" + "\\Documents\\LoadOrder.txt", " ");
            }


            
        }

        private void ChangeProjectName(object sender, KeyEventArgs e)
        {   
            if (e.Key == Key.Enter)
            {                

                if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null|| ProjectNameTextbox.Text == "")   {return;}

                ProjectDataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
                string oldFolderPath = LibraryMan.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\" + UserProject.ProjectName;
                string newFolderPath = LibraryMan.ApplicationLocation + "\\Projects\\" + WorkshopName + "\\" + ProjectNameTextbox.Text;

                if (oldFolderPath == newFolderPath) {return;}

                Directory.Move(oldFolderPath, newFolderPath);// Rename the folder at the old path to the new path

                UserProject.ProjectName = ProjectNameTextbox.Text;

                UpdateProjectXML(UserProject);




                TreeViewItem selectedItem = LibraryTreeOfWorkshops.ItemContainerGenerator.ContainerFromItem(LibraryTreeOfWorkshops.SelectedItem) as TreeViewItem;
                if (selectedItem != null) //This stuff makes it so the data grid updates.
                {
                    selectedItem.IsSelected = false;
                    selectedItem.IsSelected = true;
                }
                               

                foreach (var item in ProjectsSelector.Items)
                {
                    if (item is ProjectDataItem dataItem && dataItem.ProjectName.Equals(ProjectNameTextbox.Text, StringComparison.OrdinalIgnoreCase))
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
            FolderSelect.Description = "Please select the folder named " + WorkshopInputDirectory; //This sets a description to help remind the user what their looking for.
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
                if (WorkshopProjectsRequireSameFolderName == false)
                {
                    LibraryMan.NotificationPositive("You MAYBE selected the correct folder?",
                        "This workshop doesn't require a specific folder name to be selected. " +
                        "This is usually set when the input folder is one that users commonly want to be able to rename. " +
                        "\n\n" +
                        "If your not sure, I STRONGLY recommend checking the readme, as well as asking around. Well, i mean, you'll know if something is wrong if you launch your project and get a ton of errors. >_>;"
                    );

                    ProjectDataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
                    UserProject.ProjectInputDirectory = FolderSelect.SelectedPath;
                    TextBoxInputDirectory.Text = FolderSelect.SelectedPath;

                    UpdateProjectXML(UserProject);//ProjectName, Input, Output

                }
                else if (WorkshopInputDirectory == Path.GetFileName(FolderSelect.SelectedPath) && WorkshopProjectsRequireSameFolderName == true)
                {
                    LibraryMan.NotificationPositive("You selected the correct folder!",
                        "The folder name you selected is the same as the one this workshop is looking for. " +
                        "This can only be wrong if you selected a folder with the exact same name, but a diffrent location."
                    );

                    ProjectDataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
                    UserProject.ProjectInputDirectory = FolderSelect.SelectedPath;
                    TextBoxInputDirectory.Text = FolderSelect.SelectedPath;

                    UpdateProjectXML(UserProject);//ProjectName, Input, Output


                }
                else
                {
                    LibraryMan.NotificationNegative("Error: Wrong folder selected!",
                        "This workshop is looking for you to select a folder named \"" + WorkshopInputDirectory + "\"." +
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
                ProjectDataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
                UserProject.ProjectOutputDirectory = FolderSelect.SelectedPath;
                TextBoxOutputDirectory.Text = FolderSelect.SelectedPath;

                UpdateProjectXML(UserProject);//ProjectName, Input, Output                


            }


        }



        private void DeleteProject(object sender, RoutedEventArgs e)
        {

            ProjectDataItem UserProject = Projects[ProjectsSelector.SelectedIndex];

            Directory.Delete(LibraryMan.ApplicationLocation + "\\Projects" + "\\" + WorkshopName + "\\" + UserProject.ProjectName, true);

            TreeViewItem WorkItem = LibraryTreeOfWorkshops.SelectedItem as TreeViewItem;
            WorkItem.IsSelected = false;
            WorkItem.IsSelected = true;
            //Directory.Delete(ExePath + "\\Projects" + WorkshopName + "\\" + UserProject.ProjectName, true);
        }

        

        

        private void DocumentsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem Item = LibraryDocumentsTree.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            TextBoxWorkshopReadMe.Text = Item.Tag as string;
            DocumentNameLabel.Content = Item.Header as string;
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
