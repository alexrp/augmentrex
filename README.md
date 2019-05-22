# ![Augmentrex](Augmentrex/Properties/Hellgate.ico) Augmentrex

**Augmentrex** is a reverse engineering tool for the
[Steam version of Hellgate: London](https://store.steampowered.com/app/939520/HELLGATE_London).
It provides a generic framework for doing in-memory interaction with the
Hellgate: London process from managed code. Users can write plugins to add
additional functionality.

(The name comes from the in-game device, Augmentrex 3000, which is used to add
new affixes to items.)

## Features

Core features include:

* [EasyHook](https://easyhook.github.io)-based injection into the game process.
* Custom commands and reloadable plugins can be written in any managed language.
* Core APIs and commands for manipulating game memory and global hot keys.
* Built-in `OutputDebugString()` listener for game output.
* Automatic detection and launching of the game.

Custom commands included with the core:

* `patch-long-ray-vm`: Disables the game's ray casting engine, fixing the vast
  majority of frame rate issues (commonly known as the 1 FPS bug).
* `patch-cc-agent`: Disables the game's capsule-capsule collision agent, fixing
  frame rate issues for certain skills (e.g. Blademaster's Whirlwind).

Plugins included with the core:

* `simple-test`: Just a simple a plugin that outputs a message on startup and
  shutdown.

(Developers can use these as examples for implementing custom commands and
plugins.)

## Usage

Simply launch `Augmentrex.exe`. Augmentrex will locate the game executable via
Steam and launch it for you, then attach to the game process.

Once the game is open, you can type `patch-long-ray-vm` and/or `patch-cc-agent`
to toggle those patches (even during gameplay).

Advanced users can have a look in `Augmentrex.exe.config` if they wish to change
configuration values. Here are some values you might be interested in changing:

* `gamePath`: If Augmentrex cannot locate the game executable via Steam, you can
  set this value explicitly. For example, on my system, I would set this to
  `C:\Program Files (x86)\Steam\steamapps\common\HELLGATE_London\bin\Hellgate_sp_x86.exe`.
* `gameArguments`: Any command line arguments to pass to the game. I personally
  dislike the keyboard hook the game does to disable the Win key, so I would
  leave `-nokeyhooks` here.
* `gameConsoleEnabled` and `debugListenerEnabled`: You can set these to `False`
  if you are not interested in debug output from the game process.
* `disabledPlugins`: This can be used to disable individual plugins without
  removing the files from the Augmentrex directory.
* `runCommands`: You can use this to run commands at startup. For example, you
  could set it to
  `patch-cc-agent; patch-long-ray-vm; key --add -s F1 patch-long-ray-vm` to
  enable `patch-cc-agent` and `patch-long-ray-vm` at startup and set a Shift+F1
  key binding to toggle `patch-long-ray-vm`.

## Known Issues

The `patch-long-ray-vm` command has a few side effects to be aware of:

* You can see portal names and enemy nameplates through terrain.
* Collision detection for ranged attacks will be non-functional, allowing both
  you and certain enemy types to attack through some types of terrain.
* Enemy corpses will often just vanish when they die, or less frequently, end up
  in weird poses.
* Certain bosses (Ash and Oculis, possibly others) rely on ray casting for some
  of their attacks. You have to toggle the command off before you engage these
  bosses, or you will not be able to kill them.

As far as I am aware, the `patch-cc-agent` command has no negative side effects.

## Background

I started work on this project because I finally got sick of the dreaded 1 FPS
bug the game suffers from. Through various efforts (static reverse engineering,
dynamic debugging, CPU profiling), I finally managed to figure out why the
game's frame rate would drop so severely. It turns out that the game makes an
excessive number of ray cast queries under certain circumstances. I still need
to figure out exactly why that happens, but for now, disabling ray casting
altogether makes the game actually playable. This is done by patching the
`hkpMoppLongRayVirtualMachine::queryRayOnTree()` function from the Havok Physics
library used in the game, so that it simply returns rather than running the
bytecode passed to it. This effectively disables ray casting. Again, just to be
clear, this is treating the symptom rather than the cause, but it does at least
work.

In the future, I will be investigating various other frame rate issues with the
game, and so I figured I would need a more generic framework for making changes
to the game executable. Thus, this project, and the fix being a plugin.

By the way, while I love this game (which you can probably tell by the fact that
I bothered to do all this), I **absolutely** cannot recommend that anyone go and
buy it on Steam. This 1 FPS bug has existed for almost a decade, from all the
way back when the game was online-only. The bug was almost certainly introduced
by HanbitSoft when they took over the game from Flagship Studios. They have been
aware of the bug for all this time. Even when they re-released the game as a
single player title on Steam, they spent months pretending that people's PCs
just did not meet the game's minimum requirements.

To be perfectly clear: They are selling a broken product that is unplayable to
the vast majority of people. They were fully aware of this, and proceeded with
putting it on Steam anyway. They then went on to completely ignore the issue and
to this day have not fixed it, nor even acknowledged it. I actively encourage
everyone to report the game as being broken on the store page, because it is.
They should not be allowed to make money by selling such a blatantly broken
product.

Still, for those who bought the game and would still like to play it, this
project will help you do that.
