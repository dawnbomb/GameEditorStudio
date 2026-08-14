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
            "TIP: You can organize / reorder an editor's name list! Click and drag a name onto another name. This will NOT cause problems ingame! (Promise!)",
            "TIP: Most things in GES have right click options.",
            "TIP: You can give items a note to help identify them! Great for two enemys with the same name...",
            "TIP: You can make more then 1 editor for the same file! Great for seperating armor and accessories!" +
            "TIP: You can click drag the area between an editor's name list and the main editor panel to expand the name list size." +
            "TIP: You can right click an entry to create a Mathbox. A Mathbox lets you see the value of an entry, with a math formula. (HPx2, Atkx1.5)",
        };

        List<string> Trivia = new List<string>()
        {
            "Trivia: I first started making this program sometime in 2022. It was also my first time coding!",
            "Trivia: The was origonally called Etrian Editor, then Crystal Editor, then Crystal Tools, and now GES.",
            "Trivia: Origonally, i made this for the etrian odyssey games, and later the tales series.",
            "Trivia: Super Robot Wars: Endless Frontier was the game i used as example data to make GES.",
            "Trivia: I used to run the Splatoon 2 community.",
            "Trivia: Previously, I had a top 10 time as a sonic speedrunner." +
            "Trivia: I released a Touhou game mod manager called \"Western launcher of Eastern Origins\". Touhou fans, check it out!",
        };

        List<string> Games = new List<string>()
        {   
            "Games: Rabbit and Steel is maybe the only good multiplayer roguelike!",
            "Games: Dragon Quest 11 S is actually really damn good!",
            "Games: The Fire Emblem series is the #1 Grid based series for a reason! You should play them!",
            "Games: Slay the spire is the best card game. GO PLAY IT!",
            "Games: The \"Tales of\" series is the #1 action jrpg series! Give it a try!",
            "Games: For great modern puzzle games, try Catherine, The Witness, Baba is You, and La Mulana!",
            "Games: UFO 50 is a collection of 50 NEW retro theme'd games. Great for retro game lovers!",
            "Games: Star Ocean 2R is maybe the best action JRPG i played the last few years.",
            "Games: Final Fantasy Strangers of Paradise has some crazy good boss fights. Try it without AI allys!",
            "Games: Bloodborne finally has good PC emulation.",
            "Games: Astralibra Revision is a mostly unknown, FANTASTIC metroidvania. The graphics are weird, but the gameplay is IMPRESSIVE.",
            "Games: Danganrompa is the best mystery game series. It's so good it killed off the Ace Attorney series!",
            "Games: For the best visual novels, read Umineko, Stiens Gate, or 428 Shibuya Scramble.  ",
            "Games: Class of '09 is an offensive comedy game on steam. It's *really* funny.",
            "Games: For fans of Etrian Odyssey, go try Touhou Labyrinth 2 and Tri. They are REALLY GOOD!" +
            "Games: Guildrun is a roguelike with a demo that got VERY popular on steam and twitch. I played 50 hours of the demo alone! TRY IT :)" +
            "Games: I AM VERY EXCITED FOR FIRE EMBLEM FORTUNES WEAVE!!!" +
            "Games: I AM VERY EXCITED FOR DANGANROMPA 2x2",

        };

        List<string> Mods = new List<string>()
        {
            "Mods: Chrono Trigger Lavos Awakening is crazy good! Playing with realtime combat and max ATB speed, it gets really hard!",
            "Mods: Tales of Rebirth, Destiny DC, and Phantasia X have 100% ENG patches now.",
            "Mods: Paper Mario Master Mode is a suprisingly pretty fun mod!",
            "Mods: Final Fantasy 10 Masters Challenge is CRAAAZY FUCKING GOOD." +
            "Mods: Fire Emblem Shadows of Valentia has a Lunatic Mode mod.",
        };

        List<string> Anime = new List<string>()
        {
            "Anime: Orb on the Movements of the Earth is extremely thought provoking, a real 10/10.",
            "Anime: League of Legend's \"Arcane\" is a 10/10 masterpiece." +
            "Anime: Gnosia is the most fun time travel mysery anime i have seen in YEARS.",
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
