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
using System.Windows.Threading;

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
                    {
                        // Split the line by whitespace and take the first entry
                        // StringSplitOptions.RemoveEmptyEntries handles multiple spaces between the key and the comment
                        string keyOnly = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                        if (!string.IsNullOrEmpty(keyOnly))
                            loadOrderKeys.Add(keyOnly);
                    }
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

            foreach (DataTableEditorData DataTableData in Database.GameEditors.OfType<DataTableEditorData>()) 
            {
                LoadStandardEditor EditorSetup = new();
                EditorSetup.LoadDataTableXMLIntoDatabasePART2(TheWorkshop, Database, DataTableData);
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

        public void GenerateAllEditorXAML(WorkshopData database) 
        {
            //This happens after all editors load their data because some UI elements of an editor link to the loaded data of another editor. 

            double max = database.GameEditors.Count;
            double loadcounter = 0;
            Database.GameLibrary.LoadingProgressBar.Maximum = 100;
            Database.GameLibrary.LoadingProgressBar.Value = 0;
            Database.GameLibrary.LoadingPartText.Content = "Generating Game Editors...";
            
            foreach (Editor TheEditor in database.GameEditors)
            {
                Database.GameLibrary.LoadingStatusText.Content = TheEditor.EditorName + " (" + (loadcounter + 1).ToString() + "/" + database.GameEditors.Count + ")";
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

                if (TheEditor is DataTableEditorData DTeditor)
                {
                    DataTableEditor standardEditor = new(database, DTeditor); //Generates the Editor UI using Editor data.
                }
                if (TheEditor is TextEditorData texteditordata) //
                {
                    TextEditor ATextEditor = new TextEditor(database, texteditordata);                    
                }
                loadcounter++;
                double percent = (loadcounter / max) * 100;
                int calc = (int)percent;
                Database.GameLibrary.LoadingProgressBar.Value = calc;
                                
            }
            Database.GameLibrary.LoadingStatusText.Content = "Done~";
            Database.GameLibrary.LoadingFinalPanel.Visibility = Visibility.Visible;
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        }



        public void LoadAllWorkshopDocuments(WorkshopData WorkshopData) 
        {
            string[] WorkshopDocumentOrder = File.ReadLines(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents\\" + "LoadOrder.txt").ToArray();
            string[] WorkshopDocumentFolderNames = Directory.GetDirectories(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
            foreach (string name in WorkshopDocumentOrder)
            {
                if (WorkshopDocumentFolderNames.Contains(name))
                {
                    Document TheDocument = new Document
                    {
                        Name = name,
                        Text = System.IO.File.ReadAllText(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents\\" + name + "\\Text.txt")
                    };
                    WorkshopData.WorkshopDocumentsList.Add(TheDocument); // Adding the document object to the list
                }
            }
            foreach (string name in WorkshopDocumentFolderNames)//The, add any new documents to the document tree in alphabetical order.
            {
                if (!WorkshopDocumentOrder.Contains(name))
                {
                    Document TheDocument = new Document
                    {
                        Name = name,
                        Text = System.IO.File.ReadAllText(LibraryGES.ApplicationLocation + "\\Workshops\\" + WorkshopData.WorkshopName + "\\Documents\\" + name + "\\Text.txt")
                    };
                    WorkshopData.WorkshopDocumentsList.Add(TheDocument); // Adding the document object to the list                 

                }
            }

        }

        public void LoadAllProjectDocuments(WorkshopData WorkshopData)
        {
            WorkshopData.ProjectDocumentsList.Clear();

            if (WorkshopData.IsProjectLoaded == false) 
            {                
                return;
            }

            string[] ProjectDocumentOrder = File.ReadLines(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents\\" + "LoadOrder.txt").ToArray();
            string[] ProjectDocumentFolderNames = Directory.GetDirectories(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
            foreach (string name in ProjectDocumentOrder)//The last known list of documents for this workshop, in the order they were saved in.
            {
                if (ProjectDocumentFolderNames.Contains(name))
                {
                    Document TheDocument = new Document
                    {
                        Name = name,
                        Text = System.IO.File.ReadAllText(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents\\" + name + "\\Text.txt")
                    };
                    WorkshopData.ProjectDocumentsList.Add(TheDocument); // Adding the document object to the list

                }
            }
            foreach (string name in ProjectDocumentFolderNames)//The, add any new documents to the document tree in alphabetical order.
            {
                if (!ProjectDocumentOrder.Contains(name))
                {
                    Document TheDocument = new Document
                    {
                        Name = name,
                        Text = System.IO.File.ReadAllText(LibraryGES.ApplicationLocation + "\\Projects\\" + WorkshopData.WorkshopName + "\\" + WorkshopData.LoadedProject.ProjectName + "\\Documents\\" + name + "\\Text.txt")
                    };
                    WorkshopData.ProjectDocumentsList.Add(TheDocument); // Adding the document object to the list

                }
            }
        }


    }







}
