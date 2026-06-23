using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static GameEditorStudio.Entry;

namespace GameEditorStudio
{
    public static class DTEMethods
    {
        public static void UpdateEditorGrids(DataTableEditorData standardEditorData)
        {
            foreach (Category category in standardEditorData.CategoryList)
            {
                RemoveEmptyGroupsFromCategory(category);
            }

            foreach (Category category in standardEditorData.CategoryList)
            {
                foreach (GridItem item in category.GridItems)
                {
                    if (item is Group group)
                    {
                        UpdateGridLayout(group.ItemGrid, group.GridItems);
                        group.ItemGrid.UpdateLayout();
                    }

                    //if (item.ParentGroup != null) {  }
                }

                UpdateGridLayout(category.ItemGrid, category.GridItems);
                category.ItemGrid.UpdateLayout();

                SyncCategoryColumnWidths(category);

                category.ItemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MinHeight = 42 }); //GridLength.Auto //Height = new GridLength(42), //MaxHeight = 42, MinHeight = 10
                category.ItemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 30 }); //GridLength.Auto

                              
            }

            //SynchronizeGlobalColumnWidths(standardEditorData);

            static void RemoveEmptyGroupsFromCategory(Category category)
            {
                var emptyGroups = category.GridItems
                    .OfType<Group>()
                    .Where(g => g.GridItems.Count == 0)
                    .ToList();

                foreach (var group in emptyGroups)
                {
                    // Remove visuals
                    if (group.Visual is FrameworkElement fe &&
                        fe.Parent is Panel panel)
                    {
                        panel.Children.Remove(fe);
                    }

                    category.GridItems.Remove(group);

                    // Cleanup references
                    group.ParentGrid = null;
                    group.ParentGridItems = null;
                    group.ParentCategory = null;
                    group.ParentGroup = null;
                }
            }

            void UpdateGridLayout(Grid grid, List<GridItem> items)
            {

                //if (items.Count == 0)
                //    return; //That early return is intentional — if the last item was a group and got removed, you don’t want .Max() calls exploding. (No idea if true)

                foreach (var group in items.OfType<Group>())
                {
                    NormalizeGroupInternalColumns(group);
                }

                UpdateGroupRowSpans(items);
                UpdateGroupColumnSpans(items);

                NormalizeColumns(items);
                NormalizeRows(items);

                ApplyUpwardGravity(items);
                NormalizeRows(items);

                RebuildGridStructure(grid, items);
                AttachVisuals(grid, items);
                ApplyGridPositions(items);

                NormalizeNameWidths(items);

                ModItemIfInGroupOrNot(items);

                void ModItemIfInGroupOrNot(List<GridItem> items)
                {
                    foreach (GridItem item in items)
                    {
                        if (item is Entry entry)
                        {   
                            if (entry.ParentGroup != null) //if in group.
                            {
                                entry.EntryBorder.Margin = new Thickness(3, 5, 3, -1);
                                entry.EntryCreateNewGroup.IsEnabled = false;
                            }
                            else //if not in group.
                            {
                                entry.EntryBorder.Margin = new Thickness(5, 5, 0, -1);
                                entry.EntryCreateNewGroup.IsEnabled = true;
                            }
                        }
                    }

                }

                void NormalizeGroupInternalColumns(Group group)
                {
                    if (group.GridItems.Count == 0) return;

                    // Find which columns are actually used inside the group
                    var usedColumns = group.GridItems
                        .SelectMany(i => Enumerable.Range(i.Column, i.ColumnSpan))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();

                    // Create a mapping (e.g., if only columns 0 and 4 are used, 4 becomes 1)
                    var remap = new Dictionary<int, int>();
                    for (int i = 0; i < usedColumns.Count; i++)
                        remap[usedColumns[i]] = i;

                    foreach (var item in group.GridItems)
                    {
                        item.Column = remap[item.Column];
                    }
                }

                void UpdateGroupColumnSpans(List<GridItem> items)
                {
                    foreach (var group in items.OfType<Group>())
                    {
                        if (group.GridItems.Count > 0)
                        {
                            // The ColumnSpan of the Group in the Category grid 
                            // should equal the max column used inside it
                            group.ColumnSpan = group.GridItems.Max(i => i.Column + i.ColumnSpan);
                        }
                        else
                        {
                            group.ColumnSpan = 1;
                        }
                    }
                }

                void UpdateGroupRowSpans(List<GridItem> items)
                {
                    foreach (var group in items.OfType<Group>())
                    {
                        // RowSpan = 1 (header) + number of child items
                        int GroupRowSpan = 1; //Base 1 for the header. 

                        if (group.GridItems.Any())
                        {
                            // Group items by their column, sum the RowSpans in each column, 
                            // and find which column is the "tallest".
                            GroupRowSpan = group.GridItems
                                .GroupBy(i => i.Column)
                                .Select(g => g.Sum(i => i.RowSpan))
                                .Max();
                        }

                        group.RowSpan = GroupRowSpan + 1;
                    }
                }

                void NormalizeColumns(List<GridItem> items)
                {
                    var usedColumns = items
                        .SelectMany(i => Enumerable.Range(i.Column, i.ColumnSpan))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();

                    var remap = new Dictionary<int, int>();
                    for (int i = 0; i < usedColumns.Count; i++)
                        remap[usedColumns[i]] = i;

                    foreach (var item in items)
                        item.Column = remap[item.Column];
                }


                void NormalizeRows(List<GridItem> items)
                {
                    var usedRows = items
                        .SelectMany(i => Enumerable.Range(i.Row, i.RowSpan))
                        .Distinct()
                        .OrderBy(r => r)
                        .ToList();

                    var remap = new Dictionary<int, int>();
                    for (int i = 0; i < usedRows.Count; i++)
                        remap[usedRows[i]] = i;

                    foreach (var item in items)
                        item.Row = remap[item.Row];
                }


                void ApplyUpwardGravity(List<GridItem> items)
                {
                    // Process items top-down for stability
                    var sorted = items
                        .OrderBy(i => i.Row)
                        .ThenBy(i => i.Column)
                        .ToList();

                    // For each column, track occupied rows
                    var occupied = new Dictionary<int, HashSet<int>>();

                    //int GetNextFreeRow(GridItem item)
                    //{
                    //    int row = item.Row; // ✅ Start from current position, not 0

                    //    // Move UP until we hit an obstacle
                    //    while (row > 0)
                    //    {
                    //        bool canMoveUp = true;

                    //        // Check if row-1 is free in all columns this item spans
                    //        for (int c = item.Column; c < item.Column + item.ColumnSpan; c++)
                    //        {
                    //            if (!occupied.TryGetValue(c, out var rows))
                    //                continue;

                    //            // Check if moving up one row would cause collision
                    //            for (int r = row - 1; r < row - 1 + item.RowSpan; r++)
                    //            {
                    //                if (rows.Contains(r))
                    //                {
                    //                    canMoveUp = false;
                    //                    break;
                    //                }
                    //            }

                    //            if (!canMoveUp)
                    //                break;
                    //        }

                    //        if (!canMoveUp)
                    //            break; // Stop here, can't move up anymore

                    //        row--; // Move up one row
                    //    }

                    //    return row;
                    //}
                    int GetNextFreeRow(GridItem item)
                    {
                        int row = 0;

                        while (true)
                        {
                            bool blocked = false;

                            // ✅ Check all columns that THIS item will occupy
                            for (int c = item.Column; c < item.Column + item.ColumnSpan; c++)
                            {
                                if (!occupied.TryGetValue(c, out var rows))
                                    continue;

                                // ✅ Check all rows that THIS item will occupy
                                for (int r = row; r < row + item.RowSpan; r++)
                                {
                                    if (rows.Contains(r))
                                    {
                                        blocked = true;
                                        break;
                                    }
                                }

                                if (blocked)
                                    break;
                            }

                            if (!blocked)
                                return row;

                            row++;
                        }
                    }

                    foreach (var item in sorted)
                    {
                        int newRow = GetNextFreeRow(item);
                        item.Row = newRow;

                        // ✅ CRITICAL FIX: Mark ALL columns that this item occupies
                        for (int c = item.Column; c < item.Column + item.ColumnSpan; c++)
                        {
                            if (!occupied.TryGetValue(c, out var rows))
                                occupied[c] = rows = new HashSet<int>();

                            // ✅ Mark ALL rows that this item occupies
                            for (int r = newRow; r < newRow + item.RowSpan; r++)
                                rows.Add(r);
                        }
                    }
                }


                void RebuildGridStructure(Grid grid, List<GridItem> items)
                {
                    grid.RowDefinitions.Clear();
                    grid.ColumnDefinitions.Clear();

                    if (items.Count == 0)
                        return;

                    int maxRow = items.Max(i => i.Row + i.RowSpan);
                    int maxCol = items.Max(i => i.Column + i.ColumnSpan);

                    for (int r = 0; r < maxRow; r++)
                        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(42) }); //GridLength.Auto //Height = new GridLength(42), //MaxHeight = 42, MinHeight = 10

                    for (int c = 0; c < maxCol; c++)
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 30 }); //GridLength.Auto
                }

                void AttachVisuals(Grid grid, List<GridItem> items)
                {
                    foreach (var item in items)
                    {
                        if (item.Visual == null)
                            continue;

                        if (item.Visual is FrameworkElement fe &&
                            fe.Parent is Panel oldPanel &&
                            oldPanel != grid)
                        {
                            oldPanel.Children.Remove(item.Visual);
                        }

                        if (!grid.Children.Contains(item.Visual))
                        {
                            grid.Children.Add(item.Visual);

                            //if (item is Entry theentry) 
                            //{
                            //    if (theentry.Name == "T44") { string testa = "hi"; }
                            //    if (theentry.Name == "T55") { string testa = "hi"; }
                            //    if (theentry.Name == "T66") { string testa = "hi"; }
                            //    if (theentry.Name == "T77") { string testa = "hi"; }

                            //    if (theentry.Name == "T84") { string testa = "hi"; }
                            //    if (theentry.Name == "T85") { string testa = "hi"; }
                            //    if (theentry.Name == "T86") { string testa = "hi"; }
                            //    if (theentry.Name == "T87") { string testa = "hi"; }
                            //    if (theentry.IsMerged == false) 
                            //    {
                                    
                            //    }
                            //}
                            
                        }
                    }
                }


                void ApplyGridPositions(List<GridItem> items)
                {
                    foreach (var item in items)
                    {
                        string testa = "fsd";
                        Grid.SetRow(
                            item.Visual,
                            item.Row);
                        Grid.SetColumn(item.Visual, item.Column);
                        Grid.SetRowSpan(item.Visual, item.RowSpan);
                        Grid.SetColumnSpan(item.Visual, item.ColumnSpan);
                    }
                }

                static void NormalizeNameWidths(List<GridItem> items)
                {
                    // Group entries by column
                    var entriesByColumn = items
                        .OfType<Entry>()
                        .GroupBy(e => e.Column);

                    foreach (var columnGroup in entriesByColumn)
                    {
                        var entries = columnGroup.ToList();

                        // Reset all widths to measure natural size
                        foreach (var entry in entries)
                        {
                            entry.EntryNameTextBlock.MinWidth = 0;
                            entry.EntryNameTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        }

                        // Find the widest label in this column
                        double maxWidth = entries
                            .Select(e => e.EntryNameTextBlock.DesiredSize.Width)
                            .DefaultIfEmpty(0)
                            .Max();

                        // Apply the maximum width to all entries in this column
                        foreach (var entry in entries)
                        {
                            entry.EntryNameTextBlock.MinWidth = maxWidth;
                        }
                    }
                }
                                

                
            }


            //Stuff for group - category column width sync.
            static void SyncCategoryColumnWidths(Category category)
            {

                // 1. Dictionary to hold the absolute max width of each column silo
                var colWidths = new Dictionary<int, double>();

                // 2. Helper to measure items and record their preferred width
                void ScanItems(List<GridItem> items, int colOffset)
                {
                    foreach (var item in items)
                    {
                        if (item.Visual is FrameworkElement fe)
                        {
                            // Force the UI element to calculate how big it WANTs to be right now
                            fe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                            double desiredW = fe.DesiredSize.Width;

                            // If an item spans 1 column, it's easy. 
                            // If it spans multiple, we only sync if it's an Entry (usually 1 col).
                            // Groups are handled by scanning their internal items.
                            if (item is Entry entry)
                            {
                                int absCol = colOffset + entry.Column;
                                if (!colWidths.ContainsKey(absCol) || desiredW > colWidths[absCol])
                                    colWidths[absCol] = desiredW;
                            }
                            else if (item is Group group)
                            {
                                // Recurse into the group's internal items
                                ScanItems(group.GridItems, colOffset + group.Column);
                            }
                        }
                    }
                }

                // Pass 1: Dig through the category and all nested groups to find the "Widest" for every slot
                ScanItems(category.GridItems, 0);

                // Pass 2: Apply these MinWidths to the Category Grid
                ApplyToGrid(category.ItemGrid, 0);

                // Pass 3: Apply to every Group's internal Grid
                foreach (var group in category.GridItems.OfType<Group>())
                {
                    ApplyToGrid(group.ItemGrid, group.Column);
                }

                void ApplyToGrid(Grid grid, int offset)
                {
                    for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                    {
                        int absCol = offset + i;
                        if (colWidths.TryGetValue(absCol, out double maxW))
                        {
                            grid.ColumnDefinitions[i].MinWidth = maxW;
                        }
                    }
                }
            }


            void SynchronizeGlobalColumnWidths(DataTableEditorData data)
            {
                // Dictionary to store: [ColumnIndex] -> [MaxMeasuredWidth]
                var columnWidths = new Dictionary<int, double>();

                // --- PASS 1: COLLECT MAX WIDTHS ---
                void CollectWidths(Grid grid, int startCol)
                {
                    for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                    {
                        int absoluteCol = startCol + i;
                        // We use ActualWidth if already rendered, or DesiredSize if measuring
                        double width = grid.ColumnDefinitions[i].ActualWidth;

                        // If the grid hasn't rendered yet, we might need a manual measure 
                        // but usually ActualWidth is fine if this runs after Measure/Arrange
                        if (!columnWidths.ContainsKey(absoluteCol) || width > columnWidths[absoluteCol])
                            columnWidths[absoluteCol] = width;
                    }
                }

                foreach (var cat in data.CategoryList)
                {
                    // Check Category Grid
                    CollectWidths(cat.ItemGrid, 0);

                    // Check Group Grids
                    foreach (var group in cat.GridItems.OfType<Group>())
                    {
                        // The group's internal column 0 is actually the Category's column 'group.Column'
                        CollectWidths(group.ItemGrid, group.Column);
                    }
                }

                // --- PASS 2: APPLY MAX WIDTHS ---
                void ApplyWidths(Grid grid, int startCol)
                {
                    for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                    {
                        int absoluteCol = startCol + i;
                        if (columnWidths.TryGetValue(absoluteCol, out double maxWidth))
                        {
                            // Force the column to be at least as wide as the global max
                            grid.ColumnDefinitions[i].MinWidth = maxWidth;
                        }
                    }
                }

                foreach (var cat in data.CategoryList)
                {
                    ApplyWidths(cat.ItemGrid, 0);
                    foreach (var group in cat.GridItems.OfType<Group>())
                    {
                        ApplyWidths(group.ItemGrid, group.Column);
                    }
                }
            }
        }












        //BeginDrag
        //GetDragItems
        //DropOntoItem
        //DropToBottomOfColumn

        //Drag drop code starts here
        public static void BeginDrag( UIElement dragSource, GridItem sourceItem, ScrollViewer scrollViewer)
        {
            //var dragItems = GetDragItems(sourceItem);

            //if (dragItems.Count == 0)
            //    return;

            //var data = new DataObject(typeof(List<GridItem>), dragItems);
            //DragDrop.DoDragDrop(dragSource, data, DragDropEffects.Move);

            var dragItems = GetDragItems(sourceItem);
            if (dragItems.Count == 0) return;

            _activeScrollViewer = scrollViewer;
            var data = new DataObject(typeof(List<GridItem>), dragItems);

            // Start the timer to check mouse position 60 times a second
            _scrollTimer = new DispatcherTimer(DispatcherPriority.Background);
            _scrollTimer.Interval = TimeSpan.FromMilliseconds(1); // ~60fps //16
            _scrollTimer.Tick += DragScrollTimer_Tick;
            _scrollTimer.Start();

            // Start the blocking drag loop
            DragDrop.DoDragDrop(dragSource, data, DragDropEffects.Move);

            // Stop the timer immediately when the item is dropped
            _scrollTimer.Stop();
            _scrollTimer = null;
            _activeScrollViewer = null;
        }
        private static void DragScrollTimer_Tick(object sender, EventArgs e)
        {
            if (_activeScrollViewer == null) return;

            // Get mouse relative to the ScrollViewer
            Point mousePos = _activeScrollViewer.PointFromScreen(GetDragMousePos());

            double tolerance = 60.0;
            double scrollSpeed = 0;

            if (mousePos.Y < tolerance) // Near Top
            {
                //scrollSpeed = -15 * (1 - Math.Max(0, mousePos.Y / tolerance));
                scrollSpeed = -15;
            }
            else if (mousePos.Y > _activeScrollViewer.ActualHeight - tolerance) // Near Bottom
            {
                //double distFromBottom = _activeScrollViewer.ActualHeight - mousePos.Y;
                //scrollSpeed = 15 * (1 - Math.Max(0, distFromBottom / tolerance));
                scrollSpeed = 15;
            }

            if (scrollSpeed != 0)
            {
                _activeScrollViewer.ScrollToVerticalOffset(_activeScrollViewer.VerticalOffset + scrollSpeed);
            }
        }
        private static DispatcherTimer _scrollTimer;
        private static ScrollViewer _activeScrollViewer;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Win32Point lpPoint);
        private struct Win32Point { public int X; public int Y; }
        private static Point GetDragMousePos()
        {
            GetCursorPos(out Win32Point w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        public static List<GridItem> GetDragItems(GridItem source)
        {
            var editor = source.ParentEditor;

            // 1. Basic safety check
            if (editor == null)
                return new List<GridItem> { source };

            // 2. If Shift isn't held, just drag the single item
            if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                return new List<GridItem> { source };
            }

            var selected = editor.DataTableEditorData.EntryClass;

            // 3. Shift is held, but nothing is selected? Just take the source.
            if (selected == null)
                return new List<GridItem> { source };

            // 4. CRITICAL: Validate they are in the same Grid AND the same Column
            // If they aren't in the same column, we return an empty list (canceling the drag)
            if (selected.ParentGridItems != source.ParentGridItems || selected.Column != source.Column)
            {
                return new List<GridItem>(); // Or return new List<GridItem> { source } if you'd prefer it fallback
            }

            // 5. Determine the Row range
            int startRow = Math.Min(selected.Row, source.Row);
            int endRow = Math.Max(selected.Row, source.Row);

            // 6. Return only items that are in the SAME column and within the row range
            return source.ParentGridItems
                .Where(i => i.Column == source.Column && // Strictly same column
                            i.Row >= startRow &&
                            i.Row <= endRow)
                .OrderBy(i => i.Row)
                .ToList();
        }


        public static void DropOntoItem(DragEventArgs e, GridItem destination)
        {
            if (!e.Data.GetDataPresent(typeof(List<GridItem>)))
                return;

            var draggedItems = ((List<GridItem>)e.Data.GetData(typeof(List<GridItem>)))
                .OrderBy(i => i.Row)
                .ToList();

            if (draggedItems.Count == 0)
                return;

            var sourceGrid = draggedItems[0].ParentGrid;
            var sourceItems = draggedItems[0].ParentGridItems;
            var destGrid = destination.ParentGrid;
            var destItems = destination.ParentGridItems;
            int destColumn = destination.Column;

            // ✅ Calculate insert row based on setting
            int insertRow = LibraryGES.EntrysDropAbove
                ? destination.Row                          // Drop ABOVE destination
                : destination.Row + destination.RowSpan;   // Drop BELOW destination

            int totalSpan = draggedItems.Sum(i => i.RowSpan);

            // -----------------------------
            // REMOVE FROM SOURCE LIST
            // -----------------------------
            foreach (var item in draggedItems)
                sourceItems.Remove(item);

            // -----------------------------
            // UPDATE OWNERSHIP
            // -----------------------------
            foreach (var item in draggedItems)
            {
                item.ParentEditor = destination.ParentEditor;
                item.ParentCategory = destination.ParentCategory;
                item.ParentGroup = destination.ParentGroup;
                item.ParentGrid = destGrid;
                item.ParentGridItems = destItems;
                item.Column = destColumn;

                if (item is Group group)
                {
                    foreach (var GgItem in group.GridItems)
                    {
                        GgItem.ParentCategory = destination.ParentCategory;
                    }
                }
            }

            // -----------------------------
            // SHIFT AND INSERT
            // -----------------------------
            var columnItems = destItems
                .Where(i => i.Column == destColumn)
                .OrderBy(i => i.Row)
                .ToList();

            // Shift existing items DOWN to make room
            foreach (var item in columnItems)
            {
                if (item.Row >= insertRow)
                    item.Row += totalSpan;
            }

            // Place the dragged items into the gap
            int r = insertRow;
            foreach (var item in draggedItems)
            {
                item.Row = r;
                r += item.RowSpan;
                destItems.Add(item);
            }

            // -----------------------------
            // REBUILD GRIDS
            // -----------------------------
            UpdateEditorGrids(destination.ParentEditor.DataTableEditorData);

            e.Handled = true;
        }

        public static void DropGridItemsToBottomOfColumn(DragEventArgs draginfo, Grid thegrid, List<GridItem> gridItems, DataTableEditorData SWdata, Category category, Group group)
        {
            var destItems = gridItems;
            Grid targetGrid = thegrid;

            if (!draginfo.Data.GetDataPresent(typeof(List<GridItem>)))
                return;

            var draggedItems = (List<GridItem>)draginfo.Data.GetData(typeof(List<GridItem>));

            // 1. Calculate where the user dropped
            Point dropPoint = draginfo.GetPosition(targetGrid);
            int dropColumn = DTEMethods.GetColumnAt(targetGrid, dropPoint.X);
            int dropRow = DTEMethods.GetRowAt(targetGrid, dropPoint.Y);

            // 2. NEW VALIDATION: Calculate available space before hitting a group or existing item
            int availableRows = CalculateAvailableRows(destItems, dropColumn, dropRow);
            int requiredRows = draggedItems.Sum(i => i.RowSpan);

            if (requiredRows > availableRows)
                return; // ❌ Not enough space

            // 3. Update the Back-End Data (The "Model")
            foreach (var item in draggedItems)
            {
                if (item.ParentGridItems != null)
                    item.ParentGridItems.Remove(item);

                item.ParentEditor = SWdata as Editor;
                item.ParentCategory = category;
                item.ParentGroup = group;
                item.ParentGrid = targetGrid;
                item.ParentGridItems = destItems;
                item.Column = dropColumn;

                // ❌ OLD: Only checks items starting in this column
                // int bottomRow = destItems
                //     .Where(i => i.Column == dropColumn)
                //     .Select(i => i.Row + i.RowSpan)
                //     .DefaultIfEmpty(0)
                //     .Max();

                // ✅ NEW: Check items that OCCUPY this column (including groups with ColumnSpan)
                int bottomRow = destItems
                    .Where(i => i.Column <= dropColumn && dropColumn < i.Column + i.ColumnSpan)
                    .Select(i => i.Row + i.RowSpan)
                    .DefaultIfEmpty(0)
                    .Max();

                item.Row = bottomRow;

                if (item is Group groupX)
                {
                    foreach (var GgItem in groupX.GridItems)
                    {
                        GgItem.ParentCategory = groupX.ParentCategory;
                    }
                }

                destItems.Add(item);
            }

            // 4. Update the Front-End Visuals (The "View")
            DTEMethods.UpdateEditorGrids(SWdata);
            draginfo.Handled = true;
        }

        private static int CalculateAvailableRows(List<GridItem> items, int column, int startRow)
        {
            // Find the first item (including groups spanning into this column) at or below startRow
            int? firstBlockingRow = null;

            foreach (var item in items)
            {
                // Check if item occupies this column
                bool occupiesColumn = (item.Column <= column && column < item.Column + item.ColumnSpan);

                if (occupiesColumn && item.Row >= startRow)
                {
                    if (firstBlockingRow == null || item.Row < firstBlockingRow)
                        firstBlockingRow = item.Row;
                }
            }

            // If nothing blocking, return a large number (infinite space)
            if (firstBlockingRow == null)
                return int.MaxValue;

            // Otherwise, return the gap between startRow and the blocking item
            return firstBlockingRow.Value - startRow;
        }



























        public static void EntryActivate(Entry EntryClass) 
        {
            //if (TheWorkshop.IsPreviewMode == true) { return; }            
            Workshop TheWorkshop = EntryClass.ParentEditor.WorkshopXaml;

            EntryManager EntryData = new();
            EntryData.SetSelectedEntry(EntryClass);
            EntryData.UpdateEntryProperties(EntryClass);

            DTRightBar RightBar = EntryClass.ParentEditor.DataTableEditorData.EditorRightBar;
            RightBar.EntryNoteTextbox.Text = EntryClass.WorkshopTooltip;


            if (RightBar.ListTab.IsSelected == true) //Band-aid code fix to make the workshops "list" tab stop being selected when you select an entry, but this should really be inside the EntryBecomeActive function...
            {
                foreach (TabItem tabItem in RightBar.MainTabControl.Items)
                {

                    if (tabItem.Header != null && tabItem.Header.ToString() == RightBar.PreviousTabName)
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
            EntryTextBlock.IsHitTestVisible = false; //Makes it so mouse scroll works when there is no tooltip.

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

            EntryClass.UnderlineBorder.BorderThickness = new Thickness(0, 0, 0, 2);
            EntryClass.UnderlineBorder.Width = EntryTextBlock.Width;
            var typeface = new Typeface(
                    MainName.FontFamily,
                    MainName.FontStyle,
                    MainName.FontWeight,
                    MainName.FontStretch
                );

            var formattedText = new FormattedText(
                MainName.Text,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                MainName.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1
            );
            EntryClass.UnderlineBorder.Width = formattedText.Width;

            if (EntryClass.WorkshopTooltip == "")
            {
                EntryClass.EntryLeftGrid.ToolTip = null;
                EntryClass.UnderlineBorder.Visibility = Visibility.Collapsed;
                
            }
            else //The underline system.
            {
                EntryClass.EntryLeftGrid.ToolTip = EntryClass.WorkshopTooltip;
                EntryClass.UnderlineBorder.Visibility = Visibility.Visible;
            }
            

            //////////////This below part is for already used stuff.
            MoveEntryIfText(EntryClass, MainName);

            //Note: If i call UpdateGridLayout here, it causes an error during grid positions on first launch
            //because this runs as part of editor setup, as part of entry setup where it attaches a visual,
            //but only that entry (GridItem 1) will have it's visual attached yet. And thus null crash. 

        }

        private static void MoveEntryIfText(Entry EntryClass, Run MainName)
        {
            //This method checks if the entry is known to be name / description text, and if so, automatically moves it to the Hidden Category. 
            //Maybe later want to change this to happen if its ANY known text table. (Link text in other editors..?)

            DataTableEditorData DTEData = EntryClass.ParentEditor as DataTableEditorData;
            if (DTEData.WorkshopXaml.IsPreviewMode == true) { return; }
            string HiddenCategoryName = "Hidden";
            if (EntryClass.IsMerged == true) { return; }
            if (EntryClass.ParentCategory.CategoryName == HiddenCategoryName) { return; }

            //Part 1: Name Table Check.
            if (DTEData.NameTable != null)
            {

                //New Method?
                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) 
                {
                    if (DTEData.NameTable.TextTableFile.FileLocation == DTEData.DataTable.FileDataTable.FileLocation)
                    {
                        int Min = DTEData.DataTable.DataTableStart;
                        int Max = Min + DTEData.DataTable.DataTableRowSize;
                        if (DTEData.NameTable.TextTableStart >= Min && DTEData.NameTable.TextTableStart <= Max)
                        {
                            int NAMEMIN = DTEData.NameTable.TextTableStart - DTEData.DataTable.DataTableStart;
                            int NAMEMAX = NAMEMIN + DTEData.NameTable.TextTableCharLimit - 1;//the 1st byte is "0", so we need a -1 for proper counting.
                            if (EntryClass.RowOffset >= NAMEMIN && EntryClass.RowOffset <= NAMEMAX)
                            {
                                EntryClass.IsTextInUse = true;
                                MainName.Text = "Name";

                                if (!DTEData.CategoryList.Any(cat => cat.CategoryName == HiddenCategoryName))
                                {
                                    DTESetup TheSetup = new DTESetup();
                                    Category category = new();
                                    category.CategoryName = HiddenCategoryName;
                                    category.DTEData = DTEData;

                                    DTEData.CategoryList.Add(category);
                                    TheSetup.CreateCategory(category);
                                }

                                Category hiddenCat = DTEData.CategoryList.FirstOrDefault(cat => cat.CategoryName == HiddenCategoryName);

                                if (!hiddenCat.GridItems.OfType<Group>().Any(g => g.GroupName == HiddenCategoryName))
                                {
                                    DTESetup TheSetup = new DTESetup();
                                    Group group = new();
                                    group.GroupName = "Names";
                                    group.ParentCategory = hiddenCat;
                                    group.ParentEditor = DTEData;
                                    group.ParentGridItems = hiddenCat.GridItems;
                                    group.ParentGrid = hiddenCat.ItemGrid;

                                    hiddenCat.GridItems.Add(group);
                                    TheSetup.CreateGroup(group);
                                    // The Group named "Names" does not exist in the Hidden category
                                }

                                Group nameGroup = hiddenCat.GridItems.OfType<Group>().FirstOrDefault(group => group.GroupName == "Names");

                                {   
                                    EntryClass.ParentGridItems?.Remove(EntryClass); //Remove from grid

                                    EntryClass.ParentCategory = hiddenCat;
                                    EntryClass.ParentGroup = nameGroup;
                                    EntryClass.ParentGrid = nameGroup.ItemGrid;
                                    EntryClass.ParentGridItems = nameGroup.GridItems;

                                    EntryClass.Column = 0;
                                    EntryClass.Row = nameGroup.GridItems.Count; //Place at bottom of group.

                                    nameGroup.GridItems.Add(EntryClass);
                                }

                                return; //Stop it here. So if the name and description table are the same (lol) the name table takes priority. 
                            }
                        }
                    }
                }

                
            }

            //Part 2: Description Table Check.
            foreach (TextTable ExtraTable in DTEData.DescriptionTableList)
            {

                if (ExtraTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile) 
                {
                    if (DTEData.DataTable.FileDataTable.FileLocation == ExtraTable.TextTableFile.FileLocation)
                    {
                        int Min = DTEData.DataTable.DataTableStart;
                        int Max = Min + DTEData.DataTable.DataTableRowSize;
                        if (ExtraTable.TextTableStart >= Min && ExtraTable.TextTableStart <= Max)
                        {
                            int EXTRAMIN = ExtraTable.TextTableStart - DTEData.DataTable.DataTableStart;
                            int EXTRAMAX = EXTRAMIN + ExtraTable.TextTableCharLimit - 1;//the 1st byte is "0", so we need a -1 for proper counting.
                            if (EntryClass.RowOffset >= EXTRAMIN && EntryClass.RowOffset <= EXTRAMAX)
                            {
                                EntryClass.IsTextInUse = true;
                                MainName.Text = "Desc";

                                if (!DTEData.CategoryList.Any(cat => cat.CategoryName == HiddenCategoryName))
                                {
                                    DTESetup TheSetup = new DTESetup();
                                    Category category = new();
                                    category.CategoryName = HiddenCategoryName;
                                    category.DTEData = DTEData;

                                    DTEData.CategoryList.Add(category);
                                    TheSetup.CreateCategory(category);
                                }

                                Category hiddenCat = DTEData.CategoryList.FirstOrDefault(cat => cat.CategoryName == HiddenCategoryName);

                                if (!hiddenCat.GridItems.OfType<Group>().Any(g => g.GroupName == HiddenCategoryName))
                                {
                                    DTESetup TheSetup = new DTESetup();
                                    Group group = new();
                                    group.GroupName = "Descriptions";
                                    group.ParentCategory = hiddenCat;
                                    group.ParentEditor = DTEData;
                                    group.ParentGridItems = hiddenCat.GridItems;
                                    group.ParentGrid = hiddenCat.ItemGrid;

                                    group.Column = 1;

                                    hiddenCat.GridItems.Add(group);
                                    TheSetup.CreateGroup(group);
                                    // The Group named "Names" does not exist in the Hidden category
                                }

                                Group nameGroup = hiddenCat.GridItems.OfType<Group>().FirstOrDefault(group => group.GroupName == "Descriptions");

                                {
                                    EntryClass.ParentGridItems?.Remove(EntryClass); //Remove from grid

                                    EntryClass.ParentCategory = hiddenCat;
                                    EntryClass.ParentGroup = nameGroup;
                                    EntryClass.ParentGrid = nameGroup.ItemGrid;
                                    EntryClass.ParentGridItems = nameGroup.GridItems;

                                    EntryClass.Column = 0;
                                    EntryClass.Row = nameGroup.GridItems.Count; //Place at bottom of group.

                                    nameGroup.GridItems.Add(EntryClass);
                                }

                            }
                        }
                    }
                }
                
            }
        }


        //public static void InsertGridItem(GridItem entryToInsert, GridItem insertBelow)
        //{
        //    var targetItems = insertBelow.ParentGridItems;

        //    // 1. Remove from old location
        //    if (entryToInsert.ParentGridItems != null)
        //        entryToInsert.ParentGridItems.Remove(entryToInsert);

        //    // 2. Update ownership to match the "anchor"
        //    entryToInsert.ParentEditor = insertBelow.ParentEditor;
        //    entryToInsert.ParentCategory = insertBelow.ParentCategory;
        //    entryToInsert.ParentGroup = insertBelow.ParentGroup;
        //    entryToInsert.ParentGrid = insertBelow.ParentGrid;
        //    entryToInsert.ParentGridItems = targetItems;
        //    entryToInsert.Column = insertBelow.Column;

        //    // 3. Set the target row to exactly 1 below the anchor
        //    // We don't shift other items here; ApplyUpwardGravity and Normalize will fix overlaps
        //    entryToInsert.Row = insertBelow.Row + 1;

        //    // 4. Add to list
        //    targetItems.Add(entryToInsert);

        //    // 5. Let the global engine handle the coordinates
        //    UpdateEditorGrids(entryToInsert.ParentEditor.StandardEditorData);
        //}

        //public static void InsertGridItem(GridItem entryToInsert, GridItem insertBelow)
        //{
        //    var targetGrid = insertBelow.ParentGrid;
        //    var targetItems = insertBelow.ParentGridItems;
        //    int targetColumn = insertBelow.Column;

        //    // 1. Remove from old location safely
        //    if (entryToInsert.ParentGridItems != null)
        //    {
        //        entryToInsert.ParentGridItems.Remove(entryToInsert);
        //    }

        //    // 2. Update ownership
        //    entryToInsert.ParentEditor = insertBelow.ParentEditor;
        //    entryToInsert.ParentCategory = insertBelow.ParentCategory;
        //    entryToInsert.ParentGroup = insertBelow.ParentGroup;
        //    entryToInsert.ParentGrid = targetGrid;
        //    entryToInsert.ParentGridItems = targetItems;
        //    entryToInsert.Column = targetColumn;

        //    // 3. Temporarily place it just after the primary entry
        //    entryToInsert.Row = insertBelow.Row + 1;
        //    targetItems.Add(entryToInsert);

        //    // 4. FIX: Re-sort the entire column to prevent coordinate drift
        //    // This removes gaps and handles shifts perfectly.
        //    var columnItems = targetItems
        //        .Where(i => i.Column == targetColumn)
        //        .OrderBy(i => i.Row)
        //        .ThenBy(i => i == entryToInsert ? 1 : 0) // Ensure if rows tie, the new one stays below
        //        .ToList();

        //    int currentRow = 0;
        //    foreach (var item in columnItems)
        //    {
        //        item.Row = currentRow;
        //        currentRow += item.RowSpan;
        //    }

        //    // Rebuild UI
        //    //StandardEditorMethods.UpdateGridLayout(targetGrid, targetItems);
        //    UpdateEditorGrids(entryToInsert.ParentEditor.StandardEditorData);
        //}

        public static void InsertGridItem(GridItem item, GridItem primary)
        {
            var destItems = primary.ParentGridItems;

            if (item.ParentGridItems != null)
                item.ParentGridItems.Remove(item);

            item.ParentEditor = primary.ParentEditor;
            item.ParentCategory = primary.ParentCategory;
            item.ParentGroup = primary.ParentGroup;
            item.ParentGrid = primary.ParentGrid;
            item.ParentGridItems = destItems;
            item.Column = primary.Column;

            // 👇 THIS is the critical line
            item.Row = primary.Row; //+ 1; // + insertBelow.RowSpan;

            destItems.Add(item);
        }

        




        public static int GetColumnAt(Grid grid, double x)
        {
            double accumulatedWidth = 0;
            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                accumulatedWidth += grid.ColumnDefinitions[i].ActualWidth;
                if (x <= accumulatedWidth) return i;
            }
            return grid.ColumnDefinitions.Count - 1;
        }

        public static int GetRowAt(Grid grid, double y)
        {
            double accumulatedHeight = 0;
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                accumulatedHeight += grid.RowDefinitions[i].ActualHeight;
                if (y <= accumulatedHeight) return i;
            }
            return grid.RowDefinitions.Count - 1;
        }
        //public static void Grid_Drop(object sender, DragEventArgs e)
        //{
        //    if (TheWorkshop.IsPreviewMode) return;
        //    Grid targetGrid = sender as Grid;
        //    if (targetGrid == null) return;

        //    // 1. Determine where the mouse is
        //    Point dropPoint = e.GetPosition(targetGrid);
        //    int dropColumn = GetColumnAt(targetGrid, dropPoint.X);
        //    int dropRow = GetRowAt(targetGrid, dropPoint.Y);

        //    // 2. Identify the target list (Category or Group?)
        //    // This depends on how your UI is structured, but usually:
        //    var itemsList = GetItemsForGrid(targetGrid);

        //    // 3. VALIDATION: Is the cell empty and is everything below it empty?
        //    bool isAreaBelowEmpty = !itemsList.Any(i => i.Column == dropColumn && i.Row >= dropRow);

        //    if (!isAreaBelowEmpty)
        //    {
        //        // If there's an item at or below this row, we don't do an "empty drop"
        //        // This forces the user to drop ON an item to trigger the insert logic.
        //        return;
        //    }

        //    // 4. PERFORM MOVE
        //    if (e.Data.GetDataPresent(typeof(List<GridItem>)))
        //    {
        //        var draggedItems = (List<GridItem>)e.Data.GetData(typeof(List<GridItem>));

        //        // Move items to the new column/row
        //        foreach (var item in draggedItems)
        //        {
        //            item.ParentGridItems.Remove(item); // Remove from old list
        //            item.Column = dropColumn;
        //            item.Row = dropRow;
        //            itemsList.Add(item); // Add to new list

        //            // Note: Gravity will pull these up if there's a gap above
        //            // unless your ApplyUpwardGravity allows gaps.
        //        }

        //        StandardEditorMethods.UpdateGridLayout(targetGrid, itemsList);
        //    }
        //}

        public static void ReloadALLMenuEntrys(WorkshopData WorkshopData) 
        {
            foreach (DataTableEditorData TheDTEData in WorkshopData.GameEditors.OfType<DataTableEditorData>())
            {
                foreach (Entry entry in TheDTEData.MasterEntryList)
                {
                    if (entry.NewSubType == EntrySubTypes.Menu)
                    {
                        WorkshopData.EntryManagerOLD.ChangeEntryType(WorkshopData, EntrySubTypes.Menu, WorkshopData.WorkshopXaml, entry);
                    }
                }
            }
        }

        public static void UpdateHotbarForAllDTEEditors(WorkshopData WorkshopData) 
        {
            //The "Symbology" toggle.  (Magnifying Glass)
            foreach (DataTableEditorData editor in WorkshopData.GameEditors.OfType<DataTableEditorData>())  //Set color
            {
                if (LibraryGES.ShowSymbology == true)
                {
                    editor.DTEXaml.EntrySymbologyToggle.Foreground = Brushes.White;
                }
                else if (LibraryGES.ShowSymbology == false)
                {
                    editor.DTEXaml.EntrySymbologyToggle.Foreground = Brushes.Gray;
                }
            }

            //The "Hex/Dec" toggle.   (Dec/hex text)
            foreach (DataTableEditorData editor in WorkshopData.GameEditors.OfType<DataTableEditorData>())  //Set color
            {
                if (LibraryGES.EntryAddressType == "Decimal")
                {
                    editor.DataTableEditorData.DTEXaml.EntryAddressTypeButton.Content = "Dec";
                }
                else if (LibraryGES.EntryAddressType == "Hex")
                {
                    editor.DataTableEditorData.DTEXaml.EntryAddressTypeButton.Content = "Hex";
                }
            }

            //The "Show Hidden Entry" toggle. (Red eyeball)
            foreach (DataTableEditorData editor in WorkshopData.GameEditors.OfType<DataTableEditorData>())  //Set color
            {
                if (LibraryGES.ShowHiddenEntrys == true)
                {
                    editor.DataTableEditorData.DTEXaml.EntryHiddenToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC1E40"));
                }
                else if (LibraryGES.ShowHiddenEntrys == false)
                {
                    editor.DataTableEditorData.DTEXaml.EntryHiddenToggle.Foreground = Brushes.Gray;
                }
            }

            //The "Show ALL" toggle. (Only visible in Debug mode) (Blue eyeball)
            foreach (DataTableEditorData editor in WorkshopData.GameEditors.OfType<DataTableEditorData>())  //Set color
            {
                if (LibraryGES.DebugShowALL == true)
                {
                    editor.DataTableEditorData.DTEXaml.DebugShowALL.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E70EC"));
                }
                else if (LibraryGES.DebugShowALL == false)
                {
                    editor.DataTableEditorData.DTEXaml.DebugShowALL.Foreground = Brushes.Gray;
                }
            }

            

            
        }







    }
}
