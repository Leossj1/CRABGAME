using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CrabPrototype.Core;

namespace CrabPrototype.UI
{
    /// <summary>Menú principal estilo Crab Game: elige modo y juega (online si hay Photon, si no con bots).</summary>
    public class MainMenuUI : MonoBehaviour
    {
        GameModeType selected = GameModeType.HatKing;

        void OnGUI()
        {
            var skin = GUI.skin;
            skin.label.fontSize = 16;
            skin.button.fontSize = 18;

            GUILayout.BeginArea(new Rect(Screen.width / 2 - 250, 60, 500, Screen.height - 120));
            GUILayout.Label("🦀 CRAB GAME — PROTOTIPO", new GUIStyle(GUI.skin.label) { fontSize = 32, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(10);
            GUILayout.Label("Mismo estilo: low-poly, empujones, caos multijugador. Squid-Game vibes.", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(20);

            GUILayout.Label("Elige modo:");
            var modes = System.Enum.GetValues(typeof(GameModeType));
            foreach (GameModeType m in modes)
            {
                GUI.backgroundColor = (m == selected) ? Color.green : Color.white;
                if (GUILayout.Button(ModeName(m), GUILayout.Height(38)))
                    selected = m;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(15);

            if (GUILayout.Button("▶ JUGAR (Online si hay Photon, si no vs Bots)", GUILayout.Height(50)))
            {
                PlayerPrefs.SetInt("CrabMode", (int)selected);
                SceneManager.LoadScene("Game");
            }
            GUILayout.Space(8);
            GUILayout.Label("WASD moverse · Mouse mirar · Espacio saltar · Click/E empujar · F agarrar · Palo: click derecho",
                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13 });
            GUILayout.EndArea();
        }

        string ModeName(GameModeType m) => m switch
        {
            GameModeType.HatKing => "👑 Hat King",
            GameModeType.BombTag => "💣 Bomb Tag",
            GameModeType.KingOfHill => "⛰️ Rey de la Colina",
            GameModeType.GlassFloor => "🪟 Piso de Cristal (Squid)",
            GameModeType.Race => "🏁 Carrera",
            GameModeType.FloorIsLava => "🌋 Suelo es Lava",
            GameModeType.StickFight => "🥢 Stick Fight",
            _ => m.ToString()
        };
    }
}
