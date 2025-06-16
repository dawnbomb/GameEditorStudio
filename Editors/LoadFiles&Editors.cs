using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;
//using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace GameEditorStudio.Loading
{
    class LoadWorkshopDatabaseCode
    {

        //Later: I should create a editor's button here instead of during EditorCreate, because some editors / users may
        //want Semi-Auto mode, causeing the program to only load an editor when clicked on instead of every editor at once. This may reduce lag / wait times?

        // ////////DO NOT FORGET ThE DATABASE IS ALSO LOADED WHEN A NEW EDITOR IS CREATED.

        //This file triggers in one of two ways.
        //1: When the workshop is loaded, to load the database with XML info.
        //2: The below partial class when a editor is created, also to load a database with XML info, but also to trigger making a new editor with that info.


        public void LoadGameFilesIntoDatabase(Workshop TheWorkshop, WorkshopData Database) //Triggers when the workshop is launched.
        {
            //This doesn't happen in preview mode. 
            string EditorsFolder = @LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\";             
            
            foreach (string Editor in Directory.GetDirectories(EditorsFolder))
            {
                XElement Filesxml = XElement.Load(Editor + "\\Files.xml"); //This loads a XML from your workshop called WorkshopFiles.xml
                                
                foreach (XElement FileX in Filesxml.Descendants("File"))
                {
                    GameFile FileInfo = new(); //This class stores everything about a file.
                    FileInfo.FileName = FileX.Element("Name")?.Value;
                    FileInfo.FileLocation = FileX.Element("Location")?.Value;
                    FileInfo.FileNote = FileX.Element("Note")?.Value;
                    FileInfo.FileNotepad = FileX.Element("Notepad")?.Value;

                    bool GameFileExists = false;
                    foreach (GameFile GameFile in Database.GameFiles.Values) 
                    {
                        if (GameFile.FileLocation == FileInfo.FileLocation) 
                        {
                            GameFileExists = true;
                        }
                    }
                    if (GameFileExists == false) 
                    {
                        Database.GameFiles.Add(FileInfo.FileLocation, FileInfo);//Adding the GameFile to the Dictionary, with the Key of the FilePath so the key is ALWAYS unique.    
                    }                    
                }
            }

            foreach (GameFile GameFile in Database.GameFiles.Values)
            {    
                if (TheWorkshop.ProjectDataItem.ProjectOutputDirectory != "" && File.Exists(Path.Combine(TheWorkshop.ProjectDataItem.ProjectOutputDirectory, GameFile.FileLocation)))
                {
                    GameFile.FileBytes = File.ReadAllBytes(TheWorkshop.ProjectDataItem.ProjectOutputDirectory + "\\" + GameFile.FileLocation);
                }
                else if (TheWorkshop.ProjectDataItem.ProjectInputDirectory != "")
                {
                    GameFile.FileBytes = File.ReadAllBytes(TheWorkshop.ProjectDataItem.ProjectInputDirectory + "\\" + GameFile.FileLocation);
                }
                else
                {
                    return;
                }
            }


            



        }

        

        public void LoadEditors(Workshop TheWorkshop, WorkshopData Database) //Triggers when the workshop is launched / opened.
        {

            List<string> EditorsList = new();
            string EditorsText = LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + "LoadOrder.txt";

            if (File.Exists(EditorsText))
            {
                string[] lines = File.ReadAllLines(EditorsText);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        EditorsList.Add(line);
                    }
                }
            }

            //This first loads every editor in the EditorsList (Last save editor order).
            //Then using EditorFolderNames it loads every editor that is NOT in that list. (IE new editors you imported from somewhere)

            foreach (string EditorName in EditorsList)
            {
                string TargetXML = Path.Combine(LibraryMan.ApplicationLocation, "Workshops", TheWorkshop.WorkshopName, "Editors", EditorName, "Editor.xml");
                if (File.Exists(TargetXML))
                {
                    LoadTheEditor(TheWorkshop, Database, TargetXML);

                }
            }

            string[] EditorFolderNames = Directory.GetDirectories(LibraryMan.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
            foreach (string FolderName in EditorFolderNames)
            {
                if (!EditorsList.Contains(FolderName))
                {
                    string TargetXML = Path.Combine(LibraryMan.ApplicationLocation, "Workshops", TheWorkshop.WorkshopName, "Editors", FolderName, "Editor.xml");
                    LoadTheEditor(TheWorkshop, Database, TargetXML);
                }
                //CreateButton(TheWorkshop);
            }

            foreach (Editor TheEditor in Database.GameEditors.Values) //This loads a editors entry data, happening last so editors can load their names first, (so Menu Link From Editor can work, it needs names loaded)
            {
                if (TheEditor.EditorType == "DataTable") 
                {
                    LoadStandardEditor EditorSetup = new();
                    EditorSetup.LoadDataTableFromFilePart2(TheWorkshop, Database, TheEditor);                    
                    
                }
            }


            foreach (Editor TheEditor in Database.GameEditors.Values) //This loads a editors entry data, happening last so editors can load their names first, (so Menu Link From Editor can work, it needs names loaded)
            {
                if (TheEditor.EditorType == "DataTable")
                {
                    GenerateStandardEditor Maker = new GenerateStandardEditor();
                    Maker.GenerateNormalEditor(TheWorkshop, Database, TheEditor); //Create a editor with this information.

                }
                if (TheEditor.EditorType == "TextEditor")
                {
                    TextEditor ATextEditor = new TextEditor(Database, TheEditor);
                }
            }
        }

        //This Method is for creating a new editor while already inside a workshop. IE when going into a workshop and clicking the "New Editor" button.
        //It is diffrent from the previous stuff that was for loading the database with editors from files.
        //Both above and below load editor information into the database, and do not actually create the editor.
        //Both also run CreateEditor() at the end, which is what actually creates the editor.


        // ////////NOTE:    the database is also loaded, via the editor creator user control. This only happens when the user is making a brand new editor, all other editors are made above.

        public void LoadTheEditor(Workshop TheWorkshop, WorkshopData Database, string TargetXML)
        {
            XDocument doc = XDocument.Load(TargetXML);
            string EditorType = doc.Descendants("Type").FirstOrDefault()?.Value;

            if (EditorType == "DataTable")
            {
                LoadStandardEditor EditorSetup = new();
                EditorSetup.LoadDataTableFromFile(TheWorkshop, Database, TargetXML);
            }
            if (EditorType == "TextEditor")
            {
                LoadTextEditor EditorSetup = new();
                EditorSetup.LoadTextEditorFromFile(TheWorkshop, Database, TargetXML);
            }
        }

    }







}
