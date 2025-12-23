using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public Image fillImage; // Yeþil dolan kýsým
    public GameObject uiCanvas; // Barýn tamamý (gizlemek istersen diye)

    private Camera cam;
    private float maxHealth;

    void Start()
    {
        cam = Camera.main;
    }

    public void Initialize(float currentHealth, float _maxHealth)
    {
        maxHealth = _maxHealth;
        UpdateBar(currentHealth);
    }

    public void UpdateBar(float currentHealth)
    {
        if (fillImage != null)
        {
            // Can yüzdesini hesapla (0 ile 1 arasý)
            fillImage.fillAmount = currentHealth / maxHealth;
        }

        // Opsiyonel: Can full ise gizle, hasar alýnca göster
        // if (currentHealth >= maxHealth) uiCanvas.SetActive(false);
        // else uiCanvas.SetActive(true);
    }

    // Barýn her zaman kameraya düz bakmasýný ve dönmemesini saðlar
    void LateUpdate()
    {
        // Barýn rotasyonunu sýfýrla ki birim dönse bile bar dönmesin
        transform.rotation = Quaternion.identity;

        // Veya kameraya baktýr (3D projeler için):
        // transform.LookAt(transform.position + cam.transform.forward);
    }
}