using MinecraftClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Bots {
    public class Faction_Main : ChatBot {
        public string GetSubstringByString(string char1, string char2, string content) {
            return content.Substring((content.IndexOf(char1) + char1.Length), (content.IndexOf(char2) - content.IndexOf(char1) - char1.Length));
        }
        public override void GetText(string text) {
            Faction_Utils.SocketMinecraftMessage socketMessage = new Faction_Utils.SocketMinecraftMessage(text, "", DateTime.Now, text.Split(' '));
        }
    }
}
