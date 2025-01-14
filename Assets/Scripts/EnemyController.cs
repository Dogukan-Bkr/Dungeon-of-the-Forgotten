using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyController sýnýfý, düþman davranýþlarýný kontrol eder.
public class EnemyController : MonoBehaviour
{
    PlayerController player; // Oyuncunun referansý.
    public Vector2 patrolInterval; // Devriye sýrasýnda bekleme süresi aralýðý.
    public Vector2 dmgRange; // Saldýrý sýrasýnda yapýlacak hasar aralýðý.
    LayerMask obstacleMask, walkableMask; // Engel ve yürünebilir alan maskeleri.
    Vector2 curPos; // Düþmanýn mevcut pozisyonu.
    List<Vector2> availableMovementList = new List<Vector2>(); // Mevcut hareket yönleri.
    List<Node> nodesList = new List<Node>(); // Yol bulma düðümleri.
    bool isMoving; // Düþmanýn hareket edip etmediðini kontrol eder.
    public float alertRange; // Oyuncunun fark edileceði mesafe.
    public float chaseSpeed; // Takip hýzý.

    // Baþlangýç metodu, düþman davranýþlarýný baþlatýr.
    void Start()
    {
        player = FindObjectOfType<PlayerController>(); // Oyuncu referansýný bulur.
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player"); // Engel maskesi.
        walkableMask = LayerMask.GetMask("Wall", "Enemy"); // Yürünebilir alan maskesi.
        curPos = transform.position; // Baþlangýç pozisyonu.
        StartCoroutine(Movement()); // Düþman hareketini baþlatýr.
    }

    // Devriye yapma metodu.
    void Patrol()
    {
        availableMovementList.Clear(); // Mevcut hareket listesi temizlenir.
        Vector2 size = Vector2.one * 0.8f; // Çarpýþma kutusu boyutu.

        // Yukarý, sað, aþaðý ve sol yönlerde hareket kontrolü yapýlýr.
        Collider2D hitUp = Physics2D.OverlapBox(curPos + Vector2.up, size, 0, obstacleMask);
        if (!hitUp) { availableMovementList.Add(Vector2.up); } // Eðer yukarýda engel yoksa hareket eklenir.

        Collider2D hitRight = Physics2D.OverlapBox(curPos + Vector2.right, size, 0, obstacleMask);
        if (!hitRight) { availableMovementList.Add(Vector2.right); }

        Collider2D hitDown = Physics2D.OverlapBox(curPos + Vector2.down, size, 0, obstacleMask);
        if (!hitDown) { availableMovementList.Add(Vector2.down); }

        Collider2D hitLeft = Physics2D.OverlapBox(curPos + Vector2.left, size, 0, obstacleMask);
        if (!hitLeft) { availableMovementList.Add(Vector2.left); }

        // Mevcut yönlerden rastgele birini seçerek hareket eder.
        if (availableMovementList.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMovementList.Count);
            curPos += availableMovementList[randomIndex];
        }

