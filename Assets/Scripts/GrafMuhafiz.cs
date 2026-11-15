using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrafMuhafiz : MonoBehaviour
{
    private enum Durum { Devriye, Kovalama, GeriDonus }

    [Header("Baðlantýlar")]
    [SerializeField] private YolNoktasiGrafi graf;
    [SerializeField] private Transform oyuncu;
    [SerializeField] private YolNoktasi[] devriyeRotasi;

    [Header("Devriye Tipi")]
    [SerializeField] private bool pingPongDevriye = true; // Ýþaretliyse gidip-gelir, deðilse döngüsel gezer

    [Header("Hareket ve Görüþ Ayarlarý")]
    [SerializeField] private float devriyeHizi = 3f;
    [SerializeField] private float kovalamaHizi = 5.5f;
    [SerializeField] private float donusHizi = 720f;
    [SerializeField] private float menzil = 15f;
    [SerializeField] private float gorusAcisi = 90f;
    [SerializeField] private float gozYuksekligi = 0.6f;
    [SerializeField] private float kayipSuresi = 3f;
    [SerializeField] private LayerMask engeller;

    private Durum durum = Durum.Devriye;
    private Renderer govde;
    private LineRenderer lineRenderer;

    private int devriyeIndex = 0;
    private int devriyeYonu = 1; // 1: ileri, -1: geri
    private List<Vector3> aktifYol = new List<Vector3>();
    private int yolIndex = 0;
    private float gorunmemeSayaci = 0f;
    private float yolYenilemeSayaci = 0f;

    private void Awake()
    {
        govde = GetComponent<Renderer>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.cyan;
    }

    private void Update()
    {
        if (oyuncu == null || graf == null) return;

        Vector3 goz = transform.position + Vector3.up * gozYuksekligi;
        bool goruyor = GorusKonisinde(goz, oyuncu.position) && GorusHattiAcik(goz, oyuncu.position);

        if (goruyor)
        {
            durum = Durum.Kovalama;
            gorunmemeSayaci = 0f;
        }
        else if (durum == Durum.Kovalama)
        {
            gorunmemeSayaci += Time.deltaTime;
            if (gorunmemeSayaci >= kayipSuresi)
            {
                durum = Durum.GeriDonus;
                if (devriyeRotasi != null && devriyeRotasi.Length > 0)
                {
                    aktifYol = graf.YolBul(transform.position, devriyeRotasi[devriyeIndex].transform.position);
                    yolIndex = 0;
                }
            }
        }

        switch (durum)
        {
            case Durum.Devriye:
                if (govde != null) govde.material.color = Color.green;
                DevriyeMantik();
                break;

            case Durum.Kovalama:
                if (govde != null) govde.material.color = Color.red;
                KovalamaMantik(goruyor);
                break;

            case Durum.GeriDonus:
                if (govde != null) govde.material.color = Color.yellow;
                GeriDonusMantik();
                break;
        }

        RotayiCiz();
    }

    private void DevriyeMantik()
    {
        if (devriyeRotasi == null || devriyeRotasi.Length == 0) return;

        Vector3 hedef = devriyeRotasi[devriyeIndex].transform.position;
        aktifYol = new List<Vector3> { hedef };
        yolIndex = 0;

        NoktayaIlerle(hedef, devriyeHizi);

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(hedef.x, 0, hedef.z)) < 0.25f)
        {
            if (pingPongDevriye && devriyeRotasi.Length > 1)
            {
                if (devriyeIndex + devriyeYonu >= devriyeRotasi.Length || devriyeIndex + devriyeYonu < 0)
                {
                    devriyeYonu *= -1;
                }
                devriyeIndex += devriyeYonu;
            }
            else
            {
                devriyeIndex = (devriyeIndex + 1) % devriyeRotasi.Length;
            }
        }
    }

    private void KovalamaMantik(bool dogrudanGoruyor)
    {
        if (dogrudanGoruyor)
        {
            aktifYol = new List<Vector3> { oyuncu.position };
            yolIndex = 0;
            NoktayaIlerle(oyuncu.position, kovalamaHizi);
        }
        else
        {
            yolYenilemeSayaci += Time.deltaTime;
            if (yolYenilemeSayaci >= 0.4f || aktifYol.Count == 0)
            {
                yolYenilemeSayaci = 0f;
                aktifYol = graf.YolBul(transform.position, oyuncu.position);
                yolIndex = 0;
            }
            YolListesiniTakipEt(kovalamaHizi);
        }
    }

    private void GeriDonusMantik()
    {
        if (aktifYol == null || yolIndex >= aktifYol.Count)
        {
            durum = Durum.Devriye;
            return;
        }
        YolListesiniTakipEt(devriyeHizi);
    }

    private void YolListesiniTakipEt(float hiz)
    {
        if (aktifYol == null || yolIndex >= aktifYol.Count) return;

        Vector3 hedef = aktifYol[yolIndex];
        NoktayaIlerle(hedef, hiz);

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(hedef.x, 0, hedef.z)) < 0.3f)
        {
            yolIndex++;
        }
    }

    private void NoktayaIlerle(Vector3 hedef, float hiz)
    {
        hedef.y = transform.position.y;
        Vector3 yon = (hedef - transform.position).normalized;

        if (yon.sqrMagnitude > 0.001f)
        {
            Quaternion hedefDonus = Quaternion.LookRotation(yon);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, hedefDonus, donusHizi * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, hedef, hiz * Time.deltaTime);
    }

    private bool GorusKonisinde(Vector3 goz, Vector3 hedef)
    {
        Vector3 hedefeYon = hedef - goz;
        if (hedefeYon.magnitude > menzil) return false;

        float dot = Vector3.Dot(transform.forward, hedefeYon.normalized);
        float esikCos = Mathf.Cos((gorusAcisi * 0.5f) * Mathf.Deg2Rad);
        return dot >= esikCos;
    }

    private bool GorusHattiAcik(Vector3 goz, Vector3 hedef)
    {
        return !Physics.Linecast(goz, hedef, engeller, QueryTriggerInteraction.Ignore);
    }

    private void RotayiCiz()
    {
        if (aktifYol == null || yolIndex >= aktifYol.Count)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        int kalan = (aktifYol.Count - yolIndex) + 1;
        lineRenderer.positionCount = kalan;
        lineRenderer.SetPosition(0, transform.position + Vector3.up * 0.1f);

        for (int i = yolIndex; i < aktifYol.Count; i++)
        {
            Vector3 nokta = new Vector3(aktifYol[i].x, transform.position.y + 0.1f, aktifYol[i].z);
            lineRenderer.SetPosition((i - yolIndex) + 1, nokta);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 goz = transform.position + Vector3.up * gozYuksekligi;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(goz, menzil);

        float yarimAci = gorusAcisi * 0.5f;
        Vector3 solKenar = Quaternion.AngleAxis(-yarimAci, Vector3.up) * transform.forward;
        Vector3 sagKenar = Quaternion.AngleAxis(yarimAci, Vector3.up) * transform.forward;
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(goz, solKenar * menzil);
        Gizmos.DrawRay(goz, sagKenar * menzil);
    }
}