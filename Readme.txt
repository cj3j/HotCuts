=============================================
	Overview
=============================================
HotCuts is a floating command-line utility for executing program shortcuts. It was created as a replacement for SlickRun (http://www.bayden.com/slickrun/) but with added support for profiles and templated shortcuts.

Quick Use
To add shortcuts you just have to edit the text Shortcuts.xml file. To execute a shortcut press the hotkeys to show the command-line window, type in the shortcut alias, and press enter. Auto-complete makes it so you usually only have to type a couple of letters to find the shortcut.

The Shortcuts File
The shortcuts XML file has a few features that makes it more powerful than just a flat list of aliases and executables. Check out the ShortcutsFile page for a quick tutorial.

The Application
The floating command-line window is where you type in shortcut aliases for execution. It has a few customization options, check out the Application page.

Todo
- Extra text after the alias should be added as additional command-line arguments
- Create an installer
- Add support for exporting the shortcuts to another format

=============================================
	Application
=============================================

The application UI is where you type in shortcut aliases for execution. It has a few customization options to suit your needs. To access these options, right click on the floating UI window or tray icon (it looks like a red chili pepper).

Move Window

When you select this option, the floating window will follow your cursor around the screen until you left-click. Use it to choose where the window should sit.

Resize Window

Use this option to change the dimensions of the window. Once selected, the lower-right of the window will follow your mouse around for resizing. Left-click to finish.

Reset Settings

If for some reason the window seems to be missing, try this option to reset to defaults (your shortcuts will not be affected).

Edit Shortcuts

This will open the shortcuts file in your default text editor. The application starts using the new shortcuts as soon as the file is saved.

Options

Opens the options window where you can edit some application preferences.

Options (Shortcuts)

Xml File: enter the path to the shortcuts xml file. By default, it uses the Shortcuts.xml file that is in the same folder as the application.

Profile Selector: If you need more advanced profile selection other than just setting the default profile, you can have this field point to some executable or batch file for selecting the profile. The output of that application will be used as the profile name.

Default Profle: The name of the profile to use. If not specified, the first one is used.

Options (Application)

Hotkeys: The keystrokes used to focus on the command line window. Just select this text box and press the keys

Start With Windows: Check this box to have HotCuts? open when Windows starts up.

=============================================
	Shortcuts file
=============================================

The shortcuts XML file has a few features that makes it more powerful than just a flat list of aliases and executables. Here's the most basic form:

<HotCuts>
  <Profile name="Default">
    <Shortcut name="editshortcuts">
      <Path>Shortcuts.xml</Path>
    </Shortcut>
  </Profile>
</HotCuts>
Profile

The Profile node creates a named collection of shortcuts. Having different profiles is useful if you want to share your shortcuts file among multiple computers (work, home), but need slightly different variations of the shortcuts. For example, my "Laptop" profile inherits all the shortcuts from "Default", but has the applications start on a single screen instead of supporting multiple monitors.

Shortcut

The Shortcut node creates an aliased shortcut. Shortcuts have a few properties, one of them being Path (the path to the executable or file). With this file I can now type "editshortcuts" into the command-line and hit enter to execute the file (will open the file in notepad).

Another property of Shortcut is Params. This node defines the parameters to pass to the program that gets executed.

Inheritance

Profiles can inherit shortcuts from another profile, and shortcuts can inherit properties from other shortcuts. Just use inherits="NameToInheritFrom"

<Shortcut name="DoSomethingA">
  <Path>C:\DoSomething.bat</Path>
  <Params>FileA.txt</Params>
</Shortcut>
<Shortcut name="DoSomethingB" inherits="DoSomethingA">
  <Params>FileB.txt</Params>
</Shortcut>

Macros

The next noteworthy feature is the ability to create macros. A macro is just a way of saying "replace text A with text B". Macros are great for reducing copy-pasting when shortcuts have a lot of shared properties.

<Define name="WindowsDrive" value="C:" />
<Shortcut name="web">
  <Path>{WindowsDrive}\chrome.exe</Path>
</Shortcut>

Macro values are inserted into shortcut properties using either curly braces "{ }" or brackets "[ ]". Curly braces mean the macro needs to be defined in order for the shortcut to work. Brackets means the macro is optional, and to just put blank space if the macro isn't defined.

Also, macro values can be inserted into the value of a macro definition. So you can do things like <Define name="WindowsFolder" value="{WindowsDrive}\Windows" />

When a profile or shortcut "inherits" from another profile or shortcut, it also inherits its macro definitions.

Templates

Sometimes you have a lot of shortcuts that have many things in common, such as path or arguments. You could just copy-paste a shortcut over-and-over, but then it's a pain if you want to make a change that affects all of the shortcuts. To facilitate this, you can make shortcut templates. Think of templates as an abstract shortcut that can be inherited from.

<Template name="UE3Client">
  <Path>C:\UDK.exe</Path>
  <Params>{MapName} -log -nosound</Params>
</Template>
<Shortcut name="GunGame" inherits="UE3Client">
  <Define name=MapName" value="GunGameIntro" />
</Shortcut>
<Shortcut name="AmmoGame" inherits="UE3Client">
  <Define name=MapName" value="AmmoGameMainMenu" />
</Shortcut>

Lists and List Templates

To go one level deeper, you can define an entire list of shortcuts as a template. For example, I need roughly all the same types of shortcuts for every game project I have (shortcuts for visual studio solution, server executable, client executable, editor). The list of shortcuts for every game is pretty much the same with just a few changes here and there, so I define them in lists and list templates.

<ListTemplate name="UE3Shortcuts">
  <Define name="Executable" value="{Path}\Binaries\Win32\{GameName}.exe" />
  
  <Shortcut name="{Prefix}server">
        <Path>{Executable}</Path>
        <Params>EntryMap?Listen -log</Params>
  </Shortcut>
  <Shortcut name="{Prefix}client">
        <Path>{Executable}</Path>
        <Params>127.0.0.1 -log</Params>
  </Shortcut>
  <Shortcut name="{Prefix}editor">
        <Path>{Executable}</Path>
        <Params>editor -log</Params>
  </Shortcut>
  <Shortcut name="{Prefix}vs">
        <Path>{Path}\Development\Src\UE3.sln</Path>
  </Shortcut>
</ListTemplate>

<List name="GunGameShortcuts" inherits="UE3Shortcuts">
  <Define name="Prefix" value="g" />
  <Define name="Path" value="d:\ProjectX\GunGame\UE3" />
  <Define name="GameName" value="GunGame" />
</List>

With the list template, the List node above actually defines 4 new shortcut aliases: gserver, gclient, geditor, gvs. Notice that you can use macros when declaring the name of a shortcut. Armed with this list template, I can now quickly create the 4 shortcuts I need for the next new game project that comes up.

All Together

That's all for the various features of the shortcuts file. Here's a snippet of my own shortcuts file.

<HotCuts>

  <Profile name="Default">
        <Define name="ServerWindowX" value="1680" />
        <Define name="SingleWindowX" value="700" />
        <Define name="ConsoleWindowY" value="600" />
        <Define name="UE3Params" value="[URL] -wxwindows -WindowPosX={WindowX} -WindowPosY=0 -consoleposx={WindowX} -consoleposy={ConsoleWindowY} -log -nomovie -nosteam -nolive [ExtraParams]" />

        <Template name="UE3Server">
          <Define name="URL" value="{Map}?Listen?[ExtraURL]" />
          <Define name="WindowX" value="{ServerWindowX}" />
          <Params>{UE3Params}</Params>
        </Template>

        <Template name="UE3Client">
          <Define name="URL" value="127.0.0.1?[ExtraURL]" />
          <Define name="WindowX" value="0" />
          <Params>{UE3Params}</Params>
        </Template>

        <ListTemplate name="UE3Shortcuts">
          <Define name="Executable" value="{Path}\Binaries\Win32\{GameName}.exe" />
          
          <Shortcut name="{Prefix}server" inherits="UE3Server">
                <Path>{Executable}</Path>
          </Shortcut>
          <Shortcut name="{Prefix}client" inherits="UE3Client">
                <Path>{Executable}</Path>
          </Shortcut>
          <Shortcut name="{Prefix}ed">
                <Path>{Executable}</Path>
                <Params>editor -log</Params>
          </Shortcut>
          <Shortcut name="{Prefix}vs">
                <Path>{Path}\Development\Src\{SLN}.sln</Path>
          </Shortcut>
        </ListTemplate>
        
        <List name="KillShortcuts" inherits="UE3Shortcuts">
          <Define name="Prefix" value="k" />
          <Define name="Path" value="d:\ProjectX\KILL\UE3" />
          <Define name="GameName" value="KILLGame" />
          <Define name="SLN" value="UE3" />
          <Define name="Map" value="test_creepcombat" />
          <Define name="ExtraURL" value="?Team=1" />
        </List>

        <List name="TAShortcuts" inherits="UE3Shortcuts">
          <Define name="Prefix" value="ta" />
          <Define name="Path" value="D:\ProjectX\Proto" />
          <Define name="GameName" value="TAGame" />
          <Define name="SLN" value="ProjectX" />
          <Define name="Map" value="stadium" />
        </List>
        
        <List name="RatShortcuts" inherits="UE3Shortcuts">
          <Define name="Prefix" value="r" />
          <Define name="Path" value="D:\Rat" />
          <Define name="GameName" value="RatGame" />
          <Define name="SLN" value="ProjectX" />
          <Define name="Map" value="die_Level_01_ai" />
        </List>
        
  </Profile>

  <Profile name="Laptop" inherits="Default">
        <Define name="SingleWindowX" value="700" />
        <Define name="ServerWindowX" value="{SingleWindowX}" />
  </Profile>
  
</HotCuts>