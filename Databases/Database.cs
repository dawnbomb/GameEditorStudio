using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Xml;

namespace GameEditorStudio
{

    // This is basically a massive list of most variables to do with a workshop.
    // More specificly, it works a workshop into editors, then stores everything about every single editor.


    public static class Database 
    {
        public static GES GESMain { get; set; } //The main window.


        public static List<Tool> Tools { get; set; } = new();
        public static List<Command> Commands { get; set; } = new();
        public static List<CommonEvent> CommonEvents { get; set; } = new(); //WORKSHOP COMMONS?

        public static List<CommonEvent> CommonEventsLocal { get; set; } = new(); //A list of every common event the user has enabled locally. 

        public static List<WorkshopData> Workshops { get; set; } = new(); //A list of all workshops that exist. This is used to load the workshop list in the main menu.

        public static GameLibrary GameLibrary { get; set; } //I just wanted a easy reference here. I can see problems with this later, but thats a problem for future me. HI FUTURE ME! :D

        //Maybe a dictionary of Workshops? More then a list, something i can ID (With workshop name?)
        //I can move WorkshopName from Workshop to WorkshopData as a workshop holds it's data file anyway, and then now i can check name ID? It's an idea anyway...for making a dictionary...maybe?
        //Keep in mind... projects exist seperate from a workshop..... 
        //IMPORTENT NOTE: THE MOMENT WORKSHOPDATA IS IN HERE AS A LIST, THE WAY WORKSHOP COMMON EVENTS ARE LOADED / USED / SAVED / ETC NEED TO BE CHANGED.
        // ^ CURRENTLY WORKSHOP COMMON EVENTS ARE JUST A BOOL, SO WORKSHOPS DO NOT ACTUALLY HAVE THEIR OWN CODE INTERNAL COMMON EVENT LIST YET.

        //Moving the Events list (Currently existing in THE FUCKING MAINMENU OH GOD) to WorkshopData would also be important.
        //And that means changing ActionPacks aka MethodData class, uh, somehow, i think? (Make it so CommandMethods can reference WorkshopData / GameLibrary directly, and remove MainMenu from MethodData, but this needs events list moved out of MainMenu)
        //Also Project data might be relevant to...
    }

    public class Project //The root project data itself. 
    {
        public Version CreatedVersion { get; set; } = new Version(0, 0); //The GES version this was first created with.
        public string CreatedDate { get; set; } = ""; //The date this was first created.
        public Version SavedVersion { get; set; } = new Version(0, 0); //The last GES version this was used/saved with.
        public string SavedDate { get; set; } = ""; // The date this was last used/saved.


        /*public ICommand ButtonCommand { get; set; }*/
        public string ProjectName { get; set; } = "New Project";
        public string ProjectInputDirectory { get; set; }
        public string ProjectOutputDirectory { get; set; }
        public List<ProjectEventResource> ProjectEventResources { get; set; } = new();
        public List<Document> ProjectDocumentsList { get; set; } = new(); //In the future, move Project Documents to here, from WorkshopData. 
    }

    public class WorkshopData //When i later make this a list in true database, remember to make sure workshop common events still load / save / use properly
    {
        public EntryManager EntryManagerOLD { get; set; } = new(); //This doesn't even need to be here. Remove it later. xd

        public Version CreatedVersion { get; set; } = new Version(0, 0); //The GES version this was first created with.
        public string CreatedDate { get; set; } = ""; //The date this was first created.
        public Version SavedVersion { get; set; } = new Version(0, 0); //The last GES version this was used/saved with.
        public string SavedDate { get; set; } = ""; // The date this was last used/saved.

        ///////////////////////////////GAME LIBRARY INFO AKA BASICS///////////////////////////////////////////////////////////////////
        public string WorkshopName { get; set; } = ""; //The name of the workshop (IE name of whats selected in Game Library)
        public string WorkshopInputDirectory { get; set; } = ""; //The intended InputDirectory (Folder name) for modding this game. This helps make sure end users aren't guessing what the correct one is.
        public bool ProjectsRequireSameFolderName { get; set; } = true; //If true the project input folder must have the same name as WorkshopInputDirectory.

