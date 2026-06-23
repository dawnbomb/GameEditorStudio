using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static GameEditorStudio.Entry;
using static GameEditorStudio.EntryTypeMenu;
using TabItem = System.Windows.Controls.TabItem;
//using System.Windows.Forms;
//using System.Drawing;

namespace GameEditorStudio
{
    public class EntryManager
    {
        //This is a very complicated file to understand, feel free to ask for help.
        //In here, we take hex from a file, and load it into a entry as decimal, based on what type of entry it is (Number, checkbox / flag, bitflag, list)
        //I plan to add dropdown menus.


        //This comment is for myself mostly.

        //B! .TryParse(Name! .Text, out type! value! ); { Form1.ByteWriter( value! , enemydata_array, StartingHex! + (treeView1.SelectedNode.Index * RowSize! ) + ArrayOfset! ); }
        //B=Byte size, 1="Byte" 2="UInt16" 4="UInt32"         Name!=the name of the textbox        type!= byte / ushort / uint, only include this in the FIRST time this ever happens in a form, later copies ommit this!
        //value!=Name of variable that holds the byte type (so byte/short/int has diffrent names)       StartingHex!= The hex offset      RowSize!=How many bytes in a row    Arrayofset! = how far into the row do we start grabbing info or saving it
        //Byte.TryParse(textBoxLv.Text, out byte value8); { Form1.ByteWriter(value8, enemydata_array, 104 + (enemyTree.SelectedNode.Index * 96) + 96); } //First 1 byte save

