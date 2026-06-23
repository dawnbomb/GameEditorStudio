using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using WpfHexEditor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace GameEditorStudio
{
    
    public static partial class CommandMethodsClass //This file contains saving actions. These save XML data to the users computer. They can also be accessed fia the File menu.
    {
        
        public static void SaveAll(MethodData MethodData)
        {            

            if (MethodData.mainMenu.WorkshopData.LoadedProject != null)
            {
                foreach (Command Command in Database.Commands)
                {
                    if (Command.Key == "638835921950407398-717630473-427782251") //SaveEditors
                    {
                        MethodData EventPack = new();
                        EventPack.Command = Command;
                        EventPack.mainMenu = MethodData.mainMenu;

                        Command.TheMethod?.Invoke(EventPack);
                        break;
                    }
                }


                foreach (Command Command in Database.Commands)
                {
                    if (Command.Key == "638835921950407433-13988325-250675840") //SaveGameData
                    {
                        MethodData EventPack = new();
                        EventPack.Command = Command;
                        EventPack.mainMenu = MethodData.mainMenu;

                        Command.TheMethod?.Invoke(EventPack);
                        break;
                    }
                }

                foreach (Command Command in Database.Commands)
                {
                    if (Command.Key == "638835921950407528-367118138-951819106") //SaveLoadedProjectDocuments
                    {
                        MethodData EventPack = new();
                        EventPack.Command = Command;
                        EventPack.mainMenu = MethodData.mainMenu;

                        Command.TheMethod?.Invoke(EventPack);
                        break;
                    }
                }

            }

            foreach (Command Command in Database.Commands)
            {
                if (Command.Key == "638835921950407461-756556716-682593786") //SaveDocumentsWorkshop
                {
                    MethodData EventPack = new();
                    EventPack.Command = Command;
                    EventPack.mainMenu = MethodData.mainMenu;

                    Command.TheMethod?.Invoke(EventPack);
                    break;
                }
            }            


            foreach (Command Command in Database.Commands)
            {
                if (Command.Key == "638835921950407554-64869249-237672364") //SaveEvents
                {
                    MethodData EventPack = new();
                    EventPack.Command = Command;
                    EventPack.mainMenu = MethodData.mainMenu;

                    Command.TheMethod?.Invoke(EventPack);  
                    break;
                }
            }

            
            if (MethodData.mainMenu.WorkshopData.SelectedProject != null)
            {
                SaveProjectXML(MethodData.mainMenu.WorkshopData.SelectedProject, MethodData.mainMenu.WorkshopData);
            }
            if (MethodData.mainMenu.WorkshopData.LoadedProject != null && MethodData.mainMenu.WorkshopData.LoadedProject != MethodData.mainMenu.WorkshopData.SelectedProject)
            {
                SaveProjectXML(MethodData.mainMenu.WorkshopData.LoadedProject, MethodData.mainMenu.WorkshopData);
            }
        }


        public static void SaveEditors(MethodData MethodData)
        {
            if (MethodData.Command.WorkshopData == null)  //think about this more before adding it
            {
                if (MethodData.mainMenu.WorkshopData == null) { PixelWPF.LibraryPixel.NotificationNegative("Weird Error", "Workshop not set...?"); }
                return;
            }

            WorkshopData Database = MethodData.mainMenu.WorkshopData;
            Workshop TheWorkshop = Database.WorkshopXaml;

            if (TheWorkshop.WorkshopData.IsProjectLoaded == false) //because we save some linked data table file info...
            {
                PixelWPF.LibraryPixel.NotificationNegative("Cannot Save Editors in Preview Mode", "Your not allowed to save workshop editor's while in preview mode. I may add support for it in the future, but for now, you can't.  (Also how did you even trigger this error???)");
                return;
            }

            if (Database.GameEditors.Count == 0) { return; }

            string NameOfEditorThatCrashedSaving = "";

            try
            {
                //Step 1: make sure everything can save to a Example location. 
                string ExtraPath = "";

                ExtraPath = "\\Other\\Dummy Workshops"; //This extra string causes stuff to be saved to a path variant of the normal location, letting us test if a problem would occur, before actually saving to the right location.
                

                Directory.CreateDirectory(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName);
                SaveAllEditors(Database, TheWorkshop, ExtraPath);

                //Step 2: Delete everything in the example location.
                Directory.Delete(LibraryGES.ApplicationLocation + ExtraPath + "\\", true);

                //Step 3: Delete everything in the REAL location.
                ExtraPath = "";
                string FolderPath = LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors";
                DirectoryInfo DummyDirectory = new DirectoryInfo(FolderPath);

                foreach (FileInfo file in DummyDirectory.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo subDirectory in DummyDirectory.GetDirectories())
                {
                    subDirectory.Delete(true);
                }

                //Step 4: Save everything for real.
                SaveAllEditors(Database, TheWorkshop, ExtraPath);



                void SaveAllEditors(WorkshopData Database, Workshop TheWorkshop, string ExtraPath)
                {
                    
                    //List<string> EditorsToDelete = new List<string>(); //Used to delete aka rename old folders. Currently causes a access forbidden crash.

                    string LoadOrderFile = LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors\\" + "LoadOrder.txt"; //causes weird errors if outside this

                    string LoadOrderContent = "";   

                    //For each editor, we are going to save EVERYTHING about it to a XML.
                    //I am manually serializing because people online would not help me understand a better way
                    //and doing it manually has some upsides like complete control, useful in the future when doing updates between program versions.
                    //In the future, all foreach loops may want to be for loops, as i learned it gives a performance increase. For now this works fine.
                    foreach (Editor editor in Database.GameEditors)
                    {
                        NameOfEditorThatCrashedSaving = editor.EditorName;
                        //First we store and clear the current search bar text.
                        //Needed because otherwise it saves literally only the visible items in the treeview, but i still need it to be based on treeview for order.
                        //If you want to test removing the search bar cleansing, make a backup of the workshop first!


                        LoadOrderContent += editor.EditorKey + "     ( "+ editor.EditorName + " )" +  Environment.NewLine; //for load order text
                                                
                        Directory.CreateDirectory(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors\\" + editor.EditorName);

                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.IndentChars = ("    ");
                        settings.CloseOutput = true;
                        settings.OmitXmlDeclaration = true;
                        using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors\\" + editor.EditorName + "\\" + "\\Editor.xml", settings))
                        {

                            writer.WriteStartElement("Editor"); //This is the root of the XML                            
                            writer.WriteElementString("Seperator", "--------------------------------------------------------------------------------------------");                        
                            writer.WriteElementString("Name", editor.EditorName); //This is all misc editor data.
                            if (editor is DataTableEditorData) { writer.WriteElementString("Type", "DataTable"); }
                            else if (editor is TextEditorData) { writer.WriteElementString("Type", "TextEditor"); }
                            else { writer.WriteElementString("Type", "UNKNOWN"); }
                            writer.WriteElementString("Icon", editor.EditorIcon); //This is the name of the icon file that this editor uses.
                            writer.WriteElementString("Key", editor.EditorKey);
                            writer.WriteElementString("Seperator", "--------------------------------------------------------------------------------------------");
                            writer.WriteElementString("CreatedVersion", editor.CreatedDate.ToString());
                            writer.WriteElementString("CreatedDate", editor.CreatedDate);
                            writer.WriteElementString("SavedVersion", LibraryGES.VersionNumber.ToString());
                            writer.WriteElementString("SavedDate", DateTime.Now.ToString("MMM dd yyyy"));
                            writer.WriteElementString("Seperator", "--------------------------------------------------------------------------------------------");

                            if (editor is DataTableEditorData DTEData)
                            {
                                
                                //IF NAME TABLE EXISTS, SAVE ALL NAME TABLE INFO
                                if (DTEData.NameTable != null) 
                                {
                                    string CurrentSearchBarText = DTEData.EditorLeftBar.SearchBar.Text;
                                    DTEData.EditorLeftBar.SearchBar.Text = "";

                                    writer.WriteStartElement("NameTable"); //Info about the file referenced for & table information about the editor's Name List.
                                    SaveTextTable(DTEData.NameTable, "name");
                                    //if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) { writer.WriteElementString("LinkType", "DataFile"); }
                                    //if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile) { writer.WriteElementString("LinkType", "TextFile"); }
                                    //if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Editor) { writer.WriteElementString("LinkType", "Editor"); }
                                    //if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing) { writer.WriteElementString("LinkType", "Nothing"); }
                                    //writer.WriteElementString("FirstNameID", DTEData.NameTable.NameTableFirstNumber.ToString());
                                    //writer.WriteElementString("ItemCount", DTEData.NameTable.NameTableItemCount.ToString()); //How many names / items are in the collection this editor edits. (Like weapons, spells, etc)
                                    //if (DTEData.NameTable.TextTableFile != null)
                                    //{
                                    //    writer.WriteElementString("Location", DTEData.NameTable.TextTableFile.FileLocation);
                                    //}
                                    //if (DTEData.NameTable.TextTableLinkType != TextTable.TextTableLinkTypes.Nothing) 
                                    //{
                                    //    writer.WriteElementString("Start", DTEData.NameTable.TextTableStart.ToString());
                                    //    writer.WriteElementString("RowSize", DTEData.NameTable.TextTableRowSize.ToString());
                                    //    writer.WriteElementString("CharacterSet", DTEData.NameTable.TextTableCharacterSet);
                                    //    writer.WriteElementString("TextSize", DTEData.NameTable.TextTableTextSize.ToString());
                                    //}                                    
                                    //writer.WriteElementString("Key", DTEData.NameTable.TextTableKey.ToString());
                                    //writer.WriteElementString("Seperator", "-------------------------------------------------");
                                    //{
                                    //    writer.WriteStartElement("ItemList");
                                    //    ItemCollection items = DTEData.EditorLeftBar.TreeView.Items;
                                    //    foreach (TreeViewItem treeItem in items)
                                    //    {
                                    //        SaveItemOrFolder(writer, treeItem);
                                    //    }
                                    //    writer.WriteEndElement(); //End ItemList
                                    //}                                    

                                    writer.WriteEndElement(); // End Name Table 


                                    //writer.WriteStartElement("ItemList");
                                    //ItemCollection items = DTEData.EditorLeftBar.TreeView.Items;
                                    //foreach (TreeViewItem treeItem in items)
                                    //{
                                    //    SaveItemOrFolder(writer, treeItem);
                                    //}
                                    //writer.WriteEndElement(); //End ItemList

                                    //void SaveItemOrFolder(XmlWriter writer, TreeViewItem treeItem)
                                    //{
                                    //    TextInfo data = treeItem.Tag as TextInfo;
                                    //    string elementName = data.IsFolder == true ? "Folder" : "Item";

                                    //    writer.WriteStartElement(elementName);
                                    //    if (DTEData.NameTable.TextTableFile == null || string.IsNullOrEmpty(DTEData.NameTable.TextTableFile.FileLocation) || data.IsFolder == true)
                                    //    {
                                    //        writer.WriteElementString("Name", data.ItemName);
                                    //    }

                                    //    writer.WriteElementString("Index", data.ItemIndex.ToString());
                                    //    if (data.ItemNote != "") { writer.WriteElementString("Note", data.ItemNote); }
                                    //    if (data.ItemWorkshopTooltip != "") { writer.WriteElementString("Tooltip", data.ItemWorkshopTooltip); }                                        
                                    //    writer.WriteElementString("Key", data.ItemKey);
                                    //    int MyStart = DTEData.NameTable.TextTableStart + (data.ItemIndex * DTEData.NameTable.TextTableRowSize);
                                    //    int MyEnd = DTEData.NameTable.TextTableStart + ((data.ItemIndex + 1) * DTEData.NameTable.TextTableRowSize) - 1;
                                    //    writer.WriteElementString("RowStart", MyStart.ToString());
                                    //    writer.WriteElementString("RowEnd", MyStart.ToString());

                                    //    // Handle nested items or folders
                                    //    if (data.IsFolder == true && treeItem.HasItems)
                                    //    {
                                    //        foreach (TreeViewItem childItem in treeItem.Items)
                                    //        {
                                    //            SaveItemOrFolder(writer, childItem); // Recursive call
                                    //        }
                                    //    }

                                    //    writer.WriteEndElement(); // End Item or Folder
                                    //}

                                    DTEData.EditorLeftBar.SearchBar.Text = CurrentSearchBarText;
                                }


                                //IF DESCRIPTION TABLE EXISTS, SAVE ALL DESCRIPTION TABLE INFO
                                if (DTEData.DescriptionTableList.Count != 0) 
                                {
                                    TextTable TheDescriptionTable = DTEData.DescriptionTableList[0];
                                    writer.WriteStartElement("DescriptionTable");
                                    SaveTextTable(TheDescriptionTable, "description");
                                    //if (TheDescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) { writer.WriteElementString("LinkType", "DataFile"); }
                                    //if (TheDescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFileAdvanced) { writer.WriteElementString("LinkType", "DataFileAdvanced"); }
                                    //if (TheDescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile) { writer.WriteElementString("LinkType", "TextFile"); }
                                    //if (TheDescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Editor) { writer.WriteElementString("LinkType", "Editor"); }
                                    //if (TheDescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing) { writer.WriteElementString("LinkType", "Nothing"); }
                                    //writer.WriteElementString("Location", TheDescriptionTable.TextTableFile.FileLocation);
                                    //writer.WriteElementString("Start", TheDescriptionTable.TextTableStart.ToString());
                                    //writer.WriteElementString("RowSize", TheDescriptionTable.TextTableRowSize.ToString());
                                    //writer.WriteElementString("CharacterSet", TheDescriptionTable.TextTableCharacterSet);
                                    //writer.WriteElementString("TextSize", TheDescriptionTable.TextTableTextSize.ToString());
                                    //writer.WriteElementString("Key", TheDescriptionTable.TextTableKey.ToString());
                                    writer.WriteEndElement(); // End DescriptionTable
                                }





                                //IF DATA TABLE EXISTS, SAVE ALL DATA TABLE INFO.
                                if (DTEData.DataTable != null)
                                {
                                    writer.WriteStartElement("DataTable"); //Info about the file used for the main data of the editor.
                                    if (DTEData.DataTable.LinkType == DataTable.DataTableLinkTypes.DataFile) { writer.WriteElementString("LinkType", "DataFile"); }
                                    if (DTEData.DataTable.LinkType == DataTable.DataTableLinkTypes.Advanced) { writer.WriteElementString("LinkType", "Advanced"); }
                                    writer.WriteElementString("TableKey", DTEData.DataTable.DataTableKey);
                                    writer.WriteElementString("Location", DTEData.DataTable.FileDataTable.FileLocation);
                                    writer.WriteElementString("Start", DTEData.DataTable.DataTableStart.ToString());
                                    writer.WriteElementString("RowSize", DTEData.DataTable.DataTableRowSize.ToString());
                                    writer.WriteEndElement(); // End EditorFile

                                    writer.WriteStartElement("Grid");
                                    {
                                        writer.WriteStartElement("MergedEntryList");
                                        foreach (var entry in DTEData.MergedEntryList.OrderBy(e => e.RowOffset))
                                        {
                                            SaveAnEntry(entry);
                                        }
                                        writer.WriteEndElement(); //End MergedEntryList


                                        //Rewritten to remove column references. 
                                        writer.WriteStartElement("CategoryList");
                                        foreach (var category in DTEData.CategoryList)
                                        {
                                            if (category.GridItems.Count == 0) { continue; } //Do not save empty categories.

                                            writer.WriteStartElement("Category");
                                            writer.WriteElementString("Name", category.CategoryName);
                                            writer.WriteElementString("Key", category.Key);
                                            writer.WriteElementString("Tooltip", category.Tooltip);

                                            // Group items by their GridItem Column value for human-readable XML
                                            var itemsByColumn = category.GridItems
                                                .GroupBy(item => item.Column)
                                                .OrderBy(group => group.Key); // Sort by column index

                                            //Foreach column.
                                            foreach (var columnGroup in itemsByColumn)
                                            {
                                                writer.WriteStartElement("Column");

                                                var sortedItems = columnGroup.OrderBy(item => item.Row);

                                                //foreach item in column.
                                                foreach (GridItem item in sortedItems)
                                                {

                                                    if (item is Entry Ientry)
                                                    {
                                                        SaveAnEntry(Ientry);
                                                    }

                                                    if (item is Group group)
                                                    {
                                                        writer.WriteStartElement("Group");
                                                        writer.WriteElementString("Name", group.GroupName);
                                                        writer.WriteElementString("Tooltip", group.GroupTooltip);
                                                        writer.WriteElementString("Key", group.Key);

                                                        writer.WriteElementString("Column", item.Column.ToString());
                                                        writer.WriteElementString("Row", item.Row.ToString());
                                                        writer.WriteElementString("ColumnSpan", group.ColumnSpan.ToString());
                                                        writer.WriteElementString("RowSpan", group.RowSpan.ToString()); // your temp value

                                                        var groupItemsByColumn = group.GridItems.GroupBy(item => item.Column).OrderBy(group => group.Key);
                                                        // save foreach column inside the group
                                                        foreach (var groupColumn in groupItemsByColumn)
                                                        {
                                                            writer.WriteStartElement("Column");

                                                            var sortedGroupItems = groupColumn.OrderBy(item => item.Row);

                                                            foreach (GridItem eitem in sortedGroupItems)
                                                            {
                                                                if (eitem is Entry entry)
                                                                {
                                                                    SaveAnEntry(entry);
                                                                }
                                                                // (Optional: nested groups later if you ever allow them)
                                                            }

                                                            writer.WriteEndElement(); // End Column
                                                        }

                                                        writer.WriteEndElement(); //End Group
                                                    }


                                                } //end of items in columnm sorted by row.

                                                writer.WriteEndElement(); //End Column
                                            } //end of items in category, sorted by column.
                                            writer.WriteEndElement(); //End Category
                                        }
                                        writer.WriteEndElement(); //End CategoryList
                                    }
                                    writer.WriteEndElement(); //End Grid

                                    
                                }

                                

                                void SaveTextTable(TextTable texttable, string mytype) 
                                {
                                    if (texttable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) { writer.WriteElementString("LinkType", "DataFile"); }
                                    if (texttable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced) { writer.WriteElementString("LinkType", "Advanced"); }
                                    if (texttable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile) { writer.WriteElementString("LinkType", "TextFile"); }
                                    if (texttable.TextTableLinkType == TextTable.TextTableLinkTypes.Editor) { writer.WriteElementString("LinkType", "Editor"); }
                                    if (texttable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing) { writer.WriteElementString("LinkType", "Nothing"); }                                    
                                    if (texttable.TextTableFile != null)
                                    {
                                        writer.WriteElementString("Location", texttable.TextTableFile.FileLocation);
                                    }
                                    if (texttable.TextTableLinkType != TextTable.TextTableLinkTypes.Nothing)
                                    {
                                        writer.WriteElementString("Start", texttable.TextTableStart.ToString());
                                        writer.WriteElementString("RowSize", texttable.TextTableRowSize.ToString());
                                        writer.WriteElementString("CharacterSet", texttable.TextTableCharacterSet);
                                        writer.WriteElementString("CharacterLimit", texttable.TextTableCharLimit.ToString());
                                    }
                                    writer.WriteElementString("Key", texttable.TextTableKey.ToString());
                                    
                                    if (mytype == "name")
                                    {
                                        writer.WriteElementString("Seperator", "---------------------------------------");
                                        writer.WriteElementString("FirstNameID", texttable.TextTableFirstNameID.ToString());
                                        writer.WriteElementString("ItemCount", texttable.TextTableItemCount.ToString());  //How many names / items are in the collection this editor edits. (Like weapons, spells, etc)
                                        
                                        writer.WriteStartElement("ItemList");
                                        ItemCollection items = DTEData.EditorLeftBar.TreeView.Items;
                                        foreach (TreeViewItem treeItem in items)
                                        {
                                            SaveItemOrFolder(treeItem);
                                        }
                                        writer.WriteEndElement(); //End ItemList
                                    }

                                    if (mytype == "description" && texttable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                                    {
                                        writer.WriteElementString("Seperator", "---------------------------------------");
                                        writer.WriteElementString("ItemCount", texttable.TextTableItemCount.ToString());
                                        writer.WriteStartElement("ItemList");
                                        foreach (TextInfo textinfo in texttable.ItemList)
                                        {
                                            SaveTextRowInfo(textinfo);
                                        }
                                        writer.WriteEndElement(); //End ItemList
                                    }

                                    if (mytype == "menu")
                                    {
                                        writer.WriteElementString("Seperator", "---------------------------------------");
                                        writer.WriteElementString("FirstNameID", texttable.TextTableFirstNameID.ToString());
                                        writer.WriteElementString("ItemCount", texttable.TextTableItemCount.ToString());  //How many names / items are in the collection this editor edits. (Like weapons, spells, etc)

                                        writer.WriteStartElement("ItemList");
                                        foreach (TextInfo textinfo in texttable.ItemList)
                                        {
                                            SaveTextRowInfo(textinfo);
                                        }
                                        writer.WriteEndElement(); //End ItemList
                                        if (texttable.LinkedDTEEditor != null) 
                                        {
                                            writer.WriteElementString("LinkedEditorName", texttable.LinkedDTEEditor.EditorName);
                                            writer.WriteElementString("LinkedEditorKey", texttable.LinkedDTEEditor.EditorKey.ToString());
                                        }
                                        if (texttable.LinkedDTEEditor == null)
                                        {
                                            writer.WriteElementString("LinkedEditorName", texttable.PreviousLinkedEditorName);
                                            writer.WriteElementString("LinkedEditorKey", texttable.PreviousLinkedEditorKey.ToString());
                                        }

                                    }



                                    void SaveItemOrFolder(TreeViewItem treeItem)
                                    {
                                        TextInfo data = treeItem.Tag as TextInfo;
                                        string elementName = data.IsFolder == true ? "Folder" : "Item";

                                        writer.WriteStartElement(elementName);
                                        if (texttable.TextTableFile == null || string.IsNullOrEmpty(texttable.TextTableFile.FileLocation) || data.IsFolder == true)
                                        {
                                            writer.WriteElementString("Name", data.ItemName);
                                        }

                                        writer.WriteElementString("Index", data.ItemIndex.ToString());
                                        if (data.ItemNote != "") { writer.WriteElementString("Note", data.ItemNote); }
                                        if (data.ItemWorkshopTooltip != "") { writer.WriteElementString("Tooltip", data.ItemWorkshopTooltip); }
                                        writer.WriteElementString("Key", data.ItemKey);
                                        writer.WriteElementString("RowStart", data.RowStart.ToString());
                                        writer.WriteElementString("RowEnd", data.RowEnd.ToString());

                                        // Handle nested items or folders
                                        if (data.IsFolder == true && treeItem.HasItems)
                                        {
                                            foreach (TreeViewItem childItem in treeItem.Items)
                                            {
                                                SaveItemOrFolder(childItem); // Recursive call
                                            }
                                        }

                                        writer.WriteEndElement(); // End Item or Folder
                                    }

                                    void SaveTextRowInfo(TextInfo data)
                                    {
                                        string elementName = data.IsFolder == true ? "Folder" : "Item";

                                        writer.WriteStartElement(elementName);
                                        if (texttable.TextTableFile == null || string.IsNullOrEmpty(texttable.TextTableFile.FileLocation) || data.IsFolder == true)
                                        {
                                            writer.WriteElementString("Name", data.ItemName);
                                        }

                                        writer.WriteElementString("Index", data.ItemIndex.ToString());
                                        if (data.ItemNote != "") { writer.WriteElementString("Note", data.ItemNote); }
                                        if (data.ItemWorkshopTooltip != "") { writer.WriteElementString("Tooltip", data.ItemWorkshopTooltip); }
                                        writer.WriteElementString("Key", data.ItemKey);
                                        writer.WriteElementString("RowStart", data.RowStart.ToString());
                                        writer.WriteElementString("RowEnd", data.RowEnd.ToString());

                                        writer.WriteEndElement(); // End Item or Folder
                                    }
                                }


                                //HELPER METHODS
                                void SaveAnEntry(Entry entry)
                                {
                                    writer.WriteStartElement("Entry");
                                    writer.WriteElementString("Name", entry.Name);
                                    writer.WriteElementString("Tooltip", entry.WorkshopTooltip);
                                    writer.WriteElementString("Row", entry.Row.ToString());
                                    writer.WriteElementString("Column", entry.Column.ToString());
                                    if (entry.NewSubType == Entry.EntrySubTypes.BitFlag)
                                    { writer.WriteElementString("RowSpan", "8"); }
                                    else
                                    { writer.WriteElementString("RowSpan", entry.RowSpan.ToString()); }
                                    writer.WriteElementString("ColumnSpan", entry.ColumnSpan.ToString());   
                                    writer.WriteElementString("IsNameHidden", entry.IsNameHidden.ToString());
                                    writer.WriteElementString("IsEntryHidden", entry.IsEntryHidden.ToString());
                                    writer.WriteElementString("RowOffset", entry.RowOffset.ToString());
                                    writer.WriteElementString("Bytes", entry.Bytes.ToString());


                                    if (entry.Endianness == "1" || entry.Endianness == "2L" || entry.Endianness == "4L")
                                    { writer.WriteElementString("Endianness", "Little"); }
                                    else { writer.WriteElementString("Endianness", "Big"); }
                                    //if (entry.Bytes == 0)  { writer.WriteElementString("IsMerged", "True"); }
                                    //else { writer.WriteElementString("IsMerged", "False"); }
                                    writer.WriteElementString("IsMerged", entry.IsMerged.ToString());


                                    if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
                                    {
                                        writer.WriteStartElement("NumberBox");
                                        writer.WriteElementString("Sign", entry.EntryTypeNumberBox.NewNumberSign.ToString());
                                        writer.WriteElementString("Suffix", entry.EntryTypeNumberBox.Suffix);
                                        writer.WriteEndElement(); //End NumberBox 
                                    }


                                    if (entry.NewSubType == Entry.EntrySubTypes.CheckBox)
                                    {
                                        writer.WriteStartElement("CheckBox");
                                        writer.WriteElementString("TrueValue", entry.EntryTypeCheckBox.TrueValue.ToString());
                                        writer.WriteElementString("FalseValue", entry.EntryTypeCheckBox.FalseValue.ToString());
                                        writer.WriteEndElement(); //End CheckBox 
                                    }


                                    if (entry.NewSubType == Entry.EntrySubTypes.BitFlag)
                                    {
                                        writer.WriteStartElement("BitFlag");
                                        writer.WriteElementString("Flag1Name", entry.EntryTypeBitFlag.BitFlag1Name.ToString());
                                        writer.WriteElementString("Flag2Name", entry.EntryTypeBitFlag.BitFlag2Name.ToString());
                                        writer.WriteElementString("Flag3Name", entry.EntryTypeBitFlag.BitFlag3Name.ToString());
                                        writer.WriteElementString("Flag4Name", entry.EntryTypeBitFlag.BitFlag4Name.ToString());
                                        writer.WriteElementString("Flag5Name", entry.EntryTypeBitFlag.BitFlag5Name.ToString());
                                        writer.WriteElementString("Flag6Name", entry.EntryTypeBitFlag.BitFlag6Name.ToString());
                                        writer.WriteElementString("Flag7Name", entry.EntryTypeBitFlag.BitFlag7Name.ToString());
                                        writer.WriteElementString("Flag8Name", entry.EntryTypeBitFlag.BitFlag8Name.ToString());
                                        writer.WriteEndElement(); //End BitFlag    
                                    }



                                    if (entry.NewSubType == Entry.EntrySubTypes.Menu) //A Menu can have upto 65000 options PER entry. (2 bytes)
                                    {
                                        writer.WriteStartElement("Menu");
                                        if (entry.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.Dropdown) { writer.WriteElementString("MenuType", "Dropdown"); }
                                        if (entry.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.List) { writer.WriteElementString("MenuType", "List"); }
                                        
                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile) { writer.WriteElementString("LinkType", "DataFile"); }
                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile) { writer.WriteElementString("LinkType", "TextFile"); }
                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor) { writer.WriteElementString("LinkType", "Editor"); }
                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing) { writer.WriteElementString("LinkType", "Nothing"); }




                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile && entry.EntryTypeMenu.TextTableDataFile != null)
                                        {
                                            writer.WriteStartElement("MenuTable");
                                            SaveTextTable(entry.EntryTypeMenu.TextTableDataFile, "menu");
                                            writer.WriteEndElement(); //End SpecialTable    
                                        }
                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile && entry.EntryTypeMenu.TextTableTextFile != null)
                                        {
                                            writer.WriteStartElement("MenuTable");
                                            SaveTextTable(entry.EntryTypeMenu.TextTableTextFile, "menu");
                                            writer.WriteEndElement(); //End SpecialTable    
                                        }
                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor && entry.EntryTypeMenu.TextTableEditor != null)
                                        {
                                            writer.WriteStartElement("MenuTable");
                                            SaveTextTable(entry.EntryTypeMenu.TextTableEditor, "menu");
                                            writer.WriteEndElement(); //End SpecialTable    
                                        }
                                        if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing && entry.EntryTypeMenu.TextTableNothing != null)
                                        {
                                            writer.WriteStartElement("MenuTable");
                                            SaveTextTable(entry.EntryTypeMenu.TextTableNothing, "menu");
                                            writer.WriteEndElement(); //End SpecialTable    
                                        }
                                        writer.WriteEndElement(); //End Menu    

                                    }

                                    

                                    writer.WriteElementString("EntryKey", entry.Key); // is loaded.
                                    writer.WriteElementString("TableKey", DTEData.DataTable.DataTableKey); //Is loaded, but is unused. It's a future proofing thing.
                                    writer.WriteElementString("Start", DTEData.DataTable.DataTableStart.ToString()); //Pretty sure this isn't loaded later.
                                    writer.WriteElementString("RowSize", DTEData.DataTable.DataTableRowSize.ToString()); //pretty sure this isn't loaded later.

                                    writer.WriteEndElement(); //End Entry
                                }

                               

                            } //End OF STANDARD WIDTH TABLE EDITOR

                            if (editor is TextEditorData textdata)
                            {
                                foreach (GameFile GameFile in textdata.ListOfGameFiles) 
                                {
                                    writer.WriteStartElement("EditorFile"); //Info about the file used for the main data of the editor.
                                    writer.WriteElementString("FileLocation", GameFile.FileLocation);
                                    writer.WriteEndElement(); // End EditorFile
                                }
                               
                            }

                            writer.WriteEndElement(); //End Editor  AKA the Root of the XML   
                            writer.Flush(); //Ends the XML GameFile

                        } //End of using XmlWriter

                        using (XmlWriter FileWriter = XmlWriter.Create(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors\\" + editor.EditorName + "\\" + "\\Files.xml", settings))
                        {                            

                            List<GameFile> SomeGameFiles = new();

                            

                            FileWriter.WriteStartElement("Files"); //
                            FileWriter.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                            FileWriter.WriteElementString("VersionDate", LibraryGES.VersionDate);
                            FileWriter.WriteElementString("Seperator", "--------------------------------------------------------------------------------------------");



                            if (editor is DataTableEditorData DataTableData) 
                            {
                                if (DataTableData.NameTable != null) 
                                {
                                    if (DataTableData.NameTable.TextTableFile != null) //if name table file exists! For editors using a custom name list, it won't exist.
                                    {
                                        GameFile NFile = DataTableData.NameTable.TextTableFile;
                                        if (!SomeGameFiles.Contains(NFile))
                                        {
                                            SomeGameFiles.Add(NFile);
                                        }
                                    }
                                }

                                if (DataTableData.DataTable != null) 
                                {
                                    GameFile AFile = DataTableData.DataTable.FileDataTable;
                                    if (!SomeGameFiles.Contains(AFile))
                                    {
                                        SomeGameFiles.Add(AFile);
                                    }
                                }
                                

                                //if (DataTableData.DescriptionTableList.Count != 0)
                                //{
                                    
                                //}
                                foreach (TextTable ExtraTable in DataTableData.DescriptionTableList)
                                {
                                    GameFile DFile = ExtraTable.TextTableFile;
                                    if (!SomeGameFiles.Contains(DFile))
                                    {
                                        SomeGameFiles.Add(DFile);
                                    }
                                }


                            }                            
                            

                            if (editor is TextEditorData textdata) //Start of Text Editor Files
                            {
                                foreach (GameFile GameFile in textdata.ListOfGameFiles) 
                                {
                                    if (!SomeGameFiles.Contains(GameFile))
                                    {
                                        SomeGameFiles.Add(GameFile);
                                    }
                                }
                                
                            }

                            

                            foreach (Editor AnEditor in Database.GameEditors) 
                            {
                                if (AnEditor is not DataTableEditorData TheEditor) { continue; }

                                foreach (Entry theEntry in TheEditor.DataTableEditorData.MasterEntryList)
                                {
                                    if (theEntry.NewSubType == Entry.EntrySubTypes.Menu)
                                    {
                                        if (theEntry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile )
                                        {
                                            if (theEntry.EntryTypeMenu.TextTableDataFile != null) 
                                            {
                                                GameFile AFile = theEntry.EntryTypeMenu.TextTableDataFile.TextTableFile;
                                                if (AFile != null)
                                                {                                                    
                                                    if (!SomeGameFiles.Contains(AFile))
                                                    {
                                                        SomeGameFiles.Add(AFile);
                                                    }
                                                }
                                            }                                            
                                        }
                                        if (theEntry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                                        {
                                            if (theEntry.EntryTypeMenu.TextTableTextFile != null)
                                            {
                                                GameFile AFile = theEntry.EntryTypeMenu.TextTableTextFile.TextTableFile;
                                                if (AFile != null)
                                                {
                                                    if (!SomeGameFiles.Contains(AFile))
                                                    {
                                                        SomeGameFiles.Add(AFile);
                                                    }
                                                }
                                            }
                                        }


                                    }
                                }
                                
                            }

                            foreach (GameFile AGameFile in SomeGameFiles)
                            {
                                FileWriter.WriteStartElement("File");
                                FileWriter.WriteElementString("Name", AGameFile.FileName);
                                FileWriter.WriteElementString("Location", AGameFile.FileLocation);
                                FileWriter.WriteElementString("Note", AGameFile.FileNote);
                                FileWriter.WriteElementString("Tooltip", AGameFile.FileWorkshopTooltip);
                                FileWriter.WriteEndElement();
                            }

                            FileWriter.WriteEndElement(); // End Files
                            FileWriter.Flush(); //Ends the XML GameFile
                        }

                        //System.IO.File.Delete(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo.xml");
                        //System.IO.File.Move(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo2.xml", LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo.xml");

                        

                    } //End of foreach (database)
                    File.WriteAllText(LoadOrderFile, LoadOrderContent);

                    //This commented section wants to use the EditorsToDelete list declared at the start to delete any editors that should nolonger exist.
                    //Unfortunately i keep getting access forbidden errors / crash, so this is commented while i work on other parts of the program.
                    //as this bug is not a very big deal if users know about it and i can fix it after open beta.


                    //string[] EditorFolderNames = Directory.GetDirectories(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
                    //foreach (string FolderName in EditorFolderNames)
                    //{
                    //    using (var fs = new FileStream(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + FolderName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
                    //    {
                    //        File.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + FolderName);
                    //    }

                    //    //if (!EditorsToDelete.Contains(FolderName))
                    //    //{
                    //    //    System.IO.File.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + FolderName);
                    //    //}
                    //}

                    //foreach (string name in EditorsToDelete) 
                    //{
                    //    System.IO.File.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + name);
                    //}
                    //Delete any folders not in name list?

                } //End of SaveAllEditors (Method)    



            }
            catch
            {

                PixelWPF.LibraryPixel.NotificationNegative("Error: Editors not saved.",
                    "Editor (" + NameOfEditorThatCrashedSaving + ") caused the save operation to stop. " +
                    "\n\n" +
                    "Do not worry :) " +
                    "\n1: Your files are not corrupted." +
                    "\n2: Each part of saving is handled seperately (Editors, game files, documents, etc). There is no chance your game files are affected." +
                    "\n\n" +
                    "EXPLAINED:" +
                    "\n" +
                    "So anyway this is *probably* an invalid name crash. To help make editors easy to share, editors are saved to folders using the names you give them." +
                    "Now each operating system has a diffrent list of symbols it doesn't allow folder names to use, " +
                    "so to deal with this problem the program first runs a simulation of what would happen IF it actually saved anything." +
                    "It saves everything to a temporary folder to make sure the operating system will allow it, and if it works, THEN we perform an actual save." +
                    "This way there is no chance your actual editors folder will get corrupted or result in any other serious error. :)" +
                    "\n\n" +
                    "As you are seeing this error, it almost certinly means your operating system doesn't like atleast one of the symbols you tried using in an editor's name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n\n" +
                    "Note that, yes it's possible for there to be other causes, but like 95% of the time it's just the editor name >_>. "
                    );

                
            }


            
        }


        public static void SaveGameData(MethodData MethodData)//Database Database, Workshop TheWorkshop
        {
            //I need to make it so for the entrys of the currently selected item, if a entry is edited, that it is saved.
            //Currently the user can just swap items back and forth to save to MemoryFile, and then here MemoryFile saves to your PC.

            

            if (MethodData.Command.WorkshopData == null)  //think about this more before adding it
            {
                if (MethodData.mainMenu.WorkshopData == null) { PixelWPF.LibraryPixel.NotificationNegative("Weird Error", "Workshop not set...?"); }
                return;
            }
            if (MethodData.Command.WorkshopData.IsProjectLoaded == false) 
            {
                PixelWPF.LibraryPixel.NotificationNegative("Cannot Save Game Data In Preview Mode", "You don't seem to have a project loaded, so you can't save the workshop's game files. (How did you even trigger this error?)");
                return;
            }


            string SavePath = "";

            if (MethodData.Command.WorkshopData.LoadedProject.ProjectOutputDirectory != "")
            {
                //Make sure this actually exists!
                SavePath = MethodData.Command.WorkshopData.LoadedProject.ProjectOutputDirectory;
            }
            else if (MethodData.Command.WorkshopData.LoadedProject.ProjectInputDirectory != "")
            {
                SavePath = MethodData.Command.WorkshopData.LoadedProject.ProjectInputDirectory;
            }
            else 
            {
                return;
            }

            //add a check for if SavePath location exists

            foreach (GameFile gameFile in MethodData.Command.WorkshopData.GameFiles)
            {
                
                string TheFolderPath = Path.GetDirectoryName(Path.Combine(SavePath, gameFile.FileLocation));
                if (!Directory.Exists(TheFolderPath))
                {
                    Directory.CreateDirectory(TheFolderPath);
                }

                File.WriteAllBytes(SavePath + "\\" + gameFile.FileLocation, gameFile.FileBytes); //saves to the path i set, everything in the array.
            }
                        
        }

        public static void SaveDocumentsWorkshop(MethodData ActionPack) 
        {
            if (ActionPack.mainMenu.WorkshopData == null)  
            {
                if (ActionPack.mainMenu.WorkshopData == null) { PixelWPF.LibraryPixel.NotificationNegative("Weird Error", "Workshop not set...?"); }
                return;
            }

            //ActionPack.mainMenu.WorkshopData.WorkshopXaml.TheDocumentsUserControl.SaveAllDocumentsWorkshop();

            WorkshopData WorkshopData = ActionPack.mainMenu.WorkshopData;

            try
            {
                //First we do a test save to a dummy location. This makes it so if it crashes, file corruption won't happen to the real file locations.
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\DOCocumentationFolderTest");
                foreach (Document Document in WorkshopData.WorkshopDocumentsList)
                {
                    Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\DOCocumentationFolderTest\\" + Document.Name);
                    System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\DOCocumentationFolderTest\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.

                }
                Directory.Delete(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\DOCocumentationFolderTest", true);

                //Assuming there was no errors, the next chunk actually saves the documents.
                //Instead of trying to properly deal with the logistics of renamed document folders, we just blow up the entire Documents folder and recreate it.  
                LibraryGES.NukeDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents");


                string DocumentOrder = "";
                foreach (Document Document in WorkshopData.WorkshopDocumentsList)
                {
                    Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents\\" + Document.Name);
                    System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    DocumentOrder = DocumentOrder + Document.Name + "\n";
                }
                System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents\\" + "LoadOrder.txt", DocumentOrder);


            }
            catch
            {
                LibraryGES.NukeDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\DOCocumentationFolderTest");

                PixelWPF.LibraryPixel.NotificationNegative("Error: WORKSHOP Documentation not saved.",
                    "An error occured during the \"Saving Documentation\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
                    "\n\n" +
                    "As you were probably saving more then only your documentation, you'll be happy to hear that each part of saving is handled seperately. " +
                    "This means there is NO CHANCE that any other parts of the saving operation are affected, such as when also saving editors or saving workshop files. " +
                    "\n\n" +
                    "Documentats are saved using the names you give them to actual folders. " +
                    "This can cause problems as each operating system has a diffrent list of symbols it doesn't allow folder names to use. " +
                    "For safety, we first simulate what would happen IF it actually saved anything, " +
                    "by creating a temporary dummy folder and saving everything to that temporary folder. " +
                    "This way there is no chance your actual document folder will get corrupted or result in any other serious error. :)" +
                    "\n\n" +
                    "As your seeing this error, it means your operating system doesn't like atleast one of the symbols you tried using in a documents name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n\n" +
                    "Try changing the names of any documents you think might have caused the error, and then try saving your documents again. " +
                    "\n\n" +
                    "Yes, the problem is the document NAMES, not the text inside them."
                    );
            }
        }

        public static void SaveLoadedProjectDocuments(MethodData ActionPack)
        {
            if (ActionPack.mainMenu.WorkshopData == null)
            {
                PixelWPF.LibraryPixel.NotificationNegative("Weird Error", "Workshop not set...?");
                return;
            }

            WorkshopData WorkshopData = ActionPack.mainMenu.WorkshopData;

            if (ActionPack.mainMenu.WorkshopData.IsProjectLoaded == false)
            {
                PixelWPF.LibraryPixel.NotificationNegative("Cannot Save Project Documents", "There is no loaded project. \n(Also how did you trigger this error?)");
                return;
            }

            try
            {

                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\ProjectFolderTestSave");
                foreach (Document Document in WorkshopData.ProjectDocumentsList)
                {
                    Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\ProjectFolderTestSave\\" + Document.Name);
                    File.WriteAllText(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\ProjectFolderTestSave\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.

                }

                Directory.Delete(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\ProjectFolderTestSave", true);
                LibraryGES.NukeDirectory(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents");
                //Assuming there was no errors, the next chunk actually saves the documents.
                //Instead of trying to properly deal with the logistics of renamed document folders, we just blow up the entire Documents folder and recreate it.       


                //Directory.CreateDirectory(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documents");
                string DocumentOrder = "";
                foreach (Document Document in WorkshopData.ProjectDocumentsList)
                {
                    Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents\\" + Document.Name);
                    File.WriteAllText(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents\\" + Document.Name + "\\Text.txt", Document.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    DocumentOrder = DocumentOrder + Document.Name + "\n";

                }
                File.WriteAllText(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents\\" + "LoadOrder.txt", DocumentOrder);




            }
            catch
            {
                LibraryGES.NukeDirectory(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\ProjectFolderTestSave");

                PixelWPF.LibraryPixel.NotificationNegative("Error: PROJECT Documentation not saved.",
                    "An error occured during the \"Saving Documentation\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
                    "\n\n" +
                    "As you were probably saving more then only your documentation, you'll be happy to hear that each part of saving is handled seperately. " +
                    "This means there is NO CHANCE that any other parts of the saving operation are affected, such as when also saving editors or saving workshop files. " +
                    "\n\n" +
                    "Documentats are saved using the names you give them to actual folders. " +
                    "This can cause problems as each operating system has a diffrent list of symbols it doesn't allow folder names to use. " +
                    "For safety, we first simulate what would happen IF it actually saved anything, " +
                    "by creating a temporary dummy folder and saving everything to that temporary folder. " +
                    "This way there is no chance your actual document folder will get corrupted or result in any other serious error. :)" +
                    "\n\n" +
                    "As your seeing this error, it means your operating system doesn't like atleast one of the symbols you tried using in a documents name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n\n" +
                    "Try changing the names of any documents you think might have caused the error, and then try saving your documents again. " +
                    "\n\n" +
                    "Yes, the problem is the document NAMES, not the text inside them."
                    );

            }
        }


        public static void SaveEvents(MethodData MethodData) 
        {
            if (MethodData.mainMenu.WorkshopData == null) { PixelWPF.LibraryPixel.NotificationNegative("Weird Error","Workshop not set...?"); }

            string WorkshopName = MethodData.mainMenu.WorkshopData.WorkshopName;
            
            //Step 1: Make sure everything can save to a Example location, letting us test if a problem (crash) would occur, before actually saving to the right location.
            string ExtraPath = "\\Lab"; //This extra string causes stuff to be saved to a path variant of the normal location, 
            Directory.CreateDirectory(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\");
            Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Events\\");

            bool Failed = false; //yes this is used. WAY down below in the catch part of try catch.

            //Step 1.5: Save to the example location.
            SaveEventsToXML(ExtraPath);

            //Step 2: Delete everything in the example location.
            Directory.Delete(LibraryGES.ApplicationLocation + ExtraPath + "\\", true);
                        
            if (Failed == true) { return; }


            //Step 3: Delete everything in the REAL location.
            ExtraPath = "";
            string FolderPath = LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events";
            DirectoryInfo DummyDirectory = new DirectoryInfo(FolderPath);
            foreach (FileInfo file in DummyDirectory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subDirectory in DummyDirectory.GetDirectories())
            {
                subDirectory.Delete(true);
            }

            
            //Step 4: Save everything for real.            
            SaveEventsToXML(ExtraPath);
            SaveWorkshopXml(MethodData.mainMenu.WorkshopData);


            void SaveEventsToXML(string ExtraPath)
            {
                try
                {
                    string LoadOrderFile = LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\" + "LoadOrder.txt"; //causes weird errors if outside this
                    string LoadOrderContent = "";

                    foreach (Event Event in MethodData.mainMenu.WorkshopData.WorkshopEvents)
                    {
                        Directory.CreateDirectory(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\" + Event.DisplayName + "\\");

                        LoadOrderContent += Event.DisplayName + Environment.NewLine; //for load order text

                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.IndentChars = ("    ");
                        settings.CloseOutput = true;
                        settings.OmitXmlDeclaration = true;
                        using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\" + Event.DisplayName + "\\Event.xml", settings))
                        {
                            writer.WriteStartElement("Event");
                            writer.WriteElementString("CreatedVersion", Event.CreatedDate.ToString());
                            writer.WriteElementString("CreatedDate", Event.CreatedDate);
                            writer.WriteElementString("SavedVersion", LibraryGES.VersionNumber.ToString());
                            writer.WriteElementString("SavedDate", DateTime.Now.ToString("MMM dd yyyy"));
                            writer.WriteElementString("Seperator", "----------------------------------------------------------------------------------");
                            writer.WriteElementString("Note", "The resource's name is unused and for debugging. The current name could be diffrent.");
                            writer.WriteElementString("Note2", "The Resource's Key is the WorkshopResourceKey. Not the ProjectResourceKey.");
                            writer.WriteElementString("Seperator", "================================================================================");
                            writer.WriteElementString("Name", Event.DisplayName);
                            writer.WriteElementString("Note", Event.Note);
                            writer.WriteElementString("Tooltip", Event.Tooltip);

                            writer.WriteStartElement("CommandList");
                            foreach (EventCommand AnEventCommand in Event.CommandList)
                            {
                                writer.WriteStartElement("Command");
                                
                                writer.WriteElementString("Name", AnEventCommand.Command.DisplayName);
                                writer.WriteElementString("Key", AnEventCommand.Command.Key);                                
                                writer.WriteStartElement("ResourceList");                                
                                foreach (string thekey in AnEventCommand.ResourceKeys.Values)
                                {
                                    //if (thekey == "") { continue; }
                                    writer.WriteStartElement("Resource");
                                    EventResource eventResource = MethodData.mainMenu.WorkshopData.WorkshopEventResources.Find(thing => thing.Key == thekey);
                                    writer.WriteElementString("Name", eventResource?.Name);
                                    writer.WriteElementString("Key", thekey);  
                                    writer.WriteEndElement(); //End Command
                                }                                
                                writer.WriteEndElement(); //End Resources

                                {   //SAVE SOME SPECIAL STUFF IF THIS IS THE COMMAND PROMPT COMMAND.                                    
                                    if (AnEventCommand.Command.Key == "CMD1" || AnEventCommand.Command.Key == "CMD2" || AnEventCommand.Command.Key == "CMD3") 
                                    {
                                        writer.WriteElementString("CMD", "True");
                                        if (AnEventCommand.Command.Key == "CMD1") { writer.WriteElementString("SPECIAL_NOTE", "This (CMD1) is Stay Open Mode"); }
                                        if (AnEventCommand.Command.Key == "CMD2") { writer.WriteElementString("SPECIAL_NOTE", "This (CMD2) is Auto Close Mode"); }
                                        if (AnEventCommand.Command.Key == "CMD3") { writer.WriteElementString("SPECIAL_NOTE", "This (CMD3) is Hidden Mode"); }

                                        writer.WriteStartElement("CMDResourceList");
                                        foreach (CommandResource cmdResource in AnEventCommand.CMDList)
                                        {
                                            writer.WriteStartElement("CMDResource");
                                            writer.WriteElementString("CMDType", cmdResource.Type.ToString());
                                            writer.WriteElementString("CMDTextKey", cmdResource.CMDTextKey);                                            
                                            writer.WriteEndElement(); //End Command
                                        }
                                        writer.WriteEndElement(); //End CMDResourceList
                                    }
                                }
                                

                                writer.WriteEndElement(); //End Command
                            }
                            writer.WriteEndElement(); //End Commands

                            writer.WriteEndElement(); //End Event
                            writer.Flush(); //Ends the XML GameFile
                        }
                    }
                    File.WriteAllText(LoadOrderFile, LoadOrderContent);










                    //Save the Resources.xml
                    foreach (Event Event in MethodData.mainMenu.WorkshopData.WorkshopEvents)
                    {    
                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.IndentChars = ("    ");
                        settings.CloseOutput = true;
                        settings.OmitXmlDeclaration = true;
                        using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\" + Event.DisplayName + "\\Resources.xml", settings))
                        {
                            writer.WriteStartElement("Resources");
                            writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                            writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
                            writer.WriteElementString("Seperator", "================================================================================");

                            writer.WriteStartElement("ResourceList");
                            List<EventResource> savedresources = new();
                            List<EventResource> savedresourceparents = new();
                            foreach (EventCommand AnEventCommand in Event.CommandList)
                            {                                
                                foreach (string thekey in AnEventCommand.ResourceKeys.Values) //first save used resources only.
                                {
                                    EventResource eventResource = MethodData.mainMenu.WorkshopData.WorkshopEventResources.Find(thing => thing.Key == thekey);
                                    if (thekey == "") { continue; }
                                    if (savedresources.Contains(eventResource) || savedresourceparents.Contains(eventResource)) { continue; }
                                    SaveResource(eventResource);
                                    savedresources.Add(eventResource);
                                }
                                foreach (EventResource resource in savedresources) //then save any parent resources that the used resources rely on.
                                {
                                    if (resource.ParentKey != "" && resource.ParentKey != null) 
                                    {
                                        EventResource eventResource = MethodData.mainMenu.WorkshopData.WorkshopEventResources.Find(thing => thing.Key == resource.ParentKey);

                                        if (eventResource == null) 
                                        { throw new Exception("Forcing catch!"); } //May happen if the child parent link is broken.
                                        if (savedresources.Contains(eventResource) || savedresourceparents.Contains(eventResource))  { continue; }

                                        SaveResource(eventResource);
                                        savedresourceparents.Add(eventResource);

                                    }
                                    
                                }
                            }
                            writer.WriteEndElement(); //End ResoueceList

                            writer.WriteEndElement(); //End Event
                            writer.Flush(); //Ends the XML GameFile

                            void SaveResource(EventResource eventResource) 
                            {
                                writer.WriteStartElement("Resource");                               
                                
                                writer.WriteElementString("Name", eventResource.Name); //I had a crash here i never confirmed as fixed. It was actually because eventResource was null. I may have fixed it, but i can't recreate it to make sure...
                                writer.WriteElementString("Key", eventResource.Key);

                                if (eventResource.ResourceType == EventResource.ResourceTypes.File)
                                {
                                    writer.WriteElementString("ResourceType", "File");
                                }
                                if (eventResource.ResourceType == EventResource.ResourceTypes.Folder)
                                {
                                    writer.WriteElementString("ResourceType", "Folder");
                                }
                                if (eventResource.ResourceType == EventResource.ResourceTypes.CMDText)
                                {
                                    writer.WriteElementString("ResourceType", "CMDText");
                                }
                                if (eventResource.IsChild == false)
                                {
                                    writer.WriteElementString("IsChild", "False");
                                }
                                if (eventResource.IsChild == true)
                                {
                                    writer.WriteElementString("IsChild", "True");
                                }

                                writer.WriteElementString("RequiredName", eventResource.RequiredName.ToString());  //if full path (local)
                                writer.WriteElementString("Location", eventResource.Location);  //if partial path
                                writer.WriteElementString("ParentKey", eventResource.ParentKey); //if partial path  
                                writer.WriteEndElement(); //End Resource
                            }
                        }
                    }
                    File.WriteAllText(LoadOrderFile, LoadOrderContent);
                }
                catch
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Events failed to save properly.",
                        "Don't worry, your events SHOULD be fine, as they simulate saving before actually saving (so most save crashes dont affect actual data)" +
                        "\n\n" +
                        "I need to write the rest of this error message later." +
                        "\n\n" +
                        "PS: The program won't crash from this, but it also won't save events even if you try again. You should probably save whatever else you did, and restart the program."
                        );                    
                    Failed = true;
                    return;
                    
                }


            }
        }

        // To fix the CS0120 error, the method `SaveWorkshopXml` must either be made static or called on an instance of `CommandMethodsClass`.
        // Since the method is being called from a static context, the simplest fix is to make `SaveWorkshopXml` static.

        public static void SaveWorkshopXml(WorkshopData workshopData) //Not a command method i just wanted it in the save .cs file as the other xml saves.
        {
            //Save a test example. If this fails, the real file is not corrupted. 
            string LibraryXmlPath = LibraryGES.ApplicationLocation + "\\Workshops\\" + workshopData.WorkshopName + "\\" + "LibraryTestSave.xml";
            SaveIt();
            try
            {
                if (File.Exists(LibraryXmlPath)) { File.Delete(LibraryXmlPath); }
            }
            catch
            {

            }

            //Save over the real workshop file. The test didn't fail, so this should be fine.
            LibraryXmlPath = LibraryGES.ApplicationLocation + "\\Workshops\\" + workshopData.WorkshopName + "\\" + "Workshop.xml";
            SaveIt();

            void SaveIt()
            {
                try
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryXmlPath, settings))
                    {
                        writer.WriteStartElement("Workshop");
                        writer.WriteElementString("CreatedVersion", workshopData.CreatedVersion.ToString());
                        writer.WriteElementString("CreatedDate", workshopData.CreatedDate);
                        writer.WriteElementString("SavedVersion", LibraryGES.VersionNumber.ToString());
                        writer.WriteElementString("SavedDate", DateTime.Now.ToString("MMM dd yyyy"));
                        writer.WriteElementString("Seperator", "----------------------------------------------------------------------------------");
                        writer.WriteElementString("WorkshopName", workshopData.WorkshopName); //Note: This is for reference only, so i can tell what workshop a file is for when it's open in notepad. This isn't actually used anywhere. 
                        writer.WriteElementString("InputLocation", workshopData.WorkshopInputDirectory);
                        writer.WriteElementString("ProjectsRequireSameInputFolderName", workshopData.ProjectsRequireSameFolderName == true ? "true" : "false");
                        writer.WriteElementString("Seperator", "----------------------------------------------------------------------------------");

                        writer.WriteStartElement("ResourceList");
                        foreach (EventResource WorkshopEventResource in workshopData.WorkshopEventResources)
                        {
                            //when xml loads, variables WILL be null, even if they have a default value, if it's not written to begin with. this is very annoying. 
                            writer.WriteStartElement("Resource");
                            writer.WriteElementString("Name", WorkshopEventResource.Name);
                            writer.WriteElementString("Key", WorkshopEventResource.Key);

                            if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.File)
                            {
                                writer.WriteElementString("ResourceType", "File");
                            }
                            if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.Folder)
                            {
                                writer.WriteElementString("ResourceType", "Folder");
                            }
                            if (WorkshopEventResource.IsChild == false)
                            {
                                writer.WriteElementString("IsChild", "False");
                            }
                            if (WorkshopEventResource.IsChild == true)
                            {
                                writer.WriteElementString("IsChild", "True");
                            }

                            writer.WriteElementString("RequiredName", WorkshopEventResource.RequiredName.ToString());  //if full path (local)
                            writer.WriteElementString("Location", WorkshopEventResource.Location);  //if partial path
                            writer.WriteElementString("ParentKey", WorkshopEventResource.ParentKey); //if partial path                    


                            writer.WriteEndElement(); //End File
                        }
                        writer.WriteEndElement(); //End ResourceList 


                        writer.WriteEndElement(); //End Root (Library)
                        writer.Flush(); //Ends the XML Library file                                

                    }
                }
                catch
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Workshop.xml failed to save properly.",
                        "All saves (are supposed to be) simulated in this program, so pre-existing data should be fine... " +
                        "but...this is really weird! This one especially should never crash! What the hell did you do?!?" +
                        "\n\n" +
                        "You should DEFINATLY restart the program."
                        );
                    return;
                }
                
            }


            //Library.RefreshWorkshopTree();
        }


        public static void SaveProjectXML(Project ProjectData, WorkshopData workshopData) //Not a command method i just wanted it in the save .cs file as the other xml saves.
        {
            if (ProjectData.ProjectEventResources == null) { ProjectData.ProjectEventResources = new(); }

            string TestLocation = LibraryGES.ApplicationLocation + "\\Projects\\" + "GESDummyProject" + "\\" + ProjectData.ProjectName + "\\" + "Project.xml";
            string RealLocation = LibraryGES.ApplicationLocation + "\\Projects\\" + workshopData.WorkshopName + "\\" + ProjectData.ProjectName + "\\" + "Project.xml";




            try
            {
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Projects\\" + "GESDummyProject" + "\\" + ProjectData.ProjectName + "\\");
                SavePXML(TestLocation);
                SavePXML(RealLocation);
                Directory.Delete(LibraryGES.ApplicationLocation + "\\Projects\\GESDummyProject\\", true);

            }
            catch
            {
                PixelWPF.LibraryPixel.NotificationNegative("Error: Failed to save project.",
                        "First off don't panic, your project files are fine / not corrupted. When project saving fails it doesn't even attempt to save (this way i can make absolutly sure your files will be safe)." +
                        "\n\nThe project.xml file only contains the basic project info (Name, Input/Output folder locations, and any project resource locations). Everything else is saved seperatly (To reduce the impact of any failed to save errors). This error isn't even a bad one, just uh, restart the program and you will be fine. " +
                        "\n\nThat said, the saving process did crash, so please write down everything you remember doing before this happened and report it. Thank you! :3"
                        );
            }

            void SavePXML(string theLocation)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("    ");
                settings.CloseOutput = true;
                settings.OmitXmlDeclaration = true;
                using (XmlWriter writer = XmlWriter.Create(theLocation, settings))
                {
                    writer.WriteStartElement("Project"); //This is the root of the XML
                    writer.WriteElementString("CreatedVersion", ProjectData.CreatedDate.ToString());
                    writer.WriteElementString("CreatedDate", ProjectData.CreatedDate);
                    writer.WriteElementString("SavedVersion", LibraryGES.VersionNumber.ToString());
                    writer.WriteElementString("SavedDate", DateTime.Now.ToString("MMM dd yyyy"));
                    writer.WriteElementString("Seperator", "----------------------------------------------------------------------------------");
                    writer.WriteElementString("NOTE", "The resources are (Project Event Resources) with a key matching (Workshop Event Resources).");
                    writer.WriteElementString("Seperator", "====================================================================================");
                    writer.WriteElementString("Name", ProjectData.ProjectName);
                    writer.WriteElementString("InputLocation", ProjectData.ProjectInputDirectory);
                    writer.WriteElementString("OutputLocation", ProjectData.ProjectOutputDirectory);

                    writer.WriteStartElement("ResourceList");
                    foreach (ProjectEventResource ProjectEventData in ProjectData.ProjectEventResources)
                    {
                        writer.WriteStartElement("Resource");
                        writer.WriteElementString("Name", workshopData.WorkshopEventResources.Find(thing => thing.Key == ProjectEventData.Key).Name);
                        writer.WriteElementString("Key", ProjectEventData.Key);
                        writer.WriteElementString("Location", ProjectEventData.Location);
                        writer.WriteEndElement(); //End Event Resources
                    }
                    writer.WriteEndElement(); //End Event Resources

                    writer.WriteEndElement(); //End Project  AKA the Root of the XML   
                    writer.Flush(); //Ends the XML
                }
            }



            //DataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
            string TheProjectFolder = LibraryGES.ApplicationLocation + "\\Projects\\" + workshopData.WorkshopName + "\\" + ProjectData.ProjectName + "\\";

            if (Directory.Exists(TheProjectFolder + "\\" + "Documents" + "\\"))  //Documents Folder
            {
            }
            else
            {
                Directory.CreateDirectory(TheProjectFolder + "\\" + "Documents" + "\\");
            }

            if (Directory.Exists(TheProjectFolder + "\\" + "\\Documents\\LoadOrder.txt")) //LoadOrder.txt file
            {

            }
            else
            {
                System.IO.File.WriteAllText(TheProjectFolder + "\\" + "\\Documents\\LoadOrder.txt", " ");
            }



        }


    }
}
