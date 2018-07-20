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
