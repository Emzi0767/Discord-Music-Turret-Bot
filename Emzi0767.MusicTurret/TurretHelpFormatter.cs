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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;

namespace Emzi0767.MusicTurret
{
    /// <summary>
    /// Formats the help for users.
    /// </summary>
    public sealed class TurretHelpFormatter : BaseHelpFormatter
    {
        private TurretBot Bot { get; }
        private StringBuilder Message { get; }

        private bool _hasCommand = false;

        /// <summary>
        /// Creates a new help formatter instance.
        /// </summary>
        /// <param name="ctx">Context in which the formatter is invoked.</param>
        /// <param name="bot">Turret bot instance.</param>
        public TurretHelpFormatter(CommandContext ctx, TurretBot bot)
            : base(ctx)
        {
            this.Bot = bot;
            this.Message = new StringBuilder();

            this.Message.AppendLine("```less")
                .AppendLine($"Music Turret v{bot.BotVersion} by Emzi0767")
                .AppendLine();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this._hasCommand = true;

            this.Message.AppendLine(command.QualifiedName)
                .AppendLine(command.Description);

            if (command.Aliases?.Any() == true)
                this.Message.AppendLine($"Aliases: {string.Join(" ", command.Aliases)}");

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (this._hasCommand)
                this.Message.AppendLine()
                    .AppendLine("Available subcommands:");
            else
                this.Message.AppendLine("Available commands:");

            var maxLen = subcommands.Max(x => x.Name.Length) + 2;
            foreach (var cmd in subcommands)
                this.Message.AppendLine($"{cmd.Name.ToFixedWidth(maxLen)}    {cmd.Description}");

            return this;
        }

        public override CommandHelpMessage Build()
        {
            this.Message.Append("```");
            return new CommandHelpMessage(this.Message.ToString());
        }
    }
}
