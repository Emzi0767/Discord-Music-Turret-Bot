// This file is a part of Music Turret project.
// 
// Copyright 2018 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Emzi0767.MusicTurret.Data;
using Emzi0767.MusicTurret.Services;

namespace Emzi0767.MusicTurret.Modules
{
    [Group("admin")]
    [Aliases("botctl")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireOwner, Hidden]
    public sealed class AdminModule : BaseCommandModule
    {
        private DatabaseContext Database { get; }

        public AdminModule(DatabaseContext database)
        {
            this.Database = database;
        }

        [Command("blacklist"), Description("Sets blacklisted status for a user, channel, or guild."), Aliases("block", "unblock")]
        public async Task BlacklistAsync(CommandContext ctx,
            [Description("User whose blacklisted status to change.")] DiscordUser user,
            [Description("Whether the user should be blacklisted.")] bool blacklisted, 
            [RemainingText, Description("Reason why this user is blacklisted.")] string reason = null)
        {
            var uid = (long)user.Id;
            var block = this.Database.EntityBlacklist.SingleOrDefault(x => x.Id == uid && x.Kind == DatabaseEntityKind.User);
            if (blacklisted && block == null)
            {
                block = new DatabaseBlacklistedEntity
                {
                    Id = uid,
                    Kind = DatabaseEntityKind.User,
                    Reason = reason,
                    Since = DateTime.UtcNow
                };
                this.Database.EntityBlacklist.Add(block);
            }
            else if (!blacklisted && block != null)
            {
                this.Database.EntityBlacklist.Remove(block);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} User {user.Mention} is {(blacklisted ? "now blacklisted" : "no longer blacklisted")}.").ConfigureAwait(false);
        }

