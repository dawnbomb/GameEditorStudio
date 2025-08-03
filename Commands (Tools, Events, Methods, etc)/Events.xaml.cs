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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Management.Deployment.Preview;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static GameEditorStudio.EventsMenu;

namespace GameEditorStudio
{
    

    public partial class EventsMenu : Window
    {
        //int Commands = 0;

        public List<Event> Events { get; set; }
        public List<WorkshopResource> EventResources { get; set; }
        public TopMenu MainMenu { get; set; }

        public Event CurrentEvent { get; set; }
        //public Dictionary<string, ICommandAction> commandDictionary = new Dictionary<string, ICommandAction>();


        //List<Event> Events;

        public EventsMenu(string TheWorkshopName,  List<Event> TheEvents, List<WorkshopResource> TheEventResources, TopMenu TheMainMenu)
        {
            InitializeComponent();

            this.Title = TheWorkshopName + "Events Setup";
            Events = TheEvents;
            EventResources = TheEventResources;
            MainMenu = TheMainMenu;

            //Events = new ObservableCollection<Event>(EventsList);
            //EventListTree.ItemsSource = Events;
            foreach (Event Event in Events)
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

        //==========================Top Menu=================================

        private void CreateNewEvent(object sender, RoutedEventArgs e)
        {
            Event TheEvent = new();
            TheEvent.DisplayName = "New Event";
            Events.Add(TheEvent);

            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = TheEvent.DisplayName;
            EventListTree.Items.Add(treeViewItem);
            treeViewItem.Tag = TheEvent;
            //EventListTree

        }

        private void DeleteEventButton(object sender, RoutedEventArgs e)
        {            

            if (EventListTree.SelectedItem == null)
            {
                return;
            }

            TreeViewItem Itemm = EventListTree.SelectedItem as TreeViewItem;
            Event Event = Itemm.Tag as Event;

            Events.Remove(CurrentEvent);
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

        private void EventDescriptionBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentEvent == null) { return; }

            if (e.Key == Key.Enter)
            {
                CurrentEvent.Notepad = EventDescriptionBox.Text;

            }
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
                
                MethodData ActionPack = LibraryMan.TransformKeysToLocations(eventCommand.ResourceKeys, EventResources, MainMenu, eventCommand);
                ActionPack.Command.TheMethod(ActionPack);

            }

        }

        //=======================================================================================
        //=====================================Command Panels====================================
        //=======================================================================================


        private DockPanel CreateFakeCommandPanel() //This is only because i want this window to look like the RPG maker eventing window. :>, not actually meant to hold any commands or do anything.
        {
            DockPanel commandDockPanel = new DockPanel();
            commandDockPanel.Margin = new(10, 10, 10, 0);
            DockPanel.SetDock(commandDockPanel, Dock.Top);

            Label commandLabel = new Label();
            commandLabel.Content = "⯁ "; // Blank command symbol
            commandLabel.Padding = new(5);

            commandDockPanel.Children.Add(commandLabel);
            commandDockPanel.ContextMenu = CreateCommandContextMenu(null, commandDockPanel); // You can create a different context menu for blank commands if needed

            return commandDockPanel;
        }

        

        private DockPanel CreateRealCommandPanel(EventCommand EventCommand)
        {
            //Main
            DockPanel commandDockPanel = new();
            commandDockPanel.Margin = new(10,10,10,0);
            DockPanel.SetDock(commandDockPanel, Dock.Top);
            commandDockPanel.ContextMenu = CreateCommandContextMenu(EventCommand, commandDockPanel); //Context Menu

            //Label
            DockPanel LabelPanel = new();
            DockPanel.SetDock(LabelPanel, Dock.Top);
            commandDockPanel.Children.Add(LabelPanel);
            Label commandLabel = new();
            commandLabel.Content = "⯁ " + EventCommand.Command.DisplayName;
            commandLabel.Padding = new(5);
            LabelPanel.Children.Add(commandLabel);

            //Resources

            int i = 1;

            foreach (CommandResource ResourceData in EventCommand.Command.RequiredResourcesList ) // KeyValuePair<string, string> pair in EventCommand.Command.Resources
            {
                                    
                CreateResourcePanel(ResourceData, commandDockPanel, EventCommand, i);
                i++;
                
            }
            return commandDockPanel;
        }

