using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void LoadAllGameFilesIntoWorkshopDatabase(WorkshopData WorkshopData) //Triggers when the workshop is launched.
        {
            //This doesn't happen in preview mode. 
            string EditorsFolder = LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Editors\\";

            foreach (string Editor in Directory.GetDirectories(EditorsFolder))
            {
                XElement Filesxml = XElement.Load(Editor + "\\Files.xml"); //This loads a XML from your workshop called WorkshopFiles.xml

                foreach (XElement FileX in Filesxml.Descendants("File"))
                {
                    GameFile FileInfo = new(); //This class stores everything about a file.
                    FileInfo.FileName = FileX.Element("Name")?.Value;
                    FileInfo.FileLocation = FileX.Element("Location")?.Value;
                    FileInfo.FileNote = FileX.Element("Note")?.Value;
                    FileInfo.FileWorkshopTooltip = FileX.Element("Tooltip")?.Value;

                    bool GameFileExists = false;
                    foreach (GameFile GameFile in WorkshopData.GameFiles.Values)
                    {
                        if (GameFile.FileLocation == FileInfo.FileLocation)
                        {
                            GameFileExists = true;
                        }
                    }
                    if (GameFileExists == false)
                    {
                        WorkshopData.GameFiles.Add(FileInfo.FileLocation, FileInfo);//Adding the GameFile to the Dictionary, with the Key of the FilePath so the key is ALWAYS unique.    
                    }
                }
            }

            foreach (GameFile GameFile in WorkshopData.GameFiles.Values)
            {
                if (WorkshopData.ProjectDataItem.ProjectOutputDirectory != "" && File.Exists(Path.Combine(WorkshopData.ProjectDataItem.ProjectOutputDirectory, GameFile.FileLocation)))
                {
                    GameFile.FileBytes = File.ReadAllBytes(WorkshopData.ProjectDataItem.ProjectOutputDirectory + "\\" + GameFile.FileLocation);
                }
                else if (WorkshopData.ProjectDataItem.ProjectInputDirectory != "")
                {
                    GameFile.FileBytes = File.ReadAllBytes(WorkshopData.ProjectDataItem.ProjectInputDirectory + "\\" + GameFile.FileLocation);
                }
                else
                {
                    return;
                }
            }

        }
        

        public void ReloadAllEditorFiles(Editor editor) //For when the user wants to reload an editor's files. 
        {
            return;

            if (editor.EditorType != "DataTable") { return; } //Clean this up later. Probably turn this into a reloadSTANDARDeditor files and make one for other editor types.

            LoadFileIntoDatabase(editor.StandardEditorData.FileDataTable);
            LoadFileIntoDatabase(editor.StandardEditorData.FileNameTable);
            foreach (DescriptionTable descriptionTable in editor.StandardEditorData.DescriptionTableList) 
            {
                LoadFileIntoDatabase(descriptionTable.FileTextTable);
            }            

            
        }

        public void LoadFileIntoDatabase(GameFile gamefile)
        {
            gamefile.FileBytes = File.ReadAllBytes(gamefile.FileLocation); //wrong because gamefile location is only a partial path!
        }

    }
}
