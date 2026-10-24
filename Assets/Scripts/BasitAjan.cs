using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class BasitAjan : MonoBehaviour
{
    [SerializeField] private Transform oyuncu;
    
    private NavMeshAgent agent;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();

        // Çizgi çizim ayarları (Mavi renk ve genişlik)
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.cyan;
    }

    private void Update()
    {
        if (oyuncu == null) return;

        // 1. En kısa yoldan hedefi takip et
        agent.SetDestination(oyuncu.position);

        // 2. Yürüdüğü rotayı mavi çizgi ile çiz
        RotayiCiz();
    }

    private void RotayiCiz()
    {
        // NavMesh'in hesapladığı köşe noktalarını (path corners) al
        NavMeshPath path = agent.path;

        if (path.corners.Length < 2)
            return;

        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            // Çizginin zemine gömülmemesi için hafif yukarı kaldırıyoruz (+Y)
            Vector3 nokta = path.corners[i] + Vector3.up * 0.1f;
            lineRenderer.SetPosition(i, nokta);
        }
    }
}