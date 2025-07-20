using Ookii.Dialogs.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using Window = System.Windows.Window;

namespace GameEditorStudio
{
    // This is everything to do with the Share Menu (The general and Workshop menus)



    // NOTE: The "Workshop" menus, and tool setup screen, directly check load and save to a workshop xml unlike everything else only doing first time checks,
    //to make sure that the program works perfectly fine even with multiple workshops open, or even multiple projects from the same workshop, all at the same time. 

    public partial class MainMenu : System.Windows.Controls.UserControl
    {

        
        public WorkshopData WorkshopData { get; set; }
        public List<Event> Events { get; set; } = new();

        public List<WorkshopResource> EventResources { get; set; }

        public string WorkshopName { get; set; } = "";
        public GameLibrary GameLibrary { get; set; }

        public ProjectDataItem ProjectDataItem { get; set; }


        public MainMenu()
        {
            InitializeComponent();


            this.Loaded += new RoutedEventHandler(LoadEvent); //This is the event that's called when the window is loaded. Here, It's required to set the parent window, and also to set the WorkshopName.

            #if DEBUG
            
            #else
            ExtrasMenu.Visibility = Visibility.Collapsed; //Show only in debug mode.
            DebugMenu.Visibility = Visibility.Collapsed; //Show only in debug mode.
            #endif
        }

        public void LoadEvent(object sender, RoutedEventArgs e)
        {            

            var parentWindow = Window.GetWindow(this);
            if (parentWindow is GameLibrary GameLibraryWindow)
            {
                GameLibrary = GameLibraryWindow;
                GameLibrary.MainMenu = this;
                EventResources = GameLibrary.WorkshopEventResources;
            }

            if (parentWindow is Workshop WorkshopWindow)
            {
                WorkshopData = WorkshopWindow.MyDatabase;
                WorkshopName = WorkshopWindow.WorkshopName;
                ProjectDataItem = WorkshopWindow.ProjectDataItem;
                LoadEventResources();

                if (WorkshopWindow.IsPreviewMode == true)
                {
                    MenuInputShortcut.IsEnabled = false;
                    MenuOutputShortcut.IsEnabled = false;
                }

                LoadEventsFromXML();

                if (WorkshopWindow.IsPreviewMode == true) 
                {
                    TheMenu.IsEnabled = false;

                    MenuSaveEditors.IsEnabled = false;
                    MenuSaveGameData.IsEnabled = false;
                    MenuSaveWorkshopDocuments.IsEnabled = false;
                    MenuSaveProjectDocuments.IsEnabled = false;
                    NewEditorItem.IsEnabled = false;
                    ItemExportEditors.IsEnabled = false;

                    WorkshopMenu.IsEnabled = false;
                    ToolsMenu.IsEnabled = false;
                    EventsMenu.IsEnabled = false;
                    ShortcutsMenu.IsEnabled = false;
                    ExtrasMenu.IsEnabled = false;


                }
                
            }




        }

