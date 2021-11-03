using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Server_Socket {
    class Code_Runner {
        public static async void Execute(string code) {
            try {
                Script script = CSharpScript.Create(code, ScriptOptions.Default.WithReferences(Assembly.GetExecutingAssembly()).WithImports("Faction_Bot_Public", "System.Threading.Tasks", "System"));
                var result = await script.RunAsync();
            }
            catch (CompilationErrorException e) {
                Console.WriteLine(string.Join(Environment.NewLine, e.StackTrace));
            }
        }
    }
}
