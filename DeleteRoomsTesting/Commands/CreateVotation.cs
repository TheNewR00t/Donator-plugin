using System;
using System.Collections.Generic;
using System.Timers;
using CommandSystem;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Console;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using Round = LabApi.Features.Wrappers.Round;
using Server = LabApi.Features.Wrappers.Server;

namespace DonatorPlugin.Commands
{
    [CommandHandler(typeof(Parent))]
    public class CreateVotation : ICommand
    {
        public string Command => "cr";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Proponer un evento para votación (.evento anarquia)";

        public static bool VotacionEncendida { get; set; } = false;
        public static bool EventoActivo { get; set; } = false;

        public Timer votacionTimer;
        public static string eventoPropuesto = "";

        public static readonly Dictionary<string, int> votos = new()
        {
            { "yes", 0 },
            { "no", 0 }
        };

        public static readonly HashSet<string> jugadoresQueVotaron = new();

        private readonly HashSet<string> eventosValidos = new()
        {
            "anarquia",
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (VotacionEncendida)
                {
                    response = "❌ Ya hay una votación en curso.";
                    return false;
                }

                if (EventoActivo)
                {
                    response = "❌ Ya hay un evento activo. Espera a que finalice antes de proponer otro.";
                    return false;
                }

                if (!sender.HasPermissions("DonadorTier1.CreateVotation"))
                {
                    response = "<color=red>Permiso denegado. Solo donadores pueden usar este comando.</color>";
                    return false;
                }

                if (Round.IsRoundStarted)
                {
                    response = "<color=red>❌ Solo puedes iniciar la votación en la PRE-RONDA.</color>";
                    return false;
                }

                if (arguments.Count == 0)
                {
                    response = "Debes escribir el nombre del evento que quieres proponer.";
                    return false;
                }

                string propuesto = arguments.At(0).ToLower();

                if (!eventosValidos.Contains(propuesto))
                {
                    response = $"❌ Evento inválido. Eventos disponibles: {string.Join(", ", eventosValidos)}";
                    return false;
                }

                eventoPropuesto = propuesto;
                votos["yes"] = 0;
                votos["no"] = 0;
                jugadoresQueVotaron.Clear();
                VotacionEncendida = true;
                Round.IsLobbyLocked = true;

                foreach (Player player in Player.List)
                {
                    player.SendBroadcast(duration: 10, message: $"📢 Un donador quiere poner el evento: <b>{eventoPropuesto.ToUpper()}</b>!\nVota con <color=green>.yes</color> o <color=red>.no</color>");
                }

                response = $"🗳️ Se ha propuesto el evento '{eventoPropuesto.ToUpper()}'. Tienes 12 segundos para votar.";

                votacionTimer = new Timer(12000);
                votacionTimer.Elapsed += FinalizarVotacion;
                votacionTimer.AutoReset = false;
                votacionTimer.Start();
                Round.IsLocked = true;

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                response = "⚠️ Error al ejecutar el comando.";
                return false;
            }
        }

        public void FinalizarVotacion(object sender, ElapsedEventArgs e)
        {
            VotacionEncendida = false;
            votacionTimer.Dispose();

            int votosYes = votos["yes"];
            int votosNo = votos["no"];
            string resultado = votosYes > votosNo ? "YES" : "NO";

            foreach (Player player in Player.List)
            {
                player.SendBroadcast(duration: 10, message: $"<color=#FF6347>📊 Votación finalizada para '{eventoPropuesto.ToUpper()}'</color>\n<color=#32CD32>✅ YES: {votosYes}</color>  <color=#FF4500>❌ NO: {votosNo}</color>\n<color=#1E90FF>Resultado: {resultado}</color>");
            }

            
            Round.IsLobbyLocked = false;

            if (resultado == "YES")
            {
                EventoActivo = true;
                EjecutarEventoGanador(eventoPropuesto);
            }
        }

        public void EjecutarEventoGanador(string evento)
        {
            switch (evento)
            {
                case "anarquia":

                    foreach (Player player in Player.List)
                    {
                        player.SendBroadcast(duration: 10, message: $"🔥 ¡Evento Anarquía activado! Friendly Fire habilitado.");
                    }

                    Server.FriendlyFire = true;
                    Round.IsLocked = false;
                    break;

                case "escondite":

                    foreach (Player player in Player.List)
                    {
                        player.SendBroadcast(duration: 10, message: $"🕵️ ¡Modo Escondite activado! Encuentra un buen lugar para ocultarte.");
                    }

                    Round.IsLobbyLocked = true;
                    break;

                case "peanutrun":

                    foreach (Player player in Player.List)
                    {
                        player.SendBroadcast(duration: 10, message: $"¡Peanut Run activado! Corre por tu vida.");
                    }

                    Round.IsLobbyLocked = true;
                    break;

                default:

                    foreach (Player player in Player.List)
                    {
                        player.SendBroadcast(duration: 10, message: $"❌ Error: Evento no reconocido.");
                    }

                    break;
            }
        }

        public static bool Votar(string voto, ICommandSender sender, out string response)
        {
            if (!VotacionEncendida)
            {
                response = "❌ No hay ninguna votación activa.";
                return false;
            }

            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Este comando solo puede ser usado por jugadores.";
                return false;
            }

            string playerId = playerSender.PlayerId.ToString();

            if (jugadoresQueVotaron.Contains(playerId))
            {
                response = "❌ Ya has votado.";
                return false;
            }

            votos[voto]++;
            jugadoresQueVotaron.Add(playerId);
            response = $"✅ Has votado '{voto.ToUpper()}'.";
            return true;
        }

        public static void ResetEstado()
        {
            EventoActivo = false;
            VotacionEncendida = false;
            votos["yes"] = 0;
            votos["no"] = 0;
            jugadoresQueVotaron.Clear();
            eventoPropuesto = "";
        }
    }
}
