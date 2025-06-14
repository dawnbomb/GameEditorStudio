using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Windows.Foundation.Metadata;
using Path = System.IO.Path;

namespace GameEditorStudio
{
    /// This is for the workshop data editing inside the game library. its not a part of a workshop itself!
    public partial class WorkshopMaker : UserControl
    {
        GameLibrary Library;
        string TheMode;

        public WorkshopMaker(string TheModee)
        {
            InitializeComponent();

            TheMode = TheModee;
            this.Loaded += new RoutedEventHandler(LoadEvent);

            
        }

        public void LoadEvent(object sender, RoutedEventArgs e) 
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is GameLibrary GameLibraryWindow)
            {
                Library = GameLibraryWindow;
                //Tools = GameLibraryWindow.Tools;
                //Commands = GameLibraryWindow.Commands;
                //CommonEvents = GameLibraryWindow.CommonEvents;
                //GameLibrary.MainMenu = this;
                //EventResources = GameLibrary.EventResources;
            }

            if (TheMode == "New" ) 
            {
                ButtonCreateNewWorkshop.Content = "Create Workshop";
                WorkshopTextboxExampleInputFolder.Text = "";
            }

            if (TheMode == "Edit") 
            {
                ButtonCreateNewWorkshop.Content = "Save Workshop";
                TextBoxGameName.Text = Library.WorkshopName;                                                               
                WorkshopTextboxExampleInputFolder.Text = Library.WorkshopInputDirectory; //System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Input Directory.txt");

                ResourcePanel.Children.Clear();
                foreach (WorkshopResource EventResource in Library.WorkshopEventResources) 
                {
                    GenerateWEventResourceUI(EventResource); 
                }
            }


            
        }

        private void ButtonSetWorkshopInputFolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select where files load from (For dev, this is CrystalEditor/CE/Hexfiles)"; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            if ((bool)FolderSelect.ShowDialog(Library)) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                WorkshopTextboxExampleInputFolder.Text = Path.GetFileName(FolderSelect.SelectedPath);
            }
        }

        private void ButtonCreateNewWorkshop_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonCreateNewWorkshop.Content.ToString() == "Create Workshop")
            {
                if (TextBoxGameName.Text != null && TextBoxGameName.Text != "" && WorkshopTextboxExampleInputFolder.Text != null && WorkshopTextboxExampleInputFolder.Text != "")
                {

                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text);
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents");
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Editors");
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Tools");

                    
                    System.IO.File.WriteAllText(LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents\\LoadOrder.txt", " "); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    SaveWorkshopLibrary();

                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents\\READ ME");
                    System.IO.File.WriteAllText(LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents\\READ ME\\Text.txt", "" +
                        "When inside the workshop, please fill out the READ ME document with useful information." +
                        "This document is special, and is shown to users in the game library." +
                        "Idealy, the READ ME explains how to extract the game files, what tools are needed, and how to setup a project for that game." +
                        "\n" +
                        "\nIt may also be a good idea to include a Workshop Version Number so users can know if their version of a workshop is outdated." +
                        "Other good ideas include patch / update notes for the workshop, links to discords, forumns, wikis, and any other relevant information about a game." +
                        "\n" +
                        "\nPS: READ ME does not need to include information on how a workshop is used, if you want that, make it a seperate document." +
                        "\nThat way users who are still setting up a project and don't have the workshop open yet won't be overwhelmed with explanatory text." +
                        "\n" +
                        "\nInfo on platform (PC, Switch, PS2, etc), Region (USA, JP, EU, etc), Version (patch v1.1, expansion name, etc), Emulator (MelonDS, Azahar, etc), Etc. ");


                    var parentContainer = this.Parent as Grid;
                    if (parentContainer != null)
                    {
                        Library.Projects.Clear();
                        CollectionViewSource.GetDefaultView(Library.ProjectsSelector.ItemsSource).Refresh();
                        Library.ProjectEventResourcesPanel.Children.Clear();
                        Library.ProjectNameTextbox.Text = "";
                        Library.LibraryDocumentsTree.Items.Clear();
                        Library.EditorsTree.Items.Clear();
                        Library.TextBoxWorkshopReadMe.Text = "";
                        Library.TextBoxInputDirectory.Text = "";
                        Library.TextBoxOutputDirectory.Text = "";

                        parentContainer.Children.Remove(this);
                    }

                }
                else
                {
                    MessageBox.Show("Either you did not name the game, \nor you forgot to give it a Input Directory.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            if (ButtonCreateNewWorkshop.Content.ToString() == "Save Workshop")
            {
                if (TextBoxGameName.Text != null && TextBoxGameName.Text != "" && WorkshopTextboxExampleInputFolder.Text != null && WorkshopTextboxExampleInputFolder.Text != "")
                {
                    string OldWorkshopName = LibraryMan.ApplicationLocation + "\\Workshops\\" + Library.WorkshopName;
                    string NewWorkshopName = LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text;
                    string OldProjectsName = LibraryMan.ApplicationLocation + "\\Projects\\" + Library.WorkshopName;
                    string NewProjectsName = LibraryMan.ApplicationLocation + "\\Projects\\" + TextBoxGameName.Text;
                    try
                    {
                        Directory.Move(OldWorkshopName, NewWorkshopName);
                        Directory.Move(OldProjectsName, NewProjectsName);
                    }
                    catch (IOException exp)
                    {
                        Console.WriteLine(exp.Message);
                    }

                    SaveWorkshopLibrary();

                    var parentContainer = this.Parent as Grid;
                    if (parentContainer != null)
                    {
                        Library.Projects.Clear();
                        CollectionViewSource.GetDefaultView(Library.ProjectsSelector.ItemsSource).Refresh();
                        Library.ProjectEventResourcesPanel.Children.Clear();
                        Library.ProjectNameTextbox.Text = "";
                        Library.LibraryDocumentsTree.Items.Clear();
                        Library.EditorsTree.Items.Clear();
                        Library.TextBoxWorkshopReadMe.Text = "";
                        Library.TextBoxInputDirectory.Text = "";
                        Library.TextBoxOutputDirectory.Text = "";

                        parentContainer.Children.Remove(this);
                    }

                }
                else
                {
                    MessageBox.Show("Either you are trying to save the game without a name, \nor you somehow deleted the Input Directory.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }

            


        }


        private void SaveWorkshopLibrary()
        {            
            string LibraryXmlPath = LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\" + "LibraryTestSave.xml";   
            
            SaveIt();
            try
            {
                if (File.Exists(LibraryXmlPath)) { File.Delete(LibraryXmlPath); }
            } 
            catch 
            {
            
            }            

            LibraryXmlPath = LibraryMan.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\" + "Library.xml";  

            SaveIt();

            void SaveIt()
            {
                try 
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryXmlPath, settings))
                    {
                        writer.WriteStartElement("Library");
                        writer.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryMan.VersionDate);
                        writer.WriteElementString("InputLocation", WorkshopTextboxExampleInputFolder.Text);

                        writer.WriteStartElement("ResourceList");
                        foreach (WorkshopResource WorkshopEventResource in Library.WorkshopEventResources)
                        {
                            //when xml loads, variables WILL be null, even if they have a default value, if it's not written to begin with. this is very annoying. 
                            writer.WriteStartElement("Resource");
                            writer.WriteElementString("Name", WorkshopEventResource.Name);
                            writer.WriteElementString("Key", WorkshopEventResource.WorkshopResourceKey);

                            if (WorkshopEventResource.ResourceType == "LocalFile") 
                            {
                                writer.WriteElementString("ResourceType", "File");
                                writer.WriteElementString("PathType", "FullPath");
                            }
                            if (WorkshopEventResource.ResourceType == "LocalFolder") 
                            {
                                writer.WriteElementString("ResourceType", "Folder");
                                writer.WriteElementString("PathType", "FullPath"); 
                            }
                            if (WorkshopEventResource.ResourceType == "RelativeFile")
                            {
                                writer.WriteElementString("ResourceType", "File"); 
                                writer.WriteElementString("PathType", "PartialPath"); 
                            }
                            if (WorkshopEventResource.ResourceType == "RelativeFolder")
                            {
                                writer.WriteElementString("ResourceType", "Folder");
                                writer.WriteElementString("PathType", "PartialPath");
                            }

                            writer.WriteElementString("RequiredName", WorkshopEventResource.RequiredName.ToString());  //if full path (local)
                            writer.WriteElementString("Location", WorkshopEventResource.Location);  //if partial path
                            writer.WriteElementString("TargetKey", WorkshopEventResource.TargetKey); //if partial path                    


                            writer.WriteEndElement(); //End File
                        }
                        writer.WriteEndElement(); //End ResourceList                 

                        writer.WriteEndElement(); //End Root (Library)
                        writer.Flush(); //Ends the XML Library file                                

                    }
                }
                catch 
                {
                    string Error = "Error: A workshop's library data failed to save properly." +
                        "\nAll saves (are supposed to be) simulated in this program, so pre-existing data should be fine..." +
                        "\nbut...this is really weird! This one especially should never crash! What the hell did you do?!?" +
                        "\n" +
                        "\nYou should restart the program.";
                    Notification f2 = new(Error);
                    f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    //f2.ShowDialog();
                    return;
                }
                
            }
            

            Library.ScanForWorkshops();
        }











        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            var parentContainer = this.Parent as Grid;
            if (parentContainer != null)
            {
                parentContainer.Children.Remove(this);
            }
        }

        private void NewEventResourceLocalFile(object sender, RoutedEventArgs e)
        {
            WorkshopResource EventResource = new();
            Library.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New File";
            EventResource.ResourceType = "LocalFile";
            EventResource.WorkshopResourceKey = LibraryMan.GenerateKey();
            GenerateWEventResourceUI(EventResource);
        }

        private void NewEventResourceLocalFolder(object sender, RoutedEventArgs e)
        {
            WorkshopResource EventResource = new();
            Library.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New Folder";
            EventResource.ResourceType = "LocalFolder";
            EventResource.WorkshopResourceKey = LibraryMan.GenerateKey();
            GenerateWEventResourceUI(EventResource);
        }

        private void NewEventResourceRelativeFile(object sender, RoutedEventArgs e)
        {
            WorkshopResource EventResource = new();
            Library.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New Relative File";
            EventResource.ResourceType = "RelativeFile";
            EventResource.WorkshopResourceKey = LibraryMan.GenerateKey();
            GenerateWEventResourceUI(EventResource);
        }

        private void NewEventResourceRelativeFolder(object sender, RoutedEventArgs e)
        {
            string Error = "Being able to add a folder that is relative to another folder resource's location is an upcoming feature." +
                        "\nI'll add support soon enough, i just really wanna release the damn program already! x3" +
                        "\nIf you want it on priority for something, let me know on discord." +
                        "\n";
            Notification f2 = new(Error);
            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //f2.ShowDialog();
            return;

            WorkshopResource EventResource = new();
            Library.WorkshopEventResources.Add(EventResource);
            EventResource.Name = "New Relative Folder";
            EventResource.ResourceType = "RelativeFolder";
            EventResource.WorkshopResourceKey = LibraryMan.GenerateKey();
            GenerateWEventResourceUI(EventResource);
        }


        public void GenerateWEventResourceUI(WorkshopResource EventResource)
        {
            System.Windows.Media.Brush TheBrush = null;//(SolidColorBrush)(new BrushConverter().ConvertFrom("#131824")); //System.Windows.Media.Brushes.DarkSlateGray; //System.Windows.Media.Brushes.DarkSlateGray

            DockPanel MainPanel = new();
            ResourcePanel.Children.Add(MainPanel);
            DockPanel.SetDock(MainPanel, Dock.Top);
            MainPanel.Margin = new Thickness(4, 3, 4, 20);            
            MainPanel.Background = TheBrush;

            DockPanel TopPanel = new();
            DockPanel.SetDock(TopPanel, Dock.Top);
            MainPanel.Children.Add(TopPanel);
            TopPanel.Height = 25;
            TopPanel.Margin = new Thickness(0, 3, 0, 2);
            TopPanel.LastChildFill = false;
            TopPanel.Background = TheBrush;

            DockPanel BottomPanel = new();
            DockPanel.SetDock(BottomPanel, Dock.Top);
            MainPanel.Children.Add(BottomPanel);
            BottomPanel.Margin = new Thickness(0, 0, 0, 3);
            BottomPanel.Background = TheBrush;

            Button DeleteButton = new();
            TopPanel.Children.Add(DeleteButton);
            DockPanel.SetDock(DeleteButton, Dock.Right);
            DeleteButton.Width = 70;
            DeleteButton.Content = "Delete";
            DeleteButton.Click += (sender, e) =>
            {
                if (MainPanel.Parent is Panel parentPanel)
                {
                    parentPanel.Children.Remove(MainPanel);
                    Library.WorkshopEventResources.Remove(EventResource);
                }
            };


            

            Label Label = new();
            TopPanel.Children.Add(Label);
            DockPanel.SetDock(Label, Dock.Left);
            Label.Content = "Name:";
            Label.Margin = new Thickness(0, -4, 0, 0);


            TextBox NameBox = new TextBox();
            TopPanel.Children.Add(NameBox);
            DockPanel.SetDock(NameBox, Dock.Left);
            NameBox.Width = 170;
            NameBox.Text = EventResource.Name;
            NameBox.ToolTip = "This is not asking for the actual name, but for you to give this resource a name. \nInstead of game.exe, you can say totally say \"The game's .exe file\", or whatever.\nIE somewhere between a name, and a detailed instruction. Like \"The game exe's folder\".";
            NameBox.TextChanged += (sender, e) =>
            {
                EventResource.Name = NameBox.Text;
            };

            Label TypeLabel = new();
            TopPanel.Children.Add(TypeLabel);
            DockPanel.SetDock(TypeLabel, Dock.Left);            
            TypeLabel.Margin = new Thickness(0, -4, 0, 0);
            TypeLabel.Content = "   Type: ";
            if (EventResource.ResourceType == "LocalFile")      { TypeLabel.Content = "   Type: File"; } //TYPE IF
            if (EventResource.ResourceType == "LocalFolder")    { TypeLabel.Content = "   Type: Folder"; }  //TYPE IF
            if (EventResource.ResourceType == "RelativeFile")   { TypeLabel.Content = "   Type: File (Child)"; } //TYPE IF
            if (EventResource.ResourceType == "RelativeFolder") { TypeLabel.Content = "   Type: Folder (Child)"; }  //TYPE IF

            //ComboBox TypeComboBox = new ComboBox();
            //TopPanel.Children.Add(TypeComboBox);
            //TypeComboBox.Width = 140;
            //ComboBoxItem FileItem = new ComboBoxItem();
            //ComboBoxItem FolderItem = new ComboBoxItem();
            //ComboBoxItem ChildFileItem = new ComboBoxItem();
            //ComboBoxItem ChildFolderItem = new ComboBoxItem();            
            //FileItem.Content = "File";
            //FolderItem.Content = "Folder";
            //ChildFileItem.Content = "File (Child)";
            //ChildFolderItem.Content = "Folder (Child)";
            //TypeComboBox.Items.Add(FileItem);
            //TypeComboBox.Items.Add(FolderItem);
            //TypeComboBox.Items.Add(ChildFileItem);
            //TypeComboBox.Items.Add(ChildFolderItem);
            //if (EventResource.ResourceType == "LocalFile") { FileItem.IsSelected = true; } //TYPE IF
            //if (EventResource.ResourceType == "LocalFolder") { FolderItem.IsSelected = true; }  //TYPE IF
            //if (EventResource.ResourceType == "RelativeFile") { ChildFileItem.IsSelected = true; } //TYPE IF
            //if (EventResource.ResourceType == "RelativeFolder") { ChildFolderItem.IsSelected = true; }  //TYPE IF
            //string TypeTooltip = "Child Files/Folders are locations that are subfolders from an already set event resource folder. \nUsers don't need to set their location when starting a project, so use these when possible) ";
            //FileItem.ToolTip = TypeTooltip;
            //FolderItem.ToolTip = TypeTooltip;
            //ChildFileItem.ToolTip = TypeTooltip;
            //ChildFolderItem.ToolTip = TypeTooltip;
            //TypeComboBox.ToolTip = TypeTooltip;


            if (EventResource.ResourceType == "RelativeFile" || EventResource.ResourceType == "RelativeFolder")
            {
                ComboBox ComboBox = new();
                TopPanel.Children.Add(ComboBox);
                DockPanel.SetDock(ComboBox, Dock.Left);
                ComboBox.Width = 150;
                ComboBox.Margin = new Thickness(5, 0, 0, 0);

                // Populate the ComboBox initially and set selection based on RelativeKey
                ComboBoxItem initiallySelectedItem = null;
                foreach (WorkshopResource EventResourceX in Library.WorkshopEventResources)
                {
                    if (EventResourceX.ResourceType == "LocalFolder")
                    {
                        ComboBoxItem ComboBoxItem = new ComboBoxItem
                        {
                            Content = EventResourceX.Name,
                            Tag = EventResourceX
                        };
                        ComboBox.Items.Add(ComboBoxItem);

                        // Check if this item should be selected
                        if (EventResource.TargetKey == EventResourceX.WorkshopResourceKey)
                        {
                            initiallySelectedItem = ComboBoxItem;
                        }
                    }
                }

                if (initiallySelectedItem != null)
                {
                    ComboBox.SelectedItem = initiallySelectedItem;
                }

                ComboBox.DropDownOpened += (sender, e) =>
                {
                    var currentlySelected = ComboBox.SelectedItem as ComboBoxItem;
                    string selectedResourceKey = (currentlySelected?.Tag as WorkshopResource)?.WorkshopResourceKey;

                    ComboBox.Items.Clear();
                    ComboBoxItem newItemToSelect = null;

                    foreach (WorkshopResource EventResourceX in Library.WorkshopEventResources)
                    {
                        if (EventResourceX.ResourceType == "LocalFolder")
                        {
                            ComboBoxItem ComboBoxItem = new ComboBoxItem
                            {
                                Content = EventResourceX.Name,
                                Tag = EventResourceX
                            };
                            ComboBox.Items.Add(ComboBoxItem);

                            if (EventResourceX.WorkshopResourceKey == selectedResourceKey)
                            {
                                newItemToSelect = ComboBoxItem;
                            }
                        }
                    }

                    if (newItemToSelect != null)
                    {
                        ComboBox.SelectedItem = newItemToSelect;
                    }
                };

                ComboBox.DropDownClosed += (sender, e) =>
                {
                    if (ComboBox.SelectedItem is ComboBoxItem comboBoxItem && comboBoxItem.Tag is WorkshopResource eventResource)
                    {
                        // Update the reference key when an item is selected
                        EventResource.TargetKey = eventResource.WorkshopResourceKey;
                    }
                    else
                    {
                        // Handle the case when no item is selected
                        EventResource.TargetKey = null;  // or some default value or behavior
                    }
                };
            }


            Button BrowseButton = new();
            TopPanel.Children.Add(BrowseButton);
            DockPanel.SetDock(BrowseButton, Dock.Right);
            BrowseButton.Width = 70;
            BrowseButton.Content = "Browse...";

            Button ButtonMoveUp = new();
            TopPanel.Children.Add(ButtonMoveUp);
            DockPanel.SetDock(ButtonMoveUp, Dock.Right);
            ButtonMoveUp.Width = 85;
            ButtonMoveUp.Content = "Move Up";
            ButtonMoveUp.Margin = new Thickness(0, 0, 10, 0);
            ButtonMoveUp.Click += (sender, e) =>
            {
                if (MainPanel.Parent is DockPanel Parent) 
                {
                    LibraryMan.MoveListItemUp(Library.WorkshopEventResources, EventResource);
                    LibraryMan.MoveDockElementUp(Parent, MainPanel);
                }
                
            };

            Button ButtonMoveDown = new();
            TopPanel.Children.Add(ButtonMoveDown);
            DockPanel.SetDock(ButtonMoveDown, Dock.Right);
            ButtonMoveDown.Width = 95;
            ButtonMoveDown.Content = "Move Down";            
            ButtonMoveDown.Click += (sender, e) =>
            {
                if (MainPanel.Parent is DockPanel Parent)
                {
                    LibraryMan.MoveListItemDown(Library.WorkshopEventResources, EventResource);
                    LibraryMan.MoveDockElementDown(Parent, MainPanel);
                }
                
            };


            DockPanel BrowsePanel = new();
            DockPanel.SetDock(BrowsePanel, Dock.Left);
            BottomPanel.Children.Add(BrowsePanel);
            BrowsePanel.Background = TheBrush;

            CheckBox CheckBox = new();
            if (EventResource.ResourceType == "LocalFile" || EventResource.ResourceType == "LocalFolder")
            {
                TopPanel.Children.Add(CheckBox);
                DockPanel.SetDock(CheckBox, Dock.Left);
                CheckBox.Content = "Require Exact Name";
                CheckBox.Width = 150;
                CheckBox.Margin = new Thickness(5, 0, 0, 0);
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

                //error checking
                ProjectDataItem UserProject = Library.Projects[Library.ProjectsSelector.SelectedIndex];
                if (UserProject == null) { return; }
                if (EventResource.ResourceType == "RelativeFile" || EventResource.ResourceType == "RelativeFolder")
                {
                    if (EventResource.TargetKey == "" || EventResource.TargetKey == null)
                    {
                        MessageBox.Show("First, use the dropdown to select a local folder. Afterwards you can browse for something with a location relative to that folder.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (Library.ProjectsSelector.SelectedIndex < 0 || Library.LibraryTreeOfWorkshops.SelectedItem == null)
                    {
                        MessageBox.Show("Sorry, but you have to select a project first. Theres better ways i could code this, but i'm feeling lazy.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                }


                foreach (ProjectEventResource ProjectResourceData in UserProject.ProjectEventResources)
                {
                    if (EventResource.TargetKey == ProjectResourceData.ResourceKey) { FolderStartPath = ProjectResourceData.Location; }
                }


                if (EventResource.ResourceType == "LocalFile") { LocationString = LibraryMan.GetSelectFileName("Select a File"); }  //TYPE IF
                if (EventResource.ResourceType == "LocalFolder") { LocationString = LibraryMan.GetSelectFolderName("Select a Folder"); }  //TYPE IF                
                if (EventResource.ResourceType == "RelativeFile") { LocationString = LibraryMan.GetSelectedRelativeFilePath("Select a File", FolderStartPath); }  //TYPE IF
                if (EventResource.ResourceType == "RelativeFolder") { LocationString = LibraryMan.GetSelectedRelativeFolderPath("Select a Folder", FolderStartPath); }  //TYPE IF
                //i can fix this problem by using the project path's current local folder path.

                //if (CheckBox.IsChecked == true) 
                //{
                //    if (LocationString != null || LocationString != "")
                //    {
                //        Textbox.Text = LocationString;
                //        EventResource.RequiredName = LocationString;
                //    }
                //}
                else if (LocationString != null || LocationString != "")
                {
                    Textbox.Text = LocationString;
                    EventResource.Location = LocationString;
                }
                //UpdateProjectXML(); //This is just to link me to this, i never intend to call this method here, but this file is getting big and its hard to find v.v
                //SaveWorkshopLibrary(); //same as above
            };

        }
    }
}
