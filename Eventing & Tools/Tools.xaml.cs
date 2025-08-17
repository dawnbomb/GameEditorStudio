using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Security.Policy;
using System.ComponentModel.Design;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GameEditorStudio
{
    public partial class ToolsMenu : Window
    {
        public WorkshopData WorkshopData { get; set; } // May eventually be used to check a workshop's Common Events? 
        public TopMenu SharedMenus { get; set; } //not used, but i might use in the future.
        public List<DockPanel> dockList { get; set; } = new();
        public string ColorCode { get; set; } = "#090917";

        public ToolsMenu(TopMenu SharedMenus, WorkshopData WorkshopData)
        {
            InitializeComponent();

            this.WorkshopData = WorkshopData;  
            
            //if (WorkshopData == null ) { this.Title = "Current Workshop: None"; }   
            if (WorkshopData != null) { this.Title = "Current Workshop: " + WorkshopData.WorkshopName; }

            SetupToolsTree(dockList);
            SetupCommonEventTree(dockList);
            SetupTools();
            SetupCommonEvents();


        }

        public void SetupToolsTree(List<DockPanel> dockList)
        {
            HashSet<string> createdTabs = new HashSet<string>();  // To track which tabs have already been created

            foreach (Tool tool in Database.Tools)
            {
                if (createdTabs.Contains(tool.Category)) continue;

                createdTabs.Add(tool.Category);

                TreeViewItem treeItem = new TreeViewItem
                {
                    Header = tool.Category,
                    // Correctly assign the DockPanel to the Tag
                };
                TreeViewTools.Items.Add(treeItem);

                DockPanel panelForTab = new DockPanel
                {
                    LastChildFill = false,
                    Visibility = Visibility.Collapsed,  // Initially hidden
                    Name = $"Panel{tool.Category.Replace(" ", "")}",  // Remove spaces for a valid name
                };
                panelForTab.Style = (Style)FindResource("DockList");

                dockList.Add(panelForTab);
                TheScrollPanel.Children.Add(panelForTab);
                treeItem.Tag = panelForTab; // Set the DockPanel as the Tag

                // Attach an event handler to the tree item for when it's selected
                treeItem.Selected += (sender, e) =>
                {
                    foreach (var panel in dockList)
                    {
                        // Hide all panels
                        panel.Visibility = Visibility.Collapsed;
                    }
                    // Show only the related panel
                    panelForTab.Visibility = Visibility.Visible;
                };
            }
        }


        public void SetupCommonEventTree(List<DockPanel> dockList)
        {
            HashSet<string> createdTabs = new HashSet<string>();  // To track which tabs have already been created

            foreach (CommonEvent commonEvent in Database.CommonEvents)
            {
                if (createdTabs.Contains(commonEvent.Category)) continue;

                createdTabs.Add(commonEvent.Category);

                // Create a TreeViewItem for this common event's tab
                TreeViewItem treeItem = new TreeViewItem
                {
                    Header = commonEvent.Category,                    
                    //Tag = commonEvent.Tab  // Use Tag to store the tab name for later reference
                };
                TreeViewCommonEvents.Items.Add(treeItem);

                // Create a corresponding DockPanel for this tab
                DockPanel panelForTab = new DockPanel
                {                    
                    LastChildFill = false,
                    Visibility = Visibility.Collapsed,  // Initially hidden
                    Name = $"Panel{commonEvent.Category.Replace(" ", "")}",  // Create a valid name by removing spaces
                };
                panelForTab.Style = (Style)FindResource("DockList");

                dockList.Add(panelForTab);  // Add to the list for management
                TheScrollPanel.Children.Add(panelForTab);  // Add to the ScrollViewer (assuming a common ScrollViewer is used)
                treeItem.Tag = panelForTab;

                // Attach an event handler to the tree item for when it's selected
                treeItem.Selected += (sender, e) =>
                {
                    foreach (var panel in dockList)
                    {
                        // Hide all panels
                        panel.Visibility = Visibility.Collapsed;
                    }
                    // Show only the related panel
                    panelForTab.Visibility = Visibility.Visible;
                };
            }
        }
                

        public void SetupTools()
        {
            foreach (Tool ThisTool in Database.Tools)
            {
                Border border = new();
                border.BorderBrush = Brushes.Black; //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#101010"))
                border.Margin = new Thickness(10, 8, 10, 0);
                border.Height = 36;
                border.MinWidth = 700;
                DockPanel.SetDock(border, Dock.Top);

                DockPanel ToolPanel = new DockPanel();
                border.Child = ToolPanel;
                
                
                ToolPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                ToolPanel.MouseEnter += (sender, e) => ToolPanel_MouseEnter(sender, e, ThisTool);

                // Find corresponding TreeViewItem by Tab value and add MainPanel to its DockPanel
                foreach (TreeViewItem treeItem in TreeViewTools.Items)
                {
                    if (treeItem.Header.ToString() == ThisTool.Category)
                    {
                        DockPanel CategoryPanel = treeItem.Tag as DockPanel;
                        CategoryPanel.Children.Add(border);
                        break;
                    }
                }

                SetupToolPanel(ToolPanel, ThisTool);
            }
        }

        private void SetupToolPanel(DockPanel toolPanel, Tool tool)
        {
            // Name Label
            Label nameLabel = new Label
            {
                Content = tool.DisplayName,
                Width = 240,
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 0)
            };
            toolPanel.Children.Add(nameLabel);

            // Open Button
            Button openButton = new Button
            {
                Content = "Open...",
                Width = 82,
                FontSize = 20,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            openButton.Click += (sender, e) => OpenToolFolder(sender, e, tool);
            DockPanel.SetDock(openButton, Dock.Right);
            toolPanel.Children.Add(openButton);

            // Download Button
            if (!string.IsNullOrEmpty(tool.DownloadLink))
            {
                Button downloadButton = new Button
                {
                    Content = "Download",
                    Width = 102,
                    FontSize = 20,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                downloadButton.Click += (sender, e) => Process.Start(new ProcessStartInfo(tool.DownloadLink) { UseShellExecute = true });
                DockPanel.SetDock(downloadButton, Dock.Right);
                toolPanel.Children.Add(downloadButton);
            }

            // Browse Button
            Button browseButton = new Button
            {
                Content = "Browse...",
                Width = 98,
                FontSize = 20
            };
            browseButton.Click += (sender, e) => SaveToolsXML(sender, e, tool);
            toolPanel.Children.Add(browseButton);

            // Location TextBox
            TextBox locationTextBox = new TextBox
            {
                MinWidth = 50,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF191919")),
                FontSize = 20,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Binding locationBinding = new Binding("Location")
            {
                Source = tool,
                Mode = BindingMode.TwoWay
            };
            locationTextBox.SetBinding(TextBox.TextProperty, locationBinding);
            DockPanel.SetDock(locationTextBox, Dock.Right);
            toolPanel.Children.Add(locationTextBox);
            browseButton.Tag = locationTextBox;
        }







        public void SetupCommonEvents()
        {
            foreach (CommonEvent commonEvent in Database.CommonEvents)
            {
                Border border = new();
                border.BorderBrush = Brushes.Black; //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#101010"))
                border.Margin = new Thickness(10, 8, 10, 0);
                border.Height = 36;
                border.MinWidth = 700;
                DockPanel.SetDock(border, Dock.Top);

                

                DockPanel commandPanel = new DockPanel();
                border.Child = commandPanel;

                commandPanel.MouseEnter += (sender, e) => EventPanel_MouseEnter(sender, e, commonEvent);


                foreach (TreeViewItem treeItem in TreeViewCommonEvents.Items)
                {
                    if (treeItem.Header.ToString() == commonEvent.Category)
                    {
                        DockPanel CategoryPanel = treeItem.Tag as DockPanel;
                        CategoryPanel.Children.Add(border);
                        break;
                    }
                }                

                SetupEventControls(commandPanel, commonEvent);
            }
        }

        private void SetupEventControls(DockPanel commandPanel, CommonEvent commonEvent)
        {
            Label commandLabel = new Label
            {
                Content = commonEvent.DisplayName,
                FontSize = 20,
                Width = 200
            };
            commandPanel.Children.Add(commandLabel);

            CheckBox checkBoxLocal = new CheckBox
            {
                Foreground = Brushes.White,
                LayoutTransform = new ScaleTransform(1.8, 1.8),
                Margin = new Thickness(5, 0, 0, 0),
                IsChecked = commonEvent.Local
            };
            checkBoxLocal.Checked += (sender, e) => { commonEvent.Local = true; SaveCommonEventsLocal(); };
            checkBoxLocal.Unchecked += (sender, e) => { commonEvent.Local = false; SaveCommonEventsLocal(); };
            commandPanel.Children.Add(checkBoxLocal);

            Label labelLocal = new Label
            {
                Content = "Local",
                Width = 80
            };
            commandPanel.Children.Add(labelLocal);

            if (WorkshopData != null)  //!string.IsNullOrWhiteSpace(WorkshopData.WorkshopName)
            {
                CheckBox checkBoxWorkshop = new CheckBox
                {
                    Foreground = Brushes.White,
                    LayoutTransform = new ScaleTransform(1.8, 1.8),
                    Margin = new Thickness(5, 0, 0, 0),
                    IsChecked = commonEvent.Workshop
                };
                checkBoxWorkshop.Checked += (sender, e) => //IF CHECKED
                { 
                    commonEvent.Workshop = true; 
                    WorkshopData.WorkshopCommonEvents.Add(commonEvent);
                    SaveCommonEventsWorkshop(); 
                };
                checkBoxWorkshop.Unchecked += (sender, e) => //IF UNCHECKED
                { 
                    commonEvent.Workshop = false; 
                    WorkshopData.WorkshopCommonEvents.RemoveAll(ce => ce.Key == commonEvent.Key);
                    SaveCommonEventsWorkshop(); 
                };
                commandPanel.Children.Add(checkBoxWorkshop);

                Label labelWorkshop = new Label
                {
                    Content = "Workshop"
                };
                commandPanel.Children.Add(labelWorkshop);
            }

            Label Label = new();
            Label.Content = "This is a really long label!!! OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO";
            Label.Visibility = Visibility.Hidden; //This creates something invisible but taking space to make it look aligned pretty because i give up atm for doing it properly.
            commandPanel.Children.Add(Label);
        }




        private void ToolPanel_MouseEnter(object sender, MouseEventArgs e, Tool Tool)
        {
            ToolNameBox.Text = Tool.DisplayName;
            ToolDescriptionBox.Text = Tool.Description;
            //ToolLocationBox.Text = Tool.Location;
        }

        private void EventPanel_MouseEnter(object sender, MouseEventArgs e, CommonEvent Event)
        {
            ToolNameBox.Text = Event.DisplayName;
            ToolDescriptionBox.Text = Event.Description;
            //ToolLocationBox.Text = Tool.Location;
        }


        public void OpenToolFolder(object sender, RoutedEventArgs e, Tool Tool) 
        {
            string ToolDirectory = Path.GetDirectoryName(Tool.Location);
            if (ToolDirectory == null || ToolDirectory == "") { return; }

            try
            {                

                if (Directory.Exists(ToolDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", ToolDirectory);
                }
                else
                {
                    System.Windows.MessageBox.Show("We can't find where your tool folder is! :(" +
                        "\n" +                        
                        "\nThis error isn't really accounted for. Maybe you should save and restart the program?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }

            }
            catch
            {
                PixelWPF.LibraryPixel.NotificationGenericError();
                return;
            }
        }
        

        public void SaveToolsXML(object sender, RoutedEventArgs e, Tool Tool)
        {
            if (!Directory.Exists(LibraryGES.ApplicationLocation + "\\Settings")) 
            {
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Settings");
            }            

            VistaOpenFileDialog FileSelect = new VistaOpenFileDialog();
            FileSelect.Title = "Select the Exe";
            if ((bool)FileSelect.ShowDialog(this))
            {
                Button clickedButton = (Button)sender;
                TextBox associatedTextBox = (TextBox)clickedButton.Tag;
                associatedTextBox.Text = FileSelect.FileName;
                Tool.Location = FileSelect.FileName;
            }





            XmlWriterSettings settings = new();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + "\\Settings\\Tools.xml", settings))
            {
                writer.WriteStartElement("Tools"); //This is the root of the XML   
                writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
                foreach (Tool tool in Database.Tools)
                {
                    writer.WriteStartElement("Tool");
                    writer.WriteElementString("Name", tool.DisplayName.ToString());
                    writer.WriteElementString("Key", tool.Key.ToString());
                    writer.WriteElementString("Location", tool.Location);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); //End Tools  AKA the Root of the XML   
                writer.Flush(); //Ends the XML File
            }
        }

        public void SaveCommonEventsLocal() 
        {
            if (!Directory.Exists(LibraryGES.ApplicationLocation + "\\Settings"))
            {
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Settings");
            }

            XmlWriterSettings settings = new();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + "\\Settings\\Common Events.xml", settings))
            {

                writer.WriteStartElement("CommonEvents"); //This is the root of the XML   
                writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
                writer.WriteElementString("ReadMe", "This is a list of every common event you have globally enabled. (IE events ALWAYS available, useful for power users who mod many games) " +
                    "\nEvents enabled for a specific workshop are instead stored in Workshops/(Workshop Folder)/CommonEvents.xml " +
                    "\n(even if i move it, it'll be in there somewhere.)");


                foreach (CommonEvent commonevent in Database.CommonEvents)
                {
                    if (commonevent.Local == true)
                    {
                        writer.WriteStartElement("CommonEvent");

                        writer.WriteElementString("Name", commonevent.DisplayName);
                        writer.WriteElementString("Key", commonevent.Key);

                        writer.WriteEndElement(); //End CommonEvent 
                    }

                }



                writer.WriteEndElement(); //End Tools  AKA the Root of the XML   
                writer.Flush(); //Ends the XML File
            }
        }

        public void SaveCommonEventsWorkshop() 
        {
            XmlWriterSettings settings = new();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Common Events.xml", settings))
            {

                writer.WriteStartElement("CommonEvents"); //This is the root of the XML   
                writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
                writer.WriteElementString("ReadMe", "This is a list of every common event the workshop maker decided is relevant for modding this game." +
                    "\nEven if a user doesn't have a common event locally enabled it will appear anyway, and even if they are missing a tool or something the common event will still appear, but gray.");


                foreach (CommonEvent Event in Database.CommonEvents)
                {
                    if (Event.Workshop == true)
                    {
                        writer.WriteStartElement("CommonEvent");
                        writer.WriteElementString("Name", Event.DisplayName);
                        writer.WriteElementString("Key", Event.Key);
                        writer.WriteEndElement(); //End CommonEvent
                    }

                }



                writer.WriteEndElement(); //End Tools  AKA the Root of the XML   
                writer.Flush(); //Ends the XML File
            }
        }



        private void TreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView activeTreeView && e.NewValue is TreeViewItem selectedItem)
            {
                // Determine which tree view is the other one to clear its selection
                TreeView otherTreeView = activeTreeView == TreeViewTools ? TreeViewCommonEvents : TreeViewTools;

                // Clear selection in the other tree view by setting IsSelected to false for all items
                ClearTreeViewSelection(otherTreeView);

                // Hide all DockPanels
                foreach (DockPanel panel in TheScrollPanel.Children.OfType<DockPanel>())
                {
                    panel.Visibility = Visibility.Collapsed;
                }

                // Show the DockPanel associated with the selected item if it exists
                DockPanel correspondingPanel = selectedItem.Tag as DockPanel;
                if (correspondingPanel != null)
                {
                    correspondingPanel.Visibility = Visibility.Visible;
                }
            }
        }


        private void ClearTreeViewSelection(TreeView treeView)
        {
            foreach (TreeViewItem item in treeView.Items)
            {
                item.IsSelected = false;
                // Optionally, if items have children and you want to recursively deselect:
                ClearTreeViewItemSelection(item);
            }
        }

        private void ClearTreeViewItemSelection(TreeViewItem treeViewItem)
        {
            treeViewItem.IsSelected = false;
            foreach (TreeViewItem child in treeViewItem.Items)
            {
                ClearTreeViewItemSelection(child);
            }
        }




    }



}
