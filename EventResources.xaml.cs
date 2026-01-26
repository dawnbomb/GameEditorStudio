using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for EventResources.xaml
    /// </summary>
    public partial class EventResources : UserControl
    {
        WorkshopData workshopData { get; set; } = null; 
        EventsMenu eventsMenu { get; set; } = null; //Used to refresh what files and filenames the events window has access to.

        List<ComboBox> ParentSelectComboBoxes = new(); //A list of all comboboxes. Used to refresh them all when a resource is renamed.

        public EventResources()
        {
            InitializeComponent();
            
        }

        public void SetupEventResourcesUI(WorkshopData workshopDataX, EventsMenu eventsMenuX)
        {
            workshopData = workshopDataX;
            eventsMenu = eventsMenuX;
            foreach (EventResource EventResource in workshopData.WorkshopEventResources)
            {
                GenerateWEventResourceUI(EventResource);
            }
        }

        private void NewResourceFile(object sender, RoutedEventArgs e)
        {
            EventResource EventResource = new();
            workshopData.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New File";
            EventResource.ResourceType = EventResource.ResourceTypes.File; 
            EventResource.IsChild = false;
            EventResource.Key = PixelWPF.LibraryPixel.GenerateKey();
            GenerateWEventResourceUI(EventResource);

            foreach (ProjectData project in workshopData.ProjectsList)
            {
                ProjectEventResource projectResource = new();
                projectResource.Key = EventResource.Key;
                project.ProjectEventResources.Add(projectResource);
            }       

            eventsMenu.RefreshEventUI();
            Database.GameLibrary.RefreshProjectEventResourcesUI();
        }

        private void NewResourceFolder(object sender, RoutedEventArgs e)
        {
            EventResource EventResource = new();
            workshopData.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New Folder";
            EventResource.ResourceType = EventResource.ResourceTypes.Folder;
            EventResource.IsChild = false;
            EventResource.Key = PixelWPF.LibraryPixel.GenerateKey();
            GenerateWEventResourceUI(EventResource);

            foreach (ProjectData project in workshopData.ProjectsList)
            {
                ProjectEventResource projectResource = new();
                projectResource.Key = EventResource.Key;
                project.ProjectEventResources.Add(projectResource);
            }

            eventsMenu.RefreshEventUI();
            Database.GameLibrary.RefreshProjectEventResourcesUI();
        }
        private void NewResourceChildFile(object sender, RoutedEventArgs e)
        {
            EventResource EventResource = new();
            workshopData.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New Child File";
            EventResource.ResourceType = EventResource.ResourceTypes.File;
            EventResource.IsChild = true;
            EventResource.Key = PixelWPF.LibraryPixel.GenerateKey();
            GenerateWEventResourceUI(EventResource);

            foreach (ProjectData project in workshopData.ProjectsList)
            {
                ProjectEventResource projectResource = new();
                projectResource.Key = EventResource.Key;
                project.ProjectEventResources.Add(projectResource);
            }

            eventsMenu.RefreshEventUI();
            Database.GameLibrary.RefreshProjectEventResourcesUI();
        }

        private void NewResourceChildFolder(object sender, RoutedEventArgs e)
        {
            
            EventResource EventResource = new();
            workshopData.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New Child Folder";
            EventResource.ResourceType = EventResource.ResourceTypes.Folder;
            EventResource.IsChild = true;
            EventResource.Key = PixelWPF.LibraryPixel.GenerateKey();
            GenerateWEventResourceUI(EventResource);

            foreach (ProjectData project in workshopData.ProjectsList)
            {
                ProjectEventResource projectResource = new();
                projectResource.Key = EventResource.Key;
                project.ProjectEventResources.Add(projectResource);
            }

            eventsMenu.RefreshEventUI();
            Database.GameLibrary.RefreshProjectEventResourcesUI();
        }

        
        public void GenerateWEventResourceUI(EventResource EventResource)
        {
            System.Windows.Media.Brush TheBrush = null;//(SolidColorBrush)(new BrushConverter().ConvertFrom("#131824")); //System.Windows.Media.Brushes.DarkSlateGray; //System.Windows.Media.Brushes.DarkSlateGray

            if (EventResource.ResourceType == EventResource.ResourceTypes.CMDText) { return; }

            DockPanel MainPanel = new();
            ResourcePanel.Children.Add(MainPanel);
            DockPanel.SetDock(MainPanel, Dock.Top);
            MainPanel.Margin = new Thickness(2, 3, 4, 10);
            MainPanel.Background = TheBrush;

            DockPanel TopPanel = new();
            DockPanel.SetDock(TopPanel, Dock.Top);
            MainPanel.Children.Add(TopPanel);
            TopPanel.Height = 30;
            TopPanel.Margin = new Thickness(0, 3, 0, 2);
            TopPanel.LastChildFill = false;
            TopPanel.Background = TheBrush;

            DockPanel BottomPanel = new();
            DockPanel.SetDock(BottomPanel, Dock.Top);
            MainPanel.Children.Add(BottomPanel);
            BottomPanel.Margin = new Thickness(0, 0, 0, 3);
            BottomPanel.Background = TheBrush;

            ComboBox ComboBox = new(); //THIS IS CREATED HERE BUT MANAGED WAY LOWER AND ONLY IF IT'S A CHILD RESOURCE.

            Button DeleteButton = new();
            TopPanel.Children.Add(DeleteButton);
            DockPanel.SetDock(DeleteButton, Dock.Right);
            DeleteButton.Width = 80;
            DeleteButton.Content = "Delete";
            DeleteButton.Click += (sender, e) =>
            {
                if (MainPanel.Parent is not Panel parentPanel)
                    return;

                // Find events that are using this resource
                var eventsUsingResource = new List<string>();
                foreach (Event eventItem in workshopData.WorkshopEvents)
                {
                    foreach (EventCommand command in eventItem.CommandList)
                    {
                        if (command.ResourceKeys.Values.Any(v => v == EventResource.Key))
                        {
                            if (!eventsUsingResource.Contains(eventItem.DisplayName))
                                eventsUsingResource.Add(eventItem.DisplayName);
                        }
                    }
                }

                List<EventResource> ResourcesUsingResource = new();
                foreach (EventResource WEventResource in workshopData.WorkshopEventResources)
                {
                    if (WEventResource.ParentKey == EventResource.Key) 
                    {
                        ResourcesUsingResource.Add(WEventResource);
                    }
                }

                // Show confirmation if it's in use
                if (eventsUsingResource.Count > 0 || ResourcesUsingResource.Count > 0)
                {
                    var message = new StringBuilder();

                    if (eventsUsingResource.Count > 0)
                    {
                        message.AppendLine("This resource is used by the following events:");
                        message.AppendLine(string.Join("\n", eventsUsingResource));
                        message.AppendLine();
                    }

                    if (ResourcesUsingResource.Count > 0)
                    {
                        message.AppendLine("This resource is used by the following child resources:");
                        message.AppendLine(string.Join("\n", ResourcesUsingResource.Select(r => r.Name)));
                        message.AppendLine();
                    }

                    string lastline = "";
                    if (eventsUsingResource.Count > 0 && ResourcesUsingResource.Count > 0) { lastline = "the events and child resources using this will have their assigned resource set to None."; }
                    if (eventsUsingResource.Count > 0) { lastline = "the events using this will have their assigned resource set to None."; }
                    if (ResourcesUsingResource.Count > 0) { lastline = "the child resources using this will have their assigned resource set to None."; }

                    message.AppendLine("Are you sure you want to delete it?");                                      
                    message.AppendLine("If you delete it, " + lastline);

                    var result = MessageBox.Show(message.ToString(), "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                        return; // user canceled
                }

                // Proceed with deletion
                parentPanel.Children.Remove(MainPanel);
                workshopData.WorkshopEventResources.Remove(EventResource);

                foreach (ProjectData project in workshopData.ProjectsList)
                {
                    project.ProjectEventResources.RemoveAll(pr => pr.Key == EventResource.Key);
                }

                foreach (Event eventItem in workshopData.WorkshopEvents)
                {
                    foreach (EventCommand command in eventItem.CommandList)
                    {
                        foreach (var key in command.ResourceKeys.Keys.ToList())
                        {
                            if (command.ResourceKeys[key] == EventResource.Key)
                                command.ResourceKeys[key] = ""; // remove assignment
                        }
                    }
                }

                foreach (EventResource WeventResource in ResourcesUsingResource) 
                {
                    WeventResource.ParentKey = "";
                }

                if (ParentSelectComboBoxes.Contains(ComboBox)) { ParentSelectComboBoxes.Remove(ComboBox); }
                foreach (ComboBox comboBox in ParentSelectComboBoxes) //Refresh all comboboxes to make sure the deleted resource is gone from all of them.
                {
                    var currentlySelected = comboBox.SelectedItem as ComboBoxItem;
                    string CurrentlySelectedtemResourceKey = (currentlySelected?.Tag as EventResource)?.Key;
                    comboBox.Items.Clear();
                    ComboBoxItem NoneComboBoxItem = new();
                    NoneComboBoxItem.Content = "None";
                    comboBox.Items.Add(NoneComboBoxItem);
                    comboBox.SelectedItem = NoneComboBoxItem;
                    foreach (EventResource EventResourceX in workshopData.WorkshopEventResources)
                    {
                        if (EventResourceX.ResourceType == EventResource.ResourceTypes.Folder && EventResourceX.IsChild == false)
                        {
                            ComboBoxItem ComboBoxItem = new();
                            ComboBoxItem.Content = "📁 " + EventResourceX.Name;
                            ComboBoxItem.Tag = EventResourceX;
                            comboBox.Items.Add(ComboBoxItem);
                            if (EventResourceX.Key == CurrentlySelectedtemResourceKey)
                            {
                                comboBox.SelectedItem = ComboBoxItem; // Reselect the previously selected item if it still exists.
                            }
                        }
                    }
                }

                Database.GameLibrary.RefreshProjectEventResourcesUI();
                eventsMenu.RefreshEventUI();
            }; //End of Delete button.


            //🗎 📁

            Label NameLabel = new();
            TopPanel.Children.Add(NameLabel);
            DockPanel.SetDock(NameLabel, Dock.Left);
            NameLabel.Width = 90;
            NameLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
            NameLabel.Margin = new Thickness(0, -4, 0, 0);
            if (EventResource.ResourceType == EventResource.ResourceTypes.File) { NameLabel.Content = "🗎 Name:"; }
            if (EventResource.ResourceType == EventResource.ResourceTypes.Folder) { NameLabel.Content = "📁Name:"; }


            TextBox NameBox = new TextBox();
            TopPanel.Children.Add(NameBox);
            DockPanel.SetDock(NameBox, Dock.Left);
            NameBox.Width = 250;
            NameBox.Text = EventResource.Name;
            NameBox.ToolTip = "This is not asking for the actual name, but for you to give this resource a name. \nInstead of game.exe, you can say totally say \"The game's .exe file\", or whatever.\nIE somewhere between a name, and a detailed instruction. Like \"The game exe's folder\".";
            NameBox.TextChanged += (sender, e) =>
            {
                EventResource.Name = NameBox.Text;
                eventsMenu.RefreshEventUI();
                Database.GameLibrary.RefreshProjectEventResourcesUI();                                

                foreach (ComboBox comboBox in ParentSelectComboBoxes)  //Make all of my child resources update their comboboxes to reflect my new name.
                {
                    ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;
                    if (selectedItem.Tag == null) { continue; }
                    EventResource selectedResource = selectedItem.Tag as EventResource;

                    if (EventResource == selectedResource)
                    {
                        selectedItem.Content = "📁 " + EventResource.Name;
                    }
                    
                }
            };

            if (EventResource.IsChild == true)
            {
                Label TypeLabel = new();
                TopPanel.Children.Add(TypeLabel);
                DockPanel.SetDock(TypeLabel, Dock.Left);
                TypeLabel.Margin = new Thickness(5, -4, 0, 0);
                TypeLabel.Content = "Child Of:";
            }    
            


            if (EventResource.IsChild == true)
            {
                //the combobox is creates up above because it's referenced when deleting a resource.
                ComboBox.Name = "ParentSelectComboBox";
                TopPanel.Children.Add(ComboBox);
                DockPanel.SetDock(ComboBox, Dock.Left);
                ComboBox.Width = 180;
                ComboBox.Margin = new Thickness(5, 0, 0, 0);
                ParentSelectComboBoxes.Add(ComboBox);

                //The Initial setup.
                ComboBoxItem NoneComboBoxItem = new();
                NoneComboBoxItem.Content = "None";
                ComboBox.Items.Add(NoneComboBoxItem);
                ComboBox.SelectedItem = NoneComboBoxItem; //Select None by default.                    

                foreach (EventResource EventResourceX in workshopData.WorkshopEventResources)
                {
                    if (EventResourceX.ResourceType == EventResource.ResourceTypes.Folder && EventResourceX.IsChild == false)
                    {
                        ComboBoxItem ComboBoxItem = new();
                        ComboBoxItem.Content = "📁 " + EventResourceX.Name;
                        ComboBoxItem.Tag = EventResourceX;
                        ComboBox.Items.Add(ComboBoxItem);

                        if (EventResource.ParentKey == EventResourceX.Key)
                        {
                            ComboBox.SelectedItem = ComboBoxItem; //Select this item if it matches the parent key.
                        }
                    }
                }

                //Clear the combobox's item list and re-setup when opened. This makes sure the user has the latest list of possible parents.
                ComboBox.DropDownOpened += (sender, e) =>
                {
                    var currentlySelected = ComboBox.SelectedItem as ComboBoxItem;
                    string CurrentlySelectedtemResourceKey = (currentlySelected?.Tag as EventResource)?.Key;

                    ComboBox.Items.Clear();

                    ComboBoxItem NoneComboBoxItem = new();
                    NoneComboBoxItem.Content = "None";
                    ComboBox.Items.Add(NoneComboBoxItem);
                    ComboBox.SelectedItem = NoneComboBoxItem;

                    foreach (EventResource EventResourceX in workshopData.WorkshopEventResources)
                    {
                        if (EventResourceX.ResourceType == EventResource.ResourceTypes.Folder && EventResourceX.IsChild == false)
                        {
                            ComboBoxItem ComboBoxItem = new();
                            ComboBoxItem.Content = "📁 " + EventResourceX.Name;
                            ComboBoxItem.Tag = EventResourceX;
                            ComboBox.Items.Add(ComboBoxItem);

                            if (EventResourceX.Key == CurrentlySelectedtemResourceKey)
                            {
                                ComboBox.SelectedItem = ComboBoxItem; // Reselect the previously selected item if it still exists.
                            }
                        }
                    }

                };

                //Closed stuff.
                ComboBox.DropDownClosed += (sender, e) =>
                {
                    ComboBoxItem item = ComboBox.SelectedItem as ComboBoxItem;
                    if (item.Tag == null ) 
                    {                        
                        EventResource.ParentKey = "";
                    }
                    else if (item.Tag is EventResource eventResource)
                    {
                        EventResource.ParentKey = eventResource.Key;
                    }
                    
                };
            }


            Button BrowseButton = new();
            TopPanel.Children.Add(BrowseButton);
            DockPanel.SetDock(BrowseButton, Dock.Right);
            BrowseButton.Width = 100;
            BrowseButton.Content = "Browse...";

            //Button ButtonMoveUp = new();
            //TopPanel.Children.Add(ButtonMoveUp);
            //DockPanel.SetDock(ButtonMoveUp, Dock.Right);
            //ButtonMoveUp.Width = 85;
            //ButtonMoveUp.Content = "Move Up";
            //ButtonMoveUp.Margin = new Thickness(0, 0, 10, 0);
            //ButtonMoveUp.Click += (sender, e) =>
            //{
            //    if (MainPanel.Parent is DockPanel Parent)
            //    {
            //        LibraryGES.MoveListItemUp(workshopData.WorkshopEventResources, EventResource);
            //        LibraryGES.MoveDockElementUp(Parent, MainPanel);
            //    }

            //};

            //Button ButtonMoveDown = new();
            //TopPanel.Children.Add(ButtonMoveDown);
            //DockPanel.SetDock(ButtonMoveDown, Dock.Right);
            //ButtonMoveDown.Width = 95;
            //ButtonMoveDown.Content = "Move Down";
            //ButtonMoveDown.Click += (sender, e) =>
            //{
            //    if (MainPanel.Parent is DockPanel Parent)
            //    {
            //        LibraryGES.MoveListItemDown(workshopData.WorkshopEventResources, EventResource);
            //        LibraryGES.MoveDockElementDown(Parent, MainPanel);
            //    }

            //};


            DockPanel BrowsePanel = new();
            DockPanel.SetDock(BrowsePanel, Dock.Left);
            BottomPanel.Children.Add(BrowsePanel);
            BrowsePanel.Background = TheBrush;

            CheckBox CheckBox = new();
            if (EventResource.IsChild == false)
            {
                TopPanel.Children.Add(CheckBox);
                DockPanel.SetDock(CheckBox, Dock.Left);
                CheckBox.Content = "Require Exact Name";
                CheckBox.Width = 150;
                CheckBox.Margin = new Thickness(10, 0, 0, 0);
                CheckBox.Background = TheBrush;
                if (EventResource.RequiredName == true) { CheckBox.IsChecked = true; BrowsePanel.Visibility = Visibility.Visible; BrowseButton.Visibility = Visibility.Visible; }
                if (EventResource.RequiredName == false) { CheckBox.IsChecked = false; BrowsePanel.Visibility = Visibility.Collapsed; BrowseButton.Visibility = Visibility.Collapsed; }
                CheckBox.Checked += (sender, e) =>
                {
                    BrowsePanel.Visibility = Visibility.Visible;
                    BrowseButton.Visibility = Visibility.Visible;
                    EventResource.RequiredName = true;
                };
                CheckBox.Unchecked += (sender, e) =>
                {
                    BrowsePanel.Visibility = Visibility.Collapsed;
                    BrowseButton.Visibility = Visibility.Collapsed;
                    EventResource.RequiredName = false;
                };// this is here because if it doesn't require an exact name, there is no need to show the path. 
            }

            Label MissingLabel = new();
            MissingLabel.Content = "Project Resource Location is not set!";
            //MissingLabel.Background = Brushes.DarkRed;
            //MissingLabel.Margin = new Thickness(1);
            MissingLabel.Foreground = Brushes.Red;
            DockPanel.SetDock(MissingLabel, Dock.Left);
            TopPanel.Children.Add(MissingLabel);
            if (EventResource.IsChild == true) { MissingLabel.Visibility = Visibility.Collapsed; }
            foreach (ProjectData project in workshopData.ProjectsList)
            {
                ProjectEventResource projectEventResource = project.ProjectEventResources.Find(pr => pr.Key == EventResource.Key);
                if (projectEventResource != null)
                {

                    if (projectEventResource.Location != "" && projectEventResource.Location != null) { MissingLabel.Visibility = Visibility.Collapsed; break; }

                }
            }




            Border TextBorder = new Border();
            BrowsePanel.Children.Add(TextBorder);
            //BottomPanel.Children.Add(TextBorder);
            TextBorder.CornerRadius = new CornerRadius(8);
            TextBorder.BorderBrush = System.Windows.Media.Brushes.Black;
            TextBorder.BorderThickness = new Thickness(1.5);
            TextBorder.Padding = new Thickness(2);
            TextBorder.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF18191B"));



            TextBox Textbox = new TextBox();
            //BrowsePanel.Children.Add(Textbox);
            TextBorder.Child = Textbox;
            Textbox.Padding = new Thickness(4);
            Textbox.BorderThickness = new Thickness(0);
            Textbox.Margin = new Thickness(0);
            DockPanel.SetDock(Textbox, Dock.Left);
            Textbox.IsEnabled = false;
            Textbox.Text = EventResource.Location;

            BrowseButton.Click += (sender, e) =>
            {
                string LocationString = ""; //The string that gets put into the box & saved.
                string FolderStartPath = ""; //Where a relative file/folder starts it's search.

                //IMPORTANT NOTE:
                //
                //I made the decision that when browsing for the location of a child File/Folder,
                //that the user MUST first set the current project's resource location for the parent. I decided this because...
                //1: It's good to start the file explorer at the location of the parent folder's location.
                //2: It *feels* required if i want to allow the parent to not require a specific folder name,
                //and also somehow know what parts of the path are to the parent location vs from the parent location. 

                ProjectData UserProject = workshopData.ProjectDataItem;

                if (UserProject == null) //Setting the user project if its from library.
                {
                    GameLibrary library = Database.GameLibrary;

                    if (library.ProjectsSelector.SelectedIndex < 0 || library.LibraryTreeOfWorkshops.SelectedItem == null)
                    {
                        PixelWPF.LibraryPixel.NotificationNegative("Error: No Project Selected.", "There isn't any project selected in the game library. Go select one first. Sorry :(");
                        return;
                    }
                    UserProject = library.SelectedWorkshop.ProjectsList[library.ProjectsSelector.SelectedIndex];                    
                } 
                foreach (ProjectEventResource ProjectResourceData in UserProject.ProjectEventResources) //Setting the Folder Start path using the project's set resource location.
                {
                    if (EventResource.ParentKey == ProjectResourceData.Key) 
                    {
                        EventResource resource = workshopData.WorkshopEventResources.FirstOrDefault(r => r.Key == EventResource.ParentKey);

                        if (ProjectResourceData.Location == "" || ProjectResourceData.Location == null) //or directory exists
                        {
                            PixelWPF.LibraryPixel.NotificationNegative("Error: Parents location is blank.", "To set the location of a child resource, first go and set the project resource location for the parent folder (" + resource.Name + ")." +
                                "\n\nYes I know this is annoying, but after thinking this through A LOT, this really is the only way. I'M SORRYYY :(");
                            return;
                        }
                        if (!Directory.Exists(ProjectResourceData.Location)) //or directory exists
                        {
                            PixelWPF.LibraryPixel.NotificationNegative("Error: Parents location is missing?", "It seems like the parent folder (" + resource.Name + "), " +
                                "\nit's project resource location (" + ProjectResourceData.Location + "), " +
                                "\nit doesn't exist anymore. IE a folder does not exist at that location. " +
                                "\n\nGo fix that first! (Set a new location for it)" +
                                "\n\nIf this isn't true then i missed something T.T please report it :3");
                            return;
                        }
                        FolderStartPath = ProjectResourceData.Location; 
                    }
                }




                if (EventResource.ResourceType == EventResource.ResourceTypes.File) 
                {
                    if (EventResource.IsChild == false) { LocationString = LibraryGES.GetSelectFileName("Select a File"); }
                    if (EventResource.IsChild == true)  { LocationString = LibraryGES.GetSelectedRelativeFilePath("Select a File", FolderStartPath); }                    
                }
                if (EventResource.ResourceType == EventResource.ResourceTypes.Folder)
                {
                    if (EventResource.IsChild == false) { LocationString = LibraryGES.GetSelectFolderName("Select a Folder"); }
                    if (EventResource.IsChild == true)  { LocationString = LibraryGES.GetSelectedRelativeFolderPath("Select a Folder", FolderStartPath); }                    
                }

                //i can fix this problem by using the project path's current local folder path.

                //if (CheckBox.IsChecked == true)  //Wont matter because browse button and checkbox are mutually exclusive.
                //{
                //    if (LocationString != null || LocationString != "")
                //    {
                //        Textbox.Text = LocationString;
                //        EventResource.RequiredName = LocationString;
                //    }
                //}
                if (LocationString != null || LocationString != "")
                {
                    Textbox.Text = LocationString;
                    EventResource.Location = LocationString;
                }
                //UpdateProjectXML(); //This is just to link me to this, i never intend to call this method here, but this file is getting big and its hard to find v.v
                //SaveWorkshopLibrary(); //same as above
            };

        }

        private void ChildResourceHelp(object sender, RoutedEventArgs e)
        {
            PixelWPF.LibraryPixel.Notification("Child Resource Help",
                "Child resources are subfolder resources that are relative to another folder resource's location.\n\n" +
                "For example, if you have a folder resource, you can add a child file resource that is relative to that folder. " +
                "\n\nThe main advantage of child resources is that a user doesn't need to set the resource locations of child resources! " +
                "(because once they set the parent folder location, the rest of the path is relative)." +
                "\n\nChild resources will probably only be used by workshops doing advanced things. So if you don't understand them, just use normal resources instead." 
            );
        }
    }

        
    
}
