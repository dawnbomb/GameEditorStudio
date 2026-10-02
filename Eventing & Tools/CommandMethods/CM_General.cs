using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfHexEditor;
using Microsoft.Win32;

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

        public static void RunNonGESTool(MethodData MethodData)
        {
            Tool toolOne = MethodData.Command.RequiredToolsList[0];
            string TheFile = MethodData.ResourceLocations[0];
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = toolOne.Location, // Path to the executable
                UseShellExecute = true       // This allows starting a process associated with a file type (when needed)
            };

            startInfo.ArgumentList.Add(TheFile);

            // Start the process with the configured ProcessStartInfo
            Process.Start(startInfo);
        }

        public static void RunNonGESToolWithFile(MethodData MethodData)
        {
            Tool toolOne = MethodData.Command.RequiredToolsList[0];

            OpenFileDialog FileDialog = new OpenFileDialog();

            if (FileDialog.ShowDialog() != true)
            {
                return;
            }

            string TheFile = FileDialog.FileName;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = toolOne.Location,
                UseShellExecute = true
            };

            startInfo.ArgumentList.Add(TheFile);

            Process.Start(startInfo);
        }

        public static void RunNonGESToolWithFolder(MethodData MethodData)
        {
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

        public static void Wait200ms(MethodData MethodData) 
        {
            System.Threading.Thread.Sleep(200);
        }

        public static void Wait500ms(MethodData MethodData)
        {
            System.Threading.Thread.Sleep(500);
        }

        public static void Wait1s(MethodData MethodData)
        {
            System.Threading.Thread.Sleep(1000);
        }



        //public static void MoveFile(MethodData MethodData)
        //{
        //    if (MethodData.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) && !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
        //    {
        //        string sourceFile = MethodData.ResourceLocations[0];
        //        string destinationFolder = MethodData.ResourceLocations[1];
        //        string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
        //        try
        //        {
        //            System.IO.File.Move(sourceFile, destinationFile, overwrite: true);
        //            Console.WriteLine($"File moved from {sourceFile} to {destinationFile}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error moving file: {ex.Message}");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("Invalid file or destination path.");
        //    }
        //}
        public static void MoveFile(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string sourceFile = MethodData.ResourceLocations[0];
                string destinationFolder = MethodData.ResourceLocations[1];
                string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));

                // Prevent operating on identical paths
                if (string.Equals(Path.GetFullPath(sourceFile), Path.GetFullPath(destinationFile), StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Source and destination paths are identical. No move needed.");
                    return;
                }

                try
                {
                    // Perform move (or copy + delete across volumes)
                    System.IO.File.Move(sourceFile, destinationFile, overwrite: true);

                    // Fallback safety check: ensure source is cleaned up
                    if (System.IO.File.Exists(sourceFile))
                    {
                        System.IO.File.Delete(sourceFile);
                    }

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

        //public static void MoveFolder(MethodData MethodData)
        //{
        //    if (MethodData.ResourceLocations.Count >= 2 && !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) && !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
        //    {
        //        string sourceFolder = MethodData.ResourceLocations[0];
        //        string destinationFolder = Path.Combine(MethodData.ResourceLocations[1], new DirectoryInfo(sourceFolder).Name);                
        //        try
        //        {
        //            // If destination directory already exists, remove it before moving
        //            if (Directory.Exists(destinationFolder))
        //            {
        //                Directory.Delete(destinationFolder, recursive: true);
        //            }

        //            System.IO.Directory.Move(sourceFolder, destinationFolder);
        //            Console.WriteLine($"Folder moved from {sourceFolder} to {destinationFolder}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error moving folder: {ex.Message}");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("Invalid source or destination path.");
        //    }
        //}
        public static void MoveFolder(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string sourceFolder = MethodData.ResourceLocations[0];
                string destinationFolder = Path.Combine(MethodData.ResourceLocations[1], new DirectoryInfo(sourceFolder).Name);

                try
                {
                    if (!Directory.Exists(sourceFolder))
                    {
                        Console.WriteLine($"Source folder does not exist: {sourceFolder}");
                        return;
                    }

                    // Stack to process subdirectories iteratively (no helper method needed)
                    Stack<(string Source, string Destination)> foldersToProcess = new();
                    foldersToProcess.Push((sourceFolder, destinationFolder));

                    while (foldersToProcess.Count > 0)
                    {
                        var (currentSource, currentDest) = foldersToProcess.Pop();

                        // 1. Create target directory if it doesn't exist
                        Directory.CreateDirectory(currentDest);

                        // 2. Copy/overwrite all files in the current folder
                        foreach (string sourceFile in Directory.GetFiles(currentSource))
                        {
                            string destFile = Path.Combine(currentDest, Path.GetFileName(sourceFile));
                            File.Copy(sourceFile, destFile, overwrite: true);
                        }

                        // 3. Push subdirectories onto the stack for processing
                        foreach (string subDir in Directory.GetDirectories(currentSource))
                        {
                            string subDirName = Path.GetFileName(subDir);
                            foldersToProcess.Push((subDir, Path.Combine(currentDest, subDirName)));
                        }
                    }

                    // 4. Delete original source folder after all files are safely merged
                    Directory.Delete(sourceFolder, recursive: true);

                    Console.WriteLine($"Folder merged and moved from {sourceFolder} to {destinationFolder}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving (merging) folder: {ex.Message}");
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

        private static void RenameFile(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string sourceFile = MethodData.ResourceLocations[0];
                string newName = MethodData.ResourceLocations[1];

                // Ensure we get the directory path so the file stays in the same folder
                string directory = Path.GetDirectoryName(sourceFile) ?? string.Empty;
                string destinationFile = Path.Combine(directory, newName);

                try
                {
                    if (File.Exists(sourceFile))
                    {
                        File.Move(sourceFile, destinationFile, overwrite: true);
                        Console.WriteLine($"File renamed from {sourceFile} to {destinationFile}");
                    }
                    else
                    {
                        Console.WriteLine($"Source file does not exist: {sourceFile}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renaming file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid file or new name path.");
            }
        }

        private static void RenameFolder(MethodData MethodData)
        {
            if (MethodData.ResourceLocations.Count >= 2 &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[0]) &&
                !string.IsNullOrEmpty(MethodData.ResourceLocations[1]))
            {
                string sourceFolder = MethodData.ResourceLocations[0];
                string newName = MethodData.ResourceLocations[1];

                // Get parent directory to rename folder in-place
                DirectoryInfo directoryInfo = new DirectoryInfo(sourceFolder);
                string parentDirectory = directoryInfo.Parent?.FullName ?? string.Empty;
                string destinationFolder = Path.Combine(parentDirectory, newName);

                try
                {
                    if (Directory.Exists(sourceFolder))
                    {
                        Directory.Move(sourceFolder, destinationFolder);
                        Console.WriteLine($"Folder renamed from {sourceFolder} to {destinationFolder}");
                    }
                    else
                    {
                        Console.WriteLine($"Source folder does not exist: {sourceFolder}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renaming folder: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid source or new folder name path.");
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

        public static void OpenFileFolder(MethodData MethodData)
        {
            string resource0 = MethodData.ResourceLocations[0];
            if (resource0 == "") { return; }

            LibraryGES.OpenFileFolder(resource0);

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

        public static void CommandPromptStayOpen(MethodData MethodData) 
        {
            //This Command needs to sync the FINAL string with the button in eventing so people can trust that the button is giving accurate information.

            string FINAL = "";

            foreach (string resource in MethodData.ResourceLocations)
            {
                if (!string.IsNullOrEmpty(resource))
                {
                    string astring = resource;
                    astring = LibraryGES.PathQuoter(astring);

                    if (astring == "WTOOL")
                    {
                        FINAL = FINAL + "\"" + LibraryGES.ApplicationLocation + "\\Workshops\\" + MethodData.WorkshopData.WorkshopName + "\\Tools\\";
                    }
                    else
                    {
                        FINAL = FINAL + astring + " ";
                    }

                }
            }

            string testa = FINAL;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/k \"" + FINAL + "\"",   // or "/c" if you want it to close after running
                UseShellExecute = true
            };            

            Process process = Process.Start(startInfo);
            process?.WaitForExit();
        }

        public static void CommandPromptAutoClose(MethodData MethodData)
        {
            //This Command needs to sync the FINAL string with the button in eventing so people can trust that the button is giving accurate information.

            string FINAL = "";

            foreach (string resource in MethodData.ResourceLocations)
            {
                if (!string.IsNullOrEmpty(resource))
                {
                    string astring = resource;
                    astring = LibraryGES.PathQuoter(astring);

                    if (astring == "WTOOL")
                    {
                        FINAL = FINAL + "\"" + LibraryGES.ApplicationLocation + "\\Workshops\\" + MethodData.WorkshopData.WorkshopName + "\\Tools\\";
                    }
                    else
                    {
                        FINAL = FINAL + astring + " ";
                    }

                }
            }

            string testa = FINAL;
            
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/c \"" + FINAL + " && exit || pause\"",   // or "/c" if you want it to close after running
                UseShellExecute = true
            };            

            Process process = Process.Start(startInfo);
            process?.WaitForExit();
        }

        public static void CommandPromptHidden(MethodData MethodData)
        {
            //This Command needs to sync the FINAL string with the button in eventing so people can trust that the button is giving accurate information.

            string FINAL = "";

            foreach (string resource in MethodData.ResourceLocations)
            {
                if (!string.IsNullOrEmpty(resource))
                {
                    string astring = resource;
                    astring = LibraryGES.PathQuoter(astring);

                    if (astring == "WTOOL")
                    {
                        FINAL = FINAL + "\"" + LibraryGES.ApplicationLocation + "\\Workshops\\" + MethodData.WorkshopData.WorkshopName + "\\Tools\\";
                    }
                    else
                    {
                        FINAL = FINAL + astring + " ";
                    }

                }
            }

            string testa = FINAL;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/k \"" + FINAL + "\"",   // or "/c" if you want it to close after running
                UseShellExecute = true
            };
            Process.Start(startInfo);

            //WARNING: WTF do i do about waiting for CMD to close (IE "Finish) if CMD is hidden??
            //Solve this before i re-add support for this!!!
        }

        

    }
}
