using System.Collections.Generic;
using UnityEngine;

public class YolNoktasiGrafi : MonoBehaviour
{
    [Header("Graf Ayarlarý")]
    [SerializeField] private float maksBaglantiMesafesi = 15f;
    [SerializeField] private LayerMask engeller; // Duvarlarýn bulunduðu 'Engel' katmanýný seç

    private List<YolNoktasi> tumNoktalar = new List<YolNoktasi>();

    private void Awake()
    {
        GrafOlustur();
    }

    public void GrafOlustur()
    {
        tumNoktalar = new List<YolNoktasi>(FindObjectsByType<YolNoktasi>(FindObjectsSortMode.None));

        foreach (var nokta in tumNoktalar)
            nokta.komsular.Clear();

        for (int i = 0; i < tumNoktalar.Count; i++)
        {
            for (int j = i + 1; j < tumNoktalar.Count; j++)
            {
                Vector3 a = tumNoktalar[i].transform.position + Vector3.up * 0.5f;
                Vector3 b = tumNoktalar[j].transform.position + Vector3.up * 0.5f;

                if (Vector3.Distance(a, b) <= maksBaglantiMesafesi)
                {
                    // Arada duvar (Engel) yoksa iki noktayý birbirine baðla
                    if (!Physics.Linecast(a, b, engeller, QueryTriggerInteraction.Ignore))
                    {
                        tumNoktalar[i].komsular.Add(tumNoktalar[j]);
                        tumNoktalar[j].komsular.Add(tumNoktalar[i]);
                    }
                }
            }
        }
    }

    public YolNoktasi EnYakinNokta(Vector3 konum)
    {
        YolNoktasi enYakin = null;
        float enKisa = Mathf.Infinity;

        foreach (var nokta in tumNoktalar)
        {
            float mesafe = Vector3.Distance(konum, nokta.transform.position);
            if (mesafe < enKisa)
            {
                enKisa = mesafe;
                enYakin = nokta;
            }
        }
        return enYakin;
    }

    public List<Vector3> YolBul(Vector3 baslangicKonum, Vector3 hedefKonum)
    {
        YolNoktasi baslangic = EnYakinNokta(baslangicKonum);
        YolNoktasi hedef = EnYakinNokta(hedefKonum);

        if (baslangic == null || hedef == null) return new List<Vector3>();

        var acikListe = new List<YolNoktasi> { baslangic };
        var kapaliListe = new HashSet<YolNoktasi>();
        var ebeveyn = new Dictionary<YolNoktasi, YolNoktasi>();
        var gMaliyet = new Dictionary<YolNoktasi, float>();
        var fMaliyet = new Dictionary<YolNoktasi, float>();

        foreach (var n in tumNoktalar)
        {
            gMaliyet[n] = Mathf.Infinity;
            fMaliyet[n] = Mathf.Infinity;
        }

        gMaliyet[baslangic] = 0f;
        fMaliyet[baslangic] = Vector3.Distance(baslangic.transform.position, hedef.transform.position);

        while (acikListe.Count > 0)
        {
            YolNoktasi mevcut = acikListe[0];
            for (int i = 1; i < acikListe.Count; i++)
            {
                if (fMaliyet[acikListe[i]] < fMaliyet[mevcut])
                    mevcut = acikListe[i];
            }

            if (mevcut == hedef)
                return YoluGeriSar(ebeveyn, baslangic, hedef, hedefKonum);

            acikListe.Remove(mevcut);
            kapaliListe.Add(mevcut);

            foreach (var komsu in mevcut.komsular)
            {
                if (kapaliListe.Contains(komsu)) continue;

                float yeniG = gMaliyet[mevcut] + Vector3.Distance(mevcut.transform.position, komsu.transform.position);
                if (yeniG < gMaliyet[komsu])
                {
                    ebeveyn[komsu] = mevcut;
                    gMaliyet[komsu] = yeniG;
                    fMaliyet[komsu] = yeniG + Vector3.Distance(komsu.transform.position, hedef.transform.position);

                    if (!acikListe.Contains(komsu))
                        acikListe.Add(komsu);
                }
            }
        }

        return new List<Vector3>();
    }

    private List<Vector3> YoluGeriSar(Dictionary<YolNoktasi, YolNoktasi> ebeveyn, YolNoktasi baslangic, YolNoktasi bitis, Vector3 gercekHedef)
    {
        List<Vector3> yol = new List<Vector3> { gercekHedef };
        YolNoktasi mevcut = bitis;

        while (mevcut != baslangic && ebeveyn.ContainsKey(mevcut))
        {
            yol.Add(mevcut.transform.position);
            mevcut = ebeveyn[mevcut];
        }
        yol.Add(baslangic.transform.position);
        yol.Reverse();
        return yol;
    }

    // Scene ekranýnda noktalar arasýndaki baðlantýlarý yeþil çizgiyle gösterir
    private void OnDrawGizmos()
    {
        if (tumNoktalar == null) return;

        Gizmos.color = Color.green;
        foreach (var nokta in tumNoktalar)
        {
            if (nokta == null) continue;
            foreach (var komsu in nokta.komsular)
            {
                if (komsu != null)
                    Gizmos.DrawLine(nokta.transform.position + Vector3.up * 0.2f, komsu.transform.position + Vector3.up * 0.2f);
            }
        }
    }
}