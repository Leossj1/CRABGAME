using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.UI;
using CrabPrototype.Player;

namespace CrabPrototype.GameModes
{
    public class BombTagMode : GameModeBase
    {
        public override GameModeType Mode => GameModeType.BombTag;
        public override string DisplayName => "💣 Bomb Tag";
        public override string Instructions => "Si tienes la bomba, toca a otro para pasarla (E/Click). El que la tenga al explotar, eliminado.";
        public override bool IsLastManStanding => true;

        public float fuseTime = 20f;
        float fuse;
        CrabPlayer.PlayerRef holder;

        public override void OnRoundStart()
        {
            fuse = fuseTime;
            if (game.players.Count > 0)
            {
                holder = game.players[Random.Range(0, game.players.Count)];
                SetBomb(holder, true);
            }
        }

        void Update()
        {
            if (game == null || !game.roundActive || holder == null) return;
            fuse -= Time.deltaTime;
            // Parpadeo / escala bomba según fuse (feedback)
            if (fuse <= 0)
            {
                string n = holder.displayName;
                SetBomb(holder, false);
                game.Eliminate(holder, "bomba 💥");
                // Nueva bomba a otro vivo
                var alive = game.players.FindAll(p => p.alive);
                if (alive.Count > 1)
                {
                    fuse = fuseTime * 0.8f;
                    holder = alive[Random.Range(0, alive.Count)];
                    SetBomb(holder, true);
                    HudKillFeed.Push($"Nueva bomba en {holder.displayName}!");
                }
            }
        }

        void OnPushLanded(CrabPlayer.PushInfo info)
        {
            var from = FindRef(info.from);
            var to = FindRef(info.to);
            if (from == null || to == null || !to.alive) return;
            if (from.hasBomb && !to.hasBomb)
            {
                SetBomb(from, false);
                SetBomb(to, true);
                holder = to;
                fuse = Mathf.Max(fuse, 5f); // da un poco de aire
                HudKillFeed.Push($"{from.displayName} → 💣 → {to.displayName}");
            }
        }

        void SetBomb(CrabPlayer.PlayerRef p, bool on)
        {
            p.hasBomb = on;
            p.hasHat = false;
            CrabAvatarBuilder.SetBomb(p.controller.gameObject, on);
        }

        CrabPlayer.PlayerRef FindRef(CrabPlayer c) => game.players.Find(p => p.controller == c);

        public float FuseLeft => fuse;

        public override Vector3 GetSpawnPoint(int i, int total)
        {
            float a = i / (float)total * Mathf.PI * 2;
            return new Vector3(Mathf.Cos(a) * 7, 2, Mathf.Sin(a) * 7);
        }
    }
}
