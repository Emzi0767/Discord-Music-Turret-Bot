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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emzi0767.MusicTurret.Data;
using Emzi0767.MusicTurret.Services;
using Emzi0767.Utilities;
using Npgsql;

namespace Emzi0767.MusicTurret
{
    /// <summary>
    /// Entry point of the bot's binary.
    /// </summary>
    public static class Program
    {
        private static Dictionary<int, TurretBot> Shards { get; set; }

        /// <summary>
        /// Wrapper for asynchronous entry point.
        /// </summary>
        /// <param name="args">Command-line arguments for the binary.</param>
        public static void Main(string[] args)
        {
            // pass the execution to the asynchronous entry point
            var async = new AsyncExecutor();
            async.Execute(MainAsync(args));
        }

        /// <summary>
        /// Asynchronous entry point of the bot's binary.
        /// </summary>
        /// <param name="args">Command-line arguments for the binary.</param>
        /// <returns></returns>
        private static async Task MainAsync(string[] args)
        {
            // locate the config file
            var dockerSecret = Environment.GetEnvironmentVariable("DOCKER_SECRET");
            var cfgFile = new FileInfo(dockerSecret != null ? Path.Combine("/run/secrets", dockerSecret) : "config.json");

            // load the config file and validate it
            var cfgLoader = new TurretConfigLoader();
            var cfg = await cfgLoader.LoadConfigurationAsync(cfgFile);
            cfgLoader.ValidateConfiguration(cfg);

            // create database type mapping
            NpgsqlConnection.GlobalTypeMapper.MapEnum<DatabaseEntityKind>("entity_kind");

            // validate database
            var dbcsp = new ConnectionStringProvider(cfg.PostgreSQL);
            using (var db = new DatabaseContext(dbcsp))
            {
                var dbv = db.Metadata.SingleOrDefault(x => x.MetaKey == "schema_version");
                if (dbv == null || dbv.MetaValue != "2")
                    throw new InvalidDataException("Database schema version mismatch.");
                dbv = db.Metadata.SingleOrDefault(x => x.MetaKey == "project");
                if (dbv == null || dbv.MetaValue != "Music Turret")
                    throw new InvalidDataException("Database schema type mismatch.");
            }

            // create shards
            Shards = new Dictionary<int, TurretBot>();
            var async = new AsyncExecutor();
            for (int i = 0; i < cfg.Discord.ShardCount; i++)
            {
                var shard = new TurretBot(cfg, i, async);
                await shard.StartAsync();

                Shards[i] = shard;
            }

            // wait forever
            await Task.Delay(-1);
        }
    }
}
