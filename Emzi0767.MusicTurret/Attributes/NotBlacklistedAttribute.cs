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

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Emzi0767.MusicTurret.Data;
using Emzi0767.MusicTurret.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Emzi0767.MusicTurret.Attributes
{
    /// <summary>
    /// Verifies that the user is not blacklisted for the purpose of the command usage.
    /// </summary>
    public sealed class NotBlacklistedAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
                return Task.FromResult(false);

            if (help)
                return Task.FromResult(true);

            if (ctx.Client.CurrentApplication.Owners.Contains(ctx.User))
                return Task.FromResult(true);

            var uid = (long)ctx.User.Id;
            var cid = (long)ctx.Channel.Id;
            var gid = (long)ctx.Guild.Id;

            var db = ctx.Services.GetService<DatabaseContext>();
            var blocked = db.EntityBlacklist.Any(x => (x.Id == uid && x.Kind == DatabaseEntityKind.User) || (x.Id == cid && x.Kind == DatabaseEntityKind.Channel) || (x.Id == gid && x.Kind == DatabaseEntityKind.Guild));
            return Task.FromResult(!blocked);
        }
    }
}
