using Microsoft.VisualBasic;
using PixelWPF;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using WpfHexEditor;
using static GameEditorStudio.Entry;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for StandardEditor.xaml
    /// </summary>
    public partial class DataTableEditor : UserControl
    {
        Workshop WorkshopXaml { get; set; }
        WorkshopData WorkshopData { get; set; }
        DataTableEditorData DTEData { get; set; }

        //////////////////// NOTE: THE CODE FOR THE DESCRIPTION TEXTBOX PART OF THE EDITOR CODE IS ACTUALLY IN THE LEFTBAR CODE, AND KIND OF THE CHARACTER SET MANAGER. ///////
        //////////////////// Also I now generate a new textbox every time the item in the item list changes, although, i may move it back here in the future.  ///////
        public DataTableEditor(WorkshopData Database, Editor editor)
        {
            InitializeComponent();

            WorkshopXaml = Database.WorkshopXaml;            
            WorkshopData = Database; //Sets the database to the one passed in.
            DTEData = editor as DataTableEditorData;

            DebugShowALL.Visibility = Visibility.Collapsed; //Blue eyeball
            DebugUpdateALLGrid.Visibility = Visibility.Collapsed; //grid force update button.
            TranslationToggle.Visibility = Visibility.Collapsed; //Translation panel toggle button.
            EntryVisibilityToggle.Visibility = Visibility.Collapsed; //Clean UI mode
            #if DEBUG
            DebugUpdateALLGrid.Visibility = Visibility.Visible; 
            DebugShowALL.Visibility = Visibility.Visible;
            TranslationToggle.Visibility = Visibility.Visible;
            EntryVisibilityToggle.Visibility = Visibility.Visible; 
            #endif


            DTEData.EditorLeftBar.DataTableEditorData = DTEData;
            RightBar.DTEData = DTEData;
            DTEData.EditorRightBar = RightBar;

            RightBar.TheDocumentsUserControl.TheWorkshop = DTEData.WorkshopXaml;
            RightBar.TheDocumentsUserControl.WorkshopData = DTEData.WorkshopXaml.WorkshopData;


            LibraryGES.SetGridData(this);

            

            editor.DataTableEditorData.EditorDescriptionsPanel = DescriptionsPanel;
            editor.DataTableEditorData.MainDockPanel = EditorsPanel;

            
            

            editor.DataTableEditorData.DTEXaml = this;
            Grid.SetColumnSpan(this, 3); //This makes the standard editor take up all three columns of the main grid.


            //Removes / deletes invalid description tables. 
            { 
                //Disabled for now. If i ever re-enable this...
                //1: Needs to change to happen during LoadProject because it triggers here (when first loading the workshop) (So EVERY description table is seen as invalid)
                //2: I should add a notification so the user knows this editor's description table was removed for being invalid.  Clarity is everything!
                //3: I previously noted that DecodeDescriptions also trys to remove invalid description tables. Not sure if thats true anymore through... 


                //for (int i = 0; i < editor.DataTableEditorData.DescriptionTableList.Count; i++)
                //{
                //    var DescriptionTable = editor.DataTableEditorData.DescriptionTableList[i];

                //    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                //    {
                //        //It's extremely unlikely any description table would ever start at byte 0.
                //        //if (DescriptionTable.TextTableStart == 0 || DescriptionTable.TextTableRowSize == 0 || DescriptionTable.TextTableCharLimit == 0 || DescriptionTable.TextTableFile == null || DescriptionTable.TextTableFile.FileLocation == null)
                //        //{
                //        //    // Remove and break
                //        //    editor.DataTableEditorData.DescriptionTableList.RemoveAt(i);
                //        //    continue;
                //        //}
                //    }
                //    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                //    {
                //        //if (DescriptionTable.TextTableFile == null || DescriptionTable.TextTableFile.FileLocation == null)
                //        //{
                //        //    // Remove and break
                //        //    editor.DataTableEditorData.DescriptionTableList.RemoveAt(i);
                //        //    continue;
                //        //}
                //    }
                //    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                //    {
                //        continue; //Make something later.
                //    }
                //}
            }
            



            //Making sure the current user settings are displayed.
            if (LibraryGES.ShowHiddenEntrys == true)
            {
                EntryHiddenToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC1E40"));
            }
            else
            {
                EntryHiddenToggle.Foreground = Brushes.Gray;
            }
            if (LibraryGES.ShowHiddenEntrys == true)
            {
                DebugShowALL.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E70EC"));
            }
            else if (LibraryGES.ShowHiddenEntrys == false)
            {
                DebugShowALL.Foreground = Brushes.Gray;
            }
            if (LibraryGES.EntrysDropAbove == true)
            {
                UpDownToggleButton.Content = "^^";
            }
            if (LibraryGES.EntrysDropAbove == false)
            {
                UpDownToggleButton.Content = "vv";
            }



            //Scroll scroll = new(TheScrollviewer);
            //TheScrollviewer.PreviewMouseWheel += scroll.ScrollViewer_PreviewMouseWheel;
            //TheScrollviewer.PreviewMouseDown += scroll.ScrollViewer_PreviewMouseDown;
            //TheScrollviewer.PreviewMouseUp += scroll.ScrollViewer_PreviewMouseUp;
            //TheScrollviewer.PreviewMouseMove += scroll.ScrollViewer_PreviewMouseMove;

            //EditorsPanel.PreviewMouseWheel += scroll.ScrollViewer_PreviewMouseWheel;
            //EditorsPanel.PreviewMouseDown += scroll.ScrollViewer_PreviewMouseDown;
            //EditorsPanel.PreviewMouseUp += scroll.ScrollViewer_PreviewMouseUp;
            //EditorsPanel.PreviewMouseMove += scroll.ScrollViewer_PreviewMouseMove;

            Database.WorkshopXaml.MidGrid.Children.Remove(editor.EditorVisual); // was a line from when i thought i would rebuild a DTE when changing tables. But it doesn't cause problems so i left it in.
            editor.EditorVisual = this;
            Database.WorkshopXaml.MidGrid.Children.Add(this);

            TabButtonMaker MakeEditorButton = new();
            MakeEditorButton.CreateEditorTab(editor);

            //DataTableEditorData.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));



            GenerateUI();
        }

        public void GenerateUI() //This generates / builds / REbuilds data about this editor.
        {
            DTESetup TheSetup = new DTESetup();
            TheSetup.SetupDataTableEditorMiddle(WorkshopData, DTEData); //Create a editor with this information.
            LeftBar.SetupDataTableEditorLeftBar(WorkshopData, DTEData); //Sets up the left bar with the workshop and database info.
            
            {   //Descriptions Stuff. (Maybe replace with an UpdateDescriptions Method?)
                //the method would... ehhhhh? okay maybe not... also setting hidden entrys wouldn't even be a part of this (probably?). 
                DTEData.DTEXaml.DescriptionsBottomBar.Visibility = Visibility.Collapsed;
                if (DTEData.DescriptionTableList != null)
                {
                    if (DTEData.DescriptionTableList.Count != 0)
                    {
                        if (DTEData.NameTable != null)
                        {
                            DTEData.DTEXaml.DescriptionsBottomBar.Visibility = Visibility.Visible;
                        }
                    }
                }
            }

            //Right bar stuff is in Editor Tab Stuff for some fucking reason (???)

            TabButtonMaker EditorTabStuff = new();
            EditorTabStuff.UpdateEditorRightClickMenu(DTEData);
            //I should make a UpdateEditorTabRightClickMenu method and have it happen here
        }

        public void ProjectLoadDTETables(Project projectdata) 
        {
            
            SpreadsheetToggle.IsEnabled = true; 
            if ( projectdata == null) { SpreadsheetToggle.IsEnabled = false; } //DTEData.NameTable == null || DTEData.DataTable == null ||

            //Name Table
            if (DTEData.NameTable != null) 
            {
                //if (projectdata == null) { DTEData.NameTable.TextTableFile = null; }
                //if (DTEData.NameTable.TextTableFile == null) { DTEData.NameTable.TextTableFile = null; }

                DTEData.NameTable.TextTableFile = LibraryGES.GetGameFileUsingLocation(WorkshopData, DTEData.NameTable.GameFileLocation);

                if (DTEData.NameTable.TextTableLinkType != TextTable.TextTableLinkTypes.Nothing)
                {
                    CharacterSetManager CharacterManager = new();
                    CharacterManager.DecodeAllItemNames(DTEData);
                }
                //DTEData.EditorLeftBar. ;
            }
            
            //Data Table
            if (DTEData.DataTable != null) 
            {
                DTEData.DataTable.FileDataTable = LibraryGES.GetGameFileUsingLocation(WorkshopData, DTEData.DataTable.GameFileLocation);

                foreach (Entry entry in DTEData.MasterEntryList)
                {                    
                    if (entry.NewSubType == Entry.EntrySubTypes.Menu)
                    {
                        EntryTypeMenu menu = entry.EntryTypeMenu;
                        if (menu.TextTableDataFile != null) { menu.TextTableDataFile.TextTableFile = LibraryGES.GetGameFileUsingLocation(WorkshopData, menu.TextTableDataFile.GameFileLocation); }
                        if (menu.TextTableTextFile != null) { menu.TextTableTextFile.TextTableFile = LibraryGES.GetGameFileUsingLocation(WorkshopData, menu.TextTableTextFile.GameFileLocation); }
                        //if (menu.TextTableNothing != null) { menu.TextTableNothing.TextTableFile = LibraryGES.GetGameFileUsingLocation(WorkshopData, menu.TextTableNothing.GameFileLocation); }
                        //if (menu.TextTableEditor != null) { menu.TextTableEditor.TextTableFile = LibraryGES.GetGameFileUsingLocation(WorkshopData, menu.TextTableEditor.GameFileLocation);  }
                        //if (menu.TextTableAdvanced != null) { }
                    }
                }
            }
            
            //Description Table
            if (DTEData.DescriptionTableList.Count != 0) 
            {
                DTEData.DescriptionTableList[0].TextTableFile = LibraryGES.GetGameFileUsingLocation(WorkshopData, DTEData.DescriptionTableList[0].GameFileLocation);
            }
        }

        private void ToggleItemIDNumberVisibility(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowItemIndex == false)
            {
                LibraryGES.ShowItemIndex = true;
            }
            else if (LibraryGES.ShowItemIndex == true)
            {
                LibraryGES.ShowItemIndex = false;
            }

            foreach (DataTableEditorData datatabledata in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                foreach (TreeViewItem TreeItem in LibraryGES.GetALLTreeViewItems(datatabledata.DataTableEditorData.EditorLeftBar.TreeView))
                {
                    ItemNameBuilder(TreeItem);
                }

            }
        }

        private void ToggleTranslationPanel(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowTranslationPanel == true)
            {
                LibraryGES.ShowTranslationPanel = false;
            }
            else if (LibraryGES.ShowTranslationPanel == false)
            {
                LibraryGES.ShowTranslationPanel = true;
            }

            foreach (DataTableEditorData editor in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                if (LibraryGES.ShowTranslationPanel == true)
                {
                    TheLeftBar asdas = editor.DataTableEditorData.EditorLeftBar.LeftBarXaml as TheLeftBar;
                    asdas.TranslationsPanelBorder.Visibility = Visibility.Visible;
                    //TranslationsPanelBorder
                }
                else if (LibraryGES.ShowTranslationPanel == false)
                {
                    TheLeftBar asdas = editor.DataTableEditorData.EditorLeftBar.LeftBarXaml as TheLeftBar;
                    asdas.TranslationsPanelBorder.Visibility = Visibility.Collapsed;
                }

            }

        }

        private void ToggleEntrySymbology(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowSymbology == true)
            {
                LibraryGES.ShowSymbology = false;
            }
            else if (LibraryGES.ShowSymbology == false)
            {
                LibraryGES.ShowSymbology = true;
            }

            {   //This is the Entry ID toggle. I'm merging it into the symbology toggle because it makes sense to have them together.
                if (LibraryGES.ShowEntryAddress == true)
                {
                    LibraryGES.ShowEntryAddress = false;
                }
                else if (LibraryGES.ShowEntryAddress == false)
                {
                    LibraryGES.ShowEntryAddress = true;
                }
            }
            DTEMethods.UpdateHotbarForAllDTEEditors(WorkshopData);
            UpdateEntryDecorationsForAllEditors();
        }

        private void EntryAddressToggle(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowEntryAddress == true)
            {
                LibraryGES.ShowEntryAddress = false;
            }
            else if (LibraryGES.ShowEntryAddress == false)
            {
                LibraryGES.ShowEntryAddress = true;
            }
            UpdateEntryDecorationsForAllEditors();
        }

        private void EntryAddressType(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.EntryAddressType == "Decimal")
            {
                LibraryGES.EntryAddressType = "Hex";
                EntryAddressTypeButton.Content = "Hex";
            }
            else if (LibraryGES.EntryAddressType == "Hex")
            {
                LibraryGES.EntryAddressType = "Decimal";
                EntryAddressTypeButton.Content = "Dec";
            }
            else
            {
                LibraryGES.EntryAddressType = "Decimal";
                EntryAddressTypeButton.Content = "Dec";
            }
            DTEMethods.UpdateHotbarForAllDTEEditors(WorkshopData);
            UpdateEntryDecorationsForAllEditors();
        }

        private void EntryOffsetTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateEntryDecorationsForAllEditors();
            }
        }

        private void ToggleHiddenEntrys(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowHiddenEntrys == true)
            {
                LibraryGES.ShowHiddenEntrys = false;
            }
            else if (LibraryGES.ShowHiddenEntrys == false)
            {
                LibraryGES.ShowHiddenEntrys = true;
            }
            DTEMethods.UpdateHotbarForAllDTEEditors(WorkshopData);
            UpdateEntryDecorationsForAllEditors();
            

            if (WorkshopXaml.IsPreviewMode == true)
            {
                PixelWPF.LibraryPixel.Notification("Heyo~", "I didn't bother programming preview mode to " +
                "properly hide entrys / bytes that are in use by text. I will figure it out some other time. " +
                "Just be aware the workshop owner probably intends some of the entrys to be hidden. Sorry~");
            }
        }

        private void ToggleDebugShowALL(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.DebugShowALL == true)
            {
                LibraryGES.DebugShowALL = false;
            }
            else if (LibraryGES.DebugShowALL == false)
            {
                LibraryGES.DebugShowALL = true;
            }
            DTEMethods.UpdateHotbarForAllDTEEditors(WorkshopData); 
            UpdateEntryDecorationsForAllEditors();
        }

        private void HelpButtonClicked(object sender, RoutedEventArgs e)
        {
            PixelWPF.LibraryPixel.Notification("Helpful Info", "" +
                "- When your editor is first made, every byte will by in a giant vertical list of cube looking things that we call \"Entrys\". Each entry represents some data, and it will be on you to name them (such as Max HP, Str, EXP, Damage type, weapon type, etc). To name an entry, click it, then type a name on the right panel, and press ENTER." +
                "\n\n" +
                "\n- In general, most textboxes require you to press enter to make the change actually happen. The only exception is the value of an entry, and when using a text editor. Make sure you remember this and drill it into your brain!  " +
                "\n\n" +
                "\n- You can move entrys around to organize them, by clicking and dragging them. There is no visual for this yet, but when you release the mouse it will move. If you drop an entry over another entry, it will move to appear under it. If you drop an entry in the empty space to the right, the entry will move into a new column. You can also right click an entry and tell it to make a new column between two existing ones. " +
                "\n\n" +
                "\n- You can also move entrys in bulk. To more multiple entrys at once, first select an entry. Then hold left shift, and click drag from another entry in the samn column. It won't look like your even selecting more then 1 entry, but when you drop them somewhere, all the entrys between the one you first selected, and the one you grabbed last, will all move. I know i really need to make this give more visual feedback, but i swear programming visuals is SO VERY NOT MY THING, like it's seriously hard. Just know you *can* bulk move entrys.  " +
                "\n\n" +
                "\n- On the left side of an editor is the list of whatever the editor is for. In this program, every \"thing\" in that list is called an \"item\". For example, all the weapons in a weapons editor are called \"items\". For an enemy editor, all the enemys are called \"items\", etc. Basically, an \"Item\" is any one thing in a list. " +
                "\n\n" +
                "\n- Just like with entrys, you can click & drag items in the list to reorder them (and again, there is no visual yet). Releasing the mouse over another item moves your selected item under it. You can also right click an item in the list and create a folder. This is great to categorize items, like \"Swords\", \"Magic Classes\", or \"Enemys who first appear in the first dungeon\". Note that when a editor saves any changes back to the game files, the items are saved in their ORIGONAL order. That is to say, when you reorder items with this program, it does not actually change the order they are in inside the game files. This lets you work with them in a order you want, without breaking the order the game wants them in. But it also means I don't allow you to change their ingame order. " +
                "\n\n" +
                "\n- You can write personal notes for items at the bottom left of an editor. Notes do not save to game files, but are displayed in the editor next to an item's name (In orange text). They are useful, for example, if two weapons have the same name, then you can note them as \"Fake Ultima Blade\" and \"Real Ultima Blade\" or something.  " +
                "\n\n" +
                "\n- Finally, whenever you make a new editor like this, the \"Symbology\" system is automatically toggled on. You can toggle it on/off using the magnifying glass icon on the hotbar at the top of an editor. When symbology is ON, symbols with diffrent colors will appear on the left side of every entry. They try to give useful hints for what each entry (aka byte) could represent. For example, if an entry is only ever the values 0 or 1 across the entire editor (aka data table) then a symbol for a golden checkmark appears, indicating it's probably a checkbox type (aka a flag), such as a \"Is Female\" or \"Can Equip Bows\" flags. You can mouseover a symbol and it will give you a tooltip explaining what it means. " +
                "\n\n" +
                "\n- If you have any questions about the program, you can join my discord and ask away. In the future, i will try and make a \"wiki\" feature, that both teaches reverse engineering and how to guess what each entry represents, as well as letting users create their own pages for other users. " +
                "\n\n" +
                "\nAnyway, GOODLUCK! :D - HAVE FUN MAKING YOUR EDITOR!!! ");
        }


        private void UpdateALLGrids(object sender, RoutedEventArgs e)
        {
            DTEMethods.UpdateEditorGrids(DTEData);            
        }




    

        public void UpdateEntryDecorationsForAllEditors()
        {
            
            foreach (DataTableEditorData DataTableData in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                //if (LibraryGES.ShowSymbology == true)
                //{
                //    DataTableData.DTEXaml.EntrySymbologyToggle.Foreground = Brushes.White;

                //}
                //else if (LibraryGES.ShowSymbology == false)
                //{
                //    DataTableData.DTEXaml.EntrySymbologyToggle.Foreground = Brushes.Gray;
                //}

                //Toggle the visibility of Symbology and Entry Address Prefixs.
                foreach (Entry entry in DataTableData.MasterEntryList)
                {
                    if (LibraryGES.ShowSymbology == true)
                    {
                        entry.Symbology.Visibility = Visibility.Visible;

                    }
                    else if (LibraryGES.ShowSymbology == false)
                    {
                        entry.Symbology.Visibility = Visibility.Collapsed;
                    }

                    if (LibraryGES.ShowEntryAddress == true)
                    {
                        entry.EntryPrefix.Visibility = Visibility.Visible;
                    }
                    else if (LibraryGES.ShowEntryAddress == false)
                    {
                        entry.EntryPrefix.Visibility = Visibility.Collapsed;
                    }

                    try
                    {
                        if (LibraryGES.EntryAddressType == "Decimal") //Properties.Settings.Default.EntryPrefix = "Row Offset - Decimal Starting at 0";
                        {
                            entry.EntryPrefix.Content = entry.RowOffset + int.Parse(DataTableData.DTEXaml.EntryAddressOffsetTextbox.Text);
                        }
                        else if (LibraryGES.EntryAddressType == "Hex") //Properties.Settings.Default.EntryPrefix = "Row Offset - Hex Starting at 0x00";
                        {
                            entry.EntryPrefix.Content = (entry.RowOffset + int.Parse(DataTableData.DTEXaml.EntryAddressOffsetTextbox.Text)).ToString("X");   //.ToString("X")); // + int.Parse(EntryAddressOffsetTextbox.Text)).ToString("X");
                        }
                    }
                    catch
                    {

                    }

                }

                //Now we deal with Hidden Entrys.
                //1: Hide all CATEGORYS visibility. (Not just contents, the entire category)
                //2: Hide all COLUMNS and GROUPS.
                //3: If a column is empty, it does not get hidden. (For old drag drop code?)
                //foreach (var cat in editor.StandardEditorData.CategoryList)
                //{
                //    if (cat.ColumnList.Count != 0) if (cat.ColumnList[0].ItemBaseList.Count != 0) { cat.CatBorder.Visibility = Visibility.Collapsed; }

                //    foreach (var column in cat.ColumnList)
                //    {
                //        column.ColumnPanel.Visibility = Visibility.Collapsed;
                //        foreach (Group group in column.ItemBaseList.OfType<Group>())
                //        {
                //            group.GroupPanel.Visibility = Visibility.Collapsed;
                //        }
                //        if (column.ItemBaseList.Count == 0)
                //        {
                //            column.ColumnPanel.Visibility = Visibility.Visible;
                //        }
                //    }
                //}
                foreach (var cat in DataTableData.CategoryList)
                {
                    cat.CatBorder.Visibility = Visibility.Collapsed;

                    foreach (var Group in cat.GridItems)
                    {
                        Group.Visual.Visibility = Visibility.Collapsed;
                    }
                }


                //Code for showing entrys (and making their parents visable)
                //Show all allowed entrys, including any *Hidden* Entrys.
                foreach (Entry entry in DataTableData.MasterEntryList)
                {
                    if (entry.IsMerged == true) { continue; }

                    //Show all normal entrys (+parents). Does not show hidden or merged. 
                    if (LibraryGES.ShowHiddenEntrys == false)
                    {
                        if (entry.IsEntryHidden == true || entry.IsTextInUse == true || entry.Bytes == 0)
                        {
                            entry.Visual.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            entry.Visual.Visibility = Visibility.Visible;
                            if (entry.ParentGroup != null) { entry.ParentGroup.Visual.Visibility = Visibility.Visible; }

                            entry.ParentCategory.CatBorder.Visibility = Visibility.Visible;
                        }
                    }
                    //Show all Normal and Hidden entrys (+ parents). Does not show merged.
                    else if (LibraryGES.ShowHiddenEntrys == true && entry.Bytes != 0)
                    {
                        entry.Visual.Visibility = Visibility.Visible;
                        if (entry.ParentGroup != null) { entry.ParentGroup.Visual.Visibility = Visibility.Visible; }

                        entry.ParentCategory.CatBorder.Visibility = Visibility.Visible;
                    }
                }





            }

            if (LibraryGES.DebugShowALL == true)
            {
                DEBUGSHOWALL(); //Show all normal, hidden, and merged entrys.
            }

        }


        public void DEBUGSHOWALL()
        {
            foreach (DataTableEditorData DataTableData in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {               

                foreach (var row in DataTableData.CategoryList)
                {
                    row.CatBorder.Visibility = Visibility.Visible;

                    foreach (GridItem gitem in row.GridItems)
                    {
                        gitem.Visual.Visibility = Visibility.Visible;
                    }

                    //foreach (var column in row.ColumnList)
                    //{
                    //    column.ColumnPanel.Visibility = Visibility.Visible;

                    //    foreach (Group group in column.ItemBaseList.OfType<Group>())
                    //    {
                    //        group.GroupPanel.Visibility = Visibility.Visible;
                    //    }
                    //}
                }

                foreach (Entry entry in DataTableData.MasterEntryList)
                {
                    entry.EntryBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void ScrollSpeed(object sender, MouseWheelEventArgs e) //on mouse wheel for editor scollviewer
        {
            // I crashed once during preview mode. Probably a WPF bug out of my hands.
            // Anyway this is now in a try-catch just incase...
            try
            {
                {   //Do not eat the scroll input if hovering a combobox. 
                    DependencyObject hit = e.OriginalSource as DependencyObject;
                    while (hit != null && !(hit is ComboBox) && !(hit is ScrollViewer))
                    {
                        hit = System.Windows.Media.VisualTreeHelper.GetParent(hit);
                    }
                    if (hit is ComboBox combo && combo.IsDropDownOpen)
                    {
                        return; // DO NOT set e.Handled = true
                    }
                    if (hit is ScrollViewer nested && nested != sender)
                    {// Safety: If there is a nested ScrollViewer (like a multi-line textbox)
                        return;
                    }
                }

                ScrollViewer scv = (ScrollViewer)sender;
                // *1.0 is definalty not the default speed, im guessing its like *0.5 or something.
                double scrollTarget = scv.VerticalOffset - (e.Delta * 0.65);

                scv.ScrollToVerticalOffset(scrollTarget);
            } 
            catch 
            { 
            
            }
            
            e.Handled = true;
        }


        private void UpDownToggle(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.EntrysDropAbove == true)
            {
                LibraryGES.EntrysDropAbove = false;

            }
            else if (LibraryGES.EntrysDropAbove == false)
            {
                LibraryGES.EntrysDropAbove = true;
            }

            foreach (var editor in WorkshopData.GameEditors) 
            {
                if (LibraryGES.EntrysDropAbove == true)
                {
                    UpDownToggleButton.Content = "^^";
                }
                if (LibraryGES.EntrysDropAbove == false)
                {
                    UpDownToggleButton.Content = "vv";
                }
            }
                
        }

        private void ToggleEntryFrameVisibility(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.EntryFrameIsVisible == true)
            {
                LibraryGES.EntryFrameIsVisible = false;

            }
            else if (LibraryGES.EntryFrameIsVisible == false)
            {
                LibraryGES.EntryFrameIsVisible = true;
            }

            foreach (DataTableEditorData DataTableData in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {                

                foreach (Category category in DataTableData.CategoryList) 
                {
                    if (LibraryGES.EntryFrameIsVisible == true)
                    {                        
                        category.ItemGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121216")); //.ClearValue(Grid.BackgroundProperty);                        
                    }
                    else if (LibraryGES.EntryFrameIsVisible == false)
                    {
                        category.ItemGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                    }
                }

                foreach (Entry entry in DataTableData.MasterEntryList)
                {
                    if (LibraryGES.EntryFrameIsVisible == true)
                    {
                        entry.EntryBorder.ClearValue(Border.BorderBrushProperty);
                        entry.EntryBorder.ClearValue(Border.BackgroundProperty);
                    }
                    else if (LibraryGES.EntryFrameIsVisible == false)
                    {
                        entry.EntryBorder.BorderBrush = Brushes.Transparent;
                        entry.EntryBorder.Background = Brushes.Transparent;
                    }
                }
            }
            
        }





        public void ItemNameBuilder(TreeViewItem TreeItem)
        {
            TextInfo ItemInfo = TreeItem.Tag as TextInfo;
            TextBlock TextBlockItem = new TextBlock();

            if (ItemInfo.IsFolder == false)
            {
                if (LibraryGES.ShowItemIndex == true)
                {
                    Run RunIndex = new Run();
                    RunIndex.Text = (ItemInfo.ItemIndex + DTEData.NameTable.TextTableFirstNameID) + ": ";
                    TextBlockItem.Inlines.Add(RunIndex);
                }


            }

            if (ItemInfo.IsFolder == true)
            {
                Run RunFolder = new Run();
                RunFolder.Foreground = Brushes.Yellow;
                RunFolder.Text = "📁";
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

            foreach (DataTableEditorData DataTableData in DTEData.WorkshopXaml.WorkshopData.GameEditors.OfType<DataTableEditorData>()) //Super quick and dirty way to update entrys from other editors that are using this editors names. (Menu Link To Editor)
            {
                //this actually triggers for EVERY name item in an editor that is a name for entry X, IE this entire foreach loop is running 100+ times when loading a project...

                //if (WorkshopData.IsProjectLoaded == false) { break; } //Do not if project is unloaded. Helps prevent a crash i'm to lazy to fix right now. 
                //if (WorkshopData.IsProjectLoaded == true) { break; } //Do not if project is unloaded. Helps prevent a crash i'm to lazy to fix right now. 

                foreach (Entry entry in DataTableData.MasterEntryList)
                {
                    if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor && entry.EntryTypeMenu.TextTableEditor != null)
                    {
                        if (entry.EntryTypeMenu.TextTableEditor.LinkedDTEEditor != null)
                        {
                            if (entry.EntryTypeMenu.TextTableEditor.LinkedDTEEditor == DTEData)
                            {
                                if (entry.EntryByteDecimal == null) { return; } //This stops a blank from being created for a dropdown because its loading before the ByteDecimal is loaded, but also probably other misc problems.
                                                                                //IE i should totally not have this code chunk here, but im still to lazy to impliment this properly, so here goes this ultra bad answer i will regret later! :D
                                DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.ChangeEntryType(DTEData.WorkshopXaml.WorkshopData, EntrySubTypes.Menu, DTEData.WorkshopXaml, entry); //This might not even be the best way, or a good way, but i'm lazy atm and it fucking works.
                            }
                        }

                    }
                }
            }
        }

        private void ToggleSpreadsheet(object sender, RoutedEventArgs e)
        {
            Spreadsheet spreadsheet = new(WorkshopData, DTEData); //set column to 2
            Grid.SetColumn(spreadsheet, 0);
            Grid.SetColumnSpan(spreadsheet, 99);
            Grid.SetRow(spreadsheet, 0);
            Grid.SetRowSpan(spreadsheet, 99);

            EditorBack.Children.Add(spreadsheet); //

            //if (spreadsheet.IsVisible == false)
            //{
            //    spreadsheet.Show();
            //}
            //else if (spreadsheet.IsVisible == true)
            //{
            //    spreadsheet.Close();
            //}
        }
    }
}
