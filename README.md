# Enhanced Rich Presence for MusicBee

This plugin allows you to share your currently playing song, time remanining, play status, and automatically upload artworks for use on Discord.

![Example Screenshot](https://i.imgur.com/F3udPi3.png)

## Requirements

- MusicBee v3.x
- Visual C++ Redistributable for Visual Studio 2019

## Creating a Discord Developer App

This step is needed if your want the plugin to upload and use your artworks.

- Go to [Discord's developer application page](https://discordapp.com/developers/applications/me)
- Create a new app. Call it whatever you want it to show after "playing".
- Copy the client ID and save it for next step (located in the *app details* section)
- Make sure to add your own Playing / Paused icons. (Keys are both 'playing' and 'paused' respectively).

## Installation
- Copy the DLL files to the Plugins folder
  - Extract the zip file
  - Copy the DLL files to the `MusicBee\Plugins` directory (most likely `C:\Program Files (x86)\MusicBee\Plugins` or `%appdata%\MusicBee\Plugins`)
- Copy the MusicBee-RichPresence folder to the root of your C:\ drive.
  - Edit the configuration file to replace the `XXXXXXX` with your App ID you saved.

## License
This repo is licensed under [WFTPL](http://www.wtfpl.net/).
