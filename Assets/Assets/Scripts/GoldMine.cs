using UnityEngine;
using Photon.Pun;

public class GoldMine : MonoBehaviourPun
{
    [Header("Ayarlar")]
    public int goldAmount = 10; // Her seferinde kaç altýn versin
    public float incomeInterval = 5.0f; // Kaç saniyede bir versin

    private float timer;

    void Update()
    {
        // Sadece binanýn sahibi olan oyuncuda çalýþýr
        if (!photonView.IsMine) return;

        timer += Time.deltaTime;

        if (timer >= incomeInterval)
        {
            // Oyuncunun kasasýna para ekle
            if (ResourceManager.MyInstance != null)
            {
                ResourceManager.MyInstance.AddResource("Gold", goldAmount);
                // Efekt veya ses eklenebilir
                Debug.Log($"Pasif Gelir: +{goldAmount} Altýn");
            }
            timer = 0;
        }
    }
}