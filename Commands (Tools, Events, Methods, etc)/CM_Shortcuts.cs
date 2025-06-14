using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GameEditorStudio
{
    public partial class CommandMethodsClass //This file contains shortcut actions, they open folders on a users PC. They are always accessable via the shortcuts menu.
    {
        

        public static void OpenInputFolder(MethodData MethodData)
        {
            ProjectDataItem project = null;

            if (MethodData.WorkshopData != null) //If Workshop  //Note this is first so the existance of a workshop gets priority!!!
            {
                project = MethodData.WorkshopData.Workshop.ProjectDataItem;
            }
            else if (MethodData.GameLibrary != null) //If Library
            {
                project = MethodData.GameLibrary.ProjectsSelector.SelectedItem as ProjectDataItem;
            }

            
            if ( project == null) { return; }

            try
            {
                if (Directory.Exists(project.ProjectInputDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", project.ProjectInputDirectory);
                }
                else
                {
                    System.Windows.MessageBox.Show("We can't find where your projects input folder is! :(" +
                        "\n" +
                        "\n(This is actually a pretty serious error, so you should probably look into fixing it." +
                        "\n" +
                        "\nIf your looking at the workshop in preview mode, you will always get this error and can ignore it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch
            {

            }


        }



        public static void OpenOutputFolder(MethodData MethodData)
        {
            ProjectDataItem project = null;

            if (MethodData.WorkshopData != null) //If Workshop   //Note this is first so the existance of a workshop gets priority!!!
            {
                project = MethodData.WorkshopData.Workshop.ProjectDataItem;
            }
            else if (MethodData.GameLibrary != null) //If Library
            {
                project = MethodData.GameLibrary.ProjectsSelector.SelectedItem as ProjectDataItem;
            }
            if (project == null) { return; }

            try
            {

                if (Directory.Exists(project.ProjectOutputDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", project.ProjectOutputDirectory);
                }
                else
                {
                    System.Windows.MessageBox.Show("Your projects output folder doesn't seem to exist! :(" +
                        "\n" +
                        "\nIf you didn't set one up, saving default to your input folder instead." +
                        "\n" +
                        "\nIf your looking at the workshop in preview mode, you will always get this error and can ignore it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {

            }

        }




        public static void OpenWorkshopFolder(MethodData MethodData)
        {
            
            try
            {
                string folderPath = "";

                if (MethodData.WorkshopData != null) //If Workshop
                {
                    folderPath = LibraryMan.ApplicationLocation + "\\Workshops\\" + MethodData.WorkshopData.Workshop.WorkshopName + "\\";
                }
                else if (MethodData.GameLibrary != null) //If Library
                {
                    folderPath = LibraryMan.ApplicationLocation + "\\Workshops\\" + MethodData.GameLibrary.WorkshopName + "\\";
                }

                

                if (Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    System.Windows.MessageBox.Show("We can't find where your workshop folder is! :(" +
                        "\n" +
                        "\n(I actually have no idea how anyone could even get this error, " +
                        "\nexcept maybe moving your folders around while the program is still running, but i think windows would stop you?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {

            }

        }



        public static void OpenDownloadsFolder(MethodData MethodData)
        {
            //this uses literally nothing from the action pack, LOL.

            try
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                // Open the folder in the file explorer
                if (Directory.Exists(folderPath))
                {
                    System.Windows.MessageBox.Show("We can't find where your downloads folder is! :(" +
                        "\n" +
                        "\nIDK what would cause this error. Maybe your on a max or linux PC instead of windows?" +
                        "\nIf anyone gets this error on windows, please report it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    System.Windows.MessageBox.Show("Crystal editor folder wasn't found. This should literally never happen, please report this :(" +
                        "\n.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                //PropertiesEditorNameTableRowSize.Text = EditorClass.NameTableRowSize.ToString();
                string Error = "An error occured." +
                    "\n" +
                    "\nPlease report it :(";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }

        }

        public static void OpenCrystalEditorFolder(MethodData MethodData)
        {
            //this uses literally nothing from the action pack, LOL.

            try
            {
                string folderPath = LibraryMan.ApplicationLocation;

                // Open the folder in the file explorer
                if (Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    System.Windows.MessageBox.Show("Crystal editor folder wasn't found. This should literally never happen, please report this :(" +
                        "\n.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                }
            }
            catch
            {
                //PropertiesEditorNameTableRowSize.Text = EditorClass.NameTableRowSize.ToString();
                string Error = "An error occured." +
                    "\n" +
                    "\nPlease report it :(";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }

        }
    }
}
