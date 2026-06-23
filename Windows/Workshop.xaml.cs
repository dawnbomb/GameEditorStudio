using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
//using System.Linq;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using GameEditorStudio.Loading;
using Microsoft.Windows.Themes;
using Ookii.Dialogs.Wpf;
using WpfHexEditor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static GameEditorStudio.Entry;
using static OfficeOpenXml.ExcelErrorValue;

namespace GameEditorStudio
{
    //This file is the most complex and barely sorted part of the program. You will hate it :(
    // it's a partial class, and some other parts of the class are ...all over. 
    // i'm not great with moving stuff between classes yet, so yeah, its just a massive partial class.
    // feel free to ask questions about this, and entrymanager. 
    // 
    // I could use help sorting this, like, a lot. 
    //
    //i tried to previously sort it using giant comment walls between major sections, but i kinda stopped moving stuff to proper sections because i was lazy.
    //
    // the bottom is whatever the newest random shit is. 

    public partial class Workshop : UserControl
    {
        //This is a partial class.              
        
        public WorkshopData WorkshopData { get; set; } //The database of this workshop.  

        public bool IsPreviewMode { get; set; } //VS preview mode. In preview mode, a project folder and input directory are not used, to allow users to preview a workshop. 
        
        public bool TreeViewSelectionEnabled { get; set; } = true; //Move this later, but both Data Table Editor Left and Right bar use this.

        public TextTableManager TextSourceManager { get; set; }
        public UserControlEditorIcons UCGraphicsEditor { get; set; }


