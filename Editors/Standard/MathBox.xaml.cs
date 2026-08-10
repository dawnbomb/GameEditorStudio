using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Formats.Tar;
using System.Linq;
using System.Text;
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
using static GameEditorStudio.AutoModMathStep;
using static GameEditorStudio.MathStep;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for MathBox.xaml
    /// </summary>
    public partial class MathBox : UserControl
    {
        MathboxData MathboxData = null;

        public void NewMathboxBasedOnEntry(Entry EntryClass)
        {
            MathboxData.ParentEditor = EntryClass.ParentEditor;
            MathboxData.ParentCategory = EntryClass.ParentCategory;
            MathboxData.ParentGrid = EntryClass.ParentGrid; //currently, entrys in groups dont have a parent grid set? or it's set to null? Regardless its bugged.
            MathboxData.ParentGridItems = EntryClass.ParentGridItems;
            MathboxData.ParentGridItems.Add(this.MathboxData);
            MathboxData.ParentEditor.WorkshopData.MasterMathboxList.Add(MathboxData);
            MathboxData.ParentGroup = EntryClass.ParentGroup;
            MathboxData.Column = EntryClass.Column;
            MathboxData.Row = EntryClass.Row;

            //I may need to bump the row of everything in the same column by 1? Or everything below this entry?

            //CreateMathbox(MathboxEntry);
            MathStep mathStepFromEntry = new();
            mathStepFromEntry.ValueSource = MathValueSources.Entry;
            mathStepFromEntry.EntryTarget = EntryClass;

            MathStep mathStepMultiply = new();
            mathStepMultiply.MathPiece = MathPieces.Multiply;
            mathStepMultiply.ValueSource = MathValueSources.Number;
            mathStepMultiply.MathValue = "2";

            MathboxData.MathFormula.Add(mathStepFromEntry);
            MathboxData.MathFormula.Add(mathStepMultiply);

            DTEMethods.UpdateEditorGrids(EntryClass.ParentEditor.DataTableEditorData);
            UpdateMathResult();
        }

        public MathBox(MathboxData MBD)
        {
            InitializeComponent();
            MathboxData = MBD;

            MathboxBorder.BorderThickness = new Thickness(2);
            MathboxBorder.CornerRadius = new CornerRadius(3);
            MathboxBorder.Margin = new Thickness(5, 5, 0, -1);// Left Top Right Bottom 
            MathboxBorder.MinHeight = 28; //Height of a entry, so it doesn't shrink too small when there are no entrys in it.

            DockPanel.SetDock(MathboxBorder, Dock.Top);
            MathboxData.Visual = this;
            MathboxData.Mathbox = this;

            MathboxLabel.Content = MathboxData.Name;

            ToolTipService.SetInitialShowDelay(MathboxLeftGrid, LibraryGES.TooltipInitialDelay);
            ToolTipService.SetBetweenShowDelay(MathboxLeftGrid, LibraryGES.TooltipBetweenDelay);

            AddRightClickMenu();
            AddClickAndDrop();
            UpdateTooltip();
            UpdateNameDisplay();
        }

        public void UpdateNameDisplay() 
        {
            if (MathboxData.IsNameHidden == true) 
            {
                MathboxLeftGrid.Visibility = Visibility.Collapsed;
            }
            if (MathboxData.IsNameHidden == false)
            {
                MathboxLeftGrid.Visibility = Visibility.Visible;
            }
        }
        private void AddRightClickMenu() 
        {
            ContextMenu contextMenu = new ContextMenu();
            GridItemDockPanel.ContextMenu = contextMenu;


            MenuItem MenuItemCreateNewGroup = new MenuItem();
            MathboxData.CreateNewGroup = MenuItemCreateNewGroup;
            MenuItemCreateNewGroup.Header = "Create Group";
            MenuItemCreateNewGroup.ToolTip = "You can put entrys together inside a Group. \nYou can drag and drop entrys into a group as usual,\nand even move the group around.\n\nGroups do nothing on their own, \nthey are just another way to categorize entrys.";
            contextMenu.Items.Add(MenuItemCreateNewGroup);
            MenuItemCreateNewGroup.Click += new RoutedEventHandler(NewColumnGroup);
            void NewColumnGroup(object sender, RoutedEventArgs e)
            {
                if (MathboxData.ParentGroup != null)
                {
                    return;
                }


                Group NewGroup = new();
                NewGroup.ParentEditor = MathboxData.ParentEditor;
                NewGroup.ParentCategory = MathboxData.ParentCategory;
                NewGroup.ParentGrid = MathboxData.ParentGrid;
                NewGroup.ParentGridItems = MathboxData.ParentGridItems;
                NewGroup.ParentGridItems.Add(NewGroup);
                NewGroup.Column = MathboxData.Column;
                NewGroup.Row = MathboxData.Row;
                NewGroup.RowSpan = 1 + MathboxData.RowSpan;

                DTESetup dTESetup = new DTESetup();
                dTESetup.CreateGroup(NewGroup);

                MathboxData.ParentCategory.GridItems.Remove(MathboxData);
                MathboxData.ParentGrid.Children.Remove(MathboxData.Visual);
                var OldParentGrid = MathboxData.ParentGrid;
                var OldParentGridItems = MathboxData.ParentGridItems;


                MathboxData.ParentGroup = NewGroup;
                MathboxData.ParentGrid = NewGroup.ItemGrid;
                MathboxData.ParentGridItems = NewGroup.GridItems;
                MathboxData.ParentGroup.GridItems.Add(MathboxData);
                //EntryClass.ParentGrid.Children.Add(EntryClass.Visual);
                MathboxData.Row = 1;
                MathboxData.Column = 1;

                DTEMethods.UpdateEditorGrids(MathboxData.ParentEditor.DataTableEditorData);


            }


            Separator separator = new Separator();
            contextMenu.Items.Add(separator);
            separator.Style = (Style)Application.Current.FindResource("DashLineForMenu");


            MenuItem MenuItemDeleteMathbox = new MenuItem();
            MenuItemDeleteMathbox.Header = "Delete This Mathbox";
            contextMenu.Items.Add(MenuItemDeleteMathbox);
            MenuItemDeleteMathbox.Click += new RoutedEventHandler(DeleteThisMathbox);
            void DeleteThisMathbox(object sender, RoutedEventArgs e)
            {
                MathboxLabel.Content = "ded"; //<- this is purely for debugging. 

                //MathboxData.ParentGrid.Children.Remove(MathboxData.Visual);
                //MathboxData.ParentGrid.Children.Remove(this);
                //MathboxData.ParentGrid.Children.Remove(MathboxData.Visual);
                                
                //This answer for deleting was not properly tested to be mistake proof. 

                MathboxData.ParentEditor.WorkshopData.MasterMathboxList.Remove(MathboxData);
                MathboxData.ParentGridItems.Remove(MathboxData);                
                if (MathboxData.ParentCategory != null) { MathboxData.ParentCategory.ItemGrid.Children.Remove(this); }
                if (MathboxData.ParentGroup != null) { MathboxData.ParentGroup.ItemGrid.Children.Remove(this); }

                MathboxData.ParentGrid = null;
                MathboxData.ParentGridItems = null;
                MathboxData.ParentCategory = null;
                MathboxData.ParentGroup = null;

                DTEMethods.UpdateEditorGrids(MathboxData.ParentEditor.DataTableEditorData);
            }
        }

        private void AddClickAndDrop() 
        {   
            //this.MouseLeftButtonDown += GridItemClick;
            //MathboxLeftGrid.MouseLeftButtonDown += GridItemClick;
            //MathboxLabel.MouseLeftButtonDown += GridItemClick;
            //MathboxTooltipLine.MouseLeftButtonDown += GridItemClick;
            MathboxBorder.MouseLeftButtonDown += GridItemClick;

            //this.MouseMove += GridItemMouseMove;
            //MathboxLeftGrid.MouseMove += GridItemMouseMove;
            //MathboxLabel.MouseMove += GridItemMouseMove;
            //MathboxTooltipLine.MouseMove += GridItemMouseMove;
            MathboxBorder.MouseMove += GridItemMouseMove;

            void GridItemClick(object sender, MouseButtonEventArgs e)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {

                }
                else
                {
                    DTRightBar RightBar = MathboxData.ParentEditor.DataTableEditorData.EditorRightBar;
                    RightBar.DTEData.MathboxClass = MathboxData; //Set the Selected Group

                    RightBar.MathboxTabItem.IsSelected = true;
                    RightBar.PropertiesMathboxNameBox.Text = MathboxData.Name;
                    RightBar.PropertiesMathboxTooltipBox.Text = MathboxData.WorkshopTooltip;

                    ShowFormula();
                    
                    UpdateMathResult();

                    e.Handled = true; // Prevents the event from bubbling up to the DockPanel, which would cause the entry to be selected instead of the group.
                }
            }
            void GridItemMouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (e.OriginalSource is DependencyObject d && (d is TextBox || d is ComboBox || d is Button || d is ToggleButton)) 
                    {
                        e.Handled = true; return;
                    }

                    //||  d is Border //Was causing problems with entrys that have tooltips. I could not drag from the name aka the tooltip BORDER.
                    //but NOT having it blocks combo box (drop downs entrys) from opening. So instead i do this...
                    if (e.OriginalSource is DependencyObject b && (b is Border))
                    {
                        if (b != MathboxTooltipLine) 
                        { 
                            e.Handled = true;
                            return; 
                        }
                    }

                    DTEMethods.BeginDrag(this,MathboxData, MathboxData.ParentEditor.DataTableEditorData.DTEXaml.TheScrollviewer);
                }
            }


            GridItemDockPanel.AllowDrop = true;
            GridItemDockPanel.Drop += OnDrop;
            GridItemDockPanel.DragOver += OnDragOver;
            void OnDrop(object sender, DragEventArgs e)
            {
                //IF any GridItems being dragged include a group,
                //and destination is in a group, then cancel!
                if (e.Data.GetDataPresent(typeof(List<GridItem>)))
                {
                    var items = e.Data.GetData(typeof(List<GridItem>)) as List<GridItem>;

                    //if (EntryClass.ParentGroup != null) //If this is in a group, and data present is a group, cancel!
                    //{
                    //    foreach (GridItem theitem in items)
                    //    {
                    //        if (theitem is Group)
                    //        {
                    //            return;
                    //        }

                    //    }

                    //    foreach (GridItem theitem in items)
                    //    {
                    //        if (theitem is Group)
                    //        {
                    //            return;
                    //        }

                    //    }
                    //}

                }

                DTEMethods.DropOntoItem(e, MathboxData);  //this updates grid layouts as well.              

            }
            void OnDragOver(object sender, DragEventArgs e)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true; // 🛑 Prevent the item's parent from stealing the drop.
            }
        }




        


        public void ShowFormula() 
        {
            StackPanel FormulaStack = MathboxData.ParentEditor.DataTableEditorData.EditorRightBar.FormulaStackPanel;

            FormulaStack.Children.Clear();

            List<MathboxData> OKmathboxes = new List<MathboxData>();
            foreach (MathboxData mathdata in MathboxData.ParentEditor.DataTableEditorData.WorkshopData.MasterMathboxList) 
            {
                if (mathdata != MathboxData && mathdata.ParentEditor == MathboxData.ParentEditor) { OKmathboxes.Add(mathdata); }
            }

            foreach (MathStep MathStep in MathboxData.MathFormula)
            {
                Border border = new Border();
                border.Margin = new Thickness(3);
                FormulaStack.Children.Add(border);

                DockPanel MainDockPanel = new DockPanel();
                border.Child = MainDockPanel;


                DockPanel rightside = new DockPanel();
                DockPanel.SetDock(rightside, Dock.Right);
                

                ///////////// LEFT SIDE ////////////
                ComboBox PieceComboBox = new ComboBox();
                PieceComboBox.Margin = new Thickness(3);
                PieceComboBox.Width = 120;
                DockPanel.SetDock(PieceComboBox, Dock.Left);
                
                MainDockPanel.Children.Add(PieceComboBox);

                foreach (MathPieces value in Enum.GetValues<MathPieces>())
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = value.ToString(),
                        Tag = value
                    };

                    PieceComboBox.Items.Add(item);

                    if (MathStep.MathPiece == value)
                    {
                        PieceComboBox.SelectedItem = item;
                    }
                }
                


                ComboBox EntryOrValue = new ComboBox();
                EntryOrValue.Margin = new Thickness(3);
                EntryOrValue.Width = 90;
                DockPanel.SetDock(EntryOrValue, Dock.Left);
                MainDockPanel.Children.Add(EntryOrValue);
                //

                ComboBoxItem ValueItem = new();
                ValueItem.Content = "Number";
                EntryOrValue.Items.Add(ValueItem);
                ComboBoxItem EntryItem = new();
                EntryItem.Content = "Entry";
                EntryOrValue.Items.Add(EntryItem);
                ComboBoxItem MathboxItem = new();
                MathboxItem.Content = "Mathbox";
                //EntryOrValue.Items.Add(MathboxItem);


                if (MathStep.ValueSource == MathValueSources.Entry) { EntryItem.IsSelected = true; }
                if (MathStep.ValueSource == MathValueSources.Number) { ValueItem.IsSelected = true; }
                if (MathStep.ValueSource == MathValueSources.Mathbox) { MathboxItem.IsSelected = true; }

                MainDockPanel.Children.Add(rightside);// <- RIGHT SIDE PANEL ADDED AS LAST CHILD

                EntryOrValue.DropDownClosed += EntryOrValue_DropDownClosed;
                void EntryOrValue_DropDownClosed(object sender, EventArgs e)
                {

                    if (EntryItem.IsSelected == true)
                    {
                        MathStep.ValueSource = MathValueSources.Entry;
                        MakeRightSide();
                        UpdateMathResult();
                    }
                    if (ValueItem.IsSelected == true) 
                    {
                        MathStep.ValueSource = MathValueSources.Number;
                        MakeRightSide();
                        UpdateMathResult();
                    }
                    if (MathboxItem.IsSelected == true)
                    {
                        MathStep.ValueSource = MathValueSources.Mathbox;
                        MakeRightSide();
                        UpdateMathResult();
                    }
                }




                
                PieceComboBox.DropDownClosed += PieceComboBox_DropDownClosed;
                void PieceComboBox_DropDownClosed(object sender, EventArgs e)
                {
                    EntryOrValue.Visibility = Visibility.Visible;

                    if (PieceComboBox.SelectedItem is ComboBoxItem item && item.Tag is MathPieces mathPiece)
                    {
                        MathStep.MathPiece = mathPiece;
                        MakeRightSide();
                        UpdateMathResult();
                    }
                }









                //////////////// RIGHT SIDE/////////////

               

                MakeRightSide();

                void MakeRightSide() 
                {
                    rightside.Children.Clear();

                    Button DeleteButton = new();
                    DeleteButton.Content = "Delete";
                    DeleteButton.MaxWidth = 90;
                    DeleteButton.Margin = new Thickness(3);
                    DeleteButton.HorizontalAlignment = HorizontalAlignment.Right;
                    DockPanel.SetDock(DeleteButton, Dock.Right);
                    rightside.Children.Add(DeleteButton);
                    DeleteButton.Click += DeleteButton_Click;
                    void DeleteButton_Click(object sender, RoutedEventArgs e)
                    {
                        MathboxData.MathFormula.Remove(MathStep);
                        FormulaStack.Children.Remove(border);
                        UpdateMathResult();
                    }
                    //OKmathboxes
                    if (MathStep.ValueSource == MathStep.MathValueSources.Mathbox) 
                    {
                        ComboBox MathComboBox = new ComboBox();
                        MathComboBox.Margin = new Thickness(3);
                        DockPanel.SetDock(MathComboBox, Dock.Right);
                        rightside.Children.Add(MathComboBox);

                        int i = 0;
                        foreach (MathboxData mathdata in OKmathboxes)
                        {   

                            ComboBoxItem mathItem = new ComboBoxItem();
                            mathItem.Content = i + ": " + mathdata.Name;
                            if (mathdata.Name == "")
                            {
                                mathItem.Content = i + ": " + "???"; //+ entry.RowOffset 
                                mathItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A4F9D"));
                            }
                            mathItem.Tag = mathdata;

                            MathComboBox.Items.Add(mathItem);

                            //if (MathStep.EntryTarget == entry)
                            //{
                            //    EntryItem.IsSelected = true;
                            //}
                        }

                        //EntryBox.DropDownClosed += EntryBox_DropDownClosed;
                        //void EntryBox_DropDownClosed(object sender, EventArgs e)
                        //{
                        //    ComboBoxItem CBI = EntryBox.SelectedItem as ComboBoxItem;
                        //    if (CBI == null) { return; }
                        //    Entry theEntry = CBI.Tag as Entry;

                        //    MathStep.EntryTarget = theEntry;
                        //    MathStep.MathValue = "";
                        //    UpdateMathResult();
                        //}
                    }
                    if (MathStep.MathPiece == MathPieces.Round || MathStep.MathPiece == MathPieces.RoundUp || MathStep.MathPiece == MathPieces.RoundDown)
                    {
                        EntryOrValue.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (MathStep.ValueSource == MathStep.MathValueSources.Entry) 
                    {
                        ComboBox EntryBox = new ComboBox();
                        EntryBox.Margin = new Thickness(3);
                        DockPanel.SetDock(EntryBox, Dock.Right);
                        rightside.Children.Add(EntryBox);

                        foreach (Entry entry in MathboxData.ParentEditor.DataTableEditorData.MasterEntryList.OrderBy(e => e.RowOffset))
                        {
                            if (entry.IsMerged == true) { continue; }
                            if (entry.IsEntryHidden == true) { continue; }
                            if (entry.IsTextInUse == true) { continue; }

                            ComboBoxItem EntryItem = new ComboBoxItem();

                            StackPanel content = new StackPanel
                            {
                                Orientation = Orientation.Horizontal
                            };

                            TextBlock offsetText = new TextBlock
                            {
                                Text = entry.RowOffset + ": ",
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"))
                            };

                            TextBlock nameText = new TextBlock
                            {
                                Text = entry.Name == "" ? "???" : entry.Name,
                                Foreground = entry.Name == ""
                                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A4F7D"))
                                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CFCBC7"))
                            };

                            content.Children.Add(offsetText);
                            content.Children.Add(nameText);

                            EntryItem.Content = content;
                            EntryItem.Tag = entry;

                            EntryBox.Items.Add(EntryItem);

                            if (MathStep.EntryTarget == entry)
                            {
                                EntryItem.IsSelected = true;
                            }
                        }
                        //foreach (Entry entry in MathboxData.ParentEditor.DataTableEditorData.MasterEntryList)
                        //{
                        //    //if (entry.NewSubType != Entry.EntrySubTypes.NumberBox) { continue; }

                        //    ComboBoxItem EntryItem = new ComboBoxItem();
                        //    EntryItem.Content = entry.RowOffset + ": " + entry.Name;
                        //    if (entry.Name == "") 
                        //    { 
                        //        EntryItem.Content = entry.RowOffset + ": " + "??? "; //+ entry.RowOffset 
                        //        EntryItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A4F9D")); 
                        //    }
                        //    EntryItem.Tag = entry;
                        //    EntryBox.Items.Add(EntryItem);

                        //    if (Formula.EntryTarget == entry) { EntryItem.IsSelected = true; }
                        //}

                        EntryBox.DropDownClosed += EntryBox_DropDownClosed;
                        void EntryBox_DropDownClosed(object sender, EventArgs e)
                        {
                            ComboBoxItem CBI = EntryBox.SelectedItem as ComboBoxItem;
                            if (CBI == null) { return; }
                            Entry theEntry = CBI.Tag as Entry;

                            MathStep.EntryTarget = theEntry;
                            MathStep.MathValue = "";
                            UpdateMathResult();
                        }
                    }
                    if (MathStep.ValueSource == MathStep.MathValueSources.Number)
                    {
                        TextBox textBox = new TextBox();
                        textBox.Margin = new Thickness(3);
                        DockPanel.SetDock(textBox, Dock.Right);
                        rightside.Children.Add(textBox);

                        textBox.Text = MathStep.MathValue;


                        textBox.TextChanged += MathValueboxTextChanged;
                        void MathValueboxTextChanged(object sender, EventArgs e)
                        {
                            MathStep.MathValue = textBox.Text;
                            MathStep.EntryTarget = null;
                            UpdateMathResult();
                        }
                    }
                    
                }

                

            }
        }

        

        public void UpdateTooltip() 
        {
            if (MathboxData.WorkshopTooltip == "")
            {
                MathboxLeftGrid.ToolTip = null;
                MathboxTooltipLine.Visibility = Visibility.Collapsed;
            }
            if (MathboxData.WorkshopTooltip != "") 
            {
                MathboxLeftGrid.ToolTip = MathboxData.WorkshopTooltip;
                MathboxTooltipLine.Visibility = Visibility.Visible;
            }
        }

        public void UpdateMathResult() 
        {
            if (MathboxData.ParentEditor.WorkshopData.IsProjectLoaded == false) 
            {
                MathResultBox.Text = "";
                MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultTextbox.Text = "";
                MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultUsingInputTextbox.Text = "";
                //MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultUsingOutputTextbox.Text = "";
                return;
            }

            try
            {
                //For Current Value
                double valueSoFar = CalculateFormula(e => e.EntryByteDecimal);
                MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultTextbox.Text = valueSoFar.ToString();
                MathResultBox.Text = valueSoFar.ToString();

                //For Value from Input.
                if (MathboxData.ParentEditor.DataTableEditorData.EntryClass.EntryValueOnProjectLoadFromInput != null && MathboxData.ParentEditor.DataTableEditorData.EntryClass.EntryValueOnProjectLoadFromInput != "")
                {
                    double valueSoFarFromInput = CalculateFormula(e => e.EntryValueOnProjectLoadFromInput);
                    MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultUsingInputTextbox.Text = valueSoFarFromInput.ToString();
                }
                else { MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultUsingInputTextbox.Text = "Reload Project"; }
                

                //For Value from Output.
                //double valueSoFarFromOutput = CalculateFormula(e => e.EntryValueOnProjectLoadFromOutput);
                //MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultUsingOutputTextbox.Text = valueSoFarFromOutput.ToString();


                double CalculateFormula(Func<Entry, string> GetEntryValue)
                {
                    double valueSoFar = 0;

                    foreach (MathStep MathStep in MathboxData.MathFormula)
                    {
                        double operand;

                        switch (MathStep.ValueSource)
                        {   

                            case MathStep.MathValueSources.Entry:
                                if (MathStep.MathPiece == MathPieces.Round || MathStep.MathPiece == MathPieces.RoundUp || MathStep.MathPiece == MathPieces.RoundDown)
                                {
                                    operand = 0;
                                    break;
                                }
                                if (MathStep.EntryTarget == null) 
                                    continue;

                                if (MathStep.EntryTarget.NewSubType == Entry.EntrySubTypes.NumberBox) //to deal with possible negative values.
                                {
                                    //THIS IS A BIG TO FIX LATER.
                                    //THE FROM INPUT WONT WORK PROPERLY IF WORKING WITH NUMBERBOX.
                                    operand = double.Parse(MathStep.EntryTarget.EntryTypeNumberBox.NumberBoxTextBox.Text);
                                }
                                else
                                {
                                    operand = double.Parse(GetEntryValue(MathStep.EntryTarget));
                                }
                                break;


                            case MathStep.MathValueSources.Number:
                                if (MathStep.MathPiece == MathPieces.Round || MathStep.MathPiece == MathPieces.RoundUp || MathStep.MathPiece == MathPieces.RoundDown)
                                {
                                    operand = 0;
                                    break;
                                }
                                operand = double.Parse(MathStep.MathValue);
                                break;


                            default:
                                continue;
                        }

                        if (MathStep.MathPiece == MathPieces.Plus) { valueSoFar += operand; }
                        else if (MathStep.MathPiece == MathPieces.Plus) { valueSoFar += operand; }
                        else if (MathStep.MathPiece == MathPieces.Minus) { valueSoFar -= operand; }
                        else if (MathStep.MathPiece == MathPieces.Multiply) { valueSoFar *= operand; }
                        else if (MathStep.MathPiece == MathPieces.Divide) { if (operand != 0) valueSoFar /= operand; }
                        else if (MathStep.MathPiece == MathPieces.Max) { valueSoFar = Math.Min(valueSoFar, operand); }
                        else if (MathStep.MathPiece == MathPieces.Min) { valueSoFar = Math.Max(valueSoFar, operand); }
                        else if (MathStep.MathPiece == MathPieces.Round) { valueSoFar = Math.Round(valueSoFar); }
                        else if (MathStep.MathPiece == MathPieces.RoundUp) { valueSoFar = Math.Ceiling(valueSoFar); }
                        else if (MathStep.MathPiece == MathPieces.RoundDown) { valueSoFar = Math.Floor(valueSoFar); }
                    }

                    return valueSoFar;
                }
            }
            catch
            {
                MathResultBox.Text = "ERROR";
                MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultTextbox.Text = "ERROR";
                MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultUsingInputTextbox.Text = "ERROR";
                //MathboxData.ParentEditor.DataTableEditorData.DTEXaml.RightBar.MathboxMathResultUsingOutputTextbox.Text = "ERROR";
            }
            
        }

        

        public void CreateMathbox() 
        {


            //ContextMenu contextMenu = new ContextMenu(); // THE RIGHT CLICK MENU
        }
    }
}
