using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameEditorStudio
{
    public partial class CommandMethodsClass
    {
        public static void EtrianOdysseyCollectionPatchCatalogJson(MethodData MethodData)
        {            
            if (MethodData.ResourceLocations.Count > 0 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]))
            {
                string catalog = MethodData.ResourceLocations[0];
                string directoryPathOLD = MethodData.ResourceLocations[0];
                string catalogOLD = directoryPathOLD + "\\Etrian Odyssey_Data\\StreamingAssets\\aa\\catalog.json";

                string ToolExe = Database.Tools.Find(thing => thing.Key == "642-639235334334625086-862292529-487").Location; //Adressable Tools (Example.exe)  
                string ToolFolder = LibraryGES.GetFolderFromFilepath(ToolExe);


                string CMDText = $"\"{ToolExe}\" patchcrc \"{catalog}\"";

                if (File.Exists(catalog)) 
                {
                    //string packCommand = $"\"{NitroLocation}\" pack -p \"{GameFile}\" -r \"{OutputFolder}\\Game.nds\"";
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/C \"{CMDText}\"";
                    //Using /C means runs this command in CMD then exit.
                    //Using /K means run this command, then keep CMD open.
                    //psi.WorkingDirectory = NitroFolder;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    

                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();

                    p.WaitForExitAsync();                    
                }

                Task BackgroundTask = Task.Run(() =>
                {
                    Thread.Sleep(3000);

                    
                    string catalogOLD = LibraryGES.GetFolderFromFilepath(catalog) + "\\catalog.json.old";
                    if (File.Exists(catalogOLD))
                    {
                        File.Delete(catalogOLD);
                    }
                });



            }
            else
            {
                Console.WriteLine("Etrian Json Patch: No directory path is available.");
            }
        }


        //public static void EtrianOdysseyUnpackUnityBundle(MethodData MethodData)
        //{
        //    if (MethodData.ResourceLocations.Count > 0 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]))
        //    {
        //        string directoryPath = MethodData.ResourceLocations[0];
        //        string catalog = directoryPath + "\\Etrian Odyssey_Data\\StreamingAssets\\aa\\catalog.json";

        //        string ToolExe = Database.Tools.Find(thing => thing.Key == "642-639235334334625086-862292529-487").Location; //Adressable Tools (Example.exe)  
        //        string ToolFolder = LibraryGES.GetFolderFromFilepath(ToolExe);


        //        string CMDText = $"\"{ToolExe}\" patchcrc \"{catalog}\"";

        //        if (File.Exists(catalog))
        //        {
        //            //string packCommand = $"\"{NitroLocation}\" pack -p \"{GameFile}\" -r \"{OutputFolder}\\Game.nds\"";
        //            ProcessStartInfo psi = new ProcessStartInfo();
        //            psi.FileName = "cmd.exe";
        //            psi.Arguments = $"/C \"{CMDText}\"";
        //            //Using /C means runs this command in CMD then exit.
        //            //Using /K means run this command, then keep CMD open.
        //            //psi.WorkingDirectory = NitroFolder;
        //            psi.CreateNoWindow = true;
        //            psi.UseShellExecute = false;


        //            Process p = new Process();
        //            p.StartInfo = psi;
        //            p.Start();

        //            p.WaitForExitAsync();
        //        }

        //        Task BackgroundTask = Task.Run(() =>
        //        {
        //            Thread.Sleep(3000);

        //            string catalogOLD = directoryPath + "\\Etrian Odyssey_Data\\StreamingAssets\\aa\\catalog.json.old";
        //            if (File.Exists(catalogOLD))
        //            {
        //                File.Delete(catalogOLD);
        //            }
        //        });



        //    }
        //    else
        //    {
        //        Console.WriteLine("Etrian Json Patch: No directory path is available.");
        //    }
        //}


    }
}
