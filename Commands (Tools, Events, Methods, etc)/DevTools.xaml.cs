using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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
using System.Xml;
using static System.Net.WebRequestMethods;
using static Microsoft.IO.RecyclableMemoryStreamManager;


namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DevTools : Window
    {        

        public enum ToolTabs //No spaces or numbers.  BE CAREFUL CHANGING THESE! The enums are also the raw file names.
        {
            Hidden,   //The default value.
            Tools,
            Patchers,
            HexEditors,
        }

        public enum CommandTabs //No spaces or numbers.  BE CAREFUL CHANGING THESE! The enums are also the raw file names.
        {
            Hidden,   //The default value.
            Basic,
            Program,
            NDS,
            Misc,
            Expiramental,
        }
        //NOTE: These ENUMs were an idea when i was removing google sheet imports, on how to deal with dropdowns.
        //If i end up not using these, it's totally okay to delete them all and cleanup this part of the code.
        public enum CommandSubTabs //No spaces or numbers.  BE CAREFUL CHANGING THESE! The enums are also the raw file names.
        {
            Hidden,   //The default value.
            Basic,
            OpenTool,
            Emulator,
            Patcher,
        }

        public enum CommonEventTabs //No spaces or numbers.  BE CAREFUL CHANGING THESE! The enums are also the raw file names.
        {
            Hidden,  //The default value.
            Basic,
            Command,
            NDS,
        }

        

        public DevTools()
        {
            InitializeComponent();

            foreach (var tool in TrueDatabase.Tools)
            {
                var item = new TreeViewItem
                {
                    Header = tool.DisplayName,
                    Tag = tool
                };
                ToolsTree.Items.Add(item);
            }
            

            foreach (var command in TrueDatabase.Commands)
            {
                var item = new TreeViewItem
                {
                    Header = command.DisplayName,
                    Tag = command
                };
                CommandsTree.Items.Add(item);
            }
            

            foreach (var commonevent in TrueDatabase.CommonEvents)
            {
                var item = new TreeViewItem
                {
                    Header = commonevent.DisplayName,
                    Tag = commonevent
                };
                CommonEventsTree.Items.Add(item);
            }

                        
        }

        private void LoadEvent() 
        {
            (ToolsTree.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsSelected = true;
            (CommandsTree.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsSelected = true;
            (CommonEventsTree.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsSelected = true;
        }

        private void SetupTools()
        {
            
        }

        private void SetupCommands()
        {

        }

        private void SetupCommonEvents()
        {

        }

        private void NewTools(object sender, RoutedEventArgs e)
        {

        }

        private void ToolsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            ToolNameTextbox.Text = tool.DisplayName;            
            ToolDescriptionTextbox.Text = tool.Description;

            ToolKeyTextbox.Text = tool.Key;

            ToolExeTextbox.Text = tool.ExeName;
            ToolDownloadLink.Text = tool.DownloadLink;
            ToolNotepadTextbox.Text = tool.Notepad;

            SelectComboBoxItemByContent(ToolTabCombobox, tool.Category);

            


        }
        

        private void CommandsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;

            CommandNameTextbox.Text = command.DisplayName;
            CommandDescriptionTextbox.Text = command.Description;
            CommandNewKeyTextbox.Text = command.Key;

            CommandMethodNameTextbox.Text = command.MethodName;

            CommandNotepadTextbox.Text = command.Notepad;

            SelectComboBoxItemByContent(CommandTabCombobox, command.Category);
            SelectComboBoxItemByContent(CommandSubCombobox, command.Group);

            CommandToolsPanel.Children.Clear();
            foreach (Tool RequiredTool in command.RequiredToolsList) 
            {
                NewCommandToolPanel(RequiredTool);
            }


            CommandEventResourcesPanel.Children.Clear();
            foreach (CommandResource RequiredResource in command.RequiredResourcesList)
            {
                NewCommandResourcePanel(RequiredResource);
            }
        }

        private void CommonEventsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var commonevent = (CommonEvent)item.Tag;
            if (commonevent == null) return;

            CommonEventNameTextbox.Text = commonevent.DisplayName;
            CommonEventDescriptionTextbox.Text = commonevent.Description;

            CommonEventNotepadTextbox.Text = commonevent.Notepad;

            CommonEventNewKeyTextbox.Text = commonevent.Key;

            SelectComboBoxItemByContent(CommonEventTabCombobox, commonevent.Category);

            CommonEventCommandsPanel.Children.Clear();
            foreach (Command command in commonevent.MyCommands) 
            {
                NewCommonEventCommandPanel(command);
            }

        }

        void SelectComboBoxItemByContent(ComboBox comboBox, string targetContent)
        {
            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content?.ToString() == targetContent)
                {
                    comboBox.SelectedItem = comboItem;
                    break;
                }
            }
        }

        private void CommandButtonNewTool(object sender, RoutedEventArgs e)
        {
            
            NewCommandToolPanel(TrueDatabase.Tools[0]);
        }

        private void CommandButtonNewResource(object sender, RoutedEventArgs e)
        {
            CommandResource RequiredResource = new();
            NewCommandResourcePanel(RequiredResource);
        }

        private void CommonEventButtonNewCommand(object sender, RoutedEventArgs e)
        {
            NewCommonEventCommandPanel(TrueDatabase.Commands[0]);
        }

        private void NewCommandToolPanel(Tool RequiredTool) 
        {
            DockPanel TheDockPanel = new DockPanel();
            DockPanel.SetDock(TheDockPanel, Dock.Top);
            TheDockPanel.Margin = new Thickness(1, 1, 1, 1);


            Button DeleteButton = new Button();
            DeleteButton.Content = " Delete ";
            DockPanel.SetDock(DeleteButton, Dock.Right);
            TheDockPanel.Children.Add(DeleteButton);
            DeleteButton.Click += (s, e) =>
            {
                CommandToolsPanel.Children.Remove(TheDockPanel);
            };

            ComboBox TheComboBox = new ComboBox();
            TheComboBox.Margin = new Thickness(3);
            TheDockPanel.Children.Add(TheComboBox);

            foreach (var tool in TrueDatabase.Tools)
            {
                var toolitem = new ComboBoxItem
                {
                    Content = tool.DisplayName,
                    Tag = tool
                };
                TheComboBox.Items.Add(toolitem);

                if (RequiredTool == tool)
                {
                    toolitem.IsSelected = true;
                }
            }


            CommandToolsPanel.Children.Add(TheDockPanel);
        }

        

        private void NewCommandResourcePanel(CommandResource RequiredResource) 
        {
            DockPanel TheDockPanel = new DockPanel();
            DockPanel.SetDock(TheDockPanel, Dock.Top);
            TheDockPanel.Margin = new Thickness(1, 1, 1, 1);

            Button DeleteButton = new Button();
            DeleteButton.Content = " Delete ";
            DockPanel.SetDock(DeleteButton, Dock.Right);
            TheDockPanel.Children.Add(DeleteButton);
            DeleteButton.Click += (s, e) =>
            {
                CommandEventResourcesPanel.Children.Remove(TheDockPanel);
            };

            TextBox TheTextBox = new TextBox();
            TheTextBox.Margin = new Thickness(3);
            TheTextBox.Text = RequiredResource.Label;
            TheTextBox.Width = 130;
            TheDockPanel.Children.Add(TheTextBox);

            ComboBox TheComboBox = new ComboBox();
            TheComboBox.Margin = new Thickness(3);
            TheDockPanel.Children.Add(TheComboBox);



            ComboBoxItem ComboBoxItemFile = new ComboBoxItem();
            ComboBoxItemFile.Content = "File";
            TheComboBox.Items.Add(ComboBoxItemFile);

            ComboBoxItem ComboBoxItemFolder = new ComboBoxItem();
            ComboBoxItemFolder.Content = "Folder";
            TheComboBox.Items.Add(ComboBoxItemFolder);

            if (RequiredResource.Type == CommandResource.ResourceTypes.File)
            {
                ComboBoxItemFile.IsSelected = true;
            }
            if (RequiredResource.Type == CommandResource.ResourceTypes.Folder)
            {
                ComboBoxItemFolder.IsSelected = true;
            }



            CommandEventResourcesPanel.Children.Add(TheDockPanel);
        }

        

        private void NewCommonEventCommandPanel(Command ACommand) 
        {
            DockPanel TheDockPanel = new DockPanel();
            DockPanel.SetDock(TheDockPanel, Dock.Top);
            TheDockPanel.Margin = new Thickness(1, 1, 1, 1);

            Button DeleteButton = new Button();
            DeleteButton.Content = " Delete ";
            DockPanel.SetDock(DeleteButton, Dock.Right);
            TheDockPanel.Children.Add(DeleteButton);
            DeleteButton.Click += (s, e) =>
            {
                CommonEventCommandsPanel.Children.Remove(TheDockPanel);
            };

            ComboBox TheComboBox = new ComboBox();
            TheComboBox.Margin = new Thickness(3);
            TheDockPanel.Children.Add(TheComboBox);

            foreach (var thecommand in TrueDatabase.Commands)
            {
                var commanditem = new ComboBoxItem
                {
                    Content = thecommand.DisplayName,
                    Tag = thecommand
                };
                TheComboBox.Items.Add(commanditem);

                if (ACommand == thecommand)
                {
                    commanditem.IsSelected = true;
                }
            }

            CommonEventCommandsPanel.Children.Add(TheDockPanel);
        }



        private void ButtonSaveAllClick(object sender, RoutedEventArgs e)
        {
                        

            //Step 1: Make sure everything can save to a Example location, letting us test if a problem (crash) would occur, before actually saving to the right location.
            string ExtraPath = "\\Lab"; //This extra string causes stuff to be saved to a path variant of the normal location, 
            Directory.CreateDirectory(LibraryMan.ApplicationLocation + ExtraPath + "\\");

            //Step 1.5: Save to the example location.
            SaveDevStuff(ExtraPath);

            //Step 2: Delete everything in the example location. (The Lab folder)
            Directory.Delete(LibraryMan.ApplicationLocation + ExtraPath + "\\", true);

            ////Step 3: Delete everything in the REAL location.
            //ExtraPath = "";
            //string FolderPath = LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events";
            //DirectoryInfo DummyDirectory = new DirectoryInfo(FolderPath);
            //foreach (FileInfo file in DummyDirectory.GetFiles())
            //{
            //    file.Delete();
            //}
            //foreach (DirectoryInfo subDirectory in DummyDirectory.GetDirectories())
            //{
            //    subDirectory.Delete(true);
            //}


            //Step 4: Save everything for real.
            ExtraPath = "";
            SaveDevStuff(ExtraPath);



            void SaveDevStuff(string SaveLocation) 
            {
                try
                {
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + SaveLocation + "\\Other\\" );

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryMan.ApplicationLocation + SaveLocation + "\\Other\\Tools.xml", settings))
                    {
                        writer.WriteStartElement("Tools");
                        writer.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryMan.VersionDate);

                        writer.WriteStartElement("ToolList");
                        foreach (Tool tool in TrueDatabase.Tools)
                        {
                            writer.WriteStartElement("Tool");

                            writer.WriteElementString("Name", tool.DisplayName);
                            writer.WriteElementString("Description", tool.Description);
                            writer.WriteElementString("Notepad", tool.Notepad);
                            writer.WriteElementString("Key", tool.Key);
                            writer.WriteElementString("Category", tool.Category);
                            writer.WriteElementString("ExecutableName", tool.ExeName);
                            writer.WriteElementString("DownloadLink", tool.DownloadLink);

                            writer.WriteEndElement(); //End Tool
                        }
                        writer.WriteEndElement(); //End ToolList

                        writer.WriteEndElement(); //End Tools
                        writer.Flush(); //Ends the XML 
                    }
                }
                catch
                {
                    FailedSave("Tools");
                }

                try
                {
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + SaveLocation + "\\Other\\");

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryMan.ApplicationLocation + SaveLocation + "\\Other\\Commands.xml", settings))
                    {
                        writer.WriteStartElement("Commands");
                        writer.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryMan.VersionDate);

                        writer.WriteStartElement("CommandsList");
                        foreach (Command command in TrueDatabase.Commands)
                        {
                            writer.WriteStartElement("Command");

                            writer.WriteElementString("Name", command.DisplayName);
                            writer.WriteElementString("Description", command.Description);
                            writer.WriteElementString("Notepad", command.Notepad);
                            writer.WriteElementString("Key", command.Key);
                            writer.WriteElementString("Category", command.Category);
                            writer.WriteElementString("Group", command.Group);
                            writer.WriteElementString("MethodName", command.MethodName);
                            writer.WriteStartElement("RequiredToolList");
                            foreach (Tool RequiredTool in command.RequiredToolsList) 
                            {
                                writer.WriteStartElement("Tool");
                                writer.WriteElementString("Name", RequiredTool.DisplayName);
                                writer.WriteElementString("Key", RequiredTool.Key);
                                writer.WriteEndElement(); //End Tool
                            }
                            writer.WriteEndElement(); //End RequiredTools
                            writer.WriteStartElement("RequiredResourceList");
                            foreach (CommandResource RequiredResource in command.RequiredResourcesList)
                            {
                                writer.WriteStartElement("Resource");
                                writer.WriteElementString("Label", RequiredResource.Label);
                                writer.WriteElementString("Type", RequiredResource.Type.ToString());
                                //writer.WriteElementString("Optional", RequiredResource.IsOptional.ToString());
                                writer.WriteEndElement(); //End Resource
                            }
                            writer.WriteEndElement(); //End RequiredResources                            

                            writer.WriteEndElement(); //End Command
                        }
                        writer.WriteEndElement(); //End CommandsList

                        writer.WriteEndElement(); //End Commands
                        writer.Flush(); //Ends the XML 
                    }
                }
                catch
                {
                    FailedSave("Commands");
                }

                try
                {
                    Directory.CreateDirectory(LibraryMan.ApplicationLocation + SaveLocation + "\\Other\\");

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryMan.ApplicationLocation + SaveLocation + "\\Other\\Common Events.xml", settings))
                    {
                        writer.WriteStartElement("CommonEvents");
                        writer.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryMan.VersionDate);

                        writer.WriteStartElement("CommonEventList");
                        foreach (CommonEvent commonevent in TrueDatabase.CommonEvents)
                        {
                            writer.WriteStartElement("CommonEvent");

                            writer.WriteElementString("Name", commonevent.DisplayName);
                            writer.WriteElementString("Description", commonevent.Description);
                            writer.WriteElementString("Notepad", commonevent.Notepad);
                            writer.WriteElementString("Key", commonevent.Key);
                            writer.WriteElementString("Category", commonevent.Category);

                            writer.WriteStartElement("CommandList");
                            foreach (Command command in commonevent.MyCommands)
                            {
                                writer.WriteStartElement("Command");
                                writer.WriteElementString("Name", command.DisplayName);
                                writer.WriteElementString("Key", command.Key);
                                writer.WriteEndElement(); //End Command
                            }
                            writer.WriteEndElement(); //End Commands

                            writer.WriteEndElement(); //End CommonEvent
                        }
                        writer.WriteEndElement(); //End CommonEventList

                        writer.WriteEndElement(); //End CommonEvents
                        writer.Flush(); //Ends the XML 
                    }
                }
                catch
                {
                    FailedSave("Common Events");
                }


            }

            void FailedSave(string SomeText) 
            {
                LibraryMan.NotificationNegative("Error: Failed to save properly.",
                    "I will now crash." +
                    "\n\n" +
                    "PS: I set it up to first attempt to save to a folder named Lab to prevent data corruption, so everything should be fine. :D"
                    );

                Environment.FailFast(null); //Kills program instantly. 
                return;
            }
        }

        private void ButtonSaveToolClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            tool.DisplayName = ToolNameTextbox.Text;
            tool.Description = ToolDescriptionTextbox.Text;
            tool.Key = ToolKeyTextbox.Text;
            tool.ExeName = ToolExeTextbox.Text;
            tool.DownloadLink = ToolDownloadLink.Text;
            tool.Notepad = ToolNotepadTextbox.Text;
            tool.Category = ToolTabCombobox.Text;

            item.Header = tool.DisplayName;

        }

        private void ButtonSaveCommandClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;
                      

            command.DisplayName = CommandNameTextbox.Text;
            command.Description = CommandDescriptionTextbox.Text;
            command.Key = CommandNewKeyTextbox.Text;
            command.MethodName = CommandMethodNameTextbox.Text;
            command.Notepad = CommandNotepadTextbox.Text;
            command.Category = CommandTabCombobox.Text;
            command.Group = CommandSubCombobox.Text;
            command.RequiredToolsList.Clear();

            //Copilot did all this, and im gonna actually trust it for a change. 
            foreach (DockPanel TheDockPanel in CommandToolsPanel.Children)
            {
                ComboBox TheComboBox = (ComboBox)TheDockPanel.Children[1];
                if (TheComboBox.SelectedItem != null)
                {
                    var tool = (Tool)((ComboBoxItem)TheComboBox.SelectedItem).Tag;
                    command.RequiredToolsList.Add(tool);
                }
            }
            command.RequiredResourcesList.Clear();
            foreach (DockPanel TheDockPanel in CommandEventResourcesPanel.Children)
            {
                TextBox TheTextBox = (TextBox)TheDockPanel.Children[1];
                ComboBox TheComboBox = (ComboBox)TheDockPanel.Children[2];
                if (TheComboBox.SelectedItem != null)
                {
                    var resource = new CommandResource
                    {
                        Label = TheTextBox.Text,
                        Type = (CommandResource.ResourceTypes)Enum.Parse(typeof(CommandResource.ResourceTypes), ((ComboBoxItem)TheComboBox.SelectedItem).Content.ToString()),
                        IsOptional = false // Set this to true or false based on your logic
                    };
                    command.RequiredResourcesList.Add(resource);
                }
            }

            item.Header = command.DisplayName;
        }

        private void ButtonSaveCommonEventClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var commonevent = (CommonEvent)item.Tag;
            if (commonevent == null) return;

            commonevent.DisplayName = CommonEventNameTextbox.Text;
            commonevent.Description = CommonEventDescriptionTextbox.Text;
            commonevent.Key = CommonEventNewKeyTextbox.Text;
            commonevent.Notepad = CommonEventNotepadTextbox.Text;
            commonevent.Category = CommonEventTabCombobox.Text;
            commonevent.MyCommands.Clear();
            //Copilot did all this, and im gonna actually trust it for a change. 
            foreach (DockPanel TheDockPanel in CommonEventCommandsPanel.Children)
            {
                ComboBox TheComboBox = (ComboBox)TheDockPanel.Children[1];
                if (TheComboBox.SelectedItem != null)
                {
                    var command = (Command)((ComboBoxItem)TheComboBox.SelectedItem).Tag;
                    commonevent.MyCommands.Add(command);
                }
            }

            item.Header = commonevent.DisplayName;

        }

        private void ButtonNewTool(object sender, RoutedEventArgs e)
        {
            Tool Tool = new();
            TreeViewItem treeViewItem = new TreeViewItem
            {

                Tag = Tool,
                Header = Tool.DisplayName,
            };
            ToolsTree.Items.Add(treeViewItem);
            TrueDatabase.Tools.Add(Tool); 
        }

        private void ButtonNewCommand(object sender, RoutedEventArgs e)
        {
            Command Command = new();
            TreeViewItem treeViewItem = new TreeViewItem
            {
                Tag = Command,
                Header = Command.DisplayName,
            };
            CommandsTree.Items.Add(treeViewItem);
            TrueDatabase.Commands.Add(Command);
        }

        private void ButtonNewCommonEvent(object sender, RoutedEventArgs e)
        {
            CommonEvent CommonEvent = new();
            TreeViewItem treeViewItem = new TreeViewItem
            {
                Tag = CommonEvent,
                Header = CommonEvent.DisplayName,
            };
            CommonEventsTree.Items.Add(treeViewItem);
            TrueDatabase.CommonEvents.Add(CommonEvent);
        }

        private void ButtonDeleteTool(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            TrueDatabase.Tools.Remove(tool);
            ToolsTree.Items.Remove(item);
        }

        private void ButtonDeleteCommand(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;

            TrueDatabase.Commands.Remove(command);
            CommandsTree.Items.Remove(item);
        }

        private void ButtonDeleteCommonEvent(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var commonevent = (CommonEvent)item.Tag;
            if (commonevent == null) return;

            TrueDatabase.CommonEvents.Remove(commonevent);
            CommonEventsTree.Items.Remove(item);
        }

        private void ButtonMoveToolUp(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            LibraryMan.MoveListItemUp(TrueDatabase.Tools, tool);
            LibraryMan.MoveTreeItemUp(ToolsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveToolDown(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            LibraryMan.MoveListItemDown(TrueDatabase.Tools, tool);
            LibraryMan.MoveTreeItemDown(ToolsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommandUp(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;

            LibraryMan.MoveListItemUp(TrueDatabase.Commands, command);
            LibraryMan.MoveTreeItemUp(CommandsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommandDown(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;

            LibraryMan.MoveListItemDown(TrueDatabase.Commands, command);
            LibraryMan.MoveTreeItemDown(CommandsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommonUp(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var common = (CommonEvent)item.Tag;
            if (common == null) return;

            LibraryMan.MoveListItemUp(TrueDatabase.CommonEvents, common);
            LibraryMan.MoveTreeItemUp(CommonEventsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommonDown(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var common = (CommonEvent)item.Tag;
            if (common == null) return;

            LibraryMan.MoveListItemDown(TrueDatabase.CommonEvents, common);
            LibraryMan.MoveTreeItemDown(CommonEventsTree, item);

            item.IsSelected = true;
        }
    }
}
