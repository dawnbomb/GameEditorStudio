using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace GameEditorStudio
{
    internal class Chapter_1
    {

        public void Welcome(Tutorial Tutorial, List<Run> Tutorials, Dictionary<string, string> Links) 
        {
            
            
            Run Text1 = new("" +
                "Welcome to the first beta release of Crystal Editor!" +
                "\n\n " +
                "A project born from years of pain and suffering over wanting better, easier to understand tools. " +
                "I started as someone just wanting more editors to exist, then someone " +
                "wanting a better way to turn learned knowledge into a game editor, " +
                "and then finally into someone wanting a better community in general." +
                "So as usual, after being refused help at every turn, I decided to do it myself. " +
                "\n\n" +
                "While a good amount of it is for passion, some parts of it are from hatred as well." +
                "From community after community that is to lazy to make the bare minimum," +
                "frustrated time and again from people just telling me \"it's so easy\" or being told" +
                "to google the answer because \"It's obvious\". After dealing with infuriating " +
                "people with superiority complexes for years who all refuse to teach anything," +
                "always insisting users learn everything themselfs the hardest possible way," +
                "through terrible documentation and abyssmal tools, I made crystal editor." +
                "\n\n" +
                "Crystal Editor is an all in one modding headquarters. The intent is to have " +
                "everything a person would ever need to mod a game, all in one place." +
                "Every tool, every document, everything, all together in one program." +
                "\n\n" +
                "With crystal editor, users can make workshops, editors, and mode, add tools," +
                "and share everything easily. Modding a game has never been so easy. Simply" +
                "enter a workshop for a game you want, and away you go." +
                "\n\n" +
                "If this is your first time, i recommend you follow the tutorials. As this is a beta," +
                "features will change, or be added from time to time, and these tutorials will" +
                "become outdated. If you see something wrong, feel free to tell me on the" +
                "discord." +
                "\n\n" +
                "To continue along, please click the next tutorial on the left sidebar."
                ); Tutorials.Add(Text1);          

            

        }




        public void Catagories(Tutorial Tutorial, List<Run> Tutorials, Dictionary<string, string> Links)
        {
            

            Run Text1 = new("" +
                "Crystal editors In-Program Wiki has a variety of tutorials, from newcomer to learning advanced concepts from scrahch." +
                "Lets break them down and explain each of them. " +
                "\n\n" +
                "Basic Tutorials" +
                "\nTutorials in this section will go over everything a user needs to know, if all " +
                "you want to do is create a mod for a game using editors that already exist in this program. " +
                "These tutorials are aimed at teaching users about crystal editor itself, and teaching concepts as a whole. " +
                "Most game modders are self-taught due to terrible online documentation and human assistance, so people have rather " +
                "large gaps in their knowledge as a result. Modding games is easy, but learning how through horrible online support is awful. " +
                "After completing the basic tutorials, you will be ready to start modding any game supported by crystal editor! " +
                "It's made to be so easy, a child can do it. " +
                ""); Tutorials.Add(Text1);

            Run Text2 = new("" +
                "Advanced Tutorials" +
                "\nIf a user is willing to learn a little more, and wants to know how to CREATE new editors, the Advanced Tutorials section" +
                "will teach you how to create your own editors from scratch. You will recieve a full reverse engineering course. " +
                "Learning from the group up with plenty of examples and detailed explanations, all in plain understandable english. Every tool needed, technique used, " +
                "and more will all be given and taught in a way thats easy to understand and follow along. " +
                "These tutorials were intensionally designed for forgetful people or people with knowledge gaps. In my experience, nearly every online tutorials " +
                "at some point or another just assumes you already know some \"basics\" and i know just how incredibly frustrating it is to get 50% into" +
                "a guide only for it to tell you to do something and give no explanation whatsoever as to how. " +
                "This way, especially due to the nature of self-taught modders being so all-over. even if you struggle to retain information your supposed to be " +
                "learning so long as you can follow these tutorials / guides when wanting to create a new editor, you will still be able to succeed. " +
                "With recommended tools, and links to relevant information when it's actually useful to the topic, modding your next RPG has never been easier. " +
                ""); Tutorials.Add(Text2);

            Run Text3 = new("" +
                "Terminology" +
                "\nAside from tutorials directly related, sometimes when your reading over material and" +
                "you just really need a reminder on what some word means, or what " +
                "was taught to you a few moments ago, or maybe you just lack some " +
                "internet culture and have no idea what some acronym or word means. " +
                "Worse yet, maybe you have your own ideas about what something is, but don't understand the context in how it's being used. " +
                "Or even worse, someone is biased and teaching you incorrectly, and being as unlearned as you are, your unable to seperate the " +
                "new information from the bias. As superiority complexes are rather high in modding communities, combined with human tendency to be unwilling " +
                "to explain concepts people think you should already understand as they treat you like a child, the terminology section is a great way to learn " +
                "more about various words, their meanings, and other surrounding concepts. Some of the explanations given are agressive in nature, in order " +
                "to make clear lines in the sand on if a program is good or trash, and hopefully help people learn more easily about the reality of " +
                "various concepts there in." +
                ""); Tutorials.Add(Text3);

            Run Text4 = new("" +
                "Third Party Tools" +
                "\nThe tutorials don't stop at teaching you about modding. The tools section includes " +
                "some seriously good tutorials on how to use a number of useful " +
                "programs people often tell you to use when modding games. Best of all, " +
                "most of the programs themself are included in crystal editor, so you don't " +
                "even need to search the internet to download them. I felt including a section on this necessary, and one of the most " +
                "annoying things is having to sourge the internet for information, not sure who to trust, or hoping someone you know is available on discord. " +
                "This is especially useful for many programs that literally have no tutorials or documentations, or for those like cheat engine where the included " +
                "tutorials are so fundamentally bad no reasonable person would understand how the program could be useful without outside assistance. I have no " +
                "idea why the various program makers of the world are so dedicated to making their own programs so insufferable to try and use, but hopefully " +
                "having third party tool tutorials can help fix all that. :)" +
                ""); Tutorials.Add(Text4);

            Run Text5 = new("" +
                "Making a good mod." +
                "\nPeople can always google general design advice, and get general answers, or completly random ones from people who have never modded " +
                "a game in their life. Worse yet, the countless people who \"are going to make a game\" or \"are making their first game\" and love to " +
                "spew on forumns all about what \"good game design\" is. Information in this section attempts to be extremely pointed, and when possible, tries " +
                "to explicitly not say boring shit like \"there is no perfect answer\" but to give direct examples on why certain modding habbits are good or bad. " +
                "Advice is broken down between what i believe will be all the most common editor types. This way, you can get advice for the parts of a " +
                "game you can actually mod, and not hear anything about the parts you have no control over. When making a mod, check out this section " +
                "after your first few hours, and give it a good read. " +
                ""); Tutorials.Add(Text5);


            Run Text6 = new("" +
                "Making an Editor." +
                "\nThis is pretty much the sister version of making a good mod. Instead of advice on making mods, it's advice on making editors. " +
                "As before, the advice attempts to be extremely pointed, and split between editor type. It covers how to reconize common types of " +
                "data within a game's data table based on the specific editor your making. If you have found out what 75% of the bytes do but are struggle " +
                "to understand what the last 25% of them are, this is a great place to look. In most cases there wont be anyone online you can ask for help, " +
                "but having the wisdom of what other game developers did with their data tables in a giant reference list should help spark a better understanding " +
                "of what diffrent bytes might or might not be." +
                "\n\n" +
                "Thats all for this section. Continue onto the next tutorial."); Tutorials.Add(Text6);

            

        }


        public void Emulators(Tutorial Tutorial, List<Run> Tutorials, Dictionary<string, string> Links) 
        {
            Run Text1 = new("" +
                "Emulator veterans can skip this tutorial. For more specific information on the best emulator for each console and why, check out the Emulators section of Third Party Tools. " +
                "This is just a talk about what emulators are, their legality, and other surrounding stuff. It also serves to educate anyone who doesn't " +
                "uderstand emulators yet, because boy is it annoying to get asked about them. " +
                "\n\n" +
                "Okay so first of, an emulator is a program that acts like a game console. They allow anyone to play any console right in their PC, " +
                "and for modding their an absolute necessity. I don't mean necessity merely for convenience, but ALL of your playerbase is going to use one, " +
                "so you had damn well better start using them yourself, and understand them well enough to give others tutorials on the best settings for " +
                "whatever game mod you've made. Modernly, suprisingly, every major console in history " +
                "all have good, accurate, high performance emulators. Currently, the only system without an emulator is PS4, even the vita now has " +
                "an emulator (Although there are no relevant vita exclusives anymore, except maybe the english patch for tales of innocence R...) and " +
                "the nintendo switch has TWO high end emulators. There is a suprisingly large number of people unaware switch emulators exist, and it " +
                "continues to baffle me how under a rock some people can live. " +
                "\n\n" +
                "Anyway, another question i hear newcomers ask a lot is about legality. To be as plain as possible, emulators being illegal is just " +
                "corporate marketing tactics / fearmongering to try and make people stop using them, so they pay money for offical consoles. " +
                "It's all bullshit, infact its so legal, although i do not recommend retroarch at ALL, \"retroarch\" (It's total garbage) " +
                "it does have a copy of every major emulator all in one program, and it's offically on steam. Recently, even Dolphin, the " +
                "gamecube & wii emulator is on steam. Emulators are NOT \"A gray area\" but very much so entirely legal, and have been so for a very " +
                "long time. If you have any friends telling you otherwise on discord servers, their all straight up wrong. " +
                "\n\n" +
                "Start using emulators, their better then offical consoles in every way. You can play games at 4K resolution, with widescreen support, " +
                "some emulators even support ray tracing, and so much more. " +
                "\n\n" +
                "PS: For anyone thats a veteran still reading, the nintendo DS emulator MelosDS has offically pulled ahead of DesMuMe. You can entirely " +
                "throw DesMuMe in the trash and start using MelonDS. It even uses native raw saves, the kind on GameFAQs, so you don't need to save file convert anymore. " +
                "Best of all, it's speedup feature actually works properly. Go use it! GOGOGO~"); Tutorials.Add(Text1);



            
        }


        public void ModsExplained(Tutorial Tutorial, List<Run> Tutorials, Dictionary<string, string> Links)
        {
            Run Text1 = new("" +
                "https://gamebanana.com/wikis/755" +
                "\n\n" +
                "Again, veterans can skip this tutorial. But i see newcomers ask exactly is a mod. A \"Mod\" is any type of change to a game. Even something as " +
                "simple as changeing a single weapon to deal 10 more damage is a mod. " +
                "\n\n" +
                "More specifically, a mod is generally refered to as the new file / files " +
                "that say weapon X deals 10 more damage. When people " +
                "are talking about mods, they are actually talking about the " +
                "altered files that are diffrent from the origonal game. " +
                "\n\n" +
                "Mod files are often uploaded online, for others to download " +
                "from sites dedicated to hosting game mods. Example websites" +
                "include Romhacking.net, GameBanana, NexusMods, and " +
                "although limited, even Steam via the Steam Workshop. " +
                "\n\n" +
                "Mods are often made using something called an Editor. Editors are an easy way" +
                "to create tons of mods quickly with simple to understand interfaces. As the term implys, they literally let to change information about the game, such as " +
                "character stats, spell lists, equipment, items for sale, enemy abilities, and so on. Editors are also often uploaded " +
                "to those same websites. This program is one that both includes editors, and for a world first makes it easy to create new ones. " +
                "Usually game editors need a decent amount of programming knowledge to create, but this program removes the coding barriers usually in place, and makes it " +
                "extremely easy to create basic editors for most games. " +
                "\n\n" +
                "Another topic, is the legality of modding. Mods are legal, but they have some unfortunate problems to go alongside them. For starters, although modding " +
                "games is legal by default, a game company can force a user to accept an agreement to explicitly not modify a game that the user must agree to on the title screen. " +
                "Although these agreements are extremely rare, they have become slightly more common in recent times, but still very rare. " +
                "Second, although making a mod is legal, putting origonal game assets online is not. Most companies don't care and let people do it anyway, many " +
                "mods even on all major sites include mods that have the origonal game files. However, for people worried about staying legal, for most game mods there is an easy workaround. " +
                "Instead of downloading a modified game with game files, the creator makes a patch and you download, then has you patch the game. " +
                "The patch doesn't contain any game specific information or assets, thus it's legal. " +
                "\n\n" +
                "Mods that include modified game files make it easier for end users to merge multiple mods together unless a game has a modloader tool, " +
                "so despite how easy it is to create a patch, many mod makers opt to simply upload modified files, as most companies don't care anyway." +
                "\n\n" +
                "However, there is one last legal hurdle surrounding mods that basically everyone breaks, and is nearly unavoidable. " +
                "For those that remember, a few years ago, ATLUS a major game company filed a lawsuit against RPCS3, the PS3 emulator team to try and take them down, as RPCS3 could " +
                "play persona 5, and they wanted to protect their IP. People were modding persona 5, and ATLUS believed the patreon behind RPCS3 could offer enough funding to make it " +
                "worth their while to seriously try and go after RPCS3. Skipping ahead, they basically failed almost entirely. *Almost*. There are clear laws about reverse engineering and " +
                "the interoperability of technologies that majority protected RPCS3, however one part of this case is relevant to why it's difficult to literally go out and sell mods. " +
                "Technically, you can sell mods, except for one huge problem this case highlights extremely well. Although RPCS3's existance and what it does as a program is entirely legal, the way " +
                "it marketed itself was not. It's not legal to market something, showcasing materials that are behind copywrite protection. In this case, RPCS3 was to stop " +
                "advertising itself using Persona branding, as in to also stop including images from the games running in RPCS3 as offical marketing material. " +
                "\n\n" +
                "Now think about how that related to selling mods, or even just distribuiting them for free. In order to not break this law, you have to be able to put up a download " +
                "link without actually showing at all what it is that the mod does. Could you imagine if the steam store didn't allow images, videos, and other marketing information about " +
                "a game? You basically can't even communicate anything to other people. This is the core legal problem behind game modding, and it's enviroment. Technically, nearly every " +
                "single mod on romhacking.net violates that law. If you wanted to get around it, you would have to mod a game so much, it wouldn't even look like the same game anymore. " +
                "At that point your better off making your own in a game engine. While you might ask about game blogs, using snippets of copywrite material for commentary purposes is legal, " +
                "but programs like RPCS3 or game mods are not commentary, but meant to be tools directly using those game files. " +
                "Imagine trying to buy a game, and you know nothing about it. The seller would never suceed, and this is the core legal problem behind game mods. " +
                "Not because you can't mod stuff (Although some EULA agreements do explicitly make you agree not to mod the game) but because to advertise your mod, is itself the legal problem. " +
                "\n\n" +
                "GameBanana has a offical list of companies that give explicit permission for mods to be posted for any of their games. " +
                "There is also a list of companies that allowed limited mods, and companies that explicitly hate free advertising and they money it brings them." +
                ""); Tutorials.Add(Text1);
           
        }


































































    }
}
