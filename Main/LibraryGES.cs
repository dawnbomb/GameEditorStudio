using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ookii.Dialogs.Wpf;
using Windows.UI.Notifications;

namespace GameEditorStudio
{
    public static class LibraryGES
    {        
        
        public static string VersionDate { get; set; } = "Augest 6 2025";
        public static Version VersionNumber { get; set; } = new Version(0, 1, 5); //Version Numbers (in order) are Major.Minor.Build.Revision
        //Major is big releases.
        //Minor is new features / content.
        //Build is for Bugfixes or small changes.
        //Revision is for code rewrites that dont* affect the user. *SHOULDN'T AFFECT THE USER >:(


        public static bool ShowEntryAddress { get; set; } = false; 
        public static string EntryAddressType { get; set; } = "Decimal";
        //public static bool CollectionPrefix { get; set; } = true;
        public static bool ShowItemIndex { get; set; } = false;
        public static bool ShowHiddenEntrys { get; set; } = false;
        public static bool ShowSymbology { get; set; } = false;
        public static bool ShowTranslationPanel { get; set; } = false;

        public static bool DebugShowALL { get; set; } = false;
        //END

        public static string ApplicationLocation { get; set; } = "";

        public static int TooltipInitialDelay { get; set; } = 200; // 0.3 seconds popup 
        public static int TooltipBetweenDelay { get; set; } = 0; //// 0.0 seconds popup

        public static List<ColorTheme> ColorThemeList { get; set; } = new();
        public static WikiDataBase Wiki { get; set; } = new WikiDataBase();


        static public Color ColorRed { get; set; } = (Color)ColorConverter.ConvertFromString("#FF0000");
        static public Color ColorPink { get; set; } = (Color)ColorConverter.ConvertFromString("#FF0000");
        static public Color ColorBlue { get; set; } = (Color)ColorConverter.ConvertFromString("#FF0000");
        static public Color ColorGreen { get; set; } = (Color)ColorConverter.ConvertFromString("#FF0000");
        static public Color ColorGray { get; set; } = (Color)ColorConverter.ConvertFromString("#FF0000");

        
        static public Color ValueAlways0 { get; set; } = ColorGray;
        static public Color ValueAlwaysSame { get; set; } = ColorGray;
        static public Color ValueLessThanX { get; set; } = ColorGray;
        static public Color ValueDisabled { get; set; } = ColorGray;
        static public Color ValueText { get; set; } = ColorGray;
        static public Color ValueName { get; set; } = ColorGray;


