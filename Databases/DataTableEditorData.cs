using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GameEditorStudio
{
    public class DataTableEditorData : Editor //In some places i call this SWData
    {
        //Note: The Editor abstract class also has access to the WorkshopXaml and WorkshopData classes.
        public DataTableEditor DTEXaml { get; set; } //The XAML of this editor. 
        /////////////////////////////////////////////////////////////////////////////////////////
        public TextTable ?NameTable { get; set; } // Name Table Data 
        public DataTable ?DataTable { get; set; } // Data Table Data 
        public List<TextTable>? DescriptionTableList { get; set; } = new(); // Description Table Data //Why the fuck is this a list? Oh incase of more then 1 description... (Skill description seperate from flavour text?)
        //WARNING TO SELF:
        //Description tables live encode / decode to file bytes instead of doing it on save, so they will always be accurate, 
        // even if multiple editors want the same information to appear and be editable. :)
        //This was a great idea before, but now they are text tables.
        //So its possible some conflict may happen? Maybe....
        // idk (Maybe not? the same data would be edited either way...ehhh????)

        /////////////////////////////////////////////////////////////////////////////////////////



        public LeftBar? EditorLeftBar { get; set; } = new(); //The left bar of an editor, the item tree, search bar, etc.
        public DTRightBar EditorRightBar { get; set; }
        public DockPanel? EditorDescriptionsPanel { get; set; } = new();
        public List<Category>? CategoryList { get; set; } = new();


        // The following things are only used to keep track stuff that i'd rather not be here but 
        // because C# blocks ref variables inside click delegate events i need somewhere to store them. :(
        // They are not saved to XML or reloaded   
        public Category ?CategoryClass { get; set; }
        public Group ?GroupClass { get; set; }
        public Entry ?EntryClass { get; set; } //The entry the user is currently selecting. In DEV mode, This entry is highlighted.

        public List<Entry>? MasterEntryList { get; set; } = new(); //A master list to make it easy to make changes to all of them at once.
        public List<Entry> MergedEntryList { get; set; } = new(); //A "merged entry" is when a previous entry has a byte size of 2 or more, thus "merging" with this entry. This should not appear even if you use the hidden toggle. (But Debug Mode view can force them to visible).
        public List<Entry> HiddenEntryList { get; set; } = new(); //A list of entries that are hidden. Used to make it easy to unhide all hidden entries at once.
        //IMPORTANT NOTE TO SELF ABOUT HIDDEN ENTRYS: I never want hidden entrys to be "removed" from the editor grid. This is because of the possibility
        //of hidden entrys also being merged entrys. Merged Entrys get removed from the grid, hidden ones do not. That is my final decision. For now anyway. 

        public List<Entry> NameEntryList { get; set; } = new(); //A list of entries that are actually name / description entries. These entries do not save to the memory file, and are basically disabled.
        public List<Entry> DescriptionEntryList { get; set; } = new(); //A list of entries that are actually name / description entries. These entries do not save to the memory file, and are basically disabled.
        public int TableRowIndex { get; set; } //Not XML, used to save data when changing items in a collection. It's equal to an ItemInfo's Index.        
        public DockPanel MainDockPanel { get; set; } //The right side's panel inside a scroll viewer.
    }

    public class Category //Variables in here intensionally do not set =new(); because the editor middle rebuilds when changing tables.
    {   
        public string CategoryName { get; set; } = "New Category"; //XML the name of a row.
        public string Tooltip { get; set; } = "";
        public Border CatBorder { get; set; }
        public DockPanel CategoryBody { get; set; } //The Inner Back (Inside the border).
        public Grid TooltipGrid { get; set; }
        public Border CategoryUnderline { get; set; } //for tooltips
        public Label CategoryLabel { get; set; }
        public DataTableEditorData DTEData { get; set; }


        public string Key { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //This is only incase it's needed for future features, but is currently unused.  (Also i cant even think of any scenario where this would be required)

        public List<GridItem> GridItems { get; set; } = new();
        public Grid ItemGrid { get; set; } = new();

    }



    public abstract class GridItem 
    {
        public Editor ParentEditor { get; set; }
        public Category ParentCategory { get; set; }
        public Group ParentGroup { get; set; }
        public Grid ParentGrid { get; set; }
        public List<GridItem> ParentGridItems { get; set; }

        public int Column { get; set; }      // 0-based
        public int Row { get; set; }         // 0-based
        public int ColumnSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;

        public UIElement Visual { get; set; }
    } //A group or an entry. Groups can hold entrys, and also look cool and are a way to sort stuff. 
        

    public class Group : GridItem //Variables in here intensionally do not set =new(); because the editor middle rebuilds when changing tables.
    {
        public string GroupName { get; set; } = "New Group"; //XML The name of a column.
        public string GroupTooltip { get; set; } = "";
        public Border GroupBorder { get; set; }
        public DockPanel GroupPanel { get; set; }
        public Border GroupUnderline { get; set; } //for tooltips
        public Grid TooltipGrid { get; set; }  //for tooltips
        public Label GroupLabel { get; set; }       
        public string Key { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //Unused, Only exists incase of future features.

        public Grid ItemGrid { get; set; } //A group is something that gets it's own grid of items.
        public List<GridItem> GridItems { get; set; } = new();
        
    }   
    

    public class Entry : GridItem //Variables in here intensionally do not set =new(); because the editor middle rebuilds when changing tables.
    {
        public string Name { get; set; } = "";  //XML //The Name / Label an entry Gets. Later, it will default to "???"  
        public string WorkshopTooltip { get; set; } = ""; //XML
        public bool IsNameHidden { get; set; } = false;  //XML //Yes or No, Defaults to Yes
        public bool IsEntryHidden { get; set; } = false;  //XML  -Enabled or Disabled (AutoDisabled?)//Decides If entry can save to Memory File. Occurs when the byte is also in use by the NameTable or any ExtraTables.  
        public bool IsTextInUse { get; set; } = false; //Not XML //if true, this entry actually represents name or description text, and is basically disabled / hidden. 
        public bool IsMerged { get; set; } = false; //Saved to XML.
        public EntrySubTypes NewSubType { get; set; } = EntrySubTypes.NumberBox;
        public enum EntrySubTypes { NumberBox, CheckBox, BitFlag, Menu}
        //public int ByteStarting { get; set; } //This number is how many bytes from the start of a file the editor begins. Each entry keeps track of it's own, because some editors have more then 1 file.

        //Byte stuff / DataTable info 

        
        public string DataTableKey { get; set; } = ""; //XML //This entry's table's key. UNUSED / exists incase it's later needed, (Guests, Variable Width support, etc).
        //Note: Table Start Byte is saved to an Entrys XML, but is pulled directly from the table (for now). It's saved, much like above, in prep for guests / variable width support. It's not used (or even loaded back from XML) yet.
        //If i ever add guest entrys, fix this!
        
        public int DataTableRowSize { get; set; }  //XML RowSize? //Literally the same as in DataTableEditorData. Altho it's unnecessary, I'm keeping it here as i might need it for future features to be backwards compatable.
        //At some point, i should recode all references to RowSize to be "MyTable's RowSize". (but still keep saving rowsize to XML incase it's later used for Guests / VW support)
        //Also, Maybe even replace TableKey/StartByte/RowSize here with a straight up reference TO the fucking table class itself. Would make getting stuff MUCH easier. 
        public int RowOffset { get; set; }  //XML //This number is how many bytes into a row this entry edits. This *SHOULD BE* unique for every entry.
        public int Bytes { get; set; } = 1; //XML the size as a number. Due to the number of references it was annoying not to make it a dedicated piece of info. I could technically remove it later.
        public string Endianness { get; set; } = "1"; //XML - 1, 2L, 2B, 4L, 4B //Become a enum.


        public string Key { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //This is only incase it's needed for future features, but is currently unused.  (Also i cant even think of any scenario where this would be required)


        public string EntryByteDecimal { get; set; } //NOT XML  //Needed to deal with the true value of a checkbox or bitflag.  //THIS SHOULD ALWAYS READ AS A POSITIVE NUMBER, IF ITS NEGATIVE IT'S A BUG AND I NEED TO FIX IT! 

        public Border EntryBorder { get; set; }//The border around a entry,
        public DockPanel? EntryDockPanel { get; set; } //The entrys Grid, visable to the used, and contains lots of information.
        public Label Symbology { get; set; }
        public Label EntryPrefix { get; set; }   //Used to show the byte offset to a user. Useful when creating an editor, and you don't know what things do yet.
        //public Label? EntryLabel { get; set; } //The name of an entry, appears on it's grid.
        
        /////////////////////The label
        public Grid EntryLeftGrid { get; set; }
        public Border UnderlineBorder { get; set; }
        public TextBlock EntryNameTextBlock { get; set; } //The textblock that shows the entry's name.
        public Run RunEntryName { get; set; } = new(); //The run that shows the entry's name. This is used to change the color of the text, and to make it bold.
        


        public EntryTypeNumberBox EntryTypeNumberBox { get; set; } = new();
        public EntryTypeCheckBox EntryTypeCheckBox { get; set; } = new();
        public EntryTypeBitFlag EntryTypeBitFlag { get; set; } = new();
        public EntryTypeMenu EntryTypeMenu { get; set; } = new(); //A List is empty by default, this is not a mistake.


        public MenuItem EntryCreateNewGroup { get; set; } = new();
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
        public bool NumberBoxCanSave { get; set; } = true; //NOT XML //Makes it so number boxes do not save to MemoryFile when changing the selected Item in the current Collection.

        public enum TheNumberSigns {Unsigned, Signed } //Unsigned is positive, Signed is positive and negative.
        public string Suffix { get; set; } = ""; //XML - A suffix that appears after the number in a number box. For example, "HP" or "%". Can be left blank for no suffix.
    }

    public class EntryTypeCheckBox
    {

        public Button CheckBoxButton { get; set; }
        public int? TrueValue { get; set; } = 1; //XML - Value a checkbox uses for checked. 
        public int? FalseValue { get; set; } = 0; //XML - Value a checkbox uses for unchecked. 
        public string TrueText { get; set; } = "✔"; //✔✓ //XML  The text that appears when the checkbox is true (IE checked)
        public string FalseText { get; set; } = ""; //XML The text that appears when the checkbox is false (IE not checked)


    }


    public class EntryTypeMenu
    {
        public Button ListButton { get; set; }
        public ComboBox Dropdown { get; set; }
        public MenuTypes MenuType { get; set; } = MenuTypes.Dropdown;
        public enum MenuTypes { Dropdown, List } //Possible Menu Types
        public LinkTypes LinkType { get; set; } = LinkTypes.Editor;
        public enum LinkTypes { DataFile, DataFileAdvanced, TextFile, Editor, Nothing } //Nothing means user uses custom name list.
        

        public TextTable ?TextTableDataFile { get; set; } //DataFile Link        
        public TextTable ?TextTableTextFile { get; set; } //TextFile Link
        public TextTable ?TextTableEditor { get; set; } //Editor Link
        public TextTable ?TextTableNothing { get; set; } //Nothing Link.
        public TextTable ?TextTableAdvanced { get; set; } //Advanced Link
        


    }
















    public class LeftBar
    {
        public DataTableEditorData DataTableEditorData { get; set; }
        public TheLeftBar LeftBarXaml { get; set; }
        public TreeView TreeView { get; set; } //The tree view itself of the editor. As a reminder every editor has its own tree view.
        
        public TextBox ItemNameTextBox { get; set; } //The textbox for editing the item's name.
        public TextBox ItemNoteTextbox { get; set; } //The textbox for creating a note for that item.

        public TextBox SearchBar { get; set; }

        public TextBox ItemNotepadTextbox { get; set; }//The actual item note

    }

    public class TextInfo
    {
        public DataTableEditorData DataTableEditorData { get; set; }
        public TreeViewItem TreeItem { get; set; }

        public string ItemName { get; set; } = ""; //XML
        public int ItemIndex { get; set; } = 0; //XML Basically, the row number in a table. Note: Folders have a index of 0.   I should maybe update this so folders have an index of -1. //Basically a Key
        public string ItemKey { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //This is only incase it's needed for future features, but is currently unused. 
        public string? ItemWorkshopTooltip { get; set; } = ""; //XML the orange one
        public string ItemNote { get; set; } = ""; //XML The actual note?
        

        public string ItemOrigonalName { get; set; } = "大きな火の玉";

        public int RowStart { get; set; } // The FIRST TEXT BYTE of the text.
        public int RowEnd { get; set; } // The FINAL WRITEABLE TEXT BYTE of the text. So not the 00 byte, the one before it. 

        //If Name Table
        public bool IsChild { get; set; } = false;
        public bool IsFolder { get; set; } = false; //NOT XML  //new folder is in workshop.cs
        public List<TextInfo> MyChildren { get; set; }
    }


    public class TextTable 
    {
        public GameFile TextTableFile { get; set; } //XML? The game file with the ItemNames. (Optional as users can use a Nothing Name List). 
            public enum TextTableLinkTypes { DataFile, Advanced, TextFile, Editor, Nothing } //Definitions of every type of name table. 
        public TextTableLinkTypes TextTableLinkType { get; set; } = TextTableLinkTypes.DataFile; //The current Name Table Type. Defaults to DataFile for some reason. 
        public string TextTableCharacterSet { get; set; } = ""; //XML The name of the Cypher used to Decrypt from Hex to English. (A=1, B=2, etc). Google what a Cypher is for more information.
        public int TextTableStart { get; set; } = 0;//XML
        public int TextTableCharLimit { get; set; } = 0;//XML
        public int TextTableRowSize { get; set; } = 0;//XML

        public string TextTableKey { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //Unused. This is only here incase it's needed in the future. (I cant think of any scenario where this would be required)

        public int TextTableFirstNameID { get; set; } = 1; //XML - The display number the name list starts at. Usually 0 or 1. (But not always).

        ////////Name Table Only////////
        //Also used in menu nothing and menu other thing? //Do not use with MenuNothing or MenuEditor.
        public int TextTableItemCount { get; set; }//XML  How many items are in the collection. It determines how many rows of data the editor reads. For manual name lists, the number is always 0.
                                                   //The name table item count only happens for editors getting names from file. Any function relying on this will break when using a manual name list.
        
        public List<TextInfo> ItemList { get; set; } = new(); //XML - Info on each line of text. 

        //public int NameTableLastTextLine { get; set; }
        ////////Description Table Only////////

        public TextBox DescriptionTableTextBox { get; set; }
        public bool DescriptionTableEncodeIsEnabled { get; set; }


        ////////Link to Editor Only////////
        public DataTableEditorData ?LinkedDTEEditor { get; set; } //When a text table is pulling names from another editor, this is that linked editor. 

        public string PreviousLinkedEditorName { get; set; } = ""; //XML - The linked editor's name. Used only to preverve the info between loading and saving again when the linked editor is missing. 
        public string PreviousLinkedEditorKey { get; set; } = ""; //XML - The linked editor's key. Used only to preverve the info between loading and saving again when the linked editor is missing. 

        public string GameFileLocation { get; set; } = "";
    }

    public class DataTable 
    {
        public GameFile FileDataTable { get; set; } //Just a shortcut to the file, to cleanup code, and possibly processing time? IDK im to dumb to get what causes lag.
        public string DataTableKey { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //XML but Unused. Exists for possible future feature where an editor can use more then 1 data table.

        public enum DataTableLinkTypes { DataFile, Advanced, } //Definitions of every type of name table. 
        public DataTableLinkTypes LinkType { get; set; } = DataTableLinkTypes.DataFile; //The current Name Table Type. Defaults to DataFile for some reason. 

        //Standard DataTable Info
        public int DataTableStart { get; set; } //XML Says what byte a table of information starts at. 
        public int DataTableRowSize { get; set; } //XML How many bytes are in 1 row of the table. Also used (with Name Item Count) to determine the size of the table.


        public string GameFileLocation { get; set; } = "";
    }





}
