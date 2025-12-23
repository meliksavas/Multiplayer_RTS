using UnityEngine;
using Photon.Pun;
using UnityEngine.UI; // UI yazýlarýný güncellemek için

public class ResourceManager : MonoBehaviourPun
{
    public static ResourceManager MyInstance; // Kendi managerýmýza hýzlý ulaþmak için

    [Header("Mevcut Kaynaklar")]
    public int gold = 0;
    public int wood = 0;
    public int food = 0;
    
    [Header("UI Referanslarý")]
    // Canvas içindeki TextMeshPro objelerini buraya sürükle
    public Text goldText;
    public Text woodText;
    public Text foodText;

    private void Awake()
    {
        // Sadece kendi karakterimizinkini "MyInstance" yapýyoruz
        if (photonView.IsMine)
        {
            MyInstance = this;
            UpdateUI();
        }
    }

    // Kaynak Ekleme Fonksiyonu
    public void AddResource(string type, int amount)
    {
        if (!photonView.IsMine) return; // Sadece kendi kaynaklarýmýzý yönetiriz

        switch (type)
        {
            case "Gold":
                gold += amount;
                break;
            case "Wood":
                wood += amount;
                break;
            case "Food":
                food += amount;
                break;
        }
        UpdateUI();
    }

    // Kaynak Harcama (Bina yaparken vs kullanacaðýz)
    public bool SpendResource(string type, int amount)
    {
        switch (type)
        {
            case "Gold":
                if (gold >= amount) { gold -= amount; UpdateUI(); return true; }
                break;
            case "Wood":
                if (wood >= amount) { wood -= amount; UpdateUI(); return true; }
                break;
            case "Food":
                if (food >= amount) { food -= amount; UpdateUI(); return true; }
                break;
        }
        return false; // Yetersiz bakiye
    }

    void UpdateUI()
    {
        if (goldText) goldText.text = $"Gold: {gold}";
        if (woodText) woodText.text = $"Wood: {wood}";
        if (foodText) foodText.text = $"Food: {food}";
    }
}