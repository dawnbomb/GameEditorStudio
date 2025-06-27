using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GameEditorStudio
{
    public static class StandardEditorMethods
    {

        public static void CreateNewGroup(Entry EntryClass) 
        {
            Column EntryColumn = EntryClass.EntryColumn;


        }

        public static void MoveEntrysToEntry(List<Entry> EntrysListToMove, Entry EntryToMoveUnder)
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

        public static void MoveEntrysToColumn(List<Entry> EntrysListToMove, Column Column) 
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

        public static void EntryActivate(Workshop TheWorkshop, Entry EntryClass) 
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

            
            if (EntryClass.Notepad == "")
            {
                EntryClass.EntryLeftGrid.ToolTip = null;
                EntryClass.UnderlineBorder.BorderThickness = new Thickness(0, 0, 0, 0);
                //MainName.TextDecorations = null;
            }
            else //The underline system.
            {
                EntryClass.EntryLeftGrid.ToolTip = EntryClass.Notepad;
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


        public static void LabelWidth(Column ColumnClass)
        {


            double maxWidth = 0;
            var EntryList = ColumnClass.EntryList;

            // Measure the desired width of each label without restrictions
            foreach (Entry entry in EntryList)
            {
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
                entry.EntryNameTextBlock.MinWidth = maxWidth;
            }
        }





    }
}