        public void CreateResourcePanel(CommandResource ResourceData, DockPanel dockPanel, EventCommand EventCommand, int i) 
        {
            DockPanel ResourcePanel = new();
            dockPanel.Children.Add(ResourcePanel);
            DockPanel.SetDock(ResourcePanel, Dock.Top);

            if (ResourceData.IsOptional == true) 
            {
                Label OptionalSymbol = new();
                OptionalSymbol.Content = "⯁ ";
                OptionalSymbol.Padding = new(5, 0, 0, 0);
                ResourcePanel.Children.Add(OptionalSymbol);
                DockPanel.SetDock(OptionalSymbol, Dock.Left);
                OptionalSymbol.Foreground = Brushes.Orange;
            }            

            Label label = new();
            label.Content = ResourceData.Label;
            label.Padding = new(25,0,25,0);
            if (ResourceData.IsOptional == true) { label.Padding = new(0, 0, 25, 0); }
            ResourcePanel.Children.Add(label);
            DockPanel.SetDock(label, Dock.Left);

            ComboBox ResourceBox = new();
            DockPanel.SetDock(ResourceBox, Dock.Right);
            ResourcePanel.Children.Add(ResourceBox);
            //ResourceBox.Width = 250;

            //Label ResourceTypeLabel = new();
            //DockPanel.SetDock(ResourceTypeLabel, Dock.Right);
            //ResourceTypeLabel.HorizontalAlignment = HorizontalAlignment.Right;
            //ResourcePanel.Children.Add(ResourceTypeLabel);
            //if (ResourceData.Type == CommandResource.ResourceTypes.File) { ResourceTypeLabel.Content = "🗎 "; }
            //if (ResourceData.Type == CommandResource.ResourceTypes.Folder) { ResourceTypeLabel.Content = "📁"; }

            ComboBoxItem EmptyItem = new(); //for selecting nothing
            EmptyItem.Content = "None";
            //tem.Tag = EventResource;
            ResourceBox.Items.Add(EmptyItem);
            ResourceBox.SelectedItem = EmptyItem; //Default is None

            foreach (WorkshopResource WorkshopEventResource in EventResources) 
            {
                string MYNAME = WorkshopEventResource.Name;

                if (ResourceData.Type == CommandResource.ResourceTypes.File && (WorkshopEventResource.ResourceType == "LocalFolder" || WorkshopEventResource.ResourceType == "RelativeFolder")) 
                {
                    continue;
                }
                if (ResourceData.Type == CommandResource.ResourceTypes.Folder && (WorkshopEventResource.ResourceType == "LocalFile" || WorkshopEventResource.ResourceType == "RelativeFile"))
                {
                    continue;
                }

                string TheThingy = ""; //The file/folder we are actually using.

                if (WorkshopEventResource.RequiredName == true)
                {
                    TheThingy = WorkshopEventResource.Location;

                    if (WorkshopEventResource.Location == "") 
                    {
                        TheThingy = "ERROR";
                    }
                }
                else if (WorkshopEventResource.RequiredName == false) 
                {
                    ProjectEventResource ProjectResource = MainMenu.ProjectDataItem.ProjectEventResources.Find(thing => thing.ResourceKey == WorkshopEventResource.WorkshopResourceKey);

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
                        if (WorkshopEventResource.ResourceType == "RelativeFile" || WorkshopEventResource.ResourceType == "RelativeFolder")
                        {
                            TheThingy = System.IO.Path.GetFileName(WorkshopEventResource.Location);
                        }
                    }
                }
                

                ComboBoxItem Item = new();
                if (ResourceData.Type == CommandResource.ResourceTypes.File) 
                { 
                    Item.Content = "🗎 " + WorkshopEventResource.Name + "     ( " + TheThingy + " )"; 
                }
                if (ResourceData.Type == CommandResource.ResourceTypes.Folder) 
                { 
                    Item.Content = "📁 " + WorkshopEventResource.Name + "     ( " + TheThingy + " )";
                }
                //Item.Content = EventResource.Name;
                Item.Tag = WorkshopEventResource;
                ResourceBox.Items.Add(Item);                
            }
            // Assume 'i' is your key for which you want to set the selected item
            if (EventCommand.ResourceKeys.TryGetValue(i, out string resourceKey)) //i starts at 1
            {
                foreach (ComboBoxItem item in ResourceBox.Items)
                {
                    //Default is none up above when none item is first made
                    if (item.Tag is WorkshopResource resource && resource.WorkshopResourceKey == resourceKey)
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
                    WorkshopResource ItemEventResource = selectedItem.Tag as WorkshopResource;
                    EventCommand.ResourceKeys[i] = ""; // Update the existing key
                }
                else
                {
                    WorkshopResource ItemEventResource = selectedItem.Tag as WorkshopResource;
                    EventCommand.ResourceKeys[i] = ItemEventResource.WorkshopResourceKey; // Update the existing key

                    //// Check if the dictionary already has the key
                    //if (EventCommand.ResourceKeys.ContainsKey(i))
                    //{
                    //    EventCommand.ResourceKeys[i] = ItemEventResource.ResourceKey; // Update the existing key
                    //}
                    //else
                    //{
                    //    EventCommand.ResourceKeys.Add(i, ItemEventResource.ResourceKey); // Add new key-value pair
                    //}

                    // Optional: Display the selected item or log to the console
                    //Console.WriteLine($"Selected resource: {selectedResource.DisplayName} with key: {selectedResource.ResourceKey}");
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

            CommandsWindow.CommandAdded += AddCommandToNewMyCommandList; //fix?

        }

        private void AddCommandToNewMyCommandList(Command Command, int insertIndex)
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
                if (commandDockPanel.Parent is Panel parentPanel)
                {
                    parentPanel.Children.Remove(commandDockPanel);
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
                EventDescriptionBox.Text = "";
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
            EventDescriptionBox.Text = CurrentEvent.Notepad;
        }

        private void ButtonMoveEventUp(object sender, RoutedEventArgs e)
        {
            if (EventListTree.SelectedItem == null)
            {
                return;
            }

            TreeViewItem Itemm = EventListTree.SelectedItem as TreeViewItem;
            Event Event = Itemm.Tag as Event;

            LibraryMan.MoveListItemUp(Events, Event);
            LibraryMan.MoveTreeItemUp(EventListTree, Itemm);

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

            LibraryMan.MoveListItemDown(Events, Event);
            LibraryMan.MoveTreeItemDown(EventListTree, Itemm);

            Itemm.IsSelected = true; 
        }
    }


}


