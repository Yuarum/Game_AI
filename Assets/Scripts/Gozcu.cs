using UnityEngine;

public class Gozcu : MonoBehaviour
{
    [Header("Gözcü Ayarları")]
    [SerializeField] private Transform oyuncu;
    [SerializeField] private float menzil = 15f;          // Maksimum görme mesafesi
    [SerializeField] private float gorusAcisi = 90f;      // Koninin toplam açısı (derece)
    [SerializeField] private float gozYuksekligi = 0.6f;  // Göz noktasının offset'i
    [SerializeField] private LayerMask engeller;          // Default: Arazi ve Ağaçlar (Player SEÇİLMEMELİ!)

    private Renderer govde;

    private void Awake()
    {
        govde = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (oyuncu == null) return;

        // Gözcünün göz konumu
        Vector3 gozKonumu = transform.position + Vector3.up * gozYuksekligi;
        Vector3 oyuncuKonumu = oyuncu.position;

        // 1. Görüş Konisinde mi? & 2. Görüş Hattı Açık mı?
        bool goruyor = GorusKonisinde(gozKonumu, oyuncuKonumu) && GorusHattiAcik(gozKonumu, oyuncuKonumu);

        // Geri Bildirim: Görüyorsa Kırmızı, Görmüyorsa Yeşil
        if (govde != null && govde.material != null)
        {
            govde.material.color = goruyor ? Color.red : Color.green;
        }

        // Görüyorsa oyuncunun sağda mı solunda mı olduğunu konsola yazdır
        if (goruyor)
        {
            SagSolBilgisiYazdir(gozKonumu, oyuncuKonumu);
        }
    }

    /// <summary>
    /// Nokta çarpımı (Dot Product) kullanarak oyuncunun koni içinde olup olmadığını hesaplar.
    /// </summary>
    private bool GorusKonisinde(Vector3 goz, Vector3 hedef)
    {
        Vector3 hedefeYon = hedef - goz;
        float mesafe = hedefeYon.magnitude;

        // Menzil kontrolü
        if (mesafe > menzil) return false;

        // Yön vektörünü normalleştir
        Vector3 hedefeYonNorm = hedefeYon.normalized;
        Vector3 bakisYonu = transform.forward;

        // Nokta Çarpım (Dot Product) hesabı: cos(theta)
        float dot = Vector3.Dot(bakisYonu, hedefeYonNorm);

        // Görüş açısının yarısının kosinüsü
        float yarimAciRadyan = (gorusAcisi * 0.5f) * Mathf.Deg2Rad;
        float esikCos = Mathf.Cos(yarimAciRadyan);

        // Dot >= esikCos ise oyuncu görüş açısı içerisindedir
        return dot >= esikCos;
    }

    /// <summary>
    /// Raycast/Linecast atarak göz ile oyuncu arasında engel olup olmadığını kontrol eder.
    /// </summary>
    private bool GorusHattiAcik(Vector3 goz, Vector3 hedef)
    {
        // Gözden hedefe doğru bir çizgi çekilir.
        // Eğer arada 'engeller' katmanına dahil bir nesne varsa false döner.
        if (Physics.Linecast(goz, hedef, out RaycastHit hit, engeller, QueryTriggerInteraction.Ignore))
        {
            // Araya engel (arazi/ağaç) girdi, oyuncu görünmüyor
            return false;
        }

        return true; // Görüş hattı temiz
    }

    /// <summary>
    /// Çapraz çarpım (Cross Product) kullanarak oyuncunun sağda mı solunda mı olduğunu tespit eder.
    /// </summary>
    private void SagSolBilgisiYazdir(Vector3 goz, Vector3 hedef)
    {
        Vector3 hedefeYon = (hedef - goz).normalized;
        
        // Sol el kuralına göre Çapraz Çarpımın Y değeri: + ise Sağda, - ise Solda
        float caprazY = Vector3.Cross(transform.forward, hedefeYon).y;

        if (caprazY > 0)
        {
            Debug.Log("Oyuncu Gözcünün SAĞINDA!");
        }
        else if (caprazY < 0)
        {
            Debug.Log("Oyuncu Gözcünün SOLUNDA!");
        }
    }

    /// <summary>
    /// Scene penceresinde görüş alanını (menzil ve koni) çizer.
    /// </summary>
    private void OnDrawGizmos()
    {
        Vector3 goz = transform.position + Vector3.up * gozYuksekligi;

        // Menzil Küresi
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(goz, menzil);

        // Görüş Konisi Kenarları
        Gizmos.color = Color.blue;
        float yarimAci = gorusAcisi * 0.5f;

        Vector3 solKenar = Quaternion.AngleAxis(-yarimAci, Vector3.up) * transform.forward;
        Vector3 sagKenar = Quaternion.AngleAxis(yarimAci, Vector3.up) * transform.forward;

        Gizmos.DrawRay(goz, solKenar * menzil);
        Gizmos.DrawRay(goz, sagKenar * menzil);
    }
}