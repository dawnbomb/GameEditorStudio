using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.IO;
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
using PixelWPF;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static GameEditorStudio.EventsMenu;

namespace GameEditorStudio
{
    

    public partial class EventsMenu : UserControl
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
            //this.Title = " Events - " + workshopData.WorkshopName ;
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
            HashSet<string> existingNames = new(WorkshopEvents.Select(ev => ev.DisplayName));

            // Start with "New Event"
            string baseName = "New Event";
            string newName = baseName;
            int counter = 1;

            // Increment until we find a unique name
            while (existingNames.Contains(newName))
            {
                counter++;
                newName = $"{baseName} {counter}";
            }

            // Create the new event
            Event theEvent = new()
            {
                DisplayName = newName,
                CreatedVersion = LibraryGES.VersionNumber,
                CreatedDate = DateTime.Now.ToString("MMM dd yyyy"),
            };
            WorkshopEvents.Add(theEvent);

            // Create TreeViewItem
            TreeViewItem treeViewItem = new()
            {
                Header = theEvent.DisplayName,
                Tag = theEvent,
                IsSelected = true // Select the new event
            };

            EventListTree.Items.Add(treeViewItem);

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

        private void EventNameboxTextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = PixelWPF.LibraryPixel.CheckIfValidFolderName(EventNameBox.Text);
            if (isValid)
            {
                EventNameBox.Foreground = new SolidColorBrush(Colors.White);
                return;
            }
            else
            {
                EventNameBox.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
        }

