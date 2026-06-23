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
                            WorkshopData.GameFiles.Add(gamefile);//Adding the GameFile to the Dictionary, with the Key of the FilePath so the key is ALWAYS unique.    
                        }
                    }
                }

                var listPart1 = Enumerable.Range(1, 50).ToList();
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
            }
            catch
            {
                MessageBox.Show("The workshop failed to load all files." +
                    "\n" +
                    "\nPossible reasons are as follow:" +
                    "\n1: The input directory is incorrect" +
                    "\n2: You have moved or renamed some files." +
                    "\n3: You failed to extract everything you needed to begin with to use the workshop." +
                    "\n4: The workshop creator has changed the folder / file structure of the workshop." +
                    "\n" +
                    "\nIf you can't stop getting this error, don't keep trying, just ask for help. Especially if you can contact the workshop creator." +
                    "\n" +
                    "\nThe Program will now close as a safety measure.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);


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
