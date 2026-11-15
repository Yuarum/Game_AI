using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZiplamaBaglantisi : MonoBehaviour
{
    [SerializeField] private float ziplamaSuresi = 0.6f;
    [SerializeField] private float ziplamaYuksekligi = 1.5f;

    private NavMeshAgent agent;
    private bool zipliyorMu = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Ajan bir NavMeshLink noktasýna ulaþtýysa zýplamayý baþlat
        if (agent.isOnOffMeshLink && !zipliyorMu)
        {
            StartCoroutine(ParabolikZipla());
        }
    }

    private IEnumerator ParabolikZipla()
    {
        zipliyorMu = true;

        OffMeshLinkData veri = agent.currentOffMeshLinkData;
        Vector3 baslangic = transform.position;
        Vector3 bitis = veri.endPos + Vector3.up * agent.baseOffset;

        float gecenSure = 0f;

        while (gecenSure < ziplamaSuresi)
        {
            float t = gecenSure / ziplamaSuresi;
            // Yatayda düz ilerlerken dikeyde sinüs eðrisiyle parabol çizer
            Vector3 yatayKonum = Vector3.Lerp(baslangic, bitis, t);
            float yayYuksekligi = Mathf.Sin(t * Mathf.PI) * ziplamaYuksekligi;

            transform.position = yatayKonum + Vector3.up * yayYuksekligi;

            gecenSure += Time.deltaTime;
            yield return null;
        }

        transform.position = bitis;
        agent.CompleteOffMeshLink(); // NavMesh'e geçiþin bittiðini bildir
        zipliyorMu = false;
    }
}