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
using PixelWPF;
using WpfHexaEditor;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for StandardEditor.xaml
    /// </summary>
    public partial class StandardEditor : UserControl
    {
        Workshop TheWorkshop { get; set; }
        WorkshopData MyDatabase { get; set; }
        Editor EditorClass { get; set; }

        //////////////////// NOTE: THE CODE FOR THE DESCRIPTION TEXTBOX PART OF THE EDITOR CODE IS ACTUALLY IN THE LEFTBAR CODE, AND KIND OF THE CHARACTER SET MANAGER. ///////
        //////////////////// Also I now generate a new textbox every time the item in the item list changes, although, i may move it back here in the future.  ///////
        public StandardEditor(WorkshopData Database, Editor editor)
        {
            InitializeComponent();
            TheWorkshop = Database.WorkshopXaml;
            MyDatabase = Database; //Sets the database to the one passed in.

            if (editor.EditorBackPanel != null) //If this UI already exists, we delete it. 
            {
                Database.WorkshopXaml.MidGrid.Children.Remove(editor.EditorBackPanel);
            }

            editor.EditorBackPanel = BackPanel;
            editor.StandardEditorData.EditorDescriptionsPanel = DescriptionsPanel;
            editor.StandardEditorData.MainDockPanel = EditorsPanel;

            Database.WorkshopXaml.MidGrid.Children.Add(this);

            editor.StandardEditorData.TheXaml = this;
            Grid.SetColumnSpan(this, 3); //This makes the standard editor take up all three columns of the main grid.


            //I forget what this is even doing, but it's a foreach loop using "for" so i can remove itself if it's invalid a little later here. 
            for (int i = 0; i < editor.StandardEditorData.DescriptionTableList.Count; i++) 
            {
                var DescriptionTable = editor.StandardEditorData.DescriptionTableList[i];

                if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.DataFile)
                {
                    if (DescriptionTable.Start == 0 || DescriptionTable.RowSize == 0 || DescriptionTable.TextSize == 0 || DescriptionTable.FileTextTable == null || DescriptionTable.FileTextTable.FileLocation == null)
                    {
                        // Remove and break
                        editor.StandardEditorData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }
                if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.TextFile)
                {
                    if (DescriptionTable.FileTextTable == null || DescriptionTable.FileTextTable.FileLocation == null)
                    {
                        // Remove and break
                        editor.StandardEditorData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }
            }




            StandardEditorSetup TheSetup = new StandardEditorSetup();
            TheSetup.SetupStandardEditor(Database, editor); //Create a editor with this information.
            //editor.StandardEditorData.EditorLeftDockPanel = this.LeftBar;
            LeftBar.LeftBarSetup(TheWorkshop, Database, editor); //Sets up the left bar with the workshop and database info.

            

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

            

            Scroll scroll = new(TheScrollviewer);
            TheScrollviewer.PreviewMouseWheel += scroll.ScrollViewer_PreviewMouseWheel;
            TheScrollviewer.PreviewMouseDown += scroll.ScrollViewer_PreviewMouseDown;
            TheScrollviewer.PreviewMouseUp += scroll.ScrollViewer_PreviewMouseUp;
            TheScrollviewer.PreviewMouseMove += scroll.ScrollViewer_PreviewMouseMove;
            //EditorsPanel.PreviewMouseWheel += scroll.ScrollViewer_PreviewMouseWheel;
            //EditorsPanel.PreviewMouseDown += scroll.ScrollViewer_PreviewMouseDown;
            //EditorsPanel.PreviewMouseUp += scroll.ScrollViewer_PreviewMouseUp;
            //EditorsPanel.PreviewMouseMove += scroll.ScrollViewer_PreviewMouseMove;

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

            foreach (var editor in MyDatabase.GameEditors)
            {
                if (editor.Value.EditorType == "DataTable")
                {
                    //GetALLTreeViewItems
                    //foreach (TreeViewItem TreeItem in editor.Value.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                    foreach (TreeViewItem TreeItem in LibraryGES.GetALLTreeViewItems(editor.Value.StandardEditorData.EditorLeftDockPanel.TreeView))
                    {
                        TheWorkshop.ItemNameBuilder(TreeItem);
                    }
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
            TheWorkshop.UpdateLeftBars();
        }

        private void ToggleEntrySynbology(object sender, RoutedEventArgs e)
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
            TheWorkshop.UpdateEntryDecorationsForAllEditors();
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
            TheWorkshop.UpdateEntryDecorationsForAllEditors();
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
            

            foreach (Editor editor in MyDatabase.GameEditors.Values)  //Set color
            {
                if (editor.EditorType == "DataTable")
                {
                    if (LibraryGES.EntryAddressType == "Decimal")
                    {
                        editor.StandardEditorData.TheXaml.EntryAddressTypeButton.Content = "Dec";
                    }
                    else if (LibraryGES.EntryAddressType == "Hex")
                    {
                        editor.StandardEditorData.TheXaml.EntryAddressTypeButton.Content = "Hex";
                    }
                }
            }

            TheWorkshop.UpdateEntryDecorationsForAllEditors();
        }

        private void EntryOffsetTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TheWorkshop.UpdateEntryDecorationsForAllEditors();
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
            TheWorkshop.UpdateEntryDecorationsForAllEditors();

            foreach (Editor editor in MyDatabase.GameEditors.Values)  //Set color
            {
                if (editor.EditorType == "DataTable") 
                {
                    if (LibraryGES.ShowHiddenEntrys == true)
                    {
                        editor.StandardEditorData.TheXaml.EntryHiddenToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC1E40"));
                    }
                    else if (LibraryGES.ShowHiddenEntrys == false)
                    {
                        editor.StandardEditorData.TheXaml.EntryHiddenToggle.Foreground = Brushes.Gray;
                    }                    
                }
            }

            if (TheWorkshop.IsPreviewMode == true)
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

            foreach (Editor editor in MyDatabase.GameEditors.Values)  //Set color
            {
                if (editor.EditorType == "DataTable")
                {
                    if (LibraryGES.DebugShowALL == true)
                    {
                        editor.StandardEditorData.TheXaml.DebugShowALL.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E70EC"));
                    }
                    else if (LibraryGES.DebugShowALL == false)
                    {
                        editor.StandardEditorData.TheXaml.DebugShowALL.Foreground = Brushes.Gray;
                    }
                }
            }

            TheWorkshop.UpdateEntryDecorationsForAllEditors();
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

        private void OpenDescriptionManager(object sender, RoutedEventArgs e)
        {            
            TextSourceManager TheManager = new TextSourceManager();
            MainGrid.Children.Add(TheManager);
            TheManager.Width = MainGrid.ActualWidth;

            TheManager.SetupForDescription(MyDatabase);
        }
    }
}
