using UnityEngine;

namespace CrabPrototype.Core
{
    /// <summary>
    /// Tipos de juego del prototipo. Esencia Crab Game:
    /// interacción constante jugador-jugador (empujones) y jugador-entorno.
    /// </summary>
    public enum GameModeType
    {
        HatKing,      // Mantén el sombrero / roba el sombrero
        BombTag,      // Te pasan la bomba, explota con temporizador
        KingOfHill,   // Rey de la colina, mantén la zona
        GlassFloor,   // Piso de cristal, Squid Game
        Race,         // Carrera con obstáculos y caídas
        FloorIsLava,  // El suelo es lava, sube a plataformas
        StickFight    // Pelea con palos, knockback fuerte
    }

    [CreateAssetMenu(menuName = "Crab/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public GameModeType mode = GameModeType.HatKing;
        public int roundTimeSeconds = 120;
        public int minPlayersToStart = 2;
        public int botsIfOffline = 7;
        public float respawnHeight = -20f;
    }
}
