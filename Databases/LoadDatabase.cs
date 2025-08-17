using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using WpfHexaEditor.Properties;
using static System.Net.WebRequestMethods;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using File = System.IO.File;

namespace GameEditorStudio
{

    


    internal class LoadDatabase
    {

        //Everything in this class is called exactly once when the program first starts, and never again.
        //As the filename implies, this literally is just loading the database class and it's subclasses and their subclasses etc with all the data in xml files.
        //The only exception is the LoadToolsLocations() method, that is also called whenever the menu is opened.
        //It re-checks the last known tool locations, incase the user is moving tools around while the program is running (this is not common, but still very expected).


        //////////////////////////////////////////////////////
        ////////////////////WORKSHOPS/////////////////////////
        //////////////////////////////////////////////////////

        public void LoadWorkshops() 
        {
            if (Directory.Exists(LibraryGES.ApplicationLocation + "\\Workshops"))
            {
                string[] workshopfolders = Directory.GetDirectories(LibraryGES.ApplicationLocation + "\\Workshops", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();

                foreach (var workshopfolder in workshopfolders)
                {
                    WorkshopData workshopData = new();
                    Database.Workshops.Add(workshopData);

                    //Workshop.xml data
                    using (FileStream TargetXML = new FileStream(LibraryGES.ApplicationLocation + "\\Workshops\\" + workshopfolder + "\\Workshop.xml", FileMode.Open, FileAccess.Read))
                    {
                        XElement xml = XElement.Load(TargetXML);

                        //Loading Workshop basic data
                        workshopData.WorkshopName = xml.Element("WorkshopName")?.Value;
                        workshopData.WorkshopInputDirectory = xml.Element("InputLocation")?.Value;
                        workshopData.ProjectsRequireSameFolderName = bool.TryParse(xml.Element("ProjectsRequireSameInputFolderName")?.Value, out bool result) && result;

                        //Loading Workshop Event Resources
                        //foreach (var xmlEvent in xml.Descendants("Resource"))
                        //{
                        //    EventResource EventResource = new();
                        //    workshopData.WorkshopEventResources.Add(EventResource);

                        //    EventResource.Name = xmlEvent.Element("Name")?.Value;
                        //    EventResource.Location = xmlEvent.Element("Location")?.Value;
                        //    EventResource.RequiredName = bool.TryParse(xmlEvent.Element("RequiredName")?.Value, out var result2) ? result2 : false;
                        //    EventResource.Key = xmlEvent.Element("Key")?.Value;
                        //    EventResource.ParentKey = xmlEvent.Element("ParentKey")?.Value;
                        //    //THESE NEED TO BE UPDATED TO USE THE ISCHILD AND FILETYPE VARIABLES IF I EVER USE THIS CODE AGAIN!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        //    if (xmlEvent.Element("ResourceType")?.Value == "File" && xmlEvent.Element("PathType")?.Value == "FullPath") { EventResource.ResourceType = "LocalFile"; }
                        //    if (xmlEvent.Element("ResourceType")?.Value == "Folder" && xmlEvent.Element("PathType")?.Value == "FullPath") { EventResource.ResourceType = "LocalFolder"; }
                        //    if (xmlEvent.Element("ResourceType")?.Value == "File" && xmlEvent.Element("PathType")?.Value == "PartialPath") { EventResource.ResourceType = "RelativeFile"; }
                        //    if (xmlEvent.Element("ResourceType")?.Value == "Folder" && xmlEvent.Element("PathType")?.Value == "PartialPath") { EventResource.ResourceType = "RelativeFolder"; }

                        //}
                    }

                    

                    //Load Workshop Common Events
                    if (File.Exists(LibraryGES.ApplicationLocation + "\\Workshops\\" + workshopData.WorkshopName + "\\Common Events.xml"))
                    {
                        List<string> ListOfCommonEventKeys = new();

                        XElement xml = XElement.Load(LibraryGES.ApplicationLocation + "\\Workshops\\" + workshopData.WorkshopName + "\\Common Events.xml");
                        foreach (XElement XCommonEvent in xml.Descendants("CommonEvent"))
                        {
                            string TheKey = XCommonEvent.Element("Key")?.Value;
                            ListOfCommonEventKeys.Add(TheKey);
                        }

                        foreach (CommonEvent commonevent in Database.CommonEvents)
                        {
                            if (ListOfCommonEventKeys.Any(key => key == commonevent.Key))
                            {
                                workshopData.WorkshopCommonEvents.Add(commonevent);
                            }
                        }
                    }



                    //Load Workshop Event Resources NEW
                    string eventsDirectory = Path.Combine(LibraryGES.ApplicationLocation, "Workshops", workshopData.WorkshopName, "Events");
                    if (Directory.Exists(eventsDirectory))
                    {
                        foreach (string eventFolder in Directory.GetDirectories(eventsDirectory))
                        {
                            string resourcesPath = Path.Combine(eventFolder, "Resources.xml");

                            if (!File.Exists(resourcesPath))
                            {
                                continue;
                            }

                            using (FileStream targetXML = new FileStream(resourcesPath, FileMode.Open, FileAccess.Read))
                            {
                                XElement xml = XElement.Load(targetXML);

                                foreach (var xmlEvent in xml.Descendants("Resource"))
                                {
                                    string testkey = xmlEvent.Element("Key")?.Value;
                                    if (workshopData.WorkshopEventResources.Any(r => r.Key == testkey)) { continue; }


                                    EventResource EventResource = new();
                                    workshopData.WorkshopEventResources.Add(EventResource);

                                    EventResource.Name = xmlEvent.Element("Name")?.Value;
                                    EventResource.Location = xmlEvent.Element("Location")?.Value;
                                    EventResource.RequiredName = bool.TryParse(xmlEvent.Element("RequiredName")?.Value, out var result2) ? result2 : false;
                                    EventResource.Key = xmlEvent.Element("Key")?.Value;
                                    EventResource.ParentKey = xmlEvent.Element("ParentKey")?.Value;
                                    if (xmlEvent.Element("ResourceType")?.Value == "File")    { EventResource.ResourceType = EventResource.ResourceTypes.File; }
                                    if (xmlEvent.Element("ResourceType")?.Value == "Folder")  { EventResource.ResourceType = EventResource.ResourceTypes.Folder; }
                                    if (xmlEvent.Element("ResourceType")?.Value == "CMDText") { EventResource.ResourceType = EventResource.ResourceTypes.CMDText; }
                                    if (xmlEvent.Element("IsChild")?.Value == "False")    { EventResource.IsChild = false; }
                                    if (xmlEvent.Element("IsChild")?.Value == "True") { EventResource.IsChild = true; }
                                    
                                    

                                }
                            }
                        }
                    }



                    //Load Workshop Events                  
                    if (Directory.Exists(eventsDirectory))
                    {
                        List<string> EventsListLoadOrder = new();
                        string EventsOrderText = LibraryGES.ApplicationLocation + "\\Workshops\\" + workshopData.WorkshopName + "\\Events\\" + "LoadOrder.txt";
                        if (File.Exists(EventsOrderText))
                        {
                            string[] lines = File.ReadAllLines(EventsOrderText);
                            foreach (string line in lines)
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    EventsListLoadOrder.Add(line);
                                }
                            }
                        }

                        foreach (string eventFolder in EventsListLoadOrder) //Load all known events
                        {
                            try 
                            {
                                LoadEvent(eventFolder);
                            } 
                            catch
                            {
                                string EventThatDidNotLoad = eventFolder;
                                System.Diagnostics.Debugger.Break(); //An even inside LoadOrder didn't load because the folder is missing. 
                            }
                            

                        }

                        string[] allEventFolders = Directory.GetDirectories(eventsDirectory, "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
                        foreach (string eventFolder in allEventFolders) //Load all unknown events?
                        {
                            if (!EventsListLoadOrder.Contains(eventFolder))
                            {
                                LoadEvent(eventFolder);
                            }
                        }

                        void LoadEvent(string eventFolder)
                        {
                            string eventFile = Path.Combine(eventsDirectory + "\\" + eventFolder, "Event.xml");

                            XDocument doc = XDocument.Load(eventFile);
                            foreach (var xmlEvent in doc.Descendants("Event"))
                            {
                                Event newEvent = new Event
                                {
                                    DisplayName = xmlEvent.Element("Name")?.Value ?? "New Event",
                                    Note = xmlEvent.Element("Note")?.Value ?? string.Empty,
                                    Tooltip = xmlEvent.Element("Tooltip")?.Value ?? string.Empty,
                                    CommandList = new List<EventCommand>()
                                };

                                var commandElements = xmlEvent.Descendants("Command");
                                foreach (var commandElement in commandElements)
                                {
                                    string commandKey = commandElement.Element("Key")?.Value;
                                    if (!string.IsNullOrEmpty(commandKey))
                                    {
                                        Command matchingCommand = Database.Commands.FirstOrDefault(cmd => cmd.Key == commandKey);
                                        if (matchingCommand != null)
                                        {
                                            bool CMD = false;
                                            bool.TryParse(commandElement.Element("CMD")?.Value, out CMD);

                                            EventCommand myCommand = new EventCommand
                                            {
                                                Command = matchingCommand,
                                                ResourceKeys = new Dictionary<int, string>()
                                            };

                                            // Initialize the dictionary with default values
                                            int resourceKeyIndex = 1;
                                            foreach (CommandResource Aresource in matchingCommand.RequiredResourcesList) //var resource in matchingCommand.Resources
                                            {
                                                myCommand.ResourceKeys.Add(resourceKeyIndex++, ""); // Initialize with empty or default values   
                                            }

                                            { //LOAD SOME SPECIAL STUFF IF THIS IS THE COMMAND PROMPT COMMAND
                                                //if (CMD == true)
                                                //{
                                                //    var CMDresourceElements = commandElement.Descendants("Resource");
                                                //    foreach (var resourceElement in CMDresourceElements)
                                                //    {
                                                //        string resourceKey = resourceElement.Element("Key")?.Value ?? "";
                                                //        myCommand.ResourceKeys.Add(resourceKeyIndex++, "");
                                                //    }
                                                //}

                                                var CMDResourceListElement = commandElement.Descendants("CMDResourceList");
                                                foreach (var CMDResourceElement in CMDResourceListElement)
                                                {
                                                    foreach (var resourceElement in CMDResourceElement.Elements("CMDResource"))
                                                    {
                                                        string CMDType = resourceElement.Element("CMDType")?.Value;
                                                        if (CMDType == "File")
                                                        {
                                                            CommandResource ResourceData = new();
                                                            ResourceData.Label = "File Path From";
                                                            ResourceData.Type = CommandResource.ResourceTypes.File;
                                                            myCommand.CMDList.Add(ResourceData);

                                                            //myCommand.ResourceKeys.Add(resourceKeyIndex++, "");

                                                        }
                                                        if (CMDType == "Folder")
                                                        {
                                                            CommandResource ResourceData = new();
                                                            ResourceData.Label = "Folder Path From";
                                                            ResourceData.Type = CommandResource.ResourceTypes.Folder;
                                                            myCommand.CMDList.Add(ResourceData);
                                                        }
                                                        if (CMDType == "CMDText")
                                                        {
                                                            CommandResource ResourceData = new();
                                                            ResourceData.Label = "Your Text";
                                                            ResourceData.Type = CommandResource.ResourceTypes.CMDText;
                                                            //ResourceData.CMDString = resourceElement.Element("CMDText")?.Value;
                                                            myCommand.CMDList.Add(ResourceData);

                                                            ResourceData.CMDTextKey = resourceElement.Element("CMDTextKey")?.Value;

                                                            //EventResource TextResource = new();
                                                            //TextResource.Name = "CMD Text Resource";
                                                            //TextResource.Key = PixelWPF.LibraryPixel.GenerateKey();
                                                            //TextResource.ResourceType = EventResource.ResourceTypes.Text;
                                                            //workshopData.WorkshopEventResources.Add(TextResource);
                                                        }
                                                    }
                                                }

                                            }//END OF IF COMMAND PROMPT COMMAND

                                            // Update the dictionary with actual values from XML
                                            resourceKeyIndex = 1; // Reset index for actual values
                                            var resourceElements = commandElement.Descendants("Resource");
                                            foreach (var resourceElement in resourceElements)
                                            {
                                                string resourceKey = resourceElement.Element("Key")?.Value ?? "";

                                                if (myCommand.ResourceKeys.ContainsKey(resourceKeyIndex))
                                                {
                                                    myCommand.ResourceKeys[resourceKeyIndex] = resourceKey;
                                                }
                                                else
                                                {
                                                    myCommand.ResourceKeys.Add(resourceKeyIndex, resourceKey);
                                                }

                                                resourceKeyIndex++;
                                            }

                                            newEvent.CommandList.Add(myCommand);
                                        }
                                    }
                                }

                                workshopData.WorkshopEvents.Add(newEvent);
                            }
                        }

                    }



                    //A workshop's projects.
                    string ProjectsFolder = LibraryGES.ApplicationLocation + "\\Projects\\" + workshopfolder + "\\"; //"\\LibraryBannerArt.png";   
                    if (Directory.Exists(ProjectsFolder))
                    {
                        foreach (string TheProjectFolder in Directory.GetDirectories(ProjectsFolder))
                        {

                            using (FileStream fs = new FileStream(TheProjectFolder + "\\Project.xml", FileMode.Open, FileAccess.Read))
                            {
                                XElement xml = XElement.Load(fs);
                                string PName = xml.Element("Name")?.Value;
                                string PInput = xml.Element("InputLocation")?.Value;
                                string POutput = xml.Element("OutputLocation")?.Value;

                                List<ProjectEventResource> ProjectEventResources = new();
                                var xmlEventResources = xml.Element("ResourceList");

                                if (xmlEventResources != null)
                                {
                                    //This oIf its empty to begin with, it blanks.


                                    foreach (EventResource EventResource in workshopData.WorkshopEventResources)
                                    {
                                        if (EventResource.IsChild == true) { continue; }

                                        ProjectEventResource projectEventData = new ProjectEventResource
                                        {
                                            Key = EventResource.Key,
                                        };
                                        ProjectEventResources.Add(projectEventData);
                                    }

                                    foreach (XElement xmlEventResource in xmlEventResources.Elements("Resource"))
                                    {
                                        string resourceKey = xmlEventResource.Element("Key")?.Value;
                                        string location = xmlEventResource.Element("Location")?.Value;

                                        foreach (ProjectEventResource ProjectResourceData in ProjectEventResources)
                                        {
                                            if (resourceKey == ProjectResourceData.Key)
                                            {
                                                ProjectResourceData.Location = location;
                                            }
                                        }
                                    }
                                }

                                ProjectData projectDataItem = new();
                                workshopData.ProjectsList.Add(projectDataItem);

                                projectDataItem.ProjectName = PName;
                                projectDataItem.ProjectInputDirectory = PInput;
                                projectDataItem.ProjectOutputDirectory = POutput;
                                projectDataItem.ProjectEventResources = ProjectEventResources;


                            }

                        }



                    }


                }




            }
        }

        

        
        //////////////////////////////////////////////////////
        ////////////////////WORKSHOPS/////////////////////////
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        /////////////////////TOOLS////////////////////////////
        //////////////////////////////////////////////////////

