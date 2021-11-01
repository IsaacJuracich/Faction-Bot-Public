using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    internal class Discord_Bot {
        private static CommandService _commands = new CommandService();
        private static IServiceProvider _services;
        public static DiscordSocketClient bot;
        public static string seconds;

        public static async Task Start() {
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.MessageCacheSize = 100;
            bot = new DiscordSocketClient(config);
            await RegisterCommands();
            await bot.LoginAsync(Discord.TokenType.Bot, "", true);
            await bot.StartAsync();
            await bot.SetGameAsync("");
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
        }
        private static async Task CommandHandler(SocketMessage arg) {
            var message = arg as SocketUserMessage;
            int argPos = 0;
            if (message.HasStringPrefix("", ref argPos) || message.HasMentionPrefix(bot.CurrentUser, ref argPos)) {
                var context = new SocketCommandContext(bot, message);
                var result = await _commands.ExecuteAsync(context, argPos, _services);
            }
        }
    }
}
