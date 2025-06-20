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


    public static class TrueDatabase 
    {
        public static List<Tool> Tools { get; set; } = new();
        public static List<Command> Commands { get; set; } = new();
        public static List<CommonEvent> CommonEvents { get; set; } = new();

        public static List<CommonEvent> CommonEventsLocal { get; set; } = new(); //A list of every common event the user has enabled locally. 

        //Maybe a dictionary of Workshops? More then a list, something i can ID (With workshop name?)
        //I can move WorkshopName from Workshop to WorkshopData as a workshop holds it's data file anyway, and then now i can check name ID? It's an idea anyway...for making a dictionary...maybe?
        //Keep in mind... projects exist seperate from a workshop..... 
        //IMPORTENT NOTE: THE MOMENT WORKSHOPDATA IS IN HERE AS A LIST, THE WAY WORKSHOP COMMON EVENTS ARE LOADED / USED / SAVED / ETC NEED TO BE CHANGED.
        // ^ CURRENTLY WORKSHOP COMMON EVENTS ARE JUST A BOOL, SO WORKSHOPS DO NOT ACTUALLY HAVE THEIR OWN CODE INTERNAL COMMON EVENT LIST YET.

        //Moving the Events list (Currently existing in THE FUCKING MAINMENU OH GOD) to WorkshopData would also be important.
        //And that means changing ActionPacks aka MethodData class, uh, somehow, i think? (Make it so CommandMethods can reference WorkshopData / GameLibrary directly, and remove MainMenu from MethodData, but this needs events list moved out of MainMenu)
        //Also Project data might be relevant to...
    }

    public class WorkshopData //When i later make this a list in true database, remember to make sure workshop common events still load / save / use properly
    {
        
        //List of (Editor names as strings) (but actually the strings will be...lists or dictionaries?)



        public List<CommonEvent> CommonEventsWorkshop { get; set; } = new();
        public EntryManager EntryManager { get; set; } = new(); //This doesn't even need to be here. Remove it later. xd


        //Starting here is why Database isn't static yet, but i need to eventually fix this and just make the database be static... 
        //When i first made this program, a workshop held a database. 
        //Right before release i changed it that THE database is created when the program opens, but Now instead, a database exists
        public Workshop Workshop { get; set; } //Set when a workshop is opened.
        //Workshop is set when a workshop is actually opened. 
        public Dictionary<string, GameFile>? GameFiles { get; set; } = new(); //the term "File" prevents File.Read from working because it thinks "File" is a class. So i Needed another name.    
        public Dictionary<string, Editor>? GameEditors { get; set; } = new(); //Note that "Editors" is a folder name. So this is called "GameEditors".
        public List<Document> Documents { get; set; } = new();


        

    }

       

    public class GameFile //aka Database.GameEditors[X].
    {
        public string FileName { get; set; } = ""; //XML The actual name of the file
        public string FileLocation { get; set; } = ""; //XML The path from InputDirectory to the FileName.
        public string FileNote { get; set; } = ""; //XML. For games that have multiple files with identical names via being in diffrent folders, users can give files a nickname, but save as their real name.
        public string FileNotepad { get; set; } = ""; //XML (or it will be)
        public byte[] FileBytes { get; set; } //Not XML  //The actual loaded information from a file. This is what editors edit! This is the ENTIRE file, not just a table of bytes.
        //FileBytes is also reffered to in comments as "Memory File" due to it being a version of the file entirely in memory, and not saved back to the computer.
        //"Memory File" helps differenciate between the file's origonal bytes, and it's current ones after editing but before saving.
        
        
    }

    

    public class Editor 
    {       
        public Workshop Workshop { get; set; } //The workshop this editor is in.
        public string EditorName { get; set; } //Not XML (yet)
        public string EditorType { get; set; } //XML   types are DataTable and TextEditor  (convert this to enum later...?)
        public string EditorIcon { get; set; } //XML
        public string EditorKey { get; set; } //XML   This is the key to the editor, used for saving and loading. It is not the name of the editor, but a unique identifier.

        public DockPanel? EditorBackPanel { get; set; }   //The back of the editor. A user should not see this, used just for organization. Includes left bar and main editor.       
                
        public StandardEditorData StandardEditorData { get; set; } = new();
        public TextEditorData TextEditorData { get; set; }

        public List<Entry> ListOfEntrysUsingMyNames { get; set; } = new(); // Not XML  used to know the list of entrys to update? entrys that have menus who get text from this editor.


        //Button Data
        public Image EditorImage { get; set; } //fsdfs
        public Label EditorNameLabel { get; set; }
        public DockPanel EditorBarDockPanel { get; set; }
        public Label EditorLabel { get; set; } //The border around the editor button, used to make it look nice.
        public Button EditorButton { get; set; } //The button tab that opens the editor.



    }

    


    

    










}
