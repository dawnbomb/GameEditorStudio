using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for WToolsManager.xaml
    /// </summary>
    public partial class WToolsManager : UserControl
    {
        ToolsMenu toolsmenu {  get; set; }
        List<Tool> WorkshopTools { get; set; }

        Tool? CurrentTool { get; set; } = null;
        TreeViewItem? CurrentItem { get; set; } = null;

        public WToolsManager(ToolsMenu toolsmenu)
        {
            InitializeComponent();
            this.toolsmenu = toolsmenu;
            WorkshopTools = toolsmenu.WorkshopData.WorkshopTools;
            RefreshToolsTree();


            
           
        }


        public void warningmessage() 
        {
            PixelWPF.LibraryPixel.Notification("Quick Warning...", "The Workshop Tools Manager forces you to save your changes (to file) when exiting. " +
                "\n\nI will fix this later, sorry~" +
                "\n\nNote:" +
                "\nIf you make a mistake (like axidentally deleting a workshop tool), just click X then close game editor studio and relaunch it. " +
                "\n\n(You can safely use File -> Save Everything during the closing process, just don't make any further event or tool changes until you reload GES.)" +
                " ");
        }

        private void RefreshToolsTree() 
        {
            TreeViewTools.Items.Clear();

            foreach (Tool tool in WorkshopTools) 
            {
                TreeViewItem treeViewItem = new();
                treeViewItem.Header = tool.DisplayName;
                treeViewItem.Tag = tool;
                TreeViewTools.Items.Add(treeViewItem);
            }

            if (TreeViewTools.Items.Count != 0) 
            {
                TreeViewItem Titem = TreeViewTools.Items[0] as TreeViewItem;
                if (Titem != null) { Titem.IsSelected = true; }
            }
            
        }

        private void SaveExitButtonClick(object sender, RoutedEventArgs e)
        {
            bool WantToSave = PixelWPF.LibraryPixel.NotificationConfirm("Save?", "Would you like to save the current workshop tools set?" +
                "\n\nUnlike how the rest of GES works, this ACTUALLY saves. (I will fix this later)");
            if (WantToSave == false) { return; }

            SaveSpecialWorkshopTools();
            toolsmenu.RefreshWorkshopTools();

            var parentContainer = this.Parent as Grid;
            if (parentContainer != null)
            {
                parentContainer.Children.Remove(this);
            }
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            //bool WantToSave = PixelWPF.LibraryPixel.NotificationConfirm("Exit without saving?", "" +
            //    "Just to confirm, you want to exit WITHOUT saving any changes to workshop tools. " +
            //    "\n\nIs that correct?");
            //if (WantToSave == false) { return; }

            var parentContainer = this.Parent as Grid;
            if (parentContainer != null)
            {
                parentContainer.Children.Remove(this);
            }
        }
               

        private void HelpButtonClick(object sender, RoutedEventArgs e)
        {
            PixelWPF.LibraryPixel.Notification("Non-GES Tools","Here you can assign Tools to a workshop that are not supported by GES. " +
                "This is great for workshop specific tools, like ones are made to only work with 1 specific game, " +
                "but it's also good for tools that should be, but are not yet supported by GES. " +
                "\n\nFor example, a game specific map editor tool, or a file extraction tool made for a game with a unique file type. ");
        }

        private void AddNewSpecialTool(object sender, RoutedEventArgs e)
        {
            Tool tool = new Tool();
            WorkshopTools.Add(tool);
            RefreshToolsTree();            
        }

        private void ToolTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ToolDisplayNameTextbox.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            ToolExeTextbox.IsEnabled = false;
            DescriptionTextbox.IsEnabled = false;
            DownloadLinkTextbox.IsEnabled = false;
            NotesTextbox.IsEnabled = false;
            NoToolSelectedLabel.Visibility = Visibility.Visible;


            CurrentItem = null;
            CurrentTool = null;
            SetNoTool();

            TreeViewItem TItem = TreeViewTools.SelectedItem as TreeViewItem;
            if (TItem == null)  { return;  }
            CurrentItem = TItem;

            Tool tool = TItem.Tag as Tool;
            CurrentTool = tool;

            

            SetTool();

            ToolDisplayNameTextbox.IsEnabled = true;
            DeleteButton.IsEnabled = true;
            ToolExeTextbox.IsEnabled = true;
            DescriptionTextbox.IsEnabled = true;
            DownloadLinkTextbox.IsEnabled = true;
            NotesTextbox.IsEnabled = true;
            NoToolSelectedLabel.Visibility = Visibility.Collapsed;
        }

        private void SetNoTool() 
        {            
            ToolDisplayNameTextbox.IsEnabled = false;
            ToolDisplayNameTextbox.Text = "";
            ToolExeTextbox.IsEnabled = false;
            ToolExeTextbox.Text = "";
            DescriptionTextbox.IsEnabled = false;
            DescriptionTextbox.Text = "";
            DownloadLinkTextbox.IsEnabled= false;
            DownloadLinkTextbox.Text = "";
            NotesTextbox.IsEnabled = false;
            NotesTextbox.Text = "";

            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private void SetTool() 
        {
            if (CurrentTool == null) { return; }            

            ToolDisplayNameTextbox.IsEnabled = true;
            ToolDisplayNameTextbox.Text = CurrentTool.DisplayName;
            ToolExeTextbox.IsEnabled = true;
            ToolExeTextbox.Text = CurrentTool.ExeName;
            DescriptionTextbox.IsEnabled = true;
            DescriptionTextbox.Text = CurrentTool.Description;
            DownloadLinkTextbox.IsEnabled = true;
            DownloadLinkTextbox.Text = CurrentTool.DownloadLink;
            NotesTextbox.IsEnabled = true;
            NotesTextbox.Text = CurrentTool.Notepad;

            DeleteButton.Visibility = Visibility.Visible;

        }

        private void ToolDisplayNameTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentTool == null) { return; }
            CurrentTool.DisplayName = ToolDisplayNameTextbox.Text;
            if (CurrentItem == null) { return; }
            CurrentItem.Header = CurrentTool.DisplayName;
        }

        private void ToolExeTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentTool == null) { return; }
            CurrentTool.ExeName = ToolExeTextbox.Text;
        }

        private void DescriptionTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentTool == null) { return; }
            CurrentTool.Description = DescriptionTextbox.Text;
        }

        private void LinkTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentTool == null) { return; }
            CurrentTool.DownloadLink = DownloadLinkTextbox.Text;
        }

        private void NotesTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentTool == null) { return; }
            CurrentTool.Notepad = NotesTextbox.Text;
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (CurrentTool == null) { return; }
            bool DeleteConfirm = PixelWPF.LibraryPixel.NotificationConfirm("Delete Workshop Tool?","Doing this will also instantly break any events using this tool." +
                "\n\nNote to self: " +
                "\nLater on i should add in some way to detect how many events are currently using a workshop tool, and let the user know in the delete comfirmation message.");
            if (DeleteConfirm == false) { return; }
            WorkshopTools.Remove(CurrentTool);
            RefreshToolsTree();
        }


        private void SaveSpecialWorkshopTools()
        {           

            string WorkshopName = toolsmenu.WorkshopData.WorkshopName;

            //I should not call this file "Non-GES Tools" because the name of the program could change, and make the name confusing.
            if (WorkshopTools.Count == 0) 
            {
                if (File.Exists(LibraryGES.ApplicationLocation + "\\Workshops\\" + toolsmenu.WorkshopData.WorkshopName + "\\" + "Tools.xml")) { File.Delete(LibraryGES.ApplicationLocation + "\\Workshops\\" + toolsmenu.WorkshopData.WorkshopName + "\\" + "Tools.xml"); }

                return; 
            }

            bool Failed = false; //yes this is used. WAY down below in the catch part of try catch.

            string FakeName = "";
            string RealName = "";
            SaveSpecialToolsToXML("FakeTools.xml");

            if (Failed == true) { return; }


            if (File.Exists(LibraryGES.ApplicationLocation + "\\Workshops\\" + toolsmenu.WorkshopData.WorkshopName + "\\" + "FakeTools.xml")) { File.Delete(LibraryGES.ApplicationLocation + "\\Workshops\\" + toolsmenu.WorkshopData.WorkshopName + "\\" + "FakeTools.xml"); }
                     
            SaveSpecialToolsToXML("Tools.xml");


            void SaveSpecialToolsToXML(string SaveFileName)
            {
                try
                {
                    
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + "\\Workshops\\" + toolsmenu.WorkshopData.WorkshopName + "\\" + SaveFileName, settings))
                    {
                        writer.WriteStartElement("WorkshopTools");
                        writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                        writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
                        writer.WriteElementString("Seperator", "--------------------------------------------------------------------------------------------");
                        writer.WriteElementString("Note", "This file is for all the Non-GES Tools a workshop says it needs.");
                        writer.WriteElementString("Seperator", "--------------------------------------------------------------------------------------------");

                        writer.WriteStartElement("ToolList");
                        foreach (Tool tool in WorkshopTools)
                        {
                            writer.WriteStartElement("WorkshopTool");

                            writer.WriteElementString("Name", tool.DisplayName);
                            writer.WriteElementString("Description", tool.Description);
                            writer.WriteElementString("Notepad", tool.Notepad);
                            writer.WriteElementString("Key", tool.Key);
                            //writer.WriteElementString("Category", tool.Category);
                            writer.WriteElementString("ExecutableName", tool.ExeName);
                            writer.WriteElementString("DownloadLink", tool.DownloadLink);

                            writer.WriteEndElement(); //End Tool
                        }
                        writer.WriteEndElement(); //End ToolList

                        writer.WriteEndElement(); //End Tools
                        writer.Flush(); //Ends the XML 
                    }
                }
                catch
                {
                    Failed = true;
                    PixelWPF.LibraryPixel.NotificationNegative("Error: Failed to save properly.",
                    "Something went wrong saving FakeTools.xml so saving will stop here." +
                    "\n\n" +
                    "PS: The real tools.xml is safe :)"
                    );
                }







            }
        }

        
    }
}
