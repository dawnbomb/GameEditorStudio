using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GameEditorStudio.AutoModMathStep;
using static GameEditorStudio.MathStep;

namespace GameEditorStudio
{
    public class AutoModMathStep
    {

        public AutoModSimpleMathPieces MathPiece { get; set; } = AutoModSimpleMathPieces.Multiply;
        public string MathValue { get; set; } = "1";
        public Entry? EntryTarget { get; set; } = null;

        public string EntryKey { get; set; } = ""; //Not XML. When first loading the editor, this temporarily holds the target entry key. After all entry's are loaded, this is used to set the target entry. It is then never used again.

        public enum AutoModSimpleMathPieces //DO NOT RENAME, THESE TERMS SAVE TO XML. (If desperate, update the XML saving first).
        {
            Plus,
            Minus,
            Multiply,
            Divide,
            Max,
            Min,
            Round,
            RoundUp,
            RoundDown,
            SetToX,
        }
                
    }

    public partial class AutoMod : UserControl
    {
        public WorkshopData WData { get; set; } = null;
        public DataTableEditorData DTEData { get; set; } = null;
        public List<AutoModMathStep> MathFormula { get; set; } = new(); 

        public AutoMod()
        {
            InitializeComponent();            
        }

        public void AutomodSetup(DataTableEditorData DTEData2) 
        {
            DTEData = DTEData2;

            AutoModMathStep AutoMathSetp = new();
            MathFormula.Add(AutoMathSetp);

            RefreshFormula();
        }

        private void AddMathPieceClick(object sender, RoutedEventArgs e)
        {
            MathFormula.Add(new());
            RefreshFormula();
        }

        private void RefreshFormula() 
        {
            StackPanel FormulaStack = FormulaStackPanel;

            FormulaStack.Children.Clear();

            //List<MathboxData> OKmathboxes = new List<MathboxData>();
            //foreach (MathboxData mathdata in MathboxData.ParentEditor.DataTableEditorData.WorkshopData.MasterMathboxList)
            //{
            //    if (mathdata != MathboxData && mathdata.ParentEditor == MathboxData.ParentEditor) { OKmathboxes.Add(mathdata); }
            //}

            foreach (AutoModMathStep MathStep in MathFormula)
            {
                Border border = new Border();
                border.Margin = new Thickness(3);
                FormulaStack.Children.Add(border);

                DockPanel MainDockPanel = new DockPanel();
                border.Child = MainDockPanel;


                DockPanel rightside = new DockPanel();
                DockPanel.SetDock(rightside, Dock.Right);


                ///////////// LEFT SIDE ////////////
                ComboBox PieceComboBox = new ComboBox();
                PieceComboBox.Margin = new Thickness(3);
                PieceComboBox.Width = 120;
                DockPanel.SetDock(PieceComboBox, Dock.Left);

                MainDockPanel.Children.Add(PieceComboBox);

                foreach (AutoModSimpleMathPieces value in Enum.GetValues<AutoModSimpleMathPieces>())
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = value.ToString(),
                        Tag = value
                    };

                    PieceComboBox.Items.Add(item);

                    if (MathStep.MathPiece == value)
                    {
                        PieceComboBox.SelectedItem = item;
                    }
                }



                //ComboBox EntryOrValue = new ComboBox();
                //EntryOrValue.Margin = new Thickness(3);
                //EntryOrValue.Width = 90;
                //DockPanel.SetDock(EntryOrValue, Dock.Left);
                //MainDockPanel.Children.Add(EntryOrValue);
                ////

                //ComboBoxItem ValueItem = new();
                //ValueItem.Content = "Number";
                //EntryOrValue.Items.Add(ValueItem);
                //ComboBoxItem EntryItem = new();
                //EntryItem.Content = "Entry";
                //EntryOrValue.Items.Add(EntryItem);
                //ComboBoxItem MathboxItem = new();
                //MathboxItem.Content = "Mathbox";
                ////EntryOrValue.Items.Add(MathboxItem);


