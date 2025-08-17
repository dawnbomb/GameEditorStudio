using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; //end
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Security.Policy;
using System.ComponentModel.Design;
using GameEditorStudio;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using System.Windows.Shapes;

namespace GameEditorStudio
{

    public partial class CommandMethodsClass
    {
        public static void DummyCommand(MethodData MethodData)
        {

        }

        

        

        ////////////////////////////////////////////////////////////////////

        public static void DSNitroPack(MethodData MethodData)
        {
            string GameFile = "";            

            if (MethodData.ResourceLocations.Count == 0)
            {
                VistaOpenFileDialog UserSelection = new VistaOpenFileDialog();
                UserSelection.Title = "Please select the GAME.xml file in the NDS rom that will be repacked";
                if ((bool)UserSelection.ShowDialog())
                {                   
                    GameFile = UserSelection.FileName;
                }
                else
                {
                    return;
                }

            }
            else 
            {
                GameFile = MethodData.ResourceLocations[0];
            }

            if (Path.GetFileName(GameFile) != "GAME.xml")
            {
                return;
            }



            DirectoryInfo OutputFolder = new DirectoryInfo(Path.GetDirectoryName(GameFile)).Parent;

            string NitroLocation = Database.Tools.Find(thing => thing.Key == "638835886790069140-900000195-379288847").Location;
            string NitroFolder = Path.GetDirectoryName(NitroLocation);


            string packCommand = $"\"{NitroLocation}\" pack -p \"{GameFile}\" -r \"{OutputFolder}\\Game.nds\"";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = $"/C \"{packCommand}\"";
            psi.WorkingDirectory = NitroFolder;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            Process p = new Process();
            p.StartInfo = psi;
            p.Start();

            //p.WaitForExit();
            
        }
        


        public static void DSNitroUnpack(MethodData MethodData) //tool free version
        {
            string NDSRom = "";

            if (MethodData.ResourceLocations.Count == 0)
            {
                VistaOpenFileDialog fileDialog = new VistaOpenFileDialog();
                fileDialog.Title = "Select a file"; // Set the dialog title
                fileDialog.Filter = "All files (*.*)|*.*"; // Set the file filter
                if ((bool)fileDialog.ShowDialog()) // Show the file dialog and check if the user clicked OK //this
                {
                    NDSRom = fileDialog.FileName; 


                }
            }
            else 
            {
                NDSRom = MethodData.ResourceLocations[0];
            }

            //Check if actually a .nds file

            
            
            string RomFolder = Path.GetDirectoryName(NDSRom); // Get the directory path of the selected file
            string OutputFolder = RomFolder + "\\Unpacked " + Path.GetFileNameWithoutExtension(NDSRom);
            string NitroLocation = Database.Tools.Find(thing => thing.Key == "638835886790069140-900000195-379288847").Location; //Nitropacker
            string NitroFolder = Path.GetDirectoryName(NitroLocation);

            string UnpackCommand = $"\"{NitroLocation}\" unpack -r \"{NDSRom}\" -o \"{OutputFolder}\" -p GAME";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = $"/K \"{UnpackCommand}\"";
            psi.WorkingDirectory = $"{NitroFolder}";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }

        

        





    }
}
