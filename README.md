# Game Editor Studio

NOT open source, but i plan on figuring out lisencing stuff later.

PLEASE do NOT use this (yet) to make editors for games!!!
Any editors made will not be forward compatable with features i plan to add. 
Fixing forward compatability is currently my #1 priority, as soon as i'm confidant the program is forward compatabile i'll make a release.

If you want to help my discord is Dawnbomb.

Read about the project in much more detail over here: <https://www.crystalmods.com/index.php/crystal-editor/>

NuGet Packages used: 
Ookii.Dialogs.Wpf by OOkii Dialogs Contributors  (used for selecting files, appearently WPF has literally no way to do this by defalt, yes really)
System.Text.Encoding.CodePages by Microsoft  (used to encode / decode text from hex to english/japanese and back)

Project is C# / WPF / .net8 / Visual Studio 2022
(used to be .net7 but i changed to .net8 when it released)

# README FOR DEVELOPERS
- This is WPF instead of Avalonia UI because i'm new and it's missing Toolbox support. I will revisit when Avalonia Accelerate releases (It's supposed to have toolbox support).



OLD README

# Crystal Editor

Made in C# / WPF Targeting .net 7 Framework.

Crystal Editor is (trying to be) a universal RPG / JRPG / video game editor, modernizing the process of creating romhacks or major mods so simple a child can do it. Users can edit simple values in any game. (This is 99% of most modding projects). Things like Enemy stats, items, abilities, skills, equipment, enemy locations, evolutions, level up learnset, and so on, for *any game*.

Crystal editor is an unconventional half hex editor half modding framework, made to bridge the gap between the complexity of hex editing, and normal users.
It greatly lowers the complexity of hex-editing making it much easier for newcomers, and has been built from the ground up as the only game-focued hex editor.
There exists nothing else like it, it's unique approach to viewing, editing, and sorting data crushes everything else when it comes to video game modding.
Unlike normal hex editors, Crystal editor has bytes in decimal format. It then places these DBytes inside folders, and displays these folders in a grid-like format. 

As a 'simple' user of Crystal Editor, you can simply select a supported game, Target it on your computer, go to the editor you want, change values, save, and launch the game. It's *that* easy.

As a 'Creator' type user, you can add new games to the supported games list in just 1 click. Then, select a file you want a new editor to be made for. Tell Crystal Editor the table width of what you want to make an editor for, and give the editor an appropriate name. Crystal Editor will *Automatically* create an editor for that file / game. Next, a user can rename anything to more appropriate names, or put things into catagories. For example, one can select all of an enemys elemental resistances and put them into a catagory named "Resistances", so everything is in one location. You can even move the order of things around, like if an enemys stats are in order of Str, Mag, Def, Res, you can reorder it as Str, Def, Mag, Res. The program will still save the data back to the file in the correct order! 

On the inner technical side, Crystal Editor works by the way data is near-universall stored in video games. Regardless of what coding language was used to make a game, or what was done do it, almost every game converts all information to a file format called HEX. a Hex file conveniently, is not only easily readable, but is almost always a table. It's not just "Like" a google spreadsheet table, it is *literally* a table, and you can actually copy them to google sheets. Yes, modding games is really as simple as editing as basic table of information like a google sheet.

Modernly, modders work by copying this info to google sheets, doing everything the hard way, then copying it back. Alternatively, they spend a few years learning a coding language, and making an editor for 1 specific game. The worst part is the documentation teaching people how to do things on their own is awful, and you will almost never find anyone to help you and teach you what to do personally. It being easy doesn't matter, infact it makes it worse. People assume over and over you know more then you do, BECAUSE it's so easy, that they won't even "waste time" teaching these basics. Because of this, the modding scene fucking sucks, it's hard to get into, and the tools are intimidating, and you waste days googleing information just for your first project before giving up because google makes it look hard when it's not. Crystal Editor will be a program that can target *any* table, in *any* game, removing all this hard work, and include a full blown short but effective(short because theres not that much to learn), educational course on how to learn to do it yourself.

Modding is easy, now lets make it accessable to anyone. Together. Help me! :)


# LEGAL INFO (NOT OPEN SOURCE, ALL RIGHTS ARE RESERVED)
This project is NOT open source. You may NOT modify, distribute, or sell etc anything in here. Furthermore, you automatatically lose the rights to any work you contribute to this project the moment a pull request is accepted. I made a license.md file if you want a more detailed explanation. The short version is because I suspect once i get a stable beta off the ground, i may want to try selling it (for like 2$ or something) to afford hiring someone to help make it even better. If that bothers whoever wants to help me, we can talk about it. If i do end up profitting off it, i would pay anyone who helped create it. Anyway unless stated explicitly otherwise by me to you directly, you lose the right of ownership on anything you add to Crystal Editor.

# Documentation

Heres some random info about tools and stuff.

The program has the following Definitions: Tool, Command, Action, Event.

A Command is a class storing all kinds of data related to executing things. This is a base object / container.
An Action is code to be executed.  A command holds an action inside it. When the command is called, it executes the action it's storing.
A Tool is an actual download utility. An action may execute a tool, or do something using a tool. The command stores a reference to every tool it's action needs.
An event is a list of Commands. When an event is run, the commands are first checked to make sure their actions requirements are met (every commands tools are accounted for, etc). Afterward, it runs every command in sequence.
The database folder hold all Enums used in defining many things around the command system, all Tools data, Commands Data, and Actions Data. Events are just a list of events a end user makes, there is no master event list.


# Files
In a workshop, these is this stuff.
Documentation Folder: Meant to be sharable. I may turn this into nothing but .txt files and remove the subfoldering.
Editors:
Tools: Users can add special tools here not supported by the program that are needed for this workshop. Particularly useful for workshop specific tools, like a tool that is useful for only 1 game it was made for.

Commands.xml: A list of all commands a workshop uses. May later include events, and Event Resources.
LibraryInfo.xml: Info users won't share. A list of info about a workshop a user probably won't share with anyone else (no reason to). Loaded when a user clicks a workshop in the library.  

WorkshopInfo.xml: Users may collaborate share this. A list of editors to load in order (Consider replacing this with a EditorLoadOrder.txt) and a list of every file used in a workshop and their locations from InputFolder.
	Wait, don't i NEED to split this? if EVERYTHING about a editor was meant to be stored in its editor, isn't a file obviously related to it? a editor can't load without it's file?





Editor files / Code  (Go from either New or Load to Create)

NewXEditor:    Load editor data from preset into the Database.
LoadXEditor:   Load editor data from   XML  into the Database.
CreateXEditor: Generate an editor into a window, using data from the Database.

# History (for lore enjoyers)

This started as Etrian Editor, then became Crystal Tools, then Crystal Editor, and now Game Editor Studio. :3

Test3