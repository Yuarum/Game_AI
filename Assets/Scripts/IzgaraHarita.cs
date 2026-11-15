using UnityEngine;

public class IzgaraHarita : MonoBehaviour
{
    [Header("Izgara Ayarlarý")]
    [SerializeField] private int izgaraGenislikX = 20;
    [SerializeField] private int izgaraDerinlikZ = 20;
    [SerializeField] private float hucreBoyutu = 1f;
    [SerializeField] private LayerMask engeller;
    [SerializeField] private LayerMask zeminKatmani; // Zemin (Terrain/Plane) katmanýný seç

    public bool[,] yurunekbilir;
    public float[,] yukseklikler; // Hücrelerin Y yükseklik verisi
    public int GenislikX => izgaraGenislikX;
    public int DerinlikZ => izgaraDerinlikZ;

    private void Awake()
    {
        IzgaraOlustur();
    }

    public void IzgaraOlustur()
    {
        yurunekbilir = new bool[izgaraGenislikX, izgaraDerinlikZ];
        yukseklikler = new float[izgaraGenislikX, izgaraDerinlikZ];

        for (int x = 0; x < izgaraGenislikX; x++)
        {
            for (int z = 0; z < izgaraDerinlikZ; z++)
            {
                Vector3 dunyaNoktasi = IzgaradanDunyaya(x, z);

                // Yukarýdan aþaðýya ýþýn atarak zemin yüksekliðini bul
                if (Physics.Raycast(dunyaNoktasi + Vector3.up * 20f, Vector3.down, out RaycastHit hit, 50f, zeminKatmani))
                {
                    yukseklikler[x, z] = hit.point.y;
                    dunyaNoktasi.y = hit.point.y + 0.5f;
                }

                bool engelVar = Physics.CheckBox(
                    dunyaNoktasi,
                    Vector3.one * (hucreBoyutu * 0.45f),
                    Quaternion.identity,
                    engeller,
                    QueryTriggerInteraction.Ignore
                );
                yurunekbilir[x, z] = !engelVar;
            }
        }
    }

    public Vector3 IzgaradanDunyaya(int x, int z)
    {
        Vector3 solAlt = transform.position - new Vector3(izgaraGenislikX * hucreBoyutu / 2f, 0f, izgaraDerinlikZ * hucreBoyutu / 2f);
        float y = (yukseklikler != null) ? yukseklikler[x, z] + 0.5f : 0.5f;
        return solAlt + new Vector3(x * hucreBoyutu + hucreBoyutu / 2f, y, z * hucreBoyutu + hucreBoyutu / 2f);
    }

    public Vector2Int DunyadanIzgaraya(Vector3 dunyaKonumu)
    {
        Vector3 solAlt = transform.position - new Vector3(izgaraGenislikX * hucreBoyutu / 2f, 0f, izgaraDerinlikZ * hucreBoyutu / 2f);
        int x = Mathf.Clamp(Mathf.FloorToInt((dunyaKonumu.x - solAlt.x) / hucreBoyutu), 0, izgaraGenislikX - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt((dunyaKonumu.z - solAlt.z) / hucreBoyutu), 0, izgaraDerinlikZ - 1);
        return new Vector2Int(x, z);
    }

    private void OnDrawGizmos()
    {
        if (yurunekbilir == null) return;

        for (int x = 0; x < izgaraGenislikX; x++)
        {
            for (int z = 0; z < izgaraDerinlikZ; z++)
            {
                Gizmos.color = yurunekbilir[x, z] ? new Color(0f, 1f, 0f, 0.2f) : new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawCube(IzgaradanDunyaya(x, z), new Vector3(hucreBoyutu * 0.9f, 0.1f, hucreBoyutu * 0.9f));
            }
        }
    }
}