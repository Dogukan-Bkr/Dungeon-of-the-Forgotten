using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne y�netimi i�in gerekli k�t�phane.

// ��k�� kap�s� kontrol� i�in kullan�lan s�n�f.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))] // Gerekli bile�enleri ekler.
public class ExitDoorwayController : MonoBehaviour
{
    // Obje ilk olu�turuldu�unda �a�r�lan metot.
    void Reset()
    {
        // Rigidbody2D bile�enini kinematik olarak ayarlar.
        GetComponent<Rigidbody2D>().isKinematic = true;

        // BoxCollider2D bile�eninin boyutunu ve tetikleyici durumunu ayarlar.
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.size = Vector2.one * 0.1f; // �arp��ma kutusunun boyutu �ok k���k olacak �ekilde ayarlan�r.
        box.isTrigger = true; // �arp��ma yerine tetikleme yapmas�n� sa�lar.
    }

    // Oyuncu kap�ya dokundu�unda tetiklenen metot.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // E�er �arp��an obje "Player" etiketi ta��yorsa:
        if (collision.tag == "Player")
        {
            // Mevcut sahne yeniden y�klenir.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
