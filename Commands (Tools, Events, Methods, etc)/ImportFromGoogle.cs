using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace GameEditorStudio
{

    


    internal class ImportFromGoogle
    {
        public void ImportTableFromGoogle(GameLibrary GameLibrary) 
        {
            ImportTools(); //Imports tool info from google sheets.            
            ImportCommands(GameLibrary); //Import command info from google sheets.            
            ImportCommonEvents(); //Import event info from google sheets.                                  
            
            //NOTE: LoadCommonEventsForWorkshop happens when the tools menu itself is opened,
            //as i don't currently support pre-loading every workshops data from the library, but common events are still for the "CURRENT" workshop.
        }


        public void ImportSettings() 
        {
            LoadToolLocations(); //Load the users last known tool locations (from XML).
            LoadEnabledCommonEvents(); //Loads the user's data on if each common event was Local Enabled or not (from XML).
            //Any Static Settings
        }





        //////////////////////////////////////////////////////
        /////////////////////TOOLS////////////////////////////
        //////////////////////////////////////////////////////


        public void ImportTools()
        {
            XElement xml = XElement.Load(LibraryGES.ApplicationLocation + "\\Other\\Tools.xml");

            foreach (XElement item in xml.Descendants("Tool"))
            {
                Tool tool = new();

                tool.DisplayName = item.Element("Name")?.Value;
                tool.Description = item.Element("Description")?.Value;
                tool.ExeName = item.Element("ExecutableName")?.Value;
                tool.DownloadLink = item.Element("DownloadLink")?.Value;
                tool.Category = item.Element("Category")?.Value;
                tool.Key = item.Element("Key")?.Value;
                tool.Notepad = item.Element("Notepad")?.Value;

                TrueDatabase.Tools.Add(tool);

            }


        }

        public void LoadToolLocations()
        {
            if (!Directory.Exists(LibraryGES.ApplicationLocation + "\\Settings")) 
            {
                return;
            }
            if (!File.Exists(LibraryGES.ApplicationLocation + "\\Settings\\Tools.xml"))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(LibraryGES.ApplicationLocation + "\\Settings\\Tools.xml"); // replace this with the actual path to your XML file

            XmlNodeList toolNodes = doc.SelectNodes("/Tools/Tool");
            foreach (XmlNode toolNode in toolNodes) // For each Tool node in the XML, If the Tools dictionary has the listed tool, load it's location and General status. 
            {
                Tool TheTool = TrueDatabase.Tools.FirstOrDefault(item => item.Key == toolNode["Key"].InnerText);

                if (TheTool == null) 
                {
                    continue;
                }

                TheTool.Location = toolNode["Location"].InnerText;
            }

            

            //Part 2: Confirm the tool locations are still correct. If a tool is MIA, it's location updates to ""
            //Also automatically search the Tools folder and auto-update user paths to these.
            foreach (var tool in TrueDatabase.Tools)
            {
                var files = Directory.GetFiles(LibraryGES.ApplicationLocation + "\\Tools", tool.ExeName, SearchOption.AllDirectories);

                if (files.Length > 0)
                {
                    tool.Location = files[0]; // If the tool.exe is found, set the location.
                }

                if (!File.Exists(tool.Location))
                {
                    tool.Location = "";
                }
            }
        }


        //////////////////////////////////////////////////////
        /////////////////////TOOLS////////////////////////////
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        ////////////////////COMMANDS//////////////////////////
        //////////////////////////////////////////////////////

        public void ImportCommands(GameLibrary gameLibrary)
        {
            XElement xml = XElement.Load(LibraryGES.ApplicationLocation + "\\Other\\Commands.xml");

            foreach (XElement XCommand in xml.Descendants("Command"))
            {
                Command command = new();

                command.DisplayName = XCommand.Element("Name")?.Value;
                command.Description = XCommand.Element("Description")?.Value;
                command.Notepad = XCommand.Element("Notepad")?.Value;
                command.Key = XCommand.Element("Key")?.Value;
                command.Category = XCommand.Element("Category")?.Value;
                command.Group = XCommand.Element("Group")?.Value;
                command.MethodName = XCommand.Element("MethodName")?.Value;
                command.TheMethod = ResolveActions(XCommand.Element("MethodName")?.Value);

                foreach (XElement XTool in XCommand.Descendants("Tool"))
                {
                    string ThetoolKey = XTool.Element("Key")?.Value;
                    Tool FoundTool = TrueDatabase.Tools.Find(Tool => Tool.Key == ThetoolKey);
                    command.RequiredToolsList.Add(FoundTool);
                }

                foreach (XElement XResource in XCommand.Descendants("Resource"))
                {
                    CommandResource TheResource = new();

                    TheResource.Label = XResource.Element("Label")?.Value;
                    TheResource.Type = ParseResourceType(XResource.Element("Type")?.Value);
                    //TheResource.IsOptional = XResource.Element("Optional")?.Value == "TRUE";
                    command.RequiredResourcesList.Add(TheResource);
                }

                


                command.GameLibrary = gameLibrary;


                TrueDatabase.Commands.Add(command);

            }
            
        }


        private PointerToMethod ResolveActions(string TheMethodName)
        {
            MethodInfo method = typeof(CommandMethodsClass).GetMethod(TheMethodName, BindingFlags.Static | BindingFlags.Public);
            if (method != null)
            {
                return (MethodData ActionPack) =>
                {
                    method.Invoke(null, new object[] { ActionPack });
                    //Last time i got an error here, what actually happened was the Action's required / intake stuff (things passed to it) was missing something (in the actions class). 
                    //it pops here instead because i'm using reflection to invoke a method here, instead of where it's actually coded (the actions class)
                };
            }
            // Return null if no matching method was found
            return null;
        }




        private CommandResource.ResourceTypes ParseResourceType(string resourceType)
        {
            // You could enhance this to handle more types or unexpected inputs
            return resourceType.Equals("Folder", StringComparison.OrdinalIgnoreCase) ?
                   CommandResource.ResourceTypes.Folder : CommandResource.ResourceTypes.File;
        }




        



        //////////////////////////////////////////////////////
        ////////////////////COMMANDS//////////////////////////
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        //////////////////COMMON EVENTS///////////////////////
        //////////////////////////////////////////////////////





        public void ImportCommonEvents()
        {
            XElement xml = XElement.Load(LibraryGES.ApplicationLocation + "\\Other\\Common Events.xml");

            foreach (XElement XCommonEvent in xml.Descendants("CommonEvent"))
            {
                CommonEvent commonevent = new();

                commonevent.DisplayName = XCommonEvent.Element("Name")?.Value;
                commonevent.Description = XCommonEvent.Element("Description")?.Value;
                commonevent.Notepad = XCommonEvent.Element("Notepad")?.Value;
                commonevent.Key = XCommonEvent.Element("Key")?.Value;
                commonevent.Category = XCommonEvent.Element("Category")?.Value;
                                                
                foreach (XElement XCommand in XCommonEvent.Descendants("Command"))
                {
                    string TheCommandKey = XCommand.Element("Key")?.Value;
                    Command FoundCommand = TrueDatabase.Commands.Find(Command => Command.Key == TheCommandKey);
                    commonevent.MyCommands.Add(FoundCommand);
                }


                TrueDatabase.CommonEvents.Add(commonevent);

            }


        }





        public void LoadEnabledCommonEvents()
        {
            if (!Directory.Exists(LibraryGES.ApplicationLocation + "\\Settings"))
            {
                return;
            }
            if (!File.Exists(LibraryGES.ApplicationLocation + "\\Settings\\Common Events.xml"))
            {
                return;
            }
            

            List<string> ListOfCommonEventKeys = new();

            XElement xml = XElement.Load(LibraryGES.ApplicationLocation + "\\Settings\\Common Events.xml");
            foreach (XElement XCommonEvent in xml.Descendants("CommonEvent"))
            {
                string TheKey = XCommonEvent.Element("Key")?.Value;
                ListOfCommonEventKeys.Add(TheKey);
            }

            foreach (CommonEvent commonevent in TrueDatabase.CommonEvents)
            {
                if (ListOfCommonEventKeys.Any(key => key == commonevent.Key))
                {
                    commonevent.Local = true;
                }
                else
                {
                    commonevent.Local = false;
                }

            }




        }







    }




}


