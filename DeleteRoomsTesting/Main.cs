using System;
using DonatorPlugin.Commands;
using LabApi.Loader.Features.Plugins;

namespace DonatorPlugin
{
    public class Main : Plugin<Config>
    {
        public override string Name => "Donator plugin";

        public override string Description => "Donator plugin";

        public override string Author => "davilone32";

        public override Version Version => new Version(1, 0, 0);

        public override Version RequiredApiVersion => new Version(1, 0, 0);

        public override void Enable()
        {
            LabApi.Events.Handlers.ServerEvents.RoundStarted += restartround;
        }

        public override void Disable()
        {
            LabApi.Events.Handlers.ServerEvents.RoundStarted -= restartround;
        }

        public void restartround()
        {
            CreateVotation.ResetEstado();
        }
    }
}
