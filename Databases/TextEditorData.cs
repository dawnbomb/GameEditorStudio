using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GameEditorStudio
{
    public class TextEditorData
    {
        public GameFile EditorFile { get; set; } //Just a shortcut to the file, to cleanup code, and possibly processing time? IDK im to dumb to get what causes lag.                
        public string FileLocation { get; set; } //the path / directory to the text itself.
        public string TheText { get; set; }

        public Grid MainGrid { get; set; }

        public List<GameFile> ListOfGameFiles { get; set; } = new();
        public FileManager TextFileManager { get; set; }


    }
}
