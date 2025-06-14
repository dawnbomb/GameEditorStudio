using System;
using System.Collections.Generic;
using System.Linq;
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
            Dictionary<int, NumberCount> counts = new Dictionary<int, NumberCount>();

            for (int i = 0; i < Goal; i++)
            {
                int Num = TheWorkshop.EditorClass.StandardEditorData.FileDataTable.FileBytes[TheWorkshop.EditorClass.StandardEditorData.DataTableStart + (i * TheWorkshop.EntryClass.DataTableRowSize) + TheWorkshop.EntryClass.RowOffset];

                if (counts.ContainsKey(Num))
                {
                    counts[Num].Count++;
                    counts[Num].RowIndices.Add(i);
                }
                else
                {
                    counts[Num] = new NumberCount { Number = Num, Count = 1, RowIndices = new List<int> { i } };
                }
            }

            List<int> sortedKeys = counts.Keys.ToList();
            sortedKeys.Sort();

            foreach (int key in sortedKeys)
            {
                EntryValueInsightDataGrid.Items.Add(counts[key]);
            }

            
        }
    }
}
