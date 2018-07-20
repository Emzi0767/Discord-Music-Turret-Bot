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
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Emzi0767.MusicTurret
{
    /// <summary>
    /// Argument converter for <see cref="Uri"/> type.
    /// </summary>
    public sealed class UriConverter : IArgumentConverter<Uri>
    {
        public Task<Optional<Uri>> ConvertAsync(string value, CommandContext ctx)
        {
            try
            {
                if (value.StartsWith('<') && value.EndsWith('>'))
                    value = value.Substring(1, value.Length - 2);

                return Task.FromResult(Optional<Uri>.FromValue(new Uri(value)));
            }
            catch
            {
                return Task.FromResult(Optional<Uri>.FromNoValue());
            }
        }
    }
}
