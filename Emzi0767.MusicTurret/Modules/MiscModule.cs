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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Emzi0767.MusicTurret.Attributes;
using Humanizer;
using Microsoft.Extensions.PlatformAbstractions;

namespace Emzi0767.MusicTurret.Modules
{
    [ModuleLifespan(ModuleLifespan.Transient), NotBlacklisted]
    public sealed class MiscModule : BaseCommandModule
    {
        public TurretBot Bot { get; }

        public MiscModule(TurretBot bot)
        {
            this.Bot = bot;
        }

        [Command("about"), Description("Displays information about the bot.")]
        public async Task AboutAsync(CommandContext ctx)
        {
            var ccv = this.Bot.BotVersion;

            var dsv = ctx.Client.VersionString;
            var ncv = PlatformServices.Default
                .Application
                .RuntimeFramework
                .Version
                .ToString(2);

            try
            {
                var a = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(xa => xa.GetName().Name == "System.Private.CoreLib");
                var pth = Path.GetDirectoryName(a.Location);
                pth = Path.Combine(pth, ".version");
                using (var fs = File.OpenRead(pth))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    await sr.ReadLineAsync();
                    ncv = await sr.ReadLineAsync();
                }
            }
            catch { }

            var embed = new DiscordEmbedBuilder
            {
                Title = "About Music Turret",
                Url = "https://emzi0767.com/#!/discord/music-turret",
                Description = $"Music Turret is a bot made by Emzi0767#1837 (<@!181875147148361728>). The source code is available on "
                              + $"{Formatter.MaskedUrl("Emzi's GitHub", new Uri("https://github.com/Emzi0767/Discord-Music-Turret-Bot"), " Music Turret's source code on GitHub")}."
                              + $"\n\nThis shard is currently servicing {ctx.Client.Guilds.Count.ToString("#,##0")} guilds.",
                Color = new DiscordColor(0xFEFEFE)
            };

            embed.AddField("Bot Version", $"{DiscordEmoji.FromName(ctx.Client, ":turret:")} {Formatter.Bold(ccv)}", true)
                .AddField("DSharpPlus Version", $"{DiscordEmoji.FromName(ctx.Client, ":dsharpplus:")} {Formatter.Bold(dsv)}", true)
                .AddField(".NET Core Version", $"{DiscordEmoji.FromName(ctx.Client, ":dotnet:")} {Formatter.Bold(ncv)}", true);

            await ctx.RespondAsync("", embed: embed.Build());
        }

        [Command("uptime"), Description("Display bot's uptime."), Hidden]
        public async Task UptimeAsync(CommandContext ctx)
        {
            var upt = DateTime.Now - Process.GetCurrentProcess().StartTime;
            var ups = upt.Humanize(2);
            await ctx.RespondAsync(string.Concat("\u200b", DiscordEmoji.FromName(ctx.Client, ":turret:"), " The bot has been running for ", Formatter.Bold(ups), "."));
        }

        [Command("ping"), Description("Displays this shard's WebSocket latency."), Hidden]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.RespondAsync(string.Concat("\u200b", DiscordEmoji.FromName(ctx.Client, ":ping_pong:"), " WebSocket latency: ", ctx.Client.Ping.ToString("#,##0"), "ms."));
        }

        [Command("cleanup"), Hidden]
        public async Task CleanupAsync(CommandContext ctx, [Description("Maximum number of messages to clean up.")] int maxCount = 100)
        {
            var lid = 0ul;
            for (var i = 0; i < maxCount; i += 100)
            {
                var msgs = await ctx.Channel.GetMessagesBeforeAsync(lid != 0 ? lid : ctx.Message.Id, Math.Min(maxCount - i, 100));
                var msgsf = msgs.Where(xm => xm.Author.Id == ctx.Client.CurrentUser.Id).OrderBy(xm => xm.Id);

                var lmsg = msgsf.FirstOrDefault();
                if (lmsg == null)
                    break;

                lid = lmsg.Id;

                try
                {
                    await ctx.Channel.DeleteMessagesAsync(msgsf);
                }
                catch (UnauthorizedException)
                {
                    foreach (var xmsg in msgsf)
                        await xmsg.DeleteAsync();
                }
            }

            var msg = await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:").ToString());
            await Task.Delay(2500).ContinueWith(t => msg.DeleteAsync());
        }
    }
}
