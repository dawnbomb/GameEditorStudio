using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Xml.Linq;
using Microsoft.VisualBasic;
using static GameEditorStudio.Entry;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for DTRightBar.xaml
    /// </summary>
    public partial class DTRightBar : UserControl
    {
        public DataTableEditorData DTEData { get; set; } //The Data Table Editor sets this on creation. It's never null. 

        public UserControlCrossReference TheCrossReference { get; set; }  
        public DocumentsUserControl TheDocumentsUserControl { get; set; }

        public string PreviousTabName { get; set; } = ""; //The right bar tab to return to when unfocusing from the listview in the list tab.

        public DTRightBar()
        {
            InitializeComponent();
            

            TheCrossReference = CrossReferenceInfo;
            TheDocumentsUserControl = DocumentsControl;

            ListTab.Visibility = Visibility.Collapsed;

            EditorTabItem.Visibility = Visibility.Collapsed;
            CategoryTabItem.Visibility = Visibility.Collapsed;
            GroupTabItem.Visibility = Visibility.Collapsed;
            EntryTabItem.Visibility = Visibility.Collapsed;

            EntryTab1.Visibility = Visibility.Collapsed;
            EntryTab2.Visibility = Visibility.Collapsed;
            EntryTab3.Visibility = Visibility.Collapsed;
            EntryTab4.Visibility = Visibility.Collapsed;

            foreach (EntrySubTypes type in Enum.GetValues(typeof(EntrySubTypes)))
            {
                PropertiesEntryType.Items.Add(new ComboBoxItem { Content = type.ToString() });
            }

        }






        /////////////////////////////////////////////////////////////////////     
        ////////////////////////////// EDITOR /////////////////////////////// 
        /////////////////////////////////////////////////////////////////////
        private void TextboxChangeEditorName(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (PropertiesTextboxEditorName.Text == "")
                {
                    PropertiesTextboxEditorName.Text = DTEData.EditorName;
                    return; //If the user tries to set the editor name to nothing, just return.
                }

                DTEData.EditorName = PropertiesTextboxEditorName.Text;
                DTEData.EditorTabNameLabel.Content = DTEData.EditorName;

                //UpdateEditorTab(EditorClass);
            }
        }

        private void OpenIconManager(object sender, RoutedEventArgs e)
        {
            //NOTE: I did a LOT of research on sprites sizes, and 60 is PERFECT for a editor icon size. 
            //it's very rare for icons to be 70+ that are hard to crop, and everything 30 under can be multiplied in size. 
            //While icons size 42 probably can't be cropped and doubled, it's close enough to be on-style.
            //meanwhile vs 50 max, not only do we get sprites size 50~60+ with cropping, but more small sprites can multiply their size to average out more effectivly. 


            //HIDEMOST();
            //UserControlEditorIcons TheUserControl = new UserControlEditorIcons();
            //MidGrid.Children.Add(TheUserControl);
            //UCGraphicsEditor = TheUserControl;




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

        private void OpenTextTableManagerForNameTable(object sender, RoutedEventArgs e)
        {
            TextTableManager TextTableManager = new TextTableManager();
            DTEData.DTEXaml.EditorBack.Children.Add(TextTableManager);
            Grid.SetRow(TextTableManager, 0);
            Grid.SetColumn(TextTableManager, 0);
            Grid.SetRowSpan(TextTableManager, 99);
            Grid.SetColumnSpan(TextTableManager, 99);


            TextTableManager.SetupForNames(DTEData);
        }

        private void EditorCharacterSetDropdownClose(object sender, EventArgs e)
        {
            if (DTEData.WorkshopXaml.IsPreviewMode == true) { return; }

            if (DTEData.NameTable.TextTableCharacterSet == PropertiesEditorNameTableCharacterSetDropdown.Text)
            {
                return;
            }


            DTEData.NameTable.TextTableCharacterSet = PropertiesEditorNameTableCharacterSetDropdown.Text;
            CharacterSetManager CharacterManager = new();
            CharacterManager.DecodeAllItemNames(DTEData);


            foreach (TreeViewItem TreeViewItem in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
            {
                TextInfo ItemInfo = TreeViewItem.Tag as TextInfo;
                DTEData.DTEXaml.ItemNameBuilder(TreeViewItem);

            }
        }

        private void ChangeNameTableRowSize(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool Valid = CheckValidDataTableInfo();
                if (Valid == false) { return; }

                int Num = int.Parse(PropertiesEditorNameTableRowSize.Text);

                if (Num < 0)
                {

                    PropertiesEditorNameTableRowSize.Text = DTEData.NameTable.TextTableRowSize.ToString();
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Name Row Size cannot be less then 0.",
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



                DTEData.NameTable.TextTableRowSize = Num;
                //CharacterSetAscii SetAscii = new();
                //SetAscii.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemNames(DTEData);
                foreach (TreeViewItem TreeViewItem in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
                {
                    TextInfo ItemInfo = TreeViewItem.Tag as TextInfo;
                    DTEData.DTEXaml.ItemNameBuilder(TreeViewItem);

                }
            }
        }

        private void ChangeNameTableTextSize(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool Valid = CheckValidDataTableInfo();
                if (Valid == false) { return; }

                int Num = int.Parse(PropertiesEditorNameTableTextSize.Text);

                if (Num < 0)
                {

                    PropertiesEditorNameTableTextSize.Text = DTEData.NameTable.TextTableCharLimit.ToString();
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Name Table Text Size cannot be less then 0",
                        "This value is how many letters / characters are being read from a file. " +
                        "Hopefully it is obvious why this number cannot be less then 0." +
                        "\n\n" +
                        "The textbox has been reset to it's previous value. "
                        );
                    return;
                }



                DTEData.NameTable.TextTableCharLimit = Num;
                //CharacterSetAscii SetAscii = new();
                //SetAscii.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemNames(DTEData);
                foreach (TreeViewItem TreeViewItem in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
                {
                    TextInfo ItemInfo = TreeViewItem.Tag as TextInfo;
                    DTEData.DTEXaml.ItemNameBuilder(TreeViewItem);

                }
            }
        }

        private void ChangeNameTableStartByte(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool Valid = CheckValidDataTableInfo();
                if (Valid == false) { return; }

                int Num = int.Parse(PropertiesEditorNameTableStartByte.Text);

                if (Num < 0)
                {

                    PropertiesEditorNameTableStartByte.Text = DTEData.NameTable.TextTableStart.ToString();
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Start byte cannot be less then 0",
                        "I'm not sure why you tried to do this, but it's obviously not allowed. " +
                        "\n\n" +
                        "If this was not a mistake and there is a reason i don't understand why this would ever be desired, " +
                        "you can tell me on discord and i'll consider not explicitly preventing this behavior." +
                        "\n\n" +
                        "The textbox has been reset to it's previous value. "
                        );

                    return;
                }




                DTEData.NameTable.TextTableStart = Num;
                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemNames(DTEData);

                foreach (TreeViewItem TreeViewItem in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
                {
                    TextInfo ItemInfo = TreeViewItem.Tag as TextInfo;
                    DTEData.DTEXaml.ItemNameBuilder(TreeViewItem);

                }
            }
        }
        private void ChangeNameTableFirstNameDisplayNumber(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int Num = int.Parse(PropertiesEditorFirstNameNumber.Text);

                if (Num < 0)
                {

                    PropertiesEditorFirstNameNumber.Text = DTEData.NameTable.TextTableFirstNameID.ToString();
                    PixelWPF.LibraryPixel.NotificationNegative("Error: First Display Number cannot be less then 0",
                        "I'm not sure why you tried to do this, but it's obviously not allowed. " +
                        "\n\n" +
                        "If this was not a mistake and there is a reason i don't understand why this would ever be desired, " +
                        "you can tell me on discord and i'll consider not explicitly preventing this behavior." +
                        "\n\n" +
                        "The textbox has been reset to it's previous value. "
                        );

                    return;
                }

                DTEData.NameTable.TextTableFirstNameID = Num;

                if (DTEData.NameTable.TextTableLinkType != TextTable.TextTableLinkTypes.Nothing) 
                {                    
                    CharacterSetManager CharacterManager = new();
                    CharacterManager.DecodeAllItemNames(DTEData);
                }
                

                foreach (TreeViewItem TreeViewItem in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
                {
                    TextInfo ItemInfo = TreeViewItem.Tag as TextInfo;
                    DTEData.DTEXaml.ItemNameBuilder(TreeViewItem);

                }
            }
        }
        private void ChangeNameTableNameCount(object sender, KeyEventArgs e)
        {
            //value = EntryClass.EntryEditor.SWData.FileDataTable.FileBytes[EntryClass.EntryEditor.SWData.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");

            if (e.Key == Key.Enter)
            {
                //DTEData.NameTable.NameTableItemCount = DTEData.NameTable.NameTableItemCount;
                //DTEData.DTEXaml.GenerateUI();
                //return;

                bool Valid = CheckValidDataTableInfo();
                if (Valid == false) { return; }


                //All counts are treated as -1, in order to make it so humans can better understand items. This way item 22 is the one with number 22, not 23. (Due to 0 being a number)
                int NewNameCount = Int32.Parse(PropertiesEditorNameCount.Text);
                int OldNameCount = DTEData.NameTable.TextTableItemCount;
                int Diffrence = NewNameCount - OldNameCount;
                if (NewNameCount < 1)
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Notice: Lol no.",
                    "As a precautionary measure against any accidents, attempting to delete ALL items is not allowed." +
                    "\n\n" +
                    "The textbox has been reset to it's previous value. "
                    );

                    PropertiesEditorNameCount.Text = OldNameCount.ToString();
                    return;
                }

                if (Diffrence > 0) //If adding more names
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
                        TextInfo ItemInfo = new();
                        TreeItem.Tag = ItemInfo;
                        ItemInfo.ItemIndex = i ;



                        DTEData.NameTable.ItemList.Add(ItemInfo);
                        DTEData.EditorLeftBar.TreeView.Items.Add(TreeItem);


                    }


                    //CharacterSetAscii Encoding = new();
                    //Encoding.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
                    CharacterSetManager CharacterManager = new();
                    CharacterManager.DecodeAllItemNames(DTEData);
                    foreach (TreeViewItem TreeViewItem in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
                    {
                        TextInfo ItemInfo = TreeViewItem.Tag as TextInfo;
                        DTEData.DTEXaml.ItemNameBuilder(TreeViewItem);

                    }

                    DTEData.NameTable.TextTableItemCount = NewNameCount;


                }
                else if (Diffrence < 0) //If less names (if removing items)
                {
                    //Delete treeview items that have offsets X~Y.
                    for (int i = Diffrence; i != 0; i++)
                    {
                        int Target = OldNameCount + i; //Here, i is a negative, so we add it for the new target number.
                        foreach (TreeViewItem Item in DTEData.EditorLeftBar.TreeView.Items)
                        {
                            TextInfo ItemInfo = Item.Tag as TextInfo;
                            if (ItemInfo.ItemIndex == Target)
                            {
                                DTEData.EditorLeftBar.TreeView.Items.Remove(Item);
                                DTEData.NameTable.ItemList.Remove(ItemInfo);
                                break;
                            }
                            if (ItemInfo.IsFolder == true)
                            {
                                foreach (TreeViewItem childItem in Item.Items)
                                {
                                    TextInfo childItemInfo = childItem.Tag as TextInfo;
                                    if (childItemInfo.ItemIndex == Target)
                                    {
                                        // If the child item has the target index, remove it
                                        Item.Items.Remove(childItem);
                                        DTEData.NameTable.ItemList.Remove(childItemInfo);
                                        break;
                                    }
                                }

                            }

                        }


                    }
                    DTEData.NameTable.TextTableItemCount = NewNameCount;

                }
            }
        }

        private void OpenNameTableInHxD(object sender, RoutedEventArgs e)
        {
            foreach (Tool tool in Database.Tools)
            {
                if (tool.Key == "638835886790069008-583706364-23567425")
                {
                    if (!File.Exists(tool.Location))
                    {
                        PixelWPF.LibraryPixel.Notification("HxD Hex Editor is not available!", "To use HxD Hex Editor with Game Editor Studio, go to the Tools menu and set the location of HxD Hex Editor. ");
                        return;
                    }

                    if (DTEData.NameTable.TextTableFile == null)
                    {
                        return;
                    }

                    string filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.NameTable.TextTableFile.FileLocation;
                    if (!File.Exists(filelocation))
                    {
                        filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.NameTable.TextTableFile.FileLocation;
                    }

                    if (!File.Exists(filelocation) || !File.Exists(tool.Location) || DTEData.NameTable.TextTableFile.FileLocation == null)
                    {
                        return;
                    }

                    MethodData methodData = new MethodData();
                    methodData.ResourceLocations.Add(tool.Location);
                    methodData.ResourceLocations.Add(filelocation);
                    CommandMethodsClass.RunProgramwithfile(methodData);

                    break;
                }
            }
        }

        private void OpenNameTableIn010(object sender, RoutedEventArgs e)
        {
            foreach (Tool tool in Database.Tools)
            {
                if (tool.Key == "638835886790068999-832829574-642210583")
                {
                    if (!File.Exists(tool.Location))
                    {
                        PixelWPF.LibraryPixel.Notification("010 Editor is not available!", "To use 010 Editor with Game Editor Studio, go to the Tools menu and set the location of 010 Editor. ");
                        return;
                    }


                    if (DTEData.NameTable.TextTableFile == null)
                    {
                        return;
                    }

                    string filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.NameTable.TextTableFile.FileLocation;
                    if (!File.Exists(filelocation))
                    {
                        filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.NameTable.TextTableFile.FileLocation;
                    }

                    if (!File.Exists(filelocation) || !File.Exists(tool.Location))
                    {
                        return;
                    }

                    MethodData methodData = new MethodData();
                    methodData.ResourceLocations.Add(tool.Location);
                    methodData.ResourceLocations.Add(filelocation);
                    CommandMethodsClass.RunProgramwithfile(methodData);

                    break;
                }
            }
        }

        private void OpenDataTableManagerForDataTable(object sender, RoutedEventArgs e)
        {
            if (DTEData.NameTable == null) 
            {
                PixelWPF.LibraryPixel.Notification("No Name Table","Please first set a name table. \n\nI actually did program in support for this, maybe i'll allow it later, but aside from crash protection, i just think creating a game editor flows better if you first set a name list. :)");
                return;
            }

            DataTableManager DataTableManager = new();
            DTEData.DTEXaml.EditorBack.Children.Add(DataTableManager);
            DataTableManager.SetupForDataTable(DTEData);
            
            Grid.SetRow(DataTableManager, 0);
            Grid.SetColumn(DataTableManager, 0);
            Grid.SetRowSpan(DataTableManager, 99);
            Grid.SetColumnSpan(DataTableManager, 99);
        }

        private void ChangeEditorMainTableStartByte(object sender, KeyEventArgs e)
        {
            if (DTEData.DataTable == null) { return; }

            if (e.Key == Key.Enter)
            {
                bool Valid = CheckValidDataTableInfo();
                if (Valid == false) { return; }

                int NewStart = Int32.Parse(PropertiesEditorTableStart.Text);
                int OldStart = DTEData.DataTable.DataTableStart;
                int NumMod = NewStart - OldStart;



                //This next ForEach part makes it so if any of the new final 3 bytes of what would be the new offsets are part
                //of a merged entry, that the table start change is canceled. 

                foreach (Entry entry in DTEData.MasterEntryList)
                {
                    //This makes a number called Hoi. Hoi is the new Row offset, if the editor actually changed the start byte of the file it's editing.
                    //if it underflows, it adds tablewidth. If it overflows, it removes tablewidth. So it's always a number inside GameTableSize.
                    int Hoi = entry.RowOffset + -NumMod;
                    if (Hoi < 0) { Hoi = Hoi + DTEData.DataTable.DataTableRowSize; }
                    if (Hoi > DTEData.DataTable.DataTableRowSize - 1) { Hoi = Hoi - DTEData.DataTable.DataTableRowSize; }

                    if (Hoi == DTEData.DataTable.DataTableRowSize - 1) //If the new Zero minus One'th entry is already part of a size 2 entry, cancel.
                    {
                        if (entry.Bytes == 2)
                        {
                            PropertiesEditorTableStart.Text = OldStart.ToString();
                            return;
                        }

                    }

                    //If the new Zero -1, or -2, or -3 entrys are already part of a size 4 entry, cancel.
                    if (Hoi == DTEData.DataTable.DataTableRowSize - 1 || Hoi == DTEData.DataTable.DataTableRowSize - 2 || Hoi == DTEData.DataTable.DataTableRowSize - 3)
                    {
                        if (entry.Bytes == 4)
                        {
                            PropertiesEditorTableStart.Text = OldStart.ToString();
                            return;
                        }

                    }
                }


                //We have not triggered a cancelation, and are now going to actually change the table start.

                DTEData.DataTable.DataTableStart = NewStart; //Changes the starting byte of the editor's table, to the new one the user wanted.
                foreach (Entry entry in DTEData.MasterEntryList)
                {
                    entry.RowOffset = entry.RowOffset + -NumMod;
                    if (entry.RowOffset < 0) { entry.RowOffset = entry.RowOffset + DTEData.DataTable.DataTableRowSize; }
                    if (entry.RowOffset > DTEData.DataTable.DataTableRowSize - 1) { entry.RowOffset = entry.RowOffset - DTEData.DataTable.DataTableRowSize; }
                    entry.EntryPrefix.Content = entry.RowOffset.ToString();

                    DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.LoadEntry(DTEData, entry);
                }

            }

            // +/- 1 to all offsets?
            // If NewOffset > RowSize: NewOffset - RowSize and reload ByteValue + Entry
        }

        private void OpenWorkshopInputButton(object sender, RoutedEventArgs e)
        {
            LibraryGES.OpenFileFolder(PropertiesEditorReadGameDataFrom.Text);
        }

        private void OpenWorkshopOutputButton(object sender, RoutedEventArgs e)
        {
            LibraryGES.OpenFileFolder(EditorOutputLocationTextbox.Text);
        }

        private void OpenDataTableInHxD(object sender, RoutedEventArgs e)
        {
            foreach (Tool tool in Database.Tools)
            {
                if (tool.Key == "638835886790069008-583706364-23567425")
                {
                    if (!File.Exists(tool.Location))
                    {
                        PixelWPF.LibraryPixel.Notification("HxD Hex Editor is not available!", "To use HxD Hex Editor with Game Editor Studio, go to the Tools menu and set the location of HxD Hex Editor. ");
                        return;
                    }

                    if (DTEData.DataTable.FileDataTable == null)
                    {
                        return;
                    }

                    string filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation;
                    if (!File.Exists(filelocation))
                    {
                        filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation;
                    }

                    if (!File.Exists(filelocation) || !File.Exists(tool.Location))
                    {
                        return;
                    }

                    MethodData methodData = new MethodData();
                    methodData.ResourceLocations.Add(tool.Location);
                    methodData.ResourceLocations.Add(filelocation);
                    CommandMethodsClass.RunProgramwithfile(methodData);

                    break;
                }
            }
        }

        private void OpenDataTableIn010(object sender, RoutedEventArgs e)
        {
            foreach (Tool tool in Database.Tools)
            {
                if (tool.Key == "638835886790068999-832829574-642210583")
                {
                    if (!File.Exists(tool.Location))
                    {
                        PixelWPF.LibraryPixel.Notification("010 Editor is not available!", "To use 010 Editor with Game Editor Studio, go to the Tools menu and set the location of 010 Editor. ");
                        return;
                    }

                    if (DTEData.DataTable.FileDataTable == null)
                    {
                        return;
                    }

                    string filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation;
                    if (!File.Exists(filelocation))
                    {
                        filelocation = DTEData.WorkshopXaml.WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + DTEData.DataTable.FileDataTable.FileLocation;
                    }

                    if (!File.Exists(filelocation) || !File.Exists(tool.Location))
                    {
                        return;
                    }

                    MethodData methodData = new MethodData();
                    methodData.ResourceLocations.Add(tool.Location);
                    methodData.ResourceLocations.Add(filelocation);
                    CommandMethodsClass.RunProgramwithfile(methodData);

                    break;
                }
            }
        }




        /////////////////////////////////////////////////////////////////////     
        ////////////////////////////// Editor Helpers /////////////////////// 
        /////////////////////////////////////////////////////////////////////
        public bool CheckValidDataTableInfo() //When hitting enter on the right bar's quick access name & data table info textboxes.
        {
            //First we check if the data table supports it.  I'm lazy so i just coded it to crash if it fails. 
            try
            {
                byte[] TheFileBytes = DTEData.DataTable.FileDataTable.FileBytes;
                int StartByte = int.Parse(PropertiesEditorTableStart.Text);
                int RowSize = int.Parse(PropertiesEditorTableWidth.Text);
                int RowCount = int.Parse(PropertiesEditorNameCount.Text);  //Aka name count.
                int TestByteLocation = StartByte + (RowCount * RowSize) - 1 ; //-1 to account for byte 0.
                string CrashTest = TheFileBytes[TestByteLocation].ToString("D");
            }
            catch
            {
                //Note: Compare each against their source in EntryClass to see if they even match and callout the actual problem.
                PixelWPF.LibraryPixel.NotificationNegative("Error: Beyond end of Data Table! ",
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

                if (DTEData.DataTable != null)
                {
                    PropertiesEditorTableStart.Text = DTEData.DataTable.DataTableStart.ToString();
                    PropertiesEditorTableWidth.Text = DTEData.DataTable.DataTableRowSize.ToString();
                }

                if (DTEData.NameTable != null)
                {
                    PropertiesEditorNameCount.Text = DTEData.NameTable.TextTableItemCount.ToString();
                    PropertiesEditorNameTableTextSize.Text = DTEData.NameTable.TextTableCharLimit.ToString();
                    PropertiesEditorNameTableRowSize.Text = DTEData.NameTable.TextTableRowSize.ToString();
                    PropertiesEditorNameTableStartByte.Text = DTEData.NameTable.TextTableStart.ToString();
                    PropertiesEditorFirstNameNumber.Text = DTEData.NameTable.TextTableFirstNameID.ToString();
                    PropertiesEditorNameCount.Text = (DTEData.NameTable.TextTableItemCount).ToString();
                }

                return false;
            }

            //Then we check if the name table supports it.
            try 
            {
                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                {
                    byte[] TheFileBytes = DTEData.NameTable.TextTableFile.FileBytes;
                    int StartByte = int.Parse(PropertiesEditorNameTableStartByte.Text);
                    int RowSize = int.Parse(PropertiesEditorNameTableRowSize.Text);
                    int RowCount = int.Parse(PropertiesEditorNameCount.Text);  //Aka name count.
                    string CrashTest = TheFileBytes[StartByte + (RowCount * RowSize) + (RowSize - 1)].ToString("D");
                }

                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                {
                    string fullText = System.Text.Encoding.UTF8.GetString(DTEData.NameTable.TextTableFile.FileBytes);
                    string[] lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                    int FirstLine = int.Parse(PropertiesEditorNameTableStartByte.Text);
                    int ItemCount = int.Parse(PropertiesEditorNameCount.Text);
                    
                    int lastRequiredLine = FirstLine + ItemCount - 1;

                    string crashTest = lines[lastRequiredLine];
                }
                return true;
            } 
            catch 
            {
                PixelWPF.LibraryPixel.NotificationNegative("Error: Beyond end of Name Table!",
                            "This checks if the name table has any actual data located at byte (Name Table Start Byte + (NameCount * RowSize)). " +
                            "It seems like thats beyond the end of the name table file (text that doesn't exist). " +
                            "\n\n" +
                            "List of possible causes..." +
                            "\n1: You were editing the start byte and set it starting way to far into the file." +
                            "\n2: You were editing row size and set it way to large" +
                            "\n3: You were editing Name Count and set more names then the data table actually has." +
                            "\n4: You were editing some combination of the three and put in the wrong numbers." +
                            "\n\n" +
                            "Note: Your change was stopped & reverted. "
                            );

                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) 
                {
                    if (DTEData.DataTable != null)
                    {
                        PropertiesEditorTableStart.Text = DTEData.DataTable.DataTableStart.ToString();
                        PropertiesEditorTableWidth.Text = DTEData.DataTable.DataTableRowSize.ToString();
                    }

                    if (DTEData.NameTable != null)
                    {
                        PropertiesEditorNameCount.Text = DTEData.NameTable.TextTableItemCount.ToString();
                        PropertiesEditorNameTableTextSize.Text = DTEData.NameTable.TextTableCharLimit.ToString();
                        PropertiesEditorNameTableRowSize.Text = DTEData.NameTable.TextTableRowSize.ToString();
                        PropertiesEditorNameTableStartByte.Text = DTEData.NameTable.TextTableStart.ToString();
                        PropertiesEditorFirstNameNumber.Text = DTEData.NameTable.TextTableFirstNameID.ToString();
                        PropertiesEditorNameCount.Text = (DTEData.NameTable.TextTableItemCount).ToString();
                    }
                }
                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                {
                    if (DTEData.DataTable != null && DTEData.NameTable != null)
                    {
                        PropertiesEditorNameTableStartByte.Text = DTEData.NameTable.TextTableStart.ToString();
                        PropertiesEditorFirstNameNumber.Text = DTEData.NameTable.TextTableFirstNameID.ToString();
                        PropertiesEditorNameCount.Text = (DTEData.NameTable.TextTableItemCount).ToString();
                    }
                }

                return false;


                
            }
            
            

            throw new Exception("This should never happen, and is definatly an error. :P");
            return false; //this is an error
        }

        /////////////////////////////////////////////////////////////////////     
        ////////////////////////////// CATEGORY ///////////////////////////// 
        /////////////////////////////////////////////////////////////////////
        private void PropertiesRowNameBox_KeyDownEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.CategoryClass.CategoryName = PropertiesRowNameBox.Text;
                DTEData.CategoryClass.CategoryLabel.Content = PropertiesRowNameBox.Text;
            }
        }

        private void PropertiesRowTooltipBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DTEData.CategoryClass.Tooltip = PropertiesRowTooltipBox.Text;
            if (PropertiesRowTooltipBox.Text == "")
            {
                DTEData.CategoryClass.TooltipGrid.ToolTip = null;
                DTEData.CategoryClass.CategoryUnderline.Visibility = Visibility.Collapsed;
            }
            else
            {
                DTEData.CategoryClass.TooltipGrid.ToolTip = DTEData.CategoryClass.Tooltip;
                DTEData.CategoryClass.CategoryUnderline.Visibility = Visibility.Visible;
            }
        }

        /////////////////////////////////////////////////////////////////////     
        /////////////////////////////// GROUP /////////////////////////////// 
        /////////////////////////////////////////////////////////////////////
        private void PropertiesGroupNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.GroupClass.GroupName = PropertiesGroupNameBox.Text;
                DTEData.GroupClass.GroupLabel.Content = PropertiesGroupNameBox.Text;
            }
        }

        private void PropertiesGroupTooltipTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DTEData.GroupClass.GroupTooltip = PropertiesGroupTooltipBox.Text;

            //Hook into a Update Group Tooltip method.
            if (PropertiesGroupTooltipBox.Text == "")
            {
                DTEData.GroupClass.GroupUnderline.Visibility = Visibility.Collapsed;
                DTEData.GroupClass.TooltipGrid.ToolTip = null;
            }
            if (PropertiesGroupTooltipBox.Text != "")
            {
                DTEData.GroupClass.GroupUnderline.Visibility = Visibility.Visible;
                DTEData.GroupClass.TooltipGrid.ToolTip = DTEData.GroupClass.GroupTooltip;
            }
        }

        /////////////////////////////////////////////////////////////////////     
        ///////////////////DOCUMENTS ARE IN A USER CONTROL/////////////////// 
        /////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////     
        /////////////////////////////// AUTOMOD ///////////////////////////// 
        /////////////////////////////////////////////////////////////////////
        private void ApplyFormulaToEntryAcrossAllItems(object sender, RoutedEventArgs e)
        {
            //Almost everything here uses doubles instead of ints to make ABSOLUTELY FUCKING SURE nothing EVER goes out of range, even when adding more value types or using large negatives from super robot wars / disgaea.
            //FormulaComboBox
            //FormulaTextBox

            if (DTEData.EntryClass == null) { return; }
            if (DTEData.WorkshopXaml.IsPreviewMode == true) { return; }

            //Editor EditorClass = DTEData;
            Entry EntryClassX = DTEData.EntryClass;


            if (EntryClassX.IsEntryHidden == true || EntryClassX.IsTextInUse == true) { return; } //prevents users from axidentally modding values that should be otherwise already disabled.
            if (EntryClassX.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed) { return; }

            //if (EntryClass.EntryByteSize != "1") { return; } //temporary

            int FinalItem = 0;
            try
            {
                if (DTEData.NameTable.TextTableItemCount != 0) { FinalItem = DTEData.NameTable.TextTableItemCount; }
                if (DTEData.NameTable.TextTableItemCount == 0)
                {
                    int ItemCount = 0;
                    foreach (var Item in DTEData.NameTable.ItemList)
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
                PixelWPF.LibraryPixel.NotificationNegative("Error: ",
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
                    if (EntryClassX.Endianness == "1")
                    {
                        EntryClassX.EntryByteDecimal = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset].ToString("D");
                        CurrentValue = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset];
                    }
                    if (EntryClassX.Endianness == "2B")
                    {
                        EntryClassX.EntryByteDecimal = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset).ToString("D");
                        CurrentValue = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset);
                    }
                    if (EntryClassX.Endianness == "4B")
                    {
                        EntryClassX.EntryByteDecimal = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset).ToString("D");
                        CurrentValue = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset);
                    }
                    if (EntryClassX.Endianness == "2L")
                    {
                        ushort WrongValue = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset);
                        CurrentValue = (ushort)IPAddress.HostToNetworkOrder((short)WrongValue); // Swaps the endianness
                    }
                    if (EntryClassX.Endianness == "4L")
                    {
                        uint value = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset);
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
                    if (EntryClassX.Endianness == "1" && NewValue > 255) { NewValue = 255; } //True Max Step
                    if (EntryClassX.Endianness == "2B" && NewValue > 65535) { NewValue = 65535; }
                    if (EntryClassX.Endianness == "2L" && NewValue > 65535) { NewValue = 65535; }
                    if (EntryClassX.Endianness == "4B" && NewValue > 4294967295) { NewValue = 4294967295; }
                    if (EntryClassX.Endianness == "4L" && NewValue > 4294967295) { NewValue = 4294967295; }





                    //Saving Step
                    string Result = NewValue.ToString();

                    if (EntryClassX.Endianness == "1")  // This is saving 1 Byte Size?   // First 1 byte save
                    {
                        Byte.TryParse(Result, out byte value8);
                        { ByteManager.ByteWriter(value8, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset); }
                    }
                    if (EntryClassX.Endianness == "2L")
                    {
                        UInt16.TryParse(Result, out ushort value16);
                        value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                        { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset); } //First 2 byte save


                    }
                    if (EntryClassX.Endianness == "4L")
                    {
                        UInt32.TryParse(Result, out uint value32);
                        byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                        Array.Reverse(valueBytes); // Swap the endianness
                        value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                        { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset); } //First 4 byte save

                    }
                    if (EntryClassX.Endianness == "2B")
                    {
                        UInt16.TryParse(Result, out ushort value16);
                        { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset); } //First 2 byte save
                    }
                    if (EntryClassX.Endianness == "4B")
                    {
                        UInt32.TryParse(Result, out uint value32);
                        { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClassX.DataTableRowSize) + EntryClassX.RowOffset); } //First 4 byte save
                    }

                    //FormulaMinTextBox
                    //FormulaMaxTextBox
                }
                catch
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: ???",
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



            TreeViewItem itemm = DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
            itemm.IsSelected = false;
            itemm.IsSelected = true;
        }

        /////////////////////////////////////////////////////////////////////     
        //////////////////////////////// LISTS ////////////////////////////// 
        /////////////////////////////////////////////////////////////////////
        public int ListFirstNameID = 0; //now unused, but i'll keep it here just incase...
        public void EntryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Exit the List Tab.
            foreach (TabItem tabItem in MainTabControl.Items)
            {
                if (tabItem.Header != null && tabItem.Header.ToString() == DTEData.EditorRightBar.PreviousTabName)
                {
                    tabItem.IsSelected = true;
                    break;
                }
            }

            if (DTEData.WorkshopXaml.WorkshopData.IsProjectLoaded == false) { return; }

            ListViewItem ListItem = EntryListBox.SelectedItem as ListViewItem;
            string selectedItem = ListItem.Content.ToString();

            TextInfo textInfo = ListItem.Tag as TextInfo;
            DTEData.EntryClass.EntryByteDecimal = (textInfo.ItemIndex /*+ ListFirstNameID*/).ToString();
            //Update Entry
            DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.SaveEntry(DTEData.EntryClass);
            DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.UpdateEntryProperties(DTEData.EntryClass);

            EntryManager entryManager = new();
            entryManager.LoadMenu(DTEData.EntryClass); //List Selection Changed.
            return;

            //Set Content
            DTEData.EntryClass.EntryTypeMenu.ListButton.Content = (textInfo.ItemIndex + ListFirstNameID) + ": " + textInfo.ItemName;
            if (ListItem.Content is TextBlock textblock)
            {
                Run one = textblock.Inlines.ElementAt(0) as Run;
                Run two = textblock.Inlines.ElementAt(1) as Run;
                selectedItem = one.Text + two.Text;
                DTEData.EntryClass.EntryTypeMenu.ListButton.Content = selectedItem;
            }

            //Set Tooltip
            DTEData.EntryClass.EntryTypeMenu.ListButton.ToolTip = null; //Reset tooltip status.
            if (textInfo.ItemWorkshopTooltip != "") { DTEData.EntryClass.EntryTypeMenu.ListButton.ToolTip = textInfo.ItemWorkshopTooltip; }

            //Set Tag
            if (ListItem.Tag is TextInfo Iinfo)
            {
                DTEData.EntryClass.EntryTypeMenu.ListButton.Tag = Iinfo;
            }

            //Set EntryByteDecimal
            string input = (string)DTEData.EntryClass.EntryTypeMenu.ListButton.Content; 
            string[] parts = input.Split(':');
            string number = parts[0].Trim();
            int x = int.Parse(number);
            DTEData.EntryClass.EntryByteDecimal = (x - ListFirstNameID).ToString(); 

            


            
        }

        private void PropertiesEntryNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.Name = PropertiesNameBox.Text;

                UpdateEntryName(DTEData.EntryClass);


            }
        }

        private void HideNameCheckboxChecked(object sender, RoutedEventArgs e)
        {
            DTEData.EntryClass.IsNameHidden = true;
            PropertiesNameBox.IsEnabled = false;
            DTEMethods.UpdateEntryName(DTEData.EntryClass);
            DTEMethods.UpdateEditorGrids(DTEData.EntryClass.ParentEditor.DataTableEditorData);
        }

        private void HideNameCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            DTEData.EntryClass.IsNameHidden = false;
            PropertiesNameBox.IsEnabled = true;
            DTEMethods.UpdateEntryName(DTEData.EntryClass);
            DTEMethods.UpdateEditorGrids(DTEData.EntryClass.ParentEditor.DataTableEditorData);
        }

        private void HideEntryCheckboxChecked(object sender, RoutedEventArgs e)
        {
            //If saving is disabled, changes to this entry will not be saved.
            //This is useful to explicitly prevent users from messing with things that crash the game.
            //Also can be used for other reasons.            

            DTEData.EntryClass.IsEntryHidden = true;
            DTEData.EntryClass.EntryBorder.Style = (Style)Application.Current.Resources["HiddenSelectedEntryStyle"];
            DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.ChangeEntryType(DTEData.WorkshopXaml.WorkshopData, DTEData.EntryClass.NewSubType, DTEData.WorkshopXaml, DTEData.EntryClass);

            //NOTE: These also trigger when a entry is selected, because the checkbox is updated.
        }

        private void HideEntryCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            DTEData.EntryClass.IsEntryHidden = false;
            if (DTEData.EntryClass.IsTextInUse == false)
            {
                DTEData.EntryClass.EntryBorder.Style = (Style)Application.Current.Resources["SelectedEntryStyle"];
            }
            DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.ChangeEntryType(DTEData.WorkshopXaml.WorkshopData, DTEData.EntryClass.NewSubType, DTEData.WorkshopXaml, DTEData.EntryClass);
        }

        private void PropertiesEntryByteSizeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            //Cancel method IF: The combobox text did not change.
            if (DTEData.EntryClass.Endianness == PropertiesEntryByteSizeComboBox.Text) { return; }
                
            string FindEntryByteSize = "Dummy";
            if (DTEData.EntryClass.Endianness == "1") { FindEntryByteSize = "1 Byte"; } //This makes it so the entrys current type appears in the properties dropdown.
            if (DTEData.EntryClass.Endianness == "2L") { FindEntryByteSize = "2 Bytes Little Endian"; }
            if (DTEData.EntryClass.Endianness == "2B") { FindEntryByteSize = "2 Bytes Big Endian"; }


            //Cancel method IF: Entry is attempting to merge with an already merged entry.
            if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "2 Bytes Big Endian")
            {
                foreach (Entry entry in DTEData.MasterEntryList)
                {
                    if (entry.RowOffset == DTEData.EntryClass.RowOffset + 1)
                    {
                        if (entry.Bytes == 2)
                        {
                            PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                            PixelWPF.LibraryPixel.NotificationNegative("Error: Entry not merged.",
                                "You cannot merge an entry, with an entry that is already merged with something else. " +
                                "\n\n" +
                                "If your confused, entrys merge with those next in offset decimal order, not those that are just under them in the UI. "
                                );
                            return;
                        }
                        if (entry.IsEntryHidden == true || entry.IsTextInUse == true)
                        {
                            PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                            PixelWPF.LibraryPixel.NotificationNegative("Error: Entry not merged.",
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
                foreach (Entry entry in DTEData.MasterEntryList)
                {
                    if (entry.RowOffset == DTEData.EntryClass.RowOffset + 1 || entry.RowOffset == DTEData.EntryClass.RowOffset + 2 || entry.RowOffset == DTEData.EntryClass.RowOffset + 3)
                    {
                        if (entry.Bytes == 2 || entry.Bytes == 4)
                        {
                            PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                            PixelWPF.LibraryPixel.NotificationNegative("Error: Entry not merged.",
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
                            PixelWPF.LibraryPixel.NotificationNegative("Error: Entry not merged.",
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





            //if (DTEData.EntryClass.NewSubType == Entry.EntrySubTypes.Menu) //Cancel method IF: A Menu-Type entry wants to become size-4.
            //{
            //    if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian")
            //    {

            //        foreach (ComboBoxItem item in PropertiesEntryByteSizeComboBox.Items)
            //        {
            //            if (item.Content.ToString() == FindEntryByteSize)
            //            {
            //                PropertiesEntryByteSizeComboBox.SelectedItem = item;
            //                break;
            //            }
            //        }

            //        PixelWPF.LibraryPixel.NotificationNegative("Not yet available D:",
            //                    "I have no idea how to make a UI for a 4 byte menu that isn't ridiculously laggy, so i'm just not letting you do this until i figure it out, sorry! x3" +
            //                    "\n\n" +
            //                    "You can still make a 2 byte menu, and i've never seen a game with a menu that actually has more then the 65535 options to select, so i'm hoping this isn't a real problem for you. If it is, reach out to me on discord." +
            //                    "\n\n" +
            //                    "Anyway, i'm aware some games will just have a 4 byte menu anyway where it doesn't use bytes 3 and 4. You can set the 3rd and 4th entry to hidden and the editor will still look proper :)" +
            //                    "" +
            //                    ""
            //        );

            //        return;
            //    }
            //}

            int OldSize = DTEData.EntryClass.Bytes;
            int NewSize = 999;
            string NewSizeS = "x";


            if (PropertiesEntryByteSizeComboBox.Text == "1 Byte") { NewSize = 1; NewSizeS = "1"; }
            if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Little Endian") { NewSize = 2; NewSizeS = "2L"; }
            if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian") { NewSize = 4; NewSizeS = "4L"; }
            if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Big Endian") { NewSize = 2; NewSizeS = "2B"; }
            if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian") { NewSize = 4; NewSizeS = "4B"; }

            List<Entry> targets = new();

            if (DTEData.EntryClass.RowOffset <= DTEData.DataTable.DataTableRowSize - NewSize) //Cancel method IF: New size would overflow off the end of GameTableSize
            {

                DTEData.EntryClass.Endianness = NewSizeS;
                DTEData.EntryClass.Bytes = NewSize;                                      

                DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.LoadEntry(DTEData, DTEData.EntryClass);

                //i need to add a check to make sure the next entrys are currently ALL ByteSize 1, if even one is not, cancel the process.

                //Below is what happens to OTHER ENTRYS, NOT the main entry having it's size changed.

                List<int> list = new();

                //if Old smaller then new
                if (OldSize < NewSize) //If hiding entrys
                {
                    foreach (int i in Enumerable.Range(Math.Min(OldSize, NewSize), Math.Abs(OldSize - NewSize)))
                    {
                        list.Add(DTEData.EntryClass.RowOffset + i);
                    }
                    foreach (var num in list)
                    {
                        foreach (var entry in DTEData.MasterEntryList)
                        {
                            if (entry.RowOffset == num) //Search for entry with offset+1 over current entry.  Offset+X
                            {
                                //entry.Endianness = "0";
                                //entry.Bytes = 0;
                                //WorkshopData.EntryManager.LoadEntry(this, EditorClass, entry);
                                //entry.EntryBorder.Visibility = Visibility.Collapsed;
                                //targets.Add(entry);
                                MoveEntryToMergedList(entry);
                                break;
                            }
                        }
                    }
                }

                //This makes sure when a entry is hidden / merged, that it is relocated to wherever the primary entry is.

                ////if Old bigger then new
                if (OldSize > NewSize) //If showing entrys
                {
                    foreach (int i in Enumerable.Range(Math.Min(OldSize, NewSize), Math.Abs(OldSize - NewSize))) //.OrderByDescending(x => x)
                    {
                        list.Add(DTEData.EntryClass.RowOffset + i);
                    }
                    foreach (var num in list)
                    {
                        foreach (Entry entry in DTEData.MasterEntryList)
                        {
                            if (entry.RowOffset == num) //Search for entry with offset+1 over current entry.  Offset+X
                            {
                                //entry.Endianness = "1";
                                //entry.Bytes = 1;
                                //WorkshopData.EntryManager.LoadEntry(this, EditorClass, entry);
                                ////EntryData.ReloadEntry(Database, EditorClass, entry);
                                //entry.EntryBorder.Visibility = Visibility.Visible;
                                //targets.Add(entry);
                                RemoveEntryFromMergedList(entry);

                            }
                        }

                    }
                }


                //I am currently not allowing size 4+ of Lists, i don't know that any game that uses more then 65K options to select from in one menu.
                //I ACTUALLY NEED TO ADD THIS. ITS NOT ABOUT GAMES NOT NEEDING IT, IT'S ABOUT THAT SOME GAMES FUCKING DO IT ANYWAY, AND NOT SUPPORTING IT MAKES EDITORS LOOK UGLY!!! (AND IS MISLEADING TO USERS!)

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
            //End
            

            void MoveEntryToMergedList(Entry entry)
            {
                var oldGrid = entry.ParentGrid;
                var oldGridItems = entry.ParentGridItems;

                {
                    //My code way below keeps failing to remove visual from parent.
                    //So im doing this as a temporary answer.
                    //However, there is surely a bigger, deeper problem here that will rear it's ugly head later if i ignore this for to long...


                    UIElement MyVisual = entry.Visual;
                    DependencyObject parent = VisualTreeHelper.GetParent(MyVisual);

                    // 2. Remove based on parent type
                    if (parent is Panel panel)
                    {
                        // Handles Grid, StackPanel, Canvas, etc.
                        panel.Children.Remove(MyVisual);
                    }
                    else if (parent is ContentControl contentControl)
                    {
                        // Handles Button, Window, UserControl, etc.
                        if (contentControl.Content == MyVisual)
                            contentControl.Content = null;
                    }
                    else if (parent is Decorator decorator)
                    {
                        // Handles Border, Viewbox, etc.
                        if (decorator.Child == MyVisual)
                            decorator.Child = null;
                    }
                    else if (parent is ContentPresenter contentPresenter)
                    {
                        // Handles templates
                        if (contentPresenter.Content == MyVisual)
                            contentPresenter.Content = null;
                    }
                }
                


                if (entry.ParentGroup != null) //If entry is in a group.
                {
                    if (entry.ParentGroup.ItemGrid.Children.Contains(entry.Visual))
                    {
                        string testtt = "yes";
                    }

                    entry.ParentGroup.GridItems.Remove(entry);
                }
                else if (entry.ParentGroup == null) //If entry is not in a group.
                {

                    entry.ParentCategory.GridItems.Remove(entry);
                }
                                

                if (entry.Visual != null && oldGrid != null)
                {
                    if (entry.ParentGrid.Children.Contains(entry.Visual)) 
                    {
                        string testtt = "yes";
                    }

                    oldGrid.Children.Remove(entry.Visual);
                }
                if (entry.Visual != null && entry.ParentGrid != null)
                {
                    entry.ParentGrid.Children.Remove(entry.Visual);
                }

                entry.ParentCategory = null;
                entry.ParentGroup = null;
                entry.ParentGrid = null;
                entry.ParentGridItems = null;
                entry.Column = 0;
                entry.Row = 0;
                entry.IsMerged = true;
                entry.ParentEditor.DataTableEditorData.MergedEntryList.Add(entry);

                string name = entry.Name;
            }
            void RemoveEntryFromMergedList(Entry entry)
            {
                var editor = entry.ParentEditor;

                // Find the previous entry by RowOffset
                int targetOffset = entry.RowOffset - 1;
                Entry previousEntry = editor.DataTableEditorData.MasterEntryList.FirstOrDefault(e => e.RowOffset == targetOffset);

                { //If previous entry is not on the grid, get 2nd previous, or get 3rd previous. Just find somewhere to put the fucking entry!
                    if (previousEntry == null) { return; }
                    if (previousEntry.IsMerged == true)
                    {
                        int targetOffset2 = entry.RowOffset - 2;
                        previousEntry = editor.DataTableEditorData.MasterEntryList.FirstOrDefault(e => e.RowOffset == targetOffset2);
                    }

                    if (previousEntry == null) { return; }
                    if (previousEntry.IsMerged == true)
                    {
                        int targetOffset3 = entry.RowOffset - 3;
                        previousEntry = editor.DataTableEditorData.MasterEntryList.FirstOrDefault(e => e.RowOffset == targetOffset3);
                    }
                }
                

                entry.IsMerged = false;
                editor.DataTableEditorData.MergedEntryList.Remove(entry);
                DTEMethods.InsertGridItem(entry, previousEntry);

                EntryManager entryManager = new();
                entryManager.LoadEntry(DTEData, entry);
                DTEData.WorkshopXaml.UpdateSymbology(entry);
            }

            //UpdateSymbology(EntryClass);
            //EntryClass.ParentEditor.StandardEditorData.TheXaml.UpdateEntryDecorationsForAllEditors();
            DTEMethods.UpdateEditorGrids(DTEData.DataTableEditorData);
            DTEData.WorkshopXaml.UpdateSymbology(DTEData.EntryClass);
        }

        private void PropertiesEntryType_DropDownClosed(object sender, EventArgs e)
        {
            string asdf = PropertiesEntryType.Text;
            EntrySubTypes NewEntryType = (EntrySubTypes)Enum.Parse(typeof(EntrySubTypes), asdf);

            DTEData.EntryClass.RowSpan = 1;
            if (NewEntryType == Entry.EntrySubTypes.BitFlag) { DTEData.EntryClass.RowSpan = 8; }

            if (NewEntryType == Entry.EntrySubTypes.Menu) //Cancel method IF: A Menu-Type entry wants to become size-4.
            {
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian")
                {

                    string FindEntryType = DTEData.EntryClass.NewSubType.ToString();  //Entry Type Dropdown Menu.
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


            DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.ChangeEntryType(DTEData.WorkshopXaml.WorkshopData, NewEntryType, DTEData.WorkshopXaml, DTEData.EntryClass);

            EntryManager EManager = new();
            EManager.SetSelectedEntry(DTEData.EntryClass);
            EManager.UpdateEntryProperties(DTEData.EntryClass);
            //StandardEditorMethods.UpdateGridLayout(EntryClass.ParentCategory.ItemGrid, EntryClass.ParentCategory.GridItems);
            DTEMethods.UpdateEditorGrids(DTEData.DataTableEditorData);
        }

        //////// Number Box /////////
        private void SetNumberboxSigned(object sender, RoutedEventArgs e)
        {
            DTEData.EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Signed;

            EntryManager EntryManager = new();
            EntryManager.LoadEntry(DTEData, DTEData.EntryClass);

            DTEData.DTEXaml.RightBar.CrossReferenceInfo.FillLearnBox(DTEData);
        }

        private void SetNumberboxUnsigned(object sender, RoutedEventArgs e)
        {
            DTEData.EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Unsigned;

            EntryManager EntryManager = new();
            EntryManager.LoadEntry(DTEData, DTEData.EntryClass);

            DTEData.DTEXaml.RightBar.CrossReferenceInfo.FillLearnBox(DTEData);
        }

        private void SuffixTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DTEData == null) { return; }

            if (DTEData.EntryClass != null)
            {
                if (DTEData.EntryClass.EntryTypeNumberBox != null)
                {
                    DTEData.EntryClass.EntryTypeNumberBox.Suffix = NumberboxSuffixTextbox.Text;
                    DTEData.EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Tag = null;
                    DTEData.EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Tag = DTEData.EntryClass;
                }
            }
        }

        //////// Check Box /////////
        //    (Empty for now)


        //////// Bit Flags /////////
        private void Bitflag1NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag1Name = PropertiesEntryBitFlag1Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        private void Bitflag2NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag2Name = PropertiesEntryBitFlag2Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        private void Bitflag3NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag3Name = PropertiesEntryBitFlag3Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        private void Bitflag4NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag4Name = PropertiesEntryBitFlag4Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        private void Bitflag5NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag5Name = PropertiesEntryBitFlag5Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        private void Bitflag6NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag6Name = PropertiesEntryBitFlag6Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        private void Bitflag7NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag7Name = PropertiesEntryBitFlag7Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        private void Bitflag8NameTextboxTextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DTEData.EntryClass.EntryTypeBitFlag.BitFlag8Name = PropertiesEntryBitFlag8Name.Text;
                UpdateEntryName(DTEData.EntryClass);
            }
        }

        //////// Menu Entry /////////
        private void PropertiesMenuType_DropDownClosed(object sender, EventArgs e)
        {
            if (DropdownMenuType.Text == "Dropdown") { DTEData.EntryClass.EntryTypeMenu.MenuType = EntryTypeMenu.MenuTypes.Dropdown; }
            else if (DropdownMenuType.Text == "List") { DTEData.EntryClass.EntryTypeMenu.MenuType = EntryTypeMenu.MenuTypes.List; }

            DTEData.WorkshopXaml.WorkshopData.EntryManagerOLD.ChangeEntryType(DTEData.WorkshopXaml.WorkshopData, EntrySubTypes.Menu, DTEData.WorkshopXaml, DTEData.EntryClass);
        }        

        private void MenuEntryOpenTextTableManager(object sender, RoutedEventArgs e)
        {
            DTEData.WorkshopXaml.HIDEMOST();
            TextTableManager TheUserControl = new TextTableManager();
            DTEData.WorkshopXaml.MidGrid.Children.Add(TheUserControl);
            TheUserControl.SetupForMenu(DTEData.EntryClass, DTEData.EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox, DTEData.WorkshopXaml.WorkshopData, DTEData.WorkshopXaml, DTEData);
            DTEData.WorkshopXaml.TextSourceManager = TheUserControl;
        }

        //////// Tool Tip /////////
        private void EntryNoteTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            DTEData.EntryClass.WorkshopTooltip = EntryNoteTextbox.Text;
            DTEMethods.UpdateEntryName(DTEData.EntryClass);
        }









        public void UpdateEntryName(Entry TheEntry)
        {

            TheEntry.ParentEditor.WorkshopXaml.WorkshopData.EntryManagerOLD.LoadEntry(DTEData, DTEData.EntryClass);

            DTEMethods.UpdateEntryName(DTEData.EntryClass);
            DTEMethods.UpdateEditorGrids(DTEData.EntryClass.ParentEditor.DataTableEditorData);

        }

        private void OpenDescriptionManager(object sender, RoutedEventArgs e)
        {
            TextTableManager TheManager = new TextTableManager();
            DTEData.DTEXaml.EditorBack.Children.Add(TheManager);

            TheManager.SetupForDescription(DTEData.WorkshopData, DTEData);

            Grid.SetRow(TheManager, 0);
            Grid.SetColumn(TheManager, 0);
            Grid.SetRowSpan(TheManager, 99);
            Grid.SetColumnSpan(TheManager, 99);

        }

        
    }
}
