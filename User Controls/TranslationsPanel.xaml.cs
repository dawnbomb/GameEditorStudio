using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TranslationsPanel.xaml
    /// </summary>
    public partial class TranslationsPanel : UserControl
    {
        public TranslationsPanel()
        {
            InitializeComponent();
        }

        public async Task UpdateTranslationsPanel(ItemInfo nameitem) 
        {
            OrigonalNameTextbox.Text = nameitem.ItemOrigonalName;
            //LibreTextbox.Text = TranslateUsingLibreTranslate(nameitem.ItemOrigonalName);
            //LibreTextbox.Text = await TranslateUsingLibreTranslateAsync(nameitem.ItemOrigonalName);
            //AzureTextbox.Text = await TranslateUsingAzure(nameitem.ItemOrigonalName);
            await TranslateUsingDeepL(nameitem.ItemOrigonalName);
            //DeepLTextbox.Text = await TranslateUsingDeepL(nameitem.ItemOrigonalName);

        }

        private async Task TranslateUsingDeepL(string text)
        {
            // Replace with your actual API key — ideally load from secure config, not hardcoded.
            string apiKey = "";

            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-free.deepl.com/v2/translate");
            var parameters = new Dictionary<string, string>
            {
                { "auth_key", apiKey },
                { "text", text },
                { "target_lang", "EN" }, // Change to your desired target language
                { "source_lang", "JA" }  // Optional: let DeepL auto-detect if omitted
            };

            request.Content = new FormUrlEncodedContent(parameters);

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                using var json = JsonDocument.Parse(responseBody);
                var result = json.RootElement
                                 .GetProperty("translations")[0]
                                 .GetProperty("text")
                                 .GetString();

                DeepLTextbox.Text = result;
                //return result;
            }
            catch (Exception ex)
            {
                DeepLTextbox.Text = $"Translation failed: {ex.Message}";
                //return $"Translation failed: {ex.Message}";
            }
        }


        private string TranslateUsingLibreTranslate(string text) 
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    q = text,
                    source = "auto",
                    target = "en",
                    format = "text"
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json"); 

                var response = client.PostAsync("https://translate.astian.org/translate", content).Result; //https://libretranslate.de/translate
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;

                using var doc = JsonDocument.Parse(responseBody);
                string translatedText = doc.RootElement.GetProperty("translatedText").GetString();

                return translatedText;
            }
        }

        private async Task<string> TranslateUsingLibreTranslateAsync(string text)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var requestData = new
                {
                    q = text,
                    source = "auto",
                    target = "en",
                    format = "text"
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://translate.astian.org/translate", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);

                var translatedText = doc.RootElement.GetProperty("translatedText").GetString();
                return translatedText;
            }
            catch (Exception ex)
            {
                // Log the exception or display a message to the user
                MessageBox.Show($"Translation failed: {ex.Message}");
                return "[Translation failed]";
            }
        }

        //184df4f6-6fbf-4ec1-9b73-5c507bae2ede:fx

        

        private string TranslateUsingAzure(string text)
        {
            return "";
        }

        private string TranslateUsingGoogle2(string text)
        {
            return "";
        }
    }
}
