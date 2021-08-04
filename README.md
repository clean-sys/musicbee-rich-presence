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
Method 1: 

- Extract all files to a folder and run install.bat
- Upon starting up MusicBee you will be greeted with a Message Box that instructs you to configure your Discord Application ID.
- Musicbee will then restart and the plugin should now be ready.

Method 2:

- Manually copy all files to your MusicBee Plugins directory
- Upon starting up MusicBee you will be greeted with a Message Box that instructs you to configure your Discord Application ID.
- Musicbee will then restart and the plugin should now be ready.

## License
This repo is licensed under [WFTPL](http://www.wtfpl.net/).
