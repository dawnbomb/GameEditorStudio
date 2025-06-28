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

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TheLeftBar.xaml
    /// </summary>
    public partial class TheLeftBar : UserControl
    {
        Workshop TheWorkshop;
        WorkshopData Database;
        Editor EditorClass;
        ByteManager ByteManager = new();

        bool FirstTime = true;
        bool _mousePressedOnItem = false;
        TreeViewItem item = null;
        EventHandler statusChangedHandler = null;


        public TheLeftBar(Workshop AWorkshop, WorkshopData ADatabase, Editor AEditor) //
        {
            InitializeComponent();

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

            ItemsTree.PreviewMouseLeftButtonDown += TreeView_PreviewMouseLeftButtonDown;
            ItemsTree.PreviewMouseMove += TreeViewItemMove;

            void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                var treeView = (TreeView)sender;
                var treeViewItem = GetTreeViewItemAtPoint(treeView, e.GetPosition(treeView));

                if (treeViewItem != null)
                {
                    _mousePressedOnItem = true;
                }
            }

            void TreeViewItemMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyUp(Key.LeftShift) && _mousePressedOnItem)
                {
                    var treeView = (TreeView)sender;
                    var treeViewItem = GetTreeViewItemAtPoint(treeView, e.GetPosition(treeView));

                    if (treeViewItem != null)
                    {
                        var data = new DataObject("MoveTreeViewItem", treeViewItem);
                        DragDrop.DoDragDrop(treeViewItem, data, DragDropEffects.Move);
                    }
                }
                else
                {
                    _mousePressedOnItem = false;
                }
            }

            TreeViewItem GetTreeViewItemAtPoint(ItemsControl control, Point point)
            {
                var hitTestResult = VisualTreeHelper.HitTest(control, point);
                var visualHit = hitTestResult?.VisualHit;

                while (visualHit != null)
                {
                    if (visualHit is TreeViewItem treeViewItem)
                    {
                        return treeViewItem;
                    }

                    visualHit = VisualTreeHelper.GetParent(visualHit);
                }

                return null;
            }



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


                SetChildrenHitTestVisible(TreeItem, false);

                void SetChildrenHitTestVisible(TreeViewItem treeViewItem, bool hitTestVisible)
                {
                    foreach (var child in treeViewItem.Items)
                    {
                        if (child is TreeViewItem childTreeViewItem)
                        {
                            childTreeViewItem.IsHitTestVisible = hitTestVisible;
                            SetChildrenHitTestVisible(childTreeViewItem, hitTestVisible);
                        }
                    }
                }


                TreeItem.AllowDrop = true;
                TreeItem.Drop += ItemDrop;
                void ItemDrop(object sender, DragEventArgs e)
                {
                    e.Handled = true;

                    TheWorkshop.TreeViewSelectionEnabled = false;
                    //I am not reordering the item list. This strikes me as problematic, and i will ignore it and hope nothing bad happens. :^)
                    if (e.Data.GetDataPresent("MoveTreeViewItem") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                    {
                        TreeViewItem InputItem = (TreeViewItem)e.Data.GetData("MoveTreeViewItem");

                        if (InputItem != TreeItem)
                        {
                            ItemInfo InputItemInfo = InputItem.Tag as ItemInfo;
                            ItemInfo TreeItemInfo = InputItem.Tag as ItemInfo;
                            TreeView parentTreeView = EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView;

                            if ((InputItemInfo.TreeItem.Items.Count > 0 || InputItemInfo.IsFolder == true)  && ItemInfo.TreeItem.Parent is TreeViewItem AparentItem) //Moving a item to anywhere inside a folder triggers drop twice, once for going to item and once for going to folder.
                            {
                                //This and CreateFolder in workshop.cs together, ban nested folders from existing. 
                                //I can add this feature very post release if i want to.
                                //However, it may also be better to refuse to allow them, to prevent workshop makers from creating nightmarish folder setups. 
                                TheWorkshop.TreeViewSelectionEnabled = true;
                                return;
                            }

                            if (InputItemInfo.TreeItem.Parent is TreeViewItem parentItem) //step 1: Removing input item
                            {
                                TreeViewItem ParentFolderItem = (TreeViewItem)InputItem.Parent;
                                ParentFolderItem.Items.Remove(InputItem);
                            }
                            else
                            {
                                parentTreeView.Items.Remove(InputItem);
                            }

                            //if (ItemInfo.TreeItem.Items.Count == 0) //later account for child folders
                            //{
                            //    if (ItemInfo.TreeItem.Parent is TreeViewItem TheParentFolderItem) //step 2: Insert input item.
                            //    {
                            //        int ToIndex = TheParentFolderItem.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                            //        TheParentFolderItem.Items.Insert(ToIndex, InputItem);
                            //    }
                            //    else
                            //    {
                            //        int ToIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                            //        parentTreeView.Items.Insert(ToIndex, InputItem);
                            //    }
                            //}
                            if (ItemInfo.TreeItem.Parent is TreeViewItem TheParentFolderItem) //step 2: Insert input item.
                            {
                                int ToIndex = TheParentFolderItem.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                                TheParentFolderItem.Items.Insert(ToIndex, InputItem);
                            }
                            else 
                            {
                                int ToIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                                parentTreeView.Items.Insert(ToIndex, InputItem);
                            }
                            

                            if (ItemInfo.TreeItem.Parent is TreeViewItem parentItem3) //Step 3: Update Item Type
                            {
                                InputItemInfo.IsChild = true;
                            }
                            else
                            {
                                InputItemInfo.IsChild = false;
                            }

                            InputItem.IsSelected = true;
                        }

                    }
                    TheWorkshop.TreeViewSelectionEnabled = true;

                    foreach (TreeViewItem TreeViewItemK in EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                    {
                        TheWorkshop.ItemNameBuilder(TreeViewItemK); //Doing this again here to make sure folder item counts work as intended.
                    }



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

            foreach (var TheEditor in Database.GameEditors)  //This code chunk is my ultra lazy way to update every Dropdown & List across all editors the moment any item changes its name incase that data is used in them.
            {
                foreach (var Cats in TheEditor.Value.StandardEditorData.CategoryList)
                {
                    foreach (var Grops in Cats.ColumnList)
                    {
                        foreach (var EndEntry in Grops.EntryList)
                        {
                            if (EndEntry.NewSubType == EntrySubTypes.Menu)
                            {

                                Database.EntryManager.EntryChange(Database, EntrySubTypes.Menu, TheWorkshop, EndEntry);
                            }
                        }
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

                if (TheWorkshop.IsPreviewMode == false)
                {
                    EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNameTextBox.Text = data.ItemName.TrimEnd('\0');
                    UpdateNameCharacterCount();
                }

                EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNoteTextbox.Text = data.ItemNote;
                EditorClass.StandardEditorData.EditorLeftDockPanel.ItemNotepadTextbox.Text = data.ItemWorkshopTooltip;

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
        //        Properties.Settings.Default.Save();

        //    }
        //    else if (Properties.Settings.Default.ShowItemIndex == "Show")
        //    {
        //        Properties.Settings.Default.ShowItemIndex = "Hide";
        //        Properties.Settings.Default.Save();

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
            LabelCharacterCount.Content = "Chars: " + (ItemNameTextbox.Text.Length).ToString() + " / " + EditorClass.StandardEditorData.NameTableTextSize.ToString(); //(ItemNameTextbox.Text.Length + 1)

            if (EditorClass.StandardEditorData.FileNameTable == null) { LabelCharacterCount.Content = "Fake Name List"; }
        }











    }
}
