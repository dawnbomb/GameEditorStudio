using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
//using System.Linq;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using GameEditorStudio.Loading;
using Microsoft.Windows.Themes;
using Ookii.Dialogs.Wpf;
using WpfHexaEditor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static GameEditorStudio.Entry;
using static OfficeOpenXml.ExcelErrorValue;

namespace GameEditorStudio
{
    //This file is the most complex and barely sorted part of the program. You will hate it :(
    // it's a partial class, and some other parts of the class are ...all over. 
    // i'm not great with moving stuff between classes yet, so yeah, its just a massive partial class.
    // feel free to ask questions about this, and entrymanager. 
    // 
    // I could use help sorting this, like, a lot. 
    //
    //i tried to previously sort it using giant comment walls between major sections, but i kinda stopped moving stuff to proper sections because i was lazy.
    //
    // the bottom is whatever the newest random shit is. 

    public partial class Workshop : Window
    {
        //This is a pertial class.
        //Every file inside the Workshop folder SHOULD be a part of this partial class.
                
        public string WorkshopName { get; set; } //The name of the workshop (IE name of whats selected in Game Library)

        //I haven't decided if i want all saving here yet, or in multiple methods.
        public WorkshopData MyDatabase { get; set; } = new(); //The database of....everything to do with an editor.       

        public DocumentsUserControl TheDocumentsUserControl { get; set; } //Moved here from Database, but maybe later on, make sure this is even a needed variable?

        public string EditorName = "Blank";

        //Project Info
        public bool IsPreviewMode { get; set; } //VS preview mode. In preview mode, a project folder and input directory are not used, to allow users to preview a workshop. 
        public ProjectDataItem ProjectDataItem { get; set; }

        public TextSourceManager TextSourceManager { get; set; }
        public UserControlEditorIcons UCGraphicsEditor { get; set; }

        public Editor EditorClass { get; set; }
        public Category CategoryClass { get; set; }
        public Column ColumnClass { get; set; }
        public Group GroupClass { get; set; }
        public Entry EntryClass { get; set; }

        public List<Entry> EntryMoveList  {  get; set; } = new();
        public Entry MoveEntry { get; set; }


        public string PreviousTabName = ""; //right bar tab to return to when unfocusing listview.

        public UserControlCrossReference TheCrossReference { get; set; }

        public Workshop(string TheWorkshopName, ProjectDataItem Project, bool IsWorkshopPreviewModeActive = false) //GameLibrary GameLibrary
        {
            InitializeComponent();
            this.Title = "Game Editor Studio     Version: " + LibraryMan.VersionNumber + "   ( " + LibraryMan.VersionDate + " )";

            WorkshopName = TheWorkshopName;
            IsPreviewMode = IsWorkshopPreviewModeActive;
            ProjectDataItem = Project;  

            MyDatabase.Workshop = this;
            

            foreach (Command command in TrueDatabase.Commands)
            {
                command.WorkshopData = MyDatabase;
            }
            //var sharedMenusControl = this.FindName("MenusForToolsAndEvents") as SharedMenus;
            //sharedMenusControl.Tools = this.Tools;

            DoThing();

            void DoThing() 
            {
                (TabTest.FindName("GeneralEditor") as TabItem).Visibility = Visibility.Collapsed;
                (TabTest.FindName("GeneralRow") as TabItem).Visibility = Visibility.Collapsed;
                (TabTest.FindName("GeneralColumn") as TabItem).Visibility = Visibility.Collapsed;
                (TabTest.FindName("GeneralGroup") as TabItem).Visibility = Visibility.Collapsed;
                (TabTest.FindName("GeneralEntry") as TabItem).Visibility = Visibility.Collapsed;
                //(TabTest.FindName("GeneralDebug") as TabItem).Visibility = Visibility.Collapsed;

                EntryTab1.Visibility = Visibility.Collapsed;
                EntryTab2.Visibility = Visibility.Collapsed;
                EntryTab3.Visibility = Visibility.Collapsed;
                EntryTab4.Visibility = Visibility.Collapsed;
            }

            //This is all just making sure the current user settings are displayed.
            if (Properties.Settings.Default.EntryAddressType == "Decimal") { EntryAddressTypeButton.Content = "Dec"; }
            if (Properties.Settings.Default.EntryAddressType == "Hex") { EntryAddressTypeButton.Content = "Hex"; }


            //<ComboBoxItem Content="NumberBox"/>
            //< ComboBoxItem Content = "CheckBox" />
            //< ComboBoxItem Content = "BitFlag" />
            //< ComboBoxItem Content = "DropDown" />
            //< ComboBoxItem Content = "List" />
            //PropertiesEntryType
            foreach (EntrySubTypes type in Enum.GetValues(typeof(EntrySubTypes)))
            {
                PropertiesEntryType.Items.Add(new ComboBoxItem { Content = type.ToString() });
            }

            LoadWorkshopDatabaseCode LoadDatabase = new();
            try
            {
                if (IsPreviewMode == false)
                {
                    LoadDatabase.LoadGameFilesIntoDatabase(this, MyDatabase); //First we load workshop files into the database. }
                };
            }
            catch
            {
                MessageBox.Show("The workshop failed to load all files." +
                    "\n" +
                    "\nPossible reasons are as follow:" +
                    "\n1: The input directory is incorrect" +
                    "\n2: You have moved or renamed some files." +
                    "\n3: You failed to extract everything you needed to begin with to use the workshop." +
                    "\n4: The workshop creator has changed the folder / file structure of the workshop." +
                    "\n" +
                    "\nIf you can't stop getting this error, don't keep trying, just ask for help. Especially if you can contact the workshop creator." +
                    "\n" +
                    "\nThe Program will now close as a safety measure.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);


                Application.Current.Shutdown();
                return;
            }            
            LoadDatabase.LoadEditors(this, MyDatabase); //Then we load the editor info into the database.
            //The above method triggers CreateEditor in a loop, creating every editor using the database information.
                       
            
            
            

            //Finally we make sure everything is hidden. This is mostly so i don't have to make sure vision is collapsed all the time when developing the program.
            foreach (KeyValuePair<string, Editor> editor in MyDatabase.GameEditors)
            {
                editor.Value.EditorBackPanel.Visibility = Visibility.Collapsed;

            }

            
            GC.RefreshMemoryLimit();//I have no idea if this is useful. Its a new .net8 feature i read that changes the memory limit (automatically) to be bigger if needed.
            //I know this program lags when loading a menu or editor sometimes, so maybe this will help? 


            { //Run the click home event so it always opens to the home page.
                //FileManager.RefreshFileTree(); //commented out because it causes a crash for some reason
                HIDEALL();
                DockPanelHome.Visibility = Visibility.Visible;
                foreach (Editor editor in MyDatabase.GameEditors.Values)
                {
                    editor.EditorButton.Style = (Style)Application.Current.FindResource("ButtonEditorTab");

                }
                ButtonHome.Style = (Style)Application.Current.FindResource("ButtonCurrentEditorTab");
                //ButtonHome_Click(ButtonHome, new RoutedEventArgs()); 

            }

            if (IsPreviewMode == true) 
            {
                PropertiesTextboxEditorName.IsEnabled = false;
                PropertiesEditorReadGameDataFrom.IsEnabled = false;
                EditorOutputLocationTextbox.IsEnabled = false;
                OpenInputLocationButton.IsEnabled = false;
                OpenOutputLocationButton.IsEnabled = false;

                PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = false;
                PropertiesEditorNameTableStartByte.IsEnabled = false;
                PropertiesEditorNameTableRowSize.IsEnabled = false;
                PropertiesEditorNameTableTextSize.IsEnabled = false;
                PropertiesEditorNameCount.IsEnabled = false;

                DataTableFileBox.IsEnabled = false;
                PropertiesEditorTableStart.IsEnabled = false;
                PropertiesEditorTableWidth.IsEnabled = false;

                FileManager.IsEnabled = false;

                PropertiesRowNameBox.IsEnabled = false;
                PropertiesColumnNameBox.IsEnabled = false;

                PropertiesNameBox.IsEnabled = false;
                HideNameCheckbox.IsEnabled = false;
                HideNameCheckbox.IsEnabled = false;
                PropertiesEntryByteSizeComboBox.IsEnabled = false;
                PropertiesEntryType.IsEnabled = false;
                NumberboxSignCheckbox.IsEnabled = false;
                DropdownMenuType.IsEnabled = false;
                ButtonMenuManager.IsEnabled = false;
                EntryNoteTextbox.IsEnabled = false;


                IconManagerButton.IsEnabled = false; 

            }

            if (Properties.Settings.Default.ShowEntryAddress == true)
            {
                EntryHiddenToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC1E40"));
            }
            else 
            {
                EntryHiddenToggle.Foreground = Brushes.Gray;
            }
                

        }






        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HUD//////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void TextBoxDecConvert(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            // Validate and convert decimal to hexadecimal
            if (int.TryParse(textBox.Text, out int decimalValue))
            {
                HexBox.Text = decimalValue.ToString("X");
                textBox.Background = null; // Reset background color if input is valid
            }
            else if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Background = Brushes.Red; // Change background color if input is invalid
            }
            else
            {
                HexBox.Text = ""; // Clear HexBox if DecBox is cleared
                textBox.Background = null;
            }
        }

