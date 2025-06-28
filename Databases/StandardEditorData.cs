using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GameEditorStudio
{
    public class StandardEditorData //In some places i call this SWData
    {
        public Editor TheEditor { get; set; }

        public string TableKey { get; set; } = "";
        public GameFile FileDataTable { get; set; } //Just a shortcut to the file, to cleanup code, and possibly processing time? IDK im to dumb to get what causes lag.
        public GameFile FileNameTable { get; set; } //XML? The file that contains the ItemNames for this editors Collection Tree.

        ////////////////////////////// NAME TABLE READING DATA ///////////////////////////
        ////Name File chunk. The "Name File" has the list of names that tells the user what their editing. This is optional, as users can instead input a manual name list.
        public NameTableLinkTypes NameTableLinkType {  get; set; } = NameTableLinkTypes.DataFile;
        public string NameTableCharacterSet { get; set; } //XML The name of the Cypher used to Decrypt from Hex to English. (A=1, B=2, etc). Google what a Cypher is for more information.
        public int NameTableStart { get; set; } //XML
        public int NameTableTextSize { get; set; } //XML
        public int NameTableRowSize { get; set; } //XML
        public int NameTableItemCount { get; set; } //XML  How many items are in the collection. It determines how many rows of data the editor reads. For manual name lists, the number is always 0.
                                                    //The name table item count only happens for editors getting names from file. Any function relying on this will break when using a manual name list.

        public string NameTableKey { get; set; } = LibraryMan.GenerateKey(); //This is only incase it's needed for future features, but is currently unused.  (Also i cant even think of any scenario where this would be required)

        ///////////////////////////////// DATA TABLE READING DATA ///////////////////////////////////        
        public int DataTableStart { get; set; } //XML Says what byte a table of information starts at. 
        public int DataTableRowSize { get; set; } //XML How many bytes are in 1 row of the table. Also used (with Name Item Count) to determine the size of the table.

        ////////////////////////////////// DESCRIPTION TABLE DATA ///////////////////////////////////

        public List<DescriptionTable>? DescriptionTableList { get; set; } = new();

        /////////////////////////////////////////////////////////////////////////////////////////
        public enum NameTableLinkTypes {DataFile,TextFile,Editor,Nothing } //hex,txt
        

        //The rest of the class

        public LeftBar? EditorLeftDockPanel { get; set; } = new(); //The left bar of an editor, the item tree, search bar, etc.
        public TopBar? EditorDescriptionsPanel { get; set; }
        public List<Category>? CategoryList { get; set; } = new();


        // The following things are only used to keep track stuff that i'd rather not be here but 
        // because C# blocks ref variables inside click delegate events i need somewhere to store them. :(
        // They are not saved to XML or reloaded 

        public Entry SelectedEntry { get; set; } //The entry the user is currently selecting. In DEV mode, This entry is highlighted.
        public List<Entry>? MasterEntryList { get; set; } = new(); //A master list to make it easy to make changes to all of them at once.
        public int TableRowIndex { get; set; } //Not XML, used to save data when changing items in a collection. It's equal to an ItemInfo's Index.        
        public DockPanel MainDockPanel { get; set; } //The right side's panel inside a scroll viewer.
    }

    public class Category //NOTE: This used to be called a row, and comments all over the program may still use this name.
    {
        public string CategoryName { get; set; } = "New Category"; //XML the name of a row.
        public Border CatBorder { get; set; } = new();
        public DockPanel CategoryDockPanel { get; set; } = new();
        public List<Column>? ColumnList { get; set; } = new();
        public Label CategoryLabel { get; set; }
        public StandardEditorData SWData { get; set; }

        public string Key { get; set; } = LibraryMan.GenerateKey(); //This is only incase it's needed for future features, but is currently unused.  (Also i cant even think of any scenario where this would be required)
    }


    public class DescriptionTable //Extra tables live encode / decode to file bytes instead of doing it on save, so they will always be accurate,
    { // even if multiple editors want the same information to appear and be editable. :)
        public GameFile FileTextTable { get; set; }
        public string CharacterSet { get; set; } = "Shift-JIS";
        public int Start { get; set; } = 0;
        public int TextSize { get; set; } = 0;
        public int RowSize { get; set; } = 0;
        public TextBox ExtraTableTextBox { get; set; }
        public bool ExtraTableEncodeIsEnabled { get; set; }
        public LinkTypes LinkType { get; set; } = LinkTypes.DataFile;
        public enum LinkTypes { DataFile, TextFile, Editor, Nothing } //Nothing means user uses custom name list.

        public string Key { get; set; } = LibraryMan.GenerateKey(); //This is only incase it's needed for future features, but is currently unused.  (Also i cant even think of any scenario where this would be required)
    }

    public class LeftBar
    {
        public UserControl UserControl { get; set; }
        public DockPanel? LeftBarDockPanel { get; set; }
        public TreeView TreeView { get; set; } //The tree view itself of the editor. As a reminder every editor has its own tree view.
        public List<ItemInfo> ItemList { get; set; } = new(); //i kinda forget.
        public TextBox ItemNameTextBox { get; set; } //The textbox for editing the item's name.
        public TextBox ItemNoteTextbox { get; set; } //The textbox for creating a note for that item.

        public TextBox SearchBar { get; set; }

        public TextBox ItemNotepadTextbox { get; set; }//The actual item note

    }

    public class ItemInfo
    {
        public TreeViewItem TreeItem { get; set; }

        public string ItemName { get; set; } = ""; //XML
        public int ItemIndex { get; set; } = 0; //XML Basically, the row number in a table. Note: Folders have a index of 0.   I should maybe update this so folders have an index of -1. //Basically a Key
        public string ItemKey { get; set; } = LibraryMan.GenerateKey(); //This is only incase it's needed for future features, but is currently unused. 
        public string? ItemWorkshopTooltip { get; set; } = ""; //XML the orange one
        public string ItemNote { get; set; } = ""; //XML The actual note?
        public bool IsChild { get; set; } = false;
        public bool IsFolder { get; set; } = false; //NOT XML  //new folder is in workshop.cs

        public string ItemOrigonalName { get; set; } = "大きな火の玉";

    }

    public class TopBar //WHAT IS THIS? I FORGET D:
    {
        public DockPanel TopPanel { get; set; }
        public DockPanel PageBar { get; set; }

        public TextBox EntryNoteBox { get; set; }

    }

    

    
    public class Column
    {
        public string ColumnName { get; set; } = "New Column"; //XML The name of a column.
        public DockPanel? ColumnPanel { get; set; }
        public List<Entry>? EntryList { get; set; } = new();

        public List<CItem>? MasterList { get; set; } = new();
        public Label ColumnLabel { get; set; }
        public Category ColumnRow { get; set; }

        //public List<T> Stuff { get; set; } = new(); //This is a list of stuff that the column holds. It can be used to hold any type of object, but is currently only used to hold Entry objects.

        public string Key { get; set; } = LibraryMan.GenerateKey(); //Unused, Only exists incase of future features. 
    }

    public abstract class CItem { }

    public class Group : CItem //A way to group entries together, like a folder.
    {
        public string GroupName { get; set; } = "New Group"; //XML The name of a column.
        public string GroupTooltip { get; set; } = "";
        public Border GroupBorder { get; set; } = new();
        public DockPanel GroupPanel { get; set; } = new();
        public List<Entry> EntryList { get; set; } = new();
        public Label GroupLabel { get; set; } = new();
        public Column GroupColumn { get; set; }
        public string Key { get; set; } = LibraryMan.GenerateKey(); //Unused, Only exists incase of future features. 
    }
    

    public class Entry : CItem
    {
        public string Name { get; set; } = "";  //XML //The Name / Label an entry Gets. Later, it will default to "???"  
        public string WorkshopTooltip { get; set; } = ""; //XML
        public bool IsNameHidden { get; set; } = false;  //XML //Yes or No, Defaults to Yes
        public bool IsEntryHidden { get; set; } = false;  //XML  -Enabled or Disabled (AutoDisabled?)//Decides If entry can save to Memory File. Occurs when the byte is also in use by the NameTable or any ExtraTables.  
        public bool IsTextInUse { get; set; } = false; //Not XML //if true, this entry actually represents name or description text, and is basically disabled / hidden. 
        public EntrySubTypes NewSubType { get; set; } = EntrySubTypes.NumberBox;
        public enum EntrySubTypes { NumberBox, CheckBox, BitFlag, Menu}
        //public int ByteStarting { get; set; } //This number is how many bytes from the start of a file the editor begins. Each entry keeps track of it's own, because some editors have more then 1 file.

        //Byte stuff / DataTable info 

        //NOTE: The Table
        public string TableKey { get; set; } = ""; //XML //This entry's table's key. UNUSED / exists incase it's later needed, (Guests, Variable Width support, etc).
        //Note: Table Start Byte is saved to an Entrys XML, but is pulled directly from the table (for now). It's saved, much like above, in prep for guests / variable width support. It's not used (or even loaded back from XML) yet.
        //If i ever add guest entrys, fix this!
        //Note
        public int DataTableRowSize { get; set; }  //XML RowSize? //Literally the same as in DataTableEditorData. Altho it's unnecessary, I'm keeping it here as i might need it for future features to be backwards compatable.
        //At some point, i should recode all references to RowSize to be "MyTable's RowSize". (but still keep saving rowsize to XML incase it's later used for Guests / VW support)
        //Also, Maybe even replace TableKey/StartByte/RowSize here with a straight up reference TO the fucking table class itself. Would make getting stuff MUCH easier. 
        public int RowOffset { get; set; }  //XML //This number is how many bytes into a row this entry edits. This *SHOULD BE* unique for every entry.
        public int Bytes { get; set; } = 1; //XML the size as a number. Due to the number of references it was annoying not to make it a dedicated piece of info. I could technically remove it later.
        public string Endianness { get; set; } = "1"; //XML - 1, 2L, 2B, 4L, 4B //Become a enum.


        public string Key { get; set; } = LibraryMan.GenerateKey(); //This is only incase it's needed for future features, but is currently unused.  (Also i cant even think of any scenario where this would be required)


        public string EntryByteDecimal { get; set; } //NOT XML  //Needed to deal with the true value of a checkbox or bitflag.  //THIS SHOULD ALWAYS READ AS A POSITIVE NUMBER, IF ITS NEGATIVE IT'S A BUG AND I NEED TO FIX IT! 

        public Border EntryBorder { get; set; } //The border around a entry,
        public DockPanel? EntryDockPanel { get; set; } //The entrys Grid, visable to the used, and contains lots of information.
        public Label Symbology { get; set; }
        public Label EntryPrefix { get; set; }   //Used to show the byte offset to a user. Useful when creating an editor, and you don't know what things do yet.
        //public Label? EntryLabel { get; set; } //The name of an entry, appears on it's grid.
        
        /////////////////////The label
        public Grid EntryLeftGrid { get; set; } = new ();
        public Border UnderlineBorder { get; set; } = new();
        public TextBlock EntryNameTextBlock { get; set; } = new(); //The textblock that shows the entry's name.
        public Run RunEntryName { get; set; } = new(); //The run that shows the entry's name. This is used to change the color of the text, and to make it bold.
        


        public Editor EntryEditor { get; set; }
        public Category EntryRow { get; set; }
        public Column EntryColumn { get; set; }
        public Group EntryGroup { get; set; }

        //The grid/Dockpanel? The labels and buttons to later delete?

        public EntryTypeNumberBox EntryTypeNumberBox { get; set; } = new();
        public EntryTypeCheckBox EntryTypeCheckBox { get; set; } = new();
        public EntryTypeBitFlag EntryTypeBitFlag { get; set; } = new();
        public EntryTypeMenu EntryTypeMenu { get; set; } = new(); //A List is empty by default, this is not a mistake.







    }



    public class EntryTypeBitFlag
    {
        public DockPanel BitFlagsDockPanel { get; set; } // Used to hold the various Bigflags inside it.

        public string BitFlag1Name { get; set; } = "Flag 1"; //XML
        public DockPanel BitFlag1 { get; set; }
        public Label BitFlag1Label { get; set; }        
        public Button BitFlag1CheckBox { get; set; }
        public string BitFlag2Name { get; set; } = "Flag 2"; //XML
        public DockPanel BitFlag2 { get; set; }
        public Label BitFlag2Label { get; set; }        
        public Button BitFlag2CheckBox { get; set; }
        public string BitFlag3Name { get; set; } = "Flag 3"; //XML
        public DockPanel BitFlag3 { get; set; }
        public Label BitFlag3Label { get; set; }        
        public Button BitFlag3CheckBox { get; set; }
        public string BitFlag4Name { get; set; } = "Flag 4"; //XML
        public DockPanel BitFlag4 { get; set; }
        public Label BitFlag4Label { get; set; }        
        public Button BitFlag4CheckBox { get; set; }
        public string BitFlag5Name { get; set; } = "Flag 5"; //XML
        public DockPanel BitFlag5 { get; set; }
        public Label BitFlag5Label { get; set; }        
        public Button BitFlag5CheckBox { get; set; }
        public string BitFlag6Name { get; set; } = "Flag 6"; //XML
        public DockPanel BitFlag6 { get; set; }
        public Label BitFlag6Label { get; set; }        
        public Button BitFlag6CheckBox { get; set; }
        public string BitFlag7Name { get; set; } = "Flag 7"; //XML
        public DockPanel BitFlag7 { get; set; }
        public Label BitFlag7Label { get; set; }        
        public Button BitFlag7CheckBox { get; set; }
        public string BitFlag8Name { get; set; } = "Flag 8"; //XML
        public DockPanel BitFlag8 { get; set; }
        public Label BitFlag8Label { get; set; }        
        public Button BitFlag8CheckBox { get; set; }


        public string BitFlag1CheckText { get; set; } = "✔"; //XML
        public string BitFlag1UncheckText { get; set; } = " "; //XML
        public string BitFlag2CheckText { get; set; } = "✔"; //XML
        public string BitFlag2UncheckText { get; set; } = " "; //XML
        public string BitFlag3CheckText { get; set; } = "✔"; //XML
        public string BitFlag3UncheckText { get; set; } = " "; //XML
        public string BitFlag4CheckText { get; set; } = "✔"; //XML
        public string BitFlag4UncheckText { get; set; } = " "; //XML
        public string BitFlag5CheckText { get; set; } = "✔"; //XML
        public string BitFlag5UncheckText { get; set; } = " "; //XML
        public string BitFlag6CheckText { get; set; } = "✔"; //XML
        public string BitFlag6UncheckText { get; set; } = " "; //XML
        public string BitFlag7CheckText { get; set; } = "✔"; //XML
        public string BitFlag7UncheckText { get; set; } = " "; //XML
        public string BitFlag8CheckText { get; set; } = "✔"; //XML
        public string BitFlag8UncheckText { get; set; } = " "; //XML
    }



    public class EntryTypeNumberBox
    {
        public bool TextChangeEventWorks { get; set; } = true;
        public TextBox? NumberBoxTextBox { get; set; }
        public TheNumberSigns NewNumberSign {  get; set; } = TheNumberSigns.Unsigned; //XML - This determines if a numbersbox only accepts positive numbers, or both Positive and Negative.
        public bool NumberBoxCanSave { get; set; } //NOT XML //Makes it so number boxes do not save to MemoryFile when changing the selected Item in the current Collection.

        public enum TheNumberSigns {Unsigned, Signed } //Unsigned is positive, Signed is positive and negative.
    }

    public class EntryTypeCheckBox
    {

        public Button CheckBoxButton { get; set; }
        public int? TrueValue { get; set; } = 1; //XML - Value a checkbox uses for checked. 
        public int? FalseValue { get; set; } = 0; //XML - Value a checkbox uses for unchecked. 
        public string TrueText { get; set; } = "✔"; //XML  The text that appears when the checkbox is true (IE checked)
        public string FalseText { get; set; } = ""; //XML The text that appears when the checkbox is false (IE not checked)


    }


    public class EntryTypeMenu
    {
        public Button ListButton { get; set; }
        public ComboBox Dropdown { get; set; }
        public MenuTypes MenuType { get; set; } = MenuTypes.Dropdown;        
        public LinkTypes LinkType { get; set; } = LinkTypes.Editor;

        //Editor Name Link
        //public string EditorKey { get; set; }
        public Editor LinkedEditor { get; set; } = null;
        public string OldLinkedEditorName { get; set; } = ""; //XML - The name of the editor that was linked to this menu. Used only to preverve the info between loading and saving again when the linked editor is missing. 
        public string OldLinkedEditorKey { get; set; } = ""; //XML - The key of the editor that was linked to this menu. Used only to preverve the info between loading and saving again when the linked editor is missing. 

        //Custom Name list
        public int ListSize { get; set; } //XML If the user creates a custom name list, this is the size, i think...? //Can't this be implicit from bytes?
        public string[] NothingNameList { get; set; } //XML If the user creates a custom name list, these are the names.

        //File Name List
        public List<string> NameList { get; set; } = new();
        public GameFile GameFile { get; set; } //for menu names.
        public int Start { get; set; } = 0; //for reading from, a start byte, like usual.
        public int RowSize { get; set; } = 0;
        public int CharCount { get; set; } = 0;
        public int FirstNameID { get; set; } = 0;//What the displayed ID# is of the first line. Used when the displayed value is not the same as the text line number. 
        public int NameCount { get; set; } = 0;
        public string CharacterSet { get; set; }
        //Editor Name List





        //Enum definitions
        public enum MenuTypes { Dropdown, List }
        public enum LinkTypes { DataFile, TextFile, Editor, Nothing  } //Nothing means user uses custom name list.

    }












}
