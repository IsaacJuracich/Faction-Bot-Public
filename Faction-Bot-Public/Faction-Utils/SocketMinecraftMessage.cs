using System;
using System.Collections.Generic;

namespace Faction_Bot_Public.Faction_Utils {
    class SocketMinecraftMessage {
        public string content;
        public string player;
        public DateTime time;
        public static List<SocketMinecraftMessage> socketcollection = new List<SocketMinecraftMessage>();

        public SocketMinecraftMessage(string content, string player, DateTime time) {
            this.content = content;
            this.player = player;
            this.time = time;
            socketcollection.Add(new SocketMinecraftMessage(content, player, time));
        }
    }
}