        private void TextBoxHexConvert(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            // Validate and convert hexadecimal to decimal
            if (int.TryParse(textBox.Text, System.Globalization.NumberStyles.HexNumber, null, out int hexValue))
            {
                DecBox.Text = hexValue.ToString();
                textBox.Background = null; // Reset background color if input is valid
            }
            else if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Background = Brushes.Red; // Change background color if input is invalid
            }
            else
            {
                DecBox.Text = ""; // Clear DecBox if HexBox is cleared
                textBox.Background = null;
            }
        }






        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {

            FileManager.RefreshFileTree();

            HIDEALL();
            DockPanelHome.Visibility = Visibility.Visible;

            foreach (Editor editor in MyDatabase.GameEditors.Values)
            {
                editor.EditorButton.Style = (Style)Application.Current.FindResource("ButtonEditorTab");

            }
            ButtonHome.Style = (Style)Application.Current.FindResource("ButtonCurrentEditorTab");

        }

        private void ToggleTranslationPanel(object sender, RoutedEventArgs e)
        {   
            #if DEBUG
            if (Properties.Settings.Default.ShowTranslationPanel == true)
            {
                Properties.Settings.Default.ShowTranslationPanel = false;
            }
            else if (Properties.Settings.Default.ShowTranslationPanel == false)
            {
                Properties.Settings.Default.ShowTranslationPanel = true;
            }
            Properties.Settings.Default.Save();                
            #endif
            
            UpdateLeftBars();
        }
        private void ToggleEntrySynbology(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ShowSymbology == true)
            {
                Properties.Settings.Default.ShowSymbology = false;
            }
            else if (Properties.Settings.Default.ShowSymbology == false)
            {
                Properties.Settings.Default.ShowSymbology = true;
            }
            Properties.Settings.Default.Save();

            {   //This is the Entry ID toggle. I'm merging it into the symbology toggle because it makes sense to have them together.
                if (Properties.Settings.Default.ShowEntryAddress == true)
                {
                    Properties.Settings.Default.ShowEntryAddress = false;
                }
                else if (Properties.Settings.Default.ShowEntryAddress == false)
                {
                    Properties.Settings.Default.ShowEntryAddress = true;                    
                }
                Properties.Settings.Default.Save();
            }
            
            UpdateEntryDecorations();
        }

        private void EntryAddressToggle(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ShowEntryAddress == true)
            {
                Properties.Settings.Default.ShowEntryAddress = false;
            }
            else if (Properties.Settings.Default.ShowEntryAddress == false)
            {
                Properties.Settings.Default.ShowEntryAddress = true;
            }            
            Properties.Settings.Default.Save();
            UpdateEntryDecorations();
        }
        private void EntryAddressType(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EntryAddressType == "Decimal")
            {
                Properties.Settings.Default.EntryAddressType = "Hex";
                EntryAddressTypeButton.Content = "Hex";
            }
            else if (Properties.Settings.Default.EntryAddressType == "Hex")
            {
                Properties.Settings.Default.EntryAddressType = "Decimal";
                EntryAddressTypeButton.Content = "Dec";
            }
            else
            {
                Properties.Settings.Default.EntryAddressType = "Decimal";
                EntryAddressTypeButton.Content = "Dec";
            }
            Properties.Settings.Default.Save();
            UpdateEntryDecorations();

        }

        private void EntryOffsetTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateEntryDecorations();
            }
            
        }

        private void ToggleHiddenEntrys(object sender, RoutedEventArgs e)
        {            

            if (Properties.Settings.Default.ShowHiddenEntrys == true)
            {
                Properties.Settings.Default.ShowHiddenEntrys = false;
                EntryHiddenToggle.Foreground = Brushes.Gray;
            }
            else if (Properties.Settings.Default.ShowHiddenEntrys == false)
            {
                Properties.Settings.Default.ShowHiddenEntrys = true;
                EntryHiddenToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC1E40"));
            }
            Properties.Settings.Default.Save();
            UpdateEntryDecorations();
        }

        

        public void UpdateEntryDecorations() 
        {
            foreach (var editor in MyDatabase.GameEditors.Values)
            {
                if (editor.EditorType != "DataTable") { continue; }

                foreach (var row in editor.StandardEditorData.CategoryList)
                {
                    row.CategoryDockPanel.Visibility = Visibility.Collapsed;

                    foreach (var column in row.ColumnList)
                    {
                        column.ColumnPanel.Visibility = Visibility.Collapsed;

                        foreach (Group group in column.ItemBaseList.OfType<Group>())
                        {
                            group.GroupPanel.Visibility = Visibility.Collapsed;
                        }
                    }                    
                }

                foreach (Entry entry in editor.StandardEditorData.MasterEntryList)
                {
                    if (Properties.Settings.Default.ShowEntryAddress == true)
                    {
                        entry.EntryPrefix.Visibility = Visibility.Visible;
                    }
                    else if (Properties.Settings.Default.ShowEntryAddress == false)
                    {
                        entry.EntryPrefix.Visibility = Visibility.Collapsed;
                    }

                    try
                    {
                        if (Properties.Settings.Default.EntryAddressType == "Decimal") //Properties.Settings.Default.EntryPrefix = "Row Offset - Decimal Starting at 0";
                        {

                            entry.EntryPrefix.Content = entry.RowOffset + int.Parse(EntryAddressOffsetTextbox.Text);

                        }
                        else if (Properties.Settings.Default.EntryAddressType == "Hex") //Properties.Settings.Default.EntryPrefix = "Row Offset - Hex Starting at 0x00";
                        {
                            entry.EntryPrefix.Content = (entry.RowOffset + int.Parse(EntryAddressOffsetTextbox.Text)).ToString("X");

                        }
                    }
                    catch
                    {

                    }

                    if (Properties.Settings.Default.ShowSymbology == true)
                    {
                        entry.Symbology.Visibility = Visibility.Visible;

                    }
                    else if (Properties.Settings.Default.ShowSymbology == false)
                    {
                        entry.Symbology.Visibility = Visibility.Collapsed;
                    }

                    /////////// Showing Row/Column/Group //////////////////////

                    if (Properties.Settings.Default.ShowHiddenEntrys == false)
                    {
                        if (entry.IsEntryHidden == true || entry.IsTextInUse == true)
                        {
                            entry.EntryBorder.Visibility = Visibility.Collapsed;
                        }
                        else 
                        {
                            entry.EntryBorder.Visibility = Visibility.Visible;
                            entry.EntryColumn.ColumnPanel.Visibility = Visibility.Visible;
                            entry.EntryRow.CategoryDockPanel.Visibility = Visibility.Visible;
                            if (entry.EntryGroup != null) { entry.EntryGroup.GroupPanel.Visibility = Visibility.Visible; }
                        }
                    }
                    else if (Properties.Settings.Default.ShowHiddenEntrys == true)
                    {
                        entry.EntryBorder.Visibility = Visibility.Visible;
                        entry.EntryColumn.ColumnPanel.Visibility = Visibility.Visible;
                        entry.EntryRow.CategoryDockPanel.Visibility = Visibility.Visible;
                        if (entry.EntryGroup != null) { entry.EntryGroup.GroupPanel.Visibility = Visibility.Visible; }
                    }

                                       


                    
                }






                
            }
        }


        public void UpdateLeftBars() 
        {
            

            foreach (var editor in MyDatabase.GameEditors) 
            {
                if (editor.Value.EditorType != "DataTable") { continue; }

                if (Properties.Settings.Default.ShowTranslationPanel == true)
                {
                    TheLeftBar asdas = editor.Value.StandardEditorData.EditorLeftDockPanel.UserControl as TheLeftBar;
                    asdas.TranslationsPanelBorder.Visibility = Visibility.Visible;
                    //TranslationsPanelBorder
                }
                else if (Properties.Settings.Default.ShowTranslationPanel == false)
                {
                    TheLeftBar asdas = editor.Value.StandardEditorData.EditorLeftDockPanel.UserControl as TheLeftBar;
                    asdas.TranslationsPanelBorder.Visibility = Visibility.Collapsed;
                }

            }

            
        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HUD//////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HOME/////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////







        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HOME/////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////EDITOR PROPERTIES//////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool thing = false;
               

        private void TextboxChangeEditorName(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (PropertiesTextboxEditorName.Text == "") 
                {
                    PropertiesTextboxEditorName.Text = EditorClass.EditorName;
                    return; //If the user tries to set the editor name to nothing, just return.
                }

                MyDatabase.GameEditors.Remove(EditorClass.EditorName);
                EditorClass.EditorName = PropertiesTextboxEditorName.Text;
                EditorClass.EditorNameLabel.Content = EditorClass.EditorName;
                MyDatabase.GameEditors.Add(EditorClass.EditorName, EditorClass);
                
                UpdateEditorButton(EditorClass);
            }
        }
        

        //NOTE: I did a LOT of research on sprites sizes, and 60 is PERFECT for a editor icon size. 
        //it's very rare for icons to be 70+ that are hard to crop, and everything 30 under can be multiplied in size. 
        //While icons size 42 probably can't be cropped and doubled, it's close enough to be on-style.
        //meanwhile vs 50 max, not only do we get sprites size 50~60+ with cropping, but more small sprites can multiply their size to average out more effectivly. 

        public void UpdateEditorButton(Editor AnEditor)
        {
            //if (EditorClass.EditorName != "")
            //{
            //    EditorClass.EditorNameLabel.Content = EditorClass.EditorName;
            //}
            //else if(EditorClass.EditorName == "")
            //{
            //    EditorClass.EditorNameLabel.Content = "??? " + En;
            //}


            return;

            System.Windows.Controls.Image image = AnEditor.EditorImage;

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


        private void ChangeEditorMainTableStartByte(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                
                int NewStart = Int32.Parse(PropertiesEditorTableStart.Text);
                int OldStart = EditorClass.StandardEditorData.DataTableStart;
                int NumMod = NewStart - OldStart;




                //This next ForEach part makes it so if any of the new final 3 bytes of what would be the new offsets are part
                //of a merged entry, that the table start change is canceled. 

                foreach (Entry entry in EditorClass.StandardEditorData.MasterEntryList)
                {
                    //This makes a number called Hoi. Hoi is the new Row offset, if the editor actually changed the start byte of the file it's editing.
                    //if it underflows, it adds tablewidth. If it overflows, it removes tablewidth. So it's always a number inside GameTableSize.
                    int Hoi = entry.RowOffset + -NumMod;
                    if (Hoi < 0) { Hoi = Hoi + EditorClass.StandardEditorData.DataTableRowSize; }
                    if (Hoi > EditorClass.StandardEditorData.DataTableRowSize - 1) { Hoi = Hoi - EditorClass.StandardEditorData.DataTableRowSize; }

                    if (Hoi == EditorClass.StandardEditorData.DataTableRowSize - 1) //If the new Zero minus One'th entry is already part of a size 2 entry, cancel.
                    {
                        if (entry.Bytes == 2)
                        {
                            PropertiesEditorTableStart.Text = OldStart.ToString();
                            return;
                        }

                    }

                    //If the new Zero -1, or -2, or -3 entrys are already part of a size 4 entry, cancel.
                    if (Hoi == EditorClass.StandardEditorData.DataTableRowSize - 1 || Hoi == EditorClass.StandardEditorData.DataTableRowSize - 2 || Hoi == EditorClass.StandardEditorData.DataTableRowSize - 3)
                    {
                        if (entry.Bytes == 4)
                        {
                            PropertiesEditorTableStart.Text = OldStart.ToString();
                            return;
                        }

                    }
                }


                //We have not triggered a cancelation, and are now going to actually change the table start.

                EditorClass.StandardEditorData.DataTableStart = NewStart; //Changes the starting byte of the editor's table, to the new one the user wanted.
                foreach (Entry entry in EditorClass.StandardEditorData.MasterEntryList)
                {
                    entry.RowOffset = entry.RowOffset + -NumMod;
                    if (entry.RowOffset < 0) { entry.RowOffset = entry.RowOffset + EditorClass.StandardEditorData.DataTableRowSize; }
                    if (entry.RowOffset > EditorClass.StandardEditorData.DataTableRowSize - 1) { entry.RowOffset = entry.RowOffset - EditorClass.StandardEditorData.DataTableRowSize; }
                    entry.EntryPrefix.Content = entry.RowOffset.ToString();

                    MyDatabase.EntryManager.LoadEntry(this, EditorClass, entry);
                }
                
            }

            // +/- 1 to all offsets?
            // If NewOffset > RowSize: NewOffset - RowSize and reload ByteValue + Entry
        }







        private void ChangeNameTableStartByte(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int Num = int.Parse(PropertiesEditorNameTableStartByte.Text);

                if (Num < 0)
                {

                    PropertiesEditorNameTableStartByte.Text = EditorClass.StandardEditorData.NameTableStart.ToString();
                    LibraryMan.NotificationNegative("Error: Start byte cannot be less then 0",
                        "I'm not sure why you tried to do this, but it's obviously not allowed. " +
                        "\n\n" +
                        "If this was not a mistake and there is a reason i don't understand why this would ever be desired, " +
                        "you can tell me on discord and i'll consider not explicitly preventing this behavior." +
                        "\n\n" +
                        "The textbox has been reset to it's previous value. "
                        );
                    
                    return;
                }




                EditorClass.StandardEditorData.NameTableStart = Num;
                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(this, EditorClass, "Items");
                foreach (TreeViewItem TreeViewItem in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                {
                    ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                    ItemNameBuilder(TreeViewItem);

                }
            }
        }


        

        private void EditorCharacterSetDropdownClose(object sender, EventArgs e)
        {
            if (IsPreviewMode == true) { return; }

            if (EditorClass.StandardEditorData.NameTableCharacterSet == PropertiesEditorNameTableCharacterSetDropdown.Text) 
            {
                return;
            }


            EditorClass.StandardEditorData.NameTableCharacterSet = PropertiesEditorNameTableCharacterSetDropdown.Text;
            CharacterSetManager CharacterManager = new();
            CharacterManager.Decode(this, EditorClass, "Items");
            foreach (TreeViewItem TreeViewItem in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
            {
                ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                ItemNameBuilder(TreeViewItem);

            }
        }
        //string Error = "";
        //Notification f2 = new(Error);
        //f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        //f2.ShowDialog();


        private void ChangeNameTableTextSize(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int Num = int.Parse(PropertiesEditorNameTableTextSize.Text);

                if (Num < 0)
                {

                    PropertiesEditorNameTableTextSize.Text = EditorClass.StandardEditorData.NameTableTextSize.ToString();
                    LibraryMan.NotificationNegative("Error: Name Table Text Size cannot be less then 0",
                        "This value is how many letters / characters are being read from a file. " +
                        "Hopefully it is obvious why this number cannot be less then 0." +
                        "\n\n" +
                        "The textbox has been reset to it's previous value. "
                        );                    
                    return;
                }



                EditorClass.StandardEditorData.NameTableTextSize = Num;
                //CharacterSetAscii SetAscii = new();
                //SetAscii.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(this, EditorClass, "Items");
                foreach (TreeViewItem TreeViewItem in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                {
                    ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                    ItemNameBuilder(TreeViewItem);

                }
            }
        }

        private void ChangeNameTableRowSize(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int Num = int.Parse(PropertiesEditorNameTableRowSize.Text);

                if (Num < 0)
                {

                    PropertiesEditorNameTableRowSize.Text = EditorClass.StandardEditorData.NameTableRowSize.ToString();
                    LibraryMan.NotificationNegative("Error: Name Row Size cannot be less then 0.",
                        "If you got confused, the Row Size is how many bytes are in 1 FULL Row of a table, not just how many bytes of text your dealing with." +
                        "\n\n" +
                        "Fow example..." +
                        "\n01 02 03 04 05 00 00 00" +
                        "\n01 02 03 04 05 00 00 00" +
                        "\n\n" +
                        "The Row Size here is 8, but the text size is 5. (Well actually text size is probably 7, The max text size + a 00 byte)" +
                        "\n\n" +
                        "The textbox has been reset to it's previous value. "
                        );
                    return;
                }



                EditorClass.StandardEditorData.NameTableRowSize = Num;
                //CharacterSetAscii SetAscii = new();
                //SetAscii.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(this, EditorClass, "Items");
                foreach (TreeViewItem TreeViewItem in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                {
                    ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                    ItemNameBuilder(TreeViewItem);

                }
            }
        }

        
        private bool CheckValidDataTableInfo()
        {
            string value = null;
            try
            {
                byte[] TheFileBytes = EntryClass.EntryEditor.StandardEditorData.FileDataTable.FileBytes;
                int StartByte = int.Parse(PropertiesEditorTableStart.Text);
                int RowSize = int.Parse(PropertiesEditorTableWidth.Text);   
                int RowCount = int.Parse(PropertiesEditorNameCount.Text);  //Aka name count.
                value = TheFileBytes[StartByte + (RowCount * RowSize) + (RowSize-1)].ToString("D");
                return true;
                
            }
            catch
            {
                //Note: Compare each against their source in EntryClass to see if they even match and callout the actual problem.
                LibraryMan.NotificationNegative("Error!",
                    "This checks if the data table has any actual data located at byte (Data Table Start Byte + (NameCount * RowSize)). " +
                    "It seems like thats beyond the end of the data table file (data that doesn't exist). " +
                    "This data is used to fill out entrys, so it's important that it actually exists :P" +
                    "\n\n" +
                    "List of possible causes..." +
                    "\n1: You were editing the start byte and set it starting way to far into the file." +
                    "\n2: You were editing row size and set it way to large" +
                    "\n3: You were editing Name Count and set more names then the data table actually has." +
                    "\n4: You were editing some combination of the three and put in the wrong numbers." +
                    "\n\n" +
                    "Note: Your change was stopped / reverted. Also this is not a problem with the program, it's a safeguard. "
                    );

                PropertiesEditorTableStart.Text = EditorClass.StandardEditorData.DataTableStart.ToString();
                PropertiesEditorTableWidth.Text = EditorClass.StandardEditorData.DataTableRowSize.ToString();
                PropertiesEditorNameCount.Text = EditorClass.StandardEditorData.NameTableItemCount.ToString();
                PropertiesEditorNameTableTextSize.Text = EditorClass.StandardEditorData.NameTableTextSize.ToString();
                PropertiesEditorNameTableRowSize.Text = EditorClass.StandardEditorData.NameTableRowSize.ToString();
                PropertiesEditorNameTableStartByte.Text = EditorClass.StandardEditorData.NameTableStart.ToString();

                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile) { PropertiesEditorNameCount.Text = (EditorClass.StandardEditorData.NameTableItemCount - 1).ToString(); }

                return false;
            }
        }

        private void ChangeNameTableNameCount(object sender, KeyEventArgs e)
        {
            //value = EntryClass.EntryEditor.SWData.FileDataTable.FileBytes[EntryClass.EntryEditor.SWData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");

            if (e.Key == Key.Enter)
            {
                bool Valid = CheckValidDataTableInfo();
                if (Valid == false) { return; }


                //All counts are treated as -1, in order to make it so humans can better understand items. This way item 22 is the one with number 22, not 23. (Due to 0 being a number)
                int NewNameCount = Int32.Parse(PropertiesEditorNameCount.Text);
                int OldNameCount = EditorClass.StandardEditorData.NameTableItemCount - 1;
                int Diffrence = NewNameCount - OldNameCount;
                if (NewNameCount < 1)
                {
                    LibraryMan.Notification("Notice: Lol no.",
                    "As a precautionary measure against any accidents, attempting to delete ALL items is not allowed." +
                    "\n\n" +
                    "The textbox has been reset to it's previous value. "
                    );                    

                    PropertiesEditorNameCount.Text = OldNameCount.ToString();
                    return;
                }

                if (Diffrence > 0) //If more names
                {

                    ////This part would be to prevent crash if new name count goes beyond max possible name count of file. 
                    //int MaxNames = 4;
                    //if (NewNameCount > MaxNames) 
                    //{
                    //    string Error = "You tried to add more names then even exist in the file." +
                    //    "\n" +
                    //    "\n";
                    //    Notification f2 = new(Error);
                    //    f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    //    f2.ShowDialog();
                    //    PropertiesEditorNameCount.Text = OldNameCount.ToString();
                    //    return;
                    //}

                    for (int i = OldNameCount; i != NewNameCount; i++)
                    {
                        TreeViewItem TreeItem = new();
                        ItemInfo ItemInfo = new();
                        TreeItem.Tag = ItemInfo;
                        ItemInfo.ItemIndex = i + 1;



                        EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList.Add(ItemInfo);
                        EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items.Add(TreeItem);


                    }


                    //CharacterSetAscii Encoding = new();
                    //Encoding.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
                    CharacterSetManager CharacterManager = new();
                    CharacterManager.Decode(this, EditorClass, "Items");
                    foreach (TreeViewItem TreeViewItem in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                    {
                        ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                        ItemNameBuilder(TreeViewItem);

                    }

                    EditorClass.StandardEditorData.NameTableItemCount = NewNameCount + 1;


                }
                else if (Diffrence < 0) //If less names
                {
                    //Delete treeview items that have offsets X~Y.
                    //
                    for (int i = Diffrence; i != 0; i++)
                    {
                        int Target = OldNameCount + 1 + i; //i is a negative so we add it to lower the number.
                        foreach (TreeViewItem Item in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                        {
                            ItemInfo ItemInfo = Item.Tag as ItemInfo;
                            if (ItemInfo.ItemIndex == Target)
                            {
                                EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items.Remove(Item);
                                EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList.Remove(ItemInfo);
                                break;
                            }
                            if (ItemInfo.IsFolder == true)
                            {
                                foreach (TreeViewItem childItem in Item.Items)
                                {
                                    ItemInfo childItemInfo = childItem.Tag as ItemInfo;
                                    if (childItemInfo.ItemIndex == Target)
                                    {
                                        // If the child item has the target index, remove it
                                        Item.Items.Remove(childItem);
                                        EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList.Remove(childItemInfo);
                                        break;
                                    }
                                }

                            }

                        }


                    }
                    EditorClass.StandardEditorData.NameTableItemCount = NewNameCount + 1;

                }
            }
        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////EDITOR PROPERTIES//////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////LEFT BAR ITEM PROPERTIES////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public void CreateFolder(TreeView TreeView, TreeViewItem TreeViewItem) 
        {
            ItemInfo TreeViewItemInfo = TreeViewItem.Tag as ItemInfo;
            if (TreeViewItemInfo.IsFolder == true || TreeViewItemInfo.IsChild == true) { return; }

            TreeViewSelectionEnabled = false;


            int selectedIndex = TreeView.ItemContainerGenerator.IndexFromContainer(TreeViewItem);   
            TreeView.Items.Remove(TreeViewItem);

            TreeViewItem FolderItem = new TreeViewItem();
            ItemInfo FolderItemInfo = new();
            FolderItem.Tag = FolderItemInfo;

            FolderItemInfo.ItemName = "New Folder";
            FolderItemInfo.IsFolder = true;
            if (TreeViewItem.Tag is ItemInfo itemInfo)
            {
                itemInfo.IsChild = true;
            }

            ItemNameBuilder(FolderItem); //Created the Header text as a TextBlockItem
            TreeView.Items.Insert(selectedIndex, FolderItem);
            FolderItem.Items.Add(TreeViewItem);


            TreeViewSelectionEnabled = true;


            ContextMenu contextMenu = new ContextMenu();
            
            MenuItem MenuItemDeleteFolder = new MenuItem();
            MenuItemDeleteFolder.Header = "Delete Folder (If Empty)";
            MenuItemDeleteFolder.Click += (sender, e) => DeleteFolder(TreeView, FolderItem);
            contextMenu.Items.Add(MenuItemDeleteFolder);

            FolderItem.ContextMenu = contextMenu;
        }

        public void DeleteFolder(TreeView TreeView, TreeViewItem TreeViewItem) 
        {
            ItemInfo TreeViewItemInfo = TreeViewItem.Tag as ItemInfo;
            if (TreeViewItemInfo.IsFolder == false || TreeViewItem.Items.Count > 0) { return; } 

            TreeViewSelectionEnabled = false;            
            TreeView.Items.Remove(TreeViewItem);
            TreeViewSelectionEnabled = true;
        }
       

        

        public void ItemNameBuilder(TreeViewItem TreeItem) 
        {
            ItemInfo ItemInfo = TreeItem.Tag as ItemInfo;
            TextBlock TextBlockItem = new TextBlock();

            if (ItemInfo.IsFolder == false)
            {
                if (Properties.Settings.Default.ShowItemIndex == true)
                {
                    Run RunIndex = new Run();
                    RunIndex.Text = ItemInfo.ItemIndex + ": ";
                    TextBlockItem.Inlines.Add(RunIndex);
                }                

                
            }

            if (ItemInfo.IsFolder == true)
            {
                Run RunFolder = new Run();
                RunFolder.Foreground = Brushes.Yellow;
                RunFolder.Text = "📁 ";
                TextBlockItem.Inlines.Add(RunFolder);

                Run RunFolderCount = new Run();
                RunFolderCount.Text = "(" + TreeItem.Items.Count.ToString() + ") ";
                TextBlockItem.Inlines.Add(RunFolderCount);
            }

            Run RunMain = new Run();
            RunMain.Text = ItemInfo.ItemName;
            TextBlockItem.Inlines.Add(RunMain);

            Run RunNote = new Run();
            RunNote.Text = " " + ItemInfo.ItemNote;
            RunNote.Foreground = Brushes.Orange; // Set the foreground to red   DeepSkyBlue
            TextBlockItem.Inlines.Add(RunNote);

            if (ItemInfo.ItemWorkshopTooltip == "")
            {
                TreeItem.ToolTip = null;
                RunMain.TextDecorations = null;

            }
            if (ItemInfo.ItemWorkshopTooltip != "") 
            { 
                TreeItem.ToolTip = ItemInfo.ItemWorkshopTooltip;
                RunMain.TextDecorations = TextDecorations.Underline;
            }


            TreeItem.Header = TextBlockItem;

            foreach (Editor TheEditor in MyDatabase.GameEditors.Values) //Super quick and dirty way to update entrys from other editors that are using this editors names. (Menu Link To Editor)
            {
                if (TheEditor.EditorType == "DataTable") 
                {
                    foreach (Entry entry in TheEditor.StandardEditorData.MasterEntryList)
                    {
                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor && entry.EntryTypeMenu.LinkedEditor != null)
                        {
                            if (entry.EntryTypeMenu.LinkedEditor == EditorClass)
                            {
                                if (entry.EntryByteDecimal == null) { return; } //This stops a blank from being created for a dropdown because its loading before the ByteDecimal is loaded, but also probably other misc problems.
                                                                                   //IE i should totally not have this code chunk here, but im still to lazy to impliment this properly, so here goes this ultra bad answer i will regret later! :D

                                MyDatabase.EntryManager.EntryChange(MyDatabase, EntrySubTypes.Menu, this, entry); //This might not even be the best way, or a good way, but i'm lazy atm and it fucking works.
                            }
                        }
                    }
                    
                }
            }
        }               

        public bool TreeViewSelectionEnabled = true;

        

        

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////LEFT BAR ITEM PROPERTIES////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////PAGE PROPERTIES///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////PAGE PROPERTIES///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////ROW PROPERTIES//////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void CreateNewRowAbove(Category TheCat)
        {
            Category NewRow = new();
            NewRow.ColumnList = new List<Column>();
            NewRow.SWData = TheCat.SWData;

            int TheIndex = TheCat.SWData.CategoryList.IndexOf(TheCat);
            NewRow.SWData.CategoryList.Insert(TheIndex, NewRow);


            GenerateStandardEditor CreateSWEditorCode = new();
            CreateSWEditorCode.CreateCategory(EditorClass.StandardEditorData, NewRow, this, MyDatabase, TheIndex);


            Column ColumnClass = new();
            NewRow.ColumnList.Add(ColumnClass);
            ColumnClass.ColumnRow = NewRow;
            CreateSWEditorCode.CreateColumn(NewRow, ColumnClass, this, MyDatabase, -1);

        }


        public void CreateNewRowBelow(Category TheCat)
        {
            Category NewRow = new();
            NewRow.ColumnList = new List<Column>();
            NewRow.SWData = TheCat.SWData;

            int TheIndex = TheCat.SWData.CategoryList.IndexOf(TheCat) + 1;
            NewRow.SWData.CategoryList.Insert(TheIndex, NewRow);


            GenerateStandardEditor CreateSWEditorCode = new();
            CreateSWEditorCode.CreateCategory(EditorClass.StandardEditorData, NewRow, this, MyDatabase, TheIndex);


            Column ColumnClass = new();
            NewRow.ColumnList.Add(ColumnClass);
            ColumnClass.ColumnRow = NewRow;
            CreateSWEditorCode.CreateColumn(NewRow, ColumnClass, this, MyDatabase, -1);

        }

        public void MoveRowUp(Category TheCat) 
        {
            int primaryIndex = TheCat.SWData.MainDockPanel.Children.IndexOf(TheCat.CatBorder);
            if (primaryIndex != 0)
            {
                var secondaryIndex = primaryIndex - 1;
                Category primary = TheCat.SWData.CategoryList[primaryIndex];
                Category secondary = TheCat.SWData.CategoryList[secondaryIndex];

                TheCat.SWData.MainDockPanel.Children.Remove(primary.CatBorder);
                TheCat.SWData.CategoryList.RemoveAt(primaryIndex);
                TheCat.SWData.MainDockPanel.Children.Insert(secondaryIndex, primary.CatBorder);
                TheCat.SWData.CategoryList.Insert(secondaryIndex, primary);

            }
        }


        public void MoveRowDown(Category TheCat) 
        {
            int primaryIndex = TheCat.SWData.MainDockPanel.Children.IndexOf(TheCat.CatBorder);
            if (primaryIndex + 1 < TheCat.SWData.CategoryList.Count)
            {
                var secondaryIndex = primaryIndex + 1;
                Category primary = TheCat.SWData.CategoryList[primaryIndex];
                Category secondary = TheCat.SWData.CategoryList[secondaryIndex];

                TheCat.SWData.MainDockPanel.Children.Remove(primary.CatBorder);
                TheCat.SWData.CategoryList.RemoveAt(primaryIndex);
                TheCat.SWData.MainDockPanel.Children.Insert(primaryIndex + 1, primary.CatBorder);
                TheCat.SWData.CategoryList.Insert(secondaryIndex, primary);

            }
        }

        public void RowDelete(Category TheCat) 
        {
            if (TheCat.ColumnList.Count == 0 || (TheCat.ColumnList.Count == 1 && TheCat.ColumnList[0].ItemBaseList.Count == 0) )
            {
                TheCat.SWData.MainDockPanel.Children.Remove(TheCat.CatBorder);
                TheCat.SWData.CategoryList.Remove(TheCat);
                LibraryMan.GotoGeneralEditor(this);
            }
            
            //Add a IF: count all entrys in all columns in this row, and do IF that is 0.
            //Be careful about other Rows updating their Row order!
        }




        private void PropertiesRowNameBox_KeyDownEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CategoryClass.CategoryName = PropertiesRowNameBox.Text;
                CategoryClass.CategoryLabel.Content = PropertiesRowNameBox.Text;
            }
        }

        private void PropertiesRowTooltipBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CategoryClass.Tooltip = PropertiesRowTooltipBox.Text;            
            if (PropertiesRowTooltipBox.Text == "")
            {
                CategoryClass.CategoryLabel.ToolTip = null;
            }
            else 
            {
                CategoryClass.CategoryLabel.ToolTip = CategoryClass.Tooltip;
            }
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////ROW PROPERTIES//////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////COLUMN PROPERTIES/////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public void CreateNewColumnRight(Column TheColumn)
        {
            Column Column = new();
            Column.ColumnRow = TheColumn.ColumnRow;

            int TheIndex = TheColumn.ColumnRow.ColumnList.IndexOf(TheColumn) + 1;
            Column.ColumnRow.ColumnList.Insert(TheIndex, Column);


            GenerateStandardEditor CreateSWEditorCode = new();
            CreateSWEditorCode.CreateColumn(Column.ColumnRow, Column, this, MyDatabase, TheIndex);

        }

        public void CreateNewColumnLeft(Column TheColumn)
        {
            Column Column = new();
            Column.ColumnRow = TheColumn.ColumnRow;

            int TheIndex = TheColumn.ColumnRow.ColumnList.IndexOf(TheColumn);
            Column.ColumnRow.ColumnList.Insert(TheIndex, Column);


            GenerateStandardEditor CreateSWEditorCode = new();
            CreateSWEditorCode.CreateColumn(Column.ColumnRow, Column, this, MyDatabase, TheIndex);

        }

        public void ColumnDelete(Column TheColumn)
        {
            if (TheColumn.ItemBaseList.Count == 0)
            {
                TheColumn.ColumnRow.CategoryDockPanel.Children.Remove(TheColumn.ColumnPanel);
                TheColumn.ColumnRow.ColumnList.Remove(TheColumn);

                //LibraryMan.GotoGeneralEditor(this);
                

            }


            if (TheColumn.ColumnRow.ColumnList.Count == 0)
            {
                TheColumn.ColumnRow.SWData.MainDockPanel.Children.Remove(TheColumn.ColumnRow.CatBorder);
                TheColumn.ColumnRow.SWData.CategoryList.Remove(TheColumn.ColumnRow);
            }
            

            //I need to delete from memory?

            //I WILL need to be careful about other columns updating their column order, because it WILL be used for the order they are drawn in? 
            //wait but rows dont actually save their order as a number to XML. ???

        }



        private void PropertiesColumnNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {           

            if (e.Key == Key.Enter)
            {
                ColumnClass.ColumnName = PropertiesColumnNameBox.Text;
                ColumnClass.ColumnLabel.Content = PropertiesColumnNameBox.Text;

            }
        }





        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////COLUMN PROPERTIES///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////GROUP PROPERTIES///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void PropertiesGroupNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GroupClass.GroupName = PropertiesGroupNameBox.Text;
                GroupClass.GroupLabel.Content = PropertiesGroupNameBox.Text;
                

            }
        }

        private void PropertiesGroupTooltipTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GroupClass.GroupTooltip = PropertiesGroupTooltipBox.Text;

            //Hook into a Update Group Tooltip method.
            if (PropertiesGroupTooltipBox.Text == "") 
            {
                
            }
            if (PropertiesGroupTooltipBox.Text != "")
            {

            }
        }
        

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////GROUP PROPERTIES////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////ENTRY PROPERTIES///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void PropertiesEntryNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.Name = PropertiesNameBox.Text;
                
                UpdateEntryName(EntryClass);


            }
        }

        public void UpdateEntryName(Entry TheEntry) 
        {           


            MyDatabase.EntryManager.LoadEntry(this, EditorClass, EntryClass);

            StandardEditorMethods.UpdateEntryName(EntryClass);

            Dispatcher.InvokeAsync(() => StandardEditorMethods.LabelWidth(EntryClass.EntryColumn), System.Windows.Threading.DispatcherPriority.Loaded);
            StandardEditorMethods.LabelWidth(EntryClass.EntryColumn);
        }
        

        private void HideNameCheckboxChecked(object sender, RoutedEventArgs e)
        {
            EntryClass.IsNameHidden = true;
            StandardEditorMethods.UpdateEntryName(EntryClass);
            //EntryClass.EntryLabel.Visibility = Visibility.Collapsed;
        }

        private void HideNameCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            EntryClass.IsNameHidden = false;
            StandardEditorMethods.UpdateEntryName(EntryClass);
            //EntryClass.EntryLabel.Visibility = Visibility.Visible;
        }

        private void HideEntryCheckboxChecked(object sender, RoutedEventArgs e)
        {
            //If saving is disabled, changes to this entry will not be saved.
            //This is useful to explicitly prevent users from messing with things that crash the game.
            //Also can be used for other reasons.            

            EntryClass.IsEntryHidden = true;            
            EditorClass.StandardEditorData.SelectedEntry.EntryBorder.Style = (Style)Application.Current.Resources["HiddenSelectedEntryStyle"];
            MyDatabase.EntryManager.EntryChange(MyDatabase, EntryClass.NewSubType, this, EntryClass);
            
            //NOTE: These also trigger when a entry is selected, because the checkbox is updated.
        }

        private void HideEntryCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
           
            EntryClass.IsEntryHidden = false;
            if (EntryClass.IsTextInUse == false) 
            {
                EditorClass.StandardEditorData.SelectedEntry.EntryBorder.Style = (Style)Application.Current.Resources["SelectedEntryStyle"]; 
            }            
            MyDatabase.EntryManager.EntryChange(MyDatabase, EntryClass.NewSubType, this, EntryClass);
        }

        

        private void PropertiesEntryType_DropDownClosed(object sender, EventArgs e)
        {
            string asdf = PropertiesEntryType.Text;
            EntrySubTypes NewEntryType = (EntrySubTypes)Enum.Parse(typeof(EntrySubTypes), asdf);

            if (NewEntryType == Entry.EntrySubTypes.Menu) //Cancel method IF: A Menu-Type entry wants to become size-4.
            {
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian")
                {

                    string FindEntryType = EntryClass.NewSubType.ToString();  //Entry Type Dropdown Menu.
                    foreach (ComboBoxItem item in PropertiesEntryType.Items)
                    {
                        if (item.Content.ToString() == FindEntryType)
                        {
                            PropertiesEntryType.SelectedItem = item;
                            break;
                        }
                    }
                    
                    return;
                }
            }
            

            MyDatabase.EntryManager.EntryChange(MyDatabase, NewEntryType, this, EntryClass);

            EntryManager EManager = new();
            EManager.EntryBecomeActive(EntryClass);
            EManager.UpdateEntryProperties(this, EditorClass);
        }



        private void PropertiesEntryByteSizeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (EntryClass.Endianness != PropertiesEntryByteSizeComboBox.Text) //Cancel method IF: The combobox text did not change.
            {
                string FindEntryByteSize = "Dummy";
                if (EntryClass.Endianness == "1") { FindEntryByteSize = "1 Byte"; } //This makes it so the entrys current type appears in the properties dropdown.
                if (EntryClass.Endianness == "2L") { FindEntryByteSize = "2 Bytes Little Endian"; }
                if (EntryClass.Endianness == "2B") { FindEntryByteSize = "2 Bytes Big Endian"; }


                //Cancel method IF: Entry is attempting to merge with an already merged entry.
                if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "2 Bytes Big Endian")
                {
                    foreach (Entry entry in EditorClass.StandardEditorData.MasterEntryList)
                    {
                        if (entry.RowOffset == EntryClass.RowOffset + 1)
                        {
                            if (entry.Bytes == 2)
                            {
                                PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                LibraryMan.NotificationNegative("Error: Entry not merged.",
                                    "You cannot merge an entry, with an entry that is already merged with something else. " +
                                    "\n\n" +
                                    "If your confused, entrys merge with those next in offset decimal order, not those that are just under them in the UI. "
                                    );
                                return;
                            }
                            if (entry.IsEntryHidden == true || entry.IsTextInUse == true)
                            {
                                PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                LibraryMan.NotificationNegative("Error: Entry not merged.",
                                    "You cannot merge an entry, with an entry that is disabled. " +
                                    "\n\n" +
                                    "Disabled entrys have a red color tint and users can't edit them. " +
                                    "Entrys can be disabled for a few reasons. One, they are disabled automatically if they are text used in the editor. " +
                                    "Two, an editor maker can choose to disable them manually. As for why, there are any number of reasons, but one example " +
                                    "is if editing that information causes the game to crash. " +
                                    "\n\n" +
                                    "You can manually un-disable the entry, but entrys related to text will automatically re-disable themself when the editor loads back up, " +
                                    "even if you save them as non-disabled."
                                    );
                                return;
                            }
                        }
                    }
                    
                }

                //Cancel method IF: Entry is attempting to merge with an already merged entry.
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian")
                {
                    foreach (Entry entry in EditorClass.StandardEditorData.MasterEntryList)
                    {
                        if (entry.RowOffset == EntryClass.RowOffset + 1 || entry.RowOffset == EntryClass.RowOffset + 2 || entry.RowOffset == EntryClass.RowOffset + 3)
                        {
                            if (entry.Bytes == 2 || entry.Bytes == 4)
                            {
                                PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                LibraryMan.NotificationNegative("Error: Entry not merged.",
                                   "You cannot merge an entry, with an entry that is already merged with something else. " +
                                   "\n\n" +
                                   "Atleast one of the 4 bytes / entrys you were attempting to merge with, is already merged. " +
                                   "If your confused, entrys merge with those next in offset decimal order, not those that are just under them in the UI. "
                                    );
                                return;
                            }
                            if (entry.IsEntryHidden == true || entry.IsTextInUse == true)
                            {
                                PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                LibraryMan.NotificationNegative("Error: Entry not merged.",
                                    "You cannot merge an entry, with an entry that is disabled. " +
                                    "\n\n" +
                                    "atleast one of the 4 entrys you were trying to merge, is disabled." +
                                    "\n\n" +
                                    "Disabled entrys have a red color tint and users can't edit them. " +
                                    "Entrys can be disabled for a few reasons. One, they are disabled automatically if they are text used in the editor. " +
                                    "Two, an editor maker can choose to disable them manually. As for why, there are any number of reasons, but one example " +
                                    "is if editing that information causes the game to crash. " +
                                    "\n\n" +
                                    "You can manually un-disable the entry, but entrys related to text will automatically re-disable themself when the editor loads back up, " +
                                    "even if you save them as non-disabled."
                                    );
                                return;
                            }
                        }
                    }
                    
                }





                if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu) //Cancel method IF: A Menu-Type entry wants to become size-4.
                {


                    if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian")
                    {

                        foreach (ComboBoxItem item in PropertiesEntryByteSizeComboBox.Items)
                        {
                            if (item.Content.ToString() == FindEntryByteSize)
                            {
                                PropertiesEntryByteSizeComboBox.SelectedItem = item;
                                break;
                            }
                        }
                        return;
                    }
                }

                int OldSize = EntryClass.Bytes;
                int NewSize = 999;
                string NewSizeS = "x";


                if (PropertiesEntryByteSizeComboBox.Text == "1 Byte") { NewSize = 1; NewSizeS = "1"; }
                if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Little Endian") { NewSize = 2; NewSizeS = "2L"; }
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian") { NewSize = 4; NewSizeS = "4L"; }
                if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Big Endian") { NewSize = 2; NewSizeS = "2B"; }
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian") { NewSize = 4; NewSizeS = "4B"; }

                if (EntryClass.RowOffset <= EditorClass.StandardEditorData.DataTableRowSize - NewSize) //Cancel method IF: New size would overflow off the end of GameTableSize
                {


                    EntryClass.Endianness = NewSizeS;
                    EntryClass.Bytes = NewSize;

                    if (EntryClass.EntryTypeMenu != null) 
                    {
                        if (EntryClass.EntryTypeMenu.NothingNameList != null)
                        {
                            if (NewSize == 1)
                            {
                                string[] items = EntryClass.EntryTypeMenu.NothingNameList;
                                Array.Resize(ref items, 256);
                                EntryClass.EntryTypeMenu.NothingNameList = items;
                                EntryClass.EntryTypeMenu.ListSize = 256;
                            }
                            if (NewSize == 2)
                            {
                                string[] items = EntryClass.EntryTypeMenu.NothingNameList;
                                Array.Resize(ref items, 65536);
                                EntryClass.EntryTypeMenu.NothingNameList = items;
                                EntryClass.EntryTypeMenu.ListSize = 65536;
                            }
                        }
                    }





                    //EntryData.ReloadEntry(Database, EditorClass, EntryClass);
                    MyDatabase.EntryManager.LoadEntry(this, EditorClass, EntryClass);



                    //i need to add a check to make sure the next entrys are currently ALL ByteSize 1, if even one is not, cancel the process.


                    //Below is what happens to OTHER ENTRYS, NOT the main entry having it's size changed.


                    List<Entry> targets = new();


                    List<int> list = new();

                    //if Old smaller then new
                    if (OldSize < NewSize)
                    {
                        foreach (int i in Enumerable.Range(Math.Min(OldSize, NewSize), Math.Abs(OldSize - NewSize)))
                        {
                            list.Add(EntryClass.RowOffset + i);
                        }
                        foreach (var num in list)
                        {
                            foreach (var row in EditorClass.StandardEditorData.CategoryList)
                            {
                                foreach (var column in row.ColumnList)
                                {
                                    foreach (Entry entry in column.ItemBaseList)
                                    {
                                        if (entry.RowOffset == num) //Search for entry with offset+1 over current entry.  Offset+X
                                        {
                                            entry.Endianness = "0";
                                            entry.Bytes = 0;
                                            MyDatabase.EntryManager.LoadEntry(this, EditorClass, entry);
                                            entry.EntryBorder.Visibility = Visibility.Collapsed;

                                            targets.Add(entry);

                                        }
                                    }
                                }
                            }
                        }
                    }

                    //This makes sure when a entry is hidden / merged, that it is relocated to wherever the primary entry is.
                    foreach (var entry in targets)
                    {


                        int MyIndex = entry.EntryColumn.ItemBaseList.IndexOf(entry); //entry.EntryColumn.ColumnGrid.Children.IndexOf(entry.EntryDockPanel);
                        entry.EntryColumn.ItemBaseList.RemoveAt(MyIndex);
                        entry.EntryColumn.ColumnPanel.Children.Remove(entry.EntryBorder);


                        int EntryLocation = ColumnClass.ColumnPanel.Children.IndexOf(EntryClass.EntryBorder) - 1; //Counting starts at 1, but the first child is 0.
                        int Diffrence = entry.RowOffset - EntryClass.RowOffset;
                        ColumnClass.ItemBaseList.Insert(EntryLocation + Diffrence, entry);
                        ColumnClass.ColumnPanel.Children.Insert(EntryLocation + Diffrence + 1, entry.EntryBorder);


                        entry.EntryRow = EntryClass.EntryRow;  //  PageClass.RowList[rowIndex + 1];
                        entry.EntryColumn = EntryClass.EntryColumn;   //PageClass.RowList[rowIndex + 1].ColumnList[0];
                    }

                    ////if Old bigger then new
                    if (OldSize > NewSize)
                    {
                        foreach (int i in Enumerable.Range(Math.Min(OldSize, NewSize), Math.Abs(OldSize - NewSize))) //.OrderByDescending(x => x)
                        {
                            list.Add(EntryClass.RowOffset + i);
                        }
                        foreach (var num in list)
                        {
                            foreach (Entry entry in EditorClass.StandardEditorData.MasterEntryList)
                            {
                                if (entry.RowOffset == num) //Search for entry with offset+1 over current entry.  Offset+X
                                {
                                    entry.Endianness = "1";
                                    entry.Bytes = 1;
                                    MyDatabase.EntryManager.LoadEntry(this, EditorClass, entry);
                                    //EntryData.ReloadEntry(Database, EditorClass, entry);
                                    entry.EntryBorder.Visibility = Visibility.Visible;

                                }
                            }
                            
                        }
                    }



                    //I am currently not allowing size 4+ of Lists, i don't know that any game that uses more then 65K options to select from in one menu.
                    //I ACTUALLY NEED TO ADD THIS. ITS NOT ABOUT GAMES NOT NEEDING IT, IT'S ABOUT THAT THE FUCKING DO IT ANYWAY, AND NOT SUPPORTING IT MAKES EDITORS LOOK UGLY!!! (AND IS MISLEADING TO USERS!)

                    //if (NewSize == 4)   
                    //{
                    //    string[] items = EntryClass.ListItems;
                    //    Array.Resize(ref items, 4294967296);
                    //    EntryClass.ListItems = items;
                    //}




                }//end of IF Row Overflow  //Makes sure byte size only changes if it won't overflow off the end of GameTableSize
                else
                {
                    PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                }

            }//End of IF

            UpdateSymbology(EntryClass);
            
        }




        

        private void SetNumberboxUnsigned(object sender, RoutedEventArgs e)
        {   
            EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Unsigned;

            EntryManager EntryManager = new();
            EntryManager.LoadEntry(this, EditorClass, EntryClass);
        }

        private void SetNumberboxSigned(object sender, RoutedEventArgs e)
        {
            EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Signed;

            EntryManager EntryManager = new();
            EntryManager.LoadEntry(this, EditorClass, EntryClass);
        }




        /////////////////////////////////////// END OF ENTRY DATA ///////////////////////////////////
        /////////////////////////////////////// GOOGLE SHEETS ///////////////////////////////////////


        public void HIDEMOST() 
        {
            DockPanelHome.Visibility = Visibility.Collapsed;


            if (UCGraphicsEditor != null) { MidGrid.Children.Remove(UCGraphicsEditor); }
            if (TextSourceManager != null) { MidGrid.Children.Remove(TextSourceManager); }
        }
        

        public void HIDEALL() 
        {
            foreach (KeyValuePair<string, Editor> editor in MyDatabase.GameEditors)
            {
                editor.Value.EditorBackPanel.Visibility = Visibility.Collapsed;
            }
            DockPanelHome.Visibility = Visibility.Collapsed;          

            if (UCGraphicsEditor != null) { MidGrid.Children.Remove(UCGraphicsEditor); }
            if (TextSourceManager != null) { MidGrid.Children.Remove(TextSourceManager); }

            
             

        }

        

        

        
                

        

        private void EditorBarMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                // Scroll right
                EditorBarScrollViewer.ScrollToHorizontalOffset(EditorBarScrollViewer.HorizontalOffset + 70);
            }
            else
            {
                // Scroll left
                EditorBarScrollViewer.ScrollToHorizontalOffset(EditorBarScrollViewer.HorizontalOffset - 70);
            }

            // Mark the event as handled so it doesn't propagate further
            e.Handled = true;
        }

        
                

        private void ApplyFormulaToEntryAcrossAllItems(object sender, RoutedEventArgs e)
        {
            //Almost everything here uses doubles instead of ints to make ABSOLUTELY FUCKING SURE nothing EVER goes out of range, even when adding more value types or using large negatives from super robot wars / disgaea.
            //FormulaComboBox
            //FormulaTextBox

            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true) { return; } //prevents users from axidentally modding values that should be otherwise already disabled.
            if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed) { return; }

            //if (EntryClass.EntryByteSize != "1") { return; } //temporary

            int FinalItem = 0;
            try
            {                
                if (EditorClass.StandardEditorData.NameTableItemCount != 0) { FinalItem = EditorClass.StandardEditorData.NameTableItemCount; }
                if (EditorClass.StandardEditorData.NameTableItemCount == 0)
                {
                    int ItemCount = 0;
                    foreach (var Item in EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList)
                    {
                        if (Item.IsFolder == false)
                        {
                            ItemCount++;
                        }
                    }
                    FinalItem = ItemCount;
                }
                FinalItem = FinalItem - int.Parse(FormulaDoNotModTextBox.Text); //Allows users to NOT mod the final X number of items in the list.
            }
            catch 
            {
                LibraryMan.NotificationNegative("Error: ",
                    "An error happened during the first step of auto-mod. " +
                    "In this step, it simply tries to count how many items it's going to mod. " +
                    "This error can probably only appear if the editor is not getting it's item names from an actual game file. " +
                    "\n\n" +
                    "Anyway, Auto-mod will now cancel. Nothing has been changed."
                    );
                return;
            }
            
            
            for (int i = 0; i < FinalItem; i++) 
            {

                try 
                {
                    //Get Current Value Step
                    double CurrentValue = 0; //Will cause conflicts with negative numbers so im ignoring them for now. (Maybe i can support negative byte sizes 1 and 2 easily though ?)                
                    if (EntryClass.Endianness == "1")
                    {
                        EntryClass.EntryByteDecimal = EditorClass.StandardEditorData.FileDataTable.FileBytes[EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
                        CurrentValue = EditorClass.StandardEditorData.FileDataTable.FileBytes[EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset];
                    }
                    if (EntryClass.Endianness == "2B")
                    {
                        EntryClass.EntryByteDecimal = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                        CurrentValue = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                    }
                    if (EntryClass.Endianness == "4B")
                    {
                        EntryClass.EntryByteDecimal = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                        CurrentValue = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                    }
                    if (EntryClass.Endianness == "2L")
                    {
                        ushort WrongValue = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                        CurrentValue = (ushort)IPAddress.HostToNetworkOrder((short)WrongValue); // Swaps the endianness
                    }
                    if (EntryClass.Endianness == "4L")
                    {
                        uint value = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                        byte[] valueBytes = BitConverter.GetBytes(value);    // Swaps the endianness
                        Array.Reverse(valueBytes);                           // Swaps the endianness
                        CurrentValue = BitConverter.ToUInt32(valueBytes, 0); // Swaps the endianness
                    }

                    //set above as IF size 1
                    //make more for IF size 2L 2B 4L 4B
                    double NewValue = 0;

                    if (FormulaComboBox.Text == "Multiply") //Multiply Step
                    {
                        double multiplier = 1;
                        string formulaText = FormulaTextBox.Text.Trim().TrimEnd('%');

                        if (double.TryParse(formulaText, out double percentage)) //the max size of a double is 9 quadrillion, so it should never cause any size limit errors.
                        {
                            multiplier = percentage / 100;
                        }
                        else
                        {
                            return;
                        }

                        double MathResult = CurrentValue * multiplier;
                        NewValue = (double)Math.Round(MathResult);

                    }

                    if (FormulaComboBox.Text == "Add") //Add Step
                    {
                        NewValue = CurrentValue + double.Parse(FormulaTextBox.Text);
                    }

                    if (FormulaComboBox.Text == "Subtract") //Subtract Step
                    {
                        NewValue = CurrentValue - double.Parse(FormulaTextBox.Text);
                    }

                    //MIN Step
                    if (FormulaMinTextBox.Text != "" && FormulaMinTextBox.Text != null) { if (NewValue < double.Parse(FormulaMinTextBox.Text)) { NewValue = double.Parse(FormulaMinTextBox.Text); } }
                    if (NewValue < 0) { NewValue = 0; } //True Min Step

                    //MAX Step
                    if (FormulaMaxTextBox.Text != "" && FormulaMaxTextBox.Text != null) { if (NewValue > double.Parse(FormulaMaxTextBox.Text)) { NewValue = double.Parse(FormulaMaxTextBox.Text); } }
                    if (EntryClass.Endianness == "1" && NewValue > 255) { NewValue = 255; } //True Max Step
                    if (EntryClass.Endianness == "2B" && NewValue > 65535) { NewValue = 65535; }
                    if (EntryClass.Endianness == "2L" && NewValue > 65535) { NewValue = 65535; }
                    if (EntryClass.Endianness == "4B" && NewValue > 4294967295) { NewValue = 4294967295; }
                    if (EntryClass.Endianness == "4L" && NewValue > 4294967295) { NewValue = 4294967295; }





                    //Saving Step
                    string Result = NewValue.ToString();

                    if (EntryClass.Endianness == "1")  // This is saving 1 Byte Size?   // First 1 byte save
                    {
                        Byte.TryParse(Result, out byte value8);
                        { ByteManager.ByteWriter(value8, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset); }
                    }
                    if (EntryClass.Endianness == "2L")
                    {
                        UInt16.TryParse(Result, out ushort value16);
                        value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                        { ByteManager.ByteWriter(value16, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save


                    }
                    if (EntryClass.Endianness == "4L")
                    {
                        UInt32.TryParse(Result, out uint value32);
                        byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                        Array.Reverse(valueBytes); // Swap the endianness
                        value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                        { ByteManager.ByteWriter(value32, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save

                    }
                    if (EntryClass.Endianness == "2B")
                    {
                        UInt16.TryParse(Result, out ushort value16);
                        { ByteManager.ByteWriter(value16, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save
                    }
                    if (EntryClass.Endianness == "4B")
                    {
                        UInt32.TryParse(Result, out uint value32);
                        { ByteManager.ByteWriter(value32, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save
                    }

                    //FormulaMinTextBox
                    //FormulaMaxTextBox
                }
                catch 
                {
                    LibraryMan.NotificationNegative("Error: ???",
                        "An error happened during the actual modifying of data in memory." +
                        "\nThis means some of the items have been changed, and others have not." +
                        "\nNothing has been saved to actual files on the computer, so don't worry." +
                        "\n" +
                        "\nHowever, this is a very serious error. It is strongly recommended you close the program WITHOUT saving your game files." +
                        "\n" +
                        "\nI chose not to automatically force crash the program, to give you a chance to save some non-game file related things first. " +
                        "before you close everything, in the workshop menu you may save your documents, common events, and editors, but absolutely do not save your game files. " +
                        "If you do, you will save them with only some items being changed, but not all of them." 
                    );
                    return;
                }


            }







        }

        

        public void EntryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem ListItem = EntryListBox.SelectedItem as ListViewItem;
            //TextBlock TheBlock = ListItem.Content as TextBlock;
            string selectedItem = ListItem.Content.ToString();

            EntryClass.EntryTypeMenu.ListButton.Content = selectedItem; //TheBlock;

            //Emanager.SaveList(EntryClass);
            string input = (string)EntryClass.EntryTypeMenu.ListButton.Content; //selectedItem; //
            string[] parts = input.Split(':');
            string number = parts[0].Trim();
            EntryClass.EntryByteDecimal = number; // Console.WriteLine(number); // Output: 24


            MyDatabase.EntryManager.SaveEntry(EntryClass.EntryEditor, EntryClass);
            MyDatabase.EntryManager.UpdateEntryProperties(this, EntryClass.EntryEditor);


            foreach (TabItem tabItem in MainTabControl.Items)
            {
                if (tabItem.Header != null && tabItem.Header.ToString() == PreviousTabName)
                {
                    tabItem.IsSelected = true;
                    break;
                }
            }

        }

        

        private void DoThing2(object sender, RoutedEventArgs e)
        {
            HIDEMOST();
            TextSourceManager TheUserControl = new TextSourceManager();
            MidGrid.Children.Add(TheUserControl);
            TheUserControl.SetupForMenu(EntryClass, EntryListBox, MyDatabase, this);
            TextSourceManager = TheUserControl;
        }


        private void ToggleItemIDNumberVisibility(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ShowItemIndex == false)
            {
                Properties.Settings.Default.ShowItemIndex = true;
            }
            else if (Properties.Settings.Default.ShowItemIndex == true)
            {
                Properties.Settings.Default.ShowItemIndex = false;  
            }
            Properties.Settings.Default.Save();
            foreach (var editor in MyDatabase.GameEditors)
            {
                if (editor.Value.EditorType == "DataTable")
                {
                    foreach (TreeViewItem TreeItem in editor.Value.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                    {
                        ItemNameBuilder(TreeItem);
                    }
                }

            }
        }


        private void EntryNoteTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (EditorClass == null) { return; }
            EditorClass.StandardEditorData.SelectedEntry.WorkshopTooltip = EntryNoteTextbox.Text;
            StandardEditorMethods.UpdateEntryName(EditorClass.StandardEditorData.SelectedEntry);
        }

        private void PropertiesMenuType_DropDownClosed(object sender, EventArgs e)
        {            
            if (DropdownMenuType.Text == "Dropdown") { EntryClass.EntryTypeMenu.MenuType = EntryTypeMenu.MenuTypes.Dropdown; }
            else if (DropdownMenuType.Text == "List") { EntryClass.EntryTypeMenu.MenuType = EntryTypeMenu.MenuTypes.List; }
              
            MyDatabase.EntryManager.EntryChange(MyDatabase, EntrySubTypes.Menu, this, EntryClass);
        }

        private void ListLostFocus(object sender, RoutedEventArgs e)
        {            
            
        }


        public void UpdateSymbology(Entry EntryClass)
        {
            EntryClass.Symbology.Content = "";
            EntryClass.Symbology.Width = 48;
            EntryClass.Symbology.Foreground = Brushes.White;
            EntryClass.Symbology.FontSize = 20;
            //TheWorkshop.PropertiesEntry1Byte.Text = EditorClass.SWData.FileDataTable.FileBytes[EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.RowSize) + EntryClass.RowOffset].ToString("D");

            {
                List<int> BitFlags = new() { 2, 4, 8, 16, 32, 64, 128 };

                string ValueA = "";
                string ValueB = "";
                string ValueX = "";

                List<int> AllValues = new();

                bool IsAlwaysZero = true;
                bool IsNeverZero = true;
                bool IsAlwaysOne = true;                
                bool IsCheckboxLike = true;                
                bool IsAlwaysValueX = true;
                bool IsCheckbox = true;
                string HalfColor = "#907654";//Between red and gold, the 50% / "Ehhhhh" color.  //cd5032

                for (int i = 0; i < EntryClass.EntryEditor.StandardEditorData.NameTableItemCount; i++)
                {
                    string value = null;
                    try 
                    {                        

                        if (EntryClass.Endianness == "1")
                        {
                            value = EntryClass.EntryEditor.StandardEditorData.FileDataTable.FileBytes[EntryClass.EntryEditor.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
                        }
                        else if (EntryClass.Endianness == "2B")
                        {
                            value = BitConverter.ToUInt16(EntryClass.EntryEditor.StandardEditorData.FileDataTable.FileBytes, EntryClass.EntryEditor.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                        }
                        else if (EntryClass.Endianness == "2L")
                        {
                            ushort value2 = BitConverter.ToUInt16(EntryClass.EntryEditor.StandardEditorData.FileDataTable.FileBytes, EntryClass.EntryEditor.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                            ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
                            value = swappedValue2.ToString("D");
                        }
                        else if (EntryClass.Endianness == "4B")
                        {
                            value = BitConverter.ToUInt32(EntryClass.EntryEditor.StandardEditorData.FileDataTable.FileBytes, EntryClass.EntryEditor.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                        }
                        else if (EntryClass.Endianness == "4L")
                        {
                            uint valueK = BitConverter.ToUInt32(EntryClass.EntryEditor.StandardEditorData.FileDataTable.FileBytes, EntryClass.EntryEditor.StandardEditorData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                            byte[] valueBytes = BitConverter.GetBytes(valueK);
                            Array.Reverse(valueBytes);
                            uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
                            value = swappedValue.ToString("D");
                        }

                    } 
                    catch 
                    {
                        LibraryMan.Notification("Error: Symbology caused a crash.",
                            "List of possible causes..." +
                            "\n1: Creating a new editor where the list of names is more then the actual data." +
                            "\n2: ?????." +
                            "\n" +
                            "\nNote: Please report this."
                            );
                        Environment.FailFast(null); //Kills program instantly. 
                        return;
                    }

                    AllValues.Add(int.Parse(value));

                    if (ValueX == "")
                    {
                        ValueX = value;
                    }                    
                    if (ValueA != "" && ValueB == "")
                    {
                        ValueB = value;
                    }
                    if (ValueA == "" && ValueB == "")
                    {
                        ValueA = value;
                    }


                    IsAlwaysZero = AllValues.All(x => x == 0);
                    IsNeverZero = !AllValues.Contains(0);
                    IsCheckboxLike = AllValues.Distinct().Count() <= 2;
                    IsAlwaysOne = AllValues.All(v => v == 1);
                    IsAlwaysValueX = int.TryParse(ValueX, out int parsedX) && AllValues.All(v => v == parsedX);
                    IsCheckbox = AllValues.All(v => v == 0 || v == 1);

                }

                ToolTipService.SetInitialShowDelay(EntryClass.Symbology, 100);
                ToolTipService.SetBetweenShowDelay(EntryClass.Symbology, 100);
                                
                EntryClass.Symbology.Margin = new Thickness(-3, 0, -7, 0);

                bool IsMostlyX = AllValues.GroupBy(x => x).Any(g => (double)g.Count() / AllValues.Count >= 0.8);
                bool IsHalfX = AllValues.GroupBy(x => x).Any(g => (double)g.Count() / AllValues.Count >= 0.5);
                bool HasAtLeast10Unique = AllValues.Distinct().Count() >= 10; //unused
                bool HasAtLeast20Unique = AllValues.Distinct().Count() >= 20; //unused
                bool HasAtLeast30Unique = AllValues.Distinct().Count() >= 30; //unused
                int uniqueCount = AllValues.Distinct().Count();

                //negative number detector
                bool B1NoValuesAre128to199 = false;      // No values are between 128–199
                bool B1_4PercentAbove128 = false; // At least 4% of values are 128+
                bool B1_4PercentAbove200 = false;  // At least 10% of values are 200–255
                bool B1_4PercentBelow127 = false; // At least 10% of values are 127 or below

                if (EntryClass.Bytes == 1)
                {
                    B1NoValuesAre128to199 = !AllValues.Any(v => v >= 128 && v < 200);
                    B1_4PercentAbove200 = AllValues.Count(v => v >= 200) >= AllValues.Count * 0.04;

                    int highValuesCount = AllValues.Count(v => v >= 128);
                    B1_4PercentAbove128 = highValuesCount >= AllValues.Count * 0.04;

                    int lowValuesCount = AllValues.Count(v => v <= 127);
                    B1_4PercentBelow127 = lowValuesCount >= AllValues.Count * 0.04;
                }

                //Bitflag detectors
                bool PureBitFlags = false; //100% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64,128. Atleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.
                bool MostlyBitFlag = false; //70% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64,128. Atleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.
                bool HalfBitFlag = false; //50% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64,128. Atleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.

                if (EntryClass.Bytes == 1)
                {                    
                    var bitValues = new HashSet<int> { 0, 1, 2, 4, 8, 16, 32, 64, 128 };
                    var totalCount = AllValues.Count;
                    var uniqueValues = AllValues.Distinct().ToHashSet();

                    // Basic check: all values are valid bitflag values
                    bool allAreBitFlags = uniqueValues.All(v => bitValues.Contains(v));

                    // Extra checks:
                    int count0Or1 = AllValues.Count(v => v == 0 || v == 1);
                    int countAbove1 = AllValues.Count(v => v > 1);

                    bool hasAtLeast10PercentLow = count0Or1 >= totalCount * 0.1;
                    bool hasAtLeast10PercentHigh = countAbove1 >= totalCount * 0.1;

                    // Final result
                    PureBitFlags = allAreBitFlags && hasAtLeast10PercentLow && hasAtLeast10PercentHigh;

                    //////////Mostly bitflag

                    var bitFlags = new HashSet<int> { 2, 4, 8, 16, 32, 64, 128 };
                    int countBitFlagsOnly = AllValues.Count(v => bitFlags.Contains(v));
                    bool hasEnoughBitFlags = countBitFlagsOnly >= totalCount * 0.7;
                    MostlyBitFlag = hasEnoughBitFlags && hasAtLeast10PercentLow && hasAtLeast10PercentHigh;

                    //////////Half bitflag
                    bool hasHalfBitFlags = countBitFlagsOnly >= totalCount * 0.5;
                    HalfBitFlag = hasHalfBitFlags && hasAtLeast10PercentLow && hasAtLeast10PercentHigh;

                }

                //////////////////////////////////// ACTUALLY MAKING THE SYMBOLOGY STARTS HERE /////////////////////////////////////


                if (IsCheckboxLike == false && EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox) //IE: Warning! This should not be a checkbox!
                {                    
                    EntryClass.Symbology.Foreground = Brushes.Red;
                    EntryClass.Symbology.Content = "!!!";
                    EntryClass.Symbology.ToolTip = "This entry is probably not a checkbox, it has more then 2 possible values.   ValueA: " + ValueA + " ValueB: " + ValueB + " ValueX: " + ValueX;
                }
                else if (B1NoValuesAre128to199 == true && B1_4PercentAbove128 == true && B1_4PercentBelow127 == true && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox) //Is Probably Negative
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " -?";
                    EntryClass.Symbology.ToolTip = "This is PROBABLY data that can represent negative numbers.\n\nNo values are between 128-200, 4%+ of values are 200+, and 4%+ are 127 or less.\nThis is at the very least, extremely suspicious.\n\nPS: values 200~255 are -1 ~ -54 when read as negatives.";
                }                
                else if (B1NoValuesAre128to199 == true && B1_4PercentAbove200 == true && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox) //Is Likely Negative
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = " -?";
                    EntryClass.Symbology.ToolTip = "This is PROBABLY data that can represent negative numbers.\n\nNo values are between 128-200, 4%+ of values are 200+.\nThis is at the very least, extremely suspicious.\n\nPS: values 200~255 are -1 ~ -54 when read as negatives.";
                }

                else if (PureBitFlags == true) //Is bitflag
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " BF";
                    EntryClass.Symbology.ToolTip = "This is a pure bitflag.\n\n100% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64, or 128.\nAtleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.";
                }
                else if (MostlyBitFlag == true) //Is bitflag // && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "7BF";
                    EntryClass.Symbology.ToolTip = "This is PROBABLY a bitflag.\n\n70% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64, or 128.\nAtleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.\nThis is at the very least, extremely suspicious.";
                }
                else if (MostlyBitFlag == true) //Is bitflag  // && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "5BF";
                    EntryClass.Symbology.ToolTip = "This is PROBABLY a bitflag.\n\n50% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64, or 128.\nAtleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.\nThis is at the very least, extremely suspicious.";
                }

                else if (IsAlwaysZero == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " 0";
                    EntryClass.Symbology.ToolTip = "This entry is always zero.\n\nSometimes this means its actually part of a 2+ byte entry.";
                }                
                else if (IsAlwaysOne == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " 1";
                    EntryClass.Symbology.ToolTip = "This entry is always 1.\n\nSometimes this means its actually part of a 2+ byte entry.";

                }
                else if (IsAlwaysValueX == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " X";
                    EntryClass.Symbology.ToolTip = "This entry's value is always " + ValueX + ".";
                }
                else if (IsCheckbox == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = "✔";
                    EntryClass.Symbology.ToolTip = "This entry is a checkbox (It's always 1 or 0).\n\nOR it's a 2 byte value, like when max MP goes beyond 255.";
                    //SymbolLabel.Margin = new Thickness(-5, 0, 0, 0);
                }
                else if (IsCheckboxLike == true)
                {                    
                    EntryClass.Symbology.Foreground = Brushes.Gray; //new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Content = "✔?";
                    EntryClass.Symbology.ToolTip = "This is checkbox-like, but instead of 0 and 1, it's " + ValueA + " & " + ValueB;
                    //SymbolLabel.Margin = new Thickness(-5,0,0,0);
                }

                //Starting here, it'd be cool if "Is never 0" could stack with everything, to display even more info. 
                else if (uniqueCount > 10) //If Mostly X
                {
                    //EntryClass.Symbology.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    if (uniqueCount < 100) { EntryClass.Symbology.Content = uniqueCount; }
                    else { EntryClass.Symbology.Content = "99+"; }
                    EntryClass.Symbology.ToolTip = "This entry has " + uniqueCount + " unique values.";

                }
                else if (IsNeverZero == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Red;
                    EntryClass.Symbology.Content = " 0";
                    EntryClass.Symbology.ToolTip = "This entry is never zero.\n\nThis is actually pretty rare, so you should be suspicious. ";

                }
                else if (IsMostlyX == true) //If Mostly X
                {
                    //EntryClass.Symbology.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = " X?";
                    EntryClass.Symbology.ToolTip = "This entry's value is the same somewhere between 80% and 99% of the time.";

                }
                else if (IsHalfX == true) //If Mostly X
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "X??";
                    EntryClass.Symbology.ToolTip = "This entry's value is the same somewhere between 50% and 80% of the time.";
                    EntryClass.Symbology.FontSize = 12;

                }




                if (Properties.Settings.Default.ShowSymbology == true)
                {
                    EntryClass.Symbology.Visibility = Visibility.Visible;
                }
                if (Properties.Settings.Default.ShowSymbology == false)
                {
                    EntryClass.Symbology.Visibility = Visibility.Collapsed;
                }


            }



        }

        private void OpenWorkshopInputButton(object sender, RoutedEventArgs e)
        {            
            LibraryMan.OpenFileFolder(PropertiesEditorReadGameDataFrom.Text);
        }

        private void OpenWorkshopOutputButton(object sender, RoutedEventArgs e)
        {            
            LibraryMan.OpenFileFolder(EditorOutputLocationTextbox.Text);
        }

        private void OpenIconManager(object sender, RoutedEventArgs e)
        {
            HIDEMOST();
            UserControlEditorIcons TheUserControl = new UserControlEditorIcons();
            MidGrid.Children.Add(TheUserControl);
            UCGraphicsEditor = TheUserControl;




            //Button IconButton = new();
            //IconButton.Content = " Icon Editor ";
            //IconButton.Click += delegate
            //{
            //    TheWorkshop.HIDEMOST();
            //    UserControlEditorIcons TheUserControl = new UserControlEditorIcons();
            //    TheWorkshop.MidGrid.Children.Add(TheUserControl);
            //    TheWorkshop.UCGraphicsEditor = TheUserControl;
            //};
            //EditorHeader.Children.Add(IconButton);
            //IconButton.HorizontalAlignment = HorizontalAlignment.Right;
            //IconButton.Margin = new Thickness(4);
            //if (TheWorkshop.IsPreviewMode == true) { IconButton.IsEnabled = false; }
        }

        private void Bitflag1NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag1Name = PropertiesEntryBitFlag1Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        private void Bitflag2NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag2Name = PropertiesEntryBitFlag2Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        private void Bitflag3NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag3Name = PropertiesEntryBitFlag3Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        private void Bitflag4NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag4Name = PropertiesEntryBitFlag4Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        private void Bitflag5NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag5Name = PropertiesEntryBitFlag5Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        private void Bitflag6NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag6Name = PropertiesEntryBitFlag6Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        private void Bitflag7NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag7Name = PropertiesEntryBitFlag7Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        private void Bitflag8NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EntryClass.EntryTypeBitFlag.BitFlag8Name = PropertiesEntryBitFlag8Name.Text;
                UpdateEntryName(EntryClass);
            }
        }

        
    }

    public class NumberCount
    {
        public int Number { get; set; }
        public int Count { get; set; }
        public List<int> RowIndices { get; set; } = new List<int>();
        public string RowIndicesAsString
        {
            get
            {
                return string.Join(", ", RowIndices);
            }
        }
    }

}