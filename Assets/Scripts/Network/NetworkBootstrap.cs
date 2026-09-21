using UnityEngine;

namespace CrabPrototype.Network
{
    /// <summary>
    /// Bootstrap online con Photon PUN 2.
    /// - Si PUN 2 está instalado (define PUN_2_0), conecta a Photon Cloud y crea/une salas.
    /// - Si NO está instalado, el juego corre offline con bots automáticamente.
    /// Para activar online: Window > Asset Store > PUN 2 Free > Import, pega tu AppId en Resources/PhotonServerSettings.
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        public static bool IsOnline { get; private set; }

        [Header("Photon (opcional)")]
        public string roomName = "crab-prototipo";
        public byte maxPlayers = 12;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
#if PUN_2_0
            ConnectPhoton();
#else
            IsOnline = false;
            Debug.Log("[CrabNet] Photon no instalado → modo offline con bots. Ver README para online.");
#endif
        }

#if PUN_2_0
        void ConnectPhoton()
        {
            Photon.Pun.PhotonNetwork.ConnectUsingSettings();
            Photon.Pun.PhotonNetwork.AutomaticallySyncScene = true;
        }

        void OnConnectedToMasterBridge() { }
#endif

        // Llamados por UI
        public void QuickPlay()
        {
#if PUN_2_0
            Photon.Pun.PhotonNetwork.JoinRandomRoom();
#else
            // Offline: ir directo al juego
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
#endif
        }

        public void CreateOrJoinRoom()
        {
#if PUN_2_0
            var opt = new Photon.Realtime.RoomOptions { MaxPlayers = maxPlayers, IsVisible = true, IsOpen = true };
            Photon.Pun.PhotonNetwork.JoinOrCreateRoom(roomName, opt, Photon.Realtime.TypedLobby.Default);
#else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
#endif
        }
    }

#if PUN_2_0
    public class PhotonCallbacks : Photon.Pun.MonoBehaviourPunCallbacks
    {
        public override void OnConnectedToMaster()
        {
            Debug.Log("[CrabNet] Conectado a Photon Master");
        }
        public override void OnJoinRandomFailed(short c, string m)
        {
            Photon.Pun.PhotonNetwork.CreateRoom("crab-" + Random.Range(1000, 9999),
                new Photon.Realtime.RoomOptions { MaxPlayers = 12 });
        }
        public override void OnJoinedRoom()
        {
            NetworkBootstrap.IsOnline = true;
            Photon.Pun.PhotonNetwork.LoadLevel("Game");
        }
    }
#endif
}
