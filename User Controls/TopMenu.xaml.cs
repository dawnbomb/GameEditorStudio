using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Ookii.Dialogs.Wpf;
using PixelWPF;
using Windows.Gaming.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;

namespace GameEditorStudio
{
    // This is everything to do with the Share Menu (The general and Workshop menus)



    // NOTE: The "Workshop" menus, and tool setup screen, directly check load and save to a workshop xml unlike everything else only doing first time checks,
    //to make sure that the program works perfectly fine even with multiple workshops open, or even multiple projects from the same workshop, all at the same time. 

    public partial class TopMenu : System.Windows.Controls.UserControl
    {

        
        public WorkshopData WorkshopData { get; set; }

        public GameLibrary GameLibrary { get; set; }
        public ProjectData ProjectDataItem { get; set; }


        public TopMenu()
        {
            InitializeComponent();


            this.Loaded += new RoutedEventHandler(AfterMenuIsLoaded); //This is the event that's called when the window is loaded. Here, It's required to set the parent window, and also to set the WorkshopName.

            #if DEBUG
            
            #else
            ExtrasMenu.Visibility = Visibility.Collapsed; //Show only in debug mode.
            DebugMenu.Visibility = Visibility.Collapsed; //Show only in debug mode.
            HUDReshade.Visibility = Visibility.Collapsed;  //Show only in debug mode.
            WikiButton.Visibility = Visibility.Collapsed;  
            #endif

        }

        public void AfterMenuIsLoaded(object sender, RoutedEventArgs e)
        {
            MenuOpened();
        }

        public void MenuLibrarySetup(GameLibrary library) 
        {
            GameLibrary = library;
            //This origonally set workshop data every time the menu was opened.

            MenuSaveEditors.IsEnabled = false;
            MenuSaveGameData.IsEnabled = false;
            MenuSaveWorkshopDocuments.IsEnabled = false;
            MenuSaveProjectDocuments.IsEnabled = false;
            NewEditorItem.IsEnabled = false;
            ItemExportEditors.IsEnabled = false;
        }

