using CG.Web.MegaApiClient;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Discord {
    public class Discord_Commands : InteractiveBase<SocketCommandContext> {
        [Command("-setup", RunMode = RunMode.Async)] 
        public async Task SetupAsync() {
            if (Context.Channel is IPrivateChannel) {
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, Do this **`command`** inside of your guild.").Build());
                return;
            }
            if (Discord_Functions.doesConfigExist(Context.Guild.Id)) {
                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, You have already created a config, Do you wish to delete?").Build());
                var response = await NextMessageAsync(true, true);
                if (response.Content.ToLower() == "yes") {
                    Uri folderLink = new Uri("");
                    IEnumerable<INode> nodes = Discord_Bot.client.GetNodesFromLink(folderLink);
                    foreach (INode node in nodes.Where(x => x.Type == NodeType.File)) {
                        if (node.Name == $"{Context.Guild.Id}.ini") {
                            Discord_Bot.client.Delete(node, false);
                            await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, Config has been deleted, Do you wish to restart?").Build());
                            var response2 = await NextMessageAsync(true, true);
                            if (response2.Content.ToLower() == "yes") {
                                Discord_Bot.collection.Add(Context.User as SocketGuildUser);
                                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 1").WithDescription($"**What would you like your discord_prefix to be?**").Build());
                                var discord_prefix = await NextMessageAsync(true, true);
                                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 2").WithDescription($"**Would you like to add any admin_users right now? [If So, list all of their IDS]**").Build());
                                var admin_users = await NextMessageAsync(true, true);
                                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 1").WithDescription($"**What would you like the command_cooldown to be? [0-10] seconds**").Build());
                                var cmd_cooldown = await NextMessageAsync(true, true);
                                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("GuildSetup Finished").WithDescription($"**There is still alot more to setup, go through the set pages and manually do it**").Build());
                                Discord_Functions.generateConfigFile(Context.Guild.Id, discord_prefix.Content, admin_users.Content.Split(' ').ToList(), cmd_cooldown.Content);
                                Discord_Bot.collection.Remove(Context.User as SocketGuildUser);

                            }
                            if (response2.Content.ToLower() == "no") {
                                await ReplyAsync(embed: Discord_Functions.embed().WithDescription($"{Context.User.Mention}, You will be without a config file; therefor, the bot will not work.").Build());
                            }
                        }
                    }
                }
                return;
            }
            else {
                Discord_Bot.collection.Add(Context.User as SocketGuildUser);
                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 1").WithDescription($"**What would you like your discord_prefix to be?**").Build());
                var discord_prefix = await NextMessageAsync(true, false);
                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 2").WithDescription($"**Would you like to add any admin_users right now? [If So, list all of their IDS]**").Build());
                var admin_users = await NextMessageAsync(true, false);
                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("Question 3").WithDescription($"**What would you like the command_cooldown to be? [0-10] seconds**").Build());
                var cmd_cooldown = await NextMessageAsync(true, false);
                await Context.User.SendMessageAsync(embed: Discord_Functions.embed().WithTitle("GuildSetup Finished").WithDescription($"**There is still alot more to setup, go through the set pages and manually do it**").Build());
                Console.WriteLine($"{discord_prefix.Content} | {admin_users.Content} | {cmd_cooldown.Content}");
                Discord_Functions.generateConfigFile(Context.Guild.Id, discord_prefix.Content, admin_users.Content.Split(' ').ToList(), cmd_cooldown.Content);
                Discord_Bot.collection.Remove(Context.User as SocketGuildUser);
            }
        }
    }
}
