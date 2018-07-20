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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Emzi0767.MusicTurret.Data;

namespace Emzi0767.MusicTurret.Services
{
    /// <summary>
    /// Provides a persistent way of tracking music in various guilds.
    /// </summary>
    public sealed class MusicService
    {
        private LavalinkService Lavalink { get; }
        private RedisClient Redis { get; }
        private CSPRNG RNG { get; }
        private ConcurrentDictionary<ulong, GuildMusicData> MusicData { get; }
        private string PlayingQueues { get; set; }

        /// <summary>
        /// Creates a new instance of this music service.
        /// </summary>
        /// <param name="redis">Redis client to use for persistence.</param>
        /// <param name="rng">Cryptographically-secure random number generator implementaion.</param>
        public MusicService(RedisClient redis, CSPRNG rng, LavalinkService lavalink)
        {
            this.Lavalink = lavalink;
            this.Redis = redis;
            this.RNG = rng;
            this.MusicData = new ConcurrentDictionary<ulong, GuildMusicData>();
        }

        /// <summary>
        /// Saves data for specified guild.
        /// </summary>
        /// <param name="guild">Guild to save data for.</param>
        /// <returns></returns>
        public Task SaveDataForAsync(DiscordGuild guild)
        {
            if (this.MusicData.TryGetValue(guild.Id, out var gmd))
                return gmd.SaveAsync();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets or creates a dataset for specified guild.
        /// </summary>
        /// <param name="guild">Guild to get or create dataset for.</param>
        /// <returns>Resulting dataset.</returns>
        public async Task<GuildMusicData> GetOrCreateDataAsync(DiscordGuild guild)
        {
            if (this.MusicData.TryGetValue(guild.Id, out var gmd))
                return gmd;

            gmd = this.MusicData.AddOrUpdate(guild.Id, new GuildMusicData(guild, this.RNG, this.Lavalink, this.Redis), (k, v) => v);
            await gmd.LoadAsync().ConfigureAwait(false);

            return gmd;
        }

        /// <summary>
        /// Loads tracks from specified URL.
        /// </summary>
        /// <param name="uri">URL to load tracks from.</param>
        /// <returns>Loaded tracks.</returns>
        public Task<IEnumerable<LavalinkTrack>> GetTracksAsync(Uri uri)
            => this.Lavalink.LavalinkNode.GetTracksAsync(uri);
    }
}
