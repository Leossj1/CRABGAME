using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.UI;
using CrabPrototype.Player;

namespace CrabPrototype.GameModes
{
    public class HatKingMode : GameModeBase
    {
        public override GameModeType Mode => GameModeType.HatKing;
        public override string DisplayName => "👑 Hat King";
        public override string Instructions => "Roba el sombrero empujando (E/Click). Gana quien lo tenga más tiempo.";
        public override bool IsLastManStanding => false;

        CrabPlayer.PlayerRef hatHolder;
        float tick;

        public override void OnRoundStart()
        {
            // Sombrero aleatorio inicial
            var alive = game.players;
            if (alive.Count > 0)
            {
                hatHolder = alive[Random.Range(0, alive.Count)];
                SetHat(hatHolder, true);
            }
        }

        void Update()
        {
            if (game == null || !game.roundActive) return;
            tick += Time.deltaTime;
            if (tick >= 1f && hatHolder != null && hatHolder.alive)
            {
                tick = 0;
                game.AddScore(hatHolder.id, 1f);
            }
        }

        void OnPushLanded(CrabPlayer.PushInfo info)
        {
            // Si empujas al rey, le robas el sombrero
            var fromRef = FindRef(info.from);
            var toRef = FindRef(info.to);
            if (fromRef == null || toRef == null) return;
            if (toRef.hasHat)
            {
                SetHat(toRef, false);
                SetHat(fromRef, true);
                hatHolder = fromRef;
                HudKillFeed.Push($"{fromRef.displayName} robó el sombrero!");
            }
        }

        CrabPlayer.PlayerRef FindRef(CrabPlayer c)
        {
            return game.players.Find(p => p.controller == c);
        }

        void SetHat(CrabPlayer.PlayerRef p, bool on)
        {
            p.hasHat = on;
            CrabAvatarBuilder.SetHat(p.controller.gameObject, on);
        }

        public override Vector3 GetSpawnPoint(int i, int total)
        {
            float a = i / (float)total * Mathf.PI * 2;
            return new Vector3(Mathf.Cos(a) * 8, 2, Mathf.Sin(a) * 8);
        }
    }
}
