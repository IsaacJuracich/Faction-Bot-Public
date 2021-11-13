using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Server_Socket {
    public class SocketReader {
        public static List<SocketReader> sockets = new List<SocketReader>();
        public ulong gId;
        public string sessionId;
        public SocketReader(ulong gId, string sessionId = null) {
            this.gId = gId;
            this.sessionId = sessionId;
        }
        public static void socketSessionUpdate(SocketReader socket, string sessionId) {
            if (sockets.Contains(socket))
                socket.sessionId = sessionId;
        }
        public static SocketReader socketReturn(ulong gId) {
            foreach (var i in sockets) {
                if (i.gId == gId)
                    return i;
            }
            return null;
        }
    }
}
