using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditorStudio
{
    public partial class CommandMethodsClass
    {
        public static void DoNothing(MethodData ActionPack) 
        {
            //This is a dummy / debug method that does nothing.
            //Useful for testing the events menu without actually doing anything.
        }

        ////////////////////////////////////////////////////////////////////
        public static void OpenTool(MethodData ActionPack)
        {
            //ADD ABILITY TO SELECT NO FILE TO LAUNCH!

            Command command = ActionPack.Command; //eventCommand.Command;
            Tool toolOne = command.RequiredToolsList.First();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = toolOne.Location, // Path to the executable
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            // Start the process with the configured ProcessStartInfo
            Process.Start(startInfo);
        }


        ////////////////////////////////////////////////////////////////////

        public static void Tool1File(MethodData ActionPack)
        {

            //ADD ABILITY TO SELECT NO FILE TO LAUNCH!


            Command command = ActionPack.Command; //eventCommand.Command;
            Tool toolOne = command.RequiredToolsList.First();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = toolOne.Location, // Path to the executable
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            if (ActionPack.ResourceLocations.Count > 0)
            {
                string TheLocation = ActionPack.ResourceLocations.First();
                startInfo.Arguments = $"\"{TheLocation}\"";
            }

            // Start the process with the configured ProcessStartInfo
            Process.Start(startInfo);
        }


        public static void RunProgram(MethodData ActionPack)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = ActionPack.ResourceLocations[0],
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            Process.Start(startInfo);
        }

        public static void RunProgramwithfile(MethodData ActionPack)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = ActionPack.ResourceLocations[0],
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            startInfo.Arguments = $"\"{ActionPack.ResourceLocations[1]}\"";

            Process.Start(startInfo);
        }


        public static void MoveFile(MethodData ActionPack)
        {
            if (ActionPack.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(ActionPack.ResourceLocations[0]) && !string.IsNullOrEmpty(ActionPack.ResourceLocations[1]))
            {
                string sourceFile = ActionPack.ResourceLocations[0];
                string destinationFolder = ActionPack.ResourceLocations[1];
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

        public static void MoveFolder(MethodData ActionPack)
        {
            if (ActionPack.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(ActionPack.ResourceLocations[0]) && !string.IsNullOrEmpty(ActionPack.ResourceLocations[1]))
            {
                string sourceFolder = ActionPack.ResourceLocations[0];
                string destinationFolder = Path.Combine(ActionPack.ResourceLocations[1], new DirectoryInfo(sourceFolder).Name);
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

        public static void CopyFile(MethodData ActionPack)
        {
            if (ActionPack.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(ActionPack.ResourceLocations[0]) && !string.IsNullOrEmpty(ActionPack.ResourceLocations[1]))
            {
                string sourceFile = ActionPack.ResourceLocations[0];
                string destinationFolder = ActionPack.ResourceLocations[1];
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

        public static void CopyFolder(MethodData ActionPack)
        {
            if (ActionPack.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(ActionPack.ResourceLocations[0]) && !string.IsNullOrEmpty(ActionPack.ResourceLocations[1]))
            {
                string sourceFolder = ActionPack.ResourceLocations[0];
                string destinationFolder = Path.Combine(ActionPack.ResourceLocations[1], new DirectoryInfo(sourceFolder).Name);
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

        public static void DeleteFile(MethodData ActionPack)
        {
            if (ActionPack.ResourceLocations.Count > 0 && !string.IsNullOrEmpty(ActionPack.ResourceLocations[0]))
            {
                string filePath = ActionPack.ResourceLocations[0];
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

        public static void DeleteFolder(MethodData ActionPack)
        {
            // Check if there are any locations specified and the first location is not empty
            if (ActionPack.ResourceLocations.Count > 0 && !string.IsNullOrEmpty(ActionPack.ResourceLocations[0]))
            {
                string directoryPath = ActionPack.ResourceLocations[0];
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

    }
}
