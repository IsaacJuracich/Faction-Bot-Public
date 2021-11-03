using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    public class Discord_Functions {
        public static EmbedBuilder embed() {
            return new EmbedBuilder().WithColor(71, 108, 215).WithCurrentTimestamp();
        }
        public static bool tryDownload(string web) {
            var wc = new System.Net.WebClient();
            try {
                if (!wc.DownloadString(web).Contains("error 404"))
                    return true;
            }
            catch (Exception e) { return false; }
            return false;
        }
    }
}
