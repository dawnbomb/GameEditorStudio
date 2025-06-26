using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GameEditorStudio
{
    internal class StandardEditorMethods
    {

        public void CreateNewGroup(Entry EntryClass) 
        {
            Column EntryColumn = EntryClass.EntryColumn;


        }

        public void MoveEntrysToEntry(List<Entry> EntrysListToMove, Entry EntryToMoveUnder)
        {
            if (EntrysListToMove.Contains(EntryToMoveUnder)) 
            {
                return;
            }

            int i = 0;
            Column BeforeColumn = EntrysListToMove[0].EntryColumn; //Save the column before we change it, so we can delete it later if needed.
            Column AfterColumn = EntryToMoveUnder.EntryColumn; //Save the column after we change it, so we can delete it later if needed.

            foreach (Entry EntryToMove in EntrysListToMove) 
            {
                EntryToMove.EntryColumn.ColumnGrid.Children.Remove(EntryToMove.EntryBorder);
                EntryToMove.EntryColumn.EntryList.Remove(EntryToMove);

                int ToIndex = EntryToMoveUnder.EntryColumn.EntryList.IndexOf(EntryToMoveUnder) + i; // the i fixes a bug where entry list drops in reverse order. 

                EntryToMoveUnder.EntryColumn.ColumnGrid.Children.Insert(ToIndex + 2, EntryToMove.EntryBorder);
                EntryToMoveUnder.EntryColumn.EntryList.Insert(ToIndex + 1, EntryToMove);

                EntryToMove.EntryColumn = EntryToMoveUnder.EntryColumn; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                EntryToMove.EntryRow = EntryToMoveUnder.EntryRow;

                i++;
            }


            DeleteEmptyColumnsAndMakeNewOnes(EntryToMoveUnder.EntryEditor.StandardEditorData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);

        }

        public void MoveEntrysToColumn(List<Entry> EntrysListToMove, Column Column) 
        {
            Column BeforeColumn = EntrysListToMove[0].EntryColumn; //Save the column before we change it, so we can delete it later if needed.
            Column AfterColumn = Column; //Save the column after we change it, so we can delete it later if needed.

            foreach (Entry EntryToMove in EntrysListToMove)
            {
                EntryToMove.EntryColumn.ColumnGrid.Children.Remove(EntryToMove.EntryBorder);
                EntryToMove.EntryColumn.EntryList.Remove(EntryToMove);

                Column.ColumnGrid.Children.Add(EntryToMove.EntryBorder);
                Column.EntryList.Add(EntryToMove);

                EntryToMove.EntryColumn = Column; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                EntryToMove.EntryRow = Column.ColumnRow;
            }

            DeleteEmptyColumnsAndMakeNewOnes(Column.ColumnRow.SWData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);
        }

        public void EntryActivate(Workshop TheWorkshop, Entry EntryClass) 
        {
            Editor EditorClass = EntryClass.EntryEditor;

            EntryManager EntryData = new();
            EntryData.EntryBecomeActive(EntryClass);
            EntryData.UpdateEntryProperties(TheWorkshop, EditorClass);

            //EditorClass.SWData.EditorTopBar.EntryNoteBox.Text = EntryClass.EntryTooltip;
            TheWorkshop.EntryNoteTextbox.Text = EntryClass.Notepad;


            if (TheWorkshop.ListTab.IsSelected == true) //Band-aid code fix to make the workshops "list" tab stop being selected when you select an entry, but this should really be inside the EntryBecomeActive function...
            {
                foreach (TabItem tabItem in TheWorkshop.MainTabControl.Items)
                {

                    if (tabItem.Header != null && tabItem.Header.ToString() == TheWorkshop.PreviousTabName)
                    {
                        tabItem.IsSelected = true;
                        break;
                    }
                }
            }

            if (TheWorkshop.TheCrossReference != null) 
            {
                TheWorkshop.TheCrossReference.FillLearnBox(EditorClass, EntryClass);
            }
            
        }

        public void DeleteEmptyColumnsAndMakeNewOnes(StandardEditorData StandardEditorData)
        {
            foreach (Category CatClass in StandardEditorData.CategoryList.ToList()) //Delete any empty columns in each category.
            {
                foreach (Column ColumnClass in CatClass.ColumnList.ToList())
                {
                    if (ColumnClass.EntryList == null || ColumnClass.EntryList.Count == 0) //Delete column if it's empty. 
                    {
                        StandardEditorData.TheEditor.Workshop.ColumnDelete(ColumnClass);
                    }

                }
            }

            foreach (Category CatClass in StandardEditorData.CategoryList.ToList()) //Create a empty column at the end of each category.
            {
                Column LastColumn = CatClass.ColumnList.Last();

                if (LastColumn.EntryList.Count != 0)
                {
                    StandardEditorData.TheEditor.Workshop.CreateNewColumnRight(LastColumn);
                }

            }
        }


        public void LabelWidth(Column ColumnClass)
        {


            double maxWidth = 0;
            var EntryList = ColumnClass.EntryList;

            // Measure the desired width of each label without restrictions
            foreach (Entry entry in EntryList)
            {
                entry.EntryLabel.MinWidth = 0;
                entry.EntryLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double labelWidth = entry.EntryLabel.DesiredSize.Width;
                if (labelWidth > maxWidth)
                {
                    maxWidth = labelWidth;
                }
            }

            // Set the MinWidth of each label to the widest value
            foreach (Entry entry in EntryList)
            {
                entry.EntryLabel.MinWidth = maxWidth;
            }
        }





    }
}
