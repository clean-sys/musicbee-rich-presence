using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.Http;
using MusicBeePlugin;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Configuration;

namespace Utils
{
    public class Utility
    {
        [DllImport("kernel32.dll")]
        private static extern Int32 WideCharToMultiByte(UInt32 CodePage, UInt32 dwFlags, [MarshalAs(UnmanagedType.LPWStr)] String lpWideCharStr, Int32 cchWideChar, [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder lpMultiByteStr, Int32 cbMultiByte, IntPtr lpDefaultChar, IntPtr lpUsedDefaultChar);

        public static string Utf16ToUtf8(string utf16String)
        {
            Int32 iNewDataLen = WideCharToMultiByte(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf16String, utf16String.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
            if (iNewDataLen > 1)
            {
                StringBuilder utf8String = new StringBuilder(iNewDataLen);
                WideCharToMultiByte(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf16String, -1, utf8String, utf8String.Capacity, IntPtr.Zero, IntPtr.Zero);

                return utf8String.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
    }

    public class Discord
    {
        public static HttpContent GetAssetList()
        {
            var response = Plugin.http_client.GetAsync($"https://discordapp.com/api/oauth2/applications/{Plugin.discord_id}/assets", HttpCompletionOption.ResponseContentRead);

            return response.Result.Content;
        }

        public async static Task UploadAsset(string image_type, string album_name, string image_data)
        {
            string payload = JsonConvert.SerializeObject(new
            {
                name = album_name,
                type = image_type,
                image = image_data,
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await Plugin.http_client.PostAsync($"https://discordapp.com/api/oauth2/applications/{Plugin.discord_id}/assets", content);

            var response_content = await response.Content.ReadAsStringAsync();
        }
    }
}
