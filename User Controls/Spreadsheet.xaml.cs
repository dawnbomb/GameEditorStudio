using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameEditorStudio
{

    public class ValueInListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;

            string cellText = value.ToString();
            // The parameter will be our "0,00,000,None" list
            string[] allowedValues = parameter.ToString().Split(',');

            return allowedValues.Contains(cellText);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


    public partial class Spreadsheet : UserControl
    {
        string mode { get; set; }
        WorkshopData workshopData { get; set; }
        DataTableEditorData DTEData { get; set; }

        public Spreadsheet(WorkshopData workshopData, DataTableEditorData dtedata)
        {
            InitializeComponent();

            this.workshopData = workshopData;
            this.DTEData = dtedata;

            HexOrigonalOrder(null, null); //default to Hex Origonal order on load.
        }

        private void HexOrigonalOrder(object sender, RoutedEventArgs e) { RefreshGrid(true, true); mode = "HexOrigonal"; }
        private void HexEditorOrder(object sender, RoutedEventArgs e) { RefreshGrid(true, false); mode = "HexEditor"; } 
        private void DecEditorOrder(object sender, RoutedEventArgs e) { RefreshGrid(false, false); mode = "DecOrigonal"; }
        private void DecOrigonalOrder(object sender, RoutedEventArgs e) { RefreshGrid(false, true); mode = "DecEditor"; } 

        private void TriggerRefreshGrid(object sender, RoutedEventArgs e) 
        { 
            if (mode == "HexOrigonal") { RefreshGrid(true, true); }
            if (mode == "HexEditor") { RefreshGrid(true, false); }
            if (mode == "DecOrigonal") { RefreshGrid(false, false); }
            if (mode == "DecEditor") { RefreshGrid(false, true); }
        } 

        private void RefreshGrid(bool isHex, bool isOriginalOrder)
        {
            if (DTEData?.DataTableEditorData?.DataTable == null) return;

            System.Data.DataTable table = new System.Data.DataTable();
            int rowSize = DTEData.DataTableEditorData.DataTable.DataTableRowSize;
            var fileBytes = DTEData.DataTableEditorData.DataTable.FileDataTable.FileBytes;
            int startOffset = DTEData.DataTableEditorData.DataTable.DataTableStart;
            bool doMerge = MergeCheckbox.IsChecked == true;

            // 1. Requirement 1: Display Adjusted Item Name
            table.Columns.Add("Item Name", typeof(string));

            // 2. Build Columns (Skipping IsMerged)
            foreach (var entry in DTEData.DataTableEditorData.MasterEntryList.OrderBy(x => x.RowOffset))
            {
                // Requirement 3: Skip if this entry is already merged into a previous one
                if (doMerge && entry.IsMerged) continue;

                string colName = string.IsNullOrEmpty(entry.Name) ? entry.RowOffset.ToString() : entry.Name;
                if (entry.IsTextInUse) colName = "TEXT";

                int duplicateCounter = 1;
                string uniqueColName = colName;
                while (table.Columns.Contains(uniqueColName))
                    uniqueColName = $"{colName} ({duplicateCounter++})";

                table.Columns.Add(uniqueColName, typeof(string));
            }

            // 3. Fill Rows
            var query = DTEData.DataTableEditorData.NameTable.ItemList.Where(x => !x.IsFolder);
            if (isOriginalOrder) query = query.OrderBy(x => x.ItemIndex);

            foreach (var item in query)
            {
                System.Data.DataRow row = table.NewRow();

                int displayID = item.ItemIndex + DTEData.NameTable.TextTableFirstNameID;
                row[0] = $"{displayID}: {item.ItemName}";
                if (item.ItemNote != "" && NoteCheckbox.IsChecked == true) { row[0] = $"{displayID}: {item.ItemName}   ({item.ItemNote})"; }

                int colIdx = 1;
                foreach (var entry in DTEData.DataTableEditorData.MasterEntryList.OrderBy(x => x.RowOffset))
                {
                    // 1. If Merging is ON, skip the "slave" entries
                    if (doMerge && entry.IsMerged) continue;

                    int bytePos = startOffset + (item.ItemIndex * rowSize) + entry.RowOffset;

                    // 2. Determine how many bytes to read
                    // If merge is OFF, we ONLY read 1 byte (the standard view)
                    // If merge is ON, we read the full count (2, 4, etc.)
                    int bytesToRead = doMerge ? entry.Bytes : 1;

                    if (bytePos + (bytesToRead - 1) < fileBytes.Length)
                    {
                        if (isHex)
                        {
                            // Hex View
                            string hexResult = "";
                            for (int i = 0; i < bytesToRead; i++)
                                hexResult += fileBytes[bytePos + i].ToString("X2") + " ";

                            row[colIdx] = hexResult.Trim();
                        }
                        else
                        {
                            // Decimal View
                            // If we are only reading 1 byte, we don't need endian logic
                            if (bytesToRead == 1)
                            {
                                row[colIdx] = fileBytes[bytePos].ToString();
                            }
                            else
                            {
                                row[colIdx] = CalculateDecimalValue(fileBytes, bytePos, bytesToRead, entry.Endianness);
                            }
                        }
                    }

                    colIdx++;
                }
                table.Rows.Add(row);
            }

            MainDataGrid.Tag = isHex ? "Hex" : "Decimal";
            MainDataGrid.ItemsSource = table.DefaultView;
            
        }

        // Helper to handle Endianness
        private string CalculateDecimalValue(byte[] data, int start, int length, string endian)
        {
            if (length <= 1) return data[start].ToString();

            byte[] segment = new byte[length];
            Array.Copy(data, start, segment, 0, length);

            // If the entry is Little Endian (2L, 4L), we might need to swap 
            // to match the System's expectation. 
            // But usually, if '2B' is appearing wrong, it's because BitConverter 
            // is auto-flipping. Let's force the flip ONLY for the 'L' types.

            bool isBigEndianEntry = endian.EndsWith("B", StringComparison.OrdinalIgnoreCase);

            if (isBigEndianEntry)
            {
                // Flip it so the math matches the display order
                Array.Reverse(segment);
            }

            if (length == 2) return BitConverter.ToUInt16(segment, 0).ToString();
            if (length == 4) return BitConverter.ToUInt32(segment, 0).ToString();

            return data[start].ToString();
        }




        private void CloseButton(object sender, RoutedEventArgs e)
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }
        }



        

    }


    
}
