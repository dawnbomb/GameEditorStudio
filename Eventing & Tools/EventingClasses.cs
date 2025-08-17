using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GameEditorStudio
{

    public delegate void PointerToMethod(MethodData MethodData);
    public class MethodData //Aka this is the data the pointer is sending to the method that is being run. 
    {
        //Method data only has it's variables filled if it's part of an event. 
        //If an event is called outside an event, i have to fill in any needed data, and keep in mind if it's relevant if the event is called from the library or workshop or not. 

        public Command Command { get; set; }
        public List<string> ResourceLocations { get; set; } = new(); //TransformKeysToLocations (Called when clicking a event to run it) fills this with all events required resources filepath locations.
        
        // ========================================= Below this line: Used by very specific methods =========================================

        public TopMenu mainMenu { get; set; } //Only used by Save Commands. User events always add this during click just to be safe. It's also added on click during any File->Save event. Don't touch this.        

        public GameLibrary GameLibrary { get; set; } //Only exists if the command is called from the GameLibrary. Although i probably want to remove this. 
        public WorkshopData WorkshopData { get; set; } //Used to only exist if called from the Workshop, but now it always exists. So legacy code assuming this didn't exist might cause issues.


        //Function Key (

    }


    public class Tool //i should make the tools list static?
    {
        public string DisplayName { get; set; } = "New Tool"; //The name of the tool. IE Notepad++. This is what shows up in menus. It's also the key, but i've come to dislike having to refer to keys as variables.
        public string Description { get; set; } = ""; //A longer description of the tool. Appears in the tool setup window, and possibly as a tooltip.        
        public string Notepad { get; set; } = "";
        public string Key { get; set; } = PixelWPF.LibraryPixel.GenerateKey();
        public string Category { get; set; } = "Tools";
        
        public string DownloadLink { get; set; } = "";
        public string ExeName { get; set; } = ""; //The actual name of the executable (including the ".exe" suffix). IE notepad++.exe
        public string Location { get; set; } = ""; //The users location / path to the exe for this tool on their computer. IE "C:\Program Files\Notepad++\notepad++.exe"
        

        
        

    }


    public class Command //Info for each command. This is imported from a google sheet.
    {
        public string DisplayName { get; set; } = "New Command"; //The display name of the command that appears in the Tools dropdown Menu, or the Event Commands menu.
        public string Description { get; set; } = "";
        public string Notepad { get; set; } = "";
        public string Key { get; set; } = PixelWPF.LibraryPixel.GenerateKey();
        public string OldKey { get; set; } = ""; //The name of the command that appears in XML files. This name should never be changed. (because saved editors use old names)
        //IDEA: Change the code that normally calls an command using it's OldKey, to instead call a CommonEvent...except...that still needs a key huh...fuck.
        //Yeah, i need a way to directly call a command without a action pack. 
        //Only ShortcutOpenDownloadsFolder doesn't use the action pack, even the other shortcuts do...Ughhh this will be...complicated. 

        public string Category { get; set; } = "Upcoming"; //Decides what Tab the command appears in. 
        public string Group { get; set; } = "Basics"; //Decides what grouping the command appears in, inside a given tab. 

        public string MethodName { get; set; } = ""; //The name of the method that runs when this command is called. 

        public List<Tool> RequiredToolsList { get; set; } = new List<Tool>();
        public List<CommandResource> RequiredResourcesList { get; set; } = new();
        
        public PointerToMethod TheMethod { get; set; } //the code that runs when this Command is called.
        

        

        public WorkshopData WorkshopData { get; set; }
        public GameLibrary GameLibrary { get; set; }

    }



    public class CommonEvent
    {
        
        public string DisplayName { get; set; } = "New Common Event";
        public string Description { get; set; } = "";
        public string Notepad { get; set; } = "";
        public string Key { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //XML
        public string Category { get; set; } = "Basic"; //Used in the tools menu for commons.
        public List<Command> MyCommands { get; set; } = new(); //The list of commands this CommonEvent executes. At the time of writing this, no common event even has more then 1
                                                               //I'm making it a list anyway to be future proof, but that can also mean i messed up as i had no test examples to work with. 
        


        public bool Local { get; set; } = false; //If the Event is Common, this decides if it's always available or not.
        public bool Workshop { get; set; } = false; //If the Event is Common, this declares it Workshop Important, so it's available.   (This is loaded from XML)      
    }

    public class ProjectEventResource //The resources the user has set for their project. A Workshop decides how many of these exists.
    {
        //Save,,,
        public string Key { get; set; } //XML
        public string Location { get; set; } //XML  Used to store the path to a file/folder for a project. 
    }

    public class Event //An entire event (the kind the user creates)
    {
        public string DisplayName { get; set; } = "New Event";
        public string Note { get; set; } = "";
        public string Tooltip { get; set; } = "";
        public List<EventCommand> CommandList { get; set; } = new();        
        
    }

    public class EventCommand //This is a command in one of a workshop's events (the actual event creation window) and tracks the specific workshop resources the command is using.
    {
        //oh my fucking god the naming scheme is getting horrible @_@
        public Command ?Command { get; set; }
        public Dictionary<int, string> ResourceKeys 
        { get; 
            set; }
            = new(); //Because methoddatapack protects commands from structure changes, this can be changed to a list AFTER RELEASE.
        //Resource 1's Key, Resource 2's Key, etc. These keys match the ones in EventResource (right below this)

        public List<CommandResource> CMDList { get; set; } = new();

    }

    public class EventResource //A workshop can define any number of these resources, and events can use them to automate tasks. Some of these make a projecteventresource as well. saves to XML. Not a child of anything?
    {
        //As a reminder this is NOT a project event resource.

        public string Name { get; set; } = ""; //A Display name for the Event Resource. This is not the name of the actual thing being targeted. For organization purposes only / never referenced to do anything.
        public string Location { get; set; } = ""; //The name of actual target File or Folder itself. IE Location. Sometimes this is only a name, and somethings it's the full location path. 
        public bool RequiredName { get; set; } = false; //Decides if the exact Local file / folder name is explicitly required or now. true or false.
        public string Key { get; set; } = ""; //a completly unique identifier.          
        public ResourceTypes ResourceType { get; set; } = ResourceTypes.File; 
        public bool IsChild { get; set; } = false; //If true, this is a child of another resource. If false, this is a root resource.
        public string ParentKey { get; set; } = ""; //IF CHILD: this is the key of the parent.     



        public enum ResourceTypes { File, Folder, WTools, CMDText }
    }

    




    public class CommandResource //In DevTools->Commands, a Command has a list of These to define what WorkshopEventResources the Command actually requires.   
    {
        public string Label { get; set; } = "Name";
        public ResourceTypes Type { get; set; } = ResourceTypes.File;
        public bool IsOptional { get; set; } = false; //Technically unused, i'm only checking if it's optional, but i never actually set any to optional. 
        public enum ResourceTypes { File, Folder, WTools, CMDText }

        public string CMDTextKey { get; set; } = ""; //The key of the resource this is filling in for. Matches EventResource Key.
    }





}
