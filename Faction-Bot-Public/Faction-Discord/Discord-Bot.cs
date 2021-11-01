using CG.Web.MegaApiClient;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    internal class Discord_Bot {
        private static CommandService _commands = new CommandService();
        private static IServiceProvider _services;
        public static DiscordSocketClient bot;
        public static MegaApiClient client = new MegaApiClient();
        public static string prefix = "fbp";
        public static List<SocketGuildUser> collection = new List<SocketGuildUser>();
        public static async Task Start() {
            client.Login("", "");
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.MessageCacheSize = 100;
            bot = new DiscordSocketClient(config);
            await RegisterCommands();
            await bot.LoginAsync(Discord.TokenType.Bot, "", true);
            await bot.StartAsync();
            await bot.SetActivityAsync(new Game($"over {bot.Guilds.Count()} Guilds", ActivityType.Watching));
            await Task.Delay(-1);
        }
        private static async Task RegisterCommands() {
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(bot)
                .AddSingleton(_commands)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            bot.MessageReceived += CommandHandler;
            bot.JoinedGuild += JoinedGuild;
            bot.MessageReceived += GuildQuestions;
        }
        private static async Task GuildQuestions(SocketMessage arg) {
            if (arg.Channel is ITextChannel && collection.Contains(arg.Author)) {
                //await arg.Author.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("GuildSetup ").WithDescription($"Continue the GuildSetup in your **DMS**").Build());
            }
        }
        private static async Task JoinedGuild(SocketGuild arg) {
            Console.WriteLine($"[JoinedGuild] [{arg.Name}|ID:{arg.Id}] | Time: {DateTime.Now} | Owner: {arg.Owner}[{arg.Owner.Id}]");
            await arg.Owner.SendMessageAsync(embed: Discord_Functions.embed().WithDescription("**Guild Setup**\nYou will be asked a series of **questions** to setup this bot. You must respond to all of the questions for this to be **successful\n**Type: **`fbp-setup`** inside of your guild to get started.").Build());
            prefix = "fbp";
            return;
            System.IO.File.WriteAllText($"dump\\{arg.Id}.ini", "");
            IEnumerable<INode> nodes = client.GetNodes();
            INode parent = nodes.Single(n => n.Type == NodeType.Root);
            client.UploadFile($"dump\\{arg.Id}.ini", parent);
            System.IO.File.Delete($"dump\\{arg.Id}.ini");
            Console.WriteLine($"[GuildCreation] {arg.Id}.ini has been created and uploaded server side");
        }

        private static async Task CommandHandler(SocketMessage arg) {
            if (arg == null) return;
            var message = arg as SocketUserMessage;
            int argPos = 0;       
            if (message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(bot.CurrentUser, ref argPos)) {
                var context = new SocketCommandContext(bot, message);
                var result = await _commands.ExecuteAsync(context, argPos, _services);
            }
        }
    }
}
