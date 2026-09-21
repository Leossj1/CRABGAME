using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.UI;

namespace CrabPrototype.GameModes
{
    public class KingOfHillMode : GameModeBase
    {
        public override GameModeType Mode => GameModeType.KingOfHill;
        public override string DisplayName => "⛰️ Rey de la Colina";
        public override string Instructions => "Quédate en la zona brillante para sumar puntos. ¡Empuja a los demás fuera!";
        public override bool IsLastManStanding => false;

        GameObject hill;
        float tick;

        public override void OnRoundStart()
        {
            hill = GameObject.Find("HillZone");
        }

        void Update()
        {
            if (game == null || !game.roundActive || hill == null) return;
            tick += Time.deltaTime;
            if (tick < 1f) return;
            tick = 0;
            Vector3 c = hill.transform.position;
            float r = 4f;
            foreach (var p in game.players)
            {
                if (!p.alive) continue;
                if (Vector3.Distance(new Vector3(p.controller.transform.position.x, 0, p.controller.transform.position.z),
                                     new Vector3(c.x, 0, c.z)) < r)
                    game.AddScore(p.id, 1f);
            }
        }

        public override Vector3 GetSpawnPoint(int i, int total)
        {
            float a = i / (float)total * Mathf.PI * 2;
            return new Vector3(Mathf.Cos(a) * 12, 2, Mathf.Sin(a) * 12);
        }
    }
}
