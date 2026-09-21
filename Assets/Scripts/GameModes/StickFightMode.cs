using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.UI;
using CrabPrototype.Player;

namespace CrabPrototype.GameModes
{
    public class StickFightMode : GameModeBase
    {
        public override GameModeType Mode => GameModeType.StickFight;
        public override string DisplayName => "🥢 Stick Fight";
        public override string Instructions => "Todos con palo (click derecho / G). Saca a los demás de la arena. Último en pie gana.";
        public override bool IsLastManStanding => true;

        public override void OnRoundStart()
        {
            foreach (var p in game.players)
                p.controller.GiveStick();
        }

        public override Vector3 GetSpawnPoint(int i, int total)
        {
            float a = i / (float)total * Mathf.PI * 2;
            return new Vector3(Mathf.Cos(a) * 8, 2, Mathf.Sin(a) * 8);
        }
    }
}
