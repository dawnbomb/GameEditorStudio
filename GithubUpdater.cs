using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameEditorStudio
{
    

    class GithubUpdater
    {
        // GET https://api.github.com/repos/{owner}/{repo}/releases/latest

        Version CurrentVersion = LibraryMan.VersionNumber; 

        private static readonly HttpClient client = new();

        public static async Task CheckForUpdatesAsync()
        {
            //I have never done this before, i just asked GPT to write it for me, i hope this works! T.T
            //...okay i got it working. Appearently, github auto-generates the source code zip file automatically, based on the files on github at the moment of the release upload. Huh.
            //That and uploads are literally any old folder. Crazy. >_> It feels way to simple. 


            try
            {
                client.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue("GameEditorStudio", LibraryMan.VersionNumber.ToString() ));

                var response = await client.GetAsync("https://api.github.com/repos/dawnbomb/Crystal-Editor/releases/latest");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);

                string tag = doc.RootElement.GetProperty("tag_name").GetString();

                // Remove "v" prefix like "v1.5.2" → "1.5.2"
                if (tag.StartsWith("v")) tag = tag.Substring(1);

                Version latest = Version.Parse(tag);

                if (latest > LibraryMan.VersionNumber)
                {
                    // Replace this with your WPF popup
                    System.Windows.MessageBoxResult result =
                        System.Windows.MessageBox.Show(
                            $"New version {latest} is available!\n(Current: {LibraryMan.VersionNumber})\n\nOpen download page?",
                            "Update Available",
                            System.Windows.MessageBoxButton.YesNo,
                            System.Windows.MessageBoxImage.Information
                        );

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://github.com/dawnbomb/Crystal-Editor/releases/latest",
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LibraryMan.NotificationNegative("Error: Update Checker has failed.",
                    "IDK why this happened but Game Editor Studio is definatly not dead. You should manually check for updates." +
                    "\n\n" +
                    "PS: I have seen that sometimes the update checker fails even when the server it's connecting to IS online, " +
                    "theres obviously some bug in my code i don't understanding. " +
                    "My update checker *Mostly* works. So don't panic if you get this randomly, but if you keep getting it, yeah, probably check for an update yourself? >_>; (Sorry~)"

                    );                
                return;
            }
        }
    }
}
