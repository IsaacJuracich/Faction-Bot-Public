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
        
        public SocketMinecraftMessage(string content, string player, DateTime time, string[] args) {
            this.content = content;
            this.player = player;
            this.time = time;
            this.args = args;
        }
    }
}
