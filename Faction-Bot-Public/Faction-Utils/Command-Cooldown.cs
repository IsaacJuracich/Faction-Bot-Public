using Discord.WebSocket;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Utils {
    class Command_Cooldown {
        public static async Task cooldownHandler(string type = null, SocketUserMessage msg = null, SocketMinecraftMessage msg2 = null) {
            if (type == null) return;
            if (msg != null && msg2 == null) {

            }
            else if (msg == null && msg2 != null) {

            }
        }
    }
}
