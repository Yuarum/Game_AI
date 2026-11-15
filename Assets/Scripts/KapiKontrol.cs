using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshObstacle))]
public class KapiKontrol : MonoBehaviour
{
    [SerializeField] private float acikYukseklik = 3.5f;
    [SerializeField] private float hiz = 5f;

    private NavMeshObstacle engel;
    private Vector3 kapaliKonum;
    private Vector3 acikKonum;
    private bool acikMi = false;

    private void Awake()
    {
        engel = GetComponent<NavMeshObstacle>();
        // Kapý kapalýyken NavMesh üzerinde delik açmasýný saðlar
        engel.carving = true;

        kapaliKonum = transform.position;
        acikKonum = kapaliKonum + Vector3.up * acikYukseklik;
    }

    private void Update()
    {
        // E tuþuna basýldýðýnda kapýyý aç/kapat
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            acikMi = !acikMi;
        }

        Vector3 hedef = acikMi ? acikKonum : kapaliKonum;
        transform.position = Vector3.MoveTowards(transform.position, hedef, hiz * Time.deltaTime);

        // Kapý tamamen açýldýðýnda NavMesh engelini devre dýþý býrak
        engel.enabled = !acikMi;
    }
}