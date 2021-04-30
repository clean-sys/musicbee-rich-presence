using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.Http;
using MusicBeePlugin;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;

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

        public static string HashAlbum(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var hashed = BitConverter.ToString(new System.Security.Cryptography.SHA256Managed().ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty);
            return AssureByteSize(hashed, 10);
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
    }

    public static class Discord
    {
        public static HttpContent GetAssetList()
        {
            var response = Plugin.httpClient.GetAsync($"https://discordapp.com/api/oauth2/applications/{Plugin.DiscordId}/assets", HttpCompletionOption.ResponseContentRead);

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

            var response = await Plugin.httpClient.PostAsync($"https://discordapp.com/api/oauth2/applications/{Plugin.DiscordId}/assets", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent.Contains(albumName))
                Plugin.MbApiInterface.MB_SetBackgroundTaskMessage("Successfully uploaded artwork.");
            else
                Plugin.MbApiInterface.MB_SetBackgroundTaskMessage("Artwork upload failed, check log.");

            Plugin.Logging.LogWrite(responseContent);
        }
    }

    public class DiscordToken
    {
        private static string GrabToken(string stringx)
        {
            byte[] bytes = File.ReadAllBytes(stringx);
            string fileContents = Encoding.UTF8.GetString(bytes);
            string authToken = "";

            if (fileContents.Contains("token"))
            {
                string[] array = FindToken(fileContents).Split(new char[]
                {
                    '"'
                });

                authToken = array[0];
            }

            return authToken;
        }
        private static bool FindLDB(ref string levelDB)
        {
            if (Directory.Exists(levelDB))
            {
                foreach (FileInfo fileInfo in new DirectoryInfo(levelDB).GetFiles())
                {
                    if (fileInfo.Name.EndsWith(".ldb") && File.ReadAllText(fileInfo.FullName).Contains("token"))
                    {
                        levelDB += fileInfo.Name;
                        return levelDB.EndsWith(".ldb");
                    }
                }
                return levelDB.EndsWith(".ldb");
            }
            return false;
        }

        private static string FindToken(string path)
        {
            string[] array = path.Substring(path.IndexOf("token") + 4).Split(new char[]
            {
                '"'
            });

            List<string> list = new List<string>();
            list.AddRange(array);
            list.RemoveAt(0);

            array = list.ToArray();
            return string.Join("\"", array);
        }

        public static string GetAuthToken()
        {
            string pathStr = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\discord\\Local Storage\\leveldb\\";

            if (!FindLDB(ref pathStr))
                return "";

            string tokenStr = GrabToken(pathStr);

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
}
