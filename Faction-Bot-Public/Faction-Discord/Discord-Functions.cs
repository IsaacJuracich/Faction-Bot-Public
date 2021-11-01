using CG.Web.MegaApiClient;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    public class Discord_Functions {
        public static List<string> displayNodesRecursive(IEnumerable<INode> nodes, INode parent, int level = 0) {
            List<string> value = new List<string>();
            IEnumerable<INode> children = nodes.Where(x => x.ParentId == parent.Id);
            foreach (INode child in children) {
                string infos = $"- {child.Name} - {child.Size} bytes - {child.CreationDate}";
                value.Add(infos.PadLeft(infos.Length + level, '\t'));
                if (child.Type == NodeType.Directory)
                    displayNodesRecursive(nodes, child, level + 1);
            }
            return value;
        }
        public static EmbedBuilder embed() {
            return new EmbedBuilder().WithColor(71, 108, 215).WithCurrentTimestamp();
        }
        public static bool doesConfigExist(ulong gId) {
            Uri folderLink = new Uri("https://mega.nz/folder/8d1GQbBL#Os6d2DBL2rgwEzNOEMhUsA");
            IEnumerable<INode> nodes = Discord_Bot.client.GetNodesFromLink(folderLink);
            foreach (INode node in nodes.Where(x => x.Type == NodeType.File)) {
                if (node.Name == $"{gId}.ini") {
                    return true;
                }
            }
            return false;
        }
    }
}
