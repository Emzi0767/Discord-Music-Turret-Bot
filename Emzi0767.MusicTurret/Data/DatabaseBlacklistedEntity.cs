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
using System.ComponentModel.DataAnnotations.Schema;

namespace Emzi0767.MusicTurret.Data
{
    /// <summary>
    /// Represents an entity blacklisted from using the bot.
    /// </summary>
    [Table("entity_blacklist")]
    public partial class DatabaseBlacklistedEntity
    {
        /// <summary>
        /// Gets or sets the entity's ID.
        /// </summary>
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the entity's kind.
        /// </summary>
        [Column("kind")]
        public DatabaseEntityKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the reason why the entity was blacklisted.
        /// </summary>
        [Column("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets when the entity was blacklisted.
        /// </summary>
        [Column("since", TypeName = "timestamp with time zone")]
        public DateTime Since { get; set; }
    }
}
