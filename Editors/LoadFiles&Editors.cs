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


        public void LoadEveryEditorXMLIntoWorkshopData(Workshop TheWorkshop, WorkshopData Database) //Triggers when the workshop is launched / opened.
        {
            //I had AI rewrite this method and did not look at it very hard but it works. 
            string editorsRoot = Path.Combine(LibraryGES.ApplicationLocation,"Workshops",TheWorkshop.WorkshopData.WorkshopName,"Editors");

            string loadOrderPath = Path.Combine(editorsRoot, "LoadOrder.txt");

            // 1. Read LoadOrder.txt (Editor KEYS)
            List<string> loadOrderKeys = new();
            if (File.Exists(loadOrderPath))
            {
                foreach (string line in File.ReadAllLines(loadOrderPath))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        loadOrderKeys.Add(line.Trim());
                }
            }

            // 2. Scan ALL editor folders and build Key → XML map
            Dictionary<string, string> editorKeyToXmlPath = new();

            foreach (string editorFolder in Directory.GetDirectories(editorsRoot))
            {
                string editorXmlPath = Path.Combine(editorFolder, "Editor.xml");
                if (!File.Exists(editorXmlPath))
                    continue;

                XElement xml = XElement.Load(editorXmlPath);
                string editorKey = xml.Element("Key")?.Value;

                if (string.IsNullOrWhiteSpace(editorKey))
                    continue;

                // If duplicate keys exist, first one wins (you may want to log this)
                if (!editorKeyToXmlPath.ContainsKey(editorKey))
                {
                    editorKeyToXmlPath.Add(editorKey, editorXmlPath);
                }
            }

            // 3. PART 1 — Load editors in LoadOrder.txt order
            HashSet<string> loadedKeys = new();

            foreach (string key in loadOrderKeys)
            {
                if (editorKeyToXmlPath.TryGetValue(key, out string editorXmlPath))
                {
                    LoadEditorXML(TheWorkshop, Database, editorXmlPath);
                    loadedKeys.Add(key);
                }
                // else: key listed but editor missing — optional warning/log
            }

            // 4. PART 2 — Load any remaining editors not in LoadOrder.txt
            foreach (var kvp in editorKeyToXmlPath)
            {
                if (!loadedKeys.Contains(kvp.Key))
                {
                    LoadEditorXML(TheWorkshop, Database, kvp.Value);
                }
            }


            //   //All my old code below. Keeping it for reference just in case AI messed something up.
            
            //List<string> ListOfEditorKeys = new();
            //List<KeyValuePair<string, string>> FolderNameEditorKeyPairs = new();

            //string EditorsText = LibraryGES.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors\\" + "LoadOrder.txt";

            //if (File.Exists(EditorsText))
            //{
            //    string[] lines = File.ReadAllLines(EditorsText);
            //    foreach (string line in lines)
            //    {
            //        if (!string.IsNullOrWhiteSpace(line))
            //        {
            //            ListOfEditorKeys.Add(line);
            //        }
            //    }
            //}

            //{
            //    //PART 1: Loading editors from LordOrder.txt

            //    foreach (string EditorKey in ListOfEditorKeys)
            //    {
            //        string TargetXML = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", TheWorkshop.WorkshopData.WorkshopName, "Editors", EditorKey, "Editor.xml");
            //        if (File.Exists(TargetXML))
            //        {
            //            LoadEditorXML(TheWorkshop, Database, TargetXML);

            //        }
            //    }

            //    ////AI code rewrite
            //    //string editorsRoot = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", TheWorkshop.WorkshopData.WorkshopName, "Editors");
            //    //foreach (string EditorKey in ListOfEditorKeys)
            //    //{
            //    //    foreach (string editorFolder in Directory.GetDirectories(editorsRoot))
            //    //    {
            //    //        string targetXML = Path.Combine(editorFolder, "Editor.xml");
            //    //        if (!File.Exists(targetXML))
            //    //            continue;

            //    //        XElement xml = XElement.Load(targetXML);
            //    //        string xmlKey = xml.Element("Key")?.Value;

            //    //        if (xmlKey == EditorKey)
            //    //        {
            //    //            LoadEditorXML(TheWorkshop, Database, targetXML);
            //    //            break; // stop searching folders for this key
            //    //        }
            //    //    }
            //    //}
            //}

            //{
            //    //Database.GameEditors   and  Editor.EditorKey

            //    //PART 2: Loading any editors that are new (Did not exist in the LoadOrder.txt) (IE new editors you imported from somewhere)
            //    string[] EditorFolderNames = Directory.GetDirectories(LibraryGES.ApplicationLocation + "\\Workshops\\" + TheWorkshop.WorkshopData.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
            //    foreach (string FolderName in EditorFolderNames)
            //    {
            //        if (!ListOfEditorKeys.Contains(FolderName))
            //        {
            //            string TargetXML = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", TheWorkshop.WorkshopData.WorkshopName, "Editors", FolderName, "Editor.xml");
            //            LoadEditorXML(TheWorkshop, Database, TargetXML);
            //        }
            //        //CreateButton(TheWorkshop);
            //    }

            //}

            //Here we finish loading standard editor XMLs. These are split into 2 parts so all editors load their names first (so Menu Links to another editor's name list works properly)
            foreach (Editor TheEditor in Database.GameEditors.Values) 
            {
                if (TheEditor.EditorType == "DataTable") 
                {
                    LoadStandardEditor EditorSetup = new();
                    EditorSetup.LoadDataTableXMLIntoDatabasePART2(TheWorkshop, Database, TheEditor);                    
                    
                }
            }

            

        }

        //This Method is for creating a new editor while already inside a workshop. IE when going into a workshop and clicking the "New Editor" button.
        //It is diffrent from the previous stuff that was for loading the database with editors from files.
        //Both above and below load editor information into the database, and do not actually create the editor.
        //Both also run CreateEditor() at the end, which is what actually creates the editor.


        // ////////NOTE:    the database is also loaded, via the editor creator user control. This only happens when the user is making a brand new editor, all other editors are made above.

        public void LoadEditorXML(Workshop TheWorkshop, WorkshopData Database, string TargetXML)
        {
            XDocument doc = XDocument.Load(TargetXML);
            string EditorType = doc.Descendants("Type").FirstOrDefault()?.Value;

            if (EditorType == "DataTable")
            {
                LoadStandardEditor EditorSetup = new();
                EditorSetup.LoadDataTableXMLIntoDatabasePART1(TheWorkshop, Database, TargetXML);
            }
            if (EditorType == "TextEditor")
            {
                LoadTextEditor EditorSetup = new();
                EditorSetup.LoadTextEditorXMLIntoDatabase(TheWorkshop, Database, TargetXML);
            }
        }

        public void GenerateAllEditorUIs(WorkshopData database) 
        {
            //This happens after all editors load their data because some UI elements of an editor link to the loaded data of another editor. 

            foreach (Editor TheEditor in database.GameEditors.Values)
            {
                TabButtonMaker MakeEditorButton = new();
                MakeEditorButton.CreateTabButton(database, TheEditor); 

                if (TheEditor.EditorType == "DataTable")
                {
                    StandardEditor standardEditor = new(database, TheEditor); //Generates the Editor UI using Editor data.
                }
                if (TheEditor.EditorType == "TextEditor")
                {
                    TextEditor ATextEditor = new TextEditor(database, TheEditor);
                }
            }

            database.WorkshopXaml.UpdateEntryDecorationsForAllEditors();
        }



    }







}