        public void LoadEventResources()
        {
            List<WorkshopResource> EventResources2 = new();
            EventResources = EventResources2;

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
                    if (xmlEvent.Element("ResourceType")?.Value == "File" && xmlEvent.Element("PathType")?.Value == "FullPath") { EventResource.ResourceType = "LocalFile"; }
                    if (xmlEvent.Element("ResourceType")?.Value == "Folder" && xmlEvent.Element("PathType")?.Value == "FullPath") { EventResource.ResourceType = "LocalFolder"; }
                    if (xmlEvent.Element("ResourceType")?.Value == "File" && xmlEvent.Element("PathType")?.Value == "PartialPath") { EventResource.ResourceType = "RelativeFile"; }
                    if (xmlEvent.Element("ResourceType")?.Value == "Folder" && xmlEvent.Element("PathType")?.Value == "PartialPath") { EventResource.ResourceType = "RelativeFolder"; }

                    EventResources.Add(EventResource);
                }


            };


        }

        //========================File=========================

        private void FileMenuOpened(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null)
            {
                MenuSaveEditors.Foreground = Brushes.Gray;
                MenuSaveGameData.Foreground = Brushes.Gray;                
                MenuSaveWorkshopDocuments.Foreground = Brushes.Gray;
                MenuSaveProjectDocuments.Foreground = Brushes.Gray;
                NewEditorItem.Foreground = Brushes.Gray;
                ItemExportEditors.Foreground = Brushes.Gray;

                MenuSaveEditors.IsEnabled = false;
                MenuSaveGameData.IsEnabled = false;
                MenuSaveWorkshopDocuments.IsEnabled = false;
                MenuSaveProjectDocuments.IsEnabled = false;
                NewEditorItem.IsEnabled = false;
                ItemExportEditors.IsEnabled = false;
            }
        }

        private void SaveAll(object sender, RoutedEventArgs e)
        {            
            foreach (Command Command in TrueDatabase.Commands)
            {
                if (Command.Key == "638835921950406560-246176526-807942630") //SaveAll / Everything
                {
                    MethodData ActionPack = new();
                    ActionPack.Command = Command;
                    ActionPack.mainMenu = this;

                    Command.TheMethod?.Invoke(ActionPack);  // Pass 'command' as the argument
                    return;
                }
            }
            
        }

        private void SaveEditors(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null)
            {
                return;
            }

            foreach (Command Command in TrueDatabase.Commands)
            {
                if (Command.Key == "638835921950407398-717630473-427782251") //SaveEditors
                {
                    MethodData ActionPack = new();
                    ActionPack.Command = Command;
                    ActionPack.mainMenu = this;

                    Command.TheMethod?.Invoke(ActionPack);  // Pass 'command' as the argument
                    return;
                }
            }            
        }

        private void SaveGameData(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null) 
            {
                return;
            }

            //MethodData methodData = new MethodData();
            //methodData.WorkshopData = WorkshopData; // Set the WorkshopData property to the current workshop data
            //CommandMethodsClass.SaveGameData(methodData);

            foreach (Command Command in TrueDatabase.Commands)
            {
                if (Command.Key == "638835921950407433-13988325-250675840") //SaveGameData
                {
                    MethodData ActionPack = new();
                    ActionPack.Command = Command;
                    ActionPack.mainMenu = this;

                    Command.TheMethod?.Invoke(ActionPack);  // Pass 'command' as the argument
                    return;
                }
            }
        }

        private void SaveWorkshopDocuments(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null)
            {
                return;
            }

            foreach (Command Command in TrueDatabase.Commands)
            {
                if (Command.Key == "638835921950407461-756556716-682593786") //SaveDocumentsWorkshop
                {
                    MethodData ActionPack = new();
                    ActionPack.Command = Command;
                    ActionPack.mainMenu = this;

                    Command.TheMethod?.Invoke(ActionPack);  // Pass 'command' as the argument
                    return;
                }
            }
            
        }

        private void SaveProjectDocuments(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null)
            {
                return;
            }

            foreach (Command Command in TrueDatabase.Commands)
            {
                if (Command.Key == "638835921950407528-367118138-951819106") //SaveDocumentsProject
                {
                    MethodData ActionPack = new();
                    ActionPack.Command = Command;
                    ActionPack.mainMenu = this;

                    Command.TheMethod?.Invoke(ActionPack);  // Pass 'command' as the argument
                    return;
                }
            }
            //Database.TheDocumentsUserControl.SaveAllDocumentsProject();
        }
                

        private void SaveEvents(object sender, RoutedEventArgs e)
        {
            foreach (Command Command in TrueDatabase.Commands)
            {
                if (Command.Key == "638835921950407554-64869249-237672364") //SaveEvents
                {
                    MethodData ActionPack = new();
                    ActionPack.Command = Command;
                    ActionPack.mainMenu = this;

                    Command.TheMethod?.Invoke(ActionPack);  // Pass 'command' as the argument
                    return;
                }
            }
        }

        private void FileNewEditor(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null) { return; }

            UserControlEditorCreator EditorMaker = new UserControlEditorCreator();
            Grid.SetRow(EditorMaker, 2);
            Grid.SetColumn(EditorMaker, 1);
            Grid.SetRowSpan(EditorMaker, 3);
            Grid.SetColumnSpan(EditorMaker, 2);
            WorkshopData.Workshop.GridBack.Children.Add(EditorMaker);

            EditorMaker.RequestClose += (s, e) =>
            {
                // Assuming the UserControl is directly added to a Grid named 'ParentGrid'
                WorkshopData.Workshop.GridBack.Children.Remove(EditorMaker);
            };
        }

        

        private void ExportEditors(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null) { return; }
            ExportToGoogleSheets ExportToGoogleSheets = new();
            ExportToGoogleSheets.ExportAllDataTables(WorkshopData.Workshop);
        }



        public void LoadEventsFromXML() // Assuming 'Commands' is available in this context
        {            

            string eventsDirectory = Path.Combine(LibraryMan.ApplicationLocation, "Workshops", WorkshopName, "Events");
            if (!Directory.Exists(eventsDirectory)) { return; }


            List<string> EventsListLoadOrder = new();
            string EventsOrderText = LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Events\\" + "LoadOrder.txt";
            if (File.Exists(EventsOrderText))
            {
                string[] lines = File.ReadAllLines(EventsOrderText);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        EventsListLoadOrder.Add(line);
                    }
                }
            }

            foreach (string eventFolder in EventsListLoadOrder) //Load all known events
            {
                LoadEvent(eventFolder);
                
            }
            
            //string[] EditorFolderNames = Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
            
            string[] allEventFolders = Directory.GetDirectories(eventsDirectory, "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
            foreach (string eventFolder in allEventFolders) //Load all unknown events?
            {
                if (!EventsListLoadOrder.Contains(eventFolder))
                {
                    LoadEvent(eventFolder);
                }
            }

            void LoadEvent(string eventFolder) 
            {
                string eventFile = Path.Combine(eventsDirectory + "\\" + eventFolder, "Event.xml");

                XDocument doc = XDocument.Load(eventFile);
                foreach (var xmlEvent in doc.Descendants("Event"))
                {
                    Event newEvent = new Event
                    {
                        DisplayName = xmlEvent.Element("Name")?.Value ?? "New Event",
                        Note = xmlEvent.Element("Note")?.Value ?? string.Empty,
                        Notepad = xmlEvent.Element("Notepad")?.Value ?? string.Empty,
                        CommandList = new List<EventCommand>()
                    };

                    var commandElements = xmlEvent.Descendants("Command");
                    foreach (var commandElement in commandElements)
                    {
                        string commandKey = commandElement.Element("Key")?.Value;
                        if (!string.IsNullOrEmpty(commandKey))
                        {
                            Command matchingCommand = TrueDatabase.Commands.FirstOrDefault(cmd => cmd.Key == commandKey);
                            if (matchingCommand != null)
                            {
                                EventCommand myCommand = new EventCommand
                                {
                                    Command = matchingCommand,
                                    ResourceKeys = new Dictionary<int, string>()
                                };

                                // Initialize the dictionary with default values
                                int resourceKeyIndex = 1;
                                foreach (CommandResource Aresource in matchingCommand.RequiredResourcesList) //var resource in matchingCommand.Resources
                                {
                                    myCommand.ResourceKeys.Add(resourceKeyIndex++, ""); // Initialize with empty or default values
                                }

                                // Update the dictionary with actual values from XML
                                resourceKeyIndex = 1; // Reset index for actual values
                                var resourceElements = commandElement.Descendants("Resource");
                                foreach (var resourceElement in resourceElements)
                                {
                                    string resourceKey = resourceElement.Element("Key")?.Value;
                                    if (!string.IsNullOrEmpty(resourceKey))
                                    {
                                        if (myCommand.ResourceKeys.ContainsKey(resourceKeyIndex))
                                        {
                                            myCommand.ResourceKeys[resourceKeyIndex] = resourceKey;
                                        }
                                        else
                                        {
                                            myCommand.ResourceKeys.Add(resourceKeyIndex, resourceKey); // In case there are more XML resources than defaults
                                        }
                                        resourceKeyIndex++;
                                    }
                                }

                                newEvent.CommandList.Add(myCommand);
                            }
                        }
                    }

                    Events.Add(newEvent);
                }
            }

            
        }


        //========================File=========================
        //========================Tools=========================

        private void ReloadToolLocations() //Appearently, if i remove a tool, the program actually notices immedietly anyway, but i wanted this here anyway because if forgot, and now i want it here to double make sure. ^^;
        {
            ImportFromGoogle ImportFromGoogleSheets = new();
            ImportFromGoogleSheets.LoadToolLocations();
        }

        private void OpenToolsWindow(object sender, RoutedEventArgs e)
        {
            ToolsMenu GeneralToolSetup = new(this, WorkshopName);
            GeneralToolSetup.Owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            GeneralToolSetup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            GeneralToolSetup.Show();
        }

        //This had 4 errors for CommandsList during the great changeover from CommonEvent's Commands List to a EventCommands List.
        private void ToolsMenuOpened(object sender, RoutedEventArgs e)
        {
            ReloadToolLocations();

            var parentWindow = Window.GetWindow(this);
            if (parentWindow is GameLibrary gameLibraryWindow)
            {
                WorkshopName = gameLibraryWindow.WorkshopName;
            }
            if (parentWindow is Workshop workshopWindow)
            {
                WorkshopName = workshopWindow.WorkshopName;
            }

            while (ToolsMenu.Items.Count > 1)
            {
                ToolsMenu.Items.RemoveAt(1); // Always remove the second item, leaving the first one
            }

            LoadWorkshopCommonEvents();

            // Process local events
            foreach (CommonEvent commonEvent in TrueDatabase.CommonEvents)
            {
                if (!commonEvent.Local)
                {
                    continue;
                }

                MenuItem menuItem = new MenuItem
                {
                    Header = commonEvent.DisplayName
                };

                // Check for any invalid tools across all commands in the event
                bool anyToolInvalid = commonEvent.MyCommands.Any(command =>
                    command?.RequiredToolsList.Any(tool => string.IsNullOrEmpty(tool.Location) || !File.Exists(tool.Location)) ?? false
                );

                if (anyToolInvalid)
                {
                    menuItem.Foreground = Brushes.Gray;
                    menuItem.Click += (s, args) => MessageBox.Show("At least one tool this event uses was not found in its last known location, nor was it anywhere inside the Tools folder.\n\nPlease go to Tools Setup and add the tool!");
                }
                else
                {
                    menuItem.Click += (s, args) =>
                    {
                        // Execute all commands for this event
                        foreach (Command command in commonEvent.MyCommands)
                        {
                            MethodData ActionPack = new();
                            ActionPack.Command = command;
                            //used to send project data as well?
                            command?.TheMethod(ActionPack);
                        }
                    };
                }

                ToolsMenu.Items.Add(menuItem);
            }

            // Add separator
            Separator split1 = new();
            split1.SetResourceReference(Separator.StyleProperty, "KeySeperator");
            ToolsMenu.Items.Add(split1);

            // Process workshop-specific events
            foreach (CommonEvent commonEvent in TrueDatabase.CommonEvents)
            {
                if (!commonEvent.Workshop || commonEvent.Local)
                {
                    continue;
                }

                MenuItem menuItem = new MenuItem
                {
                    Header = commonEvent.DisplayName
                };

                // Check for any invalid tools across all commands in the event
                bool anyToolInvalid = commonEvent.MyCommands.Any(eventCommand =>
                    eventCommand?.RequiredToolsList.Any(tool => string.IsNullOrEmpty(tool.Location) || !File.Exists(tool.Location)) ?? false
                );

                if (anyToolInvalid)
                {
                    menuItem.Foreground = Brushes.Gray;
                    menuItem.Click += (s, args) => MessageBox.Show("At least one tool this event uses was not found in its last known location, nor was it anywhere inside the Tools folder.\n\nPlease go to Tools Setup and add the tool!");
                }
                else
                {
                    menuItem.Click += (s, args) =>
                    {
                        // Execute all commands for this event
                        foreach (Command command in commonEvent.MyCommands)
                        {
                            MethodData ActionPack = new();
                            ActionPack.Command = command;
                            command?.TheMethod(ActionPack);
                        }
                    };
                }

                ToolsMenu.Items.Add(menuItem);
            }
        }

        public void LoadWorkshopCommonEvents() //Dictionary<CommandName, Command> Commands
        {
            if (!File.Exists(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Common Events.xml"))
            {
                return;
            }

            List<string> ListOfCommonEventKeys = new();

            XElement xml = XElement.Load(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Common Events.xml");
            foreach (XElement XCommonEvent in xml.Descendants("CommonEvent"))
            {
                string TheKey = XCommonEvent.Element("Key")?.Value;
                ListOfCommonEventKeys.Add(TheKey);
            }

            foreach (CommonEvent commonevent in TrueDatabase.CommonEvents)
            {
                if (ListOfCommonEventKeys.Any(key => key == commonevent.Key))
                {
                    commonevent.Workshop = true;
                }
                else
                {
                    commonevent.Workshop = false;
                }

            }



        }

        //========================Tools=========================
        //========================Events=========================

        private void EventsMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ReloadToolLocations();

            while (EventsMenu.Items.Count > 1)
            {
                EventsMenu.Items.RemoveAt(1); // Always remove all items except the first one. (It opens the event manager)
            }

            if (WorkshopName == null || WorkshopName == "")
            {
                ItemEventsSetup.Foreground = Brushes.Gray;
            }
            else 
            { 
                ItemEventsSetup.Foreground = Brushes.White; 
            }

            foreach (Event Event in Events)
            {
                MenuItem menuItem = new MenuItem
                {
                    Header = Event.DisplayName
                };

                List<string> missingTools = new List<string>();
                List<string> missingResources = new List<string>();
                bool conditionsMet = true;
                bool ProjectResourceMissingFromLastKnownLocation = false;

                foreach (EventCommand myCommand in Event.CommandList)
                {
                    // Check for missing tools
                    foreach (Tool tool in myCommand.Command.RequiredToolsList)
                    {
                        if (string.IsNullOrEmpty(tool.Location) || !File.Exists(tool.Location))
                        {
                            missingTools.Add(tool.DisplayName);
                            conditionsMet = false;
                        }
                    }

                    // Check for unassigned or invalid resource keys
                    int idex = -1;
                    foreach (var resourceKeyPair in myCommand.ResourceKeys)
                    {
                        idex++;
                        if (string.IsNullOrEmpty(resourceKeyPair.Value))
                        {
                            if (myCommand.Command.RequiredResourcesList[idex].IsOptional == true) { continue; }
                            conditionsMet = false;
                            missingResources.Add(myCommand.Command.RequiredResourcesList[idex].Label);
                            //missingResources.Add(resourceKeyPair.Key.ToString());
                            continue;
                        }

                        // Find the event resource for the current resource key
                        WorkshopResource eventResource = EventResources.FirstOrDefault(er => er.WorkshopResourceKey == resourceKeyPair.Value);

                        if (eventResource == null)
                        {
                            conditionsMet = false;
                            missingResources.Add($"Resource {resourceKeyPair.Key} not defined");
                            continue;
                        }

                        string effectiveResourceLocation = "";

                        // Check if this resource is relative or absolute
                        if (!string.IsNullOrEmpty(eventResource.TargetKey))
                        {
                            // It's a relative resource, find the base resource in project resources
                            ProjectEventResource baseResource = ProjectDataItem.ProjectEventResources
                                .FirstOrDefault(res => res.ResourceKey == eventResource.TargetKey);

                            if (baseResource == null || string.IsNullOrEmpty(baseResource.Location))
                            {
                                conditionsMet = false;
                                missingResources.Add(eventResource.Name ?? $"Base Resource for {resourceKeyPair.Key}");
                                continue;
                            }

                            // Combine the base resource location with the relative path from the event resource
                            effectiveResourceLocation = Path.Combine(baseResource.Location, eventResource.Location ?? "");
                        }
                        else
                        {
                            // It's an absolute resource, find the corresponding project event resource and use its location
                            ProjectEventResource projectResource = ProjectDataItem.ProjectEventResources
                                .FirstOrDefault(res => res.ResourceKey == eventResource.WorkshopResourceKey);

                            if (projectResource == null || string.IsNullOrEmpty(projectResource.Location))
                            {
                                conditionsMet = false;
                                missingResources.Add(eventResource.Name ?? $"Missing Project Resource {eventResource.WorkshopResourceKey}");
                                continue;
                            }

                            effectiveResourceLocation = projectResource.Location;
                        }

                        // Check if the effective resource location exists based on the type (file or folder)
                        if (!ResourceExists(effectiveResourceLocation, eventResource.ResourceType))
                        {
                            conditionsMet = false;
                            ProjectResourceMissingFromLastKnownLocation = true;
                            missingResources.Add(eventResource.Name ?? $"Resource {resourceKeyPair.Key}");
                        }
                    }





                }

                if (Event.CommandList.Count == 0)
                {
                    conditionsMet = false;
                }

                if (!conditionsMet)
                {
                    string TheMissingTools = "Missing Tools: ";
                    string TheMissingResources = "Missing Resources: "; //Later use these to make the message when clicked show only stuff thats actually missing.

                    if (Event.CommandList.Count == 0) { menuItem.Header = menuItem.Header + " (No Commands)"; }
                    else if (ProjectResourceMissingFromLastKnownLocation == true) { menuItem.Header = menuItem.Header + " (Missing Project Resource!)"; }
                    else if (missingTools.Count != 0 && missingResources.Count != 0) { menuItem.Header = menuItem.Header + " (Missing Tools & Resources)"; }
                    else if (missingTools.Count != 0) { menuItem.Header = menuItem.Header + " (Missing Tools)"; }
                    else if (missingResources.Count != 0) { menuItem.Header = menuItem.Header + " (Unassigned Event Resources)"; }
                                   
                    else { menuItem.Header = menuItem.Header + " (?????)"; }

                    menuItem.Foreground = Brushes.Gray;
                    string missingToolsText = string.Join(", ", missingTools);
                    string missingResourcesText = string.Join(", ", missingResources);
                    menuItem.Click += (s, args) =>
                    {
                        string message = "Missing Tools: " + missingToolsText +
                                         "\nMissing Resources: " + missingResourcesText +
                                         "\n\nPS: Don't trust this poorly made error message. \nCheck everything, including the event data itself. ";
                        MessageBox.Show(message);
                    };
                }
                else
                {
                    menuItem.Click += (s, args) =>
                    {
                        foreach (EventCommand myCommand in Event.CommandList)
                        {
                            if (myCommand.Command.TheMethod != null)
                            {
                                MethodData actionPack = LibraryMan.TransformKeysToLocations(myCommand.ResourceKeys, EventResources, this, myCommand);
                                actionPack.mainMenu = this;
                                myCommand.Command.TheMethod(actionPack);
                            }
                            else
                            {
                                MessageBox.Show($"The command '{myCommand.Command.DisplayName}' does not have an associated action. Cannot execute this event.");
                            }
                        }
                    };
                }

                EventsMenu.Items.Add(menuItem);
            }

            bool ResourceExists(string location, string resourceType)
            {
                if (string.IsNullOrEmpty(location))
                    return false;

                if (resourceType.EndsWith("File"))
                    return File.Exists(location);

                if (resourceType.EndsWith("Folder"))
                    return Directory.Exists(location);

                return false;
            }


        }



        private void OpenEventsWindow(object sender, RoutedEventArgs e)
        {

            var parentWindow = Window.GetWindow(this);


            if (parentWindow is Workshop workshopWindow)
            {
                if (workshopWindow.WorkshopName == null || workshopWindow.WorkshopName == "") { return; }
                EventsMenu EventsSetup = new(WorkshopName, Events, EventResources, this);
                EventsSetup.Owner = workshopWindow;
                EventsSetup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                EventsSetup.Show();
            }
            if (parentWindow is GameLibrary LibraryWindow)
            {
                if (LibraryWindow.WorkshopName == null || LibraryWindow.WorkshopName == "") { return; }
                EventsMenu EventsWindow = new(WorkshopName, Events, EventResources, this);
                EventsWindow.Owner = LibraryWindow;
                EventsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                EventsWindow.Show();
            }


        }



        //======================================Events================================
        //======================================Shortcuts================================




        private void OpenInputFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = GameLibrary;
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenInputFolder(MethodData);

        }

        private void OpenOutputFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = GameLibrary;
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenOutputFolder(MethodData);
        }

        private void OpenWorkshopFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = GameLibrary;
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenWorkshopFolder(MethodData);

        }

        private void OpenDownloadsFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            CommandMethodsClass.OpenDownloadsFolder(MethodData);
        }

        private void OpenCrystalEditorFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            CommandMethodsClass.OpenCrystalEditorFolder(MethodData);
        }

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {

            UserSettings UserSettings = new();
            UserSettings.Show();                                    


        }

        private void OpenDevWindow(object sender, RoutedEventArgs e)
        {

            DevTools Window = new();
            Window.Show();                                            


        }

        private void Set1080P(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            parentWindow.Width = 1920;
            parentWindow.Height = 1040;
        }

        private void OpenHexTest(object sender, RoutedEventArgs e)
        {
            HexTest HexTest = new HexTest();
            HexTest.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            HexTest.Show();

        }

        private void DebugNotification(object sender, RoutedEventArgs e)
        {
            LibraryMan.Notification("Title Test","Content test");
        }











        //======================================Shortcuts================================
        //======================================Extras================================





    }
}
