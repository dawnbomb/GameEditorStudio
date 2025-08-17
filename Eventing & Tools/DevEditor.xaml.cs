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
    public partial class DevEditor : Window
    {        
        //The commands categorys and groups are auto-generated. They are also just strings and not enums. 

        public DevEditor()
        {
            InitializeComponent();

            List<string> ToolCategoryNames = new();
            List<string> CommandCategoryNames = new();
            List<string> CommandGroupNames = new();
            List<string> CommonCategoryNames = new();

            //ToolTabCombobox
            foreach (var tool in Database.Tools)
            {
                var item = new TreeViewItem
                {
                    Header = tool.DisplayName,
                    Tag = tool
                };
                ToolsTree.Items.Add(item);

                if (!ToolCategoryNames.Contains(tool.Category)) 
                {
                    ToolCategoryNames.Add(tool.Category);
                }
            }
            foreach (var category in ToolCategoryNames)
            {
                var comboItem = new ComboBoxItem
                {
                    Content = category
                };
                ToolTabCombobox.Items.Add(comboItem);
            }


            foreach (var command in Database.Commands)
            {
                var item = new TreeViewItem
                {
                    Header = command.DisplayName,
                    Tag = command
                };
                CommandsTree.Items.Add(item);

                if (!CommandCategoryNames.Contains(command.Category))
                {
                    CommandCategoryNames.Add(command.Category);
                }
                if (!CommandGroupNames.Contains(command.Group))
                {
                    CommandGroupNames.Add(command.Group);
                }
            }
            foreach (var category in CommandCategoryNames)
            {
                var comboItem = new ComboBoxItem
                {
                    Content = category
                };
                CommandTabCombobox.Items.Add(comboItem);
            }
            foreach (var group in CommandGroupNames)
            {
                var comboItem = new ComboBoxItem
                {
                    Content = group
                };
                CommandSubCombobox.Items.Add(comboItem);
            }


            foreach (var commonevent in Database.CommonEvents)
            {
                var item = new TreeViewItem
                {
                    Header = commonevent.DisplayName,
                    Tag = commonevent
                };
                CommonEventsTree.Items.Add(item);

                if (!CommonCategoryNames.Contains(commonevent.Category))
                {
                    CommonCategoryNames.Add(commonevent.Category);
                }
            }
            foreach (var category in CommonCategoryNames)
            {
                var comboItem = new ComboBoxItem
                {
                    Content = category
                };
                CommonEventTabCombobox.Items.Add(comboItem);
            }


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
            ToolCategoryTextbox.Text = tool.Category;




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

            CommandCategoryTextbox.Text = command.Category;
            CommandGroupTextbox.Text = command.Group;


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
            CommonCategoryTextbox.Text = commonevent.Category;

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
            
            NewCommandToolPanel(Database.Tools[0]);
        }

        private void CommandButtonNewResource(object sender, RoutedEventArgs e)
        {
            CommandResource RequiredResource = new();
            NewCommandResourcePanel(RequiredResource);
        }

        private void CommonEventButtonNewCommand(object sender, RoutedEventArgs e)
        {
            NewCommonEventCommandPanel(Database.Commands[0]);
        }

        private void NewCommandToolPanel(Tool RequiredTool) 
        {
            DockPanel TheDockPanel = new DockPanel();
            DockPanel.SetDock(TheDockPanel, Dock.Top);
            TheDockPanel.Margin = new Thickness(3);

            string numba = CommandToolsPanel.Children.Count.ToString();
            Label CountLabel = new Label();
            CountLabel.Content = "Tool " + numba + ":";
            TheDockPanel.Children.Add(CountLabel);
            DockPanel.SetDock(CountLabel, Dock.Left);

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

            foreach (var tool in Database.Tools)
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
            TheDockPanel.Margin = new Thickness(3);

            string numba = CommandEventResourcesPanel.Children.Count.ToString();
            Label CountLabel = new Label();
            CountLabel.Content = "Resource " + numba + ":";
            TheDockPanel.Children.Add(CountLabel);
            DockPanel.SetDock(CountLabel, Dock.Left);

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
            TheTextBox.Width = 150;
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

            //ComboBoxItem ComboBoxItemText = new ComboBoxItem();
            //ComboBoxItemText.Content = "Text";
            //TheComboBox.Items.Add(ComboBoxItemText);

            if (RequiredResource.Type == CommandResource.ResourceTypes.File)
            {
                ComboBoxItemFile.IsSelected = true;
            }
            if (RequiredResource.Type == CommandResource.ResourceTypes.Folder)
            {
                ComboBoxItemFolder.IsSelected = true;
            }
            //if (RequiredResource.Type == CommandResource.ResourceTypes.Text)
            //{
            //    ComboBoxItemText.IsSelected = true;
            //}



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

            foreach (var thecommand in Database.Commands)
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
            Directory.CreateDirectory(LibraryGES.ApplicationLocation + ExtraPath + "\\");

            //Step 1.5: Save to the example location.
            SaveDevStuff(ExtraPath);

            //Step 2: Delete everything in the example location. (The Lab folder)
            Directory.Delete(LibraryGES.ApplicationLocation + ExtraPath + "\\", true);

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
                    Directory.CreateDirectory(LibraryGES.ApplicationLocation + SaveLocation + "\\Other\\" );

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + SaveLocation + "\\Other\\Tools.xml", settings))
                    {
                        writer.WriteStartElement("Tools");
                        writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryGES.VersionDate);

                        writer.WriteStartElement("ToolList");
                        foreach (Tool tool in Database.Tools)
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
                    Directory.CreateDirectory(LibraryGES.ApplicationLocation + SaveLocation + "\\Other\\");

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + SaveLocation + "\\Other\\Commands.xml", settings))
                    {
                        writer.WriteStartElement("Commands");
                        writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryGES.VersionDate);

                        writer.WriteStartElement("CommandsList");
                        foreach (Command command in Database.Commands)
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
                    Directory.CreateDirectory(LibraryGES.ApplicationLocation + SaveLocation + "\\Other\\");

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + SaveLocation + "\\Other\\Common Events.xml", settings))
                    {
                        writer.WriteStartElement("CommonEvents");
                        writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryGES.VersionDate);

                        writer.WriteStartElement("CommonEventList");
                        foreach (CommonEvent commonevent in Database.CommonEvents)
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
                PixelWPF.LibraryPixel.NotificationNegative("Error: Failed to save properly.",
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
            //tool.Category = ToolTabCombobox.Text;
            tool.Category = ToolCategoryTextbox.Text;

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
            //command.Category = CommandTabCombobox.Text;
            //command.Group = CommandSubCombobox.Text;
            command.Category = CommandCategoryTextbox.Text;
            command.Group = CommandGroupTextbox.Text;
            command.RequiredToolsList.Clear();
            command.RequiredResourcesList.Clear();

            //Copilot did all this, and im gonna actually trust it for a change. 
            foreach (DockPanel TheDockPanel in CommandToolsPanel.Children)
            {
                //This will error if i change the order of the tool panel's children.
                ComboBox TheComboBox = (ComboBox)TheDockPanel.Children[2];
                if (TheComboBox.SelectedItem != null)
                {
                    var tool = (Tool)((ComboBoxItem)TheComboBox.SelectedItem).Tag;
                    command.RequiredToolsList.Add(tool);
                }
            }
            
            foreach (DockPanel TheDockPanel in CommandEventResourcesPanel.Children)
            {
                //This will error if i change the order of the resource panel's children.
                TextBox TheTextBox = (TextBox)TheDockPanel.Children[2];
                ComboBox TheComboBox = (ComboBox)TheDockPanel.Children[3];
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
            //commonevent.Category = CommonEventTabCombobox.Text;
            commonevent.Category = CommonCategoryTextbox.Text;
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
            Database.Tools.Add(Tool); 
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
            Database.Commands.Add(Command);
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
            Database.CommonEvents.Add(CommonEvent);
        }

        private void ButtonDeleteTool(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            Database.Tools.Remove(tool);
            ToolsTree.Items.Remove(item);
        }

        private void ButtonDeleteCommand(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;

            Database.Commands.Remove(command);
            CommandsTree.Items.Remove(item);
        }

        private void ButtonDeleteCommonEvent(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var commonevent = (CommonEvent)item.Tag;
            if (commonevent == null) return;

            Database.CommonEvents.Remove(commonevent);
            CommonEventsTree.Items.Remove(item);
        }

        private void ButtonMoveToolUp(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            LibraryGES.MoveListItemUp(Database.Tools, tool);
            LibraryGES.MoveTreeItemUp(ToolsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveToolDown(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)ToolsTree.SelectedItem;
            if (item == null) return;
            var tool = (Tool)item.Tag;
            if (tool == null) return;

            LibraryGES.MoveListItemDown(Database.Tools, tool);
            LibraryGES.MoveTreeItemDown(ToolsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommandUp(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;

            LibraryGES.MoveListItemUp(Database.Commands, command);
            LibraryGES.MoveTreeItemUp(CommandsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommandDown(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommandsTree.SelectedItem;
            if (item == null) return;
            var command = (Command)item.Tag;
            if (command == null) return;

            LibraryGES.MoveListItemDown(Database.Commands, command);
            LibraryGES.MoveTreeItemDown(CommandsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommonUp(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var common = (CommonEvent)item.Tag;
            if (common == null) return;

            LibraryGES.MoveListItemUp(Database.CommonEvents, common);
            LibraryGES.MoveTreeItemUp(CommonEventsTree, item);

            item.IsSelected = true;
        }

        private void ButtonMoveCommonDown(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)CommonEventsTree.SelectedItem;
            if (item == null) return;
            var common = (CommonEvent)item.Tag;
            if (common == null) return;

            LibraryGES.MoveListItemDown(Database.CommonEvents, common);
            LibraryGES.MoveTreeItemDown(CommonEventsTree, item);

            item.IsSelected = true;
        }

        private void CommandCategoryDropdownClosed(object sender, EventArgs e)
        {
            //TreeViewItem item = CommandsTree.SelectedItem as TreeViewItem;
            //if (item == null) return;
            //var command = (Command)item.Tag;
            //if (command == null) return;

            //command.Category = CommandTabCombobox.Text;

            CommandCategoryTextbox.Text = CommandTabCombobox.Text;

        }

        private void CommandGroupDropdownClosed(object sender, EventArgs e)
        {
            //TreeViewItem item = CommandsTree.SelectedItem as TreeViewItem;
            //if (item == null) return;
            //var command = (Command)item.Tag;
            //if (command == null) return;

            //command.Group = CommandSubCombobox.Text;

            CommandGroupTextbox.Text = CommandSubCombobox.Text;
        }

        private void ToolCatDropdownClosed(object sender, EventArgs e)
        {
            ToolCategoryTextbox.Text = ToolTabCombobox.Text;
        }

        private void CommonCatDropdownClosed(object sender, EventArgs e)
        {
            CommonCategoryTextbox.Text = CommonEventTabCombobox.Text;
        }
    }
}
