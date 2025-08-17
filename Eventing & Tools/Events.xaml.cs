using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.Linq;
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
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using Windows.Management.Deployment.Preview;
using Windows.Media.Devices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static GameEditorStudio.EventsMenu;

namespace GameEditorStudio
{
    

    public partial class EventsMenu : Window
    {
        //int Commands = 0;
        WorkshopData workshopData { get; set; } 

        public List<Event> WorkshopEvents { get; set; }
        public List<EventResource> EventResources { get; set; }
        public TopMenu MainMenu { get; set; }

        public Event CurrentEvent { get; set; }
        //public Dictionary<string, ICommandAction> commandDictionary = new Dictionary<string, ICommandAction>();


        //List<Event> Events; 

        public EventsMenu(WorkshopData workshopDataI, TopMenu TheMainMenu)
        {
            InitializeComponent();

            this.workshopData = workshopDataI;
            this.Title = " Events - " + workshopData.WorkshopName ;
            WorkshopEvents = workshopData.WorkshopEvents;
            EventResources = workshopData.WorkshopEventResources;
            MainMenu = TheMainMenu;

            EventResourceManager.SetupEventResourcesUI(workshopData, this);

            //Events = new ObservableCollection<Event>(EventsList);
            //EventListTree.ItemsSource = Events;
            foreach (Event Event in WorkshopEvents)
            {  
                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Header = Event.DisplayName;
                EventListTree.Items.Add(treeViewItem);
                treeViewItem.Tag = Event;
            }

            //commandDictionary["Popup"] = new PopupCommand();
            //commandDictionary["PopupBeta"] = new PopupBetaCommand();
            if (EventListTree.Items.Count > 0)
            {
                TreeViewItem item = (EventListTree.Items[0]) as TreeViewItem;

                if (item != null)
                {
                    item.IsSelected = true;
                    item.Focus();
                }
            }


        }

        public void RefreshEventUI()
        {
            if (EventListTree.SelectedItem != null)
            {
                TreeViewItem treeViewItem = EventListTree.SelectedItem as TreeViewItem;
                treeViewItem.IsSelected = false;
                treeViewItem.IsSelected = true; //Refresh the selected item, so the UI updates properly.
            }  

        }

        //==========================Top Menu=================================

        private void CreateNewEvent(object sender, RoutedEventArgs e)
        {
            Event TheEvent = new();
            TheEvent.DisplayName = "New Event";
            WorkshopEvents.Add(TheEvent);

            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = TheEvent.DisplayName;
            EventListTree.Items.Add(treeViewItem);
            treeViewItem.Tag = TheEvent;
            //EventListTree

            treeViewItem.IsSelected = true; //Select the new event.

        }

        private void DeleteEventButton(object sender, RoutedEventArgs e)
        {            

            if (EventListTree.SelectedItem == null)
            {
                return;
            }

            TreeViewItem Itemm = EventListTree.SelectedItem as TreeViewItem;
            Event Event = Itemm.Tag as Event;

            WorkshopEvents.Remove(CurrentEvent);
            EventListTree.Items.Remove(EventListTree.SelectedItem);

        }

        private void RenameEvent(object sender, KeyEventArgs e) //The event name textbox
        {
            if (CurrentEvent == null) { return; }

            if (e.Key == Key.Enter)
            {
                CurrentEvent.DisplayName = EventNameBox.Text;
                TreeViewItem itemm = EventListTree.SelectedItem as TreeViewItem;
                itemm.Header = CurrentEvent.DisplayName;

            }
        }

        private void EventTooltipTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentEvent == null) { return; }

