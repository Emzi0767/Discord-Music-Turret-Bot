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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Emzi0767.MusicTurret.Attributes;
using Emzi0767.MusicTurret.Data;
using Emzi0767.MusicTurret.Services;

namespace Emzi0767.MusicTurret.Modules
{
    [ModuleLifespan(ModuleLifespan.Transient), NotBlocked]
    public sealed class MusicModule : BaseCommandModule
    {
        private static ImmutableDictionary<int, DiscordEmoji> NumberMappings { get; }
        private static ImmutableDictionary<DiscordEmoji, int> NumberMappingsReverse { get; }

        private MusicService Music { get; }
        private YouTubeSearchProvider YouTube { get; }

        public MusicModule(MusicService music, YouTubeSearchProvider yt)
        {
            this.Music = music;
            this.YouTube = yt;
        }

        static MusicModule()
        {
            var idb = ImmutableDictionary.CreateBuilder<int, DiscordEmoji>();
            idb.Add(0, DiscordEmoji.FromUnicode("1\u20e3"));
            idb.Add(1, DiscordEmoji.FromUnicode("2\u20e3"));
            idb.Add(2, DiscordEmoji.FromUnicode("3\u20e3"));
            idb.Add(3, DiscordEmoji.FromUnicode("4\u20e3"));
            idb.Add(4, DiscordEmoji.FromUnicode("5\u20e3"));
            NumberMappings = idb.ToImmutable();
            var idb2 = ImmutableDictionary.CreateBuilder<DiscordEmoji, int>();
            idb2.AddRange(NumberMappings.ToDictionary(x => x.Value, x => x.Key));
            NumberMappingsReverse = idb2.ToImmutable();
        }

        [Command("play"), Description("Plays supplied URL or searches for specified keywords."), Aliases("p"), Priority(1)]
        public async Task PlayAsync(CommandContext ctx, 
            [Description("URL to play from.")] Uri uri)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);
            
