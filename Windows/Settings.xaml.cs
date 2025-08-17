using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace GameEditorStudio
{

    public class ColorTheme
    {
        public ColorTheme Clone() //Used to make new themes. The new this starts as a copy of the current theme.
        {
            return (ColorTheme)this.MemberwiseClone();
        }

        public string Name { get; set; } = "New Theme";

        public List<Element> ElementList { get; set; } = new();

        public Element Application { get; private set; }
        public Element ContentArea { get; private set; }
        public Element ContentBar { get; private set; }
        public Element Textbox { get; private set; }
        public Element TextboxHighlight { get; private set; }
        public Element Menu { get; private set; }
        public Element MenuMouseover { get; private set; }
        public Element Button { get; private set; }
        public Element ButtonMouseover { get; private set; }
        public Element ButtonDown { get; private set; }
        public Element Checkbox { get; private set; }        
        public Element DropDown { get; private set; }
        public Element DropDownMouseover { get; private set; }
        public Element List { get; private set; }
        public Element ListMouseover { get; private set; }
        public Element ListSelected { get; private set; }

        public Element Entry { get; private set; }
        public Element EntrySelected { get; private set; }
        public Element HiddenEntry { get; private set; }
        public Element HiddenSelectedEntry { get; private set; }


        public ColorTheme()
        {
            //Instead of loading element names from xml, i force specific names. I forget why but whatever.  
            Application = new Element { Name = "Application", HasOther = true, Note = "Other is top area.", };
            ContentArea = new Element { Name = "Content Area", HasText = false, HasOther = true, Note = "Other is top color for depth."};
            ContentBar = new Element { Name = "Content Bar", HasText = false, Note="Border is only below the bar." };
            Textbox = new Element { Name = "Textbox", };
            TextboxHighlight = new Element { Name = "Text Highlight", HasText = false, HasBorder = false, };
            Menu = new Element { Name = "Menu", };
            MenuMouseover = new Element { Name = "Menu Mouseover", HasBorder = false, };
            Button = new Element { Name = "Button", };
            ButtonMouseover = new Element { Name = "Button Mouseover", };
            ButtonDown = new Element { Name = "Button Down", };
            Checkbox = new Element { Name = "Checkbox", };
            DropDown = new Element { Name = "DropDown", };
            DropDownMouseover = new Element { Name = "DropDown Mouseover",  };
            List = new Element { Name = "List", };
            ListMouseover = new Element { Name = "List Mouseover", };
            ListSelected = new Element { Name = "List Selected", };
            Entry = new Element { Name = "Entry", HasText=false, };
            EntrySelected = new Element { Name = "Entry Selected", HasText = false, };
            HiddenEntry = new Element { Name = "Hidden Entry", HasText = false, };
            HiddenSelectedEntry = new Element { Name = "Hidden Entry Selected", HasText = false, };


            // Populate the list
            ElementList.Add(Application);
            ElementList.Add(ContentArea);
            ElementList.Add(ContentBar);
            ElementList.Add(Textbox);
            ElementList.Add(TextboxHighlight);
            ElementList.Add(Menu);
            ElementList.Add(MenuMouseover);
            ElementList.Add(Button);
            ElementList.Add(ButtonMouseover);
            ElementList.Add(ButtonDown);
            ElementList.Add(Checkbox);
            ElementList.Add(DropDown);
            ElementList.Add(DropDownMouseover);
            ElementList.Add(List);
            ElementList.Add(ListMouseover);
            ElementList.Add(ListSelected);
            ElementList.Add(Entry);
            ElementList.Add(EntrySelected);
            ElementList.Add(HiddenEntry);
            ElementList.Add(HiddenSelectedEntry);

        }
    }


    //Datagrid?
    //Borderstyle?
    //Checkbox?
    //Scrollbars? (scrollviewer, datagrid, treeviews, dropdowns?, etc)
    //richtext
    //tabcontrol & items
    //title bar
    //

    public class Element
    {
        public string Name { get; set; } = "";
        public string Note { get; set; } = "";

        public string Text { get; set; } = "#000000";
        public string Back { get; set; } = "#000000";
        public string Border { get; set; } = "#000000";
        public string Other { get; set; } = "#000000";
        public bool HasText { get; set; } = true;
        public bool HasBack { get; set; } = true;
        public bool HasBorder { get; set; } = true;
        public bool HasOther { get; set; } = false;        
    }



    public partial class UserSettings : Window
    {
        public UserSettings()
        {
            InitializeComponent();

            foreach (ColorTheme Theme in LibraryGES.ColorThemeList)
            {
                TreeViewItem Item = new();
                Item.Header = Theme.Name;
                Item.Tag = Theme;
                ColorThemeTree.Items.Add(Item);
            }

            if (ColorThemeTree.Items.Count != 0) 
            {
                TreeViewItem TheItem = ColorThemeTree.Items.GetItemAt(0) as TreeViewItem;
                TheItem.IsSelected = true;
            }
            


        }

        private void UpdateDataGrid(ColorTheme theme)
        {
            ColorDataTable.Columns.Clear();
            ColorDataTable.Items.Clear();

            // Define columns
            ColorDataTable.Columns.Add(new DataGridTextColumn { Header = "Element Name", Binding = new Binding("Name"), Width = 150 });

            // Text color column with buttons
            var textColumn = new DataGridTemplateColumn { Header = "Text", Width = 110 };
            textColumn.CellTemplate = GenerateTemplate("Text");
            ColorDataTable.Columns.Add(textColumn);

            // Background color column with buttons
            var backColumn = new DataGridTemplateColumn { Header = "Back", Width = 110 };
            backColumn.CellTemplate = GenerateTemplate("Back");
            ColorDataTable.Columns.Add(backColumn);

            // Border color column with buttons
            var BorderColumn = new DataGridTemplateColumn { Header = "Border", Width = 110 };
            BorderColumn.CellTemplate = GenerateTemplate("Border");
            ColorDataTable.Columns.Add(BorderColumn);

            // Border color column with buttons
            var OtherColumn = new DataGridTemplateColumn { Header = "Other", Width = 110 };
            OtherColumn.CellTemplate = GenerateTemplate("Other");
            ColorDataTable.Columns.Add(OtherColumn);

            var notesColumn = new DataGridTextColumn
            {
                Header = "Notes",
                Binding = new Binding("Note"),
                MinWidth = 100
            };
            ColorDataTable.Columns.Add(notesColumn);

            // Populate rows from the list of elements in the theme
            foreach (var element in theme.ElementList)
            {
                ColorDataTable.Items.Add(element);
            }
        }

        private DataTemplate GenerateTemplate(string property)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(Button));

            factory.SetValue(Button.MinHeightProperty, 26.0);

            // Set the content to display color or "None"
            factory.SetBinding(Button.ContentProperty, new Binding(property)
            {
                Converter = new ColorTextConverter()
            });

            // Bind the background color to the property using a converter
            factory.SetBinding(Button.BackgroundProperty, new Binding(property)
            {
                Converter = new StringToBrushConverter(),
                Mode = BindingMode.OneWay
            });

            factory.SetBinding(Button.VisibilityProperty, new Binding($"Has{property}")
            {
                Converter = new BooleanToVisibilityConverter()
            });

            // Add click event handler for picking a color
            factory.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => {
                Button button = sender as Button;
                Element element = ((FrameworkElement)sender).DataContext as Element;
                string currentColor = element.GetType().GetProperty(property).GetValue(element) as string ?? "None";
                var color = PickColor(currentColor);
                if (!string.IsNullOrEmpty(color) && color != "None")
                {
                    element.GetType().GetProperty(property).SetValue(element, color);
                    button.Content = color;
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                    SetButtonTextColor(button);
                    SaveTheme();
                }
            }));

            factory.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, e) =>
            {
                if (sender is Button button)
                    SetButtonTextColor(button);
            }));

            template.VisualTree = factory;

            return template;
        }

        private void SetButtonTextColor(Button button)
        {
            if (button.Background is SolidColorBrush brush)
            {
                var color = brush.Color;

                double r = color.R / 255.0;
                double g = color.G / 255.0;
                double b = color.B / 255.0;

                // Simple average brightness (not perceptual)
                double brightness = (r + g + b) / 3;

                if (color.A == 0)
                {
                    button.Foreground = Brushes.White;
                    return;
                }
                double yellowAmount = Math.Min(r, g) - b; // Rough measure of yellowness
                if (yellowAmount >= 0.6) // 60% threshold
                {
                    button.Foreground = Brushes.Black;
                    return;
                }

                // Threshold: if brightness is 0.75 or more, use black text; else white
                if (brightness >= 0.75)
                    button.Foreground = Brushes.Black;
                else
                    button.Foreground = Brushes.White;
            }
        }





        private string PickColor(string currentColor)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (!string.IsNullOrEmpty(currentColor) && currentColor != "None" && System.Drawing.ColorTranslator.FromHtml(currentColor) is System.Drawing.Color existingColor)
            {
                colorDialog.Color = existingColor;
            }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                return $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
            }

            return null;
        }

        public class ColorTextConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (string.IsNullOrEmpty(value as string) || value as string == "None")
                    return "None";
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }






        private void ColorThemeTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem Item = ColorThemeTree.SelectedItem as TreeViewItem;    
            ColorTheme Theme = Item.Tag as ColorTheme;
            UpdateDataGrid(Theme);
            LibraryGES.SwitchToColorTheme(Theme);
        }


        private void ButtonNewThemeClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem Item = ColorThemeTree.SelectedItem as TreeViewItem;
            ColorTheme CurrentTheme = Item.Tag as ColorTheme;

            ColorTheme NewTheme = CurrentTheme.Clone();
            NewTheme.Name = "New Theme";
            TreeViewItem NewItem = new();
            NewItem.Header = NewTheme.Name;
            NewItem.Tag = NewTheme;

            ColorThemeTree.Items.Add(NewItem);
        }




        public void SaveTheme() 
        {
            TreeViewItem Item = ColorThemeTree.SelectedItem as TreeViewItem;
            ColorTheme Theme = Item.Tag as ColorTheme;

            try
            {
                Directory.CreateDirectory(LibraryGES.ApplicationLocation + "\\Other\\Themes\\");

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("    ");
                settings.CloseOutput = true;
                settings.OmitXmlDeclaration = true;
                using (XmlWriter writer = XmlWriter.Create(LibraryGES.ApplicationLocation + "\\Other\\Themes\\" + Theme.Name + ".xml", settings))
                {
                    writer.WriteStartElement("Theme"); //This is the root of the XML
                    writer.WriteElementString("VersionNumber", LibraryGES.VersionNumber.ToString());
                    writer.WriteElementString("VersionDate", LibraryGES.VersionDate);
                    writer.WriteElementString("Name", Theme.Name);

                    writer.WriteStartElement("ElementList");
                    foreach (Element Element in Theme.ElementList) 
                    {
                        writer.WriteStartElement("Element");
                        writer.WriteElementString("Name", Element.Name);
                        writer.WriteElementString("Note", Element.Note);
                        writer.WriteElementString("Text", Element.Text);
                        writer.WriteElementString("Back", Element.Back);
                        writer.WriteElementString("Border", Element.Border);
                        writer.WriteElementString("Other", Element.Other);                        
                        writer.WriteEndElement(); //End Element
                    }  
                    writer.WriteEndElement(); //End ElementList

                    writer.WriteEndElement(); //End Root of the XML   
                    writer.Flush(); //Ends the XML
                }
            }
            catch
            {

            }
            LibraryGES.SwitchToColorTheme(Theme);
        }

        
    }




    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString && !string.IsNullOrEmpty(colorString) && colorString != "None")
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
                }
                catch
                {
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Not needed unless you decide to implement TwoWay binding
        }
    }

}
