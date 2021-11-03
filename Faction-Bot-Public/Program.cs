using Faction_Bot_Public.Faction_Discord;
using MinecraftClient;
using MinecraftClient.Protocol;
using MinecraftClient.Protocol.Handlers.Forge;
using MinecraftClient.Protocol.Session;
using MinecraftClient.WinAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Faction_Bot_Public {
    public static class Program {
        public static readonly string BuildInfo = null;
        // # Faction-Bot Variables & Functions # //
        public static Thread discord;
        public static void discordStart() => Discord_Bot.Start().GetAwaiter().GetResult();

        // # End # //
        static void Main(string[] args) {
            Minecraft.Client.args = args;
            discord = new Thread(new ThreadStart(discordStart));
            discord.Start();
            Minecraft.Client.run(903873824062324766);
        }
        static Program() {
            AssemblyConfigurationAttribute attribute
             = typeof(Program)
                .Assembly
                .GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
                .FirstOrDefault() as AssemblyConfigurationAttribute;
            if (attribute != null)
                BuildInfo = attribute.Configuration;
        }
    }
}
