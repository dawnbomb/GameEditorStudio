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
using WpfHexaEditor;

namespace GameEditorStudio
{
    class LoadTextEditor
    {
        public void NewTextEditorIntoDatabase(WorkshopData database, UserControlEditorCreator Maker) 
        {
            Workshop TheWorkshop = database.WorkshopXaml;

            TheWorkshop.EditorName = Maker.TextboxEditorName.Text;


            Editor editor = new(); //Creates the base class of the editor. Everything else becomes a child of this class, including other classes.
            editor.EditorName = Maker.TextboxEditorName.Text;
            editor.EditorType = "TextEditor";
            editor.EditorKey = PixelWPF.LibraryPixel.GenerateKey();
            //EditorClass.EditorIcon = Maker.DemoEditorImage.Tag as string; for old icon system.


            TextEditorData TextData = new();
            editor.TextEditorData  = TextData;
                        

            database.GameEditors.Add(TheWorkshop.EditorName, editor);

            TabButtonMaker MakeEditorButton = new();
            MakeEditorButton.CreateTabButton(database, editor);

            TextEditor ATextEditor = new TextEditor(database, editor);

            editor.EditorButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        public void LoadTextEditorXMLIntoDatabase(Workshop TheWorkshop, WorkshopData Database, string TargetXML) 
        {
            XElement xml = XElement.Load(TargetXML);
            TheWorkshop.EditorName = xml.Element("Name")?.Value; //Sets the name of the editor were working with from the name stored in XML.

            Editor EditorClass = new();          //Creates a EditorClass
            EditorClass.Workshop = TheWorkshop;
            //EditorClass.EditorName = xml.Element("Name")?.Value;
            //EditorClass.EditorName = EditorName;
            EditorClass.EditorName = Path.GetFileName(Path.GetDirectoryName(TargetXML));
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
