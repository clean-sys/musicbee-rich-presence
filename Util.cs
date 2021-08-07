using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.Http;
using MusicBeePlugin;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Utils
{

    public static class Utility
    {
        [DllImport("kernel32.dll")]
        private static extern int WideCharToMultiByte(uint codePage, uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string lpWideCharStr, int cchWideChar, [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder lpMultiByteStr, int cbMultiByte, IntPtr lpDefaultChar, IntPtr lpUsedDefaultChar);

        public static string Utf16ToUtf8(string utf16String)
        {
            int iNewDataLen = WideCharToMultiByte(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf16String, utf16String.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
            if (iNewDataLen > 1)
            {
                var utf8String = new StringBuilder(iNewDataLen);
                WideCharToMultiByte(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf16String, -1, utf8String, utf8String.Capacity, IntPtr.Zero, IntPtr.Zero);

                return utf8String.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string AssureByteSize(string input, int maxLength)
        {
            for (var i = input.Length - 1; i >= 0; i--)
            {
                if (Encoding.UTF8.GetByteCount(input.Substring(0, i + 1)) <= maxLength)
                {
                    return input.Substring(0, i + 1);
                }
            }

            return string.Empty;
        }

        public static string SanitizeAlbumName(string albumName)
        {
            var albumArray = albumName.ToCharArray();

            var symbolPattern = new Regex(@"[!@#$%^&*()+=\'[\""{\]};:<>|./?,\s-]", RegexOptions.Compiled);

            foreach (Match m in symbolPattern.Matches(albumName))
                albumArray[m.Index] = '_';

            var newAlbum = new string(albumArray).ToLower();

            return newAlbum;
        }

        public static void ReadConfig()
        {
            string confDiscordId = Plugin.IniParser.Read("AppID", "Discord");
            string confDiscordType = Plugin.IniParser.Read("DiscordType", "Discord");

            Plugin.DiscordId = confDiscordId;

            if (string.IsNullOrEmpty(confDiscordId))
                MessageBox.Show("Add your Discord Application ID for the Plugin in [Preferences -> Plugins]", "Failed to read Application ID");

            if (!string.IsNullOrEmpty(confDiscordType))
                Plugin.DiscordType = Convert.ToInt32(confDiscordType);

            if (Plugin.DiscordType == 0)
                MessageBox.Show("Add your Discord Type for the Plugin in [Preferences -> Plugins]", "Failed to read Discord Type");
        }
    }

    public static class Discord
    {
        public static HttpContent GetAssetList()
        {
            var response = Plugin.HttpClient.GetAsync($"https://discord.com/api/v9/oauth2/applications/{Plugin.DiscordId}/assets", HttpCompletionOption.ResponseContentRead);

            return response.Result.Content;
        }

        public static async Task UploadAsset(string albumName, string imageData)
        {
            var payload = JsonConvert.SerializeObject(new
            {
                name = albumName,
                type = "1",
                image = imageData,
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            Plugin.MbApiInterface.MB_SetBackgroundTaskMessage("Uploading artwork for " + albumName);

            var response = await Plugin.HttpClient.PostAsync($"https://discord.com/api/v9/oauth2/applications/{Plugin.DiscordId}/assets", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent.Contains(albumName))
                Plugin.MbApiInterface.MB_SetBackgroundTaskMessage($"Successfully Uploaded Artwork for {albumName}");
            else
                Plugin.MbApiInterface.MB_SetBackgroundTaskMessage($"Failed Artwork Upload for {albumName}");

            Plugin.Logging.LogWrite(responseContent);
        }
    }

    public class DiscordToken
    {
        public static string GetDiscordDirectory()
        {
            string directory;

            switch (Plugin.DiscordType)
            {
                case 1:
                    directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\discord\\Local Storage\\leveldb\\";
                    break;
                case 2:
                    directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\discordptb\\Local Storage\\leveldb\\";
                    break;
                case 3:
                    directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\discordcanary\\Local Storage\\leveldb\\";
                    break;
                default:
                    directory = "FAIL";
                    break;
            }

            return directory;
        }
        public static string GetAuthToken()
        {
            string tokenStr = "";
            DirectoryInfo directoryRoot = new DirectoryInfo(GetDiscordDirectory());

            foreach (var file in directoryRoot.GetFiles("*.ldb").OrderBy(f => f.LastWriteTime))
            {
                string fileOut = file.OpenText().ReadToEnd();
                Match mfaMatch = Regex.Match(fileOut, @"mfa\.[\w-]{84}");

                if (mfaMatch.Success)
                {
                    tokenStr = mfaMatch.Value;
                    break;
                }
            }

            return tokenStr;
        }
    }

    public class Logger
    {
        public Logger()
        {
            Directory.CreateDirectory("C:\\MusicBee-RichPresence");
        }

        public void LogWrite(string logMessage)
        {
            try
            {
                using (var w = File.AppendText("C:\\MusicBee-RichPresence\\log.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                Plugin.MbApiInterface.MB_SetBackgroundTaskMessage(ex.ToString());
            }
        }

        private void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine(" : {0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
                Plugin.MbApiInterface.MB_SetBackgroundTaskMessage(ex.ToString());
            }
        }
    }

    class ComboItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

}
