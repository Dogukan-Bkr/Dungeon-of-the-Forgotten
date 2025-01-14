using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli kütüphane.

// Çýkýþ kapýsý kontrolü için kullanýlan sýnýf.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))] // Gerekli bileþenleri ekler.
public class ExitDoorwayController : MonoBehaviour
{
    // Obje ilk oluþturulduðunda çaðrýlan metot.
    void Reset()
    {
        // Rigidbody2D bileþenini kinematik olarak ayarlar.
        GetComponent<Rigidbody2D>().isKinematic = true;

        // BoxCollider2D bileþeninin boyutunu ve tetikleyici durumunu ayarlar.
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.size = Vector2.one * 0.1f; // Çarpýþma kutusunun boyutu çok küçük olacak þekilde ayarlanýr.
        box.isTrigger = true; // Çarpýþma yerine tetikleme yapmasýný saðlar.
    }

    // Oyuncu kapýya dokunduðunda tetiklenen metot.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Eðer çarpýþan obje "Player" etiketi taþýyorsa:
        if (collision.tag == "Player")
        {
            // Mevcut sahne yeniden yüklenir.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
