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
