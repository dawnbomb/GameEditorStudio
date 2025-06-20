using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Net;
using System.Windows.Navigation;
using static GameEditorStudio.Entry;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static GameEditorStudio.EntryTypeMenu;
using Windows.Gaming.Preview.GamesEnumeration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using TabItem = System.Windows.Controls.TabItem;
using System.Windows.Documents;
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
                
        public void LoadEntry(Workshop TheWorkshop, Editor EditorClass, Entry EntryClass) //LOADING ISN'T TAKING SIGN INTO CONSIDERATION!!!
        {
            
            //This method simply takes the byte(s) from MemoryFile the entry controls,
            //converts them from Hex to Decimal, and loads that number into EditorClass.EntryByteDecimal.
            //Afterward (Down below) it loads the the entry with that information.

            if (TheWorkshop.IsPreviewMode == true) { return; }

            if (EntryClass.Endianness == "1") 
            { 
                EntryClass.EntryByteDecimal = EditorClass.StandardEditorData.FileDataTable.FileBytes[EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D"); 
            }
            else if (EntryClass.Endianness == "2B")
            {
                EntryClass.EntryByteDecimal = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
            }
            else if (EntryClass.Endianness == "4B")
            {
                EntryClass.EntryByteDecimal = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
            }
            else if (EntryClass.Endianness == "2L") 
            {
                ushort value2 = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
                EntryClass.EntryByteDecimal = swappedValue2.ToString("D");
            }
            else if (EntryClass.Endianness == "4L") 
            {
                uint value = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Reverse(valueBytes);
                uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
                EntryClass.EntryByteDecimal = swappedValue.ToString("D");
            }
            



            if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox) { LoadNumberBox(EntryClass); }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox)  { LoadCheckBox(EntryClass); }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag)   { LoadBitFlag(EntryClass); }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu)      { LoadMenu(EntryClass, TheWorkshop); }
        }

        public void SaveEntry(Editor EditorClass, Entry EntryClass)
        {
            

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
                if (EntryClass.IsEntryHidden == false && EntryClass.IsTextInUse == false ) 
                {
                    //If 1/2/4/r2/r4 Bytes...  

                    //NOTE: I now ALWAYS save as an unsigned value. IE, EntryByteDecimal may NEVER be a negative (Not anymore). 
                    if (EntryClass.Endianness == "1")  //This is saving 1 Byte Size?   //First 1 byte save
                    {
                        //Thing that loads       -----------------The Hex GameFile---------------------------  ---Starting Byte--- --The Tree--   --Row Size----  --Offset into a row-- --To Decimal--               
                        Byte.TryParse(EntryClass.EntryByteDecimal, out byte value8);
                        { ByteManager.ByteWriter(value8, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); }


                    }
                    else if (EntryClass.Endianness == "2L")
                    {
                        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                        value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                        { ByteManager.ByteWriter(value16, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save


                    }
                    else if (EntryClass.Endianness == "4L")
                    {
                        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                        byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                        Array.Reverse(valueBytes); // Swap the endianness
                        value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                        { ByteManager.ByteWriter(value32, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save

                    }
                    else if (EntryClass.Endianness == "2B")
                    {
                        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                        { ByteManager.ByteWriter(value16, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save

                    }
                    else if (EntryClass.Endianness == "4B")
                    {
                        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                        { ByteManager.ByteWriter(value32, EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save

                    }
                }



                //if (EntryClass.HideEntry == false)
                //{
                //    //If 1/2/4/r2/r4 Bytes...  

                //    if (EntryClass.Endianness == "1")  //This is saving 1 Byte Size?   //First 1 byte save
                //    {
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed) //&& EntryClass.NewSubType == EntrySubTypes.NumberBox
                //        {
                //            sbyte signedValue = sbyte.Parse(EntryClass.EntryByteDecimal); // Directly parse the string to a signed byte (sbyte)
                //            byte value8 = (byte)signedValue; // Convert the signed byte to an unsigned byte properly.
                //            ByteManager.ByteWriter(value8, EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                //        }
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned)
                //        {
                //            //Thing that loads       -----------------The Hex GameFile---------------------------  ---Starting Byte--- --The Tree--   --Row Size----  --Offset into a row-- --To Decimal--               
                //            Byte.TryParse(EntryClass.EntryByteDecimal, out byte value8);
                //            { ByteManager.ByteWriter(value8, EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); }
                //        }


                //    }
                //    else if (EntryClass.Endianness == "2L")
                //    {
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned)
                //        {
                //            UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                //            value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                //            { ByteManager.ByteWriter(value16, EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save

                //        }
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                //        {
                //            short signedValue16 = Int16.Parse(EntryClass.EntryByteDecimal);
                //            byte[] bytes16 = BitConverter.GetBytes(signedValue16);
                //            Array.Reverse(bytes16); // Always reverse to ensure little-endian format regardless of system architecture
                //            ByteManager.ByteWriter(BitConverter.ToInt16(bytes16, 0), EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);

                //        }


                //    }
                //    else if (EntryClass.Endianness == "4L")
                //    {
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned)
                //        {
                //            UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                //            byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                //            Array.Reverse(valueBytes); // Swap the endianness
                //            value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                //            { ByteManager.ByteWriter(value32, EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save

                //        }
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                //        {
                //            int signedValue32 = Int32.Parse(EntryClass.EntryByteDecimal);
                //            byte[] bytes32 = BitConverter.GetBytes(signedValue32);
                //            Array.Reverse(bytes32);  // Ensures little-endian format
                //            ByteManager.ByteWriter(BitConverter.ToInt32(bytes32, 0), EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);

                //        }

                //    }
                //    else if (EntryClass.Endianness == "2B")
                //    {
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned)
                //        {
                //            UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                //            { ByteManager.ByteWriter(value16, EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 2 byte save
                //        }
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                //        {
                //            short signedValue16 = Int16.Parse(EntryClass.EntryByteDecimal);
                //            ByteManager.ByteWriter((short)IPAddress.HostToNetworkOrder(signedValue16), EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);

                //        }

                //    }
                //    else if (EntryClass.Endianness == "4B")
                //    {
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned)
                //        {
                //            UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                //            { ByteManager.ByteWriter(value32, EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset); } //First 4 byte save
                //        }
                //        if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                //        {
                //            int signedValue32 = Int32.Parse(EntryClass.EntryByteDecimal);
                //            ByteManager.ByteWriter(IPAddress.HostToNetworkOrder(signedValue32), EditorClass.SWData.FileDataTable.FileBytes, EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);

                //        }

                //    }
                //}



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


        public void LoadMenu(Entry EntryClass, Workshop TheWorkshop)
        {
            
            string UnknownValue = EntryClass.EntryByteDecimal + ": " + "ERROR!"; //text for a menu name the user hasn't assigned a name to, but is a valid value.

            if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.List)
            {
                
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile || EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                {
                    bool FoundItem = false;
                    foreach (string name in EntryClass.EntryTypeMenu.NameList)
                    {                        

                        string[] parts = name.Split(':');
                        string number = parts[0].Trim();
                        if (number == EntryClass.EntryByteDecimal)  //ComboItem.Tag
                        {
                            //item.IsSelected = true;
                            FoundItem = true;
                            EntryClass.EntryTypeMenu.ListButton.Content = name; //EntryClass.EntryByteDecimal + ": " + 
                        }
                    }
                    if (FoundItem == false)
                    {
                        EntryClass.EntryTypeMenu.ListButton.Content = UnknownValue;
                        EntryClass.EntryTypeMenu.ListButton.Foreground = Brushes.Red; //Red text for unknown value
                    }
                }
                //if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                //{
                //    bool FoundItem = false;
                //    foreach (string name in EntryClass.EntryTypeMenu.NameList)
                //    {

                //        string[] parts = name.Split(':');
                //        string number = parts[0].Trim();
                //        if (number == EntryClass.EntryByteDecimal)  //ComboItem.Tag
                //        {
                //            //item.IsSelected = true;
                //            FoundItem = true;
                //            EntryClass.EntryTypeMenu.ListButton.Content = name; //EntryClass.EntryByteDecimal + ": " + 
                //        }
                //    }
                //    if (FoundItem == false)
                //    {
                //        EntryClass.EntryTypeMenu.ListButton.Content = EntryClass.EntryByteDecimal + ": " + UnknownValue;
                //    }
                //}
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor) //|| EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile
                {
                    string theName = "";

                    if (EntryClass.EntryTypeMenu.LinkedEditor != null) 
                    {
                        int anum = Int32.Parse(EntryClass.EntryByteDecimal) - EntryClass.EntryTypeMenu.FirstNameID;
                        theName = EntryClass.EntryTypeMenu.LinkedEditor.StandardEditorData.EditorLeftDockPanel.ItemList[anum].ItemName;  //EntryClass.EntryTypeMenu.NothingNameList[Int32.Parse(EntryClass.EntryByteDecimal)];

                        EntryClass.EntryTypeMenu.ListButton.Content = EntryClass.EntryByteDecimal + ": " + theName;
                    }
                    

                    if (theName == "" || theName == null)
                    { 
                        EntryClass.EntryTypeMenu.ListButton.Content = UnknownValue;
                        EntryClass.EntryTypeMenu.ListButton.Foreground = Brushes.Red; //Red text for unknown value
                    }
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing) //|| EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile
                {
                    string theName = EntryClass.EntryTypeMenu.NothingNameList[Int32.Parse(EntryClass.EntryByteDecimal)];

                    EntryClass.EntryTypeMenu.ListButton.Content = EntryClass.EntryByteDecimal + ": " + theName;

                    if (theName == "" || theName == null)
                    { 
                        EntryClass.EntryTypeMenu.ListButton.Content = UnknownValue;
                        EntryClass.EntryTypeMenu.ListButton.Foreground = Brushes.Red; //Red text for unknown value
                    }
                }

            }
            if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.Dropdown)
            {
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile || EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile || EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor || EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
                {
                    



                    bool FoundItem = false;
                    foreach (ComboBoxItem ComboItem in EntryClass.EntryTypeMenu.Dropdown.Items) 
                    {                        
                        
                        string input = ComboItem.Content as string;
                        if (input == "" || input == null) { return; }

                        string[] parts = input.Split(':');
                        string number = parts[0].Trim();
                        // = number;

                        if (number == EntryClass.EntryByteDecimal)  //ComboItem.Tag
                        { 
                            ComboItem.IsSelected = true;
                            FoundItem = true;
                        }    

                    }
                    if (FoundItem == false)
                    {

                        ComboBoxItem FakeItem = new();
                        FakeItem.Content = UnknownValue; //found above
                        FakeItem.Foreground = Brushes.Red; //Red text for unknown value
                        EntryClass.EntryTypeMenu.Dropdown.Foreground = Brushes.Red; //Red text for unknown value
                        EntryClass.EntryTypeMenu.Dropdown.Items.Add(FakeItem);
                        FakeItem.IsSelected = true;

                        //ComboBoxItem FakeItem = new();
                        //FakeItem.Content = EntryClass.EntryByteDecimal + ": " + UnknownValue;
                        //EntryClass.EntryTypeMenu.Dropdown.Items.Add(FakeItem);
                        //FakeItem.IsSelected = true;
                    }


                    //EntryClass.EntryTypeMenu.ListButton.Content = EntryClass.EntryByteDecimal + ": " + EntryClass.EntryTypeMenu.NothingItemList[Int32.Parse(EntryClass.EntryByteDecimal)];
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor) 
                {
                    
                }

            }

        }








        //////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////Create Entrys///////////////////////////////////////







        public void CreateNumberBox(Workshop TheWorkshop, Entry EntryClass)
        {
            if (EntryClass.IsNameHidden == false) { EntryClass.EntryLabel.Visibility = Visibility.Visible; }
            // Default properties if new
            if (EntryClass.EntryTypeNumberBox == null)
            {
                EntryClass.EntryTypeNumberBox = new();
                EntryClass.EntryTypeNumberBox.NewNumberSign = EntryTypeNumberBox.TheNumberSigns.Unsigned;
            }

            // Default properties end


            TextBox NumberBox = new TextBox();
            NumberBox.Height = 25;
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

            NumberBox.PreviewMouseDown += (sender, e) =>
            {
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);
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

                        if (EntryClass == EntryClass.EntryEditor.StandardEditorData.SelectedEntry)
                        {
                            SaveEntry(EntryClass.EntryEditor, EntryClass);
                            UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);
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
            if (EntryClass.IsNameHidden == false) { EntryClass.EntryLabel.Visibility = Visibility.Visible; }

            //Default properties if new
            if (EntryClass.EntryTypeCheckBox == null)
            {
                EntryClass.EntryTypeCheckBox = new();
            }
            //Default properties end

            Button CheckBox = new Button();
            CheckBox.MinWidth = 30;
            CheckBox.Height = 28;
            CheckBox.Margin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            EntryClass.EntryDockPanel.Children.Add(CheckBox);
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                    
                }
                else if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.FalseValue.ToString())
                {
                    CheckBox.Content = EntryClass.EntryTypeCheckBox.TrueText;
                    EntryClass.EntryByteDecimal = EntryClass.EntryTypeCheckBox.TrueValue.ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }

                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

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
            EntryClass.EntryLabel.Visibility = Visibility.Collapsed;

            if (EntryClass.EntryTypeBitFlag == null)
            {
                EntryClass.EntryTypeBitFlag = new();
            }


            DockPanel BitFlags = new();
            //BitFlags.Background = Brushes.Crimson;
            int BitFlagBoxHeight = 32;
            int BitMinWidth = 33;
            var BitMargin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            var DockMargin = new Thickness(0, 3, 0, 3); // Left Top Right Bottom 
            ////////////////////////////////////////////////
            DockPanel BitFlag1 = new();
            DockPanel.SetDock(BitFlag1, Dock.Top);
            BitFlag1.Margin = DockMargin;

            Label BitFlag1Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag1Name == null) { EntryClass.EntryTypeBitFlag.BitFlag1Name = "Bit 1"; }
            BitFlag1Label.Content = EntryClass.EntryTypeBitFlag.BitFlag1Name;
            BitFlag1Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag1CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag1CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag1UncheckText.ToString())
                {
                    BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 1).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }

                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag2 = new();
            DockPanel.SetDock(BitFlag2, Dock.Top);
            BitFlag2.Margin = DockMargin;

            Label BitFlag2Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag2Name == null) { EntryClass.EntryTypeBitFlag.BitFlag2Name = "Bit 2"; }
            BitFlag2Label.Content = EntryClass.EntryTypeBitFlag.BitFlag2Name;
            BitFlag2Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag2CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);                    
                }
                else if (BitFlag2CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag2UncheckText.ToString())
                {
                    BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 2).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag3 = new();
            DockPanel.SetDock(BitFlag3, Dock.Top);
            BitFlag3.Margin = DockMargin;

            Label BitFlag3Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag3Name == null) { EntryClass.EntryTypeBitFlag.BitFlag3Name = "Bit 3"; }
            BitFlag3Label.Content = EntryClass.EntryTypeBitFlag.BitFlag3Name;
            BitFlag3Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag3CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag3CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag3UncheckText.ToString())
                {
                    BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 4).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag4 = new();
            DockPanel.SetDock(BitFlag4, Dock.Top);
            BitFlag4.Margin = DockMargin;

            Label BitFlag4Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag4Name == null) { EntryClass.EntryTypeBitFlag.BitFlag4Name = "Bit 4"; }
            BitFlag4Label.Content = EntryClass.EntryTypeBitFlag.BitFlag4Name;
            BitFlag4Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag4CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag4CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag4UncheckText.ToString())
                {
                    BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 8).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag5 = new();
            DockPanel.SetDock(BitFlag5, Dock.Top);
            BitFlag5.Margin = DockMargin;

            Label BitFlag5Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag5Name == null) { EntryClass.EntryTypeBitFlag.BitFlag5Name = "Bit 5"; }
            BitFlag5Label.Content = EntryClass.EntryTypeBitFlag.BitFlag5Name;
            BitFlag5Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag5CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag5CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag5UncheckText.ToString())
                {
                    BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 16).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag6 = new();
            DockPanel.SetDock(BitFlag6, Dock.Top);
            BitFlag6.Margin = DockMargin;

            Label BitFlag6Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag6Name == null) { EntryClass.EntryTypeBitFlag.BitFlag6Name = "Bit 6"; }
            BitFlag6Label.Content = EntryClass.EntryTypeBitFlag.BitFlag6Name;
            BitFlag6Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag6CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag6CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag6UncheckText.ToString())
                {
                    BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 32).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag7 = new();
            DockPanel.SetDock(BitFlag7, Dock.Top);
            BitFlag7.Margin = DockMargin;

            Label BitFlag7Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag7Name == null) { EntryClass.EntryTypeBitFlag.BitFlag7Name = "Bit 7"; }
            BitFlag7Label.Content = EntryClass.EntryTypeBitFlag.BitFlag7Name;
            BitFlag7Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag7CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag7CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag7UncheckText.ToString())
                {
                    BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 64).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag8 = new();
            DockPanel.SetDock(BitFlag8, Dock.Top);
            BitFlag8.Margin = DockMargin;

            Label BitFlag8Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag8Name == null) { EntryClass.EntryTypeBitFlag.BitFlag8Name = "Bit 8"; }
            BitFlag8Label.Content = EntryClass.EntryTypeBitFlag.BitFlag8Name;
            BitFlag8Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag8CheckBox = new Button();
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
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag8CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag8UncheckText.ToString())
                {
                    BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 128).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);
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
            BitFlag1.Children.Add(BitFlag1Label);
            BitFlag1.Children.Add(BitFlag1CheckBox);

            BitFlags.Children.Add(BitFlag2);
            BitFlag2.Children.Add(BitFlag2Label);
            BitFlag2.Children.Add(BitFlag2CheckBox);

            BitFlags.Children.Add(BitFlag3);
            BitFlag3.Children.Add(BitFlag3Label);
            BitFlag3.Children.Add(BitFlag3CheckBox);

            BitFlags.Children.Add(BitFlag4);
            BitFlag4.Children.Add(BitFlag4Label);
            BitFlag4.Children.Add(BitFlag4CheckBox);

            BitFlags.Children.Add(BitFlag5);
            BitFlag5.Children.Add(BitFlag5Label);
            BitFlag5.Children.Add(BitFlag5CheckBox);

            BitFlags.Children.Add(BitFlag6);
            BitFlag6.Children.Add(BitFlag6Label);
            BitFlag6.Children.Add(BitFlag6CheckBox);

            BitFlags.Children.Add(BitFlag7);
            BitFlag7.Children.Add(BitFlag7Label);
            BitFlag7.Children.Add(BitFlag7CheckBox);

            BitFlags.Children.Add(BitFlag8);
            BitFlag8.Children.Add(BitFlag8Label);
            BitFlag8.Children.Add(BitFlag8CheckBox);

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
            if (EntryClass.IsNameHidden == false) { EntryClass.EntryLabel.Visibility = Visibility.Visible; }
            //Default properties if new
            if (EntryClass.EntryTypeMenu == null)
            {
                EntryClass.EntryTypeMenu = new();
            }


            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile)
            {

            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
            {

            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
            {

            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing) 
            {
                
            }

            if (EntryClass.EntryTypeMenu.NothingNameList != null)
            {//if this already exists, then it stops being a menu, then byte count changes, then goes back to this, we need a "Update list size" check basically.
                if (EntryClass.Bytes == 1)
                {
                    string[] items = EntryClass.EntryTypeMenu.NothingNameList;
                    Array.Resize(ref items, 256);
                    EntryClass.EntryTypeMenu.NothingNameList = items;
                    EntryClass.EntryTypeMenu.ListSize = 256;
                }
                if (EntryClass.Bytes == 2)
                {
                    string[] items = EntryClass.EntryTypeMenu.NothingNameList;
                    Array.Resize(ref items, 65536);
                    EntryClass.EntryTypeMenu.NothingNameList = items;
                    EntryClass.EntryTypeMenu.ListSize = 65536;
                }
            }

            if (EntryClass.EntryTypeMenu.NothingNameList == null)
            {
                if (EntryClass.Bytes == 1)
                {
                    EntryClass.EntryTypeMenu.NothingNameList = new string[256];
                    EntryClass.EntryTypeMenu.ListSize = 256;
                }
                if (EntryClass.Bytes == 2)
                {
                    EntryClass.EntryTypeMenu.NothingNameList = new string[65536];
                    EntryClass.EntryTypeMenu.ListSize = 65536;
                }
                //ListItems = new string[256],
            }


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
            Button Button = new();
            Button.MinWidth = 100;
            Button.Height = 24;
            Button.Margin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            Button.HorizontalAlignment = HorizontalAlignment.Right;
            Button.HorizontalContentAlignment = HorizontalAlignment.Left;
            Button.Padding = new Thickness(4,0,0,0);
            if (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true)
            {
                Button.IsEnabled = false;
            }
            EntryClass.EntryDockPanel.Children.Add(Button);           
            

            EntryClass.EntryTypeMenu.ListButton = Button;
            Button.Click += (sender, e) =>
            {
                if (TheWorkshop.IsPreviewMode == true) { return; }

                TheWorkshop.EntryClass = EntryClass;
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

                TheWorkshop.EntryListBox.SelectionChanged -= TheWorkshop.EntryListBox_SelectionChanged; // Remove event handler    
                TheWorkshop.EntryListBox.Items.Clear(); // Clear items
                
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile) 
                {
                    for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                    {
                        byte[] thebytes = EntryClass.EntryTypeMenu.GameFile.FileBytes;

                        int thetextsize = EntryClass.EntryTypeMenu.CharCount;
                        int thetablestart = EntryClass.EntryTypeMenu.Start;
                        int therowsize = EntryClass.EntryTypeMenu.RowSize;

                        byte[] bytes = new byte[thetextsize];
                        for (int rowindex = 0; rowindex < thetextsize; rowindex++)
                        {
                            bytes[rowindex] = thebytes[thetablestart + (i * therowsize) + rowindex]; //item.itemindex * 
                        }


                        CharacterSetManager Decoder = new();
                        string TheText = Decoder.DecodeReturn(EntryClass.EntryTypeMenu.CharacterSet, bytes);

                        ListViewItem ListItem = new();
                        ListItem.Content = (i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + TheText;
                        TheWorkshop.EntryListBox.Items.Add(ListItem);
                    }
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
                {
                    string fullText = Encoding.UTF8.GetString(EntryClass.EntryTypeMenu.GameFile.FileBytes);
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                    {
                        byte[] thebytes = EntryClass.EntryTypeMenu.GameFile.FileBytes;

                        int index = i + EntryClass.EntryTypeMenu.Start;
                        string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";



                        ListViewItem ListItem = new();
                        ListItem.Content = (i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + lineText;
                        TheWorkshop.EntryListBox.Items.Add(ListItem);


                    }
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor) 
                {
                    

                    for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                    {
                        ListViewItem ListItem = new();

                        string asdf = "ERROR!";

                        if (EntryClass.EntryTypeMenu.LinkedEditor != null)
                        {
                            foreach (ItemInfo itemInfo in EntryClass.EntryTypeMenu.LinkedEditor.StandardEditorData.EditorLeftDockPanel.ItemList)
                            {
                                if (itemInfo.ItemIndex == i)
                                {
                                    if (itemInfo.IsFolder == false)
                                    {
                                        asdf = itemInfo.ItemName;
                                        break;
                                    }
                                }
                            }

                            ListItem.Content = (i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + asdf; //EntryClass.EntryTypeMenu.NothingNameList[i]
                            TheWorkshop.EntryListBox.Items.Add(ListItem);
                        }
                        else //This is if the editor is missing or has been deleted, so the program still kinda functions and doesn't crash.
                        {                           

                            ListItem.Content = (i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + asdf; //EntryClass.EntryTypeMenu.NothingNameList[i]
                            ListItem.Foreground = Brushes.Red; //For error message
                            TheWorkshop.EntryListBox.Items.Add(ListItem);
                        }

                        
                    }
                }
                if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
                {
                    for (int i = 0; i < EntryClass.EntryTypeMenu.NothingNameList.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(EntryClass.EntryTypeMenu.NothingNameList[i]))
                        {
                            ListViewItem ListItem = new();
                            ListItem.Content = i + ": " + EntryClass.EntryTypeMenu.NothingNameList[i];
                            TheWorkshop.EntryListBox.Items.Add(ListItem);
                        }
                    }
                }
                TheWorkshop.EntryListBox.SelectionChanged += TheWorkshop.EntryListBox_SelectionChanged; // Re-attach event handler  

                
                foreach (TabItem tabItem in TheWorkshop.MainTabControl.Items) //Open the list menu   //I GOT A WEIRD ERROR DOING UNRELATED STUFF, AND ADDED "USING TABITEM" TO THE TOP, AND IT FIXED IT. I HAVE NO IDEA WHY.
                {
                    if (tabItem.Header != null && tabItem.Header.ToString() == "Lists")
                    {
                        if (TheWorkshop.MainTabControl.SelectedItem is TabItem currentTab) //store current tab to return to it after list.
                        {
                            if (currentTab.Header.ToString() != "Lists") 
                            {
                                TheWorkshop.PreviousTabName = currentTab.Header.ToString();
                            }
                            
                        }
                        tabItem.IsSelected = true;
                        tabItem.Focus();                        
                        break;
                    }
                }


            };

            if (TheWorkshop.IsPreviewMode == true) { return; }

            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile) 
            {                
                EntryClass.EntryTypeMenu.NameList.Clear();

                for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                {
                    byte[] thebytes = EntryClass.EntryTypeMenu.GameFile.FileBytes;

                    int thetextsize = EntryClass.EntryTypeMenu.CharCount;
                    int thetablestart = EntryClass.EntryTypeMenu.Start;
                    int therowsize = EntryClass.EntryTypeMenu.RowSize;

                    byte[] bytes = new byte[thetextsize];
                    for (int rowindex = 0; rowindex < thetextsize; rowindex++)
                    {
                        bytes[rowindex] = thebytes[thetablestart + (i * therowsize) + rowindex]; //item.itemindex * 
                    }


                    CharacterSetManager Decoder = new();
                    string TheText = Decoder.DecodeReturn(EntryClass.EntryTypeMenu.CharacterSet, bytes);
                    
                    EntryClass.EntryTypeMenu.NameList.Add((i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + TheText);
                }
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
            {               

                string fullText = Encoding.UTF8.GetString(EntryClass.EntryTypeMenu.GameFile.FileBytes);
                string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                EntryClass.EntryTypeMenu.NameList.Clear();

                for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                {
                    byte[] thebytes = EntryClass.EntryTypeMenu.GameFile.FileBytes;

                    int index = i; //+ EntryClass.EntryTypeMenu.Start
                    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";

                    EntryClass.EntryTypeMenu.NameList.Add((i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + lineText);
                }



            }

            if (TheWorkshop.IsPreviewMode == true) { Button.IsEnabled = false; }
        }

        public void CreateDropDown(Entry EntryClass, Workshop Workshop)
        {
            ComboBox comboBox = new();
            comboBox.MinWidth = 100;
            comboBox.Margin = new Thickness(0, 3, 3, 3); // Left Top Right Bottom 

            EntryClass.EntryTypeMenu.Dropdown = comboBox;
            EntryClass.EntryDockPanel.Children.Add(comboBox);

            if (Workshop.IsPreviewMode == true) { return; }

            
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.DataFile)
            {
                for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                {
                    ComboBoxItem comboBoxItem = new();
                    byte[] thebytes = EntryClass.EntryTypeMenu.GameFile.FileBytes;



                    int thetextsize = EntryClass.EntryTypeMenu.CharCount;
                    int thetablestart = EntryClass.EntryTypeMenu.Start;
                    int therowsize = EntryClass.EntryTypeMenu.RowSize;

                    byte[] bytes = new byte[thetextsize];
                    for (int rowindex = 0; rowindex < thetextsize; rowindex++)
                    {
                        bytes[rowindex] = thebytes[thetablestart + (i * therowsize) + rowindex]; //item.itemindex * 
                    }


                    CharacterSetManager Decoder = new();
                    string TheText = Decoder.DecodeReturn(EntryClass.EntryTypeMenu.CharacterSet, bytes);




                    comboBoxItem.Content = (i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + TheText; //EntryClass.EntryTypeMenu.NothingNameList[i]
                    comboBox.Items.Add(comboBoxItem);
                }
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.TextFile)
            {
                string fullText = Encoding.UTF8.GetString(EntryClass.EntryTypeMenu.GameFile.FileBytes);
                string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                //int LineCount = Math.Min(lines.Length, 255);

                for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                {
                    ComboBoxItem comboBoxItem = new();
                    byte[] thebytes = EntryClass.EntryTypeMenu.GameFile.FileBytes;

                    int index = i + EntryClass.EntryTypeMenu.Start;
                    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";

                    



                    comboBoxItem.Content = (i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + lineText; //EntryClass.EntryTypeMenu.NothingNameList[i]
                    comboBox.Items.Add(comboBoxItem);
                }
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Editor)
            {

                if (EntryClass.EntryTypeMenu.LinkedEditor == null) { return; } //This is if the editor it's looking for doesn't exist, didn't load, etc. 

                for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                {
                    ComboBoxItem comboBoxItem = new();

                    string asdf = "ERROR";

                    foreach (ItemInfo itemInfo in EntryClass.EntryTypeMenu.LinkedEditor.StandardEditorData.EditorLeftDockPanel.ItemList)                     
                    {
                        if (itemInfo.ItemIndex == i) 
                        {
                            if (itemInfo.IsFolder == false) 
                            {
                                asdf = itemInfo.ItemName;
                                break;
                            }
                        }
                    }



                    comboBoxItem.Content = (i + EntryClass.EntryTypeMenu.FirstNameID) + ": " + asdf; //EntryClass.EntryTypeMenu.NothingNameList[i]
                    comboBox.Items.Add(comboBoxItem);
                }

                
            }
            if (EntryClass.EntryTypeMenu.LinkType == EntryTypeMenu.LinkTypes.Nothing)
            {
                for (int i = 0; i < EntryClass.EntryTypeMenu.NothingNameList.Length; i++)
                {
                    if (!string.IsNullOrEmpty(EntryClass.EntryTypeMenu.NothingNameList[i]))
                    {
                        ComboBoxItem comboBoxItem = new();
                        comboBoxItem.Content = i + ": " + EntryClass.EntryTypeMenu.NothingNameList[i];
                        comboBox.Items.Add(comboBoxItem);

                    }
                }
            }



            comboBox.DropDownClosed += (sender, e) =>
            {
                ComboBoxItem comboItem = comboBox.SelectedItem as ComboBoxItem;
                if (comboItem == null) { return; }

                string input = comboItem.Content as string;
                if (input == "" || input == null) { return; }

                string[] parts = input.Split(':');
                string number = parts[0].Trim();
                EntryClass.EntryByteDecimal = number;

                SaveEntry(EntryClass.EntryEditor, EntryClass);
                UpdateEntryProperties(Workshop, EntryClass.EntryEditor);
            };


            if (Workshop.IsPreviewMode == true) { comboBox.IsEnabled = false; }

        }



        ///////////////////////////////////////////////////END OF ENTRYS//////////////////////////////////////////////////////////














        //////////////////////////////////////////////START OF ENTRY TYPE SWAP////////////////////////////////////////////////////


        public void EntryChange(WorkshopData Database, EntrySubTypes NewEntryType, Workshop TheWorkshop, Entry EntryClass)
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




            if (NewEntryType == Entry.EntrySubTypes.NumberBox) //Step X: Create new Entry Module using Entry.ByteD and any other data relevant to this.
            {
                CreateNumberBox(TheWorkshop, EntryClass);
                LoadNumberBox(EntryClass);
                EntryClass.NewSubType = Entry.EntrySubTypes.NumberBox;

            }


            if (NewEntryType == Entry.EntrySubTypes.CheckBox)
            {
                CreateCheckBox(TheWorkshop, EntryClass);
                LoadCheckBox(EntryClass);
                EntryClass.NewSubType = Entry.EntrySubTypes.CheckBox;
            }



            if (NewEntryType == Entry.EntrySubTypes.BitFlag)
            {
                CreateBitFlag(TheWorkshop, EntryClass);
                LoadBitFlag(EntryClass);
                EntryClass.NewSubType = Entry.EntrySubTypes.BitFlag;
            }



            if (NewEntryType == Entry.EntrySubTypes.Menu)
            {
                CreateMenu(EntryClass, TheWorkshop);
                LoadMenu(EntryClass, TheWorkshop);
                EntryClass.NewSubType = Entry.EntrySubTypes.Menu;
            }

            TheWorkshop.UpdateSymbology(EntryClass); //here because this method oddly doesn't also update entry properties?
        }




        public void EntryBecomeActive(Entry EntryClass) 
        {
            Editor EditorClass = EntryClass.EntryEditor; 

            Window parentWindow = Window.GetWindow(EntryClass.EntryDockPanel);

            if (EditorClass.Workshop.ListTab.IsSelected == true) 
            {
                EditorClass.Workshop.GeneralTab.IsSelected = true;
                
            }
            

            TabControl tabControlProp = parentWindow.FindName("EntryElementProperties") as TabControl;
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

            Entry PreviousEntry = EditorClass.StandardEditorData.SelectedEntry;
            EditorClass.StandardEditorData.SelectedEntry = EntryClass; //Note: This MUST be here, After clear EntryClass color, and before set EntryClass Color.  

            EntryStyleUpdate(PreviousEntry);            
            EntryStyleUpdate(EntryClass);

            

        }

        public void EntryStyleUpdate(Entry EntryClass) 
        {
            Entry TheSelectedEntry = EntryClass.EntryEditor.StandardEditorData.SelectedEntry;

            if (EntryClass != TheSelectedEntry) 
            {
                if (EntryClass.IsEntryHidden == false && EntryClass.IsTextInUse == false)
                {                    
                    EntryClass.EntryBorder.Style = (Style)Application.Current.Resources["EntryStyle"];
                }
                else 
                {
                    EntryClass.EntryBorder.Style = (Style)Application.Current.Resources["HiddenEntryStyle"];
                }                
            }


            if (EntryClass == TheSelectedEntry)
            {
                if (EntryClass.IsEntryHidden == false && EntryClass.IsTextInUse == false)
                {
                    EntryClass.EntryBorder.Style = (Style)Application.Current.Resources["SelectedEntryStyle"];
                }
                else
                {
                    EntryClass.EntryBorder.Style = (Style)Application.Current.Resources["HiddenSelectedEntryStyle"];
                }
            }
            
        }


        public void UpdateEntryProperties(Workshop TheWorkshop ,Editor EditorClass) //very similuar, but also happens when treeview item selection changes.
        {
            if (TheWorkshop.IsPreviewMode == true) { return; }

            Entry EntryClass = EditorClass.StandardEditorData.SelectedEntry;

            

            LibraryMan.GotoGeneralEntry(TheWorkshop);
            TheWorkshop.PropertiesNameBox.Text = EntryClass.Name;
            TheWorkshop.EditorClass = EntryClass.EntryEditor;
            TheWorkshop.CategoryClass = EntryClass.EntryRow;
            TheWorkshop.ColumnClass = EntryClass.EntryColumn;
            TheWorkshop.EntryClass = EntryClass;


            



            /////////////////////Data Analyzer/////////////////////
            TheWorkshop.PropertiesEntry1Byte.Text = EditorClass.StandardEditorData.FileDataTable.FileBytes[EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
            TheWorkshop.PropertiesEntryHex1Byte.Text = EditorClass.StandardEditorData.FileDataTable.FileBytes[EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("X2");

            TheWorkshop.PropertiesEntry1Byte.Text = EntryClass.EntryByteDecimal;


            try 
            {
                TheWorkshop.PropertiesEntry2ByteB.Text = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                TheWorkshop.PropertiesEntryHex2ByteB.Text = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X4");

                ushort value2 = BitConverter.ToUInt16(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
                TheWorkshop.PropertiesEntry2ByteL.Text = swappedValue2.ToString("D"); // Convert the swapped value4 to a string using the desired format
                TheWorkshop.PropertiesEntryHex2ByteL.Text = swappedValue2.ToString("X4"); // Convert the swapped value4 to a string using the desired format
            } 
            catch
            {
                TheWorkshop.PropertiesEntry2ByteB.Text = "END OF FILE";
                TheWorkshop.PropertiesEntryHex2ByteB.Text = "END OF FILE";
                TheWorkshop.PropertiesEntry2ByteL.Text = "END OF FILE";
                TheWorkshop.PropertiesEntryHex2ByteL.Text = "END OF FILE";
            }
            
            try
            {
                TheWorkshop.PropertiesEntry4ByteB.Text = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                TheWorkshop.PropertiesEntryHex4ByteB.Text = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("X8");

                uint value = BitConverter.ToUInt32(EditorClass.StandardEditorData.FileDataTable.FileBytes, EditorClass.StandardEditorData.DataTableStart + (EditorClass.StandardEditorData.TableRowIndex * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Reverse(valueBytes);
                uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
                TheWorkshop.PropertiesEntry4ByteL.Text = swappedValue.ToString("D");
                TheWorkshop.PropertiesEntryHex4ByteL.Text = swappedValue.ToString("X8");
            }
            catch
            {
                TheWorkshop.PropertiesEntry4ByteB.Text = "END OF FILE";
                TheWorkshop.PropertiesEntryHex4ByteB.Text = "END OF FILE";
                TheWorkshop.PropertiesEntry4ByteL.Text = "END OF FILE";
                TheWorkshop.PropertiesEntryHex4ByteL.Text = "END OF FILE";
            }



            /////////////////////Checkboxes and Bitflags/////////////////////



            //The Editor.SelectedEntry 







            //////////////////////////////////////Entry Data / Various Dropdown Menus//////////////////////////////////////////


            if (EntryClass.IsNameHidden == true) 
            {
                TheWorkshop.HideNameCheckbox.IsChecked = true;
            }
            else 
            {
                TheWorkshop.HideNameCheckbox.IsChecked = false;
            }

            if (EntryClass.IsEntryHidden == true)
            {
                TheWorkshop.HideEntryCheckbox.IsChecked = true;
            }
            else
            {
                TheWorkshop.HideEntryCheckbox.IsChecked = false;
            }

            //Entry.EntrySubTypes.NumberBox

            string FindEntryType = EntryClass.NewSubType.ToString();  //Entry Type Dropdown Menu.
            foreach (ComboBoxItem item in TheWorkshop.PropertiesEntryType.Items)
            {
                if (item.Content.ToString() == FindEntryType)
                {
                    TheWorkshop.PropertiesEntryType.SelectedItem = item;
                    break;
                }
            }
            //string FindEntryType = EntryClass.SubType;  //Entry Type Dropdown Menu.
            //foreach (ComboBoxItem item in TheWorkshop.PropertiesEntryType.Items)
            //{
            //    if (item.Content.ToString() == FindEntryType)
            //    {
            //        TheWorkshop.PropertiesEntryType.SelectedItem = item;
            //        break;
            //    }
            //}


            string FindEntryByteSize = "Dummy"; //Entry Size Dropdown Menu.
            if (EntryClass.Endianness == "1") { FindEntryByteSize = "1 Byte"; } 
            else if (EntryClass.Endianness == "2L") { FindEntryByteSize = "2 Bytes Little Endian"; }
            else if (EntryClass.Endianness == "4L") { FindEntryByteSize = "4 Bytes Little Endian"; }
            else if (EntryClass.Endianness == "2B") { FindEntryByteSize = "2 Bytes Big Endian"; }
            else if (EntryClass.Endianness == "4B") { FindEntryByteSize = "4 Bytes Big Endian"; }
            foreach (ComboBoxItem item in TheWorkshop.PropertiesEntryByteSizeComboBox.Items)
            {
                if (item.Content.ToString() == FindEntryByteSize)
                {
                    TheWorkshop.PropertiesEntryByteSizeComboBox.SelectedItem = item;
                    break;
                }
            }







            //////////////////////////////////////Settings Per Entry Type//////////////////////////////////////////
            TheWorkshop.DropdownMenuType.IsEnabled = false;  //failsafe


            if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox)
            {
                
                if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Unsigned) { TheWorkshop.NumberboxSignCheckbox.IsChecked = false; }
                if (EntryClass.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed) { TheWorkshop.NumberboxSignCheckbox.IsChecked = true; }
                
            }




            if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox) 
            {
                TheWorkshop.PropertiesEntryCheckText.Text = EntryClass.EntryTypeCheckBox.TrueText;
                TheWorkshop.PropertiesEntryUncheckText.Text = EntryClass.EntryTypeCheckBox.FalseText;

            }


            if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag) 
            {
                TheWorkshop.PropertiesEntryBitFlag1Name.Text = EntryClass.EntryTypeBitFlag.BitFlag1Name;
                TheWorkshop.PropertiesEntryBitFlag2Name.Text = EntryClass.EntryTypeBitFlag.BitFlag2Name;
                TheWorkshop.PropertiesEntryBitFlag3Name.Text = EntryClass.EntryTypeBitFlag.BitFlag3Name;
                TheWorkshop.PropertiesEntryBitFlag4Name.Text = EntryClass.EntryTypeBitFlag.BitFlag4Name;               
                
                TheWorkshop.PropertiesEntryBitFlag5Name.Text = EntryClass.EntryTypeBitFlag.BitFlag5Name;
                TheWorkshop.PropertiesEntryBitFlag6Name.Text = EntryClass.EntryTypeBitFlag.BitFlag6Name;
                TheWorkshop.PropertiesEntryBitFlag7Name.Text = EntryClass.EntryTypeBitFlag.BitFlag7Name;
                TheWorkshop.PropertiesEntryBitFlag8Name.Text = EntryClass.EntryTypeBitFlag.BitFlag8Name;
            }


            if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu) 
            {
                TheWorkshop.DropdownMenuType.IsEnabled = true;
                if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.Dropdown) { TheWorkshop.MenuTypeItemDropdown.IsSelected = true; }
                else if (EntryClass.EntryTypeMenu.MenuType == EntryTypeMenu.MenuTypes.List) { TheWorkshop.MenuTypeItemList.IsSelected = true; }
                
                
            }

            TheWorkshop.UpdateSymbology(EntryClass);


        } //End of UpdateEntryProperties Method





    } //End of Class
}
