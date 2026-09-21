using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrabPrototype.Player;
using CrabPrototype.Maps;
using CrabPrototype.Bots;
using CrabPrototype.GameModes;
using CrabPrototype.UI;

namespace CrabPrototype.Core
{
    /// <summary>
    /// Orquesta rondas: crea mapa, spawnea jugadores+bots, corre timer, declara ganador.
    /// Estilo Crab Game: rondas cortas, eliminaciones, rotación de mapas.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Config")]
        public GameConfig config;
        public GameModeType currentMode = GameModeType.HatKing;
        public int roundTime = 120;

        [Header("Refs (auto)")]
        public MapBuilder mapBuilder;
        public Transform playerContainer;

        public List<CrabPlayer.PlayerRef> players = new List<CrabPlayer.PlayerRef>();
        public Dictionary<int, float> scores = new Dictionary<int, float>();

        public float timeLeft;
        public bool roundActive;
        public string winnerName = "";

        public System.Action<string> OnWinnerDeclared;
        public System.Action<float> OnTimerTick;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (playerContainer == null)
            {
                var go = new GameObject("Players");
                playerContainer = go.transform;
            }
            if (mapBuilder == null)
                mapBuilder = FindObjectOfType<MapBuilder>() ?? gameObject.AddComponent<MapBuilder>();
        }

        void Start()
        {
            // Si vienes del menú, PlayerPrefs trae el modo elegido
            if (PlayerPrefs.HasKey("CrabMode"))
                currentMode = (GameModeType)PlayerPrefs.GetInt("CrabMode");
            StartRound(currentMode);
        }

        public void StartRound(GameModeType mode)
        {
            StopAllCoroutines();
            currentMode = mode;
            ClearPlayers();
            mapBuilder.BuildForMode(mode, this);
            var gm = GetModeBehaviour(mode);
            gm.Setup(this);
            ActiveMode = gm;
            ActiveMode.OnRoundStart();

            SpawnLocalAndBots();
            timeLeft = config != null ? config.roundTimeSeconds : roundTime;
            if (timeLeft <= 0) timeLeft = 120;
            roundActive = true;
            winnerName = "";
            StartCoroutine(RoundLoop());
        }

        public GameModeBase ActiveMode { get; private set; }

        IEnumerator RoundLoop()
        {
            while (roundActive && timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                OnTimerTick?.Invoke(timeLeft);
                CheckFallDeaths();
                yield return null;
            }
            if (roundActive) EndRoundByTime();
        }

        void EndRoundByTime()
        {
            roundActive = false;
            ActiveMode?.OnRoundEnd();
            // Ganador = mayor score, si no hay scores = último vivo
            string win = PickWinnerByScore();
            DeclareWinner(win);
        }

        public void Eliminate(CrabPlayer.PlayerRef p, string reason)
        {
            if (!roundActive || !p.alive) return;
            p.alive = false;
            ActiveMode?.OnPlayerEliminated(p, reason);
            HudKillFeed.Push($"{p.displayName} eliminado ({reason})");
            p.controller?.RespawnOrSpectate();
            CheckLastManStanding();
        }

        void CheckLastManStanding()
        {
            if (ActiveMode == null || !ActiveMode.IsLastManStanding) return;
            int alive = 0; CrabPlayer.PlayerRef last = null;
            foreach (var p in players) if (p.alive) { alive++; last = p; }
            if (alive <= 1 && players.Count > 1)
            {
                roundActive = false;
                ActiveMode.OnRoundEnd();
                DeclareWinner(last != null ? last.displayName : "Nadie");
            }
        }

        void CheckFallDeaths()
        {
            float killY = -20f;
            foreach (var p in players)
            {
                if (!p.alive || p.controller == null) continue;
                if (p.controller.transform.position.y < killY)
                    Eliminate(p, "caída");
            }
        }

        string PickWinnerByScore()
        {
            if (scores.Count == 0)
            {
                foreach (var p in players) if (p.alive) return p.displayName;
                return players.Count > 0 ? players[0].displayName : "Nadie";
            }
            int bestId = -1; float best = float.MinValue;
            foreach (var kv in scores) if (kv.Value > best) { best = kv.Value; bestId = kv.Key; }
            var pl = players.Find(x => x.id == bestId);
            return pl != null ? pl.displayName : "Nadie";
        }

        public void DeclareWinner(string name)
        {
            winnerName = name;
            OnWinnerDeclared?.Invoke(name);
            Debug.Log($"[Crab] Ganador: {name} en modo {currentMode}");
        }

        public void AddScore(int playerId, float amount)
        {
            if (!scores.ContainsKey(playerId)) scores[playerId] = 0;
            scores[playerId] += amount;
        }

        // ---------- Spawn ----------

        void ClearPlayers()
        {
            foreach (var p in players)
                if (p.controller != null) Destroy(p.controller.gameObject);
            players.Clear();
            scores.Clear();
            foreach (Transform c in playerContainer) Destroy(c.gameObject);
        }

        void SpawnLocalAndBots()
        {
            int total = 8;
#if PUN_2_0
            // Online: Photon se encarga del spawn en NetworkBootstrap.
            // Aquí solo spawneamos si NO estamos conectados (fallback offline).
            if (Network.NetworkBootstrap.IsOnline) return;
#endif
            // Offline: 1 local + bots
            for (int i = 0; i < total; i++)
            {
                bool isBot = i != 0;
                SpawnOne(i, isBot);
            }
        }

        public CrabPlayer.PlayerRef SpawnOne(int index, bool isBot)
        {
            Vector3 pos = ActiveMode != null
                ? ActiveMode.GetSpawnPoint(index, 8)
                : new Vector3(index * 2f, 2f, 0);
            var go = CrabPlayer.SpawnPrefab(pos, Quaternion.identity, playerContainer, index, isBot);
            var ctrl = go.GetComponent<CrabPlayer>();
            if (isBot) go.AddComponent<SimpleBot>();
            var pref = new CrabPlayer.PlayerRef
            {
                id = index,
                displayName = isBot ? $"Cangrejo-Bot {index}" : "Tú (Cangrejo 0)",
                controller = ctrl,
                alive = true,
                isBot = isBot
            };
            ctrl.Bind(pref);
            players.Add(pref);
            return pref;
        }

        GameModeBase GetModeBehaviour(GameModeType mode)
        {
            // Reusamos componentes en este mismo GameObject
            foreach (var m in GetComponents<GameModeBase>()) Destroy(m);
            switch (mode)
            {
                case GameModeType.HatKing: return gameObject.AddComponent<HatKingMode>();
                case GameModeType.BombTag: return gameObject.AddComponent<BombTagMode>();
                case GameModeType.KingOfHill: return gameObject.AddComponent<KingOfHillMode>();
                case GameModeType.GlassFloor: return gameObject.AddComponent<GlassFloorMode>();
                case GameModeType.Race: return gameObject.AddComponent<RaceMode>();
                case GameModeType.FloorIsLava: return gameObject.AddComponent<FloorIsLavaMode>();
                case GameModeType.StickFight: return gameObject.AddComponent<StickFightMode>();
                default: return gameObject.AddComponent<HatKingMode>();
            }
        }

        // Llamado por UI para volver a jugar / cambiar modo
        public void RestartSameMode() => StartRound(currentMode);
        public void NextMode()
        {
            int n = System.Enum.GetValues(typeof(GameModeType)).Length;
            StartRound((GameModeType)(((int)currentMode + 1) % n));
        }
    }
}
