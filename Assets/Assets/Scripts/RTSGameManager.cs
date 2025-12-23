using UnityEngine;
using Photon.Pun;
using TMPro; // UI Yazýlarý için

public class RTSGameManager : MonoBehaviourPunCallbacks
{
    public static RTSGameManager Instance;

    [Header("Spawning")]
    public Transform[] spawnPoints;
    public GameObject mainBuildingPrefab;
    public GameObject playerControllerPrefab;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    private void Awake()
    {
        // Singleton (Tekil Eriþim) Kurulumu
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("DÝKKAT: Sahnede birden fazla RTSGameManager var! Fazlalýk olan yok ediliyor.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);

        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
        Transform mySpawn = spawnPoints[index];

        if (mainBuildingPrefab != null)
            PhotonNetwork.Instantiate(mainBuildingPrefab.name, mySpawn.position, Quaternion.identity);

        if (playerControllerPrefab != null)
            PhotonNetwork.Instantiate(playerControllerPrefab.name, mySpawn.position, Quaternion.identity);
    }

    // MainBuilding.cs tarafýndan çaðrýlýr
    public void OnMainBuildingDestroyed(int loserActorNumber)
    {
        Debug.Log($"<color=red>ANA BÝNA YIKILDI! Kaybeden ID: {loserActorNumber}</color>");
        Debug.Log("RPC_GameOver gönderiliyor...");

        // PhotonView kontrolü
        if (photonView == null)
        {
            Debug.LogError("HATA: RTSGameManager üzerinde PhotonView bileþeni YOK! RPC gönderilemedi.");
            return;
        }

        // Tüm oyunculara oyunun bittiðini bildir
        photonView.RPC("RPC_GameOver", RpcTarget.All, loserActorNumber);
    }

    [PunRPC]
    void RPC_GameOver(int loserActorNumber)
    {
        Debug.Log("OYUN BÝTTÝ (RPC ALINDI)");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over Paneli açýldý.");
        }
        else
        {
            Debug.LogError("HATA: Game Over Panel referansý boþ! Inspector'dan atadýn mý?");
        }

        if (PhotonNetwork.LocalPlayer.ActorNumber == loserActorNumber)
        {
            if (gameOverText)
            {
                gameOverText.text = "DEFEAT\n(KAYBETTÝN)";
                gameOverText.color = Color.red;
            }
        }
        else
        {
            if (gameOverText)
            {
                gameOverText.text = "VICTORY\n(KAZANDIN)";
                gameOverText.color = Color.green;
            }
        }
    }
}