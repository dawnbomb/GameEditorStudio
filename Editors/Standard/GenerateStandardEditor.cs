﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.ApplicationModel.Search;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using Path = System.IO.Path;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GameEditorStudio
{
    public class GenerateStandardEditor
    {
        ScrollViewer ScrollViewer = new(); //This is the scrollviewer that allows the user to scroll through the editor.

        bool isMiddleMouseDragging = false;
        Point mouseDragStartPoint;
        Point scrollStartOffset;


        private void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                isMiddleMouseDragging = true;
                mouseDragStartPoint = e.GetPosition(ScrollViewer);
                scrollStartOffset = new Point(ScrollViewer.HorizontalOffset, ScrollViewer.VerticalOffset);
                ScrollViewer.CaptureMouse();
                ScrollViewer.Cursor = Cursors.ScrollAll;
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                isMiddleMouseDragging = false;
                ScrollViewer.ReleaseMouseCapture();
                ScrollViewer.Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMiddleMouseDragging)
            {
                Point currentMousePosition = e.GetPosition(ScrollViewer);
                Vector delta = currentMousePosition - mouseDragStartPoint;

                ScrollViewer.ScrollToHorizontalOffset(scrollStartOffset.X - delta.X);
                ScrollViewer.ScrollToVerticalOffset(scrollStartOffset.Y - delta.Y);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;

            // Adjust scrolling speed here
            double scrollAmount = e.Delta > 0 ? -30 : 30;

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);

            // Mark event as handled so other controls don't block it
            e.Handled = true;
        }







        //////////////////////////////////////// Lazy put together move scrollviewer with mouse code.
        ////////////////////////////////////////
        //////////////////////////////////////// The actual Generation code for standard editors. 





        public void GenerateNormalEditor(Workshop TheWorkshop, WorkshopData Database, Editor EditorClass)
        {
            // HEY - In the future i want to instead have a xaml for a standard editor, and greatly reduce the amount of code.
            // I tried doing it myself, but for some reason i can't understand, the LeftBar.xaml won't load in properly. 
            // I will consider this post release. 

            //This triggers when the user makes a new editor in SetupNewEditor (not a loop)
            //or when the workshop is first launched via LoadEditorModeAuto (A loop) -> LoadTheDatabase.
            //Either way, this method is what actually creates an editor.
            //This method pulls information from the Database, and builds an editor based on that.
            //The database is loaded from files on system, or for a new editor, information the user input during editor creation.
            foreach (var category in EditorClass.StandardEditorData.CategoryList) //Select thr fir
            {
                foreach (var column in category.ColumnList)
                {
                    if (column.ItemBaseList != null && column.ItemBaseList.Count > 0)
                    {
                        foreach (ItemBase itembase in column.ItemBaseList)
                        {
                            if (itembase is Group group)
                            {
                                if (group.EntryList.Count != 0) 
                                {
                                    EditorClass.StandardEditorData.SelectedEntry = group.EntryList[0];
                                    break; // Exit as soon as one is found
                                }  
                            }

                            if (itembase is Entry entry)
                            {
                                EditorClass.StandardEditorData.SelectedEntry = entry;
                                break; // Exit as soon as one is found
                            }
                        }

                        
                        
                    }
                    if (EditorClass.StandardEditorData.SelectedEntry != null) { break; }
                }
                if (EditorClass.StandardEditorData.SelectedEntry != null) { break; }
            }

            EditorClass.Workshop = TheWorkshop; 

            CreateEditor(EditorClass, TheWorkshop, Database);  //Creates the main DockPanel of the editor. All Editor GUI stuff goes inside this Dockpanel.

            MakeButton MakeEditorButton = new();
            MakeEditorButton.CreateButton(TheWorkshop, Database, EditorClass); //Creates the button a user needs to click to make this editor appear.

            TheLeftBar MakeLeftBar = new(TheWorkshop, Database, EditorClass);//Creates the LeftBar of the editor, the part that has the item collection.
            EditorClass.StandardEditorData.EditorLeftDockPanel.UserControl = MakeLeftBar;
                        

            //This creates the entire core part of the editor.
            foreach (Category CatClass in EditorClass.StandardEditorData.CategoryList)
            {
                CreateCategory(CatClass);

                foreach (Column ColumnClass in CatClass.ColumnList/*.ToList()*/)
                {                   

                    CreateColumn(ColumnClass);

                    foreach (ItemBase itembase in ColumnClass.ItemBaseList) 
                    {
                        if (itembase is Group group) 
                        {
                            CreateGroup(group, -1);

                            foreach (Entry EntryClass in group.EntryList)
                            {
                                CreateEntry(EntryClass);
                            }
                        }

                        if (itembase is Entry entry)
                        {
                            CreateEntry(entry);
                        }
                    }

                    
                    StandardEditorMethods.LabelWidth(ColumnClass); //if entry, ok, if group, uhoh.

                }
            }

            //Finally, we select the first option of every tree. A few things happen during this.
            //A lot of Various information is updated when a item becomes the selected item, so it gets it's own method. 
            EntryManager EManager = new();
            EManager.EntryBecomeActive(EditorClass.StandardEditorData.SelectedEntry);  //EditorClass.StandardEditorData.CategoryList[0].ColumnList[0].EntryList[0]
            EManager.UpdateEntryProperties(EditorClass.StandardEditorData.SelectedEntry);

            StandardEditorMethods.DeleteEmptyColumnsAndMakeNewOnes(EditorClass.StandardEditorData);


        }



        public void CreateEditor(Editor EditorClass, Workshop TheWorkshop, WorkshopData Database)
        {
            //This grid is the very back of the entire editor. Everything else is a child of this grid, and those things are all GUI.
            DockPanel EditorDockPanel = new();
            EditorDockPanel.Background = Brushes.Purple; //If the user ever SEES this grid, it's a bug. So it gets a obvious ugly color.    /////////////////COLOR/////////////////////////////         
            TheWorkshop.MidGrid.Children.Add(EditorDockPanel);

            EditorClass.EditorBackPanel = new();
            EditorClass.EditorBackPanel = EditorDockPanel;



            Grid mainGrid = new Grid(); //A grid is needed to make a grid splitter work (annoyingly). Previously i just docked a left and right dockpanel directly into editordockpanel.
            ColumnDefinition leftColumn = new ColumnDefinition { Width = new GridLength(250) };
            ColumnDefinition splitterColumn = new ColumnDefinition { Width = GridLength.Auto };
            ColumnDefinition rightColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            mainGrid.ColumnDefinitions.Add(leftColumn);
            mainGrid.ColumnDefinitions.Add(splitterColumn);
            mainGrid.ColumnDefinitions.Add(rightColumn);
            EditorDockPanel.Children.Add(mainGrid);


            DockPanel LeftBarDockPanel = new DockPanel();
            Grid.SetColumn(LeftBarDockPanel, 0);
            mainGrid.Children.Add(LeftBarDockPanel);
            EditorClass.StandardEditorData.EditorLeftDockPanel.LeftBarDockPanel = LeftBarDockPanel;



            GridSplitter GridSplitter = new GridSplitter
            {
                Style = (Style)Application.Current.FindResource("SplitterVertical"),
                Width = 10,
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //Background = (SolidColorBrush)Application.Current.Resources["NewColorThing"], // Assuming NewColorThing is a SolidColorBrush
                //BorderThickness = new Thickness(1, 0, 2, 0),
                //BorderBrush = (SolidColorBrush)Application.Current.Resources["GridSplitterBorder"]
            };
            Grid.SetColumn(GridSplitter, 1);
            mainGrid.Children.Add(GridSplitter);



            DockPanel RightDock = new();
            //RightDock.Background = Brushes.Yellow; /////////////////////////////////////COLOR/////////////////////////////////////
            //RightDock.Background = Brushes.Black;
            //RightDock.Style = (Style)Application.Current.Resources["BorderDock"]; 
            RightDock.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            Grid.SetColumn(RightDock, 2);
            mainGrid.Children.Add(RightDock);

            

            ContentControl EditorBorder = new();
            DockPanel.SetDock(EditorBorder, Dock.Top);
            //EditorBorder.BorderBrush = Brushes.LightBlue;


            DockPanel EditorPanel = new();
            EditorBorder.Content = EditorPanel;

            {
                //Note when i removed this, at the time a editors category made space above it, and not it doesn't, so i'd maybe wanna rework the margins if i re-add this ever. 

                //DockPanel EditorHeader = new();
                //DockPanel.SetDock(EditorHeader, Dock.Top);
                //EditorPanel.Children.Add(EditorHeader);
                //EditorHeader.Style = EditorHeader.TryFindResource("HeaderDock") as Style;


                //Label EditorHeaderLabel = new();
                //EditorHeaderLabel.Content = "Editor";
                //EditorHeaderLabel.FontWeight = FontWeights.Bold;
                //EditorHeader.Children.Add(EditorHeaderLabel);




                //Button IconButton = new();
                //IconButton.Content = " Icon Editor ";
                //IconButton.Click += delegate
                //{
                //    TheWorkshop.HIDEMOST();
                //    UserControlEditorIcons TheUserControl = new UserControlEditorIcons();
                //    TheWorkshop.MidGrid.Children.Add(TheUserControl);
                //    TheWorkshop.UCGraphicsEditor = TheUserControl;
                //};
                //EditorHeader.Children.Add(IconButton);
                //IconButton.HorizontalAlignment = HorizontalAlignment.Right;
                //IconButton.Margin = new Thickness(4);
                //if (TheWorkshop.IsPreviewMode == true) { IconButton.IsEnabled = false; }

            }



            {
                ContentControl DescriptionsBorder = new();
                DockPanel.SetDock(DescriptionsBorder, Dock.Bottom);
                RightDock.Children.Add(DescriptionsBorder);
                DescriptionsBorder.Margin = new Thickness(0, 12, 0, 0);


                DockPanel DescriptionsPanel = new();
                DockPanel.SetDock(DescriptionsPanel, Dock.Bottom);
                //DescriptionsPanel.VerticalAlignment = VerticalAlignment.Top;
                DescriptionsBorder.Content = DescriptionsPanel;
                //DescriptionsPanel.Height = 198;


                EditorClass.StandardEditorData.EditorDescriptionsPanel = new();
                EditorClass.StandardEditorData.EditorDescriptionsPanel.TopPanel = DescriptionsPanel;

                DockPanel DescriptionsHeader = new();
                DockPanel.SetDock(DescriptionsHeader, Dock.Top);
                DescriptionsPanel.Children.Add(DescriptionsHeader);
                DescriptionsHeader.Style = DescriptionsHeader.TryFindResource("HeaderDock") as Style;
                DescriptionsHeader.Height = 34;
                DescriptionsHeader.VerticalAlignment = VerticalAlignment.Top;
                
                Button ButtonDescriptionManager = new();
                ButtonDescriptionManager.Height = 30;
                ButtonDescriptionManager.Content = "  Description Manager  ";
                ButtonDescriptionManager.Margin = new Thickness(4,2,4,2);
                DescriptionsHeader.Children.Add(ButtonDescriptionManager);
                DockPanel.SetDock(ButtonDescriptionManager, Dock.Right);
                ButtonDescriptionManager.Click += delegate //When clicking this button, hide the previous page, and show the selected page.
                {
                    TheWorkshop.HIDEMOST();
                    TextSourceManager NewDescriptionManager = new TextSourceManager();
                    TheWorkshop.MidGrid.Children.Add(NewDescriptionManager);
                    NewDescriptionManager.SetupForDescription();
                    //TheWorkshop.NEWUCExtraEditor = NewDescriptionManager;
                };
                if (TheWorkshop.IsPreviewMode == true) { ButtonDescriptionManager.IsEnabled = false; }



                Label HeaderLabel = new();
                HeaderLabel.Content = "Description";
                HeaderLabel.FontWeight = FontWeights.Bold;
                DescriptionsHeader.Children.Add(HeaderLabel);
            }









            //I cant find where the fuck the scrollviewer is being created, so i'll just do this here... 
            //ScrollViewer.BorderThickness = new Thickness(0);

            DockPanel.SetDock(ScrollViewer, Dock.Top);
            EditorPanel.Children.Add(ScrollViewer);
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            ScrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            ScrollViewer.PreviewMouseDown += ScrollViewer_PreviewMouseDown;
            ScrollViewer.PreviewMouseUp += ScrollViewer_PreviewMouseUp;
            ScrollViewer.PreviewMouseMove += ScrollViewer_PreviewMouseMove;



            DockPanel ScrollPanel = new();
            ScrollPanel.Style = (Style)Application.Current.Resources["BorderDock"];
            DockPanel.SetDock(ScrollPanel, Dock.Top);
            ScrollPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            ScrollPanel.VerticalAlignment = VerticalAlignment.Stretch;
            ScrollViewer.Content = ScrollPanel;
            EditorClass.StandardEditorData.MainDockPanel = ScrollPanel;
            //ScrollPanel.Background = Brushes.DarkRed;
            
            ScrollPanel.Background = Brushes.Black;
            //ScrollPanel.Background = Brushes.Green;
            ScrollPanel.LastChildFill = true;


            for (int i = 0; i < EditorClass.StandardEditorData.DescriptionTableList.Count; i++) //a foreach loop but using for explicitly so i can remove itself if it's invalid a little later here. 
            {
                var DescriptionTable = EditorClass.StandardEditorData.DescriptionTableList[i];

                if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.DataFile)
                {
                    if (DescriptionTable.Start == 0 || DescriptionTable.RowSize == 0 || DescriptionTable.TextSize == 0 || DescriptionTable.FileTextTable == null || DescriptionTable.FileTextTable.FileLocation == null)
                    {
                        // Remove and break
                        EditorClass.StandardEditorData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }
                if (DescriptionTable.LinkType == DescriptionTable.LinkTypes.TextFile) 
                {
                    if (DescriptionTable.FileTextTable == null || DescriptionTable.FileTextTable.FileLocation == null)
                    {
                        // Remove and break
                        EditorClass.StandardEditorData.DescriptionTableList.RemoveAt(i);
                        continue;
                    }
                }

                
                /////////////////////////// HEY - DESCRIPTION TEXT BOX CODE WAS MOVED TO THE LEFTBAR, AND KIND OF THE CHARACTER SET MANAGER. (I may move it again later)
                /////////////////////////// I now generate a new textbox every time the item in the item list changes, although, i may move it back here in the future.

                //TextBox DescriptionTextBox = new();
                //DescriptionTextBox.AcceptsReturn = true;
                //DescriptionTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                //DescriptionTextBox.TextWrapping = TextWrapping.NoWrap;
                //DescriptionTextBox.Margin = new Thickness(5);
                //DockPanel.SetDock(DescriptionTextBox, Dock.Top);
                //EditorClass.StandardEditorData.EditorDescriptionsPanel.TopPanel.Children.Add(DescriptionTextBox);
                //DescriptionTextBox.Height = 67;
                //DescriptionTable.ExtraTableTextBox = DescriptionTextBox;
                //DescriptionTable.ExtraTableEncodeIsEnabled = true;
                //DescriptionTextBox.VerticalAlignment = VerticalAlignment.Top;
                //if (TheWorkshop.IsPreviewMode == true) { DescriptionTextBox.IsEnabled = false; }

                //DescriptionTextBox.PreviewKeyDown += (sender, e) =>
                //{
                //    if (e.Key == Key.Enter)
                //    {
                //        e.Handled = true;

                //        if (DescriptionTable.TextSize == DescriptionTable.ExtraTableTextBox.Text.Length) { return; }

                //        TextBox textBox = sender as TextBox;

                //        int caretIndex = textBox.CaretIndex;
                //        textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                //        textBox.CaretIndex = caretIndex + 1;
                //    }
                //};

                //DescriptionTextBox.PreviewTextInput += (sender, e) =>
                //{
                //    string NewText = DescriptionTable.ExtraTableTextBox.Text + e.Text;

                //    Encoding encoding;
                //    if (DescriptionTable.CharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                //    else if (DescriptionTable.CharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                //    else { return; }
                //    int NewByteSize = encoding.GetByteCount(NewText);

                //    if (NewByteSize > DescriptionTable.TextSize)
                //    {
                //        e.Handled = true;  // Mark the event as handled so the input is ignored
                //    }
                //    //else { TheWorkshop.DebugBox2.Text = "Current NameBox Text Length\n" + (NameBox.Text.Length + 1).ToString(); }

                //};

                //DescriptionTextBox.TextChanged += (sender, e) =>
                //{
                //    TreeViewItem selectedItem = EditorClass.StandardEditorData.EditorLeftDockPanel.TreeView.SelectedItem as TreeViewItem;
                //    ItemInfo ItemInfo = selectedItem.Tag as ItemInfo;
                //    if (selectedItem == null || selectedItem.Tag == null || ItemInfo.IsFolder == true || DescriptionTable.ExtraTableEncodeIsEnabled == false)
                //    {
                //        return;
                //    }

                //    CharacterSetManager CharacterSetManager = new();
                //    CharacterSetManager.EncodeDescription(TheWorkshop, EditorClass, DescriptionTable);

                //};



            }
            RightDock.Children.Add(EditorBorder);



        }
        



        public void CreateCategory(Category CatClass)
        {
            StandardEditorData SWData = CatClass.SWData; //This is the StandardEditorData that contains all the information about the editor, such as the categories and columns.
            Workshop TheWorkshop = CatClass.SWData.TheEditor.Workshop; //This is the workshop that contains the editor, and is used to access the main grid and other workshop related information.
            WorkshopData Database = CatClass.SWData.TheEditor.Workshop.MyDatabase; //This is the database that contains all the information about the game, such as the entries and columns.

            if (SWData == null || TheWorkshop == null || Database == null) { LibraryMan.NotificationNegative("Critical Error!", "Create Category error, will crash soon. Report this D:"); }
                        
            int TheIndex = CatClass.SWData.CategoryList.IndexOf(CatClass);
            SWData.MainDockPanel.Children.Insert(TheIndex, CatClass.CatBorder);

            
            if (SWData.MainDockPanel.Children.Contains(CatClass.CatBorder)) 
            {
                string ts = "Category already exists in MainDockPanel, not creating a new one.";
            }

            //A category contains a number of columns.
            //This is useful for spread-sheet like editors needing to exist.
            //Such as etrian odyssey untold 2 have skills with 20 levels, and many attributes, all assigned per level.            

            Border CatBorder = CatClass.CatBorder;
            CatBorder.Style = (Style)Application.Current.Resources["RowBorder"];
            DockPanel.SetDock(CatBorder, Dock.Top);
            CatBorder.Margin = new Thickness(0, 0, 0, 2);
            CatBorder.BorderThickness = new Thickness(0);
            //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#464646"));

            CatBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#464646"));
            
            CatBorder.VerticalAlignment = VerticalAlignment.Stretch;
            CatBorder.HorizontalAlignment = HorizontalAlignment.Stretch;



            DockPanel RowPanel = CatClass.CategoryDockPanel;
            RowPanel.Style = (Style)Application.Current.Resources["RowStyle"];
            DockPanel.SetDock(RowPanel, Dock.Top);
            RowPanel.VerticalAlignment = VerticalAlignment.Stretch; //Top Bottom
            RowPanel.HorizontalAlignment = HorizontalAlignment.Stretch; //Left Right    
            RowPanel.Margin = new Thickness(8, 0, 0, 0); // Left Top Right Bottom 
            //RowPanel.Margin = new Thickness(18, 10, 18, 10); // Left Top Right Bottom 
            
            //if (Index == -1) { SWData.MainDockPanel.Children.Add(RowPanel); }
            //else { SWData.MainDockPanel.Children.Insert(Index, RowPanel); }
            CatBorder.Child = RowPanel;

            //if (Index == -1) { PageClass.DockPanel.Children.Add(RowPanel); }
            //else { RowClass.RowPage.DockPanel.Children.Insert(Index, RowPanel); }


            Border HeaderBorder = new();
            //HeaderBorder.BorderThickness = new Thickness(1, 1, 1, 1);
            //HeaderBorder.BorderBrush = Brushes.Black;
            HeaderBorder.BorderThickness = new Thickness(0, 0, 0, 2);
            HeaderBorder.BorderBrush = Brushes.Black;
            HeaderBorder.Margin = new Thickness(-10, 0, 0, 0);
            //HeaderBorder.Visibility = Visibility.Collapsed;

            DockPanel.SetDock(HeaderBorder, Dock.Top);
            RowPanel.Children.Add(HeaderBorder);

            DockPanel Header = new();
            DockPanel.SetDock(Header, Dock.Top);
            HeaderBorder.Child = Header;
            //RowPanel.Children.Add(Header);

            Grid LabelGrid = new();
            CatClass.TooltipGrid = LabelGrid;
            if (CatClass.Tooltip != "") { LabelGrid.ToolTip = CatClass.Tooltip; }
            ToolTipService.SetInitialShowDelay(LabelGrid, LibraryMan.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(LabelGrid, LibraryMan.TooltipBetweenDelay);
            Header.Children.Add(LabelGrid);

            Label CatLabel = new Label();
            CatLabel.Content = CatClass.CategoryName;// "Entry X";    //"Row X";            
            DockPanel.SetDock(CatLabel, Dock.Left);
            CatLabel.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            //Label.HorizontalAlignment = HorizontalAlignment.Left; //Left Right
            CatLabel.Margin = new Thickness(14, 0, 0, 0); // Left Top Right Bottom 
            CatClass.CategoryLabel = CatLabel;
            LabelGrid.Children.Add(CatLabel);

            Border CatUnderLine = CatClass.CategoryUnderline; //This is the line under the label, to make it look like a header.
            CatUnderLine.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
            CatUnderLine.Margin = new Thickness(18,0,4,4);
            CatUnderLine.BorderThickness = new Thickness(0,0,0,2);
            LabelGrid.Children.Add(CatUnderLine);
            if(CatClass.Tooltip == ""){ CatUnderLine.Visibility = Visibility.Collapsed; }

            //RowProperties
            LabelGrid.MouseLeftButtonDown += RowGrid_MouseLeftButtonDown;
            //CatLabel.MouseLeftButtonDown += RowGrid_MouseLeftButtonDown;
            //RowPanel.MouseRightButtonDown += RowGrid_MouseLeftButtonDown; //Bandaid solution to make sure move row up/down and delete are targeting the correct row.
            void RowGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                LibraryMan.GotoGeneralRow(TheWorkshop);
                                
                TheWorkshop.PropertiesRowNameBox.Text = CatClass.CategoryName;
                TheWorkshop.PropertiesRowTooltipBox.Text = CatClass.Tooltip;
                TheWorkshop.CategoryClass = CatClass;
                                
                foreach (TabItem tabItem in TheWorkshop.TabTest.Items)
                {
                    if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                    {
                        tabItem.IsSelected = true;
                        break;
                    }
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
                    //CatClass.CategoryDockPanel.Visibility = Visibility.Collapsed;
                    foreach (Column column in CatClass.ColumnList)
                    {
                        column.ColumnPanel.Visibility = Visibility.Collapsed;
                    }
                    Button.Content = "Show Row";
                }

                if (Hide == false)
                {
                    //CatClass.CategoryDockPanel.Visibility = Visibility.Visible;
                    foreach (Column column in CatClass.ColumnList)
                    {
                        column.ColumnPanel.Visibility = Visibility.Visible;
                    }
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

        }
                

        public void CreateColumn(Column ColumnClass)
        {            

            int TheIndex = ColumnClass.ColumnRow.ColumnList.IndexOf(ColumnClass) + 1;
            ColumnClass.ColumnRow.CategoryDockPanel.Children.Insert(TheIndex, ColumnClass.ColumnPanel);

            Workshop TheWorkshop = ColumnClass.ColumnRow.SWData.TheEditor.Workshop; 
            //, WorkshopData Database, int Index



            DockPanel ColumnGrid = ColumnClass.ColumnPanel;
            ColumnGrid.Style = (Style)Application.Current.Resources["ColumnStyle"];
            ColumnGrid.MinWidth = 30;
            //ColumnGrid.Width = 400;
            //ColumnGrid.Height = 200;
            ColumnGrid.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            ColumnGrid.VerticalAlignment = VerticalAlignment.Stretch; //Top Bottom //Needed to make entrys drop anywhere work properly.
            ColumnGrid.LastChildFill = false;
            //ColumnGrid.HorizontalAlignment = HorizontalAlignment.Left; //Left Right
            DockPanel.SetDock(ColumnGrid, Dock.Left);
            ColumnGrid.Margin = new Thickness(2, 10, 0, 5); // Left Top Right Bottom  //(0, 5, 0, 1)
            ColumnGrid.MinHeight = 50; //Minimum height of a column, so it doesn't shrink too small when there are no entrys in it.

            

            DockPanel Header = new(); //The top part of a column, where the label is, and you can right click this part, and only this part, for a context menu.
            Header.Style = (Style)Application.Current.Resources["ColumnStyle"];
            DockPanel.SetDock(Header, Dock.Top);
            ColumnGrid.Children.Add(Header);

            Header.Visibility = Visibility.Collapsed;
            ColumnGrid.Background = Brushes.Transparent;

            ContextMenu ContextMenu = new ContextMenu();

            MenuItem MenuItemNewColumnLeft = new MenuItem();
            MenuItemNewColumnLeft.Header = "  Create New Column  (Left)  ";
            ContextMenu.Items.Add(MenuItemNewColumnLeft);
            MenuItemNewColumnLeft.Click += new RoutedEventHandler(NewColumnLeft);
            void NewColumnLeft(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewColumnLeft(ColumnClass);
            }

            MenuItem MenuItemNewColumnRight = new MenuItem();
            MenuItemNewColumnRight.Header = "  Create New Column  (Right)  ";
            ContextMenu.Items.Add(MenuItemNewColumnRight);
            MenuItemNewColumnRight.Click += new RoutedEventHandler(NewColumnRight);
            void NewColumnRight(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewColumnRight(ColumnClass);
            }



            MenuItem MenuItemDeleteColumn = new MenuItem();
            MenuItemDeleteColumn.Header = "  Delete Column  ";
            ContextMenu.Items.Add(MenuItemDeleteColumn);
            MenuItemDeleteColumn.Click += new RoutedEventHandler(DeleteColumn_Click);
            void DeleteColumn_Click(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.ColumnDelete(ColumnClass);
            }




            Header.ContextMenu = ContextMenu;


            Label Label = new Label();
            Label.Height = 36;
            //Label.Width = 120;
            Label.Margin = new Thickness(2, -5, 0, 0); // Left Top Right Bottom 
            Label.Content = ColumnClass.ColumnName; //"This is Column X";
            DockPanel.SetDock(Label, Dock.Top);
            Label.HorizontalContentAlignment = HorizontalAlignment.Center;
            //ButtonAddRow.VerticalAlignment = VerticalAlignment.Top;
            //ButtonAddRow.HorizontalAlignment = HorizontalAlignment.Right;
            Header.Children.Add(Label);
            ColumnClass.ColumnLabel = Label;

            Label.MouseLeftButtonDown += ColumnGrid_MouseLeftButtonDown;
            void ColumnGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                ColumnActivate();

            }

            void ColumnActivate()
            {
                LibraryMan.GotoGeneralColumn(TheWorkshop);
                TheWorkshop.PropertiesColumnNameBox.Text = ColumnClass.ColumnName;
                //TheWorkshop.EntryClass = EntryClass;
                TheWorkshop.CategoryClass = ColumnClass.ColumnRow;
                TheWorkshop.ColumnClass = ColumnClass;
                
                foreach (TabItem tabItem in TheWorkshop.TabTest.Items)
                {
                    if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                    {
                        tabItem.IsSelected = true;
                        //TheWorkshop.DebugBox.Text = "Hai";
                        break;
                    }
                }
            }

            //ColumnGrid.Children.Add(Label);





            Header.MouseMove += ColumnDrag;
            void ColumnDrag(object sender, MouseEventArgs e)
            {
                if (TheWorkshop.IsPreviewMode == true) { return; }

                if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyUp(Key.LeftShift)) //Single Column capture
                {
                    //var TheDockPanel = (DockPanel)sender;
                    //var TheBorder = (Border)TheDockPanel.Parent;
                    var TheDockPanel = ColumnGrid;
                    var currentPosition = e.GetPosition(ColumnGrid);
                    var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                    if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                    {
                        var data = new DataObject("MoveColumnClass", ColumnClass);
                        DragDrop.DoDragDrop(ColumnGrid, data, DragDropEffects.Move);
                    }
                    TheDockPanel.ReleaseMouseCapture();


                }




            }

            


            //This is part of how entrys can move with the mouse. The other part is the MouseMove event in CreateEntry.

            
            Header.AllowDrop = true;
            Header.Drop += ColumnHeaderDrop;
            //This makes an entry drop up at the top of a column. 
            void ColumnHeaderDrop(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent("MoveColumnClass") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                {
                    Column InputColumn = (Column)e.Data.GetData("MoveColumnClass");

                    if (InputColumn != ColumnClass)
                    {
                        //Note to self: Add a check for if the Category the group is leaving, is now empty. If so, kill it.

                        InputColumn.ColumnRow.CategoryDockPanel.Children.Remove(InputColumn.ColumnPanel);
                        int FromIndex = InputColumn.ColumnRow.ColumnList.IndexOf(InputColumn);
                        int ToIndex = ColumnClass.ColumnRow.CategoryDockPanel.Children.IndexOf(ColumnClass.ColumnPanel);
                        InputColumn.ColumnRow.ColumnList.RemoveAt(FromIndex);

                        ColumnClass.ColumnRow.CategoryDockPanel.Children.Insert(ToIndex + 1, InputColumn.ColumnPanel);
                        ColumnClass.ColumnRow.ColumnList.Insert(ToIndex, InputColumn);

                        //InputColumn.ColumnRow = ColumnClass.ColumnRow; //DO NOT REFER TO COLUMN CLASS DIRECTLY, ALTHOUGH I DONT REMEMBER WHY.                    
                        InputColumn.ColumnRow = ColumnClass.ColumnRow;
                    }

                }

                if (e.Data.GetDataPresent("MoveEntryClass") && Keyboard.IsKeyUp(Key.LeftShift))
                {
                    Entry InputEntry = (Entry)e.Data.GetData("MoveEntryClass");

                    InputEntry.EntryColumn.ColumnPanel.Children.Remove(InputEntry.EntryBorder);
                    int FromIndex = InputEntry.EntryColumn.ItemBaseList.IndexOf(InputEntry);
                    InputEntry.EntryColumn.ItemBaseList.RemoveAt(FromIndex);

                    ColumnGrid.Children.Insert(1, InputEntry.EntryBorder);
                    ColumnClass.ItemBaseList.Insert(0, InputEntry);


                    InputEntry.EntryColumn = ColumnClass;
                    InputEntry.EntryRow = ColumnClass.ColumnRow;

                }
                else if (e.Data.GetDataPresent("MoveEntryClassGroup") && Keyboard.IsKeyDown(Key.LeftShift))
                {

                    for (int i = 0; i < TheWorkshop.EntryMoveList.Count; i++)
                    {

                        var InputEntry = TheWorkshop.EntryMoveList[i];

                        InputEntry.EntryColumn.ColumnPanel.Children.Remove(InputEntry.EntryBorder);
                        InputEntry.EntryColumn.ItemBaseList.RemoveAt(InputEntry.EntryColumn.ItemBaseList.IndexOf(InputEntry));

                        ColumnGrid.Children.Insert(1 + i, InputEntry.EntryBorder);
                        ColumnClass.ItemBaseList.Insert(0 + i, InputEntry);

                        InputEntry.EntryColumn = ColumnClass;
                        InputEntry.EntryRow = ColumnClass.ColumnRow;
                    }
                }

                e.Handled = true; // 🛑 Prevent the entry's parent from stealing the drop.


            }


            ColumnGrid.AllowDrop = true;
            ColumnGrid.Drop += ColumnBodyDrop;
            //This makes an entry drop down at the bottom of a column. 
            void ColumnBodyDrop(object sender, DragEventArgs e)
            {

                if (e.Data.GetDataPresent("EntryMoveList"))
                {

                    List<Entry> EntryMoveList = (List<Entry>)e.Data.GetData("EntryMoveList");


                    StandardEditorMethods.MoveEntrysToColumn(EntryMoveList, ColumnClass);
                }

                if (e.Data.GetDataPresent("GroupToMove")) //Entry Drop
                {
                    Group agroup = (Group)e.Data.GetData("GroupToMove");

                    StandardEditorMethods.MoveGroupToBottomOfColumn(agroup, ColumnClass);

                }

                e.Handled = true; // 🛑 Prevent the entry's parent from stealing the drop.


                StandardEditorMethods.EntryActivate(TheWorkshop.EditorClass.StandardEditorData.SelectedEntry); //this prevents a crash when immedietly merging the moved entry into a 2 byte.
            }

        }

        public void CreateGroup(Group GroupClass, int Index)
        {
            if (Index == -1) { GroupClass.GroupColumn.ColumnPanel.Children.Add(GroupClass.GroupBorder); }
            else { GroupClass.GroupColumn.ColumnPanel.Children.Insert(Index, GroupClass.GroupBorder); }


            Border GroupBorder = GroupClass.GroupBorder;
            GroupBorder.Margin = new Thickness(0, 1, 0, 2);
            GroupBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030")); //Brushes.LightBlue; //464646
            GroupBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#201A20")); //Brushes.Transparent; //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141114"));  //Brushes.DarkBlue;
            //GroupBorder.BorderBrush = Brushes.Transparent; 
            DockPanel.SetDock(GroupBorder, Dock.Top);    



            DockPanel GroupPanel = GroupClass.GroupPanel;
            GroupBorder.Child = GroupPanel;
            GroupPanel.Background = Brushes.Transparent; 
            GroupPanel.LastChildFill = false;
            DockPanel.SetDock(GroupPanel, Dock.Top);

            DockPanel GroupTopPanel = new DockPanel();
            GroupTopPanel.Margin = new Thickness(0,0,0,4);
            GroupTopPanel.Background = Brushes.Transparent;
            DockPanel.SetDock(GroupTopPanel, Dock.Top);
            GroupPanel.Children.Add(GroupTopPanel);
            GroupTopPanel.LastChildFill = false;

            Grid GroupTopGrid = GroupClass.TooltipGrid;
            GroupTopGrid.Background = Brushes.Transparent;
            GroupTopPanel.Children.Add(GroupTopGrid);
            if (GroupClass.GroupTooltip != "") { GroupTopGrid.ToolTip = GroupClass.GroupTooltip; }
            ToolTipService.SetInitialShowDelay(GroupTopGrid, LibraryMan.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(GroupTopGrid, LibraryMan.TooltipBetweenDelay);
            GroupTopGrid.MouseLeftButtonDown += Group_MouseLeftButtonDown;

            Label GroupLabel = GroupClass.GroupLabel;
            GroupTopGrid.Children.Add(GroupLabel);
            GroupLabel.Content = GroupClass.GroupName;
            GroupLabel.VerticalAlignment = VerticalAlignment.Top;
            GroupLabel.Margin = new Thickness(4, 0, 0, 0);            
            DockPanel.SetDock(GroupLabel, Dock.Left);
            GroupLabel.MouseLeftButtonDown += Group_MouseLeftButtonDown;
            void Group_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                Workshop TheWorkshop = GroupClass.GroupColumn.ColumnRow.SWData.TheEditor.Workshop; //Get the workshop from the group column, which is in the group class.
                TheWorkshop.GroupClass = GroupClass;
                TheWorkshop.GeneralGroup.IsSelected = true;
                TheWorkshop.PropertiesGroupNameBox.Text = GroupClass.GroupName;
                TheWorkshop.PropertiesGroupTooltipBox.Text = GroupClass.GroupTooltip;

                e.Handled = true; // Prevents the event from bubbling up to the DockPanel, which would cause the entry to be selected instead of the group.

            }

            Border GroupUnderline = GroupClass.GroupUnderline;
            GroupUnderline.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0")); //Brushes.LightBlue; //464646
            GroupUnderline.Margin = new Thickness(8, 0, 4, 4);
            GroupUnderline.BorderThickness = new Thickness(0, 0, 0, 2);
            GroupTopGrid.Children.Add(GroupUnderline);
            //DockPanel.SetDock(GroupUnderline, Dock.Top);            
            //GroupUnderline.Width = GroupLabel.Width;
            if (GroupClass.GroupTooltip == "") { GroupUnderline.Visibility = Visibility.Collapsed; }

            


            GroupTopGrid.MouseMove += GroupDrag;
            //GroupTopPanel.MouseMove += GroupDrag;
            void GroupDrag(object sender, MouseEventArgs e)
            {
                if (GroupClass.GroupColumn.ColumnRow.SWData.TheEditor.Workshop.IsPreviewMode == true) { return; }

                if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyUp(Key.LeftShift)) //Single Column capture
                {
                    var TheDockPanel = GroupTopGrid;
                    var currentPosition = e.GetPosition(GroupTopGrid);
                    var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                    if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                    {
                        var data = new DataObject("GroupToMove", GroupClass);
                        DragDrop.DoDragDrop(GroupTopGrid, data, DragDropEffects.Move);
                    }
                    TheDockPanel.ReleaseMouseCapture();


                }




            }

            GroupTopGrid.AllowDrop = true;
            GroupTopGrid.Drop += GroupDrop;
            GroupLabel.AllowDrop = true;
            GroupLabel.Drop += GroupDrop;
            void GroupDrop(object sender, DragEventArgs e)
            {

                if (e.Data.GetDataPresent("EntryMoveList")) //Entry Drop
                {
                    List<Entry> EntryMoveList = (List<Entry>)e.Data.GetData("EntryMoveList");


                    StandardEditorMethods.MoveEntrysUnderGroup(EntryMoveList, GroupClass);

                }

                if (e.Data.GetDataPresent("GroupToMove")) //Entry Drop
                {
                    Group agroup = (Group)e.Data.GetData("GroupToMove");

                    StandardEditorMethods.MoveGroupUnderGroup(agroup, GroupClass);

                }

                e.Handled = true; // 🛑 Prevent the entry's parent from stealing the drop.

            }




            ContextMenu contextMenu = new ContextMenu(); // THE RIGHT CLICK MENU
            GroupTopGrid.ContextMenu = contextMenu;

            MenuItem MenuItemNewGroupLeft = new MenuItem();
            MenuItemNewGroupLeft.Header = "  Move Into New Group (Left)  ";
            contextMenu.Items.Add(MenuItemNewGroupLeft);
            MenuItemNewGroupLeft.Click += new RoutedEventHandler(NewGroupLeft);
            void NewGroupLeft(object sender, RoutedEventArgs e)
            {
                GroupClass.GroupColumn.ColumnRow.SWData.TheEditor.Workshop.CreateNewColumnLeft(GroupClass.GroupColumn);

                int IndexC = GroupClass.GroupColumn.ColumnRow.ColumnList.IndexOf(GroupClass.GroupColumn) - 1;
                Column ColumnC = GroupClass.GroupColumn.ColumnRow.ColumnList[IndexC];

                StandardEditorMethods.MoveGroupToBottomOfColumn(GroupClass, ColumnC);
            }


            MenuItem MenuItemNewGroupRight = new MenuItem();
            MenuItemNewGroupRight.Header = "  Move Into New Group (Right)  ";
            contextMenu.Items.Add(MenuItemNewGroupRight);
            MenuItemNewGroupRight.Click += new RoutedEventHandler(NewGroupRight);
            void NewGroupRight(object sender, RoutedEventArgs e)
            {
                GroupClass.GroupColumn.ColumnRow.SWData.TheEditor.Workshop.CreateNewColumnRight(GroupClass.GroupColumn);

                int IndexC = GroupClass.GroupColumn.ColumnRow.ColumnList.IndexOf(GroupClass.GroupColumn) + 1;
                Column ColumnC = GroupClass.GroupColumn.ColumnRow.ColumnList[IndexC];

                StandardEditorMethods.MoveGroupToBottomOfColumn(GroupClass, ColumnC);
            }
        }


        public void CreateEntry(Entry EntryClass)
        {
            //An entry is the main attraction of the program. It is the numerical value of a hex / cell in a file.
            //It can do all kinds of things, be displayed all kinds of ways, and is extremely flexable.
            //For more information, the file "EntryManager" and it's methods go over a lot about what a entry can do.

            Editor EditorClass = EntryClass.EntryEditor;
            Category CatClass = EntryClass.EntryRow;
            Column ColumnClass = EntryClass.EntryColumn;
            Workshop TheWorkshop = EntryClass.EntryEditor.Workshop;
            WorkshopData Database = EntryClass.EntryEditor.Workshop.MyDatabase;


            Border EntryBorder = EntryClass.EntryBorder;
            EntryBorder.BorderThickness = new Thickness(2);
            EntryBorder.CornerRadius = new CornerRadius(3);
            DockPanel.SetDock(EntryBorder, Dock.Top);
            EntryBorder.Margin = new Thickness(4, 0, 5, 3);// Left Top Right Bottom 
            EntryBorder.MinHeight = 38; //Height of a entry, so it doesn't shrink too small when there are no entrys in it.

            if (LibraryMan.ShowHiddenEntrys == false && (EntryClass.IsEntryHidden == true || EntryClass.IsTextInUse == true))
            {
                EntryBorder.Visibility = Visibility.Collapsed;
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


            //MenuItem EntryToNewGroup = new MenuItem();
            //EntryToNewGroup.Header = "  Create New Group  ";
            //contextMenu.Items.Add(EntryToNewGroup);
            //EntryToNewGroup.Click += new RoutedEventHandler(NewGroup);
            //void NewGroup(object sender, RoutedEventArgs e)
            //{

            //}


            MenuItem EntryCreateNewGroup = new MenuItem();
            EntryCreateNewGroup.Header = "  Create new group ";
            contextMenu.Items.Add(EntryCreateNewGroup);
            EntryCreateNewGroup.Click += new RoutedEventHandler(NewColumnGroup);
            void NewColumnGroup(object sender, RoutedEventArgs e)
            {
                if (EntryClass.EntryGroup != null) 
                {
                    return;
                }

                Column EntryColumn = EntryClass.EntryColumn;
                int ToIndex = EntryColumn.ColumnPanel.Children.IndexOf(EntryClass.EntryBorder);                
                            


                Group NewGroup = new();
                NewGroup.GroupColumn = EntryColumn;

                
                int ItemIndex = EntryColumn.ItemBaseList.IndexOf(EntryClass);
                EntryColumn.ItemBaseList.Remove(EntryClass);
                EntryColumn.ItemBaseList.Insert(ItemIndex, NewGroup);

                EntryColumn.ColumnPanel.Children.Remove(EntryClass.EntryBorder); //Remove the entry from the column, so we can add it to the group.
                EntryClass.EntryGroup = NewGroup;

                CreateGroup(NewGroup, ToIndex);

                NewGroup.EntryList.Add(EntryClass); 
                NewGroup.GroupPanel.Children.Add(EntryClass.EntryBorder); 
                


                
                //StandardEditorMethods.CreateNewGroup(EntryClass);

            }
            

            MenuItem EntryToNewLeftColumn = new MenuItem();
            EntryToNewLeftColumn.Header = "  Move into New Column  (Left)  ";
            contextMenu.Items.Add(EntryToNewLeftColumn);
            EntryToNewLeftColumn.Click += new RoutedEventHandler(NewColumnLeft);
            void NewColumnLeft(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewColumnLeft(EntryClass.EntryColumn);

                //For some reason, refering to Column is problematic, but EntryClass.EntryColumn works fine.
                //Im guessing it's because "ColumnClass" thats referenced HERE in code, isn't the same as EntryClass.EntryColumn...

                List<Entry> ListOfEntrys = new();
                ListOfEntrys.Add(EntryClass);

                int index = EntryClass.EntryColumn.ColumnRow.ColumnList.IndexOf(EntryClass.EntryColumn);
                Column toColumn = EntryClass.EntryColumn.ColumnRow.ColumnList[index - 1];

                StandardEditorMethods.MoveEntrysToColumn(ListOfEntrys, toColumn);
            }



            MenuItem EntryToNewRightColumn = new MenuItem();
            EntryToNewRightColumn.Header = "  Move into New Column  (Right)  ";
            contextMenu.Items.Add(EntryToNewRightColumn);
            EntryToNewRightColumn.Click += new RoutedEventHandler(NewColumnRight);
            void NewColumnRight(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewColumnRight(EntryClass.EntryColumn);

                //For some reason, refering to Column is problematic, but EntryClass.EntryColumn works fine.
                //Im guessing it's because "ColumnClass" thats referenced HERE in code, isn't the same as EntryClass.EntryColumn...

                List<Entry> ListOfEntrys = new();
                ListOfEntrys.Add(EntryClass);

                int index = EntryClass.EntryColumn.ColumnRow.ColumnList.IndexOf(EntryClass.EntryColumn);
                Column toColumn = EntryClass.EntryColumn.ColumnRow.ColumnList[index + 1];

                StandardEditorMethods.MoveEntrysToColumn(ListOfEntrys, toColumn);
            }

            



            EntryDockPanel.MouseLeftButtonDown += EntryGrid_MouseLeftButtonDown;
            // define the event handler method
            void EntryGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    
                }
                else
                {      
                    StandardEditorMethods.EntryActivate(EntryClass);
                    //e.Handled = true; // 🛑 Prevent the entry's parent from stealing the click event.
                }

                


            }


            



            bool _mousePressedOnEntry = false;

            EntryDockPanel.MouseLeftButtonDown += EntryDockPanel_MouseLeftButtonDown;
            EntryDockPanel.MouseMove += EntryBorder_MouseMove;

            void EntryDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (e.OriginalSource is TextBox)
                {
                    return; // If it's a TextBox, return and don't set the flag
                }

                var theDockPanel = (DockPanel)sender;
                var theBorder = (Border)theDockPanel.Parent;
                var hitTestResult = VisualTreeHelper.HitTest(theBorder, e.GetPosition(theBorder));

                if (hitTestResult != null)
                {
                    _mousePressedOnEntry = true;
                }
            }

            void EntryBorder_MouseMove(object sender, MouseEventArgs e)
            {
                if (TheWorkshop.IsPreviewMode == true) { return; }

                if (e.OriginalSource is TextBox)
                {
                    _mousePressedOnEntry = false;
                    return; // If it's a TextBox, return and don't execute the drag logic
                }

                if (e.LeftButton == MouseButtonState.Pressed && _mousePressedOnEntry)
                {
                    var theDockPanel = (DockPanel)sender;
                    var theBorder = (Border)theDockPanel.Parent;
                    var hitTestResult = VisualTreeHelper.HitTest(theBorder, e.GetPosition(theBorder));

                    if (hitTestResult != null)
                    {
                        if (Keyboard.IsKeyUp(Key.LeftShift)) // Single Entry capture
                        {
                            //I forget what all this math code does, but i think it makes sure the mouse has actually moved, so its not dropping on itself on frame 1.
                            var currentPosition = e.GetPosition(theBorder);
                            var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                            if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                            {
                                List<Entry> EntrysToMove = new();
                                EntrysToMove.Add(EntryClass);

                                var data = new DataObject("EntryMoveList", EntrysToMove);
                                DragDrop.DoDragDrop(theBorder, data, DragDropEffects.Move);
                            }                            
                        }
                        else if (Keyboard.IsKeyDown(Key.LeftShift)) // Capture Multiple Entrys
                        {
                            var ActiveEntry = EditorClass.StandardEditorData.SelectedEntry;

                            if (ActiveEntry.EntryColumn == EntryClass.EntryColumn)
                            {
                                List<Entry> EntrysToMove = new();

                                if (EntryClass.EntryGroup == null && EditorClass.StandardEditorData.SelectedEntry.EntryGroup == null) //If both are column entrys!
                                {
                                    int iOne = EntryClass.EntryColumn.ItemBaseList.IndexOf(EntryClass);
                                    int iTwo = EditorClass.StandardEditorData.SelectedEntry.EntryColumn.ItemBaseList.IndexOf(EditorClass.StandardEditorData.SelectedEntry);
                                    int startIndex = Math.Min(iOne, iTwo);
                                    int endIndex = Math.Max(iOne, iTwo);

                                    for (int i = startIndex; i <= endIndex; i++)
                                    {
                                        if (EntryClass.EntryColumn.ItemBaseList[i] is Entry entrya)
                                        {
                                            EntrysToMove.Add(entrya);
                                        }

                                    }

                                    var currentPosition = e.GetPosition(theBorder);
                                    var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                                    if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                                    {
                                        var data = new DataObject("EntryMoveList", EntrysToMove);
                                        DragDrop.DoDragDrop(theBorder, data, DragDropEffects.Move);
                                    }
                                }
                                else if (EntryClass.EntryGroup != null && EditorClass.StandardEditorData.SelectedEntry.EntryGroup != null && EntryClass.EntryGroup == EditorClass.StandardEditorData.SelectedEntry.EntryGroup) //If both are group entrys!
                                {
                                    
                                    int iOne = EntryClass.EntryGroup.EntryList.IndexOf(EntryClass);
                                    int iTwo = EditorClass.StandardEditorData.SelectedEntry.EntryGroup.EntryList.IndexOf(EditorClass.StandardEditorData.SelectedEntry);
                                    int startIndex = Math.Min(iOne, iTwo);
                                    int endIndex = Math.Max(iOne, iTwo);
                                    for (int i = startIndex; i <= endIndex; i++)
                                    {
                                        if (EntryClass.EntryGroup.EntryList[i] is Entry entrya)
                                        {
                                            EntrysToMove.Add(entrya);
                                        }
                                    }
                                    var currentPosition = e.GetPosition(theBorder);
                                    var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;
                                    if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                                    {
                                        var data = new DataObject("EntryMoveList", EntrysToMove);
                                        DragDrop.DoDragDrop(theBorder, data, DragDropEffects.Move);
                                    }
                                }
                                //From Selected Entry
                                //To EntryClass

                                
                                
                            }
                        }
                        theDockPanel.ReleaseMouseCapture();

                    }
                }
                else
                {
                    _mousePressedOnEntry = false;
                }
            }





            EntryDockPanel.AllowDrop = true;
            EntryDockPanel.Drop += EntryDrop;
            
            void EntryDrop(object sender, DragEventArgs e)
            {
                

                if (e.Data.GetDataPresent("EntryMoveList") ) //Entry Drop
                {
                    List<Entry> EntryMoveList = (List<Entry>)e.Data.GetData("EntryMoveList");
                    
                    StandardEditorMethods.MoveEntrysUnderEntry(EntryMoveList, EntryClass);

                }

                if (e.Data.GetDataPresent("GroupToMove")) //Entry Drop
                {
                    Group agroup = (Group)e.Data.GetData("GroupToMove");

                    StandardEditorMethods.MoveGroupUnderEntry(agroup, EntryClass);

                }


                e.Handled = true; // 🛑 Prevent the entry's parent from stealing the drop.


                

            }



            if (EntryClass.EntryGroup == null) { ColumnClass.ColumnPanel.Children.Add(EntryBorder); }
            if (EntryClass.EntryGroup != null) { EntryClass.EntryGroup.GroupPanel.Children.Add(EntryBorder); }
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
            if (LibraryMan.ShowEntryAddress == false) { PrefixEID.Visibility = Visibility.Collapsed; }
            if (LibraryMan.ShowEntryAddress == true) { PrefixEID.Visibility = Visibility.Visible; }
            if (LibraryMan.EntryAddressType == "Decimal") { PrefixEID.Content = EntryClass.RowOffset; }
            if (LibraryMan.EntryAddressType == "Hex") { PrefixEID.Content = (EntryClass.RowOffset + int.Parse(TheWorkshop.EntryAddressOffsetTextbox.Text)).ToString("X"); }
            EntryDockPanel.Children.Add(PrefixEID);
            EntryClass.EntryPrefix = PrefixEID;

            ///////////////////STARTING HERE IS STUFF FOR THE PAINFULLY OVER COMPLICATED SYSTEM WHERE AN ENTRY GETS UNDERLINED IF IT HAS A TOOLTIP.//////////////////////
            Grid NewPanel = EntryClass.EntryLeftGrid;
            NewPanel.Background = Brushes.Transparent;
            EntryDockPanel.Children.Add(NewPanel);
            NewPanel.Children.Add(EntryClass.EntryNameTextBlock);
            NewPanel.Children.Add(EntryClass.UnderlineBorder);  
            EntryClass.UnderlineBorder.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#A0A0A0");
            EntryClass.UnderlineBorder.Margin = new Thickness(4, 0, 0, 4); // Left Top Right Bottom
            EntryClass.UnderlineBorder.HorizontalAlignment = HorizontalAlignment.Left;
            EntryClass.EntryNameTextBlock.Inlines.Add(EntryClass.RunEntryName);

            TextBlock EntryTextBlock = EntryClass.EntryNameTextBlock;
            EntryTextBlock.Margin = new Thickness(5, 0, 1, 0);
            EntryTextBlock.FontSize = 20;
            EntryTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            EntryTextBlock.VerticalAlignment = VerticalAlignment.Center;

            ToolTipService.SetInitialShowDelay(EntryClass.EntryLeftGrid, LibraryMan.TooltipInitialDelay); 
            ToolTipService.SetBetweenShowDelay(EntryClass.EntryLeftGrid, LibraryMan.TooltipBetweenDelay); 

            ////////////////END OF UNDERLINE SYSTEM//////////////////////

            StandardEditorMethods.UpdateEntryName(EntryClass);



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

            if (TheWorkshop.IsPreviewMode == false)
            {
                EManage.EntryStyleUpdate(EntryClass); //applys style brushes to the current entry

                TheWorkshop.UpdateSymbology(EntryClass);
            }

            if (EntryClass.Bytes == 0)
            {
                EntryBorder.Visibility = Visibility.Collapsed;
            }

        }

        

    }




}
