using UnityEngine;
using CrabPrototype.Core;
using CrabPrototype.Maps;

namespace CrabPrototype
{
    /// <summary>
    /// Auto-bootstrap de la escena Game: si la escena está vacía, crea lo necesario.
    /// Así no dependemos de YAML complejo de .unity.
    /// Adjunta este script a un GameObject en la escena Game.
    /// </summary>
    public class GameSceneBootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Cámara provisional (el player local trae su FPSCam, esto es fallback)
            if (Camera.main == null)
            {
                var cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.transform.position = new Vector3(0, 10, -12);
                cam.transform.LookAt(Vector3.zero);
                cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
                cam.gameObject.AddComponent<AudioListener>();
            }
            if (FindObjectOfType<GameManager>() == null)
            {
                var gm = new GameObject("GameManager").AddComponent<GameManager>();
                gm.config = null;
                gm.roundTime = 120;
            }
            if (FindObjectOfType<MapBuilder>() == null)
                FindObjectOfType<GameManager>().gameObject.AddComponent<MapBuilder>();
            if (FindObjectOfType<UI.GameHUD>() == null)
                new GameObject("HUD").AddComponent<UI.GameHUD>();
        }
    }

    /// <summary>Bootstrap del menú.</summary>
    public class MenuSceneBootstrap : MonoBehaviour
    {
        void Awake()
        {
            if (Camera.main == null)
            {
                var cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.backgroundColor = new Color(0.15f, 0.18f, 0.25f);
                cam.transform.position = new Vector3(0, 5, -10);
            }
            if (FindObjectOfType<UI.MainMenuUI>() == null)
                new GameObject("Menu").AddComponent<UI.MainMenuUI>();
            if (FindObjectOfType<Network.NetworkBootstrap>() == null)
                new GameObject("Net").AddComponent<Network.NetworkBootstrap>();
        }
    }
}
