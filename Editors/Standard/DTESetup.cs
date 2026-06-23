using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing.Printing;

//using System.Drawing;
using System.Formats.Asn1;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using Path = System.IO.Path;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GameEditorStudio
{
    public class DTESetup
    {
        //This triggers when a editor is generated. I split the editor setup code into this file because it's a ton of code. 
        //This pulls information from the Database, and builds the main part of the editor UI based on that.
        //Stuff regarding the left bar is in a seperate file. 

        public void SetupDataTableEditorMiddle(WorkshopData Database, DataTableEditorData DTEData)
        {
            DTEData.MainDockPanel.Children.Clear();
            
            if (DTEData.EntryClass == null) //If there is not yet a selected entry, we set it to the first possible entry as the selected entry. 
            {
                foreach (var category in DTEData.CategoryList)
                {
                    foreach (GridItem itembase in category.GridItems)
                    {
                        if (itembase is Group group)
                        {
                            if (group.GridItems.Count != 0)
                            {
                                try
                                {
                                    DTEData.EntryClass = group.GridItems[0] as Entry;
                                    break; // Exit as soon as one is found
                                }
                                catch { }
                            }
                        }

                        if (itembase is Entry entry)
                        {
                            DTEData.EntryClass = entry;
                            break; // Exit as soon as one is found
                        }
                    }
                    if (DTEData.EntryClass != null) { break; }
                }
            }


            

            //This creates the entire core part of the editor.
            foreach (Category CatClass in DTEData.CategoryList)
            {
                CreateCategory(CatClass);

                foreach (GridItem itembase in CatClass.GridItems) 
                {
                    if (itembase is Group group)
                    {
                        CreateGroup(group);

                        //foreach (Entry EntryClass in group.GridItems)
                        //{
                        //    CreateEntry(EntryClass);
                        //}
                        
                    }

                    //if (itembase is Entry entry)
                    //{
                    //    CreateEntry(entry);
                    //}
                }
                
            }
            //foreach (Entry entry in DTEData.MergedEntryList)
            //{
            //    CreateEntry(entry);
            //}
            foreach (Entry entry in DTEData.MasterEntryList)
            {
                CreateEntry(entry);
            }

            //Finally, we "properly select" the selected entry. A few things happen during this.
            //A lot of Various information is updated when a item becomes the selected item, so this gets it's own method.             
            if (DTEData.EntryClass != null) 
            {
                EntryManager EManager = new();
                EManager.SetSelectedEntry(DTEData.EntryClass); 
                EManager.UpdateEntryProperties(DTEData.EntryClass);
            }

            //This rebuilds the grid visual and can forcibly move grid items around to follow certain rules. 
            //Im not sure if i actually even need this here, but oh well :p
            DTEMethods.UpdateEditorGrids(DTEData); 
            

        }




        public void CreateCategory(Category CatClass)
        {
            DataTableEditorData DTEData = CatClass.DTEData; //This is the StandardEditorData that contains all the information about the editor, such as the categories and columns.
            Workshop TheWorkshop = CatClass.DTEData.WorkshopXaml; //This is the workshop that contains the editor, and is used to access the main grid and other workshop related information.
            WorkshopData Database = CatClass.DTEData.WorkshopXaml.WorkshopData; //This is the database that contains all the information about the game, such as the entries and columns.

            if (DTEData == null || TheWorkshop == null || Database == null) { PixelWPF.LibraryPixel.NotificationNegative("Critical Error!", "Create Category error, will crash soon. Report this D:"); }

            int TheIndex = CatClass.DTEData.CategoryList.IndexOf(CatClass);
            //CatClass.CatBorder = new();

            //DependencyObject parent = VisualTreeHelper.GetParent(CatClass.CatBorder);
            //if (parent != null) { if (parent is Panel panel) { panel.Children.Remove(CatClass.CatBorder); } }          


            //if (DTEData.MainDockPanel.Children.Contains(CatClass.CatBorder))
            //{
            //    string ts = "Category already exists in MainDockPanel, not creating a new one.";
            //}
            //else {  }

            //A category contains a number of columns.
            //This is useful for spread-sheet like editors needing to exist.
            //Such as etrian odyssey untold 2 have skills with 20 levels, and many attributes, all assigned per level.            

            Border CatBorder = new();
            CatClass.CatBorder = CatBorder;
            DTEData.MainDockPanel.Children.Insert(TheIndex, CatClass.CatBorder);
            CatBorder.Style = (Style)Application.Current.Resources["RowBorder"];
            DockPanel.SetDock(CatBorder, Dock.Top);
            CatBorder.Margin = new Thickness(0, 0, 0, 0);

            CatBorder.BorderThickness = new Thickness(0);
            CatBorder.BorderBrush = new SolidColorBrush(Colors.Red);

            CatBorder.VerticalAlignment = VerticalAlignment.Stretch;
            CatBorder.HorizontalAlignment = HorizontalAlignment.Stretch;



            DockPanel CatPanel = new();
            CatClass.CategoryBody = CatPanel;
            CatPanel.LastChildFill = true;
            CatPanel.Style = (Style)Application.Current.Resources["RowStyle"];
            DockPanel.SetDock(CatPanel, Dock.Top);
            CatPanel.VerticalAlignment = VerticalAlignment.Stretch; //Top Bottom
            CatPanel.HorizontalAlignment = HorizontalAlignment.Stretch; //Left Right
            CatPanel.Background = new SolidColorBrush(Colors.Yellow);
            //CatPanel.Margin = new Thickness(8, 0, 0, 0); 



            CatBorder.Child = CatPanel;




            Border HeaderBorder = new();
            //HeaderBorder.BorderThickness = new Thickness(1, 1, 1, 1);
            //HeaderBorder.BorderBrush = Brushes.Black;
            HeaderBorder.BorderThickness = new Thickness(0, 2, 0, 2);
            //HeaderBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            HeaderBorder.BorderBrush = Brushes.Black;
            HeaderBorder.Margin = new Thickness(-10, -2, 0, 0);
            //HeaderBorder.Visibility = Visibility.Collapsed;

            DockPanel.SetDock(HeaderBorder, Dock.Top);
            CatPanel.Children.Add(HeaderBorder);

            DockPanel Header = new();
            //Header.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121216")); //Brushes.Transparent;
            DockPanel.SetDock(Header, Dock.Top);
            HeaderBorder.Child = Header;
            //RowPanel.Children.Add(Header);

            Grid LabelGrid = new();
            //LabelGrid.Background = Brushes.Transparent;
            CatClass.TooltipGrid = LabelGrid;
            if (CatClass.Tooltip != "") { LabelGrid.ToolTip = CatClass.Tooltip; }
            ToolTipService.SetInitialShowDelay(LabelGrid, LibraryGES.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(LabelGrid, LibraryGES.TooltipBetweenDelay);
            Header.Children.Add(LabelGrid);

            Label CatLabel = new();
            CatClass.CategoryLabel = CatLabel;
            //CatLabel.FontSize = 30;
            CatLabel.Content = CatClass.CategoryName;// "Entry X";    //"Row X";            
            DockPanel.SetDock(CatLabel, Dock.Left);
            CatLabel.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            //Label.HorizontalAlignment = HorizontalAlignment.Left; //Left Right
            CatLabel.Margin = new Thickness(14, 0, 0, 0); // Left Top Right Bottom 
            LabelGrid.Children.Add(CatLabel);

            Border CatUnderLine = new();
            CatClass.CategoryUnderline = CatUnderLine; //This is the line under the label, to make it look like a header.
            CatUnderLine.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
            CatUnderLine.Margin = new Thickness(18, 0, 4, 4);
            CatUnderLine.BorderThickness = new Thickness(0, 0, 0, 2);
            LabelGrid.Children.Add(CatUnderLine);
            if (CatClass.Tooltip == "") { CatUnderLine.Visibility = Visibility.Collapsed; }

            //RowProperties
            LabelGrid.MouseLeftButtonDown += RowGrid_MouseLeftButtonDown;
            //CatLabel.MouseLeftButtonDown += RowGrid_MouseLeftButtonDown;
            //RowPanel.MouseRightButtonDown += RowGrid_MouseLeftButtonDown; //Bandaid solution to make sure move row up/down and delete are targeting the correct row.
            void RowGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {                
                DTEData.EditorRightBar.CategoryTabItem.IsSelected = true; //Select the category tab in the right bar.

                DTEData.CategoryClass = CatClass;

                DTEData.EditorRightBar.PropertiesRowNameBox.Text = CatClass.CategoryName;
                DTEData.EditorRightBar.PropertiesRowTooltipBox.Text = CatClass.Tooltip;
                DTEData.EditorRightBar.DTEData.CategoryClass = CatClass;

                foreach (TabItem tabItem in DTEData.EditorRightBar.TabTest.Items)
                {
                    if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                    {
                        tabItem.IsSelected = true;
                        break;
                    }
                }

                { //Update the UI rows and columns numbers.
                    int columnCount = 0;
                    int rowCount = 0;

                    foreach (var item in CatClass.GridItems)
                    {
                        int itemRightEdge = item.Column + (item.ColumnSpan > 0 ? item.ColumnSpan : 1);
                        int itemBottomEdge = item.Row + (item.RowSpan > 0 ? item.RowSpan : 1);

                        columnCount = Math.Max(columnCount, itemRightEdge);
                        rowCount = Math.Max(rowCount, itemBottomEdge);
                    }

                    DTEData.EditorRightBar.DebugCatTextboxColumns.Text = columnCount.ToString();
                    DTEData.EditorRightBar.DebugCatTextboxRows.Text = rowCount.ToString();
                }
                

                
            }



            Button Button = new Button();
            Button.Height = 26;
            Button.Width = 100;
            Button.Margin = new Thickness(4);
            Button.Content = "Hide Row";
            DockPanel.SetDock(Button, Dock.Right);
            //Button.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            Button.HorizontalAlignment = HorizontalAlignment.Right;
            Button.Click += delegate //When clicking this button, hide the previous page, and show the selected page.
            {
                bool Hide = true;
                if (Button.Content == "Hide Row") { Hide = true; }
                if (Button.Content == "Show Row") { Hide = false; }

                if (Hide == true)
                {
                    CatClass.ItemGrid.Visibility = Visibility.Collapsed;
                    Button.Content = "Show Row";
                }

                if (Hide == false)
                {
                    CatClass.ItemGrid.Visibility = Visibility.Visible;
                    Button.Content = "Hide Row";
                }


            };
            Header.Children.Add(Button);







            ContextMenu ContextMenu = new ContextMenu(); // THE RIGHT CLICK MENU

            MenuItem MenuItemNewRowAbove = new MenuItem();
            MenuItemNewRowAbove.Header = "  Create New Category (Above)  ";
            ContextMenu.Items.Add(MenuItemNewRowAbove);
            MenuItemNewRowAbove.Click += new RoutedEventHandler(CreateNewRowAbove);
            void CreateNewRowAbove(object sender, RoutedEventArgs e)
            {
                TheWorkshop.CreateNewRowAbove(CatClass);
            }

            MenuItem MenuItemNewRowBelow = new MenuItem();
            MenuItemNewRowBelow.Header = "  Create New Category (Below)  ";
            ContextMenu.Items.Add(MenuItemNewRowBelow);
            MenuItemNewRowBelow.Click += new RoutedEventHandler(CreateNewRowBelow);
            void CreateNewRowBelow(object sender, RoutedEventArgs e)
            {
                TheWorkshop.CreateNewRowBelow(CatClass);
            }

            MenuItem MenuItemMoveRowUp = new MenuItem();
            MenuItemMoveRowUp.Header = "  Move Row Up  ";
            ContextMenu.Items.Add(MenuItemMoveRowUp);
            MenuItemMoveRowUp.Click += new RoutedEventHandler(MoveRowUp);
            void MoveRowUp(object sender, RoutedEventArgs e)
            {
                TheWorkshop.MoveRowUp(CatClass);
            }

            MenuItem MenuItemMoveRowDown = new MenuItem();
            MenuItemMoveRowDown.Header = "  Move Row Down  ";
            ContextMenu.Items.Add(MenuItemMoveRowDown);
            MenuItemMoveRowDown.Click += new RoutedEventHandler(MoveRowDown);
            void MoveRowDown(object sender, RoutedEventArgs e)
            {
                TheWorkshop.MoveRowDown(CatClass);
            }

            MenuItem MenuItemDeleteRow = new MenuItem();
            MenuItemDeleteRow.Header = "  Delete Row  ";
            ContextMenu.Items.Add(MenuItemDeleteRow);
            MenuItemDeleteRow.Click += new RoutedEventHandler(DeleteRow);
            void DeleteRow(object sender, RoutedEventArgs e)
            {
                TheWorkshop.RowDelete(CatClass);
            }

            Header.ContextMenu = ContextMenu;

            //THE NEW ITEM GRID!!!
            Grid itemGrid = new();
            CatClass.ItemGrid = itemGrid;
            CatPanel.Children.Add(itemGrid);
            DockPanel.SetDock(itemGrid, Dock.Top);
            itemGrid.MinHeight = 100;
            itemGrid.MinWidth = 100;
            itemGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121216")); //Brushes.DarkGreen;
                                                                                                          //itemGrid.Margin = new Thickness(8, 0, 0, 0);



            //CatClass.grid.Children.Add(;);
            //RowPanel.Margin = new Thickness(0, 200, 0, 0);


            //CatClass.ItemGrid.Drop += Grid_Drop;
            //CatClass.ItemGrid.DragOver += OnDragOver;
            CatClass.ItemGrid.AllowDrop = true; // Don't forget this!
            CatClass.ItemGrid.Drop += Grid_Drop;
            void Grid_Drop(object sender, DragEventArgs e)
            {
                DTEMethods.DropGridItemsToBottomOfColumn(e, CatClass.ItemGrid, CatClass.GridItems, DTEData, CatClass, null);                
            }
            DTEData.MainDockPanel.LastChildFill = true; //Does nothing, i cant find out why last CatBorder won't fill... 
        }



        public void CreateGroup(Group GroupClass)
        {
            Border GroupBorder = new();
            GroupClass.GroupBorder = GroupBorder;
            GroupBorder.Margin = new Thickness(5, 5, 0, -3); //5 5 0 -1
            GroupBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#352050")); //403069 //303030 //Brushes.LightBlue; //464646
            GroupBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1D23"));//Brushes.MediumPurple; //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1A20")); //Brushes.Transparent; //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141114"));  //Brushes.DarkBlue; //201A20
            //GroupBorder.BorderBrush = Brushes.Transparent; 
            DockPanel.SetDock(GroupBorder, Dock.Top);


            DockPanel GroupPanel = new();
            GroupClass.GroupPanel = GroupPanel;
            GroupBorder.Child = GroupPanel;
            GroupPanel.Background = Brushes.Transparent;
            GroupPanel.LastChildFill = false;
            DockPanel.SetDock(GroupPanel, Dock.Top);

            DockPanel GroupHeader = new DockPanel();
            GroupHeader.Margin = new Thickness(0, 0, 0, 0);
            GroupHeader.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0d0326"));  //Brushes.DarkBlue;
            DockPanel.SetDock(GroupHeader, Dock.Top);
            GroupClass.GroupPanel.Children.Add(GroupHeader);
            GroupHeader.LastChildFill = false;
            //GroupHeader.MaxHeight = 25;
            //GroupHeader.Height = 25;


            
            {
                GroupClass.Visual = GroupClass.GroupBorder;

                Grid itemGrid = new();
                GroupClass.ItemGrid = itemGrid;
                GroupClass.GroupPanel.Children.Add(itemGrid); //add the new green grid panel to the group panel.
                DockPanel.SetDock(itemGrid, Dock.Top);
                itemGrid.MinHeight = 20;
                itemGrid.MinWidth = 20;
                itemGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1323")); //Brushes.MediumPurple;

                GroupClass.ParentCategory.ItemGrid.Children.Add(GroupClass.GroupBorder); //maybe remove this?

                itemGrid.Margin = new Thickness(0, 0, 0, -3);
            }











            Grid GroupHeaderGrid = new();
            GroupClass.TooltipGrid = GroupHeaderGrid;
            GroupHeaderGrid.Background = Brushes.Transparent;
            GroupHeader.Children.Add(GroupHeaderGrid);
            if (GroupClass.GroupTooltip != "") { GroupHeaderGrid.ToolTip = GroupClass.GroupTooltip; }
            ToolTipService.SetInitialShowDelay(GroupHeaderGrid, LibraryGES.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(GroupHeaderGrid, LibraryGES.TooltipBetweenDelay);
            GroupHeaderGrid.MouseLeftButtonDown += Group_Click;


            Label GroupLabel = new();
            GroupClass.GroupLabel = GroupLabel;
            GroupHeaderGrid.Children.Add(GroupLabel);
            GroupLabel.Content = GroupClass.GroupName;
            GroupLabel.VerticalAlignment = VerticalAlignment.Top;
            GroupLabel.Margin = new Thickness(4, 0, 0, 0);            
            DockPanel.SetDock(GroupLabel, Dock.Left);


            Border GroupUnderline = new();
            GroupClass.GroupUnderline = GroupUnderline;
            GroupUnderline.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0")); //Brushes.LightBlue; //464646
            GroupUnderline.Margin = new Thickness(8, 0, 4, 4);
            GroupUnderline.BorderThickness = new Thickness(0, 0, 0, 2);
            GroupHeaderGrid.Children.Add(GroupUnderline);
            //DockPanel.SetDock(GroupUnderline, Dock.Top);            
            //GroupUnderline.Width = GroupLabel.Width;
            if (GroupClass.GroupTooltip == "") { GroupUnderline.Visibility = Visibility.Collapsed; }





            //EntryDockPanel.MouseLeftButtonDown += EntryDockPanel_MouseLeftButtonDown;
            //EntryDockPanel.MouseMove += EntryBorder_MouseMove;


            //DRAG DROP CODE STARTS HERE FOR GROUPS
            GroupLabel.MouseLeftButtonDown += Group_Click;
            GroupHeader.MouseLeftButtonDown += Group_Click;
            GroupLabel.MouseMove += Group_MouseMove;
            GroupHeader.MouseMove += Group_MouseMove;            
            void Group_Click(object sender, MouseButtonEventArgs e)
            {

                DTRightBar RightBar = GroupClass.ParentEditor.DataTableEditorData.EditorRightBar;
                RightBar.DTEData.GroupClass = GroupClass; //Set the Selected Group

                RightBar.GroupTabItem.IsSelected = true;                              
                RightBar.PropertiesGroupNameBox.Text = GroupClass.GroupName;
                RightBar.PropertiesGroupTooltipBox.Text = GroupClass.GroupTooltip;
                RightBar.PropertiesGroupColumnSpan.Text = GroupClass.ColumnSpan.ToString();
                RightBar.PropertiesGroupRowSpan.Text = GroupClass.RowSpan.ToString();

                e.Handled = true; // Prevents the event from bubbling up to the DockPanel, which would cause the entry to be selected instead of the group.

            }
            void Group_MouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DTEMethods.BeginDrag(
                        GroupBorder, //EntryBorder,
                        GroupClass,
                        GroupClass.ParentEditor.DataTableEditorData.DTEXaml.TheScrollviewer);
                }
            }
            

            GroupHeader.AllowDrop = true;
            GroupHeader.Drop += Group_Drop;
            GroupHeader.DragOver += Group_DragOver;
            void Group_Drop(object sender, DragEventArgs e)
            {                
                DTEMethods.DropOntoItem(e, GroupClass);
            }
            void Group_DragOver(object sender, DragEventArgs e)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }





            ContextMenu contextMenu = new ContextMenu(); // THE RIGHT CLICK MENU
            GroupHeader.ContextMenu = contextMenu;

            MenuItem MoveToNewLeftColumn = new MenuItem();
            MoveToNewLeftColumn.Header = "  Move into New Column  (Left)";
            contextMenu.Items.Add(MoveToNewLeftColumn);
            MoveToNewLeftColumn.Click += new RoutedEventHandler(MoveToANewLeftColumn);
            void MoveToANewLeftColumn(object sender, RoutedEventArgs e)
            {
                var item = GroupClass;
                Grid grid = GroupClass.ParentCategory.ItemGrid;
                List<GridItem> griditems = GroupClass.ParentCategory.GridItems;


                int newColumnIndex = item.Column + 0;
                // Step 1 — Update coordinates
                MoveItemToNewColumn(item, griditems, newColumnIndex);
                // Step 2 — Rebuild the grid
                DTEMethods.UpdateEditorGrids(GroupClass.ParentEditor.DataTableEditorData);
            }

            MenuItem MoveToNewRightColumn = new MenuItem();
            MoveToNewRightColumn.Header = "  Move into New Column  (Right)";
            contextMenu.Items.Add(MoveToNewRightColumn);
            MoveToNewRightColumn.Click += new RoutedEventHandler(MoveToANewRightColumn);
            void MoveToANewRightColumn(object sender, RoutedEventArgs e)
            {
                var item = GroupClass;
                Grid grid = GroupClass.ParentCategory.ItemGrid;
                List<GridItem> griditems = GroupClass.ParentCategory.GridItems;
                

                int newColumnIndex = item.Column + 1;
                // Step 1 — Update coordinates
                MoveItemToNewColumn(item, griditems, newColumnIndex);
                // Step 2 — Rebuild the grid
                DTEMethods.UpdateEditorGrids(GroupClass.ParentEditor.DataTableEditorData);
            }


            void MoveItemToNewColumn(GridItem item, List<GridItem> griditems, int insertColumn)
            {
                // Shift everything to the right
                foreach (var other in griditems)
                {
                    if (other == item) continue;

                    if (other.Column >= insertColumn)
                        other.Column++;
                }

                // Move this item
                item.Column = insertColumn;
                item.Row = 0;
            }


            GroupClass.ItemGrid.AllowDrop = true; // Don't forget this!
            GroupClass.ItemGrid.Drop += Grid_Drop;
            void Grid_Drop(object sender, DragEventArgs e)
            {
                DTEMethods.DropGridItemsToBottomOfColumn(e, GroupClass.ItemGrid, GroupClass.GridItems, GroupClass.ParentEditor.DataTableEditorData, GroupClass.ParentCategory, GroupClass);
            }

        }





        public void CreateEntry(Entry EntryClass)
        {
            //An entry is the main attraction of the program. It is the numerical value of a hex / cell in a file.
            //It can do all kinds of things, be displayed all kinds of ways, and is extremely flexable.
            //For more information, the file "EntryManager" and it's methods go over a lot about what a entry can do.




            Editor EditorClass = EntryClass.ParentEditor;
            //Category CatClass = EntryClass.ParentCategory;
            Workshop TheWorkshop = EntryClass.ParentEditor.WorkshopXaml;
            WorkshopData Database = EntryClass.ParentEditor.WorkshopXaml.WorkshopData;


            Border EntryBorder = new();
            EntryClass.EntryBorder = EntryBorder;
            EntryBorder.BorderThickness = new Thickness(2);
            EntryBorder.CornerRadius = new CornerRadius(3);
            DockPanel.SetDock(EntryBorder, Dock.Top);
            EntryBorder.Margin = new Thickness(5, 5, 0, -1);// Left Top Right Bottom 
            EntryBorder.MinHeight = 38; //Height of a entry, so it doesn't shrink too small when there are no entrys in it.
            //EntryBorder.MaxHeight = 38;

            if (LibraryGES.ShowHiddenEntrys == false && (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true))
            {
                EntryBorder.Visibility = Visibility.Collapsed;
            }

            {
                EntryClass.Visual = EntryClass.EntryBorder;

                if (EntryClass.IsMerged == false)
                {
                    if (EntryClass.ParentGroup == null) // If not in group.
                    {
                        EntryClass.ParentCategory.ItemGrid.Children.Add(EntryClass.EntryBorder);
                    }
                    if (EntryClass.ParentGroup != null) //If in a group.
                    {
                        EntryClass.ParentGroup.ItemGrid.Children.Add(EntryClass.EntryBorder);
                    }
                }

            }


            DockPanel EntryDockPanel = new();
            //EntryDockPanel.Style = (Style)Application.Current.Resources["EntryStyle"];
            EntryDockPanel.Background = Brushes.Transparent;
            //EntryDockPanel.MinWidth = 145;
            EntryDockPanel.MinHeight = 26;    //Any smalled and NumberBoxes "shake" when you move the cursor with the arrow keys.        
            DockPanel.SetDock(EntryDockPanel, Dock.Top);
            //EntryDockPanel.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            //EntryDockPanel.HorizontalAlignment = HorizontalAlignment.Left; //Left Right            
            //EntryDockPanel.Margin = new Thickness(3, 0, 4, 3); // Left Top Right Bottom 

            Label SymbolLabel = new();
            SymbolLabel.Margin = new Thickness(-13, 0, 0, 0); // Left Top Right Bottom
            EntryClass.Symbology = SymbolLabel;
            EntryDockPanel.Children.Add(SymbolLabel);
            DockPanel.SetDock(SymbolLabel, Dock.Left);
            SymbolLabel.VerticalContentAlignment = VerticalAlignment.Center;





            ContextMenu contextMenu = new ContextMenu(); // THE RIGHT CLICK MENU
            EntryDockPanel.ContextMenu = contextMenu;

            MenuItem EntryCreateNewGroup = new MenuItem();
            EntryClass.EntryCreateNewGroup = EntryCreateNewGroup;
            EntryCreateNewGroup.Header = "  Create new group ";
            contextMenu.Items.Add(EntryCreateNewGroup);
            EntryCreateNewGroup.Click += new RoutedEventHandler(NewColumnGroup);
            void NewColumnGroup(object sender, RoutedEventArgs e)
            {
                if (EntryClass.ParentGroup != null)
                {
                    return;
                }


                Group NewGroup = new();
                NewGroup.ParentEditor = EntryClass.ParentEditor;
                NewGroup.ParentCategory = EntryClass.ParentCategory;
                NewGroup.ParentGrid = EntryClass.ParentGrid;
                NewGroup.ParentGridItems = EntryClass.ParentGridItems;
                NewGroup.ParentGridItems.Add(NewGroup);
                NewGroup.Column = EntryClass.Column;
                NewGroup.Row = EntryClass.Row;
                NewGroup.RowSpan = 1 + EntryClass.RowSpan;

                CreateGroup(NewGroup);

                EntryClass.ParentCategory.GridItems.Remove(EntryClass);
                EntryClass.ParentGrid.Children.Remove(EntryClass.Visual);
                var OldParentGrid = EntryClass.ParentGrid;
                var OldParentGridItems = EntryClass.ParentGridItems;
                

                EntryClass.ParentGroup = NewGroup;
                EntryClass.ParentGrid = NewGroup.ItemGrid;
                EntryClass.ParentGridItems = NewGroup.GridItems;
                EntryClass.ParentGroup.GridItems.Add(EntryClass);
                //EntryClass.ParentGrid.Children.Add(EntryClass.Visual);
                EntryClass.Row = 1;
                EntryClass.Column = 1;

                DTEMethods.UpdateEditorGrids(EntryClass.ParentEditor.DataTableEditorData);


            }
                        




            MenuItem MoveToNewLeftColumn = new MenuItem();
            MoveToNewLeftColumn.Header = "  Move into New Column  (Left)";
            contextMenu.Items.Add(MoveToNewLeftColumn);
            MoveToNewLeftColumn.Click += new RoutedEventHandler(MoveToANewLeftColumn);
            void MoveToANewLeftColumn(object sender, RoutedEventArgs e)
            {
                var item = EntryClass;
                Grid grid = item.ParentCategory.ItemGrid;
                List<GridItem> griditems = item.ParentCategory.GridItems;

                if (item.ParentGroup != null)
                {
                    grid = item.ParentGroup.ItemGrid;
                    griditems = item.ParentGroup.GridItems;
                }

                int newColumnIndex = item.Column + 0;
                // Step 1 — Update coordinates
                MoveItemToNewColumn(item, griditems, newColumnIndex);
                // Step 2 — Rebuild the grid
                DTEMethods.UpdateEditorGrids(EntryClass.ParentEditor.DataTableEditorData);
            }

            MenuItem MoveToNewRightColumn = new MenuItem();
            MoveToNewRightColumn.Header = "  Move into New Column  (Right)";
            contextMenu.Items.Add(MoveToNewRightColumn);
            MoveToNewRightColumn.Click += new RoutedEventHandler(MoveToANewRightColumn);
            void MoveToANewRightColumn(object sender, RoutedEventArgs e)
            {
                var item = EntryClass;
                Grid grid = item.ParentCategory.ItemGrid;
                List<GridItem> griditems = item.ParentCategory.GridItems;

                if (item.ParentGroup != null)
                {
                    grid = item.ParentGroup.ItemGrid;
                    griditems = item.ParentGroup.GridItems;
                }

                int newColumnIndex = item.Column + 1;
                // Step 1 — Update coordinates
                MoveItemToNewColumn(item, griditems, newColumnIndex);
                // Step 2 — Rebuild the grid
                DTEMethods.UpdateEditorGrids(EntryClass.ParentEditor.DataTableEditorData);
            }

            void MoveItemToNewColumn(GridItem item, List<GridItem> griditems, int insertColumn)
            {
                // Shift everything to the right
                foreach (var other in griditems)
                {
                    if (other == item) continue;

                    if (other.Column >= insertColumn)
                        other.Column++;
                }

                // Move this item
                item.Column = insertColumn;
                item.Row = 0;                
            }

            MenuItem MoveToNewAboveCategory = new MenuItem();
            MoveToNewAboveCategory.Header = "  Move into New Category  (Above)";
            contextMenu.Items.Add(MoveToNewAboveCategory);
            MoveToNewAboveCategory.Click += new RoutedEventHandler(MoveToANewAboveCategory);
            void MoveToANewAboveCategory(object sender, RoutedEventArgs e)
            {
                MoveItemToNewCategory(EntryClass, insertAbove: true);
            }


            MenuItem MoveToNewBelowCategory = new MenuItem();
            MoveToNewBelowCategory.Header = "  Move into New Category  (Below)";
            contextMenu.Items.Add(MoveToNewBelowCategory);
            MoveToNewBelowCategory.Click += new RoutedEventHandler(MoveToANewBelowCategory);
            void MoveToANewBelowCategory(object sender, RoutedEventArgs e)
            {
                MoveItemToNewCategory(EntryClass, insertAbove: false);
            }

            void MoveItemToNewCategory(GridItem item, bool insertAbove)
            {
                var editorData = EntryClass.ParentEditor.DataTableEditorData;
                var categories = editorData.CategoryList;

                Category oldCategory = item.ParentCategory;
                int oldIndex = categories.IndexOf(oldCategory);
                if (oldIndex < 0) return;

                // 1. Create new category
                Category newCategory = new();
                newCategory.DTEData = editorData;

                // 2. Insert category above or below
                int newIndex = insertAbove ? oldIndex : oldIndex + 1;
                categories.Insert(newIndex, newCategory);

                // 3. Remove item from old parent
                item.ParentGridItems?.Remove(item);

                // 4. Reparent item
                item.ParentCategory = newCategory;
                item.ParentGroup = null;
                item.ParentGridItems = newCategory.GridItems;
                item.ParentGrid = newCategory.ItemGrid;

                item.Column = 0;
                item.Row = 0;

                // 5. Add item to new category
                newCategory.GridItems.Add(item);

                CreateCategory(newCategory);

                // 6. Rebuild visuals
                DTEMethods.UpdateEditorGrids(editorData);
            }







            ////////DRAG DROP CODE 
            EntryClass.Visual.MouseLeftButtonDown += Entry_Click;
            EntryClass.Visual.MouseMove += Entry_MouseMove;

            //EntryClass.UnderlineBorder.MouseMove += Entry_MouseMove;
            //EntryClass.EntryNameTextBlock.MouseMove += Entry_MouseMove;
            void Entry_Click(object sender, MouseButtonEventArgs e)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {

                }
                else
                {
                    DTEMethods.EntryActivate(EntryClass);
                    e.Handled = true; // 🛑 Prevent the entry's parent from stealing the click event.
                }
            }
            void Entry_MouseMove(object sender, MouseEventArgs e)
            {
                //if (e.OriginalSource is TextBox)
                //{
                //    return; // If it's a TextBox, return and don't set the flag
                //}
                //e.Handled = true; // 🛑 Prevent the entry's parent from stealing the mouse move event, which would cause problems with dragging.
                //return;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (e.OriginalSource is DependencyObject d &&
                        (d is TextBox || d is ComboBox || d is Button || d is ToggleButton)) { e.Handled = true; return; }

                    //||  d is Border //Was causing problems with entrys that have tooltips. I could not drag from the name aka the tooltip BORDER.
                    //but NOT having it blocks combo box (drop downs entrys) from opening. So instead i do this...
                    if (e.OriginalSource is DependencyObject b && (b is Border))
                    {
                        if (b != EntryClass.UnderlineBorder) { e.Handled = true; return; }
                    }

                    DTEMethods.BeginDrag(
                        EntryBorder,
                        EntryClass,
                        EntryClass.ParentEditor.DataTableEditorData.DTEXaml.TheScrollviewer);
                }
            }


            EntryDockPanel.AllowDrop = true;
            EntryDockPanel.Drop += OnDrop;
            EntryDockPanel.DragOver += OnDragOver;
            void OnDrop(object sender, DragEventArgs e)
            {
                //IF any GridItems being dragged include a group,
                //and destination is in a group, then cancel!
                if (e.Data.GetDataPresent(typeof(List<GridItem>)))
                {
                    var items = e.Data.GetData(typeof(List<GridItem>)) as List<GridItem>;

                    if (EntryClass.ParentGroup != null) //If this is in a group, and data present is a group, cancel!
                    {
                        foreach (GridItem theitem in items)
                        {
                            if (theitem is Group)
                            {
                                return;
                            }

                        }

                        foreach (GridItem theitem in items)
                        {
                            if (theitem is Group)
                            {
                                return;
                            }

                        }
                    }
                    
                }

                DTEMethods.DropOntoItem(e, EntryClass);  //this updates grid layouts as well.              
                
            }
            void OnDragOver(object sender, DragEventArgs e)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true; // 🛑 Prevent the item's parent from stealing the drop.
            }
                        

            

            


            EntryBorder.Child = EntryDockPanel;
            EntryClass.EntryDockPanel = EntryDockPanel;
            

            Label PrefixEID = new Label(); //Entry ID Prefix. 
            //Prefix.Height = 30;
            //Prefix.MinWidth = 15;
            
            PrefixEID.Width = 58;
            PrefixEID.Margin = new Thickness(0,0,-23,0);
            PrefixEID.FontSize = 20;
            PrefixEID.Content = EntryClass.RowOffset;  //"P-x";//EntryClass.EntryName;// "Entry X";
            PrefixEID.Foreground = (Brush)new BrushConverter().ConvertFrom("#20A098");
            PrefixEID.HorizontalAlignment = HorizontalAlignment.Left;
            PrefixEID.VerticalContentAlignment = VerticalAlignment.Center;
            //Prefix.Margin = new Thickness(0, 0, 0, 0); // Left Top Right Bottom 
            PrefixEID.Visibility = Visibility.Collapsed;
            if (LibraryGES.ShowEntryAddress == false) { PrefixEID.Visibility = Visibility.Collapsed; }
            if (LibraryGES.ShowEntryAddress == true) { PrefixEID.Visibility = Visibility.Visible; }
            if (LibraryGES.EntryAddressType == "Decimal") { PrefixEID.Content = EntryClass.RowOffset; }
            //if (LibraryGES.EntryAddressType == "Hex") { PrefixEID.Content = (EntryClass.RowOffset + int.Parse(TheWorkshop.EntryAddressOffsetTextbox.Text)).ToString("X"); }
            EntryDockPanel.Children.Add(PrefixEID);
            EntryClass.EntryPrefix = PrefixEID;

            ///////////////////STARTING HERE IS STUFF FOR THE PAINFULLY OVER COMPLICATED SYSTEM WHERE AN ENTRY GETS UNDERLINED IF IT HAS A TOOLTIP.//////////////////////
            TextBlock EntryTextBlock = new();
            EntryClass.EntryNameTextBlock = EntryTextBlock;
            EntryTextBlock.Margin = new Thickness(5, 0, 1, 0);
            EntryTextBlock.FontSize = 20;
            EntryTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            EntryTextBlock.VerticalAlignment = VerticalAlignment.Center;

            Border UnderlineBorder = new();
            EntryClass.UnderlineBorder = UnderlineBorder;

            Grid NewPanel = new();
            EntryClass.EntryLeftGrid = NewPanel;
            NewPanel.Background = Brushes.Transparent;
            EntryDockPanel.Children.Add(NewPanel);
            NewPanel.Children.Add(EntryClass.EntryNameTextBlock);
            NewPanel.Children.Add(EntryClass.UnderlineBorder);  
            EntryClass.UnderlineBorder.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#A0A0A0");
            EntryClass.UnderlineBorder.Margin = new Thickness(4, 0, 0, 4); // Left Top Right Bottom
            EntryClass.UnderlineBorder.HorizontalAlignment = HorizontalAlignment.Left;
            EntryClass.EntryNameTextBlock.Inlines.Add(EntryClass.RunEntryName);
            ToolTipService.SetInitialShowDelay(EntryClass.EntryLeftGrid, LibraryGES.TooltipInitialDelay); 
            ToolTipService.SetBetweenShowDelay(EntryClass.EntryLeftGrid, LibraryGES.TooltipBetweenDelay);

            ////////////////END OF UNDERLINE SYSTEM//////////////////////

            DTEMethods.UpdateEntryName(EntryClass); //Handles name updates, Tooltip underlines, becoming hidden, and checking if text???



            //A entry can be one of 5 main types currently planned.
            //Note that dropdowns are not existing right now, but would be basically the same as a list.
            //If you play with lists, it's obvious how they are related.
            EntryManager EManage = new();
            if (EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox)
            {
                EManage.CreateNumberBox(TheWorkshop, EntryClass);
            }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox)
            {
                EManage.CreateCheckBox(TheWorkshop, EntryClass);
            }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.BitFlag)
            {
                EManage.CreateBitFlag(TheWorkshop, EntryClass);
            }
            if (EntryClass.NewSubType == Entry.EntrySubTypes.Menu)
            {
                EManage.CreateMenu(EntryClass, TheWorkshop);      
            }

            //EManage.EntryStyleUpdate(EntryClass); //applys style brushes to the current entry



            TheWorkshop.UpdateSymbology(EntryClass); //Set Symbology on Entry Creation.


            if (EntryClass.Bytes == 0)
            {
                EntryBorder.Visibility = Visibility.Collapsed;
            }

            

        }


    }




}
