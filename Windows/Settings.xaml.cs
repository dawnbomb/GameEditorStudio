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
        public string Name { get; set; } = "New Theme";
        public List<Element> ElementList { get; set; } = new();

        public Element Application { get; private set; }
        public Element Header { get; private set; }
        public Element Panel { get; private set; }
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
            Application = new Element { Name = "Application" };
            Header = new Element { Name = "Header" };
            Panel = new Element { Name = "Panel" };
            Textbox = new Element { Name = "Textbox" };
            TextboxHighlight = new Element { Name = "Text Highlight" };
            Menu = new Element { Name = "Menu" };
            MenuMouseover = new Element { Name = "Menu Mouseover" };
            Button = new Element { Name = "Button" };
            ButtonMouseover = new Element { Name = "Button Mouseover" };
            ButtonDown = new Element { Name = "Button Down" };
            Checkbox = new Element { Name = "Checkbox" };
            DropDown = new Element { Name = "DropDown" };
            DropDownMouseover = new Element { Name = "DropDown Mouseover" };
            List = new Element { Name = "List" };
            ListMouseover = new Element { Name = "List Mouseover" };
            ListSelected = new Element { Name = "List Selected" };
            Entry = new Element { Name = "Entry" };
            EntrySelected = new Element { Name = "Entry Selected" };
            HiddenEntry = new Element { Name = "Hidden Entry" };
            HiddenSelectedEntry = new Element { Name = "Hidden Entry Selected" };


            // Populate the list
            ElementList.Add(Application);
            ElementList.Add(Header);
            ElementList.Add(Panel);
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
        public string Name { get; set; }
        public string Text { get; set; }
        public string Back { get; set; }
        public string Border { get; set; }
        public bool HasBorder { get; set; } = true;

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

            TreeViewItem TheItem = ColorThemeTree.Items.GetItemAt(0) as TreeViewItem;
            TheItem.IsSelected = true;

            //ColorThemeTree.


        }

        private void UpdateDataGrid(ColorTheme theme)
        {
            ColorDataTable.Columns.Clear();
            ColorDataTable.Items.Clear();

            // Define columns
            ColorDataTable.Columns.Add(new DataGridTextColumn { Header = "Element Name", Binding = new Binding("Name"), Width = 150 });

            // Text color column with buttons
            var textColumn = new DataGridTemplateColumn { Header = "Text", Width = 80 };
            textColumn.CellTemplate = GenerateTemplate("Text");
            ColorDataTable.Columns.Add(textColumn);

            // Background color column with buttons
            var backColumn = new DataGridTemplateColumn { Header = "Back", Width = 80 };
            backColumn.CellTemplate = GenerateTemplate("Back");
            ColorDataTable.Columns.Add(backColumn);

            // Border color column with buttons
            var BorderColumn = new DataGridTemplateColumn { Header = "Border", Width = 80 };
            BorderColumn.CellTemplate = GenerateTemplate("Border");
            ColorDataTable.Columns.Add(BorderColumn);

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
                    SaveTheme();
                }
            }));

            template.VisualTree = factory;
            return template;
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
                        writer.WriteElementString("Text", Element.Text);
                        writer.WriteElementString("Back", Element.Back);
                        writer.WriteElementString("Border", Element.Border);
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
