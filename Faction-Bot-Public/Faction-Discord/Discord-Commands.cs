using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    public class Discord_Commands : InteractiveBase<SocketCommandContext> {
        public static WebClient wc = new WebClient();
        [Command("setup", RunMode = RunMode.Async)] 
        public async Task SetupAsync() {
            try {
                SocketGuild g = Context.Guild;
                if (Discord_Functions.tryDownload($"https://orbitdev.tech/FBP/database/{Context.Guild.Id}.json")) {
                    await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, You have already created a config, Do you wish to delete?").Build());
                    var response = await NextMessageAsync(true, true);
                    if (response.Content.ToLower() == "yes") {

                        if (wc.DownloadString($"https://orbitdev.tech/FBP/fbpdelete.php?gID={Context.Guild.Id}.json").Contains("file exist"))
                            await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"**{Context.Guild.Id}** Config file has been deleted").Build());
                    }
                    return;
                }
                else if (!Discord_Functions.tryDownload($"https://orbitdev.tech/FBP/database/{Context.Guild.Id}.json")) {
                    Discord_Bot.collection.Add(Context.User as SocketGuildUser);
                    await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 1").WithDescription($"**What would you like your discord_prefix to be?**").Build());
                    var discord_prefix = await NextMessageAsync(true, false);
                    await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 2").WithDescription($"**Would you like to add any admin_users right now? [If So, list all of their IDS]**").Build());
                    var admin_users = await NextMessageAsync(true, false);
                    await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 3").WithDescription($"**What would you like the command_cooldown to be? [0-10] seconds**").Build());
                    var cmd_cooldown = await NextMessageAsync(true, false);
                    await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("GuildSetup Finished").WithDescription($"**There is still alot more to setup, go through the set pages and manually do it**").Build());
                    string data = null;
                    foreach (var i in admin_users.ToString().Split(' '))
                        data = data + " " + $"<@{i}>";
                    await ReplyAsync(embed: Discord_Functions.embed().WithAuthor($"Config [{g.Id}]", Context.User.GetAvatarUrl()).WithDescription($"" +
                        $"**d_prefix**: {discord_prefix}\n" +
                        $"**admin_users**: {data}\n" +
                        $"**cmd_cooldown**: {cmd_cooldown} seconds\n" +
                        $"**configurer**: {Context.User.Mention}\n" +
                        $"**guild_name**: {g.Name}").Build());
                    using (var client = new WebClient()) {
                        Faction_Settings.Settings s = new Faction_Settings.Settings(".", new List<ulong>(), 0, g.OwnerId);
                        JsonSerializerSettings settings1 = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
                        string contents = JsonConvert.SerializeObject(s, Formatting.Indented, settings1);
                        System.IO.File.WriteAllText($"dump\\{Context.Guild.Id}.json", contents);
                        var response = client.UploadFile($"https://orbitdev.tech/FBP/fbpadd.php?gID={Context.Guild.Id}", $"dump\\{Context.Guild.Id}.json");
                        var d = Encoding.Default.GetString(response);
                        Console.WriteLine("[Server] Data from server: " + d);
                        Discord_Bot.collection.Remove(Context.User as SocketGuildUser);
                    }
                }
            } 
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
        }
        [Command("set", RunMode = RunMode.Async)]
        public async Task SetAsync(string setting, string content) {
            try {
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
        }
        [Command("Login")]
        public async Task LaunchAsync() {
            var c = JsonConvert.DeserializeObject<Faction_Settings.Settings>(new WebClient().DownloadString($"https://orbitdev.tech/FBP/database/{Context.Guild.Id}.json"));
            if (Minecraft.Client.client != null) {
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"**Connected Data**\n\n" +
                    $"**Connected Server:** {Minecraft.Client.client.GetServerHost()}\n" +
                    $"**Connected Account:** {Minecraft.Client.client.GetUsername()}").Build());
                return;
            }
            if (c.d_adminusers.Contains(Context.User.Id)) {
                Minecraft.Client.run(Context.Guild.Id);
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"**Login Data**\n\n" +
                    $"**Connected Server:** {Minecraft.Client.client.GetServerHost()}\n" +
                    $"**Connected Account:** {Minecraft.Client.client.GetUsername()}").Build());
                new WebClient().DownloadString($"https://orbitdev.tech/FBP/fbpconnection.php?type=add&data={Context.Guild.Id}");
            }
            else {
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, You are not an **admin_user**").Build());
            }
        }
        [Command("Logout")]
        public async Task LogoutAsync() {
            var c = JsonConvert.DeserializeObject<Faction_Settings.Settings>(new WebClient().DownloadString($"https://orbitdev.tech/FBP/database/{Context.Guild.Id}.json"));
            if (Minecraft.Client.client == null) {
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, No account connected").Build());
                return;
            }
            if (c.d_adminusers.Contains(Context.User.Id)) {
                string acc = Minecraft.Client.client.GetUsername();
                Minecraft.Client.client.Disconnect();
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"**`{acc}`** Has logged out").Build());
                new WebClient().DownloadString($"https://orbitdev.tech/FBP/fbpconnection.php?type=remove&data={Context.Guild.Id}");
            }
            else {
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, You are not an **admin_user**").Build());
            }
        }
        [Command("outputbuffer")]
        public async Task outputbuffer() {
            var c = JsonConvert.DeserializeObject<Faction_Settings.Settings>(new WebClient().DownloadString($"https://orbitdev.tech/FBP/database/{Context.Guild.Id}.json"));
            if (c.d_adminusers.Contains(Context.User.Id)) {
                if (c.premium) {
                    if (Server_Socket.SocketUser.socketuserExist(Context.Guild.Id, Context.User.Id)) {
                        await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"").Build());
                    }
                    else {
                        await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, You are not a **socket_user**").Build());
                    }
                }
                else {
                    await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, This is a premium tier feature").Build());
                }
            }
            else {
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, You are not an **admin_user**").Build());
            }
        }
    }
}
