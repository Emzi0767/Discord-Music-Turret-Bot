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
