using UnityEngine;
using Photon.Pun;

public class ResourceSource : MonoBehaviourPun
{
    public enum ResourceType { Wood, Food }

    [Header("Ayarlar")]
    public ResourceType resourceType;
    public int amountLeft = 500;
    private int maxAmount;

    [Header("UI")]
    public GameObject healthBarPrefab;
    private HealthBar healthBarScript;

    void Start()
    {
        maxAmount = amountLeft;

        if (healthBarPrefab != null)
        {
            // Barý oluþtur
            // Aðaçlarýn boyu genelde uzun olduðu için Y ekseninde biraz daha yukarý (2.5f) alýyoruz.
            Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
            GameObject barObj = Instantiate(healthBarPrefab, spawnPos, Quaternion.identity, transform);

            // --- DÜZELTME 1: ÖLÇEK SABÝTLEME ---
            // Ana obje (Aðaç) büyütülmüþ olsa bile barýn boyutu sabit kalsýn.
            // Eðer bar çok küçük veya büyük görünürse buradaki 0.01f deðerlerini deðiþtir.
            barObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // --- DÜZELTME 2: GÖRÜNÜRLÜK GARANTÝSÝ ---
            Canvas canvas = barObj.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 100; // 100 yaparak kesinlikle en öne alýyoruz.
            }

            healthBarScript = barObj.GetComponent<HealthBar>();
            if (healthBarScript != null)
            {
                healthBarScript.Initialize(amountLeft, maxAmount);
            }
        }
        else
        {
            // Eðer bu hatayý görüyorsan: Resources klasöründeki Tree/Sheep prefabýna git ve kutuyu doldur.
            Debug.LogError($"HATA: {gameObject.name} prefabýnda 'Health Bar Prefab' kutusu boþ!");
        }
    }

    public int Harvest(int damageOrAmount)
    {
        if (amountLeft <= 0) return 0;

        int amountToGive = Mathf.Min(damageOrAmount, amountLeft);
        photonView.RPC("RPC_DecreaseAmount", RpcTarget.All, amountToGive);
        return amountToGive;
    }

    [PunRPC]
    void RPC_DecreaseAmount(int amount)
    {
        amountLeft -= amount;

        if (healthBarScript != null)
        {
            healthBarScript.UpdateBar(amountLeft);
        }

        if (amountLeft <= 0)
        {
            if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
    }
}