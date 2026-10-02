using System;
using System.Collections.Generic;
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

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TextDumpHelp.xaml
    /// </summary>
    public partial class TextDumpHelp : UserControl
    {        

        private static readonly Encoding ShiftJisEncoding;

        static TextDumpHelp()
        {
            // Register additional encodings (including Shift-JIS) for .NET Core / .NET 5+
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ShiftJisEncoding = Encoding.GetEncoding("shift_jis");
        }

        public TextDumpHelp(DataTableEditorData DTEData)
        {
            InitializeComponent();

            DTEData.DTEXaml.EditorBack.Children.Add(this);
            Grid.SetRow(this, 0);
            Grid.SetColumn(this, 0);
            Grid.SetRowSpan(this, 99);
            Grid.SetColumnSpan(this, 99);
        }

        private void HexDataTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (HexDataTextbox == null) { return; }
            if (NextLineTextbox == null) { return; }

            string hexInput = HexDataTextbox.Text;
            string delimiterHex = NextLineTextbox.Text;

            // Parse hex string into a byte array
            byte[] rawBytes = ParseHexToBytes(hexInput);
            if (rawBytes.Length == 0)
            {
                ResultTextbox.Text = string.Empty;
                return;
            }

            // Parse line separator hex bytes (e.g., "00" or "0D 0A")
            byte[] delimiterBytes = ParseHexToBytes(delimiterHex);

            // Decode bytes to text using Shift-JIS
            ResultTextbox.Text = DecodeShiftJis(rawBytes, delimiterBytes);
        }

        private byte[] ParseHexToBytes(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Array.Empty<byte>();

            // Remove non-hex characters (spaces, newlines, commas, etc.)
            string cleanHex = new string(hex.Where(c => Uri.IsHexDigit(c)).ToArray());

            // If odd length, ignore incomplete last trailing digit
            int byteCount = cleanHex.Length / 2;
            byte[] bytes = new byte[byteCount];

            for (int i = 0; i < byteCount; i++)
            {
                bytes[i] = Convert.ToByte(cleanHex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private string DecodeShiftJis(byte[] data, byte[] delimiter)
        {
            List<string> lines = new List<string>();

            if (delimiter.Length == 0)
            {
                // If no delimiter specified, decode whole array directly
                return ShiftJisEncoding.GetString(data);
            }

            int startIndex = 0;
            for (int i = 0; i <= data.Length - delimiter.Length; i++)
            {
                if (IsMatch(data, i, delimiter))
                {
                    int length = i - startIndex;
                    string lineText = ShiftJisEncoding.GetString(data, startIndex, length);
                    lines.Add(lineText);

                    i += delimiter.Length - 1; // Advance past the delimiter
                    startIndex = i + 1;
                }
            }

            // Handle trailing bytes after the last delimiter
            if (startIndex < data.Length)
            {
                lines.Add(ShiftJisEncoding.GetString(data, startIndex, data.Length - startIndex));
            }

            return string.Join(Environment.NewLine, lines);
        }

        private bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            for (int i = 0; i < candidate.Length; i++)
            {
                if (array[position + i] != candidate[i])
                    return false;
            }
            return true;
        }






















        private void ConvertToHalfWidth(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ResultTextbox.Text))
            {
                ConvertTextbox.Text = string.Empty;
                return;
            }

            ConvertTextbox.Text = ToHalfWidth(ResultTextbox.Text);
        }

        private void ConvertToFullWidth(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ResultTextbox.Text))
            {
                ConvertTextbox.Text = string.Empty;
                return;
            }

            ConvertTextbox.Text = ToFullWidth(ResultTextbox.Text);
        }

        /// <summary>
        /// Converts Full-Width (Zenkaku) characters to Half-Width (Hankaku).
        /// </summary>
        private string ToHalfWidth(string input)
        {
            char[] buffer = input.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];

                // Full-width space (U+3000) -> Standard ASCII space (U+0020)
                if (c == '\u3000')
                {
                    buffer[i] = ' ';
                }
                // Full-width ASCII range (U+FF01 to U+FF5E) -> Standard ASCII range (U+0021 to U+007E)
                else if (c >= '\uFF01' && c <= '\uFF5E')
                {
                    buffer[i] = (char)(c - 0xFEE0);
                }
            }

            return new string(buffer);
        }

        /// <summary>
        /// Converts Half-Width (Hankaku) characters to Full-Width (Zenkaku).
        /// </summary>
        private string ToFullWidth(string input)
        {
            char[] buffer = input.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];

                // Standard ASCII space (U+0020) -> Full-width space (U+3000)
                if (c == ' ')
                {
                    buffer[i] = '\u3000';
                }
                // Standard ASCII range (U+0021 to U+007E) -> Full-width ASCII range (U+FF01 to U+FF5E)
                else if (c >= '\u0021' && c <= '\u007E')
                {
                    buffer[i] = (char)(c + 0xFEE0);
                }
            }

            return new string(buffer);
        }






        private void Exit(object sender, RoutedEventArgs e)
        {
            var parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Children.Remove(this);
            }
        }
    }
}
