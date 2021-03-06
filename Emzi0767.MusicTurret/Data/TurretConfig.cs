﻿// This file is a part of Music Turret project.
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

using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Emzi0767.MusicTurret.Data
{
    /// <summary>
    /// Represents the entire configuration file.
    /// </summary>
    public sealed class TurretConfig
    {
        /// <summary>
        /// Gets this configuration file's version data.
        /// </summary>
        [JsonProperty("version")]
        public TurretConfigVersion Version { get; private set; } = new TurretConfigVersion();

        /// <summary>
        /// Gets the Discord configuration.
        /// </summary>
        [JsonProperty("discord")]
        public TurretConfigDiscord Discord { get; private set; } = new TurretConfigDiscord();

        /// <summary>
        /// Gets the PostgreSQL configuration.
        /// </summary>
        [JsonProperty("postgres")]
        public TurretConfigPostgres PostgreSQL { get; private set; } = new TurretConfigPostgres();

        /// <summary>
        /// Gets the Redis configuration.
        /// </summary>
        [JsonProperty("redis")]
        public TurretConfigRedis Redis { get; private set; } = new TurretConfigRedis();

        /// <summary>
        /// Gets the Lavalink configuration.
        /// </summary>
        [JsonProperty("lavalink")]
        public TurretConfigLavalink Lavalink { get; private set; } = new TurretConfigLavalink();

        /// <summary>
        /// Gets the YouTube API configuration.
        /// </summary>
        [JsonProperty("youtube")]
        public TurretConfigYouTube YouTube { get; private set; } = new TurretConfigYouTube();
    }

    /// <summary>
    /// Represents version section of the configuration file.
    /// </summary>
    public sealed class TurretConfigVersion
    {
        /// <summary>
        /// Gets the numeric version of the config data.
        /// </summary>
        [JsonProperty("config")]
        public int Configuration { get; private set; }
    }

    /// <summary>
    /// Represents Discord section of the configuration file.
    /// </summary>
    public sealed class TurretConfigDiscord
    {
        /// <summary>
        /// Gets the token for Discord API.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; private set; } = "insert_token_here";

        /// <summary>
        /// Gets the default command prefixes of the bot.
        /// </summary>
        [JsonProperty("prefixes")]
        public ImmutableArray<string> DefaultPrefixes { get; private set; } = new[] { "t!", ">>", ".." }.ToImmutableArray();

        /// <summary>
        /// Gets whether to enable the user mention prefix for the bot.
        /// </summary>
        [JsonProperty("mention_prefix")]
        public bool EnableMentionPrefix { get; private set; } = true;

        /// <summary>
        /// Gets the size of the message cache. 0 means disable.
        /// </summary>
        [JsonProperty("message_cache_size")]
        public int MessageCacheSize { get; private set; } = 512;

        /// <summary>
        /// Gets the total number of shards on which the bot will operate.
        /// </summary>
        [JsonProperty("shards")]
        public int ShardCount { get; private set; } = 1;

        /// <summary>
        /// Gets the game the bot will be playing. Null means disable.
        /// </summary>
        [JsonProperty("game")]
        public string Game { get; private set; } = "lots of music";
    }

    /// <summary>
    /// Represents PostgreSQL section of the configuration file.
    /// </summary>
    public sealed class TurretConfigPostgres
    {
        /// <summary>
        /// Gets the hostname of the PostgreSQL server.
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; private set; } = "localhost";

        /// <summary>
        /// Gets the port of the PostgreSQL server.
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; private set; } = 5432;

        /// <summary>
        /// Gets the name of the PostgreSQL database.
        /// </summary>
        [JsonProperty("database")]
        public string Database { get; private set; } = "discord_turret";

        /// <summary>
        /// Gets the username for PostgreSQL authentication.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; private set; } = "discord_turret";

        /// <summary>
        /// Gets the password for PostgreSQL authentication.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; private set; } = "hunter2";

        /// <summary>
        /// Gets whether to use SSL/TLS for communication with PostgreSQL server.
        /// </summary>
        [JsonProperty("ssl")]
        public bool UseEncryption { get; private set; } = true;

        /// <summary>
        /// Gets whether to trust the PostgreSQL server certificate unconditionally.
        /// </summary>
        [JsonProperty("trust_certificate")]
        public bool TrustServerCertificate { get; private set; } = true;

        /// <summary>
        /// Gets the required database schema version.
        /// </summary>
        [JsonProperty("schema_version")]
        public int SchemaVersion { get; private set; } = 1;
    }

    /// <summary>
    /// Represents Redis section of the configuration file.
    /// </summary>
    public sealed class TurretConfigRedis
    {
        /// <summary>
        /// Gets the hostname of the Redis server.
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; private set; } = "localhost";

        /// <summary>
        /// Gets the port used to communicate with Redis server.
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; private set; } = 6379;

        /// <summary>
        /// Gets the password used to authenticate with Redis server.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; private set; } = null;

        /// <summary>
        /// Gets whether to use SSL/TLS for communication with Redis server.
        /// </summary>
        [JsonProperty("ssl")]
        public bool UseEncryption { get; private set; } = true;
    }

    /// <summary>
    /// Represents Lavalink section of the configuration file.
    /// </summary>
    public sealed class TurretConfigLavalink
    {
        /// <summary>
        /// Gets the hostname of the Lavalink node server.
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; private set; } = "localhost";

        /// <summary>
        /// Gets the port of the Lavalink node.
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; private set; } = 2333;

        /// <summary>
        /// Gets the password to Lavalink node.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; private set; } = "youshallnotpass";
    }

    /// <summary>
    /// Represents YouTube section of the configuration file.
    /// </summary>
    public sealed class TurretConfigYouTube
    {
        /// <summary>
        /// Gets the API key for YouTube's data API.
        /// </summary>
        [JsonProperty("api_key")]
        public string ApiKey { get; private set; } = "insert_api_key_here";
    }
}
