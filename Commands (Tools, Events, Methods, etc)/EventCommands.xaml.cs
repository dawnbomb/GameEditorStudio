using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for EventCommands.xaml
    /// </summary>
    public partial class CommandsWindow : Window
    {
        public delegate void CommandAddedHandler(Command command, int insertIndex);
        public event CommandAddedHandler CommandAdded;

        private int InsertIndex;

        public CommandsWindow(int insertIndex) //, SharedMenus SharedMenus, string TheWorkshopName
        {
            InitializeComponent();
            InsertIndex = insertIndex;

            TabControl TheTabControl = new();
            TheTabControl.TabStripPlacement = Dock.Left;
            TheTabControl.BorderBrush = null;
            MainDockPanel.Children.Add(TheTabControl);

            CreateTabControlTabs(TheTabControl);
            SetupGroups(TheTabControl);
        }

        private void EventPanel_MouseEnter(object sender, MouseEventArgs e, Command Command)
        {
            ToolNameBox.Text = Command.DisplayName;
            ToolDescriptionBox.Text = Command.Description;
            //ToolLocationBox.Text = Tool.Location;
        }


        public void CreateTabControlTabs(TabControl TheTabControl)
        {
            HashSet<string> createdTabs = new HashSet<string>();

            foreach (Command Command in TrueDatabase.Commands)
            {
                // Check if the tab has already been created, and if so, continue to the next command
                if (createdTabs.Contains(Command.Category))
                    continue;

                // Create a new tab item if it's a new tab type
                TabItem TabItem = new TabItem();
                TabItem.Header = $"{Command.Category}";
                TabItem.MinWidth = 90;
                TheTabControl.Items.Add(TabItem);

                // Mark this tab as created
                createdTabs.Add(Command.Category);

                ScrollViewer ScrollViewer = new ScrollViewer();
                TabItem.Content = ScrollViewer;

                Grid MainTabGrid = new Grid();
                ScrollViewer.Content = MainTabGrid;
                MainTabGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10170d")); // Dark Green: 10170d
                MainTabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainTabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                DockPanel LeftDock = new DockPanel();
                LeftDock.Name = "LeftDock";
                Grid.SetColumn(LeftDock, 0);
                MainTabGrid.Children.Add(LeftDock);

                DockPanel RightDock = new DockPanel();
                RightDock.Name = "RightDock";
                Grid.SetColumn(RightDock, 1);
                MainTabGrid.Children.Add(RightDock);
            }
        }


        public void SetupGroups(TabControl TheTabControl)
        {
            foreach (TabItem Tab in TheTabControl.Items)
            {
                ScrollViewer ScrollViewer = (ScrollViewer)Tab.Content;
                Grid MainTabGrid = (Grid)ScrollViewer.Content;
                DockPanel LeftDock = null;
                DockPanel RightDock = null;

                foreach (var child in MainTabGrid.Children)
                {
                    if (child is DockPanel dockPanel)
                    {
                        if (dockPanel.Name == "LeftDock")
                        {
                            LeftDock = dockPanel;
                        }
                        else if (dockPanel.Name == "RightDock")
                        {
                            RightDock = dockPanel;
                        }
                    }
                }

                List<DockPanel> DockList = new();
                bool even = true;


                foreach (Command Command in TrueDatabase.Commands)
                {

                    if (Tab.Header.ToString() == Command.Category)
                    {
                        if (Command.Group == null || Command.Group == "") { continue; }

                        if (!DockList.Any(dockPanel => dockPanel.Name == Command.Group))
                        {
                            Border Border = new();
                            if (even == true) { LeftDock.Children.Add(Border); even = false; }
                            else { RightDock.Children.Add(Border); even = true; }
                            //Border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1d0d29")); //Black: FF191919    Dark Green: 10170d   FF101015
                            Border.BorderThickness = new Thickness(1);
                            Border.CornerRadius = new CornerRadius(8);
                            Border.Margin = new Thickness(5);
                            DockPanel.SetDock(Border, Dock.Top);
                            //BorderThickness="1"  CornerRadius="8,8,8,8" DockPanel.Dock="Top" Margin="5,5,5,5"

                            DockPanel GroupPanel = new();

                            GroupPanel.Name = Command.Group.ToString();
                            GroupPanel.LastChildFill = false;
                            GroupPanel.Margin = new Thickness(0, 5, 0, 5);
                            DockList.Add(GroupPanel);
                            Border.Child = GroupPanel;
                            //GroupPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1c453a")); //Black: FF191919    Dark Green: 10170d   FF101015
                            GroupPanel.Background = null;

                            Label Label = new();
                            Label.Content = Command.Group.ToString();
                            GroupPanel.Children.Add(Label);
                            DockPanel.SetDock(Label, Dock.Top);
                        }

                        Button Button2 = new();
                        Button2.Content = Command.DisplayName;
                        Button2.Margin = new Thickness(10, 0, 10, 4);
                        DockPanel.SetDock(Button2, Dock.Top);
                        Button2.MouseEnter += (sender, e) => EventPanel_MouseEnter(sender, e, Command); //////////The hover description box.

                        Button2.Click += (sender, e) =>
                        {
                            CommandAdded?.Invoke(Command, InsertIndex);
                            this.Close();
                        };


                        DockPanel FoundDockPanel = DockList.FirstOrDefault(dp => dp.Name == Command.Group.ToString());
                        FoundDockPanel.Children.Add(Button2);


                        //break;
                    }
                }




            }

            //Make groupings first. Then populate them?
            //ForEach Tab, Make groups by making one for every SubType for every Command, so long as the Command is of Type (TabName).
        }

        


















        private void AddCommandRunMelonDS(object sender, RoutedEventArgs e)
        {
            //var command = // Retrieve or create the specific command instance
            //CommandAdded?.Invoke(command, InsertIndex);
            //this.Close();
        }

        private void AddCommandOpenHxDHexEditor(object sender, RoutedEventArgs e)
        {
            //CommandAdded?.Invoke(CName.RunHxDHexEditor, InsertIndex);
            //this.Close();
        }
    }
}
