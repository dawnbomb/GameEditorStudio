using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for StandardEditor.xaml
    /// </summary>
    public partial class StandardEditor : UserControl
    {
        Workshop TheWorkshop { get; set; }
        WorkshopData MyDatabase { get; set; }
        Editor EditorClass { get; set; }

        public StandardEditor(Workshop AWorkshop, WorkshopData Database, Editor EditorClass)
        {
            InitializeComponent();

            TheWorkshop = AWorkshop; //Sets the workshop to the one passed in.
            MyDatabase = Database; //Sets the database to the one passed in.
            this.EditorClass = EditorClass; //Sets the editor class to the one passed in.

            LeftBar.LeftBarSetup(AWorkshop, Database, EditorClass); //Sets up the left bar with the workshop and database info.

            EditorClass.StandardEditorData.EditorDescriptionsPanel = DescriptionsPanel;
            EditorClass.StandardEditorData.MainDockPanel = EditorsPanel;

            ////This is all just making sure the current user settings are displayed.
            //if (LibraryGES.EntryAddressType == "Decimal") { EntryAddressTypeButton.Content = "Dec"; }
            //if (LibraryGES.EntryAddressType == "Hex") { EntryAddressTypeButton.Content = "Hex"; }

            if (LibraryGES.ShowHiddenEntrys == true)
            {
                EntryHiddenToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC1E40"));
            }
            else
            {
                EntryHiddenToggle.Foreground = Brushes.Gray;
            }
            if (LibraryGES.ShowHiddenEntrys == true)
            {
                DebugShowALL.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E70EC"));
            }
            else if (LibraryGES.ShowHiddenEntrys == false)
            {
                DebugShowALL.Foreground = Brushes.Gray;
            }

            //RightDock.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            Scroll scroll = new(TheScrollviewer);
            TheScrollviewer.PreviewMouseWheel += scroll.ScrollViewer_PreviewMouseWheel;
            TheScrollviewer.PreviewMouseDown += scroll.ScrollViewer_PreviewMouseDown;
            TheScrollviewer.PreviewMouseUp += scroll.ScrollViewer_PreviewMouseUp;
            TheScrollviewer.PreviewMouseMove += scroll.ScrollViewer_PreviewMouseMove;
            //EditorsPanel.PreviewMouseWheel += scroll.ScrollViewer_PreviewMouseWheel;
            //EditorsPanel.PreviewMouseDown += scroll.ScrollViewer_PreviewMouseDown;
            //EditorsPanel.PreviewMouseUp += scroll.ScrollViewer_PreviewMouseUp;
            //EditorsPanel.PreviewMouseMove += scroll.ScrollViewer_PreviewMouseMove;

        }

        private void ToggleItemIDNumberVisibility(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowItemIndex == false)
            {
                LibraryGES.ShowItemIndex = true;
            }
            else if (LibraryGES.ShowItemIndex == true)
            {
                LibraryGES.ShowItemIndex = false;
            }

            foreach (var editor in MyDatabase.GameEditors)
            {
                if (editor.Value.EditorType == "DataTable")
                {
                    foreach (TreeViewItem TreeItem in editor.Value.StandardEditorData.EditorLeftDockPanel.TreeView.Items)
                    {
                        TheWorkshop.ItemNameBuilder(TreeItem);
                    }
                }

            }
        }

        private void ToggleTranslationPanel(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowTranslationPanel == true)
            {
                LibraryGES.ShowTranslationPanel = false;
            }
            else if (LibraryGES.ShowTranslationPanel == false)
            {
                LibraryGES.ShowTranslationPanel = true;
            }
            TheWorkshop.UpdateLeftBars();
        }

        private void ToggleEntrySynbology(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowSymbology == true)
            {
                LibraryGES.ShowSymbology = false;
            }
            else if (LibraryGES.ShowSymbology == false)
            {
                LibraryGES.ShowSymbology = true;
            }

            {   //This is the Entry ID toggle. I'm merging it into the symbology toggle because it makes sense to have them together.
                if (LibraryGES.ShowEntryAddress == true)
                {
                    LibraryGES.ShowEntryAddress = false;
                }
                else if (LibraryGES.ShowEntryAddress == false)
                {
                    LibraryGES.ShowEntryAddress = true;
                }
            }
            TheWorkshop.UpdateEntryDecorations();
        }

        private void EntryAddressToggle(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowEntryAddress == true)
            {
                LibraryGES.ShowEntryAddress = false;
            }
            else if (LibraryGES.ShowEntryAddress == false)
            {
                LibraryGES.ShowEntryAddress = true;
            }
            TheWorkshop.UpdateEntryDecorations();
        }

        private void EntryAddressType(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.EntryAddressType == "Decimal")
            {
                LibraryGES.EntryAddressType = "Hex";
                EntryAddressTypeButton.Content = "Hex";
            }
            else if (LibraryGES.EntryAddressType == "Hex")
            {
                LibraryGES.EntryAddressType = "Decimal";
                EntryAddressTypeButton.Content = "Dec";
            }
            else
            {
                LibraryGES.EntryAddressType = "Decimal";
                EntryAddressTypeButton.Content = "Dec";
            }
            

            foreach (Editor editor in MyDatabase.GameEditors.Values)  //Set color
            {
                if (editor.EditorType == "DataTable")
                {
                    if (LibraryGES.EntryAddressType == "Decimal")
                    {
                        editor.StandardEditorData.TheXaml.EntryAddressTypeButton.Content = "Dec";
                    }
                    else if (LibraryGES.EntryAddressType == "Hex")
                    {
                        editor.StandardEditorData.TheXaml.EntryAddressTypeButton.Content = "Hex";
                    }
                }
            }

            TheWorkshop.UpdateEntryDecorations();
        }

        private void EntryOffsetTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TheWorkshop.UpdateEntryDecorations();
            }
        }

        private void ToggleHiddenEntrys(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.ShowHiddenEntrys == true)
            {
                LibraryGES.ShowHiddenEntrys = false;
            }
            else if (LibraryGES.ShowHiddenEntrys == false)
            {
                LibraryGES.ShowHiddenEntrys = true;
            }
            TheWorkshop.UpdateEntryDecorations();

            foreach (Editor editor in MyDatabase.GameEditors.Values)  //Set color
            {
                if (editor.EditorType == "DataTable") 
                {
                    if (LibraryGES.ShowHiddenEntrys == true)
                    {
                        editor.StandardEditorData.TheXaml.EntryHiddenToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC1E40"));
                    }
                    else if (LibraryGES.ShowHiddenEntrys == false)
                    {
                        editor.StandardEditorData.TheXaml.EntryHiddenToggle.Foreground = Brushes.Gray;
                    }                    
                }
            }

            if (TheWorkshop.IsPreviewMode == true)
            {
                PixelWPF.LibraryPixel.Notification("Heyo~", "I didn't bother programming preview mode to " +
                "properly hide entrys / bytes that are in use by text. I will figure it out some other time. " +
                "Just be aware the workshop owner probably intends some of the entrys to be hidden. Sorry~");
            }
        }

        private void ToggleDebugShowALL(object sender, RoutedEventArgs e)
        {
            if (LibraryGES.DebugShowALL == true)
            {
                LibraryGES.DebugShowALL = false;
            }
            else if (LibraryGES.DebugShowALL == false)
            {
                LibraryGES.DebugShowALL = true;
            }

            foreach (Editor editor in MyDatabase.GameEditors.Values)  //Set color
            {
                if (editor.EditorType == "DataTable")
                {
                    if (LibraryGES.DebugShowALL == true)
                    {
                        editor.StandardEditorData.TheXaml.DebugShowALL.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E70EC"));
                    }
                    else if (LibraryGES.DebugShowALL == false)
                    {
                        editor.StandardEditorData.TheXaml.DebugShowALL.Foreground = Brushes.Gray;
                    }
                }
            }

            TheWorkshop.UpdateEntryDecorations();
        }
    }
}