        public static void SwitchToColorTheme(ColorTheme Theme)
        {
                        

            try 
            {
                //Note: This will crash (fail catch) if it's reading color string "".
                //IF the element part does not exist (HasText = false for example) then i need to comment the loading line here!!!
                //If the button says None thats the same as "" and will crash, IE no nones should exist.
                //I could fix this by checking HasText or setting to #000000 if its "" on load, but i'll just leave this problem for another day. 

                Application.Current.Resources["ApplicationText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Application.Text));
                Application.Current.Resources["ApplicationBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Application.Back));
                Application.Current.Resources["ApplicationBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Application.Border));
                Application.Current.Resources["ApplicationOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Application.Other));

                //Application.Current.Resources["ContentAreaText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentArea.Text));
                Application.Current.Resources["ContentAreaBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentArea.Back));
                Application.Current.Resources["ContentAreaBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentArea.Border));
                Application.Current.Resources["ContentAreaOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentArea.Other));

                //Application.Current.Resources["ContentBarText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentBar.Text));
                Application.Current.Resources["ContentBarBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentBar.Back));
                Application.Current.Resources["ContentBarBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentBar.Border));
                //Application.Current.Resources["ContentBarOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ContentBar.Other));

                Application.Current.Resources["TextboxText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Textbox.Text));
                Application.Current.Resources["TextboxBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Textbox.Back));
                Application.Current.Resources["TextboxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Textbox.Border));
                //Application.Current.Resources["TextboxOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Textbox.Other));
                //Application.Current.Resources["TextboxHighlightText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.TextboxHighlight.Text));
                Application.Current.Resources["TextboxHighlightBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.TextboxHighlight.Back));
                //Application.Current.Resources["TextboxHighlightBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.TextboxHighlight.Border));
                //Application.Current.Resources["TextboxHighlightOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.TextboxHighlight.Other));

                Application.Current.Resources["MenuText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Menu.Text));
                Application.Current.Resources["MenuBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Menu.Back));
                Application.Current.Resources["MenuBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Menu.Border));
                //Application.Current.Resources["MenuOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Menu.Other));
                Application.Current.Resources["MenuMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.MenuMouseover.Text));
                Application.Current.Resources["MenuMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.MenuMouseover.Back));
                //Application.Current.Resources["MenuMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.MenuMouseover.Border));
                //Application.Current.Resources["MenuMouseoverOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.MenuMouseover.Other));

                Application.Current.Resources["ButtonText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Button.Text));
                Application.Current.Resources["ButtonBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Button.Back));
                Application.Current.Resources["ButtonBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Button.Border));
                //Application.Current.Resources["ButtonOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Button.Other));
                Application.Current.Resources["ButtonMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonMouseover.Text));
                Application.Current.Resources["ButtonMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonMouseover.Back));
                Application.Current.Resources["ButtonMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonMouseover.Border));
                //Application.Current.Resources["ButtonMouseoverOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonMouseover.Other));
                Application.Current.Resources["ButtonDownText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonDown.Text));
                Application.Current.Resources["ButtonDownBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonDown.Back));
                Application.Current.Resources["ButtonDownBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonDown.Border));
                //Application.Current.Resources["ButtonDownOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonDown.Other));

                Application.Current.Resources["CheckboxText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Checkbox.Text));
                Application.Current.Resources["CheckboxBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Checkbox.Back));
                Application.Current.Resources["CheckboxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Checkbox.Border));
                //Application.Current.Resources["CheckboxOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Checkbox.Other));

                Application.Current.Resources["ListText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.List.Text));
                Application.Current.Resources["ListBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.List.Back));
                Application.Current.Resources["ListBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.List.Border));
                //Application.Current.Resources["ListOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.List.Other));
                Application.Current.Resources["ListMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListMouseover.Text));
                Application.Current.Resources["ListMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListMouseover.Back));
                Application.Current.Resources["ListMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListMouseover.Border));
                //Application.Current.Resources["ListMouseoverOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListMouseover.Other));
                Application.Current.Resources["ListSelectedText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListSelected.Text));
                Application.Current.Resources["ListSelectedBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListSelected.Back));
                Application.Current.Resources["ListSelectedBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListSelected.Border));
                //Application.Current.Resources["ListSelectedOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListSelected.Other));

                Application.Current.Resources["DropDownText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDown.Text));
                Application.Current.Resources["DropDownBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDown.Back));
                Application.Current.Resources["DropDownBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDown.Border));
                //Application.Current.Resources["DropDownOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDown.Other));
                Application.Current.Resources["DropDownItemMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDownMouseover.Text));
                Application.Current.Resources["DropDownItemMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDownMouseover.Back));
                Application.Current.Resources["DropDownItemMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDownMouseover.Border));
                //Application.Current.Resources["DropDownItemMouseoverOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDownMouseover.Other));

                //Application.Current.Resources["EntryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Entry.Text));
                Application.Current.Resources["EntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Entry.Back));
                Application.Current.Resources["EntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Entry.Border));
                //Application.Current.Resources["EntryOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Entry.Other));
                //Application.Current.Resources["SelectedEntryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.EntrySelected.Text));
                Application.Current.Resources["SelectedEntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.EntrySelected.Back));
                Application.Current.Resources["SelectedEntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.EntrySelected.Border));
                //Application.Current.Resources["SelectedEntryOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.EntrySelected.Other));

                //Application.Current.Resources["HiddenEntryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenEntry.Text));
                Application.Current.Resources["HiddenEntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenEntry.Back));
                Application.Current.Resources["HiddenEntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenEntry.Border));
                //Application.Current.Resources["HiddenEntryOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenEntry.Other));
                //Application.Current.Resources["HiddenSelectedEntryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenSelectedEntry.Text));
                Application.Current.Resources["HiddenSelectedEntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenSelectedEntry.Back));
                Application.Current.Resources["HiddenSelectedEntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenSelectedEntry.Border));
                //Application.Current.Resources["HiddenSelectedEntryOther"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenSelectedEntry.Other));
            } 
            catch { }
            
        }

        public static void NukeDirectory(string TheDirectory) 
        {
            //Deletes a folder, and all sub-folders (assuming none are in use)

            if (Directory.Exists(TheDirectory))
            {                
                DirectoryInfo directoryInfo = new DirectoryInfo(TheDirectory);

                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
                {
                    subDirectory.Delete(true);
                }
            }

            
        }

        public static void OpenFileFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Check if the file exists
            if (File.Exists(path))
            {
                // Use explorer to open the folder and select the file
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
                {
                    UseShellExecute = true
                });
            }
            else if (Directory.Exists(path))
            {
                // If it's a directory, open the directory without selecting a file
                Process.Start(new ProcessStartInfo("explorer.exe", path)
                {
                    UseShellExecute = true
                });
            }
        }

        public static void OpenFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {                
                return;
            }

            // Check if the path exists and is a directory
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        public static string GetSelectedFilePath(string Description, string Message = null)
        {
            VistaOpenFileDialog FileSelect = new VistaOpenFileDialog();
            FileSelect.Title = Description;
            if ((bool)FileSelect.ShowDialog())
            {                
                string ThePath = FileSelect.FileName;
                return ThePath;                

                
            }
            else { return ""; }
            
        }

        public static string GetFolderFromFilepath(string Filepath) 
        {
            return Path.GetDirectoryName(Filepath);
        }

        public static string GetSelectedFolderPath(string Description, string Message = null) 
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = Description; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            if ((bool)FolderSelect.ShowDialog()) //This triggers the folder selection screen, and if the user does not cancel out...
            {

                string FolderPath = FolderSelect.SelectedPath;
                return FolderPath;

            }
            else { return ""; }

            
        }

        

        public static string GetSelectFileName(string Description)
        {
            VistaOpenFileDialog FileSelect = new VistaOpenFileDialog();
            FileSelect.Title = Description;
            if ((bool)FileSelect.ShowDialog())
            {
                string NameOnly = Path.GetFileName(FileSelect.FileName);                
                return NameOnly;
            }
            else { return ""; }

        }

        public static string GetSelectFolderName(string Description)
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = Description; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            if ((bool)FolderSelect.ShowDialog()) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                string NameOnly = Path.GetFileName(FolderSelect.SelectedPath);

                string FolderPath = FolderSelect.SelectedPath;
                return NameOnly;
            }
            else { return ""; }

        }

        public static string GetSelectedRelativeFilePath(string description, string folderPath)
        {
            VistaOpenFileDialog fileSelect = new VistaOpenFileDialog();
            fileSelect.Title = description;
            fileSelect.InitialDirectory = folderPath;  // Start at the specified folder path

            if (fileSelect.ShowDialog() == true)
            {
                string selectedFilePath = fileSelect.FileName;  // Get the full path of the selected file

                // Ensure the selected file path starts with the initial folder path
                if (selectedFilePath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase)) //Value cannot be null
                {                    
                    return selectedFilePath.Substring(folderPath.Length).TrimStart('\\'); // Return the relative path
                }
                else
                {                    
                    return ""; // Return an empty string if the selected file is not within the folder path
                }
            }
            return "";  // Return an empty string if no file is selected
        }

        public static string GetSelectedRelativeFolderPath(string description, string folderPath)
        {
            VistaFolderBrowserDialog folderSelect = new(); // Using Ookii.Dialogs.WPF
            folderSelect.Description = description; // Description for the user
            folderSelect.UseDescriptionForTitle = true;
            folderSelect.SelectedPath = folderPath + "\\"; // Start inside this folder

            if (folderSelect.ShowDialog() == true) // If the user selects a folder
            {
                string selectedFolderPath = folderSelect.SelectedPath; // Full path to selected folder

                // Ensure the selected folder path starts with the initial folder path
                if (selectedFolderPath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Return the relative path from folderPath to selected folder
                    return selectedFolderPath.Substring(folderPath.Length).TrimStart('\\');
                }
                else
                {
                    // Return an empty string if the selected folder is not inside the base folder
                    return "";
                }
            }
            return ""; // Return an empty string if no folder is selected


        }


        public static MethodData TransformKeysToLocations(Dictionary<int, string> ResourceKeys, List<EventResource> EventResources, TopMenu TheMenu, EventCommand myCommand)
        {
            MethodData MethodData = new();
            MethodData.WorkshopData = TheMenu.WorkshopData;
            MethodData.GameLibrary = TheMenu.GameLibrary;
            MethodData.Command = myCommand.Command;            
            

            if (ResourceKeys.Count == 0) { return MethodData; }
            

            foreach (KeyValuePair<int, string> Pair in ResourceKeys) //For each EventCommand resource...
            {
                if (Pair.Value == "WTOOLS") { MethodData.ResourceLocations.Add("WTOOLS"); continue; }
                

                string FullPath = "";

                EventResource TheEventResource = null;

                foreach (EventResource EventResource in EventResources)  //Look inside the event resources...
                {                    
                    if (EventResource.Key == Pair.Value) //Find the matching resource info...
                    {
                        TheEventResource = EventResource;
                        break;
                        //and check if it's Local or Relative.
                    }
                }
                
                if (TheEventResource == null) { continue; } //for optional resource commands, altho i added this sloppily and may cause a bug. 

                if (TheEventResource.ResourceType == EventResource.ResourceTypes.CMDText) 
                {
                    FullPath = TheEventResource.Location;
                    MethodData.ResourceLocations.Add(FullPath);
                    continue;
                }

                if (TheEventResource.ParentKey == "") //If not relative, get path from project!
                {
                    foreach (ProjectEventResource ProjectEventResource in TheMenu.ProjectDataItem.ProjectEventResources)
                    {
                        if (Pair.Value == ProjectEventResource.Key)
                        {
                            FullPath = ProjectEventResource.Location;
                            break;
                        }
                    }
                }
                if (TheEventResource.ParentKey != "") //If relative, lets get 2 and combine!
                {
                    foreach (ProjectEventResource ProjectEventResource in TheMenu.ProjectDataItem.ProjectEventResources)
                    {
                        if (TheEventResource.ParentKey == ProjectEventResource.Key)
                        {
                            FullPath = ProjectEventResource.Location + "\\" + TheEventResource.Location;
                            break;
                        }
                    }


                }
                MethodData.ResourceLocations.Add(FullPath);
            }//end foreach pair

            return MethodData;
        }

        public static void GotoGeneralHide(Workshop TheWorkshop) //On Delete Editor
        {
            var generalRowTab = TheWorkshop.TabTest.Visibility = Visibility.Hidden;
        }

        public static void GotoRightBarGeneralTab(Workshop TheWorkshop) 
        {
            var GeneralTabControl = TheWorkshop.TabTest.Visibility = Visibility.Visible;

            var generalRowTab = TheWorkshop.TabTest.FindName("GeneralEditor") as TabItem;
            if (generalRowTab == null)
            {
                throw new InvalidOperationException("Tab item 'GeneralEditor' not found.");
            }
            generalRowTab.IsSelected = true;
        }

        public static void GotoGeneralRow(Workshop TheWorkshop)
        {
            var generalRowTab = TheWorkshop.TabTest.FindName("GeneralRow") as TabItem;
            if (generalRowTab == null)
            {
                throw new InvalidOperationException("Tab item 'GeneralRow' not found.");
            }
            generalRowTab.IsSelected = true;
        }

        public static void GotoGeneralColumn(Workshop TheWorkshop)
        {
            var generalRowTab = TheWorkshop.TabTest.FindName("GeneralColumn") as TabItem;
            if (generalRowTab == null)
            {
                throw new InvalidOperationException("Tab item 'GeneralColumn' not found.");
            }
            generalRowTab.IsSelected = true;
        }
        public static void GotoGeneralEntry(Workshop TheWorkshop)
        {
            var generalRowTab = TheWorkshop.TabTest.FindName("GeneralEntry") as TabItem;
            if (generalRowTab == null)
            {
                throw new InvalidOperationException("Tab item 'GeneralEntry' not found.");
            }
            generalRowTab.IsSelected = true;
        }




        public static GameFile GetGameFileUsingLocation(WorkshopData Database, string Location) 
        {
            foreach (KeyValuePair<string, GameFile> GameFile in Database.GameFiles)
            {                
                if (Location == GameFile.Value.FileLocation)
                {
                    return GameFile.Value;
                }                                
            }
            
            return null;
        }


        public static void MoveListItemUp<T>(List<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index > 0)
            {
                list.RemoveAt(index);
                list.Insert(index - 1, item);
            }
        }

        public static void MoveListItemDown<T>(List<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index >= 0 && index < list.Count - 1)
            {
                list.RemoveAt(index);
                list.Insert(index + 1, item);
            }
        }

        public static void MoveTreeItemUp(TreeView treeView, TreeViewItem item)
        {
            ItemCollection items = treeView.Items;
            int index = items.IndexOf(item);
            if (index > 0)
            {
                items.RemoveAt(index);
                items.Insert(index - 1, item);
            }
        }

        public static void MoveTreeItemDown(TreeView treeView, TreeViewItem item)
        {
            ItemCollection items = treeView.Items;
            int index = items.IndexOf(item);
            if (index >= 0 && index < items.Count - 1)
            {
                items.RemoveAt(index);
                items.Insert(index + 1, item);
            }
        }

        public static void MoveDockElementUp(DockPanel dockPanel, UIElement element)
        {
            int index = dockPanel.Children.IndexOf(element);
            if (index > 0)
            {
                dockPanel.Children.RemoveAt(index);
                dockPanel.Children.Insert(index - 1, element);
            }
        }

        public static void MoveDockElementDown(DockPanel dockPanel, UIElement element)
        {
            int index = dockPanel.Children.IndexOf(element);
            if (index >= 0 && index < dockPanel.Children.Count - 1)
            {
                dockPanel.Children.RemoveAt(index);
                dockPanel.Children.Insert(index + 1, element);
            }
        }








        



    }




    
}




