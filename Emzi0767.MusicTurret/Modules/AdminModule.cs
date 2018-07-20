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
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Emzi0767.MusicTurret.Data;
using Emzi0767.MusicTurret.Services;

namespace Emzi0767.MusicTurret.Modules
{
    [ModuleLifespan(ModuleLifespan.Transient), RequireOwner, Hidden]
    public sealed class AdminModule : BaseCommandModule
    {
        private DatabaseContext Database { get; }
        private TurretBot Bot { get; }

        public AdminModule(DatabaseContext database, TurretBot bot)
        {
            this.Database = database;
            this.Bot = bot;
        }

        [Command("setblock"), Description("Sets blocked status for a user, channel, or guild."), Aliases("block", "unblock")]
        public async Task SetBlockAsync(CommandContext ctx, 
            [Description("Whether the user should be blocked.")] bool blocked, 
            [Description("User whose block status to change.")] DiscordUser user,
            [RemainingText, Description("Reason why this user is blocked.")] string reason = null)
        {
            var uid = (long)user.Id;
            var block = this.Database.BlockedEntities.SingleOrDefault(x => x.Id == uid && x.Kind == DatabaseEntityKind.User);
            if (blocked && block == null)
            {
                block = new DatabaseBlockedEntity
                {
                    Id = uid,
                    Kind = DatabaseEntityKind.User,
                    Reason = reason,
                    Since = DateTime.UtcNow
                };
                this.Database.BlockedEntities.Add(block);
            }
            else if (!blocked && block != null)
            {
                this.Database.BlockedEntities.Remove(block);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} User {user.Mention} is {(blocked ? "now blocked" : "no longer blocked")}.").ConfigureAwait(false);
        }

        [Command("setblock")]
        public async Task SetBlockAsync(CommandContext ctx,
            [Description("Whether the user should be blocked.")] bool blocked,
            [Description("Channel of which block status to change.")] DiscordChannel channel,
            [RemainingText, Description("Reason why this user is blocked.")] string reason = null)
        {
            var cid = (long)channel.Id;
            var block = this.Database.BlockedEntities.SingleOrDefault(x => x.Id == cid && x.Kind == DatabaseEntityKind.Channel);
            if (blocked && block == null)
            {
                block = new DatabaseBlockedEntity
                {
                    Id = cid,
                    Kind = DatabaseEntityKind.Channel,
                    Reason = reason,
                    Since = DateTime.UtcNow
                };
                this.Database.BlockedEntities.Add(block);
            }
            else if (!blocked && block != null)
            {
                this.Database.BlockedEntities.Remove(block);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Channel {channel.Mention} is {(blocked ? "now blocked" : "no longer blocked")}.").ConfigureAwait(false);
        }

        [Command("setblock")]
        public async Task SetBlockAsync(CommandContext ctx,
            [Description("Whether the user should be blocked.")] bool blocked,
            [Description("Guild of which block status to change.")] DiscordGuild guild,
            [RemainingText, Description("Reason why this user is blocked.")] string reason = null)
        {
            var gid = (long)guild.Id;
            var block = this.Database.BlockedEntities.SingleOrDefault(x => x.Id == gid && x.Kind == DatabaseEntityKind.Guild);
            if (blocked && block == null)
            {
                block = new DatabaseBlockedEntity
                {
                    Id = gid,
                    Kind = DatabaseEntityKind.Guild,
                    Reason = reason,
                    Since = DateTime.UtcNow
                };
                this.Database.BlockedEntities.Add(block);
            }
            else if (!blocked && block != null)
            {
                this.Database.BlockedEntities.Remove(block);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Guild {Formatter.Bold(Formatter.Sanitize(guild.Name))} is {(blocked ? "now blocked" : "no longer blocked")}.").ConfigureAwait(false);
        }

        [Command("addprefix"), Description("Adds a prefix to this guild's command prefixes."), Aliases("addpfix")]
        public async Task AddPrefixAsync(CommandContext ctx, 
            [RemainingText, Description("Prefix to add to this guild's prefixes.")] string prefix)
        {
            if (this.Bot.Configuration.Discord.DefaultPrefixes.Contains(prefix))
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} Cannot add default prefix.").ConfigureAwait(false);
                return;
            }

            var gid = (long)ctx.Guild.Id;
            var gpfix = this.Database.Prefixes.SingleOrDefault(x => x.GuildId == gid);
            if (gpfix == null)
            {
                gpfix = new DatabasePrefix
                {
                    GuildId = gid,
                    Prefixes = new[] { prefix },
                    EnableDefault = true
                };
                this.Database.Prefixes.Add(gpfix);
            }
            else if (!gpfix.Prefixes.Contains(prefix))
            {
                gpfix.Prefixes = gpfix.Prefixes.Concat(new[] { prefix }).ToArray();
                this.Database.Prefixes.Update(gpfix);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Prefix added.").ConfigureAwait(false);
        }

        [Command("removeprefix"), Description("Removes a prefix from this guild's command prefixes."), Aliases("rmpfix")]
        public async Task RemovePrefixAsync(CommandContext ctx,
            [RemainingText, Description("Prefix to remove from this guild's prefixes.")] string prefix)
        {
            var gid = (long)ctx.Guild.Id;
            var gpfix = this.Database.Prefixes.SingleOrDefault(x => x.GuildId == gid);
            if (gpfix != null && gpfix.Prefixes.Contains(prefix))
            {
                gpfix.Prefixes = gpfix.Prefixes.Concat(new[] { prefix }).ToArray();
                this.Database.Prefixes.Update(gpfix);
            }
            else if (gpfix != null && !gpfix.Prefixes.Contains(prefix))
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} This prefix is not configured.").ConfigureAwait(false);
                return;
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Prefix removed.").ConfigureAwait(false);
        }

        [Command("configuredefaultprefixes"), Description("Configures whether default prefixes are to be enabled in this guild."), Aliases("cfgdefpfx")]
        public async Task ConfigureDefaultPrefixesAsync(CommandContext ctx,
            [RemainingText, Description("Whether default prefixes are to be enabled.")] bool enable)
        {
            var gid = (long)ctx.Guild.Id;
            var gpfix = this.Database.Prefixes.SingleOrDefault(x => x.GuildId == gid);
            if (gpfix == null)
            {
                gpfix = new DatabasePrefix
                {
                    GuildId = gid,
                    Prefixes = new string[] { },
                    EnableDefault = enable
                };
                this.Database.Prefixes.Add(gpfix);
            }
            else
            {
                gpfix.EnableDefault = enable;
                this.Database.Prefixes.Update(gpfix);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Setting saved.").ConfigureAwait(false);
        }
    }
}
