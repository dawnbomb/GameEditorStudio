# Game Editor Studio

[![Discord Server](https://discordapp.com/api/guilds/324979738533822464/embed.png)](https://discord.gg/mhrZqjRyKx)

Game Editor Studio (GES) is a program that lets you create highly customizable editors for video games without any coding knowledge. GES is made with forward compatibility in mind, ensuring that future features will work seamlessly with existing editors.

## [Download Latest Build](https://github.com/dawnbomb/GameEditorStudio/releases/latest)
> Note that automatic updates are not a thing yet, so for now you will need to manually update every so often.

## Getting Started
If you want to create mods using existing editors, first select a game from the workshops list on the left side of the window. A readme will open, and it SHOULD tell you how to extract any game files required. Then create a new project for that workshop, set and input and output folder (Input is game files, output is where your modded files will save to), and launch your project! Your good to go!

If instead you want to create editors for a game noone has ever done before, you will need to create a new workshop for that game. Then make a dummy project for that workshop, launch the workshop, then go to File -> Create New Editor. From there, you can create a new standard editor using a name table and a data table. The name table is a list of names of the things the editor is to be editing, and the data table is the actual data that will be edited. There are tutoials included but if you need help, as me on discord, as it would help me write better tutoials. 


## Introduction

Game editor studio's main features are:

* Create a "workshop" for a game. It holds any number of editors for that game. 
* The "Standard Editor". (Characters, Spells, Classes, Enemys, Weapons, etc).
	* Creating a new one using a name table, and a data table. There are tutorials included. 
	* Each table byte is represented as an "Entry" (Max HP, Str, Gold, etc). 
	* You can freely **move entrys around** into columns, rows, and even "Groups" (basically folders). 
	* Entrys can be turned into checkboxes, bitflags, and dropdown menus. You can even set them to hidden. 
	* Dropdown menus can source text just like an editor, or pull it's name list directly from an existing editor. 
	* You can also give entrys a tooltip, with no size limit.   

* The "Items List" of a Standard Editor.
	* The List of things an editor is actually editing (Weapons, Classes, Spells, etc) are always called "Items".
	* Items can be reordered, and will always save back to the games files in the origonal order!
	* Items can even be sorted into folders! Just right click an item and select "Create Folder".
	* You can give any item a note. Notes appear next to the item name as orange text (so they stand out).
	* You can also give an item a detailed tooltip. Items with tooltips appear underlined, and stand out ontop of notes.
	* Items can be renamed, and they save back to game files. **This is fantastic for making english patches!**
	* More english patch features are also planned! :D 
	* Also, if you add a descriptions table to an editor, users can also edit item descriptions!

* Create a "Text Editor". 
	* A text editor lets you add any number of text files to it. 
	* Text editors are newer and don't have good formatting support yet, but they will in the future.
	* Each text editor can hold any number of files. 
	* Great usage examples, one is an "AI Editor", another is a "Stage event editor". 
	* Basically, they are good for games that use actual text for anything.

* Tools & Events System
	* GES supports 25ish third party tools (HxD Hex editor, UABEAvalonia, Floating IPS, etc)
	* All tools have a download button INSIDE the program.
	* You can declare that your game needs specific tools, like Nitro Packer to unpack and repack nds roms. 
	* You can also declare some "Common Events". Like "Unpack NDS Rom".
	* Any common events enabled for a game will appear in your tools menu as quick events!
	* Aside from Tools, and enabling common events, are also more advanced events.
	* Made to look like the eventing system in RPG maker, you can create complex events to run.
	* Events can chain "Commands" together. There exist all kinds of commands for you to use.
	* A example Event is... [Save Everything] -> [Repack NDS Rom] -> [Run MelonDS with rom]
	* Even a simple event can be useful. [Save Everything] -> [Run target exe] is good for PC games. 
	* By chaining them all together in 1 event, you create a 1 click solution to test your changes!
	* In the future, many more third party tools, and many more commands will be added.   

* Other misc features.
	* You can export any editor into a google sheets table. Then import to google sheets and share online!
	* "Auto-Mod" lets you create simple mods quickly. (All enemys give 0 exp, all items have x2 buy cost, etc) 
	* A Documantation system exists. Workshops can hold any number of text files for you to edit.
	* Project documents exist as well. Create both public (Workshop text files) and private (project text files). 
	* You can preview a workshop. This gives you an idea of what it looks like without the creator needing any screenshots. 
	* From the Library window (before you open a workshop) you can already read the documents of the workshop.
	* If a workshop has a README text file, it will be automatically selected when selecting a workshop to open.
		* There is a shortcuts menu at the top of the screen to open relevant folders on your PC.
	* Most parts of the program, you can right click them and they will have additional options. 
	* A symbology system exists. When enables, entrys tell you information about themself, useful to finding out what they do.
		* for example, "I am always 0 or 1", or "I am only 1~5 and 255, so i probably represent a negative value".

## Future Plans

Honestly there is so many things i want to add to this program. But heres a quick list of things in no particular order.
* Support for creating editors where the data table has each row has a diffrent byte count.
* I want to rework the right bar in the standard editor to be much more refined.
* I want to change the eventing system to one where resources are defined in the events window instead of by the workshop.
* I want to have a in-program "Wiki" that gives tips and tricks on modding games. 
	* I want this because it's really hard to learn game modding techniques. People mislead you SO much... 
* I want to add support for either creating JSON editors, or XML editors. Yes i'm serious. 
* I want to try and have a panel to can open that will attempt to machine translate your game. 
	* automatic english patches, or atleast an idea of what it's supposed to say, would be really cool!
* I want like 50 more tools and 30 more commands.
* I want a button that lets you view an editor's raw data in a hex editor.
* I'd like to add an automtic update installer. 
* Ability to right click a dropdown menu entry, select GOTO, and it opens that item in it's editor.
* A history feature showing your last X items you were looking at in this editor.
* A way to tag a editor as a specific type of editor (Skills, Classes, enemys).
	* When a editor is of a specific type, it's entrys and UI elements reflect that. 
* Even more cool stuff.
* I want to look back at newyears at the end of 2025 and go "WOW, I REALLY MADE THE WORLD A BETTER PLACE!" :D


## README FOR DEVELOPERS

If you want to help develop GES then i'd love to have you! I have a big list of new features i'd like to add and could use the help! Ping me on my discord server, or message me on discord (i'm dawnbomb).  

* GES Project Details
	* Made in visual studio 2022 
	* Made in C# 
	* I'm using .NET9
	* Framework is WPF. It's not Avalonia UI because Avalonia is missing Toolbox support and me being new really needs it D: (I will CONSIDER revisiting Avalonia when Accelerate releases as it's supposed to have toolbox support).
	* This my first ever project. I taught myself how to code to make this, so i'm sure there is lots of bad code, and that i'm not following standards at all. 

* NuGet Packages used: 
	* Ookii.Dialogs.Wpf by OOkii Dialogs Contributors  (used for selecting files, appearently WPF has literally no way to do this by defalt, yes really)
	* System.Text.Encoding.CodePages by Microsoft  (Used to decode and re-encode english and japanese text from hex)
	* EPPlus by EPPlus Software AB (used in the feature that lets you export editors, but is intended to be imported to google sheets and shared online)
	* WPFHexaEditor by Derek Tremblay (Not actually used yet, but it will be in the future to allow another way to visualize the editor data)
	* Appearently i'm using Microsoft.NET.ILLink.Tasks although i don't remember adding it. 




## History (for lore enjoyers)

This started as Etrian Editor, then became Crystal Tools, then Crystal Editor, and now Game Editor Studio. :3


- June 2025: I renamed the project from Crystal Editor to Game Editor Studio because i didn't want the acronym to be CE (The same as cheat engine).
- July 2 2025: First real public Release! (0.1.1)






## Lisence / Legal stuff.

- This program makes it easy to create editors, mods, patches, etc. Anything you create is a "contribution" and if i see your stuff online i can and will be adding them to this main download. 
- This is not open source. *FOR NOW.*
- But only because i spent like 120+ hours trying to understand all kinds of legal stuff between open source, lisences, terms and conditions, EULAs, CLAs, ICLAs, and more, and it was melting my brain and eating up WAY TO MUCH OF MY TIME vs actually developing the damn thing.
- This is just a complaint but, i can't believe how impressivly *unhelpful* r/opensource is at actually helping people understand how to make their project be open source. I got so many replies saying "You don't need this", "It's easier not to", "Theres no need". I KNOW THERES "NO NEED" BUT I'M ON R/OPENSOURCE FOR A REASON YOU FFFFffff.....SDLJFDSLKJFS!!!! LIKE OMG DUDE! ARRRRRGGGG!!!!!
- Anyway, its not open source only because it's been a legal nightmare. If you want something, just reach out to me on discord and i'll give you permission in the form of a written message.



