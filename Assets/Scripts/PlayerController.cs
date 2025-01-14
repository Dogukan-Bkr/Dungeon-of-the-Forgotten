using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerController sýnýfý, oyuncu karakterinin hareketini kontrol eder.
public class PlayerController : MonoBehaviour
{
    // Oyuncunun hareket hýzý.
    public float speed;

    // Engel ve çarpýþma tespiti için kullanýlan maskeler.
    LayerMask obstacleMask;
    Vector2 targetPos; // Oyuncunun gitmek istediði hedef pozisyon.
    Transform GFX; // Oyuncunun görsel bileþenini (Sprite) kontrol eder.
    float flipX; // Sprite'ýn yatay eksende dönme yönü.
    bool isMoving; // Oyuncunun hareket edip etmediðini kontrol eder.

    // Oyuncunun sahne yüklendiðinde çaðrýlan baþlangýç metodu.
    void Start()
    {
        // Engel ve düþman katmanlarý belirlenir.
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");

        // SpriteRenderer'ýn transformu alýnýr.
        GFX = GetComponentInChildren<SpriteRenderer>().transform;

        // Baþlangýç yatay ölçek deðeri kaydedilir.
        flipX = GFX.localScale.x;
    }

    // Her karede çaðrýlan metot.
    void Update()
    {
        // Oyuncu hareketini kontrol eder.
        Move();
    }

    // Oyuncunun hareketini saðlayan metot.
    void Move()
    {
        // Klavye girdilerine göre yatay ve dikey hareket yönlerini belirler.
        float horz = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
        float vert = System.Math.Sign(Input.GetAxisRaw("Vertical"));

        // Eðer herhangi bir yönde hareket girdisi varsa:
        if (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0)
        {
            // Yatay hareket varsa, Sprite'ýn yönü ayarlanýr.
            if (Mathf.Abs(horz) > 0)
            {
                GFX.localScale = new Vector2(flipX * horz, GFX.localScale.y);
            }

            // Oyuncu þu an hareket etmiyorsa hedef pozisyon belirlenir.
            if (!isMoving)
            {
                // Yatay veya dikey yönde hedef pozisyon belirleme.
                if (Mathf.Abs(horz) > 0)
                {
                    targetPos = new Vector2(transform.position.x + horz, transform.position.y);
                }
                else if (Mathf.Abs(vert) > 0)
                {
                    targetPos = new Vector2(transform.position.x, transform.position.y + vert);
                }

                // Hedef pozisyonda engel olup olmadýðýný kontrol eder.
                Vector2 hitSize = Vector2.one * 0.8f; // Çarpýþma kutusunun boyutu.
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask);

                // Eðer hedef pozisyonda engel yoksa hareket baþlatýlýr.
                if (!hit)
                {
                    StartCoroutine(SmoothMove());
                }
            }
        }
    }

    // Oyuncunun hedef pozisyona pürüzsüz hareketini saðlayan Coroutine.
    IEnumerator SmoothMove()
    {
        isMoving = true; // Hareket durumu aktif edilir.

        // Hedef pozisyona ulaþýlana kadar pozisyon güncellenir.
        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null; // Bir sonraki kareye kadar bekler.
        }

        // Hedef pozisyona ulaþýldýðýnda pozisyon kesin olarak ayarlanýr.
        transform.position = targetPos;
        isMoving = false; // Hareket durumu kapatýlýr.
    }
}
