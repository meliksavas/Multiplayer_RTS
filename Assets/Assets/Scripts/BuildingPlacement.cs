using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class BuildingPlacement : MonoBehaviourPun
{
    [System.Serializable]
    public struct BuildingData
    {
        public string name;
        public string prefabName; // Resources klasöründeki prefab adý
        public GameObject ghostPrefab; // Yarý saydam görsel prefab
        public int goldCost;
        public int woodCost;
    }

    public List<BuildingData> buildings; // Inspector'dan binalarý ekle (0: Maden, 1: Kýþla vb.)

    [Header("Ayarlar")]
    public LayerMask obstacleLayer; // Ýnþaat yapýlamayacak yerler (Aðaç, Su, Baþka Bina)

    private BuildingData? currentBuilding = null; // Þu an seçili bina var mý?
    private GameObject currentGhost; // Mouse ucundaki hayalet obje
    private Camera cam;

    public bool IsPlacing { get { return currentBuilding != null; } } // PlayerInteraction kontrolü için

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Eðer bir bina seçiliyse
        if (currentBuilding != null && currentGhost != null)
        {
            HandleBuildingMode();
        }

        // Test Tuþlarý (Daha sonra UI Butonlarýna baðlayacaðýz)
        if (Input.GetKeyDown(KeyCode.B)) SelectBuilding(0); // Örn: 0. Ýndex'teki binayý seç
    }

    // UI Butonlarý bu fonksiyonu çaðýracak
    public void SelectBuilding(int index)
    {
        if (index < 0 || index >= buildings.Count) return;

        // Varsa eski hayaleti temizle
        CancelBuilding();

        BuildingData data = buildings[index];

        // Kaynak Yeterli mi?
        if (ResourceManager.MyInstance.gold < data.goldCost || ResourceManager.MyInstance.wood < data.woodCost)
        {
            Debug.Log("Yetersiz Kaynak!");
            return;
        }

        currentBuilding = data;

        // Hayalet objeyi mouse ucunda oluþtur
        Vector3 mousePos = GetMouseWorldPos();
        currentGhost = Instantiate(data.ghostPrefab, mousePos, Quaternion.identity);
    }

    void HandleBuildingMode()
    {
        Vector3 mousePos = GetMouseWorldPos();
        currentGhost.transform.position = mousePos;

        // Ýptal (Sað Týk)
        if (Input.GetMouseButtonDown(1))
        {
            CancelBuilding();
            return;
        }

        // Ýnþa Et (Sol Týk)
        if (Input.GetMouseButtonDown(0))
        {
            if (CanPlace(mousePos))
            {
                PlaceBuilding(mousePos);
            }
            else
            {
                Debug.Log("Buraya inþa edilemez! (Engel var)");
            }
        }
    }

    bool CanPlace(Vector3 pos)
    {
        // 2 birim yarýçapýnda engel var mý kontrol et
        Collider2D hit = Physics2D.OverlapCircle(pos, 1.5f, obstacleLayer);
        return hit == null;
    }

    void PlaceBuilding(Vector3 pos)
    {
        // Kaynaðý Harca (ResourceManager PlayerController üzerinde olmalý)
        if (ResourceManager.MyInstance.SpendResource("Gold", currentBuilding.Value.goldCost) &&
            ResourceManager.MyInstance.SpendResource("Wood", currentBuilding.Value.woodCost))
        {
            // Network üzerinden binayý oluþtur
            PhotonNetwork.Instantiate(currentBuilding.Value.prefabName, pos, Quaternion.identity);

            // Ýnþaat modundan çýk
            CancelBuilding();
        }
    }

    public void CancelBuilding()
    {
        currentBuilding = null;
        if (currentGhost != null) Destroy(currentGhost);
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }
}