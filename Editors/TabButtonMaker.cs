using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfHexEditor;

namespace GameEditorStudio
{
    internal class TabButtonMaker
    {



        public async Task CreateEditorTab(Editor EditorClass) //This is the editor button at the top
        {
            WorkshopData WorkshopData = EditorClass.WorkshopData;
            Workshop TheWorkshop = WorkshopData.WorkshopXaml;

            EditorClass.EditorTabImage = new();
            EditorClass.EditorTabImage.Visibility = Visibility.Collapsed;

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
            EditorClass.EditorTab = EditorTabButton;
            EditorTabButton.Margin = new Thickness(0, 0, 4, 0);
            EditorTabButton.AllowDrop = true;
            EditorTabButton.Tag = EditorClass;
            DockPanel.SetDock(EditorTabButton, Dock.Left);
            TheWorkshop.EditorBar.Children.Add(EditorTabButton);


            Label TheEditorNameLabel = new();
            EditorClass.EditorTabNameLabel = TheEditorNameLabel;
            TheEditorNameLabel.VerticalAlignment = VerticalAlignment.Bottom;
            TheEditorNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            TheEditorNameLabel.Content = EditorClass.EditorName;
            TheEditorNameLabel.Margin = new Thickness(3, 0, 3, 0);

            EditorTabButton.Content = TheEditorNameLabel;


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

            EditorTabButton.Drop += (sender, e) => //I had Gemeni recode this as part of a code refactor from dictionarys to lists. 
            {
                if (e.Data.GetDataPresent("MoveEditor"))
                {
                    Button draggedButton = (Button)e.Data.GetData("MoveEditor");
                    Button targetButton = (Button)sender;

                    if (draggedButton == targetButton) return;

                    // 1. Update the UI (The Visual Buttons in the bar)
                    var panel = TheWorkshop.EditorBar;
                    panel.Children.Remove(draggedButton);
                    int uiIndex = panel.Children.IndexOf(targetButton) + 1;
                    panel.Children.Insert(uiIndex, draggedButton);

                    // 2. Update the Data (The actual List<Editor>)
                    // We get the Editor objects from the Button Tags
                    Editor draggedEditor = (Editor)draggedButton.Tag;
                    Editor targetEditor = (Editor)targetButton.Tag;

                    var editors = WorkshopData.GameEditors;

                    // Remove the dragged editor from its current spot
                    if (editors.Remove(draggedEditor))
                    {
                        // Find where the target editor is now
                        int targetIndex = editors.IndexOf(targetEditor);

                        // Insert the dragged editor right after the target
                        editors.Insert(targetIndex + 1, draggedEditor);
                    }
                }
            };





            


            EditorTabButton.Click += (sender, e) =>
            {
                Editor TheEditorClass = EditorClass;
                

                if (TheEditorClass == null)
                {
                    return;
                }

                {   //Set tab colors.                    
                    foreach (Editor editor in WorkshopData.GameEditors)
                    {
                        editor.EditorTab.Style = (Style)Application.Current.FindResource("ButtonEditorTab");

                    }
                    TheWorkshop.ButtonHome.Style = (Style)Application.Current.FindResource("ButtonEditorTab");
                    EditorTabButton.Style = (Style)Application.Current.FindResource("ButtonCurrentEditorTab");

                }


                TheWorkshop.HIDEALL();
                TheEditorClass.EditorVisual.Visibility = Visibility.Visible;
                                
                

                

                if (EditorClass is DataTableEditorData DTEData)
                {
                    DTRightBar RightBar = DTEData.EditorRightBar;

                    

                    RightBar.PropertiesTextboxEditorName.Text = EditorClass.EditorName;

                    DTEData.EditorRightBar.DocumentsControl.TabClicked();
                    RightBar.EditorTabItem.IsSelected = true;

                    //Data Table Stuff.
                    if (DTEData.DataTable == null) 
                    {
                        //Disable right bar data table textboxes (because there is no data table)
                        RightBar.PropertiesEditorTableStart.IsEnabled = false;
                        RightBar.OpenInputLocationButton.IsEnabled = false;
                        RightBar.OpenOutputLocationButton.IsEnabled = false;
                        RightBar.BtnDataTblOutputOpenHxD.IsEnabled = false;
                        RightBar.PropertiesEditorReadGameDataFrom.Text = "No Data Table Set";
                        RightBar.EditorOutputLocationTextbox.Text = "No Data Table Set";
                        RightBar.BtnDataTblOutputOpen010.IsEnabled = false;
                    }
                    if (DTEData.DataTable != null) 
                    {
                        //Enable right bar data table textboxes (because there is no data table)
                        RightBar.PropertiesEditorTableStart.IsEnabled = true;
                        RightBar.OpenInputLocationButton.IsEnabled = true;
                        RightBar.OpenOutputLocationButton.IsEnabled = true;
                        RightBar.BtnDataTblOutputOpenHxD.IsEnabled = true;
                        RightBar.BtnDataTblOutputOpen010.IsEnabled = true;


                        if (TheWorkshop.IsPreviewMode == false)
                        {
                            if (TheWorkshop.WorkshopData.LoadedProject.ProjectInputDirectory != "")
                            {
                                RightBar.PropertiesEditorReadGameDataFrom.Text = TheWorkshop.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation;
                            }
                            else
                            {
                                RightBar.PropertiesEditorReadGameDataFrom.Text = "You didn't set a project input folder, or it nolonger exists. :(";
                            }

                            if (TheWorkshop.WorkshopData.LoadedProject.ProjectOutputDirectory != "")
                            {
                                RightBar.EditorOutputLocationTextbox.Text = TheWorkshop.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation;
                            }
                            else
                            {
                                RightBar.EditorOutputLocationTextbox.Text = "You didn't set a project output folder, or it nolonger exists. :(";
                            }
                        }
                        else
                        {
                            RightBar.PropertiesEditorReadGameDataFrom.Text = "Your in preview mode!";
                            RightBar.EditorOutputLocationTextbox.Text = "Your in preview mode!";
                        }

                        //Seperator////////////

                        if (TheWorkshop.IsPreviewMode == false)
                        {
                            RightBar.DataTableFileBox.Text = DTEData.DataTable.FileDataTable.FileName;
                        }
                        else
                        {
                            RightBar.DataTableFileBox.Text = "PREVIEW MODE";
                        }

                        RightBar.PropertiesEditorTableStart.Text = DTEData.DataTable.DataTableStart.ToString();
                        RightBar.PropertiesEditorTableWidth.Text = DTEData.DataTable.DataTableRowSize.ToString();

                        RightBar.EntryNoteTextbox.Text = DTEData.EntryClass.WorkshopTooltip;

                        RightBar.DTEData.EntryClass = DTEData.EntryClass; //This is the entry that is currently selected in the editor.
                        RightBar.DTEData.CategoryClass = RightBar.DTEData.EntryClass.ParentCategory; //This is the category that is currently selected in the editor.
                        if (RightBar.DTEData.EntryClass.ParentGroup != null)
                        {
                            RightBar.DTEData.GroupClass = RightBar.DTEData.EntryClass.ParentGroup; //Set the Selected Group.
                        }
                    }

                    //Name Table Stuff.
                    if (DTEData.NameTable == null) 
                    {
                        //Disable right bar name textboxes (because there is no name table)
                        RightBar.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = false;
                        RightBar.PropertiesEditorNameTableStartByte.IsEnabled = false;
                        RightBar.PropertiesEditorNameTableRowSize.IsEnabled = false;
                        RightBar.PropertiesEditorNameTableTextSize.IsEnabled = false;
                        RightBar.PropertiesEditorFirstNameNumber.IsEnabled = false;
                        RightBar.PropertiesEditorNameCount.IsEnabled = false;
                        RightBar.BtnNameTblOutputOpenHxD.IsEnabled = false;
                        RightBar.BtnNameTblOutputOpen010.IsEnabled = false;
                    }                        
                    if (DTEData.NameTable != null) 
                    {
                        //Enable right bar name textboxes (because there is a name table)
                        RightBar.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = true;
                        RightBar.PropertiesEditorNameTableStartByte.IsEnabled = true;
                        RightBar.PropertiesEditorNameTableRowSize.IsEnabled = true;
                        RightBar.PropertiesEditorNameTableTextSize.IsEnabled = true;
                        RightBar.PropertiesEditorFirstNameNumber.IsEnabled = true;
                        RightBar.PropertiesEditorNameCount.IsEnabled = true;
                        RightBar.BtnNameTblOutputOpenHxD.IsEnabled = true;
                        RightBar.BtnNameTblOutputOpen010.IsEnabled = true;


                        //Set basic info
                        RightBar.PropertiesEditorNameTableTextSize.Text = DTEData.NameTable.TextTableCharLimit.ToString();
                        RightBar.PropertiesEditorNameTableStartByte.Text = DTEData.NameTable.TextTableStart.ToString();
                        RightBar.PropertiesEditorNameTableRowSize.Text = DTEData.NameTable.TextTableRowSize.ToString();
                        RightBar.PropertiesEditorNameCount.Text = (DTEData.NameTable.TextTableItemCount).ToString();
                        RightBar.PropertiesEditorFirstNameNumber.Text = DTEData.NameTable.TextTableFirstNameID.ToString();

                        if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                        {
                            RightBar.LabelCharacterSet.Visibility = Visibility.Visible;
                            RightBar.PropertiesEditorNameTableCharacterSetDropdown.Visibility = Visibility.Visible;
                            RightBar.LabelRowSize.Visibility = Visibility.Visible;
                            RightBar.PropertiesEditorNameTableRowSize.Visibility = Visibility.Visible;
                            RightBar.LabelTextSize.Visibility = Visibility.Visible;
                            RightBar.PropertiesEditorNameTableTextSize.Visibility = Visibility.Visible;

                            RightBar.LabelStart.Visibility = Visibility.Visible;
                            RightBar.PropertiesEditorNameTableStartByte.Visibility = Visibility.Visible;
                            RightBar.LabelStart.Content = "Start Byte";
                        }
                        if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                        {
                            RightBar.LabelCharacterSet.Visibility = Visibility.Visible;
                            RightBar.PropertiesEditorNameTableCharacterSetDropdown.Visibility = Visibility.Visible;
                            RightBar.LabelRowSize.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableRowSize.Visibility = Visibility.Collapsed;
                            RightBar.LabelTextSize.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableTextSize.Visibility = Visibility.Collapsed;

                            RightBar.LabelStart.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableStartByte.Visibility = Visibility.Collapsed;
                            RightBar.LabelStart.Content = "Start Byte";

                            RightBar.PropertiesEditorNameCount.IsEnabled = false;
                        }
                        if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                        {
                            RightBar.LabelCharacterSet.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableCharacterSetDropdown.Visibility = Visibility.Collapsed;
                            RightBar.LabelRowSize.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableRowSize.Visibility = Visibility.Collapsed;
                            RightBar.LabelTextSize.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableTextSize.Visibility = Visibility.Collapsed;

                            RightBar.LabelStart.Visibility = Visibility.Visible;
                            RightBar.PropertiesEditorNameTableStartByte.Visibility = Visibility.Visible;
                            RightBar.LabelStart.Content = "First Line";
                        }
                        if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing)
                        {
                            RightBar.NameTableFileBox.Text = "Fake Name List";

                            RightBar.LabelCharacterSet.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableCharacterSetDropdown.Visibility = Visibility.Collapsed;
                            RightBar.LabelRowSize.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableRowSize.Visibility = Visibility.Collapsed;
                            RightBar.LabelTextSize.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableTextSize.Visibility = Visibility.Collapsed;

                            RightBar.LabelStart.Visibility = Visibility.Collapsed;
                            RightBar.PropertiesEditorNameTableStartByte.Visibility = Visibility.Collapsed;


                        }

                        foreach (var item in RightBar.PropertiesEditorNameTableCharacterSetDropdown.Items)
                        {

                            if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == DTEData.NameTable.TextTableCharacterSet)
                            {
                                RightBar.PropertiesEditorNameTableCharacterSetDropdown.SelectedItem = comboBoxItem;
                                break;
                            }
                        }



                        if (DTEData.NameTable.TextTableFile != null)
                        {
                            RightBar.PropertiesEditorNameTableTextSize.IsEnabled = true;
                            RightBar.PropertiesEditorNameTableStartByte.IsEnabled = true;
                            RightBar.PropertiesEditorNameTableRowSize.IsEnabled = true;
                            RightBar.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = true;
                            RightBar.NameTableFileBox.Text = DTEData.NameTable.TextTableFile.FileName;

                        }
                        else
                        {
                            RightBar.PropertiesEditorNameTableTextSize.IsEnabled = false;
                            RightBar.PropertiesEditorNameTableStartByte.IsEnabled = false;
                            RightBar.PropertiesEditorNameTableRowSize.IsEnabled = false;
                            RightBar.PropertiesEditorNameCount.IsEnabled = false;
                            RightBar.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = false;
                        }

                    }
                    if (WorkshopData.IsProjectLoaded == false) 
                    {
                        //Name Table
                        RightBar.NameTableManagerButton.IsEnabled = false;
                        RightBar.BtnNameTblOutputOpenHxD.IsEnabled = false;
                        RightBar.BtnNameTblOutputOpen010.IsEnabled = false;
                        RightBar.PropertiesEditorFirstNameNumber.IsEnabled = false;

                        //Data Table
                        RightBar.DataTableManagerButton.IsEnabled = false;
                        RightBar.BtnDataTblOutputOpenHxD.IsEnabled = false;
                        RightBar.BtnDataTblOutputOpen010.IsEnabled = false;
                        RightBar.PropertiesEditorTableStart.IsEnabled = false;
                        RightBar.PropertiesEditorReadGameDataFrom.IsEnabled = false;
                        RightBar.OpenInputLocationButton.IsEnabled = false;
                        RightBar.EditorOutputLocationTextbox.IsEnabled = false;
                        RightBar.OpenOutputLocationButton.IsEnabled = false;

                        //Description Table
                        RightBar.DescriptionTableManagerButton.IsEnabled = false;

                    }
                    if (WorkshopData.IsProjectLoaded == true) 
                    {
                        //Name Table
                        RightBar.NameTableManagerButton.IsEnabled = true;

                        //Data Table
                        RightBar.DataTableManagerButton.IsEnabled = true;
                        RightBar.PropertiesEditorReadGameDataFrom.IsEnabled = true;
                        RightBar.EditorOutputLocationTextbox.IsEnabled = true;

                        //Description Table
                        RightBar.DescriptionTableManagerButton.IsEnabled = true;
                    }
                    

                    


                    
                }


