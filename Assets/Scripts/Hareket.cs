using UnityEngine;

public static class Hareket
{
    public static Vector3 IstenenYon(Vector2 girdi, Transform kamera)
    {
        Vector3 ileri = kamera.forward; ileri.y = 0f; ileri.Normalize();
        Vector3 sag   = kamera.right;   sag.y   = 0f; sag.Normalize();
        Vector3 yon = ileri * girdi.y + sag * girdi.x;
        return Vector3.ClampMagnitude(yon, 1f);
    }
}