using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for UserControlEditorGraphics.xaml
    /// </summary>
    public partial class UserControlEditorIcons : UserControl
    {
        Workshop TheWorkshop;
        Boolean IAMSETUP = false;
        UserControlEditorCreator TheEditorCreator = null; 

        public UserControlEditorIcons()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LoadEvent);
        }

        public void LoadEvent(object sender, RoutedEventArgs e) 
        {
            DependencyObject? current = this;
            while (current != null && current is not UserControlEditorCreator)
            {
                current = VisualTreeHelper.GetParent(current);
            }

            if (current is UserControlEditorCreator)
            {
                TheEditorCreator = (UserControlEditorCreator)current;
                ExitButton.Visibility = Visibility.Collapsed;
                ExitButton.IsEnabled = false;

            }


            var parentWindow = Window.GetWindow(this);
            if (parentWindow is Workshop workshopWindow)
            {
                TheWorkshop = workshopWindow;

                if (IAMSETUP == false)
                {
                    Setup();
                    IAMSETUP = true;
                }
            }

        }


        public void Setup()
        {
            string GraphicsFolder = Path.Combine(LibraryMan.ApplicationLocation, "Other\\Icons");

            string[] directories = Directory.GetDirectories(GraphicsFolder);

            foreach (string directory in directories)
            {

                //DoAThingOld(directory);
                DoAThingNew(directory);
            }
        }

        private void DoAThingNew(string directory)
        {
            string folderName = new DirectoryInfo(directory).Name;

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 139)) // Dark blue
            };

            // StackPanel to hold multiple category sections (General + subfolders)
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)) // Dark background
            };

            // Add the "General" category
            AddCategoryPanel(mainPanel, "General", Directory.GetFiles(directory, "*.png"));

            // Add subfolder categories
            string[] subfolders = Directory.GetDirectories(directory);
            foreach (string subfolder in subfolders)
            {
                string[] imageFiles = Directory.GetFiles(subfolder, "*.png");
                if (imageFiles.Length > 0)
                {
                    string subfolderName = new DirectoryInfo(subfolder).Name;
                    AddCategoryPanel(mainPanel, subfolderName, imageFiles);
                }
            }

            scrollViewer.Content = mainPanel;

            TabItem newTab = new TabItem
            {
                Header = folderName,
                Content = scrollViewer
            };

            GraphicsTabber.Items.Add(newTab);
        }

        private void AddCategoryPanel(Panel parent, string categoryName, string[] imagePaths)
        {
            // Header label
            TextBlock header = new TextBlock
            {
                Text = categoryName,
                FontSize = 16,
                Foreground = Brushes.White,
                Margin = new Thickness(10, 20, 10, 5)
            };
            parent.Children.Add(header);

            // WrapPanel to hold images
            WrapPanel wrapPanel = new WrapPanel
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            foreach (string filePath in imagePaths)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                int scale = CalculateIntegerScale(bitmap.PixelWidth, bitmap.PixelHeight, 120, 90);

                Image image = new Image
                {
                    Source = bitmap,
                    Width = bitmap.PixelWidth * scale,
                    Height = bitmap.PixelHeight * scale,
                    Stretch = Stretch.None,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true,
                    Margin = new Thickness(2),
                    MaxWidth = 120,
                    MaxHeight = 90,
                    Tag = filePath
                };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                image.Stretch = Stretch.UniformToFill;

                var border = new Border
                {
                    Child = image,
                    Width = 130,
                    Height = 100,
                    Margin = new Thickness(5)
                };

                border.MouseLeftButtonUp += (sender, e) =>
                {
                    string clickedImagePath = image.Tag as string;
                    string imageName = System.IO.Path.GetFileName(clickedImagePath);

                    if (TheEditorCreator != null)
                    {
                        ChangeDemoEditorGraphic(imageName);
                    }
                    else
                    {
                        ChangeCurrentEditorGraphic(imageName);
                    }
                };

                wrapPanel.Children.Add(border);
            }

            parent.Children.Add(wrapPanel);
        }



        private void DoAThingOld(string directory) 
        {
            string folderName = new DirectoryInfo(directory).Name;

            // Create a ScrollViewer with a dark blue background
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 139)) // DarkBlue color
            };

            // Create an ItemsControl with a WrapPanel as its items panel
            ItemsControl itemsControl = new ItemsControl();
            itemsControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(WrapPanel)));
            itemsControl.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)); // DarkBlue color

            // Get all image files in the current directory
            string[] fileEntries = Directory.GetFiles(directory, "*.png"); // Assuming you only want .png files
            foreach (string filePath in fileEntries)
            {
                // Create an Image control for each file
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                int scale = CalculateIntegerScale(bitmap.PixelWidth, bitmap.PixelHeight, 120, 90);
                Image image = new Image
                {
                    Source = bitmap,
                    Width = bitmap.PixelWidth * scale,
                    Height = bitmap.PixelHeight * scale,
                    Stretch = Stretch.None,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true, //https://learn.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.uselayoutrounding?view=windowsdesktop-9.0

                    Margin = new Thickness(2), // Add some space around the image

                    MaxWidth = 120,
                    MaxHeight = 90,
                    Tag = filePath, 

                };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                image.Stretch = Stretch.UniformToFill;

                var border = new Border
                {
                    Child = image,
                    Width = 130,
                    Height = 100,
                    Margin = new Thickness(5),
                };



                // Handle the MouseLeftButtonUp event to detect clicks
                border.MouseLeftButtonUp += (sender, e) =>
                {
                    // Retrieve the file path from the image tag
                    string clickedImagePath = image.Tag as string;
                    string imageName = System.IO.Path.GetFileName(clickedImagePath); // Extract just the file name

                    if (TheEditorCreator != null)
                    {
                        ChangeDemoEditorGraphic(imageName);
                    }
                    else
                    {
                        ChangeCurrentEditorGraphic(imageName);
                    }

                };

                // Add the image to the ItemsControl
                itemsControl.Items.Add(border);
            }

            // Set the content of the ScrollViewer to the ItemsControl
            scrollViewer.Content = itemsControl;

            // Create a TabItem with the ScrollViewer as its content
            TabItem newTab = new TabItem
            {
                Header = folderName,
                Content = scrollViewer
            };

            // Add the TabItem to the TabControl
            GraphicsTabber.Items.Add(newTab);
        }

        private int CalculateIntegerScale(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            int scaleX = maxWidth / originalWidth;
            int scaleY = maxHeight / originalHeight;
            int scale = Math.Min(scaleX, scaleY);
            return Math.Max(1, scale); // Never go below 1x
        }

        public void ChangeDemoEditorGraphic(string GraphicName)
        {
            return;

            

            //string baseGraphicsPath = Path.Combine(LibraryMan.ApplicationLocation, "Other\\Icons");
            //string[] files = Directory.GetFiles(LibraryMan.ApplicationLocation + "\\Other\\Icons\\", GraphicName, SearchOption.AllDirectories);
            //if (files.Length > 0)
            //{
            //    // File found, use the first found instance
            //    string foundFilePath = files[0];
            //    BitmapImage bitmap = new BitmapImage(new Uri(foundFilePath));
            //    TheEditorCreator.DemoEditorImage.Source = bitmap;
            //    TheEditorCreator.DemoEditorImage.Tag = GraphicName; // Store the full path as a tag, or just the file name if you prefer

            //    int scale = CalculateIntegerScale(bitmap.PixelWidth, bitmap.PixelHeight, 120, 90);
            //    TheEditorCreator.DemoEditorImage.Width = bitmap.PixelWidth * scale;
            //    TheEditorCreator.DemoEditorImage.Height = bitmap.PixelHeight * scale;
            //}

        }

        public void ChangeCurrentEditorGraphic(string GraphicName) 
        {
            TheWorkshop.EditorClass.EditorIcon = GraphicName;

            string baseGraphicsPath = Path.Combine(LibraryMan.ApplicationLocation, "Other\\Icons");
            string[] files = Directory.GetFiles(LibraryMan.ApplicationLocation + "\\Other\\Icons\\", GraphicName, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                // File found, use the first found instance
                string foundFilePath = files[0];
                TheWorkshop.EditorClass.EditorImage.Source = new BitmapImage(new Uri(foundFilePath));
            }

            TheWorkshop.UpdateEditorButton(TheWorkshop.EditorClass);
            
        }

        private void ToggleMyTabControl(object sender, RoutedEventArgs e)
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }
        }
    }
}
