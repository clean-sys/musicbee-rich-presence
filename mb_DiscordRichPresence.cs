using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using DiscordInterface;
using Utils;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using IniParser;
using IniParser.Model;

namespace MusicBeePlugin
{
	public partial class Plugin
	{
		private static FileIniDataParser iniParser = new FileIniDataParser();

		private readonly PluginInfo _about = new PluginInfo();

		private static DiscordRPC.RichPresence _rpcPresence = new DiscordRPC.RichPresence();

		public static MusicBeeApiInterface MbApiInterface;

		public static readonly Logger Logging = new Logger();
		
		public static readonly HttpClient httpClient = new HttpClient();

		public static string DiscordId = "";

		public PluginInfo Initialise(IntPtr apiInterfacePtr)
		{
			IniData data = iniParser.ReadFile(@"C:\\MusicBee-RichPresence\\Configuration.ini");
			DiscordId = data["Discord"]["AppID"].ToString();

			MbApiInterface = new MusicBeeApiInterface();
			MbApiInterface.Initialise(apiInterfacePtr);
			_about.PluginInfoVersion = PluginInfoVersion;
			_about.Name = "Discord Rich Presence";
			_about.Description = "A Richer Rich Presence for Musicbee.";
			_about.Author = "@maybeclean";
			_about.TargetApplication = "";   
			_about.Type = PluginType.General;
			_about.VersionMajor = 1;  
			_about.VersionMinor = 0;
			_about.Revision = 01; 
			_about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
			_about.ConfigurationPanelHeight = 0;

			if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", DiscordToken.GetAuthToken());

			InitialiseDiscord();

			return _about;
		}

		private void InitialiseDiscord()
		{
			var handlers = new DiscordRPC.DiscordEventHandlers
			{
				readyCallback = HandleReadyCallback,
				errorCallback = HandleErrorCallback,
				disconnectedCallback = HandleDisconnectedCallback
			};


			DiscordRPC.Initialize(DiscordId, ref handlers, true, null);
		}

		private void HandleReadyCallback() { }
		private void HandleErrorCallback(int errorCode, string message) { }
		private void HandleDisconnectedCallback(int errorCode, string message) { }

		private async void ProcessArtwork(string album)
		{
			var assetList = await Discord.GetAssetList().ReadAsStringAsync();

			var artworkData = MbApiInterface.NowPlaying_GetArtwork();

			var dataUri = "data:image/png;base64," + artworkData;

			var albumAssured = Utility.AssureByteSize(album, 32); //Asset keys larger than 32 bytes will cause an exception.

			if (assetList.Contains(albumAssured))
			{
				_rpcPresence.largeImageKey = albumAssured;
				return;
			}
			else
			{
				_rpcPresence.largeImageKey = "temp_uploading";
				await Discord.UploadAsset(albumAssured, dataUri);
				_rpcPresence.largeImageKey = albumAssured;
				return;
			}
		}

		private void UpdatePresence(string artist, string trackArtist, string track, string album, string duration, bool playing, int position, int volume, bool handleArtworks = false)
		{
			track = Utility.Utf16ToUtf8(track + " ");
			artist = Utility.Utf16ToUtf8("by " + artist);
			trackArtist = Utility.Utf16ToUtf8("by " + trackArtist);

			_rpcPresence.state = Utility.AssureByteSize(trackArtist.Substring(0, trackArtist.Length - 1), 128);
			_rpcPresence.details = Utility.AssureByteSize(track.Substring(0, track.Length - 1), 128);

			string largeText = " ";

			if (string.IsNullOrEmpty(album))
				largeText = track + " " + trackArtist;
			else
				largeText = album + " " + artist;

			_rpcPresence.largeImageText = Utility.AssureByteSize(largeText.Substring(0, largeText.Length - 1), 128);

			string cleanedAlbum = Utility.SanitizeAlbumName(album);

			if (handleArtworks)
				ProcessArtwork(cleanedAlbum);

			long now = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

			if (playing)
			{
				_rpcPresence.startTimestamp = now - position;

				string[] durations = duration.Split(':');
				long end = System.Convert.ToInt64(durations[0]) * 60 + System.Convert.ToInt64(durations[1]);

				_rpcPresence.endTimestamp = _rpcPresence.startTimestamp + end;
				_rpcPresence.smallImageKey = "playing";
				_rpcPresence.smallImageText = "Playing at " + volume.ToString() + "%";
			}
			else
			{
				_rpcPresence.endTimestamp = 0;
				_rpcPresence.startTimestamp = 0;
				_rpcPresence.smallImageKey = "paused";
				_rpcPresence.smallImageText = "Paused";
			}

			DiscordRPC.UpdatePresence(ref _rpcPresence);
		}

		public void Close(PluginCloseReason reason)
		{
			DiscordRPC.Shutdown();
		}

		public void ReceiveNotification(string sourceFileUrl, NotificationType type)
		{
			string artist = MbApiInterface.NowPlaying_GetFileTag(MetaDataType.AlbumArtist);
			string trackArtist = MbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
			string trackTitle = MbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle);
			string album = MbApiInterface.NowPlaying_GetFileTag(MetaDataType.Album);
			string duration = MbApiInterface.NowPlaying_GetFileProperty(FilePropertyType.Duration);
			int volume = Convert.ToInt32(MbApiInterface.Player_GetVolume() * 100.0f);
			int position = MbApiInterface.Player_GetPosition();

			if (string.IsNullOrEmpty(artist))
				artist = "[artist empty]";

			switch (type)
			{
				case NotificationType.PlayStateChanged:
					switch (MbApiInterface.Player_GetPlayState())
					{
						case PlayState.Playing:
							UpdatePresence(artist, trackArtist, trackTitle, album, duration, true, position / 1000, volume);
							break;
						case PlayState.Paused:
							UpdatePresence(artist, trackArtist, trackTitle, album, duration, false, 0, volume);
							break;
					}
					break;

				case NotificationType.TrackChanged:
					UpdatePresence(artist, trackArtist, trackTitle, album, duration, true, 0, volume, true);
					break;

				case NotificationType.VolumeLevelChanged:
					if (MbApiInterface.Player_GetPlayState() == PlayState.Playing)
							UpdatePresence(artist, trackArtist, trackTitle, album, duration, true, position / 1000, volume);
					break;
			}
		}
	}
}
