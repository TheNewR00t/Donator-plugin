
using System.Collections.Generic;
using System;
using CommandSystem;

namespace DonatorPlugin.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Parent : ParentCommand
    {
        public override string Command { get; } = "eventos";
        public override string[] Aliases { get; } = { "dt", "dts" };
        public override string Description { get; } = string.Empty;
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new CreateVotation());
            RegisterCommand(new VotoYes());
            RegisterCommand(new VotoNo());
        }
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Error pon cr <evento>, yes o no";
            return false;
        }
    }
}
