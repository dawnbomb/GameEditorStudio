using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using static GameEditorStudio.Entry;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TheLeftBar.xaml
    /// </summary>
    public partial class TheLeftBar : UserControl
    {
        
        Workshop WorkshopXaml { get; set; }
        WorkshopData WorkshopData { get; set; }
        DataTableEditorData DTEData { get; set; }

        bool FirstTime { get; set; } = true;
        bool _mousePressedOnItem { get; set; } = false;
        TreeViewItem item { get; set; } = null;
        EventHandler statusChangedHandler { get; set; } = null;

        //THIS IS A UNDO POINT
        // TO SEE WHERE TO STOP
        //WHAT i am DOING


        private List<TreeViewItem>? draggedItems { get; set; }
        private TreeViewItem? originallySelectedItem { get; set; }

        private Point _dragStartPoint;
        private bool _isPotentialDrag;
        private List<TreeViewItem> _preparedDraggedItems;

        private void ItemsTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WorkshopXaml.IsPreviewMode == true) return;

            var clickedItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (clickedItem == null) return;

            // Record the starting point of the click
            _dragStartPoint = e.GetPosition(null);
            _isPotentialDrag = true;

            // Logic to figure out WHICH items would be dragged (Shift-selection logic)
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var selectedItem = ItemsTree.SelectedItem as TreeViewItem ?? FindTreeViewItem(ItemsTree, ItemsTree.SelectedItem);
                if (selectedItem != null)
                {
                    var itemsAtLevel = GetItemsAtSameLevel(selectedItem);
                    int index1 = itemsAtLevel.IndexOf(selectedItem);
                    int index2 = itemsAtLevel.IndexOf(clickedItem);

                    if (index1 != -1 && index2 != -1)
                    {
                        int start = Math.Min(index1, index2);
                        int end = Math.Max(index1, index2);
                        var range = itemsAtLevel.GetRange(start, end - start + 1);

                        if (AllSameParent(range))
                        {
                            _preparedDraggedItems = range;
                        }
                    }
                }
            }
            else
            {
                _preparedDraggedItems = new List<TreeViewItem> { clickedItem };
            }
        }

        private void ItemsTree_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPotentialDrag || e.LeftButton != MouseButtonState.Pressed) return;

            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            if (Math.Abs(diff.X) > 12 || Math.Abs(diff.Y) > 12)
            {
                _isPotentialDrag = false;

                if (_preparedDraggedItems != null && _preparedDraggedItems.Count > 0)
                {
                    // Start the drag
                    DragDrop.DoDragDrop(ItemsTree, _preparedDraggedItems, DragDropEffects.Move);

                    // Important: After the drag finishes, re-confirm selection
                    if (_preparedDraggedItems.Count > 0)
                    {
                        _preparedDraggedItems[0].IsSelected = true;
                        _preparedDraggedItems[0].Focus();
                    }

                    _preparedDraggedItems = null;
                }
            }
        }

        private void ItemsTree_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPotentialDrag = false;
            _preparedDraggedItems = null;
        }

        private void ItemsTree_DragOver(object sender, DragEventArgs e)
        {
            // Only allow move if dragging list of TreeViewItems
            if (!e.Data.GetDataPresent(typeof(List<TreeViewItem>)))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void ItemsTree_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(List<TreeViewItem>)))
                return;

            var targetItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (targetItem == null)
                return;

            var targetInfo = targetItem.Tag as TextInfo;
            if (targetInfo == null)
                return;

            var itemsToMove = e.Data.GetData(typeof(List<TreeViewItem>)) as List<TreeViewItem>;
            if (itemsToMove == null || itemsToMove.Count == 0)
                return;

            // === Rule 1: Prevent dropping a folder under any item that is already a child ===
            if (targetInfo.IsChild && itemsToMove.Any(i => (i.Tag as TextInfo)?.IsFolder == true))
                return;

            // Prevent dropping onto self or descendants
            if (itemsToMove.Contains(targetItem) || itemsToMove.Any(d => IsDescendantOf(targetItem, d)))
                return;

            var parent = GetParentTreeViewItem(targetItem);
            ItemCollection siblingsCollection = parent != null ? parent.Items : ItemsTree.Items;

            int targetIndex = siblingsCollection.IndexOf(targetItem);

            // Adjust for index shift if moving downward within the same collection
            int shiftCount = itemsToMove.Count(d =>
            {
                var oldParent = GetParentTreeViewItem(d);
                ItemCollection oldCollection = oldParent != null ? oldParent.Items : ItemsTree.Items;
                return oldCollection == siblingsCollection && oldCollection.IndexOf(d) < targetIndex;
            });

            targetIndex -= shiftCount;

            // Remove dragged items from old parents
            foreach (var draggedItem in itemsToMove)
            {
                var oldParent = GetParentTreeViewItem(draggedItem);
                if (oldParent != null)
                    oldParent.Items.Remove(draggedItem);
                else
                    ItemsTree.Items.Remove(draggedItem);
            }

            // === Rule 2 & 3: Set IsChild depending on the target item's position ===
            bool newIsChild = targetInfo.IsChild;

            // Insert after target
            int insertIndex = targetIndex + 1;
            foreach (var draggedItem in itemsToMove)
            {
                siblingsCollection.Insert(insertIndex, draggedItem);

                if (draggedItem.Tag is TextInfo info)
                    info.IsChild = newIsChild;

                insertIndex++;
            }

            // Restore original selection
            if (originallySelectedItem != null)
            {
                originallySelectedItem.IsSelected = true;
                originallySelectedItem.BringIntoView();
            }

            originallySelectedItem = null;
            draggedItems = null;
            
            foreach (TreeViewItem TreeViewItemK in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
            {
                DTEData.DTEXaml.ItemNameBuilder(TreeViewItemK); //Doing this here to make sure folder item counts work as intended.
            }

            e.Handled = true;

            UpdateNamesItemListOrder();
            //DTEData.DTEXaml.GenerateUI();
            DTEMethods.ReloadALLMenuEntrys(WorkshopData);
        }

        private void UpdateNamesItemListOrder()
        {
            // 1. Create a temporary list to hold the new order
            List<TextInfo> newOrderedList = new List<TextInfo>();

            // 2. Start the recursive scan from the root of the TreeView
            foreach (var item in ItemsTree.Items)
            {
                if (item is TreeViewItem tvi)
                {
                    ScanTreeViewRecursive(tvi, newOrderedList);
                }
            }

            // 3. Update the main data source
            DTEData.NameTable.ItemList = newOrderedList;

            // Update the count to reflect the new list size
            DTEData.NameTable.TextTableItemCount = newOrderedList.Count;
        }

        private void ScanTreeViewRecursive(TreeViewItem parentItem, List<TextInfo> resultList)
        {
            if (parentItem.Tag is TextInfo info)
            {
                // Add the current item to the list
                resultList.Add(info);

                // If this is a folder, it might have children
                if (info.IsFolder)
                {
                    // Clear the old children list and prepare for the new order
                    info.MyChildren = new List<TextInfo>();

                    foreach (var child in parentItem.Items)
                    {
                        if (child is TreeViewItem childTvi && childTvi.Tag is TextInfo childInfo)
                        {
                            // Add to the parent's children collection
                            info.MyChildren.Add(childInfo);

                            // Continue scanning deeper (in case of nested folders)
                            ScanTreeViewRecursive(childTvi, resultList);
                        }
                    }
                }
            }
        }


        // Helper: Get TreeViewItem's parent TreeViewItem (null if root level)
        private TreeViewItem? GetParentTreeViewItem(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (parent != null && !(parent is TreeViewItem))
                parent = VisualTreeHelper.GetParent(parent);

            return parent as TreeViewItem;
        }

        // Helper: Check if candidate is descendant of possibleAncestor
        private bool IsDescendantOf(TreeViewItem candidate, TreeViewItem possibleAncestor)
        {
            var parent = GetParentTreeViewItem(candidate);
            while (parent != null)
            {
                if (parent == possibleAncestor)
                    return true;
                parent = GetParentTreeViewItem(parent);
            }
            return false;
        }

        // Helper: Get TreeViewItem at same level as the given item (its siblings)
        private List<TreeViewItem> GetItemsAtSameLevel(TreeViewItem item)
        {
            var parent = GetParentTreeViewItem(item);
            ItemCollection collection;

            if (parent != null)
                collection = parent.Items;
            else
                collection = ItemsTree.Items;

            return collection.Cast<TreeViewItem>().ToList();
        }

        // Helper: Check all items share same parent
        private bool AllSameParent(List<TreeViewItem> items)
        {
            if (items.Count == 0)
                return true;

            var parent = GetParentTreeViewItem(items[0]);

            return items.All(i => GetParentTreeViewItem(i) == parent);
        }

        // Helper: Visual tree search upwards for ancestor of type T
        private static T? VisualUpwardSearch<T>(DependencyObject? source) where T : DependencyObject
        {
            while (source != null && !(source is T))
            {
                DependencyObject? parent = null;

                // Try visual parent first
                if (source is Visual || source is System.Windows.Media.Media3D.Visual3D)
                {
                    parent = VisualTreeHelper.GetParent(source);
                }

                // If no visual parent, try logical parent
                if (parent == null)
                {
                    parent = LogicalTreeHelper.GetParent(source);
                }

                source = parent;
            }

            return source as T;
        }

        // Helper: Find TreeViewItem container from data item (if SelectedItem is not a TreeViewItem)
        private TreeViewItem? FindTreeViewItem(ItemsControl container, object dataItem)
        {
            if (container == null)
                return null;

            for (int i = 0; i < container.Items.Count; i++)
            {
                var item = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (item == null)
                    continue;

                if (item.DataContext == dataItem || item == dataItem)
                    return item;

                var childItem = FindTreeViewItem(item, dataItem);
                if (childItem != null)
                    return childItem;
            }

            return null;
        }






























        public TheLeftBar() //
        {
            InitializeComponent();                  
            
        }

        private void UpdateItem()
        {
            if (WorkshopXaml.IsPreviewMode == true) { return; }
            TreeViewItem selectedItem = DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
            if (selectedItem == null && selectedItem.Tag == null) { return; }

            TextInfo ItemInfo = selectedItem.Tag as TextInfo;

            ItemInfo.ItemName = ItemNameTextbox.Text;
            ItemInfo.ItemNote = ItemNoteTextbox.Text;
            DTEData.DTEXaml.ItemNameBuilder(selectedItem);



            if (DTEData.NameTable.TextTableLinkType != TextTable.TextTableLinkTypes.Nothing)
            {
                if (ItemInfo.IsFolder == false && DTEData.NameTable.TextTableFile.FileLocation != "" && DTEData.NameTable.TextTableFile.FileLocation != null) //The NameTableFilePath check prevents crashing when saving a note when the editor gets names from user instead of from file.
                {
                    CharacterSetManager CharacterSetManager = new();
                    CharacterSetManager.EncodeItem(DTEData, ItemInfo);
                }
            }

            DTEMethods.ReloadALLMenuEntrys(WorkshopData); //Reload menus so menus using this editors name order get updated. 
        }

        public void SetupDataTableEditorLeftBar(WorkshopData ADatabase, DataTableEditorData TheDataTableEditorData) 
        {            
            WorkshopData = ADatabase;
            WorkshopXaml = WorkshopData.WorkshopXaml;
            DTEData = TheDataTableEditorData;

            DTEData.EditorLeftBar.LeftBarXaml = this;
            DTEData.EditorLeftBar.SearchBar = SearchBar;
            DTEData.EditorLeftBar.TreeView = ItemsTree;
            DTEData.EditorLeftBar.ItemNameTextBox = ItemNameTextbox;
            DTEData.EditorLeftBar.ItemNoteTextbox = ItemNoteTextbox;
            DTEData.EditorLeftBar.ItemNotepadTextbox = ItemNotepadTextbox;


            if (ADatabase.IsProjectLoaded == false)
            {
                SearchBar.IsEnabled = false;
                ItemsTree.IsEnabled = false;

                ItemNameTextbox.IsEnabled = false;
                ItemNoteTextbox.IsEnabled = false;
                ItemNotepadTextbox.IsEnabled = false;
                
            }
            if (ADatabase.IsProjectLoaded == true)
            {
                SearchBar.IsEnabled = true;
                ItemsTree.IsEnabled = true;

                ItemNameTextbox.IsEnabled = true;
                ItemNoteTextbox.IsEnabled = true;
                ItemNotepadTextbox.IsEnabled = true;
            }


            SearchBar.GotFocus += (sender, e) =>
            {
                if (SearchBar.Text == "🔎 Search.....")
                {
                    SearchBar.Text = "";
                }
            };
            SearchBar.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrEmpty(SearchBar.Text))
                {
                    SearchBar.Text = "🔎 Search.....";
                }
            };

            ItemsSetup();

            
            ItemsTree.PreviewMouseLeftButtonDown += ItemsTree_PreviewMouseLeftButtonDown;
            ItemsTree.Drop += ItemsTree_Drop;
            ItemsTree.DragOver += ItemsTree_DragOver;

            ItemsTree.PreviewMouseMove += ItemsTree_PreviewMouseMove;
            ItemsTree.PreviewMouseLeftButtonUp += ItemsTree_PreviewMouseLeftButtonUp;

            // Allow dropping on tree view items
            ItemsTree.AllowDrop = true;

            
            



            ContextMenu contextMenu = new ContextMenu();
            ItemsLabel.ContextMenu = contextMenu;

            MenuItem MenuItemCreateNewItem = new MenuItem();
            MenuItemCreateNewItem.Header = "Export current item names (in origonal name order) to a text file.";
            contextMenu.Items.Add(MenuItemCreateNewItem);
            MenuItemCreateNewItem.Click += (sender, e) => { ExportItemNamesOrigonalToTextFile(); };

            MenuItem MenuItemCreateNewItem2 = new MenuItem();
            MenuItemCreateNewItem2.Header = "Export current item names (in current name order) to a text file.";
            contextMenu.Items.Add(MenuItemCreateNewItem2);
            MenuItemCreateNewItem2.Click += (sender, e) => { ExportItemNamesCurrentToTextFile(); };
            //MenuItemCreateNewItem2.Click += (sender, e) => TheWorkshop.ExportItemNamesCurrentToTextFile(EditorClass);
        }

        private void ItemsSetup()
        {
            TextInfo previouslySelectedItemTag = null;
            {
                TreeViewItem previousItem = DTEData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
                if (previousItem != null)
                {
                    TextInfo ApreviousItemTag = previousItem.Tag as TextInfo;
                    if (ApreviousItemTag.IsFolder == false) { previouslySelectedItemTag = ApreviousItemTag; }
                }
            }

            DTEData.EditorLeftBar.TreeView.Items.Clear();

            if (DTEData.NameTable != null)
            {
                foreach (TextInfo ItemInfo in DTEData.NameTable.ItemList)
                {
                    TreeViewItem TreeItem = new();
                    TreeItem.Tag = ItemInfo;
                    ItemInfo.TreeItem = TreeItem;
                    //This literally just sets the tree item header, but theres actually a lot of item customiation so
                    //it gets it's own method in workshop.cs so the user can customize it then re-run the method.
                    DTEData.DTEXaml.ItemNameBuilder(TreeItem);

                    if (ItemInfo.IsChild == true)
                    {
                        TreeViewItem finalItem = (TreeViewItem)ItemsTree.Items.GetItemAt(ItemsTree.Items.Count - 1);
                        finalItem.Items.Add(TreeItem);
                    }
                    else
                    {
                        ItemsTree.Items.Add(TreeItem);
                    }


                    ContextMenu contextMenu = new ContextMenu();
                    TreeItem.ContextMenu = contextMenu;

                    if (ItemInfo.IsFolder == false)
                    {
                        MenuItem MenuItemCreateFolder = new MenuItem();
                        MenuItemCreateFolder.Header = "Create Folder (If not in folder)";
                        MenuItemCreateFolder.Click += (sender, e) => CreateFolder(ItemsTree, TreeItem);
                        contextMenu.Items.Add(MenuItemCreateFolder);
                    }
                    else if (ItemInfo.IsFolder == true)
                    {
                        MenuItem MenuItemDeleteFolder = new MenuItem();
                        MenuItemDeleteFolder.Header = "Delete Folder (If Empty)";
                        MenuItemDeleteFolder.Click += (sender, e) => DeleteFolder(ItemsTree, TreeItem);
                        contextMenu.Items.Add(MenuItemDeleteFolder);
                    }



                    void CreateFolder(TreeView TreeView, TreeViewItem TreeViewItem)
                    {
                        TextInfo TreeViewItemInfo = TreeViewItem.Tag as TextInfo;
                        if (TreeViewItemInfo.IsFolder == true || TreeViewItemInfo.IsChild == true) { return; }

                        DTEData.WorkshopXaml.TreeViewSelectionEnabled = false;


                        int selectedIndex = TreeView.ItemContainerGenerator.IndexFromContainer(TreeViewItem);
                        TreeView.Items.Remove(TreeViewItem);

                        TreeViewItem FolderItem = new TreeViewItem();
                        TextInfo FolderItemInfo = new();
                        FolderItem.Tag = FolderItemInfo;

                        FolderItemInfo.ItemName = "New Folder";
                        FolderItemInfo.IsFolder = true;
                        if (TreeViewItem.Tag is TextInfo itemInfo)
                        {
                            itemInfo.IsChild = true;
                        }


                        TreeView.Items.Insert(selectedIndex, FolderItem);
                        FolderItem.Items.Add(TreeViewItem);
                        DTEData.DTEXaml.ItemNameBuilder(FolderItem); //Created the Header text as a TextBlockItem


                        FolderItem.IsExpanded = true;
                        DTEData.WorkshopXaml.TreeViewSelectionEnabled = true;
                        TreeViewItem.IsSelected = true;

                        ContextMenu contextMenu = new ContextMenu();

                        MenuItem MenuItemDeleteFolder = new MenuItem();
                        MenuItemDeleteFolder.Header = "Delete Folder (If Empty)";
                        MenuItemDeleteFolder.Click += (sender, e) => DeleteFolder(TreeView, FolderItem);
                        contextMenu.Items.Add(MenuItemDeleteFolder);

                        FolderItem.ContextMenu = contextMenu;
                    }

                    void DeleteFolder(TreeView TreeView, TreeViewItem TreeViewItem)
                    {
                        TextInfo TreeViewItemInfo = TreeViewItem.Tag as TextInfo;
                        if (TreeViewItemInfo.IsFolder == false || TreeViewItem.Items.Count > 0) { return; }

                        DTEData.WorkshopXaml.TreeViewSelectionEnabled = false;
                        TreeView.Items.Remove(TreeViewItem);
                        DTEData.WorkshopXaml.TreeViewSelectionEnabled = true;
                    }

                }
            }




            foreach (TreeViewItem TreeViewItemK in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
            {
                DTEData.DTEXaml.ItemNameBuilder(TreeViewItemK); //Doing this again here to make sure folder item counts work as intended.
            }


            //Select the first name item in the left bar. 
            foreach (TreeViewItem item in ItemsTree.Items)
            {
                TextInfo textinfo = item.Tag as TextInfo;
                try
                {
                    if (textinfo.IsFolder == false)
                    {
                        item.IsSelected = true;
                        break;
                    }
                    if (textinfo.IsFolder == true)
                    {
                        bool found = false;
                        foreach (TreeViewItem Fitem in item.Items)
                        {
                            TextInfo Ftextinfo = Fitem.Tag as TextInfo;
                            try
                            {
                                if (Ftextinfo.IsFolder == false)
                                {
                                    Fitem.IsSelected = true;
                                    found = true;
                                    item.IsExpanded = true;
                                    break;
                                }
                            }
                            catch
                            {
                                continue; //If this ever triggers it's almost certinly an error and needs to be fixed. 
                            }
                        }
                        if (found == true) { break; }
                        continue;
                    }


                }
                catch //just in case, idk why this would ever trigger though...?
                {
                    continue; //If this ever triggers it's almost certinly an error and needs to be fixed. 
                }

            }

            if (previouslySelectedItemTag != null)
            {
                foreach (TreeViewItem TreeViewItemP in LibraryGES.GetALLTreeViewItems(DTEData.EditorLeftBar.TreeView))
                {

                    TextInfo ThisItemTag = TreeViewItemP.Tag as TextInfo;
                    if (ThisItemTag.IsFolder == false)
                    {
                        if (previouslySelectedItemTag == ThisItemTag) { TreeViewItemP.IsSelected = true; break; }
                    }
                }
            }



        }

        private async void ItemsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) //When the selected item is changed, we save the current item entry info, and load the new items info.
        {
            if (WorkshopXaml.TreeViewSelectionEnabled == false) //I disable selection effects sometimes when modifying items in the collection while it is open.
            {
                return;
            }
            DTEData.DTEXaml.DescriptionsBottomBar.Visibility = Visibility.Collapsed;

            var selectedItem = e.NewValue as TreeViewItem;
            if (selectedItem == null) { return; }

            TextInfo data = selectedItem.Tag as TextInfo;
            DTEData.EditorLeftBar.ItemNameTextBox.Text = data.ItemName.TrimEnd('\0');
            DTEData.EditorLeftBar.ItemNoteTextbox.Text = data.ItemNote;
            DTEData.EditorLeftBar.ItemNotepadTextbox.Text = data.ItemWorkshopTooltip;
            UpdateNameCharacterCount();

            if (DTEData.DescriptionTableList.Count > 0)
            {
                DTEData.DTEXaml.DescriptionsBottomBar.Visibility = Visibility.Visible;
                CharacterSetManager CharacterSetManager = new();
                CharacterSetManager.DecodeDescriptions(WorkshopXaml, DTEData);

                //This updates the char count. Yes this is shit to put this code here, no i don't care. 
                {

                    if (DTEData.DescriptionTableList[0].DescriptionTableTextBox != null) //If for load project... (Remove later?)
                    {
                        string NewText = DTEData.DescriptionTableList[0].DescriptionTableTextBox.Text; //+ e.Text;
                        Encoding encoding;
                        if (DTEData.DescriptionTableList[0].TextTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                        else if (DTEData.DescriptionTableList[0].TextTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                        else { return; }
                        int NewByteSize = encoding.GetByteCount(NewText);

                        if (DTEData.DescriptionTableList[0].TextTableLinkType == TextTable.TextTableLinkTypes.DataFile)
                        {
                            DTEData.DTEXaml.DescriptionCharCount.Content = "Chars: " + NewByteSize + " / " + DTEData.DescriptionTableList[0].TextTableCharLimit;
                        }
                        if (DTEData.DescriptionTableList[0].TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                        {
                            foreach (TextInfo textInfo in DTEData.DescriptionTableList[0].ItemList)
                            {
                                if (data.ItemIndex == textInfo.ItemIndex)
                                {
                                    int Padding = textInfo.RowEnd + 1 - textInfo.RowStart;

                                    DTEData.DTEXaml.DescriptionCharCount.Content = "Chars: " + NewByteSize + " / " + Padding;
                                }
                            }
                        }


                    }

                    if (WorkshopData.IsProjectLoaded == false) { DTEData.DTEXaml.DescriptionCharCount.Content = "Not available in preview mode..."; }

                } // End of Update Char Count.


            }

            if (selectedItem.Items.Count == 0) //This might cause bugs later with 0 child folders.
            {
                DTEData.TableRowIndex = data.ItemIndex;

                EntryManager EManager = new();

                for (int i = 0; i < DTEData.MasterEntryList.Count; i++) //Changed from a foreach to a for loop, it reduced vesperia items load time from 6700ms to 6300ms. 
                {
                    Entry entry = DTEData.MasterEntryList[i];

                    if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
                    {
                        entry.EntryTypeNumberBox.NumberBoxCanSave = false;
                    }

                    
                    EManager.LoadEntry(DTEData, entry);

                    //if (FirstTime == false)
                    //{
                    //    //Task.Run(() => {  EManager.UpdateEntryHexProperties(DTEData); }  );  
                    //    //EManager.UpdateEntryHexProperties(DTEData); //This also updates the cross reference sheet.
                    //}

                    if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
                    {
                        entry.EntryTypeNumberBox.NumberBoxCanSave = true;
                    }
                }


                //TheWorkshop.EntryProperties.Visibility = Visibility.Collapsed;
                //TheWorkshop.ItemProperties.Visibility = Visibility.Visible;
                //FirstTime = false;

                if (FirstTime == false)
                {
                    //Task.Run(() => {  EManager.UpdateEntryHexProperties(DTEData); }  );  
                    EManager.UpdateEntryHexProperties(DTEData); //This also updates the cross reference sheet.
                }
                //EManager.UpdateEntryHexProperties(DTEData); //This also updates the cross reference sheet.
                FirstTime = false;
            }

            //TranslationsPanel translationsPanel = this.TranslationsPanel;
            //await translationsPanel.UpdateTranslationsPanel(data);


        }


























        private void ExportItemNamesOrigonalToTextFile()
        {
            List<TextInfo> allItems = new List<TextInfo>();

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = ItemsTree.Items.Count - 1; i >= 0; i--)
                stack.Push((TreeViewItem)ItemsTree.Items[i]);

            while (stack.Count > 0)
            {
                TreeViewItem item = stack.Pop();
                TextInfo itemInfo = item.Tag as TextInfo;
                if (itemInfo != null && !itemInfo.IsFolder)
                {
                    allItems.Add(itemInfo);
                }

                // Add children to stack
                for (int i = item.Items.Count - 1; i >= 0; i--)
                    stack.Push((TreeViewItem)item.Items[i]);
            }

            // Sort by ItemIndex
            var sortedItems = allItems.OrderBy(ii => ii.ItemIndex);

            // Build output
            string Output = string.Join(Environment.NewLine, sortedItems.Select(ii => ii.ItemName.TrimEnd('\0')));

            // Ask user where to save
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save Item Names (using origonal name order)",
                Filter = "Text File (*.txt)|*.txt",
                DefaultExt = ".txt",
                FileName = "ItemNames.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, Output);
            }
        }

        private void ExportItemNamesCurrentToTextFile()
        {
            string Output = "";

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();

            // seed with root items (push in reverse so top item is processed first)
            for (int i = ItemsTree.Items.Count - 1; i >= 0; i--)
            {
                stack.Push((TreeViewItem)ItemsTree.Items[i]);
            }

            while (stack.Count > 0)
            {
                TreeViewItem item = stack.Pop();

                TextInfo itemInfo = item.Tag as TextInfo;
                if (itemInfo != null)
                {
                    // still skip folders
                    if (itemInfo.IsFolder == false)
                    {
                        Output += itemInfo.ItemName.TrimEnd('\0') + Environment.NewLine;
                    }
                }

                // push children in reverse so they maintain visual order
                for (int i = item.Items.Count - 1; i >= 0; i--)
                {
                    stack.Push((TreeViewItem)item.Items[i]);
                }
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save Item Names (using current name order)",
                Filter = "Text File (*.txt)|*.txt",
                DefaultExt = ".txt",
                FileName = "ItemNames.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, Output);
            }
        }


        






        private void SearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchBar.TextChanged += (sender, e) =>
            {
                string searchText = SearchBar.Text;

                if (!string.IsNullOrEmpty(searchText) && searchText != "🔎 Search.....")
                {
                    if (string.IsNullOrEmpty(searchText))
                    {
                        ItemsTree.Items.Filter = null;
                    }
                    else
                    {
                        ItemsTree.Items.Filter = (item) =>
                        {
                            if (((TreeViewItem)item).Tag is TextInfo itemInfo)
                            {
                                // Search for the search text within the item name
                                if (itemInfo.ItemName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || itemInfo.ItemNote.Contains(searchText, StringComparison.OrdinalIgnoreCase) || itemInfo.ItemWorkshopTooltip.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }

                                // Check the children of the current TreeViewItem
                                foreach (var childItem in ((TreeViewItem)item).Items)
                                {
                                    if (childItem is TreeViewItem childTreeViewItem && childTreeViewItem.Tag is TextInfo childInfo)
                                    {
                                        if (childInfo.ItemName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || childInfo.ItemNote.Contains(searchText, StringComparison.OrdinalIgnoreCase) || childInfo.ItemWorkshopTooltip.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        };
                    }
                }
                else
                {
                    // Reset the filter when the search text is empty or the placeholder
                    ItemsTree.Items.Filter = null;
                }


            };
        }

        private void ItemNameboxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ItemsTree.SelectedItem == null) { return; }

            if (e.Key == Key.Space || (e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9))
            {
                string NewText;

                if (e.Key == Key.Space)
                {
                    NewText = ItemNameTextbox.Text + " ";
                }
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                    NewText = ItemNameTextbox.Text + (char)('A' + (e.Key - Key.A));
                }
                else // e.Key >= Key.D0 && e.Key <= Key.D9
                {
                    NewText = ItemNameTextbox.Text + (char)('0' + (e.Key - Key.D0));
                }

                Encoding encoding;
                if (DTEData.NameTable.TextTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (DTEData.NameTable.TextTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { return; }
                int NewByteSize = encoding.GetByteCount(NewText);

                if (NewByteSize > DTEData.NameTable.TextTableCharLimit && DTEData.NameTable.TextTableLinkType != TextTable.TextTableLinkTypes.Advanced)
                {                    
                    e.Handled = true;  // Mark the event as handled so the input is ignored
                }
                else if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced) 
                {
                    TreeViewItem selectedItem = DTEData.DataTableEditorData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
                    if (selectedItem == null) return;
                    TextInfo selectedInfo = selectedItem.Tag as TextInfo;
                    if (selectedInfo == null) return;

                    int CharLimit = selectedInfo.RowEnd + 1 - selectedInfo.RowStart;

                    if (NewByteSize > CharLimit) 
                    {
                        e.Handled = true;
                    }
                    
                }
                
            }
            
        }

        private void ItemNameboxKeyDown(object sender, KeyEventArgs e)
        {
            if (ItemsTree.SelectedItem == null) { return; }

            if (e.Key == Key.Enter)
            {
                UpdateItem();
            }
        }

        private void ItemNoteboxKeyDown(object sender, KeyEventArgs e)
        {
            if (ItemsTree.SelectedItem == null) { return; }

            if (e.Key == Key.Enter)
            {
                UpdateItem();
            }
        }

        private void ItemNameboxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ItemsTree.SelectedItem == null) { return; }

            UpdateNameCharacterCount();
        }        

        

        

        private void NotepadTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ItemsTree.SelectedItem == null) { return; }

            TreeViewItem selectedItem = DTEData.DataTableEditorData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                //selectedItem.ToolTip = ItemNotepadTextbox.Text;
                // Get the selected ItemInfo class
                TextInfo selectedInfo = selectedItem.Tag as TextInfo;
                if (selectedInfo != null)
                {
                    // Update the Name property with the text from the TextBox
                    selectedInfo.ItemWorkshopTooltip = ItemNotepadTextbox.Text;
                    DTEData.DTEXaml.ItemNameBuilder(selectedItem);
                }
            }
        }       



        private void UpdateNameCharacterCount() 
        {
            if (WorkshopXaml.IsPreviewMode == true)
            {
                return;
            }

            LabelCharacterCount.Content = "";

            try 
            {

                if (DTEData.NameTable.TextTableFile == null)
                {
                    LabelCharacterCount.Content = "Fake Name List";
                }
                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.DataFile || DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.TextFile)
                {
                    LabelCharacterCount.Content = "Chars: " + (ItemNameTextbox.Text.Length).ToString() + " / " + DTEData.NameTable.TextTableCharLimit.ToString(); //(ItemNameTextbox.Text.Length + 1)
                }
                if (DTEData.NameTable.TextTableLinkType == TextTable.TextTableLinkTypes.Advanced)
                {
                    TreeViewItem selectedItem = DTEData.DataTableEditorData.EditorLeftBar.TreeView.SelectedItem as TreeViewItem;
                    if (selectedItem == null) return;
                    TextInfo selectedInfo = selectedItem.Tag as TextInfo;
                    if (selectedInfo == null) return;

                    int CharLimit = selectedInfo.RowEnd + 1 - selectedInfo.RowStart; //DTEData.NameTable.TextTableTextSize

                    LabelCharacterCount.Content = "Chars: " + (ItemNameTextbox.Text.Length).ToString() + " / " + CharLimit.ToString(); //(ItemNameTextbox.Text.Length + 1)
                }

                

                
            }
            catch 
            { 
                
            }
            
        }











    }
}
