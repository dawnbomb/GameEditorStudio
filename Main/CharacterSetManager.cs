using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace GameEditorStudio
{
    class CharacterSetManager
    {
        //This file is a cluster fuck, i know.

        //This file handles decoding and encoding text in the Item List, and any Extra Tables.
        //It changes from Hex to english or other languages, and those languages back to hex.
        //A "Character Set" is a list of symbols associated with hex. Standard ASCII is english text (an english character set). Shift-JIS is Japanese text.

        //At some point, remake this entire file, into only "DecodeText" and "EncodeText", and have decode just return the decoded string, and encode just return the encoded bytes.
        //Maybe also later have a function that decodes a text table based on variable width table definitions? 

        public void DecodeAllItemTexts(TextTable TextTable)  //For Menus...
        {
            string Cypher = TextTable.TextTableCharacterSet;

            Encoding encoding = null; ;
            if (TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile || TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
            {

                if (Cypher == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (Cypher == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { throw new InvalidOperationException("Unsupported character set"); }
            }

            foreach (TextInfo Item in TextTable.ItemList)
            {
                if (Item.IsFolder == true) { continue; }

                if (TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile || TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                {
                    //int Padding = ItemInfo.RowEnd + 1 - ItemInfo.RowStart;
                    //string TheText = ItemInfo.ItemName.PadRight(Padding, '\0');
                    //byte[] bytes = encoding.GetBytes(TheText);
                    //for (int i = 0; i + ItemInfo.RowStart <= ItemInfo.RowEnd; i++)
                    //{
                    //    ByteManager.ByteWriter(bytes[i], DTEData.NameTable.TextTableFile.FileBytes, ItemInfo.RowStart + i);
                    //}

                    int MyTextSize = Item.RowEnd + 1 - Item.RowStart;
                    byte[] bytes = new byte[MyTextSize];
                    for (int ColumnIndex = 0; ColumnIndex < MyTextSize; ColumnIndex++)
                    {
                        bytes[ColumnIndex] = TextTable.TextTableFile.FileBytes[Item.RowStart + ColumnIndex];
                    }
                    //byte[] bytes = new byte[DTEData.NameTable.TextTableTextSize];
                    //for (int ColumnIndex = 0; ColumnIndex < DTEData.NameTable.TextTableTextSize; ColumnIndex++)
                    //{
                    //    bytes[ColumnIndex] = DTEData.NameTable.TextTableFile.FileBytes[DTEData.NameTable.TextTableStart + (Item.ItemIndex * DTEData.NameTable.TextTableRowSize) + ColumnIndex];
                    //}
                    Item.ItemName = encoding.GetString(bytes);
                }
                if (TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                {
                    string fullText = Encoding.UTF8.GetString(TextTable.TextTableFile.FileBytes);
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);


                    //for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                    //{

                    //    int index = i + EntryClass.EntryTypeMenu.Start;
                    //    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";


                    //    comboBoxItem.Content = (i + EntryClass.EntryTypeMenu.Start) + ": " + lineText; 
                    //}

                    Item.ItemName = lines[TextTable.TextTableStart + Item.ItemIndex];
                }
                if (TextTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing) //I forget why this was made, but this should never trigger?
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        throw new InvalidOperationException("Decode was triggers despite NameTable being linked to Nothing, WHY DID THIS HAPPEN?"); //i want to remove this from even being possible.
                    }

                    string fullText = Encoding.UTF8.GetString(TextTable.TextTableFile.FileBytes);

                    // Split into lines (assuming \n or \r\n)
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    // Get the specific line
                    int index = Item.ItemIndex;
                    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";


                    Item.ItemName = lineText;
                }
            }
        }

        public void DecodeAllItemNames(DataTableEditorData DTEData)
        {
            
            string Cypher = DTEData.NameTable.TextTableCharacterSet;

            Encoding encoding = null; ;
            if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile || DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced) 
            {   
                
                if (Cypher == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (Cypher == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { throw new InvalidOperationException("Unsupported character set"); }
            }



            foreach (TextInfo Item in DTEData.NameTable.ItemList)
            {
                if (Item.IsFolder == true) { continue; }

                if (DTEData.WorkshopData.IsProjectLoaded == false) 
                {
                    Item.ItemName = "Not Loaded";
                    continue;
                }

                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile || DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                {
                    //int Padding = ItemInfo.RowEnd + 1 - ItemInfo.RowStart;
                    //string TheText = ItemInfo.ItemName.PadRight(Padding, '\0');
                    //byte[] bytes = encoding.GetBytes(TheText);
                    //for (int i = 0; i + ItemInfo.RowStart <= ItemInfo.RowEnd; i++)
                    //{
                    //    ByteManager.ByteWriter(bytes[i], DTEData.NameTable.TextTableFile.FileBytes, ItemInfo.RowStart + i);
                    //}

                    int MyTextSize = Item.RowEnd + 1 - Item.RowStart;
                    byte[] bytes = new byte[MyTextSize];
                    for (int ColumnIndex = 0; ColumnIndex < MyTextSize; ColumnIndex++)
                    {
                        bytes[ColumnIndex] = DTEData.NameTable.TextTableFile.FileBytes[Item.RowStart + ColumnIndex];
                    }
                    //byte[] bytes = new byte[DTEData.NameTable.TextTableTextSize];
                    //for (int ColumnIndex = 0; ColumnIndex < DTEData.NameTable.TextTableTextSize; ColumnIndex++)
                    //{
                    //    bytes[ColumnIndex] = DTEData.NameTable.TextTableFile.FileBytes[DTEData.NameTable.TextTableStart + (Item.ItemIndex * DTEData.NameTable.TextTableRowSize) + ColumnIndex];
                    //}
                    Item.ItemName = encoding.GetString(bytes);
                }
                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                {
                    string fullText = Encoding.UTF8.GetString(DTEData.NameTable.TextTableFile.FileBytes);
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);


                    //for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                    //{

                    //    int index = i + EntryClass.EntryTypeMenu.Start;
                    //    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";


                    //    comboBoxItem.Content = (i + EntryClass.EntryTypeMenu.Start) + ": " + lineText; 
                    //}

                    Item.ItemName = lines[DTEData.NameTable.TextTableStart + Item.ItemIndex];
                }
                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Nothing) //I forget why this was made, but this should never trigger?
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        //I did not program in support for this, so it should never happen...
                        throw new InvalidOperationException("Decode was triggered despite NameTable being linked to Nothing, WHY DID THIS HAPPEN?"); //i want to remove this from even being possible.
                    }

                    string fullText = Encoding.UTF8.GetString(DTEData.NameTable.TextTableFile.FileBytes);

                    // Split into lines (assuming \n or \r\n)
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    // Get the specific line
                    int index = Item.ItemIndex;
                    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";


                    Item.ItemName = lineText;
                }
            }


        }

        public void DecodeDescriptions(Workshop TheWorkshop, DataTableEditorData DTEData) 
        {
            DTEData.EditorDescriptionsPanel.Children.OfType<TextBox>().ToList().ForEach(tb => DTEData.EditorDescriptionsPanel.Children.Remove(tb)); //Remove all old textboxes.

            if (TheWorkshop.IsPreviewMode == true) { return; }            

            //!!
            //PART 1: Delete any invalid / corrupted description tables.
            for (int i = 0; i < DTEData.DescriptionTableList.Count; i++) 
            {
                TextTable ExtraTable = DTEData.DescriptionTableList[i];

                if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                {
                    if (ExtraTable.TextTableStart == 0 || ExtraTable.TextTableRowSize == 0 || ExtraTable.TextTableCharLimit == 0 || ExtraTable.TextTableFile == null || ExtraTable.TextTableFile.FileLocation == null)
                    {
                        // Remove and break
                        DTEData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }
                if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                {
                    //Make something later (What would even count as a corrupt advanced table?)
                }
                if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                {
                    if (ExtraTable.TextTableFile == null || ExtraTable.TextTableFile.FileLocation == null)
                    {
                        // Remove and break
                        DTEData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }
            }

            //!!
            //PART 2: Decode the description and set the textbox to work with it.
            TextInfo NameItem = (DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem)?.Tag as TextInfo;

            foreach (TextTable ExtraTable in DTEData.DescriptionTableList)
            {
                //IMPORTANT NOTE:
                //Description textboxes directly decodes the text from the file currently in memory, instead of using a description table's text info, 
                //because it helps create a fake version of data binding. 
                //I should recode this in the future, especially when implimenting actual data binding. 
                if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) //Description type is X
                {
                    
                    Encoding encoding;
                    if (ExtraTable.TextTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                    else if (ExtraTable.TextTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                    else { return; }


                    byte[] bytes = new byte[ExtraTable.TextTableCharLimit];
                    for (int RowIndex = 0; RowIndex < ExtraTable.TextTableCharLimit; RowIndex++)
                    {
                        bytes[RowIndex] = ExtraTable.TextTableFile.FileBytes[ExtraTable.TextTableStart + (NameItem.ItemIndex * ExtraTable.TextTableRowSize) + RowIndex];
                    }

                    TextInfo FAKEtextinfo = new(); //new brute force way to support the new everything is a text table system. 
                    FAKEtextinfo.RowStart = ExtraTable.TextTableStart + (NameItem.ItemIndex * ExtraTable.TextTableRowSize);
                    FAKEtextinfo.RowEnd = ExtraTable.TextTableStart + (NameItem.ItemIndex * ExtraTable.TextTableRowSize) + ExtraTable.TextTableCharLimit - 1;

                    CreateDescriptionTextbox(TheWorkshop, DTEData, FAKEtextinfo);

                    ExtraTable.DescriptionTableEncodeIsEnabled = false;
                    ExtraTable.DescriptionTableTextBox.Text = encoding.GetString(bytes).TrimEnd('\0');
                    ExtraTable.DescriptionTableEncodeIsEnabled = true;


                    //NameBox.Text = data.ItemName.TrimEnd('\0');
                }
                if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                {
                                                            
                    foreach (TextInfo textInfo in ExtraTable.ItemList) 
                    {
                        if (NameItem.ItemIndex == textInfo.ItemIndex) 
                        {
                            Encoding encoding;
                            if (ExtraTable.TextTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                            else if (ExtraTable.TextTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                            else { return; }

                            int MyTextSize = textInfo.RowEnd + 1 - textInfo.RowStart;
                            byte[] bytes = new byte[MyTextSize];
                            for (int ColumnIndex = 0; ColumnIndex < MyTextSize; ColumnIndex++)
                            {
                                bytes[ColumnIndex] = ExtraTable.TextTableFile.FileBytes[textInfo.RowStart + ColumnIndex]; //Index out of range
                            }

                            CreateDescriptionTextbox(TheWorkshop, DTEData, null);

                            ExtraTable.DescriptionTableEncodeIsEnabled = false;
                            ExtraTable.DescriptionTableTextBox.Text = encoding.GetString(bytes).TrimEnd('\0');
                            ExtraTable.DescriptionTableEncodeIsEnabled = true;
                            break;
                        }
                    }

                    
                }
                if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile) //Description type is X
                {
                    if (ExtraTable.TextTableFile == null || ExtraTable.TextTableFile.FileLocation == null) { break; }
                    

                    // Convert byte array to full string
                    string fullText = Encoding.UTF8.GetString(ExtraTable.TextTableFile.FileBytes);

                    // Split into lines (assuming \n or \r\n)
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    // Get the specific line
                    int index = NameItem.ItemIndex + ExtraTable.TextTableStart;

                    {   //Do not create description box if going beyond the end of the text file.
                        int TrueIndex = NameItem.ItemIndex + 1;
                        if (TrueIndex > lines.Length - ExtraTable.TextTableStart) 
                        {
                            break;
                        }
                    }


                    //TextInfo FAKEtextinfo = new(); //new brute force way to support the new everything is a text table system. 
                    //FAKEtextinfo.RowStart = ExtraTable.TextTableStart + (NameItem.ItemIndex * ExtraTable.TextTableRowSize);
                    //FAKEtextinfo.RowEnd = ExtraTable.TextTableStart + (NameItem.ItemIndex * ExtraTable.TextTableRowSize) + ExtraTable.TextTableCharLimit - 1;


                    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";

                    CreateDescriptionTextbox(TheWorkshop, DTEData, null);

                    ExtraTable.DescriptionTableEncodeIsEnabled = false;
                    ExtraTable.DescriptionTableTextBox.Text = lineText;
                    ExtraTable.DescriptionTableEncodeIsEnabled = true;

                }


                

                
            }

            
        }


        private void CreateDescriptionTextbox(Workshop TheWorkshop, DataTableEditorData DTEData, TextInfo FAKEtextinfo) 
        {
            //I will need to fix this later if i want to ever re-add multiple description support.

            DTEData.DTEXaml.DescriptionCharCount.Content = "";

            for (int i = 0; i < DTEData.DescriptionTableList.Count; i++)
            {
                var DescriptionTable = DTEData.DescriptionTableList[i];

                
                TextBox DescriptionTextBox = new();
                DescriptionTextBox.AcceptsReturn = true;
                DescriptionTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                DescriptionTextBox.TextWrapping = TextWrapping.NoWrap;
                DescriptionTextBox.Margin = new Thickness(5);
                DockPanel.SetDock(DescriptionTextBox, Dock.Top);
                DescriptionTextBox.MinHeight = 76;
                DescriptionTextBox.VerticalAlignment = VerticalAlignment.Top;
                DTEData.EditorDescriptionsPanel.Children.Add(DescriptionTextBox);
                

                TreeViewItem selectedItem222 = DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
                TextInfo NameItemInfo = selectedItem222.Tag as TextInfo;
                if (NameItemInfo.IsFolder == true)
                {
                    DescriptionTextBox.IsEnabled = false;
                    return;
                }

                
                
                DescriptionTable.DescriptionTableTextBox = DescriptionTextBox;
                DescriptionTable.DescriptionTableEncodeIsEnabled = true;
                
                DescriptionTextBox.IsEnabled = true;


                

                DescriptionTextBox.PreviewKeyDown += (sender, e) => //BLOCK INPUT IF KEY IS THE ENTER KEY
                {
                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile && e.Key == Key.Enter)
                    {
                        e.Handled = true;

                        if (DescriptionTable.TextTableCharLimit == DescriptionTable.DescriptionTableTextBox.Text.Length) { return; }

                        TextBox textBox = sender as TextBox;

                        int caretIndex = textBox.CaretIndex;
                        textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                        textBox.CaretIndex = caretIndex + 1;
                    }
                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced && e.Key == Key.Enter)
                    {
                        e.Handled = true;

                        foreach (TextInfo textInfo in DescriptionTable.ItemList)
                        {
                            if (NameItemInfo.ItemIndex == textInfo.ItemIndex)
                            {
                                //for (int i = 0; i + ItemInfo.RowStart <= ItemInfo.RowEnd; i++)
                                //{
                                //    ByteManager.ByteWriter(bytes[i], ExtraTable.TextTableFile.FileBytes, ItemInfo.RowStart + i);
                                //}

                                int Padding = textInfo.RowEnd + 1 - textInfo.RowStart;
                                if (Padding == DescriptionTable.DescriptionTableTextBox.Text.Length) { return; }

                                TextBox textBox = sender as TextBox;

                                int caretIndex = textBox.CaretIndex;
                                textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                                textBox.CaretIndex = caretIndex + 1;
                            }
                        }

                        
                    }
                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile && e.Key == Key.Enter)
                    {
                        e.Handled = true;
                    }
                };

                DescriptionTextBox.PreviewTextInput += (sender, e) => //Block text change IF... (Beyond end of row, or end of text file, etc...)
                {
                    string NewText = DescriptionTable.DescriptionTableTextBox.Text + e.Text;

                    Encoding encoding;
                    if (DescriptionTable.TextTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                    else if (DescriptionTable.TextTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                    else { return; }
                    int NewByteSize = encoding.GetByteCount(NewText);

                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile && NewByteSize > DescriptionTable.TextTableCharLimit)
                    {
                        e.Handled = true;  // Mark the event as handled so the input is ignored
                    }                    
                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile && (e.Text.Contains("\r") || e.Text.Contains("\n")))
                    {
                        e.Handled = true;  // Mark the event as handled so the input is ignored
                    }
                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                    {
                        foreach (TextInfo textInfo in DescriptionTable.ItemList)
                        {
                            if (NameItemInfo.ItemIndex == textInfo.ItemIndex)
                            {
                                int CharLimit = textInfo.RowEnd + 1 - textInfo.RowStart;
                                if (NewByteSize > CharLimit)
                                {
                                    e.Handled = true;  // Mark the event as handled so the input is ignored
                                }
                                break;
                            }
                        }
                    }


                };

                DescriptionTextBox.TextChanged += (sender, e) =>
                {
                    TreeViewItem selectedItem = DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
                    TextInfo ItemInfo = selectedItem.Tag as TextInfo;
                    if (selectedItem == null || selectedItem.Tag == null || ItemInfo.IsFolder == true || DescriptionTable.DescriptionTableEncodeIsEnabled == false)
                    {
                        return;
                    }

                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                    {
                        //foreach (TextInfo textInfo in DescriptionTable.ItemList)
                        //{
                        //    if (NameItemInfo.ItemIndex == textInfo.ItemIndex)
                        //    {
                        //        CharacterSetManager CharacterSetManager = new();
                        //        CharacterSetManager.EncodeDescription(TheWorkshop, DTEData, DescriptionTable, textInfo);
                        //    }
                        //}

                        CharacterSetManager CharacterSetManagerB = new();
                        CharacterSetManagerB.EncodeDescription(TheWorkshop, DTEData, DescriptionTable, FAKEtextinfo);
                    }
                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                    {
                        foreach (TextInfo textInfo in DescriptionTable.ItemList)
                        {
                            if (NameItemInfo.ItemIndex == textInfo.ItemIndex)
                            {
                                CharacterSetManager CharacterSetManager = new();
                                CharacterSetManager.EncodeDescription(TheWorkshop, DTEData, DescriptionTable, textInfo);
                            }
                        }
                    }
                    if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                    {
                        CharacterSetManager CharacterSetManager = new();
                        CharacterSetManager.EncodeDescription(TheWorkshop, DTEData, DescriptionTable, null);
                    }
                    
                        

                    { //Update Descriptions char count
                        string NewText = DescriptionTable.DescriptionTableTextBox.Text; //+ e.Text;
                        Encoding encoding;
                        if (DescriptionTable.TextTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                        else if (DescriptionTable.TextTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                        else { return; }
                        int NewByteSize = encoding.GetByteCount(NewText);

                        if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                        {
                            DTEData.DTEXaml.DescriptionCharCount.Content = "Chars: " + NewByteSize + " / " + DescriptionTable.TextTableCharLimit;
                        }
                        if (DescriptionTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                        {
                            foreach (TextInfo textInfo in DescriptionTable.ItemList)
                            {
                                if (NameItemInfo.ItemIndex == textInfo.ItemIndex)
                                {
                                    int Padding = textInfo.RowEnd + 1 - textInfo.RowStart;

                                    DTEData.DTEXaml.DescriptionCharCount.Content = "Chars: " + NewByteSize + " / " + Padding;
                                }
                            }
                        }
                        
                        
                        
                    }
                    

                    //I should display  some Char Count Limit for descriptions panel.
                    //else { TheWorkshop.DebugBox2.Text = "Current NameBox Text Length\n" + (NameBox.Text.Length + 1).ToString(); }
                };



            }
        }

        public string DecodeReturn(string Cypher, byte[] TheBytes)
        {

            Encoding encoding;
            if (Cypher == "ASCII+ANSI") { encoding = Encoding.ASCII; }
            else if (Cypher == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
            else { return null; } //just crash if this happens, as its a major error.
            
            return encoding.GetString(TheBytes);

        }



        public void EncodeItem(DataTableEditorData DTEData, TextInfo ItemInfo = null)
        {
            
            string Cypher = DTEData.NameTable.TextTableCharacterSet;
            

            if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile || DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
            {
                Encoding encoding;

                if (Cypher == "ASCII+ANSI")
                {
                    encoding = Encoding.ASCII;
                }
                else if (Cypher == "Shift-JIS")
                {
                    encoding = Encoding.GetEncoding("shift_jis");
                }
                else
                {
                    throw new InvalidOperationException("Unsupported character set");
                }
                                
                int Padding = ItemInfo.RowEnd + 1 - ItemInfo.RowStart;
                string TheText = ItemInfo.ItemName.PadRight(Padding, '\0');
                byte[] bytes = encoding.GetBytes(TheText);

                for (int i = 0; i + ItemInfo.RowStart <= ItemInfo.RowEnd; i++)
                {
                    ByteManager.ByteWriter(bytes[i], DTEData.NameTable.TextTableFile.FileBytes, ItemInfo.RowStart + i);
                }
            }
            if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
            {
                string origonalText = Encoding.UTF8.GetString(DTEData.NameTable.TextTableFile.FileBytes);

                // Split into lines
                string[] lines = origonalText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                int rowIndex = DTEData.TableRowIndex + DTEData.NameTable.TextTableStart;

                //// Resize lines if needed
                //if (rowIndex >= lines.Length) //i forgot what this actually does so it's disabled until i remember. :>
                //{
                //    Array.Resize(ref lines, rowIndex + 1);
                //}

                // Replace or insert line at the target index
                lines[rowIndex] = ItemInfo.ItemName;

                // Recombine and re-encode
                string newText = string.Join("\r\n", lines);
                DTEData.NameTable.TextTableFile.FileBytes = Encoding.UTF8.GetBytes(newText);

            }


        }


        public void EncodeDescription(Workshop TheWorkshop, DataTableEditorData DTEData, TextTable ExtraTable, TextInfo ItemInfo) 
        {
            if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) 
            {
                string Cypher = ExtraTable.TextTableCharacterSet;

                Encoding encoding;
                if (Cypher == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (Cypher == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { return; } //make this throw an error notification?

                int Padding = ItemInfo.RowEnd + 1 - ItemInfo.RowStart;
                string TheText = ExtraTable.DescriptionTableTextBox.Text.PadRight(Padding, '\0');
                byte[] bytes = encoding.GetBytes(TheText);

                for (int i = 0; i < ExtraTable.TextTableCharLimit; i++)
                {
                    ByteManager.ByteWriter(bytes[i], ExtraTable.TextTableFile.FileBytes, ExtraTable.TextTableStart + (DTEData.TableRowIndex * ExtraTable.TextTableRowSize) + i);
                }
            }
            if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
            {
                string Cypher = ExtraTable.TextTableCharacterSet;

                Encoding encoding;
                if (Cypher == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (Cypher == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { return; } //make this throw an error notification?

                int Padding = ItemInfo.RowEnd + 1 - ItemInfo.RowStart;
                string TheText = ExtraTable.DescriptionTableTextBox.Text.PadRight(Padding, '\0');
                byte[] bytes = encoding.GetBytes(TheText);

                for (int i = 0; i < Padding; i++)
                {
                    ByteManager.ByteWriter(bytes[i], ExtraTable.TextTableFile.FileBytes, ItemInfo.RowStart + i);
                }
                
            }
            if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile) 
            {
                // Decode current file content
                string fullText = Encoding.UTF8.GetString(ExtraTable.TextTableFile.FileBytes);

                // Split into lines
                string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                int rowIndex = DTEData.TableRowIndex;

                // Resize lines if needed
                if (rowIndex >= lines.Length)
                {
                    Array.Resize(ref lines, rowIndex + 1);
                }

                // Replace or insert line at the target index
                lines[rowIndex] = ExtraTable.DescriptionTableTextBox.Text;

                // Recombine and re-encode
                string updatedText = string.Join("\r\n", lines);
                ExtraTable.TextTableFile.FileBytes = Encoding.UTF8.GetBytes(updatedText);
            }






        }


    }
}
