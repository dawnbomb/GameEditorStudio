using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Windows.Security.Authentication.Web.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace GameEditorStudio
{
    
    public partial class CommandMethodsClass //This file contains saving actions. These save XML data to the users computer. They can also be accessed fia the File menu.
    {

        public static void SaveAll(MethodData MethodData)
        {                   

            foreach (Command Command in TrueDatabase.Commands)
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

            //CommandMethodsClass.SaveGameData(MethodData);

            foreach (Command Command in TrueDatabase.Commands)
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

            foreach (Command Command in TrueDatabase.Commands)
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

            foreach (Command Command in TrueDatabase.Commands)
            {
                if (Command.Key == "638835921950407528-367118138-951819106") //SaveDocumentsProject
                {
                    MethodData EventPack = new();
                    EventPack.Command = Command;
                    EventPack.mainMenu = MethodData.mainMenu;

                    Command.TheMethod?.Invoke(EventPack);  
                    break;
                }
            }            

            //Command.Database.TheDocumentsUserControl.SaveAllDocumentsWorkshop();
            //Command.Database.TheDocumentsUserControl.SaveAllDocumentsProject();



            foreach (Command Command in TrueDatabase.Commands)
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
        }


        public static void SaveEditors(MethodData ActionPack)
        {
            if (ActionPack.Command.WorkshopData == null)  //think about this more before adding it
            {
                return;
            }

            WorkshopData Database = ActionPack.mainMenu.WorkshopData;
            Workshop TheWorkshop = Database.Workshop;

            if (TheWorkshop.IsPreviewMode == true) { return; }

            if (Database.GameEditors.Count == 0) { return; }

            try
            {
                //Step 1: make sure everything can save to a Example location. 
                string ExtraPath = "";

                ExtraPath = "\\Other\\Dummy Workshops"; //This extra string causes stuff to be saved to a path variant of the normal location, letting us test if a problem would occur, before actually saving to the right location.
                

                Directory.CreateDirectory(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName);
                SaveAllEditors(Database, TheWorkshop, ExtraPath);

                //Step 2: Delete everything in the example location.
                Directory.Delete(LibraryMan.ApplicationLocation + ExtraPath + "\\", true);

                //Step 3: Delete everything in the REAL location.
                ExtraPath = "";
                string FolderPath = LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors";
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
                    
                    List<string> EditorsToDelete = new List<string>(); //Used to delete aka rename old folders. Currently causes a access forbidden crash.

                    string LoadOrderFile = LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + "LoadOrder.txt"; //causes weird errors if outside this

                    string LoadOrderContent = "";   

                    //For each editor, we are going to save EVERYTHING about it to a XML.
                    //I am manually serializing because people online would not help me understand a better way
                    //and doing it manually has some upsides like complete control, useful in the future when doing updates between program versions.
                    //In the future, all foreach loops may want to be for loops, as i learned it gives a performance increase. For now this works fine.
                    foreach (KeyValuePair<string, Editor> editor in Database.GameEditors)
                    {
                        //First we store and clear the current search bar text.
                        //Needed because otherwise it saves literally only the visible items in the treeview, but i still need it to be based on treeview for order.
                        //If you want to test removing the search bar cleansing, make a backup of the workshop first!
                        
                        


                        LoadOrderContent += editor.Key + Environment.NewLine; //for load order text

                        EditorsToDelete.Add(editor.Key);//Add editor string to name list
                        Directory.CreateDirectory(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key);

                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.IndentChars = ("    ");
                        settings.CloseOutput = true;
                        settings.OmitXmlDeclaration = true;
                        using (XmlWriter writer = XmlWriter.Create(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\Editor.xml", settings))
                        {

                            writer.WriteStartElement("Editor"); //This is the root of the XML
                            writer.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                            writer.WriteElementString("VersionDate", LibraryMan.VersionDate);                            
                            writer.WriteElementString("Name", editor.Key); //This is all misc editor data.
                            writer.WriteElementString("Type", editor.Value.EditorType);
                            writer.WriteElementString("Icon", editor.Value.EditorIcon); //This is the name of the file that this editor uses.
                            writer.WriteElementString("Key", editor.Value.EditorKey);
                            writer.WriteElementString("Seperator", "----------------------------------------------------------------------------------");


                            if (editor.Value.EditorType == "DataTable")
                            {
                                string CurrentSearchBarText = editor.Value.StandardEditorData.EditorLeftDockPanel.SearchBar.Text;
                                editor.Value.StandardEditorData.EditorLeftDockPanel.SearchBar.Text = "";

                                writer.WriteStartElement("NameTable"); //Info about the file referenced for & table information about the editor's Name List.
                                if (editor.Value.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.DataFile) { writer.WriteElementString("LinkType", "DataFile"); }
                                if (editor.Value.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.TextFile) { writer.WriteElementString("LinkType", "TextFile"); }
                                if (editor.Value.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.Editor) { writer.WriteElementString("LinkType", "Editor"); }
                                if (editor.Value.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.Nothing) { writer.WriteElementString("LinkType", "Nothing"); }
                                if (editor.Value.StandardEditorData.FileNameTable != null)
                                {
                                    writer.WriteElementString("Location", editor.Value.StandardEditorData.FileNameTable.FileLocation);
                                }
                                else
                                {
                                    writer.WriteElementString("Location", "");
                                }
                                writer.WriteElementString("Start", editor.Value.StandardEditorData.NameTableStart.ToString());
                                writer.WriteElementString("RowSize", editor.Value.StandardEditorData.NameTableRowSize.ToString());
                                writer.WriteElementString("CharacterSet", editor.Value.StandardEditorData.NameTableCharacterSet);
                                writer.WriteElementString("TextSize", editor.Value.StandardEditorData.NameTableTextSize.ToString());
                                writer.WriteElementString("ItemCount", editor.Value.StandardEditorData.NameTableItemCount.ToString()); //How many names / items are in the collection this editor edits. (Like weapons, spells, etc)
                                writer.WriteElementString("Key", editor.Value.StandardEditorData.NameTableKey.ToString());
                                writer.WriteEndElement(); // End NameFile


                                
                                writer.WriteStartElement("DataTable"); //Info about the file used for the main data of the editor.
                                writer.WriteElementString("TableKey", editor.Value.StandardEditorData.TableKey);
                                writer.WriteElementString("Location", editor.Value.StandardEditorData.FileDataTable.FileLocation);
                                writer.WriteElementString("Start", editor.Value.StandardEditorData.DataTableStart.ToString());
                                writer.WriteElementString("RowSize", editor.Value.StandardEditorData.DataTableRowSize.ToString());
                                writer.WriteEndElement(); // End EditorFile


                                if (editor.Value.StandardEditorData.DescriptionTableList.Count != 0) 
                                {
                                    DescriptionTable DescriptionTable = editor.Value.StandardEditorData.DescriptionTableList[0];
                                    writer.WriteStartElement("DescriptionTable");
                                    if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.DataFile) { writer.WriteElementString("LinkType", "DataFile"); }
                                    if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.TextFile) { writer.WriteElementString("LinkType", "TextFile"); }
                                    if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.Editor) { writer.WriteElementString("LinkType", "Editor"); }
                                    if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.Nothing) { writer.WriteElementString("LinkType", "Nothing"); }
                                    writer.WriteElementString("Location", DescriptionTable.FileTextTable.FileLocation);
                                    writer.WriteElementString("Start", DescriptionTable.Start.ToString());
                                    writer.WriteElementString("RowSize", DescriptionTable.RowSize.ToString());
                                    writer.WriteElementString("CharacterSet", DescriptionTable.CharacterSet);
                                    writer.WriteElementString("TextSize", DescriptionTable.TextSize.ToString());
                                    writer.WriteElementString("Key", DescriptionTable.Key.ToString());
                                    writer.WriteEndElement(); // End DescriptionTable
                                }
                                
                                

                                
                                
                                

                                writer.WriteStartElement("ItemList");   // Start of the ItemList
                                void SaveItemOrFolder(XmlWriter writer, TreeViewItem treeItem)
                                {
                                    ItemInfo data = treeItem.Tag as ItemInfo;
                                    string elementName = data.IsFolder == true ? "Folder" : "Item";

                                    writer.WriteStartElement(elementName);
                                    if (editor.Value.StandardEditorData.FileNameTable == null || string.IsNullOrEmpty(editor.Value.StandardEditorData.FileNameTable.FileLocation) || data.IsFolder == true)
                                    {
                                        writer.WriteElementString("Name", data.ItemName);
                                    }
                                                                        
                                    writer.WriteElementString("Index", data.ItemIndex.ToString());
                                    writer.WriteElementString("Note", data.ItemNote);
                                    writer.WriteElementString("Notepad", data.ItemNotepad);
                                    writer.WriteElementString("Key", data.ItemKey);

                                    // Handle nested items or folders
                                    if (data.IsFolder == true && treeItem.HasItems)
                                    {
                                        foreach (TreeViewItem childItem in treeItem.Items)
                                        {
                                            SaveItemOrFolder(writer, childItem); // Recursive call
                                        }
                                    }

                                    writer.WriteEndElement(); // End Item or Folder
                                }

                                // Process each top-level item or folder
                                ItemCollection items = editor.Value.StandardEditorData.EditorLeftDockPanel.TreeView.Items;
                                foreach (TreeViewItem treeItem in items)
                                {
                                    SaveItemOrFolder(writer, treeItem);
                                }
                                writer.WriteEndElement(); //End ItemList



                                

                                writer.WriteStartElement("CategoryList");
                                foreach (var row in editor.Value.StandardEditorData.CategoryList)
                                {
                                    writer.WriteStartElement("Category");
                                    writer.WriteElementString("Name", row.CategoryName);
                                    writer.WriteElementString("Key", row.Key);


                                    foreach (var column in row.ColumnList)
                                    {
                                        writer.WriteStartElement("Column");                                        
                                        writer.WriteElementString("Name", column.ColumnName);
                                        writer.WriteElementString("Key", column.Key);


                                        foreach (var entry in column.EntryList)
                                        {
                                            writer.WriteStartElement("Entry");                                            
                                            writer.WriteElementString("Name", entry.Name);
                                            writer.WriteElementString("Notepad", entry.Notepad);
                                            writer.WriteElementString("IsNameHidden", entry.IsNameHidden.ToString());
                                            writer.WriteElementString("IsEntryHidden", entry.IsEntryHidden.ToString());  
                                            writer.WriteElementString("RowOffset", entry.RowOffset.ToString());
                                            writer.WriteElementString("Bytes", entry.Bytes.ToString());
                                            //writer.WriteElementString("Endianness", entry.Endianness.ToString());

                                            if (entry.Endianness == "1" || entry.Endianness == "2L" || entry.Endianness == "4L") { writer.WriteElementString("Endianness", "Little"); }
                                            else { writer.WriteElementString("Endianness", "Big"); }

                                            //writer.WriteElementString("TypeX", entry.SubType);


                                            if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
                                            {
                                                writer.WriteStartElement("NumberBox");
                                                writer.WriteElementString("Sign", entry.EntryTypeNumberBox.NewNumberSign.ToString());
                                                writer.WriteEndElement(); //End NumberBox 
                                            }


                                            if (entry.NewSubType == Entry.EntrySubTypes.CheckBox)
                                            {
                                                writer.WriteStartElement("CheckBox");
                                                //writer.WriteElementString("TrueText", entry.EntryTypeCheckBox.TrueText.ToString());
                                                //writer.WriteElementString("FalseText", entry.EntryTypeCheckBox.FalseText.ToString());
                                                writer.WriteElementString("TrueValue", entry.EntryTypeCheckBox.TrueValue.ToString());
                                                writer.WriteElementString("FalseValue", entry.EntryTypeCheckBox.FalseValue.ToString());
                                                writer.WriteEndElement(); //End CheckBox 
                                            }


                                            if (entry.NewSubType == Entry.EntrySubTypes.BitFlag)
                                            {
                                                writer.WriteStartElement("BitFlag");
                                                writer.WriteElementString("Flag1Name", entry.EntryTypeBitFlag.BitFlag1Name.ToString());
                                                //writer.WriteElementString("Flag1CheckText", entry.EntryTypeBitFlag.BitFlag1CheckText.ToString());
                                                //writer.WriteElementString("Flag1UncheckText", entry.EntryTypeBitFlag.BitFlag1UncheckText.ToString());
                                                writer.WriteElementString("Flag2Name", entry.EntryTypeBitFlag.BitFlag2Name.ToString());
                                                //writer.WriteElementString("Flag2CheckText", entry.EntryTypeBitFlag.BitFlag2CheckText.ToString());
                                                //writer.WriteElementString("Flag2UncheckText", entry.EntryTypeBitFlag.BitFlag2UncheckText.ToString());
                                                writer.WriteElementString("Flag3Name", entry.EntryTypeBitFlag.BitFlag3Name.ToString());
                                                //writer.WriteElementString("Flag3CheckText", entry.EntryTypeBitFlag.BitFlag3CheckText.ToString());
                                                //writer.WriteElementString("Flag3UncheckText", entry.EntryTypeBitFlag.BitFlag3UncheckText.ToString());
                                                writer.WriteElementString("Flag4Name", entry.EntryTypeBitFlag.BitFlag4Name.ToString());
                                                //writer.WriteElementString("Flag4CheckText", entry.EntryTypeBitFlag.BitFlag4CheckText.ToString());
                                                //writer.WriteElementString("Flag4UncheckText", entry.EntryTypeBitFlag.BitFlag4UncheckText.ToString());
                                                writer.WriteElementString("Flag5Name", entry.EntryTypeBitFlag.BitFlag5Name.ToString());
                                                //writer.WriteElementString("Flag5CheckText", entry.EntryTypeBitFlag.BitFlag5CheckText.ToString());
                                                //writer.WriteElementString("Flag5UncheckText", entry.EntryTypeBitFlag.BitFlag5UncheckText.ToString());
                                                writer.WriteElementString("Flag6Name", entry.EntryTypeBitFlag.BitFlag6Name.ToString());
                                                //writer.WriteElementString("Flag6CheckText", entry.EntryTypeBitFlag.BitFlag6CheckText.ToString());
                                                //writer.WriteElementString("Flag6UncheckText", entry.EntryTypeBitFlag.BitFlag6UncheckText.ToString());
                                                writer.WriteElementString("Flag7Name", entry.EntryTypeBitFlag.BitFlag7Name.ToString());
                                                //writer.WriteElementString("Flag7CheckText", entry.EntryTypeBitFlag.BitFlag7CheckText.ToString());
                                                //writer.WriteElementString("Flag7UncheckText", entry.EntryTypeBitFlag.BitFlag7UncheckText.ToString());
                                                writer.WriteElementString("Flag8Name", entry.EntryTypeBitFlag.BitFlag8Name.ToString());
                                                //writer.WriteElementString("Flag8CheckText", entry.EntryTypeBitFlag.BitFlag8CheckText.ToString());
                                                //writer.WriteElementString("Flag8UncheckText", entry.EntryTypeBitFlag.BitFlag8UncheckText.ToString());
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

                                                                                             

                                                if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile) 
                                                {
                                                    writer.WriteStartElement("FileData");
                                                    writer.WriteElementString("FirstNameID", entry.EntryTypeMenu.FirstNameID.ToString());
                                                    writer.WriteElementString("Location", entry.EntryTypeMenu.GameFile.FileLocation);
                                                    writer.WriteElementString("Start", entry.EntryTypeMenu.Start.ToString());
                                                    writer.WriteElementString("RowSize", entry.EntryTypeMenu.RowSize.ToString());
                                                    writer.WriteElementString("CharacterSet", entry.EntryTypeMenu.CharacterSet.ToString());
                                                    writer.WriteElementString("TextSize", entry.EntryTypeMenu.CharCount.ToString());
                                                    writer.WriteElementString("NameCount", entry.EntryTypeMenu.NameCount.ToString());
                                                    writer.WriteEndElement(); //End FileData
                                                }
                                                if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                                                {
                                                    writer.WriteStartElement("TextData");
                                                    writer.WriteElementString("FirstNameID", entry.EntryTypeMenu.FirstNameID.ToString());
                                                    writer.WriteElementString("Location", entry.EntryTypeMenu.GameFile.FileLocation);
                                                    writer.WriteElementString("Start", entry.EntryTypeMenu.Start.ToString());
                                                    writer.WriteElementString("NameCount", entry.EntryTypeMenu.NameCount.ToString());
                                                    writer.WriteEndElement(); //End FileData
                                                }
                                                if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
                                                {
                                                    writer.WriteStartElement("EditorData");
                                                    writer.WriteElementString("FirstNameID", entry.EntryTypeMenu.FirstNameID.ToString());
                                                    if (entry.EntryTypeMenu.LinkedEditor == null)
                                                    {
                                                        writer.WriteElementString("EditorName", entry.EntryTypeMenu.OldLinkedEditorName); //Yes we're saving the OLD Editor. This is NOT an axident!
                                                        writer.WriteElementString("EditorKey", entry.EntryTypeMenu.OldLinkedEditorKey);
                                                    }
                                                    else 
                                                    {
                                                        writer.WriteElementString("EditorName", entry.EntryTypeMenu.LinkedEditor.EditorName); //Only used to notify if a link to editor is missing. (The editor is missing)
                                                        writer.WriteElementString("EditorKey", entry.EntryTypeMenu.LinkedEditor.EditorKey);
                                                    }                                                    
                                                    //writer.WriteElementString("Start", entry.EntryTypeMenu.Start.ToString());
                                                    writer.WriteElementString("NameCount", entry.EntryTypeMenu.NameCount.ToString());
                                                    writer.WriteEndElement(); //End FileData
                                                }
                                                if (entry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
                                                {
                                                    writer.WriteStartElement("NameList");
                                                    for (int i = 0; i < entry.EntryTypeMenu.NothingNameList.Length; i++)
                                                    {
                                                        if (!string.IsNullOrEmpty(entry.EntryTypeMenu.NothingNameList[i]))
                                                        {
                                                            string listItem = $"{i}: {entry.EntryTypeMenu.NothingNameList[i]}";
                                                            writer.WriteElementString("Item", listItem);
                                                        }
                                                    }
                                                    writer.WriteEndElement(); //End NameList
                                                }
                                                
                                                writer.WriteEndElement(); //End Menu    

                                            }

                                            writer.WriteElementString("TableKey", editor.Value.StandardEditorData.TableKey);
                                            writer.WriteElementString("Start", editor.Value.StandardEditorData.DataTableStart.ToString());
                                            writer.WriteElementString("RowSize", editor.Value.StandardEditorData.DataTableRowSize.ToString());
                                            writer.WriteElementString("EntryKey", entry.Key);
                                            //writer.WriteElementString("RowSize", entry.DataTableRowSize.ToString());

                                            writer.WriteEndElement(); //End Entry
                                        }
                                        writer.WriteEndElement(); //End Column
                                    }
                                    writer.WriteEndElement(); //End Category
                                }
                                writer.WriteEndElement(); //End CategoryList

                                editor.Value.StandardEditorData.EditorLeftDockPanel.SearchBar.Text = CurrentSearchBarText;

                            } //End IF STANDARD WIDTH TABLE EDITOR

                            if (editor.Value.EditorType == "TextEditor")
                            {
                                foreach (GameFile GameFile in editor.Value.TextEditorData.ListOfGameFiles) 
                                {
                                    writer.WriteStartElement("EditorFile"); //Info about the file used for the main data of the editor.
                                    writer.WriteElementString("FileLocation", GameFile.FileLocation);
                                    writer.WriteEndElement(); // End EditorFile
                                }
                               
                            }

                            writer.WriteEndElement(); //End Editor  AKA the Root of the XML   
                            writer.Flush(); //Ends the XML GameFile

                        } //End of using XmlWriter

                        using (XmlWriter FileWriter = XmlWriter.Create(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\Files.xml", settings))
                        {                            

                            List<GameFile> SomeGameFiles = new();

                            

                            FileWriter.WriteStartElement("Files"); //
                            FileWriter.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                            FileWriter.WriteElementString("VersionDate", LibraryMan.VersionDate);


                            if (editor.Value.StandardEditorData.FileNameTable != null) //if name table file exists! For editors using a custom name list, it won't exist.
                            {
                                GameFile AFile = editor.Value.StandardEditorData.FileNameTable;
                                if (!SomeGameFiles.Contains(AFile)) 
                                {
                                    SomeGameFiles.Add(AFile);
                                }
                                
                                
                            }
                            if (editor.Value.EditorType == "DataTable") 
                            {
                                GameFile AFile = editor.Value.StandardEditorData.FileDataTable;
                                if (!SomeGameFiles.Contains(AFile))
                                {
                                    SomeGameFiles.Add(AFile);
                                }                            
                            }                            
                            foreach (DescriptionTable ExtraTable in editor.Value.StandardEditorData.DescriptionTableList)
                            {
                                GameFile AFile = ExtraTable.FileTextTable;
                                if (!SomeGameFiles.Contains(AFile))
                                {
                                    SomeGameFiles.Add(AFile);
                                }
                                

                            }

                            if (editor.Value.EditorType == "TextEditor") //Start of Text Editor Files
                            {
                                foreach (GameFile GameFile in editor.Value.TextEditorData.ListOfGameFiles) 
                                {
                                    if (!SomeGameFiles.Contains(GameFile))
                                    {
                                        SomeGameFiles.Add(GameFile);
                                    }
                                }
                                
                            }

                            foreach (Editor TheEditor in Database.GameEditors.Values) 
                            {
                                foreach (Category TheCat in TheEditor.StandardEditorData.CategoryList) 
                                {
                                    foreach (Column TheColumn in TheCat.ColumnList) 
                                    {
                                        foreach (Entry theEntry in TheColumn.EntryList) 
                                        {
                                            if (theEntry.NewSubType == Entry.EntrySubTypes.Menu) 
                                            {
                                                if (theEntry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile || theEntry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile) 
                                                {
                                                    if (theEntry.EntryTypeMenu.GameFile != null) 
                                                    {
                                                        GameFile AFile = theEntry.EntryTypeMenu.GameFile;
                                                        if (!SomeGameFiles.Contains(AFile))
                                                        {
                                                            SomeGameFiles.Add(AFile);
                                                        }
                                                    }
                                                }

                                                //if (theEntry.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
                                                //{
                                                    
                                                //}



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
                                FileWriter.WriteElementString("Notepad", AGameFile.FileNotepad);
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

                LibraryMan.NotificationNegative("Error: Editors not saved.",
                    "An error occured during the \"Saving Editors\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
                    "\n\n" +
                    "As you were probably saving more then just your editors, you'll be happy to hear that each part of saving is handled seperately. This means there is NO CHANCE that any other parts of the saving operation are affected, such as when also saving documents or game giles. " +
                    "\n\n" +
                    "Anyway as for editors, To help users know which documents are which on their computer, we save editors using the names you give them to actual folders. " +
                    "Each operating system has a diffrent list of symbols it doesn't allow folder names to use. " +
                    "To deal with this problem the program first runs a simulation of what would happen IF it actually saved anything, " +
                    "by creating a temporary dummy folder and saving everything to that temporary folder. " +
                    "This way there is no chance your actual editors folder will get corrupted or result in any other serious error. :)" +
                    "\n\n" +
                    "As your seeing this error, it means your operating system doesn't like atleast one of the symbols you tried using in a editor's name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n\n" +
                    "Try changing the names of any editors you think might have caused the error, and then try saving your editors again. "
                    );

                
            }


            
        }


        public static void SaveGameData(MethodData ActionPack)//Database Database, Workshop TheWorkshop
        {
            //I need to make it so for the entrys of the currently selected item, if a entry is edited, that it is saved.
            //Currently the user can just swap items back and forth to save to MemoryFile, and then here MemoryFile saves to your PC.

            if (ActionPack.Command.WorkshopData == null)  //think about this more before adding it
            {
                return;
            }

            string SavePath = "";

            if (ActionPack.Command.WorkshopData.Workshop.ProjectDataItem.ProjectOutputDirectory != "")
            {
                //Make sure this actually exists!
                SavePath = ActionPack.Command.WorkshopData.Workshop.ProjectDataItem.ProjectOutputDirectory;
            }
            else if (ActionPack.Command.WorkshopData.Workshop.ProjectDataItem.ProjectInputDirectory != "")
            {
                SavePath = ActionPack.Command.WorkshopData.Workshop.ProjectDataItem.ProjectInputDirectory;
            }
            else 
            {
                return;
            }

            //add a check for if SavePath location exists

            foreach (KeyValuePair<string, GameFile> HFile in ActionPack.Command.WorkshopData.GameFiles)
            {
                File.WriteAllBytes(SavePath + "\\" + HFile.Value.FileLocation, HFile.Value.FileBytes); //saves to the path i set, everything in the array.
            }


        }

        public static void SaveDocumentsWorkshop(MethodData ActionPack) 
        {
            if (ActionPack.mainMenu.WorkshopData == null)  
            {
                return;
            }

            ActionPack.mainMenu.WorkshopData.Workshop.TheDocumentsUserControl.SaveAllDocumentsWorkshop();
        }

        public static void SaveDocumentsProject(MethodData ActionPack)
        {
            if (ActionPack.mainMenu.WorkshopData == null)
            {
                return;
            }
            ActionPack.mainMenu.WorkshopData.Workshop.TheDocumentsUserControl.SaveAllDocumentsProject();
        }


        public static void SaveEvents(MethodData MethodData) 
        {
            string WorkshopName = MethodData.mainMenu.WorkshopName;
            
            //Step 1: Make sure everything can save to a Example location, letting us test if a problem (crash) would occur, before actually saving to the right location.
            string ExtraPath = "\\Lab"; //This extra string causes stuff to be saved to a path variant of the normal location, 
            Directory.CreateDirectory(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\");
            Directory.CreateDirectory(LibraryMan.ApplicationLocation + "\\Workshops\\" + WorkshopName + "\\Events\\");

            //Step 1.5: Save to the example location.
            SaveEventsToXML(ExtraPath);

            //Step 2: Delete everything in the example location.
            Directory.Delete(LibraryMan.ApplicationLocation + ExtraPath + "\\", true);

            bool Failed = false;
            if (Failed == true) { return; }

            //Step 3: Delete everything in the REAL location.
            ExtraPath = "";
            string FolderPath = LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events";
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


            void SaveEventsToXML(string ExtraPath)
            {
                try
                {
                    string LoadOrderFile = LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\" + "LoadOrder.txt"; //causes weird errors if outside this
                    string LoadOrderContent = "";

                    foreach (Event Event in MethodData.mainMenu.Events)
                    {
                        Directory.CreateDirectory(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\" + Event.DisplayName + "\\");

                        LoadOrderContent += Event.DisplayName + Environment.NewLine; //for load order text

                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.IndentChars = ("    ");
                        settings.CloseOutput = true;
                        settings.OmitXmlDeclaration = true;
                        using (XmlWriter writer = XmlWriter.Create(LibraryMan.ApplicationLocation + ExtraPath + "\\Workshops\\" + WorkshopName + "\\Events\\" + Event.DisplayName + "\\Event.xml", settings))
                        {
                            writer.WriteStartElement("Event");
                            writer.WriteElementString("VersionNumber", LibraryMan.VersionNumber.ToString());
                            writer.WriteElementString("VersionDate", LibraryMan.VersionDate);
                            writer.WriteElementString("Note", "The resource's name is unused and for debugging. The current name could be diffrent.");
                            writer.WriteElementString("Note2", "The Resource's Key is the WorkshopResourceKey. Not the ProjectResourceKey.");
                            
                            writer.WriteElementString("Seperator", "================================================================================");
                            writer.WriteElementString("Name", Event.DisplayName);
                            writer.WriteElementString("Note", Event.Note);
                            writer.WriteElementString("Notepad", Event.Notepad);

                            writer.WriteStartElement("CommandList");
                            foreach (EventCommand AnEventCommand in Event.CommandList)
                            {
                                writer.WriteStartElement("Command");

                                writer.WriteElementString("Name", AnEventCommand.Command.DisplayName);
                                writer.WriteElementString("Key", AnEventCommand.Command.Key);                                
                                writer.WriteStartElement("ResourceList");                                
                                foreach (string thekey in AnEventCommand.ResourceKeys.Values)
                                {
                                    if (thekey == "") { continue; }
                                    writer.WriteStartElement("Resource");
                                    writer.WriteElementString("Name", MethodData.mainMenu.EventResources.Find(thing => thing.WorkshopResourceKey == thekey).Name );
                                    writer.WriteElementString("Key", thekey); 
                                    writer.WriteEndElement(); //End Command
                                }
                                writer.WriteEndElement(); //End Resources
                                writer.WriteEndElement(); //End Command
                            }
                            writer.WriteEndElement(); //End Commands

                            writer.WriteEndElement(); //End Event
                            writer.Flush(); //Ends the XML GameFile
                        }
                    }
                    File.WriteAllText(LoadOrderFile, LoadOrderContent);
                }
                catch
                {
                    LibraryMan.NotificationNegative("Error: Events failed to save properly.",
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

        
    }
}