        public void LoadToolsList()
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

                Database.Tools.Add(tool);

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
                Tool TheTool = Database.Tools.FirstOrDefault(item => item.Key == toolNode["Key"].InnerText);

                if (TheTool == null) 
                {
                    continue;
                }

                TheTool.Location = toolNode["Location"].InnerText;
            }

            

            //Part 2: Confirm the tool locations are still correct. If a tool is MIA, it's location updates to ""
            //Also automatically search the Tools folder and auto-update user paths to these.
            foreach (var tool in Database.Tools)
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

        public void LoadCommandsList(GameLibrary gameLibrary)
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
                    Tool FoundTool = Database.Tools.Find(Tool => Tool.Key == ThetoolKey);
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


                Database.Commands.Add(command);

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





        public void LoadCommonEventsList()
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
                    Command FoundCommand = Database.Commands.Find(Command => Command.Key == TheCommandKey);
                    commonevent.MyCommands.Add(FoundCommand);
                }


                Database.CommonEvents.Add(commonevent);

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

            foreach (CommonEvent commonevent in Database.CommonEvents)
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


        //////////////////////////////////////////////////////
        //////////////////COMMON EVENTS///////////////////////
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        //////////////////////OTHER///////////////////////////
        //////////////////////////////////////////////////////

        public void LoadThemes()
        {
            //LibraryMan.ColorThemeList.Clear();
            string themesDirectory = Path.Combine(LibraryGES.ApplicationLocation, "Other\\Themes");

            if (Directory.Exists(themesDirectory))
            {
                foreach (string themeFile in Directory.GetFiles(themesDirectory, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    XElement themeXml = XElement.Load(themeFile);
                    ColorTheme theme = new ColorTheme
                    {
                        Name = (string)themeXml.Element("Name")
                    };

                    foreach (XElement xmlElement in themeXml.Descendants("Element"))
                    {
                        string elementName = (string)xmlElement.Element("Name"); //I do not load name, i set it manually one for each element. 
                        string text = (string)xmlElement.Element("Text");
                        string back = (string)xmlElement.Element("Back");
                        string border = (string)xmlElement.Element("Border");
                        string other = (string)xmlElement.Element("Other");

                        // Find the corresponding element in the theme by name
                        Element themeElement = theme.ElementList.FirstOrDefault(e => e.Name == elementName);
                        if (themeElement != null)
                        {
                            themeElement.Text = text;
                            themeElement.Back = back;
                            themeElement.Border = border;
                            themeElement.Other = other;
                        }
                    }

                    LibraryGES.ColorThemeList.Add(theme);
                }
            }
            else
            {
                Console.WriteLine("No themes directory found.");
            }

            try
            {
                ColorTheme LastTheme = LibraryGES.ColorThemeList.FirstOrDefault(e => e.Name == "Default");
                LibraryGES.SwitchToColorTheme(LastTheme);
            }
            catch
            {

            }

        }




    }




}


