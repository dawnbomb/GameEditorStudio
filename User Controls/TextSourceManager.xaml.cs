using System;
using System.Collections.Generic;
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
using static GameEditorStudio.Entry;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TextSourceManager.xaml
    /// </summary>
    public partial class TextSourceManager : UserControl
    {
        WorkshopData Database { get; set; }
        Entry EntryClass { get; set; }
        ListBox EntryListBox { get; set; }
        Workshop Workshop { get; set; }
        bool DescriptionMode { get; set; } = false;
        bool MenuMode { get; set; } = false; 

        public TextSourceManager()
        {
            InitializeComponent();

            Tab1.Visibility = Visibility.Collapsed;
            Tab2.Visibility = Visibility.Collapsed;
            Tab22.Visibility = Visibility.Collapsed;
            Tab3.Visibility = Visibility.Collapsed;

            ComboBoxListType.SelectedIndex = 0;

            this.Loaded += new RoutedEventHandler(LoadEvent);
            //Database = database;

            TextFileNameIDLabel.Visibility = Visibility.Collapsed;
            TextFileFileNameIDTextBox.Visibility = Visibility.Collapsed;
            TextLinkNameIDHelpText.Visibility = Visibility.Collapsed;

            #if DEBUG
            #else
            DataFileDebugButton.Visibility = Visibility.Collapsed; //Remove the debug button in release builds.
            #endif
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
                ButtonExitWithoutSaving.Visibility = Visibility.Collapsed;
                ButtonSaveAndExit.Visibility = Visibility.Collapsed;

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

            if (parentWindow is Workshop workshopWindow)
            {
                Database = workshopWindow.MyDatabase;


                EditorsTreeView.Items.Clear();
                foreach (var Editor2 in Database.GameEditors)
                {
                    Editor Editor = Editor2.Value;

                    TreeViewItem Item = new TreeViewItem();
                    Item.Header = Editor.EditorName;
                    Item.Tag = Editor;

                    EditorsTreeView.Items.Add(Item);
                }

                if (EntryClass != null) 
                {
                    if (EntryClass.EntryTypeMenu.LinkedEditor != null)
                    {
                        foreach (TreeViewItem TItem in EditorsTreeView.Items)
                        {
                            Editor TheEditor = TItem.Tag as Editor;
                            if (TheEditor == EntryClass.EntryTypeMenu.LinkedEditor)
                            {
                                TItem.IsSelected = true;
                                EditorFileNameIDTextBox.Text = EntryClass.EntryTypeMenu.FirstNameID.ToString();
                                break;
                            }
                        }
                    }
                }
                

            }

        }

        //public event EventHandler RequestClose;
        //private void CancelEditorCreation(object sender, RoutedEventArgs e)
        //{
        //    RequestClose?.Invoke(this, EventArgs.Empty);
        //}

        public void SetupForDescription() 
        {
            DescriptionMode = true;

            TextLastLineNameLabel.Visibility = Visibility.Collapsed;
            TextLastLineTextBox.Visibility = Visibility.Collapsed;
            TextBoxLastLineHelpText.Visibility = Visibility.Collapsed;
            ButtonSetLastLine.Visibility = Visibility.Collapsed;

            LabelNameCount.Visibility = Visibility.Collapsed;
            FileNameCountTextBox.Visibility = Visibility.Collapsed;
            DataLinkNameCountHelpPanel.Visibility = Visibility.Collapsed;

            NameIDLabel.Visibility = Visibility.Collapsed;
            FileNameIDTextBox.Visibility = Visibility.Collapsed;

            ComboBoxListType.Items.RemoveAt(3); //Remove from Nothing
            ComboBoxListType.Items.RemoveAt(2); //Remove from Editor
            //NOTE: IF I EVER ALLOW EDITORS TO GET NAMES FROM ANOTHER EDITOR, ENTRY MENUS LINKING TO EDITORS WILL BREAK. 
            //I CAN'T THINK OF A REASON TO DO THIS THOUGH, SO I DON'T THINK IT WILL BE A PROBLEM. 
            //(IE, Editor A has a entry getting names from editor B, but editor B is getting names from C. Well, i guess as entrys load AFTER editor names, so maybe this won't be a problem after all.
            //Still, i should keep the posibility of this in mind if i ever do this.)

            var parentWindow = Window.GetWindow(this);

            if (parentWindow is Workshop workshopWindow)
            {
                Database = workshopWindow.MyDatabase;

            }

            if (Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList.Count != 0)
            {
                DescriptionTable TextTable = Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList[0];
                FileStartTextBox.Text = TextTable.Start.ToString();
                FileTextSizeTextBox.Text = TextTable.TextSize.ToString();
                FileFullRowSizeTextBox.Text = TextTable.RowSize.ToString();
                CharacterSetComboBox.Text = TextTable.CharacterSet;

                

                if (Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList[0].LinkType == DescriptionTable.LinkTypes.TextFile)
                {
                    ComboBoxListType.Text = "Link to Text File";
                    TabControlListType.SelectedIndex = 1;

                }

                DataFileManager.Loaded += new RoutedEventHandler(ShowDescriptionFileInUse);

            }
        }
        public void ShowDescriptionFileInUse(object sender, RoutedEventArgs e)
        {
            foreach (TreeViewItem Item3 in DataFileManager.TreeGameFiles.Items) //FileTreeExtraTable.Items
            {
                if (Item3.Tag == Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList[0].FileTextTable)
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
            foreach (TreeViewItem Item3 in FileManagerForTextFiles.TreeGameFiles.Items) //FileTreeExtraTable.Items
            {
                if (Item3.Tag == Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList[0].FileTextTable)
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
        }

        public void SetupForMenu(Entry EntryClass2, ListBox entryListBox, WorkshopData database, Workshop TheWorkshop)
        {
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

            TextFileNameIDLabel.Visibility = Visibility.Visible;
            TextFileFileNameIDTextBox.Visibility = Visibility.Visible;
            TextLinkNameIDHelpText.Visibility = Visibility.Visible;



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
            //DataFileManager.Loaded += new RoutedEventHandler(ShowMenuDataFileInUse);
            DataFileManager.SelectItem(EntryClass);

            FileStartTextBox.Text = EntryClass.EntryTypeMenu.Start.ToString();
            FileFullRowSizeTextBox.Text = EntryClass.EntryTypeMenu.RowSize.ToString();
            FileTextSizeTextBox.Text = EntryClass.EntryTypeMenu.CharCount.ToString();
            FileNameIDTextBox.Text = EntryClass.EntryTypeMenu.FirstNameID.ToString();
            FileNameCountTextBox.Text = EntryClass.EntryTypeMenu.NameCount.ToString();

            //Text File Setup
            //FileManagerForTextFiles.Loaded += new RoutedEventHandler(ShowMenuDataFileInUse);
            FileManagerForTextFiles.SelectItem(EntryClass);
            TextFirstLineTextBox.Text = EntryClass.EntryTypeMenu.Start.ToString();
            TextLastLineTextBox.Text = (EntryClass.EntryTypeMenu.NameCount + EntryClass.EntryTypeMenu.Start - 1).ToString();
            TextFileFileNameIDTextBox.Text = EntryClass.EntryTypeMenu.FirstNameID.ToString();

            //Editor Setuo
            if (EntryClass.EntryTypeMenu.LinkedEditor != null) 
            {
                foreach (TreeViewItem TItem in EditorsTreeView.Items) 
                {
                    Editor TheEditor = TItem.Tag as Editor;
                    if (TheEditor == EntryClass.EntryTypeMenu.LinkedEditor) 
                    {
                        TItem.IsSelected = true;
                        EditorFileNameIDTextBox.Text = EntryClass.EntryTypeMenu.FirstNameID.ToString();
                        break;
                    }
                }
            }
            

            //Nothing List Setup
            ItemsNumBox.Clear();
            ItemsEditBox.Clear();
            //ItemsNumBox.Text = "0";
            StringBuilder NumsText = new StringBuilder("0");
            StringBuilder itemsText = new StringBuilder(EntryClass.EntryTypeMenu.NothingNameList[0]);
            for (int i = 1; i < EntryClass.EntryTypeMenu.NothingNameList.Length; i++)
            {
                NumsText.Append("\r");
                NumsText.Append(i);
                itemsText.Append("\r");
                itemsText.Append(EntryClass.EntryTypeMenu.NothingNameList[i]);
            }
            ItemsNumBox.Text = NumsText.ToString();
            ItemsEditBox.Text = itemsText.ToString();



        }

        private void FileLinkDebug(object sender, RoutedEventArgs e)
        {
            FileStartTextBox.Text = "9";
            FileFullRowSizeTextBox.Text = "176";
            FileTextSizeTextBox.Text = "31";
            FileNameIDTextBox.Text = "0";
            FileNameCountTextBox.Text = "37";

            foreach (TreeViewItem Item3 in DataFileManager.TreeGameFiles.Items) //FileTreeExtraTable.Items
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
            foreach (TreeViewItem Item3 in DataFileManager.TreeGameFiles.Items) //FileTreeExtraTable.Items
            {
                if (Item3.Tag == Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList[0].FileTextTable)
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
        }

        private void ExitWithoutSaving(object sender, RoutedEventArgs e)
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }
        }

        private void SaveAndExit(object sender, RoutedEventArgs e)
        {
            if (DescriptionMode == false && MenuMode == false) { return; }

            if (Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList.Count == 0)
            {
                DescriptionTable NEW_DescriptionTable = new();
                Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList.Add(NEW_DescriptionTable);

            }

            if (DescriptionMode == true) 
            {
                SaveDescriptions(); 
            }
            if (MenuMode == true)
            {
                SaveMenu();
            }

            //This part updates the current description, and i made select retrigger decoding and moved description textbox creation to decoding, so it also creates the textbox instantly if the user just added a new description.
            Database.Workshop.TreeViewSelectionEnabled = false;
            TreeViewItem TheItem = Database.Workshop.EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.SelectedItem as TreeViewItem;
            TheItem.IsSelected = false;
            Database.Workshop.TreeViewSelectionEnabled = true;
            TheItem.IsSelected = true;

            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }


        }

        private void SaveDescriptions()
        {
            
            DescriptionTable DescriptionTable = Database.Workshop.EditorClass.StandardEditorData.DescriptionTableList[0];


            if (ComboBoxListType.Text == "Link to Data File") 
            {
                DescriptionTable.LinkType = DescriptionTable.LinkTypes.DataFile;

                TreeViewItem FileItem = DataFileManager.TreeGameFiles.SelectedItem as TreeViewItem; //The file the user is pulling item descriptions from.


                if (FileItem.Tag as GameFile != null)
                {
                    DescriptionTable.FileTextTable = FileItem.Tag as GameFile;
                }
                DescriptionTable.CharacterSet = CharacterSetComboBox.Text;
                DescriptionTable.Start = int.Parse(FileStartTextBox.Text);
                DescriptionTable.TextSize = int.Parse(FileTextSizeTextBox.Text);
                DescriptionTable.RowSize = int.Parse(FileFullRowSizeTextBox.Text);

                
                
            }
            if (ComboBoxListType.Text == "Link to Text File") 
            {

                DescriptionTable.LinkType = DescriptionTable.LinkTypes.TextFile;

                TreeViewItem FileItem = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem; //The file the user is pulling item descriptions from.

                if (FileItem.Tag as GameFile != null)
                {
                    DescriptionTable.FileTextTable = FileItem.Tag as GameFile;
                }


            }
            //Link to Editor is intentionally disabled and cannot be used for descriptions. (I can't think of any reason to allow it)
            //Link to Nothing is also bocked. 



        }

        private void SaveMenu() 
        {
            

            if (ComboBoxListType.Text == "Link to Data File")
            {                
                TreeViewItem Item = DataFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
                if (Item == null) { return; }
                GameFile TheFile = Item.Tag as GameFile;
                if (TheFile == null) { return; }
                EntryClass.EntryTypeMenu.GameFile = TheFile;

                EntryClass.EntryTypeMenu.Start = Int32.Parse(FileStartTextBox.Text);
                EntryClass.EntryTypeMenu.RowSize = Int32.Parse(FileFullRowSizeTextBox.Text);
                EntryClass.EntryTypeMenu.CharCount = Int32.Parse(FileTextSizeTextBox.Text);
                EntryClass.EntryTypeMenu.FirstNameID = Int32.Parse(FileNameIDTextBox.Text); 
                EntryClass.EntryTypeMenu.NameCount = Int32.Parse(FileNameCountTextBox.Text);
                if (AsciiItem.IsSelected == true) { EntryClass.EntryTypeMenu.CharacterSet = "ASCII+ANSI"; }
                if (ShiftJISItem.IsSelected == true) { EntryClass.EntryTypeMenu.CharacterSet = "Shift-JIS"; }

                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.DataFile;

            }
            if (ComboBoxListType.Text == "Link to Text File") 
            {
                TreeViewItem Item = FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem;
                if (Item == null) { return; }
                GameFile TheFile = Item.Tag as GameFile;
                if (TheFile == null) { return; }
                EntryClass.EntryTypeMenu.GameFile = TheFile;
                EntryClass.EntryTypeMenu.Start = int.Parse(TextFirstLineTextBox.Text);
                EntryClass.EntryTypeMenu.NameCount = Int32.Parse(TextLastLineTextBox.Text) - Int32.Parse(TextFirstLineTextBox.Text) + 1; //+1 to account for line zero
                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.TextFile;
                EntryClass.EntryTypeMenu.FirstNameID = Int32.Parse(TextFileFileNameIDTextBox.Text);

                if (EntryClass.Bytes == 1)
                {
                    if (EntryClass.EntryTypeMenu.NameCount > 256) { EntryClass.EntryTypeMenu.NameCount = 256; }
                }
                if (EntryClass.Bytes == 2)
                {
                    if (EntryClass.EntryTypeMenu.NameCount > 65536) { EntryClass.EntryTypeMenu.NameCount = 65536; }
                }
            }

            if (ComboBoxListType.Text == "Link to Editor")
            {
                //return;
                

                TreeViewItem Item = EditorsTreeView.SelectedItem as TreeViewItem;
                if (Item == null) { return; }
                Editor TheEditor = Item.Tag as Editor;
                if (TheEditor == null) { return; }

                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.Editor;
                EntryClass.EntryTypeMenu.LinkedEditor = TheEditor;
                EntryClass.EntryTypeMenu.FirstNameID = Int32.Parse(EditorFileNameIDTextBox.Text);
                EntryClass.EntryTypeMenu.NameCount = 0;

                foreach (ItemInfo TheItem in TheEditor.StandardEditorData.EditorLeftDockPanel.ItemList)
                {
                    if (TheItem.IsFolder == false)
                    {
                        EntryClass.EntryTypeMenu.NameCount++;
                    }
                }

            }

            if (ComboBoxListType.Text == "Link to Nothing")
            {
                EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.Nothing;

                string[] lines = ItemsEditBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i < EntryClass.EntryTypeMenu.NothingNameList.Length)
                    {
                        EntryClass.EntryTypeMenu.NothingNameList[i] = lines[i].Trim();
                    }
                    else
                    {
                        // Handle case where there are more lines than array elements
                        break;
                    }
                }
                //ButtonListEditSave.Content = "Save";


                //EntryListBox.SelectionChanged -= Workshop.EntryListBox_SelectionChanged; // Remove event handler   
                //EntryListBox.Items.Clear();
                //for (int i = 0; i < EntryClass.EntryTypeMenu.NothingNameList.Length; i++)
                //{
                //    if (!string.IsNullOrEmpty(EntryClass.EntryTypeMenu.NothingNameList[i]))
                //    {
                //        string itemText = i + ": " + EntryClass.EntryTypeMenu.NothingNameList[i];
                //        EntryListBox.Items.Add(itemText);
                //    }
                //}
                //EntryListBox.SelectionChanged += Workshop.EntryListBox_SelectionChanged; // Re-attach event handler  


                //Database.EntryManager.LoadMenu(EntryClass);

                //ButtonListEditSave.Content = "Edit";
                MenuPanelEdit.Visibility = Visibility.Collapsed;
            }





            if (EntryClass.EntryTypeMenu.NameCount > 255 && EntryClass.Endianness == "1")
            {
                EntryClass.EntryTypeMenu.NameCount = 255;
            }
            if (EntryClass.EntryTypeMenu.NameCount > 65535 && (EntryClass.Endianness == "2L" || EntryClass.Endianness == "2B"))
            {
                EntryClass.EntryTypeMenu.NameCount = 65535;
            }

            MenuPanelEdit.Visibility = Visibility.Collapsed;

            Database.EntryManager.EntryChange(Database, EntrySubTypes.Menu, Workshop, EntryClass);
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

            int LineCount = lines.Length;

            //int LineCount = Math.Min(lines.Length, 255);

            TextLastLineTextBox.Text = LineCount.ToString();
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
            if (DataFileNamesPreviewTextbox == null || DataFileManager.TreeGameFiles.SelectedItem == null) 
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
                ANameCount = Int32.Parse(FileNameCountTextBox.Text); // - 1
                AStartByte = Int32.Parse(FileStartTextBox.Text);
                ARowSize = Int32.Parse(FileFullRowSizeTextBox.Text);
                ATextSize = Int32.Parse(FileTextSizeTextBox.Text);
                AEncoding = CharacterSetComboBox.Text;

                if (AEncoding == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (AEncoding == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { throw new InvalidOperationException("Unsupported character set"); }
            }
            catch
            {
                DataFileNamesPreviewTextbox.Text = "ERROR";
                return;
            }

            DataFileNamesPreviewTextbox.Text = "";

            try
            {
                for (int i = 0; i < (ANameCount + 1); i++)
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

            foreach (ItemInfo TheItem in TheEditor.StandardEditorData.EditorLeftDockPanel.ItemList)
            {
                if (TheItem.IsFolder == false)
                {
                    ANameCount++;
                }
            }


            for (int i = 0; i < ANameCount; i++)
            {

                string TheItemName = "ERROR";

                foreach (ItemInfo itemInfo in TheEditor.StandardEditorData.EditorLeftDockPanel.ItemList)
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
            return;

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

    }
}
