using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.UI;
using CrabPrototype.Player;

namespace CrabPrototype.GameModes
{
    /// <summary>
    /// Piso de cristal estilo Squid Game: puente de pares, uno falso (cae) y uno real.
    /// Llegar al final gana. Caer = eliminado.
    /// </summary>
    public class GlassFloorMode : GameModeBase
    {
        public override GameModeType Mode => GameModeType.GlassFloor;
        public override string DisplayName => "🪟 Piso de Cristal";
        public override string Instructions => "Cruza el puente. Un cristal es falso en cada par. ¡Mira a otros caer primero!";

        public override void OnRoundStart() { }

        public override void OnPlayerEliminated(Player.CrabPlayer.PlayerRef player, string reason) { }

        // Gana el primero que cruza (detectado por FinishTrigger creado en MapBuilder)
        public void OnPlayerFinished(Player.CrabPlayer.PlayerRef p)
        {
            if (!game.roundActive) return;
            game.roundActive = false;
            OnRoundEnd();
            game.DeclareWinner(p.displayName);
        }

        public override Vector3 GetSpawnPoint(int i, int total)
        {
            return new Vector3((i - total / 2f) * 1.5f, 2, -8);
        }
    }
}

