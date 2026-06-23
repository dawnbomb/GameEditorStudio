using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using WpfHexEditor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace GameEditorStudio
{
    class LoadStandardEditor
    {
        public void NewDataTableEditor(Workshop TheWorkshop, UserControlEditorCreator Maker) 
        {
            WorkshopData database = TheWorkshop.WorkshopData;


            DataTableEditorData TheDataTableData = new();
            DataTableEditorData DataTableData = TheDataTableData; //To help reduce reference count...
            DataTableData.DataTableEditorData = DataTableData;
            DataTableData.WorkshopXaml = TheWorkshop;
            DataTableData.WorkshopData = database;
            DataTableData.EditorName = Maker.TextboxEditorName.Text;            

            DataTableData.CreatedDate = DateTime.Now.ToString("MMM dd yyyy");
            DataTableData.CreatedVersion = LibraryGES.VersionNumber;
            
            database.GameEditors.Add(DataTableData);

            DataTableEditor standardEditor = new(database, DataTableData);

            DataTableData.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        

        public void LoadDataTableXMLIntoDatabasePART1(Workshop TheWorkshop, WorkshopData Database, string TargetXML)
        {   
            XElement xml = XElement.Load(TargetXML);

            //TheWorkshop.EditorName = xml.Element("Name")?.Value; //Sets the name of the editor were working with from the name stored in XML.
            string ename = Path.GetFileName(Path.GetDirectoryName(TargetXML)); //Sets the name of the editor were working with from the name editor's folder name.
            

            DataTableEditorData TheDataTableData = new();          //Creates a EditorClass
            DataTableEditorData DTEData = TheDataTableData;
            DTEData.DataTableEditorData = DTEData;
            DTEData.WorkshopXaml = TheWorkshop;
            DTEData.WorkshopData = Database;
            DTEData.EditorName = Path.GetFileName(Path.GetDirectoryName(TargetXML));
            DTEData.EditorIcon = xml.Element("Icon")?.Value;
            DTEData.EditorKey = xml.Element("Key")?.Value;

            foreach (Editor someeditor in Database.GameEditors) 
            {
                if (someeditor.EditorKey == DTEData.EditorKey) 
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Editor Key Conflict!",
                    "Editor 1: " + ename +
                    "\nEditor 2: " + DTEData.EditorName +
                    "\n" +
                    "\nYou got this error because your trying to load a copy of an existing editor. Every editor has a unique key, and renaming a copy of an editor doesn't change that key. " +
                    "\n" +
                    "\nI'll add a way to duplicate an editor in the future, for now if you REALLY want both of them, just open the editor.xml of your copy and give it a random key yourself. " +
                    "\n" +
                    "\nPS: You CAN rename copys of workshops. " +
                    "\nPPS: The program will now crash :("
                    );

                    Process.GetCurrentProcess().Kill(); //Force crash.
                }
            }

            DTEData.CreatedDate = xml.Element("CreatedDate")?.Value ?? "";
            DTEData.CreatedVersion = Version.TryParse(xml.Element("CreatedVersion")?.Value, out var vx1) ? vx1 : new Version(0, 0);
            DTEData.SavedDate = xml.Element("SavedDate")?.Value ?? "";
            DTEData.SavedVersion = Version.TryParse(xml.Element("SavedVersion")?.Value, out var vx2) ? vx2 : new Version(0, 0);



            var nameTableElement = xml.Elements("NameTable").FirstOrDefault();
            if (nameTableElement != null)
            {
                DTEData.NameTable = new();
                foreach (XElement item in xml.Descendants("NameTable"))
                {
                    if (item.Element("LinkType")?.Value == "DataFile") { DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.DataFile; }
                    if (item.Element("LinkType")?.Value == "DataFileAdvanced") { DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.Advanced; }
                    if (item.Element("LinkType")?.Value == "TextFile") { DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.TextFile; }
                    if (item.Element("LinkType")?.Value == "Editor") { DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.Editor; }
                    if (item.Element("LinkType")?.Value == "Nothing") { DTEData.NameTable.TextTableLinkType = TextTable.TextTableLinkTypes.Nothing; }
                    DTEData.NameTable.TextTableFile = LibraryGES.GetGameFileUsingLocation(Database, item.Element("Location")?.Value);
                    DTEData.NameTable.GameFileLocation = item.Element("Location")?.Value;
                    DTEData.NameTable.TextTableCharacterSet = item.Element("CharacterSet")?.Value;
                    DTEData.NameTable.TextTableStart = Int32.Parse(item.Element("Start")?.Value ?? "0");
                    DTEData.NameTable.TextTableCharLimit = Int32.Parse(item.Element("CharacterLimit")?.Value ?? "0");
                    DTEData.NameTable.TextTableRowSize = Int32.Parse(item.Element("RowSize")?.Value ?? "0");
                    DTEData.NameTable.TextTableFirstNameID = Int32.Parse(item.Element("FirstNameID")?.Value);
                    DTEData.NameTable.TextTableItemCount = Int32.Parse(item.Element("ItemCount")?.Value);
                    DTEData.NameTable.TextTableKey = item.Element("Key")?.Value;
                }

                List<TextInfo> itemList = new List<TextInfo>();
                XElement itemListElement = xml.Descendants("ItemList").FirstOrDefault();
                bool SetChild = false;
                LoadItems(itemListElement, itemList); // Local function for recursive loading
                void LoadItems(XElement parentElement, List<TextInfo> itemList)
                {
                    foreach (XElement element in parentElement.Elements())
                    {
                        if (element.Name == "Item" || element.Name == "Folder")
                        {
                            TextInfo itemInfo = new TextInfo
                            {
                                ItemName = element.Element("Name")?.Value ?? "Not Loaded",
                                ItemIndex = int.Parse(element.Element("Index")?.Value ?? "0"),
                                ItemNote = element.Element("Note")?.Value ?? "",
                                ItemWorkshopTooltip = element.Element("Tooltip")?.Value ?? "",
                                ItemKey = element.Element("Key")?.Value,
                                RowStart = int.Parse(element.Element("RowStart")?.Value ?? "-1"),
                                RowEnd = int.Parse(element.Element("RowEnd")?.Value ?? "-1"),

                            };
                            //itemInfo.RowStart = DTEData.NameTable.TextTableStart + (itemInfo.ItemIndex * DTEData.NameTable.TextTableRowSize);
                            //itemInfo.RowEnd = DTEData.NameTable.TextTableStart + ((itemInfo.ItemIndex + 1) * DTEData.NameTable.TextTableRowSize) - 1;
                            //itemInfo.RowEnd = itemInfo.RowStart + DTEData.NameTable.TextTableTextSize - 1;

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
                DTEData.NameTable.ItemList = itemList;



                if (TheWorkshop.IsPreviewMode == false && DTEData.NameTable.TextTableLinkType != TextTable.TextTableLinkTypes.Nothing)
                {
                    CharacterSetManager CharacterManager = new();
                    CharacterManager.DecodeAllItemNames(DTEData);
                }

            }



            var dataTableElement = xml.Elements("DataTable").FirstOrDefault();
            if (dataTableElement != null) 
            {
                DTEData.DataTable = new();
                foreach (XElement item in xml.Descendants("DataTable"))
                {
                    DTEData.DataTable.FileDataTable = LibraryGES.GetGameFileUsingLocation(Database, item.Element("Location")?.Value);
                    DTEData.DataTable.GameFileLocation = item.Element("Location")?.Value;
                    DTEData.DataTable.DataTableStart = Int32.Parse(item.Element("Start")?.Value);
                    DTEData.DataTable.DataTableRowSize = Int32.Parse(item.Element("RowSize")?.Value);
                    DTEData.DataTable.DataTableKey = item.Element("TableKey")?.Value;
                }
            }



            var descriptionTableElement = xml.Elements("DescriptionTable").FirstOrDefault();
            if (descriptionTableElement != null)
            {
                foreach (XElement Ditem in xml.Descendants("DescriptionTable"))
                {
                    TextTable TheDescriptionTable = new();
                    if (Ditem.Element("LinkType")?.Value == "DataFile") { TheDescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.DataFile; }
                    if (Ditem.Element("LinkType")?.Value == "Advanced") { TheDescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.Advanced; }
                    if (Ditem.Element("LinkType")?.Value == "TextFile") { TheDescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.TextFile; }
                    if (Ditem.Element("LinkType")?.Value == "Editor") { TheDescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.Editor; }
                    if (Ditem.Element("LinkType")?.Value == "Nothing") { TheDescriptionTable.TextTableLinkType = TextTable.TextTableLinkTypes.Nothing; }
                    TheDescriptionTable.TextTableFile = LibraryGES.GetGameFileUsingLocation(Database, Ditem.Element("Location")?.Value);
                    TheDescriptionTable.GameFileLocation = Ditem.Element("Location")?.Value;
                    TheDescriptionTable.TextTableStart = Int32.Parse(Ditem.Element("Start")?.Value);
                    TheDescriptionTable.TextTableRowSize = Int32.Parse(Ditem.Element("RowSize")?.Value);
                    TheDescriptionTable.TextTableCharacterSet = Ditem.Element("CharacterSet")?.Value;
                    TheDescriptionTable.TextTableCharLimit = Int32.Parse(Ditem.Element("CharacterLimit")?.Value);
                    TheDescriptionTable.TextTableKey = Ditem.Element("Key")?.Value;
                    DTEData.DescriptionTableList.Add(TheDescriptionTable);

                    if (TheDescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced) 
                    {
                        TheDescriptionTable.TextTableItemCount = Int32.Parse(Ditem.Element("ItemCount")?.Value);

                        List<TextInfo> itemList = new List<TextInfo>();
                        XElement itemListElement = Ditem.Descendants("ItemList").FirstOrDefault();
                        bool SetChild = false;
                        LoadItems(itemListElement, itemList); // Local function for recursive loading
                        void LoadItems(XElement parentElement, List<TextInfo> itemList)
                        {
                            foreach (XElement element in parentElement.Elements())
                            {
                                if (element.Name == "Item" || element.Name == "Folder")
                                {
                                    TextInfo itemInfo = new TextInfo
                                    {
                                        ItemName = element.Element("Name")?.Value ?? "Unnamed",
                                        ItemIndex = int.Parse(element.Element("Index")?.Value ?? "0"),
                                        ItemNote = element.Element("Note")?.Value ?? "",
                                        ItemWorkshopTooltip = element.Element("Tooltip")?.Value ?? "",
                                        ItemKey = element.Element("Key")?.Value,
                                        RowStart = int.Parse(element.Element("RowStart")?.Value ?? "-1"),
                                        RowEnd = int.Parse(element.Element("RowEnd")?.Value ?? "-1"),

                                    };
                                    //itemInfo.RowStart = DTEData.NameTable.TextTableStart + (itemInfo.ItemIndex * DTEData.NameTable.TextTableRowSize);
                                    //itemInfo.RowEnd = DTEData.NameTable.TextTableStart + ((itemInfo.ItemIndex + 1) * DTEData.NameTable.TextTableRowSize) - 1;
                                    //itemInfo.RowEnd = itemInfo.RowStart + DTEData.NameTable.TextTableTextSize - 1;

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
                        TheDescriptionTable.ItemList = itemList;
                    }
                }
            }

            

            foreach (XElement item in xml.Descendants("DataTableList")) 
            {
                
            }




            try 
            {
                Database.GameEditors.Add(DTEData); 
            }
            catch 
            {
                PixelWPF.LibraryPixel.NotificationNegative("Error: Failed editor add code 173",
                    ""
                    );

                Process.GetCurrentProcess().Kill(); //Force crash.
            }


        }

        public void LoadDataTableXMLIntoDatabasePART2(Workshop TheWorkshop, WorkshopData Database, Editor EditorClass) 
        {
            string TargetXML = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", TheWorkshop.WorkshopData.WorkshopName, "Editors", EditorClass.EditorName, "Editor.xml");

            XElement xml = XElement.Load(TargetXML);

            
            

            if (EditorClass.DataTableEditorData.DataTable != null) 
            {
                int numba = xml.Descendants("MergedEntryList").Count();

                //Note: doing .Elements prevents a foreach from running even if there is 0 descendants (yes really, thats required).
                //IE: i didn't do this for the other loading parts so they may eventually bug / cause crashes.
                //But i'm to lazy to adjust them for now.
                foreach (XElement Mentry in xml.Descendants("MergedEntryList").Elements("Entry"))
                {
                    LoadEntry(Mentry, null, null);
                }

                foreach (XElement Xrow in xml.Descendants("Category")) //.Descendants gets all levels of children, not just immediate. 
                {

                    Category CategoryClass = new();
                    CategoryClass.CategoryName = Xrow.Element("Name")?.Value;
                    CategoryClass.Key = Xrow.Element("Key")?.Value;
                    CategoryClass.Tooltip = Xrow.Element("Tooltip")?.Value;
                    CategoryClass.DTEData = EditorClass.DataTableEditorData;

                    EditorClass.DataTableEditorData.CategoryList.Add(CategoryClass);



                    foreach (XElement Xcolumn in Xrow.Elements("Column")) //.Elements is only immediate children.
                    {

                        foreach (XElement XCitem in Xcolumn.Elements())
                        {
                            if (XCitem.Name == "Group")
                            {
                                Group GroupClass = new();
                                GroupClass.Column = Int32.Parse(XCitem.Element("Column")?.Value);
                                GroupClass.ColumnSpan = Int32.Parse(XCitem.Element("ColumnSpan")?.Value);
                                //GroupClass.Row = CategoryClass.GridItems.Count();
                                GroupClass.Row = Int32.Parse(XCitem.Element("Row")?.Value);
                                GroupClass.RowSpan = Int32.Parse(XCitem.Element("RowSpan")?.Value);
                                CategoryClass.GridItems.Add(GroupClass);

                                GroupClass.ParentCategory = CategoryClass;
                                GroupClass.ParentGrid = CategoryClass.ItemGrid;
                                GroupClass.ParentGridItems = CategoryClass.GridItems;
                                GroupClass.ParentEditor = EditorClass;
                                GroupClass.ParentGroup = null;
                                GroupClass.GroupName = XCitem.Element("Name")?.Value;
                                GroupClass.Key = XCitem.Element("Key")?.Value;
                                GroupClass.GroupTooltip = XCitem.Element("Tooltip")?.Value;

                                foreach (XElement Xentry in XCitem.Elements("Entry"))
                                {
                                    LoadEntry(Xentry, CategoryClass, GroupClass);
                                }

                                foreach (XElement Gcolumn in XCitem.Elements("Column"))
                                {
                                    foreach (XElement Xentry in Gcolumn.Elements("Entry"))
                                    {
                                        LoadEntry(Xentry, CategoryClass, GroupClass);
                                    }
                                }


                            }
                            else if (XCitem.Name == "Entry")
                            {
                                LoadEntry(XCitem, CategoryClass, null); // Pass the XElement if needed
                            }
                        } //End of Citem loop





                    } //End of column loop



                } //End of row loop
            }
            
            
            

            

            void LoadEntry(XElement Xentry, Category MyCategory, Group MyGroup)
            {
                Entry EntryClass = new();
                EditorClass.DataTableEditorData.MasterEntryList.Add(EntryClass);

                EntryClass.ParentEditor = EditorClass;

                EntryClass.Name = Xentry.Element("Name")?.Value;

                EntryClass.IsEntryHidden = Convert.ToBoolean(Xentry.Element("IsEntryHidden")?.Value);
                EntryClass.IsMerged = Convert.ToBoolean(Xentry.Element("IsMerged")?.Value);
                if (EntryClass.IsEntryHidden == true)
                {
                    EntryClass.ParentEditor.DataTableEditorData.HiddenEntryList.Add(EntryClass);
                }
                if (EntryClass.IsMerged == true)
                {
                    EntryClass.ParentEditor.DataTableEditorData.MergedEntryList.Add(EntryClass);
                }
                else if (EntryClass.IsMerged == false)
                {
                    EntryClass.ParentCategory = MyCategory;
                    EntryClass.Column = Int32.Parse(Xentry.Element("Column")?.Value);
                    EntryClass.Row = Int32.Parse(Xentry.Element("Row")?.Value);

                    if (MyGroup != null) //If entry is in a group.
                    {
                        EntryClass.ParentGroup = MyGroup;
                        EntryClass.ParentGrid = MyGroup.ItemGrid;
                        EntryClass.ParentGridItems = MyGroup.GridItems;
                        EntryClass.ParentGroup.GridItems.Add(EntryClass);
                    }
                    else if (MyGroup == null) //If entry is not in a group.
                    {
                        EntryClass.ParentGroup = null;
                        EntryClass.ParentGrid = MyCategory.ItemGrid;
                        EntryClass.ParentGridItems = MyCategory.GridItems;
                        MyCategory.GridItems.Add(EntryClass);
                    }
                }




                EntryClass.ColumnSpan = Int32.Parse(Xentry.Element("ColumnSpan")?.Value);
                EntryClass.RowSpan = Int32.Parse(Xentry.Element("RowSpan")?.Value);

                
                EntryClass.WorkshopTooltip = Xentry.Element("Tooltip")?.Value;
                EntryClass.IsNameHidden = Convert.ToBoolean(Xentry.Element("IsNameHidden")?.Value);
                EntryClass.DataTableKey = Xentry.Element("TableKey")?.Value;
                EntryClass.DataTableRowSize = Int32.Parse(Xentry.Element("RowSize")?.Value);
                EntryClass.RowOffset = Int32.Parse(Xentry.Element("RowOffset")?.Value);
                EntryClass.Bytes = Int32.Parse(Xentry.Element("Bytes")?.Value);
                EntryClass.Key = Xentry.Element("EntryKey")?.Value;

                if (EntryClass.Bytes == 0) //correcting old XMLs where entrys were 0 bytes instead of IsMerged.
                {
                    EntryClass.Endianness = "1";
                    EntryClass.Bytes = 1;
                }

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
                        EntryClass.EntryTypeNumberBox.Suffix = XNumberBox.Element("Suffix")?.Value;
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
                                                

                        
                        if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile) 
                        {
                            XElement MenuTable = XList.Element("MenuTable");
                            if (MenuTable == null) 
                            {
                                continue;
                            }

                            TextTable textTable = new();
                            EntryClass.EntryTypeMenu.TextTableDataFile = textTable;
                            textTable.TextTableLinkType = TextTable.TextTableLinkTypes.DataFile;

                            textTable.TextTableFile = LibraryGES.GetGameFileUsingLocation(Database, MenuTable.Element("Location")?.Value);
                            textTable.GameFileLocation = MenuTable.Element("Location")?.Value;
                            textTable.TextTableCharacterSet = MenuTable.Element("CharacterSet")?.Value;                            
                            textTable.TextTableStart = Int32.Parse(MenuTable.Element("Start")?.Value);
                            textTable.TextTableRowSize = Int32.Parse(MenuTable.Element("RowSize")?.Value);                            
                            textTable.TextTableCharLimit = Int32.Parse(MenuTable.Element("CharacterLimit")?.Value);
                            textTable.TextTableFirstNameID = Int32.Parse(MenuTable.Element("FirstNameID")?.Value);
                            textTable.TextTableItemCount = Int32.Parse(MenuTable.Element("ItemCount")?.Value);
                            textTable.TextTableKey = MenuTable.Element("Key")?.Value;


                            List<TextInfo> itemList = new List<TextInfo>();
                            XElement itemListElement = MenuTable.Descendants("ItemList").FirstOrDefault();
                            bool SetChild = false;
                            LoadItems(itemListElement, itemList); // Local function for recursive loading
                            void LoadItems(XElement parentElement, List<TextInfo> itemList)
                            {
                                foreach (XElement element in parentElement.Elements())
                                {
                                    if (element.Name == "Item" || element.Name == "Folder")
                                    {
                                        TextInfo itemInfo = new TextInfo
                                        {
                                            ItemName = element.Element("Name")?.Value ?? "Unnamed",
                                            ItemIndex = int.Parse(element.Element("Index")?.Value ?? "0"),
                                            ItemNote = element.Element("Note")?.Value ?? "",
                                            ItemWorkshopTooltip = element.Element("Tooltip")?.Value ?? "",
                                            ItemKey = element.Element("Key")?.Value,
                                            RowStart = int.Parse(element.Element("RowStart")?.Value ?? "-1"),
                                            RowEnd = int.Parse(element.Element("RowEnd")?.Value ?? "-1"),

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
                            textTable.ItemList = itemList;

                            if (TheWorkshop.IsPreviewMode == false)
                            {
                                CharacterSetManager CharacterManager = new();
                                CharacterManager.DecodeAllItemTexts(textTable);
                            }
                        }
                        if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile) 
                        {
                            XElement MenuTable = XList.Element("MenuTable");
                            if (MenuTable == null)
                            {
                                continue;
                            }

                            TextTable textTable = new();
                            EntryClass.EntryTypeMenu.TextTableTextFile = textTable;
                            textTable.TextTableLinkType = TextTable.TextTableLinkTypes.TextFile;

                            textTable.TextTableFile = LibraryGES.GetGameFileUsingLocation(Database, MenuTable.Element("Location")?.Value);
                            textTable.GameFileLocation = MenuTable.Element("Location")?.Value;
                            textTable.TextTableItemCount = Int32.Parse(MenuTable.Element("ItemCount")?.Value);
                            textTable.TextTableStart = Int32.Parse(MenuTable.Element("Start")?.Value);
                            textTable.TextTableFirstNameID = Int32.Parse(MenuTable.Element("FirstNameID")?.Value);


                            List<TextInfo> itemList = new List<TextInfo>();
                            XElement itemListElement = MenuTable.Descendants("ItemList").FirstOrDefault();
                            bool SetChild = false;
                            LoadItems(itemListElement, itemList); // Local function for recursive loading
                            void LoadItems(XElement parentElement, List<TextInfo> itemList)
                            {
                                foreach (XElement element in parentElement.Elements())
                                {
                                    if (element.Name == "Item" || element.Name == "Folder")
                                    {
                                        TextInfo itemInfo = new TextInfo
                                        {
                                            ItemName = element.Element("Name")?.Value ?? "Unnamed",
                                            ItemIndex = int.Parse(element.Element("Index")?.Value ?? "0"),
                                            ItemNote = element.Element("Note")?.Value ?? "",
                                            ItemWorkshopTooltip = element.Element("Tooltip")?.Value ?? "",
                                            ItemKey = element.Element("Key")?.Value,
                                            RowStart = int.Parse(element.Element("RowStart")?.Value ?? "-1"),
                                            RowEnd = int.Parse(element.Element("RowEnd")?.Value ?? "-1"),

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
                            textTable.ItemList = itemList;

                            if (TheWorkshop.IsPreviewMode == false)
                            {
                                CharacterSetManager CharacterManager = new();
                                CharacterManager.DecodeAllItemTexts(textTable);
                            }
                        }
                        if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor) 
                        {
                            XElement MenuTable = XList.Element("MenuTable");
                            if (MenuTable == null)
                            {
                                continue;
                            }

                            TextTable texttable = new();
                            EntryClass.EntryTypeMenu.TextTableEditor = texttable;
                            texttable.TextTableLinkType = TextTable.TextTableLinkTypes.Editor;
                                                        
                            texttable.TextTableFirstNameID = Int32.Parse(MenuTable.Element("FirstNameID")?.Value);
                            texttable.TextTableItemCount = Int32.Parse(MenuTable.Element("ItemCount")?.Value);
                            texttable.PreviousLinkedEditorName = MenuTable.Element("LinkedEditorName")?.Value;
                            texttable.PreviousLinkedEditorKey = MenuTable.Element("LinkedEditorKey")?.Value;


                            string TheEditorKey = MenuTable.Element("LinkedEditorKey")?.Value;

                            foreach (DataTableEditorData AnEditor in Database.GameEditors.OfType<DataTableEditorData>())
                            {
                                if (AnEditor.EditorKey == TheEditorKey) { texttable.LinkedDTEEditor = AnEditor; break; }
                            }

                            if (texttable.LinkedDTEEditor == null)
                            {
                                //Notification Notification = new("The editor was not found." +
                                //    "\n");

                                string theEditorName = texttable.PreviousLinkedEditorName;
                                string thisEditorName = EditorClass.EditorName;
                                string thisEntryName = EntryClass.Name;
                                string thisEntryID = EntryClass.RowOffset.ToString();

                                PixelWPF.LibraryPixel.NotificationNegative("Error: Editor " + theEditorName + " is missing!",
                                    "Editor (" + thisEditorName + ") has entry (" + thisEntryName + ") trying to pull names from the missing editor (" + theEditorName + ")! " +
                                    "\n\n" +
                                    "Anyway, the editor will still load, but the " +
                                    "\nentry (" + thisEntryName + ") (Entry ID: " + thisEntryID + ") " +
                                    "\nwill be disabled until the missing editor is back. Or you adjust this entry." +
                                    "\n\n" +
                                    "The editor will otherwise work just fine, and you can still save data just fine. "
                                    );

                            }


                        }                        
                        if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
                        {
                            XElement MenuTable = XList.Element("MenuTable");
                            if (MenuTable == null)
                            {
                                continue;
                            }

                            TextTable textTable = new();
                            EntryClass.EntryTypeMenu.TextTableNothing = textTable;
                            textTable.TextTableLinkType = TextTable.TextTableLinkTypes.Nothing;

                            textTable.TextTableFirstNameID = Int32.Parse(MenuTable.Element("FirstNameID")?.Value);

                            List<TextInfo> itemList = new List<TextInfo>();
                            XElement itemListElement = MenuTable.Descendants("ItemList").FirstOrDefault();
                            bool SetChild = false;
                            LoadItems(itemListElement, itemList); // Local function for recursive loading
                            void LoadItems(XElement parentElement, List<TextInfo> itemList)
                            {
                                foreach (XElement element in parentElement.Elements())
                                {
                                    if (element.Name == "Item" || element.Name == "Folder")
                                    {
                                        TextInfo itemInfo = new TextInfo
                                        {
                                            ItemName = element.Element("Name")?.Value ?? "Unnamed",
                                            ItemIndex = int.Parse(element.Element("Index")?.Value ?? "0"),
                                            ItemNote = element.Element("Note")?.Value ?? "",
                                            ItemWorkshopTooltip = element.Element("Tooltip")?.Value ?? "",
                                            ItemKey = element.Element("Key")?.Value,
                                            RowStart = int.Parse(element.Element("RowStart")?.Value ?? "-1"),
                                            RowEnd = int.Parse(element.Element("RowEnd")?.Value ?? "-1"),

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
                            textTable.ItemList = itemList;
                                                        

                        }
                        

                    }
                }




            }//End of LoadEntry method

        } //End of LoadDataTableXMLIntoDatabasePART2




    }
}
