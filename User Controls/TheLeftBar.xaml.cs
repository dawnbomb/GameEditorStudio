using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GameEditorStudio.Entry;


using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TheLeftBar.xaml
    /// </summary>
    public partial class TheLeftBar : UserControl
    {
        Workshop TheWorkshop { get; set; }
        WorkshopData Database { get; set; }
        Editor EditorClass { get; set; }

        bool FirstTime { get; set; } = true;
        bool _mousePressedOnItem { get; set; } = false;
        TreeViewItem item { get; set; } = null;
        EventHandler statusChangedHandler { get; set; } = null;

        //THIS IS A UNDO POINT
        // TO SEE WHERE TO STOP
        //WHAT i am DOING


        private List<TreeViewItem>? draggedItems { get; set; }
        private TreeViewItem? originallySelectedItem { get; set; }

        private void ItemsTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TheWorkshop.IsPreviewMode == true) { return; }

            var clickedItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (clickedItem == null)
                return;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var selectedItem = ItemsTree.SelectedItem as TreeViewItem ?? FindTreeViewItem(ItemsTree, ItemsTree.SelectedItem);
                if (selectedItem == null)
                    return;

                originallySelectedItem = selectedItem; // <--- Save original selection

                var itemsAtLevel = GetItemsAtSameLevel(selectedItem);
                int index1 = itemsAtLevel.IndexOf(selectedItem);
                int index2 = itemsAtLevel.IndexOf(clickedItem);
                if (index1 == -1 || index2 == -1)
                    return;

                int start = Math.Min(index1, index2);
                int end = Math.Max(index1, index2);
                draggedItems = itemsAtLevel.GetRange(start, end - start + 1);

                if (!AllSameParent(draggedItems))
                {
                    draggedItems = null;
                    return;
                }
            }
            else
            {
                draggedItems = new List<TreeViewItem> { clickedItem };
                originallySelectedItem = clickedItem; // <--- Save original selection
            }

            if (draggedItems.Count > 0)
            {
                DragDrop.DoDragDrop(ItemsTree, draggedItems, DragDropEffects.Move);
            }
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

            var targetInfo = targetItem.Tag as ItemInfo;
            if (targetInfo == null)
                return;

            var itemsToMove = e.Data.GetData(typeof(List<TreeViewItem>)) as List<TreeViewItem>;
            if (itemsToMove == null || itemsToMove.Count == 0)
                return;

            // === Rule 1: Prevent dropping a folder under any item that is already a child ===
            if (targetInfo.IsChild && itemsToMove.Any(i => (i.Tag as ItemInfo)?.IsFolder == true))
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

                if (draggedItem.Tag is ItemInfo info)
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

            foreach (TreeViewItem TreeViewItemK in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
            {
                TheWorkshop.ItemNameBuilder(TreeViewItemK); //Doing this here to make sure folder item counts work as intended.
            }

            e.Handled = true;
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

        public void LeftBarSetup(Workshop AWorkshop, WorkshopData ADatabase, Editor AEditor) 
        {
            TheWorkshop = AWorkshop;
            Database = ADatabase;
            EditorClass = AEditor;


            EditorClass.StandardEditorData.EditorLeftDockPanel.LeftBarDockPanel.Children.Add(this);
            EditorClass.StandardEditorData.EditorLeftDockPanel.SearchBar = SearchBar;
            EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView = ItemsTree;
            EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNameTextBox = ItemNameTextbox;
            EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNoteTextbox = ItemNoteTextbox;
            EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNotepadTextbox = ItemNotepadTextbox;


            if (TheWorkshop.IsPreviewMode == true)
            {
                SearchBar.IsEnabled = false;
                ItemNameTextbox.IsEnabled = false;
                ItemNoteTextbox.IsEnabled = false;
                ItemNotepadTextbox.IsEnabled = false;
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

            // Allow dropping on tree view items
            ItemsTree.AllowDrop = true;



            //This part makes it so every editor auto-selects it's first option.
            //I should clean this up later but it took so long to get working at all i don't care right now.            
            statusChangedHandler = (sender, e) =>
            {
                if (EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.ItemContainerGenerator.StatusChanged -= statusChangedHandler;
                    item = EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                    if (item != null)
                    {
                        item.IsSelected = true;
                    }
                }
            };

            if (EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.ItemContainerGenerator.StatusChanged += statusChangedHandler;
            }
            else
            {
                item = EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                if (item != null)
                {
                    item.IsSelected = true;
                }
            }
        }


        private void ItemsSetup() 
        {
            

            foreach (ItemInfo ItemInfo in EditorClass.StandardEditorData.EditorLeftDockPanel.ItemList)
            {
                TreeViewItem TreeItem = new();
                TreeItem.Tag = ItemInfo;
                ItemInfo.TreeItem = TreeItem;
                //This literally just sets the tree item header, but theres actually a lot of item customiation so
                //it gets it's own method in workshop.cs so the user can customize it then re-run the method.
                TheWorkshop.ItemNameBuilder(TreeItem);


                ContextMenu contextMenu = new ContextMenu();

                if (ItemInfo.IsFolder == false)
                {
                    MenuItem MenuItemCreateFolder = new MenuItem();
                    MenuItemCreateFolder.Header = "Create Folder (If not in folder)";
                    MenuItemCreateFolder.Click += (sender, e) => TheWorkshop.CreateFolder(ItemsTree, TreeItem);
                    contextMenu.Items.Add(MenuItemCreateFolder);
                }
                else if (ItemInfo.IsFolder == true)
                {
                    MenuItem MenuItemDeleteFolder = new MenuItem();
                    MenuItemDeleteFolder.Header = "Delete Folder (If Empty)";
                    MenuItemDeleteFolder.Click += (sender, e) => TheWorkshop.DeleteFolder(ItemsTree, TreeItem);
                    contextMenu.Items.Add(MenuItemDeleteFolder);
                }



                TreeItem.ContextMenu = contextMenu;







                if (ItemInfo.IsChild == true)
                {
                    TreeViewItem finalItem = (TreeViewItem)ItemsTree.Items.GetItemAt(ItemsTree.Items.Count - 1);
                    finalItem.Items.Add(TreeItem);
                }
                else
                {
                    ItemsTree.Items.Add(TreeItem);
                }


                

                

            }

            foreach (TreeViewItem TreeViewItemK in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
            {
                TheWorkshop.ItemNameBuilder(TreeViewItemK); //Doing this again here to make sure folder item counts work as intended.
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
                            if (((TreeViewItem)item).Tag is ItemInfo itemInfo)
                            {
                                // Search for the search text within the item name
                                if (itemInfo.ItemName.Contains(searchText))
                                {
                                    return true;
                                }

                                // Check the children of the current TreeViewItem
                                foreach (var childItem in ((TreeViewItem)item).Items)
                                {
                                    if (childItem is TreeViewItem childTreeViewItem && childTreeViewItem.Tag is ItemInfo childInfo)
                                    {
                                        if (childInfo.ItemName.Contains(searchText))
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
                if (EditorClass.StandardEditorData.NameTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (EditorClass.StandardEditorData.NameTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { return; }
                int NewByteSize = encoding.GetByteCount(NewText);

                if (NewByteSize > EditorClass.StandardEditorData.NameTableTextSize)
                {                    
                    e.Handled = true;  // Mark the event as handled so the input is ignored
                }
                else {  }
                
            }
            
        }

        private void ItemNameboxKeyDown(object sender, KeyEventArgs e)
        {            

            if (e.Key == Key.Enter)
            {
                UpdateItem();
            }
        }

        private void ItemNoteboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateItem();
            }
        }

        private void ItemNameboxTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateNameCharacterCount();
        }        

        private void UpdateItem() 
        {
            if (TheWorkshop.IsPreviewMode == true) { return; }
            TreeViewItem selectedItem = EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.SelectedItem as TreeViewItem;
            if (selectedItem == null && selectedItem.Tag == null) { return; }

            ItemInfo ItemInfo = selectedItem.Tag as ItemInfo;

            ItemInfo.ItemName = ItemNameTextbox.Text;
            ItemInfo.ItemNote = ItemNoteTextbox.Text;
            TheWorkshop.ItemNameBuilder(selectedItem);

            if (EditorClass.StandardEditorData.NameTableLinkType != StandardEditorData.NameTableLinkTypes.Nothing)
            {
                if (ItemInfo.IsFolder == false && EditorClass.StandardEditorData.FileNameTable.FileLocation != "" && EditorClass.StandardEditorData.FileNameTable.FileLocation != null) //The NameTableFilePath check prevents crashing when saving a note when the editor gets names from user instead of from file.
                {
                    CharacterSetManager CharacterSetManager = new();
                    CharacterSetManager.Encode(TheWorkshop, EditorClass, "Item", ItemInfo);
                }
            }

            //This code chunk is my ultra lazy way to update every Dropdown & List across all editors the moment any item changes its name incase that data is used in them.
            foreach (var TheEditor in Database.GameEditors.Values)  
            {
                foreach (Entry entry in TheEditor.StandardEditorData.MasterEntryList) 
                {
                    if (entry.NewSubType == EntrySubTypes.Menu)
                    {

                        Database.EntryManager.EntryChange(Database, EntrySubTypes.Menu, TheWorkshop, entry);
                    }
                }
                
            }


        }

        

        private void NotepadTextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewItem selectedItem = EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                //selectedItem.ToolTip = ItemNotepadTextbox.Text;
                // Get the selected ItemInfo class
                ItemInfo selectedInfo = selectedItem.Tag as ItemInfo;
                if (selectedInfo != null)
                {
                    // Update the Name property with the text from the TextBox
                    selectedInfo.ItemWorkshopTooltip = ItemNotepadTextbox.Text;
                    TheWorkshop.ItemNameBuilder(selectedItem);
                }
            }
        }

        private async void ItemsTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) //When the selected item is changed, we save the current item entry info, and load the new items info.
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //I disable selection effects sometimes when modifying items in the collection while it is open.
            if (TheWorkshop.TreeViewSelectionEnabled)
            {
                var selectedItem = e.NewValue as TreeViewItem;
                ItemInfo data = selectedItem.Tag as ItemInfo;
                EntryManager EManager = new();

                
                
                EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNameTextBox.Text = data.ItemName.TrimEnd('\0');
                EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNoteTextbox.Text = data.ItemNote;
                EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNotepadTextbox.Text = data.ItemWorkshopTooltip;
                UpdateNameCharacterCount();

                if (EditorClass.StandardEditorData.DescriptionTableList.Count > 0)
                {
                    CharacterSetManager CharacterSetManager = new();
                    CharacterSetManager.DecodeDescriptions(TheWorkshop, EditorClass);
                }

                if (selectedItem.Items.Count == 0) //This might cause bugs later with 0 child folders.
                {
                    EditorClass.StandardEditorData.TableRowIndex = data.ItemIndex;

                    for (int i = 0; i < EditorClass.StandardEditorData.MasterEntryList.Count; i++) //Changed from a foreach to a for loop, it reduced vesperia items load time from 6700ms to 6300ms. 
                    {
                        Entry entry = EditorClass.StandardEditorData.MasterEntryList[i];

                        if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
                        {
                            entry.EntryTypeNumberBox.NumberBoxCanSave = false;
                        }
                        
                        EManager.LoadEntry(TheWorkshop, EditorClass, entry);

                        if (FirstTime == false)
                        {
                            //EManager.UpdateEntryProperties(TheWorkshop, EditorClass);
                            EManager.UpdateEntryHexProperties(TheWorkshop, EditorClass);
                        }

                        if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
                        {
                            entry.EntryTypeNumberBox.NumberBoxCanSave = true;
                        }
                    }
                    

                    //TheWorkshop.EntryProperties.Visibility = Visibility.Collapsed;
                    //TheWorkshop.ItemProperties.Visibility = Visibility.Visible;
                    FirstTime = false;
                }
                
                //TranslationsPanel translationsPanel = this.TranslationsPanel;
                //await translationsPanel.UpdateTranslationsPanel(data);
                                

            } //End of If selection is enabled.

            stopwatch.Stop();
            Debug.WriteLine($"[Timer] Item Select took {stopwatch.ElapsedMilliseconds} ms");
        }




        //private void ToggleItemIDNumberVisibility(object sender, RoutedEventArgs e)
        //{
        //    if (Properties.Settings.Default.ShowItemIndex == "Hide")
        //    {
        //        Properties.Settings.Default.ShowItemIndex = "Show";

        //    }
        //    else if (Properties.Settings.Default.ShowItemIndex == "Show")
        //    {
        //        Properties.Settings.Default.ShowItemIndex = "Hide";

        //    }
        //    foreach (var editor in Database.GameEditors)
        //    {
        //        if (editor.Value.EditorType == "DataTable")
        //        {
        //            foreach (TreeViewItem TreeItem in editor.Value.SWData.EditorLeftDockPanel.TreeView.Items)
        //            {
        //                TheWorkshop.ItemNameBuilder(TreeItem);
        //            }
        //        }

        //    }
        //}

        private void UpdateNameCharacterCount() 
        {
            if (TheWorkshop.IsPreviewMode == true)
            {
                return;
            }

            LabelCharacterCount.Content = "Chars: " + (ItemNameTextbox.Text.Length).ToString() + " / " + EditorClass.StandardEditorData.NameTableTextSize.ToString(); //(ItemNameTextbox.Text.Length + 1)

            if (EditorClass.StandardEditorData.FileNameTable == null) { LabelCharacterCount.Content = "Fake Name List"; }
        }











    }
}
