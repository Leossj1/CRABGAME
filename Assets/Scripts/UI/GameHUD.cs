using UnityEngine;
using CrabPrototype.Core;

namespace CrabPrototype.UI
{
    /// <summary>HUD: timer, modo, instrucciones, scores y pantalla de ganador. Todo con IMGUI para prototipo (sin prefabs).</summary>
    public class GameHUD : MonoBehaviour
    {
        GameManager gm;
        string winner = null;

        void Start()
        {
            gm = GameManager.Instance ?? FindObjectOfType<GameManager>();
            if (gm != null) gm.OnWinnerDeclared += OnWin;
        }

        void OnWin(string w) { winner = w; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        void OnGUI()
        {
            if (gm == null) return;
            GUI.skin.label.fontSize = 16;

            // Top bar
            GUILayout.BeginArea(new Rect(10, 10, 600, 120));
            GUILayout.Label($"Modo: {gm.ActiveMode?.DisplayName ?? gm.currentMode.ToString()}  |  ⏱ {Mathf.CeilToInt(gm.timeLeft)}s");
            if (gm.ActiveMode != null) GUILayout.Label(gm.ActiveMode.Instructions);
            GUILayout.EndArea();

            // Scores (top-right)
            GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 300));
            GUILayout.Label("— Puntos —");
            foreach (var p in gm.players)
            {
                float s = gm.scores.TryGetValue(p.id, out var v) ? v : 0;
                string tags = "";
                if (p.hasHat) tags += " 👑";
                if (p.hasBomb) tags += " 💣";
                if (!p.alive) tags += " ☠";
                GUILayout.Label($"{p.displayName}: {s:0}{tags}");
            }
            // Info bomba
            if (gm.ActiveMode is GameModes.BombTagMode b)
                GUILayout.Label($"Bomba explota en: {b.FuseLeft:0.0}s");
            GUILayout.EndArea();

            // Killfeed abajo-izquierda
            HudKillFeed.Draw();

            // Crosshair
            if (winner == null)
            {
                GUI.Label(new Rect(Screen.width / 2 - 5, Screen.height / 2 - 5, 20, 20), "+");
            }

            // Pantalla ganador
            if (winner != null)
            {
                GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 120, 400, 240), "");
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 180, Screen.height / 2 - 100, 360, 200));
                GUILayout.Label($"🏆 GANADOR: {winner}", new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(15);
                if (GUILayout.Button("🔁 Repetir modo", GUILayout.Height(40))) { winner = null; gm.RestartSameMode(); }
                if (GUILayout.Button("⏭ Siguiente modo", GUILayout.Height(40))) { winner = null; gm.NextMode(); }
                if (GUILayout.Button("🏠 Menú", GUILayout.Height(40)))
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                GUILayout.EndArea();
            }

            // Botón menú chico
            if (winner == null && GUI.Button(new Rect(10, Screen.height - 40, 120, 30), "Salir"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public static class HudKillFeed
    {
        static readonly System.Collections.Generic.Queue<(string, float)> feed = new();
        public static void Push(string msg)
        {
            feed.Enqueue((msg, Time.time + 5f));
            while (feed.Count > 5) feed.Dequeue();
            Debug.Log("[Feed] " + msg);
        }
        public static void Draw()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 180, 500, 140));
            foreach (var (msg, until) in feed.ToArray())
                if (Time.time < until) GUILayout.Label(msg);
            GUILayout.EndArea();
        }
    }
}
