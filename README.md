# Enhanced Rich Presence for MusicBee

This plugin allows you to share your currently playing song, time remanining, play status, and automatically upload artworks for use on Discord.
This repo is [SonarCloud](https://sonarcloud.io/dashboard?id=cleaninfla_musicbee-rich-presence) enabled.

![Example Screenshot](https://i.imgur.com/F3udPi3.png)

## Requirements

- MusicBee v3.x
- Visual C++ Redistributable for Visual Studio 2019

## Installation
- Precompiled binaries coming soon.*

- Copy the DLL files to the Plugins folder
  - Extract the zip file
  - Copy the DLL files to the `MusicBee\Plugins` directory (most likely `C:\Program Files (x86)\MusicBee\Plugins` or `%appdata%\MusicBee\Plugins`)
  - Re/start MusicBee

## Getting your Authorization Token (Required for next step)
- Go to [Discord's developer application page](https://discordapp.com/developers/applications/me)
- Open your browser's developer tools and head to the "network" tab.
- Refresh the page and then click on a request.
- Look for a "Authorization" value that starts with "mfa"
- Copy that and save it for the next step.

![Authorization Example](https://i.imgur.com/znyZY8I.png)

### Creating a Discord Developer App

This step is needed if your want the plugin to upload and use your artworks.

- Go to [Discord's developer application page](https://discordapp.com/developers/applications/me)
- Create a new app. Call it whatever you want it to show after "playing".
- Copy the client ID (located in the *app details* section)
- Paste it into [line 22 of the main file](https://gitlab.com/cleaninfla/musicbee-rich-presence/-/blob/master/mb_DiscordRichPresence.cs#L22)
- Paste the MFA token you saved from the last step into [line 42](https://gitlab.com/cleaninfla/musicbee-rich-presence/-/blob/master/mb_DiscordRichPresence.cs#L42)
- Make sure to add your own Playing / Paused icons. (Keys are both 'playing' and 'paused' respectively).
- Compile & Profit?

## License
This repo is licensed under [WFTPL](http://www.wtfpl.net/).
