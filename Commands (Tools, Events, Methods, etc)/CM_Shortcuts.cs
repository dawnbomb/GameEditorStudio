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
                    LibraryGES.OpenFolder(project.ProjectInputDirectory);
                }
                else
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Project Input Folder not found.",
                        "This is actually a pretty serious error, so you should probably look into fixing it." 
                        );
                    //Note that if your looking at the workshop in preview mode, you will always get this error and can ignore it.
                    //^ I removed that line because really, it should be my job to make sure thats not even possible to begin with. 

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
                    LibraryGES.OpenFolder(project.ProjectOutputDirectory);
                }
                else
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Project Output Folder not found.",
                        "Your projects output folder doesn't seem to exist! :(" +
                        "\n\n" +
                        "If you didn't set one up, saving is SUPPOSED to default to your input folder instead."
                        );
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
                    folderPath = LibraryGES.ApplicationLocation + "\\Workshops\\" + MethodData.WorkshopData.Workshop.WorkshopName + "\\";
                }
                else if (MethodData.GameLibrary != null) //If Library
                {
                    folderPath = LibraryGES.ApplicationLocation + "\\Workshops\\" + MethodData.GameLibrary.WorkshopName + "\\";
                }

                

                if (Directory.Exists(folderPath))
                {
                    LibraryGES.OpenFolder(folderPath);
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
                    LibraryGES.OpenFolder(folderPath);
                }
                else
                {
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Can't find downloads folder.",
                        "We can't find where your downloads folder is! :( " +
                        "IDK what would cause this error. Maybe your on mac or linux PC instead of windows? " +
                        "If anyone gets this error on windows, please report it."
                        );                    
                }
            }
            catch
            {
                PixelWPF.LibraryPixel.NotificationGenericError();             
            }

        }

        public static void OpenCrystalEditorFolder(MethodData MethodData)
        {
            //this uses literally nothing from the action pack, LOL.

            try
            {
                string folderPath = LibraryGES.ApplicationLocation;

                // Open the folder in the file explorer
                if (Directory.Exists(folderPath))
                {
                    LibraryGES.OpenFolder(folderPath);
                }
                else
                {
                    System.Windows.MessageBox.Show("Crystal editor folder wasn't found. This should literally never happen, please report this :(" +
                        "\n.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                }
            }
            catch
            {
                PixelWPF.LibraryPixel.NotificationGenericError();
            }

        }
    }
}
