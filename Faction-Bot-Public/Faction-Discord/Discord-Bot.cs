using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    internal class Discord_Bot {
        private static CommandService _commands = new CommandService();
        private static IServiceProvider _services;
        public static DiscordSocketClient bot;
        public static string prefix = "fbp";
        public static List<SocketGuildUser> collection = new List<SocketGuildUser>();
        public static async Task Start() {
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.MessageCacheSize = 100;
            config.AlwaysDownloadUsers = true;
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
        }

        private static async Task CommandHandler(SocketMessage arg) {
            if (arg == null || arg.Channel is IPrivateChannel) return;
            var message = arg as SocketUserMessage;
            int argPos = 0;
            var context = new SocketCommandContext(bot, message);
            if (Discord_Functions.tryDownload($"https://orbitdev.tech/FBP/database/{context.Guild.Id}.json")) {
                var c = JsonConvert.DeserializeObject<Faction_Settings.Settings>(new WebClient().DownloadString($"https://orbitdev.tech/FBP/database/{context.Guild.Id}.json"));
                prefix = c.d_prefix;
                if (prefix == "{prefix}") {
                    prefix = "fbp";
                    await context.Channel.SendMessageAsync(embed: Discord_Functions.embed().WithDescription($"{context.User.Mention}, There is no prefix set | Default value: **fbp**").Build());
                }
            }
            if (!Discord_Functions.tryDownload($"https://orbitdev.tech/FBP/database/{context.Guild.Id}.json"))
                prefix = "fbp";
            if (message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(bot.CurrentUser, ref argPos)) {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