                //TheWorkshop.EditorClass = TheEditorClass; //
                
                
                //2x was moved from here to above.


            };

            



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

            UpdateEditorTab(EditorClass); //Does nothing ATM, was used for old tabs get sprites idea.

            
        }

        public void UpdateEditorTab(Editor AnEditor)
        {
            //NOTE: I did a LOT of research on sprites sizes, and 60 is PERFECT for a editor icon size. 
            //it's very rare for icons to be 70+ that are hard to crop, and everything 30 under can be multiplied in size. 
            //While icons size 42 probably can't be cropped and doubled, it's close enough to be on-style.
            //meanwhile vs 50 max, not only do we get sprites size 50~60+ with cropping, but more small sprites can multiply their size to average out more effectivly. 

            //if (EditorClass.EditorName != "")
            //{
            //    EditorClass.EditorNameLabel.Content = EditorClass.EditorName;
            //}
            //else if(EditorClass.EditorName == "")
            //{
            //    EditorClass.EditorNameLabel.Content = "??? " + En;
            //}


            return;

            System.Windows.Controls.Image image = AnEditor.EditorTabImage;

            BitmapSource bitmap = image.Source as BitmapSource;
            int scale = CalculateIntegerScale(bitmap.PixelWidth, bitmap.PixelHeight, 120, 90);

            int CalculateIntegerScale(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
            {
                int scaleX = maxWidth / originalWidth;
                int scaleY = maxHeight / originalHeight;
                int scale = Math.Min(scaleX, scaleY);
                return Math.Max(1, scale); // Never go below 1x
            }

            {
                image.Source = bitmap;
                image.Width = bitmap.PixelWidth * scale;
                image.Height = bitmap.PixelHeight * scale;
                image.Stretch = Stretch.None;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.VerticalAlignment = VerticalAlignment.Center;
                //image.HorizontalAlignment = HorizontalAlignment.Left;
                //image.VerticalAlignment = VerticalAlignment.Top;
                image.SnapsToDevicePixels = true;
                image.UseLayoutRounding = true;  // https://learn.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.uselayoutrounding?view=windowsdesktop-9.0
                //image.ClipToBounds = true;  //This actually caused problems where the leftmost and topmost pixel lines were being minorly shrunk to like half size (subpixel nonsense)

                //image.MaxWidth = 120;
                //image.MaxHeight = 90;

                //image.MinWidth = 75;
                //image.MaxHeight = 75;    
                //image.MinWidth = 60;
                //image.MaxHeight = 62;                
                //AnEditor.EditorDock2.MinWidth = 85;

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

                image.Margin = new Thickness(3, 3, 3, 0);
                image.Stretch = Stretch.UniformToFill;
            }


            //image.MaxWidth = 0;
            //AnEditor.EditorDock2.MaxWidth = 9999999;
            //AnEditor.EditorDock2.UpdateLayout(); 

            //double DockWidth = AnEditor.EditorDock2.ActualWidth;

            //image.MaxWidth = 100;
            //image.MaxHeight = 100;
            //AnEditor.EditorDock2.MaxWidth = DockWidth;
            //AnEditor.EditorDock2.UpdateLayout();

            //If Dock is wider then 100 AND image width, make the text wrap to line 2. 

            //{
            //    AnEditor.EditorImage.MinWidth = 60;
            //    AnEditor.EditorImage.MaxHeight = 62;
            //    AnEditor.EditorImage.HorizontalAlignment = HorizontalAlignment.Center;
            //    AnEditor.EditorImage.VerticalAlignment = VerticalAlignment.Center;
            //    AnEditor.EditorImage.ClipToBounds = true;
            //    AnEditor.EditorImage.SnapsToDevicePixels = true;
            //    AnEditor.EditorImage.UseLayoutRounding = true; // https://learn.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.uselayoutrounding?view=windowsdesktop-9.0
            //    RenderOptions.SetBitmapScalingMode(AnEditor.EditorImage, BitmapScalingMode.NearestNeighbor);

            //    AnEditor.EditorImage.Margin = new Thickness(0, 0, 0, 0);
            //}

            //AnEditor.EditorImage.Stretch = Stretch.UniformToFill;

            //AnEditor.EditorImage.MaxWidth = 0;
            //AnEditor.EditorDock2.MaxWidth = 9999999;
            //AnEditor.EditorDock2.UpdateLayout();

            //double DockWidth = AnEditor.EditorDock2.ActualWidth;

            //AnEditor.EditorImage.MaxWidth = double.PositiveInfinity;
            //AnEditor.EditorDock2.MaxWidth = DockWidth;
            //AnEditor.EditorDock2.UpdateLayout();

            //if (AnEditor.EditorImage.ActualHeight > 62)
            //{
            //    AnEditor.EditorImage.Stretch = Stretch.Uniform;
            //}



            //AnEditor.EditorImage.StretchDirection = StretchDirection.UpOnly;
        }

        public void UpdateEditorRightClickMenu(Editor EditorClass)
        {
            WorkshopData WorkshopData = EditorClass.WorkshopData;
            Workshop TheWorkshop = WorkshopData.WorkshopXaml;

            ContextMenu NewContextMenu = new ContextMenu();
            EditorClass.EditorTab.ContextMenu = NewContextMenu;

            MenuItem ReloadEditor = new MenuItem(); //Upcoming, but disabled for now. 
            ReloadEditor.Header = "Reload Editor (Not finished)";
            ReloadEditor.Click += ReloadEditor_Click; // Event handler for click action
            //contextMenu.Items.Add(ReloadEditor);
            void ReloadEditor_Click(object sender, RoutedEventArgs e)
            {
                FileLoading fileLoading = new();
                fileLoading.ReloadAllEditorFiles(EditorClass);



                DataTableEditor standardEditor = new(WorkshopData, EditorClass);

                //???
                //Trigger ReloadFile in a FileManager.cs for every File in use by a editor...
                //Trigger ReloadEditor in some kind of EditorManager.cs file?
                //Be mindful about linked menus...
            }
                        

            MenuItem OpenEditor = new MenuItem();
            OpenEditor.Header = "Open Editor Folder";
            OpenEditor.Click += OpenEditorFolder_Click; // Event handler for click action
            NewContextMenu.Items.Add(OpenEditor);
            void OpenEditorFolder_Click(object sender, RoutedEventArgs e)
            {
                string EditorFolderPath = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", TheWorkshop.WorkshopData.WorkshopName, "Editors", EditorClass.EditorName);
                LibraryGES.OpenFolder(EditorFolderPath);
            }

            MenuItem OpenEditorXML = new MenuItem();
            OpenEditorXML.Header = "Open Editor.XML";
            NewContextMenu.Items.Add(OpenEditorXML);
            OpenEditorXML.Click += OpenTheEditorXML; // Event handler for click action
            void OpenTheEditorXML(object sender, RoutedEventArgs e)
            {
                LibraryGES.OpenFile(LibraryGES.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors\\" + EditorClass.EditorName + "\\Editor.xml");

            }

            MenuItem DeleteEditorItem = new MenuItem();
            DeleteEditorItem.Header = "Delete Editor";
            DeleteEditorItem.Click += DeleteEditorClick; // Event handler for click action
            NewContextMenu.Items.Add(DeleteEditorItem);
            void DeleteEditorClick(object sender, RoutedEventArgs e)
            {
                foreach (Editor editor in WorkshopData.GameEditors.ToArray()) //.ToArray() lets me add/remove from a list during a foreach loop.
                {
                    if (editor == EditorClass)
                    {
                        System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("Are you sure you want to delete this editor?", "Delete Editor", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.YesNo);

                        if (dr == System.Windows.Forms.DialogResult.Yes)
                        {
                            try
                            {
                                WorkshopData.GameEditors.Remove(editor);
                                TheWorkshop.MidGrid.Children.Remove(editor.EditorVisual);
                                TheWorkshop.EditorBar.Children.Remove(editor.EditorTab);
                                //LibraryGES.GotoGeneralHide(TheWorkshop);

                                TheWorkshop.ButtonHome.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

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



            if (EditorClass is DataTableEditorData DTEData)
            {
                Separator separator = new();
                separator.SetResourceReference(Separator.StyleProperty, "DashLineForMenu");
                NewContextMenu.Items.Add(separator);

                MenuItem OpenNTFile = new MenuItem();
                OpenNTFile.Header = "Open Name Table File Folder (Input)";
                OpenNTFile.Click += OpenNTFile_Click; // Event handler for click action
                NewContextMenu.Items.Add(OpenNTFile);
                void OpenNTFile_Click(object sender, RoutedEventArgs e)
                {
                    LibraryGES.OpenFileFolder(TheWorkshop.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.NameTable.TextTableFile.FileLocation);
                }
                if (DTEData.NameTable == null) { OpenNTFile.IsEnabled = false; }
                else if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing) { OpenNTFile.IsEnabled = false; }

                MenuItem OpenNTFileOutput = new MenuItem();
                OpenNTFileOutput.Header = "Open Name Table File Folder (Output)";
                OpenNTFileOutput.Click += OpenNTFileOutput_Click; // Event handler for click action
                NewContextMenu.Items.Add(OpenNTFileOutput);
                void OpenNTFileOutput_Click(object sender, RoutedEventArgs e)
                {
                    LibraryGES.OpenFileFolder(TheWorkshop.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.NameTable.TextTableFile.FileLocation);
                }
                if (DTEData.NameTable == null) { OpenNTFileOutput.IsEnabled = false; }
                else if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing) { OpenNTFileOutput.IsEnabled = false; }

                MenuItem OpenDTFile = new MenuItem();
                OpenDTFile.Header = "Open Data Table File Folder (Input)";
                OpenDTFile.Click += OpenDTFile_Click; // Event handler for click action
                NewContextMenu.Items.Add(OpenDTFile);
                void OpenDTFile_Click(object sender, RoutedEventArgs e)
                {
                    LibraryGES.OpenFileFolder(TheWorkshop.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation);
                }
                if (DTEData.DataTable == null) { OpenDTFile.IsEnabled = false; }

                MenuItem OpenDTFileOutput = new MenuItem();
                OpenDTFileOutput.Header = "Open Data Table File Folder (Output)";
                OpenDTFileOutput.Click += OpenDTFileOutput_Click; // Event handler for click action
                NewContextMenu.Items.Add(OpenDTFileOutput);
                void OpenDTFileOutput_Click(object sender, RoutedEventArgs e)
                {
                    LibraryGES.OpenFileFolder(TheWorkshop.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation);
                }
                if (DTEData.DataTable == null) { OpenDTFileOutput.IsEnabled = false; }

                MenuItem OpenDescriptionFolder = new MenuItem();
                OpenDescriptionFolder.Header = "Open Description Table File Folder (Input)";
                OpenDescriptionFolder.Click += OpenDTFolder_Click; // Event handler for click action  
                NewContextMenu.Items.Add(OpenDescriptionFolder);                
                void OpenDTFolder_Click(object sender, RoutedEventArgs e)
                {
                    LibraryGES.OpenFileFolder(TheWorkshop.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.DescriptionTableList[0].TextTableFile.FileLocation);
                }
                OpenDescriptionFolder.IsEnabled = false;
                try
                {
                    if (DTEData.DescriptionTableList != null)
                    {
                        if (DTEData.DescriptionTableList[0] != null)
                        {
                            if (DTEData.DescriptionTableList[0].TextTableFile != null)
                            {
                                if (DTEData.DescriptionTableList[0].TextTableFile.FileLocation != null)
                                {
                                    if (DTEData.DescriptionTableList[0].TextTableFile.FileLocation != "")
                                    {
                                        OpenDescriptionFolder.IsEnabled = true;
                                    }
                                }
                            }                            
                        }
                    }
                }
                catch
                {
                    OpenDescriptionFolder.IsEnabled = false;
                }


                MenuItem OpenDescriptionFolderOutput = new MenuItem();
                OpenDescriptionFolderOutput.Header = "Open Description Table File Folder (Output)";
                OpenDescriptionFolderOutput.Click += OpenDTFolderOutput_Click; // Event handler for click action   
                NewContextMenu.Items.Add(OpenDescriptionFolderOutput);                
                void OpenDTFolderOutput_Click(object sender, RoutedEventArgs e)
                {
                    LibraryGES.OpenFileFolder(TheWorkshop.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.DescriptionTableList[0].TextTableFile.FileLocation);
                }
                OpenDescriptionFolderOutput.IsEnabled = false;
                try
                {
                    if (DTEData.DescriptionTableList != null)
                    {
                        if (DTEData.DescriptionTableList[0] != null)
                        {
                            if (DTEData.DescriptionTableList[0].TextTableFile != null)
                            {
                                if (DTEData.DescriptionTableList[0].TextTableFile.FileLocation != null)
                                {
                                    if (DTEData.DescriptionTableList[0].TextTableFile.FileLocation != "")
                                    {
                                        OpenDescriptionFolderOutput.IsEnabled = true;
                                    }
                                }
                            }                            
                        }
                    }
                }
                catch
                {
                    OpenDescriptionFolderOutput.IsEnabled = false;
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

            if (EditorClass is TextEditorData texteditor)
            {
                //Separator separator = new();
                //NewContextMenu.Items.Add(separator);
            }


        }
            
    }
}
