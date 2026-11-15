using System.Collections.Generic;
using UnityEngine;

public class YolNoktasi : MonoBehaviour
{
    [HideInInspector] public List<YolNoktasi> komsular = new List<YolNoktasi>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.35f);
    }
}