using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Server_Socket {
    public class SocketUser {
        public bool whitelisted;
        public ulong discordId;
        public string ip;
        public SocketUser(bool whitelisted, ulong discordId, string ip) {
            this.whitelisted = whitelisted;
            this.discordId = discordId;
            this.ip = ip;
        }
        public static bool socketuserExist(ulong discordId, ulong dId) {
            var c = JsonConvert.DeserializeObject<Faction_Settings.Settings>(new WebClient().DownloadString($"https://orbitdev.tech/FBP/database/{discordId}.json"));
            foreach (var i in c.socketUser) {
                if (i.discordId == dId)
                    return true;
            }
            return false;
        }
    }
}
