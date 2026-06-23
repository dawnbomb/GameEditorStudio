using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static OfficeOpenXml.ExcelErrorValue;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for UserControlCrossReference.xaml
    /// </summary>
    public partial class UserControlCrossReference : UserControl
    {
        public UserControlCrossReference()
        {
            InitializeComponent();
        }

        public async void FillLearnBox(DataTableEditorData DTEData)
        {   

            EntryValueInsightDataGrid.Items.Clear();

            // Safety Checks
            if (DTEData.WorkshopXaml.IsPreviewMode == true) { return; }
            if (DTEData.NameTable == null) { return; }

            Entry TheEntry = DTEData.EntryClass;
            int startID = DTEData.NameTable.TextTableFirstNameID; // Get the starting ID (e.g., 100)

            // 1. Determine how many items to loop through
            int Goal = DTEData.NameTable.TextTableItemCount;
            if (Goal == 0)
            {
                foreach (var Item in DTEData.NameTable.ItemList)
                {
                    if (Item.IsFolder == false) { Goal++; }
                }
            }

            // 2. Map the values found in the file
            Dictionary<long, NumberCount> counts = new();

            for (int i = 0; i < Goal; i++)
            {
                long DecValueNum = 0;
                int offset = DTEData.DataTable.DataTableStart + (i * TheEntry.DataTableRowSize) + TheEntry.RowOffset;
                bool isSigned = TheEntry.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed;

                // Reading the bytes based on Endianness
                if (TheEntry.Endianness == "1")
                {
                    byte b = DTEData.DataTable.FileDataTable.FileBytes[offset];
                    DecValueNum = isSigned ? (sbyte)b : b;
                }
                else if (TheEntry.Endianness == "2B")
                {
                    byte[] bytes = DTEData.DataTable.FileDataTable.FileBytes.Skip(offset).Take(2).Reverse().ToArray();
                    DecValueNum = isSigned ? BitConverter.ToInt16(bytes, 0) : BitConverter.ToUInt16(bytes, 0);
                }
                else if (TheEntry.Endianness == "2L")
                {
                    DecValueNum = isSigned ? BitConverter.ToInt16(DTEData.DataTable.FileDataTable.FileBytes, offset)
                                           : BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, offset);                    
                }
                else if (TheEntry.Endianness == "4B")
                {
                    byte[] bytes = DTEData.DataTable.FileDataTable.FileBytes.Skip(offset).Take(4).Reverse().ToArray();
                    DecValueNum = isSigned ? BitConverter.ToInt32(bytes, 0) : BitConverter.ToUInt32(bytes, 0);
                }
                else if (TheEntry.Endianness == "4L")
                {
                    DecValueNum = isSigned ? BitConverter.ToInt32(DTEData.DataTable.FileDataTable.FileBytes, offset)
                                          : BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, offset);                    
                }

                // Add to dictionary
                if (counts.ContainsKey(DecValueNum))
                {
                    counts[DecValueNum].Count++;
                    counts[DecValueNum].RowIndices.Add(i);
                }
                else
                {
                    counts[DecValueNum] = new NumberCount { Number = DecValueNum, Count = 1, RowIndices = new List<int> { i } };
                }
            }

            // 3. Sort and convert indices to the specific "Item ID#" format
            List<long> sortedKeys = counts.Keys.ToList();
            sortedKeys.Sort();

            foreach (long key in sortedKeys)
            {
                var entry = counts[key];

                // This line does the magic: (Index + StartID)
                entry.RowIndicesAsString = string.Join(", ", entry.RowIndices.Select(idx => (idx + startID).ToString()));

                EntryValueInsightDataGrid.Items.Add(entry);
            }

            DTEData.EditorRightBar.LabelUniqueValueCount.Content = $"Unique Value Count: {counts.Count}";
        }



        public class NumberCount
        {
            public long Number { get; set; }
            public int Count { get; set; }
            public List<int> RowIndices { get; set; } = new List<int>();
            // This is what the DataGrid binds to
            public string RowIndicesAsString { get; set; }
        }
    }
}
