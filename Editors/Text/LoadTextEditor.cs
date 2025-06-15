using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace GameEditorStudio
{
    class LoadTextEditor
    {
        public void NewTextEditorIntoDatabase(WorkshopData Database, UserControlEditorCreator Maker) 
        {
            Workshop TheWorkshop = Database.Workshop;

            TheWorkshop.EditorName = Maker.TextboxEditorName.Text;


            Editor EditorClass = new(); //Creates the base class of the editor. Everything else becomes a child of this class, including other classes.
            EditorClass.EditorName = Maker.TextboxEditorName.Text;
            EditorClass.EditorType = "TextEditor";
            EditorClass.EditorKey = LibraryMan.GenerateKey();
            //EditorClass.EditorIcon = Maker.DemoEditorImage.Tag as string; for old icon system.


            TextEditorData TextData = new();
            EditorClass.TextEditorData  = TextData;

            //var selectedItem = Maker.FileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            //GameFile TextFile = selectedItem.Tag as GameFile;
            //string Key = TextFile.FileLocation;
            //EditorClass.TextData.EditorFile = TextFile;

            //TextData.FileLocation = Key;//Database.GameFiles[Key].FilePath;
            //TextData.TheText = System.IO.File.ReadAllText(Database.Workshop.ProjectDataItem.ProjectInputDirectory + "\\" + TextData.FileLocation);

            

            Database.GameEditors.Add(TheWorkshop.EditorName, EditorClass);


            TextEditor ATextEditor = new TextEditor(Database, EditorClass);
        }

        public void LoadTextEditorFromFile(Workshop TheWorkshop, WorkshopData Database, string TargetXML) 
        {
            XElement xml = XElement.Load(TargetXML);
            TheWorkshop.EditorName = xml.Element("Name")?.Value; //Sets the name of the editor were working with from the name stored in XML.

            Editor EditorClass = new();          //Creates a EditorClass
            EditorClass.Workshop = TheWorkshop; 
            EditorClass.EditorName = xml.Element("Name")?.Value;
            EditorClass.EditorIcon = xml.Element("Icon")?.Value;
            EditorClass.EditorType = xml.Element("Type")?.Value;
            EditorClass.EditorKey = xml.Element("Key")?.Value;

            EditorClass.TextEditorData = new();

            if (TheWorkshop.IsPreviewMode == false) 
            {
                foreach (XElement item in xml.Descendants("EditorFile"))
                {
                    string ItemLocation = item.Element("FileLocation")?.Value;

                    foreach (GameFile gameFile in Database.GameFiles.Values) 
                    {
                        if (gameFile.FileLocation == ItemLocation) 
                        {
                            EditorClass.TextEditorData.ListOfGameFiles.Add(gameFile);
                        }
                    }
                }
                //EditorClass.TextEditorData.TheText = System.IO.File.ReadAllText(Database.Workshop.ProjectDataItem.ProjectInputDirectory + "\\" + EditorClass.TextEditorData.FileLocation);
            }
            

            Database.GameEditors.Add(TheWorkshop.EditorName, EditorClass); //Adds a core (aka the value) with the Key (New editor name from textbox) to the database dictionary.



            
        }

    }
}
