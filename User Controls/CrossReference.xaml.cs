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
        Workshop TheWorkshop;

        public UserControlCrossReference()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LoadEvent);
        }

        public void LoadEvent(object sender, RoutedEventArgs e) 
        {
            var parentWindow = Window.GetWindow(this);

            if (parentWindow is Workshop workshopWindow)
            {                
                TheWorkshop = workshopWindow;
                TheWorkshop.TheCrossReference = this;
            }
        }
                

        public void FillLearnBox(Editor TheEditor, Entry TheEntry)
        {
            if (TheWorkshop.IsPreviewMode == true) 
            {
                return;
            }
            

            EntryValueInsightDataGrid.Items.Clear();

            int Goal = TheWorkshop.EditorClass.StandardEditorData.NameTableItemCount;
            if (Goal == 0) //Makes editors that don't get item names from a file work with sheet exports.
            {
                foreach (var Item in TheWorkshop.EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList)
                {
                    if (Item.IsFolder == false)
                    {
                        Goal++;
                    }
                }
            }
            //Dictionary<int, NumberCount> counts = new Dictionary<int, NumberCount>();
            Dictionary<long, NumberCount> counts = new();

            for (int i = 0; i < Goal; i++)
            {
                StandardEditorData TheData = TheWorkshop.EditorClass.StandardEditorData;

                //if bytes
                //int Num = TheData.FileDataTable.FileBytes[TheWorkshop.EditorClass.StandardEditorData.DataTableStart + (i * TheWorkshop.EntryClass.DataTableRowSize) + TheWorkshop.EntryClass.RowOffset];



                long num = 0;

                int offset = TheData.DataTableStart + (i * TheEntry.DataTableRowSize) + TheEntry.RowOffset;

                bool isSigned = TheEntry.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed;

                if (TheEntry.Endianness == "1")
                {
                    byte b = TheData.FileDataTable.FileBytes[offset];
                    num = isSigned ? (sbyte)b : b;
                }
                else if (TheEntry.Endianness == "2B")
                {
                    if (isSigned)
                        num = BitConverter.ToInt16(TheData.FileDataTable.FileBytes, offset);
                    else
                        num = BitConverter.ToUInt16(TheData.FileDataTable.FileBytes, offset);
                }
                else if (TheEntry.Endianness == "2L")
                {
                    byte[] bytes = TheData.FileDataTable.FileBytes.Skip(offset).Take(2).ToArray();
                    Array.Reverse(bytes);
                    if (isSigned)
                        num = BitConverter.ToInt16(bytes, 0);
                    else
                        num = BitConverter.ToUInt16(bytes, 0);
                }
                else if (TheEntry.Endianness == "4B")
                {
                    if (isSigned)
                        num = BitConverter.ToInt32(TheData.FileDataTable.FileBytes, offset);
                    else
                        num = BitConverter.ToUInt32(TheData.FileDataTable.FileBytes, offset);
                }
                else if (TheEntry.Endianness == "4L")
                {
                    byte[] bytes = TheData.FileDataTable.FileBytes.Skip(offset).Take(4).ToArray();
                    Array.Reverse(bytes);
                    if (isSigned)
                        num = BitConverter.ToInt32(bytes, 0);
                    else
                        num = BitConverter.ToUInt32(bytes, 0);
                }


                if (counts.ContainsKey(num))
                {
                    counts[num].Count++;
                    counts[num].RowIndices.Add(i);
                }
                else
                {
                    counts[num] = new NumberCount { Number = num, Count = 1, RowIndices = new List<int> { i } };
                }

                //int Num = Int32.Parse(value); 

                //if (counts.ContainsKey(Num))
                //{
                //    counts[Num].Count++;
                //    counts[Num].RowIndices.Add(i);
                //}
                //else
                //{
                //    counts[Num] = new NumberCount { Number = Num, Count = 1, RowIndices = new List<int> { i } };
                //}
            }

            List<long> sortedKeys = counts.Keys.ToList();
            sortedKeys.Sort();

            foreach (long key in sortedKeys)
            {
                EntryValueInsightDataGrid.Items.Add(counts[key]);
            }

            
        }
    }
}
