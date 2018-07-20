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
using System.Reflection;
using System.Text;
using DSharpPlus;
using Emzi0767.MusicTurret.Data;
using Npgsql;

namespace Emzi0767.MusicTurret
{
    /// <summary>
    /// Helper class containing various static helper methods and properties, as well as extension methods.
    /// </summary>
    public static class TurretUtilities
    {
        /// <summary>
        /// Gets the properly-configured UTF8 encoder.
        /// </summary>
        public static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

        /// <summary>
        /// Converts this instance of PostgreSQL configuration section into a PostgreSQL connection string.
        /// </summary>
        /// <param name="config">Configuration section to convert.</param>
        /// <returns>PostgreSQL connection string.</returns>
        public static string ToPostgresConnectionString(this TurretConfigPostgres config)
        {
            // check if config is null
            if (config == null)
                throw new NullReferenceException();

            // build the connection string out of supplied parameters
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = config.Hostname,
                Port = config.Port,

                Database = config.Database,
                Username = config.Username,
                Password = config.Password,

                SslMode = config.UseEncryption ? SslMode.Require : SslMode.Disable,
                TrustServerCertificate = config.TrustServerCertificate
            };
            return csb.ConnectionString;
        }

        /// <summary>
        /// Converts the string to a fixed-width string.
        /// </summary>
        /// <param name="s">String to fix the width of.</param>
        /// <param name="targetLength">Length that the string should be.</param>
        /// <returns>Adjusted string.</returns>
        public static string ToFixedWidth(this string s, int targetLength)
        {
            if (s == null)
                throw new NullReferenceException();

            if (s.Length < targetLength)
                return s.PadRight(targetLength, ' ');

            if (s.Length > targetLength)
                return s.Substring(0, targetLength);

            return s;
        }

        /// <summary>
        /// Gets the version of the bot's assembly.
        /// </summary>
        /// <returns>Bot version.</returns>
        public static string GetBotVersion()
        {
            var a = Assembly.GetExecutingAssembly();
            var av = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return av.InformationalVersion;
        }

        /// <summary>
        /// Converts given <see cref="TimeSpan"/> to a duration string.
        /// </summary>
        /// <param name="ts">Time span to convert.</param>
        /// <returns>Duration string.</returns>
        public static string ToDurationString(this TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return ts.ToString(@"h\:mm\:ss");
            return ts.ToString(@"m\:ss");
        }

        /// <summary>
        /// Converts given <see cref="MusicItem"/> to a track string.
        /// </summary>
        /// <param name="mi">Music item to convert.</param>
        /// <returns>Track string.</returns>
        public static string ToTrackString(this MusicItem x)
        {
            return $"{Formatter.Bold(Formatter.Sanitize(x.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(x.Track.Author))} [{x.Track.Length.ToDurationString()}] (added by {x.RequestedBy.DisplayName})";
        }
    }
}
