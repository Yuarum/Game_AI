using UnityEngine;
using UnityEngine.AI; // NavMesh kütüphanesi

public class GozcuNavMesh : MonoBehaviour
{
    [SerializeField] private Transform oyuncu;
    [SerializeField] private float menzil = 15f;
    [SerializeField] private float gorusAcisi = 90f;
    [SerializeField] private float gozYuksekligi = 0.6f;
    [SerializeField] private LayerMask engeller;

    private NavMeshAgent agent;
    private Renderer govde;
    private Vector3 sonGorulenKonum;
    private bool oyuncuGorunuyor;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        govde = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (oyuncu == null) return;

        Vector3 goz = transform.position + Vector3.up * gozYuksekligi;
        bool suAnGoruyor = GorusKonisinde(goz, oyuncu.position) && GorusHattiAcik(goz, oyuncu.position);

        if (suAnGoruyor)
        {
            oyuncuGorunuyor = true;
            sonGorulenKonum = oyuncu.position;
            govde.material.color = Color.red;

            // NavMeshAgent ile doğrudan oyuncuya git
            agent.SetDestination(oyuncu.position);
        }
        else
        {
            if (oyuncuGorunuyor)
            {
                // Görüş kaybedildi! Tepenin/duvarın arkasındaki en son noktaya NavMesh ile git
                oyuncuGorunuyor = false;
                govde.material.color = Color.yellow;
                
                agent.SetDestination(sonGorulenKonum);
            }

            // Hedefe ulaştı mı kontrolü
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                govde.material.color = Color.green;
            }
        }
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
}