            var tracks = await this.Music.GetTracksAsync(uri).ConfigureAwait(false);
            if (!tracks.Any())
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msfrown:")} No tracks were found at specified link.").ConfigureAwait(false);
                return;
            }

            var trackCount = tracks.Count();
            foreach (var track in tracks)
                gmd.Enqueue(new MusicItem(track, ctx.Member));

            await gmd.CreatePlayerAsync(chn).ConfigureAwait(false);
            gmd.Play();

            if (trackCount > 1)
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Added {trackCount:#,##0} tracks to playback queue.").ConfigureAwait(false);
            else
            {
                var track = tracks.First();
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Added {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))} to the playback queue.").ConfigureAwait(false);
            }
        }

        [Command("play"), Priority(0)]
        public async Task PlayAsync(CommandContext ctx, 
            [RemainingText, Description("Terms to search for.")] string term)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);
            var interactivity = ctx.Client.GetInteractivity();

            var results = await this.YouTube.SearchAsync(term).ConfigureAwait(false);
            if (!results.Any())
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msfrown:")} Nothing was found.").ConfigureAwait(false);
                return;
            }

            var msgC = string.Join("\n", results.Select((x, i) => $"{NumberMappings[i]} {Formatter.Bold(Formatter.Sanitize(x.Title))} by {Formatter.Bold(Formatter.Sanitize(x.Author))}"));
            var msg = await ctx.RespondAsync(msgC).ConfigureAwait(false);
            foreach (var emoji in NumberMappings.Values)
                await msg.CreateReactionAsync(emoji).ConfigureAwait(false);
            var res = await interactivity.WaitForMessageReactionAsync(x => NumberMappingsReverse.ContainsKey(x), msg, ctx.User, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            if (res == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msfrown:")} No choice was made.").ConfigureAwait(false);
                return;
            }

            var elInd = NumberMappingsReverse[res.Emoji];
            var el = results.ElementAt(elInd);
            var url = new Uri($"https://youtu.be/{el.Id}");

            var tracks = await this.Music.GetTracksAsync(url).ConfigureAwait(false);
            if (!tracks.Any())
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msfrown:")} No tracks were found at specified link.").ConfigureAwait(false);
                return;
            }

            var trackCount = tracks.Count();
            foreach (var track in tracks)
                gmd.Enqueue(new MusicItem(track, ctx.Member));

            await gmd.CreatePlayerAsync(chn).ConfigureAwait(false);
            gmd.Play();

            if (trackCount > 1)
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Added {trackCount:#,##0} tracks to playback queue.").ConfigureAwait(false);
            else
            {
                var track = tracks.First();
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Added {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))} to the playback queue.").ConfigureAwait(false);
            }
        }

        [Command("stop"), Description("Stops playback and quits the voice channel.")]
        public async Task StopAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            int rmd = gmd.EmptyQueue();
            gmd.Stop();
            await gmd.DestroyPlayerAsync().ConfigureAwait(false);

            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Removed {rmd:#,##0} tracks from the queue.").ConfigureAwait(false);
        }

        [Command("pause"), Description("Pauses playback.")]
        public async Task PauseAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            gmd.Pause();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Playback paused. Use {Formatter.InlineCode($"{ctx.Prefix}resume")} to resume playback.").ConfigureAwait(false);
        }

        [Command("resume"), Description("Resumes playback."), Aliases("unpause")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            gmd.Resume();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Playback resumed.").ConfigureAwait(false);
        }

        [Command("skip"), Description("Skips current track."), Aliases("next")]
        public async Task SkipAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            var track = gmd.NowPlaying;
            gmd.Stop();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} {Formatter.Bold(Formatter.Sanitize(track.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Track.Author))} skipped.").ConfigureAwait(false);
        }

        [Command("seek"), Description("Seeks to specified time in current track.")]
        public async Task SeekAsync(CommandContext ctx,
            [RemainingText, Description("Which time point to seek to.")] TimeSpan position)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);
            gmd.Seek(position, false);
        }

        [Command("forward"), Description("Forwards the track by specified amount of time.")]
        public async Task ForwardAsync(CommandContext ctx,
            [RemainingText, Description("By how much to forward.")] TimeSpan offset)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);
            gmd.Seek(offset, true);
        }

        [Command("rewind"), Description("Rewinds the track by specified amount of time.")]
        public async Task RewindAsync(CommandContext ctx,
            [RemainingText, Description("By how much to rewind.")] TimeSpan offset)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);
            gmd.Seek(-offset, true);
        }

        [Command("volume"), Description("Sets playback volume."), Aliases("v")]
        public async Task VolumeAsync(CommandContext ctx,
            [Description("Volume to set. Can be 0-150. Default 100.")] int volume = 100)
        {
            if (volume < 0 || volume > 150)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} Volume must be greater than 0, and less than or equal to 150.").ConfigureAwait(false);
                return;
            }

            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            gmd.SetVolume(volume);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Volume set to {volume}%.").ConfigureAwait(false);
        }

        [Command("restart"), Description("Restarts the playback of the current track.")]
        public async Task RestartAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            var track = gmd.NowPlaying;
            gmd.Restart();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} {Formatter.Bold(Formatter.Sanitize(track.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Track.Author))} restarted.").ConfigureAwait(false);
        }

        [Command("repeat"), Description("Changes repeat mode of the queue."), Aliases("loop")]
        public async Task RepeatAsync(CommandContext ctx, 
            [Description("Repeat mode. Can be all, single, or none.")] string mode = null)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            var rmc = new RepeatModeConverter();
            if (!rmc.TryFromString(mode, out var rm))
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} Invalid repeat mode specified.").ConfigureAwait(false);
                return;
            }

            gmd.SetRepeatMode(rm);
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Repeat mode set to {rm}.").ConfigureAwait(false);
        }

        [Command("shuffle"), Description("Toggles shuffle mode.")]
        public async Task ShuffleAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            if (gmd.IsShuffled)
            {
                gmd.StopShuffle();
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Queue is no longer shuffled.").ConfigureAwait(false);
            }
            else
            {
                gmd.Shuffle();
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Queue is now shuffled.").ConfigureAwait(false);
            }
        }

        [Command("reshuffle"), Description("Reshuffles the queue. If queue is not shuffled, it won't enable shuffle mode.")]
        public async Task ReshuffleAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            gmd.Reshuffle();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} Queue reshuffled.").ConfigureAwait(false);
        }

        [Command("remove"), Description("Removes a track from playback queue."), Aliases("del", "rm")]
        public async Task RemoveAsync(CommandContext ctx,
            [Description("Which track to remove.")] int index)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            var itemN = gmd.Remove(index - 1);
            if (itemN == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} No such track.").ConfigureAwait(false);
                return;
            }

            var track = itemN.Value;
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msokhand:")} {Formatter.Bold(Formatter.Sanitize(track.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Track.Author))} removed.").ConfigureAwait(false);
        }

        [Command("queue"), Description("Displays current playback queue."), Aliases("q")]
        public async Task QueueAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);
            var interactivity = ctx.Client.GetInteractivity();

            if (gmd.RepeatMode == RepeatMode.Single)
            {
                var track = gmd.NowPlaying;
                await ctx.RespondAsync($"Queue repeats {Formatter.Bold(Formatter.Sanitize(track.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Track.Author))}.").ConfigureAwait(false);
                return;
            }

            var pageCount = gmd.Queue.Count / 10 + 1;
            if (gmd.Queue.Count % 10 == 0) pageCount--;
            var pages = gmd.Queue.Select(x => x.ToTrackString())
                .Select((s, i) => new { str = s, index = i })
                .GroupBy(x => x.index / 10)
                .Select(xg => new Page { Content = $"Now playing: {gmd.NowPlaying.ToTrackString()}\n\n{string.Join("\n", xg.Select(xa => $"`{xa.index + 1:00}` {xa.str}"))}\n\n{(gmd.RepeatMode == RepeatMode.All ? "The entire queue is repeated.\n\n" : "")}Page {xg.Key + 1}/{pageCount}" });

            if (!pages.Any())
                pages = new List<Page>() { new Page { Content = "Queue is empty!" } };

            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, pages, TimeSpan.FromMinutes(2), TimeoutBehaviour.Ignore);
        }

        [Command("nowplaying"), Description("Displays information about currently-played track."), Aliases("np")]
        public async Task NowPlayingAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            var track = gmd.NowPlaying;
            if (gmd.NowPlaying.Track.TrackString == null)
            {
                await ctx.RespondAsync($"Not playing.").ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Track.Author))} [{gmd.GetCurrentPosition().ToDurationString()}/{gmd.NowPlaying.Track.Length.ToDurationString()}] requested by {Formatter.Bold(Formatter.Sanitize(gmd.NowPlaying.RequestedBy.DisplayName))}.").ConfigureAwait(false);
            }
        }

        [Command("playerinfo"), Description("Displays information about current player."), Aliases("pinfo", "pinf"), Hidden]
        public async Task PlayerInfoAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msraisedhand:")} You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            var gmd = await this.Music.GetOrCreateDataAsync(ctx.Guild).ConfigureAwait(false);

            await ctx.RespondAsync($"Queue length: {gmd.Queue.Count}\nIs shuffled? {(gmd.IsShuffled ? "Yes" : "No")}\nRepeat mode: {gmd.RepeatMode}\nVolume: {gmd.Volume}%").ConfigureAwait(false);
        }
    }
}
