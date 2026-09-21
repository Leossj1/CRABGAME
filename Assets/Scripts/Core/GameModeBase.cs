using UnityEngine;
using CrabPrototype.Player;

namespace CrabPrototype.Core
{
    /// <summary>
    /// Base para todos los modos. Cada modo decide spawn points,
    /// condición de victoria y qué pasa al morir / caer.
    /// Funciona offline (bots) y online (Photon si está instalado).
    /// </summary>
    public abstract class GameModeBase : MonoBehaviour
    {
        public abstract GameModeType Mode { get; }
        public virtual string DisplayName => Mode.ToString();
        public virtual string Instructions => "";

        protected GameManager game;

        public virtual void Setup(GameManager manager)
        {
            game = manager;
        }

        public virtual void OnRoundStart() { }
        public virtual void OnRoundEnd() { }
        public virtual void OnPlayerEliminated(CrabPlayer.PlayerRef player, string reason) { }
        public virtual void OnPlayerFell(CrabPlayer.PlayerRef player) { }
        public virtual Vector3 GetSpawnPoint(int index, int total) => new Vector3(index * 2f, 2f, 0);

        public virtual bool IsLastManStanding => true;
    }
}