                //if (MathStep.ValueSource == AutoModMathStep.AutoModSimpleMathValueSources.Entry) { EntryItem.IsSelected = true; }
                //if (MathStep.ValueSource == AutoModMathStep.AutoModSimpleMathValueSources.Number) { ValueItem.IsSelected = true; }
                //if (MathStep.ValueSource == AutoModMathStep.MathValueSources.Mathbox) { MathboxItem.IsSelected = true; }

                MainDockPanel.Children.Add(rightside);// <- RIGHT SIDE PANEL ADDED AS LAST CHILD

                //EntryOrValue.DropDownClosed += EntryOrValue_DropDownClosed;
                //void EntryOrValue_DropDownClosed(object sender, EventArgs e)
                //{

                //    if (EntryItem.IsSelected == true)
                //    {
                //        MathStep.ValueSource = AutoModMathStep.AutoModSimpleMathValueSources.Entry;
                //        MakeRightSide();
                //        //UpdateMathResult();
                //    }
                //    if (ValueItem.IsSelected == true)
                //    {
                //        MathStep.ValueSource = AutoModMathStep.AutoModSimpleMathValueSources.Number;
                //        MakeRightSide();
                //        //UpdateMathResult();
                //    }
                //    //if (MathboxItem.IsSelected == true)
                //    //{
                //    //    MathStep.ValueSource = AutoModMathStep.MathValueSources.Mathbox;
                //    //    MakeRightSide();
                //    //    UpdateMathResult();
                //    //}
                //}





                PieceComboBox.DropDownClosed += PieceComboBox_DropDownClosed;
                void PieceComboBox_DropDownClosed(object sender, EventArgs e)
                {                    

                    if (PieceComboBox.SelectedItem is ComboBoxItem item && item.Tag is AutoModSimpleMathPieces mathPiece)
                    {
                        MathStep.MathPiece = mathPiece;
                        MakeRightSide();
                        UpdateAutoModExampleMathResults();
                    }
                }









                //////////////// RIGHT SIDE/////////////



                MakeRightSide();

