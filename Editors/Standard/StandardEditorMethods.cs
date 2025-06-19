using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditorStudio
{
    internal class StandardEditorMethods
    {
                
        public void MoveEntrysToEntry(List<Entry> EntrysListToMove, Entry EntryToMoveUnder)
        {
            if (EntrysListToMove.Contains(EntryToMoveUnder)) 
            {
                return;
            }

            foreach (Entry EntryToMove in EntrysListToMove) 
            {
                EntryToMove.EntryColumn.ColumnGrid.Children.Remove(EntryToMove.EntryBorder);
                EntryToMove.EntryColumn.EntryList.Remove(EntryToMove);

                int ToIndex = EntryToMoveUnder.EntryColumn.EntryList.IndexOf(EntryToMoveUnder);

                EntryToMoveUnder.EntryColumn.ColumnGrid.Children.Insert(ToIndex + 2, EntryToMove.EntryBorder);
                EntryToMoveUnder.EntryColumn.EntryList.Insert(ToIndex + 1, EntryToMove);

                EntryToMove.EntryColumn = EntryToMoveUnder.EntryColumn; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                EntryToMove.EntryRow = EntryToMoveUnder.EntryRow;
            }


            DeleteEmptyColumnsAndMakeNewOnes(EntryToMoveUnder.EntryEditor.StandardEditorData);
        }

        public void MoveEntrysToColumn(List<Entry> EntrysListToMove, Column Column) 
        {
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


        


    }
}
