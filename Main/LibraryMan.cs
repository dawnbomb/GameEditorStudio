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
    public static class LibraryMan
    {        
        
        public static string VersionDate { get; set; } = "July 2 2025";
        public static Version VersionNumber { get; set; } = new Version(0, 1, 1, 0); //Version Numbers (in order) are Major.Minor.Build.Revision
        //Major is big releases.
        //Minor is new features / content.
        //Build is for Bugfixes or small changes.
        //Revision is for code rewrites that dont* affect the user. *SHOULDN'T AFFECT THE USER >:(

        //OLD SETTINGS
        public static string ColorTheme { get; set; } = "Dark"; 
        public static bool ShowEntryAddress { get; set; } = false; 
        public static string EntryAddressType { get; set; } = "Decimal";
        //public static bool CollectionPrefix { get; set; } = true;
        public static bool ShowItemIndex { get; set; } = false;
        public static bool ShowHiddenEntrys { get; set; } = false;
        public static bool ShowSymbology { get; set; } = false;
        public static bool ShowTranslationPanel { get; set; } = false;
        //END

        public static string ApplicationLocation { get; set; } = "";
        public static string SettingsFolderName { get; set; } = "Settings";

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
                Application.Current.Resources["ApplicationText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Application.Text));
                Application.Current.Resources["ApplicationBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Application.Back));
                Application.Current.Resources["ApplicationBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Application.Border));

                Application.Current.Resources["HeaderText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Header.Text));
                Application.Current.Resources["HeaderBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Header.Back));
                Application.Current.Resources["HeaderBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Header.Border));

                Application.Current.Resources["PanelText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Panel.Text));
                Application.Current.Resources["PanelBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Panel.Back));
                Application.Current.Resources["PanelBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Panel.Border));

                Application.Current.Resources["TextboxText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Textbox.Text));
                Application.Current.Resources["TextboxBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Textbox.Back));
                Application.Current.Resources["TextboxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Textbox.Border));
                //Application.Current.Resources["TextboxHighlightText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.TextboxHighlight.Text));
                Application.Current.Resources["TextboxHighlightBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.TextboxHighlight.Text));
                //No Highlight Border

                Application.Current.Resources["MenuText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Menu.Text));
                Application.Current.Resources["MenuBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Menu.Back));
                Application.Current.Resources["MenuBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Menu.Border));
                Application.Current.Resources["MenuMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.MenuMouseover.Text));
                Application.Current.Resources["MenuMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.MenuMouseover.Back));
                Application.Current.Resources["MenuMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.MenuMouseover.Border));

                Application.Current.Resources["ButtonText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Button.Text));
                Application.Current.Resources["ButtonBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Button.Back));
                Application.Current.Resources["ButtonBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Button.Border));
                Application.Current.Resources["ButtonMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonMouseover.Text));
                Application.Current.Resources["ButtonMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonMouseover.Back));
                Application.Current.Resources["ButtonMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonMouseover.Border));
                Application.Current.Resources["ButtonDownText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonDown.Text));
                Application.Current.Resources["ButtonDownBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonDown.Back));
                Application.Current.Resources["ButtonDownBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ButtonDown.Border));

                Application.Current.Resources["CheckboxText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Checkbox.Text));
                Application.Current.Resources["CheckboxBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Checkbox.Back));
                Application.Current.Resources["CheckboxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Checkbox.Border));

                Application.Current.Resources["ListText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.List.Text));
                Application.Current.Resources["ListBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.List.Back));
                Application.Current.Resources["ListBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.List.Border));
                Application.Current.Resources["ListMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListMouseover.Text));
                Application.Current.Resources["ListMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListMouseover.Back));
                Application.Current.Resources["ListMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListMouseover.Border));
                Application.Current.Resources["ListSelectedText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListSelected.Text));
                Application.Current.Resources["ListSelectedBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListSelected.Back));
                Application.Current.Resources["ListSelectedBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.ListSelected.Border));

                Application.Current.Resources["DropDownText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDown.Text));
                Application.Current.Resources["DropDownBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDown.Back));
                Application.Current.Resources["DropDownBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDown.Border));
                Application.Current.Resources["DropDownItemMouseoverText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDownMouseover.Text));
                Application.Current.Resources["DropDownItemMouseoverBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDownMouseover.Back));
                Application.Current.Resources["DropDownItemMouseoverBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.DropDownMouseover.Border));
                //Application.Current.Resources["DarkMode_TextboxBackround"] = new SolidColorBrush(Colors.Blue);

                Application.Current.Resources["EntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Entry.Back));
                Application.Current.Resources["EntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.Entry.Border));
                Application.Current.Resources["SelectedEntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.EntrySelected.Back));
                Application.Current.Resources["SelectedEntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.EntrySelected.Border));

                Application.Current.Resources["HiddenEntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenEntry.Back));
                Application.Current.Resources["HiddenEntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenEntry.Border));
                Application.Current.Resources["HiddenSelectedEntryBack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenSelectedEntry.Back));
                Application.Current.Resources["HiddenSelectedEntryBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.HiddenSelectedEntry.Border));
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

        public static string GetSelectedRelativeFilePath(string description, string folderPath)
        {
            VistaOpenFileDialog fileSelect = new VistaOpenFileDialog();
            fileSelect.Title = description;
            fileSelect.InitialDirectory = folderPath;  // Start at the specified folder path

            if (fileSelect.ShowDialog() == true)
            {
                string selectedFilePath = fileSelect.FileName;  // Get the full path of the selected file

                // Ensure the selected file path starts with the initial folder path
                if (selectedFilePath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Return the relative path
                    return selectedFilePath.Substring(folderPath.Length).TrimStart('\\');
                }
                else
                {
                    // Return an empty string if the selected file is not within the folder path
                    return "";
                }
            }

            return "";  // Return an empty string if no file is selected
        }

        public static string GetSelectedRelativeFolderPath(string Description, string FolderPath) 
        {
            return "Relative Folder is Disabled for now / upcoming feature";
        }

        public static string GetSelectFileName(string Description, string Message = null)
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

        public static string GetSelectFolderName(string Description, string Message = null)
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


        public static MethodData TransformKeysToLocations(Dictionary<int, string> ResourceKeys, List<WorkshopResource> EventResources, MainMenu TheMenu, EventCommand myCommand)
        {
            MethodData MethodData = new();
            MethodData.WorkshopData = TheMenu.WorkshopData;
            MethodData.GameLibrary = TheMenu.GameLibrary;
            MethodData.Command = myCommand.Command;            
            

            if (ResourceKeys.Count == 0) { return MethodData; }
            

            foreach (KeyValuePair<int, string> Pair in ResourceKeys) //For each EventCommand resource...
            {
                string FullPath = "";

                WorkshopResource TheEventResource = null;

                foreach (WorkshopResource EventResource in EventResources)  //Look inside the event resources...
                {                    
                    if (EventResource.WorkshopResourceKey == Pair.Value) //Find the matching resource info...
                    {
                        TheEventResource = EventResource;

                        //and check if it's Local or Relative.
                    }
                }
                
                if (TheEventResource == null) { continue; } //for optional resource commands, altho i added this sloppily and may cause a bug. 

                if (TheEventResource.TargetKey == "") //If not relative, get path from project!
                {
                    foreach (ProjectEventResource ProjectEventResource in TheMenu.ProjectDataItem.ProjectEventResources)
                    {
                        if (Pair.Value == ProjectEventResource.ResourceKey)
                        {
                            FullPath = ProjectEventResource.Location;
                        }
                    }
                }
                if (TheEventResource.TargetKey != "") //If relative, lets get 2 and combine!
                {
                    foreach (ProjectEventResource ProjectEventResource in TheMenu.ProjectDataItem.ProjectEventResources)
                    {
                        if (TheEventResource.TargetKey == ProjectEventResource.ResourceKey)
                        {

                            FullPath = ProjectEventResource.Location + "\\" + TheEventResource.Location;
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

        public static void GotoGeneralEditor(Workshop TheWorkshop) 
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

        public static string GenerateKey()
        {
            string Key = DateTimeOffset.UtcNow.Ticks.ToString() + "-" + new Random().Next(0, 999999999).ToString() + "-" + new Random().Next(0, 999999999).ToString();
            return Key ; 
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


        public static void Notification(string Title, string Text) 
        {
            Notification Notification = new("", Title, Text);
            bool? result = Notification.ShowDialog();
            if (result == true) // User clicked OK
            {
                
            }
            else // User closed the window or cancelled
            {
                
            }
        }

        public static void NotificationPositive(string Title, string Text)
        {
            Notification Notification = new("✔",Title, Text);
            bool? result = Notification.ShowDialog();
            if (result == true) // User clicked OK
            {

            }
            else // User closed the window or cancelled
            {

            }

        }

        public static void NotificationNegative(string Title, string Text)
        {
            Notification Notification = new("X", Title, Text);
            bool? result = Notification.ShowDialog();
            if (result == true) // User clicked OK
            {

            }
            else // User closed the window or cancelled
            {

            }
        }

        public static void NotificationGenericError()
        {
            Notification Notification = new("X", "An error occured.", "Please report it :(");
            bool? result = Notification.ShowDialog();
            if (result == true) // User clicked OK
            {

            }
            else // User closed the window or cancelled
            {

            }
        }






        



    }




    
}




