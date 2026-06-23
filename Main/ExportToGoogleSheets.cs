using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameEditorStudio
{
    internal class ExportToGoogleSheets
    {

        //This file has functions to convert an editor into a csv, that can be imported to google sheets (or excel).
        //this makes it easy to share editors online, and crowdsource editor creation via public entry labeling.
        //It does nothing else, and is not interconnected to anything else other then the convert to google sheet button in a workshop.

        public void ExportAllDataTables(Workshop TheWorkshop)
        {
            foreach (DataTableEditorData DataTableData in TheWorkshop.WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                ToGoogleSheet(TheWorkshop, DataTableData);

            }
        }



        public void ToGoogleSheet(Workshop TheWorkshop, DataTableEditorData DTEData)
        {
            if (DTEData.NameTable == null) { return; }
            if (DTEData.DataTable == null) { return; }
            //if (DTEData.DescriptionTableList == null) { return;  }

            //It exports in Decimal and Hex, and in both origonal game order, and current editor sorted order. 
            //So it exports 4 folders in total, and it's run for every editor, so all 4 folders get exports for every editor in that folders format.
            string EditorDataDecimalCurrent = "";
            string EditorDataHexCurrent = "";
            string EditorDataDecimalOrigonal = "";
            string EditorDataHexOrigonal = "";

            //Skip the very top left of the google sheet
            EditorDataDecimalCurrent = EditorDataDecimalCurrent + " ,";
            EditorDataHexCurrent = EditorDataHexCurrent + " ,";
            EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + " ,";
            EditorDataHexOrigonal = EditorDataHexOrigonal + " ,";


            int Columns = DTEData.DataTableEditorData.DataTable.DataTableRowSize; //crash M2
            var TheFile = DTEData.DataTableEditorData.DataTable.FileDataTable.FileBytes;

            for (int ColumnInRow1 = 0; ColumnInRow1 != Columns; ColumnInRow1++) //Setting up the first row, to get Column / Entry names.
            {
                foreach (Entry entry in DTEData.DataTableEditorData.MasterEntryList)
                {
                    if (entry.RowOffset == ColumnInRow1)
                    {
                        if (entry.IsTextInUse == false && entry.Name != "") 
                        {
                            EditorDataDecimalCurrent = EditorDataDecimalCurrent + entry.Name + ",";
                            EditorDataHexCurrent = EditorDataHexCurrent + entry.Name + ",";
                            EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + entry.Name + ",";
                            EditorDataHexOrigonal = EditorDataHexOrigonal + entry.Name + ",";
                        }
                        if (entry.IsTextInUse == false && entry.Name == "")
                        {
                            EditorDataDecimalCurrent = EditorDataDecimalCurrent + entry.RowOffset + ",";
                            EditorDataHexCurrent = EditorDataHexCurrent + entry.RowOffset + ",";
                            EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + entry.RowOffset + ",";
                            EditorDataHexOrigonal = EditorDataHexOrigonal + entry.RowOffset + ",";
                        }
                        if (entry.IsTextInUse == true)
                        {
                            EditorDataDecimalCurrent = EditorDataDecimalCurrent + "TEXT" + ",";
                            EditorDataHexCurrent = EditorDataHexCurrent + "TEXT" + ",";
                            EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + "TEXT" + ",";
                            EditorDataHexOrigonal = EditorDataHexOrigonal + "TEXT" + ",";
                        }

                    }
                }
            }
            EditorDataDecimalCurrent = EditorDataDecimalCurrent + "\r\n";
            EditorDataHexCurrent = EditorDataHexCurrent + "\r\n";
            EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + "\r\n";
            EditorDataHexOrigonal = EditorDataHexOrigonal + "\r\n";

            int Rows = 0;
            foreach (var Item in DTEData.DataTableEditorData.NameTable.ItemList)
            {
                if (Item.IsFolder == true)
                {
                    continue;
                    
                }

                //The names of each item. Item folders are ID 0 so the first item name might be a folder lol
                EditorDataDecimalCurrent = EditorDataDecimalCurrent + Item.ItemIndex + ": " + Item.ItemName + ","; 
                EditorDataHexCurrent = EditorDataHexCurrent + Item.ItemIndex + ": " + Item.ItemName + ",";
                

                for (int c = 0; c != Columns; c++)
                {
                    //For Decimal Export
                    string TheByte = TheFile[DTEData.DataTableEditorData.DataTable.DataTableStart + (Item.ItemIndex * Columns) + c].ToString("X2");
                    int decimalValue = Convert.ToInt32(TheByte, 16);
                    EditorDataDecimalCurrent = EditorDataDecimalCurrent + decimalValue.ToString() + ",";

                    //For Hex Export
                    EditorDataHexCurrent = EditorDataHexCurrent + TheFile[DTEData.DataTableEditorData.DataTable.DataTableStart + (Item.ItemIndex * Columns) + c].ToString("X2") + ",";
                }
                EditorDataDecimalCurrent = EditorDataDecimalCurrent + "\r\n";
                EditorDataHexCurrent = EditorDataHexCurrent + "\r\n";
                Rows++;
            }

            //Same as above but for Origonal order.
            for (int Index = 0; Index != Rows; Index++) 
            {
                foreach (var Item in DTEData.DataTableEditorData.NameTable.ItemList) 
                {
                    if (Item.IsFolder == true || Item.ItemIndex != Index)
                    {
                        continue;
                    }

                    EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + Item.ItemIndex + ": " + Item.ItemName + ",";
                    EditorDataHexOrigonal = EditorDataHexOrigonal + Item.ItemIndex + ": " + Item.ItemName + ",";

                    for (int c = 0; c != Columns; c++)
                    {
                        //For Decimal Export
                        string TheByte = TheFile[DTEData.DataTableEditorData.DataTable.DataTableStart + (Index * Columns) + c].ToString("X2");
                        int decimalValue = Convert.ToInt32(TheByte, 16);
                        EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + decimalValue.ToString() + ",";

                        //For Hex Export
                        EditorDataHexOrigonal = EditorDataHexOrigonal + TheFile[DTEData.DataTableEditorData.DataTable.DataTableStart + (Index * Columns) + c].ToString("X2") + ",";
                    }
                    EditorDataDecimalOrigonal = EditorDataDecimalOrigonal + "\r\n";
                    EditorDataHexOrigonal = EditorDataHexOrigonal + "\r\n";
                    Rows++;
                }

                
            }


            //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
            Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Decimal (Editor Order)");
            System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Decimal (Editor Order)\\" + DTEData.EditorName + ".csv", EditorDataDecimalCurrent);

            Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Hex (Editor Order)");
            System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Hex (Editor Order)\\" + DTEData.EditorName + ".csv", EditorDataHexCurrent);

            Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Decimal (Origonal Order)");
            System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Decimal (Origonal Order)\\" + DTEData.EditorName + ".csv", EditorDataDecimalOrigonal);

            Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Hex (Origonal Order)");
            System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Hex (Origonal Order)\\" + DTEData.EditorName + ".csv", EditorDataHexOrigonal);

            File.Create(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Exported " + DateTime.Now.ToString("MMMM d yyyy h;mmtt") + ".txt").Dispose(); ;
            LibraryGES.OpenFolder(LibraryGES.ApplicationLocation + "\\Editor Exports\\" + TheWorkshop.WorkshopData.WorkshopName);

        }








    }
}
