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

using Emzi0767.MusicTurret.Data;
using Microsoft.EntityFrameworkCore;

namespace Emzi0767.MusicTurret.Services
{
    /// <summary>
    /// Connection context service for Turret's database.
    /// </summary>
    public partial class DatabaseContext : DbContext
    {
        /// <summary>
        /// Gets or sets metadata for this database.
        /// </summary>
        public virtual DbSet<DatabaseMetadata> Metadata { get; set; }

        /// <summary>
        /// Gets or sets configured per-guild prefixes.
        /// </summary>
        public virtual DbSet<DatabasePrefix> Prefixes { get; set; }

        /// <summary>
        /// Gets or sets entities that are blacklisted from using the bot. 
        /// </summary>
        public virtual DbSet<DatabaseBlacklistedEntity> EntityBlacklist { get; set; }

        private ConnectionStringProvider ConnectionStringProvider { get; }

        /// <summary>
        /// Creates a new database context with specified connection string provider.
        /// </summary>
        /// <param name="csp">Connection string provider to use when connecting to PostgreSQL.</param>
        public DatabaseContext(ConnectionStringProvider csp)
        {
            this.ConnectionStringProvider = csp;
        }

        /// <summary>
        /// Creates a new database context with specified context options and connection string provider.
        /// </summary>
        /// <param name="options">Database context options.</param>
        /// <param name="csp">Connection string provider to use when connecting to PostgreSQL.</param>
        public DatabaseContext(DbContextOptions<DatabaseContext> options, ConnectionStringProvider csp)
            : base(options)
        {
            this.ConnectionStringProvider = csp;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(this.ConnectionStringProvider.GetConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<DatabaseEntityKind>();

            modelBuilder.Entity<DatabaseMetadata>(entity =>
            {
                entity.Property(e => e.MetaKey).ValueGeneratedNever();
            });

            modelBuilder.Entity<DatabasePrefix>(entity =>
            {
                entity.Property(e => e.GuildId).ValueGeneratedNever();
            });

            modelBuilder.Entity<DatabaseBlacklistedEntity>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Kind });
            });
        }
    }
}