        [Command("blacklist")]
        public async Task BlacklistAsync(CommandContext ctx,
            [Description("Channel of which blacklisted status to change.")] DiscordChannel channel,
            [Description("Whether the user should be blacklisted.")] bool blacklisted,
            [RemainingText, Description("Reason why this user is blacklisted.")] string reason = null)
        {
            var cid = (long)channel.Id;
            var block = this.Database.EntityBlacklist.SingleOrDefault(x => x.Id == cid && x.Kind == DatabaseEntityKind.Channel);
            if (blacklisted && block == null)
            {
                block = new DatabaseBlacklistedEntity
                {
                    Id = cid,
                    Kind = DatabaseEntityKind.Channel,
                    Reason = reason,
                    Since = DateTime.UtcNow
                };
                this.Database.EntityBlacklist.Add(block);
            }
            else if (!blacklisted && block != null)
            {
                this.Database.EntityBlacklist.Remove(block);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Channel {channel.Mention} is {(blacklisted ? "now blacklisted" : "no longer blacklisted")}.").ConfigureAwait(false);
        }

        [Command("blacklist")]
        public async Task BlacklistAsync(CommandContext ctx,
            [Description("Guild of which blacklisted status to change.")] DiscordGuild guild,
            [Description("Whether the user should be blacklisted.")] bool blacklisted,
            [RemainingText, Description("Reason why this user is blacklisted.")] string reason = null)
        {
            var gid = (long)guild.Id;
            var block = this.Database.EntityBlacklist.SingleOrDefault(x => x.Id == gid && x.Kind == DatabaseEntityKind.Guild);
            if (blacklisted && block == null)
            {
                block = new DatabaseBlacklistedEntity
                {
                    Id = gid,
                    Kind = DatabaseEntityKind.Guild,
                    Reason = reason,
                    Since = DateTime.UtcNow
                };
                this.Database.EntityBlacklist.Add(block);
            }
            else if (!blacklisted && block != null)
            {
                this.Database.EntityBlacklist.Remove(block);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Guild {Formatter.Bold(Formatter.Sanitize(guild.Name))} is {(blacklisted ? "now blacklisted" : "no longer blacklisted")}.").ConfigureAwait(false);
        }

        [Command("blacklist")]
        public async Task BlacklistAsync(CommandContext ctx)
        {
            var sb = new StringBuilder("Following entities are blacklisted:\n\n");

            foreach (var x in this.Database.EntityBlacklist)
                sb.Append($"{(ulong)x.Id} ({x.Kind}, since {x.Since:yyyy-MM-dd HH:mm:ss zzz}): {(string.IsNullOrWhiteSpace(x.Reason) ? "no reason specified" : x.Reason)}\n");

            await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
        }

        [Group("prefix"), ModuleLifespan(ModuleLifespan.Transient), Description("Commands for managing the prefixes that trigger the bot's commands."), Aliases("pfx")]
        public class Prefix : BaseCommandModule
        {
            private DatabaseContext Database { get; }
            private TurretBot Bot { get; }

            public Prefix(DatabaseContext database, TurretBot bot)
            {
                this.Database = database;
                this.Bot = bot;
            }

            [GroupCommand]
            public async Task ListAsync(CommandContext ctx)
            {
                var gid = (long)ctx.Guild.Id;
                var gpfx = this.Database.Prefixes.SingleOrDefault(x => x.GuildId == gid);
                var dcfg = this.Bot.Configuration.Discord;

                var sb = new StringBuilder();
                sb.Append($"Prefixes for {Formatter.Sanitize(ctx.Guild.Name)}:\n\n");
                if (gpfx == null)
                {
                    if (dcfg.DefaultPrefixes.Any())
                        sb.Append(string.Join(" ", dcfg.DefaultPrefixes.Select(Formatter.InlineCode)));

                    if (dcfg.DefaultPrefixes.Any() && dcfg.EnableMentionPrefix)
                        sb.Append(" ");

                    if (dcfg.EnableMentionPrefix)
                        sb.Append(ctx.Client.CurrentUser.Mention);
                }
                else
                {
                    if (dcfg.EnableMentionPrefix)
                        sb.Append(ctx.Client.CurrentUser.Mention);

                    if (dcfg.EnableMentionPrefix && gpfx.EnableDefault == true && dcfg.DefaultPrefixes.Any())
                        sb.Append(" ");

                    if (gpfx.EnableDefault == true && dcfg.DefaultPrefixes.Any())
                    {
                        sb.Append(string.Join(" ", dcfg.DefaultPrefixes.Select(Formatter.InlineCode)));
                    }

                    if (gpfx.EnableDefault == true && dcfg.DefaultPrefixes.Any() && gpfx.Prefixes?.Any() == true)
                        sb.Append(" ");

                    if (gpfx.Prefixes?.Any() == true)
                    {
                        sb.Append(string.Join(" ", gpfx.Prefixes.Select(Formatter.InlineCode)));
                    }
                }

                await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
            }

            [Command("add"), Description("Adds a prefix to this guild's command prefixes.")]
            public async Task AddPrefixAsync(CommandContext ctx,
                [Description("Prefix to add to this guild's prefixes.")] string prefix)
            {
                var gid = (long)ctx.Guild.Id;
                var gpfx = this.Database.Prefixes.SingleOrDefault(x => x.GuildId == gid);

                if (gpfx?.EnableDefault != false && this.Bot.Configuration.Discord.DefaultPrefixes.Contains(prefix))
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} Cannot add default prefix.").ConfigureAwait(false);
                    return;
                }

                if (gpfx == null)
                {
                    gpfx = new DatabasePrefix
                    {
                        GuildId = gid,
                        Prefixes = new[] { prefix },
                        EnableDefault = true
                    };
                    this.Database.Prefixes.Add(gpfx);
                }
                else if (!gpfx.Prefixes.Contains(prefix))
                {
                    gpfx.Prefixes = gpfx.Prefixes.Concat(new[] { prefix }).ToArray();
                    this.Database.Prefixes.Update(gpfx);
                }

                await this.Database.SaveChangesAsync().ConfigureAwait(false);
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Prefix added.").ConfigureAwait(false);
            }

            [Command("remove"), Description("Removes a prefix from this guild's command prefixes."), Aliases("rm", "delete", "del")]
            public async Task RemovePrefixAsync(CommandContext ctx,
                [Description("Prefix to remove from this guild's prefixes.")] string prefix)
            {
                var gid = (long)ctx.Guild.Id;
                var gpfx = this.Database.Prefixes.SingleOrDefault(x => x.GuildId == gid);
                if (gpfx != null && gpfx.Prefixes.Contains(prefix))
                {
                    gpfx.Prefixes = gpfx.Prefixes.Except(new[] { prefix }).ToArray();
                    this.Database.Prefixes.Update(gpfx);
                }
                else if (gpfx != null && !gpfx.Prefixes.Contains(prefix))
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} This prefix is not configured.").ConfigureAwait(false);
                    return;
                }

                await this.Database.SaveChangesAsync().ConfigureAwait(false);
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Prefix removed.").ConfigureAwait(false);
            }

            [Command("enabledefault"), Description("Configures whether default prefixes are to be enabled in this guild."), Aliases("default", "def")]
            public async Task ConfigureDefaultPrefixesAsync(CommandContext ctx,
                [RemainingText, Description("Whether default prefixes are to be enabled.")] bool enable)
            {
                var gid = (long)ctx.Guild.Id;
                var gpfx = this.Database.Prefixes.SingleOrDefault(x => x.GuildId == gid);
                if (gpfx == null)
                {
                    gpfx = new DatabasePrefix
                    {
                        GuildId = gid,
                        Prefixes = new string[] { },
                        EnableDefault = enable
                    };
                    this.Database.Prefixes.Add(gpfx);
                }
                else
                {
                    gpfx.EnableDefault = enable;
                    this.Database.Prefixes.Update(gpfx);
                }

                await this.Database.SaveChangesAsync().ConfigureAwait(false);
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Setting saved.").ConfigureAwait(false);
            }
        }
    }
}
