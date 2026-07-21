using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for Release.xaml
    /// </summary>
    public partial class Release : UserControl
    {
        public Release()
        {
            InitializeComponent();
        }

        private void SetMyGESFolder(object sender, RoutedEventArgs e)
        {            
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog();//This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Set my local Game Editor Studio folder: "; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.
            {   //Smart seleting the folder to start in.
                string inputPath = MyGESfolderTextbox.Text + "\\";
                DirectoryInfo? current = new DirectoryInfo(inputPath);
                while (current != null && !current.Exists)
                {
                    current = current.Parent;
                }
                if (current != null)
                {
                    FolderSelect.SelectedPath = current.FullName + "\\";
                }
            }

            if ((bool)FolderSelect.ShowDialog(Window.GetWindow(this))) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                MyGESfolderTextbox.Text = FolderSelect.SelectedPath;

            }
        }

        private void SetReleaseGESFolder(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog();//This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Set my release folder: "; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.
            {   //Smart seleting the folder to start in.
                string inputPath = ReleaseGESFolderTextbox.Text + "\\";
                DirectoryInfo? current = new DirectoryInfo(inputPath);
                while (current != null && !current.Exists)
                {
                    current = current.Parent;
                }
                if (current != null)
                {
                    FolderSelect.SelectedPath = current.FullName + "\\";
                }
            }

            if ((bool)FolderSelect.ShowDialog(Window.GetWindow(this))) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                ReleaseGESFolderTextbox.Text = FolderSelect.SelectedPath;

            }
        }


        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // Copy all subfolders
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }

        private void DOIT(object sender, RoutedEventArgs e)
        {
            // MyGESfolderTextbox.Text
            // ReleaseGESFolderTextbox.Text



            string GESfolder = MyGESfolderTextbox.Text;
            string ReleaseFolder = ReleaseGESFolderTextbox.Text;
            //NOTE: If release name already exists, cancel!

            ///////////COPYING FROM MY GES FOLDER/////////////////
            if (CopyGESexe.IsChecked == true) 
            {
                string sourceExe = Path.Combine(GESfolder, "Game Editor Studio.exe");
                string destExe = Path.Combine(ReleaseFolder, "Game Editor Studio.exe");

                if (File.Exists(sourceExe))
                {
                    File.Copy(sourceExe, destExe, overwrite: true);
                }
                else
                {
                    MessageBox.Show("Game Editor Studio.exe was not found.");
                }
            }

            if (CopyOtherFolder.IsChecked == true)
            {
                string sourceFolder = Path.Combine(GESfolder, "Other");
                string destFolder = Path.Combine(ReleaseFolder, "Other");

                if (Directory.Exists(sourceFolder))
                {
                    CopyDirectory(sourceFolder, destFolder);
                }
            }

            ///////////IN RELEASE FOLDER/////////////////
            if (DeleteProjectsFromRelease.IsChecked == true)
            {
                string projectsFolder = Path.Combine(ReleaseFolder, "Projects");
                if (Directory.Exists(projectsFolder)) { Directory.Delete(projectsFolder, true); }
            }

            if (DeleteUserSettingsFromRelease.IsChecked == true)
            {
                string settingsFolder = Path.Combine(ReleaseFolder, "Settings");
                if (Directory.Exists(settingsFolder)) { Directory.Delete(settingsFolder, true); }
            }

            if (DeleteToolsFolderFromRelease.IsChecked == true)
            {
                string toolsFolder = Path.Combine(ReleaseFolder, "Tools");
                if (Directory.Exists(toolsFolder))  { Directory.Delete(toolsFolder, true); }
            }

            ///////////IN RELEASE - OTHER FOLDER/////////////////
            string otherFolder = Path.Combine(ReleaseFolder, "Other");

            if (Directory.Exists(otherFolder))
            {
                if (DeleteColorSplash.IsChecked == true)
                {
                    string colorSplashFolder = Path.Combine(otherFolder, "ColorSplash");
                    if (Directory.Exists(colorSplashFolder)) Directory.Delete(colorSplashFolder, true);
                }

                if (DeleteToolImages.IsChecked == true)
                {
                    string toolImagesFolder = Path.Combine(otherFolder, "Tool Images");
                    if (Directory.Exists(toolImagesFolder)) Directory.Delete(toolImagesFolder, true);
                }

                if (DeleteWiki.IsChecked == true)
                {
                    string wikiFolder = Path.Combine(otherFolder, "Wiki");
                    if (Directory.Exists(wikiFolder)) Directory.Delete(wikiFolder, true);
                }
            }

            ///////////EXTRAS/////////////////
            string zippedFilePath = "";
            if (ZipReleaseFolder.IsChecked == true)
            {
                string zipName = "Game Editor Studio v"
                    + LibraryGES.VersionNumber
                    + " "
                    + LibraryGES.VersionDate
                    + ".zip";

                string releaseParentFolder = Directory.GetParent(ReleaseGESFolderTextbox.Text).FullName;

                zippedFilePath = Path.Combine(releaseParentFolder, zipName);

                // Delete old zip if it already exists
                if (File.Exists(zippedFilePath))
                {
                    File.Delete(zippedFilePath);
                }

                ZipFile.CreateFromDirectory(ReleaseGESFolderTextbox.Text, zippedFilePath, CompressionLevel.Optimal, false);
            }

            if (OpenZippedLocation.IsChecked == true && File.Exists(zippedFilePath))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = "/select,\"" + zippedFilePath + "\"",
                    UseShellExecute = true
                });
            }

            //if (OpenGESGithub.IsChecked == true)
            //{

            //}

            //if (Something.IsChecked == true)
            //{

            //}
            
        }

        private void OpenGESGithub(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/dawnbomb/GameEditorStudio";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
    }
}