                void MakeRightSide()
                {
                    rightside.Children.Clear();

                    Button DeleteButton = new();
                    DeleteButton.Content = "Delete";
                    DeleteButton.MaxWidth = 90;
                    DeleteButton.Margin = new Thickness(3);
                    DeleteButton.HorizontalAlignment = HorizontalAlignment.Right;
                    DockPanel.SetDock(DeleteButton, Dock.Right);
                    rightside.Children.Add(DeleteButton);
                    DeleteButton.Click += DeleteButton_Click;
                    void DeleteButton_Click(object sender, RoutedEventArgs e)
                    {
                        MathFormula.Remove(MathStep);
                        FormulaStack.Children.Remove(border);
                        UpdateAutoModExampleMathResults();
                    }
                    //OKmathboxes
                    //if (MathStep.ValueSource == MathStep.AutoModMathValueSources.Mathbox)
                    //{
                    //    ComboBox MathComboBox = new ComboBox();
                    //    MathComboBox.Margin = new Thickness(3);
                    //    DockPanel.SetDock(MathComboBox, Dock.Right);
                    //    rightside.Children.Add(MathComboBox);

                    //    int i = 0;
                    //    foreach (MathboxData mathdata in OKmathboxes)
                    //    {

                    //        ComboBoxItem mathItem = new ComboBoxItem();
                    //        mathItem.Content = i + ": " + mathdata.Name;
                    //        if (mathdata.Name == "")
                    //        {
                    //            mathItem.Content = i + ": " + "???"; //+ entry.RowOffset 
                    //            mathItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A4F9D"));
                    //        }
                    //        mathItem.Tag = mathdata;

                    //        MathComboBox.Items.Add(mathItem);
                            
                    //    }

                    //}
                    //if (MathStep.MathPiece == AutoModSimpleMathPieces.Round || MathStep.MathPiece == AutoModSimpleMathPieces.RoundUp || MathStep.MathPiece == AutoModSimpleMathPieces.RoundDown)
                    //{
                    //    EntryOrValue.Visibility = Visibility.Collapsed;
                    //    return;
                    //}
                    //if (MathStep.ValueSource == AutoModMathStep.AutoModSimpleMathValueSources.Entry)
                    //{
                    //    ComboBox EntryBox = new ComboBox();
                    //    EntryBox.Margin = new Thickness(3);
                    //    DockPanel.SetDock(EntryBox, Dock.Right);
                    //    rightside.Children.Add(EntryBox);

                    //    foreach (Entry entry in DTEData.MasterEntryList.OrderBy(e => e.RowOffset))
                    //    {
                    //        if (entry.IsMerged == true) { continue; }
                    //        if (entry.IsEntryHidden == true) { continue; }
                    //        if (entry.IsTextInUse == true) { continue; }

                    //        ComboBoxItem EntryItem = new ComboBoxItem();

                    //        StackPanel content = new StackPanel
                    //        {
                    //            Orientation = Orientation.Horizontal
                    //        };

                    //        TextBlock offsetText = new TextBlock
                    //        {
                    //            Text = entry.RowOffset + ": ",
                    //            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"))
                    //        };

                    //        TextBlock nameText = new TextBlock
                    //        {
                    //            Text = entry.Name == "" ? "???" : entry.Name,
                    //            Foreground = entry.Name == ""
                    //                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A4F7D"))
                    //                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CFCBC7"))
                    //        };

                    //        content.Children.Add(offsetText);
                    //        content.Children.Add(nameText);

                    //        EntryItem.Content = content;
                    //        EntryItem.Tag = entry;

                    //        EntryBox.Items.Add(EntryItem);

                    //        if (MathStep.EntryTarget == entry)
                    //        {
                    //            EntryItem.IsSelected = true;
                    //        }
                    //    }

                    //    EntryBox.DropDownClosed += EntryBox_DropDownClosed;
                    //    void EntryBox_DropDownClosed(object sender, EventArgs e)
                    //    {
                    //        ComboBoxItem CBI = EntryBox.SelectedItem as ComboBoxItem;
                    //        if (CBI == null) { return; }
                    //        Entry theEntry = CBI.Tag as Entry;

                    //        MathStep.EntryTarget = theEntry;
                    //        MathStep.MathValue = "";
                    //        //UpdateMathResult();
                    //    }
                    //}
                    //if (MathStep.ValueSource == AutoModMathStep.AutoModSimpleMathValueSources.Number)
                    if(true)
                    {
                        TextBox textBox = new TextBox();
                        textBox.Margin = new Thickness(3);
                        DockPanel.SetDock(textBox, Dock.Right);
                        rightside.Children.Add(textBox);

                        textBox.Text = MathStep.MathValue;


                        textBox.TextChanged += MathValueboxTextChanged;
                        void MathValueboxTextChanged(object sender, EventArgs e)
                        {
                            MathStep.MathValue = textBox.Text;
                            MathStep.EntryTarget = null;
                            UpdateAutoModExampleMathResults();
                        }
                    }

                }



            }
        }

        public void UpdateAutoModExampleMathResults() 
        {
            if (DTEData.WorkshopData.IsProjectLoaded == false)
            {
                AutoModExampleResultTextBox.Text = "No Project";
                AutoModExampleResultUsingInputTextBox.Text = "No Project";
                return;
            }

            try
            {                
                double EntryValue = DTEMathMethods.GetEntryValueFrom(DTEData.EntryClass, DTEMathMethods.EntryValueSource.FromCurrent);
                double NewValue = CalculateFormulaAndClamp(EntryValue);
                AutoModExampleResultTextBox.Text = NewValue.ToString();

                if (DTEData.EntryClass.EntryValueOnProjectLoadFromInput != null && DTEData.EntryClass.EntryValueOnProjectLoadFromInput != "")
                {
                    double EntryInputValue = DTEMathMethods.GetEntryValueFrom(DTEData.EntryClass, DTEMathMethods.EntryValueSource.FromInput);
                    if (EntryValue == EntryInputValue)
                    {
                        AutoModExampleResultUsingInputTextBox.Text = "Same";
                        AutoModExampleResultUsingInputTextBox.IsEnabled = false;
                        ButtonApplyAutoModUsingInput.IsEnabled = false;
                    }
                    else
                    {
                        double NewInputValue = CalculateFormulaAndClamp(EntryInputValue);
                        AutoModExampleResultUsingInputTextBox.Text = NewInputValue.ToString();
                        AutoModExampleResultUsingInputTextBox.IsEnabled = true;
                        ButtonApplyAutoModUsingInput.IsEnabled = true;
                    }
                }
                else 
                {
                    AutoModExampleResultUsingInputTextBox.Text = "Reload Project";
                    AutoModExampleResultUsingInputTextBox.IsEnabled = false;
                    ButtonApplyAutoModUsingInput.IsEnabled = false;
                }
                

                
            }
            catch
            {
                AutoModExampleResultTextBox.Text = "ERROR";
                AutoModExampleResultUsingInputTextBox.Text = "ERROR";
            }

        }

        double CalculateFormulaAndClamp(double EntryValueSoFar)
        {
            foreach (AutoModMathStep MathStep in MathFormula)
            {
                double mathvalue = double.Parse(MathStep.MathValue);

                if (MathStep.MathPiece == AutoModSimpleMathPieces.Plus) { EntryValueSoFar += mathvalue; }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.Plus) { EntryValueSoFar += mathvalue; }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.Minus) { EntryValueSoFar -= mathvalue; }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.Multiply) { EntryValueSoFar *= mathvalue; }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.Divide) { if (mathvalue != 0) EntryValueSoFar /= mathvalue; }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.Max) { EntryValueSoFar = Math.Min(EntryValueSoFar, mathvalue); }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.Min) { EntryValueSoFar = Math.Max(EntryValueSoFar, mathvalue); }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.Round) { EntryValueSoFar = Math.Round(EntryValueSoFar); }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.RoundUp) { EntryValueSoFar = Math.Ceiling(EntryValueSoFar); }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.RoundDown) { EntryValueSoFar = Math.Floor(EntryValueSoFar); }
                else if (MathStep.MathPiece == AutoModSimpleMathPieces.SetToX) { EntryValueSoFar = mathvalue; }

            }
            EntryValueSoFar = DTEMathMethods.ReturnClampedValueForEntrySizeAndSign(DTEData.EntryClass, EntryValueSoFar);
            return EntryValueSoFar;
        }

        private void ApplyAutoModMath(object sender, RoutedEventArgs e)
        {
            DoTheMath("Normal");
        }

        private void ApplyAutoModMathUsingInput(object sender, RoutedEventArgs e)
        {
            DoTheMath("Input");
        }
        private void ApplyAutoModMathUsingLarger(object sender, RoutedEventArgs e)
        {
            DoTheMath("Larger");
        }
        private void ApplyAutoModMathUsingSmaller(object sender, RoutedEventArgs e)
        {
            DoTheMath("Smaller");
        }

        private void DoTheMath(string mode) 
        {
            //Almost everything here uses doubles instead of ints to make ABSOLUTELY FUCKING SURE nothing EVER goes out of range,
            //even when adding more value types or using large negatives from super robot wars / disgaea.

            if (DTEData.EntryClass == null) { return; }
            if (DTEData.WorkshopXaml.IsPreviewMode == true) { return; }

            bool SaveContinueCheck = PixelWPF.LibraryPixel.NotificationConfirm("Did you save first?", "This will apply the current math formula, to every value in the current list, for the selected entry.\n\nFor example:\nIf your selecting an enemy's HP Entry, and you set *2, then ALL enemys will get *2 HP. \n\nAnyway, are you sure you want to do this? Select No if you want to save first.\n\nReminder: Changes are *never* automatically saved. If you don't like them, simply reload the project. ");
            if (SaveContinueCheck == false) { return; }

            Entry EntryX = DTEData.EntryClass;

            if (EntryX.IsEntryHidden == true || EntryX.IsTextInUse == true) { return; } //prevents users from axidentally modding values that should be otherwise already disabled.

            foreach (AutoModMathStep mathstep in MathFormula) { if (mathstep.MathValue == "") { return; } }


            int FinalItem = 0;
            try
            {
                if (DTEData.NameTable.TextTableItemCount != 0) { FinalItem = DTEData.NameTable.TextTableItemCount; }
                if (DTEData.NameTable.TextTableItemCount == 0)
                {
                    int ItemCount = 0;
                    foreach (var Item in DTEData.NameTable.ItemList)
                    {
                        if (Item.IsFolder == false)
                        {
                            ItemCount++;
                        }
                    }
                    FinalItem = ItemCount;
                }
                FinalItem = FinalItem - int.Parse(FormulaDoNotModTextBox.Text); //Allows users to NOT mod the final X number of items in the list.
            }
            catch
            {
                PixelWPF.LibraryPixel.NotificationNegative("Error: ",
                    "An error happened during the first step of auto-mod. " +
                    "In this step, it simply tries to count how many items it's going to mod. " +
                    "This error can probably only appear if the editor is not getting it's item names from an actual game file. " +
                    "\n\n" +
                    "Anyway, Auto-mod will now cancel. Nothing has been changed."
                    );
                return;
            }


            for (int i = 0; i < FinalItem; i++)
            {

                try
                {
                    //Get Current Value Step
                    double CurrentValue = 0;
                    double CurrentValueFromInput = 0;
                    if (EntryX.Endianness == "1")
                    {
                        CurrentValue = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset];
                        CurrentValueFromInput = LibraryGES.GetMirrorGameFileFromInput(DTEData.DataTable.FileDataTable, DTEData.WorkshopData).FileBytes[DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset];
                    }
                    if (EntryX.Endianness == "2B")
                    {
                        ushort value2 = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);
                        ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
                        CurrentValue = swappedValue2;

                        ushort valueI = BitConverter.ToUInt16(LibraryGES.GetMirrorGameFileFromInput(DTEData.DataTable.FileDataTable, DTEData.WorkshopData).FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);
                        ushort swappedValueI = (ushort)IPAddress.HostToNetworkOrder((short)valueI); // Swap the endianness
                        CurrentValueFromInput = swappedValueI;
                    }
                    if (EntryX.Endianness == "4B")
                    {
                        uint value = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);
                        byte[] valueBytes = BitConverter.GetBytes(value);
                        Array.Reverse(valueBytes);
                        uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
                        CurrentValue = swappedValue;

                        uint valueI = BitConverter.ToUInt32(LibraryGES.GetMirrorGameFileFromInput(DTEData.DataTable.FileDataTable, DTEData.WorkshopData).FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);
                        byte[] valueBytesI = BitConverter.GetBytes(valueI);
                        Array.Reverse(valueBytesI);
                        uint swappedValueI = BitConverter.ToUInt32(valueBytesI, 0);
                        CurrentValueFromInput = swappedValueI;
                    }
                    if (EntryX.Endianness == "2L")
                    {
                        CurrentValue = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);

                        CurrentValueFromInput = BitConverter.ToUInt16(LibraryGES.GetMirrorGameFileFromInput(DTEData.DataTable.FileDataTable, DTEData.WorkshopData).FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);
                    }
                    if (EntryX.Endianness == "4L")
                    {
                        CurrentValue = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);

                        CurrentValueFromInput = BitConverter.ToUInt32(LibraryGES.GetMirrorGameFileFromInput(DTEData.DataTable.FileDataTable, DTEData.WorkshopData).FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset);
                    }


                    //Step: Adjust for Signed Values.
                    if (EntryX.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                    {
                        if (EntryX.Bytes == 1)
                        {
                            if (CurrentValue > 127)
                            {
                                CurrentValue = CurrentValue - 256;
                            }
                            if (CurrentValueFromInput > 127)
                            {
                                CurrentValueFromInput = CurrentValueFromInput - 256;
                            }
                        }
                        if (EntryX.Bytes == 2)
                        {
                            if (CurrentValue > 32767)
                            {
                                CurrentValue = CurrentValue - 65536;
                            }
                            if (CurrentValueFromInput > 32767)
                            {
                                CurrentValueFromInput = CurrentValueFromInput - 65536;
                            }
                        }
                        if (EntryX.Bytes == 4)
                        {
                            if (CurrentValue > 2147483647)
                            {
                                CurrentValue = CurrentValue - 4294967296;
                            }
                            if (CurrentValueFromInput > 2147483647)
                            {
                                CurrentValueFromInput = CurrentValueFromInput - 4294967296;
                            }
                        }
                    }

                    //Step Calc
                    double NewValue = CalculateFormulaAndClamp(CurrentValue);
                    double NewValueFromInput = CalculateFormulaAndClamp(CurrentValueFromInput);

                    
                    if (mode == "Normal") 
                    {
                        savestep(NewValue);
                    }
                    if (mode == "Input") 
                    {
                        savestep(NewValueFromInput);
                    }
                    if (mode == "Larger")
                    {
                        if (NewValue < NewValueFromInput) { NewValue = NewValueFromInput; }
                        savestep(NewValue);
                    }
                    if (mode == "Smaller")
                    {
                        if (NewValue > NewValueFromInput) { NewValue = NewValueFromInput; }
                        savestep(NewValue);
                    }


                    void savestep(double SaveValue) 
                    {
                        //Step Swap Signage again
                        if (SaveValue < 0)
                        {
                            if (EntryX.Bytes == 1)
                            {
                                SaveValue = SaveValue + 256;
                            }
                            if (EntryX.Bytes == 2)
                            {
                                SaveValue = SaveValue + 65536;
                            }
                            if (EntryX.Bytes == 4)
                            {
                                SaveValue = SaveValue + 4294967296;
                            }
                        }


                        //Saving Step
                        string Result = SaveValue.ToString();

                        if (EntryX.Endianness == "1")  // This is saving 1 Byte Size?   // First 1 byte save
                        {
                            Byte.TryParse(Result, out byte value8);
                            { ByteManager.ByteWriter(value8, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset); }
                        }
                        if (EntryX.Endianness == "2L")
                        {
                            UInt16.TryParse(Result, out ushort value16);
                            { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset); } //First 2 byte save

                        }
                        if (EntryX.Endianness == "4L")
                        {
                            UInt32.TryParse(Result, out uint value32);
                            { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset); } //First 4 byte save

                        }
                        if (EntryX.Endianness == "2B")
                        {
                            UInt16.TryParse(Result, out ushort value16);
                            value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                            { ByteManager.ByteWriter(value16, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset); } //First 2 byte save

                        }
                        if (EntryX.Endianness == "4B")
                        {
                            UInt32.TryParse(Result, out uint value32);
                            byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                            Array.Reverse(valueBytes); // Swap the endianness
                            value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                            { ByteManager.ByteWriter(value32, DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryX.DataTableRowSize) + EntryX.RowOffset); } //First 4 byte save

                        }
                    }
                    


                }
                catch
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: ???",
                        "An error happened during the actual modifying of data in memory." +
                        "\nThis means some of the items have been changed, and others have not." +
                        "\nNothing has been saved to actual files on the computer, YET, so don't worry." +
                        "\n" +
                        "\nHowever, this is a very serious error. It is strongly recommended you close the program WITHOUT saving your game files." +
                        "\n" +
                        "\nI chose not to automatically force crash the program, to give you a chance to save some non-game file related things first. " +
                        "Before you close everything, in the workshop menu you may save your documents, events, and editors, but absolutely do not save your game files. " +
                        "If you do, you will save them with only some items being changed, but not all of them."
                    );
                    return;
                }


            }

            TreeViewItem itemm = DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
            itemm.IsSelected = false;
            itemm.IsSelected = true;
        }

        private void FormulaHelp(object sender, RoutedEventArgs e)
        {
            PixelWPF.LibraryPixel.Notification("Auto Mod Help",
                "This will apply the current math formula, to every value in the current list, for the selected entry." +
                "\n\nFor example:" +
                "\nIf your select an Enemy's HP Entry, and you set Multiply by 2, and click apply, then ALL enemys will get *2 HP." +
                "\n" +
                "\nPS: Atleast at the time of writing this, the entry history does not show changes from a math formula.");
        }

        
    }
}
