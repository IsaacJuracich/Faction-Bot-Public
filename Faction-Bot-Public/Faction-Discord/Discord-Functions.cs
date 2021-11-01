using CG.Web.MegaApiClient;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    public class Discord_Functions {
        public static List<string> DisplayNodesRecursive(IEnumerable<INode> nodes, INode parent, int level = 0) {
            List<string> value = new List<string>();
            IEnumerable<INode> children = nodes.Where(x => x.ParentId == parent.Id);
            foreach (INode child in children) {
                string infos = $"- {child.Name} - {child.Size} bytes - {child.CreationDate}";
                value.Add(infos.PadLeft(infos.Length + level, '\t'));
                if (child.Type == NodeType.Directory)
                    DisplayNodesRecursive(nodes, child, level + 1);
            }
            return value;
        }
    }
}
