using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Windows.Graphics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace GameEditorStudio
{
    class LoadStandardEditor
    {

        public void NewSWEditorIntoDatabase(Workshop TheWorkshop, UserControlEditorCreator Maker) //This triggers when the user creates a new editor.
        {
            WorkshopData Database = TheWorkshop.MyDatabase;

            TheWorkshop.EditorName = Maker.TextboxEditorName.Text;

            

            Editor EditorClass = new(); //Creates the base class of the editor. Everything else becomes a child of this class, including other classes.
            EditorClass.EditorName = Maker.TextboxEditorName.Text;
            EditorClass.EditorKey = LibraryMan.GenerateKey();
            EditorClass.EditorType = "DataTable";
            //EditorClass.EditorIcon = Maker.DemoEditorImage.Tag as string; //For old editor icon system. 


            EditorClass.StandardEditorData.DataTableStart = int.Parse(Maker.TextBoxDataTableBaseAddress.Text);
            EditorClass.StandardEditorData.DataTableRowSize = int.Parse(Maker.TextBoxDataTableRowSize.Text);
            EditorClass.StandardEditorData.TableKey = LibraryMan.GenerateKey();
            
            var selectedItem = Maker.FileManager.TreeGameFiles.SelectedItem as TreeViewItem; //The file the user wants to make an editor for.
            GameFile HexFile = selectedItem.Tag as GameFile;
            string Key = HexFile.FileLocation;


            //EditorClass.SWData.EditorLocation = Database.GameFiles[Key].FileLocation;
            EditorClass.StandardEditorData.FileDataTable = HexFile;
            int itemIndex = 0;

            //This part determines how the list of item names is gotten.
            //Type 1: Use user inputs names to a textbox and it uses those..
            //Type 2: The user points to a file to get them directly. It users more user info + needs to convert the bytes via character encoding.
            if (Maker.Names == "DataFile")
            {
                EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.DataFile;

                TreeViewItem TreeItem = Maker.TextSourceManager.DataFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
                GameFile NameFile = TreeItem.Tag as GameFile;

                EditorClass.StandardEditorData.FileNameTable = NameFile;
                EditorClass.StandardEditorData.NameTableCharacterSet = Maker.TextSourceManager.CharacterSetComboBox.Text;
                EditorClass.StandardEditorData.NameTableStart = int.Parse(Maker.TextSourceManager.FileStartTextBox.Text);
                EditorClass.StandardEditorData.NameTableTextSize = int.Parse(Maker.TextSourceManager.FileTextSizeTextBox.Text);
                EditorClass.StandardEditorData.NameTableRowSize = int.Parse(Maker.TextSourceManager.FileFullRowSizeTextBox.Text);
                EditorClass.StandardEditorData.NameTableItemCount = int.Parse(Maker.TextSourceManager.FileNameCountTextBox.Text);


                for (int i = 0; i < EditorClass.StandardEditorData.NameTableItemCount; i++)
                {
                    ItemInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList.Add(ItemInfo);
                }

                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(TheWorkshop, EditorClass, "Items");

            }
            if (Maker.Names == "TextFile")
            {
                EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.TextFile;

                TreeViewItem TreeItem = Maker.TextSourceManager.FileManagerForTextFiles.TreeGameFiles.SelectedItem as TreeViewItem;
                GameFile NameFile = TreeItem.Tag as GameFile;

                EditorClass.StandardEditorData.FileNameTable = NameFile;
                EditorClass.StandardEditorData.NameTableStart = int.Parse(Maker.TextSourceManager.TextFirstLineTextBox.Text);
                EditorClass.StandardEditorData.NameTableItemCount = int.Parse(Maker.TextSourceManager.TextLastLineTextBox.Text) - int.Parse(Maker.TextSourceManager.TextFirstLineTextBox.Text) +1;
                

                for (int i = 0; i < EditorClass.StandardEditorData.NameTableItemCount; i++)
                {
                    ItemInfo ItemInfo = new();
                    ItemInfo.ItemIndex = i;
                    EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList.Add(ItemInfo);
                }
                
                

                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(TheWorkshop, EditorClass, "Items");
            }
            if (Maker.Names == "Editor")
            {
                EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.Editor;

                Notification Notification = new("I didn't actually make any code for Editor text from editor." +
                    "\n Make some in LoadSWEditor.cs" +
                    "\nNow we crash :>");
                Environment.FailFast(null); //Kills program instantly. 

            }
            if (Maker.Names == "Nothing")
            {
                EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.Nothing;

                foreach (string line in Maker.TextSourceManager.ItemsEditBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    ItemInfo IInfo = new();
                    IInfo.ItemName = line;
                    IInfo.ItemIndex = itemIndex;
                    itemIndex++;
                    EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList.Add(IInfo);
                }
                EditorClass.StandardEditorData.NameTableItemCount = itemIndex; //this might fix symbology not working for custom name lists. Revisit if its still broken.
                
            }
            
          

            Category RowClass = new();
            EditorClass.StandardEditorData.CategoryList.Add(RowClass);
            RowClass.SWData = EditorClass.StandardEditorData;

            Column ColumnClass = new();
            RowClass.ColumnList.Add(ColumnClass);



            //ColumnClass.EntryList = new List<Entry>();
            for (int i = 0; i <= Int32.Parse(Maker.TextBoxDataTableRowSize.Text) - 1; i++)
            {
                //This is the default settings of every entry when a new editor is created.
                //All of these can be changed by the user, and need to be saved to XML, and loaded back from XML.
                //There exist some more as well, but those aren't strictly necessary to a new entry.

                Entry EntryClass = new();
                ColumnClass.EntryList.Add(EntryClass);
                
                EntryClass.DataTableRowSize = Int32.Parse(Maker.TextBoxDataTableRowSize.Text);
                EntryClass.RowOffset = i;
                EntryClass.TableKey = EditorClass.StandardEditorData.TableKey;

                //Reminder this method is for creating a NEW editor, not loading one from a file. 
            }



            Database.GameEditors.Add(TheWorkshop.EditorName, EditorClass);

            CreateStandardEditor EditorMaker = new CreateStandardEditor();
            EditorMaker.CreateNormalEditor(TheWorkshop, Database, EditorClass); //Create a editor with this information.
            //This is not inside any loop, so it really just makes an editor.

            


        }


        

        public void LoadDataTableFromFile(Workshop TheWorkshop, WorkshopData Database, string TargetXML)
        {


            XElement xml = XElement.Load(TargetXML);

            TheWorkshop.EditorName = xml.Element("Name")?.Value; //Sets the name of the editor were working with from the name stored in XML.


            Editor EditorClass = new();          //Creates a EditorClass
            EditorClass.Workshop = TheWorkshop;
            EditorClass.EditorName = xml.Element("Name")?.Value;
            EditorClass.EditorType = xml.Element("Type")?.Value;
            EditorClass.EditorIcon = xml.Element("Icon")?.Value;
            EditorClass.EditorKey = xml.Element("Key")?.Value;

            foreach (XElement item in xml.Descendants("NameTable"))
            {
                if (item.Element("LinkType")?.Value == "DataFile") { EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.DataFile; }
                if (item.Element("LinkType")?.Value == "TextFile") { EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.TextFile; }
                if (item.Element("LinkType")?.Value == "Editor")   { EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.Editor; }
                if (item.Element("LinkType")?.Value == "Nothing")  { EditorClass.StandardEditorData.NameTableLinkType = StandardEditorData.NameTableLinkTypes.Nothing; }
                EditorClass.StandardEditorData.FileNameTable = LibraryMan.GetGameFileUsingLocation(Database, item.Element("Location")?.Value);
                EditorClass.StandardEditorData.NameTableCharacterSet = item.Element("CharacterSet")?.Value;
                EditorClass.StandardEditorData.NameTableStart = Int32.Parse(item.Element("Start")?.Value);
                EditorClass.StandardEditorData.NameTableTextSize = Int32.Parse(item.Element("TextSize")?.Value);
                EditorClass.StandardEditorData.NameTableRowSize = Int32.Parse(item.Element("RowSize")?.Value);
                EditorClass.StandardEditorData.NameTableItemCount = Int32.Parse(item.Element("ItemCount")?.Value);
                EditorClass.StandardEditorData.NameTableKey = item.Element("Key")?.Value;
            }

            foreach (XElement item in xml.Descendants("DataTable"))
            {
                EditorClass.StandardEditorData.FileDataTable = LibraryMan.GetGameFileUsingLocation(Database, item.Element("Location")?.Value);
                EditorClass.StandardEditorData.DataTableStart = Int32.Parse(item.Element("Start")?.Value);
                EditorClass.StandardEditorData.DataTableRowSize = Int32.Parse(item.Element("RowSize")?.Value);
                EditorClass.StandardEditorData.TableKey = item.Element("TableKey")?.Value;
            }

            foreach (XElement item in xml.Descendants("DataTableList")) 
            {
                
            }
                        
            foreach (XElement item in xml.Descendants("DescriptionTable"))
            {
                DescriptionTable ExtraTable = new();
                if (item.Element("LinkType")?.Value == "DataFile") { ExtraTable.LinkType = DescriptionTable.LinkTypes.DataFile; }
                if (item.Element("LinkType")?.Value == "TextFile") { ExtraTable.LinkType = DescriptionTable.LinkTypes.TextFile; }
                if (item.Element("LinkType")?.Value == "Editor") { ExtraTable.LinkType = DescriptionTable.LinkTypes.Editor; }
                if (item.Element("LinkType")?.Value == "Nothing") { ExtraTable.LinkType = DescriptionTable.LinkTypes.Nothing; }
                ExtraTable.FileTextTable = LibraryMan.GetGameFileUsingLocation(Database, item.Element("Location")?.Value);   
                ExtraTable.Start = Int32.Parse(item.Element("Start")?.Value);
                ExtraTable.RowSize = Int32.Parse(item.Element("RowSize")?.Value);
                ExtraTable.CharacterSet = item.Element("CharacterSet")?.Value;
                ExtraTable.TextSize = Int32.Parse(item.Element("TextSize")?.Value);
                ExtraTable.Key = item.Element("Key")?.Value;
                EditorClass.StandardEditorData.DescriptionTableList.Add(ExtraTable);
            }


            List<ItemInfo> itemList = new List<ItemInfo>();
            XElement itemListElement = xml.Descendants("ItemList").FirstOrDefault();
            bool SetChild = false;
            LoadItems(itemListElement, itemList); // Local function for recursive loading
            void LoadItems(XElement parentElement, List<ItemInfo> itemList)
            {
                foreach (XElement element in parentElement.Elements())
                {
                    if (element.Name == "Item" || element.Name == "Folder")
                    {
                        ItemInfo itemInfo = new ItemInfo
                        {
                            ItemName = element.Element("Name")?.Value ?? "Unnamed",
                            ItemIndex = int.Parse(element.Element("Index")?.Value ?? "0"),
                            ItemNote = element.Element("Note")?.Value,
                            ItemNotepad = element.Element("Notepad")?.Value,
                            ItemKey = element.Element("Key")?.Value,

                        };
                        if (SetChild == true) { itemInfo.IsChild = true; }
                        itemList.Add(itemInfo);

                        // If it's a folder, it might contain more items inside
                        if (element.Name == "Folder")
                        {
                            itemInfo.IsFolder = true;
                            SetChild = true;
                            LoadItems(element, itemList); // Recursive call
                            SetChild = false;
                        }
                    }
                }
            }
            EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList = itemList;
            

            if (TheWorkshop.IsPreviewMode == false && EditorClass.StandardEditorData.NameTableLinkType != StandardEditorData.NameTableLinkTypes.Nothing)
            {
                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(TheWorkshop, EditorClass, "Items");
            }






            
            Database.GameEditors.Add(TheWorkshop.EditorName, EditorClass); //Adds a core (aka the value) with the Key (New editor name from textbox) to the database dictionary.





            //CreateSWEditorCode Maker = new CreateSWEditorCode();
            //Maker.CreateSWEditor(TheWorkshop, Database, EditorClass); //Create a editor with this information.


        }

        public void LoadDataTableFromFilePart2(Workshop TheWorkshop, WorkshopData Database, Editor EditorClass) 
        {
            string TargetXML = Path.Combine(LibraryMan.ApplicationLocation, "Workshops", TheWorkshop.WorkshopName, "Editors", EditorClass.EditorName, "Editor.xml");

            XElement xml = XElement.Load(TargetXML);

            foreach (XElement Xrow in xml.Descendants("Category"))
            {
                string rowName = Xrow.Element("Name")?.Value;
                string rowKey = Xrow.Element("Key")?.Value;
                //int rowOrder = int.Parse(row.Element("RowOrder")?.Value);
                Category CategoryClass = new Category { CategoryName = rowName, Key = rowKey }; //, RowOrder = rowOrder
                CategoryClass.SWData = EditorClass.StandardEditorData;

                EditorClass.StandardEditorData.CategoryList.Add(CategoryClass);

                foreach (XElement Xcolumn in Xrow.Descendants("Group"))
                {
                    string columnName = Xcolumn.Element("Name")?.Value;
                    string columnKey = Xcolumn.Element("Key")?.Value;
                    Column ColumnClass = new Column { ColumnName = columnName, Key = columnKey }; //, ColumnOrder = columnOrder


                    CategoryClass.ColumnList ??= new List<Column>(); // Add the  object to the List
                    CategoryClass.ColumnList.Add(ColumnClass);
                    ColumnClass.EntryList ??= new List<Entry>(); // Add the  object to the List
                    foreach (XElement Xentry in Xcolumn.Descendants("Entry"))
                    {

                        Entry EntryClass = new();

                        EntryClass.Name = Xentry.Element("Name")?.Value;
                        EntryClass.Notepad = Xentry.Element("Notepad")?.Value;
                        EntryClass.IsNameHidden = Convert.ToBoolean(Xentry.Element("IsNameHidden")?.Value);
                        EntryClass.IsEntryHidden = Convert.ToBoolean(Xentry.Element("IsEntryHidden")?.Value);
                        EntryClass.TableKey = Xentry.Element("TableKey")?.Value;
                        EntryClass.DataTableRowSize = Int32.Parse(Xentry.Element("RowSize")?.Value);
                        EntryClass.RowOffset = Int32.Parse(Xentry.Element("RowOffset")?.Value);
                        EntryClass.Bytes = Int32.Parse(Xentry.Element("Bytes")?.Value);
                        EntryClass.Key = Xentry.Element("EntryKey")?.Value;
                        //EntryClass.Endianness = Xentry.Element("Endianness")?.Value;

                        string teemp = Xentry.Element("Endianness")?.Value;
                        if (EntryClass.Bytes == 1)
                        {
                            EntryClass.Endianness = "1";
                        }
                        else if (EntryClass.Bytes == 2 && teemp == "Little")
                        {
                            EntryClass.Endianness = "2L";
                        }
                        else if (EntryClass.Bytes == 2 && teemp == "Big")
                        {
                            EntryClass.Endianness = "2B";
                        }
                        else if (EntryClass.Bytes == 4 && teemp == "Little")
                        {
                            EntryClass.Endianness = "4L";
                        }
                        else if (EntryClass.Bytes == 4 && teemp == "Big")
                        {
                            EntryClass.Endianness = "4B";
                        }

                        //EntryClass.SubType = Xentry.Element("TypeX")?.Value;

                        if (Xentry.Descendants("NumberBox").Any()) { EntryClass.NewSubType = Entry.EntrySubTypes.NumberBox; }
                        if (Xentry.Descendants("CheckBox").Any()) { EntryClass.NewSubType = Entry.EntrySubTypes.CheckBox; }
                        if (Xentry.Descendants("BitFlag").Any()) { EntryClass.NewSubType = Entry.EntrySubTypes.BitFlag; }
                        if (Xentry.Descendants("Menu").Any()) { EntryClass.NewSubType = Entry.EntrySubTypes.Menu; }

                        if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox)
                        {
                            EntryClass.EntryTypeNumberBox = new();
                            foreach (XElement XNumberBox in Xentry.Descendants("NumberBox"))
                            {
                                string SignValue = XNumberBox.Element("Sign")?.Value;
                                if (SignValue == "Signed") { EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Signed; }
                                else if (SignValue == "Unsigned") { EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Unsigned; }
                            }
                        }

                        if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox)
                        {
                            EntryClass.EntryTypeCheckBox = new();
                            foreach (XElement XCheckBox in Xentry.Descendants("CheckBox"))
                            {
                                //EntryClass.EntryTypeCheckBox.TrueText = XCheckBox.Element("TrueText")?.Value;
                                //EntryClass.EntryTypeCheckBox.FalseText = XCheckBox.Element("FalseText")?.Value;
                                EntryClass.EntryTypeCheckBox.TrueValue = Int32.Parse(XCheckBox.Element("TrueValue")?.Value);
                                EntryClass.EntryTypeCheckBox.FalseValue = Int32.Parse(XCheckBox.Element("FalseValue")?.Value);
                            }
                        }



                        if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag)
                        {
                            EntryClass.EntryTypeBitFlag = new();
                            foreach (XElement XBitFlag in Xentry.Descendants("BitFlag"))
                            {
                                EntryClass.EntryTypeBitFlag.BitFlag1Name = XBitFlag.Element("Flag1Name")?.Value;
                                EntryClass.EntryTypeBitFlag.BitFlag2Name = XBitFlag.Element("Flag2Name")?.Value;
                                EntryClass.EntryTypeBitFlag.BitFlag3Name = XBitFlag.Element("Flag3Name")?.Value;
                                EntryClass.EntryTypeBitFlag.BitFlag4Name = XBitFlag.Element("Flag4Name")?.Value;
                                EntryClass.EntryTypeBitFlag.BitFlag5Name = XBitFlag.Element("Flag5Name")?.Value;
                                EntryClass.EntryTypeBitFlag.BitFlag6Name = XBitFlag.Element("Flag6Name")?.Value;
                                EntryClass.EntryTypeBitFlag.BitFlag7Name = XBitFlag.Element("Flag7Name")?.Value;
                                EntryClass.EntryTypeBitFlag.BitFlag8Name = XBitFlag.Element("Flag8Name")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag1CheckText = XBitFlag.Element("Flag1CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag2CheckText = XBitFlag.Element("Flag2CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag3CheckText = XBitFlag.Element("Flag3CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag4CheckText = XBitFlag.Element("Flag4CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag5CheckText = XBitFlag.Element("Flag5CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag6CheckText = XBitFlag.Element("Flag6CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag7CheckText = XBitFlag.Element("Flag7CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag8CheckText = XBitFlag.Element("Flag8CheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag1UncheckText = XBitFlag.Element("Flag1UncheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag2UncheckText = XBitFlag.Element("Flag2UncheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag3UncheckText = XBitFlag.Element("Flag3UncheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag4UncheckText = XBitFlag.Element("Flag4UncheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag5UncheckText = XBitFlag.Element("Flag5UncheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag6UncheckText = XBitFlag.Element("Flag6UncheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag7UncheckText = XBitFlag.Element("Flag7UncheckText")?.Value;
                                //EntryClass.EntryTypeBitFlag.BitFlag8UncheckText = XBitFlag.Element("Flag8UncheckText")?.Value;

                            }
                        }



                        if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu)
                        {
                            EntryClass.EntryTypeMenu = new();
                            foreach (XElement XList in Xentry.Descendants("Menu"))
                            {
                                string menuTypeStr = XList.Element("MenuType")?.Value;
                                if (menuTypeStr == "Dropdown") { EntryClass.EntryTypeMenu.MenuType = EntryTypeMenu.MenuTypes.Dropdown; }
                                else if (menuTypeStr == "List") { EntryClass.EntryTypeMenu.MenuType = EntryTypeMenu.MenuTypes.List; }

                                string linkTypeStr = XList.Element("LinkType")?.Value;
                                if (linkTypeStr == "DataFile") { EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.DataFile; }
                                else if (linkTypeStr == "TextFile") { EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.TextFile; }
                                else if (linkTypeStr == "Editor") { EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.Editor; }
                                else if (linkTypeStr == "Nothing") { EntryClass.EntryTypeMenu.LinkType = EntryTypeMenu.LinkTypes.Nothing; }

                                if (EntryClass.Bytes == 1) { EntryClass.EntryTypeMenu.ListSize = 256; }    //yes, 256 not 255                                
                                else if (EntryClass.Bytes == 2) { EntryClass.EntryTypeMenu.ListSize = 65536; }  //yes, 65536 not 65535                                                               
                                string[] listItems = new string[EntryClass.EntryTypeMenu.ListSize];


                                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile)
                                {
                                    XElement XThing = XList.Element("FileData");
                                    EntryClass.EntryTypeMenu.GameFile = LibraryMan.GetGameFileUsingLocation(Database, XThing.Element("Location")?.Value);
                                    EntryClass.EntryTypeMenu.FirstNameID = Int32.Parse(XThing.Element("FirstNameID")?.Value);
                                    EntryClass.EntryTypeMenu.Start = Int32.Parse(XThing.Element("Start")?.Value);
                                    EntryClass.EntryTypeMenu.RowSize = Int32.Parse(XThing.Element("RowSize")?.Value);
                                    EntryClass.EntryTypeMenu.CharacterSet = XThing.Element("CharacterSet")?.Value;
                                    EntryClass.EntryTypeMenu.CharCount = Int32.Parse(XThing.Element("TextSize")?.Value);
                                    EntryClass.EntryTypeMenu.NameCount = Int32.Parse(XThing.Element("NameCount")?.Value);
                                }
                                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                                {
                                    XElement XThing = XList.Element("TextData");
                                    EntryClass.EntryTypeMenu.GameFile = LibraryMan.GetGameFileUsingLocation(Database, XThing.Element("Location")?.Value);
                                    EntryClass.EntryTypeMenu.NameCount = Int32.Parse(XThing.Element("NameCount")?.Value);
                                    EntryClass.EntryTypeMenu.Start = Int32.Parse(XThing.Element("Start")?.Value);
                                    EntryClass.EntryTypeMenu.FirstNameID = Int32.Parse(XThing.Element("FirstNameID")?.Value);
                                }
                                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
                                {
                                    XElement XThing = XList.Element("EditorData");
                                    EntryClass.EntryTypeMenu.NameCount = Int32.Parse(XThing.Element("NameCount")?.Value);
                                    //EntryClass.EntryTypeMenu.Start = Int32.Parse(XThing.Element("Start")?.Value);
                                    EntryClass.EntryTypeMenu.FirstNameID = Int32.Parse(XThing.Element("FirstNameID")?.Value);
                                    EntryClass.EntryTypeMenu.OldLinkedEditorName = XThing.Element("EditorName")?.Value;
                                    EntryClass.EntryTypeMenu.OldLinkedEditorKey = XThing.Element("EditorKey")?.Value;
                                    string TheEditorKey = XThing.Element("EditorKey")?.Value;

                                    foreach (Editor AnEditor in Database.GameEditors.Values)
                                    {
                                        if (AnEditor.EditorKey == TheEditorKey) { EntryClass.EntryTypeMenu.LinkedEditor = AnEditor; }
                                    }

                                    if (EntryClass.EntryTypeMenu.LinkedEditor == null)
                                    {
                                        //Notification Notification = new("The editor was not found." +
                                        //    "\n");

                                        string theEditorName = XThing.Element("EditorName")?.Value;
                                        string thisEditorName = EditorClass.EditorName;
                                        string thisEntryName = EntryClass.Name;
                                        string thisEntryID = EntryClass.RowOffset.ToString();

                                        Notification Notification = new("Editor \"" + theEditorName + "\" is missing, and editor \"" + thisEditorName + "\" uses it" +
                                            "\n" + thisEditorName + " will load anyway, but it's entry \"" + thisEntryName + "\" (Entry ID: " + thisEntryID + ") won't display text properly." +
                                            "\nInstead of completly not working, that entry (should) use red ERROR text in place of the text it's support to have." +
                                            "\nThe editor will otherwise function perfectly normally, and you can save data just fine. " +
                                            "\n" + 
                                            "\nYou can fix this IF you know what your doing by changing that entrys linked text data." +
                                            "\n(PS: It's probably a menu type entry)");
                                    }

                                    //if (EntryClass.EntryTypeMenu.LinkedEditor == null)
                                    //{
                                    //    throw new Exception("Editor not found in database.");
                                    //}


                                }
                                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
                                {


                                    XElement XItemList = XList.Element("NameList");
                                    foreach (XElement XItem in XItemList.Elements("Item"))
                                    {
                                        string listItemValue = XItem.Value;
                                        int colonIndex = listItemValue.IndexOf(':');
                                        if (colonIndex >= 0)
                                        {
                                            string indexString = listItemValue.Substring(0, colonIndex).Trim(); // Extract the index and text from the list item value
                                            string text = listItemValue.Substring(colonIndex + 1).Trim();

                                            if (int.TryParse(indexString, out int index)) // Try to parse the index as an integer
                                            {
                                                if (index >= 0 && index < listItems.Length) // Check if the index is within the range of the list items array
                                                {
                                                    listItems[index] = text; // Add the text to the list items array at the specified index
                                                }
                                            }
                                        }
                                    }

                                }
                                EntryClass.EntryTypeMenu.NothingNameList = listItems;

                            }
                        }



                        ColumnClass.EntryList.Add(EntryClass);
                    }
                }

            }


            
        }




    }
}
