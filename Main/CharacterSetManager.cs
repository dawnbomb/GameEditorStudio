using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace GameEditorStudio
{
    class CharacterSetManager
    {
        //This file handles decoding and encoding text in the Item List, and any Extra Tables.
        //It changes from Hex to english or other languages, and those languages back to hex.
        //A "Character Set" is a list of symbols associated with hex. Standard ASCII is english text (an english character set). Shift-JIS is Japanese text.
        
        //Maybe I should have a function that decodes just one string, and then the plural version command just runs the single version X times?
                

        public void Decode(Workshop TheWorkshop, Editor EditorClass, string Doing)
        {
            
            string Cypher = EditorClass.StandardEditorData.NameTableCharacterSet;

            Encoding encoding = null; ;
            if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.DataFile) 
            {   
                
                if (Cypher == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (Cypher == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { throw new InvalidOperationException("Unsupported character set"); }
            }
            



            if (Doing == "Items")
            {
                foreach (var Item in EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList)
                {
                    if (Item.IsFolder == false)
                    {
                        if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.DataFile)
                        {
                            byte[] bytes = new byte[EditorClass.StandardEditorData.NameTableTextSize];
                            for (int RowIndex = 0; RowIndex < EditorClass.StandardEditorData.NameTableTextSize; RowIndex++)
                            {
                                bytes[RowIndex] = EditorClass.StandardEditorData.FileNameTable.FileBytes[EditorClass.StandardEditorData.NameTableStart + (Item.ItemIndex * EditorClass.StandardEditorData.NameTableRowSize) + RowIndex];
                            }
                            Item.ItemName = encoding.GetString(bytes);
                        }
                        if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.TextFile)
                        {
                            string fullText = Encoding.UTF8.GetString(EditorClass.StandardEditorData.FileNameTable.FileBytes);
                            string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                                                        

                            //for (int i = 0; i < EntryClass.EntryTypeMenu.NameCount; i++)
                            //{
                                
                            //    int index = i + EntryClass.EntryTypeMenu.Start;
                            //    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";


                            //    comboBoxItem.Content = (i + EntryClass.EntryTypeMenu.Start) + ": " + lineText; 
                            //}

                            Item.ItemName = lines[EditorClass.StandardEditorData.NameTableStart + Item.ItemIndex];
                        }
                        if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.Nothing) //I forget why this was made, but this should never trigger?
                        {
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                throw new InvalidOperationException("Decode was triggers despite NameTable being linked to Nothing, WHY DID THIS HAPPEN?"); //i want to remove this from even being possible.
                            }

                            string fullText = Encoding.UTF8.GetString(EditorClass.StandardEditorData.FileNameTable.FileBytes);

                            // Split into lines (assuming \n or \r\n)
                            string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                            // Get the specific line
                            int index = Item.ItemIndex;
                            string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";                                                        

                            
                            Item.ItemName = lineText;
                        }
                        
                    }
                }
            }
                     
            
        }

        public void DecodeExtras(Workshop TheWorkshop, Editor EditorClass) 
        {
            if (TheWorkshop.IsPreviewMode == true) { return; }

            EditorClass.StandardEditorData.EditorDescriptionsPanel.TopPanel.Children.OfType<TextBox>().ToList().ForEach(tb => EditorClass.StandardEditorData.EditorDescriptionsPanel.TopPanel.Children.Remove(tb)); //Remove all old textboxes.
            for (int i = 0; i < EditorClass.StandardEditorData.DescriptionTableList.Count; i++) //Delete any invalid description tables. A foreach loop but using for explicitly so i can remove the tables. 
            {
                DescriptionTable ExtraTable = EditorClass.StandardEditorData.DescriptionTableList[i];

                if (ExtraTable.LinkType == DescriptionTable.LinkTypes.DataFile)
                {
                    if (ExtraTable.Start == 0 || ExtraTable.RowSize == 0 || ExtraTable.TextSize == 0 || ExtraTable.FileTextTable == null || ExtraTable.FileTextTable.FileLocation == null)
                    {
                        // Remove and break
                        EditorClass.StandardEditorData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }
                if (ExtraTable.LinkType == DescriptionTable.LinkTypes.DataFile)
                {
                    if (ExtraTable.FileTextTable == null || ExtraTable.FileTextTable.FileLocation == null)
                    {
                        // Remove and break
                        EditorClass.StandardEditorData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }
            }



            ItemInfo TheItem = (EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.SelectedItem as TreeViewItem)?.Tag as ItemInfo;

            foreach (DescriptionTable ExtraTable in EditorClass.StandardEditorData.DescriptionTableList)
            {
                if (ExtraTable.LinkType == DescriptionTable.LinkTypes.DataFile) //Description type is X
                {
                    
                    Encoding encoding;
                    if (ExtraTable.CharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                    else if (ExtraTable.CharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                    else { return; }

                    byte[] bytes = new byte[ExtraTable.TextSize];
                    for (int RowIndex = 0; RowIndex < ExtraTable.TextSize; RowIndex++)
                    {
                        bytes[RowIndex] = ExtraTable.FileTextTable.FileBytes[ExtraTable.Start + (TheItem.ItemIndex * ExtraTable.RowSize) + RowIndex];
                    }

                    CreateDescriptionTextbox(TheWorkshop, EditorClass);

                    ExtraTable.ExtraTableEncodeIsEnabled = false;
                    ExtraTable.ExtraTableTextBox.Text = encoding.GetString(bytes).TrimEnd('\0');
                    ExtraTable.ExtraTableEncodeIsEnabled = true;
                    //NameBox.Text = data.ItemName.TrimEnd('\0');
                }
                if (ExtraTable.LinkType == DescriptionTable.LinkTypes.TextFile) //Description type is X
                {
                    if (ExtraTable.FileTextTable == null || ExtraTable.FileTextTable.FileLocation == null) { break; }

                    // Convert byte array to full string
                    string fullText = Encoding.UTF8.GetString(ExtraTable.FileTextTable.FileBytes);

                    // Split into lines (assuming \n or \r\n)
                    string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    // Get the specific line
                    int index = TheItem.ItemIndex;
                    string lineText = (index >= 0 && index < lines.Length) ? lines[index] : "";

                    CreateDescriptionTextbox(TheWorkshop, EditorClass);

                    ExtraTable.ExtraTableEncodeIsEnabled = false;
                    ExtraTable.ExtraTableTextBox.Text = lineText;
                    ExtraTable.ExtraTableEncodeIsEnabled = true;

                }


                

                
            }

            
        }


        private void CreateDescriptionTextbox(Workshop TheWorkshop, Editor EditorClass) 
        {
            //I will need to fix this later if i want to ever re-add multiple description support.
            
            for (int i = 0; i < EditorClass.StandardEditorData.DescriptionTableList.Count; i++)
            {
                var ExtraTable = EditorClass.StandardEditorData.DescriptionTableList[i];


                TextBox ExtraTextBox = new();
                ExtraTextBox.AcceptsReturn = true;
                ExtraTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                ExtraTextBox.TextWrapping = TextWrapping.NoWrap;
                ExtraTextBox.Margin = new Thickness(5);
                DockPanel.SetDock(ExtraTextBox, Dock.Top);
                EditorClass.StandardEditorData.EditorDescriptionsPanel.TopPanel.Children.Add(ExtraTextBox);
                ExtraTextBox.Height = 67;
                ExtraTable.ExtraTableTextBox = ExtraTextBox;
                ExtraTable.ExtraTableEncodeIsEnabled = true;
                ExtraTextBox.VerticalAlignment = VerticalAlignment.Top;

                ExtraTextBox.PreviewKeyDown += (sender, e) =>
                {
                    if (ExtraTable.LinkType == DescriptionTable.LinkTypes.DataFile && e.Key == Key.Enter)
                    {
                        e.Handled = true;

                        if (ExtraTable.TextSize == ExtraTable.ExtraTableTextBox.Text.Length) { return; }

                        TextBox textBox = sender as TextBox;

                        int caretIndex = textBox.CaretIndex;
                        textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                        textBox.CaretIndex = caretIndex + 1;
                    }
                    if (ExtraTable.LinkType == DescriptionTable.LinkTypes.TextFile && e.Key == Key.Enter)
                    {
                        e.Handled = true;
                    }
                };

                ExtraTextBox.PreviewTextInput += (sender, e) =>
                {
                    string NewText = ExtraTable.ExtraTableTextBox.Text + e.Text;

                    Encoding encoding;
                    if (ExtraTable.CharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                    else if (ExtraTable.CharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                    else { return; }
                    int NewByteSize = encoding.GetByteCount(NewText);

                    if (ExtraTable.LinkType == DescriptionTable.LinkTypes.DataFile && NewByteSize > ExtraTable.TextSize)
                    {
                        e.Handled = true;  // Mark the event as handled so the input is ignored
                    }
                    if (ExtraTable.LinkType == DescriptionTable.LinkTypes.TextFile && (e.Text.Contains("\r") || e.Text.Contains("\n")))
                    {
                        e.Handled = true;  // Mark the event as handled so the input is ignored
                    }
                    //else { TheWorkshop.DebugBox2.Text = "Current NameBox Text Length\n" + (NameBox.Text.Length + 1).ToString(); }

                };

                ExtraTextBox.TextChanged += (sender, e) =>
                {
                    TreeViewItem selectedItem = EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.SelectedItem as TreeViewItem;
                    ItemInfo ItemInfo = selectedItem.Tag as ItemInfo;
                    if (selectedItem == null || selectedItem.Tag == null || ItemInfo.IsFolder == true || ExtraTable.ExtraTableEncodeIsEnabled == false)
                    {
                        return;
                    }

                    CharacterSetManager CharacterSetManager = new();
                    CharacterSetManager.EncodeExtra(TheWorkshop, EditorClass, ExtraTable);

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



        public void Encode(Workshop TheWorkshop, Editor EditorClass, string Doing, ItemInfo ItemInfo = null)
        {
            

            string Cypher = "";
            if (Doing == "Item")
            {
                Cypher = EditorClass.StandardEditorData.NameTableCharacterSet;
            }
            

            if (Doing == "Item")
            {
                if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.DataFile) 
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

                    //string TheText = TheWorkshop.PropertiesItemTextboxName.Text.PadRight(EditorClass.NameTableRowSize, '\0');
                    string TheText = ItemInfo.ItemName.PadRight(EditorClass.StandardEditorData.NameTableTextSize, '\0');
                    byte[] bytes = encoding.GetBytes(TheText);

                    for (int i = 0; i < EditorClass.StandardEditorData.NameTableTextSize; i++)
                    {
                        ByteManager.ByteWriter(bytes[i], EditorClass.StandardEditorData.FileNameTable.FileBytes, EditorClass.StandardEditorData.NameTableStart + (EditorClass.StandardEditorData.TableRowIndex * EditorClass.StandardEditorData.NameTableRowSize) + i);
                    }
                }
                if (EditorClass.StandardEditorData.NameTableLinkType == StandardEditorData.NameTableLinkTypes.TextFile)
                {
                    string origonalText = Encoding.UTF8.GetString(EditorClass.StandardEditorData.FileNameTable.FileBytes);

                    // Split into lines
                    string[] lines = origonalText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    int rowIndex = EditorClass.StandardEditorData.TableRowIndex + EditorClass.StandardEditorData.NameTableStart;

                    //// Resize lines if needed
                    //if (rowIndex >= lines.Length) //i forgot what this actually does so it's disabled until i remember. :>
                    //{
                    //    Array.Resize(ref lines, rowIndex + 1);
                    //}

                    // Replace or insert line at the target index
                    lines[rowIndex] = ItemInfo.ItemName;

                    // Recombine and re-encode
                    string newText = string.Join("\r\n", lines);
                    EditorClass.StandardEditorData.FileNameTable.FileBytes = Encoding.UTF8.GetBytes(newText);

                }

                
            }
            
            
        }


        public void EncodeExtra(Workshop TheWorkshop, Editor EditorClass, DescriptionTable ExtraTable) 
        {
            if (ExtraTable.LinkType == DescriptionTable.LinkTypes.DataFile) 
            {
                string Cypher = ExtraTable.CharacterSet;

                Encoding encoding;
                if (Cypher == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (Cypher == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { return; } //make this throw an error notification?


                string TheText = ExtraTable.ExtraTableTextBox.Text.PadRight(ExtraTable.TextSize, '\0');
                byte[] bytes = encoding.GetBytes(TheText);

                for (int i = 0; i < ExtraTable.TextSize; i++)
                {
                    ByteManager.ByteWriter(bytes[i], ExtraTable.FileTextTable.FileBytes, ExtraTable.Start + (EditorClass.StandardEditorData.TableRowIndex * ExtraTable.RowSize) + i);
                }
            }
            if (ExtraTable.LinkType == DescriptionTable.LinkTypes.TextFile) 
            {
                // Decode current file content
                string fullText = Encoding.UTF8.GetString(ExtraTable.FileTextTable.FileBytes);

                // Split into lines
                string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                int rowIndex = EditorClass.StandardEditorData.TableRowIndex;

                // Resize lines if needed
                if (rowIndex >= lines.Length)
                {
                    Array.Resize(ref lines, rowIndex + 1);
                }

                // Replace or insert line at the target index
                lines[rowIndex] = ExtraTable.ExtraTableTextBox.Text;

                // Recombine and re-encode
                string updatedText = string.Join("\r\n", lines);
                ExtraTable.FileTextTable.FileBytes = Encoding.UTF8.GetBytes(updatedText);
            }






        }


    }
}
