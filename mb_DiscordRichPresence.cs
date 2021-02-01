using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using DiscordInterface;
using Utils;
using System.Net.Http.Headers;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;

        private PluginInfo about = new PluginInfo();

        public static readonly HttpClient http_client = new HttpClient();

        public static string discord_id = "Your Discord APP ID";

        public static DiscordRPC.RichPresence rpc_presence = new DiscordRPC.RichPresence();

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "Discord Rich Presence";
            about.Description = "A Richer Rich Presence for Musicbee.";
            about.Author = "@cleaninfla";
            about.TargetApplication = "";   
            about.Type = PluginType.General;
            about.VersionMajor = 1;  
            about.VersionMinor = 0;
            about.Revision = 01; 
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;

            http_client.DefaultRequestHeaders.Add("Authorization", "Your Authorization Key");

            InitialiseDiscord();

            return about;
        }

        private void InitialiseDiscord()
        {
            DiscordRPC.DiscordEventHandlers handlers = new DiscordRPC.DiscordEventHandlers();
            handlers.readyCallback = HandleReadyCallback;
            handlers.errorCallback = HandleErrorCallback;
            handlers.disconnectedCallback = HandleDisconnectedCallback;
            DiscordRPC.Initialize(discord_id, ref handlers, true, null);
        }

        private void HandleReadyCallback() { }
        private void HandleErrorCallback(int errorCode, string message) { }
        private void HandleDisconnectedCallback(int errorCode, string message) { }

        private async void ProcessArtwork(string album)
        {
            string asset_list = await Discord.GetAssetList().ReadAsStringAsync();

            string artwork_data = mbApiInterface.NowPlaying_GetArtwork();

            string data_uri = "data:image/png;base64," + artwork_data;

            if (asset_list.Contains(album))
            {
                rpc_presence.largeImageKey = album;
                return;
            }
            else
            {
                rpc_presence.largeImageKey = "temp_uploading";
                await Discord.UploadAsset("1", album, data_uri);
                rpc_presence.largeImageKey = album;
                return;
            }
        }

        private void UpdatePresence(string artist, string track_artist, string track, string album, string duration, Boolean playing, int position, bool handle_artworks = false)
        {
            track = Utility.Utf16ToUtf8(track + " ");
            artist = Utility.Utf16ToUtf8("by " + artist);

            rpc_presence.state = track_artist.Substring(0, track_artist.Length - 1);
            rpc_presence.details = track.Substring(0, track.Length - 1);

            string large_text = " ";

            if (string.IsNullOrEmpty(album))
                large_text = track + " " + track_artist;
            else
                large_text = album + " " + artist;

            rpc_presence.largeImageText = large_text.Substring(0, large_text.Length - 1);

            char[] albumArray = album.ToCharArray();

            for (int i = 0; i < album.Length; i++)
            {
                if (album[i] == ' ' || album[i] == ':' || album[i] == '(' || album[i] == ')' || album[i] == '#') albumArray[i] = '_'; // Discord doesn't like these chars in asset names.

                else albumArray[i] = album[i];
            }

            string new_album = new String(albumArray).ToLower();

            if (handle_artworks)
                ProcessArtwork(new_album);

            long now = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            if (playing)
            {
                rpc_presence.startTimestamp = now - position;

                string[] durations = duration.Split(':');

                long end = System.Convert.ToInt64(durations[0]) * 60 + System.Convert.ToInt64(durations[1]);

                rpc_presence.endTimestamp = rpc_presence.startTimestamp + end;

                rpc_presence.smallImageKey = "playing";
                rpc_presence.smallImageText = "Playing";
            }

            else
            {
                rpc_presence.endTimestamp = 0;
                rpc_presence.startTimestamp = 0;
                rpc_presence.smallImageKey = "paused";
                rpc_presence.smallImageText = "Paused";
            }

            DiscordRPC.UpdatePresence(ref rpc_presence);
        }

        public bool Configure(IntPtr panelHandle)
        {
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            
            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Panel.FromHandle(panelHandle);
                Label prompt = new Label();
                prompt.AutoSize = true;
                prompt.Location = new Point(0, 0);
                prompt.Text = "prompt:";
                TextBox textBox = new TextBox();
                textBox.Bounds = new Rectangle(60, 0, 100, textBox.Height);
                configPanel.Controls.AddRange(new Control[] { prompt, textBox });
            }
            return false;
        }

        public void SaveSettings()
        {
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
        }

        public void Close(PluginCloseReason reason)
        {
            DiscordRPC.Shutdown();
        }

        public void Uninstall()
        {
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.AlbumArtist);
            string track_artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
            string track_title = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle);
            string album = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Album);
            string duration = mbApiInterface.NowPlaying_GetFileProperty(FilePropertyType.Duration);
            float volume = mbApiInterface.Player_GetVolume();
            int position = mbApiInterface.Player_GetPosition();

            if (string.IsNullOrEmpty(artist)) { artist = "[artist empty]"; }

            switch (type)
            {
                case NotificationType.PlayStateChanged:
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
                            UpdatePresence(artist, track_artist, track_title, album, duration, true, position / 1000);
                            break;
                        case PlayState.Paused:
                            UpdatePresence(artist, track_artist, track_title, album, duration, false, 0);
                            break;
                    }
                    break;
                case NotificationType.TrackChanged:
                    UpdatePresence(artist, track_artist, track_title, album, duration, true, 0, true);
                    break;
            }
        }
    }
}