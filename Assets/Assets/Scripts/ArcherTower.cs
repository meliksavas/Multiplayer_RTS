using UnityEngine;
using Photon.Pun;
using System.Collections;

public class ArcherTower : MonoBehaviourPun
{
    [Header("Stats")]
    public float maxHealth = 500f;
    public float currentHealth;
    public float attackRange = 6f; // Saldýrý menzili
    public float damage = 15f;     // Vuruþ hasarý
    public float attackSpeed = 1.5f; // Saniye baþýna saldýrý hýzý

    [Header("Görsel & Referanslar")]
    public GameObject healthBarPrefab; // Can barý prefabýný buraya sürükle
    public float attackDelay = 0.3f;   // Animasyon baþladýktan kaç sn sonra hasar iþlesin? (Ok yaydan çýkma aný)

    private Animator archerAnimator; // Child objedeki archer'ýn animatörü
    private HealthBar healthBarScript;
    private float nextAttackTime = 0f;
    private Transform currentTarget;

    void Start()
    {
        currentHealth = maxHealth;

        // 1. Child objelerdeki (tepedeki Archer'ýn) Animator'ünü bul
        archerAnimator = GetComponentInChildren<Animator>();

        // 2. HealthBar Oluþtur (MainBuilding ile ayný mantýk)
        if (healthBarPrefab != null)
        {
            // Barý kulenin biraz tepesine koy
            Vector3 spawnPos = transform.position + new Vector3(0, 3.5f, 0);
            GameObject barObj = Instantiate(healthBarPrefab, spawnPos, Quaternion.identity, transform);

            barObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // Görünürlük ayarý
            Canvas canvas = barObj.GetComponent<Canvas>();
            if (canvas != null) { canvas.overrideSorting = true; canvas.sortingOrder = 100; }

            healthBarScript = barObj.GetComponent<HealthBar>();
            healthBarScript.Initialize(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        // Sadece binanýn sahibi (kuran kiþi) saldýrý mantýðýný çalýþtýrýr
        if (!photonView.IsMine) return;

        if (currentTarget == null)
        {
            FindTarget();
        }
        else
        {
            // Hedef menzilden çýktý mý veya öldü mü?
            if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > attackRange)
            {
                currentTarget = null;
                return;
            }

            // Saldýrý zamaný geldiyse
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
                nextAttackTime = Time.time + 1f / attackSpeed;
            }
        }
    }

    void FindTarget()
    {
        // Menzil içindeki her þeyi tara
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            // Önce Asker var mý?
            UnitController enemy = hit.GetComponent<UnitController>();
            if (enemy != null && !enemy.photonView.IsMine)
            {
                currentTarget = enemy.transform;
                return;
            }
            // Asker yoksa düþman binasý var mý? (Gerekirse açarsýn)
            /*
            MainBuilding enemyBuilding = hit.GetComponent<MainBuilding>();
            if (enemyBuilding != null && !enemyBuilding.photonView.IsMine)
            {
                currentTarget = enemyBuilding.transform;
                return;
            }
            */
        }
    }

    IEnumerator AttackRoutine()
    {
        // 1. Animasyonu Baþlat (Tüm aðda görünür)
        photonView.RPC("RPC_PlayTowerAnim", RpcTarget.All);

        // 2. Okun "vurma anýna" kadar bekle (Görsel senkronizasyon için)
        yield return new WaitForSeconds(attackDelay);

        if (currentTarget == null) yield break;

        // 3. Hasarý Ver (Projectile yok, direkt can yakma)
        UnitController unit = currentTarget.GetComponent<UnitController>();
        if (unit != null)
        {
            unit.TakeDamage(damage);
        }
    }

    // --- RPC'ler (Að Senkronizasyonu) ---

    [PunRPC]
    void RPC_PlayTowerAnim()
    {
        // Tepedeki Archer'ýn "Attack" triggerýný tetikle
        if (archerAnimator != null)
        {
            archerAnimator.SetTrigger("Attack");
        }
    }

    // --- Hasar Alma (Kuleye saldýrýlýrsa) ---
    public void TakeDamage(float amount)
    {
        photonView.RPC("RPC_TowerTakeDamage", RpcTarget.All, amount);
    }

    [PunRPC]
    void RPC_TowerTakeDamage(float amount)
    {
        currentHealth -= amount;
        if (healthBarScript != null) healthBarScript.UpdateBar(currentHealth);

        if (currentHealth <= 0 && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // Editörde menzili görmek için kýrmýzý çember çiz
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}