        private void RenameEvent(object sender, KeyEventArgs e) //The event name textbox
        {
            if (CurrentEvent == null) { return; }

            bool isValid = PixelWPF.LibraryPixel.CheckIfValidFolderName(EventNameBox.Text);
            if (!isValid)
            {
                EventNameBox.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            if (e.Key == Key.Enter)
            {
                foreach (Event Event in WorkshopEvents) 
                {
                    if (Event.DisplayName == EventNameBox.Text) 
                    {
                        PixelWPF.LibraryPixel.NotificationNegative("You can't use that name! D:", "Another event already has that name! \nPick something else!  >:(");
                        return;
                    } 
                }

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

            if (!LibraryPixel.NotificationConfirm("Force Run Event?","This forces the event to run, ignoring all conditions (Tool requirements, resource path requirements, etc)" +
                "\n\nRun this event?" +
                "\n\n(To run it normally IE with requirement checks, run it from the Events menu, instead of this events manager)")) { return; }

            foreach (EventCommand eventCommand in CurrentEvent.CommandList)
            {                

                if (eventCommand.Command.TheMethod == null) { continue; }
                
                MethodData ActionPack = LibraryGES.TransformKeysToLocations(eventCommand.ResourceKeys, EventResources, MainMenu, eventCommand);

                Tool? existingTool = workshopData.WorkshopTools.FirstOrDefault(t => t.Key == eventCommand.WorkshopToolKey);
                if (existingTool != null) { ActionPack.Command.RequiredToolsList.Add(existingTool); }
                
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
            if (EventCommand.Command.Key == "CMD1" || EventCommand.Command.Key == "CMD2" || EventCommand.Command.Key == "CMD3") 
            {
                CMDStuff(TopPanel, EventCommand, commandDockPanel); //Adds buttons for the command prompt command to the Top Panel.
            }
            if (EventCommand.Command.Key == "RunWorkshopTool" || EventCommand.Command.Key == "RunWorkshopToolWithFile" || EventCommand.Command.Key == "RunWorkshopToolWithFolder")
            {
                SpecialWorkshopTool(TopPanel, EventCommand, commandDockPanel); //Adds UI for selecting a Non-GES workshop tool.
            }


            //Resources
            int i = 1;
            foreach (CommandResource ResourceData in EventCommand.Command.RequiredResourcesList ) // KeyValuePair<string, string> pair in EventCommand.Command.Resources
            {
                                    
                CreateResourceRowOnPanel(ResourceData, commandDockPanel, EventCommand, i);
                i++;
                
            }

            int Ci = 1; //IF COMMAND PROMPT COMMAND, WE ADD SOME SPECIAL ROWS.
            foreach (CommandResource ResourceData in EventCommand.CMDList) // KeyValuePair<string, string> pair in EventCommand.Command.Resources
            {

                CreateResourceRowOnPanel(ResourceData, commandDockPanel, EventCommand, Ci);
                Ci++;
            }

            return commandBorder;
        }

        private void SpecialWorkshopTool(DockPanel TopPanel, EventCommand EventCommand, DockPanel commandDockPanel) 
        {
            DockPanel ResourcePanel = new();
            commandDockPanel.Children.Add(ResourcePanel);
            DockPanel.SetDock(ResourcePanel, Dock.Bottom);


            Label label = new Label();
            label.Content = "Non GES Tool: ";
            DockPanel.SetDock(label, Dock.Left);
            ResourcePanel.Children.Add(label);

            ComboBox toolbox = new();
            ResourcePanel.Children.Add(toolbox);
            DockPanel.SetDock(toolbox, Dock.Right);

            foreach (Tool tool in workshopData.WorkshopTools) 
            {
                ComboBoxItem toolItem = new ComboBoxItem();
                toolItem.Content = tool.DisplayName;
                toolItem.Tag = tool;
                toolbox.Items.Add(toolItem);

                if (EventCommand.WorkshopToolKey == tool.Key) { toolItem.IsSelected = true; }
            }

            toolbox.DropDownClosed += (sender, e) =>
            {
                ComboBoxItem toolItem = toolbox.SelectedItem as ComboBoxItem;
                if (toolItem == null) { EventCommand.WorkshopToolKey = ""; return; }

                Tool tool = toolItem.Tag as Tool;
                EventCommand.WorkshopToolKey = tool.Key;                
            };

        }

        private void CMDStuff(DockPanel TopPanel, EventCommand EventCommand, DockPanel commandDockPanel) 
        {            
            Button WorkshopToolBtn = new();
            WorkshopToolBtn.Content = "Workshop Tool";
            WorkshopToolBtn.Width = 155;
            WorkshopToolBtn.Margin = new(4, 4, 4, 4);
            DockPanel.SetDock(WorkshopToolBtn, Dock.Right);
            WorkshopToolBtn.HorizontalAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(WorkshopToolBtn);
            WorkshopToolBtn.Click += (sender, e) =>
            {
                //This also happens in LoadDatabase.cs when loading the command prompt command into an event. Any changes here need to happen over there as well.
                CommandResource ResourceData = new();
                ResourceData.Label = "Workshop Tool";
                ResourceData.Type = CommandResource.ResourceTypes.WTool;
                EventCommand.CMDList.Add(ResourceData);

                EventResource WToolResource = new();
                WToolResource.Name = "WTool Resource";
                WToolResource.Key = PixelWPF.LibraryPixel.GenerateKey();
                WToolResource.ResourceType = EventResource.ResourceTypes.CMDWTool;
                workshopData.WorkshopEventResources.Add(WToolResource);

                ResourceData.CMDWToolKey = WToolResource.Key; // Link the command resource to the event resource

                EventCommand.ResourceKeys.Add(EventCommand.CMDList.Count, WToolResource.Key);  //This is important, or the resource order won't be correct in the end. 

                UpdateEventCommandsUI();
            };

            Button ToolButton = new();
            ToolButton.Content = "GES Tool";
            ToolButton.Width = 105;
            ToolButton.Margin = new(4, 4, 4, 4);
            DockPanel.SetDock(ToolButton, Dock.Right);
            ToolButton.HorizontalAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(ToolButton);
            ToolButton.Click += (sender, e) =>
            {
                //This also happens in LoadDatabase.cs when loading the command prompt command into an event. Any changes here need to happen over there as well.
                CommandResource ResourceData = new();
                ResourceData.Label = "GES Tool";
                ResourceData.Type = CommandResource.ResourceTypes.GTool;
                EventCommand.CMDList.Add(ResourceData);

                EventResource GToolResource = new();
                GToolResource.Name = "GTool Resource";
                GToolResource.Key = PixelWPF.LibraryPixel.GenerateKey();
                GToolResource.ResourceType = EventResource.ResourceTypes.CMDGTool;
                workshopData.WorkshopEventResources.Add(GToolResource);

                ResourceData.CMDGToolKey = GToolResource.Key; // Link the command resource to the event resource

                EventCommand.ResourceKeys.Add(EventCommand.CMDList.Count, GToolResource.Key);  //This is important, or the resource order won't be correct in the end. 

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
            HelpBtn.Content = "Tutorial!";
            HelpBtn.Width = 110;
            HelpBtn.Margin = new(20, 4, 4, 4);
            DockPanel.SetDock(HelpBtn, Dock.Left);
            HelpBtn.HorizontalAlignment = HorizontalAlignment.Left;
            TopPanel.Children.Add(HelpBtn);
            HelpBtn.Click += (sender, e) =>
            {
                PixelWPF.LibraryPixel.Notification("Build-A-Command Tutorial",
                    "This command lets build a code piece that gets sent to run in command prompt. Great for slightly more complex things, run any program with any number of file / folder requirements!" +
                    "\n" +
                    "\n----------------------------" +
                    "\nThe File / Folder buttons: " +
                    "\nAdd the location of a file / folder resource to the final text. " +
                    "\n" +   
                    "\nTools / Workshop Tools:" +
                    "\nAdd the location of a tool.exe." +
                    "\n" +
                    "\nThe Text Button:" +
                    "\nThis lets you add your own custom text. It is annoyingly common for most game modding tools to ask users to use specific text commands with command prompt. " +
                    "\n" +
                    "\nNOTE 1: " +
                    "\nThe Final Text lets you preview the exact text that gets sent to command prompt when the event is actually run." +
                    "\n" +
                    "\nNOTE 2: " +
                    "\nLocations are always surrounded with \"quotation marks\" because command prompt requires it (this not a bug).  " +
                    "\nIf you select a child resource, you get the \"Parent Location + Child Location.\"" +
                    "\n" +
                    "\n" +
                    "\nREALLY IMPORTANT TIP:" +
                    "\nDo NOT mistakenly think Auto-Close mode (close command prompt when task is complete) is \"just better\". " +
                    "\nFor many tasks, the user would *VERY MUCH* like to see command prompt SAY that it's finished." +
                    "\nDO NOT IGNORE THIS ADVICE! USE WITH CAUTION!" +
                    "\n" +
                    "\nFINAL NOTE: " +
                    "\nThe program waits for CMD to close before it continues running the event (or running at all). " +
                    "");
            };



            ComboBox CMDModeBox = new();
            CMDModeBox.Width = 190;
            CMDModeBox.Margin = new(4, 4, 4, 4);
            DockPanel.SetDock(CMDModeBox, Dock.Right);
            CMDModeBox.HorizontalAlignment = HorizontalAlignment.Right;
            TopPanel.Children.Add(CMDModeBox);

            ComboBoxItem DebugModeItem = new();
            DebugModeItem.Content = "Stay Open Mode";
            CMDModeBox.Items.Add(DebugModeItem);

            ComboBoxItem AutoCloseItem = new();
            AutoCloseItem.Content = "Auto Close Mode";
            CMDModeBox.Items.Add(AutoCloseItem);

            ComboBoxItem HiddenItem = new();
            HiddenItem.Content = "Hidden Mode";
            //CMDModeBox.Items.Add(HiddenItem); //Temporary disable Hidden mode untill i finish coding it.

            if (EventCommand.Command.Key == "CMD1") { CMDModeBox.SelectedItem = DebugModeItem; }
            if (EventCommand.Command.Key == "CMD2") { CMDModeBox.SelectedItem = AutoCloseItem; }
            if (EventCommand.Command.Key == "CMD3") { CMDModeBox.SelectedItem = HiddenItem; }

            CMDModeBox.SelectionChanged += (sender, e) =>
            {
                if (CMDModeBox.SelectedItem == DebugModeItem) 
                {
                    Command command = Database.Commands.Find(Command => Command.Key == "CMD1");
                    EventCommand.Command = command;
                }
                else if (CMDModeBox.SelectedItem == AutoCloseItem) 
                {
                    Command command = Database.Commands.Find(Command => Command.Key == "CMD2");
                    EventCommand.Command = command;
                }
                else if (CMDModeBox.SelectedItem == HiddenItem) 
                {
                    Command command = Database.Commands.Find(Command => Command.Key == "CMD3");
                    EventCommand.Command = command;
                }
            };



            ///////////////////////////////////////
            DockPanel ResourcePanel = new();
            commandDockPanel.Children.Add(ResourcePanel);
            DockPanel.SetDock(ResourcePanel, Dock.Bottom);

            //Label label = new();
            //label.Content = "Final Text to CMD: ";
            //label.Padding = new(25, 0, 25, 0);
            //ResourcePanel.Children.Add(label);
            //DockPanel.SetDock(label, Dock.Left);
            //
            RichTextBox finalbox = new();
            EventCommand.FinalTextbox = finalbox;
            //TextBox finalbox = EventCommand.FinalTextbox;
            //finalbox.IsEnabled = false;
            //finalbox.ToolTip = finalbox.Text;
            //finalbox.TextWrapping = TextWrapping.Wrap;

            Label finaltextLbl = new();
            finaltextLbl.Content = " Final Text: ";
            finaltextLbl.Margin = new(2);
            DockPanel.SetDock(finaltextLbl, Dock.Left);
            ResourcePanel.Children.Add(finaltextLbl);

            ResourcePanel.Children.Add(finalbox); //Needs to be after finalBtn to show up right.

            UpdateFinalCMDText(EventCommand);


        }

        public void UpdateFinalCMDText(EventCommand EventCommand)
        {
            MethodData methodData = LibraryGES.TransformKeysToLocations(EventCommand.ResourceKeys, EventResources, MainMenu, EventCommand);

            RichTextBox FinalTextbox = EventCommand.FinalTextbox;
            if (FinalTextbox == null) { return; }


            //FinalTextbox.Text = "";

            // Ensure the RichTextBox has a document with a paragraph to write into
            if (FinalTextbox.Document.Blocks.FirstBlock is not Paragraph paragraph)
            {
                paragraph = new Paragraph();
                FinalTextbox.Document.Blocks.Clear();
                FinalTextbox.Document.Blocks.Add(paragraph);
            }
            else
            {
                paragraph.Inlines.Clear(); // Clear previous runs if needed
            }

            foreach (string LocationText in methodData.ResourceLocations)
            {
                if (!string.IsNullOrEmpty(LocationText))
                {
                    string astring = LocationText;
                    astring = LibraryGES.PathQuoter(astring);

                    Run textRun = new Run(astring + " ");

                    // Check if the text is one of the warning messages
                    if (astring.Contains("NOT SET:"))
                    {
                        textRun.Foreground = Brushes.Red;
                    }

                    paragraph.Inlines.Add(textRun);
                }
            }

            //if (astring == "WTOOL")
            //{
            //    FinalTextbox.Text = FinalTextbox.Text + "\"" + "Workshop Tool!" + "\"";
            //    //FinalTextbox.Text = FinalTextbox.Text + "\"" + LibraryGES.ApplicationLocation + "\\Workshops\\" + workshopData.WorkshopName + "\\Tools\\";
            //}
            //else
            //{
            //    FinalTextbox.Text = FinalTextbox.Text + astring + " ";
            //}
        }

        public void CreateResourceRowOnPanel(CommandResource CommandResourceData, DockPanel dockPanel, EventCommand EventCommand, int i) 
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

            

            Button Dbutton = new(); //for CMD command only
            if (EventCommand.CMDList.Count != 0) //IF COMMAND PROMPT, MAKE A CHECKBOX FOR SURROUNDING PATH WITH QUOTES
            {
                Dbutton.Content = " Delete ";
                Dbutton.Margin = new(4, 2, 4, 2);
                DockPanel.SetDock(Dbutton, Dock.Right);
                ResourcePanel.Children.Add(Dbutton);
                Dbutton.Click += (sender, e) =>
                {
                    EventCommand.CMDList.Remove(CommandResourceData);
                    EventCommand.ResourceKeys.Remove(i); //This is important, or the resource order won't be correct in the end. 

                    var reordered = EventCommand.ResourceKeys
                        .OrderBy(kvp => kvp.Key)
                        .Select((kvp, index) => new KeyValuePair<int, string>(index + 1, kvp.Value))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    EventCommand.ResourceKeys = reordered;

                    UpdateEventCommandsUI();
                };

            }
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
                    UpdateFinalCMDText(EventCommand);
                };
                //EventCommand.ResourceKeys[i] = TheEventResource.Key;

                return;
            }
            
            //Everything below here assume it's a dropdown / combobox thing.


            ComboBox ResourceBox = new();
            DockPanel.SetDock(ResourceBox, Dock.Right);
            ResourcePanel.Children.Add(ResourceBox);
            //ResourceBox.Width = 250;

            ComboBoxItem EmptyItem = new(); //for selecting nothing
            EmptyItem.Content = "None";
            //tem.Tag = EventResource;
            ResourceBox.Items.Add(EmptyItem);
            ResourceBox.SelectedItem = EmptyItem; //Default is None

            if (CommandResourceData.Type == CommandResource.ResourceTypes.WTool) 
            {
                EventResource TheEventResource = null;
                foreach (EventResource er in EventResources)
                {
                    if (er.Key == CommandResourceData.CMDWToolKey)
                    {
                        TheEventResource = er;
                        break;
                    }
                }

                                
                foreach (Tool WTool in workshopData.WorkshopTools)
                {
                    ComboBoxItem Item = new();
                    Item.Tag = WTool;

                    Grid itemGrid = new Grid
                    {
                        Background = Brushes.Transparent
                    };

                    // Define column widths with MinWidth so text expands gracefully if needed
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 320 });  // DisplayName column
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 320 }); // ExeName column
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                  // Warning column