        public void MenuWorkshopSetup(Workshop workshop) 
        {
            WorkshopData = workshop.WorkshopData;
            ProjectDataItem = WorkshopData.ProjectDataItem;

            if (workshop.IsPreviewMode == true)
            {
                TheMenu.IsEnabled = false;

                MenuInputShortcut.IsEnabled = false;
                MenuOutputShortcut.IsEnabled = false;

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

        private void MenuOpened() //Triggers every time any menu is opened.
        {
            if (GameLibrary != null) //When in the Game Library.  We grab the currently selected workshop.
            {
                WorkshopData = GameLibrary.SelectedWorkshop;
            }
            //The workshop control loads itself as a reference on creation, and never again. So it doesn't need to update. 

            ReloadToolLocations();
        }

        private void MenuOpened(object sender, RoutedEventArgs e)
        {
            MenuOpened();
        }


        //========================File=========================
        

        private void SaveAll(object sender, RoutedEventArgs e)
        {            
            foreach (Command Command in Database.Commands)
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

            foreach (Command Command in Database.Commands)
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

            foreach (Command Command in Database.Commands)
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

            foreach (Command Command in Database.Commands)
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

            foreach (Command Command in Database.Commands)
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
            foreach (Command Command in Database.Commands)
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
            EditorMaker.TheWorkshop = WorkshopData.WorkshopXaml;
            Grid.SetRow(EditorMaker, 2);
            Grid.SetColumn(EditorMaker, 1);
            Grid.SetRowSpan(EditorMaker, 3);
            Grid.SetColumnSpan(EditorMaker, 2);
            WorkshopData.WorkshopXaml.GridBack.Children.Add(EditorMaker);

            EditorMaker.RequestClose += (s, e) =>
            {
                // Assuming the UserControl is directly added to a Grid named 'ParentGrid'
                WorkshopData.WorkshopXaml.GridBack.Children.Remove(EditorMaker);
            };
        }

        

        private void ExportEditors(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null) { return; }
            ExportToGoogleSheets ExportToGoogleSheets = new();
            ExportToGoogleSheets.ExportAllDataTables(WorkshopData.WorkshopXaml);
        }

                


        //========================File=========================
        //========================Tools=========================

        private void ReloadToolLocations() //Appearently, if i remove a tool, the program actually notices immedietly anyway, but i wanted this here anyway because if forgot, and now i want it here to double make sure. ^^;
        {
            LoadDatabase LoadingDatabase = new();
            LoadingDatabase.LoadToolLocations();
        }

        private void OpenToolsWindow(object sender, RoutedEventArgs e)
        {
            ToolsMenu GeneralToolSetup = new(this, WorkshopData);
            GeneralToolSetup.Owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            GeneralToolSetup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            GeneralToolSetup.Show();
        }

        //This had 4 errors for CommandsList during the great changeover from CommonEvent's Commands List to a EventCommands List.
        private void ToolsMenuOpened(object sender, RoutedEventArgs e)
        {
            
            MenuOpened();

            while (ToolsMenu.Items.Count > 2)
            {
                ToolsMenu.Items.RemoveAt(2); 
            }

            LoadWorkshopCommonEvents();

            // Process local events
            foreach (CommonEvent commonEvent in Database.CommonEvents)
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
            split1.SetResourceReference(Separator.StyleProperty, "DashLineForMenu");
            ToolsMenu.Items.Add(split1);

            // Process workshop-specific events
            foreach (CommonEvent commonEvent in Database.CommonEvents)
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
            if (WorkshopData == null) { return; }

            foreach (CommonEvent commonevent in Database.CommonEvents)
            {
                if (WorkshopData.WorkshopCommonEvents.Any(cevent => cevent.Key == commonevent.Key))
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
            
            MenuOpened();

            while (EventsMenu.Items.Count > 2)
            {
                EventsMenu.Items.RemoveAt(2); // Always remove all items except the first two. 
            }

            if (WorkshopData == null)  //WorkshopName == null || WorkshopName == ""
            {                
                ItemEventsSetup.IsEnabled = false;
                return;
            }
            else 
            {
                ItemEventsSetup.IsEnabled = true;
            }

            foreach (Event Event in WorkshopData.WorkshopEvents)
            {
                if (WorkshopData.WorkshopEvents.Count == 0) { break; }

                MenuItem menuItem = new();
                TextBlock menuName = new();

                // Create the run for the text
                Run runname = new Run(Event.DisplayName);
                runname.Foreground = (SolidColorBrush)System.Windows.Application.Current.FindResource("MenuText");  //Brushes.White; //(SolidColorBrush)System.Windows.Application.Current.FindResource("ApplicationText");
                menuName.Inlines.Add(runname);

                // If tooltip exists, set it and add thick underline
                if (!string.IsNullOrEmpty(Event.Tooltip))
                {
                    menuItem.ToolTip = Event.Tooltip;

                    // Create custom underline with thickness 2
                    var underlineBrush = (SolidColorBrush)System.Windows.Application.Current.FindResource("ApplicationText");

                    var underline = new TextDecoration
                    {
                        Location = TextDecorationLocation.Underline,
                        Pen = new Pen(underlineBrush, 2),
                        PenThicknessUnit = TextDecorationUnit.Pixel
                    };

                    runname.TextDecorations = new TextDecorationCollection { underline };
                }

                menuItem.Header = menuName;

                                
                List<string> missingEventCommands = new List<string>();
                List<string> missingEventChildParentLinks = new List<string>();
                List<string> missingTools = new List<string>();                
                List<string> missingProjectResources = new List<string>();
                bool conditionsMet = true;
                bool BrokenEvent = false;
                bool MissingRequirements = false;
                bool CMDTHING = false;

                foreach (EventCommand myCommand in Event.CommandList)
                {
                    //if (myCommand.CMDList.Count != 0) { continue; }

                    // Check for missing tools
                    foreach (Tool tool in myCommand.Command.RequiredToolsList)
                    {
                        if (string.IsNullOrEmpty(tool.Location) || !File.Exists(tool.Location))
                        {
                            conditionsMet = false;
                            MissingRequirements = true;
                            missingTools.Add(tool.DisplayName);
                            
                        }
                    }

                    // Check for unassigned or invalid resource keys
                    int idex = -1;
                    foreach (var resourceKeyPair in myCommand.ResourceKeys)
                    {
                        idex++;
                        if (string.IsNullOrEmpty(resourceKeyPair.Value))
                        {
                            if (myCommand.CMDList.Count != 0 ){ conditionsMet = false; BrokenEvent = true; CMDTHING = true; continue; }
                            if (myCommand.Command.RequiredResourcesList[idex].IsOptional == true) { continue; }

                            conditionsMet = false;
                            BrokenEvent = true;
                            missingEventCommands.Add(myCommand.Command.RequiredResourcesList[idex].Label);
                            continue;
                        }

                        // Find the event resource for the current resource key
                        EventResource eventResource = WorkshopData.WorkshopEventResources.FirstOrDefault(er => er.Key == resourceKeyPair.Value);

                        if (eventResource == null)
                        {
                            conditionsMet = false;
                            BrokenEvent = true;
                            missingEventCommands.Add($"Resource {resourceKeyPair.Key} not defined");
                            continue;
                        }

                        if (eventResource.ResourceType == EventResource.ResourceTypes.CMDText) { continue; } //CMD COMMAND

                        string effectiveResourceLocation = "";

                        // Check if this resource is a child. IE if path is relative or absolute
                        if (eventResource.IsChild == true) //!string.IsNullOrEmpty(eventResource.ParentKey)
                        {
                            // It's a relative resource, find the base resource in project resources
                            ProjectEventResource projectResource = ProjectDataItem.ProjectEventResources.FirstOrDefault(res => res.Key == eventResource.ParentKey); //PARENT key

                            if (projectResource == null )
                            {
                                conditionsMet = false;
                                BrokenEvent = true;
                                missingEventChildParentLinks.Add(eventResource.Name ?? $"Base Resource for {resourceKeyPair.Key}");
                                continue;
                            }
                            else if (string.IsNullOrEmpty(projectResource.Location))
                            {
                                conditionsMet = false;                                
                                MissingRequirements = true;
                                missingProjectResources.Add(eventResource.Name ?? $"Base Resource for {resourceKeyPair.Key}");
                                continue;
                            }

                            // Combine the base resource location with the relative path from the event resource
                            effectiveResourceLocation = Path.Combine(projectResource.Location, eventResource.Location ?? "");
                        }
                        else if (eventResource.IsChild == false) 
                        {
                            // It's an absolute resource, find the corresponding project event resource and use its location
                            ProjectEventResource projectResource = ProjectDataItem.ProjectEventResources.FirstOrDefault(res => res.Key == eventResource.Key); //MY key

                            if (projectResource == null )
                            {
                                conditionsMet = false;
                                MissingRequirements = true;    
                                missingProjectResources.Add(eventResource.Name ?? $"Missing Project Resource {eventResource.Key}");
                                continue;
                            }
                            else if (string.IsNullOrEmpty(projectResource.Location)) //same as above, for now...
                            {
                                conditionsMet = false;
                                MissingRequirements = true;                          
                                missingProjectResources.Add(eventResource.Name ?? $"Missing Project Resource {eventResource.Key}");
                                continue;
                            }

                            effectiveResourceLocation = projectResource.Location;
                        }

                        // Check if the effective resource location exists based on the type (file or folder)
                        if (!ResourceExists(effectiveResourceLocation, eventResource))
                        {
                            conditionsMet = false;
                            MissingRequirements = true;                      
                            missingProjectResources.Add(eventResource.Name ?? $"Resource {resourceKeyPair.Key}");
                        }
                    }





                }

                if (Event.CommandList.Count == 0)
                {
                    conditionsMet = false;
                    BrokenEvent = true;
                }

                if (!conditionsMet)
                {
                    runname.Foreground = Brushes.Gray;

                    if (BrokenEvent == true)
                    {
                        if (Event.CommandList.Count == 0) { Run runX = new Run(" (Broken Event: No Commands)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.GreenYellow; }
                        else if (CMDTHING == true) { Run runX = new Run(" (Broken Event: CMD Command Unassigned)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.GreenYellow; }
                        else if (missingEventCommands.Count != 0 && missingEventChildParentLinks.Count != 0) { Run runX = new Run(" (Broken Event: Unassigned Commands & Child Links)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.GreenYellow; }
                        else if (missingEventCommands.Count != 0) { Run runX = new Run(" (Broken Event: Unassigned Commands)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.GreenYellow; }
                        else if (missingEventChildParentLinks.Count != 0) { Run runX = new Run(" (Broken Event: Unassigned Child-Parent Links)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.GreenYellow; }
                        else { Run runX = new Run(" (Unknown Error)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.GreenYellow; }

                        string missingCommandsText = string.Join("\n, ", missingEventCommands);
                        string missingChildParentLinkText = string.Join("\n, ", missingEventChildParentLinks);                        
                        menuItem.Click += (s, args) =>
                        {
                            string message = "";
                            if (missingEventCommands.Count != 0) { message = message + "Unassigned Event Commands: \n" + missingCommandsText + "\n\n"; }
                            if (missingEventChildParentLinks.Count != 0) { message = message + "Unassigned Child-Parent Links: \n" + missingChildParentLinkText; }

                            if (Event.CommandList.Count == 0) { message = "This event doesn't have any commands."; }
                            if (CMDTHING == true) { message = "This event has one or more commands that use command prompt. Atleast one of them has an empty resource link."; }

                            message = message + "\n\nBroken events are an error for workshop creators to fix. Report this error to whoever made the workshop! D:";

                            MessageBox.Show(message);
                        };


                    }
                    else if (MissingRequirements == true)
                    {
                        if (missingTools.Count != 0 && missingProjectResources.Count != 0) { Run runX = new Run(" (Missing: Tools & Resources)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.OrangeRed; }
                        else if (missingTools.Count != 0) { Run runX = new Run(" (Missing: Tools)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.OrangeRed; }
                        else if (missingProjectResources.Count != 0) { Run runX = new Run(" (Missing: Project Resources)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.OrangeRed; }
                        else { Run runX = new Run(" (Unknown Error)"); menuName.Inlines.Add(runX); runX.Foreground = Brushes.OrangeRed; }

                        string missingToolsText = string.Join("\n", missingTools);
                        string missingProjectResourcesText = string.Join("\n", missingProjectResources);
                        menuItem.Click += (s, args) =>
                        {
                            string message = "";
                            if (missingTools.Count != 0) { message = message + "Missing Tools: \n" + missingToolsText + "\n\n"; }
                            if (missingProjectResources.Count != 0) { message = message + "Missing Project Resources: \n" + missingProjectResourcesText; }
                            
                            MessageBox.Show(message);
                        };


                    }
                    else //Unknown Problem
                    {
                        Run runX = new Run(" (Unknown Error)");
                        menuName.Inlines.Add(runX);
                        runX.Foreground = Brushes.LightGray;
                    }

                    
                }
                else //if event will work perfectly fine.
                {
                    menuItem.Click += (s, args) =>
                    {
                        foreach (EventCommand myCommand in Event.CommandList)
                        {
                            if (myCommand.Command.TheMethod != null)
                            {
                                MethodData actionPack = LibraryGES.TransformKeysToLocations(myCommand.ResourceKeys, WorkshopData.WorkshopEventResources, this, myCommand);
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

            bool ResourceExists(string location, EventResource eventResource)
            {
                
                if (string.IsNullOrEmpty(location))
                    return false;

                if (eventResource.ResourceType == EventResource.ResourceTypes.File)
                    return File.Exists(location);

                if (eventResource.ResourceType == EventResource.ResourceTypes.Folder)
                    return Directory.Exists(location);

                return false;
            }


        }



        private void OpenEventsWindow(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.WorkshopName == null || WorkshopData.WorkshopName == "") { return; }

            EventsMenu EventsControl = new(WorkshopData, this);
            Grid.SetColumnSpan(EventsControl, 99);
            Grid.SetRowSpan(EventsControl, 99);
            Database.GESMain.GESGrid.Children.Add(EventsControl);


            //Previous Code. 
            //var parentWindow = Window.GetWindow(this);
            //if (parentWindow is Workshop workshopWindow)
            //{
            //    if (workshopWindow.WorkshopData.WorkshopName == null || workshopWindow.WorkshopData.WorkshopName == "") { return; }

            //    EventsMenu EventsControl = new(WorkshopData, this);                
            //    Grid.SetColumnSpan(EventsControl, 99);
            //    Grid.SetRowSpan(EventsControl, 99);
            //    workshopWindow.GridBack.Children.Add(EventsControl);
            //}
            //if (parentWindow is GameLibrary LibraryWindow)
            //{
            //    if (LibraryWindow.SelectedWorkshop.WorkshopName == null || LibraryWindow.SelectedWorkshop.WorkshopName == "") { return; }

            //    EventsMenu EventsControl = new(WorkshopData, this);
            //    Grid.SetColumnSpan(EventsControl, 99);
            //    Grid.SetRowSpan(EventsControl, 99);
            //    LibraryWindow.LibraryGrid.Children.Add(EventsControl);
            //}


        }



        //======================================Events================================
        //======================================Shortcuts================================


        private void OpenCrystalEditorFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            CommandMethodsClass.OpenCrystalEditorFolder(MethodData);
        }
        private void OpenDownloadsFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            CommandMethodsClass.OpenDownloadsFolder(MethodData);
        }
        private void OpenProjectFolder(object sender, RoutedEventArgs e)
        {
            MethodData MethodData = new();
            MethodData.GameLibrary = GameLibrary;
            MethodData.WorkshopData = WorkshopData;
            CommandMethodsClass.OpenProjectFolder(MethodData);

        }
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

        

        

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {

            UserSettings UserSettings = new();
            UserSettings.Show();                                    


        }

        private void OpenDevWindow(object sender, RoutedEventArgs e)
        {

            DevEditor Window = new();
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
            PixelWPF.LibraryPixel.Notification("Title Test","Content test");
        }











        //======================================Shortcuts================================
        //======================================Extras================================



        private void TextBoxDecConvert(object sender, TextChangedEventArgs e)
        {
            // Save cursor position
            int caret = DecBox.CaretIndex;
            string originalText = DecBox.Text;

            if (ulong.TryParse(originalText.Trim(), out ulong value))
            {
                // Don't update the box that triggered this
                if (!DecBox.IsFocused)
                {
                    DecBox.Text = value.ToString();
                    DecBox.CaretIndex = caret;
                }

                byte[] littleEndian = BitConverter.GetBytes(value);
                int actualLen = littleEndian.Length;
                while (actualLen > 1 && littleEndian[actualLen - 1] == 0) actualLen--;

                var trimmedLittle = littleEndian.Take(actualLen).ToArray();

                if (!HexBoxLil.IsFocused)
                {
                    HexBoxLil.Text = string.Concat(trimmedLittle.Select(b => b.ToString("X2")));
                }

                if (!HexBoxBig.IsFocused)
                {
                    var big = trimmedLittle.Reverse().ToArray();
                    HexBoxBig.Text = string.Concat(big.Select(b => b.ToString("X2")));
                }

                // Clear backgrounds
                DecBox.ClearValue(TextBox.BackgroundProperty);
                HexBoxLil.ClearValue(TextBox.BackgroundProperty);
                HexBoxBig.ClearValue(TextBox.BackgroundProperty);
            }
            else
            {
                DecBox.Background = Brushes.Red;

                if (originalText == "") 
                {
                    DecBox.ClearValue(TextBox.BackgroundProperty);
                }
                
            }
        }

        private void TextBoxHexLilConvert(object sender, TextChangedEventArgs e)
        {
            int caret = HexBoxLil.CaretIndex;
            string text = HexBoxLil.Text.Trim();

            try
            {
                if (text.Length % 2 != 0)
                    text = "0" + text; // pad odd-length

                byte[] bytes = Enumerable.Range(0, text.Length / 2)
                    .Select(i => Convert.ToByte(text.Substring(i * 2, 2), 16))
                    .ToArray();

                if (bytes.Length > 8)
                    throw new OverflowException();

                byte[] padded = new byte[8];
                Array.Copy(bytes, padded, bytes.Length);
                ulong value = BitConverter.ToUInt64(padded, 0);

                if (!DecBox.IsFocused)
                    DecBox.Text = value.ToString();

                if (!HexBoxBig.IsFocused)
                {
                    var big = bytes.Reverse().ToArray();
                    HexBoxBig.Text = string.Concat(big.Select(b => b.ToString("X2")));
                }

                DecBox.ClearValue(TextBox.BackgroundProperty);
                HexBoxLil.ClearValue(TextBox.BackgroundProperty);
                HexBoxBig.ClearValue(TextBox.BackgroundProperty);
            }
            catch
            {
                HexBoxLil.Background = Brushes.Red;
            }

            HexBoxLil.CaretIndex = caret;
        }

        private void TextBoxHexBigConvert(object sender, TextChangedEventArgs e)
        {
            int caret = HexBoxBig.CaretIndex;
            string text = HexBoxBig.Text.Trim();

            try
            {
                if (text.Length % 2 != 0)
                    text = "0" + text;

                byte[] bytes = Enumerable.Range(0, text.Length / 2)
                    .Select(i => Convert.ToByte(text.Substring(i * 2, 2), 16))
                    .ToArray();

                if (bytes.Length > 8)
                    throw new OverflowException();

                var reversed = bytes.Reverse().ToArray();
                byte[] padded = new byte[8];
                Array.Copy(reversed, padded, reversed.Length);
                ulong value = BitConverter.ToUInt64(padded, 0);

                if (!DecBox.IsFocused)
                    DecBox.Text = value.ToString();

                if (!HexBoxLil.IsFocused)
                {
                    HexBoxLil.Text = string.Concat(reversed.Select(b => b.ToString("X2")));
                }

                DecBox.ClearValue(TextBox.BackgroundProperty);
                HexBoxLil.ClearValue(TextBox.BackgroundProperty);
                HexBoxBig.ClearValue(TextBox.BackgroundProperty);
            }
            catch
            {
                HexBoxBig.Background = Brushes.Red;
            }

            HexBoxBig.CaretIndex = caret;
        }



        //======================================TopRight================================

        private void OpenReshade(object sender, RoutedEventArgs e)
        {  
            string colorsplash = LibraryGES.ApplicationLocation + "/Other/ColorSplash/Reshade Color Splash.exe";
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = colorsplash,                    
                    UseShellExecute = true // Needed if launching .exe directly (not using redirected IO)
                },
                EnableRaisingEvents = true
            };
            process.Start();  
        }

        private void OpenWiki(object sender, RoutedEventArgs e)
        {
            Tutorial f2 = new Tutorial();
            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            f2.Show();   
            
        }

        private void OpenDiscord(object sender, RoutedEventArgs e)
        {
            string url = "https://discord.gg/mhrZqjRyKx";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        private void WebsiteButton(object sender, RoutedEventArgs e)
        {
            string url = "https://www.crystalmods.com/";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        
        }

        private void GithubButton(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/dawnbomb/GameEditorStudio";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        private void OpenPatchnotes(object sender, RoutedEventArgs e)
        {
            PixelWPF.Patchnotes patchnotes = new();
            patchnotes.LoadPatchnotes(LibraryGES.ApplicationLocation + "\\Other\\Patchnotes.txt");
            patchnotes.Show();
        }

        private void OpenPatchnotestxt(object sender, MouseButtonEventArgs e)
        {
            string filePath = Path.Combine(LibraryGES.ApplicationLocation, "Other", "Patchnotes.txt");

            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = filePath,
                    UseShellExecute = true // opens with default program
                });
            }
        }

        
    }
}
