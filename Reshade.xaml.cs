using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using IWshRuntimeLibrary;
using File = System.IO.File; //needed for shortcut links

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for Reshade.xaml
    /// </summary>
    public partial class Reshade : Window
    {
        public Reshade()
        {
            InitializeComponent();
        }


        public void ReshadePopup() //Trigger the window.
        {
            //if () 
            {
                //ReshadeInstall();
            }
        }

        private void ButtonInstallReshade(object sender, RoutedEventArgs e)
        {
            ReshadeInstall();
        }

        private void ButtonUpdateReshadeFolder(object sender, RoutedEventArgs e)
        {
            SetupReshadeDocumentsFolder();
            PixelWPF.LibraryPixel.Notification("ReShade Folder Updated!", "The reshade folder in your Documents was updated to the latest / setup to begin with. :)");
        }

        public void ReshadeInstall() //Start actual reshade install
        {
            //Becuase reshade updates change the name of the setup exe, i search for every program with ReShade_Setup prefix, and run the first one i find.
            string exeDirectory = Path.Combine(LibraryGES.ApplicationLocation, "Other\\Reshade");
            string[] files = Directory.GetFiles(exeDirectory, "ReShade_Setup*.exe");
            if (files.Length > 0)
            {
                string GamePath = "";

                OpenFileDialog openFileDialog = new();
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {                    
                    GamePath = openFileDialog.FileName;
                }

                if (GamePath == "") { return; }

                string GameFolder = LibraryGES.GetFolderFromFilepath(GamePath);
                NukeShadersAndPresetsInGameFolder(GameFolder);
                SetupReshadeDocumentsFolder();
                EditReshadeIni(GameFolder);

                string ReshadeExe = files[0];
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ReshadeExe,
                        Arguments = $"\"{GamePath}\"", // Pass game EXE path as an argument, wrapped in quotes in case of spaces
                        UseShellExecute = true // Needed if launching .exe directly (not using redirected IO)
                    },
                    EnableRaisingEvents = true
                };

                // Hook up event handler
                process.Exited += (sender, args) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AfterReshadeSetup(GamePath);
                        
                    });
                };

                process.Start();
            }
            
        }

        private void AfterReshadeSetup(string GamePath) 
        {               
            string GameFolder = LibraryGES.GetFolderFromFilepath(GamePath);

            string reshadeIniPath = Path.Combine(GameFolder, "ReShade.ini");
            if (File.Exists(reshadeIniPath)) //IF reshade is actually installed to the game. 
            {                
                CleanupGameFolder(GameFolder);
                PixelWPF.LibraryPixel.Notification("ReShade has been INSTALLED!!!", "Normally reshade creates some folders inside your game's exe folder. Instead of that, i have setup a Reshade folder in your Documents folder, and it will be your new main location for all things Reshade going forward. All games you install reshade to will load presets from there, and all screenshots are saved there. \n\nAlso a shortcut to this new location was created in your game's exe folder, so you can access reshade stuff like you normally would. :) Also HAVE FUUUN!!! :D");

                this.Close();
            }
            
            
        }

        //I should copy the README to the Documents Reshade folder, or actually, include it inside the folder to begin with so it copys over automatically...yeah that makes sense. 



        public void SetupReshadeDocumentsFolder()
        {
            string sourceFolder = Path.Combine(LibraryGES.ApplicationLocation, "Other\\Reshade\\Reshade");
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string destinationFolder = Path.Combine(documentsFolder, "Reshade");

            try
            {
                // Create destination root
                Directory.CreateDirectory(destinationFolder);

                // Create all subdirectories, even if they're empty
                foreach (string dir in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    string relativePath = dir.Substring(sourceFolder.Length + 1);
                    string destinationDir = Path.Combine(destinationFolder, relativePath);

                    if (!Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }
                }

                // Copy all files (if not already present)
                foreach (string file in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    string relativePath = file.Substring(sourceFolder.Length + 1);
                    string destinationPath = Path.Combine(destinationFolder, relativePath);

                    string destinationDir = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    if (!File.Exists(destinationPath))
                    {
                        File.Copy(file, destinationPath);
                    }
                }
                
            }
            catch (Exception ex)
            {
                PixelWPF.LibraryPixel.NotificationNegative("Error!", "Failed to copy ReShade folder.\n\nThis really should not happen, so, uh, IDK, oh fuck?" + ex.Message);
            }
        }
        //Reminder to delete any duplicate shaders!


        public void CleanupGameFolder(string GameFolder)
        {
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            {   //Copy over any new shaders / textures from later reshade updates to the master Reshade folder.
                string shadersFolder = Path.Combine(GameFolder, "reshade-shaders");                

                string FromShadersFolder = Path.Combine(shadersFolder, "Shaders");
                string FromTexturesFolder = Path.Combine(shadersFolder, "Textures");

                string ToShadersFolder = Path.Combine(documentsFolder, "Reshade\\Reshade Shaders");
                string ToTexturesFolder = Path.Combine(documentsFolder, "Reshade\\Reshade Textures");

                CopyDirectory(FromShadersFolder, ToShadersFolder);
                CopyDirectory(FromTexturesFolder, ToTexturesFolder);
            }
            

            NukeShadersAndPresetsInGameFolder(GameFolder);

            //Edit the ReShade.ini
            EditReshadeIni(GameFolder);


            

            //Create a windows shortcut to Documents\Reshade and put it in GameFolder.
            string reshadeTarget = Path.Combine(documentsFolder, "Reshade");
            string shortcutPath = Path.Combine(GameFolder, "Reshade Folder.lnk");
            CreateShortcut(shortcutPath, reshadeTarget, "Open the Reshade documents folder.");

        }

        private void NukeShadersAndPresetsInGameFolder(string GameFolder) 
        {
            string presetsFolder = Path.Combine(GameFolder, "reshade-presets");
            string shadersFolder = Path.Combine(GameFolder, "reshade-shaders");
            string presetIniFile = Path.Combine(GameFolder, "ReShadePreset.ini");

            if (Directory.Exists(presetsFolder))
            {
                Directory.Delete(presetsFolder, recursive: true);
            }
            if (Directory.Exists(shadersFolder))
            {   
                Directory.Delete(shadersFolder, recursive: true);
            }
            if (File.Exists(presetIniFile)) //Delete the default blank preset ReShadePreset.ini that gets put in the GameFolder
            {
                File.Delete(presetIniFile);
            }
        }



        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }




        private void EditReshadeIni(string GameFolder) 
        {
            string iniPath = Path.Combine(GameFolder, "reshade.ini");
            if (!File.Exists(iniPath))
                return;

            string userDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string presetPath = Path.Combine(userDocs, "Reshade", "Reshade Presets", "Color Splash for Normal 3D Games.ini");
            string shadersPath = Path.Combine(userDocs, "Reshade", "Reshade Shaders") + "\\**";
            string texturesPath = Path.Combine(userDocs, "Reshade", "Reshade Textures") + "\\**";
            string screenshotPath = Path.Combine(userDocs, "Reshade", "Reshade Screenshots");

            Dictionary<string, string> inputKeys = new()
    {
        { "KeyEffects", "45,0,0,0" },
        { "KeyNextPreset", "34,0,0,0" },
        { "KeyPreviousPreset", "33,0,0,0" },
        { "KeyReload", "35,0,0,0" },
        { "KeyScreenshot", "44,0,0,0" },
    };

            Dictionary<string, string> generalSettings = new()
    {
        { "PresetPath", presetPath },
        { "EffectSearchPaths", shadersPath },
        { "TextureSearchPaths", texturesPath },
        { "PresetTransitionDuration", "0" }
    };

            string screenshotKey = "SavePath";

            List<string> originalLines = File.ReadAllLines(iniPath).ToList();
            List<string> outputLines = new();
            string currentSection = "";
            HashSet<string> insertedKeys = new();

            foreach (string rawLine in originalLines)
            {
                string trimmed = rawLine.Trim();

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Trim('[', ']');
                    outputLines.Add(rawLine);
                    continue;
                }

                // Skip misplaced keys
                if (generalSettings.Keys.Any(k => trimmed.StartsWith(k + "=")) ||
                    trimmed.StartsWith(screenshotKey + "="))
                {
                    continue;
                }

                // Replace input keys in [INPUT]
                if (currentSection == "INPUT")
                {
                    bool matchedKey = false;
                    foreach (var kvp in inputKeys)
                    {
                        if (trimmed.StartsWith(kvp.Key + "="))
                        {
                            outputLines.Add($"{kvp.Key}={kvp.Value}");
                            insertedKeys.Add(kvp.Key);
                            matchedKey = true;
                            break;
                        }
                    }

                    if (matchedKey)
                        continue;
                }

                outputLines.Add(rawLine);
            }

            // Helper to insert or update a key in a given section
            void EnsureKey(string section, string key, string value)
            {
                int sectionIndex = outputLines.FindIndex(line => line.Trim() == $"[{section}]");
                if (sectionIndex == -1)
                {
                    outputLines.Add("");
                    outputLines.Add($"[{section}]");
                    outputLines.Add($"{key}={value}");
                }
                else
                {
                    int insertIndex = sectionIndex + 1;
                    bool replaced = false;

                    while (insertIndex < outputLines.Count && !outputLines[insertIndex].TrimStart().StartsWith("["))
                    {
                        if (outputLines[insertIndex].TrimStart().StartsWith(key + "="))
                        {
                            outputLines[insertIndex] = $"{key}={value}";
                            replaced = true;
                            break;
                        }
                        insertIndex++;
                    }

                    if (!replaced)
                        outputLines.Insert(insertIndex, $"{key}={value}");
                }
            }

            foreach (var kvp in generalSettings)
                EnsureKey("GENERAL", kvp.Key, kvp.Value);

            EnsureKey("SCREENSHOT", screenshotKey, screenshotPath);

            foreach (var kvp in inputKeys)
            {
                if (!insertedKeys.Contains(kvp.Key))
                    EnsureKey("INPUT", kvp.Key, kvp.Value);
            }

            File.WriteAllLines(iniPath, outputLines);
        }








        private void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(sourceDir))
                return;

            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string description = "")
        {
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.Description = description;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.Save();
        }

    }
}