                    // 1. Column 0: Display Name
                    TextBlock nameText = new TextBlock
                    {
                        Text = WTool.DisplayName
                    };
                    Grid.SetColumn(nameText, 0);
                    itemGrid.Children.Add(nameText);

                    // 2. Column 1: Exe Name
                    TextBlock exeText = new TextBlock
                    {
                        Text = $"({WTool.ExeName})",
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(10, 0, 0, 0) // Margin to keep distance from Column 0
                    };
                    Grid.SetColumn(exeText, 1);
                    itemGrid.Children.Add(exeText);

                    // 3. Column 2: Location Warning
                    if (!File.Exists(WTool.Location))
                    {
                        TextBlock warningText = new TextBlock
                        {
                            Text = "(Location not set!)",
                            Foreground = Brushes.Red,
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        Grid.SetColumn(warningText, 2);
                        itemGrid.Children.Add(warningText);
                    }

                    Item.Content = itemGrid;
                    ResourceBox.Items.Add(Item);

                    if (WTool.Key == TheEventResource.Location)
                    {
                        Item.IsSelected = true;
                    }
                }


                ResourceBox.DropDownClosed += (sender, e) =>
                {
                    ComboBox comboBox = sender as ComboBox;
                    ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;
                    if (selectedItem.Content == "None")
                    {                        
                        EventCommand.ResourceKeys[i] = ""; // Update the existing key                        
                    }
                    else
                    {
                        //EventResource ItemEventResource = selectedItem.Tag as EventResource;
                        //EventCommand.ResourceKeys[i] = ItemEventResource.Key; // Update the existing key

                        Tool WTool = selectedItem.Tag as Tool;
                        TheEventResource.Location = WTool.Key;
                        EventCommand.ResourceKeys[i] = TheEventResource.Key;
                    }
                    UpdateFinalCMDText(EventCommand);
                };
                return;             
            }
            if (CommandResourceData.Type == CommandResource.ResourceTypes.GTool)
            {
                EventResource TheEventResource = null;
                foreach (EventResource er in EventResources)
                {
                    if (er.Key == CommandResourceData.CMDGToolKey)
                    {
                        TheEventResource = er;
                        break;
                    }
                }

                                
                foreach (Tool GTool in Database.Tools)
                {
                    ComboBoxItem Item = new();
                    Item.Tag = GTool;

                    Grid itemGrid = new Grid
                    {
                        Background = Brushes.Transparent
                    };

                    // Use MinWidth instead of fixed Width so columns expand if text is too long
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 320 });  // DisplayName column
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 320 }); // ExeName column
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                  // Warning column

                    // 1. Column 0: Display Name
                    TextBlock nameText = new TextBlock
                    {
                        Text = GTool.DisplayName
                    };
                    Grid.SetColumn(nameText, 0);
                    itemGrid.Children.Add(nameText);

                    // 2. Column 1: Exe Name
                    TextBlock exeText = new TextBlock
                    {
                        Text = $"({GTool.ExeName})",
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(10, 0, 0, 0) // Small margin so text doesn't touch Column 0
                    };
                    Grid.SetColumn(exeText, 1);
                    itemGrid.Children.Add(exeText);

                    // 3. Column 2: Location Warning
                    if (!File.Exists(GTool.Location))
                    {
                        TextBlock warningText = new TextBlock
                        {
                            Text = "(Location not set!)",
                            Foreground = Brushes.Red,
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        Grid.SetColumn(warningText, 2);
                        itemGrid.Children.Add(warningText);
                    }

                    Item.Content = itemGrid;
                    ResourceBox.Items.Add(Item);

                    if (GTool.Key == TheEventResource.Location)
                    {
                        Item.IsSelected = true;
                    }
                }


                ResourceBox.DropDownClosed += (sender, e) =>
                {
                    ComboBox comboBox = sender as ComboBox;
                    ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;
                    if (selectedItem.Content == "None")
                    {                        
                        EventCommand.ResourceKeys[i] = ""; // Update the existing key                        
                    }
                    else
                    {
                        //EventResource ItemEventResource = selectedItem.Tag as EventResource;
                        //EventCommand.ResourceKeys[i] = ItemEventResource.Key; // Update the existing key

                        Tool GTool = selectedItem.Tag as Tool;
                        TheEventResource.Location = GTool.Key;
                        EventCommand.ResourceKeys[i] = TheEventResource.Key;
                    }
                    UpdateFinalCMDText(EventCommand);
                };
                return;
            }



            foreach (EventResource EventResource in EventResources) 
            {
                string MYNAME = EventResource.Name;

                if (CommandResourceData.Type == CommandResource.ResourceTypes.File && (EventResource.ResourceType != EventResource.ResourceTypes.File))
                {
                    continue;
                }
                if (CommandResourceData.Type == CommandResource.ResourceTypes.Folder && (EventResource.ResourceType != EventResource.ResourceTypes.Folder))
                {
                    continue;
                }
                //if (EventResource.ResourceType == EventResource.ResourceTypes.CMDText) 
                //{
                //    continue;
                //}
                               
                
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
                    //EventResource ItemEventResource = selectedItem.Tag as EventResource;
                    EventCommand.ResourceKeys[i] = ""; // Update the existing key
                }
                else
                {
                    EventResource ItemEventResource = selectedItem.Tag as EventResource;
                    EventCommand.ResourceKeys[i] = ItemEventResource.Key; // Update the existing key


                }
                UpdateFinalCMDText(EventCommand);
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
            Window parentWindow = Window.GetWindow(this);
            if (this.Parent is Panel parentPanel)
            {
                CommandsWindow.Owner = parentWindow;
                CommandsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                CommandsWindow.Show();
            }            

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
            Window parentWindow = Window.GetWindow(this);

            if (this.Parent is Panel parentPanel)
            {
                eventingTutorial.Owner = parentWindow;
                eventingTutorial.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                eventingTutorial.Show();
            }
            
        }

        private void ButtonExitEventing(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(this);
            }

            workshopData.WorkshopXaml.HomeControl.RefreshProjectEventResourcesUI(workshopData.SelectedProject);
        }

        
    }


}


