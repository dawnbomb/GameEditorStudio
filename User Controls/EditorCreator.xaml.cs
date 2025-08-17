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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for UserControlEditorCreator.xaml
    /// </summary>
    public partial class UserControlEditorCreator : UserControl
    {
        Workshop TheWorkshop { get; set; }
        public UserControlEditorCreator()
        {
            InitializeComponent();

            #if DEBUG
            #else
            DataTableDebugButton.Visibility = Visibility.Collapsed; //This is a debug button, it shows the data table for the editor creation process. It is not needed in the final product.
            #endif

            this.Loaded += new RoutedEventHandler(LoadEvent);
            
        }

        public void LoadEvent(object sender, RoutedEventArgs e) 
        {
            var parentWindow = Window.GetWindow(this);

            if (parentWindow is Workshop workshopWindow)
            {                
                TheWorkshop = workshopWindow;

            }

            //TabEditorType.Visibility = Visibility.Collapsed;
            //StandardWidthPart2.Visibility = Visibility.Collapsed;
            //StandardWidthPart3.Visibility = Visibility.Collapsed;
            //StandardWidthPartF.Visibility = Visibility.Collapsed;

            
        }       
                

        public event EventHandler RequestClose;
        private void CancelEditorCreation(object sender, RoutedEventArgs e)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        //PopulateFileTree(FileTreeDataTable);





        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////   EDITOR TAB   //////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void GoToStandardWidth(object sender, RoutedEventArgs e)
        {
            {
                if (TextboxEditorName.Text == "")
                {
                    LabelErrorNotice.Content = "Please set a editor name";
                    LabelErrorNotice.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E0101"));
                    return;
                }
                //if (DemoEditorImage.Source == null)
                //{
                //    LabelErrorNotice.Content = "Please select a editor icon! D:<";
                //    LabelErrorNotice.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E0101"));
                //    return;

                //}
                LabelErrorNotice.Content = "";
                LabelErrorNotice.Background = Brushes.Transparent;
            }
            

            foreach (TabItem tabItem in TabMaker.Items)
            {
                if (tabItem.Name == "TabTextSource")
                {
                    TabMaker.SelectedItem = tabItem;
                    break;
                }
            }
        }

        private void GoToTextEditor(object sender, RoutedEventArgs e)
        {
            {
                if (TextboxEditorName.Text == "")
                {
                    LabelErrorNotice.Content = "Please set a editor name";
                    LabelErrorNotice.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E0101"));
                    return;
                }
                //if (DemoEditorImage.Source == null)
                //{
                //    LabelErrorNotice.Content = "Please select a editor icon! D:<";
                //    LabelErrorNotice.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E0101"));
                //    return;

                //}
                LabelErrorNotice.Content = "";
                LabelErrorNotice.Background = Brushes.Transparent;
            }

            StartNewTextEditor();
        }

        private void CheckForMissingInfo() 
        {
            
        }

        /////////////////////////////////////////////////////////      Name and File              ///////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////      Width               ///////////////////////////////////////////////////////////
        private void ButtonBackToNameAndFile(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tabItem in TabMaker.Items)
            {
                if (tabItem.Name == "TabTextSource")
                {    
                    TabMaker.SelectedItem = tabItem;
                    break;
                }
            }
        }
        private void ButtonNextToNameList(object sender, RoutedEventArgs e)
        {
            TextBoxDataTableBaseAddress.Background = null;
            TextBoxDataTableRowSize.Background = null;

            if (TextBoxDataTableBaseAddress.Text == null || TextBoxDataTableBaseAddress.Text == "")
            {
                TextBoxDataTableBaseAddress.Background = Brushes.Red;
            }
            if (TextBoxDataTableRowSize.Text == null || TextBoxDataTableRowSize.Text == "" || TextBoxDataTableRowSize.Text == "0")
            {
                TextBoxDataTableRowSize.Background = Brushes.Red;
            }
            if (TextBoxDataTableRowSize.Text.Contains("a") || TextBoxDataTableRowSize.Text.Contains("s") || TextBoxDataTableRowSize.Text.Contains("d") || TextBoxDataTableRowSize.Text.Contains("w"))
            {
                TextBoxDataTableRowSize.Background = Brushes.Red;
            }
            if (TextBoxDataTableBaseAddress.Background == Brushes.Red || TextBoxDataTableRowSize.Background == Brushes.Red)
            {
                return;
            }

            if (FileManager.TreeGameFiles.SelectedItem == null)
            {
                //LabelErrorNotice.Content = "Please select a file to make an editor with.";
                //error = true;
                return;
            }

            foreach (TabItem tabItem in TabMaker.Items)
            {               
                if (tabItem.Name == "StandardWidthPartF")
                {
                    TabMaker.SelectedItem = tabItem;
                    break; 
                }
            }

            //PopulateFileTree(PartNameTableTreeviewFiles);
        }
        /////////////////////////////////////////////////////////      Width              ///////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////      Item Names              ///////////////////////////////////////////////////////////
        public string Names = ""; //actually used, it's pulled in setupSWeditor (new)
        private void ButtonBackToWidth(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tabItem in TabMaker.Items)
            {
                if (tabItem.Name == "TabEditorType")
                {
                    TabMaker.SelectedItem = tabItem;
                    break;
                }
            }
        }
        
        private void ButtonNextToFinalFILE(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tabItem in TextSourceManager.TabControlListType.Items)
            {
                if (tabItem.IsSelected == true)
                {   
                    Names = tabItem.Tag as string;                    
                    break;
                }
            }

            if (Names == null || Names == "") { return; } //failsafe

            foreach (TabItem tabItem in TabMaker.Items)
            {
                if (tabItem.Name == "StandardWidthPart2")
                {
                    TabMaker.SelectedItem = tabItem;
                    break;
                }
            }
            

            
        }
        
        /////////////////////////////////////////////////////////      Item Names              ///////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////      FINAL              ///////////////////////////////////////////////////////////
        private void ButtonBackToNameList(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tabItem in TabMaker.Items)
            {
                if (tabItem.Name == "StandardWidthPart2")
                {
                    TabMaker.SelectedItem = tabItem;
                    break;
                }
            }
        }
        private void ButtonCreateStandardWidthEditor(object sender, RoutedEventArgs e)
        {    

            NewSWEditorData(); //This is actually in the Load Database file.
            //It works by loading an editors worth of information into the database, then triggering the standard make an editor stuff.


            //When making a new editor, i forcibly turn on symbology.
            LibraryGES.ShowSymbology = true;
            LibraryGES.ShowEntryAddress = true; //i now also force on E-IDs
            TheWorkshop.UpdateEntryDecorations();
            
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////    FINAL   //////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        




        public void NewSWEditorData() //This triggers when the user creates a new editor.
        {

            LoadStandardEditor EditorMaker = new LoadStandardEditor();
            EditorMaker.NewStandardEditorIntoDatabase(TheWorkshop, this);

            RequestClose?.Invoke(this, EventArgs.Empty);            


        }


        public void StartNewTextEditor()
        {
            WorkshopData Database = TheWorkshop.WorkshopData;

            LoadTextEditor EditorMaker = new LoadTextEditor();
            EditorMaker.NewTextEditorIntoDatabase(Database, this);


            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void EditorNameTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            //LabelDemoEditorName.Content = TextboxEditorName.Text;
            //if (TextboxEditorName.Text == "") 
            //{
            //    LabelDemoEditorName.Content = "Name";
            //}
        }

        private void DebugButtonClick(object sender, RoutedEventArgs e)
        {
            TextBoxDataTableBaseAddress.Text = "8";
            TextBoxDataTableRowSize.Text = "176";

            foreach (TreeViewItem Item3 in FileManager.TreeGameFiles.Items) //FileTreeExtraTable.Items
            {
                GameFile TheFile = Item3.Tag as GameFile;
                if (TheFile.FileName == "skill.bin")
                {
                    Item3.IsSelected = true;
                    break;
                }
            }
        }
    }











    
}
