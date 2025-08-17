using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfHexaEditor.Properties;

namespace GameEditorStudio
{
    public partial class CommandMethodsClass
    {
        public static void DoNothing(MethodData MethodData) 
        {
            
            //This is a dummy / debug method that does nothing.
            //Useful for testing the events menu without actually doing anything.
        }

        ////////////////////////////////////////////////////////////////////
        public static void OpenTool(MethodData MethodData)
        {
            //ADD ABILITY TO SELECT NO FILE TO LAUNCH!
            
            Tool toolOne = MethodData.Command.RequiredToolsList[0];
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = toolOne.Location, // Path to the executable
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            // Start the process with the configured ProcessStartInfo
            Process.Start(startInfo);
        }


        ////////////////////////////////////////////////////////////////////

        public static void Tool1File(MethodData MethodData)
        {

            //ADD ABILITY TO SELECT NO FILE TO LAUNCH!


            Command command = MethodData.Command; //eventCommand.Command;
            Tool toolOne = command.RequiredToolsList.First();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = toolOne.Location, // Path to the executable
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            if (MethodData.ResourceLocations.Count > 0)
            {
                string TheLocation = MethodData.ResourceLocations.First();
                startInfo.Arguments = $"\"{TheLocation}\"";
            }

            // Start the process with the configured ProcessStartInfo
            Process.Start(startInfo);
        }


        public static void RunProgram(MethodData MethodData)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = MethodData.ResourceLocations[0],
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            Process.Start(startInfo);
        }

        


        public static void MoveFile(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) && !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string sourceFile = MethodData.ResourceLocations[0];
                string destinationFolder = MethodData.ResourceLocations[1];
                string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
                try
                {
                    System.IO.File.Move(sourceFile, destinationFile);
                    Console.WriteLine($"File moved from {sourceFile} to {destinationFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid file or destination path.");
            }
        }

        public static void MoveFolder(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) && !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string sourceFolder = MethodData.ResourceLocations[0];
                string destinationFolder = Path.Combine(MethodData.ResourceLocations[1], new DirectoryInfo(sourceFolder).Name);
                try
                {
                    System.IO.Directory.Move(sourceFolder, destinationFolder);
                    Console.WriteLine($"Folder moved from {sourceFolder} to {destinationFolder}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving folder: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid source or destination path.");
            }
        }

        public static void CopyFile(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) && !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string sourceFile = MethodData.ResourceLocations[0];
                string destinationFolder = MethodData.ResourceLocations[1];
                string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
                try
                {
                    System.IO.File.Copy(sourceFile, destinationFile, true);
                    Console.WriteLine($"File copied from {sourceFile} to {destinationFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid file or destination path.");
            }
        }

        public static void CopyFolder(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) && !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string resource0 = MethodData.ResourceLocations[0];
                string sourceFolder = MethodData.ResourceLocations[0];
                string destinationFolder = Path.Combine(MethodData.ResourceLocations[1], new DirectoryInfo(sourceFolder).Name);
                try
                {
                    CopyDirectory(sourceFolder, destinationFolder);
                    Console.WriteLine($"Folder copied from {sourceFolder} to {destinationFolder}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying folder: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid source or destination path.");
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDir}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                string temppath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(temppath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destinationDir, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }

        public static void DeleteFile(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count > 0 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]))
            {
                string filePath = MethodData.ResourceLocations[0];
                try
                {
                    System.IO.File.Delete(filePath);  // Use System.IO.File.Delete to delete the file
                    Console.WriteLine($"File deleted: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("The Delete File Command failed.");
            }
        }

        public static void DeleteFolder(MethodData MethodData)
        {
            // Check if there are any locations specified and the first location is not empty
            if (MethodData.ResourceLocations.Count > 0 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]))
            {
                string directoryPath = MethodData.ResourceLocations[0];
                try
                {
                    System.IO.Directory.Delete(directoryPath, true);  // Deletes the directory and all subdirectories and files
                    Console.WriteLine($"Directory deleted: {directoryPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting directory: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No directory path is available to delete.");
            }
        }

        public static void OpenFolder(MethodData MethodData)
        {
            string resource0 = MethodData.ResourceLocations[0];
            if (resource0 == "") { return; }

            LibraryGES.OpenFolder(resource0);

        }


        public static void RunProgramwithfile(MethodData MethodData)
        {
            string resource0 = MethodData.ResourceLocations[0];
            string resource1 = MethodData.ResourceLocations[1];

            string FINAL1 = resource0 + resource1;

            string FINAL2 = resource0 + resource1;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = MethodData.ResourceLocations[0],
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            startInfo.Arguments = $"\"{MethodData.ResourceLocations[1]}\"";

            Process.Start(startInfo);
        }

        public static void CommandPrompt(MethodData MethodData) 
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo()
            //{
            //    FileName = "cmd.exe",  //MethodData.ResourceLocations[0],
            //    UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            //};

            string FINAL = "";

            foreach (string resource in MethodData.ResourceLocations)
            {
                if (!string.IsNullOrEmpty(resource))
                {
                    FINAL = FINAL + resource;
                }
            }

            string truefinal = FINAL;

            string resource0 = MethodData.ResourceLocations[0];
            string resource1 = MethodData.ResourceLocations[1];
            //string resource2 = MethodData.ResourceLocations[2];
            //string resource3 = MethodData.ResourceLocations[3];

            string FINAL1 = resource0 + resource1;
            //string FINAL2 = resource0 + resource1 + resource2;
            //string FINAL3 = resource0 + resource1 + resource2 + resource3;
            //string FINAL4 = resource0 + resource1 + resource2 + resource3 + resource4;

            //File.WriteAllText(resource0, resource1);
            //File.WriteAllText(@"C:\\Users\\Dawnbomb\\Downloads\\UnitiaText Tool\\F One\\MyFile.txt", resource3 + "\n" + FINAL2);

            //Process.Start(startInfo);
        }

    }
}