        public Workshop(WorkshopData mydata, Project Project, bool IsWorkshopPreviewModeActive = false) //GameLibrary GameLibrary
        {
            InitializeComponent();

            WorkshopData = mydata;            
            WorkshopData.WorkshopXaml = this;
            HomeControl.FileManager.WorkshopXaml = this;
            WorkshopData.WorkshopXaml.MenusForToolsAndEvents.WorkshopData = WorkshopData; //Menu Set WorkshopData.
            
            foreach (Command command in Database.Commands)
            {
                command.WorkshopData = WorkshopData;
            }

            #if DEBUG

            #else
            
            #endif
            
            IsPreviewMode = IsWorkshopPreviewModeActive;
            WorkshopData.LoadedProject = Project;  
            if (IsPreviewMode == false)
            { //LOAD PROJECT 
                WorkshopData.IsProjectLoaded = true; //FOR NOW

                LoadWorkshopDatabaseCode LoadDatabaseB = new();
                LoadDatabaseB.LoadAllProjectDocuments(WorkshopData);
                WorkshopData.WorkshopXaml.MenusForToolsAndEvents.SetupTopMenuForProject(WorkshopData.WorkshopXaml);

                FileLoading fileLoading = new();
                fileLoading.TryLoadAllGameFilesIntoWorkshopDatabase(WorkshopData); //First we load workshop files into the database. }

                HomeControl.HomeLoadProjectButton.Visibility = Visibility.Collapsed; 
                HomeControl.HomeNewProjectButton.Visibility = Visibility.Collapsed; 
                HomeControl.HomeUnloadProjectButton.Visibility = Visibility.Collapsed; 
            }

            
            
            LoadWorkshopDatabaseCode LoadDatabase = new();
            LoadDatabase.LoadEveryEditorXMLIntoWorkshopData(this, WorkshopData); //Then we load the editor info into the database.
            LoadDatabase.GenerateAllEditorXAML(WorkshopData);
            LoadDatabase.LoadAllWorkshopDocuments(WorkshopData);           
            DTEMethods.UpdateHotbarForAllDTEEditors(WorkshopData); //Syncs the hotbar icon state between all DTE editors. 
            
            
            


            if (IsPreviewMode == true) 
            {
                //PropertiesTextboxEditorName.IsEnabled = false;
                //PropertiesEditorReadGameDataFrom.IsEnabled = false;
                //EditorOutputLocationTextbox.IsEnabled = false;
                //OpenInputLocationButton.IsEnabled = false;
                //OpenOutputLocationButton.IsEnabled = false;
                
                //PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = false;
                //PropertiesEditorNameTableStartByte.IsEnabled = false;
                //PropertiesEditorNameTableRowSize.IsEnabled = false;
                //PropertiesEditorNameTableTextSize.IsEnabled = false;
                //PropertiesEditorNameCount.IsEnabled = false;
                
                //DataTableFileBox.IsEnabled = false;
                //PropertiesEditorTableStart.IsEnabled = false;
                //PropertiesEditorTableWidth.IsEnabled = false;
                

                //PropertiesRowNameBox.IsEnabled = false;
                //PropertiesRowTooltipBox.IsEnabled = false;

                //PropertiesGroupNameBox.IsEnabled = false;
                //PropertiesGroupTooltipBox.IsEnabled = false;

                //PropertiesNameBox.IsEnabled = false;
                //HideNameCheckbox.IsEnabled = false;
                //HideEntryCheckbox.IsEnabled = false;
                //PropertiesEntryByteSizeComboBox.IsEnabled = false;
                //PropertiesEntryType.IsEnabled = false;
                //NumberboxSignCheckbox.IsEnabled = false;
                //DropdownMenuType.IsEnabled = false;
                //foreach (ComboBoxItem item in DropdownMenuType.Items) { item.IsEnabled = false; }
                //ButtonMenuManager.IsEnabled = false;
                //EntryNoteTextbox.IsEnabled = false;
                
                //IconManagerButton.IsEnabled = false; 
            }            

            GC.RefreshMemoryLimit(); //Not sure if useful, it's a new .net8 feature to automatically increase memory limit as needed. Might reduce lag? Probably won't hurt? 

            HomeControl.HomeSetup(WorkshopData); //Sets up the Home Tab.        
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HUD//////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////





        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {

            HomeControl.FileManager.RefreshFileTree();

            HIDEALL();
            //DockPanelHome.Visibility = Visibility.Visible;

            foreach (Editor editor in WorkshopData.GameEditors)
            {
                editor.EditorTab.Style = (Style)Application.Current.FindResource("ButtonEditorTab");

            }
            ButtonHome.Style = (Style)Application.Current.FindResource("ButtonCurrentEditorTab");

        }






        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HUD//////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////ROW PROPERTIES//////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void CreateNewRowAbove(Category TheCat)
        {
            Category NewRow = new();
            NewRow.DTEData = TheCat.DTEData;

            int TheIndex = TheCat.DTEData.CategoryList.IndexOf(TheCat);
            NewRow.DTEData.CategoryList.Insert(TheIndex, NewRow);


            DTESetup CreateSWEditorCode = new();
            CreateSWEditorCode.CreateCategory(NewRow);
        }


        public void CreateNewRowBelow(Category TheCat)
        {
            Category NewRow = new();
            NewRow.DTEData = TheCat.DTEData;

            int TheIndex = TheCat.DTEData.CategoryList.IndexOf(TheCat) + 1;
            NewRow.DTEData.CategoryList.Insert(TheIndex, NewRow);


            DTESetup CreateSWEditorCode = new();
            CreateSWEditorCode.CreateCategory(NewRow);
        }

        public void MoveRowUp(Category TheCat) 
        {
            int primaryIndex = TheCat.DTEData.MainDockPanel.Children.IndexOf(TheCat.CatBorder);
            if (primaryIndex != 0)
            {
                var secondaryIndex = primaryIndex - 1;
                Category primary = TheCat.DTEData.CategoryList[primaryIndex];
                Category secondary = TheCat.DTEData.CategoryList[secondaryIndex];

                TheCat.DTEData.MainDockPanel.Children.Remove(primary.CatBorder);
                TheCat.DTEData.CategoryList.RemoveAt(primaryIndex);
                TheCat.DTEData.MainDockPanel.Children.Insert(secondaryIndex, primary.CatBorder);
                TheCat.DTEData.CategoryList.Insert(secondaryIndex, primary);

            }
        }


        public void MoveRowDown(Category TheCat) 
        {
            int primaryIndex = TheCat.DTEData.MainDockPanel.Children.IndexOf(TheCat.CatBorder);
            if (primaryIndex + 1 < TheCat.DTEData.CategoryList.Count)
            {
                var secondaryIndex = primaryIndex + 1;
                Category primary = TheCat.DTEData.CategoryList[primaryIndex];
                Category secondary = TheCat.DTEData.CategoryList[secondaryIndex];

                TheCat.DTEData.MainDockPanel.Children.Remove(primary.CatBorder);
                TheCat.DTEData.CategoryList.RemoveAt(primaryIndex);
                TheCat.DTEData.MainDockPanel.Children.Insert(primaryIndex + 1, primary.CatBorder);
                TheCat.DTEData.CategoryList.Insert(secondaryIndex, primary);

            }
        }

        public void RowDelete(Category TheCat) 
        {
            DTEMethods.UpdateEditorGrids(TheCat.DTEData);
            if (TheCat.GridItems.Count == 0)
            {
                TheCat.DTEData.MainDockPanel.Children.Remove(TheCat.CatBorder);
                TheCat.DTEData.CategoryList.Remove(TheCat);
                //LibraryGES.GotoRightBarGeneralTab(this);
            }
            DTEMethods.UpdateEditorGrids(TheCat.DTEData);
            //Possibly delete this and make row reletion automatic.
            //Although, a user might lose important information in a category tooltip. 

        }





        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////ROW PROPERTIES//////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////GOOGLE SHEETS///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




        public void HIDEMOST() 
        {
            //DockPanelHome.Visibility = Visibility.Collapsed;


            if (UCGraphicsEditor != null) { MidGrid.Children.Remove(UCGraphicsEditor); }
            if (TextSourceManager != null) { MidGrid.Children.Remove(TextSourceManager); }
        }
        

        public void HIDEALL() 
        {
            foreach (Editor editor in WorkshopData.GameEditors)
            {
                editor.EditorVisual.Visibility = Visibility.Collapsed;
            }

            if (UCGraphicsEditor != null) { MidGrid.Children.Remove(UCGraphicsEditor); }
            if (TextSourceManager != null) { MidGrid.Children.Remove(TextSourceManager); }

            
             

        }


        
                

        

        private void EditorBarMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                // Scroll right
                EditorBarScrollViewer.ScrollToHorizontalOffset(EditorBarScrollViewer.HorizontalOffset + 70);
            }
            else
            {
                // Scroll left
                EditorBarScrollViewer.ScrollToHorizontalOffset(EditorBarScrollViewer.HorizontalOffset - 70);
            }

