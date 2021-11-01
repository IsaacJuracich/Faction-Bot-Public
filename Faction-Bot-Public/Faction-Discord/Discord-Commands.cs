using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    public class Discord_Commands : InteractiveBase<SocketCommandContext> {
        [Command("test")]
        public async Task TestAsync() {
            await NextMessageAsync(true, false);
        }
    }
}
