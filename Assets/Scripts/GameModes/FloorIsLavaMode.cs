using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.UI;

namespace CrabPrototype.GameModes
{
    public class FloorIsLavaMode : GameModeBase
    {
        public override GameModeType Mode => GameModeType.FloorIsLava;
        public override string DisplayName => "🌋 Suelo es Lava";
        public override string Instructions => "La lava sube. Sube a plataformas. Último en sobrevivir gana. Empuja a otros a la lava.";
        public override bool IsLastManStanding => true;

        public float lavaRiseSpeed = 0.35f;
        GameObject lava;

        public override void OnRoundStart()
        {
            lava = GameObject.Find("Lava");
        }

        void Update()
        {
            if (game == null || !game.roundActive) return;
            if (lava != null)
                lava.transform.position += Vector3.up * lavaRiseSpeed * Time.deltaTime;

            // Tocar lava = eliminado
            if (lava != null)
            {
                float ly = lava.transform.position.y;
                foreach (var p in game.players)
                {
                    if (!p.alive) continue;
                    if (p.controller.transform.position.y < ly + 0.2f)
                        game.Eliminate(p, "lava 🌋");
                }
            }
        }

        public override Vector3 GetSpawnPoint(int i, int total)
        {
            float a = i / (float)total * Mathf.PI * 2;
            return new Vector3(Mathf.Cos(a) * 9, 3, Mathf.Sin(a) * 9);
        }
    }
}
