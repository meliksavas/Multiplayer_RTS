using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Selection Settings")]
    public RectTransform selectionBoxUI;
    public LayerMask unitLayer;
    public LayerMask resourceLayer;
    public LayerMask groundLayer;

    [Header("Formation Settings")]
    public float unitGap = 0.8f;

    private Vector2 startMousePos;
    private Vector2 currentMousePos;
    private Rect selectionRect;

    private List<UnitController> selectedUnits = new List<UnitController>();
    private Camera cam;
    private BuildingPlacement buildingPlacer;
    private PhotonView view; // PhotonView referansı

    void Start()
    {
        cam = Camera.main;
        buildingPlacer = GetComponent<BuildingPlacement>();
        view = GetComponent<PhotonView>(); // Referansı al

        // --- YENİ DÜZELTME: RAKİBİN UI'INI GİZLE ---
        // Eğer bu karakter benim değilse, üzerindeki Canvas'ı (butonları, sağlık barlarını değil, main canvası) kapat.
        // Genelde Player prefab'ının içinde bir "Canvas" olur.
        if (!view.IsMine)
        {
            Canvas myCanvas = GetComponentInChildren<Canvas>();
            if (myCanvas != null)
            {
                // World Space (HealthBar) canvaslarını kapatmamak için kontrol edebiliriz
                // Ama genelde PlayerController içindeki Canvas "Overlay"dir ve sadece oyuncuya özeldir.
                if (myCanvas.renderMode == RenderMode.ScreenSpaceOverlay || myCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    myCanvas.gameObject.SetActive(false);
                }
            }
            return; // Benim değilse UI setup'ı yapmaya gerek yok
        }
        // -------------------------------------------

        // Otomatik UI Bulma
        if (selectionBoxUI == null && view.IsMine)
        {
            GameObject foundObj = GameObject.Find("SelectionBoxUI");
            if (foundObj != null) selectionBoxUI = foundObj.GetComponent<RectTransform>();
        }
        if (selectionBoxUI)
        {
            selectionBoxUI.gameObject.SetActive(false);
            selectionBoxUI.pivot = new Vector2(0.5f, 0.5f);
            selectionBoxUI.anchorMin = new Vector2(0.5f, 0.5f);
            selectionBoxUI.anchorMax = new Vector2(0.5f, 0.5f);
        }
    }

    void Update()
    {
        if (!view.IsMine) return;

        if (buildingPlacer != null && buildingPlacer.IsPlacing) return;

        HandleSelectionInput();
        HandleMovementInput();

        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnPawn();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnGoldMine();
        }
    }

    void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startMousePos = Input.mousePosition;
            selectionRect = new Rect();
            if (selectionBoxUI) selectionBoxUI.gameObject.SetActive(true);
            if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
        }

        if (Input.GetMouseButton(0))
        {
            currentMousePos = Input.mousePosition;
            UpdateSelectionVisual();
            UpdateSelectionRect();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectionBoxUI) selectionBoxUI.gameObject.SetActive(false);
            SelectUnits();
        }
    }

    void HandleMovementInput()
    {
        // Listeyi bir kez daha kontrol et (Garanti olsun)
        selectedUnits.RemoveAll(unit => unit == null);
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Vector3 targetCenter = cam.ScreenToWorldPoint(Input.mousePosition);
            targetCenter.z = 0;

            RaycastHit2D hitEnemy = Physics2D.Raycast(targetCenter, Vector2.zero, Mathf.Infinity, unitLayer);
            if (hitEnemy.collider != null)
            {
                UnitController enemy = hitEnemy.collider.GetComponent<UnitController>();
                if (enemy != null && !enemy.photonView.IsMine)
                {
                    foreach (var unit in selectedUnits) unit.SetTarget(enemy.transform);
                    return;
                }
            }

            RaycastHit2D hitResource = Physics2D.Raycast(targetCenter, Vector2.zero, Mathf.Infinity, resourceLayer);
            if (hitResource.collider != null)
            {
                ResourceSource resource = hitResource.collider.GetComponent<ResourceSource>();
                if (resource != null)
                {
                    foreach (var unit in selectedUnits) unit.SetResourceTarget(resource);
                    return;
                }
            }

            List<Vector3> formationPoints = GetSpiralFormation(targetCenter, selectedUnits.Count, unitGap);
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                if (i < formationPoints.Count) selectedUnits[i].MoveToPosition(formationPoints[i]);
            }
        }
    }

    List<Vector3> GetSpiralFormation(Vector3 center, int unitCount, float gap)
    {
        List<Vector3> points = new List<Vector3>();
        points.Add(center);
        if (unitCount <= 1) return points;
        int currentCount = 1;
        float radius = gap;
        while (currentCount < unitCount)
        {
            float circumference = 2 * Mathf.PI * radius;
            int unitsInRing = Mathf.FloorToInt(circumference / gap);
            for (int i = 0; i < unitsInRing; i++)
            {
                if (currentCount >= unitCount) break;
                float angle = i * (360f / unitsInRing);
                Vector3 pos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * radius;
                points.Add(center + pos);
                currentCount++;
            }
            radius += gap;
        }
        return points;
    }

    void UpdateSelectionVisual()
    {
        if (!selectionBoxUI) return;
        Vector2 center = (startMousePos + currentMousePos) / 2;
        selectionBoxUI.position = center;
        selectionBoxUI.sizeDelta = new Vector2(Mathf.Abs(startMousePos.x - currentMousePos.x), Mathf.Abs(startMousePos.y - currentMousePos.y));
    }

    void UpdateSelectionRect()
    {
        if (Input.mousePosition.x < startMousePos.x) { selectionRect.xMin = Input.mousePosition.x; selectionRect.xMax = startMousePos.x; }
        else { selectionRect.xMin = startMousePos.x; selectionRect.xMax = Input.mousePosition.x; }
        if (Input.mousePosition.y < startMousePos.y) { selectionRect.yMin = Input.mousePosition.y; selectionRect.yMax = startMousePos.y; }
        else { selectionRect.yMin = startMousePos.y; selectionRect.yMax = Input.mousePosition.y; }
    }

    void SelectUnits()
    {
        UnitController[] allUnits = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (!unit.photonView.IsMine) continue;
            Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position);
            bool inside = selectionRect.Contains(screenPos);
            bool clicked = Vector2.Distance(startMousePos, Input.mousePosition) < 10f && Vector2.Distance(screenPos, Input.mousePosition) < 30f;
            if (inside || clicked) { selectedUnits.Add(unit); unit.SetSelected(true); }
        }
    }

    void DeselectAll()
    {
        foreach (var unit in selectedUnits) unit.SetSelected(false);
        selectedUnits.Clear();
    }

    public void SpawnPawn()
    {
        // P tuşuna basıldığında çağrılır, güvenli olması için IsMine kontrolünü buraya da ekledim.
        if (!GetComponent<PhotonView>().IsMine) return;

        Vector3 spawnPos = transform.position;
        spawnPos.z = 0f;
        spawnPos.x += Random.Range(-2f, 2f);
        spawnPos.y += Random.Range(-2f, 2f);
        PhotonNetwork.Instantiate("Pawn", spawnPos, Quaternion.identity);
    }
    public void SpawnArcher()
    {
        // P tuşuna basıldığında çağrılır, güvenli olması için IsMine kontrolünü buraya da ekledim.
        if (!GetComponent<PhotonView>().IsMine) return;

        Vector3 spawnPos = transform.position;
        spawnPos.z = 0f;
        spawnPos.x += Random.Range(-2f, 2f);
        spawnPos.y += Random.Range(-2f, 2f);
        PhotonNetwork.Instantiate("Archer", spawnPos, Quaternion.identity);
    }
    public void SpawnSoldier()
    {
        // P tuşuna basıldığında çağrılır, güvenli olması için IsMine kontrolünü buraya da ekledim.
        if (!GetComponent<PhotonView>().IsMine) return;

        Vector3 spawnPos = transform.position;
        spawnPos.z = 0f;
        spawnPos.x += Random.Range(-2f, 2f);
        spawnPos.y += Random.Range(-2f, 2f);
        PhotonNetwork.Instantiate("Soldier", spawnPos, Quaternion.identity);
    }

    public void SpawnGoldMine()
    {
        // --- DÜZELTME: SADECE KENDİ BUTONUMUZ ÇALIŞSIN ---
        if (!GetComponent<PhotonView>().IsMine) return;
        // -------------------------------------------------

        if (buildingPlacer != null)
        {
            buildingPlacer.SelectBuilding(0);
        }
        else
        {
            Vector3 spawnPos = transform.position;
            spawnPos.x += 4f;
            PhotonNetwork.Instantiate("GoldMine", spawnPos, Quaternion.identity);
            Debug.LogWarning("BuildingPlacement scripti bulunamadı, direkt spawn edildi.");
        }
    }
    public void SpawnArcherTower()
    {
        // --- DÜZELTME: SADECE KENDİ BUTONUMUZ ÇALIŞSIN ---
        if (!GetComponent<PhotonView>().IsMine) return;
        // -------------------------------------------------

        if (buildingPlacer != null)
        {
            buildingPlacer.SelectBuilding(1);
        }
        else
        {
            Vector3 spawnPos = transform.position;
            spawnPos.x += 4f;
            PhotonNetwork.Instantiate("Tower", spawnPos, Quaternion.identity);
            Debug.LogWarning("BuildingPlacement scripti bulunamadı, direkt spawn edildi.");
        }
    }
}