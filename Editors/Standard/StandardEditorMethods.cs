using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GameEditorStudio
{
    public static class StandardEditorMethods
    {
                

        public static void MoveGroupToBottomOfColumn(Group Group, Column ToColumn) 
        {
            Column BeforeColumn = Group.GroupColumn; //Save the column before we change it, so we can delete it later if needed.
            Column AfterColumn = ToColumn; //Save the column after we change it, so we can delete it later if needed.

            Group.GroupColumn.ItemBaseList.Remove(Group);
            Group.GroupColumn.ColumnPanel.Children.Remove(Group.GroupBorder);

            ToColumn.ColumnPanel.Children.Add(Group.GroupBorder);
            ToColumn.ItemBaseList.Add(Group);

            Group.GroupColumn = ToColumn;
            foreach (Entry entry in Group.EntryList)
            {
                entry.EntryColumn = Group.GroupColumn; //Update the entry's column to the new group column.
                entry.EntryRow = Group.GroupColumn.ColumnRow; //Update the entry's row to the new group column's row.
            }

            DeleteEmptyColumnsAndMakeNewOnes(AfterColumn.ColumnRow.SWData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);
        }

        public static void MoveGroupUnderEntry(Group Group, Entry EntryToMoveUnder) 
        {
            Column BeforeColumn = Group.GroupColumn; 
            Column AfterColumn = EntryToMoveUnder.EntryColumn; 

            if (Group.EntryList.Contains(EntryToMoveUnder))
            {
                return;
            }

            Group.GroupColumn.ItemBaseList.Remove(Group);
            Group.GroupColumn.ColumnPanel.Children.Remove(Group.GroupBorder);

            int ToIndex = EntryToMoveUnder.EntryColumn.ItemBaseList.IndexOf(EntryToMoveUnder);
            EntryToMoveUnder.EntryColumn.ColumnPanel.Children.Insert(ToIndex + 2, Group.GroupBorder);
            EntryToMoveUnder.EntryColumn.ItemBaseList.Insert(ToIndex + 1, Group);

            Group.GroupColumn = EntryToMoveUnder.EntryColumn;
            foreach (Entry entry in Group.EntryList)
            {
                entry.EntryColumn = Group.GroupColumn; //Update the entry's column to the new group column.
                entry.EntryRow = Group.GroupColumn.ColumnRow; //Update the entry's row to the new group column's row.
            }

            DeleteEmptyColumnsAndMakeNewOnes(EntryToMoveUnder.EntryEditor.StandardEditorData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);
        }

        public static void MoveGroupUnderGroup(Group Group, Group GroupToMoveUnder)
        {
            Column BeforeColumn = Group.GroupColumn;
            Column AfterColumn = GroupToMoveUnder.GroupColumn;

            Group.GroupColumn.ItemBaseList.Remove(Group);
            Group.GroupColumn.ColumnPanel.Children.Remove(Group.GroupBorder);

            int ToIndex = GroupToMoveUnder.GroupColumn.ItemBaseList.IndexOf(GroupToMoveUnder);
            GroupToMoveUnder.GroupColumn.ColumnPanel.Children.Insert(ToIndex + 2, Group.GroupBorder);
            GroupToMoveUnder.GroupColumn.ItemBaseList.Insert(ToIndex + 1, Group);

            Group.GroupColumn = GroupToMoveUnder.GroupColumn;
            foreach (Entry entry in Group.EntryList) 
            {
                entry.EntryColumn = Group.GroupColumn; //Update the entry's column to the new group column.
                entry.EntryRow = Group.GroupColumn.ColumnRow; //Update the entry's row to the new group column's row.
            }

            DeleteEmptyColumnsAndMakeNewOnes(GroupToMoveUnder.GroupColumn.ColumnRow.SWData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);
        }

        public static void MoveEntrysToColumn(List<Entry> EntrysListToMove, Column Column)
        {
            Column BeforeColumn = EntrysListToMove[0].EntryColumn; //Save the column before we change it, so we can delete it later if needed.
            Column AfterColumn = Column; //Save the column after we change it, so we can delete it later if needed.
            List<Entry> MasterList = Column.ColumnRow.SWData.MasterEntryList; //Get the master list of entrys.            

            foreach (Entry EntryToMove in EntrysListToMove)
            {
                DoThing(EntryToMove); // First thing

                int repeats = EntryToMove.Bytes - 1;

                for (int x = 1; x <= repeats; x++) // Starts at 1 because first DoThing already ran
                {
                    int targetOffset = EntryToMove.RowOffset + x;

                    Entry? NextEntryToMove = MasterList.Find(e => e.RowOffset == targetOffset);

                    if (NextEntryToMove != null)
                    {
                        DoThing(NextEntryToMove);
                    }
                }
            }

            DeleteEmptyColumnsAndMakeNewOnes(AfterColumn.ColumnRow.SWData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);

            void DoThing(Entry EntryToMove) 
            {
                if (EntryToMove.EntryGroup != null)
                {
                    EntryToMove.EntryGroup.GroupPanel.Children.Remove(EntryToMove.EntryBorder);
                    EntryToMove.EntryGroup.EntryList.Remove(EntryToMove);
                    EntryToMove.EntryGroup = null;
                }

                EntryToMove.EntryColumn.ColumnPanel.Children.Remove(EntryToMove.EntryBorder);
                EntryToMove.EntryColumn.ItemBaseList.Remove(EntryToMove);

                Column.ColumnPanel.Children.Add(EntryToMove.EntryBorder);
                Column.ItemBaseList.Add(EntryToMove);

                EntryToMove.EntryColumn = Column; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                EntryToMove.EntryRow = Column.ColumnRow;
            }
        }

        public static void MoveEntrysUnderEntry(List<Entry> EntrysListToMove, Entry FirstEntryToMoveUnder)
        {
            if (EntrysListToMove.Contains(FirstEntryToMoveUnder)) 
            {
                return;
            }
            
            List<Entry> MasterList = FirstEntryToMoveUnder.EntryEditor.StandardEditorData.MasterEntryList; //Get the master list of entrys.

            Column BeforeColumn = EntrysListToMove[0].EntryColumn; //Save the column before we change it, so we can delete it later if needed.
            Column AfterColumn = FirstEntryToMoveUnder.EntryColumn; //Save the column after we change it, so we can delete it later if needed.

            Entry EntryToMoveUnder = FirstEntryToMoveUnder; //This is the entry we are moving under.

            foreach (Entry EntryToMove in EntrysListToMove)
            {
                DoThing(EntryToMove); // First thing

                int repeats = EntryToMove.Bytes - 1;

                for (int x = 1; x <= repeats; x++) // Starts at 1 because first DoThing already ran
                {
                    int targetOffset = EntryToMove.RowOffset + x;

                    Entry? NextEntryToMove = MasterList.Find(e => e.RowOffset == targetOffset);

                    if (NextEntryToMove != null)
                    {
                        DoThing(NextEntryToMove);
                    }
                }
            }


            DeleteEmptyColumnsAndMakeNewOnes(FirstEntryToMoveUnder.EntryEditor.StandardEditorData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);


            void DoThing(Entry EntryToMove) 
            {
                if (EntryToMove.EntryGroup != null) //FROM GROUP.
                {
                    EntryToMove.EntryGroup.GroupPanel.Children.Remove(EntryToMove.EntryBorder);
                    EntryToMove.EntryGroup.EntryList.Remove(EntryToMove);
                    EntryToMove.EntryGroup = null;
                }
                else if (EntryToMove.EntryGroup == null) //FROM COULMN.
                {
                    EntryToMove.EntryColumn.ColumnPanel.Children.Remove(EntryToMove.EntryBorder);
                    EntryToMove.EntryColumn.ItemBaseList.Remove(EntryToMove);
                }

                if (EntryToMoveUnder.EntryGroup != null) //TO GROUP.
                {
                    int ToIndex = EntryToMoveUnder.EntryGroup.EntryList.IndexOf(EntryToMoveUnder); // the i fixes a bug where entry list drops in reverse order. 

                    EntryToMoveUnder.EntryGroup.GroupPanel.Children.Insert(ToIndex + 2, EntryToMove.EntryBorder);
                    EntryToMoveUnder.EntryGroup.EntryList.Insert(ToIndex + 1, EntryToMove);

                    EntryToMove.EntryGroup = EntryToMoveUnder.EntryGroup;
                }
                else if (EntryToMoveUnder.EntryGroup == null) //TO COLUMN. 
                {
                    int ToIndex = EntryToMoveUnder.EntryColumn.ItemBaseList.IndexOf(EntryToMoveUnder); // the i fixes a bug where entry list drops in reverse order. 

                    EntryToMoveUnder.EntryColumn.ColumnPanel.Children.Insert(ToIndex + 2, EntryToMove.EntryBorder);
                    EntryToMoveUnder.EntryColumn.ItemBaseList.Insert(ToIndex + 1, EntryToMove);
                }

                EntryToMove.EntryColumn = EntryToMoveUnder.EntryColumn; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                EntryToMove.EntryRow = EntryToMoveUnder.EntryRow;

                EntryToMoveUnder = EntryToMove;
                                
            }

        }

        public static void MoveEntrysUnderGroup(List<Entry> EntrysListToMove, Group GroupToMoveUnder) 
        {
            Column BeforeColumn = EntrysListToMove[0].EntryColumn;
            Column AfterColumn = GroupToMoveUnder.GroupColumn;
            List<Entry> MasterList = AfterColumn.ColumnRow.SWData.MasterEntryList; //Get the master list of entrys.

            foreach (Entry EntryToMove in EntrysListToMove)
            {
                DoThing(EntryToMove); // First thing

                int repeats = EntryToMove.Bytes - 1;

                for (int x = 1; x <= repeats; x++) // Starts at 1 because first DoThing already ran
                {
                    int targetOffset = EntryToMove.RowOffset + x;

                    Entry? NextEntryToMove = MasterList.Find(e => e.RowOffset == targetOffset);

                    if (NextEntryToMove != null)
                    {
                        DoThing(NextEntryToMove);
                    }
                }
            }

            DeleteEmptyColumnsAndMakeNewOnes(AfterColumn.ColumnRow.SWData);
            LabelWidth(BeforeColumn);
            LabelWidth(AfterColumn);

            void DoThing(Entry EntryToMove) 
            {
                if (EntryToMove.EntryGroup != null) //FROM GROUP
                {
                    //Group BeforeGroup = EntryToMove.EntryGroup;
                    EntryToMove.EntryGroup.GroupPanel.Children.Remove(EntryToMove.EntryBorder);
                    EntryToMove.EntryGroup.EntryList.Remove(EntryToMove);
                }
                else if (EntryToMove.EntryGroup == null) //FROM COLUMN
                {
                    EntryToMove.EntryColumn.ColumnPanel.Children.Remove(EntryToMove.EntryBorder);
                    EntryToMove.EntryColumn.ItemBaseList.Remove(EntryToMove);
                }

                { //TO COLUMN 
                    int ToIndex = GroupToMoveUnder.GroupColumn.ItemBaseList.IndexOf(GroupToMoveUnder); // the i fixes a bug where entry list drops in reverse order. 

                    GroupToMoveUnder.GroupColumn.ColumnPanel.Children.Insert(ToIndex + 2, EntryToMove.EntryBorder);
                    GroupToMoveUnder.GroupColumn.ItemBaseList.Insert(ToIndex + 1, EntryToMove);
                }

                EntryToMove.EntryColumn = AfterColumn; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.
                EntryToMove.EntryGroup = null;
                EntryToMove.EntryRow = AfterColumn.ColumnRow;                
            }
        }
                

        

        public static void EntryActivate(Entry EntryClass) 
        {
            //if (TheWorkshop.IsPreviewMode == true) { return; }            
            Workshop TheWorkshop = EntryClass.EntryEditor.Workshop;

            EntryManager EntryData = new();
            EntryData.EntryBecomeActive(EntryClass);
            EntryData.UpdateEntryProperties(EntryClass);

            //EditorClass.SWData.EditorTopBar.EntryNoteBox.Text = EntryClass.EntryTooltip;
            TheWorkshop.EntryNoteTextbox.Text = EntryClass.WorkshopTooltip;


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

                        

        }

        public static void UpdateEntryName(Entry EntryClass) 
        {
            //add a option to properties where a entrys can have a Icon on the left side. for easy, universal, user styling / expression.
            TextBlock EntryTextBlock = EntryClass.EntryNameTextBlock;
            

            EntryClass.EntryNameTextBlock = EntryTextBlock;

            Run MainName = EntryClass.RunEntryName;
            //MainName.VerticalAlignment = VerticalAlignment.Center;

            if (EntryClass.IsNameHidden == false) { EntryClass.EntryNameTextBlock.Visibility = Visibility.Visible; }
            EntryClass.EntryNameTextBlock.Visibility = Visibility.Visible;

            if (EntryClass.IsNameHidden == true) 
            {
                MainName.Text = "";
                EntryClass.EntryNameTextBlock.Visibility = Visibility.Collapsed;
            }
            else if (EntryClass.Name == "")
            {
                MainName.Text = "??? " + EntryClass.RowOffset;
            }
            else if (EntryClass.Name != "")
            {
                MainName.Text = EntryClass.Name;
            }

            
            if (EntryClass.WorkshopTooltip == "")
            {
                EntryClass.EntryLeftGrid.ToolTip = null;
                EntryClass.UnderlineBorder.BorderThickness = new Thickness(0, 0, 0, 0);
                //MainName.TextDecorations = null;
            }
            else //The underline system.
            {
                EntryClass.EntryLeftGrid.ToolTip = EntryClass.WorkshopTooltip;
                EntryClass.UnderlineBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                EntryClass.UnderlineBorder.Width = EntryTextBlock.Width;
                //MainName.TextDecorations = TextDecorations.Underline;

                var typeface = new Typeface(
                    MainName.FontFamily,
                    MainName.FontStyle,
                    MainName.FontWeight,
                    MainName.FontStretch
                );

                var formattedText = new FormattedText(
                    MainName.Text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    MainName.FontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    1
                );

                EntryClass.UnderlineBorder.Width = formattedText.Width;

            }

            //////////////This below part is for already used stuff.
            Editor EditorClass = EntryClass.EntryEditor;
            Workshop TheWorkshop = EditorClass.Workshop;


            //This last code auto-sets entrys to hidden if the entry's byte is also apart of a text table.
            //I may want to change this to happen if its ANY known text table.
            if (EditorClass.StandardEditorData.FileNameTable != null) //Happens when a table uses a user name list instead of from a game file.
            {
                if (EditorClass.StandardEditorData.FileNameTable.FileLocation != null)
                {
                    if (EditorClass.StandardEditorData.FileNameTable.FileLocation == EditorClass.StandardEditorData.FileDataTable.FileLocation)
                    {
                        int Min = EditorClass.StandardEditorData.DataTableStart;
                        int Max = Min + EditorClass.StandardEditorData.DataTableRowSize;
                        if (EditorClass.StandardEditorData.NameTableStart >= Min && EditorClass.StandardEditorData.NameTableStart <= Max)
                        {
                            int NAMEMIN = EditorClass.StandardEditorData.NameTableStart - EditorClass.StandardEditorData.DataTableStart;
                            int NAMEMAX = NAMEMIN + EditorClass.StandardEditorData.NameTableTextSize - 1;//the 1st byte is "0", so we need a -1 for proper counting.
                            if (EntryClass.RowOffset >= NAMEMIN && EntryClass.RowOffset <= NAMEMAX)
                            {
                                EntryClass.IsTextInUse = true;
                                MainName.Text = "Name";
                            }
                        }
                    }
                }
            }


            if (TheWorkshop.IsPreviewMode == false)
            {
                foreach (DescriptionTable ExtraTable in EditorClass.StandardEditorData.DescriptionTableList)
                {
                    if (EditorClass.StandardEditorData.FileDataTable.FileLocation == ExtraTable.FileTextTable.FileLocation)
                    {
                        int Min = EditorClass.StandardEditorData.DataTableStart;
                        int Max = Min + EditorClass.StandardEditorData.DataTableRowSize;
                        if (ExtraTable.Start >= Min && ExtraTable.Start <= Max)
                        {
                            int EXTRAMIN = ExtraTable.Start - EditorClass.StandardEditorData.DataTableStart;
                            int EXTRAMAX = EXTRAMIN + ExtraTable.TextSize - 1;//the 1st byte is "0", so we need a -1 for proper counting.
                            if (EntryClass.RowOffset >= EXTRAMIN && EntryClass.RowOffset <= EXTRAMAX)
                            {
                                EntryClass.IsTextInUse = true;
                                MainName.Text = "Text";
                            }
                        }
                    }
                }
            }


            LabelWidth(EntryClass.EntryColumn);


        }

        public static void DeleteEmptyColumnsAndMakeNewOnes(StandardEditorData StandardEditorData)
        {


            foreach (Category CatClass in StandardEditorData.CategoryList.ToList()) //Delete any empty columns in each category.
            {
                foreach (Column ColumnClass in CatClass.ColumnList.ToList())
                {
                    foreach (Group GroupClass in ColumnClass.ItemBaseList.OfType<Group>().ToList()) //Delete any empty groups in each column.
                    {
                        if (GroupClass.EntryList.Count == 0) //Delete group if it's empty. 
                        {
                            DeleteGroup(GroupClass);
                        }
                    }

                    if (ColumnClass.ItemBaseList == null || ColumnClass.ItemBaseList.Count == 0) //Delete column if it's empty. 
                    {
                        StandardEditorData.TheEditor.Workshop.ColumnDelete(ColumnClass);
                    }

                }
            }

            foreach (Category CatClass in StandardEditorData.CategoryList.ToList()) //Create a empty column at the end of each category.
            {
                Column LastColumn = CatClass.ColumnList.Last();

                if (LastColumn.ItemBaseList.Count != 0)
                {
                    StandardEditorData.TheEditor.Workshop.CreateNewColumnRight(LastColumn);
                }

            }
        }

        public static void DeleteGroup(Group Group) 
        {
            //if (Group.GroupColumn.MasterList.Contains(Group)) 
            //{

            //}
            if (Group.EntryList.Count == 0)
            {
                Group.GroupColumn.ColumnPanel.Children.Remove(Group.GroupBorder);
                Group.GroupColumn.ItemBaseList.Remove(Group);
            }
            

        }


        public static void LabelWidth(Column ColumnClass)
        {


            double maxWidth = 0;
            var EntryList = ColumnClass.ColumnRow.SWData.MasterEntryList;

            //var EntryList = ColumnClass.ItemBaseList; //THE OLD CODE BEFORE GROUPS
            //.OfType<Entry>()

            // Measure the desired width of each label without restrictions
            foreach (Entry entry in EntryList)
            {
                if (entry.EntryColumn != ColumnClass) 
                {
                    continue;
                }

                entry.EntryNameTextBlock.MinWidth = 0;
                entry.EntryNameTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double labelWidth = entry.EntryNameTextBlock.DesiredSize.Width;
                if (labelWidth > maxWidth)
                {
                    maxWidth = labelWidth;
                }
            }

            // Set the MinWidth of each label to the widest value
            foreach (Entry entry in EntryList)
            {
                if (entry.EntryColumn != ColumnClass)
                {
                    continue;
                }
                entry.EntryNameTextBlock.MinWidth = maxWidth;
            }
        }





    }
}
