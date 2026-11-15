using System.Collections.Generic;
using UnityEngine;

public class AYildizYolBulucu : MonoBehaviour
{
    [SerializeField] private IzgaraHarita harita;
    [SerializeField] private float egimCarpani = 10f; // Yokuþ çýkma cezasý
    [SerializeField] private float maksTirmanmaYuksekligi = 1.2f; // Çok dik uçurumlarý geçilmez say

    private class Dugum
    {
        public int x, z;
        public float gMaliyet;
        public float hMaliyet;
        public float fMaliyet => gMaliyet + hMaliyet;
        public Dugum ebeveyn;

        public Dugum(int x, int z) { this.x = x; this.z = z; }
    }

    public List<Vector3> YolBul(Vector3 baslangicDunya, Vector3 hedefDunya)
    {
        Vector2Int baslangic = harita.DunyadanIzgaraya(baslangicDunya);
        Vector2Int hedef = harita.DunyadanIzgaraya(hedefDunya);

        Dugum[,] dugumler = new Dugum[harita.GenislikX, harita.DerinlikZ];
        for (int x = 0; x < harita.GenislikX; x++)
            for (int z = 0; z < harita.DerinlikZ; z++)
                dugumler[x, z] = new Dugum(x, z);

        List<Dugum> acikListe = new List<Dugum>();
        HashSet<Dugum> kapaliListe = new HashSet<Dugum>();

        Dugum baslangicDugum = dugumler[baslangic.x, baslangic.y];
        Dugum hedefDugum = dugumler[hedef.x, hedef.y];
        acikListe.Add(baslangicDugum);

        while (acikListe.Count > 0)
        {
            Dugum mevcut = acikListe[0];
            for (int i = 1; i < acikListe.Count; i++)
            {
                if (acikListe[i].fMaliyet < mevcut.fMaliyet ||
                   (acikListe[i].fMaliyet == mevcut.fMaliyet && acikListe[i].hMaliyet < mevcut.hMaliyet))
                {
                    mevcut = acikListe[i];
                }
            }

            acikListe.Remove(mevcut);
            kapaliListe.Add(mevcut);

            if (mevcut == hedefDugum)
                return YoluGeriSar(baslangicDugum, hedefDugum);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;

                    int kontrolX = mevcut.x + dx;
                    int kontrolZ = mevcut.z + dz;

                    if (kontrolX >= 0 && kontrolX < harita.GenislikX && kontrolZ >= 0 && kontrolZ < harita.DerinlikZ)
                    {
                        if (!harita.yurunekbilir[kontrolX, kontrolZ] || kapaliListe.Contains(dugumler[kontrolX, kontrolZ]))
                            continue;

                        // Yükseklik farký (Eðim) kontrolü
                        float yukseklikFarki = Mathf.Abs(harita.yukseklikler[kontrolX, kontrolZ] - harita.yukseklikler[mevcut.x, mevcut.z]);
                        if (yukseklikFarki > maksTirmanmaYuksekligi)
                            continue;

                        if (dx != 0 && dz != 0)
                        {
                            if (!harita.yurunekbilir[mevcut.x + dx, mevcut.z] || !harita.yurunekbilir[mevcut.x, mevcut.z + dz])
                                continue;
                        }

                        Dugum komsu = dugumler[kontrolX, kontrolZ];
                        float tabanMaliyet = (dx == 0 || dz == 0) ? 1f : 1.414f;
                        float egimMaliyeti = yukseklikFarki * egimCarpani;
                        float yeniGMaliyet = mevcut.gMaliyet + tabanMaliyet + egimMaliyeti;

                        if (yeniGMaliyet < komsu.gMaliyet || !acikListe.Contains(komsu))
                        {
                            komsu.gMaliyet = yeniGMaliyet;
                            komsu.hMaliyet = Vector2Int.Distance(new Vector2Int(kontrolX, kontrolZ), hedef);
                            komsu.ebeveyn = mevcut;

                            if (!acikListe.Contains(komsu))
                                acikListe.Add(komsu);
                        }
                    }
                }
            }
        }

        return new List<Vector3>();
    }

    private List<Vector3> YoluGeriSar(Dugum baslangic, Dugum bitis)
    {
        List<Vector3> yol = new List<Vector3>();
        Dugum mevcut = bitis;

        while (mevcut != baslangic)
        {
            yol.Add(harita.IzgaradanDunyaya(mevcut.x, mevcut.z));
            mevcut = mevcut.ebeveyn;
        }
        yol.Reverse();
        return yol;
    }
}