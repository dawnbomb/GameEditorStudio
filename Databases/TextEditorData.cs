using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GameEditorStudio
{
    public class TextEditorData : Editor
    {
        public TextEditor TextEditorXaml { get; set; }
        public List<GameFile> ListOfGameFiles { get; set; } = new(); //The file list this text editor is using. 
        public Grid MainGrid { get; set; }        //Is this just Editor Back Panel?
        public FileManager TextFileManager { get; set; }

        public List<string> GameFileLocations { get; set; } = new(); //This is a list of file locations that this editor is using. This is used to check if a file is already being used by this editor, so it doesn't get added twice.
    }
}
