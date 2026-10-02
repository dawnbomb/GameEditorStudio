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

        public DockPanel WPanel { get; set; }
        List<Tool> WSTools { get; set; }

        public ToolsMenu(TopMenu SharedMenus, WorkshopData WorkshopData)
        {
            InitializeComponent();

            this.WorkshopData = WorkshopData;

            //Properties.Settings.Default.ToolsFolder = "";
            //Properties.Settings.Default.Save();

            //if (WorkshopData == null ) { this.Title = "Current Workshop: None"; }   
            if (WorkshopData != null) { this.Title = "Current Workshop: " + WorkshopData.WorkshopName; }
                        
            if (Directory.Exists(Properties.Settings.Default.ToolsFolder)) { ToolsFolderTextbox.Text = Properties.Settings.Default.ToolsFolder; }

            try { SetupToolsTree(dockList); }    catch  { PixelWPF.LibraryPixel.Notification("Tools Crash 1",""); }
            try { SetupCommonEventTree(dockList); }catch { PixelWPF.LibraryPixel.Notification("Tools Crash 2", ""); }
            try { SetupTools(Database.Tools, true); }  catch { PixelWPF.LibraryPixel.Notification("Tools Crash 3", ""); }
            try { SetupCommonEvents(); } catch { PixelWPF.LibraryPixel.Notification("Tools Crash 4", ""); }
            try { SetupThisWorkshop(); } catch { PixelWPF.LibraryPixel.Notification("Tools Crash 5", ""); }            

        }

        private void SetToolsFolderButtonClick(object sender, RoutedEventArgs e)
        {
            PixelWPF.LibraryPixel.Notification("Tools Folder Advice", "" +
                "GES scans your tools folder for tools every time you open a menu. Having a tools folder is super useful!" +
                "\n" +
                "\n====== TIPS =====" +
                "\n1: The tools folder should have ALL your game modding tools. " +
                "\n" +
                "\n2: Put it somewhere OTHER then your GES folder, with a descriptive name like \"Game Romhacking Tools\" or \"Game Modding Tools\". " +
                "\n" +
                "\n3: Organize tools into sub-folders, like \"Hex Editors\", \"SNES Tools\", \"Unity Tools\", etc. " +
                "");


            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select where your Tools folder is. (Or whatever you named it)"; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            {   //Smart seleting the folder to start in.
                //string FolderPath = TextBoxOutputDirectory.Text + "\\";
                //DirectoryInfo? current = new DirectoryInfo(FolderPath);
                //while (current != null && !current.Exists)
                //{
                //    current = current.Parent;
                //}
                //if (current != null)
                //{
                //    FolderSelect.SelectedPath = current.FullName + "\\";
                //}
            }
            if ((bool)FolderSelect.ShowDialog(Window.GetWindow(this))) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                ToolsFolderTextbox.Text = FolderSelect.SelectedPath;
                Properties.Settings.Default.ToolsFolder = FolderSelect.SelectedPath;
                Properties.Settings.Default.Save();

                RefreshWorkshopTools();
            }
        }

        private void OpenToolsFolderButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Directory.Exists(ToolsFolderTextbox.Text))
                {
                    System.Diagnostics.Process.Start("explorer.exe", ToolsFolderTextbox.Text);
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

        public void SetupThisWorkshop() 
        {
            TreeViewItem Witem = new();
            Witem.Header = "Tools in use";
            WorkshopTree.Items.Add(Witem);

            DockPanel TheWPanel = new();
            WPanel = TheWPanel;
            WPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            DockPanel.SetDock(WPanel, Dock.Top);
            WPanel.Style = (Style)FindResource("DockList");
            WPanel.LastChildFill = false;

            dockList.Add(WPanel);
            TheScrollPanel.Children.Add(WPanel);

            WPanel.Visibility = Visibility.Collapsed;  // Initially hidden



            Witem.Tag = WPanel; // Set the DockPanel as the Tag

            Witem.Selected += (sender, e) =>
            {
                foreach (var panel in dockList)
                {
                    panel.Visibility = Visibility.Collapsed;
                }
                WPanel.Visibility = Visibility.Visible;
            };

            try { RefreshWorkshopTools(); }
            catch { PixelWPF.LibraryPixel.Notification("Tools Crash 6", ""); }
            

            Witem.IsSelected = true;
        }

        public void RefreshWorkshopTools() 
        {
            try 
            {
                if (this.WPanel != null)
                {
                    this.WPanel.Children.Clear();
                }

                if (WorkshopData == null) { WorkshopLeftControl.Visibility = Visibility.Collapsed; return; } //Both are here to double make sure.
                if (WorkshopData.WorkshopXaml == null) { WorkshopLeftControl.Visibility = Visibility.Collapsed; return; } //Both are here to double make sure.
                WSTools = WorkshopData.WorkshopTools;
            }
            catch { PixelWPF.LibraryPixel.Notification("Tools Crash 9", ""); }

            
                                    


            Label label = new();
            DockPanel.SetDock(label, Dock.Top);
            WPanel.Children.Add(label);

            //WorkshopData.
            List<Tool> ToolsInUseByCommons = new();
            foreach (CommonEvent commonEvent in Database.CommonEvents)
            {
                if (commonEvent.Workshop == false) { continue; }

                foreach (Command com in commonEvent.MyCommands)
                {
                    foreach (Tool tool in com.RequiredToolsList)
                    {
                        if (!ToolsInUseByCommons.Contains(tool)) 
                        {
                            ToolsInUseByCommons.Add(tool);
                        }                        
                    }
                }
            }
            if (WorkshopData != null) 
            {
                foreach (Event Event in WorkshopData.WorkshopEvents)
                {
                    foreach (EventCommand Ecom in Event.CommandList)
                    {
                        if (Ecom.Command != null) 
                        {
                            foreach (Tool tool in Ecom.Command.RequiredToolsList)
                            {
                                if (tool == null) 
                                { 
                                    PixelWPF.LibraryPixel.Notification("Command Unset Tool Requirement Crash!", "" +
                                        "Command Name: \n\"" + Ecom.Command.DisplayName + "\"" +
                                        "\n\nI (The dev of GES) forgot to set a required tool for this command. " +
                                        "" +
                                        "\n\nThe only other explanation, is in the future i may add support for users to create their own tool requirements, and maybe thats set wrong..." +
                                        "but it's almost certinly my fault." +
                                        "\n\nPlease report! :<" +
                                        "\n\nBecause this is harmless on it's own, i won't force crash you. But GES will probably crash soon from something related to this."
                                    ); 
                                }
                                if (!ToolsInUseByCommons.Contains(tool))
                                {
                                    ToolsInUseByCommons.Add(tool);
                                }
                            }
                            //foreach (CommandResource CR in Ecom.CMDList) 
                            //{
                            //    if (CR.CMDGToolKey != "") 
                            //    {
                            //        foreach (Tool tool in Database.Tools) 
                            //        {
                            //            if (CR.CMDGToolKey == tool.Key) 
                            //            {
                            //                if (!ToolsInUseByCommons.Contains(tool))
                            //                {
                            //                    ToolsInUseByCommons.Add(tool);
                            //                    break;
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                        }                        
                    }                    
                }
            }
            


            //
            label.Content = "GES Tools in use by this workshop (" + ToolsInUseByCommons.Count + ")   (Tools used by Build-A-Command not here yet)";

            try { SetupTools(ToolsInUseByCommons, false); }
            catch { PixelWPF.LibraryPixel.Notification("Tools Crash 7", ""); }
            

            DockPanel WTPanel = new DockPanel();
            DockPanel.SetDock(WTPanel, Dock.Top);
            WPanel.Children.Add(WTPanel);
            WTPanel.LastChildFill = false;
            WTPanel.Margin = new Thickness(5, 20, 5, 10);
            WTPanel.Background = Brushes.Transparent;

            Label labelW = new();
            DockPanel.SetDock(labelW, Dock.Left);
            WTPanel.Children.Add(labelW);

            try { labelW.Content = "Non-GES Tools for this Workshop (" + WSTools.Count + ")"; }
            catch { PixelWPF.LibraryPixel.Notification("Tools Crash 10", ""); }
            

            Button button = new();
            button.Content = "Manage special workshop tools";
            DockPanel.SetDock(button, Dock.Right);
            WTPanel.Children.Add(button);
            button.Click += (sender, e) => OpenThing();

            void OpenThing()
            {
                WToolsManager WToolManagerControl = new(this);
                TheToolsWindowGrid.Children.Add(WToolManagerControl);
                WToolManagerControl.warningmessage();
            }

            try { SetupTools(WSTools, false); }
            catch { PixelWPF.LibraryPixel.Notification("Tools Crash 8", ""); }
            
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
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    
                };
                DockPanel.SetDock(panelForTab, Dock.Top);
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
                

        public void SetupTools(List<Tool> ToolsList, bool ForGESTools)
        {
            foreach (Tool ThisTool in ToolsList)
            {
                Border border = new();
                border.BorderBrush = Brushes.Black; //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#101010"))
                border.Margin = new Thickness(10, 8, 10, 0);
                border.Height = 36;
                border.MinWidth = 700;
                border.Width = double.NaN; // Auto width to fill available space
                DockPanel.SetDock(border, Dock.Top);

                DockPanel ToolPanel = new DockPanel();
                border.Child = ToolPanel;
                
                
                ToolPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                ToolPanel.MouseEnter += (sender, e) => ToolPanel_MouseEnter(sender, e, ThisTool);

                // Find corresponding TreeViewItem by Tab value and add MainPanel to its DockPanel
                if (ForGESTools == true) 
                {
                    foreach (TreeViewItem treeItem in TreeViewTools.Items)
                    {
                        if (treeItem.Header.ToString() == ThisTool.Category)
                        {
                            DockPanel CategoryPanel = treeItem.Tag as DockPanel;
                            CategoryPanel.Children.Add(border);
                            break;
                        }
                    }
                }
                if (ForGESTools == false)
                {
                    WPanel.Children.Add(border);
                }
                

                SetupToolPanel(ToolPanel, ThisTool, ForGESTools);
            }
        }

        private void SetupToolPanel(DockPanel toolPanel, Tool tool, bool ForGESTools)
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
                FontSize = 20,
                
            };
            toolPanel.Children.Add(browseButton);
            if (ForGESTools == true) { browseButton.Click += (sender, e) => SaveToolsXML(sender, e, tool,  ForGESTools); }
            if (ForGESTools == false) { browseButton.Click += (sender, e) => SaveToolsXML(sender, e, tool,  ForGESTools); }
            


            // Location TextBox
            TextBox locationTextBox = new TextBox
            {
                MinWidth = 50,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF191919")),
                FontSize = 20,
                VerticalContentAlignment = VerticalAlignment.Center,
                
            };
            ToolTipService.SetInitialShowDelay(locationTextBox, LibraryGES.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(locationTextBox, LibraryGES.TooltipBetweenDelay);
            Binding locationBinding = new Binding("Location")
            {
                Source = tool,
                Mode = BindingMode.TwoWay
            };
            locationTextBox.SetBinding(TextBox.TextProperty, locationBinding);
            locationTextBox.SetBinding(TextBox.ToolTipProperty, locationBinding);
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
        

        public void SaveToolsXML(object sender, RoutedEventArgs e, Tool Tool,  bool ForGESTools)
        {
            if (!Directory.Exists(LibraryGES.ApplicationLocation + "\\Settings")) 
            {
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Settings");
            }            

            VistaOpenFileDialog FileSelect = new VistaOpenFileDialog();
            FileSelect.Title = "Select the Exe:" + Tool.ExeName;
            if ((bool)FileSelect.ShowDialog(this))
            {
                if (Path.GetFileName(FileSelect.FileName)  != Tool.ExeName) 
                {
                    PixelWPF.LibraryPixel.NotificationUnknown("Maybe Wrong?","Expected name:\n" + Tool.ExeName + "\n\nYou Selected:\n" + Path.GetFileName(FileSelect.FileName) + "\n\nThe exe will be set anyway (for edge cases where you might be using a custom build of a tool). \n\nJust beware problems can happen when using events with a wrong exe.");
                    
                }


                Button clickedButton = (Button)sender;
                TextBox associatedTextBox = (TextBox)clickedButton.Tag;
                associatedTextBox.Text = FileSelect.FileName;
                Tool.Location = FileSelect.FileName;
                associatedTextBox.ToolTip = Tool.Location;
            }

            if (ForGESTools == false) 
            {
                if (!Database.MasterWorkshopTools.Any(t => t.Key == Tool.Key))
                {
                    Database.MasterWorkshopTools.Add(Tool);
                }
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
                foreach (Tool tool in Database.MasterWorkshopTools)
                {
                    writer.WriteStartElement("WorkshopTool");
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
            if (File.Exists(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Common Events.xml")) 
            {
                File.Delete(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Common Events.xml");
            }

            bool HasAnyCommonEvents = false;
            foreach (CommonEvent Event in Database.CommonEvents)
            {
                if (Event.Workshop == true)
                {
                    HasAnyCommonEvents = true;
                }
            }
            if (HasAnyCommonEvents == false) { return; }


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
                if (activeTreeView != WorkshopTree) { ClearTreeViewSelection(WorkshopTree); }
                if (activeTreeView != TreeViewTools) { ClearTreeViewSelection(TreeViewTools); }
                if (activeTreeView != TreeViewCommonEvents) { ClearTreeViewSelection(TreeViewCommonEvents); }

                foreach (DockPanel panel in TheScrollPanel.Children.OfType<DockPanel>())
                {
                    panel.Visibility = Visibility.Collapsed;
                }

                DockPanel TreeItemPanel = selectedItem.Tag as DockPanel;
                if (TreeItemPanel != null){ TreeItemPanel.Visibility = Visibility.Visible; }

            }
        }

        private void ClearTreeViewSelection(TreeView treeView)
        {
            foreach (TreeViewItem item in treeView.Items)
            {
                item.IsSelected = false;
            }
        }

        
    }



}
