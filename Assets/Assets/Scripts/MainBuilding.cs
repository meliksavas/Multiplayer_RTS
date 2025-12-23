using UnityEngine;
using Photon.Pun;

public class MainBuilding : MonoBehaviourPun
{
    [Header("Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;

    [Header("UI")]
    public GameObject healthBarPrefab;
    private HealthBar healthBarScript;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 4f, 0);
            GameObject barObj = Instantiate(healthBarPrefab, spawnPos, Quaternion.identity, transform);
            barObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            Canvas canvas = barObj.GetComponent<Canvas>();
            if (canvas != null) { canvas.overrideSorting = true; canvas.sortingOrder = 100; }

            healthBarScript = barObj.GetComponent<HealthBar>();
            healthBarScript.Initialize(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, amount);
    }

    [PunRPC]
    void RPC_TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Bina Hasar Aldý. Kalan Can: {currentHealth}");

        if (healthBarScript != null) healthBarScript.UpdateBar(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Bina Yýkýlýyor... Die() fonksiyonu çalýþtý.");

        if (photonView.IsMine)
        {
            if (RTSGameManager.Instance != null)
            {
                int ownerId = (photonView.Owner != null) ? photonView.Owner.ActorNumber : -1;
                RTSGameManager.Instance.OnMainBuildingDestroyed(ownerId);
            }
            else
            {
                Debug.LogError("CRITICAL ERROR: RTSGameManager.Instance NULL! Sahnede GameManager yok mu?");
            }

            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Debug.Log("Bu bina bana ait deðil, yok etme emrini sahibi verecek.");
        }
    }
}