        // Pürüzsüz hareket saðlar.
        StartCoroutine(SmoothMove(Random.Range(patrolInterval.x, patrolInterval.y)));
    }

    // Düþmanýn saldýrý yapmasýný saðlayan metot.
    void Attack()
    {
        int roll = Random.Range(0, 100); // Saldýrý þansý için rastgele bir sayý.
        if (roll > 50) // %50 þansla saldýrýr.
        {
            float dmgAmount = Mathf.Ceil(Random.Range(dmgRange.x, dmgRange.y)); // Hasar miktarý.
            Debug.Log(name + " attacked and hit for " + dmgAmount + " points of damage"); // Konsola hasar bilgisi yazdýrýlýr.
        }
        else
        {
            Debug.Log(name + " attacked and missed"); // Saldýrý baþarýsýz olursa.
        }
    }

    // Yol bulma algoritmasýnda düðüm kontrolü yapan metot.
    void CheckNode(Vector2 chkPoint, Vector2 parent)
    {
        Vector2 size = Vector2.one * 0.5f; // Çarpýþma kutusu boyutu.
        Collider2D hit = Physics2D.OverlapBox(chkPoint, size, 0, walkableMask);
        if (!hit)
        {
            nodesList.Add(new Node(chkPoint, parent)); // Eðer pozisyonda engel yoksa düðüm eklenir.
        }
    }

    // Oyuncuya doðru bir sonraki adýmý bulur.
    Vector2 FindNextStep(Vector2 startPos, Vector2 targetPos)
    {
        int listIndex = 0; // Düðüm listesi indeksi.
        Vector2 myPos = startPos; // Baþlangýç pozisyonu.
        nodesList.Clear(); // Mevcut düðüm listesi temizlenir.
        nodesList.Add(new Node(startPos, targetPos)); // Baþlangýç düðümü eklenir.

        // Hedefe ulaþana veya düðüm listesi tükenene kadar devam eder.
        while (myPos != targetPos && listIndex < 1000 && nodesList.Count > 0)
        {
            // Komþu düðümleri kontrol eder.
            CheckNode(myPos + Vector2.up, myPos);
            CheckNode(myPos + Vector2.right, myPos);
            CheckNode(myPos + Vector2.down, myPos);
            CheckNode(myPos + Vector2.left, myPos);

            listIndex++;
            if (listIndex < nodesList.Count)
            {
                myPos = nodesList[listIndex].position; // Yeni pozisyona ilerler.
            }
        }

        // Eðer hedefe ulaþýlmýþsa, yolun bir adýmýný döndürür.
        if (myPos == targetPos)
        {
            nodesList.Reverse(); // Düðüm listesi ters çevrilir.
            for (int i = 0; i < nodesList.Count; i++)
            {
                if (myPos == nodesList[i].position)
                {
                    if (nodesList[i].parent == startPos)
                    {
                        return myPos; // Bir sonraki adýmý döndürür.
                    }
                    myPos = nodesList[i].parent; // Bir önceki düðüme geri döner.
                }
            }
        }
        return startPos; // Eðer yol bulunamazsa baþlangýç pozisyonunu döndürür.
    }

    // Pürüzsüz hareket saðlayan coroutine.
    IEnumerator SmoothMove(float speed)
    {
        isMoving = true; // Hareket durumu aktif edilir.

        // Hedef pozisyona ulaþana kadar hareket eder.
        while (Vector2.Distance(transform.position, curPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, curPos, 5f * Time.deltaTime); // Pozisyon güncellenir.
            yield return null; // Bir sonraki kareye kadar bekler.
        }

        transform.position = curPos; // Son pozisyon ayarlanýr.
        yield return new WaitForSeconds(speed); // Hareket aralýðý kadar bekler.

        isMoving = false; // Hareket durumu kapatýlýr.
    }

    // Düþmanýn hareketlerini yöneten coroutine.
    IEnumerator Movement()
    {
        while (true) // Sürekli devam eden döngü.
        {
            yield return new WaitForSeconds(0.1f); // Her adým arasýnda kýsa bir bekleme.

            if (!isMoving) // Eðer düþman hareket etmiyorsa:
            {
                float dist = Vector2.Distance(transform.position, player.transform.position); // Oyuncuya olan mesafeyi hesaplar.

                if (dist <= alertRange) // Eðer oyuncu belirli bir mesafedeyse:
                {
                    if (dist <= 1.1f) // Oyuncu çok yakýnsa saldýrýr.
                    {
                        Attack();
                        yield return new WaitForSeconds(Random.Range(0.5f, 1.15f)); // Saldýrý aralýðý kadar bekler.
                    }
                    else // Oyuncu mesafedeyse onu takip eder.
                    {
                        Vector2 newPos = FindNextStep(transform.position, player.transform.position);
                        if (newPos != curPos)
                        {
                            curPos = newPos; // Yeni hedef pozisyon belirlenir.
                            StartCoroutine(SmoothMove(chaseSpeed));
                        }
                        else
                        {
                            Patrol(); // Eðer oyuncuya ulaþamazsa devriye yapar.
                        }
                    }
                }
                else
                {
                    Patrol(); // Oyuncu uzaktaysa devriye yapar.
                }
            }
        }
    }
}
