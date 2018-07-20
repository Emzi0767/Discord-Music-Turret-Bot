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
