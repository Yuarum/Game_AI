using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class IzgaraAjan : MonoBehaviour
{
    [SerializeField] private AYildizYolBulucu yolBulucu;
    [SerializeField] private Transform oyuncu;
    [SerializeField] private float hareketHizi = 4f;
    [SerializeField] private float donusHizi = 720f;
    [SerializeField] private float yolYenilemeSuresi = 0.5f;

    private LineRenderer lineRenderer;
    private List<Vector3> mevcutYol = new List<Vector3>();
    private int hedefIndex = 0;
    private float yenilemeSayaci = 0f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // BasitAjan.cs ile birebir ayný çizgi ayarlarý
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.cyan;
    }

    private void Update()
    {
        if (oyuncu == null || yolBulucu == null) return;

        yenilemeSayaci += Time.deltaTime;
        if (yenilemeSayaci >= yolYenilemeSuresi)
        {
            yenilemeSayaci = 0f;
            mevcutYol = yolBulucu.YolBul(transform.position, oyuncu.position);
            hedefIndex = 0;
        }

        YoluTakipEt();
        RotayiCiz();
    }

    private void YoluTakipEt()
    {
        if (mevcutYol == null || hedefIndex >= mevcutYol.Count) return;

        Vector3 hedefNokta = mevcutYol[hedefIndex];
        hedefNokta.y = transform.position.y;

        Vector3 yon = (hedefNokta - transform.position).normalized;
        if (yon.sqrMagnitude > 0.001f)
        {
            Quaternion hedefDonus = Quaternion.LookRotation(yon);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, hedefDonus, donusHizi * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, hedefNokta, hareketHizi * Time.deltaTime);

        if (Vector3.Distance(transform.position, hedefNokta) < 0.1f)
        {
            hedefIndex++;
        }
    }

    private void RotayiCiz()
    {
        if (mevcutYol == null || hedefIndex >= mevcutYol.Count)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        int kalanNoktaSayisi = (mevcutYol.Count - hedefIndex) + 1;
        lineRenderer.positionCount = kalanNoktaSayisi;
        lineRenderer.SetPosition(0, transform.position + Vector3.up * 0.1f);

        for (int i = hedefIndex; i < mevcutYol.Count; i++)
        {
            Vector3 nokta = new Vector3(mevcutYol[i].x, transform.position.y + 0.1f, mevcutYol[i].z);
            lineRenderer.SetPosition((i - hedefIndex) + 1, nokta);
        }
    }
}