            CurrentEvent.Tooltip = EventTooltipBox.Text;
        }

        private void RunEventButton(object sender, RoutedEventArgs e) //The run this event button.
        {
            if (CurrentEvent == null || CurrentEvent.CommandList == null || CurrentEvent.CommandList.Count == 0)
            {
                return;
            }

            foreach (EventCommand eventCommand in CurrentEvent.CommandList)
            {                

                if (eventCommand.Command.TheMethod == null) { continue; }
                
                MethodData ActionPack = LibraryGES.TransformKeysToLocations(eventCommand.ResourceKeys, EventResources, MainMenu, eventCommand);
                ActionPack.Command.TheMethod(ActionPack);

            }

        }

        //=======================================================================================
        //=====================================Command Panels====================================
        //=======================================================================================


        private Border CreateFakeCommandPanel() //This is only because i want this window to look like the RPG maker eventing window. :>, not actually meant to hold any commands or do anything.
        {
            Border commandBorder = new();            

            DockPanel commandDockPanel = new DockPanel();
            commandBorder.Child = commandDockPanel;
            commandBorder.Margin = new(10, 10, 10, 0);
            DockPanel.SetDock(commandBorder, Dock.Top);


            Label commandLabel = new Label();
            commandLabel.Content = "⯁ "; // Blank command symbol
            commandLabel.Padding = new(5);

            commandDockPanel.Children.Add(commandLabel);
            commandDockPanel.ContextMenu = CreateCommandContextMenu(null, commandDockPanel); // You can create a different context menu for blank commands if needed

            return commandBorder;
        }

        

        private Border CreateRealCommandPanel(EventCommand EventCommand)
        {
            Border commandBorder = new();


            //Main
            DockPanel commandDockPanel = new();
            commandBorder.Child = commandDockPanel;
            commandBorder.Margin = new(10,10,10,0);
            DockPanel.SetDock(commandBorder, Dock.Top);
            commandDockPanel.ContextMenu = CreateCommandContextMenu(EventCommand, commandDockPanel); //Context Menu

            //Label
            DockPanel TopPanel = new();
            TopPanel.LastChildFill = false;
            DockPanel.SetDock(TopPanel, Dock.Top);
            commandDockPanel.Children.Add(TopPanel);

            Label commandLabel = new();
            commandLabel.Content = "⯁ " + EventCommand.Command.DisplayName;
            commandLabel.Padding = new(5);
            TopPanel.Children.Add(commandLabel);

            //IF COMMAND PROMPT COMMAND, WE ADD SOME EXTRA CONTROLS.
            if (EventCommand.Command.Key == "638907232781932877-460670541-291625304") 
            {
                CMDStuff(TopPanel, EventCommand, commandDockPanel); //Adds buttons for the command prompt command to the Top Panel.
            }
            

            //Resources
            int i = 1;
            foreach (CommandResource ResourceData in EventCommand.Command.RequiredResourcesList ) // KeyValuePair<string, string> pair in EventCommand.Command.Resources
            {
                                    
                CreateResourcePanel(ResourceData, commandDockPanel, EventCommand, i);
                i++;
                
            }

            int Ci = 1;
            foreach (CommandResource ResourceData in EventCommand.CMDList) // KeyValuePair<string, string> pair in EventCommand.Command.Resources
            {

                CreateResourcePanel(ResourceData, commandDockPanel, EventCommand, Ci);
                Ci++;
            }

            return commandBorder;
        }

        private void CMDStuff(DockPanel TopPanel, EventCommand EventCommand, DockPanel commandDockPanel) 
        {
            Button ToolsPathBtn = new();
            ToolsPathBtn.Content = "ToolsPath";
            ToolsPathBtn.Width = 105;
            ToolsPathBtn.Margin = new(4, 4, 4, 4);
            DockPanel.SetDock(ToolsPathBtn, Dock.Right);
            ToolsPathBtn.HorizontalAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(ToolsPathBtn);
            ToolsPathBtn.Click += (sender, e) =>
            {
                //This also happens in LoadDatabase.cs when loading the command prompt command into an event. Any changes here need to happen over there as well.
                CommandResource ResourceData = new();
                ResourceData.Label = "Workshop Tools Folder Path";
                ResourceData.Type = CommandResource.ResourceTypes.WTools;
                EventCommand.CMDList.Add(ResourceData);

                EventCommand.ResourceKeys.Add(EventCommand.CMDList.Count, "WTOOLS"); //This is important, or the resource order won't be correct in the end. 

                UpdateEventCommandsUI();
            };

            

            Button FolderResourcePathBtn = new();
            FolderResourcePathBtn.Content = "Folder";
            FolderResourcePathBtn.Width = 75;
            FolderResourcePathBtn.Margin = new(4, 4, 4, 4);
            DockPanel.SetDock(FolderResourcePathBtn, Dock.Right);
            FolderResourcePathBtn.HorizontalAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(FolderResourcePathBtn);
            FolderResourcePathBtn.Click += (sender, e) =>
            {
                //This also happens in LoadDatabase.cs when loading the command prompt command into an event. Any changes here need to happen over there as well.
                CommandResource ResourceData = new();
                ResourceData.Label = "Folder Path From";
                ResourceData.Type = CommandResource.ResourceTypes.Folder; 
                EventCommand.CMDList.Add(ResourceData);

                EventCommand.ResourceKeys.Add(EventCommand.CMDList.Count, ""); //This is important, or the resource order won't be correct in the end. 

                UpdateEventCommandsUI();
            };

            Button FileResourcePathBtn = new();
            FileResourcePathBtn.Content = "File";
            FileResourcePathBtn.Width = 75; //155
            FileResourcePathBtn.Margin = new(4, 4, 4, 4);
            DockPanel.SetDock(FileResourcePathBtn, Dock.Right);
            FileResourcePathBtn.HorizontalAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(FileResourcePathBtn);
            FileResourcePathBtn.Click += (sender, e) =>
            {
                //This also happens in LoadDatabase.cs when loading the command prompt command into an event. Any changes here need to happen over there as well.
                CommandResource ResourceData = new();
                ResourceData.Label = "File Path From";
                ResourceData.Type = CommandResource.ResourceTypes.File;
                EventCommand.CMDList.Add(ResourceData);

                EventCommand.ResourceKeys.Add(EventCommand.CMDList.Count, ""); //This is important, or the resource order won't be correct in the end. 

                UpdateEventCommandsUI();
            };

            Button TextPathBtn = new();
            TextPathBtn.Content = "Text";
            TextPathBtn.Width = 80;
            TextPathBtn.Margin = new(4, 4, 4, 4);
            DockPanel.SetDock(TextPathBtn, Dock.Right);
            TextPathBtn.HorizontalAlignment = HorizontalAlignment.Right;
            TextPathBtn.HorizontalContentAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(TextPathBtn);
            TextPathBtn.Click += (sender, e) =>
            {
                CommandResource ResourceData = new();
                ResourceData.Label = "Your Text";
                ResourceData.Type = CommandResource.ResourceTypes.CMDText;
                EventCommand.CMDList.Add(ResourceData);

                EventResource TextResource = new();
                TextResource.Name = "CMD Text Resource";
                TextResource.Key = PixelWPF.LibraryPixel.GenerateKey();
                TextResource.ResourceType = EventResource.ResourceTypes.CMDText;
                workshopData.WorkshopEventResources.Add(TextResource);

                ResourceData.CMDTextKey = TextResource.Key; // Link the command resource to the event resource

                EventCommand.ResourceKeys.Add(EventCommand.CMDList.Count, TextResource.Key);  //This is important, or the resource order won't be correct in the end. 

                UpdateEventCommandsUI();
            };

            Button HelpBtn = new();
            HelpBtn.Content = "Help!";
            HelpBtn.Width = 70;
            HelpBtn.Margin = new(0, 4, 4, 4);
            DockPanel.SetDock(HelpBtn, Dock.Right);
            HelpBtn.HorizontalAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(HelpBtn);
            HelpBtn.Click += (sender, e) =>
            {
                PixelWPF.LibraryPixel.Notification("Command Prompt Help",
                    "The command lets you send text commands to run in command prompt. " +
                    "As there are all kinds of things a user could be wanting to do with CMD, it has some special controls. This command sends one (and only one) final string to be run in command prompt, and buttons that let you add references to locations." +
                    "\n" +
                    "\nThe GES Path button adds the path to where the game editor studio's exe is on a end users computer. " +
                    "\n" +
                    "\nThe Resource Path button adds the path of a resource. If you are using a child resource, the path becomes parent path + child path (IE you don't need to select parent first, it automatically grabs it's parent).)" +
                    "\n" +
                    "\nThe Add Text button lets you add your own text. " +
                    "\n" +
                    "\nNote that spaces are not automatically added.");
            };


            ///////////////////////////////////////
            DockPanel ResourcePanel = new();
            commandDockPanel.Children.Add(ResourcePanel);
            DockPanel.SetDock(ResourcePanel, Dock.Bottom);
            
            Label label = new();
            label.Content = "Mouse hover for final string: ";
            label.Padding = new(25, 0, 25, 0);
            ResourcePanel.Children.Add(label);
            DockPanel.SetDock(label, Dock.Left);

            Button finalBtn = new();
            finalBtn.Content = "Example";
            DockPanel.SetDock(finalBtn, Dock.Left);
            ResourcePanel.Children.Add(finalBtn);
            finalBtn.Click += (sender, e) =>
            {
                PixelWPF.LibraryPixel.Notification("Dummy",
                    "");
            };

            TextBox finalbox = new();
            finalbox.IsEnabled = false;            
            finalbox.ToolTip = finalbox.Text;
            ResourcePanel.Children.Add(finalbox);



            finalbox.Text = "Final String: "; //This is the final string that will be sent to command prompt.

        }

        public void CreateResourcePanel(CommandResource CommandResourceData, DockPanel dockPanel, EventCommand EventCommand, int i) 
        {
            DockPanel ResourcePanel = new();
            dockPanel.Children.Add(ResourcePanel);
            DockPanel.SetDock(ResourcePanel, Dock.Top);

            if (CommandResourceData.IsOptional == true) 
            {
                Label OptionalSymbol = new();
                OptionalSymbol.Content = "⯁ ";
                OptionalSymbol.Padding = new(5, 0, 0, 0);
                ResourcePanel.Children.Add(OptionalSymbol);
                DockPanel.SetDock(OptionalSymbol, Dock.Left);
                OptionalSymbol.Foreground = Brushes.Orange;
            }            

            Label label = new();
            label.Content = CommandResourceData.Label;
            label.Padding = new(25,0,25,0);
            if (CommandResourceData.IsOptional == true) { label.Padding = new(0, 0, 25, 0); }
            ResourcePanel.Children.Add(label);
            DockPanel.SetDock(label, Dock.Left);

            if (CommandResourceData.Type == CommandResource.ResourceTypes.WTools) { return; }

            if (CommandResourceData.Type == CommandResource.ResourceTypes.CMDText) //IF COMMAND PROMPT, MAKE A TEXT INPUT AND STOP HERE (NO DROPDOWN)
            {
                EventResource TheEventResource = null;
                foreach (EventResource er in EventResources)
                {
                    if (er.Key == CommandResourceData.CMDTextKey)
                    {
                        TheEventResource = er;
                        break;
                    }
                }

                TextBox stringbox = new();
                stringbox.Text = TheEventResource.Location;
                DockPanel.SetDock(stringbox, Dock.Right);
                ResourcePanel.Children.Add(stringbox);
                stringbox.TextChanged += (sender, e) =>
                {
                    TheEventResource.Location = stringbox.Text;
                    EventCommand.ResourceKeys[i] = TheEventResource.Key; // Clear the resource key for text resources
                };
                //EventCommand.ResourceKeys[i] = TheEventResource.Key;

                return;
            }
            

            ComboBox ResourceBox = new();
            DockPanel.SetDock(ResourceBox, Dock.Right);
            ResourcePanel.Children.Add(ResourceBox);
            //ResourceBox.Width = 250;

            ComboBoxItem EmptyItem = new(); //for selecting nothing
            EmptyItem.Content = "None";
            //tem.Tag = EventResource;
            ResourceBox.Items.Add(EmptyItem);
            ResourceBox.SelectedItem = EmptyItem; //Default is None

            foreach (EventResource EventResource in EventResources) 
            {
                string MYNAME = EventResource.Name;

                if (CommandResourceData.Type == CommandResource.ResourceTypes.File && (EventResource.ResourceType == EventResource.ResourceTypes.Folder))
                {
                    continue;
                }
                if (CommandResourceData.Type == CommandResource.ResourceTypes.Folder && (EventResource.ResourceType == EventResource.ResourceTypes.File))
                {
                    continue;
                }
                if (EventResource.ResourceType == EventResource.ResourceTypes.CMDText) 
                {
                    continue;
                }

                string TheThingy = ""; //The file/folder we are actually using.

                if (EventResource.RequiredName == true)
                {
                    TheThingy = EventResource.Location;

                    if (EventResource.Location == "") 
                    {
                        TheThingy = "ERROR";
                    }
                }
                else if (EventResource.RequiredName == false) 
                {
                    ProjectEventResource ProjectResource = MainMenu.ProjectDataItem.ProjectEventResources.Find(thing => thing.Key == EventResource.Key);

                    if (ProjectResource != null)
                    {
                        string sdfsdf = System.IO.Path.GetFileName(ProjectResource.Location);
                        TheThingy = sdfsdf;

                        if (sdfsdf == "" || sdfsdf == null)
                        {
                            TheThingy = "ERROR";
                        }

                        
                    }
                    else if (ProjectResource == null)
                    {
                        if (EventResource.IsChild == true)
                        {
                            TheThingy = System.IO.Path.GetFileName(EventResource.Location);
                        }
                    }
                }
                
                TextBlock tex = new();
                ComboBoxItem Item = new();
                Item.Content = tex;
                if (CommandResourceData.Type == CommandResource.ResourceTypes.File) 
                {
                    Run name = new();
                    name.Text = "🗎 " + EventResource.Name;
                    name.Foreground = Brushes.White;
                    tex.Inlines.Add(name);
                                      
                }
                if (CommandResourceData.Type == CommandResource.ResourceTypes.Folder) 
                {
                    Run name = new();
                    name.Text = "📁 " + EventResource.Name;
                    name.Foreground = Brushes.White;
                    tex.Inlines.Add(name);
                    //Item.Content = "📁 " + WorkshopEventResource.Name;                                     
                }
                if (EventResource.RequiredName == true) 
                {
                    Run run = new();
                    run.Text = "     (Required Name)";
                    run.Foreground = Brushes.Orange;
                    tex.Inlines.Add(run);
                }
                if (EventResource.IsChild == true)
                {
                    Run run = new();
                    run.Text = "     (Child)";
                    run.Foreground = Brushes.Orange;
                    tex.Inlines.Add(run);
                }


                Item.Tag = EventResource;
                ResourceBox.Items.Add(Item);                
            }
            // Assume 'i' is your key for which you want to set the selected item
            if (EventCommand.ResourceKeys.TryGetValue(i, out string resourceKey)) //i starts at 1
            {
                foreach (ComboBoxItem item in ResourceBox.Items)
                {
                    //Default is none up above when none item is first made
                    if (item.Tag is EventResource resource && resource.Key == resourceKey)
                    {
                        ResourceBox.SelectedItem = item;
                        break; // Exit the loop once the matching item is found and selected
                    }
                }
            }



            ResourceBox.DropDownClosed += (sender, e) =>
            {
                ComboBox comboBox = sender as ComboBox;
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;
                if (selectedItem.Content == "None") 
                {
                    //make gpt figure this out T.T  
                    //i want to set the resource to "". but whats even happening and coding on reos couch is uncomfy D:
                    //What happens if event resource nolonger exists? (for events window, and for command execute?)
                    EventResource ItemEventResource = selectedItem.Tag as EventResource;
                    EventCommand.ResourceKeys[i] = ""; // Update the existing key
                }
                else
                {
                    EventResource ItemEventResource = selectedItem.Tag as EventResource;
                    EventCommand.ResourceKeys[i] = ItemEventResource.Key; // Update the existing key

                    //// Check if the dictionary already has the key
                    //if (EventCommand.ResourceKeys.ContainsKey(i))
                    //{
                    //    EventCommand.ResourceKeys[i] = ItemEventResource.ResourceKey; // Update the existing key
                    //}
                    //else
                    //{
                    //    EventCommand.ResourceKeys.Add(i, ItemEventResource.ResourceKey); // Add new key-value pair
                    //}

                }
            };
        }





        //=======================================================================
        //==========================Context Menu=================================
        //=======================================================================

        private ContextMenu CreateCommandContextMenu(EventCommand myCommand, DockPanel commandDockPanel)
        {
            ContextMenu contextMenu = new ContextMenu();

            // Add menu items here
            // Example:
            MenuItem NewItem = new MenuItem();
            NewItem.Header = "New Command...";
            NewItem.Click += (sender, e) => CreateNewCommandForThisEvent(myCommand);
            contextMenu.Items.Add(NewItem);

            MenuItem menuItem1 = new MenuItem();
            menuItem1.Header = "Delete Command";
            menuItem1.Click += (sender, e) => DeleteCommand(myCommand, commandDockPanel);
            contextMenu.Items.Add(menuItem1);



            // Add more items as needed

            return contextMenu;
        }

        private void CreateNewCommandForThisEvent(EventCommand myCommand)
        {

            int insertIndex = -1; // Default to an invalid index

            if (myCommand != null)
            {
                // Find the index of the myCommand in the CommandsList
                for (int i = 0; i < CurrentEvent.CommandList.Count; i++)
                {
                    if (CurrentEvent.CommandList[i] == myCommand)
                    {
                        insertIndex = i;
                        break;
                    }
                }
            }
            else
            {
                // If it's a blank command, set to insert at the end
                insertIndex = CurrentEvent.CommandList.Count;
            }

            // Open the EventManagerCommands window with the insertIndex
            CommandsWindow CommandsWindow = new CommandsWindow(insertIndex);
            CommandsWindow.Owner = this; // 'this' refers to the current window
            CommandsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CommandsWindow.Show();

            CommandsWindow.CommandAdded += AddCommandToMyNewCommandList; //fix?

        }

        private void AddCommandToMyNewCommandList(Command Command, int insertIndex)
        {
            if (Command == null) { throw new InvalidOperationException("Command object is null, intentionally causing a crash."); }

            EventCommand myCommand = new(); //This is exactly where the "My command" is created and sent out.
            myCommand.Command = Command;

            // Adjust index to insert after the selected command
            insertIndex = insertIndex >= 0 ? insertIndex + 1 : CurrentEvent.CommandList.Count;

            // Check if the index is within the valid range
            if (insertIndex >= 0 && insertIndex <= CurrentEvent.CommandList.Count)
            {
                CurrentEvent.CommandList.Insert(insertIndex, myCommand);
            }
            else
            {
                // If the index is out of range, add it to the end
                CurrentEvent.CommandList.Add(myCommand);
            }

            {   //This code block HOPEFULLY adresses resource keys being set.
                //Before i had a issue where a newly created command would not set any needed keys, thus the events menu would say the event is valid to run untill the program is restarted.
                //If i ever remove this or touch resource key stuff, i need to remember to update this code.
                int i = 0;
                foreach (CommandResource asdf in myCommand.Command.RequiredResourcesList)
                {
                    i++;
                    myCommand.ResourceKeys.Add(i, "");

                }
            }
            

            UpdateEventCommandsUI();
            
        }



        private void DeleteCommand(EventCommand myCommand, DockPanel commandDockPanel)
        {
            if (myCommand != null)
            {
                if (commandDockPanel.Parent is Border parentborder)
                {
                    if (parentborder.Parent is Panel parentPanel)
                    {
                        parentPanel.Children.Remove(parentborder);
                    }
                }
                CurrentEvent.CommandList.Remove(myCommand);
                //Commands.Remove(command);
                //command = null;

            }
            else
            {
                // Handle the option click for a blank command
            }
        }

        //=======================================================================
        //=============================Misc Code=================================
        //=======================================================================

        private void EventTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (EventListTree.SelectedItem == null)
            {
                CurrentEvent = null;
                UpdateEventCommandsUI();
                return;
            }
            else
            {
                TreeViewItem itemm = EventListTree.SelectedItem as TreeViewItem;
                CurrentEvent = itemm.Tag as Event;
                UpdateEventCommandsUI();

                
            }

        }

        private void UpdateEventCommandsUI()
        {            

            EventCommandsDockPanel.Children.Clear();
            

            if (CurrentEvent == null) 
            {
                EventNameBox.Text = "";
                EventTooltipBox.Text = "";
                return;
            }

            if (CurrentEvent.CommandList.Count != 0)
            {
                foreach (EventCommand myCommand in CurrentEvent.CommandList)
                {                    
                    EventCommandsDockPanel.Children.Add(CreateRealCommandPanel(myCommand));
                }
            }

            EventCommandsDockPanel.Children.Add(CreateFakeCommandPanel()); // Add a blank command at the end
            EventNameBox.Text = CurrentEvent.DisplayName;
            EventTooltipBox.Text = CurrentEvent.Tooltip;
        }

        private void ButtonMoveEventUp(object sender, RoutedEventArgs e)
        {
            if (EventListTree.SelectedItem == null)
            {
                return;
            }

            TreeViewItem Itemm = EventListTree.SelectedItem as TreeViewItem;
            Event Event = Itemm.Tag as Event;

            LibraryGES.MoveListItemUp(WorkshopEvents, Event);
            LibraryGES.MoveTreeItemUp(EventListTree, Itemm);

            Itemm.IsSelected = true;
        }

        private void ButtonMoveEventDown(object sender, RoutedEventArgs e)
        {
            if (EventListTree.SelectedItem == null)
            {
                return;
            }

            TreeViewItem Itemm = EventListTree.SelectedItem as TreeViewItem;
            Event Event = Itemm.Tag as Event;

            LibraryGES.MoveListItemDown(WorkshopEvents, Event);
            LibraryGES.MoveTreeItemDown(EventListTree, Itemm);

            Itemm.IsSelected = true; 
        }

        private void OpenEventingTutorial(object sender, RoutedEventArgs e)
        {
            EventingTutorial eventingTutorial = new EventingTutorial();
            eventingTutorial.Owner = this;
            eventingTutorial.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            eventingTutorial.Show();
        }
    }


}


