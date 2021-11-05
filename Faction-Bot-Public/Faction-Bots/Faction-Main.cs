using MinecraftClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Bots {
    public class Faction_Main : ChatBot {
        public override void Initialize() {
            Server_Socket.SocketUpload.socketUpload();
        }
        public override async void GetText(string text) {
            text = GetVerbatim(text).Replace("\n", "");
            if (text.Trim().Length < 1 || text == null || text.Replace(" ", "").Length < 1) return;
            Server_Socket.SocketUpload.tosocket = Server_Socket.SocketUpload.tosocket + text + "\n";
        }
        public override void AfterGameJoined() {
            var c = JsonConvert.DeserializeObject<Faction_Settings.Settings>(new WebClient().DownloadString($"https://orbitdev.tech/FBP/database/{Minecraft.Client.gId}.json"));
            SendText(c.m_hubcmd);
        }
    }
}
