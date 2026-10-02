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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        WorkshopData WorkshopData { get; set; }
        TextEditorData TextEditorData { get; set; }

        private readonly List<int> _searchIndices = new(); //For Search Bar
        private int _currentSearchIndex = -1; //For Search Bar
        private DispatcherTimer _debounceTimer; //To make typing not lag.

        public TextEditor(WorkshopData Database, TextEditorData TheTextEditorData)
        {
            InitializeComponent();

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;

            //This
            WorkshopData = Database;
            TextEditorData = TheTextEditorData;

            //My Data
            TextEditorData.TextEditorXaml = this;
            TextEditorData.EditorVisual = this;
            TextEditorData.TextFileManager = TextFileManager;
            TextEditorData.MainGrid = MainGrid;

            //File Manager
            TextFileManager.IsTextEditor = true;
            TextFileManager.TextEditorData = TextEditorData;
            TextFileManager.WorkshopXaml = Database.WorkshopXaml;
            TextFileManager.TreeGameFiles.SelectedItemChanged += TESTTHING;

            //Documents
            DocumentsControl.WorkshopData = WorkshopData;
            DocumentsControl.TheWorkshopXaml = WorkshopData.WorkshopXaml;

            //Tab
            TabButtonMaker MakeEditorButton = new();
            MakeEditorButton.CreateEditorTab(TextEditorData);
            MakeEditorButton.UpdateEditorRightClickMenu(TextEditorData);

            //Finally, we appear!
            Database.WorkshopXaml.MidGrid.Children.Add(this);

            GenerateUI();
        }

        public void GenerateUI() 
        {
            TextFileManager.RefreshFileTree();

            if (WorkshopData.IsProjectLoaded == true)
            {
                TheTextBox.IsEnabled = true;
                TheLineBox.IsEnabled = true;
                TextFileManager.IsEnabled = true;

                if (TextEditorData.GameFileLocations.Count != 0) 
                {
                    TreeViewItem item = TextFileManager.TreeGameFiles.Items[0] as TreeViewItem;
                    item.IsSelected = true;
                }
            }
            if (WorkshopData.IsProjectLoaded == false) 
            {
                TheTextBox.IsEnabled = false;
                TheLineBox.IsEnabled = false;
                TextFileManager.IsEnabled = false;
            }
            
        }

        private void TESTTHING(object sender, RoutedEventArgs e)
        {
            TreeViewItem Item = TextFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { TheTextBox.Text = ""; TheLineBox.Clear(); return; }
            GameFile GameFile = Item.Tag as GameFile;
            if (GameFile == null) { TheTextBox.Text = ""; TheLineBox.Clear(); return; }

            //TheTextBox.Text = GameFile.FileBytes;
            string fullText = Encoding.UTF8.GetString(GameFile.FileBytes);
            string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            TheTextBox.Text = fullText; // Clear it first if needed
        }
                

        private void ToggleLineID(object sender, RoutedEventArgs e)
        {
            if (TheLineBox.Visibility == Visibility.Visible) 
            {
                TheLineBox.Visibility = Visibility.Collapsed;
                LineIDToggle.Foreground = Brushes.Gray;
            }
            else
            {
                TheLineBox.Visibility = Visibility.Visible;
                LineIDToggle.Foreground = Brushes.White;
            }
        }

        private void ToggleRightSideBar(object sender, RoutedEventArgs e)
        {
            if (RightSideBar.Visibility == Visibility.Visible)
            {
                RightSideBar.Visibility = Visibility.Collapsed;
                RightBarToggle.Foreground = Brushes.Gray;
            }
            else 
            {
                RightSideBar.Visibility = Visibility.Visible;
                RightBarToggle.Foreground = Brushes.White;
            }
        }

        //private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TreeViewItem Item = TextFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
        //    if (Item == null) { return; }
        //    GameFile GameFile = Item.Tag as GameFile;
        //    if (GameFile == null) { return; }

        //    TheLineBox.Clear();
        //    string[] lines = TheTextBox.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

        //    string LineCount = "0";
        //    for (int i = 1; i < lines.Length; i++)
        //    {
        //        LineCount += "\r" + i;

        //    }

        //    GameFile.FileBytes = Encoding.UTF8.GetBytes(TheTextBox.Text);
        //    TheLineBox.Text = LineCount;
        //}
        private void TextBoxKeyDown(object sender, KeyEventArgs e) //This code ONLY makes it so the user can Ctrl F to Search.
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.LoadedProject == null) { return; }

            // Check if Ctrl + F was pressed
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SearchBar.Focus();
                SearchBar.SelectAll();

                // Prevent default browser/textbox behavior or typing 'f'
                e.Handled = true;
            }
        }
        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            // Reset and restart timer on each keystroke
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop(); // Fire only once per pause

            TreeViewItem Item = TextFileManager.TreeGameFiles.SelectedItem as TreeViewItem;
            if (Item == null) { return; }
            GameFile GameFile = Item.Tag as GameFile;
            if (GameFile == null) { return; }

            string fullText = TheTextBox.Text;

            GameFile.FileBytes = Encoding.UTF8.GetBytes(fullText);

            int lineCount = 0;
            if (!string.IsNullOrEmpty(fullText))
            {
                lineCount = 1;
                for (int i = 0; i < fullText.Length; i++)
                {
                    if (fullText[i] == '\n') lineCount++;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("0");
            for (int i = 1; i < lineCount; i++)
            {
                sb.Append('\r');
                sb.Append(i);
            }

            TheLineBox.Text = sb.ToString();
        }








        // Triggered whenever the user types in the search bar
        // Handles Enter key press inside the SearchBar
        private void SearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.LoadedProject == null) { return; }

            _searchIndices.Clear();
            _currentSearchIndex = -1;

            string searchText = SearchBar.Text;
            string mainText = TheTextBox.Text;

            // Skip counting if search bar is empty or placeholder
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(mainText) || searchText == "Search")
            {
                SearchResultLabel.Content = "0 / 0";
                return;
            }

            // Pre-calculate all match positions in background without jumping/scrolling
            int index = mainText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            while (index != -1)
            {
                _searchIndices.Add(index);
                index = mainText.IndexOf(searchText, index + searchText.Length, StringComparison.OrdinalIgnoreCase);
            }

            // Immediately reflect total count (e.g., "0 / 5")
            SearchResultLabel.Content = $"0 / {_searchIndices.Count}";
        }

        private void SearchBarKeyDown(object sender, KeyEventArgs e)
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.LoadedProject == null) { return; }

            if (e.Key == Key.Enter)
            {
                if (_searchIndices.Count > 0)
                {
                    // First Enter press moves to result 1; subsequent presses go to Next
                    if (_currentSearchIndex == -1)
                    {
                        _currentSearchIndex = 0;
                        HighlightCurrentMatch();
                    }
                    else
                    {
                        SearchNextClick(sender, e);
                    }
                }

                e.Handled = true;
            }
        }

        // Core search logic
        private void PerformSearch()
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.LoadedProject == null) { return; }

            _searchIndices.Clear();
            _currentSearchIndex = -1;

            string searchText = SearchBar.Text;
            string mainText = TheTextBox.Text;

            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(mainText) || searchText == "Search")
            {
                SearchResultLabel.Content = "0 / 0";
                return;
            }

            // Find all occurrence indices (case-insensitive)
            int index = mainText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            while (index != -1)
            {
                _searchIndices.Add(index);
                index = mainText.IndexOf(searchText, index + searchText.Length, StringComparison.OrdinalIgnoreCase);
            }

            // Display results
            if (_searchIndices.Count > 0)
            {
                _currentSearchIndex = 0;
                HighlightCurrentMatch();
            }
            else
            {
                SearchResultLabel.Content = "0 / 0";
            }
        }

        // "Next" Button Click
        private void SearchNextClick(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.LoadedProject == null) { return; }

            // Perform search first if results haven't been calculated yet
            if (_searchIndices.Count == 0)
            {
                PerformSearch();
                return;
            }

            _currentSearchIndex++;
            if (_currentSearchIndex >= _searchIndices.Count)
            {
                _currentSearchIndex = 0; // Wrap around to first match
            }

            HighlightCurrentMatch();
        }

        // "Prev" Button Click
        private void SearchPreviousClick(object sender, RoutedEventArgs e)
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.LoadedProject == null) { return; }

            // Perform search first if results haven't been calculated yet
            if (_searchIndices.Count == 0)
            {
                PerformSearch();
                return;
            }

            _currentSearchIndex--;
            if (_currentSearchIndex < 0)
            {
                _currentSearchIndex = _searchIndices.Count - 1; // Wrap around to last match
            }

            HighlightCurrentMatch();
        }

        // Selects and scrolls to the active match
        private void HighlightCurrentMatch()
        {
            if (WorkshopData == null) { return; }
            if (WorkshopData.LoadedProject == null) { return; }

            if (_currentSearchIndex < 0 || _currentSearchIndex >= _searchIndices.Count) return;

            int startIndex = _searchIndices[_currentSearchIndex];
            int searchLength = SearchBar.Text.Length;

            // 1. Update Label
            SearchResultLabel.Content = $"{_currentSearchIndex + 1} / {_searchIndices.Count}";

            // 2. Select text and scroll to line
            TheTextBox.Select(startIndex, searchLength);

            int lineIndex = TheTextBox.GetLineIndexFromCharacterIndex(startIndex);
            if (lineIndex != -1)
            {
                TheTextBox.ScrollToLine(lineIndex);
            }

            // 3. Briefly focus TheTextBox to force WPF to draw the selection, 
            // then immediately return focus to the SearchBar
            TheTextBox.Focus();
            Keyboard.Focus(SearchBar);
        }

        
    }
}

