using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Server_Socket {
    public class SocketUpload {
        public static string tosocket = null;
        public static async void socketUpload() {
            while (true) {
                new WebClient().DownloadString($"https://orbitdev.tech/FBP/output-buffer.php?dId=903873824062324766&code={Faction_Discord.Discord_Commands.sessionID}&data={tosocket}");
                tosocket = null;
                await Task.Delay(5500);
            }
        }
    } 
}
