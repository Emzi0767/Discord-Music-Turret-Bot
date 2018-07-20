// This file is a part of Music Turret project.
// 
// Copyright (C) 2018-2021 Emzi0767
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using Emzi0767.MusicTurret.Attributes;
using Emzi0767.MusicTurret.Data;
using Emzi0767.MusicTurret.Services;
using Emzi0767.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Emzi0767.MusicTurret
{
    /// <summary>
    /// Represents a single shard of the Music Turret bot.
    /// </summary>
    public sealed class TurretBot
    {
        /// <summary>
        /// Gets the tag used when emitting log events from the bot.
        /// </summary>
        public static EventId LogEvent { get; } = new EventId(1000, "MTurret");

        /// <summary>
        /// Gets the discord client instance for this bot shard.
        /// </summary>
        public DiscordClient Discord { get; }

        /// <summary>
        /// Gets the CommandsNext extension instance.
        /// </summary>
        public CommandsNextExtension CommandsNext { get; }

        /// <summary>
        /// Gets the Interactivity extension instance.
        /// </summary>
        public InteractivityExtension Interactivity { get; }

        /// <summary>
        /// Gets the Lavalink instance.
        /// </summary>
        public LavalinkExtension Lavalink { get; }

        /// <summary>
        /// Gets the ID of this shard.
        /// </summary>
        public int ShardId { get; }

        /// <summary>
        /// Gets the version of the bot.
        /// </summary>
        public string BotVersion { get; }

        /// <summary>
        /// Gets the configuration of this bot.
        /// </summary>
        public TurretConfig Configuration { get; }

        private ConnectionStringProvider ConnectionStringProvider { get; }
        private AsyncExecutor AsyncExecutor { get; }
        private Timer GameTimer { get; set; } = null;
        private IServiceProvider Services { get; }

        private readonly object _logLock = new object();

        /// <summary>
        /// Creates a new instance of Music Turret bot shard handler.
        /// </summary>
        /// <param name="cfg">Configuration options for the shard.</param>
        /// <param name="shardId">ID of this shard.</param>
        /// <param name="async">Synchronous executor of asynchronous tasks.</param>
        public TurretBot(TurretConfig cfg, int shardId, AsyncExecutor async)
        {
            // assign the properties
            this.ShardId = shardId;
            this.BotVersion = TurretUtilities.GetBotVersion();
            this.Configuration = cfg;
            this.ConnectionStringProvider = new ConnectionStringProvider(cfg.PostgreSQL);
            this.AsyncExecutor = async;

            // create discord client instance
            this.Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfg.Discord.Token,
                TokenType = TokenType.Bot,
                ShardCount = cfg.Discord.ShardCount,
                ShardId = this.ShardId,
                Intents = DiscordIntents.All,

                AutoReconnect = true,
                ReconnectIndefinitely = true,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                LargeThreshold = 250
            });

            // attach event handlers
            this.Discord.Ready += this.Discord_Ready;
            this.Discord.GuildDownloadCompleted += this.Discord_GuildDownloadCompleted;
            this.Discord.SocketErrored += this.Discord_SocketErrored;
            this.Discord.GuildAvailable += this.Discord_GuildAvailable;
            this.Discord.VoiceStateUpdated += this.Discord_VoiceStateUpdated;

            // create service provider
            this.Services = new ServiceCollection()
                .AddTransient<SecureRandom>()
                .AddSingleton(this.ConnectionStringProvider)
                .AddSingleton<MusicService>()
                .AddScoped<DatabaseContext>()
                .AddSingleton(new RedisClient(cfg.Redis))
                .AddSingleton(new LavalinkService(cfg.Lavalink, this.Discord))
                .AddSingleton(new YouTubeSearchProvider(cfg.YouTube))
                .AddSingleton(this)
                .BuildServiceProvider(true);

            // create CommandsNext
            this.CommandsNext = this.Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDms = false,
                IgnoreExtraArguments = false,

                EnableDefaultHelp = true,
                DefaultHelpChecks = new[] { new NotBlacklistedAttribute() },

                EnableMentionPrefix = cfg.Discord.EnableMentionPrefix,
                PrefixResolver = this.ResolvePrefixAsync,
                
                Services = this.Services
            });

            // set help formatter
            this.CommandsNext.SetHelpFormatter<TurretHelpFormatter>();

            // attach event handlers
            this.CommandsNext.CommandExecuted += this.CommandsNext_CommandExecuted;
            this.CommandsNext.CommandErrored += this.CommandsNext_CommandErrored;

            // create commands
            this.CommandsNext.RegisterCommands(Assembly.GetExecutingAssembly());

            // create interactivity
            this.Interactivity = this.Discord.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(30)
            });

            // create lavalink
            this.Lavalink = this.Discord.UseLavalink();
        }

        /// <summary>
        /// Signals the Discord client to connect to API and starts the bot.
        /// </summary>
        /// <returns></returns>
        public Task StartAsync()
        {
            this.Discord.Logger.LogInformation(LogEvent, "Booting Turret shard.");
            return this.Discord.ConnectAsync();
        }

        private Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(LogEvent, "Client is ready to process events");

            if (this.GameTimer == null && !string.IsNullOrWhiteSpace(this.Configuration.Discord.Game))
                this.GameTimer = new Timer(this.GameTimerCallback, sender, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private Task Discord_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            sender.Logger.LogInformation(LogEvent, "All guilds are now available");
            return Task.CompletedTask;
        }

        private Task Discord_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            sender.Logger.LogCritical(LogEvent, $"Socket threw an exception {ex.GetType()}: {ex.Message}");
            return Task.CompletedTask;
        }

        private Task Discord_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation(LogEvent, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private async Task Discord_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            var music = this.Services.GetService<MusicService>();
            var gmd = await music.GetOrCreateDataAsync(e.Guild);
            if (e.After.Channel == null && e.User == this.Discord.CurrentUser)
            {
                await gmd.StopAsync();
                await gmd.DestroyPlayerAsync();
                return;
            }

            if (e.User == this.Discord.CurrentUser)
                return;

            var chn = gmd.Channel;
            if (chn == null || chn != e.Before.Channel)
                return;

            var usrs = chn.Users;
            if (gmd.IsPlaying && !usrs.Any(x => !x.IsBot))
            {
                sender.Logger.LogInformation(LogEvent, $"All users left voice in {e.Guild.Name}, pausing playback");
                await gmd.PauseAsync();
                await gmd.SaveAsync();
                
                if (gmd.CommandChannel != null)
                    await gmd.CommandChannel.SendMessageAsync($"{DiscordEmoji.FromName(sender, ":play_pause:")} All users left the channel, playback paused. You can resume it by joining the channel and using the `resume` command.");
            }
        }

        private Task CommandsNext_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(LogEvent,
                $"User '{e.Context.User.Username}#{e.Context.User.Discriminator}' ({e.Context.User.Id}) executed '{e.Command.QualifiedName}' in #{e.Context.Channel.Name} ({e.Context.Channel.Id})");
            return Task.CompletedTask;
        }

        private async Task CommandsNext_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(LogEvent,
                $"User '{e.Context.User.Username}#{e.Context.User.Discriminator}' ({e.Context.User.Id}) tried to execute '{e.Command?.QualifiedName ?? "<unknown command>"}' "
                + $"in #{e.Context.Channel.Name} ({e.Context.Channel.Id}) and failed with {e.Exception.GetType()}: {e.Exception.Message}");
            DiscordEmbedBuilder embed = null;

            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            if (ex is CommandNotFoundException)
            { } // ignore
            else if (ex is CommandCancelledException)
            { } // ignore
            else if (ex is ChecksFailedException cfe)
            {
                if (!cfe.FailedChecks.OfType<NotBlacklistedAttribute>().Any())
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Permission denied",
                        Description = $"{DiscordEmoji.FromName(e.Context.Client, ":msraisedhand:")} You lack permissions necessary to run this command.",
                        Color = new DiscordColor(0xFF0000)
                    };
            }
            else
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "A problem occured while executing the command",
                    Description = $"{Formatter.InlineCode(e.Command.QualifiedName)} threw an exception: `{ex.GetType()}: {ex.Message}`",
                    Color = new DiscordColor(0xFF0000)
                };
            }

            if (embed != null)
                await e.Context.RespondAsync("", embed: embed.Build());
        }

        private Task<int> ResolvePrefixAsync(DiscordMessage msg)
        {
            var gld = msg.Channel.Guild;
            if (gld == null)
                return Task.FromResult(-1);

            using var db = new DatabaseContext(this.ConnectionStringProvider);

            var gldId = (long)gld.Id;
            var gpfix = db.Prefixes.SingleOrDefault(x => x.GuildId == gldId);
            if (gpfix != null)
            {
                foreach (var pfix in gpfix.Prefixes)
                {
                    var pfixLocation = msg.GetStringPrefixLength(pfix);
                    if (pfixLocation != -1)
                        return Task.FromResult(pfixLocation);
                }

                if (!gpfix.EnableDefault)
                    return Task.FromResult(-1);
            }

            foreach (var pfix in this.Configuration.Discord.DefaultPrefixes)
            {
                var pfixLocation = msg.GetStringPrefixLength(pfix);
                if (pfixLocation != -1)
                    return Task.FromResult(pfixLocation);
            }

            return Task.FromResult(-1);
        }

        private void GameTimerCallback(object _)
        {
            var client = _ as DiscordClient;
            try
            {
                this.AsyncExecutor.Execute(client.UpdateStatusAsync(new DiscordActivity(this.Configuration.Discord.Game), UserStatus.Online, null));
                client.Logger.LogInformation(LogEvent, "Presence updated");
            }
            catch (Exception ex)
            {
                client.Logger.LogError(LogEvent, $"Could not update presence ({ex.GetType()}: {ex.Message})");
            }
        }
    }
}
