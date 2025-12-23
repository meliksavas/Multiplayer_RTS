using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap kütüphanesi eklendi
using Photon.Pun;

public class MapSpawner : MonoBehaviourPunCallbacks
{
    [Header("Referanslar")]
    public Tilemap groundTilemap; // Haritanýn olduðu Tilemap'i buraya sürükle

    [Header("Prefab Ýsimleri (Resources Klasöründe Olmalý)")]
    public string treePrefab = "Tree";
    public string sheepPrefab = "Sheep";

    [Header("Spawn Sayýlarý")]
    public int treeCount = 40;
    public int sheepCount = 15;

    [Header("Ayarlar")]
    public LayerMask obstacleLayer;

    // Tilemap'in sýnýrlarýný burada tutacaðýz
    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Start()
    {
        // Tilemap atanmýþ mý kontrol et
        if (groundTilemap == null)
        {
            Debug.LogError("MapSpawner: Lütfen Inspector'dan Ground Tilemap'i atayýn!");
            return;
        }

        // Tilemap sýnýrlarýný hesapla (Hücre koordinatlarýný Dünya koordinatlarýna çeviriyoruz)
        CalculateMapBounds();

        // Sadece Master Client haritayý oluþturur
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnMap();
        }
    }

    void CalculateMapBounds()
    {
        // Tilemap'in kapladýðý alaný sýkýþtýr (Compress) ki boþluklar gitsin
        groundTilemap.CompressBounds();

        // Hücre (Cell) koordinatlarýný al
        BoundsInt bounds = groundTilemap.cellBounds;

        // Dünya (World) koordinatlarýna çevir
        // min: Sol Alt köþe, max: Sað Üst köþe
        minBounds = groundTilemap.CellToWorld(bounds.min);
        maxBounds = groundTilemap.CellToWorld(bounds.max);

        Debug.Log($"Harita Sýnýrlarý Bulundu: Min({minBounds}) - Max({maxBounds})");
    }

    void SpawnMap()
    {
        Debug.Log("Harita oluþturuluyor (Kaynaklar)...");

        // 1. Aðaçlarý Rastgele Daðýt
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 pos = GetRandomValidPosition();
            if (pos != Vector3.zero)
                PhotonNetwork.InstantiateRoomObject(treePrefab, pos, Quaternion.identity);
        }

        // 2. Koyunlarý Rastgele Daðýt
        for (int i = 0; i < sheepCount; i++)
        {
            Vector3 pos = GetRandomValidPosition();
            if (pos != Vector3.zero)
                PhotonNetwork.InstantiateRoomObject(sheepPrefab, pos, Quaternion.identity);
        }
    }

    Vector3 GetRandomValidPosition()
    {
        int maxAttempts = 10;
        // Tilemap sýnýrlarýndan 1 birim içeriden baþla ki tam kenara gelmesin
        float padding = 1.0f;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Tilemap'in gerçek sýnýrlarý içinde rastgele nokta seç
            float x = Random.Range(minBounds.x + padding, maxBounds.x - padding);
            float y = Random.Range(minBounds.y + padding, maxBounds.y - padding);
            Vector3 candidatePos = new Vector3(x, y, 0);

            // Çakýþma kontrolü
            if (Physics2D.OverlapCircle(candidatePos, 1.0f, obstacleLayer) == null)
            {
                return candidatePos;
            }
        }
        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        // Harita sýnýrlarýný editörde sarý çerçeve olarak göster
        if (groundTilemap != null)
        {
            groundTilemap.CompressBounds();

            // Hatalý hesaplama satýrlarý kaldýrýldý, doðrudan localBounds kullanýlýyor.
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groundTilemap.localBounds.center, groundTilemap.localBounds.size);
        }
    }
}