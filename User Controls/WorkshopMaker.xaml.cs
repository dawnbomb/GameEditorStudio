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
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is GameLibrary GameLibraryWindow)
            {
                Library = GameLibraryWindow;
            }

            if (TheMode == "New" ) 
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
            if ((bool)FolderSelect.ShowDialog(Library)) //This triggers the folder selection screen, and if the user does not cancel out...
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

            SaveWorkshopLibrary();

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



        private void SaveWorkshopLibrary()
        {          
            //Save a test example. If this fails, the real file is not corrupted. 
            string LibraryXmlPath = LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\" + "LibraryTestSave.xml";               
            SaveIt();
            try
            {
                if (File.Exists(LibraryXmlPath)) { File.Delete(LibraryXmlPath); }
            } 
            catch 
            {
            
            }

            //Save over the real workshop file. The test didn't fail, so this should be fine.
            LibraryXmlPath = LibraryGES.ApplicationLocation + "\\Workshops\\" + TextBoxGameName.Text + "\\" + "Workshop.xml";  
            SaveIt();

            void SaveIt()
            {
                try 
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryXmlPath, settings))
                    {
                        writer.WriteStartElement("Workshop");
                        writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
                        writer.WriteElementString("WorkshopName", workshopData.WorkshopName); //Note: This is for reference only, so i can tell what workshop a file is for when it's open in notepad. This isn't actually used anywhere. 
                        writer.WriteElementString("InputLocation", workshopData.WorkshopInputDirectory);
                        writer.WriteElementString("ProjectsRequireSameInputFolderName", workshopData.ProjectsRequireSameFolderName == true ? "true" : "false");

                        writer.WriteStartElement("ResourceList");
                        foreach (EventResource WorkshopEventResource in workshopData.WorkshopEventResources)
                        {
                            //when xml loads, variables WILL be null, even if they have a default value, if it's not written to begin with. this is very annoying. 
                            writer.WriteStartElement("Resource");
                            writer.WriteElementString("Name", WorkshopEventResource.Name);
                            writer.WriteElementString("Key", WorkshopEventResource.Key);

                            if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.File)
                            {
                                writer.WriteElementString("ResourceType", "File");
                            }
                            if (WorkshopEventResource.ResourceType == EventResource.ResourceTypes.Folder)
                            {
                                writer.WriteElementString("ResourceType", "Folder");
                            }
                            if (WorkshopEventResource.IsChild == false)
                            {
                                writer.WriteElementString("IsChild", "False");
                            }
                            if (WorkshopEventResource.IsChild == true)
                            {
                                writer.WriteElementString("IsChild", "True");
                            }

                            writer.WriteElementString("RequiredName", WorkshopEventResource.RequiredName.ToString());  //if full path (local)
                            writer.WriteElementString("Location", WorkshopEventResource.Location);  //if partial path
                            writer.WriteElementString("TargetKey", WorkshopEventResource.ParentKey); //if partial path                    


                            writer.WriteEndElement(); //End File
                        }
                        writer.WriteEndElement(); //End ResourceList 


                        writer.WriteEndElement(); //End Root (Library)
                        writer.Flush(); //Ends the XML Library file                                

                    }
                }
                catch 
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Workshop.xml failed to save properly.",
                        "All saves (are supposed to be) simulated in this program, so pre-existing data should be fine... " +
                        "but...this is really weird! This one especially should never crash! What the hell did you do?!?" +
                        "\n\n" +
                        "You should DEFINATLY restart the program."
                        );
                    return;
                }
                
            }
            

            Library.RefreshWorkshopTree();
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