            // Mark the event as handled so it doesn't propagate further
            e.Handled = true;
        }
        


        public void UpdateSymbology(Entry EntryClass)
        {
            if (LibraryGES.ShowSymbology == true)
            {
                EntryClass.Symbology.Visibility = Visibility.Visible;
            }
            if (LibraryGES.ShowSymbology == false)
            {
                EntryClass.Symbology.Visibility = Visibility.Collapsed;
            }

            DataTableEditorData DTEData = EntryClass.ParentEditor.DataTableEditorData;

            

            EntryClass.Symbology.Content = "No";
            EntryClass.Symbology.Width = 48;
            EntryClass.Symbology.Foreground = Brushes.Gray;
            EntryClass.Symbology.FontSize = 20;
            EntryClass.Symbology.ToolTip = "Symbology does not work in preview mode. \nPlease load a project.";

            EntryClass.Symbology.Margin = new Thickness(-3, 0, -7, 0);
            ToolTipService.SetInitialShowDelay(EntryClass.Symbology, 100);
            ToolTipService.SetBetweenShowDelay(EntryClass.Symbology, 100);            

            if (WorkshopData.IsProjectLoaded == false) { return; }            
            if (EntryClass.ParentEditor.DataTableEditorData.NameTable == null) { return; } //This was to test nameless editors, remove later.

            
            if (EntryClass.Bytes == 0) { return; }
            //TheWorkshop.PropertiesEntry1Byte.Text = EditorClass.SWData.FileDataTable.FileBytes[EditorClass.SWData.DataTableStart + (EditorClass.SWData.TableRowIndex * EntryClass.RowSize) + EntryClass.RowOffset].ToString("D");

            {
                List<long> BitFlags = new() { 2, 4, 8, 16, 32, 64, 128 };

                string ValueA = "";
                string ValueB = "";
                string ValueX = "";

                List<long> AllValues = new();

                bool IsAlwaysZero = true;
                bool IsNeverZero = true;
                bool IsAlwaysOne = true;                
                bool IsCheckboxLike = true;                
                bool IsAlwaysValueX = true;
                bool IsCheckbox = true;
                string HalfColor = "#907654";//Between red and gold, the 50% / "Ehhhhh" color.  //cd5032

                for (int i = 0; i < DTEData.NameTable.TextTableItemCount; i++)
                {
                    string value = null;
                    try 
                    {                        

                        if (EntryClass.Endianness == "1")
                        {
                            value = DTEData.DataTable.FileDataTable.FileBytes[DTEData.DataTable.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset].ToString("D");
                        }
                        else if (EntryClass.Endianness == "2B")
                        {
                            ushort value2 = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                            ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
                            value = swappedValue2.ToString("D");
                            
                        }
                        else if (EntryClass.Endianness == "2L")
                        {
                            value = BitConverter.ToUInt16(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                        }
                        else if (EntryClass.Endianness == "4B")
                        {
                            uint valueK = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset);
                            byte[] valueBytes = BitConverter.GetBytes(valueK);
                            Array.Reverse(valueBytes);
                            uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
                            value = swappedValue.ToString("D");
                        }
                        else if (EntryClass.Endianness == "4L")
                        {
                            value = BitConverter.ToUInt32(DTEData.DataTable.FileDataTable.FileBytes, DTEData.DataTable.DataTableStart + (i * EntryClass.DataTableRowSize) + EntryClass.RowOffset).ToString("D");
                                                        
                        }

                    } 
                    catch 
                    {
                        PixelWPF.LibraryPixel.Notification("Error: Symbology caused a crash.",
                            "List of possible causes..." +
                            "\n1: Setting a new Name Table or Data Table where the list of names in the name table is more then the possible rows of the data table's attached file. (Trys to read beyond end of file)." +
                            "\n2: ?????." +
                            "\n" +
                            "\nNote: Please report this."
                            );
                        Environment.FailFast(null); //Kills program instantly. 
                        return;
                    }

                    AllValues.Add(long.Parse(value));

                    if (ValueX == "")
                    {
                        ValueX = value;
                    }                    
                    if (ValueA == "")
                    {
                        ValueA = value;
                    }
                    if (ValueB == "" && ValueA != value)
                    {
                        ValueB = value;
                    }


                    IsAlwaysZero = AllValues.All(x => x == 0);
                    IsNeverZero = !AllValues.Contains(0);
                    IsCheckboxLike = AllValues.Distinct().Count() <= 2;
                    IsAlwaysOne = AllValues.All(v => v == 1);
                    IsAlwaysValueX = long.TryParse(ValueX, out long parsedX) && AllValues.All(v => v == parsedX);
                    IsCheckbox = AllValues.All(v => v == 0 || v == 1);

                }
                                

                bool IsMostlyX = AllValues.GroupBy(x => x).Any(g => (double)g.Count() / AllValues.Count >= 0.8);
                bool IsHalfX = AllValues.GroupBy(x => x).Any(g => (double)g.Count() / AllValues.Count >= 0.5);
                bool HasAtLeast10Unique = AllValues.Distinct().Count() >= 10; //unused
                bool HasAtLeast20Unique = AllValues.Distinct().Count() >= 20; //unused
                bool HasAtLeast30Unique = AllValues.Distinct().Count() >= 30; //unused
                long uniqueCount = AllValues.Distinct().Count();

                //negative number detector
                bool B1NoValuesAre128to199 = false;      // No values are between 128–199
                bool B1_3PercentAbove128 = false; // At least 4% of values are 128+
                bool B1_4PercentAbove200 = false;  // At least 10% of values are 200–255
                bool B1_4PercentBelow127 = false; // At least 10% of values are 127 or below

                if (EntryClass.Bytes == 1)
                {
                    B1NoValuesAre128to199 = !AllValues.Any(v => v >= 128 && v < 200);
                    B1_4PercentAbove200 = AllValues.Count(v => v >= 200) >= AllValues.Count * 0.04;

                    long highValuesCount = AllValues.Count(v => v >= 128);
                    B1_3PercentAbove128 = highValuesCount >= AllValues.Count * 0.03;

                    long lowValuesCount = AllValues.Count(v => v <= 127);
                    B1_4PercentBelow127 = lowValuesCount >= AllValues.Count * 0.04;
                }

                //Bitflag detectors
                bool PureBitFlags = false; //100% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64,128. Atleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.
                bool MostlyBitFlag = false; //70% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64,128. Atleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.
                bool HalfBitFlag = false; //50% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64,128. Atleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.

                if (EntryClass.Bytes == 1)
                {                    
                    var bitValues = new HashSet<long> { 0, 1, 2, 4, 8, 16, 32, 64, 128 };
                    var totalCount = AllValues.Count;
                    var uniqueValues = AllValues.Distinct().ToHashSet();

                    // Basic check: all values are valid bitflag values
                    bool allAreBitFlags = uniqueValues.All(v => bitValues.Contains(v));

                    // Extra checks:
                    long count0Or1 = AllValues.Count(v => v == 0 || v == 1);
                    long countAbove1 = AllValues.Count(v => v > 1);

                    bool hasAtLeast10PercentLow = count0Or1 >= totalCount * 0.1;
                    bool hasAtLeast10PercentHigh = countAbove1 >= totalCount * 0.1;

                    // Final result
                    PureBitFlags = allAreBitFlags && hasAtLeast10PercentLow && hasAtLeast10PercentHigh;

                    //////////Mostly bitflag

                    var bitFlags = new HashSet<long> { 2, 4, 8, 16, 32, 64, 128 };
                    long countBitFlagsOnly = AllValues.Count(v => bitFlags.Contains(v));
                    bool hasEnoughBitFlags = countBitFlagsOnly >= totalCount * 0.7;
                    MostlyBitFlag = hasEnoughBitFlags && hasAtLeast10PercentLow && hasAtLeast10PercentHigh;

                    //////////Half bitflag
                    bool hasHalfBitFlags = countBitFlagsOnly >= totalCount * 0.5;
                    HalfBitFlag = hasHalfBitFlags && hasAtLeast10PercentLow && hasAtLeast10PercentHigh;

                }

                bool B2NoValuesAre32768to40000 = AllValues.All(v => v < 32768 || v > 40000);
                bool B2_1PercentAbove40000 = AllValues.Count(v => v >= 40000) / (double)AllValues.Count >= 0.01;

                bool B4NoValuesAre2BTo2_4B = AllValues.All(v => v < 2147483648 || v > 2400000000);
                bool B4_1PercentAbove2_4B = AllValues.Count(v => v >= 2400000000) / (double)AllValues.Count >= 0.01;

                //////////////////////////////////// ACTUALLY MAKING THE SYMBOLOGY STARTS HERE /////////////////////////////////////


                if (IsCheckboxLike == false && EntryClass.NewSubType == Entry.EntrySubTypes.CheckBox) //IE: Warning! This should not be a checkbox!
                {                    
                    EntryClass.Symbology.Foreground = Brushes.Red;
                    EntryClass.Symbology.Content = "!!!";
                    EntryClass.Symbology.ToolTip = "This entry is probably not a checkbox, it has more then 2 possible values.   ValueA: " + ValueA + " ValueB: " + ValueB + " ValueX: " + ValueX;
                }
                else if (EntryClass.Bytes == 1 && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox && B1NoValuesAre128to199 == true && B1_3PercentAbove128 == true) //Is Probably Negative  // && B1_4PercentBelow127 == true
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "-N";
                    EntryClass.Symbology.ToolTip = "This might be a numberbox that supports negative numbers.\n\n- No values are between 128-200.\n- 3%+ of values are 200+.\nThis is at least suspicious.\n\nPS: values 200~255 are -54 ~ -1 when read as negatives.";
                }
                else if (EntryClass.Bytes == 2 && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox && B2NoValuesAre32768to40000 && B2_1PercentAbove40000)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = "-N";
                    EntryClass.Symbology.ToolTip = "This might be a numberbox that supports negative numbers.\n\n- No values are between 32768-40000.\n- 1%+ of values are 40000+.\nThis is at least suspicious, but as this entry is more then 1 byte, it's almost guarenteed.";
                }
                else if (EntryClass.Bytes == 4 && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox && B4NoValuesAre2BTo2_4B && B4_1PercentAbove2_4B)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = "-N";
                    EntryClass.Symbology.ToolTip = "This might be a numberbox that supports negative numbers.\n\n- No values are between 2147483648-2400000000.\n- 1%+ of values are 2400000000+.\nThis is at least suspicious, but as this entry is more then 1 byte, it's almost guarenteed.";
                }
                //else if (EntryClass.Bytes == 1 && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox && B1NoValuesAre128to199 == true && B1_4PercentAbove128 == true) //Is Likely Negative
                //{
                //    EntryClass.Symbology.Foreground = Brushes.Gray;
                //    EntryClass.Symbology.Content = " -?";
                //    EntryClass.Symbology.ToolTip = "This is PROBABLY data that can represent negative numbers.\n\nNo values are between 128-200, 4%+ of values are 200+.\nThis is at the very least, extremely suspicious.\n\nPS: values 200~255 are -1 ~ -54 when read as negatives.";
                //}

                else if (PureBitFlags == true) //Is bitflag
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " ⚑";
                    EntryClass.Symbology.ToolTip = "This is a pure bitflag.\n\n100% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64, or 128.\nAtleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.";
                }
                else if (MostlyBitFlag == true) //Is bitflag // && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "7BF";
                    EntryClass.Symbology.ToolTip = "This is PROBABLY a bitflag.\n\n70% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64, or 128.\nAtleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.\nThis is at the very least, extremely suspicious.";
                }
                else if (MostlyBitFlag == true) //Is bitflag  // && EntryClass.NewSubType == Entry.EntrySubTypes.NumberBox
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "5BF";
                    EntryClass.Symbology.ToolTip = "This is PROBABLY a bitflag.\n\n50% of values (that are not 0 or 1) are exactly 2,4,8,16,32,64, or 128.\nAtleast 10% of values are 1 or 0, and atleast 10% are 2,4,8,16,32,64, or 128.\nThis is at the very least, extremely suspicious.";
                }

                else if (IsAlwaysZero == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " 0";
                    EntryClass.Symbology.ToolTip = "This entry is always zero.\n\nSometimes this means its actually part of a 2+ byte entry.";
                }                
                else if (IsAlwaysOne == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " 1";
                    EntryClass.Symbology.ToolTip = "This entry is always 1.\n\nSometimes this means its actually part of a 2+ byte entry.";

                }
                else if (IsAlwaysValueX == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = " X";
                    EntryClass.Symbology.ToolTip = "This entry's value is always " + ValueX + ".";
                }
                else if (IsCheckbox == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Orange;
                    EntryClass.Symbology.Content = "✔";
                    EntryClass.Symbology.ToolTip = "This entry is a checkbox (It's always 1 or 0).\n\nOR it's a 2 byte value, like when max MP goes beyond 255.";
                    //SymbolLabel.Margin = new Thickness(-5, 0, 0, 0);
                }
                else if (IsCheckboxLike == true)
                {                    
                    EntryClass.Symbology.Foreground = Brushes.Gray; //new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Content = "✔?";
                    EntryClass.Symbology.ToolTip = "This is checkbox-like, but instead of 0 and 1, it's " + ValueA + " & " + ValueB;
                    //SymbolLabel.Margin = new Thickness(-5,0,0,0);
                }

                //Starting here, it'd be cool if "Is never 0" could stack with everything, to display even more info. 
                else if (uniqueCount > 10) //If Mostly X
                {
                    //EntryClass.Symbology.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    if (uniqueCount < 100) { EntryClass.Symbology.Content = uniqueCount; }
                    else { EntryClass.Symbology.Content = "99+"; }
                    EntryClass.Symbology.ToolTip = "This entry has " + uniqueCount + " unique values.";

                }
                else if (IsNeverZero == true)
                {
                    EntryClass.Symbology.Foreground = Brushes.Red;
                    EntryClass.Symbology.Content = " 0";
                    EntryClass.Symbology.ToolTip = "This entry is never zero.\n\nThis is actually pretty rare, so you should be suspicious. ";

                }
                else if (AllValues.All(v => v == 0 || v == 1 || v == 2) && AllValues.Contains(0) && AllValues.Contains(1) && AllValues.Contains(2)) //Is always 0/1/2
                {
                    //EntryClass.Symbology.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "012";
                    EntryClass.Symbology.ToolTip = "This entry's value is always 0, 1, or 2.";

                }
                else if (IsMostlyX == true) //If Mostly X
                {
                    //EntryClass.Symbology.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = " X?";
                    EntryClass.Symbology.ToolTip = "This entry's value is the same somewhere between 80% and 99% of the time.";

                }
                else if (IsHalfX == true) //If Mostly X
                {
                    EntryClass.Symbology.Foreground = Brushes.Gray;
                    EntryClass.Symbology.Content = "X??";
                    EntryClass.Symbology.ToolTip = "This entry's value is the same somewhere between 50% and 80% of the time.";
                    EntryClass.Symbology.FontSize = 20;

                }
                else //If Mostly X
                {
                    //EntryClass.Symbology.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HalfColor));
                    EntryClass.Symbology.Foreground = Brushes.Gray; 
                    EntryClass.Symbology.Content = "" + uniqueCount;
                    EntryClass.Symbology.ToolTip = "This entry has " + uniqueCount + " unique values.";

                }




                


            }



        }

    }

    public class NumberCount
    {
        public long Number { get; set; }  // <-- changed from int to long
        public int Count { get; set; }
        public List<int> RowIndices { get; set; } = new List<int>();

        public string RowIndicesAsString
        {
            get
            {
                return string.Join(", ", RowIndices);
            }
        }
    }

}