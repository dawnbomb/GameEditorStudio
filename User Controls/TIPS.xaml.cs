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

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for TIPS.xaml
    /// </summary>
    public partial class TIPS : UserControl
    {
        List<string> Tips = new List<string>()
        {
            "TIP: You can double click a project to open it. This is a bit faster then using the Launch button.",
            "TIP: You can drag drop to reorder editor items! This won't save to game files, but the editor will remember!",
            "TIP: You can right click a file in the Files Manager and open it's file location! ",
            "TIP: You can give items a note to help identify them! Great for two enemys with the same name...",
            "TIP: You can make more then 1 editor for the same file! Great for seperating armor and accessories!",
            "TIP: If you join the discord, i'll answer any questions you have! :)",
            "TIP: Like in Unreal Engine 5 blueprints, you can hold down the middle mouse wheel and drag to move around the entry panel in a standard editor.",
        };

        List<string> Trivia = new List<string>()
        {
            "Trivia: I first started making this program sometime in 2022. It was also my first time coding!",
            "Trivia: The was origonally called Etrian Editor, then Crystal Editor, then Crystal Tools, and now GES.",
            "Trivia: Origonally, i made this for the etrian odyssey games, and later the tales series.",
            "Trivia: Super Robot Wars: Endless Frontier was the game i used as example data to make GES.",
            "Trivia: Take advantage of templates to speed up development.",
            "Trivia: I used to run the Splatoon 2 community.",
            "Trivia: Previously, I had a top 10 time as a sonic speedrunner.",
        };

        List<string> Games = new List<string>()
        {   
            "Games: Rabbit and Steel is a crazy good multiplayer roguelike!",
            "Games: Dragon Quest 11 S is actually really damn good!",
            "Games: The Fire Emblem series is the #1 Grid based series for a reason!",
            "Games: Slay the spire is the best card game there is. GO PLAY IT!",
            "Games: The \"Tales of\" series is the #1 action jrpg series! Give it a try!",
            "Games: Catherine, The Witness, Baba is You, and La Mulana are great modern puzzle games!",
            "Games: UFO 50 is a collection of 50 retro style games, and its actually pretty fun!",
            "Games: Star Ocean 2R is maybe the best action JRPG i played the last few years.",
            "Games: Final Fantasy Strangers of Paradise has some crazy good boss fights. Try it without AI allys!",
            "Games: Bloodborne finally has good PC emulation.",
            "Games: Astralibra Revision is mostly unknown, and the graphics are weird at best, but it's VERY impressive.",
            "Games: I'm a huge Danganrompa fan. It's so damn good it killed the Ace Attorney series!",
            "Games: Umineko is, even today, the best visual novel there is.",
            "Games: Class of '09 is an offensive comedy game on steam. It's *really* funny.",
            "Games: Touhou Labyrinth Tri is CRAZY GOOD on the highest difficulty.",

        };

        List<string> Mods = new List<string>()
        {
            "Mods: Chrono Trigger Lavos Awakening is crazy good! Playing with realtime combat and max ATB speed, it gets really hard!",
            "Mods: Tales of Rebirth, Destiny DC, and Phantasia X have 100% ENG patches now.",
            "Mods: Paper Mario Master Mode is a suprisingly pretty fun mod!",
            "Mods: Final Fantasy 10 Masters Challenge is CRAAAZY FUCKING GOOD.",
        };

        List<string> Anime = new List<string>()
        {
            "Anime: Orb on the Movements of the Earth is extremely thought provoking, a real 10/10.",
            "Anime: League of Legend's \"Arcane\" is a 10/10 masterpiece.",
        };

        public TIPS()
        {
            InitializeComponent();
            ShowRandomTip();

        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ShowRandomTip()
        {
            Random rand = new Random();
            int tipType = rand.Next(0, 10); 
            string selectedTip = string.Empty;
            switch (tipType)
            {
                // 0-6: Tip
                // 7: Trivia
                // 8: Games
                // 9: Mods
                // 10: Anime
                case 0:
                    selectedTip = Tips[rand.Next(Tips.Count)];
                    break;
                case 1:
                    selectedTip = Tips[rand.Next(Tips.Count)];
                    break;
                case 2:
                    selectedTip = Tips[rand.Next(Tips.Count)];
                    break;
                case 3:
                    selectedTip = Tips[rand.Next(Tips.Count)];
                    break;
                case 4:
                    selectedTip = Tips[rand.Next(Tips.Count)];
                    break;
                case 5:
                    selectedTip = Tips[rand.Next(Tips.Count)];
                    break;
                case 6:
                    selectedTip = Tips[rand.Next(Tips.Count)];
                    break;
                case 7:
                    selectedTip = Trivia[rand.Next(Trivia.Count)];
                    break;
                case 8:
                    selectedTip = Games[rand.Next(Games.Count)];
                    break;
                case 9:
                    selectedTip = Mods[rand.Next(Mods.Count)];
                    break;
                case 10:
                    selectedTip = Mods[rand.Next(Anime.Count)];
                    break;
            }
            TipTextLabel.Content = selectedTip;
        }
    }
}