        public void LoadEntry(DataTableEditorData DTEData, Entry EntryClass) //LOADING ISN'T TAKING SIGN INTO CONSIDERATION!!!
        {
            //When the left bar selects a new item, this method triggers to load each entry's data based on Item Index.

            //This method simply takes the byte(s) from MemoryFile the entry controls,
            //converts them from Hex to Decimal, and loads that number into EditorClass.EntryByteDecimal.
            //Afterward (Down below) it loads the the entry with that information.

            if (DTEData.WorkshopXaml.IsPreviewMode == true) { return; }

            ////ANSWER A
            //if (EntryClass.Endianness == "1")
            //{
            //    EntryClass.EntryByteDecimal = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
            //}
            //else if (EntryClass.Endianness == "2B")
            //{
            //    EntryClass.EntryByteDecimal = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
            //}
            //else if (EntryClass.Endianness == "4B")
            //{
            //    EntryClass.EntryByteDecimal = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
            //}
            //else if (EntryClass.Endianness == "2L")
            //{
            //    ushort value2 = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
            //    ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
            //    EntryClass.EntryByteDecimal = swappedValue2.ToString("D");
            //}
            //else if (EntryClass.Endianness == "4L")
            //{
            //    uint value = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
            //    byte[] valueBytes = BitConverter.GetBytes(value);
            //    Array.Reverse(valueBytes);
            //    uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
            //    EntryClass.EntryByteDecimal = swappedValue.ToString("D");
            //}

            //ANSWER B
            if (EntryClass.Endianness == "1")
            {
                EntryClass.EntryByteDecimal = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
            }
            else if (EntryClass.Endianness == "2B")
            {
                ushort value2 = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
                EntryClass.EntryByteDecimal = swappedValue2.ToString("D");
            }
            else if (EntryClass.Endianness == "4B")
            {
                uint value = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Reverse(valueBytes);
                uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
                EntryClass.EntryByteDecimal = swappedValue.ToString("D");
            }
            else if (EntryClass.Endianness == "2L")
            {
                EntryClass.EntryByteDecimal = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                
            }
            else if (EntryClass.Endianness == "4L")
            {
                EntryClass.EntryByteDecimal = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                
            }


            if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox) 
            { 
                LoadNumberBox(EntryClass); //Load Entry
            }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox)
            { 
                LoadCheckBox(EntryClass); //Load Entry
            }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag)
            {
                LoadBitFlag(EntryClass); //Load Entry
            }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu) 
            { 
                LoadMenu(EntryClass); //Load Entry
            }

            //WARNING: I tried making symbology update whenever an entry is loaded, but it causes a LOT OF FUCKING LAG when swapping the selected item. DO NOT DO THIS! FIND A BETTER ANSWER! 
            
        }

        public void SaveEntry(Entry EntryClass)
        {
            DataTableEditorData DTEData = EntryClass.ParentEditor.DataTableEditorData;

            if (EntryClass.Endianness != "0")
            {
                //This Method takes the number in EntryClass.EntryByteDecimal, converts it from Decimal to Hex, then saves it to the correct file location.
                //This is the only way anything is saved to MemoryFile / eventually actual file.
                //This is triggered whenever any entry's module is changed in any way.


                //if (EntryClass.EntryType == "NumberBox") { SaveNumberBox(EntryClass); }
                //if (EntryClass.EntryType == "CheckBox")  { SaveCheckBox(EntryClass); }
                //If (EntryClass.EntryType == "BitFlag")  { SaveBitFlag(EntryClass); }
                //If Dropdown
                //if (EntryClass.EntryType == "List") { SaveList(EntryClass); }


                //Checks if the entry allows saving.
                //Creators may want to disable specific entrys from being editable if changing them causes the game to crash.
                //There could be other uses as well. All entrys default to true.
                //if (EntryClass.IsEntryHidden == false && EntryClass.IsTextInUse == false)
                //{
                //    //If 1/2/4/r2/r4 Bytes...  

                //    //NOTE: I now ALWAYS save as an unsigned value. IE, EntryByteDecimal may NEVER be a negative (Not anymore). 
                //    if (EntryClass.Endianness == "1")  //This is saving 1 Byte Size?   //First 1 byte save
                //    {
                //        //Thing that loads       -----------------The Hex GameFile---------------------------  ---Starting Byte--- --The Tree--   --Row Size----  --Offset into a row-- --To Decimal--               
                //        Byte.TryParse(EntryClass.EntryByteDecimal, out byte value8);
                //        { ByteManager.ByteWriter(value8, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); }

                //    }
                //    else if (EntryClass.Endianness == "2L")
                //    {
                //        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                //        value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                //        { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save


                //    }
                //    else if (EntryClass.Endianness == "4L")
                //    {
                //        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                //        byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                //        Array.Reverse(valueBytes); // Swap the endianness
                //        value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                //        { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save

                //    }
                //    else if (EntryClass.Endianness == "2B")
                //    {
                //        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                //        { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save

                //    }
                //    else if (EntryClass.Endianness == "4B")
                //    {
                //        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                //        { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save

                //    }
                //}


                if (EntryClass.IsEntryHidden == false && EntryClass.IsTextInUse == false)
                {
                    //If 1/2/4/r2/r4 Bytes...  

                    //NOTE: I now ALWAYS save as an unsigned value. IE, EntryByteDecimal may NEVER be a negative (Not anymore). 
                    if (EntryClass.Endianness == "1")  //This is saving 1 Byte Size?   //First 1 byte save
                    {
                        //Thing that loads       -----------------The Hex GameFile---------------------------  ---Starting Byte--- --The Tree--   --Row Size----  --Offset into a row-- --To Decimal--               
                        Byte.TryParse(EntryClass.EntryByteDecimal, out byte value8);
                        { ByteManager.ByteWriter(value8, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); }

                    }
                    else if (EntryClass.Endianness == "2L")
                    {
                        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                        { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save

                    }
                    else if (EntryClass.Endianness == "4L")
                    {
                        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                        { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save
                    }
                    else if (EntryClass.Endianness == "2B")
                    {
                        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                        value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                        { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save

                    }
                    else if (EntryClass.Endianness == "4B")
                    {
                        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                        byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                        Array.Reverse(valueBytes); // Swap the endianness
                        value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                        { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save

                    }
                }





            }//End IF ByteSize !=0
        }


        public void LoadNumberBox(Entry EntryClass)
        {
            EntryClass.EntryTypeNumberBox.TextChangeEventWorks = false; //This prevent the textbox's change text event from firing. 

            if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned)
            {
                EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = EntryClass.EntryByteDecimal;
            }
            if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
            {
                long longValue;
                if (long.TryParse(EntryClass.EntryByteDecimal, out longValue))
                {
                    if (EntryClass.Bytes == 1)
                    {
                        if (longValue > 127)
                        {
                            EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = (longValue - 256).ToString();
                        }
                        else { EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = longValue.ToString(); }

                    }
                    if (EntryClass.Bytes == 2)
                    {
                        if (longValue > 32767)
                        {
                            EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = (longValue - 65536).ToString();
                        }
                        else { EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = longValue.ToString(); }
                    }
                    if (EntryClass.Bytes == 4)
                    {
                        if (longValue > 2147483647)
                        {
                            EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = (longValue - 4294967296).ToString();
                        }
                        else { EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = longValue.ToString(); }
                    }
                }
            }

            EntryClass.EntryTypeNumberBox.TextChangeEventWorks = true;
        }


        public void LoadCheckBox(Entry EntryClass)
        {
            if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.TrueValue.ToString())
            {
                EntryClass.EntryTypeCheckBox.CheckBoxButton.Content = EntryClass.EntryTypeCheckBox.TrueText; // = UserCheck
            }
            else if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.FalseValue.ToString())
            {
                EntryClass.EntryTypeCheckBox.CheckBoxButton.Content = EntryClass.EntryTypeCheckBox.FalseText; //= UserUncheck
            }
            else
            {
                EntryClass.EntryTypeCheckBox.CheckBoxButton.Content = "??? " + EntryClass.EntryByteDecimal + " ";
            }
        }

        public void LoadBitFlag(Entry EntryClass)
        {
            int Num = Int32.Parse(EntryClass.EntryByteDecimal);

            if (Num > 127) { EntryClass.EntryTypeBitFlag.BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8CheckText; Num = Num - 128; } else { EntryClass.EntryTypeBitFlag.BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8UncheckText; } //Flag 0/128
            if (Num > 63) { EntryClass.EntryTypeBitFlag.BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7CheckText; Num = Num - 64; } else { EntryClass.EntryTypeBitFlag.BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7UncheckText; }  //Flag 0/64
            if (Num > 31) { EntryClass.EntryTypeBitFlag.BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6CheckText; Num = Num - 32; } else { EntryClass.EntryTypeBitFlag.BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6UncheckText; }  //Flag 0/32
            if (Num > 15) { EntryClass.EntryTypeBitFlag.BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5CheckText; Num = Num - 16; } else { EntryClass.EntryTypeBitFlag.BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5UncheckText; }  //Flag 0/16
            if (Num > 7) { EntryClass.EntryTypeBitFlag.BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4CheckText; Num = Num - 8; } else { EntryClass.EntryTypeBitFlag.BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4UncheckText; }  //Flag 0/8
            if (Num > 3) { EntryClass.EntryTypeBitFlag.BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3CheckText; Num = Num - 4; } else { EntryClass.EntryTypeBitFlag.BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3UncheckText; }  //Flag 0/4
            if (Num > 1) { EntryClass.EntryTypeBitFlag.BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2CheckText; Num = Num - 2; } else { EntryClass.EntryTypeBitFlag.BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2UncheckText; }  //Flag 0/2
            if (Num > 0) { EntryClass.EntryTypeBitFlag.BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1CheckText; Num = Num - 1; } else { EntryClass.EntryTypeBitFlag.BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1UncheckText; }  //Flag 0/1

            EntryClass.EntryTypeBitFlag.BitFlag1Label.Content = EntryClass.EntryTypeBitFlag.BitFlag1Name;
            EntryClass.EntryTypeBitFlag.BitFlag2Label.Content = EntryClass.EntryTypeBitFlag.BitFlag2Name;
            EntryClass.EntryTypeBitFlag.BitFlag3Label.Content = EntryClass.EntryTypeBitFlag.BitFlag3Name;
            EntryClass.EntryTypeBitFlag.BitFlag4Label.Content = EntryClass.EntryTypeBitFlag.BitFlag4Name;
            EntryClass.EntryTypeBitFlag.BitFlag5Label.Content = EntryClass.EntryTypeBitFlag.BitFlag5Name;
            EntryClass.EntryTypeBitFlag.BitFlag6Label.Content = EntryClass.EntryTypeBitFlag.BitFlag6Name;
            EntryClass.EntryTypeBitFlag.BitFlag7Label.Content = EntryClass.EntryTypeBitFlag.BitFlag7Name;
            EntryClass.EntryTypeBitFlag.BitFlag8Label.Content = EntryClass.EntryTypeBitFlag.BitFlag8Name;
        }


        public void LoadMenu(Entry EntryClass)
        {            

            string MenuText = EntryClass.EntryByteDecimal + ": " + "???"; //text for a menu name the user hasn't assigned a name to, but is a valid value.
            bool found = false; //If not found, text becomes ERROR! and red.             

            if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.List)
            {
                EntryClass.EntryTypeMenu.ListButton.ToolTip = null; //Reset tooltip status.


                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile )
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableDataFile;
                    CharacterSetManager CSM = new();
                    CSM.DecodeAllItemTexts(texttable); //Temp fix for preview mode, but it could probably cause problems later? >_>
                    foreach (TextInfo textinfo in texttable.ItemList) 
                    {
                        if (textinfo.ItemIndex.ToString() == EntryClass.EntryByteDecimal) 
                        {
                            found = true;                            
                            MenuText = (int.Parse(EntryClass.EntryByteDecimal) + texttable.TextTableFirstNameID) + ": " + textinfo.ItemName;
                            EntryClass.EntryTypeMenu.ListButton.Tag = textinfo;
                            if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.ListButton.ToolTip = textinfo.ItemWorkshopTooltip; }                            
                            break;
                        }
                    }   
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableTextFile;
                    CharacterSetManager CSM = new();
                    CSM.DecodeAllItemTexts(texttable); //Temp fix for preview mode, but it could probably cause problems later? >_>
                    foreach (TextInfo textinfo in texttable.ItemList)
                    {
                        if (textinfo.ItemIndex.ToString() == EntryClass.EntryByteDecimal)
                        {
                            found = true;
                            MenuText = (int.Parse(EntryClass.EntryByteDecimal) + texttable.TextTableFirstNameID) + ": " + textinfo.ItemName;
                            EntryClass.EntryTypeMenu.ListButton.Tag = textinfo;
                            if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.ListButton.ToolTip = textinfo.ItemWorkshopTooltip; }                            
                            break;
                        }
                    }
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
                {    
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableEditor;
                    foreach (TextInfo textinfo in texttable.LinkedDTEEditor.NameTable.ItemList) //slightly diffrent, it goes over the linked Name Table's items. 
                    {
                        if (textinfo.ItemIndex.ToString() == EntryClass.EntryByteDecimal)
                        //if (textinfo.ItemIndex.ToString() == (int.Parse(EntryClass.EntryByteDecimal) + texttable.LinkedDTEEditor.NameTable.TextTableFirstNameID).ToString())
                        {
                            found = true;
                            MenuText = (int.Parse(EntryClass.EntryByteDecimal) + texttable.LinkedDTEEditor.NameTable.TextTableFirstNameID).ToString() + ": " + textinfo.ItemName;
                            //MenuText = EntryClass.EntryByteDecimal + ": " + textinfo.ItemName;
                            EntryClass.EntryTypeMenu.ListButton.Tag = textinfo;
                            if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.ListButton.ToolTip = textinfo.ItemWorkshopTooltip; }                            
                            break;
                        }
                    }
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing) 
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableNothing;
                    foreach (TextInfo textinfo in texttable.ItemList)
                    {
                        if (textinfo.ItemIndex.ToString() == EntryClass.EntryByteDecimal)
                        {
                            found = true;
                            MenuText = (int.Parse(EntryClass.EntryByteDecimal) + texttable.TextTableFirstNameID) + ": " + textinfo.ItemName;
                            EntryClass.EntryTypeMenu.ListButton.Tag = textinfo;
                            if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.ListButton.ToolTip = textinfo.ItemWorkshopTooltip; }                            
                            break;
                        }
                    }
                }


                
                if (found == true) 
                {
                    EntryClass.EntryTypeMenu.ListButton.Content = MenuText;
                    EntryClass.EntryTypeMenu.ListButton.Foreground = (Brush)(new BrushConverter().ConvertFrom("#E1E1E1"));
                    //EntryClass.EntryTypeMenu.ListButton.Background = (Brush)(new BrushConverter().ConvertFrom("#232323"));

                    EntryClass.EntryTypeMenu.ListButton.IsEnabled = true;
                }
                if (found == false) 
                {
                    EntryClass.EntryTypeMenu.ListButton.Content = MenuText;
                    //EntryClass.EntryTypeMenu.ListButton.Foreground = Brushes.Red; //Red text for unknown value
                    EntryClass.EntryTypeMenu.ListButton.Foreground = (Brush)(new BrushConverter().ConvertFrom("#888888"));
                    //EntryClass.EntryTypeMenu.ListButton.Foreground = (Brush)(new BrushConverter().ConvertFrom("#696969"));
                    //EntryClass.EntryTypeMenu.ListButton.Foreground = (Brush)(new BrushConverter().ConvertFrom("#EE1111"));
                    //EntryClass.EntryTypeMenu.ListButton.Background = (Brush)(new BrushConverter().ConvertFrom("#17181A"));
                    EntryClass.EntryTypeMenu.ListButton.IsEnabled = false;

                    EntryClass.EntryTypeMenu.ListButton.ToolTip = 
                        "This entry's value was not found in the linked text table." +
                        "\n(This is not a critical error.) " +
                        "\n\nTo help prevent mistakes, this is disabled when this happens. " +
                        "\n\nIf using a \"Nothing\" Table, you can add ? marks to the\nrelevant rows and it will be usable.";

                    //ComboBoxItem FakeItem = new();
                    //FakeItem.Content = MenuText; //found above
                    //FakeItem.Foreground = Brushes.Red; //Red text for unknown value
                    //EntryClass.EntryTypeMenu.Dropdown.Foreground = Brushes.Red; //Red text for unknown value
                    //EntryClass.EntryTypeMenu.Dropdown.Items.Add(FakeItem);

                    //TextInfo FakeInfo = new();
                    //FakeInfo.ItemIndex = int.Parse(EntryClass.EntryByteDecimal);
                    //FakeInfo.ItemName = "ERROR!";
                    //FakeItem.Tag = FakeInfo;

                    //FakeItem.IsSelected = true;
                }
                

            }








            if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.Dropdown)
            {
                EntryClass.EntryTypeMenu.Dropdown.ToolTip = null; //Reset tooltip status.
                int StartNum = 0;
                for (int i = EntryClass.EntryTypeMenu.Dropdown.Items.Count - 1; i >= 0; i--) //Remove all fake items.
                {
                    // Get the item at the current index
                    if (EntryClass.EntryTypeMenu.Dropdown.Items[i] is ComboBoxItem item)
                    {
                        // Check the tag
                        if (item.Tag?.ToString() == "Fake")
                        {
                            EntryClass.EntryTypeMenu.Dropdown.Items.RemoveAt(i);
                        }
                    }
                }                

                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile)
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableDataFile;
                    foreach (ComboBoxItem ComboItem in EntryClass.EntryTypeMenu.Dropdown.Items)
                    {
                        TextInfo textinfo = ComboItem.Tag as TextInfo;
                        if (textinfo.ItemIndex.ToString() != EntryClass.EntryByteDecimal) { continue; }

                        ComboItem.IsSelected = true;
                        found = true;
                        if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.Dropdown.ToolTip = textinfo.ItemWorkshopTooltip; }
                        
                        break;
                    }
                    StartNum = texttable.TextTableFirstNameID;

                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableTextFile;
                    foreach (ComboBoxItem ComboItem in EntryClass.EntryTypeMenu.Dropdown.Items)
                    {
                        TextInfo textinfo = ComboItem.Tag as TextInfo;
                        if (textinfo.ItemIndex.ToString() != EntryClass.EntryByteDecimal) { continue; }

                        ComboItem.IsSelected = true;
                        found = true;
                        if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.Dropdown.ToolTip = textinfo.ItemWorkshopTooltip; }
                        
                        break;
                    }
                    StartNum = texttable.TextTableFirstNameID;
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableEditor;
                    foreach (ComboBoxItem ComboItem in EntryClass.EntryTypeMenu.Dropdown.Items)
                    {
                        if (EntryClass.ParentEditor.WorkshopData.IsProjectLoaded == false) 
                        {
                            ComboItem.IsSelected = true;
                            found = true; 
                            //EntryClass.EntryTypeMenu.Dropdown.ToolTip = "You can see "; 
                            break; 
                        }

                        TextInfo textinfo = ComboItem.Tag as TextInfo;
                        if (textinfo.ItemIndex.ToString() != EntryClass.EntryByteDecimal) { continue; }

                        ComboItem.IsSelected = true;
                        found = true;
                        if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.Dropdown.ToolTip = textinfo.ItemWorkshopTooltip; }
                        
                        break;
                    }
                    try 
                    {
                        if (texttable != null) { if (texttable.LinkedDTEEditor != null) { StartNum = texttable.LinkedDTEEditor.NameTable.TextTableFirstNameID; }   }
                        
                    } catch { }
                    
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableNothing;
                    foreach (ComboBoxItem ComboItem in EntryClass.EntryTypeMenu.Dropdown.Items)
                    {
                        if (EntryClass.ParentEditor.WorkshopData.IsProjectLoaded == false)
                        {
                            ComboItem.IsSelected = true;
                            found = true;
                            //EntryClass.EntryTypeMenu.Dropdown.ToolTip = "You can see "; 
                            break;
                        }

                        TextInfo textinfo = ComboItem.Tag as TextInfo;
                        if (textinfo == null) { continue; }
                        if (textinfo.ItemIndex.ToString() != EntryClass.EntryByteDecimal) { continue; }

                        ComboItem.IsSelected = true;
                        found = true;
                        if (textinfo.ItemWorkshopTooltip != "") { EntryClass.EntryTypeMenu.Dropdown.ToolTip = textinfo.ItemWorkshopTooltip; }
                        
                        break;
                    }
                    StartNum = texttable.TextTableFirstNameID;
                }



                if (found == true) 
                {
                    EntryClass.EntryTypeMenu.Dropdown.IsEnabled = true;
                }
                if (found == false)
                {


                    ComboBoxItem FakeItem = new();
                    //FakeItem.Content = MenuText; //found above
                    FakeItem.Content = (int.Parse(EntryClass.EntryByteDecimal) + StartNum) + ": ???" ; //found above
                    FakeItem.Foreground = Brushes.Red; //Red text for unknown value
                    EntryClass.EntryTypeMenu.Dropdown.Foreground = Brushes.Red; //Red text for unknown value
                    EntryClass.EntryTypeMenu.Dropdown.Items.Add(FakeItem);

                    //NOTE: the last time i crashed when EntryByteDecimal was null,
                    //it was because the left bar for editors was not selecting an item until the editor tab was clicked.
                    //This is fixed now, but if this ever crashes again, it would be because EntryByteDecimal is not loaded to begin with. 
                    TextInfo FakeInfo = new();
                    FakeInfo.ItemIndex = int.Parse(EntryClass.EntryByteDecimal) + StartNum; 
                    FakeInfo.ItemName = "ERROR!";
                    //FakeItem.Tag = FakeInfo;
                    FakeItem.Tag = "Fake";

                    FakeItem.IsSelected = true;

                    EntryClass.EntryTypeMenu.Dropdown.IsEnabled = false;
                    EntryClass.EntryTypeMenu.Dropdown.ToolTip =
                        "This entry's value was not found in the linked text table." +
                        "\n(This is not a critical error.) " +
                        "\n\nTo help prevent mistakes, this is disabled when this happens. " +
                        "\n\nIf using a \"Nothing\" Table, you can add ? marks to the\nrelevant rows and it will be usable.";
                    //FakeItem.ToolTip =
                    //    "This entry's value was not found in the linked text table." +
                    //    "\n(This is not a critical error.) " +
                    //    "\n\nTo help prevent mistakes, this is disabled when this happens. " +
                    //    "\n\nIf using a \"Nothing\" Table, you can add ? marks to the\nrelevant rows and it will be usable.";

                }

            }

        }








        //////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////Create Entrys///////////////////////////////////////







        public void CreateNumberBox(Workshop TheWorkshop, Entry EntryClass)
        {
            //if (EntryClass.IsNameHidden == false) { EntryClass.EntryNameTextBlock.Visibility = Visibility.Visible; }
            // Default properties if new
            if (EntryClass.EntryTypeNumberBox == null)
            {
                EntryClass.EntryTypeNumberBox = new();
                EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Unsigned;
            }

            // Default properties end


            TextBox NumberBox = new TextBox();
            NumberBox.Tag = EntryClass;
            //NumberBox.Style = Application.Current.Resources["NumberBox"] as Style;
            NumberBox.Style = Application.Current.Resources["EntrySuffixTextBox"] as Style;
            NumberBox.Height = 28;
            NumberBox.MinWidth = 50;
            NumberBox.FontSize = 20;
            NumberBox.Margin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom
            NumberBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            EntryClass.EntryDockPanel.Children.Add(NumberBox);
            EntryClass.EntryTypeNumberBox.NumberBoxTextBox = NumberBox;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                NumberBox.IsEnabled = false;
            }


            //DockPanel UpDownDock = new DockPanel();
            //UpDownDock.Width = 13;
            //UpDownDock.Height = 28;
            ////EntryClass.EntryDockPanel.Children.Add(UpDownDock);
            //UpDownDock.Margin = new Thickness(0, 0, 3, 0);

            //Button upbtn = new Button();
            //upbtn.Style = Application.Current.Resources["ButtonEntryNumberTop"] as Style;
            ////upbtn.Width = 15;
            //upbtn.Height = 14;
            //upbtn.Content = "▲";
            //upbtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD"));
            //upbtn.FontSize = 10;
            //upbtn.HorizontalAlignment = HorizontalAlignment.Stretch;
            ////upbtn.VerticalAlignment = VerticalAlignment.Top;
            //upbtn.Margin = new Thickness(0, 0, 0, 0); // Left Top Right Bottom
            //UpDownDock.Children.Add(upbtn);
            //DockPanel.SetDock(upbtn, Dock.Top);

            //Button downbtn = new Button();
            //downbtn.Style = Application.Current.Resources["ButtonEntryNumberBottom"] as Style;
            ////updown.Style = Application.Current.Resources["ButtonEntryUpDown"] as Style;
            ////downbtn.Width = 15;
            //downbtn.Height = 14;
            //downbtn.Content = "v";
            //downbtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
            //downbtn.FontSize = 10;
            //downbtn.HorizontalAlignment = HorizontalAlignment.Stretch;
            ////downbtn.VerticalAlignment = VerticalAlignment.Top;
            //downbtn.Margin = new Thickness(0, 0, 0, 0); // Left Top Right Bottom
            //UpDownDock.Children.Add(downbtn);
            //DockPanel.SetDock(downbtn, Dock.Bottom);


            NumberBox.PreviewMouseDown += (sender, e) =>
            {
                DTEMethods.EntryActivate(EntryClass);
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
            };
            NumberBox.PreviewTextInput += (sender, e) =>
            {

                bool isDigit = char.IsDigit(e.Text, e.Text.Length - 1); // Check if its is a number or a minus sign (for signed numbers)
                bool isMinusSign = e.Text == "-" && EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed;

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
            };


            NumberBox.TextChanged += (sender, e) =>
            {
                if (EntryClass.EntryTypeNumberBox.TextChangeEventWorks == false)
                {
                    return;
                }


                if (EntryClass.EntryTypeNumberBox.NumberBoxCanSave == true)
                {
                    long longValue;
                    long UnsignedValue = 0;
                    if (long.TryParse(NumberBox.Text, out longValue))
                    {
                        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned)
                        {
                            if (EntryClass.Bytes == 1)
                            {
                                longValue = Math.Clamp(longValue, 0, 255);
                                UnsignedValue = longValue;
                            }
                            else if (EntryClass.Bytes == 2)
                            {
                                longValue = Math.Clamp(longValue, 0, 65535);
                                UnsignedValue = longValue;
                            }
                            else if (EntryClass.Bytes == 4)
                            {
                                longValue = Math.Clamp(longValue, 0, 4294967295);
                                UnsignedValue = longValue;
                            }
                        }
                        else if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                        {
                            if (EntryClass.Bytes == 1)
                            {
                                longValue = Math.Clamp(longValue, -128, 127);
                                UnsignedValue = (longValue >= 0) ? longValue : longValue + 256;
                            }
                            else if (EntryClass.Bytes == 2)
                            {
                                longValue = Math.Clamp(longValue, -32768, 32767);
                                UnsignedValue = (longValue >= 0) ? longValue : longValue + 65536;
                            }
                            else if (EntryClass.Bytes == 4)
                            {
                                longValue = Math.Clamp(longValue, -2147483648, 2147483647);
                                UnsignedValue = (longValue >= 0) ? longValue : longValue + 4294967296;
                            }

                        }

                        NumberBox.Text = longValue.ToString();
                        EntryClass.EntryByteDecimal = UnsignedValue.ToString();

                        if (EntryClass == EntryClass.ParentEditor.DataTableEditorData.EntryClass)
                        {
                            SaveEntry(EntryClass);
                            //UpdateEntryProperties(EntryClass);
                            DTEMethods.EntryActivate(EntryClass);
                        }
                    }
                }
            };

            NumberBox.TextInput += (sender, e) =>
            {
                // TheWorkshop.DebugBox.Text = "WTF";
            };

            if (TheWorkshop.IsPreviewMode == true) { NumberBox.IsEnabled = false; }
        }




        public void CreateCheckBox(Workshop TheWorkshop, Entry EntryClass)
        {
            //if (EntryClass.IsNameHidden == false) { EntryClass.EntryNameTextBlock.Visibility = Visibility.Visible; }

            //Default properties if new
            if (EntryClass.EntryTypeCheckBox == null)
            {
                EntryClass.EntryTypeCheckBox = new();
            }
            //Default properties end

            Button CheckBox = new Button();
            CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            CheckBox.MinWidth = 30;
            CheckBox.Height = 30;
            CheckBox.Margin = new Thickness(5, -1, 3, 0); // Left Top Right Bottom 
            CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            CheckBox.VerticalContentAlignment = VerticalAlignment.Top;

            {
                //Add to 1 before the end.
                int count = EntryClass.EntryDockPanel.Children.Count;
                int insertIndex = count > 0 ? count - 1 : 0;
                EntryClass.EntryDockPanel.Children.Insert(insertIndex, CheckBox);
                //EntryClass.EntryDockPanel.Children.Add(CheckBox);
            }

            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                CheckBox.IsEnabled = false;
            }
            CheckBox.Click += delegate
            {


                if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.TrueValue.ToString())
                {
                    CheckBox.Content = EntryClass.EntryTypeCheckBox.FalseText;
                    EntryClass.EntryByteDecimal = EntryClass.EntryTypeCheckBox.FalseValue.ToString();
                    SaveEntry(EntryClass);

                }
                else if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.FalseValue.ToString())
                {
                    CheckBox.Content = EntryClass.EntryTypeCheckBox.TrueText;
                    EntryClass.EntryByteDecimal = EntryClass.EntryTypeCheckBox.TrueValue.ToString();
                    SaveEntry(EntryClass);
                }

                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            EntryClass.EntryTypeCheckBox.CheckBoxButton = CheckBox;
            //EntryClass.EntryType
            //Add a new property option, to deicde if text used says On/Off or Yes/No or Custom?
            //User can also customize the color of the button (+ based on user color mode?).
            //A few options like this go a long way for user flexability!

            if (TheWorkshop.IsPreviewMode == true) { CheckBox.IsEnabled = false; }
        }

        public void CreateBitFlag(Workshop TheWorkshop, Entry EntryClass)
        {
            EntryClass.EntryNameTextBlock.Visibility = Visibility.Collapsed;

            if (EntryClass.EntryTypeBitFlag == null)
            {
                EntryClass.EntryTypeBitFlag = new();
            }


            DockPanel BitFlags = new();
            BitFlags.Background = Brushes.Transparent;
            //BitFlags.Background = Brushes.Crimson;
            int BitFlagBoxHeight = 31;
            int BitMinWidth = 33;
            var BitMargin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            var DockMargin = new Thickness(0, 3, 0, 3); // Left Top Right Bottom 

            ////////////////////////////////////////////////
            DockPanel BitFlag1 = new();
            BitFlag1.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag1, Dock.Top);
            BitFlag1.Margin = DockMargin;

            Label BitFlag1Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag1Name == null) { EntryClass.EntryTypeBitFlag.BitFlag1Name = "Bit 1"; }
            BitFlag1Label.Content = EntryClass.EntryTypeBitFlag.BitFlag1Name;
            BitFlag1Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag1CheckBox = new Button();
            BitFlag1CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag1CheckBox.MinWidth = BitMinWidth;
            BitFlag1CheckBox.Height = BitFlagBoxHeight;
            BitFlag1CheckBox.Margin = BitMargin;
            BitFlag1CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag1CheckBox.IsEnabled = false;
            }
            BitFlag1CheckBox.Click += delegate
            {


                if (BitFlag1CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag1CheckText.ToString())
                {
                    BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 1).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag1CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag1UncheckText.ToString())
                {
                    BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 1).ToString();
                    SaveEntry(EntryClass);
                }

                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag2 = new();
            BitFlag2.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag2, Dock.Top);
            BitFlag2.Margin = DockMargin;

            Label BitFlag2Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag2Name == null) { EntryClass.EntryTypeBitFlag.BitFlag2Name = "Bit 2"; }
            BitFlag2Label.Content = EntryClass.EntryTypeBitFlag.BitFlag2Name;
            BitFlag2Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag2CheckBox = new Button();
            BitFlag2CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag2CheckBox.MinWidth = BitMinWidth;
            BitFlag2CheckBox.Height = BitFlagBoxHeight;
            BitFlag2CheckBox.Margin = BitMargin;
            BitFlag2CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag2CheckBox.IsEnabled = false;
            }
            BitFlag2CheckBox.Click += delegate
            {


                if (BitFlag2CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag2CheckText.ToString())
                {
                    BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 2).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag2CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag2UncheckText.ToString())
                {
                    BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 2).ToString();
                    SaveEntry(EntryClass);
                }
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag3 = new();
            BitFlag3.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag3, Dock.Top);
            BitFlag3.Margin = DockMargin;

            Label BitFlag3Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag3Name == null) { EntryClass.EntryTypeBitFlag.BitFlag3Name = "Bit 3"; }
            BitFlag3Label.Content = EntryClass.EntryTypeBitFlag.BitFlag3Name;
            BitFlag3Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag3CheckBox = new Button();
            BitFlag3CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag3CheckBox.MinWidth = BitMinWidth;
            BitFlag3CheckBox.Height = BitFlagBoxHeight;
            BitFlag3CheckBox.Margin = BitMargin;
            BitFlag3CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag3CheckBox.IsEnabled = false;
            }
            BitFlag3CheckBox.Click += delegate
            {


                if (BitFlag3CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag3CheckText.ToString())
                {
                    BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 4).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag3CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag3UncheckText.ToString())
                {
                    BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 4).ToString();
                    SaveEntry(EntryClass);
                }
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag4 = new();
            BitFlag4.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag4, Dock.Top);
            BitFlag4.Margin = DockMargin;

            Label BitFlag4Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag4Name == null) { EntryClass.EntryTypeBitFlag.BitFlag4Name = "Bit 4"; }
            BitFlag4Label.Content = EntryClass.EntryTypeBitFlag.BitFlag4Name;
            BitFlag4Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag4CheckBox = new Button();
            BitFlag4CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag4CheckBox.MinWidth = BitMinWidth;
            BitFlag4CheckBox.Height = BitFlagBoxHeight;
            BitFlag4CheckBox.Margin = BitMargin;
            BitFlag4CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag4CheckBox.IsEnabled = false;
            }
            BitFlag4CheckBox.Click += delegate
            {


                if (BitFlag4CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag4CheckText.ToString())
                {
                    BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 8).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag4CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag4UncheckText.ToString())
                {
                    BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 8).ToString();
                    SaveEntry(EntryClass);
                }
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag5 = new();
            BitFlag5.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag5, Dock.Top);
            BitFlag5.Margin = DockMargin;

            Label BitFlag5Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag5Name == null) { EntryClass.EntryTypeBitFlag.BitFlag5Name = "Bit 5"; }
            BitFlag5Label.Content = EntryClass.EntryTypeBitFlag.BitFlag5Name;
            BitFlag5Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag5CheckBox = new Button();
            BitFlag5CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag5CheckBox.MinWidth = BitMinWidth;
            BitFlag5CheckBox.Height = BitFlagBoxHeight;
            BitFlag5CheckBox.Margin = BitMargin;
            BitFlag5CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag5CheckBox.IsEnabled = false;
            }
            BitFlag5CheckBox.Click += delegate
            {


                if (BitFlag5CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag5CheckText.ToString())
                {
                    BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 16).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag5CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag5UncheckText.ToString())
                {
                    BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 16).ToString();
                    SaveEntry(EntryClass);
                }
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag6 = new();
            BitFlag6.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag6, Dock.Top);
            BitFlag6.Margin = DockMargin;

            Label BitFlag6Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag6Name == null) { EntryClass.EntryTypeBitFlag.BitFlag6Name = "Bit 6"; }
            BitFlag6Label.Content = EntryClass.EntryTypeBitFlag.BitFlag6Name;
            BitFlag6Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag6CheckBox = new Button();
            BitFlag6CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag6CheckBox.MinWidth = BitMinWidth;
            BitFlag6CheckBox.Height = BitFlagBoxHeight;
            BitFlag6CheckBox.Margin = BitMargin;
            BitFlag6CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag6CheckBox.IsEnabled = false;
            }
            BitFlag6CheckBox.Click += delegate
            {


                if (BitFlag6CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag6CheckText.ToString())
                {
                    BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 32).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag6CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag6UncheckText.ToString())
                {
                    BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 32).ToString();
                    SaveEntry(EntryClass);
                }
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag7 = new();
            BitFlag7.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag7, Dock.Top);
            BitFlag7.Margin = DockMargin;

            Label BitFlag7Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag7Name == null) { EntryClass.EntryTypeBitFlag.BitFlag7Name = "Bit 7"; }
            BitFlag7Label.Content = EntryClass.EntryTypeBitFlag.BitFlag7Name;
            BitFlag7Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag7CheckBox = new Button();
            BitFlag7CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag7CheckBox.MinWidth = BitMinWidth;
            BitFlag7CheckBox.Height = BitFlagBoxHeight;
            BitFlag7CheckBox.Margin = BitMargin;
            BitFlag7CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag7CheckBox.IsEnabled = false;
            }
            BitFlag7CheckBox.Click += delegate
            {


                if (BitFlag7CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag7CheckText.ToString())
                {
                    BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 64).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag7CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag7UncheckText.ToString())
                {
                    BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 64).ToString();
                    SaveEntry(EntryClass);
                }
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag8 = new();
            BitFlag8.Background = Brushes.Transparent;
            DockPanel.SetDock(BitFlag8, Dock.Top);
            BitFlag8.Margin = DockMargin;

            Label BitFlag8Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag8Name == null) { EntryClass.EntryTypeBitFlag.BitFlag8Name = "Bit 8"; }
            BitFlag8Label.Content = EntryClass.EntryTypeBitFlag.BitFlag8Name;
            BitFlag8Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag8CheckBox = new Button();
            BitFlag8CheckBox.Style = Application.Current.Resources["ButtonEntryCheckbox"] as Style;
            BitFlag8CheckBox.MinWidth = BitMinWidth;
            BitFlag8CheckBox.Height = BitFlagBoxHeight;
            BitFlag8CheckBox.Margin = BitMargin;
            BitFlag8CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                BitFlag8CheckBox.IsEnabled = false;
            }
            BitFlag8CheckBox.Click += delegate
            {


                if (BitFlag8CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag8CheckText.ToString())
                {
                    BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 128).ToString();
                    SaveEntry(EntryClass);
                }
                else if (BitFlag8CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag8UncheckText.ToString())
                {
                    BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 128).ToString();
                    SaveEntry(EntryClass);
                }
                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);
            };
            ////////////////////////////////////////////////

            EntryClass.EntryDockPanel.Children.Add(BitFlags);

            EntryClass.EntryTypeBitFlag.BitFlagsDockPanel = BitFlags;
            EntryClass.EntryTypeBitFlag.BitFlag1 = BitFlag1;
            EntryClass.EntryTypeBitFlag.BitFlag2 = BitFlag2;
            EntryClass.EntryTypeBitFlag.BitFlag3 = BitFlag3;
            EntryClass.EntryTypeBitFlag.BitFlag4 = BitFlag4;
            EntryClass.EntryTypeBitFlag.BitFlag5 = BitFlag5;
            EntryClass.EntryTypeBitFlag.BitFlag6 = BitFlag6;
            EntryClass.EntryTypeBitFlag.BitFlag7 = BitFlag7;
            EntryClass.EntryTypeBitFlag.BitFlag8 = BitFlag8;

            EntryClass.EntryTypeBitFlag.BitFlag1Label = BitFlag1Label;
            EntryClass.EntryTypeBitFlag.BitFlag2Label = BitFlag2Label;
            EntryClass.EntryTypeBitFlag.BitFlag3Label = BitFlag3Label;
            EntryClass.EntryTypeBitFlag.BitFlag4Label = BitFlag4Label;
            EntryClass.EntryTypeBitFlag.BitFlag5Label = BitFlag5Label;
            EntryClass.EntryTypeBitFlag.BitFlag6Label = BitFlag6Label;
            EntryClass.EntryTypeBitFlag.BitFlag7Label = BitFlag7Label;
            EntryClass.EntryTypeBitFlag.BitFlag8Label = BitFlag8Label;

            EntryClass.EntryTypeBitFlag.BitFlag1CheckBox = BitFlag1CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag2CheckBox = BitFlag2CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag3CheckBox = BitFlag3CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag4CheckBox = BitFlag4CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag5CheckBox = BitFlag5CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag6CheckBox = BitFlag6CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag7CheckBox = BitFlag7CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag8CheckBox = BitFlag8CheckBox;

            BitFlags.Children.Add(BitFlag1);
            BitFlag1.Children.Add(BitFlag1CheckBox);
            BitFlag1.Children.Add(BitFlag1Label);
            //BitFlag1.Children.Add(BitFlag1CheckBox);

            BitFlags.Children.Add(BitFlag2);
            BitFlag2.Children.Add(BitFlag2CheckBox);
            BitFlag2.Children.Add(BitFlag2Label);
            //BitFlag2.Children.Add(BitFlag2CheckBox);

            BitFlags.Children.Add(BitFlag3);
            BitFlag3.Children.Add(BitFlag3CheckBox);
            BitFlag3.Children.Add(BitFlag3Label);
            //BitFlag3.Children.Add(BitFlag3CheckBox);

            BitFlags.Children.Add(BitFlag4);
            BitFlag4.Children.Add(BitFlag4CheckBox);
            BitFlag4.Children.Add(BitFlag4Label);
            //BitFlag4.Children.Add(BitFlag4CheckBox);

            BitFlags.Children.Add(BitFlag5);
            BitFlag5.Children.Add(BitFlag5CheckBox);
            BitFlag5.Children.Add(BitFlag5Label);
            //BitFlag5.Children.Add(BitFlag5CheckBox);

            BitFlags.Children.Add(BitFlag6);
            BitFlag6.Children.Add(BitFlag6CheckBox);
            BitFlag6.Children.Add(BitFlag6Label);
            //BitFlag6.Children.Add(BitFlag6CheckBox);

            BitFlags.Children.Add(BitFlag7);
            BitFlag7.Children.Add(BitFlag7CheckBox);
            BitFlag7.Children.Add(BitFlag7Label);
            //BitFlag7.Children.Add(BitFlag7CheckBox);

            BitFlags.Children.Add(BitFlag8);
            BitFlag8.Children.Add(BitFlag8CheckBox);
            BitFlag8.Children.Add(BitFlag8Label);
            //BitFlag8.Children.Add(BitFlag8CheckBox);

            if (TheWorkshop.IsPreviewMode == true)
            {
                BitFlag1CheckBox.IsEnabled = false;
                BitFlag2CheckBox.IsEnabled = false;
                BitFlag3CheckBox.IsEnabled = false;
                BitFlag4CheckBox.IsEnabled = false;
                BitFlag5CheckBox.IsEnabled = false;
                BitFlag6CheckBox.IsEnabled = false;
                BitFlag7CheckBox.IsEnabled = false;
                BitFlag8CheckBox.IsEnabled = false;
            }
            //BitFlag4.Width = BitFlags.Width;
            //Create a right aligned dockpanel, then Grids inside it going down, that themself have the label and Checkbox of each bit.

            //Note: Only a 1 byte MyEntry can turn into a bitflag

            //Editing it goes into properties. (As in, the user can change the name label of each flag
            //Editing might also include a option, for a custom flag graphic for each?

            //A Bitflag is 8 Flags (Checkboxes) in 1 Column, Docked Top, alignment right. (So 8 of these checkboxes are going down the right side)                
            //Thats it? Due to a bitflag always being the same checked/unchecked values, this seems kinda easy.
            //(Other then needing a good looking default checked / unchecked graphic)


            //A bunch of IF statements can exist, working backwards.
            //0: int Num = Byte Hex2Dec;
            //1: If Num >= 128, Flag 8 = on, and Num -128
            //2: If Num >= 64,  Flag 7 = on, and Num -64
            //3: If Num >= 32,  Flag 6 = on, and Num -32
            //4: If Num >= 16,  Flag 5 = on, and Num -16
            //5: If Num >= 8,   Flag 4 = on, and Num -8
            //6: If Num >= 4,   Flag 3 = on, and Num -4
            //7: If Num >= 2,   Flag 2 = on, and Num -2
            //8: If Num >= 1,   Flag 1 = on, and Num -1
            //Tada! the flags are not properly set for the user! 
            //Clicking a flag changes Num and the Bytes value up or down, based on if it's turning On or Off.
            //and thats it!
        }


        public void CreateMenu(Entry EntryClass, Workshop TheWorkshop)
        {
            if (EntryClass.EntryTypeMenu == null)
            {
                EntryClass.EntryTypeMenu = new();
            }

            //if (TheWorkshop.WorkshopData.LazyIsProjectLoaded == true) { return; }

            if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.List)
            {
                CreateList(EntryClass, TheWorkshop);
            }
            if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.Dropdown)
            {
                CreateDropDown(EntryClass, TheWorkshop);
            }
        }


        public void CreateList(Entry EntryClass, Workshop TheWorkshop)
        {
            Button ListButton = new();
            ToolTipService.SetInitialShowDelay(ListButton, LibraryGES.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(ListButton, LibraryGES.TooltipBetweenDelay);
            ListButton.MinWidth = 100;
            ListButton.Height = 30;
            ListButton.Margin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            ListButton.HorizontalAlignment = HorizontalAlignment.Right;
            ListButton.HorizontalContentAlignment = HorizontalAlignment.Left;
            ListButton.Padding = new Thickness(4, 0, 0, 0);
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                ListButton.IsEnabled = false;
            }
            EntryClass.EntryDockPanel.Children.Add(ListButton);


            ContextMenu contextMenu = new();
            ListButton.ContextMenu = contextMenu;

            MenuItem goToEditorMenuItem = new();
            goToEditorMenuItem.Header = "Go to Linked Editor";            
            contextMenu.Items.Add(goToEditorMenuItem);
            goToEditorMenuItem.Click += (sender, e) =>
            {
                if (ListButton.Tag is TextInfo Iinfo) 
                {
                    Editor linkedEditor = EntryClass.EntryTypeMenu.TextTableEditor.LinkedDTEEditor;
                    linkedEditor.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    foreach (TreeViewItem treeitem in linkedEditor.DataTableEditorData.EditorLeftBar.TreeView.Items)
                    {
                        if (treeitem.Tag == null) { return; }

                        TextInfo treeItemInfo = treeitem.Tag as TextInfo;
                        if (treeItemInfo == Iinfo)
                        {
                            treeitem.IsSelected = true;
                            treeitem.BringIntoView();
                            break;
                        }

                        if (treeItemInfo.IsFolder == true) 
                        {
                            foreach (TreeViewItem Ftreeitem in treeitem.Items)
                            {
                                if (Ftreeitem.Tag == null) { return; }

                                TextInfo FtreeItemInfo = Ftreeitem.Tag as TextInfo;
                                if (FtreeItemInfo == Iinfo)
                                {
                                    treeitem.IsExpanded = true;
                                    Ftreeitem.IsSelected = true;
                                    Ftreeitem.BringIntoView();
                                    break;
                                }
                            }
                        }
                    }
                }
            };
            try //Name the editor link
            {
                if (EntryClass.EntryTypeMenu.TextTableEditor != null) 
                {
                    if (EntryClass.EntryTypeMenu.TextTableEditor.LinkedDTEEditor != null) 
                    {
                        goToEditorMenuItem.Header = "Go to Linked Editor" + " (" + EntryClass.EntryTypeMenu.TextTableEditor.LinkedDTEEditor.EditorName + ")";
                    }
                }  
            } catch { }


            EntryClass.EntryTypeMenu.ListButton = ListButton;
            ListButton.Click += (sender, e) =>
            {
                //DTEMethods.EntryActivate(EntryClass);

                //SetSelectedEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

                EntryClass.ParentEditor.DataTableEditorData.EntryClass = EntryClass;
                DTRightBar RightBar = EntryClass.ParentEditor.DataTableEditorData.EditorRightBar;
                

                EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.SelectionChanged -= EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox_SelectionChanged; // Remove event handler    
                EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.Items.Clear(); // Clear items

                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile)
                {
                    if (TheWorkshop.IsPreviewMode == true) { return; }
                    
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableDataFile;
                    for (int i = 0; i < texttable.TextTableItemCount; i++)
                    {
                        byte[] thebytes = texttable.TextTableFile.FileBytes;

                        int thetextsize = texttable.TextTableCharLimit;
                        int thetablestart = texttable.TextTableStart;
                        int therowsize = texttable.TextTableRowSize;

                        byte[] bytes = new byte[thetextsize];
                        for (int rowindex = 0; rowindex < thetextsize; rowindex++)
                        {
                            bytes[rowindex] = thebytes[thetablestart + (i * therowsize) + rowindex]; //item.itemindex * 
                        }


                        CharacterSetManager Decoder = new();
                        string TheText = Decoder.DecodeReturn(texttable.TextTableCharacterSet, bytes);

                        ListViewItem ListItem = new();
                        ListItem.Content = (i + texttable.TextTableFirstNameID) + ": " + TheText;
                        ListItem.Tag = texttable.ItemList[i];
                        EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.Items.Add(ListItem);
                    }
                    RightBar.ListFirstNameID = texttable.TextTableFirstNameID;
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                {
                    if (TheWorkshop.IsPreviewMode == true) { return; }


                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableTextFile;
                    string fullText = Encoding.UTF8.GetString(texttable.TextTableFile.FileBytes);
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    for (int i = 0; i < texttable.TextTableItemCount; i++)
                    {
                        byte[] thebytes = texttable.TextTableFile.FileBytes;

                        int index = i + texttable.TextTableStart;
                        string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";

                        ListViewItem ListItem = new();
                        ListItem.Content = (i + texttable.TextTableFirstNameID) + ": " + lineText;
                        ListItem.Tag = texttable.ItemList[i];
                        EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.Items.Add(ListItem);
                    }
                    RightBar.ListFirstNameID = texttable.TextTableFirstNameID;

                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
                {
                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableEditor;
                    if (texttable.LinkedDTEEditor == null) { return; } //This is if the editor it's looking for doesn't exist, didn't load, etc. (Why is this here? Isn't this a critical error?)

                    //for (int i = 0; i < texttable.TextTableItemCount; i++)
                    foreach (TextInfo itemInfo in texttable.LinkedDTEEditor.DataTableEditorData.NameTable.ItemList)
                    {
                        if (itemInfo.IsFolder == true) {continue; }

                        ListViewItem ListItem = new();
                        ListItem.Tag = itemInfo;

                        TextBlock itext = new();

                        Run Prefix = new((itemInfo.ItemIndex + texttable.LinkedDTEEditor.NameTable.TextTableFirstNameID) + ": ");
                        itext.Inlines.Add(Prefix);

                        Run myname = new(itemInfo.ItemName);
                        itext.Inlines.Add(myname);
                        if (itemInfo.ItemWorkshopTooltip != "")
                        {
                            myname.ToolTip = itemInfo.ItemWorkshopTooltip;
                            myname.TextDecorations = TextDecorations.Underline;
                            ToolTipService.SetInitialShowDelay(myname, LibraryGES.TooltipInitialDelay);
                            ToolTipService.SetBetweenShowDelay(myname, LibraryGES.TooltipBetweenDelay);
                        }

                        if (itemInfo.ItemNote != "")
                        {
                            Run mynote = new("   (" + itemInfo.ItemNote + ")");
                            itext.Inlines.Add(mynote);
                            mynote.Foreground = Brushes.Orange;
                        }


                        ListItem.Content = itext;


                        EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.Items.Add(ListItem);
                    }
                    //RightBar.ListFirstNameID = texttable.TextTableFirstNameID;
                    RightBar.ListFirstNameID = texttable.LinkedDTEEditor.DataTableEditorData.NameTable.TextTableFirstNameID;                    
                    
                    //    else //This is if the editor is missing or has been deleted, so the program still kinda functions and doesn't crash.
                    //    {

                    //        ListItem.Content = (i + texttable.TextTableFirstNameID) + ": " + iname; //EntryClass.EntryTypeMenu.NothingNameList[i]
                    //        ListItem.Foreground = Brushes.Red; //For error message
                    //        EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.Items.Add(ListItem);
                    //    }


                    //}


                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
                {   

                    TextTable texttable = EntryClass.EntryTypeMenu.TextTableNothing;
                    foreach (TextInfo textinfo in texttable.ItemList) 
                    {
                        ListViewItem ListItem = new();
                        ListItem.Content = (textinfo.ItemIndex + texttable.TextTableFirstNameID)  + ": " + textinfo.ItemName;
                        ListItem.Tag = textinfo;
                        EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.Items.Add(ListItem);
                    }
                    RightBar.ListFirstNameID = texttable.TextTableFirstNameID;


                }
                EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox.SelectionChanged += EntryClass.ParentEditor.DataTableEditorData.DTEXaml.RightBar.EntryListBox_SelectionChanged; // Re-attach event handler  


                foreach (TabItem tabItem in RightBar.MainTabControl.Items) //Open the list menu   //I GOT A WEIRD ERROR DOING UNRELATED STUFF, AND ADDED "USING TABITEM" TO THE TOP, AND IT FIXED IT. I HAVE NO IDEA WHY.
                {
                    if (tabItem.Header != null && tabItem.Header.ToString() == "Lists")
                    {
                        if (RightBar.MainTabControl.SelectedItem is TabItem currentTab) //store current tab to return to it after list.
                        {
                            if (currentTab.Header.ToString() != "Lists")
                            {
                                RightBar.PreviousTabName = currentTab.Header.ToString();
                            }

                        }
                        tabItem.IsSelected = true;
                        tabItem.Focus();
                        break;
                    }
                }


            };

            if (TheWorkshop.IsPreviewMode == true) { ListButton.Content = "Preview Mode"; return; }
            if (TheWorkshop.IsPreviewMode == true) { ListButton.IsEnabled = false; }
        }

        public void CreateDropDown(Entry EntryClass, Workshop Workshop)
        {
            //Triggered only during UI creation, when a entry first becomes a menu, and when it changes from a list to a dropdown.
            //Note that the UI being recreated is now semi-common. 

            

            ComboBox comboBox = new();
            comboBox.MinWidth = 100;
            comboBox.Margin = new Thickness(0, 3, 3, 3); // Left Top Right Bottom 



            EntryClass.EntryTypeMenu.Dropdown = comboBox;
            EntryClass.EntryDockPanel.Children.Add(comboBox);

            ToolTipService.SetInitialShowDelay(comboBox, LibraryGES.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(comboBox, LibraryGES.TooltipBetweenDelay);

            //if (Workshop.IsPreviewMode == true) { return; }


            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile)
            {
                if (Workshop.IsPreviewMode == true)
                {   
                    comboBox.IsEnabled = false;
                    return;
                }

                TextTable texttable = EntryClass.EntryTypeMenu.TextTableDataFile;
                for (int i = 0; i < texttable.TextTableItemCount; i++)
                {
                    ComboBoxItem comboBoxItem = new();
                    ToolTipService.SetInitialShowDelay(comboBoxItem, LibraryGES.TooltipInitialDelay);
                    ToolTipService.SetBetweenShowDelay(comboBoxItem, LibraryGES.TooltipBetweenDelay);
                    byte[] thebytes = texttable.TextTableFile.FileBytes;



                    int thetextsize = texttable.TextTableCharLimit;
                    int thetablestart = texttable.TextTableStart;
                    int therowsize = texttable.TextTableRowSize;

                    byte[] bytes = new byte[thetextsize];
                    for (int rowindex = 0; rowindex < thetextsize; rowindex++)
                    {
                        bytes[rowindex] = thebytes[thetablestart + (i * therowsize) + rowindex]; //item.itemindex * 
                    }


                    CharacterSetManager Decoder = new();
                    string TheText = Decoder.DecodeReturn(texttable.TextTableCharacterSet, bytes);




                    comboBoxItem.Content = (i + texttable.TextTableFirstNameID) + ": " + TheText; //EntryClass.EntryTypeMenu.NothingNameList[i]
                    comboBoxItem.Tag = texttable.ItemList[i]; //So we can get the item info later if needed.
                    comboBox.Items.Add(comboBoxItem);
                }
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
            {
                if (Workshop.IsPreviewMode == true)
                {                    
                    comboBox.IsEnabled = false;
                    return;
                }


                TextTable texttable = EntryClass.EntryTypeMenu.TextTableTextFile;

                string fullText = Encoding.UTF8.GetString(texttable.TextTableFile.FileBytes);
                string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                //int LineCount = Math.Min(lines.Length, 255);

                for (int i = 0; i < texttable.TextTableItemCount; i++)
                {
                    ComboBoxItem comboBoxItem = new();
                    ToolTipService.SetInitialShowDelay(comboBoxItem, LibraryGES.TooltipInitialDelay);
                    ToolTipService.SetBetweenShowDelay(comboBoxItem, LibraryGES.TooltipBetweenDelay);
                    byte[] thebytes = texttable.TextTableFile.FileBytes;

                    int index = i + texttable.TextTableStart;
                    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";





                    comboBoxItem.Content = (i + texttable.TextTableFirstNameID) + ": " + lineText; //EntryClass.EntryTypeMenu.NothingNameList[i]
                    comboBoxItem.Tag = texttable.ItemList[i]; //So we can get the item info later if needed.
                    comboBox.Items.Add(comboBoxItem);
                }
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
            {
                if (Workshop.WorkshopData.IsProjectLoaded == false)
                {
                    ComboBoxItem previewItem = new();
                    ToolTipService.SetInitialShowDelay(previewItem, LibraryGES.TooltipInitialDelay);
                    ToolTipService.SetBetweenShowDelay(previewItem, LibraryGES.TooltipBetweenDelay);
                    comboBox.Items.Add(previewItem);
                    previewItem.Content = "Preview Mode";
                    previewItem.Tag = "Preview";
                    previewItem.Foreground = Brushes.Gray; //Doesn't work, bah...
                    previewItem.IsSelected = true;
                    //return;
                }


                TextTable texttable = EntryClass.EntryTypeMenu.TextTableEditor;
                if (texttable == null) { return; } //Bandaid fix for newly created menus because this is the current default. May cause unexpected problems.
                if (texttable.LinkedDTEEditor == null) { return; }
                //if (texttable.LinkedDTEEditor == null) { return; } //This is if the editor it's looking for doesn't exist, didn't load, etc. (Why is this here? Isn't this a critical error?)
                foreach (TextInfo textInfo in texttable.LinkedDTEEditor.NameTable.ItemList)
                {
                    if (textInfo.IsFolder == true) { continue; }
                    ComboBoxItem comboBoxItem = new();
                    ToolTipService.SetInitialShowDelay(comboBoxItem, LibraryGES.TooltipInitialDelay);
                    ToolTipService.SetBetweenShowDelay(comboBoxItem, LibraryGES.TooltipBetweenDelay);

                    //comboBoxItem.Content = (textInfo.ItemIndex + texttable.TextTableFirstNameID) + ": " + textInfo.ItemName;
                    comboBoxItem.Content = (textInfo.ItemIndex + texttable.LinkedDTEEditor.NameTable.TextTableFirstNameID) + ": " + textInfo.ItemName;
                    if (textInfo.ItemWorkshopTooltip != "") { comboBoxItem.ToolTip = textInfo.ItemWorkshopTooltip; }
                    comboBoxItem.Tag = textInfo; //So we can get the item info later if needed.
                    comboBox.Items.Add(comboBoxItem);
                }




                //Link to editor right click stuff.
                ContextMenu contextMenu = new();
                comboBox.ContextMenu = contextMenu;

                MenuItem goToEditorMenuItem = new();
                goToEditorMenuItem.Header = "Go to Linked Editor";
                contextMenu.Items.Add(goToEditorMenuItem);
                goToEditorMenuItem.Click += (sender, e) =>
                {
                    if (texttable.LinkedDTEEditor == null) { return; }
                    if (comboBox.SelectedItem == null) { return; }

                    ComboBoxItem comboBoxItem = comboBox.SelectedItem as ComboBoxItem;
                    if (comboBoxItem.Tag == null) { return; }

                    TextInfo itemInfo = comboBoxItem.Tag as TextInfo;

                    Editor linkedEditor = texttable.LinkedDTEEditor;
                    linkedEditor.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    foreach (TreeViewItem treeitem in linkedEditor.DataTableEditorData.EditorLeftBar.TreeView.Items)
                    {
                        if (treeitem.Tag == null) { continue; }

                        TextInfo treeItemInfo = treeitem.Tag as TextInfo;
                        if (treeItemInfo == itemInfo)
                        {
                            treeitem.IsSelected = true;
                            treeitem.BringIntoView();
                            break;
                        }
                        
                        if (treeItemInfo.IsFolder == true) 
                        {
                            foreach (TreeViewItem Ftreeitem in treeitem.Items)
                            {
                                if (Ftreeitem.Tag == null) { continue; }

                                TextInfo FitemInfo = Ftreeitem.Tag as TextInfo;
                                if (FitemInfo == itemInfo)
                                {
                                    treeitem.IsExpanded = true;
                                    Ftreeitem.IsSelected = true;                                    
                                    Ftreeitem.BringIntoView();
                                    break;
                                }

                                
                            }
                        }
                    }
                };
                try { goToEditorMenuItem.Header = "Go to Linked Editor" + " (" + texttable.LinkedDTEEditor.EditorName + ")"; } catch { }



            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
            {
                if (Workshop.WorkshopData.IsProjectLoaded == false)
                {
                    ComboBoxItem previewItem = new();
                    ToolTipService.SetInitialShowDelay(previewItem, LibraryGES.TooltipInitialDelay);
                    ToolTipService.SetBetweenShowDelay(previewItem, LibraryGES.TooltipBetweenDelay);
                    comboBox.Items.Add(previewItem);
                    previewItem.Content = "Preview Mode";
                    previewItem.Tag = "Preview";
                    previewItem.Foreground = Brushes.Gray; //Doesn't work, bah...
                    previewItem.IsSelected = true;
                    //return;
                }



                TextTable texttable = EntryClass.EntryTypeMenu.TextTableNothing;   
                foreach (TextInfo textInfo in texttable.ItemList)
                {
                    ComboBoxItem comboBoxItem = new();
                    ToolTipService.SetInitialShowDelay(comboBoxItem, LibraryGES.TooltipInitialDelay);
                    ToolTipService.SetBetweenShowDelay(comboBoxItem, LibraryGES.TooltipBetweenDelay);
                    comboBoxItem.Content = (textInfo.ItemIndex + texttable.TextTableFirstNameID) + ": " + textInfo.ItemName;
                    comboBoxItem.Tag = textInfo; 
                    comboBox.Items.Add(comboBoxItem);
                }
            }

            
            
            comboBox.MouseDown += (sender, e) =>
            {
                e.Handled = true; // Prevents the ComboBox from immediately closing after opening due to losing focus.
            };
            comboBox.MouseMove += (sender, e) =>
            {
               
                e.Handled = true; // Prevents the ComboBox from immediately closing after opening due to losing focus.
            };


            ComboBoxItem previewitem = null;
            comboBox.DropDownOpened += (sender, e) =>
            {
                DTEMethods.EntryActivate(EntryClass);
            };

            comboBox.DropDownClosed += (sender, e) =>
            {
                if (Workshop.WorkshopData.IsProjectLoaded == false) 
                {
                    comboBox.SelectedItem = comboBox.Items[0];
                    return; 
                }

                ComboBoxItem comboItem = comboBox.SelectedItem as ComboBoxItem;
                if (comboItem == null) { return; }

                TextInfo textInfo = comboItem.Tag as TextInfo;
                EntryClass.EntryByteDecimal = textInfo.ItemIndex.ToString();

                SaveEntry(EntryClass);
                //UpdateEntryProperties(EntryClass);
                //StandardEditorMethods.EntryActivate(EntryClass);
                DTEMethods.EntryActivate(EntryClass);

                comboBox.ToolTip = null;
                if (comboItem.Tag != null)
                {
                    if (comboItem.Tag is TextInfo itemInfo)
                    {
                        if (itemInfo.ItemWorkshopTooltip != "") 
                        {
                            comboBox.ToolTip = itemInfo.ItemWorkshopTooltip;
                        }
                        
                    }
                }


            };







        }



        ///////////////////////////////////////////////////END OF ENTRYS//////////////////////////////////////////////////////////














        //////////////////////////////////////////////START OF ENTRY TYPE SWAP////////////////////////////////////////////////////


        public void ChangeEntryType(WorkshopData Database, EntrySubTypes NewEntryType, Workshop TheWorkshop, Entry EntryClass)
        {
            

            
            //This chunk is commented out as a lazy way to ensure disabled entrys actually become disabled.
            //if (EntryClass.EntrySubType == NewEntryType) //This check isn't to stop anything, just save processing power, but it could be deleted if desired.
            //{
            //    return;
            //}


            //I may crash as i build this, as i attempt to delete a thing that already doesn't exist?
            if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox)
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeNumberBox.NumberBoxTextBox);

            }

            if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox)
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeCheckBox.CheckBoxButton);

            }

            if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag)
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeBitFlag.BitFlagsDockPanel);

            }

            if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu)
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeMenu.ListButton);
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeMenu.Dropdown);

            }

            //How do i actually destroy something, such that it also isn't in memory, and isn't just "removed"?
            //Does the garbage collector automatically get it? or not?
            //NumBox.Parent.Children.Remove(NumBox);
            //NumBox.Dispose();
            //NumBox = null; 

            //////////////////////Deleting MyEntry modules//////////////////////////////
            //////////////////////Creating MyEntry modules//////////////////////////////

            if (EntryClass.IsNameHidden == false) { EntryClass.EntryNameTextBlock.Visibility = Visibility.Visible; }
            if (EntryClass.IsNameHidden == true) { EntryClass.EntryNameTextBlock.Visibility = Visibility.Collapsed; }

            if (NewEntryType == Entry.EntrySubTypes.NumberBox) //Step X: Create new Entry Module using Entry.ByteD and any other data relevant to this.
            {
                CreateNumberBox(TheWorkshop, EntryClass);
                LoadNumberBox(EntryClass); //Change Entry Type.
                EntryClass.NewSubType = Entry.EntrySubTypes.NumberBox;

            }


            if (NewEntryType == Entry.EntrySubTypes.CheckBox)
            {
                CreateCheckBox(TheWorkshop, EntryClass);
                LoadCheckBox(EntryClass); //Change Entry Type.
                EntryClass.NewSubType = Entry.EntrySubTypes.CheckBox;
            }



            if (NewEntryType == Entry.EntrySubTypes.BitFlag)
            {
                CreateBitFlag(TheWorkshop, EntryClass);
                LoadBitFlag(EntryClass); //Change Entry Type.
                EntryClass.NewSubType = Entry.EntrySubTypes.BitFlag;
            }
                        

            if (NewEntryType == Entry.EntrySubTypes.Menu)
            {
                CreateMenu(EntryClass, TheWorkshop);
                LoadMenu(EntryClass); //Change Entry Type.
                EntryClass.NewSubType = Entry.EntrySubTypes.Menu;
            }

            
            TheWorkshop.UpdateSymbology(EntryClass); //Update Symbology when Entry Type is changed. Example: Red !!! when a checkbox type is obviously not a bool.
        }




        public void SetSelectedEntry(Entry EntryClass) 
        {
            Editor EditorClass = EntryClass.ParentEditor; 

            //Window parentWindow = Window.GetWindow(EntryClass.EntryDockPanel);

            DTRightBar RightBar = EditorClass.DataTableEditorData.EditorRightBar;

            if (EditorClass.DataTableEditorData.DTEXaml.RightBar.ListTab.IsSelected == true) 
            {
                RightBar.GeneralTab.IsSelected = true;                
            }            

            TabControl tabControlProp = RightBar.EntryElementProperties; //The tab control for the Entry Type's Properties.
            foreach (TabItem tabItem in tabControlProp.Items)
            {                
                if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox && tabItem.Header != null && tabItem.Header.ToString() == "NumberBox")
                {
                    tabItem.IsSelected = true;
                    break;
                }
                if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox && tabItem.Header != null && tabItem.Header.ToString() == "CheckBox")
                {
                    tabItem.IsSelected = true;
                    break;
                }
                if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag && tabItem.Header != null && tabItem.Header.ToString() == "BitFlag")
                {
                    tabItem.IsSelected = true;
                    break;
                }
                if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu && tabItem.Header != null && tabItem.Header.ToString() == "Menu")
                {
                    tabItem.IsSelected = true;
                    break;
                }
            }

            Entry PreviousEntry = EditorClass.DataTableEditorData.EntryClass;
            EditorClass.DataTableEditorData.EntryClass = EntryClass; //Note: This MUST be here, After clear EntryClass color, and before set EntryClass Color.  

            EntryStyleUpdate(EditorClass.DataTableEditorData);   

            RightBar.PropertiesNameBox.IsEnabled = true;
            if (EntryClass.IsNameHidden == true) { RightBar.PropertiesNameBox.IsEnabled = false; }

        }

        public void EntryStyleUpdate(DataTableEditorData DTEdata) 
        {

            foreach (Entry entry in DTEdata.MasterEntryList) 
            {
                if (entry != DTEdata.EntryClass)
                {
                    if (entry.IsEntryHidden == false && entry.IsTextInUse == false)
                    {
                        entry.EntryBorder.Style = (Style)Application.Current.Resources["EntryStyle"];
                    }
                    else
                    {
                        entry.EntryBorder.Style = (Style)Application.Current.Resources["HiddenEntryStyle"];
                    }
                }


                if (entry == DTEdata.EntryClass)
                {
                    if (entry.IsEntryHidden == false && entry.IsTextInUse == false)
                    {
                        entry.EntryBorder.Style = (Style)Application.Current.Resources["SelectedEntryStyle"];
                    }
                    else
                    {
                        entry.EntryBorder.Style = (Style)Application.Current.Resources["HiddenSelectedEntryStyle"];
                    }
                }
            }

            
            
        }


        public void UpdateEntryProperties(Entry EntryClass) 
        {
            //This used to happen on tree view selection change, but it caused a shit ton of lag. Now it doesn't. if my program starts lagging again, consider if this is responsible! >:(

            Workshop TheWorkshop = EntryClass.ParentEditor.WorkshopXaml;
            Editor EditorClass = EntryClass.ParentEditor;

            DTRightBar RightBar = EntryClass.ParentEditor.DataTableEditorData.EditorRightBar;

            RightBar.EntryTabItem.IsSelected = true;

            //////////////////////////////////////Workship Update//////////////////////////////////////////
            RightBar.DTEData.CategoryClass = EntryClass.ParentCategory;


            //////////////////////////////////////Right Bar Settings Update//////////////////////////////////////////
            RightBar.PropertiesNameBox.Text = EntryClass.Name;   
            
            if (EntryClass.IsNameHidden == true) 
            {
                RightBar.HideNameCheckbox.IsChecked = true;
            }
            else 
            {
                RightBar.HideNameCheckbox.IsChecked = false;
            }

            if (EntryClass.IsEntryHidden == true)
            {
                RightBar.HideEntryCheckbox.IsChecked = true;
            }
            else
            {
                RightBar.HideEntryCheckbox.IsChecked = false;
            }            

            string FindEntryType = EntryClass.NewSubType.ToString();  //Entry Type Dropdown Menu.
            foreach (ComboBoxItem item in RightBar.PropertiesEntryType.Items)
            {
                if (item.Content.ToString() == FindEntryType)
                {
                    RightBar.PropertiesEntryType.SelectedItem = item;
                    break;
                }
            }


            string FindEntryByteSize = "Dummy"; //Entry Size Dropdown Menu.
            if (EntryClass.Endianness == "1") { FindEntryByteSize = "1 Byte"; } 
            else if (EntryClass.Endianness == "2L") { FindEntryByteSize = "2 Bytes Little Endian"; }
            else if (EntryClass.Endianness == "4L") { FindEntryByteSize = "4 Bytes Little Endian"; }
            else if (EntryClass.Endianness == "2B") { FindEntryByteSize = "2 Bytes Big Endian"; }
            else if (EntryClass.Endianness == "4B") { FindEntryByteSize = "4 Bytes Big Endian"; }
            foreach (ComboBoxItem item in RightBar.PropertiesEntryByteSizeComboBox.Items)
            {
                if (item.Content.ToString() == FindEntryByteSize)
                {
                    RightBar.PropertiesEntryByteSizeComboBox.SelectedItem = item;
                    break;
                }
            }


            //////////////////////////////////////Right Bar Submenu Settings Update//////////////////////////////////////////
            if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox)
            {                
                if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned) { RightBar.NumberboxSignCheckbox.IsChecked = false; }
                if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed) { RightBar.NumberboxSignCheckbox.IsChecked = true; }
                RightBar.NumberboxSuffixTextbox.Text = EntryClass.EntryTypeNumberBox.Suffix;
            }

            if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox) 
            {
                RightBar.PropertiesEntryCheckText.Text = EntryClass.EntryTypeCheckBox.TrueText;
                RightBar.PropertiesEntryUncheckText.Text = EntryClass.EntryTypeCheckBox.FalseText;
            }

            if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag) 
            {
                RightBar.PropertiesEntryBitFlag1Name.Text = EntryClass.EntryTypeBitFlag.BitFlag1Name;
                RightBar.PropertiesEntryBitFlag2Name.Text = EntryClass.EntryTypeBitFlag.BitFlag2Name;
                RightBar.PropertiesEntryBitFlag3Name.Text = EntryClass.EntryTypeBitFlag.BitFlag3Name;
                RightBar.PropertiesEntryBitFlag4Name.Text = EntryClass.EntryTypeBitFlag.BitFlag4Name; 
                RightBar.PropertiesEntryBitFlag5Name.Text = EntryClass.EntryTypeBitFlag.BitFlag5Name;
                RightBar.PropertiesEntryBitFlag6Name.Text = EntryClass.EntryTypeBitFlag.BitFlag6Name;
                RightBar.PropertiesEntryBitFlag7Name.Text = EntryClass.EntryTypeBitFlag.BitFlag7Name;
                RightBar.PropertiesEntryBitFlag8Name.Text = EntryClass.EntryTypeBitFlag.BitFlag8Name;
            }


            if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu)
            {
                RightBar.DropdownMenuType.IsEnabled = true;
                if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.Dropdown) { RightBar.MenuTypeItemDropdown.IsSelected = true; }
                else if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.List) { RightBar.MenuTypeItemList.IsSelected = true; }
                
            }
            else //failsafe
            {
                RightBar.DropdownMenuType.IsEnabled = false; 
            }

            //////////////////////////////////////Right Bar Hex Data Update//////////////////////////////////////////
            UpdateEntryHexProperties(EntryClass.ParentEditor.DataTableEditorData);

            //////////////////////////////////////END//////////////////////////////////////////            
            TheWorkshop.UpdateSymbology(EntryClass); //Update Symbology when Entry Value is changed.

        } //End of UpdateEntryProperties Method


        public async void UpdateEntryHexProperties(DataTableEditorData DTEData)
        {

            Entry EntryClass = DTEData.EntryClass;
            DTRightBar RightBar = DTEData.EditorRightBar;

            if (DTEData.DataTable == null) { return; }
            

            RightBar.PropertiesEntryHexAddressTextbox.Text = (DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X2");

            //string thetext = await Task.Run(() => { return (DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X2"); });
            //RightBar.PropertiesEntryHexAddressTextbox.Text = thetext;

            /////////////////////If project is loaded or not/////////////////////                        
            if (DTEData.WorkshopData.IsProjectLoaded == false)
            {
                //If NO project
                RightBar.PropertiesEntry1Byte.Text = "";
                RightBar.PropertiesEntryHex1Byte.Text = "";
                RightBar.PropertiesEntry2ByteB.Text = "";
                RightBar.PropertiesEntryHex2ByteB.Text = "";
                RightBar.PropertiesEntry2ByteL.Text = "";
                RightBar.PropertiesEntryHex2ByteL.Text = "";
                RightBar.PropertiesEntry4ByteB.Text = "";
                RightBar.PropertiesEntryHex4ByteB.Text = "";
                RightBar.PropertiesEntry4ByteL.Text = "";
                RightBar.PropertiesEntryHex4ByteL.Text = "";
                RightBar.PropertiesEntry1ByteNegative.Text = "";
                RightBar.PropertiesEntry2ByteBNegative.Text = "";
                RightBar.PropertiesEntry2ByteLNegative.Text = "";
                RightBar.PropertiesEntry4ByteBNegative.Text = "";
                RightBar.PropertiesEntry4ByteLNegative.Text = "";

                RightBar.PropertiesEntry1Byte.IsEnabled = false;
                RightBar.PropertiesEntryHex1Byte.IsEnabled = false;
                RightBar.PropertiesEntry2ByteB.IsEnabled = false;
                RightBar.PropertiesEntryHex2ByteB.IsEnabled = false;
                RightBar.PropertiesEntry2ByteL.IsEnabled = false;
                RightBar.PropertiesEntryHex2ByteL.IsEnabled = false;
                RightBar.PropertiesEntry4ByteB.IsEnabled = false;
                RightBar.PropertiesEntryHex4ByteB.IsEnabled = false;
                RightBar.PropertiesEntry4ByteL.IsEnabled = false;
                RightBar.PropertiesEntryHex4ByteL.IsEnabled = false;
                RightBar.PropertiesEntry1ByteNegative.IsEnabled = false;
                RightBar.PropertiesEntry2ByteBNegative.IsEnabled = false;
                RightBar.PropertiesEntry2ByteLNegative.IsEnabled = false;
                RightBar.PropertiesEntry4ByteBNegative.IsEnabled = false;
                RightBar.PropertiesEntry4ByteLNegative.IsEnabled = false;
                return;

            }
            else if (DTEData.WorkshopData.IsProjectLoaded == true)
            {
                //If there IS a project!
                RightBar.PropertiesEntry1Byte.IsEnabled = true;
                RightBar.PropertiesEntryHex1Byte.IsEnabled = true;
                RightBar.PropertiesEntry2ByteB.IsEnabled = true;
                RightBar.PropertiesEntryHex2ByteB.IsEnabled = true;
                RightBar.PropertiesEntry2ByteL.IsEnabled = true;
                RightBar.PropertiesEntryHex2ByteL.IsEnabled = true;
                RightBar.PropertiesEntry4ByteB.IsEnabled = true;
                RightBar.PropertiesEntryHex4ByteB.IsEnabled = true;
                RightBar.PropertiesEntry4ByteL.IsEnabled = true;
                RightBar.PropertiesEntryHex4ByteL.IsEnabled = true;
                RightBar.PropertiesEntry1ByteNegative.IsEnabled = true;
                RightBar.PropertiesEntry2ByteBNegative.IsEnabled = true;
                RightBar.PropertiesEntry2ByteLNegative.IsEnabled = true;
                RightBar.PropertiesEntry4ByteBNegative.IsEnabled = true;
                RightBar.PropertiesEntry4ByteLNegative.IsEnabled = true;
            }


            /////////////////////Data Analyzer/////////////////////
            RightBar.PropertiesEntry1Byte.Text = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
            RightBar.PropertiesEntryHex1Byte.Text = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("X2");

            try
            {
                ushort value2 = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness

                RightBar.PropertiesEntry2ByteL.Text = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                RightBar.PropertiesEntryHex2ByteL.Text = swappedValue2.ToString("X4"); // Convert the swapped value4 to a string using the desired format                
                RightBar.PropertiesEntry2ByteB.Text = swappedValue2.ToString("D"); // Convert the swapped value4 to a string using the desired format 
                RightBar.PropertiesEntryHex2ByteB.Text = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X4");

            }
            catch
            {
                RightBar.PropertiesEntry2ByteL.Text = "END OF FILE";
                RightBar.PropertiesEntryHex2ByteL.Text = "END OF FILE";
                RightBar.PropertiesEntry2ByteB.Text = "END OF FILE";
                RightBar.PropertiesEntryHex2ByteB.Text = "END OF FILE";
            }

            try
            {
                uint value = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Reverse(valueBytes);
                uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);

                RightBar.PropertiesEntry4ByteB.Text = swappedValue.ToString("D");
                RightBar.PropertiesEntryHex4ByteB.Text = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X8");

                RightBar.PropertiesEntry4ByteL.Text = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                RightBar.PropertiesEntryHex4ByteL.Text = swappedValue.ToString("X8");
            }
            catch
            {
                RightBar.PropertiesEntry4ByteB.Text = "END OF FILE";
                RightBar.PropertiesEntryHex4ByteB.Text = "END OF FILE";
                RightBar.PropertiesEntry4ByteL.Text = "END OF FILE";
                RightBar.PropertiesEntryHex4ByteL.Text = "END OF FILE";
            }


            //Starting here is for attempting to read possible negative values. 
            // === 1 Byte (Signed) ===
            try
            {
                sbyte signedByte = (sbyte)DTEData.DataTable.FileDataTable.FileBytes[
                    DTEData.DataTable.DataTableStart +
                    (DTEData.TableRowIndex * EntryClass.DataTableRowSize) +
                    EntryClass.RowOffset];
                RightBar.PropertiesEntry1ByteNegative.Text = signedByte < 0 ? signedByte.ToString("D") : "";

                if (RightBar.PropertiesEntry1ByteNegative.Text == "") //If it can't be read as a negative...
                {
                    //RightBar.PropertiesEntry1ByteNegative.Text = "Positive Only";
                    RightBar.PropertiesEntry1ByteNegative.IsEnabled = false;
                }
            }
            catch
            {
                RightBar.PropertiesEntry1ByteNegative.Text = "END OF FILE";
            }

            // === 2 Byte (Signed) ===
            try
            {
                int offset2 = DTEData.DataTable.DataTableStart +
                              (DTEData.TableRowIndex * EntryClass.DataTableRowSize) +
                              EntryClass.RowOffset;

                short s2L = BitConverter.ToInt16(DTEData.DataTable.FileDataTable.FileBytes, offset2);
                RightBar.PropertiesEntry2ByteLNegative.Text = s2L < 0 ? s2L.ToString("D") : "";


                byte[] signedBytes2B = DTEData.DataTable.FileDataTable.FileBytes
                    .Skip(offset2).Take(2).ToArray();
                Array.Reverse(signedBytes2B);
                short swappedS2B = BitConverter.ToInt16(signedBytes2B, 0);
                RightBar.PropertiesEntry2ByteBNegative.Text = swappedS2B < 0 ? swappedS2B.ToString("D") : "";



                if (RightBar.PropertiesEntry2ByteBNegative.Text == "") //If it can't be read as a negative...
                {
                    //RightBar.PropertiesEntry2ByteBNegative.Text = "Positive Only";
                    RightBar.PropertiesEntry2ByteBNegative.IsEnabled = false;
                }
                if (RightBar.PropertiesEntry2ByteLNegative.Text == "") //If it can't be read as a negative...
                {
                    //RightBar.PropertiesEntry2ByteLNegative.Text = "Positive Only";
                    RightBar.PropertiesEntry2ByteLNegative.IsEnabled = false;
                }
            }
            catch
            {
                RightBar.PropertiesEntry2ByteBNegative.Text = "END OF FILE";
                RightBar.PropertiesEntry2ByteLNegative.Text = "END OF FILE";
            }

            // === 4 Byte (Signed) ===
            try
            {
                int offset4 = DTEData.DataTable.DataTableStart +
                              (DTEData.TableRowIndex * EntryClass.DataTableRowSize) +
                              EntryClass.RowOffset;

                int s4L = BitConverter.ToInt32(DTEData.DataTable.FileDataTable.FileBytes, offset4);
                RightBar.PropertiesEntry4ByteLNegative.Text = s4L < 0 ? s4L.ToString("D") : "";


                byte[] signedBytes4B = DTEData.DataTable.FileDataTable.FileBytes
                    .Skip(offset4).Take(4).ToArray();
                Array.Reverse(signedBytes4B);
                int swappedS4B = BitConverter.ToInt32(signedBytes4B, 0);
                RightBar.PropertiesEntry4ByteBNegative.Text = swappedS4B < 0 ? swappedS4B.ToString("D") : "";


                if (RightBar.PropertiesEntry4ByteBNegative.Text == "") //If it can't be read as a negative...
                {
                    //RightBar.PropertiesEntry4ByteBNegative.Text = "Positive Only";
                    RightBar.PropertiesEntry4ByteBNegative.IsEnabled = false;
                }
                if (RightBar.PropertiesEntry4ByteLNegative.Text == "") //If it can't be read as a negative...
                {
                    //RightBar.PropertiesEntry4ByteLNegative.Text = "Positive Only";
                    RightBar.PropertiesEntry4ByteLNegative.IsEnabled = false;
                }
            }
            catch
            {
                RightBar.PropertiesEntry4ByteBNegative.Text = "END OF FILE";
                RightBar.PropertiesEntry4ByteLNegative.Text = "END OF FILE";
            }

            DTEData.DTEXaml.RightBar.TheCrossReference.FillLearnBox(DTEData);
        }

        //public async void UpdateEntryHexProperties(DataTableEditorData DTEData) 
        //{

        //    Entry EntryClass = DTEData.EntryClass;
        //    DTRightBar RightBar = DTEData.EditorRightBar; 

        //    RightBar.PropertiesEntryHexAddressTextbox.Text = (DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X2");

        //    /////////////////////If project is loaded or not/////////////////////                        
        //    if (DTEData.WorkshopData.IsProjectLoaded == false) 
        //    {
        //        //If NO project
        //        RightBar.PropertiesEntry1Byte.Text = "";
        //        RightBar.PropertiesEntryHex1Byte.Text = "";
        //        RightBar.PropertiesEntry2ByteB.Text = "";
        //        RightBar.PropertiesEntryHex2ByteB.Text = "";
        //        RightBar.PropertiesEntry2ByteL.Text = "";
        //        RightBar.PropertiesEntryHex2ByteL.Text = "";
        //        RightBar.PropertiesEntry4ByteB.Text = "";
        //        RightBar.PropertiesEntryHex4ByteB.Text = "";
        //        RightBar.PropertiesEntry4ByteL.Text = "";
        //        RightBar.PropertiesEntryHex4ByteL.Text = "";
        //        RightBar.PropertiesEntry1ByteNegative.Text = "";
        //        RightBar.PropertiesEntry2ByteBNegative.Text = "";
        //        RightBar.PropertiesEntry2ByteLNegative.Text = "";
        //        RightBar.PropertiesEntry4ByteBNegative.Text = "";
        //        RightBar.PropertiesEntry4ByteLNegative.Text = "";

        //        RightBar.PropertiesEntry1Byte.IsEnabled = false;
        //        RightBar.PropertiesEntryHex1Byte.IsEnabled = false;
        //        RightBar.PropertiesEntry2ByteB.IsEnabled = false;
        //        RightBar.PropertiesEntryHex2ByteB.IsEnabled = false;
        //        RightBar.PropertiesEntry2ByteL.IsEnabled = false;
        //        RightBar.PropertiesEntryHex2ByteL.IsEnabled = false;
        //        RightBar.PropertiesEntry4ByteB.IsEnabled = false;
        //        RightBar.PropertiesEntryHex4ByteB.IsEnabled = false;
        //        RightBar.PropertiesEntry4ByteL.IsEnabled = false;
        //        RightBar.PropertiesEntryHex4ByteL.IsEnabled = false;
        //        RightBar.PropertiesEntry1ByteNegative.IsEnabled = false;
        //        RightBar.PropertiesEntry2ByteBNegative.IsEnabled = false;
        //        RightBar.PropertiesEntry2ByteLNegative.IsEnabled = false;
        //        RightBar.PropertiesEntry4ByteBNegative.IsEnabled = false;
        //        RightBar.PropertiesEntry4ByteLNegative.IsEnabled = false;
        //        return;

        //    }
        //    else if (DTEData.WorkshopData.IsProjectLoaded == true)
        //    {
        //        //If there IS a project!
        //        RightBar.PropertiesEntry1Byte.IsEnabled = true;
        //        RightBar.PropertiesEntryHex1Byte.IsEnabled = true;
        //        RightBar.PropertiesEntry2ByteB.IsEnabled = true;
        //        RightBar.PropertiesEntryHex2ByteB.IsEnabled = true;
        //        RightBar.PropertiesEntry2ByteL.IsEnabled = true;
        //        RightBar.PropertiesEntryHex2ByteL.IsEnabled = true;
        //        RightBar.PropertiesEntry4ByteB.IsEnabled = true;
        //        RightBar.PropertiesEntryHex4ByteB.IsEnabled = true;
        //        RightBar.PropertiesEntry4ByteL.IsEnabled = true;
        //        RightBar.PropertiesEntryHex4ByteL.IsEnabled = true;
        //        RightBar.PropertiesEntry1ByteNegative.IsEnabled = true;
        //        RightBar.PropertiesEntry2ByteBNegative.IsEnabled = true;
        //        RightBar.PropertiesEntry2ByteLNegative.IsEnabled = true;
        //        RightBar.PropertiesEntry4ByteBNegative.IsEnabled = true;
        //        RightBar.PropertiesEntry4ByteLNegative.IsEnabled = true;
        //    }


        //    /////////////////////Data Analyzer/////////////////////
        //    RightBar.PropertiesEntry1Byte.Text = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
        //    RightBar.PropertiesEntryHex1Byte.Text = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("X2");

        //    try
        //    {
        //        ushort value2 = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
        //        ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness

        //        RightBar.PropertiesEntry2ByteL.Text = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
        //        RightBar.PropertiesEntryHex2ByteL.Text = swappedValue2.ToString("X4"); // Convert the swapped value4 to a string using the desired format                
        //        RightBar.PropertiesEntry2ByteB.Text = swappedValue2.ToString("D"); // Convert the swapped value4 to a string using the desired format 
        //        RightBar.PropertiesEntryHex2ByteB.Text = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X4");

        //    }
        //    catch
        //    {
        //        RightBar.PropertiesEntry2ByteL.Text = "END OF FILE";
        //        RightBar.PropertiesEntryHex2ByteL.Text = "END OF FILE";
        //        RightBar.PropertiesEntry2ByteB.Text = "END OF FILE";
        //        RightBar.PropertiesEntryHex2ByteB.Text = "END OF FILE";                
        //    }

        //    try
        //    {
        //        uint value = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
        //        byte[] valueBytes = BitConverter.GetBytes(value);
        //        Array.Reverse(valueBytes);
        //        uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);

        //        RightBar.PropertiesEntry4ByteB.Text = swappedValue.ToString("D");
        //        RightBar.PropertiesEntryHex4ByteB.Text = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X8");

        //        RightBar.PropertiesEntry4ByteL.Text = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (DTEData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");  
        //        RightBar.PropertiesEntryHex4ByteL.Text = swappedValue.ToString("X8");
        //    }
        //    catch
        //    {
        //        RightBar.PropertiesEntry4ByteB.Text = "END OF FILE";
        //        RightBar.PropertiesEntryHex4ByteB.Text = "END OF FILE";
        //        RightBar.PropertiesEntry4ByteL.Text = "END OF FILE";
        //        RightBar.PropertiesEntryHex4ByteL.Text = "END OF FILE";
        //    }


        //    //Starting here is for attempting to read possible negative values. 
        //    // === 1 Byte (Signed) ===
        //    try
        //    {
        //        sbyte signedByte = (sbyte)DTEData.DataTable.FileDataTable.FileBytes[
        //            DTEData.DataTable.DataTableStart +
        //            (DTEData.TableRowIndex * EntryClass.DataTableRowSize) +
        //            EntryClass.RowOffset];
        //        RightBar.PropertiesEntry1ByteNegative.Text = signedByte < 0 ? signedByte.ToString("D") : "";

        //        if (RightBar.PropertiesEntry1ByteNegative.Text == "") //If it can't be read as a negative...
        //        {
        //            //RightBar.PropertiesEntry1ByteNegative.Text = "Positive Only";
        //            RightBar.PropertiesEntry1ByteNegative.IsEnabled = false;
        //        }
        //    }
        //    catch
        //    {
        //        RightBar.PropertiesEntry1ByteNegative.Text = "END OF FILE";
        //    }

        //    // === 2 Byte (Signed) ===
        //    try
        //    {
        //        int offset2 = DTEData.DataTable.DataTableStart +
        //                      (DTEData.TableRowIndex * EntryClass.DataTableRowSize) +
        //                      EntryClass.RowOffset;

        //        short s2L = BitConverter.ToInt16(DTEData.DataTable.FileDataTable.FileBytes, offset2);
        //        RightBar.PropertiesEntry2ByteLNegative.Text = s2L < 0 ? s2L.ToString("D") : "";


        //        byte[] signedBytes2B = DTEData.DataTable.FileDataTable.FileBytes
        //            .Skip(offset2).Take(2).ToArray();
        //        Array.Reverse(signedBytes2B);
        //        short swappedS2B = BitConverter.ToInt16(signedBytes2B, 0);                
        //        RightBar.PropertiesEntry2ByteBNegative.Text = swappedS2B < 0 ? swappedS2B.ToString("D") : ""; 



        //        if (RightBar.PropertiesEntry2ByteBNegative.Text == "") //If it can't be read as a negative...
        //        {
        //            //RightBar.PropertiesEntry2ByteBNegative.Text = "Positive Only";
        //            RightBar.PropertiesEntry2ByteBNegative.IsEnabled = false;
        //        }
        //        if (RightBar.PropertiesEntry2ByteLNegative.Text == "") //If it can't be read as a negative...
        //        {
        //            //RightBar.PropertiesEntry2ByteLNegative.Text = "Positive Only";
        //            RightBar.PropertiesEntry2ByteLNegative.IsEnabled = false;
        //        }
        //    }
        //    catch
        //    {
        //        RightBar.PropertiesEntry2ByteBNegative.Text = "END OF FILE";
        //        RightBar.PropertiesEntry2ByteLNegative.Text = "END OF FILE";
        //    }

        //    // === 4 Byte (Signed) ===
        //    try
        //    {
        //        int offset4 = DTEData.DataTable.DataTableStart +
        //                      (DTEData.TableRowIndex * EntryClass.DataTableRowSize) +
        //                      EntryClass.RowOffset;

        //        int s4L = BitConverter.ToInt32(DTEData.DataTable.FileDataTable.FileBytes, offset4);
        //        RightBar.PropertiesEntry4ByteLNegative.Text = s4L < 0 ? s4L.ToString("D") : "";


        //        byte[] signedBytes4B = DTEData.DataTable.FileDataTable.FileBytes
        //            .Skip(offset4).Take(4).ToArray();
        //        Array.Reverse(signedBytes4B);
        //        int swappedS4B = BitConverter.ToInt32(signedBytes4B, 0);
        //        RightBar.PropertiesEntry4ByteBNegative.Text = swappedS4B < 0 ? swappedS4B.ToString("D") : "";


        //        if (RightBar.PropertiesEntry4ByteBNegative.Text == "") //If it can't be read as a negative...
        //        {
        //            //RightBar.PropertiesEntry4ByteBNegative.Text = "Positive Only";
        //            RightBar.PropertiesEntry4ByteBNegative.IsEnabled = false;
        //        }
        //        if (RightBar.PropertiesEntry4ByteLNegative.Text == "") //If it can't be read as a negative...
        //        {
        //            //RightBar.PropertiesEntry4ByteLNegative.Text = "Positive Only";
        //            RightBar.PropertiesEntry4ByteLNegative.IsEnabled = false;
        //        }
        //    }
        //    catch
        //    {
        //        RightBar.PropertiesEntry4ByteBNegative.Text = "END OF FILE";
        //        RightBar.PropertiesEntry4ByteLNegative.Text = "END OF FILE";
        //    }

        //    DTEData.DTEXaml.RightBar.TheCrossReference.FillLearnBox(DTEData);
        //}





    } //End of Class
}
