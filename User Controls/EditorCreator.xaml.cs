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
using Microsoft.VisualBasic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for UserControlEditorCreator.xaml
    /// </summary>
    public partial class UserControlEditorCreator : UserControl
    {
        public Workshop TheWorkshop { get; set; }
        public string Names { get; set; } = ""; //actually used, it's pulled in setupSWeditor (new)

        public UserControlEditorCreator()
        {
            InitializeComponent();

            #if DEBUG
            #else            
            #endif




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



        private void EditorNameTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            //LabelDemoEditorName.Content = TextboxEditorName.Text;
            //if (TextboxEditorName.Text == "") 
            //{
            //    LabelDemoEditorName.Content = "Name";
            //}
        }




        private void NewDataTableEditor(object sender, RoutedEventArgs e)
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

                //LabelErrorNotice.Content = "";
                //LabelErrorNotice.Background = Brushes.Transparent;
            }


            LoadStandardEditor EditorMaker = new LoadStandardEditor();
            EditorMaker.NewDataTableEditor(TheWorkshop, this);

            RequestClose?.Invoke(this, EventArgs.Empty);


            DTEMethods.UpdateHotbarForAllDTEEditors(TheWorkshop.WorkshopData);
            //foreach (DataTableEditorData DataTableData in TheWorkshop.WorkshopData.GameEditors.OfType<DataTableEditorData>())
            //{
            //    DataTableData.DataTableEditorData.DTEXaml.UpdateEntryDecorationsForAllEditors();
            //    break;
            //}

        }

        private void StartNewTextEditor(object sender, RoutedEventArgs e)
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

            WorkshopData Database = TheWorkshop.WorkshopData;

            LoadTextEditor EditorMaker = new LoadTextEditor();
            EditorMaker.NewTextEditorIntoDatabase(Database, this);

            RequestClose?.Invoke(this, EventArgs.Empty);
        }

                



    }











    
}
