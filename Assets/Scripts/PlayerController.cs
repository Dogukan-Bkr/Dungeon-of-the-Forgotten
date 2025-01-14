using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerController s�n�f�, oyuncu karakterinin hareketini kontrol eder.
public class PlayerController : MonoBehaviour
{
    // Oyuncunun hareket h�z�.
    public float speed;

    // Engel ve �arp��ma tespiti i�in kullan�lan maskeler.
    LayerMask obstacleMask;
    Vector2 targetPos; // Oyuncunun gitmek istedi�i hedef pozisyon.
    Transform GFX; // Oyuncunun g�rsel bile�enini (Sprite) kontrol eder.
    float flipX; // Sprite'�n yatay eksende d�nme y�n�.
    bool isMoving; // Oyuncunun hareket edip etmedi�ini kontrol eder.

    // Oyuncunun sahne y�klendi�inde �a�r�lan ba�lang�� metodu.
    void Start()
    {
        // Engel ve d��man katmanlar� belirlenir.
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");

        // SpriteRenderer'�n transformu al�n�r.
        GFX = GetComponentInChildren<SpriteRenderer>().transform;

        // Ba�lang�� yatay �l�ek de�eri kaydedilir.
        flipX = GFX.localScale.x;
    }

    // Her karede �a�r�lan metot.
    void Update()
    {
        // Oyuncu hareketini kontrol eder.
        Move();
    }

    // Oyuncunun hareketini sa�layan metot.
    void Move()
    {
        // Klavye girdilerine g�re yatay ve dikey hareket y�nlerini belirler.
        float horz = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
        float vert = System.Math.Sign(Input.GetAxisRaw("Vertical"));

        // E�er herhangi bir y�nde hareket girdisi varsa:
        if (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0)
        {
            // Yatay hareket varsa, Sprite'�n y�n� ayarlan�r.
            if (Mathf.Abs(horz) > 0)
            {
                GFX.localScale = new Vector2(flipX * horz, GFX.localScale.y);
            }

            // Oyuncu �u an hareket etmiyorsa hedef pozisyon belirlenir.
            if (!isMoving)
            {
                // Yatay veya dikey y�nde hedef pozisyon belirleme.
                if (Mathf.Abs(horz) > 0)
                {
                    targetPos = new Vector2(transform.position.x + horz, transform.position.y);
                }
                else if (Mathf.Abs(vert) > 0)
                {
                    targetPos = new Vector2(transform.position.x, transform.position.y + vert);
                }

                // Hedef pozisyonda engel olup olmad���n� kontrol eder.
                Vector2 hitSize = Vector2.one * 0.8f; // �arp��ma kutusunun boyutu.
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask);

                // E�er hedef pozisyonda engel yoksa hareket ba�lat�l�r.
                if (!hit)
                {
                    StartCoroutine(SmoothMove());
                }
            }
        }
    }

    // Oyuncunun hedef pozisyona p�r�zs�z hareketini sa�layan Coroutine.
    IEnumerator SmoothMove()
    {
        isMoving = true; // Hareket durumu aktif edilir.

        // Hedef pozisyona ula��lana kadar pozisyon g�ncellenir.
        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null; // Bir sonraki kareye kadar bekler.
        }

        // Hedef pozisyona ula��ld���nda pozisyon kesin olarak ayarlan�r.
        transform.position = targetPos;
        isMoving = false; // Hareket durumu kapat�l�r.
    }
}
