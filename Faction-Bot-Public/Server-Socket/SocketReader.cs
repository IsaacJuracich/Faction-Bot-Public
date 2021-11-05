using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Server_Socket {
    public class SocketReader {
        public List<SocketReader> sockets = new List<SocketReader>();
        public ulong gId;
        public string sessionId;
        public SocketReader(ulong gId, string sessionId = null) {
            this.gId = gId;
            this.sessionId = sessionId;
        }
        public void socketSessionUpdate(SocketReader socket, string sessionId) {
            if (sockets.Contains(socket))
                socket.sessionId = sessionId;
        }
        public SocketReader socketReturn(ulong gId) {
            foreach (var i in sockets) {
                if (i.gId == gId)
                    return i;
            }
            return null;
        }
    }
}