        public Intro Intro { get; set; } = new ();
        public List<EventResource> WorkshopEventResources { get; set; } = new();
        public List<Project> ProjectsList { get; set; } = new();
        public List<CommonEvent> WorkshopCommonEvents { get; set; } = new(); //WORKSHOP COMMONS?
        public List<Event> WorkshopEvents { get; set; } = new();


        //////////////////////////////ADVANCED INFO AFTER OPENING WORKSHOP/////////////////////////////////////////////////////////////
        public Workshop WorkshopXaml { get; set; } //Set when a workshop is actually opened.
        public List<GameFile> GameFiles { get; set; } = new(); //The name "File" prevents File.Read from working. Anyway, this is every file in the workshop.    
        public List<Editor> GameEditors { get; set; } = new(); //Note that "Editors" is a folder name. So this is called "GameEditors".   
        public Project ?SelectedProject { get; set; } // The currently selected project in the home tab. Events use the SELECTED project rather then the LOADED one. 
        public Project ?LoadedProject { get; set; } // The current project whose game files are loaded into the workshop and editors.
        public bool IsProjectLoaded { get; set; } = false; //This bool makes it easy to track code diffrences between when a project is, or is not, currently loaded.        


        //////////////////////////////FOR LATER/////////////////////////////////////////////////////////////

        public List<Document> WorkshopDocumentsList { get; set; } = new();
        public List<Document> ProjectDocumentsList { get; set; } = new(); //Loads when a project loads. (Including load project button in home tab).
        public Document ?CurrentDocument { get; set; }
    }

    public class Intro 
    {
        public string DefaultIntroText { get; } = "This is the intro text for this workshop. It can be used to give users important information about modding this game!";
        public string IntroText { get; set; } = ""; //The intro text for the workshop. This is shown in the Game Library when the workshop is selected.
        
    }

    public class GameFile //aka Database.GameEditors[X].
    {
        public string FileName { get; set; } = ""; //XML The actual name of the file
        public string FileLocation { get; set; } = ""; //XML The path from InputDirectory to the FileName.
        public string FileNote { get; set; } = ""; //XML. For games that have multiple files with identical names via being in diffrent folders, users can give files a nickname, but save as their real name.
        public string FileWorkshopTooltip { get; set; } = ""; //XML (or it will be)
        public byte[] FileBytes { get; set; } //Not XML  //The actual loaded information from a file. This is what editors edit! This is the ENTIRE file, not just a table of bytes.
        //FileBytes is also reffered to in comments as "Memory File" due to it being a version of the file entirely in memory, and not saved back to the computer.
        //"Memory File" helps differenciate between the file's origonal bytes, and it's current ones after editing but before saving.
        
        
    }

    

    public abstract class Editor //Data that every editor has. 
    {
        public Workshop WorkshopXaml { get; set; } //A backlink to the workshop this editor is in.   
        public WorkshopData WorkshopData { get; set; } 

        //Editor Specific  Also i eventually need to make some enum to explicitly state what kind of editor this is... 
        //Actually it does exist, it'e the EditorType above. Clean this up later...
        public DataTableEditorData DataTableEditorData { get; set; }  //I should remove this as it's already an editor, but 500, 293, 199, 99 references is a lot... Remove this later >_>

        public string EditorName { get; set; } //Not USED in XML (yet)
        public string EditorKey { get; set; } = PixelWPF.LibraryPixel.GenerateKey();  //XML   This is the key to the editor, used for saving and loading. It is not the name of the editor, but a unique identifier.
        public UIElement EditorVisual { get; set; } //The Xaml of the editor. IE the actual visual element. 

        //Editor Tab Data
        public Button ?EditorTab { get; set; } //This being nullable IS used.
        public Label EditorTabNameLabel { get; set; } //The label inside the Editor Tab. 
        public Image EditorTabImage { get; set; } //An old mechanic that i may re-add at a later time. Mostly dummied out for now.
        public string EditorIcon { get; set; } //XML


        //Meta Data
        public Version CreatedVersion { get; set; } = new Version(0, 0); //The GES version this was first created with.
        public string CreatedDate { get; set; } = ""; //The date this was first created.
        public Version SavedVersion { get; set; } = new Version(0, 0); //The last GES version this was used/saved with.
        public string SavedDate { get; set; } = ""; // The date this was last used/saved.
    }

    


    

    










}
