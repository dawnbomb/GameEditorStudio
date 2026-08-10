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
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
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

    public partial class GameLibrary : UserControl
    {        
        public WorkshopData? SelectedWorkshop { get; set; }

        //Order of operations is...
        //1: Window Initalize
        //2: User Control Initalize
        //3: Window.Loaded
        //4: User Control.Loaded

        public GameLibrary()
        {
            InitializeComponent();               
            Database.GameLibrary = this;

            {   //I am intensionally leaving this existing but collapsed, and related code, incase i later want project related stuff existing again in the game library.
                //ProjectsContent.Visibility = Visibility.Collapsed; //TEMP FOR TESTING
                ProjectResourceControl.Visibility = Visibility.Collapsed; //TEMP FOR TESTING
            }
            if (Properties.Settings.Default.ShowRecentWorkshops == false)
            {
                RecentWorkshopsContent.Visibility = Visibility.Collapsed;
            }

            

            LoadDatabase LoadDatabase = new(); //Must happen before Setup Commands, because commands use tools.   
            //LoadDatabase.LoadWiki();
            //LoadDatabase.LoadToolImageLocations(); //currently also causes a popup on launch of all images it detects.
            LoadDatabase.LoadThemes(); //Loads from Other/Themes - A theme is a list of colors for the UI. Users can create their own color themes. 
            LoadDatabase.LoadToolsList(); //Loads from Other/Tools.xml.            
            LoadDatabase.LoadCommandsList(this); //Loads from Other/Commands.xml.            
            LoadDatabase.LoadCommonEventsList(); //Loads from Other/Common Events.xml.   
            //NOTE: LoadCommonEventsForWorkshop happens when the tools menu itself is opened,
            //as i don't currently support pre-loading every workshops data from the library, but common events are still for the "CURRENT" workshop.
            LoadDatabase.LoadToolLocations(); //Load user's last known tool locations.
            LoadDatabase.LoadEnabledCommonEvents(); //Loads from Settings/Common Events.xml the user's enabled common events.
            LoadDatabase.LoadWorkshops_INCLUDING_EVENTS(); //Events are loaded here. - - -  Does not fully load the workshops, that happens when one is launched. 
            
            RefreshWorkshopTree();
            RefreshRecentWorkshopTree();


            Dispatcher.InvokeAsync(async () => await PixelWPF.GithubUpdater.CheckForUpdatesAsync("GameEditorStudio", "dawnbomb/GameEditorStudio/releases/latest", LibraryGES.VersionNumber));

            MainMenu.MenuLibrarySetup(this);

            
            
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

        public void RefreshRecentWorkshopTree()
        {
            LibraryTreeOfRecentWorkshops.Items.Clear();
            foreach (string RecentWorkshopName in LibraryGES.RecentWorkshops)
            {
                foreach (TreeViewItem treeitem in LibraryTreeOfWorkshops.Items)
                {
                    WorkshopData workshopData = treeitem.Tag as WorkshopData;
                    if (workshopData.WorkshopName == RecentWorkshopName)
                    {
                        TreeViewItem copy = new TreeViewItem();

                        copy.Header = treeitem.Header;
                        copy.Tag = treeitem.Tag;
                        copy.ToolTip = treeitem.ToolTip;
                        copy.ContextMenu = treeitem.ContextMenu;

                        copy.MouseRightButtonDown += (s, e) =>
                        {
                            copy.IsSelected = true; //If i ever remove this, make sure all right click options function properly (Especially Open Workshop Folder as that invokes a CommandMethod.)
                            treeitem.IsSelected = true;
                        };

                        LibraryTreeOfRecentWorkshops.Items.Add(copy);
                    }
                }
            }
        }

        
        private void RecentWorkshopsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem RTreeItem = LibraryTreeOfRecentWorkshops.SelectedItem as TreeViewItem;
            if (RTreeItem == null) { return; }
            WorkshopData RworkshopData = RTreeItem.Tag as WorkshopData;
                        
            foreach (TreeViewItem treeitem in LibraryTreeOfWorkshops.Items)
            {
                WorkshopData workshopData = treeitem.Tag as WorkshopData;
                if (workshopData == RworkshopData)
                {
                    treeitem.IsSelected = true;                    
                }
            }
        }

        private void LibraryTreeOfWorkshops_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            EditorsTree.Items.Clear();
            LibraryDocumentsTree.Items.Clear();


            if (LibraryTreeOfWorkshops.SelectedItem == null)
            {
                SelectedWorkshop = null;
                return;
            }

            TreeViewItem treeItem = LibraryTreeOfWorkshops.SelectedItem as TreeViewItem;
            SelectedWorkshop = treeItem.Tag as WorkshopData;

            {//Unselect recent workshop if not current workshop.
                foreach (TreeViewItem Rtreeitem in LibraryTreeOfRecentWorkshops.Items)
                {
                    WorkshopData RworkshopData = Rtreeitem.Tag as WorkshopData;
                    if (Rtreeitem.IsSelected == true) 
                    {
                        if (SelectedWorkshop != RworkshopData) 
                        {
                            Rtreeitem.IsSelected = false;
                        }
                    }
                    if (Rtreeitem.IsSelected == false && SelectedWorkshop == RworkshopData) 
                    {
                        Rtreeitem.IsSelected = true;
                    }
                                        
                }
            }
           

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

                        


            { //Right sidebar stuff.

                //WorkshopCreatedLabel

                //WorkshopInfoDocuments.Content = "Documents: " + Convert.ToString(System.IO.Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Documentation", "*", SearchOption.TopDirectoryOnly).Count());

                //Workshop Info Panel
                WorkshopCreatedLabel.Content = "GES v" + SelectedWorkshop.CreatedVersion + " " + SelectedWorkshop.CreatedDate + "";
                if (SelectedWorkshop.CreatedVersion.ToString() == "0.0") { WorkshopCreatedLabel.Content = "UNKNOWN"; }
                WorkshopSavedLabel.Content = "GES v" + SelectedWorkshop.LastUsedVersion + " " + SelectedWorkshop.LastUsedDate + "";
                if (SelectedWorkshop.LastUsedVersion.ToString() == "0.0") { WorkshopSavedLabel.Content = "NEVER USED"; }

                //Documents Panel
                int DocumentCount = 0;
                if (SelectedWorkshop.Intro.IntroText != "")
                {   
                    TreeViewItem item = new();
                    item.Header = "Intro to: " + SelectedWorkshop.WorkshopName;
                    item.Tag = SelectedWorkshop.Intro.IntroText;
                    LibraryDocumentsTree.Items.Add(item);

                    item.IsSelected = true;
                    DocumentCount++;
                }
                else 
                {
                    TreeViewItem item = new();
                    item.Header = "Default Workshop Intro";
                    item.Tag = SelectedWorkshop.Intro.DefaultIntroText;
                    LibraryDocumentsTree.Items.Add(item);

                    item.IsSelected = true;
                }
                string documentationPath = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", SelectedWorkshop.WorkshopName, "Documents");
                if (Directory.Exists(documentationPath)) 
                {
                    string[] folderPaths = Directory.GetDirectories(documentationPath);
                    foreach (string folderPath in folderPaths)
                    {
                        TreeViewItem item = new();
                        item.Header = new DirectoryInfo(folderPath).Name;
                        item.Tag = System.IO.File.ReadAllText(folderPath + "\\Text.txt");
                        LibraryDocumentsTree.Items.Add(item);
                        DocumentCount++;

                        //string headerText = (item.Header as string)?.Replace(" ", "").ToLower(); //Check if a readme exists, but ignore caps and spaces, so it will always find it.
                        //if (headerText == "readme")
                        //{
                        //    item.IsSelected = true;
                        //}

                        ContextMenu contextMenu = new ContextMenu();
                        item.ContextMenu = contextMenu;
                        MenuItem openMenuItem = new MenuItem { Header = "Open Document Folder" };
                        contextMenu.Items.Add(openMenuItem);
                        openMenuItem.Click += (s, e) =>
                        {
                            LibraryGES.OpenFileFolder(folderPath + "\\Text.txt");
                        };
                    }
                }
                //DocumentCountLabel.Content = "(" + LibraryDocumentsTree.Items.Count + ")";
                DocumentCountLabel.Content = "(" + DocumentCount + ")";


                //Editors Panel
                string WorkshopEditorsFolder = LibraryGES.ApplicationLocation + "\\Workshops\\" + SelectedWorkshop.WorkshopName + "\\Editors\\";
                string[] EditorFoldersList = Directory.GetDirectories(WorkshopEditorsFolder);

                // Create TreeViewItems for each folder and add them to the EditorsTree
                foreach (string folder in EditorFoldersList)
                {
                    string folderName = new DirectoryInfo(folder).Name;
                    TreeViewItem folderItem = new TreeViewItem { Header = folderName };
                    EditorsTree.Items.Add(folderItem);
                }
                EditorCountLabel.Content = "(" + EditorsTree.Items.Count + ")";
            }

        }

        


        private void LaunchWorkshopPreviewMode(object sender, RoutedEventArgs e)
        {
            if (SelectedWorkshop.CreatedVersion > LibraryGES.VersionNumber)
            {
                PixelWPF.LibraryPixel.Notification("Workshop from future version of GES!",
                        "The version of GES your using is so old it's before this workshop was even first created! " +
                        "As such, it may use features only added in future versions. " +
                        "\n" +
                        "\nWorse yet, it may open, and even save properly, but in doing so the workshop may lose important data that is only acnowledged by future versions of GES." +
                        "\n" +
                        "\nI won't stop you from *trying* to use it anyway, but be *VERY* careful of this."
                        );
            }
            else if (SelectedWorkshop.LastUsedVersion > LibraryGES.VersionNumber)
            {
                PixelWPF.LibraryPixel.Notification("Workshop last used in future GES version!",
                        "The workshop you are trying to open was last used in a later version of GES, " +
                        "and it may use features only added in future versions. " +
                        "\n" +
                        "\nWorse yet, it may open, and save, but in doing so the workshop may lose important data that is only acnowledged by future versions of GES." +
                        "\n" +
                        "\nI won't stop you from *trying* to use it anyway, but just be aware and *VERY* careful of this."
                        );
            }


            {//Update recent workshops list.
                if (LibraryGES.RecentWorkshops.Contains(SelectedWorkshop.WorkshopName)) { LibraryGES.RecentWorkshops.Remove(SelectedWorkshop.WorkshopName); }
                LibraryGES.RecentWorkshops.Insert(0, SelectedWorkshop.WorkshopName);                
                if (LibraryGES.RecentWorkshops.Count > 3) { LibraryGES.RecentWorkshops.RemoveAt(3); }
                if (LibraryGES.RecentWorkshops.Count > 3) { LibraryGES.RecentWorkshops.RemoveAt(3); }
                if (LibraryGES.RecentWorkshops.Count > 3) { LibraryGES.RecentWorkshops.RemoveAt(3); }

                string NewRecentList = "";
                bool first = true;
                foreach (string name in LibraryGES.RecentWorkshops) 
                {
                    if (first == false) { NewRecentList += "|"; }
                    NewRecentList += name;
                    first = false;
                }
                Properties.Settings.Default.RecentWorkshops = NewRecentList;
            }



            {//Loading bar code 
                LoadingFinalPanel.Visibility = Visibility.Collapsed;
                LoadingPanel.Visibility = Visibility.Visible;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            }

            

            if (SelectedWorkshop.WorkshopXaml != null) 
            {
                Database.GESMain.GESGrid.Children.Add(SelectedWorkshop.WorkshopXaml);
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                Workshop TheWorkshop = new Workshop(SelectedWorkshop); //Thing One, the workshop
                //Database.GESMain.GESGrid.Children.Add(TheWorkshop);
                //LoadingPanel.Visibility = Visibility.Collapsed;
            }
            //Workshop TheWorkshop = new Workshop(SelectedWorkshop); //Thing One, the workshop
            //Database.GESMain.GESGrid.Children.Add(TheWorkshop);

            Properties.Settings.Default.LastWorkshop = SelectedWorkshop.WorkshopName; //Set the workshop name in settings, so it can be used by other parts of the program. 
            Properties.Settings.Default.Save();

            
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
            

            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null || SelectedWorkshop == null)
            {                
                RefreshProjectEventResourcesUI();
                return;
            }


            
            Project UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
                        

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
            Project UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];
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

        

        //=================Button inputs==================
        

        

        private void DocumentsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem Item = LibraryDocumentsTree.SelectedItem as TreeViewItem;
            if (Item == null) { return; }

            string defaulttext = "Welcome to my early beta of Game Editor Studio! " +
                "\n\nThis program lets you create and share game editors without knowing how to code! " +
                "\n\nIt also has easy access to romhacking tools, and LOTS of QoL! " +
                "\n(save often and report bugs / crashes on the discord)" +
                "\n\n\n\nPS: Most things have right click options." +
                "\n\n\n\nMade in C# / WPF / .Net10 " +
                "\n\nTo join the dev team, reach out to me on discord! (Link top right) " +
                "\nLets make game modding simple, easy, and fun! :)";

            DocumentNameLabel.Content = Item.Header as string;           
                        
            Dictionary<string, BitmapImage> images = new();
            //foreach (WikiImage wimage in document.ImagesList) { images.Add(wimage.FileName, wimage.Bitmap); }
            PixelWPF.LibraryText.TextToStackPanel(Item.Tag as string, StackPanelForWorkshopDocument, images);
                      
            if (Item.Header as string == "READ ME" || Item.Header as string == "README" || Item.Header as string == "readme" || Item.Header as string == "read me") 
            {
                DocumentNameLabel.Content = SelectedWorkshop.WorkshopName + " - " + Item.Header as string;
                PixelWPF.LibraryText.TextToStackPanel(defaulttext, StackPanelForWorkshopDocument, images);
            }
        }
        private void OpenProjectFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = this;
            MethodData.WorkshopData = SelectedWorkshop;
            CommandMethodsClass.OpenSelectedProjectFolder(MethodData);
        }

        private void OpenInput(object sender, RoutedEventArgs e)
        {

            MethodData MethodData = new();
            MethodData.GameLibrary = this;
            CommandMethodsClass.OpenSelectedProjectInputFolder(MethodData);

        }

        private void OpenOutput(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = this;
            CommandMethodsClass.OpenSelectedProjectOutputFolder(MethodData);

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

            Project UserProject = SelectedWorkshop.ProjectsList[ProjectsSelector.SelectedIndex];

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
