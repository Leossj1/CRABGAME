using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.UI;
using CrabPrototype.Player;

namespace CrabPrototype.GameModes
{
    public class RaceMode : GameModeBase
    {
        public override GameModeType Mode => GameModeType.Race;
        public override string DisplayName => "🏁 Carrera";
        public override string Instructions => "Corre a la meta. Obstáculos te tiran. Empujar vale. Primero en llegar gana.";

        public void OnPlayerFinished(CrabPlayer.PlayerRef p)
        {
            if (!game.roundActive) return;
            game.roundActive = false;
            OnRoundEnd();
            game.DeclareWinner(p.displayName);
        }

        public override Vector3 GetSpawnPoint(int i, int total)
        {
            return new Vector3((i - total / 2f) * 1.5f, 2, -14);
        }
    }
}
