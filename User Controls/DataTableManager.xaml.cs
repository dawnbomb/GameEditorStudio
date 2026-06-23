using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using Microsoft.VisualBasic;
using System.Text.Json;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for DataTableManager.xaml
    /// </summary>
    public partial class DataTableManager : UserControl
    {
        public DataTableEditorData DTEData { get; set; }

        public DataTableManager()
        {
            InitializeComponent();
            TabDataFile.Visibility = Visibility.Collapsed;
            TabAdvanced.Visibility = Visibility.Collapsed;
            TabJSON.Visibility = Visibility.Collapsed;
            TabXML.Visibility = Visibility.Collapsed;
            //Row size or File changes cause it to remake the entry list.


            #if DEBUG
            #else
            DataTableDebugButton.Visibility = Visibility.Collapsed; //Remove the debug button in release builds.
            #endif
        }

        public void SetupForDataTable(DataTableEditorData TheDTEData) 
        {
            DTEData = TheDTEData;

            if (TheDTEData.DataTable != null) 
            {
                //FileManager
                TextBoxDataTableBaseAddress.Text = TheDTEData.DataTable.DataTableStart.ToString();
                TextBoxDataTableRowSize.Text = TheDTEData.DataTable.DataTableRowSize.ToString();

                if (TheDTEData.DataTable.FileDataTable != null) { FileManager.Loaded += new RoutedEventHandler(SelectNameFile); }                
                void SelectNameFile(object sender, RoutedEventArgs e)
                {
                    foreach (TreeViewItem fileItem in LibraryGES.GetALLTreeViewItems(FileManager.TreeGameFiles)) //FileTreeExtraTable.Items
                    {
                        if (fileItem.Tag == TheDTEData.DataTable.FileDataTable)
                        {
                            fileItem.IsSelected = true;
                            break;
                        }
                    }
                }
            }
            

        }

        private void DebugButtonClick(object sender, RoutedEventArgs e)
        {
            TextBoxDataTableBaseAddress.Text = "8";
            TextBoxDataTableRowSize.Text = "176";

            foreach (TreeViewItem Item3 in FileManager.TreeGameFiles.Items) //FileTreeExtraTable.Items
            {
                GameFile TheFile = Item3.Tag as GameFile;
                if (TheFile.FileName == "skill.bin")
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
        }

        private void ButtonExitWithoutSaving(object sender, RoutedEventArgs e)
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }
        }

        private void ButtonSaveDataTable(object sender, RoutedEventArgs e)
        {
            //if (DataTableEditorData.NameTable == null) 
            //{
            //    PixelWPF.LibraryPixel.NotificationNegative("Name Table is not set","Please first set a name table.\n\nThis is really just because i was lazy and SUPER did not feel like programming in letting you use a data table before a name table. \n\nSorry~ x3");
            //    return;
            //}

            //Error checking (for missing info) 
            {
                TextBoxDataTableBaseAddress.Background = null;
                TextBoxDataTableRowSize.Background = null;

                if (TextBoxDataTableBaseAddress.Text == null || TextBoxDataTableBaseAddress.Text == "")
                {
                    TextBoxDataTableBaseAddress.Background = Brushes.Red;
                }
                if (TextBoxDataTableRowSize.Text == null || TextBoxDataTableRowSize.Text == "" || TextBoxDataTableRowSize.Text == "0")
                {
                    TextBoxDataTableRowSize.Background = Brushes.Red;
                }
                if (TextBoxDataTableRowSize.Text.Contains("a") || TextBoxDataTableRowSize.Text.Contains("s") || TextBoxDataTableRowSize.Text.Contains("d") || TextBoxDataTableRowSize.Text.Contains("w"))
                {
                    TextBoxDataTableRowSize.Background = Brushes.Red;
                }
                if (TextBoxDataTableBaseAddress.Background == Brushes.Red || TextBoxDataTableRowSize.Background == Brushes.Red)
                {
                    return;
                }

                if (FileManager.TreeGameFiles.SelectedItem == null)
                {
                    //LabelErrorNotice.Content = "Please select a file to make an editor with.";
                    //error = true;
                    return;
                }
            }





            //NewSWEditorData(); //This is actually in the Load Database file.
            ////It works by loading an editors worth of information into the database, then triggering the standard make an editor stuff.


            ////When making a new editor, i forcibly turn on symbology.
            //LibraryGES.ShowSymbology = true;
            //LibraryGES.ShowEntryAddress = true; //i now also force on E-IDs
            //LibraryGES.ShowHiddenEntrys = true;

            //foreach (DataTableEditorData DataTableData in TheWorkshop.WorkshopData.GameEditors.OfType<DataTableEditorData>())
            //{
            //    DataTableData.DataTableEditorData.TheXaml.UpdateEntryDecorationsForAllEditors();
            //    break;
            //}


            SaveNewDataTable();
        }

        private void SaveNewDataTable() 
        {
            if (DTEData.DataTable != null) 
            {
                if (PixelWPF.LibraryPixel.NotificationConfirm("Create a new data table?",
                "This will completly overwrite the current categorys, entrys, layout, and entry tooltips. " +
                "\n\nPS: If your just trying to change the start byte, click the editor tab and change it on the right bar. That keeps your layout and other data :3") == false)
                {
                    return;
                }
            }
            


            DTEData.DataTable = new(); //Just always make a new one for any change? 
            DTEData.MasterEntryList = new();
            DTEData.MergedEntryList = new();
            DTEData.HiddenEntryList = new();
            DTEData.CategoryList = new(); //and Create a new category list...
            DTEData.CategoryClass = null;
            DTEData.GroupClass = null;
            DTEData.EntryClass = null;

            Category CatClass = new(); //and add a new category into the new list...
            DTEData.CategoryList.Add(CatClass);
            CatClass.DTEData = DTEData.DataTableEditorData;
            //if (DataTableEditorData.DataTable == null) 
            //{

            //}



            {   //Set the data table data... 
                DTEData.DataTable.DataTableStart = int.Parse(TextBoxDataTableBaseAddress.Text);
                DTEData.DataTable.DataTableRowSize = int.Parse(TextBoxDataTableRowSize.Text);
                DTEData.DataTable.DataTableKey = PixelWPF.LibraryPixel.GenerateKey();

                var selectedItem = FileManager.TreeGameFiles.SelectedItem as TreeViewItem; //The file the user wants to make an editor for.
                DTEData.DataTable.FileDataTable = selectedItem.Tag as GameFile;
            }


            //Create entrys based on the new data table...
            for (int i = 0; i <= Int32.Parse(TextBoxDataTableRowSize.Text) - 1; i++)
            {
                //This is the default settings of every entry when a new editor is created.
                //All of these can be changed by the user, and need to be saved to XML, and loaded back from XML.
                //There exist some more as well, but those aren't strictly necessary to a new entry.

                Entry EntryClass = new();
                EntryClass.ParentCategory = CatClass; //This is the row this entry belongs to.
                EntryClass.ParentEditor = DTEData; //This is the editor this entry belongs to.
                EntryClass.ParentGrid = CatClass.ItemGrid;
                EntryClass.ParentGridItems = CatClass.GridItems;

                EntryClass.Row = i;
                EntryClass.Column = 1;

                CatClass.GridItems.Add(EntryClass);
                DTEData.MasterEntryList.Add(EntryClass);

                EntryClass.DataTableRowSize = Int32.Parse(TextBoxDataTableRowSize.Text);
                EntryClass.RowOffset = i;
                EntryClass.DataTableKey = DTEData.DataTable.DataTableKey;

                //Reminder this method is for creating a NEW editor, not loading one from a file. 
            }


            //Then make a new editor for it.
            DTEData.DTEXaml.GenerateUI();
            Closethis();

            {   //Turn on helpful stuff for a new data table
                LibraryGES.ShowSymbology = true;
                LibraryGES.ShowEntryAddress = true;
                LibraryGES.ShowHiddenEntrys = true;

                DTEMethods.UpdateHotbarForAllDTEEditors(DTEData.WorkshopData);
                foreach (DataTableEditorData DataTableData in DTEData.WorkshopXaml.WorkshopData.GameEditors.OfType<DataTableEditorData>())
                {
                    DataTableData.DataTableEditorData.DTEXaml.UpdateEntryDecorationsForAllEditors();
                    break;
                }

                
            }
        }


        private void ButtonDeleteDataTable(object sender, RoutedEventArgs e)
        {
            DTEData.DataTable = null;
                    
            DTEData.CategoryList = new();
            DTEData.MasterEntryList = new();
            DTEData.MergedEntryList = new();
            DTEData.HiddenEntryList = new();
            DTEData.CategoryClass = null;
            DTEData.GroupClass = null;
            DTEData.EntryClass = null;            

            DTEData.DTEXaml.GenerateUI();
            Closethis();
        }

        private void Closethis()
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }

            DTEData.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void DataSourceDropdownClosed(object sender, EventArgs e)
        {
            ComboBoxItem comboboxitem = ComboBoxTableType.SelectedItem as ComboBoxItem;

            if (comboboxitem.Name == "DataFile")
            {
                TabDataFile.IsSelected = true;
            }
            if (comboboxitem.Name == "Advanced")
            {
                TabAdvanced.IsSelected = true;
            }
            if (comboboxitem.Name == "XML")
            {
                TabXML.IsSelected = true;
            }
            if (comboboxitem.Name == "JSON")
            {
                TabJSON.IsSelected = true;
            }

        }


        //public void UpdateJSONTree()
        //{
        //    if (JSONFileManager.TreeGameFiles.SelectedItem == null)
        //    {
        //        return;
        //    }

        //    TreeViewItem Item = JSONFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
        //    if (Item == null) { return; }
        //    GameFile TheFile = Item.Tag as GameFile;
        //    if (TheFile == null) { return; }

        //    //if (TheFile.FileName) { }

        //    //JSONTree.Items
        //}

        public void UpdateJSONTree()
        {
            if (JSONFileManager.TreeGameFiles.SelectedItem is TreeViewItem item && item.Tag is GameFile theFile)
            {
                if (theFile.FileBytes == null || theFile.FileBytes.Length == 0) return;

                try
                {
                    JSONTree.Items.Clear();

                    // Convert your "Memory File" bytes to a ReadOnlySpan for the JSON parser
                    // This is very fast and doesn't require creating giant strings in memory
                    using (JsonDocument doc = JsonDocument.Parse(theFile.FileBytes))
                    {
                        PopulateJSONTree(doc.RootElement.Clone(), theFile.FileName, JSONTree);
                    }
                }
                catch (JsonException ex)
                {
                    JSONTree.Items.Add(new TreeViewItem { Header = "Invalid JSON Format: " + ex.LineNumber });
                }
            }
        }


        private void PopulateJSONTree(JsonElement element, string name, ItemsControl parentItem)
        {
            // 1. Determine if this is a "Parent" type (Object or Array)
            if (element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.Array)
            {
                TreeViewItem newItem = new TreeViewItem();
                string typeBracket = element.ValueKind == JsonValueKind.Array ? "[ ]" : "{ }";

                newItem.Header = $"{typeBracket} {name}";
                newItem.Tag = element; // We keep the data here for later!
                newItem.IsExpanded = false; // Keep it collapsed by default so it's clean

                parentItem.Items.Add(newItem);

                // 2. If it's an Object (like your "Properties" or "Rows" keys)
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in element.EnumerateObject())
                    {
                        // Only add children if they are also Parents (Objects or Arrays)
                        if (property.Value.ValueKind == JsonValueKind.Object ||
                            property.Value.ValueKind == JsonValueKind.Array)
                        {
                            PopulateJSONTree(property.Value, property.Name, newItem);
                        }
                    }
                }
                // 3. If it's an Array (like the very root of your file)
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        // If the array contains objects, let's peek inside them
                        if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                        {
                            // For UE DataTables, the first object usually has a "Name" property
                            string subName = $"Item {index}";
                            if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("Name", out JsonElement nameProp))
                            {
                                subName = nameProp.GetString();
                            }

                            PopulateJSONTree(item, subName, newItem);
                        }
                        index++;
                    }
                }
            }
        }
    }
}
