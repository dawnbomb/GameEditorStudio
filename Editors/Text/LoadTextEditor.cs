using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using WpfHexEditor;

namespace GameEditorStudio
{
    class LoadTextEditor
    {
        public void NewTextEditorIntoDatabase(WorkshopData database, UserControlEditorCreator Maker) 
        {
            TextEditorData editor = new();            
            editor.EditorName = Maker.TextboxEditorName.Text;

            editor.WorkshopData = database;
            editor.WorkshopXaml = database.WorkshopXaml;

            database.GameEditors.Add(editor);
                        

            TextEditor ATextEditor = new TextEditor(database, editor);

            editor.EditorTab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        public void LoadTextEditorXMLIntoDatabase(Workshop TheWorkshop, WorkshopData Database, string TargetXML) 
        {
            XElement xml = XElement.Load(TargetXML);

            TextEditorData TextEditorClass = new();          //Creates a EditorClass
            TextEditorClass.WorkshopXaml = TheWorkshop;
            TextEditorClass.WorkshopData = Database;
            TextEditorClass.EditorName = Path.GetFileName(Path.GetDirectoryName(TargetXML));
            TextEditorClass.EditorIcon = xml.Element("Icon")?.Value;
            TextEditorClass.EditorKey = xml.Element("Key")?.Value;

            foreach (XElement item in xml.Descendants("EditorFile"))
            {
                string ItemLocation = item.Element("FileLocation")?.Value;
                TextEditorClass.GameFileLocations.Add(ItemLocation);

                foreach (GameFile gameFile in Database.GameFiles)
                {
                    if (gameFile.FileLocation == ItemLocation)
                    {
                        TextEditorClass.ListOfGameFiles.Add(gameFile);
                    }
                }
            }

            
            
            Database.GameEditors.Add(TextEditorClass); //Adds a core (aka the value) with the Key (New editor name from textbox) to the database dictionary.
            
        }

    }
}
