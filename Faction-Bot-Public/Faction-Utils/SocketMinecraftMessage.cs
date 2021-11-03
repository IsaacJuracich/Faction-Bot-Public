using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Utils {
    class SocketMinecraftMessage {
        public string content;
        public string player;
        public DateTime time;
        public string[] args;
        public static List<SocketMinecraftMessage> socketcollection = new List<SocketMinecraftMessage>();

        public SocketMinecraftMessage(string content, string player, DateTime time, string[] args) {
            this.content = content;
            this.player = player;
            this.time = time;
            this.args = args;
            socketcollection.Add(new SocketMinecraftMessage(content, player, time, args));
        }
    }
}
