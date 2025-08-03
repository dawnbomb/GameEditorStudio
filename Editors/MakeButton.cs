using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace GameEditorStudio
{
    internal class MakeButton
    {



        public async Task CreateButton(Workshop TheWorkshop, WorkshopData WorkshopData, Editor EditorClass) //This is the editor button at the top
        {            

            EditorClass.EditorImage = new();

            EditorClass.EditorImage.Visibility = Visibility.Collapsed;

            //if (EditorClass.EditorIcon != null)
            //{
            //    string[] files = Directory.GetFiles(LibraryMan.ApplicationLocation + "\\Other\\Icons\\", EditorClass.EditorIcon, SearchOption.AllDirectories);
            //    string GraphicLocation = files[0];
            //    EditorClass.EditorImage.Source = new BitmapImage(new Uri(GraphicLocation));
            //}
            //else
            //{
            //    //Use this dummy question mark, without actually setting it to be it.
            //    EditorClass.EditorImage.Source = new BitmapImage(new Uri(string.Format(LibraryMan.ApplicationLocation + "\\Other\\Icons\\Question Mark.png")));

            //}

            

                        

            Button EditorTabButton = new();
            EditorClass.EditorButton = EditorTabButton;
            EditorTabButton.Margin = new Thickness(0, 0, 4, 0);
            EditorTabButton.AllowDrop = true;
            EditorTabButton.Tag = EditorClass;
            DockPanel.SetDock(EditorTabButton, Dock.Left);
            TheWorkshop.EditorBar.Children.Add(EditorTabButton);

            Point _dragStart;     
            EditorTabButton.PreviewMouseLeftButtonDown += (sender, e) =>  // Capture the drag start position early
            {
                _dragStart = e.GetPosition(null);
            };
            
            EditorTabButton.PreviewMouseMove += (sender, e) =>  // Detect drag on PreviewMouseMove, not MouseMove
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point current = e.GetPosition(null);
                    Vector delta = current - _dragStart;

                    if (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        var data = new DataObject("MoveEditor", EditorTabButton);
                        DragDrop.DoDragDrop(EditorTabButton, data, DragDropEffects.Move);
                    }
                }
            };

            EditorTabButton.Drop += (sender, e) =>
            {
                if (e.Data.GetDataPresent("MoveEditor"))
                {
                    Button DropInput = (Button)e.Data.GetData("MoveEditor");
                    Button target = (Button)sender;

                    if (DropInput == target) return;

                    var panel = TheWorkshop.EditorBar;
                    panel.Children.Remove(DropInput);
                    int index = panel.Children.IndexOf(target) + 1;
                    panel.Children.Insert(index, DropInput);

                    {   //When i gave editor buttons a new look, somehow i broke this and i have no idea how. I just asked GPT to gimme something that works. I can remake this for readability later. T.T
                        var editors = WorkshopData.GameEditors;
                        var entries = editors.ToList();
                        string draggedKey = entries.First(kv => kv.Value == (Editor)DropInput.Tag).Key;
                        string targetKey = entries.First(kv => kv.Value == (Editor)target.Tag).Key;
                        var draggedEntry = entries.First(kv => kv.Key == draggedKey);
                        entries.Remove(draggedEntry);
                        int targetIndex = entries.FindIndex(kv => kv.Key == targetKey);
                        entries.Insert(targetIndex + 1, draggedEntry);
                        editors.Clear();
                        foreach (var kv in entries)
                        {
                            editors.Add(kv.Key, kv.Value);
                        }
                    }
                    
                }
            };
                        




            Label TheEditorNameLabel = new();
            EditorClass.EditorLabel = TheEditorNameLabel;
            TheEditorNameLabel.VerticalAlignment = VerticalAlignment.Bottom;
            TheEditorNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            TheEditorNameLabel.Content = EditorClass.EditorName;
            EditorClass.EditorNameLabel = TheEditorNameLabel;
            TheEditorNameLabel.Margin = new Thickness(3, 0, 3, 0);


            EditorTabButton.Click += (sender, e) =>
            {
                Editor TheEditorClass = EditorClass;

                if (TheEditorClass == null)
                {
                    return;
                }

                {   //Set tab colors.                    
                    foreach (Editor editor in WorkshopData.GameEditors.Values)
                    {
                        editor.EditorButton.Style = (Style)Application.Current.FindResource("ButtonEditorTab");

                    }
                    TheWorkshop.ButtonHome.Style = (Style)Application.Current.FindResource("ButtonEditorTab");
                    EditorTabButton.Style = (Style)Application.Current.FindResource("ButtonCurrentEditorTab");

                }


                TheWorkshop.HIDEALL();
                TheEditorClass.EditorBackPanel.Visibility = Visibility.Visible;

                LibraryMan.GotoRightBarGeneralTab(TheWorkshop);

                TheWorkshop.PropertiesTextboxEditorName.Text = TheEditorNameLabel.Content.ToString();

                if (EditorClass.EditorType == "DataTable")
                {
                    if (TheWorkshop.IsPreviewMode == false)
                    {
                        TheWorkshop.DataTableFileBox.Text = TheEditorClass.StandardEditorData.FileDataTable.FileName;
                    }
                    else
                    {
                        TheWorkshop.DataTableFileBox.Text = "PREVIEW MODE";
                    }

                    TheWorkshop.PropertiesEditorTableStart.Text = TheEditorClass.StandardEditorData.DataTableStart.ToString();
                    TheWorkshop.PropertiesEditorTableWidth.Text = TheEditorClass.StandardEditorData.DataTableRowSize.ToString();

                    TheWorkshop.PropertiesEditorNameTableTextSize.Text = TheEditorClass.StandardEditorData.NameTableTextSize.ToString();
                    TheWorkshop.PropertiesEditorNameTableStartByte.Text = TheEditorClass.StandardEditorData.NameTableStart.ToString();
                    TheWorkshop.PropertiesEditorNameTableRowSize.Text = TheEditorClass.StandardEditorData.NameTableRowSize.ToString();
                    TheWorkshop.PropertiesEditorNameCount.Text = (TheEditorClass.StandardEditorData.NameTableItemCount - 1).ToString();

                    if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.DataFile)
                    {
                        TheWorkshop.LabelCharacterSet.Visibility = Visibility.Visible;
                        TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.Visibility = Visibility.Visible;
                        TheWorkshop.LabelRowSize.Visibility = Visibility.Visible;
                        TheWorkshop.PropertiesEditorNameTableRowSize.Visibility = Visibility.Visible;
                        TheWorkshop.LabelTextSize.Visibility = Visibility.Visible;
                        TheWorkshop.PropertiesEditorNameTableTextSize.Visibility = Visibility.Visible;

                        TheWorkshop.LabelStart.Visibility = Visibility.Visible;
                        TheWorkshop.PropertiesEditorNameTableStartByte.Visibility = Visibility.Visible;
                        TheWorkshop.LabelStart.Content = "Start Byte";
                    }
                    if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.TextFile)
                    {
                        TheWorkshop.LabelCharacterSet.Visibility = Visibility.Collapsed;
                        TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.Visibility = Visibility.Collapsed;
                        TheWorkshop.LabelRowSize.Visibility = Visibility.Collapsed;
                        TheWorkshop.PropertiesEditorNameTableRowSize.Visibility = Visibility.Collapsed;
                        TheWorkshop.LabelTextSize.Visibility = Visibility.Collapsed;
                        TheWorkshop.PropertiesEditorNameTableTextSize.Visibility = Visibility.Collapsed;

                        TheWorkshop.LabelStart.Visibility = Visibility.Visible;
                        TheWorkshop.PropertiesEditorNameTableStartByte.Visibility = Visibility.Visible;
                        TheWorkshop.LabelStart.Content = "First Line";
                    }
                    if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.Nothing)
                    {
                        TheWorkshop.NameTableFileBox.Text = "Fake Name List";

                        TheWorkshop.LabelCharacterSet.Visibility = Visibility.Collapsed;
                        TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.Visibility = Visibility.Collapsed;
                        TheWorkshop.LabelRowSize.Visibility = Visibility.Collapsed;
                        TheWorkshop.PropertiesEditorNameTableRowSize.Visibility = Visibility.Collapsed;
                        TheWorkshop.LabelTextSize.Visibility = Visibility.Collapsed;
                        TheWorkshop.PropertiesEditorNameTableTextSize.Visibility = Visibility.Collapsed;

                        TheWorkshop.LabelStart.Visibility = Visibility.Collapsed;
                        TheWorkshop.PropertiesEditorNameTableStartByte.Visibility = Visibility.Collapsed;


                    }

                    foreach (var item in TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.Items)
                    {

                        if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == TheEditorClass.StandardEditorData.NameTableCharacterSet)
                        {
                            TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.SelectedItem = comboBoxItem;
                            break;
                        }
                    }

                    if (TheWorkshop.IsPreviewMode == false)
                    {
                        if (TheWorkshop.ProjectDataItem.ProjectInputDirectory != "")
                        {
                            TheWorkshop.PropertiesEditorReadGameDataFrom.Text = TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\" + TheEditorClass.StandardEditorData.FileDataTable.FileLocation;
                        }
                        else
                        {
                            TheWorkshop.PropertiesEditorReadGameDataFrom.Text = "You didn't set a project input folder, or it nolonger exists. :(";
                        }

                        if (TheWorkshop.ProjectDataItem.ProjectOutputDirectory != "")
                        {
                            TheWorkshop.EditorOutputLocationTextbox.Text = TheWorkshop.ProjectDataItem.ProjectOutputDirectory + "\\" + TheEditorClass.StandardEditorData.FileDataTable.FileLocation;
                        }
                        else
                        {
                            TheWorkshop.EditorOutputLocationTextbox.Text = "You didn't set a project output folder, or it nolonger exists. :(";
                        }
                    }
                    else
                    {
                        TheWorkshop.PropertiesEditorReadGameDataFrom.Text = "Your in preview mode!";
                        TheWorkshop.EditorOutputLocationTextbox.Text = "Your in preview mode!";
                    }





                    if (EditorClass.StandardEditorData.FileNameTable != null)
                    {
                        TheWorkshop.PropertiesEditorNameTableTextSize.IsEnabled = true;
                        TheWorkshop.PropertiesEditorNameTableStartByte.IsEnabled = true;
                        TheWorkshop.PropertiesEditorNameTableRowSize.IsEnabled = true;
                        TheWorkshop.PropertiesEditorNameCount.IsEnabled = true;
                        TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = true;
                        TheWorkshop.NameTableFileBox.Text = TheEditorClass.StandardEditorData.FileNameTable.FileName;

                    }
                    else
                    {
                        TheWorkshop.PropertiesEditorNameTableTextSize.IsEnabled = false;
                        TheWorkshop.PropertiesEditorNameTableStartByte.IsEnabled = false;
                        TheWorkshop.PropertiesEditorNameTableRowSize.IsEnabled = false;
                        TheWorkshop.PropertiesEditorNameCount.IsEnabled = false;
                        TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = false;
                    }

                    TheWorkshop.EntryNoteTextbox.Text = TheEditorClass.StandardEditorData.SelectedEntry.WorkshopTooltip;



                    //var UC = TheEditorClass.SWData.EditorLeftDockPanel.UserControl as TheLeftBar;
                    //UC.LabelCharsMax.Content = TheEditorClass.SWData.NameTableTextSize.ToString();
                }


                TheWorkshop.EditorClass = TheEditorClass; //this used to be EditorClass = TheEditorClass; and i changed it because i might have meant this, but also maybe not and i made a new bug?
                TheWorkshop.EntryClass = TheEditorClass.StandardEditorData.SelectedEntry; //This is the entry that is currently selected in the editor.
                TheWorkshop.ColumnClass = TheWorkshop.EntryClass.EntryColumn;
                TheWorkshop.CategoryClass = TheWorkshop.ColumnClass.ColumnRow; //This is the category that is currently selected in the editor.
                if (TheWorkshop.EntryClass.EntryGroup != null) 
                {
                    TheWorkshop.GroupClass = TheWorkshop.EntryClass.EntryGroup; //This is the group that is currently selected in the editor.
                }
                



            };

            ContextMenu contextMenu = new ContextMenu();
            EditorTabButton.ContextMenu = contextMenu;

            

            if (EditorClass.EditorType == "DataTable") 
            {
                MenuItem OpenEditor = new MenuItem();
                OpenEditor.Header = "Open Editor Folder";
                OpenEditor.Click += OpenEditorFolder_Click; // Event handler for click action
                contextMenu.Items.Add(OpenEditor);
                void OpenEditorFolder_Click(object sender, RoutedEventArgs e)
                {
                    string EditorFolderPath = Path.Combine(LibraryMan.ApplicationLocation, "Workshops", TheWorkshop.WorkshopName, "Editors", EditorClass.EditorName);
                    LibraryMan.OpenFolder(EditorFolderPath);
                }

                MenuItem OpenNTFile = new MenuItem();
                OpenNTFile.Header = "Open Name Table File Folder";
                OpenNTFile.Click += OpenNTFile_Click; // Event handler for click action
                contextMenu.Items.Add(OpenNTFile);
                void OpenNTFile_Click(object sender, RoutedEventArgs e)
                {
                    LibraryMan.OpenFileFolder(TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\" + EditorClass.StandardEditorData.FileNameTable.FileLocation);
                }
                if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.Nothing) { OpenNTFile.IsEnabled = false; }

                MenuItem OpenDTFile = new MenuItem();
                OpenDTFile.Header = "Open Data Table File Folder";
                OpenDTFile.Click += OpenDTFile_Click; // Event handler for click action
                contextMenu.Items.Add(OpenDTFile);
                void OpenDTFile_Click(object sender, RoutedEventArgs e)
                {                    
                    LibraryMan.OpenFileFolder(TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\"+ EditorClass.StandardEditorData.FileDataTable.FileLocation);
                }


                MenuItem OpenDescriptionFolder = new MenuItem();
                OpenDescriptionFolder.Header = "Open Description Table File Folder";
                contextMenu.Items.Add(OpenDescriptionFolder);
                OpenDescriptionFolder.IsEnabled = false;
                try 
                {
                    if (EditorClass.StandardEditorData.DescriptionTableList != null)
                    {
                        if (EditorClass.StandardEditorData.DescriptionTableList[0] != null)
                        {
                            if (EditorClass.StandardEditorData.DescriptionTableList[0].FileTextTable.FileLocation != null)
                            {
                                if (EditorClass.StandardEditorData.DescriptionTableList[0].FileTextTable.FileLocation != "")
                                {
                                    
                                    OpenDescriptionFolder.Click += OpenDTFolder_Click; // Event handler for click action                                   
                                    void OpenDTFolder_Click(object sender, RoutedEventArgs e)
                                    {
                                        LibraryMan.OpenFileFolder(TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\" + EditorClass.StandardEditorData.DescriptionTableList[0].FileTextTable.FileLocation);
                                    }
                                    OpenDescriptionFolder.IsEnabled = true;
                                }

                            }
                        }
                    }
                } 
                catch 
                {
                
                }
                

                
                

                //if (EditorClass.SWData.ExtraTableList[0] != null && EditorClass.SWData.ExtraTableList[0].FileTextTable.FileLocation != null && EditorClass.SWData.ExtraTableList[0].FileTextTable.FileLocation != "") 
                //{
                //    MenuItem OpenDescriptionFolder = new MenuItem();
                //    OpenDescriptionFolder.Header = "Open Description Table File Location";
                //    OpenDescriptionFolder.Click += OpenDTFolder_Click; // Event handler for click action
                //    contextMenu.Items.Add(OpenDescriptionFolder);
                //    void OpenDTFolder_Click(object sender, RoutedEventArgs e)
                //    {
                //        LibraryMan.OpenFileFolder(TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\" + EditorClass.SWData.ExtraTableList[0].FileTextTable.FileLocation);
                //    }
                //}



            }

            MenuItem DeleteEditorItem = new MenuItem();
            DeleteEditorItem.Header = "Delete Editor";
            DeleteEditorItem.Click += DeleteEditorClick; // Event handler for click action
            contextMenu.Items.Add(DeleteEditorItem);

            void DeleteEditorClick(object sender, RoutedEventArgs e)
            {
                foreach (KeyValuePair<string, Editor> editor in WorkshopData.GameEditors)
                {
                    if (editor.Value == EditorClass)
                    {
                        System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("Are you sure you want to delete this editor?", "Delete Editor", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.YesNo);

                        if (dr == System.Windows.Forms.DialogResult.Yes)
                        {
                            try
                            {
                                TheWorkshop.EditorBar.Children.Remove(EditorClass.EditorBarDockPanel);

                                //EditorsTree.Items.Remove(editor.Value.EditorTreeViewitem); //editor.Key

                                WorkshopData.GameEditors.Remove(editor.Key);
                                TheWorkshop.MidGrid.Children.Remove(editor.Value.EditorBackPanel);
                                TheWorkshop.EditorBar.Children.Remove(EditorTabButton);
                                LibraryMan.GotoGeneralHide(TheWorkshop);


                                //also hide the details panel for entrys / column / row etc?

                                //Directory.Delete(ExePath + "\\Workshops\\" + WorkshopName + "\\Editors\\" + editor.Key, true);
                                //Returns the view to home should be here.



                            }
                            catch (IOException ex)
                            {
                                // File is being used, so it cannot be deleted
                                Console.WriteLine("File is being used: " + ex.Message);
                            }
                        }

                    }
                }
            }





            EditorTabButton.Content = TheEditorNameLabel;
            //DockPanel.SetDock(TheEditorNameLabel, Dock.Bottom);
            //EditorDock2.Children.Add(TheEditorNameLabel);






            //DockPanel.SetDock(EditorBorder, Dock.Left);
            //TheWorkshop.EditorBar.Children.Add(EditorBorder);
            //EditorBorder.Child = EditorDock;





            //DockPanel.SetDock(EditorDock, Dock.Left);
            //TheWorkshop.EditorBar.Children.Add(EditorDock);


            ////EditorBorder.BorderThickness = new Thickness(2);
            ////EditorDock.Children.Add(EditorBorder);

            ////EditorBorder.Child = EditorDock2;
            ////EditorDock2.Background = (Brush)(new BrushConverter().ConvertFrom("#191919"));

            //DockPanel.SetDock(TheEditorNameLabel, Dock.Bottom);
            //EditorDock2.Children.Add(TheEditorNameLabel);
            //DockPanel.SetDock(EditorClass.EditorImage, Dock.Top);
            //EditorDock2.Children.Add(EditorClass.EditorImage);

            //EditorClass.EditorDock2 = EditorDock2;

            //EditorBorder.Style = (Style)Application.Current.Resources["EditorButtonBorder"];

            //TheEditorNameLabel.Visibility = Visibility.Collapsed;

            //Make sure its actually rendering all 60x60
            //Maybe expand the size of the entire dock panel 
            //collapse text to get better idea of true size
            //Maybe temp expand dock panel vertical size A TON to get the true size for a 60 pixel
            //How many editors can a true 60 pixel hold?
            //at 50 / old method it was 19

            //Characters
            //Enemys
            //Skills/Spells/Arts
            //Items
            //Weapons (Sword, Axe, Lance, Bow, Tome, Staff, Scythe, Gun)
            //Armor
            //Accessories
            //Class or Job (Progression)
            //Encounter Table
            //Unique 1 (Summons?)
            //Unique 2
            //Unique 3 (#12)

            //Social Links
            //Creatures/Mechs
            //Food / Cooking
            //Resource Farms
            //Crafting


            //Passives
            //Shops 
            //Progression (Class or Job)
            //Quests
            //Chests
            //Guild / Clan
            //Fishing Minigame

            await Task.Delay(1); //This makes the buttons actually load so that the size update can properly run (it uses the currently rendered size of the dockpanel / image)

            TheWorkshop.UpdateEditorButton(EditorClass);


        }
    }
}
