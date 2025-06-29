using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;

namespace DonatorPlugin.Commands
{
    [CommandHandler(typeof(Parent))]
    public class VotoNo : ICommand
    {
        public string Command => "no";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Vota no en la votación activa.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            return CreateVotation.Votar("no", sender, out response);
        }
    }
}
