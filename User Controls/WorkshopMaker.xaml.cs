using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Windows.Foundation.Metadata;
using Path = System.IO.Path;

namespace GameEditorStudio
{
    /// This is for the workshop data editing inside the game library. its not a part of a workshop itself!
    public partial class WorkshopMaker : UserControl
    {
        GameLibrary Library { get; set; }
        string TheMode { get; set; }
        WorkshopData workshopData { get; set; }

        public WorkshopMaker(string TheModee, WorkshopData workshopData2)
        {
            InitializeComponent();

            TheMode = TheModee;
            workshopData = workshopData2;
            this.Loaded += new RoutedEventHandler(LoadEvent);

            
        }

        public void LoadEvent(object sender, RoutedEventArgs e) 
        {
            Library = Database.GameLibrary;

            //var parentWindow = Window.GetWindow(this);
            //if (parentWindow is GameLibrary GameLibraryWindow)
            //{
            //    Library = GameLibraryWindow;
            //}

            if (TheMode == "New")
            {
                ButtonCreateNewWorkshop.Content = "Create Workshop";
                WorkshopTextboxExampleInputFolder.Text = "";
                WorkshopCheckboxSameFolderName.IsChecked = true;
            }

            if (TheMode == "Edit")
            {
                ButtonCreateNewWorkshop.Content = "Save Workshop";
                TextBoxGameName.Text = workshopData.WorkshopName;
                WorkshopTextboxExampleInputFolder.Text = workshopData.WorkshopInputDirectory; //System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Input Directory.txt");
                WorkshopCheckboxSameFolderName.IsChecked = workshopData.ProjectsRequireSameFolderName;


                //ResourcePanel.Children.Clear();
                //foreach (WorkshopResource EventResource in workshopData.WorkshopEventResources) 
                //{
                //    GenerateWEventResourceUI(EventResource); 
                //}
            }



        }

        private void ButtonSetWorkshopInputFolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Select the root / base folder. This folder should contain all game files. For example, a rom's unpacked folder, or game install folder."; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.  
            if ((bool)FolderSelect.ShowDialog(Window.GetWindow(Library))) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                WorkshopTextboxExampleInputFolder.Text = Path.GetFileName(FolderSelect.SelectedPath);
            }
        }

        private void ButtonCreateNewWorkshop_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxGameName.Text == null && TextBoxGameName.Text == "" && WorkshopTextboxExampleInputFolder.Text == null && WorkshopTextboxExampleInputFolder.Text == "")
            {
                PixelWPF.LibraryPixel.NotificationNegative("Warning: Something is Missing!",
                    "Either the workshop name is empty, or the Input Folder is blank."
                );
                return;
            }
            

            if (ButtonCreateNewWorkshop.Content.ToString() == "Create Workshop")
            {   
                Database.Workshops.Add(workshopData);

                workshopData.CreatedDate = DateTime.Now.ToString("MMM dd yyyy");
                workshopData.CreatedVersion = LibraryGES.VersionNumber;

                //These can't be placed outside this IF or the later move command in save workshop will fail because folder already exists.
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text);
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents");
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Editors");
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Tools");
                System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents\\LoadOrder.txt", " "); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                

                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents\\READ ME");
                System.IO.File.WriteAllText(LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\Documents\\READ ME\\Text.txt", "" +
                    "The READ ME is always the first loaded document, so fill it out with useful info! " +
                    "Here are some examples :3" +
                    "\n- How to extract the game files (If required)." +
                    "\n- What tools are needed (If any). " +
                    "\n- What folder is the input folder, and where it's located." +
                    "\n- What game platform? (PC, Switch, PS2, etc)" +
                    "\n- What game region? (USA, JP, EU, etc)" +
                    "\n- What game patch number? (patch v1.1, expansion name, etc)" +
                    "\n- What discord communities, forumns, wikis, exist?" +
                    ""
                );

                
            }
            if (ButtonCreateNewWorkshop.Content.ToString() == "Save Workshop")
            {
                string OldWorkshopName = LibraryGES.ApplicationLocation + "\\Workshops\\" + Library.SelectedWorkshop.WorkshopName;
                string NewWorkshopName = LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text;
                string OldProjectsName = LibraryGES.ApplicationLocation + "\\Projects\\" + Library.SelectedWorkshop.WorkshopName;
                string NewProjectsName = LibraryGES.ApplicationLocation + "\\Projects\\" + TextBoxGameName.Text;
                try
                {
                    Directory.Move(OldWorkshopName, NewWorkshopName);
                    Directory.Move(OldProjectsName, NewProjectsName);
                }
                catch (IOException exp)
                {
                    Console.WriteLine(exp.Message);
                }                                

            }

            workshopData.WorkshopName = TextBoxGameName.Text;
            workshopData.WorkshopInputDirectory = WorkshopTextboxExampleInputFolder.Text;
            workshopData.ProjectsRequireSameFolderName = WorkshopCheckboxSameFolderName.IsChecked == true ? true : false;

            CommandMethodsClass.SaveWorkshopXml(workshopData);
            Library.RefreshWorkshopTree();

            //Exit this and reselect the workshop in the library.
            var parentContainer = this.Parent as Grid;
            if (parentContainer != null)
            {
                if (Library.ProjectsSelector.ItemsSource != null) 
                {
                    CollectionViewSource.GetDefaultView(Library.ProjectsSelector.ItemsSource).Refresh();
                }
               
                foreach (TreeViewItem item in Library.LibraryTreeOfWorkshops.Items)
                {
                    if (item.Tag as WorkshopData == workshopData) 
                    { 
                        item.IsSelected = true; 
                    }
                }

                parentContainer.Children.Remove(this);
            }


        }



        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            var parentContainer = this.Parent as Grid;
            if (parentContainer != null)
            {
                parentContainer.Children.Remove(this);
            }
        }

        


        
    }
}
