using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;

namespace GameEditorStudio
{
    class FileLoading
    {//Below was old comments when this was merged with editor loading.


        //Later: I should create a editor's button here instead of during EditorCreate, because some editors / users may
        //want Semi-Auto mode, causeing the program to only load an editor when clicked on instead of every editor at once. This may reduce lag / wait times?

        // ////////DO NOT FORGET ThE DATABASE IS ALSO LOADED WHEN A NEW EDITOR IS CREATED.

        //This file triggers in one of two ways.
        //1: When the workshop is loaded, to load the database with XML info.
        //2: The below partial class when a editor is created, also to load a database with XML info, but also to trigger making a new editor with that info.

        public void TryLoadAllGameFilesIntoWorkshopDatabase(WorkshopData WorkshopData) //Triggers when the workshop is launched.
        {
            //This method is ONLY loading the files into the workshop!
            //This method is NOT loading the files into any of the editors!

            WorkshopData.GameFiles.Clear();
            WorkshopData.GameFilesFromInput.Clear();
            if (WorkshopData.IsProjectLoaded == false) { return; }

            

            try
            {
                //This doesn't happen in preview mode. 
                string EditorsFolder = LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Editors\\";

                foreach (string Editor in Directory.GetDirectories(EditorsFolder))
                {
                    XElement Filesxml = XElement.Load(Editor + "\\Files.xml"); //This loads a XML from your workshop called WorkshopFiles.xml

                    foreach (XElement FileX in Filesxml.Descendants("File"))
                    {
                        GameFile gamefile = new(); //This class stores everything about a file.
                        gamefile.FileName = FileX.Element("Name")?.Value;
                        gamefile.FileLocation = FileX.Element("Location")?.Value;
                        gamefile.FileNote = FileX.Element("Note")?.Value;
                        gamefile.FileWorkshopTooltip = FileX.Element("Tooltip")?.Value;

                        string filelocation = FileX.Element("Location")?.Value;                        

                        bool GameFileExists = false;
                        foreach (GameFile TheGameFile in WorkshopData.GameFiles)
                        {
                            if (TheGameFile.FileLocation == gamefile.FileLocation)
                            {
                                GameFileExists = true; //Don't add a file to workshop gamefiles if it's already loaded in.
                            }
                        }
                        if (GameFileExists == false)
                        {
                            //GameFile gamefile = new(); //This class stores everything about a file.
                            //gamefile.FileName = FileX.Element("Name")?.Value;
                            //gamefile.FileLocation = FileX.Element("Location")?.Value;
                            //gamefile.FileNote = FileX.Element("Note")?.Value;
                            //gamefile.FileWorkshopTooltip = FileX.Element("Tooltip")?.Value;

                            WorkshopData.GameFiles.Add(gamefile);//Adding the GameFile to the Dictionary, with the Key of the FilePath so the key is ALWAYS unique.    
                        }
                    }
                    
                    if (LibraryGES.AlsoLoadInputFiles == true) //INPUT VERSION LOADING
                    {
                        foreach (XElement FileX in Filesxml.Descendants("File"))
                        {
                            GameFile gamefileFromInput = new(); //This class stores everything about a file.
                            gamefileFromInput.FileName = FileX.Element("Name")?.Value;
                            gamefileFromInput.FileLocation = FileX.Element("Location")?.Value;
                            gamefileFromInput.FileNote = FileX.Element("Note")?.Value;
                            gamefileFromInput.FileWorkshopTooltip = FileX.Element("Tooltip")?.Value;

                            string filelocation = FileX.Element("Location")?.Value;

                            bool GameFileExists = false;
                            foreach (GameFile TheGameFileFromInput in WorkshopData.GameFilesFromInput)
                            {
                                if (TheGameFileFromInput.FileLocation == gamefileFromInput.FileLocation)
                                {
                                    GameFileExists = true; //Don't add a file to workshop gamefiles if it's already loaded in.
                                }
                            }
                            if (GameFileExists == false)
                            {
                                //GameFile gamefileFromInput = new(); //This class stores everything about a file.
                                //gamefileFromInput.FileName = FileX.Element("Name")?.Value;
                                //gamefileFromInput.FileLocation = FileX.Element("Location")?.Value;
                                //gamefileFromInput.FileNote = FileX.Element("Note")?.Value;
                                //gamefileFromInput.FileWorkshopTooltip = FileX.Element("Tooltip")?.Value;

                                WorkshopData.GameFilesFromInput.Add(gamefileFromInput);//Adding the GameFile to the Dictionary, with the Key of the FilePath so the key is ALWAYS unique.    
                            }
                        }
                    }
                    

                }
                
                //var listPart1 = Enumerable.Range(1, 50).ToList();
                double max = WorkshopData.GameFiles.Count;
                double loadcounter = 0;
                Database.GameLibrary.LoadingProgressBar.Maximum = 100;
                Database.GameLibrary.LoadingProgressBar.Value = 0;
                Database.GameLibrary.LoadingPartText.Content = "Part 1: Loading Game Files...";
                WorkshopData.WorkshopXaml.HomeControl.LoadingProgressBar.Maximum = 100;
                WorkshopData.WorkshopXaml.HomeControl.LoadingProgressBar.Value = 0;
                WorkshopData.WorkshopXaml.HomeControl.LoadingPartText.Content = "Part 1: Loading Game Files...";

                foreach (GameFile GameFile in WorkshopData.GameFiles)
                {
                    Database.GameLibrary.LoadingStatusText.Content = GameFile.FileName + " (" + loadcounter.ToString() + "/" + WorkshopData.GameFiles.Count + ")";
                    WorkshopData.WorkshopXaml.HomeControl.LoadingStatusText.Content = GameFile.FileName + " (" + loadcounter.ToString() + "/" + WorkshopData.GameFiles.Count + ")";
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

                    if (WorkshopData.LoadedProject.ProjectOutputDirectory != "" && File.Exists(Path.Combine(WorkshopData.LoadedProject.ProjectOutputDirectory, GameFile.FileLocation)))
                    {
                        string Location = WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + GameFile.FileLocation;
                        GameFile.FileBytes = File.ReadAllBytes(WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + GameFile.FileLocation);
                    }
                    else if (WorkshopData.LoadedProject.ProjectInputDirectory != "")
                    {
                        GameFile.FileBytes = File.ReadAllBytes(WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + GameFile.FileLocation);
                    }
                    else
                    {
                        return;
                    }
                    loadcounter++;
                    double percent = (loadcounter / max) * 100;
                    int calc = (int)percent;
                    Database.GameLibrary.LoadingProgressBar.Value = calc;
                    WorkshopData.WorkshopXaml.HomeControl.LoadingProgressBar.Value = calc;
                }
                
                if (LibraryGES.AlsoLoadInputFiles == true) //INPUT VERSION LOADING
                {
                    WorkshopData.WorkshopXaml.HomeControl.LoadingPartText.Content = "Part 1.5: Loading Game Files (from input)...";
                    foreach (GameFile GameFileFromInput in WorkshopData.GameFilesFromInput)
                    {
                        Database.GameLibrary.LoadingStatusText.Content = GameFileFromInput.FileName + " (" + loadcounter.ToString() + "/" + WorkshopData.GameFilesFromInput.Count + ")";
                        WorkshopData.WorkshopXaml.HomeControl.LoadingStatusText.Content = GameFileFromInput.FileName + " (" + loadcounter.ToString() + "/" + WorkshopData.GameFilesFromInput.Count + ")";
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                                                
                        if (WorkshopData.LoadedProject.ProjectInputDirectory != "" && File.Exists(Path.Combine(WorkshopData.LoadedProject.ProjectInputDirectory, GameFileFromInput.FileLocation)))
                        {
                            string fullpath = WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + GameFileFromInput.FileLocation;

                            //If i ever crash because of this again, make sure the program is building to x64 and not x86. 
                            //x86 has a RAM limit of about 2~4GB.
                            //x64 RAM limit is about 1 BILLION GB.
                            GameFileFromInput.FileBytes = File.ReadAllBytes(fullpath);
                        }
                        else
                        {                            
                            return;
                        }                        


                        //loadcounter++;
                        //double percent = (loadcounter / max) * 100;
                        //int calc = (int)percent;
                        //Database.GameLibrary.LoadingProgressBar.Value = calc;
                        //WorkshopData.WorkshopXaml.HomeControl.LoadingProgressBar.Value = calc;
                    }
                }
                

                //List<Task> loadFilesTasks = new();

                //foreach (GameFile GameFile in WorkshopData.GameFiles)
                //{
                //    loadFilesTasks.Add(Task.Run(async () =>
                //    {
                //        if (WorkshopData.LoadedProject.ProjectOutputDirectory != "" && File.Exists(Path.Combine(WorkshopData.LoadedProject.ProjectOutputDirectory, GameFile.FileLocation)))
                //        {
                //            GameFile.FileBytes = File.ReadAllBytes(WorkshopData.LoadedProject.ProjectOutputDirectory + "\\" + GameFile.FileLocation);
                //        }
                //        else if (WorkshopData.LoadedProject.ProjectInputDirectory != "")
                //        {
                //            GameFile.FileBytes = File.ReadAllBytes(WorkshopData.LoadedProject.ProjectInputDirectory + "\\" + GameFile.FileLocation);
                //        }
                //        else
                //        {
                //            return;
                //        }

                //    }));


                //}
                //await Task.WhenAll(loadFilesTasks);
            }
            catch (Exception ex)
            {   
                MessageBox.Show("ERROR" + ex.ToString(), "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                //If i ever crash because of this again, make sure the program is building to x64 and not x86. 
                //x86 has a RAM limit of about 2~4GB.
                //x64 RAM limit is about 1 BILLION GB.

                PixelWPF.LibraryPixel.Notification("ERROR",
                    "The workshop failed to load all files." +
                    "\n" +
                    "\nPossible reasons are as follow:" +
                    "\n1: The input directory is incorrect" +
                    "\n2: You have moved or renamed some files." +
                    "\n3: You failed to extract everything you needed to begin with to use the workshop." +
                    "\n4: The workshop creator has changed the folder / file structure of the workshop." +
                    "\n" +
                    "\n5: It's possible you don't have enough RAM to load this many files, or that i (yet again) accidentally built the program to be for x86 giving it a RAM limit of 2~4GB instead of having no RAM limits." +
                    "\n" +
                    "\nIf you can't stop getting this error, don't keep trying, just ask for help. (On the discord, or from the workshop creator)." +
                    "\n" +
                    "\nThe Program will now close as a safety measure.");

                Application.Current.Shutdown();
                return;
            }

            Database.GameLibrary.LoadingStatusText.Content = "Done~";
            WorkshopData.WorkshopXaml.HomeControl.LoadingStatusText.Content = "Done~";
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        

        }
        

        public void ReloadAllEditorFiles(Editor editor) //For when the user wants to reload an editor's files. 
        {
            return;

            if (editor is not DataTableEditorData DataTableData) { return; } //Clean this up later. Probably turn this into a reloadSTANDARDeditor files and make one for other editor types.

            LoadFileIntoDatabase(editor.DataTableEditorData.NameTable.TextTableFile);
            LoadFileIntoDatabase(editor.DataTableEditorData.NameTable.TextTableFile);
            foreach (TextTable descriptionTable in editor.DataTableEditorData.DescriptionTableList) 
            {
                LoadFileIntoDatabase(descriptionTable.TextTableFile);
            }            

            
        }

        public void LoadFileIntoDatabase(GameFile gamefile)
        {
            gamefile.FileBytes = File.ReadAllBytes(gamefile.FileLocation); //wrong because gamefile location is only a partial path!
        }

    }
}
