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
    /// Represents a table metadata property.
    /// </summary>
    [Table("metadata")]
    public partial class DatabaseMetadata
    {
        /// <summary>
        /// Gets or sets the name of the metadata property.
        /// </summary>
        [Key]
        [Column("meta_key")]
        public string MetaKey { get; set; }

        /// <summary>
        /// Gets or sets the value of the metadata property.
        /// </summary>
        [Required]
        [Column("meta_value")]
        public string MetaValue { get; set; }
    }
}
