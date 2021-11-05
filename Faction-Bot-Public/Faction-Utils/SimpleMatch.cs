using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Faction_Bot_Public.Faction_Utils {
    public class SimpleMatch {
        public string char1;
        public string char2;
        public SimpleMatch(string char1, string char2) {
            this.char1 = char1;
            this.char2 = char2;
        }
        public string GetSubstringByString(string char1, string char2, string content) {
            return content.Substring((content.IndexOf(char1) + char1.Length), (content.IndexOf(char2) - content.IndexOf(char1) - char1.Length));
        }
        public List<SimpleMatch> match(string text, string pattern) {
            List<SimpleMatch> matches = new List<SimpleMatch>();
            return matches;
        }
    }
}
