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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emzi0767.MusicTurret.Data
{
    /// <summary>
    /// Represents command prefix configuration for various guilds.
    /// </summary>
    [Table("prefixes")]
    public partial class DatabasePrefix
    {
        /// <summary>
        /// Gets or sets the guild ID for these prefixes.
        /// </summary>
        [Key]
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the prefixes in use for this guild.
        /// </summary>
        [Required]
        [Column("prefixes")]
        public string[] Prefixes { get; set; }

        /// <summary>
        /// Gets or sets whether the default prefixes should remain active in the guild.
        /// </summary>
        [Required]
        [Column("enable_default")]
        public bool EnableDefault { get; set; }
    }
}
