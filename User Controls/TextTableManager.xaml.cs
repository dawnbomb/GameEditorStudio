using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Formats.Tar;
using System.Linq;
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
using Microsoft.VisualBasic;
using PixelWPF;
using WpfHexEditor;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static GameEditorStudio.Entry;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TextSourceManager.xaml
    /// </summary>
    public partial class TextTableManager : UserControl
    {   
        DataTableEditorData DTEData { get; set; }
        WorkshopData Database { get; set; }
        Entry EntryClass { get; set; }
        ListBox EntryListBox { get; set; }
        public Workshop Workshop { get; set; }
        bool NamesMode { get; set; } = false;
        bool DescriptionMode { get; set; } = false;
        bool MenuMode { get; set; } = false;
        UserControlEditorCreator EditorCreator { get; set; }

        public TextTableManager()
        {
            InitializeComponent();

            Tab1.Visibility = Visibility.Collapsed;
            Tab2.Visibility = Visibility.Collapsed;
            Tab22.Visibility = Visibility.Collapsed;
            Tab3.Visibility = Visibility.Collapsed;
            Tab5.Visibility = Visibility.Collapsed;

            ComboBoxListType.SelectedIndex = 0;

            this.Loaded += new RoutedEventHandler(LoadEvent);
            //Database = database;


            #if DEBUG
            #else
            DataFileDebugButton.Visibility = Visibility.Collapsed; //Remove the debug button in release builds.
            AdvancedDebugButton.Visibility = Visibility.Collapsed; 
            #endif

            {
                //Link to Data File
                ToolTipService.SetInitialShowDelay(DataCharSetUnderline, LibraryGES.TooltipInitialDelay);
                ToolTipService.SetBetweenShowDelay(DataCharSetUnderline, LibraryGES.TooltipBetweenDelay);
                ToolTipService.SetInitialShowDelay(NameIDLabelUnderline, LibraryGES.TooltipInitialDelay);
                ToolTipService.SetBetweenShowDelay(NameIDLabelUnderline, LibraryGES.TooltipBetweenDelay);
                ToolTipService.SetInitialShowDelay(UnderlineNameCountFile, LibraryGES.TooltipInitialDelay);
                ToolTipService.SetBetweenShowDelay(UnderlineNameCountFile, LibraryGES.TooltipBetweenDelay);

                //Link to Nothing
                ToolTipService.SetInitialShowDelay(NothingFirstNameUnderline, LibraryGES.TooltipInitialDelay);
                ToolTipService.SetBetweenShowDelay(NothingFirstNameUnderline, LibraryGES.TooltipBetweenDelay);

                //Link to Advanced
                ToolTipService.SetInitialShowDelay(AdvCharSetUnderline, LibraryGES.TooltipInitialDelay);
                ToolTipService.SetBetweenShowDelay(AdvCharSetUnderline, LibraryGES.TooltipBetweenDelay);
                ToolTipService.SetInitialShowDelay(NameIDLabelUnderline2, LibraryGES.TooltipInitialDelay);
                ToolTipService.SetBetweenShowDelay(NameIDLabelUnderline2, LibraryGES.TooltipBetweenDelay);
                ToolTipService.SetInitialShowDelay(UnderlineNameCountFile2, LibraryGES.TooltipInitialDelay);
                ToolTipService.SetBetweenShowDelay(UnderlineNameCountFile2, LibraryGES.TooltipBetweenDelay);
                


            }
        }

        public void LoadEvent(object sender, RoutedEventArgs e)
        {
            DependencyObject? current = this;

            while (current != null && current is not UserControlEditorCreator)
            {
                current = VisualTreeHelper.GetParent(current);
                
            }
            

            if (current is UserControlEditorCreator)
            {
                EditorCreator = (UserControlEditorCreator)current;                

                ButtonExitWithoutSaving.Visibility = Visibility.Collapsed;

                NameIDLabel.Visibility = Visibility.Collapsed;
                FileNameIDTextBox.Visibility = Visibility.Collapsed;
                DataLinkFirstNameIDHelpPanel.Visibility = Visibility.Collapsed;

                foreach (ComboBoxItem CItem in ComboBoxListType.Items)
                {
                    if (CItem.Content as string == "Link to Editor")
                    {
                        ComboBoxListType.Items.Remove(CItem);
                        break;
                    }
                }

                //Remove from Editor
                //NOTE: IF I EVER ALLOW EDITORS TO GET NAMES FROM ANOTHER EDITOR, ENTRY MENUS LINKING TO EDITORS WILL BREAK. 
                //I CAN'T THINK OF A REASON TO DO THIS THOUGH, SO I DON'T THINK IT WILL BE A PROBLEM. 
                //(IE, Editor A has a entry getting names from editor B, but editor B is getting names from C. Well, i guess as entrys load AFTER editor names, so maybe this won't be a problem after all.
                //Still, i should keep the posibility of this in mind if i ever do this.)
            }

            var parentWindow = Window.GetWindow(this);


        }


        public void SetupForNames(DataTableEditorData TheDataTableEditorData)
        {
            //Supported Links: DataFile, TextFile, Nothing.

            //IMPORTANT NOTE: THIS ONLY WORKS FOR NAMES FOR AN ENTRY (MENU MODE). 
            //IF I EVER ADD SUPPORT FOR NAMES FROM EDITORS FOR THINGS OTHER THEN ENTRYS, I MUST RETHINK?
            //PS: DON'T LET EDITORS GET NAMES FROM EDITORS WITHOUT TESTING IF ANY INFINITE LOOPS WILL OCCUR.
            //AND DONT FORGET EXTENDED LOOPS, LIKE A gets from B, B gets from C, C gets from A.
            //Well, i guess as entrys load AFTER editor names, so maybe this won't be a problem after all?
            NamesMode = true;

            DTEData = TheDataTableEditorData;
            Database = TheDataTableEditorData.WorkshopXaml.WorkshopData;

            ComboBoxListType.Items.RemoveAt(2); //Remove Names from Editor

            //Setup for editor.
            EditorMenuLinkFirstIDExplainLabel.Visibility = Visibility.Collapsed;

            //Setup for nothing.
            MenuByteValueExplainText.Visibility = Visibility.Collapsed;
            NothingBtnSetUsed1.Visibility = Visibility.Collapsed;
            NothingBtnSetUsed2.Visibility = Visibility.Collapsed;


            if (TheDataTableEditorData.NameTable != null) //Set tab based on name table type.
            {
                TextTable TextTable = DTEData.NameTable;

                if (TheDataTableEditorData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced) 
                {
                    ComboBoxListType.Text = "Link to Advanced";
                    TabControlListType.SelectedIndex = 4;

                    

                    DataFileManagerAdvanced.Loaded += new RoutedEventHandler(SelectNameFile);
                    void SelectNameFile(object sender, RoutedEventArgs e)
                    {
                        AdvCharacterSetComboBox.Text = TextTable.TextTableCharacterSet;
                        AdvNameCountTextBox.Text = Math.Max(TextTable.TextTableItemCount, 0).ToString();
                        AdvFirstNameIDTextbox.Text = TextTable.TextTableFirstNameID.ToString();


                        
                        AdvRowStartBox.Clear();
                        AdvRowEndBox.Clear();
                        string starttext = "";
                        string endtext = "";
                        foreach (TextInfo textInfo in TextTable.ItemList)
                        {
                            starttext = starttext + textInfo.RowStart.ToString() + "\n";
                            endtext = endtext + textInfo.RowEnd.ToString() + "\n";
                        }
                        AdvRowStartBox.Text = starttext;
                        AdvRowEndBox.Text = endtext;

                        foreach (TreeViewItem fileItem in LibraryGES.GetALLTreeViewItems(DataFileManagerAdvanced.TreeGameFiles)) //FileTreeExtraTable.Items
                        {
                            if (fileItem.Tag == DTEData.NameTable.TextTableFile)
                            {
                                fileItem.IsSelected = true;
                                break;
                            }
                        }
                        UpdateAdvancedPreview();
                    }

                    //AdvRowStartBox
                    //AdvRowStartBox
                }
                if (TheDataTableEditorData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                {
                    LinkDataFile.IsSelected = true;
                    TabControlListType.SelectedIndex = 0;

                    CharacterSetComboBox.Text = TextTable.TextTableCharacterSet;
                    FileFullRowSizeTextBox.Text = TextTable.TextTableRowSize.ToString();
                    FileTextSizeTextBox.Text = TextTable.TextTableCharLimit.ToString();
                    FileStartTextBox.Text = TextTable.TextTableStart.ToString();
                    FileNameCountTextBox.Text = Math.Max(TextTable.TextTableItemCount, 0).ToString();
                    FileNameIDTextBox.Text = TextTable.TextTableFirstNameID.ToString();
                    
                    //for data file link
                    DataFileManager.Loaded += new RoutedEventHandler(SelectNameFile);
                    void SelectNameFile(object sender, RoutedEventArgs e)
                    {
                        foreach (TreeViewItem fileItem in LibraryGES.GetALLTreeViewItems(DataFileManager.TreeGameFiles)) //FileTreeExtraTable.Items
                        {
                            if (fileItem.Tag == DTEData.NameTable.TextTableFile)
                            {
                                fileItem.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
                if (TheDataTableEditorData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                {
                    LinkTextFile.IsSelected = true;
                    TabControlListType.SelectedIndex = 1;
                    TextFileFileNameIDTextBox.Text = TextTable.TextTableFirstNameID.ToString();


                    TextFirstLineTextBox.Text = TextTable.TextTableStart.ToString();
                    TextLastLineTextBox.Text = (Math.Max(TextTable.TextTableStart + TextTable.TextTableItemCount - 1, 0)).ToString();
                    //TextLastLineTextBox.Text = TextTable.

                    //For name link
                    FileManagerForTextFiles.Loaded += new RoutedEventHandler(SelectNameText);
                    void SelectNameText(object sender, RoutedEventArgs e)
                    {
                        foreach (TreeViewItem fileItem in LibraryGES.GetALLTreeViewItems(FileManagerForTextFiles.TreeGameFiles)) //FileTreeExtraTable.Items
                        {
                            if (fileItem.Tag == DTEData.NameTable.TextTableFile)
                            {
                                fileItem.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
                if (TheDataTableEditorData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Editor)
                {
                    LinkEditor.IsSelected = true;
                    TabControlListType.SelectedIndex = 2;
                }
                if (TheDataTableEditorData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing)
                {
                    LinkNothing.IsSelected = true;
                    TabControlListType.SelectedIndex = 3;

                    FileFirstNameIDNothingTextBox.Text = TextTable.TextTableFirstNameID.ToString();

                    if (TheDataTableEditorData.NameTable.ItemList.Count != 0)
                    {
                        //Nothing List Setup
                        ItemsNumBox.Clear();
                        ItemsEditBox.Clear();
                        //ItemsNumBox.Text = "0";
                        StringBuilder NumsText = new StringBuilder("0");
                        StringBuilder itemsText = new StringBuilder(TheDataTableEditorData.NameTable.ItemList[0].ItemName);
                        for (int i = 1; i < TheDataTableEditorData.NameTable.ItemList.Count; i++)
                        {
                            NumsText.Append("\r");
                            NumsText.Append(i);
                            itemsText.Append("\r");
                            itemsText.Append(TheDataTableEditorData.NameTable.ItemList[i].ItemName);
                        }
                        ItemsNumBox.Text = NumsText.ToString();
                        ItemsEditBox.Text = itemsText.ToString();
                    }
                }
            }


        }

        public void SetupForDescription(WorkshopData wdata, DataTableEditorData TheDTEData)
        {
            //Supported Links: DataFile, TextFile.

            DTEData = TheDTEData;
            Database = wdata;
            DescriptionMode = true;


            {
                LabelNameCount.Visibility = Visibility.Collapsed;
                UnderlineNameCountFile.Visibility = Visibility.Collapsed;
                FileNameCountTextBox.Visibility = Visibility.Collapsed;
                DataLinkNameCountHelpPanel.Visibility = Visibility.Collapsed;

                NameIDLabel.Visibility = Visibility.Collapsed;
                NameIDLabelUnderline.Visibility = Visibility.Collapsed;
                FileNameIDTextBox.Visibility = Visibility.Collapsed;

                //DataFilePreviewBorder.Visibility = Visibility.Collapsed;
            }
            
            {
                TextLastLineNameLabel.Visibility = Visibility.Collapsed;
                TextLastLineTextBox.Visibility = Visibility.Collapsed;
                TextBoxLastLineHelpText.Visibility = Visibility.Collapsed;
                ButtonSetLastLine.Visibility = Visibility.Collapsed;

                TextFileNameIDLabel.Visibility = Visibility.Collapsed;
                TextFileFileNameIDTextBox.Visibility = Visibility.Collapsed;
                TextLinkNameIDHelpText.Visibility = Visibility.Collapsed;

                //TextNameListPreviewBorder.Visibility = Visibility.Collapsed;
            }
            {
                MenuByteValueExplainText.Visibility = Visibility.Collapsed;
            }
            

            ComboBoxListType.Items.RemoveAt(3); //Remove Names from Nothing
            ComboBoxListType.Items.RemoveAt(2); //Remove Names from Editor
            //NOTE: IF I EVER ALLOW EDITORS TO GET NAMES FROM ANOTHER EDITOR, ENTRY MENUS LINKING TO EDITORS WILL BREAK. 
            //I CAN'T THINK OF A REASON TO DO THIS THOUGH, SO I DON'T THINK IT WILL BE A PROBLEM. 
            //(IE, Editor A has a entry getting names from editor B, but editor B is getting names from C. Well, i guess as entrys load AFTER editor names, so maybe this won't be a problem after all.
            //Still, i should keep the posibility of this in mind if i ever do this.)

            var parentWindow = Window.GetWindow(this);


            if (DTEData.DescriptionTableList.Count != 0)
            {
                TextTable TextTable = DTEData.DescriptionTableList[0];

                if (TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                {
                    ComboBoxListType.Text = "Link to Advanced";
                    TabControlListType.SelectedIndex = 4;

                    

                    DataFileManagerAdvanced.Loaded += new RoutedEventHandler(SelectNameFile);
                    void SelectNameFile(object sender, RoutedEventArgs e)
                    {
                        AdvCharacterSetComboBox.Text = TextTable.TextTableCharacterSet;
                        AdvNameCountTextBox.Text = Math.Max(TextTable.TextTableItemCount, 0).ToString();
                        AdvFirstNameIDTextbox.Text = TextTable.TextTableFirstNameID.ToString();

                        AdvRowStartBox.Clear();
                        AdvRowEndBox.Clear();
                        string starttext = "";
                        string endtext = "";
                        foreach (TextInfo textInfo in TextTable.ItemList)
                        {
                            starttext = starttext + textInfo.RowStart.ToString() + "\n";
                            endtext = endtext + textInfo.RowEnd.ToString() + "\n";
                        }
                        AdvRowStartBox.Text = starttext;
                        AdvRowEndBox.Text = endtext;

                        foreach (TreeViewItem fileItem in LibraryGES.GetALLTreeViewItems(DataFileManagerAdvanced.TreeGameFiles)) //FileTreeExtraTable.Items
                        {
                            if (fileItem.Tag == TextTable.TextTableFile)
                            {
                                fileItem.IsSelected = true;
                                break;
                            }
                        }
                        UpdateAdvancedPreview();
                    }
                    
                }
                else //everything before i added advanced. Code seems weird compared to setup for names. hmmm...
                {
                    FileStartTextBox.Text = TextTable.TextTableStart.ToString();
                    FileTextSizeTextBox.Text = TextTable.TextTableCharLimit.ToString();
                    FileFullRowSizeTextBox.Text = TextTable.TextTableRowSize.ToString();
                    CharacterSetComboBox.Text = TextTable.TextTableCharacterSet;

                    if (DTEData.NameTable != null)
                    {
                        FileNameCountTextBox.Text = Math.Max(DTEData.NameTable.TextTableItemCount, 0).ToString();
                        FileNameIDTextBox.Text = DTEData.NameTable.TextTableFirstNameID.ToString();
                        TextLastLineTextBox.Text = (Math.Max(DTEData.NameTable.TextTableStart + DTEData.NameTable.TextTableItemCount - 1, 0)).ToString();
                    }


                    if (TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                    {
                        ComboBoxListType.Text = "Link to Text File";
                        TabControlListType.SelectedIndex = 1;

                        TextFirstLineTextBox.Text = TextTable.TextTableStart.ToString();
                        
                    }

                    DataFileManager.Loaded += new RoutedEventHandler(ShowDescriptionFileInUse);
                }

            }
        }

        public void ShowDescriptionFileInUse(object sender, RoutedEventArgs e)
        {
            foreach (TreeViewItem Item3 in LibraryGES.GetALLTreeViewItems(DataFileManager.TreeGameFiles)) //FileTreeExtraTable.Items
            {
                if (Item3.Tag == DTEData.DescriptionTableList[0].TextTableFile)
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
            foreach (TreeViewItem Item3 in LibraryGES.GetALLTreeViewItems(FileManagerForTextFiles.TreeGameFiles)) //FileTreeExtraTable.Items
            {
                if (Item3.Tag == DTEData.DescriptionTableList[0].TextTableFile)
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
        }

        public void SetupForMenu(Entry EntryClass2, ListBox entryListBox, WorkshopData database, Workshop TheWorkshop, DataTableEditorData TheDTEData)
        {
            //Supported Links: DataFile, TextFile, Editor, Nothing.
            ComboBoxListType.Items.RemoveAt(4); //Remove Names from Advanced

            DTEData = TheDTEData;
            EntryClass = EntryClass2;
            EntryListBox = entryListBox;
            Database = database;
            Workshop = TheWorkshop;

            MenuMode = true;
            NameIDLabel.Visibility = Visibility.Visible;
            FileNameIDTextBox.Visibility = Visibility.Visible;
            EditLinkFirstNameIDHelpText.Visibility = Visibility.Visible;

            LabelNameCount.Visibility = Visibility.Visible;
            FileNameCountTextBox.Visibility = Visibility.Visible;
            DataLinkNameCountHelpPanel.Visibility = Visibility.Visible;
            



            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile)
            {
                ComboBoxListType.Text = "Link to Data File";
                TabControlListType.SelectedIndex = 0;
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
            {
                ComboBoxListType.Text = "Link to Text File";
                TabControlListType.SelectedIndex = 1;
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
            {
                ComboBoxListType.Text = "Link to Editor";
                TabControlListType.SelectedIndex = 2;
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
            {
                ComboBoxListType.Text = "Link to Nothing";
                TabControlListType.SelectedIndex = 3;
            }



            //Data File Setup
            FileNameIDTextBox.Text = "0";
            if (EntryClass.EntryTypeMenu.TextTableDataFile != null)
            {
                TextTable texttable = EntryClass.EntryTypeMenu.TextTableDataFile;

                DataFileManager.SelectFile(texttable.TextTableFile);

                FileStartTextBox.Text = texttable.TextTableStart.ToString();
                FileFullRowSizeTextBox.Text = texttable.TextTableRowSize.ToString();
                FileTextSizeTextBox.Text = texttable.TextTableCharLimit.ToString();
                FileNameIDTextBox.Text = texttable.TextTableFirstNameID.ToString();
                FileNameCountTextBox.Text = texttable.TextTableItemCount.ToString();
            }


            //Text File Setup
            TextFileFileNameIDTextBox.Text = "0";
            if (EntryClass.EntryTypeMenu.TextTableTextFile != null)
            {
                TextTable texttable = EntryClass.EntryTypeMenu.TextTableTextFile;

                FileManagerForTextFiles.SelectFile(texttable.TextTableFile);

                TextFirstLineTextBox.Text = texttable.TextTableStart.ToString();
                TextLastLineTextBox.Text = (texttable.TextTableItemCount - 1 + texttable.TextTableStart).ToString();
                TextFileFileNameIDTextBox.Text = texttable.TextTableFirstNameID.ToString();
            }


            //Editor Setup
            SetupForNewDTEditorNamesOLD(database);
            EditorFileNameIDTextBox.Text = "0";
            EditorNameIDLabel.Visibility = Visibility.Collapsed;
            EditorFileNameIDTextBox.Visibility = Visibility.Collapsed;
            if (EntryClass.EntryTypeMenu.TextTableEditor != null)
            {
                if (EntryClass.EntryTypeMenu.TextTableEditor.LinkedDTEEditor != null) 
                {
                    foreach (TreeViewItem TItem in LibraryGES.GetALLTreeViewItems(EditorsTreeView)) //Select the Editor.
                    {
                        Editor TheEditor = TItem.Tag as Editor;
                        if (TheEditor == EntryClass.EntryTypeMenu.TextTableEditor.LinkedDTEEditor)
                        {
                            TItem.IsSelected = true;
                            break;
                        }
                    }

                    EditorFileNameIDTextBox.Text = EntryClass.EntryTypeMenu.TextTableEditor.TextTableFirstNameID.ToString();
                }                

                
            }

            
            //Nothing Setup
            FileFirstNameIDNothingTextBox.Text = "0";
            ItemsNumBox.Clear();
            ItemsEditBox.Clear();
            if (EntryClass.EntryTypeMenu.TextTableNothing != null)
            {
                TextTable texttable = EntryClass.EntryTypeMenu.TextTableNothing;

                FileFirstNameIDNothingTextBox.Text = texttable.TextTableFirstNameID.ToString();


                int MinIndex = texttable.ItemList.Min(x => x.ItemIndex);
                int MaxIndex = texttable.ItemList.Max(x => x.ItemIndex);
                ////Nothing List Setup
                ItemsNumBox.Clear();
                ItemsEditBox.Clear();
                StringBuilder NumsText = new StringBuilder("0");
                StringBuilder itemsText = new StringBuilder("");
                if (MinIndex == 0) 
                {
                    var item = texttable.ItemList.FirstOrDefault(x => x.ItemIndex == 0);
                    if (item != null) 
                    {
                        itemsText.Append(item.ItemName);
                    }
                }
                //for (int i = 1; i < TheDataTableEditorData.NameTable.ItemList.Count; i++)
                //{
                //    NumsText.Append("\r");
                //    NumsText.Append(i);
                //    itemsText.Append("\r");
                //    itemsText.Append(TheDataTableEditorData.NameTable.ItemList[i].ItemName);
                //}
                //ItemsNumBox.Text = NumsText.ToString();
                //ItemsEditBox.Text = itemsText.ToString();

                for (int i = 1; i < MaxIndex + 1; i++)
                {
                    NumsText.Append("\r");
                    NumsText.Append(i);
                    itemsText.Append("\r");
                    var item = texttable.ItemList.FirstOrDefault(x => x.ItemIndex == i);
                    if (item != null)
                    {
                        itemsText.Append(item.ItemName);
                    }
                }
                ItemsNumBox.Text = NumsText.ToString();
                ItemsEditBox.Text = itemsText.ToString();



                //{   //Part 2: Set the Texts box.
                //    string itemsText = "";               
                //    int rows = 0;
                //    if (EntryClass.Bytes == 1) { rows = 255; }
                //    if (EntryClass.Bytes == 2) { rows = 65535; }
                //    for (int i = 0; i < rows; i++)
                //    {                        
                //        var item = texttable.ItemList.FirstOrDefault(x => x.ItemIndex == i);
                //        if (item != null){ itemsText = itemsText + item.ItemName; }
                //        itemsText = itemsText + "\r";
                //    }
                //    ItemsEditBox.Text = itemsText.ToString();
                //}



                

            }



            


            


            



        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////// SETUP //////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////// SAVE TEXT TABLES /////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ButtonClickSaveTextTable(object sender, RoutedEventArgs e)
        {
            if (NamesMode == true) { SaveNamesTable(); return; }
            if (DescriptionMode == true) { SaveDescriptions(); return; }
            if (MenuMode == true){ SaveMenu(); return; }
            
        }

        private void SaveNamesTable() 
        {
            //Supported Links: DataFile, TextFile, Nothing.
            TabItem tabItem = TabControlListType.SelectedItem as TabItem;
            string TheTextTableType = tabItem.Tag as string;

            //Error checking first.
            bool canpass = true;            
            if (TheTextTableType == "DataFile") 
            {                
                FileFullRowSizeTextBox.Background = null;
                FileTextSizeTextBox.Background = null;
                FileStartTextBox.Background = null;
                FileNameCountTextBox.Background = null;
                FileNameIDTextBox.Background = null;
                DataFileManager.TreeGameFiles.Background = null;

                if (FileFullRowSizeTextBox.Text == null || FileFullRowSizeTextBox.Text == "" || FileFullRowSizeTextBox.Text == "0")
                {
                    FileFullRowSizeTextBox.Background = Brushes.Red;
                    canpass = false;
                }
                if (FileTextSizeTextBox.Text == null || FileTextSizeTextBox.Text == "" || FileTextSizeTextBox.Text == "0")
                {
                    FileTextSizeTextBox.Background = Brushes.DarkRed;
                    canpass = false;
                }
                if (FileStartTextBox.Text == null || FileStartTextBox.Text == "")
                {
                    FileStartTextBox.Background = Brushes.DarkRed;
                    canpass = false;
                }
                if (FileNameCountTextBox.Text == null || FileNameCountTextBox.Text == "" || FileNameCountTextBox.Text == "0")
                {
                    FileNameCountTextBox.Background = Brushes.DarkRed;
                    canpass = false;
                }
                if (FileNameIDTextBox.Text == null || FileNameIDTextBox.Text == "")
                {
                    FileNameIDTextBox.Background = Brushes.DarkRed;
                    canpass = false;
                }
                if (DataFileManager.TreeGameFiles.SelectedItem == null)
                {
                    DataFileManager.TreeGameFiles.Background = Brushes.DarkRed;
                    canpass = false;
                    //LibraryPixel.NotificationNegative("No File Selected", "Please select a file to make an editor with.");
                }
                
            }
            if (TheTextTableType == "TextFile") 
            {
                try { int.Parse(TextFirstLineTextBox.Text); } catch { TextFirstLineTextBox.Background = Brushes.DarkRed; }
                try { int.Parse(TextLastLineTextBox.Text); } catch { TextLastLineTextBox.Background = Brushes.DarkRed; } 

                if (FileManagerForTextFiles.TreeGameFiles.SelectedItem == null)
                {
                    FileManagerForTextFiles.TreeGameFiles.Background = Brushes.DarkRed;
                    canpass = false;
                }

                try
                {
                    if (int.Parse(TextLastLineTextBox.Text) - int.Parse(TextFirstLineTextBox.Text) < 0)
                    {
                        TextLastLineTextBox.Background = Brushes.DarkRed;
                        TextFirstLineTextBox.Background = Brushes.DarkRed;
                        canpass = false;
                    }
                }
                catch
                {
                    canpass = false;
                }
            }
            if (TheTextTableType == "Editor")
            {
                //Not supported
                canpass = false;
            }
            if (TheTextTableType == "Nothing") 
            {
                if (DTEData.DataTable != null) 
                {
                    int MaxDTRows = CheckMaxDataTableRows();
                    if (MaxDTRows < ItemsEditBox.LineCount) 
                    {
                        PixelWPF.LibraryPixel.NotificationNegative("Too Many Names", "" +
                            "Your trying to use more names then the DataTable File can support (reading past end of file)." +
                            "\n" +
                            "\nNameTable rows: " + ItemsEditBox.LineCount + 
                            "\nDataTable max rows: " + MaxDTRows);
                        return;
                    }

                    int CheckMaxDataTableRows() 
                    {
                        byte[] TheFileBytes = DTEData.DataTable.FileDataTable.FileBytes;
                        int MaxRows = (TheFileBytes.Length - DTEData.DataTable.DataTableStart) / DTEData.DataTable.DataTableRowSize;
                        return MaxRows;
                    }
                }
                //Supported but theres nothing to error check?
            }
            if (canpass == false)
            {
                return;
            }
            //End of error checking


            //Now we ask if the user is sure...            
            //List<Entry> MenuEntrys = new(); //First check for entrys using this editors name list.
            string MenuEntrys = "";
            foreach (DataTableEditorData DTEDataX in DTEData.WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                foreach (Entry entry in DTEDataX.MasterEntryList)
                {
                    if (entry.NewSubType == EntrySubTypes.Menu && entry.EntryTypeMenu != null)
                    {
                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor && entry.EntryTypeMenu.TextTableEditor != null)
                        {
                            if (entry.EntryTypeMenu.TextTableEditor.LinkedDTEEditor != null)
                            {
                                if (entry.EntryTypeMenu.TextTableEditor.LinkedDTEEditor.EditorKey == DTEData.EditorKey)
                                {
                                    MenuEntrys = MenuEntrys + entry.ParentEditor.EditorName + ": " + entry.Name + " (Offset: " + entry.RowOffset + ")\n";
                                    //MenuEntrys.Add(entry.EntryTypeMenu.TextTableEditor.LinkedDTEEditor.EditorName + ": " + entry.Name + " (Offset: " + entry.RowOffset + ")\n");
                                }
                            }
                        }
                    }
                }
            }

            
            //...if it's okay despite this editor already having a name list
            if (DTEData.NameTable != null)
            {
                if (PixelWPF.LibraryPixel.NotificationConfirm("Create a new name table?",
                "This will completly overwrite the current name list, including layout, notes, and tooltips. " +
                "\n\nPS: You can click the editor tab and change data on the right bar (press enter to apply) and it will make changes without overwriting the name list.") 
                    == false)
                {
                    return;
                }
            }

            //...if it's okay even if some entrys are using this editor's name list. 
            if (MenuEntrys != "")
            {
                if (PixelWPF.LibraryPixel.NotificationConfirm("NOTE: Name List in use",
                "Entrys using this name list:" +
                "\n\n" +
                //"\nEditor  |  Entry Name  |  Offset" + 
                MenuEntrys +
                "\n\nSet a new name list anyway?\n(This will not crash, i just wanted you to know what entrys will need to be updated)" +
                "")
                    == false)
                {
                    return;
                }
            }


            //Start of actually setting new name table.
            DTEData.NameTable = new();
            DTEData.NameTable.ItemList = new();

            if (DTEData.NameTable == null)
            {

            }
            if (DTEData.NameTable.ItemList == null)
            {

            }

            //TextTable texttable = DataTableEditorData.NameTable;
            //texttable.TextTableFile = ;

            //if (TheEditor is DataTableEditorData DTeditor)
            //{
            //    DataTableEditor standardEditor = new(database, DTeditor); //Generates the Editor UI using Editor data.
            //}

            //UserControlEditorCreator Maker = ;

            int itemIndex = 0;
            //This part determines how the list of item names is gotten.
            //Type 1: Use user inputs names to a textbox and it uses those..
            //Type 2: The user points to a file to get them directly. It users more user info + needs to convert the bytes via character encoding.


            if (TheTextTableType == "Advanced")
            {
                DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.Advanced;

                TreeViewItem TreeItem = DataFileManagerAdvanced.TreeGameFiles.SelectedItem as TreeViewItem;
                GameFile NameFile = TreeItem.Tag as GameFile;

                DTEData.NameTable.TextTableFile = NameFile;
                DTEData.NameTable.TextTableCharacterSet = AdvCharacterSetComboBox.Text;
                DTEData.NameTable.TextTableItemCount = int.Parse(AdvNameCountTextBox.Text);
                DTEData.NameTable.TextTableFirstNameID = int.Parse(AdvFirstNameIDTextbox.Text);

                int[] StartValues = AdvRowStartBox.Text.Split('\n')
                .Select(line => int.TryParse(line.Trim(), out int val) ? val : 0)
                .ToArray();

                int[] EndValues = AdvRowEndBox.Text.Split('\n')
                    .Select(line => int.TryParse(line.Trim(), out int val) ? val : 0)
                    .ToArray();

                for (int i = 0; i < DTEData.NameTable.TextTableItemCount; i++)
                {
                    int currentRowStart = StartValues.Length > i ? StartValues[i] : 0;
                    int currentRowEnd = EndValues.Length > i ? EndValues[i] : 0;

                    TextInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    ItemInfo.RowStart = currentRowStart;
                    ItemInfo.RowEnd = currentRowEnd;
                    DTEData.NameTable.ItemList.Add(ItemInfo);
                }

                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemNames(DTEData);
            }
            if (TheTextTableType == "DataFile")
            {
                DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.DataFile;

                TreeViewItem TreeItem = DataFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
                GameFile NameFile = TreeItem.Tag as GameFile;

                DTEData.NameTable.TextTableFile = NameFile;
                DTEData.NameTable.TextTableCharacterSet = CharacterSetComboBox.Text;
                DTEData.NameTable.TextTableStart = int.Parse(FileStartTextBox.Text);
                DTEData.NameTable.TextTableCharLimit = int.Parse(FileTextSizeTextBox.Text);
                DTEData.NameTable.TextTableRowSize = int.Parse(FileFullRowSizeTextBox.Text);
                DTEData.NameTable.TextTableItemCount = int.Parse(FileNameCountTextBox.Text);
                DTEData.NameTable.TextTableFirstNameID = int.Parse(FileNameIDTextBox.Text);

                for (int i = 0; i < DTEData.NameTable.TextTableItemCount; i++)
                {
                    TextInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    ItemInfo.RowStart = DTEData.NameTable.TextTableStart + (ItemInfo.ItemIndex * DTEData.NameTable.TextTableRowSize);
                    ItemInfo.RowEnd = ItemInfo.RowStart + DTEData.NameTable.TextTableCharLimit - 1;
                    DTEData.NameTable.ItemList.Add(ItemInfo);
                }

                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemNames(DTEData);

            }
            if (TheTextTableType == "TextFile")
            {
                DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.TextFile;
                DTEData.NameTable.TextTableCharacterSet = "";

                TreeViewItem TreeItem = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem;
                GameFile NameFile = TreeItem.Tag as GameFile;

                DTEData.NameTable.TextTableFile = NameFile;
                DTEData.NameTable.TextTableStart = int.Parse(TextFirstLineTextBox.Text);
                DTEData.NameTable.TextTableItemCount = int.Parse(TextLastLineTextBox.Text) - int.Parse(TextFirstLineTextBox.Text) + 1;
                DTEData.NameTable.TextTableFirstNameID = int.Parse(TextFileFileNameIDTextBox.Text);

                for (int i = 0; i < DTEData.NameTable.TextTableItemCount; i++)
                {
                    TextInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    DTEData.NameTable.ItemList.Add(ItemInfo);
                }

                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemNames(DTEData);
            }
            if (TheTextTableType == "Editor")
            {
                FileFullRowSizeTextBox.Background = null;
                if (EditorFileNameIDTextBox.Text == null || EditorFileNameIDTextBox.Text == "")
                {
                    FileStartTextBox.Background = Brushes.DarkRed;
                    return;
                }

                DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.Editor;
                DTEData.NameTable.TextTableCharacterSet = "";

                PixelWPF.LibraryPixel.NotificationNegative("Error: How did you even trigger this?",
                    "I didn't actually make any code for getting Editor text from another editor. Huh. Also now your gonna crash, and you should definatly report this!!! "
                    );
                Environment.FailFast(null); //Kills program instantly. 

            }
            if (TheTextTableType == "Nothing")
            {
                DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.Nothing;
                DTEData.NameTable.TextTableCharacterSet = "";

                DTEData.NameTable.TextTableFirstNameID = int.Parse(FileFirstNameIDNothingTextBox.Text);

                var lines = ItemsEditBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    TextInfo IInfo = new();
                    IInfo.ItemName = line;
                    IInfo.ItemIndex = itemIndex;
                    itemIndex++;
                    DTEData.NameTable.ItemList.Add(IInfo);
                }
                DTEData.NameTable.TextTableItemCount = itemIndex; //this might fix symbology not working for custom name lists. Revisit if its still broken.

            }


            //StandardEditorSetup TheSetup = new StandardEditorSetup();
            //TheSetup.SetupDataTableEditorMiddle(Database, DTEData);
            //DTEData.EditorLeftBar.LeftBarXaml.SetupDataTableEditorLeftBar(DTEData.WorkshopXaml, Database, DTEData);
            //DataTableEditor standardEditor = new(DTEData.WorkshopXaml.WorkshopData, DTEData);
            //DTEData.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            DTEData.DTEXaml.GenerateUI();
            Closethis();
        }

        

        private void SaveDescriptions()
        {
            //Supported Links: DataFile, TextFile.

            //Error Checking
            TabItem tabItem = TabControlListType.SelectedItem as TabItem;
            string TheTextTableType = tabItem.Tag as string;

            bool canpass = true;
            if (TheTextTableType == "DataFile")
            {
                FileFullRowSizeTextBox.Background = null;
                FileTextSizeTextBox.Background = null;
                FileStartTextBox.Background = null;                
                DataFileManager.TreeGameFiles.Background = null;

                if (FileFullRowSizeTextBox.Text == null || FileFullRowSizeTextBox.Text == "" || FileFullRowSizeTextBox.Text == "0")
                {
                    FileFullRowSizeTextBox.Background = Brushes.Red;
                    canpass = false;
                }
                if (FileTextSizeTextBox.Text == null || FileTextSizeTextBox.Text == "" || FileTextSizeTextBox.Text == "0")
                {
                    FileTextSizeTextBox.Background = Brushes.DarkRed;
                    canpass = false;
                }
                if (FileStartTextBox.Text == null || FileStartTextBox.Text == "")
                {
                    FileStartTextBox.Background = Brushes.DarkRed;
                    canpass = false;
                }                
                if (DataFileManager.TreeGameFiles.SelectedItem == null)
                {
                    DataFileManager.TreeGameFiles.Background = Brushes.DarkRed;
                    canpass = false;
                    //LibraryPixel.NotificationNegative("No File Selected", "Please select a file to make an editor with.");
                }

            }
            if (TheTextTableType == "TextFile")
            {
                try { int.Parse(TextFirstLineTextBox.Text); } catch { TextFirstLineTextBox.Background = Brushes.DarkRed; canpass = false; }                

                if (FileManagerForTextFiles.TreeGameFiles.SelectedItem == null)
                {
                    FileManagerForTextFiles.TreeGameFiles.Background = Brushes.DarkRed;
                    canpass = false;
                }

            }
            if (TheTextTableType == "Editor")
            {
                //Not supported
                canpass = false;
            }
            if (TheTextTableType == "Nothing")
            {
                //Not supported
                canpass = false;
            }
            if (canpass == false)
            {
                return;
            }
            //End of error checking

            //Start of actually setting new description table

            

            if (DTEData.DescriptionTableList.Count == 0)
            {
                //There is no important description data, so i probably don't need a confirm prompt...
                //if (PixelWPF.LibraryPixel.NotificationConfirm("Create a new description table?",
                //"This will completly overwrite the current name list, including layout, notes, and tooltips. " +
                //"\n\nPS: You can click the editor tab and change data on the right bar (press enter to apply) and it will make changes without overwriting the name list.") == false)
                //{
                //    return;
                //}

                TextTable NEW_DescriptionTable = new();
                DTEData.DescriptionTableList.Add(NEW_DescriptionTable);
            }
            TextTable DescriptionTable = DTEData.DescriptionTableList[0];


            if (TheTextTableType == "Advanced")
            {
                DescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.Advanced;

                TreeViewItem TreeItem = DataFileManagerAdvanced.TreeGameFiles.SelectedItem as TreeViewItem;
                GameFile NameFile = TreeItem.Tag as GameFile;                

                DescriptionTable.TextTableFile = NameFile;

                DescriptionTable.TextTableCharacterSet = AdvCharacterSetComboBox.Text;
                DescriptionTable.TextTableItemCount = int.Parse(AdvNameCountTextBox.Text);
                DescriptionTable.TextTableFirstNameID = int.Parse(AdvFirstNameIDTextbox.Text);

                int[] StartValues = AdvRowStartBox.Text.Split('\n')
                .Select(line => int.TryParse(line.Trim(), out int val) ? val : 0)
                .ToArray();

                int[] EndValues = AdvRowEndBox.Text.Split('\n')
                    .Select(line => int.TryParse(line.Trim(), out int val) ? val : 0)
                    .ToArray();

                for (int i = 0; i < DescriptionTable.TextTableItemCount; i++)
                {
                    int currentRowStart = StartValues.Length > i ? StartValues[i] : 0;
                    int currentRowEnd = EndValues.Length > i ? EndValues[i] : 0;

                    TextInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    ItemInfo.RowStart = currentRowStart;
                    ItemInfo.RowEnd = currentRowEnd;
                    DescriptionTable.ItemList.Add(ItemInfo);
                }

            }
            if (ComboBoxListType.Text == "Link to Data File")
            {
                DescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.DataFile;

                TreeViewItem FileItem = DataFileManager.TreeGameFiles.SelectedItem as TreeViewItem; //The file the user is pulling item descriptions from.
                if (FileItem.Tag as GameFile != null)
                {
                    DescriptionTable.TextTableFile = FileItem.Tag as GameFile;
                }

                DescriptionTable.TextTableCharacterSet = CharacterSetComboBox.Text;
                DescriptionTable.TextTableStart = int.Parse(FileStartTextBox.Text);
                DescriptionTable.TextTableCharLimit = int.Parse(FileTextSizeTextBox.Text);
                DescriptionTable.TextTableRowSize = int.Parse(FileFullRowSizeTextBox.Text);



            }
            if (ComboBoxListType.Text == "Link to Text File")
            {
                DescriptionTable.TextTableStart = int.Parse(TextFirstLineTextBox.Text);

                DescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.TextFile;

                TreeViewItem FileItem = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem; //The file the user is pulling item descriptions from.

                if (FileItem.Tag as GameFile != null)
                {
                    DescriptionTable.TextTableFile = FileItem.Tag as GameFile;
                }


            }
            //Link to Editor is intentionally disabled and cannot be used for descriptions. (I can't think of any reason to allow it)
            //Link to Nothing is also bocked. 


            //This part updates the current description, and i made select retrigger decoding and moved description textbox creation to decoding, so it also creates the textbox instantly if the user just added a new description.


            //Database.WorkshopXaml.TreeViewSelectionEnabled = false;
            //TreeViewItem TheItem = DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
            //TheItem.IsSelected = false;
            //Database.WorkshopXaml.TreeViewSelectionEnabled = true;
            //TheItem.IsSelected = true;

            DTEData.DTEXaml.GenerateUI();
            Closethis();
        }

        private void SaveMenu()
        {
            //Supported Links: DataFile, TextFile, Editor, Nothing.

            if (ComboBoxListType.Text == "Link to Data File")
            {

                //NEW table version.
                TreeViewItem Item = DataFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
                if (Item == null) { return; }
                GameFile TheFile = Item.Tag as GameFile;
                if (TheFile == null) { return; }
                EntryClass.EntryTypeMenu.TextTableDataFile = new();
                TextTable texttable = EntryClass.EntryTypeMenu.TextTableDataFile;

                texttable.TextTableFile = TheFile;
                if (AsciiItem.IsSelected == true) { texttable.TextTableCharacterSet = "ASCII+ANSI"; }
                if (ShiftJISItem.IsSelected == true) { texttable.TextTableCharacterSet = "Shift-JIS"; }
                texttable.TextTableStart = Int32.Parse(FileStartTextBox.Text);
                texttable.TextTableRowSize = Int32.Parse(FileFullRowSizeTextBox.Text);
                texttable.TextTableCharLimit = Int32.Parse(FileTextSizeTextBox.Text);
                texttable.TextTableFirstNameID = Int32.Parse(FileNameIDTextBox.Text);
                texttable.TextTableItemCount = Int32.Parse(FileNameCountTextBox.Text);
                

                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.DataFile;
                texttable.TextTableLinkType = TextTable.TextTableLinkTypes.DataFile;

                for (int i = 0; i < texttable.TextTableItemCount; i++)
                {
                    TextInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    ItemInfo.RowStart = texttable.TextTableStart + (ItemInfo.ItemIndex * texttable.TextTableRowSize);
                    ItemInfo.RowEnd = ItemInfo.RowStart + texttable.TextTableCharLimit - 1;
                    texttable.ItemList.Add(ItemInfo);
                }
                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemTexts(texttable);

            }
            if (ComboBoxListType.Text == "Link to Text File")
            {
                //NEW
                TreeViewItem Item = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem;
                if (Item == null) { return; }
                GameFile TheFile = Item.Tag as GameFile;
                if (TheFile == null) { return; }

                EntryClass.EntryTypeMenu.TextTableTextFile = new();
                TextTable texttable = EntryClass.EntryTypeMenu.TextTableTextFile;

                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.TextFile;
                texttable.TextTableLinkType = TextTable.TextTableLinkTypes.TextFile;

                texttable.TextTableFile = TheFile;
                texttable.TextTableStart = int.Parse(TextFirstLineTextBox.Text);
                texttable.TextTableItemCount = Int32.Parse(TextLastLineTextBox.Text) - Int32.Parse(TextFirstLineTextBox.Text) + 1;
                texttable.TextTableLinkType = TextTable.TextTableLinkTypes.TextFile;
                texttable.TextTableFirstNameID = Int32.Parse(TextFileFileNameIDTextBox.Text);

                
                for (int i = 0; i < texttable.TextTableItemCount; i++)
                {
                    TextInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    texttable.ItemList.Add(ItemInfo);
                }
                CharacterSetManager CharacterManager = new();
                CharacterManager.DecodeAllItemTexts(texttable);

            }

            if (ComboBoxListType.Text == "Link to Editor")
            {
                
                TreeViewItem Item = EditorsTreeView.SelectedItem as TreeViewItem;
                if (Item == null) { return; }
                Editor TheEditor = Item.Tag as Editor;
                if (TheEditor == null) { return; }
                
                EntryClass.EntryTypeMenu.TextTableEditor = new();
                TextTable texttable = EntryClass.EntryTypeMenu.TextTableEditor;

                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.Editor;
                texttable.TextTableLinkType = TextTable.TextTableLinkTypes.Editor;

                texttable.LinkedDTEEditor = TheEditor.DataTableEditorData;//Important
                texttable.TextTableFirstNameID = Int32.Parse(EditorFileNameIDTextBox.Text);

                //Instead of this, i could just have entrys check the linked editor's name count...?
                //Also would this break on editors with a folder item? >_>
                texttable.TextTableItemCount = 0;

            }

            if (ComboBoxListType.Text == "Link to Nothing")
            {
                
                //NEW
                EntryClass.EntryTypeMenu.TextTableNothing = new();
                TextTable texttable = EntryClass.EntryTypeMenu.TextTableNothing;

                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.Nothing;
                texttable.TextTableLinkType = TextTable.TextTableLinkTypes.Nothing;

                texttable.TextTableFirstNameID = int.Parse(FileFirstNameIDNothingTextBox.Text);

                var lines = ItemsEditBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                int NIndex = -1;
                foreach (string line in lines)
                {
                    NIndex++;
                    if (line == "") { continue; }
                    TextInfo IInfo = new();
                    IInfo.ItemName = line;
                    IInfo.ItemIndex = NIndex;                    
                    texttable.ItemList.Add(IInfo);
                }                



            }
            if (ComboBoxListType.Text == "Link to Advanced") 
            {
                //EntryClass.EntryTypeMenu.TextTableAdvanced = new();
                //TextTable texttable = EntryClass.EntryTypeMenu.TextTableAdvanced;

                //EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.DataFileAdvanced;
                //texttable.TextTableLinkType = TextTable.TextTableLinkTypes.Advanced;
            }




            MenuPanelEdit.Visibility = Visibility.Collapsed;

            Database.EntryManagerOLD.ChangeEntryType(Database, EntrySubTypes.Menu, Workshop, EntryClass);

            //DTEData.DTEXaml.GenerateUI();
            Closethis();
            DTEMethods.EntryActivate(EntryClass);
        }

        

        private void ButtonClickDeleteTextTable(object sender, RoutedEventArgs e)
        {
            if (NamesMode == true) { DeleteNameTable();  return; }
            if (DescriptionMode == true) { DeleteDescriptionTable(); return; }
            if (MenuMode == true) { DeleteMenuTable(); return; }
        }

        private void DeleteNameTable() 
        {
            DTEData.NameTable = null;
            //DTEData.NameTable.ItemList = new();

            DTEData.DTEXaml.GenerateUI();
            Closethis();
        }

        private void DeleteDescriptionTable()
        {
            DTEData.DescriptionTableList = new();

            DTEData.DTEXaml.GenerateUI();
            Closethis();
        }

        private void DeleteMenuTable()
        {
            PixelWPF.LibraryPixel.Notification("UHHHHH","SOOO, i didn't feel like programming the ability to empty a menu's text table (Mostly because this would be almost never used), so i just didn't. \n\nAlso currently the program would fail to save an editor if it had a menu with a text table link. \n\nI'll add support for this some other time :p ");
            //Closethis();
        }

        private void ExitWithoutSaving(object sender, RoutedEventArgs e)
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }
        }

        private void Closethis() 
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }

            DTEData.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////// SAVE TEXT TABLES /////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////// OTHER //////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SetupForNewDTEditorNamesOLD(WorkshopData wdata) 
        {
            //IMPORTANT NOTE: THIS ONLY WORKS FOR NAMES FOR AN ENTRY (MENU MODE). 
            //IF I EVER ADD SUPPORT FOR NAMES FROM EDITORS FOR THINGS OTHER THEN ENTRYS, I MUST RETHINK?
            //PS: DON'T LET EDITORS GET NAMES FROM EDITORS WITHOUT TESTING IF ANY INFINITE LOOPS WILL OCCUR.
            //AND DONT FORGET EXTENDED LOOPS, LIKE A gets from B, B gets from C, C gets from A.
            //Well, i guess as entrys load AFTER editor names, so maybe this won't be a problem after all?

            Database = wdata;

            EditorsTreeView.Items.Clear();
            foreach (var Editor2 in Database.GameEditors)
            {
                Editor Editor = Editor2;

                TreeViewItem Item = new TreeViewItem();
                Item.Header = Editor.EditorName;
                Item.Tag = Editor;

                EditorsTreeView.Items.Add(Item);
            }

            //Might be unnecessary?
            //if (EntryClass != null)
            //{
            //    if (EntryClass.EntryTypeMenu.LinkedEditor != null)
            //    {
            //        foreach (TreeViewItem TItem in EditorsTreeView.Items)
            //        {
            //            Editor TheEditor = TItem.Tag as Editor;
            //            if (TheEditor == EntryClass.EntryTypeMenu.LinkedEditor)
            //            {
            //                TItem.IsSelected = true;
            //                EditorFileNameIDTextBox.Text = EntryClass.EntryTypeMenu.FirstNameID.ToString();
            //                break;
            //            }
            //        }
            //    }
            //}
        }
        //public event EventHandler RequestClose;
        //private void CancelEditorCreation(object sender, RoutedEventArgs e)
        //{
        //    RequestClose?.Invoke(this, EventArgs.Empty);
        //}


        


        private void FileLinkDebug(object sender, RoutedEventArgs e)
        {
            FileStartTextBox.Text = "9";
            FileFullRowSizeTextBox.Text = "176";
            FileTextSizeTextBox.Text = "31";
            FileNameIDTextBox.Text = "0";
            FileNameCountTextBox.Text = "37";
            
            foreach (TreeViewItem Item3 in LibraryGES.GetALLTreeViewItems(DataFileManager.TreeGameFiles)) //FileTreeExtraTable.Items
            {
                GameFile TheFile = Item3.Tag as GameFile;
                if (TheFile.FileName == "skill.bin")
                {
                    Item3.IsSelected = true;
                    break;
                }
            }

        }

        public void ShowMenuDataFileInUse(object sender, RoutedEventArgs e)
        {            
            foreach (TreeViewItem Item3 in LibraryGES.GetALLTreeViewItems(DataFileManager.TreeGameFiles)) //FileTreeExtraTable.Items
            {
                if (Item3.Tag == DTEData.DescriptionTableList[0].TextTableFile)
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
        }

        

        
        

        private void TextSourceDropdownClosed(object sender, EventArgs e)
        {
            
            if (ComboBoxListType.Text == "Link to Data File")
            {
                TabControlListType.SelectedIndex = 0;
            }
            if (ComboBoxListType.Text == "Link to Text File")
            {
                TabControlListType.SelectedIndex = 1;
            }
            if (ComboBoxListType.Text == "Link to Editor")
            {
                TabControlListType.SelectedIndex = 2;
            }
            if (ComboBoxListType.Text == "Link to Nothing")
            {
                TabControlListType.SelectedIndex = 3;
            }
            if (ComboBoxListType.Text == "Link to Advanced")
            {
                Tab5.IsSelected = true;
                //TabControlListType.SelectedIndex = 4;
            }

        }


        private void TextboxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool isDigit = char.IsDigit(e.Text, e.Text.Length - 1); // Check if its is a number or a minus sign (for signed numbers)
            bool isMinusSign = e.Text == "-";

            // Check if the minus sign is at the start of the input (cursor at the start)
            if (isMinusSign)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null && textBox.CaretIndex != 0)
                {
                    e.Handled = true; // Only allow minus at the beginning
                }
            }
            else if (!isDigit)
            {
                e.Handled = true; // If not a number, mark the event as handled
            }
        }

        private void TextFileSetAllRowsButton(object sender, RoutedEventArgs e)
        {
            if (FileManagerForTextFiles.TreeGameFiles.SelectedItem == null) { return; }

            TreeViewItem TheItem = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem;
            GameFile TheFile = TheItem.Tag as GameFile;

            string fullText = Encoding.UTF8.GetString(TheFile.FileBytes);
            string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            int LineCount = Math.Max(lines.Length - 1, 0) ;

            //int LineCount = Math.Min(lines.Length, 255);

            TextLastLineTextBox.Text = LineCount.ToString();
        }

        private void RunUpdateTextFileNameListPreview(object sender, TextChangedEventArgs e)
        {
            UpdateTextFileNameListPreview();
        }

        public void UpdateTextFileNameListPreview()
        {
            //TextFileOrigonalTextbox
            TextFilePreviewTextbox.Text = ""; // Clear it first
            TextFileOrigonalTextbox.Text = ""; // Clear it first

            try 
            {
                TreeViewItem TreeItem = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem;
                if (TreeItem != null) 
                {
                    GameFile NameFile = TreeItem.Tag as GameFile;

                    string fullText = Encoding.UTF8.GetString(NameFile.FileBytes);
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        TextFileOrigonalTextbox.Text += (i) + ": " + lines[i] + "\n";
                    }
                }
                
            } catch { TextFileOrigonalTextbox.Text = "Error for some reason :3"; }

            try 
            {
                TreeViewItem TreeItem = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem;
                if (TreeItem != null) 
                {
                    GameFile NameFile = TreeItem.Tag as GameFile;

                    string fullText = Encoding.UTF8.GetString(NameFile.FileBytes);
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    int id = 0;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (i < int.Parse(TextFirstLineTextBox.Text)) { continue; }
                        if (i > int.Parse(TextLastLineTextBox.Text)) { continue; }
                        TextFilePreviewTextbox.Text += (id + int.Parse(TextFileFileNameIDTextBox.Text)) + ": " + lines[i] + "\n";
                        id++;
                    }
                }
                
            } 
            catch 
            {
                TextFilePreviewTextbox.Text = "Error for some reason :3";
            }
            
        }


        private void DataFileTextboxesTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDataFileNamesPreview();
        }

        private void DataFileCharacterSetDropdownClosed(object sender, EventArgs e)
        {
            UpdateDataFileNamesPreview();
        }


        
        public void UpdateDataFileNamesPreview() 
        {
            if (DataFileManager.TreeGameFiles.SelectedItem == null) 
            {
                return;
            }

            TreeViewItem Item = DataFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            GameFile TheFile = Item.Tag as GameFile;
            if (TheFile == null) { return; }

            int AFirstNameID = 0;
            int ANameCount = 0;
            int AStartByte = 0;
            int ARowSize = 0;
            int ATextSize = 0;
            Encoding encoding = null; ;
            string AEncoding = "ASCII+ANSI";

            try
            {
                AFirstNameID = Int32.Parse(FileNameIDTextBox.Text);
                ANameCount = Int32.Parse(FileNameCountTextBox.Text);
                AStartByte = Int32.Parse(FileStartTextBox.Text);
                ARowSize = Int32.Parse(FileFullRowSizeTextBox.Text);
                ATextSize = Int32.Parse(FileTextSizeTextBox.Text);
                AEncoding = CharacterSetComboBox.Text;

                if (AEncoding == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (AEncoding == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { throw new InvalidOperationException("Unsupported character set"); }
                DataFileNamesPreviewTextbox.Text = "";
            }
            catch
            {
                DataFileNamesPreviewTextbox.Text = "ERROR";
                return;
            }

            

            try
            {
                for (int i = 0; i < (ANameCount); i++)
                {

                    byte[] bytes = new byte[ATextSize];
                    for (int RowIndex = 0; RowIndex < ATextSize; RowIndex++)
                    {
                        bytes[RowIndex] = TheFile.FileBytes[AStartByte + (i * ARowSize) + RowIndex];
                    }

                    DataFileNamesPreviewTextbox.Text = DataFileNamesPreviewTextbox.Text + (i + AFirstNameID) + ": " + encoding.GetString(bytes) + "\n"; // + 1

                }
            }
            catch 
            {
                DataFileNamesPreviewTextbox.Text = "ERROR";
                return;
            }
            
        }







        private void EditorsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateEditorNamespreview();
        }

        private void EditorFirstNameIDTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEditorNamespreview();
        }

        private void UpdateEditorNamespreview() 
        {
            if (EditorNamesPreviewTextbox == null || EditorsTreeView.SelectedItem == null)
            {
                return;
            }

            EditorNamesPreviewTextbox.Text = "";
                        


            TreeViewItem Item = EditorsTreeView.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            Editor TheEditor = Item.Tag as Editor;
            if (TheEditor == null) { return; }


            int TheFirstNameID = 0;
            int ANameCount = 0;

            try
            {
                TheFirstNameID = Int32.Parse(EditorFileNameIDTextBox.Text);
            }
            catch
            {

            }

            foreach (TextInfo TheItem in TheEditor.DataTableEditorData.NameTable.ItemList)
            {
                if (TheItem.IsFolder == false)
                {
                    ANameCount++;
                }
            }


            for (int i = 0; i < ANameCount; i++)
            {

                string TheItemName = "ERROR";

                foreach (TextInfo itemInfo in TheEditor.DataTableEditorData.NameTable.ItemList)
                {
                    if (itemInfo.ItemIndex == i)
                    {
                        if (itemInfo.IsFolder == false)
                        {
                            TheItemName = itemInfo.ItemName;
                            break;
                        }
                    }
                }


                EditorNamesPreviewTextbox.Text = EditorNamesPreviewTextbox.Text + (i + TheFirstNameID) + ": " + TheItemName + "\n";

            }
        }

        private void NothingNameListTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            //return;

            ItemsNumBox.Clear();
            string[] lines = ItemsEditBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                sb.AppendLine(i.ToString());
            }

            ItemsNumBox.Text = sb.ToString();

        }


        private void NothingItemsEditBoxPreviewKeyDown(object sender, KeyEventArgs e) //more GPT code to stop the user from axidentally deleting lines in the Nothing Name Textbox.
        {
            //var tb = (TextBox)sender;
            //int lineCountBefore = tb.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).Length;

            //string newText = tb.Text;
            //int caret = tb.CaretIndex;
            //int selStart = tb.SelectionStart;
            //int selLength = tb.SelectionLength;

            //if (e.Key == Key.Back)
            //{
            //    if (selLength > 0)
            //    {
            //        newText = newText.Remove(selStart, selLength);
            //    }
            //    else if (caret > 0)
            //    {
            //        newText = newText.Remove(caret - 1, 1);
            //    }
            //}
            //else if (e.Key == Key.Delete)
            //{
            //    if (selLength > 0)
            //    {
            //        newText = newText.Remove(selStart, selLength);
            //    }
            //    else if (caret < newText.Length)
            //    {
            //        newText = newText.Remove(caret, 1);
            //    }
            //}
            //else return; // Let other keys through

            //int lineCountAfter = newText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).Length;


            //if (lineCountAfter != lineCountBefore)
            //{                
            //    e.Handled = true; // Block if line count decreased
            //}
        }



        //ADVANCED STARTS HERE//
        private void AdvNameCountTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAdvancedPreview();
        }

        private void AdvFirstNameTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAdvancedPreview();
        } 

        private void UpdateAdvancedPreview()
        {
            if (AdvNamesPreviewTextbox == null) { return; } //Stops a strange launch error / crash

            int AFirstNameID = 0;
            int ANameCount = 0;
            Encoding encoding = null;
            string AEncoding = "ASCII+ANSI";

            try
            {
                AFirstNameID = Int32.Parse(AdvFirstNameIDTextbox.Text);
                ANameCount = Int32.Parse(AdvNameCountTextBox.Text);
                AEncoding = AdvCharacterSetComboBox.Text;

                if (AEncoding == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (AEncoding == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { throw new InvalidOperationException("Unsupported character set"); }

                AdvItemsBox.Clear();
                for (int i = 0 + AFirstNameID; i < ANameCount + AFirstNameID; i++)
                {
                    AdvItemsBox.Text = AdvItemsBox.Text + i.ToString() + "\n";
                }
            }
            catch
            {
                AdvNamesPreviewTextbox.Text = "ERROR" + "\n\n" + "Check Info Panel...";
                return;
            }


            if (DataFileManagerAdvanced.TreeGameFiles.SelectedItem == null)
            {
                AdvNamesPreviewTextbox.Text = "ERROR" + "\n\n" + "No Game File Selected...";
                return;
            }

            TreeViewItem Item = DataFileManagerAdvanced.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            GameFile TheFile = Item.Tag as GameFile;
            if (TheFile == null) { return; }

            

            //string[] StartValues; //AdvRowStartBox.Text
            //string[] EndValues; //AdvRowEndBox.Text
            //try
            //{
            //    for (int i = 0; i < (ANameCount); i++)
            //    {

            //        byte[] bytes = new byte[ATextSize];
            //        for (int RowIndex = 0; RowIndex < ATextSize; RowIndex++)
            //        {
            //            bytes[RowIndex] = TheFile.FileBytes[AStartByte + (i * ARowSize) + RowIndex];
            //        }

            //        AdvNamesPreviewTextbox.Text = AdvNamesPreviewTextbox.Text + (i + AFirstNameID) + ": " + encoding.GetString(bytes) + "\n"; // + 1

            //    }
            //}
            //catch
            //{
            //    AdvNamesPreviewTextbox.Text = "ERROR";
            //    return;
            //}


            int[] StartValues = AdvRowStartBox.Text.Split('\n')
                .Select(line => int.TryParse(line.Trim(), out int val) ? val : 0)
                .ToArray();

            int[] EndValues = AdvRowEndBox.Text.Split('\n')
                .Select(line => int.TryParse(line.Trim(), out int val) ? val : 0)
                .ToArray();

            // Use a StringBuilder for the preview text - it's MUCH faster than string += string
            StringBuilder sb = new StringBuilder();

            try
            {
                AdvNamesPreviewTextbox.Clear();

                for (int i = 0; i < ANameCount; i++)
                {
                    int currentRowStart = StartValues.Length > i ? StartValues[i] : 0;
                    int currentRowEnd = EndValues.Length > i ? EndValues[i] : 0;

                    // Calculate size
                    int currentRowSize = (currentRowEnd + 1) - currentRowStart;

                    // 1. Validation Logic
                    bool isValidRead = true;
                    string previewName = "??????????";

                    // Condition A: Size must be greater than 0
                    // Condition B: Size cannot exceed 10,000 bytes (Safety Threshold)
                    // Condition C: Start must not be beyond file length (Extra Safety)
                    if (currentRowSize <= 0 || currentRowSize > 10000 || currentRowStart + currentRowSize > TheFile.FileBytes.Length)
                    {
                        isValidRead = false;
                    }

                    if (isValidRead)
                    {
                        try
                        {
                            byte[] bytes = new byte[currentRowSize];

                            for (int byteIndex = 0; byteIndex < currentRowSize; byteIndex++)
                            {
                                bytes[byteIndex] = TheFile.FileBytes[currentRowStart + byteIndex];
                            }

                            previewName = encoding.GetString(bytes).Replace("\0", "");
                        }
                        catch
                        {
                            isValidRead = false; // Fallback if something weird happens during read
                        }
                    }

                    // Always append the line, but use previewName (either the text or "???")
                    sb.AppendLine($"{i + AFirstNameID}: {previewName}");
                }

                AdvNamesPreviewTextbox.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Advanced Preview Error: " + ex.Message);
            }

        }

        private void AdvancedDebug(object sender, RoutedEventArgs e)
        {
            
            AdvRowStartBox.Text = "188426\r\n188437\r\n188451\r\n188463\r\n188484";
            AdvRowEndBox.Text = "188430\r\n188442\r\n188455\r\n188472\r\n188488";

        }

        private void AdvancedRowStartTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAdvancedPreview(); 
        }

        private void AdvancedRowEndTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAdvancedPreview();
        }


        private void NothingSetEmptyUsedLines(object sender, RoutedEventArgs e)
        {
            SetupNothingList(false);
        }

        private void NothingSetAllUsedLinesToQuestionMark(object sender, RoutedEventArgs e) 
        {
            SetupNothingList(true);
        }
        private void SetupNothingList(bool OverwriteText)
        {
            int Goal = DTEData.NameTable.TextTableItemCount;
            if (Goal == 0) //Makes editors that don't get item names from a file work with sheet exports.
            {
                foreach (var Item in DTEData.NameTable.ItemList)
                {
                    if (Item.IsFolder == false)
                    {
                        Goal++;
                    }
                }
            }

            List<long> UsedValues = new();

            Dictionary<long, NumberCount> counts = new();

            Entry TheEntry = DTEData.EntryClass;

            for (int i = 0; i < Goal; i++)
            {
                long ValueNum = 0;

                int offset = DTEData.DataTable.DataTableStart + (i * TheEntry.DataTableRowSize) + TheEntry.RowOffset;

                bool isSigned = TheEntry.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed;

                if (TheEntry.Endianness == "1")
                {
                    byte b = DTEData.DataTable.FileDataTable.FileBytes[offset];
                    ValueNum = isSigned ? (sbyte)b : b;
                }
                else if (TheEntry.Endianness == "2B")
                {
                    if (isSigned)
                        ValueNum = BitConverter.ToInt16(DTEData.DataTable.FileDataTable.FileBytes, offset);
                    else
                        ValueNum = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, offset);
                }
                else if (TheEntry.Endianness == "2L")
                {
                    byte[] bytes = DTEData.DataTable.FileDataTable.FileBytes.Skip(offset).Take(2).ToArray();
                    Array.Reverse(bytes);
                    if (isSigned)
                        ValueNum = BitConverter.ToInt16(bytes, 0);
                    else
                        ValueNum = BitConverter.ToUInt16(bytes, 0);
                }
                else if (TheEntry.Endianness == "4B")
                {
                    if (isSigned)
                        ValueNum = BitConverter.ToInt32(DTEData.DataTable.FileDataTable.FileBytes, offset);
                    else
                        ValueNum = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, offset);
                }
                else if (TheEntry.Endianness == "4L")
                {
                    byte[] bytes = DTEData.DataTable.FileDataTable.FileBytes.Skip(offset).Take(4).ToArray();
                    Array.Reverse(bytes);
                    if (isSigned)
                        ValueNum = BitConverter.ToInt32(bytes, 0);
                    else
                        ValueNum = BitConverter.ToUInt32(bytes, 0);
                }


                if (counts.ContainsKey(ValueNum))
                {
                    counts[ValueNum].Count++;
                    counts[ValueNum].RowIndices.Add(i);
                }
                else
                {
                    UsedValues.Add(ValueNum);
                    counts[ValueNum] = new NumberCount { Number = ValueNum, Count = 1, RowIndices = new List<int> { i } };
                }

            }

            List<long> sortedKeys = counts.Keys.ToList();
            sortedKeys.Sort();

            foreach (long key in sortedKeys)
            {
                //EntryValueInsightDataGrid.Items.Add(counts[key]);
            }


            //I had AI code this entire part below this line.
            var usedSet = new HashSet<int>(UsedValues.Select(x => (int)x));

            long maxVal = UsedValues.Any() ? (long)UsedValues.Max() : 0;
            long size = Math.Max((long)ItemsEditBox.LineCount, maxVal + 1);

            var lines = ItemsEditBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < size; i++)
            {
                if (i > 0) sb.Append("\n");

                // Grab the existing text if we are within bounds
                string existingLine = (i < lines.Length) ? lines[i] : "";
                bool lineHasContent = !string.IsNullOrEmpty(existingLine);

                // Logic: 
                // If we ARE allowed to overwrite OR if the line is currently empty/new...
                // ...and it's marked as USED, then write "USED".
                if (usedSet.Contains(i) && (OverwriteText || !lineHasContent))
                {
                    sb.Append("???");
                }
                else
                {
                    // Otherwise, keep what was there (even if it's an empty string)
                    sb.Append(existingLine);
                }
            }

            ItemsEditBox.Text = sb.ToString();
        }

        private void NothingSetAllLinesToEmpty(object sender, RoutedEventArgs e)
        {
            
            int size = ItemsEditBox.LineCount;
            string newtext = "";
            for (int i = 1; i < size; i++) 
            {
                newtext = newtext + "\n";
            }
            ItemsEditBox.Text = newtext;
            //var lines = ItemsEditBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            //int NIndex = -1;
            //foreach (string line in lines)
            //{
            //    NIndex++;
            //    if (line == "") { continue; }
            //    TextInfo IInfo = new();
            //    IInfo.ItemName = line;
            //    IInfo.ItemIndex = NIndex;
            //    texttable.ItemList.Add(IInfo);
            //}
        }

        private void NothingSetEmptyLinesToQuestionMarks(object sender, RoutedEventArgs e)
        {
            int size = ItemsEditBox.LineCount;
            var lines = ItemsEditBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string newtext = lines[0];
            if (newtext == "") { newtext = "???"; }
            for (int i = 1; i < size; i++)
            {
                if (lines[i] != "") { newtext = newtext + "\n" + lines[i]; }
                if (lines[i] == "") { newtext = newtext + "\n???"; }
            }            
            ItemsEditBox.Text = newtext;
        }
        private void NothingSetAllLinesToQuestionMarks(object sender, RoutedEventArgs e)
        {
            int size = ItemsEditBox.LineCount;
            string newtext = "???";
            for (int i = 1; i < size; i++)
            {
                newtext = newtext + "\n???";
            }
            ItemsEditBox.Text = newtext;
        }
        

        
    }
}
