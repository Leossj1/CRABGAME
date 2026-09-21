using UnityEngine;
using UnityEngine.SceneManagement;
using CrabPrototype.Core;
using CrabPrototype.Maps;

namespace CrabPrototype
{
    /// <summary>
    /// Auto-arranque sin depender del contenido del .unity.
    /// Funciona aunque las escenas estén vacías: inyecta GameManager/HUD/Menu según escena.
    /// </summary>
    public static class AutoBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneLoaded()
        {
            string scene = SceneManager.GetActiveScene().name;
            // Si abre directamente sin usar escena guardada (ej. Play sin escena), no hacemos nada
            if (string.IsNullOrEmpty(scene)) scene = "Game";

            if (scene == "Game")
                EnsureGame();
            else
                EnsureMenu();
        }

        static void EnsureGame()
        {
            if (Camera.main == null)
            {
                var cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.transform.position = new Vector3(0, 10, -12);
                cam.transform.LookAt(Vector3.zero);
                cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
            }
            var gm = Object.FindObjectOfType<GameManager>();
            if (gm == null)
            {
                gm = new GameObject("GameManager").AddComponent<GameManager>();
                gm.roundTime = 120;
            }
            if (Object.FindObjectOfType<MapBuilder>() == null)
                gm.gameObject.AddComponent<MapBuilder>();
            if (Object.FindObjectOfType<UI.GameHUD>() == null)
                new GameObject("HUD").AddComponent<UI.GameHUD>();
        }

        static void EnsureMenu()
        {
            if (Camera.main == null)
            {
                var cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.backgroundColor = new Color(0.15f, 0.18f, 0.25f);
            }
            if (Object.FindObjectOfType<UI.MainMenuUI>() == null)
                new GameObject("Menu").AddComponent<UI.MainMenuUI>();
            if (Object.FindObjectOfType<Network.NetworkBootstrap>() == null)
                new GameObject("Net").AddComponent<Network.NetworkBootstrap>();
        }
    }
}
